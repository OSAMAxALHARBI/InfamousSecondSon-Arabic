# inFAMOUS Second Son - Arabic translation: restore the original game files.
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
function Test-Game([string]$dir) { return ($dir -and (Test-Path (Join-Path $dir "art\cache\lang_english_text.xpps")) -and (Test-Path (Join-Path $dir "eboot.bin"))) }
if (Get-Process -Name "inFAMOUS Second Son", "tlg-probe-v2" -ErrorAction SilentlyContinue) {
    Show-Msg "أغلق اللعبة أولًا ثم حاول مرة أخرى." "Close the game first, then try again." "Warning"; exit 1 }
$game = $env:ISSAR_GAME
$saved = Join-Path $here "game_folder.txt"
if (-not $game -and (Test-Path $saved)) { $game = (Get-Content -Raw -Encoding UTF8 $saved).Trim() }
if (-not (Test-Game $game)) { $game = Split-Path -Parent $here }
if (-not (Test-Game $game)) {
    $dlg = New-Object System.Windows.Forms.FolderBrowserDialog
    $dlg.Description = "اختر مجلد اللعبة`r`nChoose the game folder"
    if ($dlg.ShowDialog() -ne "OK" -or -not (Test-Game $dlg.SelectedPath)) { exit 1 }
    $game = $dlg.SelectedPath
}
$bak = Join-Path $game "InfamousArabic-backup"
if (-not (Test-Path $bak)) {
    Show-Msg "ما لقيت النسخة الاحتياطية (InfamousArabic-backup). لإرجاع الملفات الأصلية استخرج اللعبة والتحديث من جديد." "No backup found (InfamousArabic-backup). To get the original files back, extract the game and the update again." "Warning"; exit 1 }
foreach ($f in $m.files) {
    $b = Join-Path $bak $f.path
    $dst = Join-Path $game $f.path
    if (Test-Path $b) {
        if (Test-Path $dst) { Set-ItemProperty -Path $dst -Name IsReadOnly -Value $false }
        Copy-Item $b $dst -Force }
}
Remove-Item -Recurse -Force $bak
Show-Msg "تمت إزالة التعريب وعادت ملفات اللعبة الأصلية." "The Arabic translation was removed and the original game files are back."
