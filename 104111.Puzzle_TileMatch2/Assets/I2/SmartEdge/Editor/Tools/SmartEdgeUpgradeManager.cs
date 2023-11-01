using UnityEngine;
using UnityEditor;
using System.Collections;

namespace I2.SmartEdge
{
	[InitializeOnLoad]
	public class SmartEdgeUpgradeManager
	{
		static SmartEdgeUpgradeManager()
		{
			I2.I2Analytics.PluginsVersion["I2 SmartEdge"] = SE_InspectorTools.GetVersion();
            I2.I2Analytics.SendAnalytics("I2 SmartEdge", SE_InspectorTools.GetVersion()); // Tracks Unity version usage to know when is safe to discontinue support to old unity versions
		}
	}



    public static class UpgradeManagerHelper
    {
        public static bool HasAttributeOfType<T>(this System.Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0;
        }
    }
}