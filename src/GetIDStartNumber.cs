using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MorphTool
{
    public partial class GetIDStartNumber : Form
    {
        public int StartID { get { return startID; } }
        private int startID;
        public GetIDStartNumber()
        {
            InitializeComponent();
        }

        private void Go_button_Click(object sender, EventArgs e)
        {
            if (int.TryParse(startNumber.Text, out startID))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Not a valid integer number!");
                return;
            }
        }

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
