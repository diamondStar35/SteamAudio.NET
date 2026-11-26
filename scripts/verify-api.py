"""Audit handwritten native declarations against the pinned C header; never writes binding source."""
import pathlib, re, xml.etree.ElementTree as xml
root=pathlib.Path(__file__).resolve().parents[1]
header=(root/'Native/include/phonon.h').read_text()
clean=re.sub(r'/\*[\s\S]*?\*/|//[^\n]*','',header)
handles=set(re.findall(r'DECLARE_OPAQUE_HANDLE\((IPL\w+)\);',clean))
callbacks={name:(ret,args) for ret,name,args in re.findall(r'typedef\s+(\w+\s*\*?)\s*\(IPLCALL\s*\*\s*(IPL\w+)\)\((.*?)\);',clean,re.S)}
def name(s):return s.removeprefix('IPL').replace('HRTF','Hrtf').replace('SIMD','Simd')
primitives={'void':'void','char':'byte','int':'int','float':'float','IPLint8':'sbyte','IPLuint8':'byte','IPLint16':'short','IPLuint16':'ushort','IPLint32':'int','IPLuint32':'uint','IPLint64':'long','IPLuint64':'ulong','IPLfloat32':'float','IPLfloat64':'double','IPLbyte':'byte','IPLsize':'nuint','IPLstring':'byte*','IPLbool':'Bool','IPLerror':'Error'}
def ctype(s):
    s=re.sub(r'\bconst\b|\bvolatile\b','',s).strip();stars=s.count('*');s=s.replace('*','').strip()
    if s in callbacks:
        ret,args=callbacks[s];params=[] if args.strip() in ['', 'void'] else [ctype(re.sub(r'\b\w+\s*$','',x).strip()) for x in args.split(',')]
        return 'delegate*unmanaged<'+','.join(params+[ctype(ret)])+'>'
    return primitives.get(s,name(s)+('Handle' if s in handles else ''))+'*'*stars
native={n:(ctype(ret),[ctype(re.sub(r'\b\w+\s*$','',p).strip()) for p in args.split(',')] if args.strip() not in ['','void'] else []) for ret,n,args in re.findall(r'IPLAPI\s+([\w* ]+?)\s+IPLCALL\s+(ipl\w+)\s*\((.*?)\);',clean,re.S)}
source='\n'.join(p.read_text() for p in (root/'Src/Interop').glob('*.cs'))
managed={}
# Split parameters only outside function-pointer generic angle brackets.
def split(args):
    values=[];start=0;depth=0
    for i,char in enumerate(args):
        if char=='<':depth+=1
        elif char=='>':depth-=1
        elif char==',' and depth==0:values.append(args[start:i]);start=i+1
    if args.strip():values.append(args[start:])
    return values
for entry,ret,args in re.findall(r'\[LibraryImport\(Library, EntryPoint = "(ipl\w+)"\)\]\s*public static partial (.*?) \w+\((.*?)\);',source,re.S):
    managed[entry]=(ret.strip(),[re.sub(r'\s+','',re.sub(r'\s+@?\w+$','',p.strip())) for p in split(args)])
if native!=managed:
    for key in sorted(native.keys()|managed.keys()):
        if native.get(key)!=managed.get(key):print(key, 'SDK:',native.get(key),'C#:',managed.get(key))
    raise SystemExit('Native API signature mismatch')
if len(native)!=216:raise SystemExit('Unexpected SDK API surface; update the audited version explicitly')
for file in (root/'Src').rglob('*.cs'):
    if 'obj' in file.parts or 'bin' in file.parts:continue
    docs='\n'.join(line.split('///',1)[1] for line in file.read_text().splitlines() if '///' in line)
    xml.fromstring('<documentation>'+docs+'</documentation>')
    if re.search(r'\\[A-Za-z]+|@(param|return|brief|remarks|c)\b',docs):raise SystemExit('Unconverted Doxygen in '+str(file))
print('216 native signatures match SDK headers; XML documentation is well formed.')
