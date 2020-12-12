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
    public partial class FormEdit : Form
    {
        public string name;
        public string link;

        public FormEdit()
        {
            InitializeComponent();
        }

        private void FormEdit_Shown(object sender, EventArgs e)
        {
            textBoxName.Text = name;
            textBoxLink.Text = link;
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            name = textBoxName.Text;
            link = textBoxLink.Text;
            this.DialogResult = DialogResult.OK;
        }
    }
}
