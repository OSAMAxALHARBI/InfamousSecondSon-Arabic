// inFAMOUS Second Son (PS4 game in the iss_pc PC port) - Arabic translation tool.
// One window, three steps: make the game folder from the PS4 PKG files, install the translation, uninstall it.
// Same logic as the release's Prepare-Game.ps1 / Install.ps1 / Uninstall.ps1 (GAMES/InfamousSecondSon/TOOLS/make_release.py).
// Build (C# 5, .NET Framework 4 that ships with Windows): see build.cmd. No game file is inside the program.
// Headless test mode: InfamousArabic.exe --prepare <gamePkg> <updatePkg> <pkgToolDir> <outDir> | --install <game> |
// --uninstall <game>; writes the log to InfamousArabic.log next to the exe and exits 0 on success.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

[assembly: AssemblyTitle("inFAMOUS Second Son Arabic")]
[assembly: AssemblyProduct("inFAMOUS Second Son Arabic Translation")]
[assembly: AssemblyCompany("Osama")]
[assembly: AssemblyDescription("Installs the Arabic translation of inFAMOUS Second Son for the iss_pc PC port")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

namespace InfamousArabic
{
    class Failure : Exception { public Failure(string ar, string en) : base(ar + "\r\n" + en) { } }

    static class Core
    {
        public static Action<string> Log = delegate { };
        public static readonly string Here = AppDomain.CurrentDomain.BaseDirectory;
        const string BackupName = "InfamousArabic-backup";

        // ------------------------------------------------------------ step 1: PKG files -> game folder

        static readonly Dictionary<uint, string> EntryFiles = BuildEntryFiles();
        static Dictionary<uint, string> BuildEntryFiles()
        {
            var d = new Dictionary<uint, string> {
                {0x1000, "param.sfo"}, {0x1001, "playgo-chunk.dat"}, {0x1002, "playgo-chunk.sha"},
                {0x1003, "playgo-manifest.xml"}, {0x1004, "pronunciation.xml"}, {0x1005, "pronunciation.sig"},
                {0x1006, "pic1.png"}, {0x1007, "pubtoolinfo.dat"}, {0x1008, "app/playgo-chunk.dat"},
                {0x1009, "app/playgo-chunk.sha"}, {0x100A, "app/playgo-manifest.xml"}, {0x100B, "shareparam.json"},
                {0x0408, "origin-deltainfo.dat"}, {0x1200, "icon0.png"}, {0x1220, "pic0.png"}, {0x1240, "snd0.at9"},
                {0x1260, "changeinfo/changeinfo.xml"}, {0x1400, "trophy/trophy00.trp"} };
            for (uint i = 0; i < 30; i++) d[0x1261 + i] = string.Format("changeinfo/changeinfo_{0:D2}.xml", i);
            return d;
        }

        static uint BE32(byte[] b, int o) { return (uint)(b[o] << 24 | b[o + 1] << 16 | b[o + 2] << 8 | b[o + 3]); }

        static void ReadFull(Stream s, byte[] buf, int n)
        {
            int got = 0;
            while (got < n) { int k = s.Read(buf, got, n - got); if (k <= 0) throw new IOException("short read"); got += k; }
        }

        // the sce_sys files a PKG keeps in the clear (PkgTool's pkg_extract writes only the encrypted file system)
        public static void ExportSceSys(string pkg, string dest)
        {
            using (var fs = File.OpenRead(pkg))
            {
                var head = new byte[32];
                ReadFull(fs, head, 32);
                if (head[0] != 0x7F || head[1] != (byte)'C' || head[2] != (byte)'N' || head[3] != (byte)'T')
                    throw new Failure("الملف مو PKG حق PS4: " + pkg, "Not a PS4 package: " + pkg);
                uint count = BE32(head, 0x10), table = BE32(head, 0x18);
                var raw = new byte[count * 32];
                fs.Position = table;
                ReadFull(fs, raw, raw.Length);
                for (int i = 0; i < count; i++)
                {
                    uint id = BE32(raw, i * 32), flags = BE32(raw, i * 32 + 8);
                    uint off = BE32(raw, i * 32 + 16), size = BE32(raw, i * 32 + 20);
                    string name;
                    if (!EntryFiles.TryGetValue(id, out name) || (flags & 0x80000000u) != 0) continue;
                    string path = Path.Combine(dest, name.Replace('/', '\\'));
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    var buf = new byte[size];
                    fs.Position = off;
                    ReadFull(fs, buf, (int)size);
                    File.WriteAllBytes(path, buf);
                }
            }
        }

        public static bool HasDotnet()
        {
            try { return RunDotnet("--list-runtimes", false) == 0; } catch { return false; }
        }

        static int RunDotnet(string args, bool log)
        {
            var psi = new ProcessStartInfo("dotnet", args)
            {
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            };
            psi.EnvironmentVariables["DOTNET_ROLL_FORWARD"] = "Major";   // PkgTool.Core 0.2 targets .NET Core 3.0
            using (var p = Process.Start(psi))
            {
                p.OutputDataReceived += (s, e) => { if (log && !string.IsNullOrEmpty(e.Data)) Log("   " + e.Data); };
                p.ErrorDataReceived += (s, e) => { if (log && !string.IsNullOrEmpty(e.Data)) Log("   " + e.Data); };
                p.BeginOutputReadLine(); p.BeginErrorReadLine();
                p.WaitForExit();
                return p.ExitCode;
            }
        }

        static void ExtractPkg(string tool, string pkg, string outDir)
        {
            string args = string.Format("\"{0}\" pkg_extract \"{1}\" \"{2}\"", Path.Combine(tool, "PkgTool.Core.dll"), pkg, outDir);
            if (RunDotnet(args, true) != 0 || !Directory.Exists(Path.Combine(outDir, "uroot")))
                throw new Failure("PkgTool ما قدر يفك الملف: " + pkg, "PkgTool could not extract: " + pkg);
        }

        static void MoveTreeOver(string from, string to)
        {
            foreach (string f in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
            {
                string dst = Path.Combine(to, f.Substring(from.Length).TrimStart('\\'));
                Directory.CreateDirectory(Path.GetDirectoryName(dst));
                if (File.Exists(dst)) { File.SetAttributes(dst, FileAttributes.Normal); File.Delete(dst); }
                File.Move(f, dst);
            }
        }

        public static string Prepare(string gamePkg, string updatePkg, string tool, string outDir)
        {
            if (!File.Exists(Path.Combine(tool, "PkgTool.Core.dll")))
                throw new Failure("ما لقيت PkgTool.Core.dll في مجلد PkgTool.Core.", "PkgTool.Core.dll is not in the PkgTool.Core folder.");
            if (!HasDotnet())
                throw new Failure("ثبّت .NET Runtime (الإصدار 6 أو أحدث) من موقع مايكروسوفت ثم حاول مرة ثانية.",
                                  "Install the .NET Runtime (version 6 or newer) from Microsoft, then try again.");
            string game = Path.Combine(outDir, "inFAMOUS Second Son");
            if (Directory.Exists(game))
                throw new Failure("المجلد موجود من قبل: " + game, "The folder already exists: " + game);
            string tmp = Path.Combine(outDir, "iss-extract-tmp");
            if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
            Directory.CreateDirectory(tmp);
            Log("فك ملف اللعبة (ياخذ وقت) / Extracting the game PKG (this takes a while)...");
            ExtractPkg(tool, gamePkg, Path.Combine(tmp, "base"));
            Log("فك ملف التحديث / Extracting the update PKG...");
            ExtractPkg(tool, updatePkg, Path.Combine(tmp, "update"));
            Log("دمج التحديث / Merging the update over the game...");
            Directory.Move(Path.Combine(tmp, "base", "uroot"), game);
            MoveTreeOver(Path.Combine(tmp, "update", "uroot"), game);
            Log("كتابة sce_sys / Writing sce_sys...");
            ExportSceSys(gamePkg, Path.Combine(game, "sce_sys"));
            ExportSceSys(updatePkg, Path.Combine(game, "sce_sys"));    // the update's param.sfo (1.07) wins
            Directory.Delete(tmp, true);
            if (!File.Exists(Path.Combine(game, "eboot.bin")))
                throw new Failure("ما طلع ملف eboot.bin. تأكد من ملفات الـ PKG.", "eboot.bin is missing. Check your PKG files.");
            return game;
        }

        // ------------------------------------------------------------ steps 2 and 3: install / uninstall

        public static bool IsGame(string dir)
        {
            return !string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, "eboot.bin"))
                && File.Exists(Path.Combine(dir, "art\\cache\\lang_english_text.xpps"));
        }

        static void CheckGameClosed()
        {
            if (Process.GetProcessesByName("inFAMOUS Second Son").Length > 0 || Process.GetProcessesByName("tlg-probe-v2").Length > 0)
                throw new Failure("سكّر اللعبة أول وبعدين حاول مرة ثانية.", "Close the game first, then try again.");
        }

        static string Sha(string path)
        {
            using (var s = File.OpenRead(path))
            using (var h = SHA256.Create())
                return BitConverter.ToString(h.ComputeHash(s)).Replace("-", "");
        }

        static ArrayList ManifestFiles()
        {
            string p = Path.Combine(Here, "patch\\manifest.json");
            if (!File.Exists(p) || !File.Exists(Path.Combine(Here, "patch\\data.bin")))
                throw new Failure("مجلد patch ناقص. فك ضغط ملف التعريب كامل وشغّل البرنامج من داخله.",
                                  "The patch folder is missing. Extract the whole translation zip and run the program from inside it.");
            var js = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };
            var m = (Dictionary<string, object>)js.DeserializeObject(File.ReadAllText(p, Encoding.UTF8));
            return new ArrayList((object[])m["files"]);
        }

        static void CopyRange(Stream from, Stream to, long off, long n, byte[] buf)
        {
            from.Seek(off, SeekOrigin.Begin);
            while (n > 0)
            {
                int k = from.Read(buf, 0, (int)Math.Min(buf.Length, n));
                if (k <= 0) throw new IOException("short read");
                to.Write(buf, 0, k); n -= k;
            }
        }

        public static void Install(string game, Action<int> progress)
        {
            CheckGameClosed();
            if (!IsGame(game))
                throw new Failure("هذا مو مجلد اللعبة (لازم فيه eboot.bin ومجلد art).", "This is not the game folder (it needs eboot.bin and the art folder).");
            var todo = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> f in ManifestFiles())
            {
                string dst = Path.Combine(game, (string)f["path"]);
                Log("فحص / Checking " + f["path"]);
                string h = Sha(dst);
                if (h == (string)f["patched_sha256"]) continue;
                if (h != (string)f["stock_sha256"])
                    throw new Failure("الملف " + f["path"] + " ما يطابق النسخة الأمريكية الأصلية (CUSA00223، الإصدار 1.07). جهّز مجلد اللعبة من جديد ثم ثبّت التعريب.",
                                      f["path"] + " does not match the original US game (CUSA00223, version 1.07). Make the game folder again, then install the translation.");
                todo.Add(f);
            }
            if (todo.Count == 0) { Log("التعريب مثبّت من قبل / The Arabic translation is already installed."); return; }
            string bak = Path.Combine(game, BackupName);
            foreach (var f in todo)
            {
                string b = Path.Combine(bak, (string)f["path"]);
                if (File.Exists(b)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(b));
                File.Copy(Path.Combine(game, (string)f["path"]), b);
                Log("نسخة احتياطية / Backed up " + f["path"]);
            }
            var buf = new byte[1 << 20];
            var zero = new byte[1 << 20];
            using (var data = File.OpenRead(Path.Combine(Here, "patch\\data.bin")))
            {
                int done = 0;
                foreach (var f in todo)
                {
                    string dst = Path.Combine(game, (string)f["path"]);
                    string tmp = dst + ".arabic-tmp";
                    Log("تركيب / Writing " + f["path"]);
                    using (var src = File.OpenRead(Path.Combine(bak, (string)f["path"])))
                    using (var output = File.Create(tmp))
                    {
                        foreach (object[] op in (object[])f["ops"])
                        {
                            int kind = Convert.ToInt32(op[0]);
                            long off = Convert.ToInt64(op[1]), n = Convert.ToInt64(op[2]);
                            if (kind == 0) CopyRange(src, output, off, n, buf);
                            else if (kind == 1) CopyRange(data, output, off, n, buf);
                            else while (n > 0) { int k = (int)Math.Min(zero.Length, n); output.Write(zero, 0, k); n -= k; }
                        }
                    }
                    if (Sha(tmp) != (string)f["patched_sha256"])
                    {
                        File.Delete(tmp);
                        throw new Failure("فشل بناء الملف " + f["path"] + " وما تغيّر. شغّل الإزالة.",
                                          "Building " + f["path"] + " failed; that file was not changed. Run Uninstall.");
                    }
                    File.SetAttributes(dst, FileAttributes.Normal);
                    File.Delete(dst);
                    File.Move(tmp, dst);
                    progress(++done * 100 / todo.Count);
                }
            }
            File.WriteAllText(Path.Combine(Here, "game_folder.txt"), game, Encoding.UTF8);
        }

        public static void Uninstall(string game)
        {
            CheckGameClosed();
            if (!IsGame(game))
                throw new Failure("هذا مو مجلد اللعبة.", "This is not the game folder.");
            string bak = Path.Combine(game, BackupName);
            if (!Directory.Exists(bak))
                throw new Failure("ما لقيت النسخة الاحتياطية (InfamousArabic-backup). لإرجاع الملفات الأصلية جهّز مجلد اللعبة من جديد.",
                                  "No backup found (InfamousArabic-backup). To get the original files back, make the game folder again.");
            foreach (Dictionary<string, object> f in ManifestFiles())
            {
                string b = Path.Combine(bak, (string)f["path"]), dst = Path.Combine(game, (string)f["path"]);
                if (!File.Exists(b)) continue;
                if (File.Exists(dst)) File.SetAttributes(dst, FileAttributes.Normal);
                File.Copy(b, dst, true);
                Log("رجّعت / Restored " + f["path"]);
            }
            foreach (string b in Directory.GetFiles(bak, "*", SearchOption.AllDirectories))
                File.SetAttributes(b, FileAttributes.Normal);         // backups of read-only originals are read-only too
            Directory.Delete(bak, true);
        }

        public static string SavedGame()
        {
            string p = Path.Combine(Here, "game_folder.txt");
            return File.Exists(p) ? File.ReadAllText(p, Encoding.UTF8).Trim() : "";
        }
    }

    class MainForm : Form
    {
        readonly TextBox gamePkg = new TextBox(), updatePkg = new TextBox(), toolDir = new TextBox(), outDir = new TextBox(), gameDir = new TextBox();
        readonly TextBox log = new TextBox();
        readonly ProgressBar bar = new ProgressBar();
        readonly List<Button> actions = new List<Button>();
        static readonly Font Body = new Font("Segoe UI", 9.5f), Head = new Font("Segoe UI", 11f, FontStyle.Bold);

        public MainForm()
        {
            Text = "inFAMOUS Second Son — التعريب العربي / Arabic Translation";
            RightToLeft = RightToLeft.Yes; RightToLeftLayout = true;
            Font = Body; StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(760, 820); MinimumSize = new Size(700, 760);
            var col = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Padding = new Padding(12) };
            Controls.Add(col);

            col.Controls.Add(Title("١. جهّز مجلد اللعبة من ملفات الـ PKG", "1. Make the game folder from the PKG files"));
            col.Controls.Add(Note("تحتاج .NET Runtime 6 أو أحدث، وأداة PkgTool.Core 0.2. تخطّ هذي الخطوة إذا عندك مجلد اللعبة جاهز.", "Needs the .NET Runtime 6+ and PkgTool.Core 0.2. Skip this step if your game folder is ready."));
            col.Controls.Add(Links());
            col.Controls.Add(Row("ملف اللعبة (PKG)\r\nGame PKG", gamePkg, () => PickFile(gamePkg)));
            col.Controls.Add(Row("ملف التحديث (PKG)\r\nUpdate 1.07 PKG", updatePkg, () => PickFile(updatePkg)));
            col.Controls.Add(Row("مجلد PkgTool.Core\r\nPkgTool.Core folder", toolDir, () => PickFolder(toolDir)));
            col.Controls.Add(Row("وين تبي مجلد اللعبة (35 جيجا)\r\nWhere to make it (35 GB)", outDir, () => PickFolder(outDir)));
            col.Controls.Add(AddAction("جهّز مجلد اللعبة — Make the game folder", OnPrepare));

            col.Controls.Add(Title("٢. ثبّت التعريب", "2. Install the translation"));
            col.Controls.Add(Note("قبلها شغّل اللعبة في البورت مرة، وخل لغة اللعبة English في شاشة Play.", "First start the game in the port once, with the game language on English (Play screen)."));
            col.Controls.Add(Row("مجلد اللعبة\r\nGame folder (with eboot.bin)", gameDir, () => PickFolder(gameDir)));
            col.Controls.Add(AddAction("ثبّت التعريب — Install", OnInstall));

            col.Controls.Add(Title("٣. إزالة التعريب", "3. Uninstall"));
            col.Controls.Add(AddAction("أزل التعريب — Uninstall", OnUninstall));

            bar.Width = 720; bar.Height = 18; bar.Margin = new Padding(0, 14, 0, 6);
            col.Controls.Add(bar);
            log.Multiline = true; log.ReadOnly = true; log.ScrollBars = ScrollBars.Vertical; log.Width = 720; log.Height = 150;
            log.RightToLeft = RightToLeft.No;
            col.Controls.Add(log);

            gameDir.Text = Core.SavedGame();
            Core.Log = s => BeginInvoke((Action)(() => log.AppendText(s + "\r\n")));
        }

        static Control Title(string ar, string en)
        {
            var p = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Margin = new Padding(0, 16, 0, 4) };
            p.Controls.Add(new Label { Text = ar, Font = Head, AutoSize = true, Margin = new Padding(0) });
            p.Controls.Add(new Label { Text = en, AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(0), RightToLeft = RightToLeft.No });
            return p;
        }
        static Control Note(string ar, string en)
        {
            var p = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, Margin = new Padding(0, 2, 0, 2) };
            p.Controls.Add(new Label { Text = ar, AutoSize = true, MaximumSize = new Size(720, 0), ForeColor = Color.DimGray, Margin = new Padding(0) });
            p.Controls.Add(new Label { Text = en, AutoSize = true, MaximumSize = new Size(720, 0), ForeColor = Color.DimGray, Margin = new Padding(0), RightToLeft = RightToLeft.No });
            return p;
        }

        static Control Links()
        {
            var p = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 2, 0, 4) };
            p.Controls.Add(Link(".NET Runtime", "https://dotnet.microsoft.com/download/dotnet"));
            p.Controls.Add(Link("PkgTool.Core 0.2", "https://github.com/maxton/LibOrbisPkg/releases/tag/v0.2"));
            p.Controls.Add(Link("iss_pc 1.0", "https://github.com/HayatoG/iss_pc/releases/tag/v1.0"));
            return p;
        }

        static LinkLabel Link(string text, string url)
        {
            var l = new LinkLabel { Text = text, AutoSize = true, Margin = new Padding(0, 0, 16, 0) };
            l.LinkClicked += (s, e) => Process.Start(url);
            return l;
        }

        static Control Row(string label, TextBox box, Action browse)
        {
            var p = new TableLayoutPanel { ColumnCount = 3, Width = 720, Height = 44, Margin = new Padding(0, 2, 0, 2) };
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 230));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            p.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            p.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Right | AnchorStyles.Left, TextAlign = ContentAlignment.MiddleRight }, 0, 0);
            box.Anchor = AnchorStyles.Left | AnchorStyles.Right; box.RightToLeft = RightToLeft.No;
            p.Controls.Add(box, 1, 0);
            var b = new Button { Text = "اختيار...", Anchor = AnchorStyles.Left | AnchorStyles.Right, Height = 28 };
            b.Click += (s, e) => browse();
            p.Controls.Add(b, 2, 0);
            return p;
        }

        Button AddAction(string text, EventHandler click)
        {
            var b = new Button { Text = text, AutoSize = true, Padding = new Padding(10, 4, 10, 4), Margin = new Padding(0, 6, 0, 2) };
            b.Click += click;
            actions.Add(b);
            return b;
        }

        static void PickFile(TextBox box)
        {
            using (var d = new OpenFileDialog { Filter = "PS4 PKG (*.pkg)|*.pkg|All files (*.*)|*.*" })
                if (d.ShowDialog() == DialogResult.OK) box.Text = d.FileName;
        }

        static void PickFolder(TextBox box)
        {
            using (var d = new FolderBrowserDialog { SelectedPath = box.Text })
                if (d.ShowDialog() == DialogResult.OK) box.Text = d.SelectedPath;
        }

        void Run(bool marquee, ThreadStart work)
        {
            foreach (var b in actions) b.Enabled = false;
            bar.Style = marquee ? ProgressBarStyle.Marquee : ProgressBarStyle.Continuous;
            bar.Value = 0;
            new Thread(() =>
            {
                try { work(); }
                catch (Failure f) { Done(f.Message, MessageBoxIcon.Error); return; }
                catch (Exception e) { Done("صار خطأ:\r\nError: " + e.Message, MessageBoxIcon.Error); return; }
            }) { IsBackground = true }.Start();
        }

        void Done(string message, MessageBoxIcon icon)
        {
            BeginInvoke((Action)(() =>
            {
                foreach (var b in actions) b.Enabled = true;
                bar.Style = ProgressBarStyle.Continuous;
                bar.Value = icon == MessageBoxIcon.Error ? 0 : 100;
                log.AppendText(message + "\r\n");
                MessageBox.Show(this, message, "inFAMOUS Second Son", MessageBoxButtons.OK, icon,
                                MessageBoxDefaultButton.Button1, MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
            }));
        }

        void OnPrepare(object sender, EventArgs e)
        {
            string g = gamePkg.Text.Trim(), u = updatePkg.Text.Trim(), t = toolDir.Text.Trim(), o = outDir.Text.Trim();
            if (!File.Exists(g) || !File.Exists(u) || !Directory.Exists(t) || !Directory.Exists(o))
            {
                MessageBox.Show(this, "اختر الملفين والمجلدين أول.\r\nChoose both files and both folders first.", Text);
                return;
            }
            Run(true, () =>
            {
                string game = Core.Prepare(g, u, t, o);
                BeginInvoke((Action)(() => gameDir.Text = game));
                Done("تم تجهيز مجلد اللعبة:\r\nThe game folder is ready:\r\n" + game + "\r\n\r\nاختاره في البورت (Game folder)، وبعدين ثبّت التعريب (الخطوة ٢).\r\nChoose it in the port (Game folder), then install the translation (step 2).", MessageBoxIcon.Information);
            });
        }

        void OnInstall(object sender, EventArgs e)
        {
            string game = gameDir.Text.Trim();
            Run(false, () =>
            {
                Core.Install(game, p => BeginInvoke((Action)(() => bar.Value = p)));
                Done("تم تثبيت التعريب.\r\nThe Arabic translation is installed.\r\n\r\n" +
                     "- خل لغة اللعبة في البورت على English.\r\n- أول تشغيل بعد التثبيت ياخذ وقت طويل (البورت يعيد تجهيز الشيدرات).\r\n- فعّل الترجمة النصية: «خيارات» ← «الترجمة النصية» ← «تشغيل».\r\n\r\n" +
                     "- Keep the game language on English in the port.\r\n- The first start after installing takes a long time (the port compiles its shaders again).\r\n- Turn on subtitles in Options.", MessageBoxIcon.Information);
            });
        }

        void OnUninstall(object sender, EventArgs e)
        {
            string game = gameDir.Text.Trim();
            Run(false, () =>
            {
                Core.Uninstall(game);
                Done("تمت إزالة التعريب وعادت ملفات اللعبة الأصلية.\r\nThe Arabic translation was removed and the original game files are back.", MessageBoxIcon.Information);
            });
        }
    }

    static class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length > 0) return Headless(args);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            return 0;
        }

        static int Headless(string[] a)
        {
            var log = new StringBuilder();
            Core.Log = s => { lock (log) log.AppendLine(s); };
            int rc = 0;
            try
            {
                if (a[0] == "--prepare" && a.Length == 5) Core.Log("GAME " + Core.Prepare(a[1], a[2], a[3], a[4]));
                else if (a[0] == "--install" && a.Length == 2) Core.Install(a[1], p => { });
                else if (a[0] == "--uninstall" && a.Length == 2) Core.Uninstall(a[1]);
                else { Core.Log("usage: --prepare <gamePkg> <updatePkg> <pkgToolDir> <outDir> | --install <game> | --uninstall <game>"); rc = 2; }
            }
            catch (Exception e) { Core.Log("ERROR " + e.Message); rc = 1; }
            File.WriteAllText(Path.Combine(Core.Here, "InfamousArabic.log"), log.ToString(), Encoding.UTF8);
            return rc;
        }
    }
}
