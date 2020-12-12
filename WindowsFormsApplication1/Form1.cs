using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using WindowsInput;
using WebDav;
using System.Configuration;
using Microsoft.Win32;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        bool exitButton = false;
        WebDavClient client = new WebDavClient();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon1.Visible = true;
            }  
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;  
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (exitButton == false) {
                Hide();
                notifyIcon1.Visible = true;
                e.Cancel = true;                        
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            exitButton = true;
            Application.Exit();
        }

        private void ВыходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Выйти из приложения?", "Вы уверены?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                exitButton = true;
                Application.Exit();
            }
        }

        private async void ButtonSave_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Сохранить параметры?", "Вы уверены?",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;

                string objects = "";
                string objectsNames = "";
                string objectsModDateTime = "";

                foreach (ListViewItem item in listViewWebDavObjects.Items)
                {
                    var result = await client.Propfind(item.Tag.ToString());
                    if (result.IsSuccessful)
                    {
                        objects += $"{item.Tag.ToString()};";
                        objectsNames += $"{item.Text.ToString()};";
                        foreach (var res in result.Resources)
                        {
                            objectsModDateTime += $"{res.LastModifiedDate};";
                        }
                    }
                }

                confCollection["objects"].Value = objects;
                confCollection["objectsNames"].Value = objectsNames;
                confCollection["objectsModDateTime"].Value = objectsModDateTime;

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
            }
        }

        private async void Timer1_Tick(object sender, EventArgs e)
        {
            string[] objects = ConfigurationManager.AppSettings["objects"].Split(';');
            string[] objectsNames = ConfigurationManager.AppSettings["objectsNames"].Split(';');
            string[] objectsModDateTime = ConfigurationManager.AppSettings["objectsModDateTime"].Split(';');
            int index = 0;
            foreach (string objectLink in objects)
            {
                if (String.IsNullOrEmpty(objectLink)) continue;
                try
                {
                    var result = await client.Propfind(objectLink);
                    if (result.IsSuccessful)
                    {
                        string modifiedby;
                        string dateModify;
                        foreach (var res in result.Resources)
                        {
                            dateModify = res.LastModifiedDate.ToString();
                            modifiedby = res.Properties.ToList().Find(a => a.ToString().Contains("modifiedby")).Value;
                            if (dateModify != objectsModDateTime[index])
                            {
                                #region Сохранение даты изменения
                                objectsModDateTime[index] = dateModify;
                                string objectsModDateTimeString = "";
                                foreach (var item in objectsModDateTime)
                                {
                                    objectsModDateTimeString += $"{item};";
                                }
                                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                                KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;
                                confCollection["objectsModDateTime"].Value = objectsModDateTimeString;
                                config.Save(ConfigurationSaveMode.Modified);
                                ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
                                #endregion
                                var fAlert = new FormAlert(objectsNames[index], dateModify, modifiedby, objectLink);
                                fAlert.Show();
                            }
                        }
                    }
                }
                catch (Exception){ }
                index++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] objectsNames = ConfigurationManager.AppSettings["objectsNames"].Split(';');
            string[] objects = ConfigurationManager.AppSettings["objects"].Split(';');
            int index = 0;
            foreach (string item in objectsNames)
            {
                if (String.IsNullOrEmpty(item)) continue;
                ListViewItem lvi = new ListViewItem();
                lvi.Tag = objects[index];
                lvi.Text = item;
                lvi.ImageKey = "link.png";
                listViewWebDavObjects.Items.Add(lvi);
                index++;
            }

            checkBoxAutostart.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["autoStart"]);
            checkBoxMinimize.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["minimize"]);
            numericUpDownInterval.Value = Convert.ToDecimal(ConfigurationManager.AppSettings["timerInterval"])/60000;
            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerInterval"]);
        }

        private void NumericUpDownInterval_ValueChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;

            var interval = Convert.ToInt32(numericUpDownInterval.Value * 60000);
            timer1.Interval = interval;
            confCollection["timerInterval"].Value = interval.ToString();
            
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (Convert.ToBoolean(ConfigurationManager.AppSettings["minimize"])) {
                Hide();
                notifyIcon1.Visible = true;
            }
        }

        private void CheckBoxAutostart_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (checkBoxAutostart.Checked)
            {
                key.SetValue("WebDavObjectsSpy", Application.ExecutablePath);
            }
            else {
                key.DeleteValue("WebDavObjectsSpy", false);
            }
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;
            confCollection["autoStart"].Value = checkBoxAutostart.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            FormEdit fe = new FormEdit();
            if (fe.ShowDialog() == DialogResult.OK)
            {
                if (!String.IsNullOrEmpty(fe.link) && !String.IsNullOrEmpty(fe.name))
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = fe.link;
                    lvi.Text = fe.name;
                    lvi.ImageKey = "link.png";
                    listViewWebDavObjects.Items.Add(lvi);
                }
            };
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listViewWebDavObjects.SelectedItems)
            {
                listViewWebDavObjects.Items.Remove(item);
            }  
        }

        private void ButtonEdit_Click(object sender, EventArgs e)
        {
            if (listViewWebDavObjects.SelectedItems.Count > 0) {
                FormEdit fe = new FormEdit();
                fe.name = listViewWebDavObjects.SelectedItems[0].Text;
                fe.link = listViewWebDavObjects.SelectedItems[0].Tag.ToString();
                if (fe.ShowDialog() == DialogResult.OK) {
                    listViewWebDavObjects.SelectedItems[0].Text = fe.name;
                    listViewWebDavObjects.SelectedItems[0].Tag = fe.link;
                };
            }
        }

        private void CheckBoxMinimize_CheckedChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection confCollection = config.AppSettings.Settings;
            confCollection["minimize"].Value = checkBoxMinimize.Checked.ToString();
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(config.AppSettings.SectionInformation.Name);
        }

    }
}
