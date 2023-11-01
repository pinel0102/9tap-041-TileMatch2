using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

namespace I2.SmartEdge
{
	class SmartEdgeTextureInspector
	{
		public SerializedProperty   mSerialized_Enable, mSerialized_Texture, mSerialized_Tile, 
		mSerialized_Offset, mSerialized_Pivot, mSerialized_Angle, mSerialized_Region,
		mSerialized_CustomRegion, mSerialized_MappingType;

		public void Set( SerializedProperty prop, SerializedProperty propMapping=null)
		{
            if (propMapping == null)
                propMapping = prop;

			mSerialized_Enable 		= prop.FindPropertyRelative ("_Enable");
            mSerialized_Texture     = prop.FindPropertyRelative("_Texture");
            mSerialized_MappingType = propMapping.FindPropertyRelative ("_MappingType");
			mSerialized_Tile 		= propMapping.FindPropertyRelative ("_Tile");
			mSerialized_Offset 		= propMapping.FindPropertyRelative ("_Offset");
			mSerialized_Pivot 		= propMapping.FindPropertyRelative ("_Pivot");
			mSerialized_Angle 		= propMapping.FindPropertyRelative ("_Angle");
			mSerialized_Region		= propMapping.FindPropertyRelative ("_Region");
			mSerialized_CustomRegion= propMapping.FindPropertyRelative ("_CustomRegion");
		}
		
		public void OnGUI_Texture ( bool AllowBorder, bool AllowEditParams, ref bool makeVerticesDirty, ref bool makeMaterialDirty )
		{
			GUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            if (mSerialized_Enable!=null)
			{
				GUILayout.BeginVertical(GUILayout.Width(1));
				GUILayout.Space(45);
				mSerialized_Enable.boolValue = EditorGUILayout.Toggle(mSerialized_Enable.boolValue, GUILayout.Width(20));
				GUILayout.EndVertical();
				
				GUI.backgroundColor = GUITools.LightGray;
				GUITools.BeginContents();
				GUI.backgroundColor = Color.white;
			}
			
			GUILayout.BeginHorizontal();
            Texture newTexture = EditorGUILayout.ObjectField(mSerialized_Texture.objectReferenceValue, typeof(Texture), false, GUILayout.Width(130), GUILayout.Height(130)) as Texture;
            if (newTexture != mSerialized_Texture.objectReferenceValue)
            {
                mSerialized_Enable.boolValue = (newTexture != null);
                mSerialized_Texture.objectReferenceValue = newTexture;
             }

            bool changed = EditorGUI.EndChangeCheck();
            makeMaterialDirty |= changed;
            makeVerticesDirty |= changed;
            EditorGUI.BeginChangeCheck();
			
			GUILayout.BeginVertical();

				GUI.enabled = AllowEditParams;
				int prevOption = mSerialized_MappingType.enumValueIndex;

                /////GUITools.DrawTabs( mSerialized_MappingType, null, 20);//////////
                int newIndex = GUITools.DrawTabs(prevOption, new string[] { "Region", "Custom" }, null, 20);
                if (prevOption != newIndex)
                mSerialized_MappingType.enumValueIndex = newIndex;
                AllowBorder = false;                                      //  Remove this once BORNDER MAPPING gets supported as a shader variant
                ///////////////////

                if (mSerialized_MappingType.enumValueIndex==0 && !AllowBorder)
					mSerialized_MappingType.enumValueIndex = prevOption;

				GUILayout.BeginVertical("AS TextArea", GUILayout.Height(110));
				GUILayout.Space(2);

				if (mSerialized_MappingType.enumValueIndex==0)
					OnGUI_Mapping_Border();
				else
					OnGUI_Mapping_Region();

				GUILayout.EndVertical();
				GUI.enabled = true;

            GUILayout.EndVertical();
			GUILayout.EndVertical();
			
            makeVerticesDirty |= EditorGUI.EndChangeCheck();

			if (mSerialized_Enable!=null)
				GUITools.EndContents(false);
			
			GUILayout.EndHorizontal();
		}

		void OnGUI_Mapping_Border()
		{
            EditorGUI.BeginChangeCheck();
    			float x =EditorGUILayout.Slider (new GUIContent("Offset"), mSerialized_Offset.vector2Value.x, -32, 32);
			if (EditorGUI.EndChangeCheck())
				mSerialized_Offset.vector2Value = new Vector2(x, mSerialized_Offset.vector2Value.y);

			EditorGUILayout.PropertyField (mSerialized_Tile, new GUIContent("Tile"));
		}

		void OnGUI_Mapping_Region()
		{
			EditorGUIUtility.labelWidth = 80;
			if (mSerialized_MappingType.enumValueIndex==1)
				EditorGUILayout.PropertyField (mSerialized_Region, new GUIContent("UV Mapping"));
			else
				EditorGUILayout.PropertyField (mSerialized_CustomRegion, new GUIContent("UV Mapping"));
			EditorGUIUtility.labelWidth = 50;
			GUILayout.Space(5);

			EditorGUILayout.PropertyField (mSerialized_Offset, new GUIContent("Offset"));
			EditorGUILayout.PropertyField (mSerialized_Tile, new GUIContent("Tile"));
			//GUI.enabled = !mSerialized_EnableBorderMapping.boolValue;
			
			GUILayout.Space(10);
			EditorGUILayout.PropertyField (mSerialized_Pivot, new GUIContent("Pivot"));
			EditorGUILayout.PropertyField (mSerialized_Angle, new GUIContent("Angle"));
		}

        public bool IsUsed()
        {
            return mSerialized_Enable.boolValue && (mSerialized_Texture.objectReferenceValue!=null);
        }
    }
}