using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace I2.SmartEdge
{
    [ExecuteInEditMode]
    public class SmartEdgeManager : MonoBehaviour
    {
        // SmartEdge components that are running animations
        List<SmartEdge> mUpdate_Animations = new List<SmartEdge>();

        #region Setup
        void Initialize()
        {
            StartCoroutine(UpdateAnimations());
        }

         public static void RegisterAnimation(SmartEdge se)
        {
            if (!Application.isPlaying)
                return;
            var manager = singleton;

            if (!manager.mUpdate_Animations.Contains(se))
                manager.mUpdate_Animations.Add(se);
        }

        public static void UnregisterAnimation(SmartEdge se)
        {
            if (!Application.isPlaying)
                return;
            var manager = singleton;
            manager.mUpdate_Animations.Remove(se);
        }
        #endregion


        IEnumerator UpdateAnimations()
        {
            while (true)
            {
                // Update all animations and then remove the ones that are not longer playing
                for (int i = mUpdate_Animations.Count - 1; i >= 0; --i)
                {
                    if (!mUpdate_Animations[i].UpdateAnimations())
                        mUpdate_Animations.RemoveAt(i);
                }

                yield return null;
            }
        }

        #region Singleton
        static SmartEdgeManager mSingleton;
        static SmartEdgeManager singleton
        {
            get
            {
                if (mSingleton == null)
                {
                    mSingleton = (SmartEdgeManager)FindObjectOfType(typeof(SmartEdgeManager));

                    if (mSingleton == null)
                    {
                        GameObject go = new GameObject();
                        //go.hideFlags = go.hideFlags | HideFlags.HideAndDontSave;
                        go.hideFlags = go.hideFlags | HideFlags.DontSave;
                        go.name = "[singleton] SmartEdgeManager";

                        mSingleton = go.AddComponent<SmartEdgeManager>();
                        mSingleton.Initialize();
                    }

                }

                return mSingleton;
            }
        }

        void OnDestroy()
        {
            if (mSingleton == this)
                mSingleton = null;
        }

        void Awake()
        {
            if (mSingleton == null)
            {
                mSingleton = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            if (mSingleton != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion

    }
}