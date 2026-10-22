import pathlib, hashlib, subprocess, json, shutil

root = pathlib.Path(__file__).parent
cd = root / 'CD'
out = root / 'DancingBabyModern'
(out / 'Videos').mkdir(parents=True, exist_ok=True)
(out / 'Music').mkdir(exist_ok=True)
ffmpeg = r'C:\Program Files\Jellyfin\Server\ffmpeg.exe'
seen = {}
records = []
for p in sorted(cd.rglob('*')):
    if p.suffix.lower() not in ('.avi', '.mov'):
        continue
    digest = hashlib.sha256(p.read_bytes()).hexdigest()
    if digest in seen:
        records.append(dict(source=str(p.relative_to(cd)), duplicate_of=seen[digest], sha256=digest))
        continue
    name = '%03d_%s.mp4' % (len(seen) + 1, p.stem)
    seen[digest] = name
    dest = out / 'Videos' / name
    result = subprocess.run([ffmpeg, '-nostdin', '-hide_banner', '-loglevel', 'error', '-y',
        '-i', str(p), '-map', '0:v:0', '-map', '0:a?', '-vf',
        'scale=trunc(iw/2)*2:trunc(ih/2)*2', '-c:v', 'libx264', '-preset', 'fast',
        '-crf', '18', '-pix_fmt', 'yuv420p', '-c:a', 'aac', '-movflags', '+faststart', str(dest)],
        capture_output=True, text=True)
    records.append(dict(source=str(p.relative_to(cd)), output=name, sha256=digest,
                        success=result.returncode == 0, diagnostics=result.stderr.strip()))
    print(name, 'OK' if result.returncode == 0 else result.stderr[:200], flush=True)
for folder in ('MIDI', 'FILES'):
    for p in (cd / folder).rglob('*'):
        if p.suffix.lower() in ('.mid', '.wav'):
            dest = out / 'Music' / folder / p.relative_to(cd / folder)
            dest.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(p, dest)
shutil.copy2(cd / 'README.TXT', out / 'Original-CD-README.txt')
shutil.copy2(cd / 'READ1ST.TXT', out / 'Original-CD-READ1ST.txt')
(root / 'conversion-manifest.json').write_text(json.dumps(records, indent=2), encoding='utf-8')
print('Complete:', len(seen), 'unique videos;', sum(r.get('success', False) for r in records), 'converted', flush=True)
