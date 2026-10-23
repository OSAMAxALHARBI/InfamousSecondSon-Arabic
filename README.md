# تعريب inFAMOUS Second Son | inFAMOUS Second Son Arabic

<div dir="rtl">

تعريب inFAMOUS Second Son (نسخة PS4) لبورت iss_pc على الكمبيوتر: الحوارات والمشاهد والأخبار التلفزيونية والقوائم والمهام والقدرات والترقيات والنصائح والجوائز.

التعريب ما يحتوي أي ملف من ملفات اللعبة. البرنامج يعدّل ملفات النص اللي عندك أنت.

## اللي تحتاجه

1. **كمبيوتر:** ويندوز 10 أو 11 (64 بت)، كرت شاشة يدعم Vulkan 1.3 (يفضّل 6 جيجا ذاكرة شاشة أو أكثر)، وحوالي 35 جيجا مساحة.
2. **بورت iss_pc الإصدار 1.0** (مجاني): https://github.com/HayatoG/iss_pc/releases/tag/v1.0 ، حمّل `infamous-windows-x86_64.zip`.
3. **نسختك من اللعبة على PS4**، النسخة الأمريكية **CUSA00223**، كملفين PKG: ملف اللعبة وملف **التحديث 1.07** (الأسماء تختلف، مثل `...CUSA00223-USA-Game-PS4.pkg` و `...CUSA00223-USA-Update-v1.07-PS4.pkg`). ملفات الـ DLC مو مطلوبة.
4. **لفك ملفات الـ PKG:** [.NET Runtime](https://dotnet.microsoft.com/download/dotnet) (الإصدار 6 أو أحدث، من مايكروسوفت) وأداة LibOrbisPkg PkgTool.Core 0.2: https://github.com/maxton/LibOrbisPkg/releases/tag/v0.2 ، حمّل `PkgTool.Core-0.2.231.zip` وفك ضغطه.

## الخطوات

1. حمّل `InfamousSecondSon-Arabic-1.0.zip` من [Releases](../../releases/latest) وفك ضغطه.
2. افتح `InfamousArabic.exe`. إذا طلعت رسالة "Windows protected your PC" اضغط More info ثم Run anyway (البرنامج مو موقّع رقميًا).
3. **الخطوة ٢ في البرنامج («جهّز مجلد اللعبة»):** اختر ملف اللعبة PKG، ملف التحديث 1.07 PKG، مجلد PkgTool.Core (اللي فيه `PkgTool.Core.dll`)، والمكان اللي تبي فيه مجلد اللعبة. اضغط «جهّز مجلد اللعبة» وانتظر (عندي ياخذ حوالي 3 دقائق). يطلع لك مجلد `inFAMOUS Second Son`. ملفات الـ PKG حقتك ما تتغيّر.
4. **البورت:** فك ضغطه وافتح `inFAMOUS Second Son.exe`. في **Game folder** اختر مجلد اللعبة، وفي شاشة **Play** خل لغة اللعبة **English** (التعريب يحل محل الإنجليزي). اضغط **Play** مرة وتأكد إن اللعبة تشتغل، وبعدين سكّرها.
5. **الخطوة ٣ في البرنامج:** اضغط «ثبّت التعريب». البرنامج يتأكد إن ملفاتك أصلية ويحفظ نسخة منها في `InfamousArabic-backup` داخل مجلد اللعبة.
6. شغّل اللعبة من البورت. **أول تشغيل بعد التثبيت ياخذ وقت طويل** (عندي حوالي 15 دقيقة): البورت يعيد تجهيز الشيدرات. التشغيل اللي بعده عادي.
7. فعّل الترجمة النصية (مقفلة في اللعبة من الأساس): «خيارات» ← «الترجمة النصية» ← «تشغيل».

إذا سحبت اللعبة من جهازك بنفسك (GoldHEN + Itemzflow) وعندك مجلد اللعبة جاهز، تخطّ الخطوة ٣.

## الإزالة
افتح `InfamousArabic.exe` واضغط «أزل التعريب» (الخطوة ٣ في البرنامج). ترجع ملفاتك الأصلية من النسخة الاحتياطية.

## ملاحظات
- باقي بالإنجليزي: عناوين المقدمة (مرسومة داخل الفيديو)، كلمة XP، وأسماء العلامات التجارية.
- إذا برنامج الحماية منع `InfamousArabic.exe`: ملفات PowerShell (`Prepare-Game.ps1` و `Install.ps1` و `Uninstall.ps1`) تسوي نفس الشي. قبل تشغيلها: كليك يمين على الملف ← Properties ← علّم Unblock ← OK، وبعدين كليك يمين ← Run with PowerShell.
- كود البرنامج موجود في مجلد [`app`](app) في هذي الصفحة، وتقدر تبنيه بنفسك بـ `build.cmd`.
- مشاكل البورت نفسه (تعليق، أداء، يد التحكم) تُسأل في صفحة البورت، مو هنا.

</div>

---

## English

Arabic translation of inFAMOUS Second Son (PS4 version) for the iss_pc PC port: dialogue, cutscenes, TV news, menus,
missions, powers, upgrades, tips and trophies.

No game file is included. The program patches the text files of your own copy.

### What you need

1. **PC:** Windows 10 or 11 (64-bit), a graphics card with Vulkan 1.3 (6 GB of VRAM recommended), about 35 GB free.
2. **The iss_pc port, version 1.0** (free): https://github.com/HayatoG/iss_pc/releases/tag/v1.0, file
   `infamous-windows-x86_64.zip`.
3. **Your own PS4 copy**, US version **CUSA00223**, as two PKG files: the game and **update 1.07** (names vary, for
   example `...CUSA00223-USA-Game-PS4.pkg` and `...CUSA00223-USA-Update-v1.07-PS4.pkg`). DLC files are not needed.
4. **To unpack the PKG files:** the [.NET Runtime](https://dotnet.microsoft.com/download/dotnet) (version 6 or newer,
   from Microsoft) and LibOrbisPkg PkgTool.Core 0.2: https://github.com/maxton/LibOrbisPkg/releases/tag/v0.2, file
   `PkgTool.Core-0.2.231.zip`, extracted anywhere.

### Steps

1. Download `InfamousSecondSon-Arabic-1.0.zip` from [Releases](../../releases/latest) and extract it.
2. Open `InfamousArabic.exe`. If "Windows protected your PC" appears, click More info, then Run anyway (the program is
   not digitally signed).
3. **Step 2 in the program ("Make the game folder"):** choose the game PKG, the 1.07 update PKG, the PkgTool.Core folder (the one with
   `PkgTool.Core.dll`) and where to make the game folder. Click "Make the game folder" and wait (about 3 minutes on my
   PC). You get a folder named `inFAMOUS Second Son`. Your PKG files are not changed.
4. **The port:** extract it and open `inFAMOUS Second Son.exe`. Under **Game folder**, choose the game folder; on the
   **Play** screen, set the game language to **English** (the translation replaces English). Click **Play** once,
   check that the game starts, then close it.
5. **Step 3 in the program:** click "Install". The program checks that your files are the original ones and keeps a
   copy in `InfamousArabic-backup` inside the game folder.
6. Start the game from the port. **The first start after installing takes a long time** (about 15 minutes on my PC):
   the port compiles its shaders again. Later starts are normal.
7. Turn on subtitles (off by default in this game): «خيارات» (Options) > «الترجمة النصية» (Subtitles) > «تشغيل» (On).

If you dumped the game from your own console (GoldHEN + Itemzflow) and already have the game folder, skip step 3.

### Uninstall
Open `InfamousArabic.exe` and click "Uninstall" (step 3 in the program). Your original files come back from the backup.

### Notes
- Still in English: the opening titles (painted into the video), the word XP, and brand names.
- If your antivirus blocks `InfamousArabic.exe`: the PowerShell files (`Prepare-Game.ps1`, `Install.ps1`,
  `Uninstall.ps1`) do the same. Before running one: right-click it > Properties > tick Unblock > OK, then right-click >
  Run with PowerShell.
- The program's source is in the [`app`](app) folder of this repository; `build.cmd` builds it.
- Problems with the port itself (crashes, performance, controllers) belong on the port's page, not here.

### Credits
- Translation: Osama.
- Arabic font: El Messiri by the El Messiri Project Authors, SIL Open Font License 1.1 (text in `licenses`), built
  into the game's bitmap fonts.
- iss_pc PC port by HayatoG. PKG unpacking: LibOrbisPkg by maxton (LGPL 3.0, downloaded separately).
