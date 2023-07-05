import os
import sys

oProjName = sys.argv[1]
oTagName = sys.argv[2]
oReplaceTagName = sys.argv[3]

oCurPath = os.getcwd()
os.system(f"python UnityModuleUpdater.py \"{oProjName}\"")

oCmdInfos = [
	{
		"Cmd": f"git tag -d \"{oTagName}\"",
		"SubmoduleCmd": f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git tag -d {oTagName}\""
	},

	{
		"Cmd": f"git push origin --delete \"{oTagName}\"",
		"SubmoduleCmd": f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git push origin --delete {oTagName}\""
	},

	{
		"Cmd": f"git tag \"{oReplaceTagName}\"",
		"SubmoduleCmd": f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git tag {oReplaceTagName}\""
	},

	{
		"Cmd": f"git push origin --tags",
		"SubmoduleCmd": f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git push origin --tags\""
	},
]

for oCmdInfo in oCmdInfos:
	os.chdir(f"{oCurPath}/../..")

	try:
		os.system(oCmdInfo["Cmd"])
	finally:
		os.chdir(oCurPath)
		os.system(oCmdInfo["SubmoduleCmd"])
