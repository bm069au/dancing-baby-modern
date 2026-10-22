# Dancing Baby Windows restoration
Revision: DANCING-BABY-2026-10-19-R1
Summary: https://drive.google.com/file/d/1OEGDqinlN6sqIv-f0Xebe7AO5HdSijUR/view

## Purpose, sources and Brett's contribution
Brett supplied DancingBabyScreensaver_99228.zip and recalled a favourite screensaver around 1999. He asked whether it could be adapted to current Windows .scr, suggested Git projects might share the graphics, then explicitly directed: "ok grab that iso and work from that". This is a separate personal restoration, not Fairybread, Palace or Duke development. Stable START HERE and full standing order read; no Dancing Baby entry found in that index. No claim of exhaustive unrelated record audit.

Initial ZIP contains only Setup_0023.exe, internally FRIENDS App, signed by 10sIT Oy. Static inspection only; not executed. The original CD was located at https://archive.org/details/the-complete-dancing-baby-screensaver-cd-rom . Original 1998 Atomic Media/NCDS README and READ1ST retained. GitHub alternatives found were placeholders/recreations; Lively supports generic video screensavers but was not installed or used. Original animation and music rights remain with their owners; no public upload of assets or executables.

## Requirements and implementation
DB-001 AGREED: restore original Dancing Baby assets for current Windows screensaver use. Implemented locally, manual acceptance pending.
DB-002 IMPLEMENTED ASSISTANT CHOICE: portable 64-bit .NET Framework/WPF player, H.264/AAC video conversion, source images unchanged in character and resolution; no old installer/components installed. Existing local Jellyfin ffmpeg used offline; Jellyfin service/configuration unchanged.
DB-003 AGREED: Brett explicitly confirmed music must be optional, recalling the original music synchronization as fun but annoying. Silent default implemented. Other IMPLEMENTED ASSISTANT CHOICES: choose animation, optional original MIDI/WAV music or video sound, shuffle after about 30 seconds at clip boundary. Music takes priority over video audio and loops independently; beat-synchronised dancing is not implemented. Same animation on every screen, aspect ratio preserved. Windows screensaver /s, /c and /p entry points. Per-monitor DPI manifest; settings in LocalAppData/DancingBabyModern/settings.json. Full-screen exits on key, click or mouse movement. Windows owns lock-on-resume; user must enable it if desired. Visual acceptance remains pending.

## Actual artifacts and evidence
Local root: E:\\Visual_studio\\FairybreadBridge\\DancingBabyInspection
Downloaded DancingBaby1998.iso: 277172224 bytes; SHA256 E7421F258217233BB7CCAF741E741D85F6F966891AD2430D20D6966355D0D484.
CD extracted without mounting or executing originals: 195 AVI + 8 MOV; 1818 MIDI + 5 WAV. SHA256 dedup gives 194 distinct videos, 9 duplicates. All 194 converted successfully, zero failures; full original-source hashes and conversion diagnostics in conversion-manifest.json. 1823 music files copied.
DancingBabyModern/DancingBaby.scr compiled successfully. Final SHA256 336B4808983248BA32AF29D352A6DA76954BEE01C441DEEB2C55906C178CBDC3. C# source, manifest, build.ps1 and prepare_media.py retained alongside CD and ISO. Portable ZIP DancingBabyModern-Windows.zip: 58054307 bytes. Videos/Music must remain beside .scr.

## Validation and limits
All conversion operations completed, zero conversion failures; no claim every animation visually reviewed. Bounded Windows MediaPlayer check opened BABYCHA1 at 260x200, duration 8 seconds, rendered a frame with 7570 nonblack pixels, exit 0. Captured frame visually inspected: original dancing baby visible against black. Initial shell-based invocation produced no diagnostic; corrected direct process invocation passed. Final subsequent changes: direct-process preview launch, DPI manifest and settings scroll container; compile passed, no additional runtime check of those changes. No UI automation, paid AI/API, other agents or extra downloads besides ISO.
Manual checks OPEN: settings layout, full-screen preview/exit, Windows install and preview-host behaviour, multi-monitor/mixed-DPI operation, MIDI/WAV and embedded audio. Low-resolution originals remain low-resolution; no HD remake. Windows N may need Media Feature Pack. Original password mechanism/CD audio not restored; Windows controls secure resume. Codecs converted rather than installed.

## Next bounded user exercise
Open Configure.cmd, select 029_BABYCHA1 (or another animation), use Preview full screen and check exit. Choose music if wanted. Save, then right-click DancingBaby.scr > Install; choose wait time and optional Windows logon-on-resume. Keep whole directory in a permanent location. Moving it requires reselecting paths/reinstalling.

## Save and Git state
This root is not a Git repository; no commit, push or remote-equality claim. Local ISO, extracted CD, videos/music, executable, ZIP and diagnostic artifacts excluded from Drive prose save. Only this detailed record and matching summary are saved privately, with reciprocal links and matching revision verified before updating stable index. Original chat and source CD remain provenance. Save receipt appended after actual Drive results.
