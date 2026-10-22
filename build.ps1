$ErrorActionPreference = 'Stop'
$babyRoot = $PSScriptRoot
$babyWpf = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\WPF'
& 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe' /nologo /target:winexe /platform:x64 /optimize+ "/win32manifest:$babyRoot\app.manifest" "/out:$babyRoot\DancingBabyModern\DancingBaby.scr" "/reference:$babyWpf\PresentationCore.dll" "/reference:$babyWpf\PresentationFramework.dll" "/reference:$babyWpf\WindowsBase.dll" /reference:System.Xaml.dll /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll "$babyRoot\DancingBaby.cs"
if ($LASTEXITCODE -ne 0) { throw 'Screensaver build failed.' }
