using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using System.Threading.Tasks;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets.Settings;
#endif

public class AddressableLoader
{
    private static AddressableLoader _instance;
    public static AddressableLoader Instance {
        get {
            if (_instance == null) _instance = new AddressableLoader();
            return _instance;
        }
    }

    public bool isLoading { get { return _isLoading; }}
    public bool isDone { get { return _isDone; }}
    public int completeCount{ get { return _completeCount; }}
    public float ProgressFast{ get { return _progress; }}  
    public float Progress{ get { return GetProgress(); }}

    private const int _delayFast = 50;
    private const int _delaySlow = 100;

    private int _delay = _delayFast;
    private bool _isQuit = false;
    private bool _isLoading;
    private bool _isDone;
    private float _progress;
    private float _progressSum;
    private int _completeCount;
    private int _assetCount;   
    private float _assetCountInverse;    
    private IList<IResourceLocation> _location;
    private List<GameObject> _gameObjects = new List<GameObject>();
    private GameObject _loadedObject;
    private AsyncOperationHandle<GameObject> currentHandle;
    private List<AsyncOperationHandle<GameObject>> currentHandleArray = new List<AsyncOperationHandle<GameObject>>();
    private readonly Vector3 localPositionDefault = Vector3.zero;
    private readonly Vector3 localScaleDefault = Vector3.one;

    public void LoadAsset(AssetReference asset, int count = 1, Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null, Action<GameObject> finishCallback = null)
    {
        _isLoading = true;
        _isDone = false;
        _isQuit = false;
        _loadedObject = null;
        _completeCount = 0;
        _progress = 0;

        _assetCount = count;
        _assetCountInverse = 1f / _assetCount;

        Addressables.LoadResourceLocationsAsync(asset).Completed += (handle) =>
        {            
            _location = handle.Result;
            currentHandleArray.Clear();

            InstantiateAsyncRoof(parent, localPosition, localScale, finishCallback);
        };
    }

    public void LoadAssetLabel(AssetLabelReference assetLabel, Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null, Action<GameObject> finishCallback = null)
    {
        _isLoading = true;
        _isDone = false;
        _isQuit = false;   
        _loadedObject = null;
        _completeCount = 0;
        _progress = 0;

        Addressables.LoadResourceLocationsAsync(assetLabel).Completed += (handle) =>
        {   
            _location = handle.Result;            
            _assetCount = _location.Count;
            _assetCountInverse = 1f / _assetCount;            
            currentHandleArray.Clear();

            InstantiateAsync(_location, parent, localPosition, localScale, finishCallback, true);            
        };
    }

    private async void InstantiateAsyncRoof(Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null, Action<GameObject> finishCallback = null)
    {
        for (int i = 0; i < _assetCount; i++)
        {
            if (_isQuit) return;

            await Task.Delay(_delay);

            InstantiateAsync(_location, parent, localPosition, localScale, finishCallback, false);
        }
    }
 
    private async void InstantiateAsync(IList<IResourceLocation> location, Transform parent = null, Vector3? localPosition = null, Vector3? localScale = null, Action<GameObject> finishCallback = null, bool internalAwait = true)
    {
        for (int i = 0; i < location.Count; i++)
        {
            if (_isQuit) return;

            if (internalAwait)
                await Task.Delay(_delay);
            
            currentHandle = Addressables.InstantiateAsync(location[i], Vector3.one, Quaternion.identity);
            currentHandle.Completed += (handle) =>
            {
                _loadedObject = handle.Result;
                
                if (_isQuit) 
                {
                    Addressables.ReleaseInstance(_loadedObject);
                    return;
                }

                _gameObjects.Add(_loadedObject);

                _completeCount++;                
                _isDone = _completeCount == _assetCount; 
                _isLoading = !_isDone;

                if (_isDone)
                    _progress = 1;
                else
                    _progress += _assetCountInverse;

                if (parent)
                    _loadedObject.transform.SetParent(parent);

                if (localPosition == null)
                    _loadedObject.transform.localPosition = localPositionDefault;
                else
                    _loadedObject.transform.localPosition = localPosition.Value;

                if (localScale == null)
                    _loadedObject.transform.localScale = localScaleDefault;
                else
                    _loadedObject.transform.localScale = localScale.Value;                

                //Debug.Log(CodeManager.GetMethodName() + string.Format("{0}, {1}/{2}, {3}", _loadedObject.name, loadedIndex, _assetCount, Progress));

                if (finishCallback != null) 
                    finishCallback.Invoke(_loadedObject);
            };

            currentHandleArray.Add(currentHandle);
        }
    }

    private float GetProgress()
    {
        if (_assetCount == 0 || currentHandleArray.Count == 0)
            return 0;

        _progressSum = 0;
        
        for (int i=0; i < currentHandleArray.Count; i++)
        {
            _progressSum += currentHandleArray[i].PercentComplete;
        }        

        return _progressSum * _assetCountInverse;
    }

    public void SetAwaitMode(AwaitMode mode = AwaitMode.Fast)
    {
        //Debug.Log(CodeManager.GetMethodName() + mode);

        switch(mode)
        {
            case AwaitMode.Slow:
                _delay = _delaySlow;
                break;
            default:
                _delay = _delayFast;
                break;
        }
    }
    
    public void Release()
    {
        _isQuit = true;

        if (_gameObjects.Count == 0)
            return;
 
        while (_gameObjects.Count > 0)
        {
            int index = _gameObjects.Count - 1;    
            if (_gameObjects[index] != null)
                Addressables.ReleaseInstance(_gameObjects[index]);
            _gameObjects.RemoveAt(index);
        }
    }

    public enum AwaitMode
    {
        Fast,
        Slow
    }
}


#if UNITY_EDITOR

[InitializeOnLoad]
class AutoBuildAddressable
{
    static bool useAutoBuildAddressable = true;

    static AutoBuildAddressable()
    {
        if (useAutoBuildAddressable)
        {
            BuildPlayerWindow.RegisterBuildPlayerHandler(
                new Action<BuildPlayerOptions>(buildPlayerOptions => {
                    BuildAddressable();
                    BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(buildPlayerOptions);
                }));
        }
    }

    static void BuildAddressable()
    {
        Debug.Log(CodeManager.GetMethodName());

        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
    }
}

#endif
