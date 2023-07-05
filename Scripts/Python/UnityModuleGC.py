import os
import sys

oProjName = sys.argv[1]

os.system(f"python UnityModuleUpdater.py \"{oProjName}\"")
os.system(f"python UnityModuleCmdExecuter.py \"{oProjName}\" \"git gc --force\"")
