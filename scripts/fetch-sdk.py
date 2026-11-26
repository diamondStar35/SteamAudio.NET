"""Fetch and verify Valve's pinned SDK; stage shared assets without building native code."""
import hashlib, json, pathlib, shutil, urllib.request, zipfile
root=pathlib.Path(__file__).resolve().parents[1]
spec=json.loads((root/'Native/sdk.json').read_text())
cache=root/'.cache/sdk'; cache.mkdir(parents=True,exist_ok=True)
archive=cache/('steamaudio_'+spec['version']+'.zip')
if not archive.exists(): urllib.request.urlretrieve(spec['url'],archive)
with archive.open('rb') as stream:
    if hashlib.file_digest(stream,'sha256').hexdigest()!=spec['sha256']: raise RuntimeError('SDK SHA-256 mismatch')
with zipfile.ZipFile(archive) as package:
    for entry in package.infolist():
        if not (cache/entry.filename).resolve().is_relative_to(cache.resolve()): raise RuntimeError('Unsafe archive path')
    package.extractall(cache)
for rid,folder in {'win-x64':'windows-x64','linux-x64':'linux-x64','osx-x64':'osx','osx-arm64':'osx','android-arm64':'android-armv8'}.items():
    target=root/'Native/runtimes'/rid/'native';target.mkdir(parents=True,exist_ok=True)
    for library in (cache/'steamaudio/lib'/folder).iterdir():
        if library.is_file() and library.suffix in {'.dll','.so','.dylib'}: shutil.copy2(library,target/library.name)
notices=root/'Native/notices';notices.mkdir(parents=True,exist_ok=True)
shutil.copy2(cache/'steamaudio/THIRDPARTY.md',notices/'Valve-THIRDPARTY.md')
print('Verified SDK '+spec['version']+' shared assets staged.')
