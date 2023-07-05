import os
import sys
import platform

oProjName = sys.argv[1]
oCommitMsg = sys.argv[2]
oBranchName = sys.argv[3]

os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git add .\"")
oCommitMsg = f"\"{oCommitMsg}\"" if "WINDOWS" in platform.system().upper() else oCommitMsg

# 윈도우즈 플랫폼 일 경우
if "WINDOWS" in platform.system().upper():
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git commit -m \"{oCommitMsg}\"\"")
else:
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git commit -m \'{oCommitMsg}\'\"")

# 브랜치 이름이 유효 할 경우
if oBranchName:
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git push origin -u {oBranchName}\"")
else:
	os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git push\"")
	
