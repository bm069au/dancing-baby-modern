# Dancing Baby Modern — Windows screensaver

Private personal restoration backup. This repository contains the C# screensaver source, manifest, build/media preparation scripts, conversion manifest and original restoration notes.

The complete working portable package is attached to GitHub Release backup-2026-10-22 as DancingBabyModern-Windows.zip. Extract the entire ZIP together: the .scr requires its Videos and Music directories. Music is optional and off by default. See DancingBabyModern/README.txt for configuration and installation instructions.

Build with build.ps1 on Windows using the installed .NET Framework compiler. prepare_media.py expects the separately held original CD files and a local ffmpeg installation; its current ffmpeg path is machine-specific. Original CD ISO/extraction and unrelated old installer are not committed. The release preserves the converted media already used by the working screensaver. No public asset distribution or new runtime test is claimed.

Backup date: 22 October 2026. Existing restoration notes preserve historical validation and limitations; Brett subsequently reported enjoying the screensaver.
