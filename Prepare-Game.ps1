# inFAMOUS Second Son: build the game folder the iss_pc port needs from your two PS4 package files
# (the game PKG and the 1.07 update PKG). Your PKG files are only read, never changed.
#
# Needs: the .NET runtime (6 or newer) and LibOrbisPkg PkgTool.Core 0.2 (GitHub: maxton/LibOrbisPkg, file
# PkgTool.Core-0.2.231.zip). Run: right-click > Run with PowerShell.
# Scripted: env ISS_GAME_PKG, ISS_UPDATE_PKG, ISS_PKGTOOL (folder with PkgTool.Core.dll), ISS_OUT.

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Windows.Forms

function Show-Msg($ar, $en, $kind = 'Information') {
    if ($env:ISS_QUIET) { Write-Host "[$kind] $en"; return }
    [System.Windows.Forms.MessageBox]::Show("$ar`r`n`r`n$en", 'inFAMOUS Second Son', 'OK', $kind) | Out-Null
}
function Pick-File($title) {
    $d = New-Object System.Windows.Forms.OpenFileDialog
    $d.Title = $title; $d.Filter = 'PS4 package (*.pkg)|*.pkg'
    if ($d.ShowDialog() -ne 'OK') { exit 1 }
    $d.FileName
}
function Pick-Folder($title) {
    $d = New-Object System.Windows.Forms.FolderBrowserDialog
    $d.Description = $title
    if ($d.ShowDialog() -ne 'OK') { exit 1 }
    $d.SelectedPath
}

# sce_sys files stored in the clear in the PKG (entry id -> file name), as LibOrbisPkg names them
$EntryFiles = @{
    0x1000 = 'param.sfo'; 0x1001 = 'playgo-chunk.dat'; 0x1002 = 'playgo-chunk.sha'; 0x1003 = 'playgo-manifest.xml'
    0x1004 = 'pronunciation.xml'; 0x1005 = 'pronunciation.sig'; 0x1006 = 'pic1.png'; 0x1007 = 'pubtoolinfo.dat'
    0x1008 = 'app/playgo-chunk.dat'; 0x1009 = 'app/playgo-chunk.sha'; 0x100A = 'app/playgo-manifest.xml'
    0x100B = 'shareparam.json'; 0x0408 = 'origin-deltainfo.dat'; 0x1200 = 'icon0.png'; 0x1220 = 'pic0.png'
    0x1240 = 'snd0.at9'; 0x1260 = 'changeinfo/changeinfo.xml'; 0x1400 = 'trophy/trophy00.trp'
}
for ($i = 0; $i -lt 30; $i++) { $EntryFiles[0x1261 + $i] = ('changeinfo/changeinfo_{0:D2}.xml' -f $i) }

function Read-BE32([byte[]]$b, [int]$o) { ([uint32]$b[$o] -shl 24) -bor ([uint32]$b[$o + 1] -shl 16) -bor ([uint32]$b[$o + 2] -shl 8) -bor [uint32]$b[$o + 3] }

