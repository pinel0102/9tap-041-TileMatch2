import os
import sys
import platform

oProjName = sys.argv[1]

oSubmoduleInfos = [
	{
		"Name": ".Module.UnityStudy",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityStudyDefine",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityStudyUtility",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityStudyImporter",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommon",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonDefine",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonAccess",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonFactory",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonExtension",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonFunction",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonUtility",
		"Path": f"{oProjName}/Packages"
	},
	
	{
		"Name": ".Module.UnityCommonExternals",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonAds",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonFlurry",
		"Path": f"{oProjName}/Packages"
	},
	
	{
		"Name": ".Module.UnityCommonFacebook",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonFirebase",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonAppsFlyer",
		"Path": f"{oProjName}/Packages"
	},
	
	{
		"Name": ".Module.UnityCommonGameCenter",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonPurchase",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonNotification",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonPlayfab",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": ".Module.UnityCommonImporter",
		"Path": f"{oProjName}/Packages"
	},

	{
		"Name": "NativePlugins",
		"Path": oProjName
	},

	{
		"Name": "UnityPackages",
		"Path": oProjName
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
	oPath = FindPath(f"{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}")
	oModulePath = FindPath(f".git/modules/Client/{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}")

	# 서브 모듈이 존재 할 경우
	if os.path.exists(oPath):
		os.system(f"git submodule deinit -f \"{oPath}\"")
		os.system(f"git rm -f \"{oPath}\"")

	# 윈도우즈 플랫폼 일 경우
	if "WINDOWS" in platform.system().upper():
		os.system(f"rmdir /s /q \"{oPath}\"")
		os.system(f"rmdir /s /q \"{oModulePath}\"")
	else:
		os.system(f"rm -rf \"{oPath}\"")
		os.system(f"rm -rf \"{oModulePath}\"")
