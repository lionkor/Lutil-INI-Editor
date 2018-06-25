using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Lutil;

namespace IniEditor
{
    public partial class Form1 : Form
    {
        Ini ini;
        List<TreeNode> results;
        int searchIndex = 0;
        string log = @"logs\" + $"{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}_{DateTime.Now.Hour}-{DateTime.Now.Minute}-{DateTime.Now.Second}.log";
        string selectedNode = "";
        string selectedParent = "";
        List<string> logs;
        public Form1 ()
        {
            InitializeComponent ();
            string tmpFile = @"tmp\" + $"ignore_{DateTime.Now.Ticks}.ini";
            Directory.CreateDirectory ("tmp");
            Directory.CreateDirectory ("logs");
            File.WriteAllText (tmpFile, "");
            ini = new Ini (tmpFile);
            logs = new List<string> ();
        }

        void refreshTreeView ()
        {
            txtNewSection.AutoCompleteCustomSource.Clear ();
            treeView1.Nodes.Clear ();
            foreach (var section in ini.Sections)
            {
                treeView1.Nodes.Add (section, section);
                foreach (var key in ini.GetAll (section))
                {
                    treeView1.Nodes[section].Nodes.Add (key, key);
                }
            }
            txtNewSection.AutoCompleteCustomSource.AddRange (ini.Sections);
        }

        private void openFileDialog1_FileOk ( object sender, CancelEventArgs e )
        {
            ini = new Ini (openFileDialog1.FileName);
            refreshTreeView ();
            Log ($"Opened {ini.Path}");
        }

        private void openToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            openFileDialog1.ShowDialog ();
        }

        private void form1_FormClosing ( object sender, FormClosingEventArgs e )
        {
            foreach (var file in Directory.GetFiles ("tmp"))
            {
                File.Delete (file);
                Log ($"Deleted {file}");
            }
            Log ($"Program closing...");
            File.WriteAllLines (log, logs);
        }

        private void treeView1_AfterSelect ( object sender, TreeViewEventArgs e )
        {
            selectedNode = treeView1.SelectedNode.Text;
            if (treeView1.SelectedNode.Parent != null)
            {
                selectedParent = treeView1.SelectedNode.Parent.Text;
                txtNewSection.Text = selectedParent;
            }
            else
            {
                txtNewSection.Text = selectedNode;
            }
            textBox1.Text = ini.Get (selectedParent, selectedNode);
            textBox2.Text = ini.Get (selectedParent, selectedNode);
        }

        private void textBox1_TextChanged ( object sender, EventArgs e )
        {
            if (checkAutoSave.Checked && textBox1.Text.Length > 0)
            {
                ini.Set (selectedParent, selectedNode, textBox1.Text, false);
                Log ($"Saved new value for '{selectedNode}' in '{selectedParent}': '{textBox1.Text}'");
            }
        }

        private void textBox1_Leave ( object sender, EventArgs e )
        {
            textBox2.Text = ini.Get (selectedParent, selectedNode);
        }

        private void btnSet1_Click ( object sender, EventArgs e )
        {
            textBox1.Text = "1";
        }

        private void btnSet0_Click ( object sender, EventArgs e )
        {
            textBox1.Text = "0";
        }

        private void btnSetTrue_Click ( object sender, EventArgs e )
        {
            textBox1.Text = "True";
        }

        private void btnSetFalse_Click ( object sender, EventArgs e )
        {
            textBox1.Text = "False";
        }

        private void btnEditReset_Click ( object sender, EventArgs e )
        {
            textBox1.Text = ini.Get (selectedParent, selectedNode);
            textBox2.Text = ini.Get (selectedParent, selectedNode);
            Log ($"Reset button used");
        }

        private void btnEditSave_Click ( object sender, EventArgs e )
        {
            if (textBox1.Text.Length > 0)
            {
                ini.Set (selectedParent, selectedNode, textBox1.Text, false);
                textBox2.Text = ini.Get (selectedParent, selectedNode);
                Log ($"Saved new value for '{selectedNode}' in '{selectedParent}': '{textBox1.Text}'");
                return;
            }
            textBox2.Text = ini.Get (selectedParent, selectedNode);
            Log ($"Attempt to save new value failed (text-box empty)");
        }

