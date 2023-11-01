using UnityEngine;
using UnityEditor;
using System.Collections;

namespace I2.SmartEdge
{
	public class SmartEdgeTools
	{
		#region Skin

		public static GUISkin SmartEdgeSkin { 
			get{
				if (mSmartEdgeSkin==null)
					mSmartEdgeSkin = Resources.Load<GUISkin>("SmartEdge Skin");
				return mSmartEdgeSkin;
			}
		}
		static GUISkin mSmartEdgeSkin;

		#endregion

		#region Texture UV Settings


		#endregion
	}
}