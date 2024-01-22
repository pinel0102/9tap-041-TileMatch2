using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;

public static class CExtensions {
    //! 포함 여부를 검사한다
    public static bool ExIsContainsAdsNetworkID(this PlistElementArray a_oSender, string a_oNetworkID) {
        for(int i = 0; i < a_oSender.values.Count; ++i) {
            var oAdsNetworkIDInfo = a_oSender.values[i].AsDict();
            var oAdsNetworkIDs = oAdsNetworkIDInfo.values;

            // 광고 네트워크 식별자가 존재 할 경우
            if(oAdsNetworkIDs.TryGetValue("SKAdNetworkIdentifier", out PlistElement oValue) && oValue.AsString().Equals(a_oNetworkID)) {
                return true;
            }
        }

        return false;
    }

    //! 배열을 반환한다
    public static PlistElementArray ExGetArray(this PlistDocument a_oSender, string a_oKey) {
        try {
            return a_oSender.root[a_oKey].AsArray();
        } catch {
            return a_oSender.root.CreateArray(a_oKey);
        }
    }
}
#endif			// #if UNITY_IOS

//! 빌드 프로세스 처리자
public class BuildProcssHandler {
    #region 함수
    //! 빌드가 완료 되었을 경우
    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget a_eTarget, string a_oPath) 
    {
#if UNITY_IOS

        string attString = "∙ Advertisements that match your interests\n∙ An improved personalized experience over time";
        string adReportEndPoint = "https://appsflyer-skadnetwork.com/";

        var oAdsNetworkIDs = new string[] {
            
            "22mmun2rn5.skadnetwork",
            "238da6jt44.skadnetwork",
            "24t9a8vw3c.skadnetwork",
            "2u9pt9hc89.skadnetwork",
            "3qy4746246.skadnetwork",
            "3rd42ekr43.skadnetwork",
            "3sh42y64q3.skadnetwork",
            "424m5254lk.skadnetwork",
            "4468km3ulz.skadnetwork",
            "44jx6755aq.skadnetwork",
            "44n7hlldy6.skadnetwork",
            "488r3q3dtq.skadnetwork",
            "4dzt52r2t5.skadnetwork",
            "4fzdc2evr5.skadnetwork",
            "4pfyvq9l8r.skadnetwork",
            "578prtvx9j.skadnetwork",
            "5a6flpkh64.skadnetwork",
            "5lm9lj6jb7.skadnetwork",
            "5tjdwbrq8w.skadnetwork",
            "7rz58n8ntl.skadnetwork",
            "7ug5zh24hu.skadnetwork",
            "8s468mfl3y.skadnetwork",
            "9rd848q2bz.skadnetwork",
            "9t245vhmpl.skadnetwork",
            "av6w8kgt66.skadnetwork",
            "bvpn9ufa9b.skadnetwork",
            "c6k4g5qg8m.skadnetwork",
            "cstr6suwn9.skadnetwork",
            "ejvt5qm6ak.skadnetwork",
            "f38h382jlk.skadnetwork",
            "f73kdq92p3.skadnetwork",
            "g28c52eehv.skadnetwork",
            "glqzh8vgby.skadnetwork",
            "gta9lk7p23.skadnetwork",
            "hs6bdukanm.skadnetwork",
            "kbd757ywx3.skadnetwork",
            "klf5c3l5u5.skadnetwork",
            "lr83yxwka7.skadnetwork",
            "ludvb6z3bs.skadnetwork",
            "m8dbw4sv7c.skadnetwork",
            "mlmmfzh3r3.skadnetwork",
            "mtkv5xtk9e.skadnetwork",
            "n38lu8286q.skadnetwork",
            "n9x2a789qt.skadnetwork",
            "ppxm28t8ap.skadnetwork",
            "prcb7njmu6.skadnetwork",
            "s39g8k73mm.skadnetwork",
            "su67r6k2v3.skadnetwork",
            "t38b2kh725.skadnetwork",
            "tl55sbb4fm.skadnetwork",
            "v72qych5uu.skadnetwork",
            "v79kvwwj4g.skadnetwork",
            "v9wttpbfk9.skadnetwork",
            "wg4vff78zm.skadnetwork",
            "wzmmz9fp6w.skadnetwork",
            "yclnxrl5pm.skadnetwork",
            "ydx93a7ass.skadnetwork",
            "zmvfpc5aq8.skadnetwork",
            "mp6xlyr22a.skadnetwork",
            "275upjj5gd.skadnetwork",
            "6g9af3uyq4.skadnetwork",
            "9nlqeag3gk.skadnetwork",
            "cg4yq2srnc.skadnetwork",
            "qqp299437r.skadnetwork",
            "rx5hdcabgc.skadnetwork",
            "u679fj5vs4.skadnetwork",
            "uw77j35x4d.skadnetwork",
            "2fnua5tdw4.skadnetwork",
            "3qcr597p9d.skadnetwork",
            "e5fvkxwrpn.skadnetwork",
            "ecpz2srf59.skadnetwork",
            "hjevpa356n.skadnetwork",
            "k674qkevps.skadnetwork",
            "n6fk4nfna4.skadnetwork",
            "p78axxw29g.skadnetwork",
            "y2ed4ez56y.skadnetwork",
            "zq492l623r.skadnetwork",
            "32z4fx6l9h.skadnetwork",
            "523jb4fst2.skadnetwork",
            "54nzkqm89y.skadnetwork",
            "5l3tpt7t6e.skadnetwork",
            "6xzpu9s2p8.skadnetwork",
            "79pbpufp6p.skadnetwork",
            "9b89h5y424.skadnetwork",
            "cj5566h2ga.skadnetwork",
            "feyaarzu9v.skadnetwork",
            "ggvn48r87g.skadnetwork",
            "pwa73g5rt2.skadnetwork",
            "xy9t38ct57.skadnetwork",
            "4w7y6s5ca2.skadnetwork",
            "737z793b9f.skadnetwork",
            "dzg6xy7pwj.skadnetwork",
            "hdw39hrw9y.skadnetwork",
            "mls7yz5dvl.skadnetwork",
            "w9q455wk68.skadnetwork",
            "x44k69ngh6.skadnetwork",
            "y45688jllp.skadnetwork",
            "252b5q8x7y.skadnetwork",
            "9g2aggbj52.skadnetwork",
            "nu4557a4je.skadnetwork",
            "v4nxqhlyqp.skadnetwork",
            "r26jy69rpl.skadnetwork",
            "eh6m2bh4zr.skadnetwork",
            "8m87ys6875.skadnetwork",
            "97r2b46745.skadnetwork",
            "52fl2v3hgk.skadnetwork",
            "9yg77x724h.skadnetwork",
            "gvmwg8q7h5.skadnetwork",
            "n66cz3y3bx.skadnetwork",
            "nzq8sh4pbs.skadnetwork",
            "pu4na253f3.skadnetwork",
            "yrqqpx2mcb.skadnetwork",
            "z4gj7hsk7h.skadnetwork",
            "f7s53z58qe.skadnetwork",
            "7953jerfzd.skadnetwork"
            
        };

        // iOS 플랫폼 일 경우
        if(a_eTarget == BuildTarget.iOS) {
            

            string oProjFilepath = PBXProject.GetPBXProjectPath(a_oPath);
            string oPlistFilepath = string.Format("{0}/Info.plist", a_oPath);

            // 프로젝트 옵션을 설정한다 {
            var oProj = new PBXProject();
            oProj.ReadFromFile(oProjFilepath);
            
            if(oProj != null) {
                Debug.Log(CodeManager.GetMethodName() + "set project.pbxproj");
                
                string oMainGUID = oProj.GetUnityMainTargetGuid();
                string oFrameworkGUID = oProj.GetUnityFrameworkTargetGuid();				

                oProj.AddCapability(oMainGUID, PBXCapabilityType.InAppPurchase);
                oProj.AddCapability(oFrameworkGUID, PBXCapabilityType.InAppPurchase);

                oProj.AddFrameworkToProject(oMainGUID, "StoreKit.framework", false);
                oProj.AddFrameworkToProject(oFrameworkGUID, "StoreKit.framework", false);
                
                // [ks.kim] AppTrackingTransparency Framework 삭제.
                oProj.RemoveFrameworkFromProject(oMainGUID, "AppTrackingTransparency.framework");
                oProj.RemoveFrameworkFromProject(oFrameworkGUID, "AppTrackingTransparency.framework");

                oProj.WriteToFile(oProjFilepath);
            }
            // 프로젝트 옵션을 설정한다 }

            // Plist 옵션을 설정한다 {
            var oDocument = new PlistDocument();
            oDocument.ReadFromFile(oPlistFilepath);
            
            if(oDocument != null && oDocument.root != null) {
                Debug.Log(CodeManager.GetMethodName() + "set info.plist");
                
                oDocument.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
                oDocument.root.SetString("NSUserTrackingUsageDescription", attString);
                oDocument.root.SetString("NSAdvertisingAttributionReportEndpoint", adReportEndPoint);
                oDocument.root.SetString("GADApplicationIdentifier", ProjectManager.ironSource_Admob_AppID_iOS);
                
                // [ks.kim] UIRequiredDeviceCapabilities array 삭제.
                var oUIRequiredDeviceCapabilitiesList = oDocument.ExGetArray("UIRequiredDeviceCapabilities");
                oUIRequiredDeviceCapabilitiesList.values.Clear();

                var oAdsNetworkItemList = oDocument.ExGetArray("SKAdNetworkItems");

                for(int i = 0; i < oAdsNetworkIDs.Length; ++i) {
                    // 광고 네트워크 식별자가 없을 경우
                    if(!oAdsNetworkItemList.ExIsContainsAdsNetworkID(oAdsNetworkIDs[i])) {
                        var oAdsNetworkIDInfo = oAdsNetworkItemList.AddDict();
                        oAdsNetworkIDInfo.SetString("SKAdNetworkIdentifier", oAdsNetworkIDs[i]);
                    }
                }

                oDocument.WriteToFile(oPlistFilepath);
            }
            // Plist 옵션을 설정한다 }
        }
#endif			// #if UNITY_IOS
    }
    #endregion			// 함수
}

#endif