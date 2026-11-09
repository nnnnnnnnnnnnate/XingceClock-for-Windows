using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Media;
using System.Windows.Forms;

namespace XingceClock
{
    public class RunForm : Form
    {
        private const int AlertSeconds = 5;

        private List<Stage> stages;
        private int index;
        private bool pipeline;

        private System.Windows.Forms.Timer timer;
        private DateTime endUtc;
        private TimeSpan remainingOnPause;
        private bool paused;
        private bool alerting;
        private bool finishedAll;
        private DateTime alertEndUtc;
        private SoundPlayer player;
        private string wavPath;
        private int tickCount;

        private Label lblSeq;
        private Label lblStage;
        private Label lblTime;
        private Label lblNext;
        private Button btnPause;
        private Button btnSkip;
        private Button btnStop;
        private Button btnAlert;

        public RunForm(List<Stage> stageList, int startIndex, bool isPipeline)
        {
            stages = stageList;
            index = startIndex;
            pipeline = isPipeline;

            Text = "行测计时器";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            TopMost = true;
            BackColor = Color.White;
            Font = new Font("Microsoft YaHei UI", 9F);
            ClientSize = new Size(420, 300);

            lblSeq = new Label();
            lblSeq.Location = new Point(20, 12);
            lblSeq.Size = new Size(380, 18);
            lblSeq.ForeColor = Color.Gray;
            Controls.Add(lblSeq);

            lblStage = new Label();
            lblStage.Location = new Point(20, 34);
            lblStage.Size = new Size(380, 36);
            lblStage.Font = new Font("Microsoft YaHei UI", 15F, FontStyle.Bold);
            lblStage.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(lblStage);

            lblTime = new Label();
            lblTime.Location = new Point(20, 76);
            lblTime.Size = new Size(380, 100);
            lblTime.Font = new Font("Consolas", 48F, FontStyle.Bold);
            lblTime.TextAlign = ContentAlignment.MiddleCenter;
            Controls.Add(lblTime);

            lblNext = new Label();
            lblNext.Location = new Point(20, 184);
            lblNext.Size = new Size(380, 48);
            lblNext.ForeColor = Color.Gray;
            Controls.Add(lblNext);

            btnPause = new Button();
            btnPause.Location = new Point(20, 250);
            btnPause.Size = new Size(110, 40);
            btnPause.Text = "暂停";
            btnPause.Click += new EventHandler(BtnPause_Click);
            Controls.Add(btnPause);

            btnSkip = new Button();
            btnSkip.Location = new Point(140, 250);
            btnSkip.Size = new Size(120, 40);
            btnSkip.Text = "跳过本段";
            btnSkip.Click += new EventHandler(BtnSkip_Click);
            Controls.Add(btnSkip);

            btnStop = new Button();
            btnStop.Location = new Point(270, 250);
            btnStop.Size = new Size(130, 40);
            btnStop.Text = "停止";
            btnStop.Click += new EventHandler(BtnStop_Click);
            Controls.Add(btnStop);

            btnAlert = new Button();
            btnAlert.Location = new Point(20, 250);
            btnAlert.Size = new Size(240, 40);
            btnAlert.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            btnAlert.Visible = false;
            btnAlert.Click += new EventHandler(BtnAlert_Click);
            Controls.Add(btnAlert);

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 200;
            timer.Tick += new EventHandler(OnTick);

            wavPath = FindWav();

            FormClosing += new FormClosingEventHandler(RunForm_FormClosing);

            BeginStage();
        }

        private void RunForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopAlertSound();
            timer.Stop();
        }

        private string FindWav()
        {
            string[] candidates = new string[] {
                @"C:\Windows\Media\Alarm03.wav",
                @"C:\Windows\Media\Alarm02.wav",
                @"C:\Windows\Media\Alarm01.wav",
                @"C:\Windows\Media\Ring05.wav",
                @"C:\Windows\Media\Ring01.wav",
                @"C:\Windows\Media\chimes.wav",
                @"C:\Windows\Media\ding.wav"
            };
            foreach (string p in candidates)
            {
                try { if (File.Exists(p)) return p; } catch { }
            }
            return null;
        }

        private void BeginStage()
        {
            Stage s = stages[index];
            alerting = false;
            finishedAll = false;
            paused = false;
            endUtc = DateTime.UtcNow + TimeSpan.FromMinutes(s.Minutes);
            Text = s.Name + " - 计时中";
            lblSeq.Text = pipeline
                ? string.Format("流水线 第 {0}/{1} 项", index + 1, stages.Count)
                : "单独计时";
            lblStage.Text = s.Name;
            lblTime.Text = FormatRemaining(TimeSpan.FromMinutes(s.Minutes));
            lblTime.ForeColor = Color.FromArgb(32, 32, 32);
            lblTime.BackColor = Color.White;
            btnPause.Visible = true;
            btnSkip.Visible = true;
            btnAlert.Visible = false;
            btnPause.Text = "暂停";
            btnPause.Enabled = true;
            btnSkip.Enabled = true;
            btnStop.Enabled = true;
            SetNextLabel();
            timer.Start();
        }

