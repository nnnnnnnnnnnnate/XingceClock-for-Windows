using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace XingceClock
{
    public class MainForm : Form
    {
        private List<Stage> stages;
        private ListView lv;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDel;
        private Button btnUp;
        private Button btnDown;
        private Button btnSingle;
        private Button btnPipe;
        private Label lblTotal;
        private Label lblTip;

        public MainForm()
        {
            Text = "行测计时器";
            Font = new Font("Microsoft YaHei UI", 9F);
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(464, 502);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            lv = new ListView();
            lv.Location = new Point(12, 12);
            lv.Size = new Size(310, 392);
            lv.View = View.Details;
            lv.FullRowSelect = true;
            lv.MultiSelect = false;
            lv.HideSelection = false;
            lv.Columns.Add("名称", 200);
            lv.Columns.Add("分钟", 95);
            lv.DoubleClick += new EventHandler(Lv_DoubleClick);
            lv.SelectedIndexChanged += new EventHandler(Lv_SelectedIndexChanged);
            Controls.Add(lv);

            btnAdd = MakeButton("添加", 332, 12);
            btnEdit = MakeButton("修改", 332, 47);
            btnDel = MakeButton("删除", 332, 82);
            btnUp = MakeButton("上移", 332, 117);
            btnDown = MakeButton("下移", 332, 152);
            btnAdd.Click += new EventHandler(BtnAdd_Click);
            btnEdit.Click += new EventHandler(BtnEdit_Click);
            btnDel.Click += new EventHandler(BtnDel_Click);
            btnUp.Click += new EventHandler(BtnUp_Click);
            btnDown.Click += new EventHandler(BtnDown_Click);

            lblTip = new Label();
            lblTip.Location = new Point(332, 192);
            lblTip.Size = new Size(120, 220);
            lblTip.ForeColor = Color.Gray;
            lblTip.Text = "双击某一项\r\n= 直接单独开始\r\n\r\n选中某一项\r\n可 修改/删除/\r\n上移/下移\r\n\r\n“流水线整串走完”\r\n会按清单顺序\r\n逐段倒计时,\r\n每段到点自动响铃\r\n并开始下一段";
            Controls.Add(lblTip);

            lblTotal = new Label();
            lblTotal.Location = new Point(12, 408);
            lblTotal.Size = new Size(310, 20);
            lblTotal.ForeColor = Color.Gray;
            Controls.Add(lblTotal);

            btnSingle = new Button();
            btnSingle.Location = new Point(12, 434);
            btnSingle.Size = new Size(205, 48);
            btnSingle.Text = "单独走选中项";
            btnSingle.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            btnSingle.Click += new EventHandler(BtnSingle_Click);
            Controls.Add(btnSingle);

            btnPipe = new Button();
            btnPipe.Location = new Point(227, 434);
            btnPipe.Size = new Size(225, 48);
            btnPipe.Text = "流水线整串走完";
            btnPipe.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
            btnPipe.Click += new EventHandler(BtnPipe_Click);
            Controls.Add(btnPipe);

            Load += new EventHandler(MainForm_Load);
        }

        private Button MakeButton(string text, int x, int y)
        {
            Button b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(120, 30);
            Controls.Add(b);
            return b;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            stages = StageStore.Load();
            Populate();
        }

        private void Populate()
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            foreach (Stage s in stages)
            {
                ListViewItem it = new ListViewItem(s.Name);
                it.SubItems.Add(FormatMinutes(s.Minutes));
                lv.Items.Add(it);
            }
            lv.EndUpdate();
            double total = 0;
            foreach (Stage s in stages) total += s.Minutes;
            lblTotal.Text = string.Format("共 {0} 项,总时长 {1} 分钟",
                stages.Count, total.ToString("0.##", CultureInfo.InvariantCulture));
            UpdateButtons();
        }

        private string FormatMinutes(double m)
        {
            return m.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private void UpdateButtons()
        {
            bool has = (stages != null && lv.SelectedIndices.Count == 1);
            int idx = has ? lv.SelectedIndices[0] : -1;
            btnEdit.Enabled = has;
            btnDel.Enabled = has;
            btnSingle.Enabled = has;
            btnUp.Enabled = has && idx > 0;
            btnDown.Enabled = has && stages != null && idx < stages.Count - 1;
            btnPipe.Enabled = stages != null && stages.Count > 0;
        }

        private int SelectedIndex()
        {
            if (lv.SelectedIndices.Count == 1) return lv.SelectedIndices[0];
            return -1;
        }

        private void Lv_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtons();
        }

        private void Lv_DoubleClick(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx >= 0) RunSingle(idx);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            EditDialog d = new EditDialog("添加闹钟", null);
            if (d.ShowDialog(this) == DialogResult.OK && d.Result != null)
            {
                stages.Add(d.Result);
                StageStore.Save(stages);
                Populate();
                lv.Items[stages.Count - 1].Selected = true;
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx < 0) return;
            EditDialog d = new EditDialog("修改闹钟", stages[idx]);
            if (d.ShowDialog(this) == DialogResult.OK && d.Result != null)
            {
                stages[idx] = d.Result;
                StageStore.Save(stages);
                Populate();
                lv.Items[idx].Selected = true;
            }
        }

        private void BtnDel_Click(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx < 0) return;
            DialogResult r = MessageBox.Show(this,
                string.Format("确定删除“{0}”吗?", stages[idx].Name),
                "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r == DialogResult.Yes)
            {
                stages.RemoveAt(idx);
                StageStore.Save(stages);
                Populate();
            }
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx > 0)
            {
                Stage tmp = stages[idx - 1];
                stages[idx - 1] = stages[idx];
                stages[idx] = tmp;
                StageStore.Save(stages);
                Populate();
                lv.Items[idx - 1].Selected = true;
            }
        }

        private void BtnDown_Click(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx >= 0 && idx < stages.Count - 1)
            {
                Stage tmp = stages[idx + 1];
                stages[idx + 1] = stages[idx];
                stages[idx] = tmp;
                StageStore.Save(stages);
                Populate();
                lv.Items[idx + 1].Selected = true;
            }
        }

        private void BtnSingle_Click(object sender, EventArgs e)
        {
            int idx = SelectedIndex();
            if (idx >= 0) RunSingle(idx);
        }

        private void BtnPipe_Click(object sender, EventArgs e)
        {
            if (stages.Count == 0) return;
            List<Stage> copy = new List<Stage>();
            foreach (Stage s in stages) copy.Add(new Stage(s.Name, s.Minutes));
            OpenRun(copy, 0, true);
        }

        private void RunSingle(int idx)
        {
            List<Stage> copy = new List<Stage>();
            copy.Add(new Stage(stages[idx].Name, stages[idx].Minutes));
            OpenRun(copy, 0, false);
        }

        private void OpenRun(List<Stage> list, int start, bool pipeline)
        {
            RunForm rf = new RunForm(list, start, pipeline);
            rf.FormClosed += new FormClosedEventHandler(delegate(object o, FormClosedEventArgs ea)
            {
                this.Show();
                this.Activate();
            });
            this.Hide();
            rf.Show();
        }
    }
}
