using UnityEngine;
 
/// <summary>
/// Be aware this will not prevent a non singleton constructor
///   such as `T myT = new T();`
/// To prevent that, add `protected T () {}` to your singleton class.
/// 
/// As a note, this is made as MonoBehaviour because we need Coroutines.
/// </summary>
public class SingletonMono<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
 
    private static object _lock = new object();
 
    public static T Instance
    {
        get
        {
            lock(_lock)
            {
                if (_instance == null)
                {
                    _instance = (T) FindObjectOfType(typeof(T));
 
                    if ( FindObjectsOfType(typeof(T)).Length > 1 )
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopenning the scene might fix it.");
                        return _instance;
                    }
 
                    if (_instance == null)
                    { 
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(singleton) "+ typeof(T).ToString();
 
                        Debug.LogWarning("[Singleton] An instance of " + typeof(T) + 
                            " is needed in the scene, so '" + singleton +
                            "' was created. But please create instance in the scene edit.");
                    } else {
                        //Debug.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
                    }
                }
 
                return _instance;
            }
        }
    } 
}