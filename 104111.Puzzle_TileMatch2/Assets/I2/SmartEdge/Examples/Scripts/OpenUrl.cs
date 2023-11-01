using UnityEngine;

namespace I2.SmartEdge
{
    public class OpenUrl : MonoBehaviour
    {
        public void OpenTutorials() { Open("http://inter-illusion.com/assets/SmartEdgeManual/SmartEdge.html"); }
        public void OpenAssetStore() { Open("https://www.assetstore.unity3d.com/#!/content/32312"); }

        public void Open( string url )
        {
            Application.OpenURL(url);
        }
    }
}