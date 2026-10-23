# inFAMOUS Second Son (PS4 game in the iss_pc PC port) - Arabic translation installer.
# Right-click this file > Run with PowerShell.
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Windows.Forms
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$m = Get-Content -Raw -Encoding UTF8 -Path (Join-Path $here "patch\manifest.json") | ConvertFrom-Json
$title = "inFAMOUS Second Son - Arabic Translation"
function Show-Msg([string]$ar, [string]$en, [string]$icon = "Information") {
    if ($env:ISSAR_QUIET) { Write-Output "[$icon] $en"; return }   # scripted test runs
    $enL = ($en -split "`r`n" | ForEach-Object { [string][char]0x202A + $_ + [string][char]0x202C }) -join "`r`n"
    $opts = [System.Windows.Forms.MessageBoxOptions]::RtlReading -bor [System.Windows.Forms.MessageBoxOptions]::RightAlign
    [void][System.Windows.Forms.MessageBox]::Show("$ar`r`n`r`n$enL", $title, "OK", $icon, "Button1", $opts)
}
function Get-Sha([string]$p) {
    $s = [System.IO.File]::OpenRead($p)
    try { return [System.BitConverter]::ToString([System.Security.Cryptography.SHA256]::Create().ComputeHash($s)).Replace("-", "") }
    finally { $s.Dispose() } }
function Test-Game([string]$dir) { return ($dir -and (Test-Path (Join-Path $dir "art\cache\lang_english_text.xpps")) -and (Test-Path (Join-Path $dir "eboot.bin"))) }
function Find-Game {
    if ($env:ISSAR_GAME) { return $env:ISSAR_GAME }
    $saved = Join-Path $here "game_folder.txt"
    $cands = @((Split-Path -Parent $here), (Split-Path -Parent (Split-Path -Parent $here)))
    if (Test-Path $saved) { $cands = @((Get-Content -Raw -Encoding UTF8 $saved).Trim()) + $cands }
    foreach ($c in $cands) { if (Test-Game $c) { return (Resolve-Path $c).Path } }
    $dlg = New-Object System.Windows.Forms.FolderBrowserDialog
    $dlg.Description = "اختر مجلد اللعبة (اللي فيه eboot.bin ومجلد art)`r`nChoose the game folder (the one with eboot.bin and the art folder)"
    while ($dlg.ShowDialog() -eq "OK") {
        if (Test-Game $dlg.SelectedPath) { return $dlg.SelectedPath }
        Show-Msg "هذا مو مجلد اللعبة." "This is not the game folder." "Warning"
    }
    return $null
}
function Copy-Range($from, $to, [long]$off, [long]$n, [byte[]]$buf) {
    [void]$from.Seek($off, "Begin")
    while ($n -gt 0) {
        $k = $from.Read($buf, 0, [int][Math]::Min($buf.Length, $n))
        if ($k -le 0) { throw "short read" }
        $to.Write($buf, 0, $k); $n -= $k
    }
}

if (Get-Process -Name "inFAMOUS Second Son", "tlg-probe-v2" -ErrorAction SilentlyContinue) {
    Show-Msg "أغلق اللعبة أولًا ثم حاول مرة أخرى." "Close the game first, then try again." "Warning"; exit 1 }
$game = Find-Game
if (-not $game) { exit 1 }
$todo = @()
foreach ($f in $m.files) {
    $dst = Join-Path $game $f.path
    $h = Get-Sha $dst
    if ($h -eq $f.patched_sha256) { continue }
    if ($h -ne $f.stock_sha256) {
        Show-Msg "الملف $($f.path) ما يطابق النسخة الأمريكية الأصلية (CUSA00223، الإصدار 1.07). استخرج اللعبة والتحديث من جديد ثم ثبّت التعريب." "$($f.path) does not match the original US game (CUSA00223, version 1.07). Extract the game and the update again, then install the translation." "Error"; exit 1 }
    $todo += $f
}
if ($todo.Count -eq 0) {
    Show-Msg "التعريب مثبّت مسبقًا." "The Arabic translation is already installed."; exit 0 }
$bak = Join-Path $game "InfamousArabic-backup"
foreach ($f in $todo) {
    $b = Join-Path $bak $f.path
    if (-not (Test-Path $b)) {
        New-Item -ItemType Directory -Force (Split-Path -Parent $b) | Out-Null
        Copy-Item (Join-Path $game $f.path) $b -Force
    }
}
$data = [System.IO.File]::OpenRead((Join-Path $here "patch\data.bin"))
$buf = [byte[]]::new(1048576)
$zero = [byte[]]::new(1048576)
try {
    foreach ($f in $todo) {
        $src = [System.IO.File]::OpenRead((Join-Path $bak $f.path))
        $tmp = (Join-Path $game $f.path) + ".arabic-tmp"
        $out = [System.IO.File]::Create($tmp)
        try {
            foreach ($op in $f.ops) {
                $kind = [int]$op[0]; $off = [long]$op[1]; $n = [long]$op[2]
                if ($kind -eq 0) { Copy-Range $src $out $off $n $buf }
                elseif ($kind -eq 1) { Copy-Range $data $out $off $n $buf }
                else { while ($n -gt 0) { $k = [int][Math]::Min($zero.Length, $n); $out.Write($zero, 0, $k); $n -= $k } }
            }
        } finally { $out.Dispose(); $src.Dispose() }
        if ((Get-Sha $tmp) -ne $f.patched_sha256) {
            Remove-Item -Force $tmp
            Show-Msg "فشل بناء الملف $($f.path). لم يتغير هذا الملف. شغّل Uninstall.ps1." "Building $($f.path) failed; that file was not changed. Run Uninstall.ps1." "Error"; exit 1 }
        $dst = Join-Path $game $f.path
        if (Test-Path $dst) { Set-ItemProperty -Path $dst -Name IsReadOnly -Value $false }
        Move-Item -Force $tmp $dst
    }
} finally { $data.Dispose() }
Set-Content -Encoding UTF8 -Path (Join-Path $here "game_folder.txt") -Value $game
Show-Msg "تم تثبيت التعريب.`r`nخل لغة اللعبة في البورت على English. أول تشغيل بعد التثبيت ياخذ وقت طويل (البورت يعيد تجهيز الشيدرات).`r`nفعّل الترجمة النصية من «خيارات».`r`n$game" "The Arabic translation is installed.`r`nKeep the game language on English in the port. The first start after installing takes a long time (the port compiles its shaders again).`r`nTurn on subtitles in Options.`r`n$game"
