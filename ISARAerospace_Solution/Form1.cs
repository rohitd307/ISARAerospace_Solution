using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Web;
using Microsoft.SqlServer.Server;

namespace ISARAerospace_Solution
{
    public partial class Form1 : Form
    {
        DataTable dt = new DataTable();

        public Form1()
        {
            InitializeComponent();
            btnLoadData.Enabled = false;
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            bindData(txtFilePath.Text);
        }

        private void bindData(string filePath)
        {
            try
            {
                firstBind(filePath);
            }

            catch (Exception)
            {
                MessageBox.Show("Problem Occurred.!!\nApplication restarting...");
                Application.Restart();
            }

            if (dt.Rows.Count > 0)
            {
                dgvBooks.DataSource = dt;
                //dgvBooks.ReadOnly = true;
                formatGridView();
            }

        }

        private void firstBind(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length > 0)
            {
                //first line to create header
                string firstLine = lines[0];
                string[] headerLabels = firstLine.Split(';');
                foreach (string headerWord in headerLabels)
                {
                    dt.Columns.Add(new DataColumn(headerWord));
                }
                //For Data
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] dataWords = lines[i].Split(';');
                    DataRow dr = dt.NewRow();
                    int columnIndex = 0;
                    DataGridViewComboBoxColumn binding = new DataGridViewComboBoxColumn();
                    foreach (string headerWord in headerLabels)
                    {
                        dr[headerWord] = dataWords[columnIndex++];
                    }
                    dt.Rows.Add(dr);
                }
            }
        }

        private void formatGridView()
        {
            addComboBoxColumn();
            addCheckBoxColumn();
            addDescriptionButtonColumn();
            formatPriceColumn();
        }

        private void addComboBoxColumn()
        {
            DataGridViewComboBoxCell bookBinding = new DataGridViewComboBoxCell();

            var values = dt.AsEnumerable().Select(_ => _.Field<string>("Binding")).Distinct();
            bookBinding.Items.AddRange(values.ToArray());

            DataGridViewColumn cc = new DataGridViewColumn(bookBinding);
            dgvBooks.Columns.Add(cc);
            dgvBooks.Columns[7].HeaderText = dgvBooks.Columns[5].HeaderText;
            dgvBooks.Columns[5].Visible = false;

            foreach (DataGridViewRow item in dgvBooks.Rows)
            {
                item.Cells[7].Value = item.Cells[5].Value;
                item.DefaultCellStyle.BackColor = Color.DarkGray;
            }
        }

        private void addCheckBoxColumn()
        {
            string header = dgvBooks.Columns[4].HeaderText.ToString();
            DataGridViewCheckBoxColumn dgvCmb = new DataGridViewCheckBoxColumn();

            dgvCmb.ValueType = typeof(bool);
            dgvCmb.Name = "Stock";
            dgvCmb.HeaderText = header;

            dgvBooks.Columns.Add(dgvCmb);
            checkGridCombo();
        }

        private void checkGridCombo()
        {
            foreach (DataGridViewRow item in dgvBooks.Rows)
            {
                if (item.Cells[4].Value != null)
                {
                    if (item.Cells[4].Value.Equals("yes"))
                        item.Cells[8].Value = true;
                    else
                    {
                        item.Cells[8].Value = false;
                        item.DefaultCellStyle.BackColor = Color.Yellow;
                    }
                }

                dgvBooks.Columns[4].Visible = false;
            }
        }

        private void addDescriptionButtonColumn()
        {
            DataGridViewButtonColumn btn = new DataGridViewButtonColumn();

            btn.HeaderText = "Description";
            btn.Text = "Click Here";
            btn.Name = "btnDescription";
            btn.UseColumnTextForButtonValue = true;
            dgvBooks.Columns.Add(btn);

            dgvBooks.Columns[6].Visible = false;
            dgvBooks.CellClick += new DataGridViewCellEventHandler(dgvBooks_CellClick);
        }

        private void dgvBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                foreach (DataGridViewRow item in dgvBooks.Rows)
                {
                    if (item.Cells[6].Value != null)
                    {
                        MessageBox.Show(item.Cells[6].Value.ToString());
                        break;
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dgvBooks.Rows)
            {
                if (item.Cells[8].Value != null)
                {
                    if (!Convert.ToBoolean(item.Cells[8].Value))
                        dgvBooks.Rows.RemoveAt(item.Index);
                }
            }
            dgvBooks.Refresh();
        }

        private void formatPriceColumn()
        {
            List<double> values = new List<double>();
            foreach (DataGridViewRow item in dgvBooks.Rows)
            {
                if (item.Cells[3].Value != null)
                    values.Add(double.Parse(item.Cells["Price"].Value.ToString()));
            }
            double[] price = values.ToArray();
            Array.Sort(price);
            Array.Reverse(price);

            colorGradient(price);
        }

        private void colorGradient(double[] price)
        {
            Color textColor = Color.DarkRed;
            foreach (double value in price)
            {
                foreach (DataGridViewRow item in dgvBooks.Rows)
                {
                    if (item.Cells[3].Value != null)
                    {
                        if (value == Convert.ToDouble(item.Cells[3].Value))
                        {
                            item.Cells[3].Style.ForeColor = textColor;
                            textColor = ControlPaint.Light(textColor);
                        }
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                txtFilePath.Text = openFileDialog1.FileName;
                btnLoadData.Enabled = true;
            }
        }
    }
}
