using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.UI;

namespace I2.SmartEdge
{
	public partial class SmartEdge_Inspector
	{
		SerializedProperty 	mSerialized_Deformation;

		
		void RegisterProperty_Deformation()
		{
			mSerialized_Deformation 		= serializedObject.FindProperty("_Deformation");

            RegisterProperty_Deformation_FreeTransform();
        }
		
		void OnGUI_Deformation ()
		{
            OnGUI_FreeTransform();
            OnGUI_FollowSpline();
        }

        void OnGUI_FollowSpline()
        {
            OnGUI_CommingSoon("Follow Spline");
        }

        void OnGUI_CommingSoon(string sectionName)
        {
            if (!GUITools.DrawHeader(sectionName, "SE " + sectionName, true, false, (x)=> { }, disabledColor: GUITools.LightGray))
                return;

            GUITools.BeginContents();

            GUILayout.Label("Coming Soon");
            GUILayout.Label("If you want this feature ASAP, give it your vote!");
            GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Check the Roadmap"))
                    Application.OpenURL(SE_InspectorTools.URL_Roadmap);
            GUILayout.EndHorizontal();

            GUITools.EndContents();
        }
    }
}