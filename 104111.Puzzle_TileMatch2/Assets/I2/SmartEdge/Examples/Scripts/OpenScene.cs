using UnityEngine;

namespace I2.SmartEdge
{
    public class OpenScene : MonoBehaviour
    {
        public void Open( string sceneName )
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
            #else
                Application.LoadLevel(sceneName);
            #endif
        }

        public void OpenNext()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
                int nextScene = (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex+1) % UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextScene, UnityEngine.SceneManagement.LoadSceneMode.Single);
            #else
                Application.LoadLevel((Application.loadedLevel + 1) % Application.levelCount);
            #endif
        }
    }
}