function Export-SceSys($pkg, $dest) {
    $fs = [System.IO.File]::OpenRead($pkg)
    try {
        $head = New-Object byte[] 32; [void]$fs.Read($head, 0, 32)
        if ($head[0] -ne 0x7F -or $head[1] -ne 0x43 -or $head[2] -ne 0x4E -or $head[3] -ne 0x54) { throw "$pkg is not a PS4 package" }
        $count = Read-BE32 $head 0x10; $table = Read-BE32 $head 0x18
        $raw = New-Object byte[] ($count * 32); $fs.Position = $table; [void]$fs.Read($raw, 0, $raw.Length)
        for ($i = 0; $i -lt $count; $i++) {
            $id = Read-BE32 $raw ($i * 32); $flags = Read-BE32 $raw ($i * 32 + 8)
            $off = Read-BE32 $raw ($i * 32 + 16); $size = Read-BE32 $raw ($i * 32 + 20)
            $name = $EntryFiles[[int]$id]
            if (-not $name -or ($flags -band 0x80000000)) { continue }   # unknown or encrypted entry
            $path = Join-Path $dest ($name -replace '/', '\')
            New-Item -ItemType Directory -Force (Split-Path $path) | Out-Null
            $buf = New-Object byte[] $size; $fs.Position = $off; [void]$fs.Read($buf, 0, $size)
            [System.IO.File]::WriteAllBytes($path, $buf)
        }
    } finally { $fs.Close() }
}

function Extract-Pkg($pkg, $out) {
    $env:DOTNET_ROLL_FORWARD = 'Major'                # PkgTool.Core 0.2 targets .NET Core 3.0
    & dotnet (Join-Path $tool 'PkgTool.Core.dll') pkg_extract $pkg $out
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path (Join-Path $out 'uroot'))) { throw "PkgTool could not extract $pkg" }
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Show-Msg 'ثبّت .NET Runtime (الإصدار 6 أو أحدث) من موقع مايكروسوفت، وبعدين شغّل هذا الملف مرة ثانية.' 'Install the .NET Runtime (version 6 or newer) from Microsoft, then run this file again.' 'Error'; exit 1
}
$gamePkg = if ($env:ISS_GAME_PKG) { $env:ISS_GAME_PKG } else { Pick-File 'Game PKG (CUSA00223) / ملف اللعبة' }
$updPkg = if ($env:ISS_UPDATE_PKG) { $env:ISS_UPDATE_PKG } else { Pick-File 'Update 1.07 PKG / ملف التحديث 1.07' }
$tool = if ($env:ISS_PKGTOOL) { $env:ISS_PKGTOOL } else { Pick-Folder 'PkgTool.Core folder (PkgTool.Core.dll) / مجلد PkgTool.Core' }
$outDir = if ($env:ISS_OUT) { $env:ISS_OUT } else { Pick-Folder 'Where to make the game folder (needs ~35 GB) / وين تبي مجلد اللعبة (يحتاج 35 جيجا)' }
if (-not (Test-Path (Join-Path $tool 'PkgTool.Core.dll'))) { Show-Msg 'ما لقيت PkgTool.Core.dll في المجلد اللي اخترته.' 'PkgTool.Core.dll is not in that folder.' 'Error'; exit 1 }

$game = Join-Path $outDir 'inFAMOUS Second Son'
if (Test-Path $game) { Show-Msg "المجلد موجود من قبل: $game" "The folder already exists: $game" 'Error'; exit 1 }
$tmp = Join-Path $outDir 'iss-extract-tmp'
New-Item -ItemType Directory -Force $tmp | Out-Null
Write-Host 'Extracting the game PKG (this takes a while)...'
Extract-Pkg $gamePkg (Join-Path $tmp 'base')
Write-Host 'Extracting the update PKG...'
Extract-Pkg $updPkg (Join-Path $tmp 'update')
Write-Host 'Merging the update over the game...'
Move-Item (Join-Path $tmp 'base\uroot') $game
robocopy (Join-Path $tmp 'update\uroot') $game /E /MOVE /IS /IT /NFL /NDL /NJH /NJS /NP | Out-Null
if ($LASTEXITCODE -ge 8) { throw 'merging the update failed' }
Write-Host 'Writing sce_sys...'
Export-SceSys $gamePkg (Join-Path $game 'sce_sys')
Export-SceSys $updPkg (Join-Path $game 'sce_sys')               # the update's param.sfo (1.07) wins
Remove-Item -Recurse -Force $tmp
if (-not (Test-Path (Join-Path $game 'eboot.bin'))) { Show-Msg 'ما طلع ملف eboot.bin. تأكد من ملفات الـ PKG.' 'eboot.bin is missing. Check your PKG files.' 'Error'; exit 1 }
Show-Msg "تم تجهيز مجلد اللعبة:`r`n$game`r`nاختاره في البورت (Game folder)." "The game folder is ready:`r`n$game`r`nChoose it in the port (Game folder)."
