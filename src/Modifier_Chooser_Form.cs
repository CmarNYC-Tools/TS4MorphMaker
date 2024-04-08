/* TS4 MorphMaker, a tool for creating custom content for The Sims 4,
   Copyright (C) 2014  C. Marinetti

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
   The author may be contacted at modthesims.info, username cmarNYC. */

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
    public partial class Modifier_Chooser_Form : Form
    {
        SortedDictionary<string, ulong> customDict;
        SortedDictionary<string, ulong> eaDict;
        public ulong ReturnInstance { get; set; }
        public string ReturnName { get; set; }
        public bool ReturnIsEA { get; set; }

        public Modifier_Chooser_Form(List<SMOD> smodList, Dictionary<ulong, string> smodDictEA, bool showRemove)
        {
            InitializeComponent();
            if (!showRemove) Chooser_Remove_button.Visible = false;
            customDict = new SortedDictionary<string, ulong>();
            for (int i = 0; i < smodList.Count; i++)
            {
                if (!customDict.ContainsKey(smodList[i].smodName)) 
                    customDict.Add(smodList[i].smodName, smodList[i].publicKey[0].Instance);
            }
            eaDict = new SortedDictionary<string, ulong>();
            foreach (KeyValuePair<ulong, string> pair in smodDictEA)
            {
                eaDict.Add(pair.Value, pair.Key);
            }
            List_Custom();
        }

        public Modifier_Chooser_Form(List<Sculpt> sculptList, Dictionary<ulong, string> sculptDictEA)
        {
            InitializeComponent();
            Custom_radioButton.Text = "Custom Sculpt";
            EA_radioButton.Text = "Game Sculpt";
            Chooser_Remove_button.Visible = false;
            customDict = new SortedDictionary<string, ulong>();
            for (int i = 0; i < sculptList.Count; i++)
            {
                if (!customDict.ContainsKey(sculptList[i].SculptName)) 
                    customDict.Add(sculptList[i].SculptName, sculptList[i].publicKey[0].Instance);
            }
            eaDict = new SortedDictionary<string, ulong>();
            foreach (KeyValuePair<ulong, string> pair in sculptDictEA)
            {
                eaDict.Add(pair.Value, pair.Key);
            }
            List_Custom();
        }

        private void List_Custom()
        {
            SMOD_Chooser_comboBox.Items.Clear();
            SMOD_Chooser_comboBox.Items.AddRange((new List<string>(customDict.Keys)).ToArray());
        }

        private void List_EA()
        {
            SMOD_Chooser_comboBox.Items.Clear();
            SMOD_Chooser_comboBox.Items.AddRange((new List<string>(eaDict.Keys)).ToArray());
        }

        private void Custom_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (Custom_radioButton.Checked) List_Custom();
        }

        private void EA_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (EA_radioButton.Checked) List_EA();
        }

        private void SMOD_Chooser_Go_button_Click(object sender, EventArgs e)
        {
            ReturnName = (string)SMOD_Chooser_comboBox.SelectedItem;
            if (Custom_radioButton.Checked)
            {
                ReturnInstance = customDict[ReturnName];
                ReturnIsEA = false;
            }
            else
            {
                ReturnInstance = eaDict[ReturnName];
                ReturnName += " (EA)";
                ReturnIsEA = true;
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SMOD_Chooser_Cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void SMOD_Chooser_Remove_button_Click(object sender, EventArgs e)
        {
            ReturnInstance = 0;
            ReturnName = "";
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
