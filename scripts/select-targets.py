"""Validate manual workflow inputs before expanding jobs."""
import json, os, pathlib
spec=json.loads((pathlib.Path(__file__).resolve().parents[1]/'Native/sdk.json').read_text())
requested=os.environ['TARGETS'].strip();targets=spec['targets'] if requested=='all' else list(dict.fromkeys(t.strip() for t in requested.split(',')))
mode=os.environ['LINKAGE']
if mode not in ['static','dynamic','both'] or not targets or any(t not in spec['targets'] for t in targets):raise SystemExit('Invalid target or linkage selection')
if mode=='dynamic' and 'ios-arm64' in targets:raise SystemExit('iOS has static assets only; exclude ios-arm64 or select static/both')
runners={'win-x64':'windows-2022','linux-x64':'ubuntu-24.04','osx-x64':'macos-15-intel','osx-arm64':'macos-15','android-arm64':'ubuntu-24.04','ios-arm64':'macos-15'}
rows=[{'target':t,'linkage':link,'runner':runners[t]} for t in targets for link in (['static'] if t=='ios-arm64' else ['static','dynamic'] if mode=='both' else [mode])]
print('value='+json.dumps({'include':rows},separators=(',',':')))
