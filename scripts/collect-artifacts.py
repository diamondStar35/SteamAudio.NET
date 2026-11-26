import os,pathlib,shutil
root=pathlib.Path(__file__).resolve().parents[1];out=root/'.cache/upload';out.mkdir(parents=True,exist_ok=True)
target=os.environ['STEAM_TARGET'];mode=os.environ['STEAM_LINKAGE']
paths=['Native/'+('static' if mode=='static' else 'runtimes')+'/'+target,'Native/notices','artifacts/'+target,'Tests/TestResults']
for path in paths:
    source=root/path
    if source.exists():shutil.copytree(source,out/path,dirs_exist_ok=True)