        private void SetNextLabel()
        {
            if (pipeline && index + 1 < stages.Count)
            {
                Stage next = stages[index + 1];
                lblNext.Text = string.Format("接下来:{0}({1} 分钟)",
                    next.Name, FormatMinutes(next.Minutes));
            }
            else if (pipeline)
            {
                lblNext.Text = "这是最后一项";
            }
            else
            {
                lblNext.Text = "到点会响铃提醒你";
            }
        }

        private string FormatMinutes(double m)
        {
            return m.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private string FormatRemaining(TimeSpan t)
        {
            if (t < TimeSpan.Zero) t = TimeSpan.Zero;
            int totalSec = (int)Math.Ceiling(t.TotalSeconds);
            int h = totalSec / 3600;
            int m = (totalSec % 3600) / 60;
            int sec = totalSec % 60;
            if (h > 0) return string.Format("{0}:{1:00}:{2:00}", h, m, sec);
            return string.Format("{0:00}:{1:00}", m, sec);
        }

        private void OnTick(object sender, EventArgs e)
        {
            tickCount++;

            if (alerting)
            {
                if (tickCount % 2 == 0)
                    lblTime.ForeColor = Color.Firebrick;
                else
                    lblTime.ForeColor = Color.FromArgb(32, 32, 32);
                if (player == null && tickCount % 5 == 0)
                {
                    try { SystemSounds.Exclamation.Play(); } catch { }
                }
                if (!finishedAll)
                {
                    TimeSpan left = alertEndUtc - DateTime.UtcNow;
                    if (left <= TimeSpan.Zero)
                    {
                        StopAlertSound();
                        index++;
                        BeginStage();
                    }
                    else
                    {
                        Stage next = stages[index + 1];
                        lblNext.Text = string.Format("{0} 秒后自动开始:{1}({2} 分钟)",
                            (int)Math.Ceiling(left.TotalSeconds),
                            next.Name, FormatMinutes(next.Minutes));
                    }
                }
                return;
            }

            if (paused) return;

            TimeSpan rem = endUtc - DateTime.UtcNow;
            if (rem <= TimeSpan.Zero)
            {
                StageEnded();
                return;
            }
            lblTime.Text = FormatRemaining(rem);
        }

        private void StageEnded()
        {
            alerting = true;
            bool hasMore = pipeline && index + 1 < stages.Count;
            lblTime.Text = "00:00";
            lblTime.ForeColor = Color.Firebrick;
            btnPause.Visible = false;
            btnSkip.Visible = false;
            btnAlert.Visible = true;
            btnAlert.Text = hasMore ? "立即开始下一项" : "知道了";
            Text = "时间到!";
            PlayAlertLoop();

            if (hasMore)
            {
                alertEndUtc = DateTime.UtcNow + TimeSpan.FromSeconds(AlertSeconds);
            }
            else
            {
                finishedAll = true;
                if (pipeline)
                {
                    lblSeq.Text = "流水线完成";
                    lblStage.Text = "全部完成!";
                }
                else
                {
                    lblSeq.Text = "单独计时完成";
                    lblStage.Text = stages[index].Name + " 时间到!";
                }
                lblNext.Text = "点“知道了”停止铃声并返回";
            }
        }

        private void PlayAlertLoop()
        {
            if (wavPath != null)
            {
                try
                {
                    player = new SoundPlayer(wavPath);
                    player.PlayLooping();
                    return;
                }
                catch { player = null; }
            }
            player = null;
            try { SystemSounds.Exclamation.Play(); } catch { }
        }

        private void StopAlertSound()
        {
            if (player != null)
            {
                try { player.Stop(); } catch { }
                player = null;
            }
        }

        private void BtnPause_Click(object sender, EventArgs e)
        {
            if (alerting) return;
            if (!paused)
            {
                remainingOnPause = endUtc - DateTime.UtcNow;
                if (remainingOnPause < TimeSpan.Zero) remainingOnPause = TimeSpan.Zero;
                paused = true;
                btnPause.Text = "继续";
                lblTime.Text = FormatRemaining(remainingOnPause);
                lblTime.ForeColor = Color.Gray;
                Text = "已暂停 - " + stages[index].Name;
            }
            else
            {
                endUtc = DateTime.UtcNow + remainingOnPause;
                paused = false;
                btnPause.Text = "暂停";
                lblTime.ForeColor = Color.FromArgb(32, 32, 32);
                Text = stages[index].Name + " - 计时中";
            }
        }

        private void BtnSkip_Click(object sender, EventArgs e)
        {
            if (alerting) return;
            if (pipeline && index + 1 < stages.Count)
            {
                index++;
                BeginStage();
            }
            else
            {
                Close();
            }
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            DialogResult r = MessageBox.Show(this, "确定要停止计时吗?", "确认",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                StopAlertSound();
                timer.Stop();
                Close();
            }
        }

        private void BtnAlert_Click(object sender, EventArgs e)
        {
            StopAlertSound();
            if (!finishedAll)
            {
                index++;
                BeginStage();
            }
            else
            {
                Close();
            }
        }
    }
}
