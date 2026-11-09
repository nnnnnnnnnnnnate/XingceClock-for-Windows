using System;
using System.Drawing;
using System.Windows.Forms;

namespace XingceClock
{
    public class EditDialog : Form
    {
        public Stage Result;
        private TextBox txtName;
        private NumericUpDown numMin;

        public EditDialog(string title, Stage initial)
        {
            Text = title;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(330, 152);
            Font = new Font("Microsoft YaHei UI", 10F);

            Label lblName = new Label();
            lblName.Text = "名称:";
            lblName.Location = new Point(18, 22);
            lblName.Size = new Size(58, 24);
            Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(80, 18);
            txtName.Size = new Size(230, 27);
            Controls.Add(txtName);

            Label lblMin = new Label();
            lblMin.Text = "分钟:";
            lblMin.Location = new Point(18, 62);
            lblMin.Size = new Size(58, 24);
            Controls.Add(lblMin);

            numMin = new NumericUpDown();
            numMin.Location = new Point(80, 58);
            numMin.Size = new Size(100, 27);
            numMin.DecimalPlaces = 1;
            numMin.Increment = 1;
            numMin.Minimum = 0.5M;
            numMin.Maximum = 600M;
            Controls.Add(numMin);

            Label lblHint = new Label();
            lblHint.Text = "可填 0.5";
            lblHint.ForeColor = Color.Gray;
            lblHint.Location = new Point(190, 62);
            lblHint.Size = new Size(120, 24);
            Controls.Add(lblHint);

            Button btnOK = new Button();
            btnOK.Text = "确定";
            btnOK.Location = new Point(80, 108);
            btnOK.Size = new Size(100, 32);
            Controls.Add(btnOK);

            Button btnCancel = new Button();
            btnCancel.Text = "取消";
            btnCancel.Location = new Point(190, 108);
            btnCancel.Size = new Size(100, 32);
            btnCancel.DialogResult = DialogResult.Cancel;
            Controls.Add(btnCancel);

            AcceptButton = btnOK;
            CancelButton = btnCancel;

            if (initial != null)
            {
                txtName.Text = initial.Name;
                numMin.Value = (decimal)initial.Minutes;
            }

            btnOK.Click += new EventHandler(BtnOK_Click);
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show(this, "名称不能为空", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.DialogResult = DialogResult.None;
                return;
            }
            Result = new Stage(name, (double)numMin.Value);
        }
    }
}
