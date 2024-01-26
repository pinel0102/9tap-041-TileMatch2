using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections.Generic;
using System.Linq;

namespace NineTap.Common
{
    public class SceneManager
    {
        #region Types
        public struct GetSceneResult<T> where T : UIScene
        {
            public GetSceneResult(T scene = null, bool isFromStack = false)
            {
                Scene = scene;
                IsFromStack = isFromStack;
            }

            public T Scene { get; }
            public bool IsFromStack { get; }
        }
        #endregion

        private readonly Stack<UIScene> m_sceneStack;

        private UIScene m_currentScene = null;
        public UIScene CurrentScene => m_currentScene;

        private UIScene m_prevScene = null;
        public UIScene PrevScene => m_prevScene;

        private int m_rootRaycasterDisable = 0;
        public int RootRaycasterDisable
        {
            get => m_rootRaycasterDisable;
            set
            {
                m_rootRaycasterDisable = value;
                if (m_rootRaycaster != null)
                {
                    m_rootRaycaster.enabled = m_rootRaycasterDisable == 0;
                }
            }
        }

        private GameObject m_root;
        public GameObject Root
        {
            get
            {
                if (m_root == null)
                {
                    m_root = new GameObject { name = "Root"};
                }

                return m_root;
            }
        }

        private int m_processId;
        private int m_id;

        private Transform m_sceneRoot;
        private GraphicRaycaster m_rootRaycaster = null;

        public SceneManager(GameObject sceneRoot)
        {
            m_sceneStack = new();
            m_sceneRoot = sceneRoot.transform;
            if (!sceneRoot.TryGetComponent(out m_rootRaycaster))
            {
                m_rootRaycaster = sceneRoot.AddComponent<GraphicRaycaster>();
            }
        }

        public void ShowSceneUI<T>(UIParameter uiParameter, bool clearStack = false) where T : UIScene
        {
            bool forceDestroyPrevScene = clearStack;

            //	ID 발급
            int requestId = GetRequestId();

            // 동시 요청을 허용하지 않음.
            if (m_processId != requestId)
            {
                throw new OperationCanceledException();
            }

            RootRaycasterDisable++;

            //	이미 같은 Scene이 열려있는 상태일 경우
            if (m_currentScene is T)
            {
                EndProcess(requestId);
                return;
            }

            m_prevScene = m_currentScene;
            m_currentScene = null;

            if (clearStack)
            {
                foreach (UIScene scene in m_sceneStack)
                {
                    GameObject.Destroy(scene.CachedGameObject);
                }

                m_sceneStack.Clear();
            }

            GetSceneResult<T> result = GetScene<T>();
            if (result.Scene == null)
            {
                Debug.LogError("씬 생성 실패");
                EndProcess(requestId);
                return;
            }

            //	만약 stack에 저장되어있었다면 그동안의 History를 날린다.
            if (result.IsFromStack)
            {
                forceDestroyPrevScene = true;

                while (m_sceneStack.Count > 0)
                {
                    UIScene scene = m_sceneStack.Pop();
                    if (scene == result.Scene)
                    {
                        break;
                    }

                    GameObject.Destroy(scene.CachedGameObject);
                }
            }

            m_currentScene = result.Scene;

            uiParameter ??= new DefaultParameterWithoutHUD();

            if (!result.IsFromStack)
            {
                m_currentScene.OnSetup(uiParameter);
            }

            m_currentScene.CachedParameter = uiParameter;
            m_currentScene.Show();
            HidePrevScene(forceDestroyPrevScene);

            EndProcess(requestId);
        }

        public void BackSceneUI()
        {
            //	ID 발급
            int requestId = GetRequestId();

            // 동시 요청을 허용하지 않음.
            if (m_processId != requestId)
            {
                return;
            }

            RootRaycasterDisable++;

            //	스택에 쌓여있는 씬이 없으면 뒤로가기 불가
            if (m_sceneStack.Count == 0)
            {
                EndProcess(requestId);
                return;
            }

            m_prevScene = m_currentScene;

            UIScene scene = m_sceneStack.Pop();
            m_currentScene = scene;

            m_currentScene.Show();

            //	숨기고 (뒤로가기로 씬을 전환할땐 stack하지 않는다.)
            HidePrevScene(forceDestroy: true);
            EndProcess(requestId);
        }

        private int GetRequestId()
        {
            if (m_id == 0 && m_processId == 0)
            {
                m_id = 1;
                m_processId = 1;
            }
            else
            {
                m_id++;
            }

            return m_id;
        }

        private void EndProcess(int requestId)
        {
            m_processId = requestId + 1;
            RootRaycasterDisable--;
        }

        /// <summary>
        /// 씬을 숨긴다.
        /// </summary>
        private void HidePrevScene(bool forceDestroy)
        {
            if (m_prevScene == null)
            {
                return;
            }

            UIScene prevScene = m_prevScene;
            UIScene.HideType hideOption = prevScene.HideOption;

            m_prevScene = null;

            if (hideOption == UIScene.HideType.Hide && !forceDestroy)
            {
                m_sceneStack.Push(prevScene);
            }

            try
            {
                prevScene.Hide();
            }
            finally
            {
                if (hideOption == UIScene.HideType.Destroy || forceDestroy)
                {
                    GameObject.Destroy(prevScene.CachedGameObject);
                    Resources.UnloadUnusedAssets();
                }
            }
        }

        private GetSceneResult<T> GetScene<T>() where T : UIScene
        {
            UIScene scene = m_sceneStack.FirstOrDefault(scene => scene.GetType() == typeof(T));
            if (scene != null)
            {
                return new GetSceneResult<T>(scene as T, isFromStack: true);
            }

            GameObject prefab = LoadUIPrefab<T>();
            if (prefab == null)
            {
                return new GetSceneResult<T>();
            }

            GameObject instance = UnityEngine.Object.Instantiate(prefab, m_sceneRoot);
            UIScene value = instance.GetComponent<UIScene>();

            return new GetSceneResult<T>(value as T);
        }

        private GameObject LoadUIPrefab<T>() where T : UIBase
        {
            return LoadUIPrefab(ResourcePathAttribute.GetPath<T>());
        }

        private GameObject LoadUIPrefab(string prefabPath)
        {
            //	@Note: 비동기 로딩
            UnityEngine.Object prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"{prefabPath} 불러오기 실패");
                return null;
            }

            return (GameObject)prefab;
        }
    }
}
