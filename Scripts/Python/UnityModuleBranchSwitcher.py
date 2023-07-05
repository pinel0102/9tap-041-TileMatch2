import os
import sys

oProjName = sys.argv[1]
oBranchName = sys.argv[2]
oOriginBranchName = sys.argv[3]

os.system(f"python UnityModuleUpdater.py \"{oProjName}\"")

# 원본 브랜치 이름이 유효 할 경우
if oOriginBranchName:
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git checkout -b {oBranchName} {oOriginBranchName}\"")
else:
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git checkout {oBranchName}\"")
