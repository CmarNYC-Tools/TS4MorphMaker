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
using System.Globalization;
using System.Windows.Forms;

namespace MorphTool
{
    public partial class BoneEntryEditor : Form
    {
        private Dictionary<uint, string> boneHash2Name;
        private Dictionary<string, uint> boneName2Hash;
        private int currentBone;
        public SMOD.BoneEntry[] BoneEntryReturnList;

        public BoneEntryEditor(SMOD.BoneEntry[] boneEntryList, BOND bond, Dictionary<uint, string> boneNameDict)
        {
            InitializeComponent();
            boneHash2Name = new Dictionary<uint, string>();
            boneName2Hash = new Dictionary<string, uint>();
            foreach (BOND.BoneAdjust bone in bond.adjustments)
            {
                string bonename;
                if (!boneNameDict.TryGetValue(bone.slotHash, out bonename)) bonename = "Unknown";
                boneHash2Name.Add(bone.slotHash, bonename);
                boneName2Hash.Add(bonename, bone.slotHash);
                bones_comboBox.Items.Add(bonename);
            }
            foreach (SMOD.BoneEntry b in boneEntryList)
            {
                string[] s = new string[2];
                s[0] = b.boneHash.ToString("X8");
                s[1] = boneHash2Name[b.boneHash];
                int ind = BoneEntry_dataGridView.Rows.Add(s);
                BoneEntry_dataGridView.Rows[ind].Tag = b;
            }
            if (boneEntryList.Length > 0) ShowBoneEntry(0);
        }

        private void ShowBoneEntry(int index)
        {
            SMOD.BoneEntry b = (SMOD.BoneEntry)BoneEntry_dataGridView.Rows[index].Tag;
            bones_comboBox.SelectedItem = boneHash2Name[b.boneHash];
            multiplier.Text = b.multiplier.ToString();
        }

        private void Add_button_Click(object sender, EventArgs e)
        {
            bones_comboBox.SelectedIndex = 0;
            string newbonename = (string)bones_comboBox.SelectedItem;
            uint hash = boneName2Hash[newbonename];
            SMOD.BoneEntry newbone = new SMOD.BoneEntry(hash, 1f);
            for (int i = 0; i < BoneEntry_dataGridView.Rows.Count; i++)
            {
                BoneEntry_dataGridView.Rows[i].Selected = false;
            }
            currentBone = BoneEntry_dataGridView.Rows.Add(new string[] { hash.ToString(), newbonename });
            BoneEntry_dataGridView.Rows[currentBone].Selected = true;
            ShowBoneEntry(currentBone);
        }

        private void BoneEntry_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;
            if (e.RowIndex != currentBone)
            {
                if (!SaveBone(e.RowIndex))
                {
                    BoneEntry_dataGridView.Rows[currentBone].Selected = true;
                    BoneEntry_dataGridView.Rows[e.RowIndex].Selected = false;
                    return;
                }
                currentBone = e.RowIndex;
            }
            ShowBoneEntry(currentBone);
            if (e.ColumnIndex == 2)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this bone entry?", "Delete Entry", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    BoneEntry_dataGridView.Rows.RemoveAt(e.RowIndex);
                    if (BoneEntry_dataGridView.Rows.Count > 0)
                    {
                        BoneEntry_dataGridView.Rows[0].Selected = true;
                        ShowBoneEntry(0);
                    }
                    currentBone = 0;
                    return;
                }
            }
        } 

        private bool SaveBone(int index)
        {
            uint hash = boneName2Hash[(string)bones_comboBox.SelectedItem];
            float mult;
            if (!float.TryParse(multiplier.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out mult))
            {
                MessageBox.Show("You have not entered a valid number for the multiplier!");
                return false;
            }
            SMOD.BoneEntry b = new SMOD.BoneEntry(hash, mult);
            BoneEntry_dataGridView.Rows[index].Tag = b;
            return true;
        }
        
        private void Save_button_Click(object sender, EventArgs e)
        {
            if (!SaveBone(currentBone)) return;
            List<SMOD.BoneEntry> bones = new List<SMOD.BoneEntry>();
            foreach (DataGridViewRow row in BoneEntry_dataGridView.Rows)
            {
                bones.Add((SMOD.BoneEntry)row.Tag);
            }
            BoneEntryReturnList = bones.ToArray();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_button_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
