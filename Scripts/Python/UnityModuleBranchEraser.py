import os
import sys

oProjName = sys.argv[1]
oBranchName = sys.argv[2]

os.system(f"python UnityModuleUpdater.py \"{oProjName}\"")
os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git branch -D {oBranchName}\"")
os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git push origin --delete {oBranchName}\"")
