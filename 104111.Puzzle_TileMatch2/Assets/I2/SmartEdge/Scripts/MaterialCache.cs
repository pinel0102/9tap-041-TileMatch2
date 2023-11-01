using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace I2.SmartEdge
{
	[System.Serializable]
    public abstract class MaterialDef
	{
        public Shader shader;

		public abstract bool IsEqualTo( MaterialDef other );
		public abstract void Apply( Material material );
        public abstract MaterialDef Clone();
		public abstract void ReleaseToPool();
	}
	
	public class MaterialCache// : MonoBehaviour
	{		
		#region Variables Singleton
		
		public static MaterialCache pInstance { 
			get{
				if (mInstance==null)
				{
					//GameObject go = new GameObject("SmartEdge Material Cache");
					//go.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
					//go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideAndDontSave;
					//GameObject.DontDestroyOnLoad(go);
					//mInstance = go.AddComponent<MaterialCache>();
					mInstance = new MaterialCache();
				}
				return mInstance;
			}
		}
		static MaterialCache mInstance;
		
		#endregion

		#region Variables Cache

		//[System.Serializable]
		public class MaterialInstance
		{
			public Material material;
			public MaterialDef definition;

			public int NumberInstances;	// How many objects are referencing this one
		}
		//public static Dictionary<Material, MaterialInstance> mMaterialInstances = new Dictionary<Material, MaterialInstance>();
        public static List<MaterialInstance> mMaterialInstances = new List<MaterialInstance>();

		#endregion

		#region Management

		public void ReleaseMaterial( Material material )
		{
			MaterialInstance instance = null;
            for (int i = 0, imax = mMaterialInstances.Count; i < imax; ++i)
            {
                instance = mMaterialInstances[i];
                if (instance.material == material)
                {
                    instance.NumberInstances--;
                    if (instance.NumberInstances <= 0)
                    {
#if UNITY_EDITOR
                        //Object.DestroyImmediate(material);
#else
					Object.Destroy(material);
#endif

                        mMaterialInstances.RemoveAt(i);
                        instance.definition.ReleaseToPool();
                        //Debug.Log ("Destroying mat " + instance.ID + " (remaining: "+mMaterialInstances.Count+")");
                    }
                    return;
                }
            }
		}

        public void Print()
        {
            //Debug.LogFormat("<color=blue>Material Cache ({0})</color>", mMaterialInstances.Count);
            //foreach (var kvp in mMaterialInstances)
              //  Debug.LogFormat("Def: ({0}) {1} {2}", kvp.Value.NumberInstances, kvp.Value.definition.shader.name, ((MaterialDef_SDF)kvp.Value.definition).TexFace);
        }

        public static MaterialInstance GetMaterialInstance( Material mat )
        {
            for (int i = 0, imax = mMaterialInstances.Count; i < imax; ++i)
                if (mMaterialInstances[i].material == mat)
                    return mMaterialInstances[i];
            return null;
        }


        public static Material GetMaterial( Material currentMat, MaterialDef definition )
		{
            var cache = pInstance;

            //--[ Clear deleted materials ]-----------------
            for (int i = mMaterialInstances.Count - 1; i >= 0; --i)
                if (mMaterialInstances[i].material == null)
                    mMaterialInstances.RemoveAt(i);


			//--[ Use or update the current instance ]--------------------
			MaterialInstance instance = null, similar = null;
			if (currentMat!=null)
			{
                instance = GetMaterialInstance(currentMat);
                if (instance != null)
                {
                    if (definition.IsEqualTo(instance.definition))
                        return currentMat;

                    //--[ If there is any other material matching this definition, use that ]---------
                    similar = cache.GetSimilarInstance(definition);
                    if (similar != null)
                    {
                        similar.NumberInstances++;
                        cache.ReleaseMaterial(currentMat);
                        return similar.material;
                    }

                    //--[ if this is the only definition of this instance, change the material to the new one]---
                    if (instance.NumberInstances == 1)
                    {
                        instance.definition.ReleaseToPool();
                        instance.definition = definition.Clone();
                        definition.Apply(currentMat);
                        return currentMat;
                    }

                    //--[ Nothing can be used or reused, so lets release this one and create a new one ]--------
                    cache.ReleaseMaterial(currentMat);
                }
			}

			//--[ If there is any other material matching this definition, use that ]---------
			instance = instance!=null ? similar : cache.GetSimilarInstance( definition );
			if (instance!=null)
			{
				instance.NumberInstances++;
				return instance.material;
			}
			
			//--[ Create a new Instance ]--------------
			
			instance = new MaterialInstance();
			instance.NumberInstances = 1;
            instance.definition = definition.Clone();
			instance.material = new Material(definition.shader);
			definition.Apply(instance.material);
			
			mMaterialInstances.Add(instance);
			return instance.material;
		}
		
		MaterialInstance GetSimilarInstance( MaterialDef definition )
		{
            for (int i = 0, imax = mMaterialInstances.Count; i < imax; ++i)
                if (mMaterialInstances[i].definition.IsEqualTo(definition))
                    return mMaterialInstances[i];
			return null;
		}


		~MaterialCache()
		{
            mMaterialInstances.Clear();
		}

		#endregion
	}
}