        private void saveToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            ini.SaveData ();
            Log ($"Saved to file '{ini.Path}'");
        }

        private void button1_Click ( object sender, EventArgs e )
        {
            var section = txtNewSection.Text;
            var key = txtNewKey.Text;
            var value = txtNewValue.Text;
            if (section.Length > 0 && key.Length > 0 && value.Length > 0)
            {
                ini.Add (section, key, value, false);
                Log ($"New entry added: '{key}' in '{section}': '{value}'");
            }
            refreshTreeView ();
        }

        public void Log ( string message )
        {
            logs.Add (DateTime.Now.ToLongTimeString () + ": " + message);
        }

        private void saveAsToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            saveFileDialog1.ShowDialog ();
        }

        private void saveFileDialog1_FileOk ( object sender, CancelEventArgs e )
        {
            File.WriteAllText (saveFileDialog1.FileName, "");
            ini.SaveData (saveFileDialog1.FileName);
            ini = new Ini (saveFileDialog1.FileName);
            Log ($"Saved to file '{saveFileDialog1.FileName}'");
            Log ($"Changed working directory to new file location");
        }

        private void txtNewSection_KeyDown ( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.Return)
            {
                button1.PerformClick ();
            }
        }

        private void txtNewKey_KeyDown ( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.Return)
            {
                button1.PerformClick ();
            }
        }

        private void txtNewValue_KeyDown ( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.Return)
            {
                button1.PerformClick ();
            }
        }

        private void expandAllToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            treeView1.ExpandAll ();
        }

        private void collapseAllToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            refreshTreeView ();
        }

        private void button2_Click ( object sender, EventArgs e )
        {
            // remove button
            if (treeView1.SelectedNode != null)
            {
                if (treeView1.SelectedNode.Parent == null)
                {
                    var result = MessageBox.Show (
                        "You are about to remove a section and all the keys held within. " +
                        $"Are you sure that you want to remove '{treeView1.SelectedNode.Text}' and the " +
                        $"{ini.CountIn (treeView1.SelectedNode.Text)} keys it holds?", "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1,
                        0);
                    if (result == DialogResult.Yes)
                    {
                        ini.RemoveSection (treeView1.SelectedNode.Text, false);
                        treeView1.SelectedNode.Remove ();
                        refreshTreeView ();
                    }
                }
                else
                {
                    var result = MessageBox.Show (
                        $"Are you sure that you want to remove '{treeView1.SelectedNode.Text}' in '{treeView1.SelectedNode.Parent.Text}'", "Remove",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1,
                        0);
                    if (result == DialogResult.Yes)
                    {
                        ini.RemoveKey (treeView1.SelectedNode.Parent.Text, treeView1.SelectedNode.Text, false);
                        treeView1.SelectedNode.Remove ();
                        refreshTreeView ();
                    }
                }
            }
        }

        private void refreshToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            refreshTreeView ();
        }

        private void searchBox_KeyDown_1 ( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.Return)
            {
                btnSearch.PerformClick ();
            }
        }

        private void btnSearch_Click ( object sender, EventArgs e )
        {
            if (searchBox.Text.Length > 0)
            {
                results = new List<TreeNode> ();
                string text = searchBox.Text;
                foreach (TreeNode parent in treeView1.Nodes)
                {
                    if (parent.Text.Contains (text))
                    {
                        results.Add (parent);
                    }
                    foreach (TreeNode node in parent.Nodes)
                    {
                        if (node.Text.Contains (text))
                        {
                            results.Add (node);
                        }
                    }
                }
                btnNext.Enabled = true;
                btnPrevious.Enabled = true;
            }
        }

        private void btnNext_Click ( object sender, EventArgs e )
        {
            if (searchIndex < results.Count - 1)
            {
                searchIndex++;
            }
            else
            {
                searchIndex = 0;
            }
            if (results != null && results.Count > 0)
            {
                treeView1.SelectedNode = results[searchIndex];
            }
        }

        private void btnPrevious_Click ( object sender, EventArgs e )
        {
            if (searchIndex == 0)
            {
                searchIndex = results.Count - 1;
            }
            else
            {
                searchIndex--;
            }
            if (results != null && results.Count > 0)
            {
                treeView1.SelectedNode = results[searchIndex];
            }
        }

        private void textBox1_KeyDown ( object sender, KeyEventArgs e )
        {
            if (e.KeyCode == Keys.Return)
            {
                btnEditSave.PerformClick ();
            }
        }

        private void showGuideToolStripMenuItem_Click ( object sender, EventArgs e )
        {
            new Guide ().Show ();
        }
    }
}
