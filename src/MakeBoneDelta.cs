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
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        int currentAdjustIndex;
        BOND currentBOND = null;
        RIG currentRig = GetTS4Rig(Species.Human, AgeGender.Adult);
        RIG.Bone currentBone;
        Quaternion currentRotation = Quaternion.Identity;
        AgeGender currentModelGender = AgeGender.Female;

        private void CustomRig_button_Click(object sender, EventArgs e)
        {
            string path = GetFilename("Select Custom RIG file", RIGfilter);
            if (String.Compare(path, " ") < 0 || !File.Exists(path)) return;
            BinaryReader br;
            RIG rig;
            if ((br = new BinaryReader(File.OpenRead(path))) != null)
            {
                using (br)
                {
                    rig = new RIG(br);
                }
                br.Dispose();
            }
            else
            {
                MessageBox.Show("Can't open custom RIG " + path + "!");
                return;
            }
            foreach (RIG.Bone bone in rig.Bones)
            {
                if (!boneHashDict.ContainsKey(bone.BoneHash))
                {
                    boneHashDict.Add(bone.BoneHash, bone.BoneName);
                    boneNameDict.Add(bone.BoneName, bone.BoneHash);
                }
            }
            currentRig = rig;
            rigsList.Add(rig);
            BONDRigs_comboBox.SelectedIndex = BONDRigs_comboBox.Items.Add(Path.GetFileNameWithoutExtension(path));
        }

        private void BOND_New_button_Click(object sender, EventArgs e)
        {
            if (BOND_dataGridView.RowCount > 0)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to start a new Bone Morph file?", "Start New File", MessageBoxButtons.OKCancel);
                if (res == DialogResult.Cancel) return;
            }
            BOND_dataGridView.Rows.Clear();
            ClearBoneInfo();
            BOND_Identifier.Tag = null;
            BOND_Identifier.Text = "";
            currentBOND = null;
            currentRig = new RIG(rigsList[BONDRigs_comboBox.SelectedIndex]);
            BoneDeltaPreviewUpdate(true);
            currentBOND = new BOND();
            currentAdjustIndex = 0;
            BOND_NewBone_button_Click(sender, e);
        }

        private void BOND_File_button_Click(object sender, EventArgs e)
        {
            BOND_File.Text = GetFilename("Select BoneDelta/BOND/SlotAdjust file", BONDfilter);
            if (String.Compare(BOND_File.Text, " ") <= 0) return;
            currentRig = new RIG(rigsList[BONDRigs_comboBox.SelectedIndex]);
            BOND_dataGridView.Rows.Clear();
            ClearBoneInfo();
            if (GetBONDData(BOND_File.Text, out currentBOND))
            {
                BOND_Identifier.Tag = currentBOND.publicKey[0].Instance;
                BOND_Identifier.Text = "0x" + currentBOND.publicKey[0].Instance.ToString("X16");
                foreach (BOND.BoneAdjust ba in currentBOND.adjustments)
                {
                    string[] str = new string[2];
                    str[0] = "0x" + ba.slotHash.ToString("X8");
                    if (!boneHashDict.TryGetValue(ba.slotHash, out str[1]))
                    {
                        str[1] = "unknown";
                    }
                    int ind = BOND_dataGridView.Rows.Add(str);
                    BOND_dataGridView.Rows[ind].Cells[0].Tag = ba.slotHash;
                    BOND_dataGridView.Rows[ind].HeaderCell.Value = ind.ToString();
                }
            }
            else
            {
                MessageBox.Show("Can't read BoneDelta file!");
                return;
            }
            BOND_dataGridView.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
            if (currentBOND.adjustments.Length > 0)
            {
                BOND_dataGridView.Rows[0].Selected = true;
                currentAdjustIndex = 0;
                ShowBoneInfo(0);
            }
            BoneAdjust_groupBox.Enabled = true;
            BoneDeltaPreviewUpdate(true);
        }

        private void BOND_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentAdjustIndex)
            {
                //if (!SaveBone())
                //{
                //    BOND_dataGridView.Rows[e.RowIndex].Selected = false;
                //    BOND_dataGridView.Rows[currentAdjustIndex].Selected = true;
                //    return;
                //}
                currentAdjustIndex = e.RowIndex;
            }
            if (e.ColumnIndex == 2)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this bone adjustment?", "Delete Adjustment", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    currentBOND.RemoveBoneAdjust(e.RowIndex);
                    BOND_dataGridView.Rows.RemoveAt(e.RowIndex);
                    ClearBoneInfo();
                    currentAdjustIndex = 0;
                    BoneAdjust_groupBox.Enabled = BOND_dataGridView.RowCount > 0;
                    BoneDeltaPreviewUpdate(false);
                }
            }
            for (int i = 0; i < BOND_dataGridView.Rows.Count; i++)
            {
                BOND_dataGridView.Rows[i].Selected = false;
            }
            if (BOND_dataGridView.Rows.Count > 0)
            {
                BOND_dataGridView.Rows[currentAdjustIndex].Selected = true;
                ShowBoneInfo(currentAdjustIndex);
            }
        }

        private void BOND_NewBone_button_Click(object sender, EventArgs e)
        {
            if (BOND_dataGridView.Rows.Count > 0 && !SaveBone()) return;
            BOND_Bones_comboBox.SelectedIndexChanged -= BOND_Bones_comboBox_SelectedIndexChanged;
            ClearBoneInfo();
            BOND_Bones_comboBox.SelectedIndex = 0;
            BOND_Bones_comboBox.SelectedIndexChanged += BOND_Bones_comboBox_SelectedIndexChanged;
            for (int i = 0; i < BOND_dataGridView.Rows.Count; i++)
            {
                BOND_dataGridView.Rows[i].Selected = false;
            }
            BOND.BoneAdjust adjust = new BOND.BoneAdjust();
            currentBOND.AddBoneAdjust(adjust);
            string[] str = new string[2];
            str[0] = "0x" + adjust.slotHash.ToString("X8");
            str[1] = "undefined";
            int ind = BOND_dataGridView.Rows.Add(str);
            BOND_dataGridView.Rows[ind].HeaderCell.Value = ind.ToString();
            BOND_dataGridView.Rows[ind].Selected = true;
            BoneAdjust_groupBox.Enabled = true;
            currentAdjustIndex = ind;
            Setup_Bone();
            BOND_dataGridView.Refresh();
        }

        private void Setup_Bone()
        {
            uint hash;
            string bname = BOND_Bones_comboBox.GetItemText(BOND_Bones_comboBox.SelectedItem);
            boneNameDict.TryGetValue(bname, out hash);
            currentBone = currentRig.GetBone(hash);

            if (currentBone != null)
            {
                currentRotation = currentBone.MorphRotation;
            }
            else
            {
                currentRotation = Quaternion.Identity;
            }

            BOND_dataGridView.Rows[currentAdjustIndex].Cells[0].Value = "0x" + hash.ToString("X8");
            BOND_dataGridView.Rows[currentAdjustIndex].Cells[0].Tag = hash;
            BOND_dataGridView.Rows[currentAdjustIndex].Cells[1].Value = bname;
        }

        private bool SaveBone()
        {
            if (currentBOND.adjustments.Length == 0) return true;
            BOND.BoneAdjust adjust = new BOND.BoneAdjust(currentBOND.adjustments[currentAdjustIndex]);
            string bname = BOND_dataGridView.Rows[currentAdjustIndex].Cells[1].Value.ToString();
            adjust.slotHash = (uint)BOND_dataGridView.Rows[currentAdjustIndex].Cells[0].Tag;
            float ox, oy, oz, sx, sy, sz, rx, ry, rz;
            if (!float.TryParse(BOND_OffsetX.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out ox) ||
                !float.TryParse(BOND_OffsetY.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out oy) ||
                !float.TryParse(BOND_OffsetZ.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out oz) ||
                !float.TryParse(BOND_ScaleX.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out sx) ||
                !float.TryParse(BOND_ScaleY.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out sy) ||
                !float.TryParse(BOND_ScaleZ.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out sz) ||
                !float.TryParse(BOND_QuatX.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out rx) ||
                !float.TryParse(BOND_QuatY.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out ry) ||
                !float.TryParse(BOND_QuatZ.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out rz)) 
            {
                MessageBox.Show("One or more numeric values is not in a correct format!");
                return false;
            }
            Vector3 offset = new Vector3((double)BOND_OffsetX.Tag, (double)BOND_OffsetY.Tag, (double)BOND_OffsetZ.Tag);
            Vector3 scale = new Vector3((double)BOND_ScaleX.Tag, (double)BOND_ScaleY.Tag, (double)BOND_ScaleZ.Tag);
            Euler rot = new Euler((double)BOND_QuatX.Tag, (double)BOND_QuatY.Tag, (double)BOND_QuatZ.Tag);
            if (BONDDegrees_radioButton.Checked) rot = rot * (Math.PI / 180f);
            Quaternion q = new Quaternion(rot);
            q.Normalize();
            if (BONDGlobal_radioButton.Checked)
            {
                Vector3 unit = new Vector3(1d, 1d, 1d);
                if (!(q.isIdentity || q.isEmpty)) q = currentRotation.Conjugate() * q * currentRotation;
                offset = ((currentRotation.Conjugate() * offset) * currentRotation).toVector3();
                scale = (currentRotation.toMatrix3x3().Inverse() * Matrix3D.FromScale(scale + unit)).Scale - unit;
              //  scale = ((currentRotation.Conjugate() * (scale + unit)) * currentRotation).toVector3() - unit;
            }
            adjust.offsetX = offset.X;
            adjust.offsetY = offset.Y;
            adjust.offsetZ = offset.Z;
            adjust.scaleX = scale.X;
            adjust.scaleY = scale.Y;
            adjust.scaleZ = scale.Z;
            adjust.quatX = (float)q.X;
            adjust.quatY = (float)q.Y;
            adjust.quatZ = (float)q.Z;
            adjust.quatW = (float)q.W;
            currentBOND.adjustments[currentAdjustIndex] = adjust;
            
            BOND_dataGridView.Rows[currentAdjustIndex].Selected = true;
            BoneDeltaPreviewUpdate(false);
            return true;
        }

        private void BOND_Save_button_Click(object sender, EventArgs e)
        {
            if (!SaveBone()) return;
            ulong publicLink;
            string identifier = "";
            if (BOND_Identifier.Tag == null)
            {
                if (String.Compare(BOND_Identifier.Text, " ") <= 0)
                {
                    MessageBox.Show("You must enter a unique morph identifier!");
                    BOND_Identifier.Focus();
                    return;
                }
                publicLink = FNVhash.FNV64(BOND_Identifier.Text) | 0x8000000000000000U;
                identifier = "_" + BOND_Identifier.Text;
            }
            else
            {
                if (!UInt64.TryParse(BOND_Identifier.Text.Replace("0x", ""), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out publicLink))
                {
                    MessageBox.Show("The BoneDelta Instance Identifier is not in the correct hex format!");
                    return;
                }
            }
            currentBOND.publicKey[0] = new TGI((uint)ResourceTypes.BoneDelta, 0, publicLink);
            WriteBONDFile("Save modified BoneDelta file", currentBOND, "S4_0355E0A6_00000000_" + publicLink.ToString("X16") + identifier);
        }

        private void ShowBoneInfo(int index)
        {
            InputCheckOff();
            bool boneFound = false;
            BOND.BoneAdjust adjust = currentBOND.adjustments[index];
            for (int i = 0; i < BOND_Bones_comboBox.Items.Count; i++)
            {
                uint hash;
                boneNameDict.TryGetValue(BOND_Bones_comboBox.GetItemText(BOND_Bones_comboBox.Items[i]), out hash);
                if (adjust.slotHash == hash)
                {
                    boneFound = true;
                    BOND_Bones_comboBox.SelectedIndex = i;
                }
            }
            if (!boneFound)
            {
                BOND_Bones_comboBox.SelectedIndex = BOND_Bones_comboBox.Items.Add("0x" + adjust.slotHash.ToString("X8"));
            }

            Quaternion q = new Quaternion(adjust.quatX, adjust.quatY, adjust.quatZ, adjust.quatW);
            if (q.isEmpty) q = Quaternion.Identity;
            q.Balance();
            if (BONDGlobal_radioButton.Checked)
            {
                Vector3 offset = ((currentRotation * new Vector3(adjust.offsetX, adjust.offsetY, adjust.offsetZ)) * currentRotation.Inverse()).toVector3();
                Vector3 scale = (currentRotation.toMatrix3x3() * Matrix3D.FromScale(new Vector3(adjust.scaleX + 1d, adjust.scaleY + 1d, adjust.scaleZ + 1d))).Scale - new Vector3(1d, 1d, 1d);
                //MessageBox.Show(new Vector3(adjust.scaleX, adjust.scaleY, adjust.scaleZ) + Environment.NewLine +
                //    (rotationMatrix * Matrix3x3.FromScale(new Vector3(adjust.scaleX + 1f, adjust.scaleY + 1f, adjust.scaleZ + 1))).Scale.ToString() + Environment.NewLine +
                //    scale.ToString() + Environment.NewLine +
                //    (rotationMatrix.Transpose() * Matrix3x3.FromScale(scale + new Vector3(1f, 1f, 1f))).Scale.ToString());
                PrettyNumber(BOND_OffsetX, offset.X);
                PrettyNumber(BOND_OffsetY, offset.Y);
                PrettyNumber(BOND_OffsetZ, offset.Z);
                PrettyNumber(BOND_ScaleX, scale.X);
                PrettyNumber(BOND_ScaleY, scale.Y);
                PrettyNumber(BOND_ScaleZ, scale.Z);
                if (!q.isEmpty && !q.isIdentity) q = currentRotation * q * currentRotation.Conjugate();
              //  MessageBox.Show(q.ToString());
            }
            else
            {
                PrettyNumber(BOND_OffsetX, adjust.offsetX);
                PrettyNumber(BOND_OffsetY, adjust.offsetY);
                PrettyNumber(BOND_OffsetZ, adjust.offsetZ);
                PrettyNumber(BOND_ScaleX, adjust.scaleX);
                PrettyNumber(BOND_ScaleY, adjust.scaleY);
                PrettyNumber(BOND_ScaleZ, adjust.scaleZ);
            }

            Euler rot = q.toEuler();
            if (BONDDegrees_radioButton.Checked) rot = rot * (180f / Math.PI);
            PrettyNumber(BOND_QuatX, rot.X);
            PrettyNumber(BOND_QuatY, rot.Y);
            PrettyNumber(BOND_QuatZ, rot.Z);

            InputCheckOn();
        }
        
        private void ClearBoneInfo()
        {
            InputCheckOff();
            BOND_OffsetX.Text = "0";
            BOND_OffsetY.Text = "0";
            BOND_OffsetZ.Text = "0";
            BOND_ScaleX.Text = "0";
            BOND_ScaleY.Text = "0";
            BOND_ScaleZ.Text = "0";
            BOND_QuatX.Text = "0";
            BOND_QuatY.Text = "0";
            BOND_QuatZ.Text = "0";
            BOND_OffsetX.Tag = 0d;
            BOND_OffsetY.Tag = 0d;
            BOND_OffsetZ.Tag = 0d;
            BOND_ScaleX.Tag = 0d;
            BOND_ScaleY.Tag = 0d;
            BOND_ScaleZ.Tag = 0d;
            BOND_QuatX.Tag = 0d;
            BOND_QuatY.Tag = 0d;
            BOND_QuatZ.Tag = 0d;
            InputCheckOn();
        }

        private void BONDGlobal_radioButton_CheckedChanged(object sender, EventArgs e)
        {            
            //Vector3 offset = new Vector3(ox, oy, oz); 
            //Vector3 scale = new Vector3(sx, sy, sz);
            //Euler rot = new Euler((double)BOND_QuatX.Tag, (double)BOND_QuatY.Tag, (double)BOND_QuatZ.Tag);
            BOND.BoneAdjust adjust = currentBOND.adjustments[currentAdjustIndex];
            Vector3 offset = new Vector3(adjust.offsetX, adjust.offsetY, adjust.offsetZ);
            Vector3 scale = new Vector3(adjust.scaleX, adjust.scaleY, adjust.scaleZ);
            Quaternion q = new Quaternion(adjust.quatX, adjust.quatY, adjust.quatZ, adjust.quatW);
            if (q.isEmpty) q = Quaternion.Identity;
            q.Balance();

            Vector3 unit = new Vector3(1d, 1d, 1d);
            if (BONDGlobal_radioButton.Checked)
            {
                offset = ((currentRotation * offset) * currentRotation.Conjugate()).toVector3();
                scale = (currentRotation.toMatrix3x3() * Matrix3D.FromScale(scale + unit)).Scale - unit;
                if (!(q.isIdentity || q.isEmpty)) q = currentRotation * q * currentRotation.Conjugate();
            }
            Euler rot = q.toEuler();
            if (BONDDegrees_radioButton.Checked) rot = rot * (180f / Math.PI);
            InputCheckOff();
            PrettyNumber(BOND_OffsetX, offset.X);
            PrettyNumber(BOND_OffsetY, offset.Y);
            PrettyNumber(BOND_OffsetZ, offset.Z);
            PrettyNumber(BOND_ScaleX, scale.X);
            PrettyNumber(BOND_ScaleY, scale.Y);
            PrettyNumber(BOND_ScaleZ, scale.Z);
            PrettyNumber(BOND_QuatX, rot.X);
            PrettyNumber(BOND_QuatY, rot.Y);
            PrettyNumber(BOND_QuatZ, rot.Z);
            InputCheckOn();
        }

        private void BONDRadians_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            BOND.BoneAdjust adjust = currentBOND.adjustments[currentAdjustIndex];
            Quaternion q = new Quaternion(adjust.quatX, adjust.quatY, adjust.quatZ, adjust.quatW);
            if (q.isEmpty) q = Quaternion.Identity;
            q.Balance();
            if (BONDGlobal_radioButton.Checked)
            {
                if (!(q.isIdentity || q.isEmpty)) q = currentRotation * q * currentRotation.Conjugate();
            }
            Euler rot = q.toEuler();
            if (BONDDegrees_radioButton.Checked) rot = rot * (180f / Math.PI);
            InputCheckOff();
            PrettyNumber(BOND_QuatX, rot.X);
            PrettyNumber(BOND_QuatY, rot.Y);
            PrettyNumber(BOND_QuatZ, rot.Z);
            InputCheckOn();
        }

        private void BONDRigs_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            currentRig = new RIG(rigsList[BONDRigs_comboBox.SelectedIndex]);
            LoadRigBones(currentRig);
            int index = BONDRigs_comboBox.SelectedIndex;
            if (!(index == 0 && currentModelGender == AgeGender.Male)) index++;
            BONDmodel_comboBox.SelectedIndex = index;
        }

        private void LoadRigBones(RIG rig)
        {
            BOND_Bones_comboBox.Items.Clear();
            foreach (RIG.Bone bone in currentRig.Bones)
            {
                BOND_Bones_comboBox.Items.Add(bone.BoneName);
            }
            if (BOND_dataGridView.Rows.Count > 0) ShowBoneInfo(currentAdjustIndex);
        }

        private void PrettyNumber(TextBox textbox, double d)
        {
            textbox.Text = Math.Abs(d) > 0.00001 ? d.ToString("G5") : "0";
            textbox.Tag = d;
        }

        private void BOND_Bones_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (BOND_dataGridView.RowCount == 0 || BOND_Bones_comboBox.SelectedIndex < 0) return;
            Setup_Bone();
        }

        private void BOND_Input_TextChanged(object sender, EventArgs e)
        {
            TextBox text = sender as TextBox;
            if (string.Compare(text.Text, " ") <= 0 || string.Compare(text.Text, ".") == 0 || string.Compare(text.Text, "-.") == 0) return;
            double d;
            if (!double.TryParse(text.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out d))
            {
                MessageBox.Show("You have entered an invalid number!");
                return;
            }
            text.Tag = d;
            SaveBone();
        }

        private void InputCheckOff()
        {
            BOND_OffsetX.TextChanged -= BOND_Input_TextChanged;
            BOND_OffsetY.TextChanged -= BOND_Input_TextChanged;
            BOND_OffsetZ.TextChanged -= BOND_Input_TextChanged;
            BOND_QuatX.TextChanged -= BOND_Input_TextChanged;
            BOND_QuatY.TextChanged -= BOND_Input_TextChanged;
            BOND_QuatZ.TextChanged -= BOND_Input_TextChanged;
            BOND_ScaleX.TextChanged -= BOND_Input_TextChanged;
            BOND_ScaleY.TextChanged -= BOND_Input_TextChanged;
            BOND_ScaleZ.TextChanged -= BOND_Input_TextChanged;
        }

        private void InputCheckOn()
        {
            BOND_OffsetX.TextChanged += BOND_Input_TextChanged;
            BOND_OffsetY.TextChanged += BOND_Input_TextChanged;
            BOND_OffsetZ.TextChanged += BOND_Input_TextChanged;
            BOND_QuatX.TextChanged += BOND_Input_TextChanged;
            BOND_QuatY.TextChanged += BOND_Input_TextChanged;
            BOND_QuatZ.TextChanged += BOND_Input_TextChanged;
            BOND_ScaleX.TextChanged += BOND_Input_TextChanged;
            BOND_ScaleY.TextChanged += BOND_Input_TextChanged;
            BOND_ScaleZ.TextChanged += BOND_Input_TextChanged;
        }

        private void BoneDeltaReadMe_button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This preview may not predict how a bone morph appears" + Environment.NewLine +
                            "in CAS or the game. Some offsets, scaling, and/or rotations" + Environment.NewLine +
                            "may not work as expected, and in some cases part of the" + Environment.NewLine +
                            "sim may disappear. Disappearance may be unpredictable." + Environment.NewLine +
                            "How TS4 implements bone deltas is still poorly understood.");
        }
    }
}
