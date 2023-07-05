import os
import sys

oProjName = sys.argv[1]
oBranchName = sys.argv[2]
oProjRootPath = sys.argv[3]

oSubmoduleInfos = [
	{
		"Name": "NativePlugins",
		"Path": oProjName,
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_nativeplugins.git"
	},

	{
		"Name": "UnityPackages",
		"Path": oProjName,
		"URL": "https://gitlab.com/9tapmodule.repository/0300000001.module_unitypackages.git"
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

		os.system(f"git submodule add -f \"{oURL}\" \"{oFullPath}\"")

	oSubmodulePath = f"{oProjRootPath}/{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}" if oProjRootPath else f"{oSubmoduleInfo['Path']}/{oSubmoduleInfo['Name']}"
	os.system(f"git submodule set-branch --branch \"{oBranchName}\" \"{oSubmodulePath}\"")

os.system(f"python UnityModuleRemoteURLUpdater.py \"{oProjName}\"")
