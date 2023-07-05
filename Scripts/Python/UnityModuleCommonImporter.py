import os
import sys

oProjName = sys.argv[1]
oBranchName = sys.argv[2]
oProjRootPath = sys.argv[3]

oSubmoduleInfos = [
	{
		"Name": ".Module.UnityCommon",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommon.git"
	},

	{
		"Name": ".Module.UnityCommonDefine",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommondefine.git"
	},

	{
		"Name": ".Module.UnityCommonAccess",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonaccess.git"
	},

	{
		"Name": ".Module.UnityCommonFactory",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonfactory.git"
	},

	{
		"Name": ".Module.UnityCommonExtension",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonextension.git"
	},

	{
		"Name": ".Module.UnityCommonFunction",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonfunction.git"
	},

	{
		"Name": ".Module.UnityCommonUtility",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonutility.git"
	},

	{
		"Name": ".Module.UnityCommonExternals",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonexternals.git"
	},

	{
		"Name": ".Module.UnityCommonAds",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonads.git"
	},

	{
		"Name": ".Module.UnityCommonFlurry",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonflurry.git"
	},
	
	{
		"Name": ".Module.UnityCommonFacebook",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonfacebook.git"
	},

	{
		"Name": ".Module.UnityCommonFirebase",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonfirebase.git"
	},

	{
		"Name": ".Module.UnityCommonAppsFlyer",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonappsflyer.git"
	},
	
	{
		"Name": ".Module.UnityCommonGameCenter",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommongamecenter.git"
	},

	{
		"Name": ".Module.UnityCommonPurchase",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonpurchase.git"
	},

	{
		"Name": ".Module.UnityCommonNotification",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonnotification.git"
	},

	{
		"Name": ".Module.UnityCommonPlayfab",
		"Path": f"{oProjName}/Packages",
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitycommonplayfab.git"
	}
]

# 경로를 탐색한다
def FindPath(a_oBasePath):
	for i in range(0, 10):
		# 디렉토리가 존재 할 경우
		if os.path.exists(a_oBasePath):
			return a_oBasePath

		a_oBasePath = f"../{a_oBasePath}"
		
	return a_oBasePath

for oSubmoduleInfo in oSubmoduleInfos:
	oURL = oSubmoduleInfo["URL"]
	oPath = f"../../{oSubmoduleInfo['Path']}"
	oFullPath = f"../../{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}"

	# 서브 모듈이 없을 경우
	if not os.path.exists(oFullPath):
		# 디렉토리가 없을 경우
		if not os.path.exists(oPath):
			os.makedirs(oPath)

		os.system(f"git submodule add -f {oURL} {oFullPath}")

	oSubmodulePath = f"{oProjRootPath}/{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}" if oProjRootPath else f"{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}"
	os.system(f"git submodule set-branch --branch \"{oBranchName}\" \"{oSubmodulePath}\"")

os.system(f"python UnityModuleRemoteURLUpdater.py \"{oProjName}\"")
