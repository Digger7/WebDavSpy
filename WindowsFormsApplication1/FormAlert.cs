using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class FormAlert : Form
    {
        string link = "";
        public FormAlert(string name="", string date="", string author="", string objectLink="")
        {
            InitializeComponent();
            link = objectLink;
            labelName.Text = $"Имя: {name}";
            labelDate.Text = $"Дата: {date}";
            labelAuthor.Text = $"Изменил: {author}";
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ButtonOpen_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(link)) {
                this.Close();
                System.Diagnostics.Process.Start(link);
            }
        }
    }
}
