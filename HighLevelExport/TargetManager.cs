using HighLevelExport.Helper;
using HighLevelExport.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HighLevelExport
{
    public partial class TargetManager : Form
    {
        private ExportHistoryEntities db = new ExportHistoryEntities();
        public TargetManager()
        {
            InitializeComponent();
        }
        private DBHelper helper = new DBHelper();

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void TargetManager_Load(object sender, EventArgs e)
        {
            dataGridView1.AutoGenerateColumns = false;

            var connectionString = helper.getConnectionString();
            var listObj = new List<CaseObject>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (MySqlCommand cmd = helper.getListCaseName(connection))
                {
                    using (MySqlDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            var tempObj = new CaseObject
                            {
                                id = rdr.GetString(0),
                                name = rdr.GetString(1),
                                //brief = rdr.GetString(2),
                                //owner = rdr.GetString(3),
                                //dateCreated = rdr.GetString(4),
                                //dateUpdated = rdr.GetString(5),
                                //sensitivity = rdr.GetString(6),
                                //priority    = rdr.GetString(7),
                                //status = rdr.GetString(8),
                                //group = rdr.GetString(9),
                                //trashedTime = rdr.GetString(10),
                            };
                            listObj.Add(tempObj);
                        }
                    }
                }
            }
            Dictionary<string, string> test = new Dictionary<string, string>();
            foreach(var item in listObj)
            {
                test.Add(item.id, item.name);
            }
            //test.Add("1", "test_export");
            //test.Add("2", "MVThang");
            //test.Add("3", "CAHN");
            comboBox1.DataSource = new BindingSource(test, null);
            comboBox1.DisplayMember = "Value";
            comboBox1.ValueMember = "Key";

            lblSelectedId.Text = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
            reloadData();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblSelectedId.Text = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var selectedText = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Value;
            var selectedKey = ((KeyValuePair<string, string>)comboBox1.SelectedItem).Key;
            var tempTarget = new ExportTarget { TargetId = selectedKey, TargetName = selectedText };
            
            var existedTarget = db.ExportTargets.Where(m => m.TargetName == selectedText && m.TargetId == selectedKey).FirstOrDefault();
            if (existedTarget == null)
            {
                db.ExportTargets.Add(tempTarget);
                db.SaveChanges();
                reloadData();
            }
            else
            {
                MessageBox.Show(this, "Target already added!", "Failed!");
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(lblId.Text))
            {
                var tempId = Convert.ToInt32(lblId.Text);
                var target = db.ExportTargets.Where(m => m.Id == tempId).FirstOrDefault();
                DialogResult dialogResult = MessageBox.Show("Do you really want to delete target with Case: " + target.TargetName + ", Id: "+ target.TargetId, "Some Title", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    //do something
                    db.ExportTargets.Remove(target);
                    db.SaveChanges();
                    reloadData();
                    MessageBox.Show(this, "Target has been deleted", "Success!");
                }
                else if (dialogResult == DialogResult.No)
                {
                    //do something else
                }
            }
            else
            {
                MessageBox.Show(this, "Must select the record you want to delete first!", "Failed!");
            }
        }

        private void reloadData()
        {
            var listData = db.ExportTargets.ToList();
            dataGridView1.DataSource = listData;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex != -1)
            {

                lblId.Text = dataGridView1.Rows[e.RowIndex].Cells["Id"].Value.ToString();
                comboBox1.SelectedIndex = comboBox1.FindString(dataGridView1.Rows[e.RowIndex].Cells["TargetName"].Value.ToString());
                lblSelectedId.Text = dataGridView1.Rows[e.RowIndex].Cells["TargetId"].Value.ToString();
            }
        }
    }
}
