using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace XingceClock
{
    internal static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            try { SetProcessDPIAware(); } catch { }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length >= 2 && args[0] == "--shot")
            {
                RunShot(args[1], args.Length >= 3 && args[2] == "--demo-run");
                return;
            }

            Application.Run(new MainForm());
        }

        // 界面自查:打开指定界面,置顶后只截取自身窗口区域存成 PNG,然后自动退出
        private static void RunShot(string pngPath, bool demoRun)
        {
            Form f;
            if (demoRun)
            {
                List<Stage> demo = new List<Stage>();
                demo.Add(new Stage("资料分析(演示)", 25));
                f = new RunForm(demo, 0, true);
            }
            else
            {
                f = new MainForm();
            }
            f.StartPosition = FormStartPosition.Manual;
            f.Location = new Point(60, 60);
            f.TopMost = true;
            f.Shown += delegate
            {
                System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
                t.Interval = 900;
                t.Tick += delegate
                {
                    t.Stop();
                    t.Dispose();
                    try
                    {
                        Rectangle r = f.Bounds;
                        using (Bitmap bmp = new Bitmap(r.Width, r.Height))
                        {
                            using (Graphics g = Graphics.FromImage(bmp))
                            {
                                g.CopyFromScreen(r.Left, r.Top, 0, 0, new Size(r.Width, r.Height));
                            }
                            bmp.Save(pngPath, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                    catch { }
                    f.Close();
                };
                t.Start();
            };
            Application.Run(f);
        }
    }
}
