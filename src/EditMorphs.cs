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
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using s4pi.ImageResource;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        int currentSMOD = 0;
        int currentSculpt = 0;

        public void SetupMorphsEditors()
        {
            SMODSpecies_comboBox.Items.AddRange(Enum.GetNames(typeof(Species)));
            SMODSpecies_comboBox.SelectedIndex = 0;
            SMODRegion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimRegion)));
            SMODRegion_comboBox.SelectedIndex = 0;
            SMODSubregion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimSubRegion)));
            SMODSubregion_comboBox.SelectedIndex = 0;
            SculptSpecies_comboBox.Items.AddRange(Enum.GetNames(typeof(Species)));
            SculptSpecies_comboBox.SelectedIndex = 0;
            SculptRegion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimRegion)).Where(s => !s.Contains("CUSTOM_")).ToArray());
            SculptRegion_comboBox.SelectedIndex = 0;
            SculptSubregion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimSubRegion)));
            SculptSubregion_comboBox.SelectedIndex = 0;
        }

        public Species GetPreviewSpecies(ComboBox combobox)
        {
            return (Species)(combobox.SelectedIndex + 1);
        }
        
        public string GetSmodName(ulong instance, out bool isEA)
        {
            if (instance == 0) { isEA = false; return "None"; }
            int ind = ListKeyLookUp(smodList, instance);
            string smodName = null;
            if (ind >= 0)
            {
                smodName = smodList[ind].smodName;
                isEA = false;
            }
            else
            {
                isEA = true;
                try { smodName = smodDictEA[instance] + " (EA)"; }
                catch 
                {
                    smodName = "0x" + instance.ToString("X16");
                    Predicate<IResourceIndexEntry> predSmod = r => r.ResourceType == (uint)ResourceTypes.SimModifier &
                        r.ResourceGroup == 0x00000000U & r.Instance == instance;
                    isEA = IsInGamePacks(predSmod);
                    smodName += isEA ? " (EA)" : " (Not found)";
                }
            }
            return smodName;
        }

        private void Show_MorphButtonStates(bool hasMorphResource, bool EAmorphResource, Button importButton, Button deleteButton, Button exportButton)
        {
            if (hasMorphResource || EAmorphResource)
            {
                exportButton.Visible = true;
            }
            else
            {
                exportButton.Visible = false;
            }
            Show_MorphButtonStates(hasMorphResource, EAmorphResource, importButton, deleteButton);
        }

        private void Show_MorphButtonStates(bool hasMorphResource, bool EAmorphResource, Button importButton, Button deleteButton)
        {
            if (hasMorphResource || EAmorphResource)
            {
                importButton.Text = "Replace";
                deleteButton.Visible = true;
            }
            else
            {
                importButton.Text = "Import";
                deleteButton.Visible = false;
            }
        }

        private void ListSMOD()
        {
            SMODs_dataGridView.Rows.Clear();
            ClearSMOD();
            for (int i = 0; i < smodList.Count; i++)
            {
                string[] s = new string[2];
                s[0] = smodList[i].smodName;
                s[1] = ListMorphResources(smodList[i]);
                SMODs_dataGridView.Rows.Add(s);
            }
            currentSMOD = 0;
            if (smodList.Count > 0)
            {
                ShowSMOD(smodList[0]);
                EditSmods_panel.Enabled = true;
            }
            else
            {
                EditSmods_panel.Enabled = false;
            }
        }

        private string ListMorphResources(SMOD smod)
        {
            string s = "";
            if (smod.BGEOKey != null && smod.BGEOKey.Length > 0 && smod.BGEOKey[0] != null && smod.BGEOKey[0].Instance != 0) s += "BGEO" + (smod.bgeo != null ? " (Custom) " : (IsInGamePacks(smod.BGEOKey[0]) ? " (EA) " : " (Not found) "));
            if (smod.deformerMapShapeKey != null && smod.deformerMapShapeKey.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "DMap Shape" + (smod.shape != null ? " (Custom) " : (IsInGamePacks(smod.deformerMapShapeKey) ? " (EA) " : " (Not found) "));
            if (smod.deformerMapNormalKey != null && smod.deformerMapNormalKey.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "DMap Normals" + (smod.normals != null ? " (Custom) " : (IsInGamePacks(smod.deformerMapNormalKey) ? " (EA) " : " (Not found) "));
            if (smod.bonePoseKey != null && smod.bonePoseKey.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "Bone Delta" + (smod.bond != null ? " (Custom) " : (IsInGamePacks(smod.bonePoseKey) ? " (EA) " : " (Not found) "));
            return s;
        }

        private void SMODs_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentSMOD)
            {
                SaveSMOD(smodList[currentSMOD]);
                currentSMOD = e.RowIndex;
            }
            ShowSMOD(smodList[e.RowIndex]);
            if (e.ColumnIndex == 2)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this Sim Modifier?", "Delete Sim Modifier", MessageBoxButtons.YesNo);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    smodList.RemoveAt(e.RowIndex);
                    ListSMOD();
                }
            }
        }

        private void ShowSMOD(SMOD smod)
        {
            EditSmods_panel.Enabled = true;
            SMODmorphName.Text = smod.smodName;
            ShowAgeGender(smod.ageGender, SMODage_checkedListBox, SMODgender_checkedListBox);
            SMODRegion_comboBox.SelectedItem = smod.region.ToString();
            SMODSubregion_comboBox.SelectedItem = smod.subRegion.ToString();

            Show_MorphButtonStates(smod.bgeo != null, (smod.bgeo == null & smod.BGEOKey.Length > 0 ? IsInGamePacks(smod.BGEOKey[0]) : false), SMOD_BGEO_button, SMOD_BGEO_Delete_button, SMOD_BGEO_Export_button);
            Show_MorphButtonStates(smod.shape != null, (smod.shape == null && smod.deformerMapShapeKey.Instance > 0 ? IsInGamePacks(smod.deformerMapShapeKey) : false), SMOD_DMapShape_button, SMOD_DMapShape_Delete_button, SMOD_DMapShape_Export_button);
            Show_MorphButtonStates(smod.normals != null, (smod.normals == null && smod.deformerMapNormalKey.Instance > 0 ? IsInGamePacks(smod.deformerMapNormalKey) : false), SMOD_DMapNormals_button, SMOD_DMapNormals_Delete_button, SMOD_DMapNormals_Export_button);
            Show_MorphButtonStates(smod.bond != null, (smod.bond == null && smod.bonePoseKey.Instance > 0 ? IsInGamePacks(smod.bonePoseKey) : false), SMOD_BoneDelta_button, SMOD_BoneDelta_Delete_button, SMOD_BoneDelta_Export_button);

            SMODboneEntries_button.Enabled = smod.bonePoseKey.Instance > 0;
        }

        private void ClearSMOD()
        {
            SMODmorphName.Text = "";
            ShowAgeGender(AgeGender.None, SMODage_checkedListBox, SMODgender_checkedListBox);
            SMODRegion_comboBox.SelectedIndex = 0;
            SMODSubregion_comboBox.SelectedIndex = 0;

            Show_MorphButtonStates(false, false, SMOD_BGEO_button, SMOD_BGEO_Delete_button, SMOD_BGEO_Export_button);
            Show_MorphButtonStates(false, false, SMOD_DMapShape_button, SMOD_DMapShape_Delete_button, SMOD_DMapShape_Export_button);
            Show_MorphButtonStates(false, false, SMOD_DMapNormals_button, SMOD_DMapNormals_Delete_button, SMOD_DMapNormals_Export_button);
            Show_MorphButtonStates(false, false, SMOD_BoneDelta_button, SMOD_BoneDelta_Delete_button, SMOD_BoneDelta_Export_button);
        }

        private void SMOD_Clone_button_Click(object sender, EventArgs e)
        {
            ClonePickerForm pick = new ClonePickerForm(gamePackages, ResourceTypes.SimModifier, hotcDictEA, presetDictEA, smodDictEA, dmapDictEA);
            pick.ShowDialog();
            if (pick.DialogResult == DialogResult.OK)
            {
                Cursor curse = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                smodList.AddRange(pick.clonedSmodList);
                ListSMOD();
                this.Cursor = curse;
                Application.DoEvents();
            }
        }

        private void SMOD_Add_button_Click(object sender, EventArgs e)
        {
            if (smodList.Count > 0) SaveSMOD(smodList[currentSMOD]);
            SMOD smod = new SMOD();
            smod.smodName = "Custom Sim Modifier 0x" + ran.Next().ToString();
            int ind = SMODs_dataGridView.Rows.Add(new string[] { smod.smodName, "" });
            smodList.Add(smod);
            SMODs_dataGridView.Rows[currentSMOD].Selected = false;
            SMODs_dataGridView.Rows[ind].Selected = true;
            currentSMOD = ind;
            ShowSMOD(smodList[currentSMOD]);
        }

        private void SMOD_BGEO_button_Click(object sender, EventArgs e)
        {
            string bgeoFile = GetFilename("Select .bgeo/.blendgeom file", BGEOfilter);
            if (String.Compare(bgeoFile, " ") > 0)
            {
                BGEO bgeo = null;
                if (!GetBgeoData(bgeoFile, out bgeo))
                {
                    MessageBox.Show("Can't read BGEO file!");
                    return;
                }
                smodList[currentSMOD].bgeo = bgeo;
                smodList[currentSMOD].BGEOKey = new TGI[] { new TGI(bgeo.PublicKey) };
                SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
                Show_MorphButtonStates(true, false, SMOD_BGEO_button, SMOD_BGEO_Delete_button, SMOD_BGEO_Export_button);
            }
        }

        private void SMOD_DMapShape_button_Click(object sender, EventArgs e)
        {
            string dmapFile = GetFilename("Select shape .dmap/.deformermap file", DMapfilter);
            if (String.Compare(dmapFile, " ") > 0)
            {
                DMap dmap = null;
                if (!ReadDMap(dmapFile, out dmap))
                {
                    MessageBox.Show("Can't read DMap Shape file!");
                    return;
                }
                if (dmap.ShapeOrNormal != ShapeOrNormals.SHAPE_DEFORMER)
                {
                    MessageBox.Show("You have imported a DMap Normals file, should be a Shape file!");
                    return;
                }
                smodList[currentSMOD].shape = dmap;
                smodList[currentSMOD].deformerMapShapeKey = new TGI((uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(smodList[currentSMOD].smodName + "_Shape") | 0x8000000000000000);
                SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
                Show_MorphButtonStates(true, false, SMOD_DMapShape_button, SMOD_DMapShape_Delete_button, SMOD_DMapShape_Export_button);
            }
        }

        private void SMOD_DMapNormals_button_Click(object sender, EventArgs e)
        {
            string dmapFile = GetFilename("Select normals .dmap/.deformermap file", DMapfilter);
            if (String.Compare(dmapFile, " ") > 0)
            {
                DMap dmap = null;
                if (!ReadDMap(dmapFile, out dmap))
                {
                    MessageBox.Show("Can't read DMap Shape file!");
                    return;
                }
                if (dmap.ShapeOrNormal != ShapeOrNormals.NORMALS_DEFORMER)
                {
                    MessageBox.Show("You have imported a DMap Shape file, should be a Normals file!");
                    return;
                }
                smodList[currentSMOD].normals = dmap;
                smodList[currentSMOD].deformerMapNormalKey = new TGI((uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(smodList[currentSMOD].smodName + "_Normals") | 0x8000000000000000);
                SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
                Show_MorphButtonStates(true, false, SMOD_DMapNormals_button, SMOD_DMapNormals_Delete_button, SMOD_DMapNormals_Export_button);
            }
        }

        private void SMOD_BoneDelta_button_Click(object sender, EventArgs e)
        {
            string bondFile = GetFilename("Select BonePose/BoneDelta file", BONDfilter);
            if (String.Compare(bondFile, " ") > 0)
            {
                BOND bond = null;
                if (!GetBONDData(bondFile, out bond))
                {
                    MessageBox.Show("Can't read BoneDelta file!");
                    return;
                }
                smodList[currentSMOD].bond = bond;
                smodList[currentSMOD].bonePoseKey = new TGI((uint)ResourceTypes.BoneDelta, 0, FNVhash.FNV64(smodList[currentSMOD].smodName + "_BOND") | 0x8000000000000000);
                SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
                Show_MorphButtonStates(true, false, SMOD_BoneDelta_button, SMOD_BoneDelta_Delete_button, SMOD_BoneDelta_Export_button);
                SMODboneEntries_button.Enabled = true;
            }
        }

        private void SMOD_BGEO_Delete_button_Click(object sender, EventArgs e)
        {
            smodList[currentSMOD].bgeo = null;
            smodList[currentSMOD].BGEOKey = new TGI[0];
            SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
            Show_MorphButtonStates(false, false, SMOD_BGEO_button, SMOD_BGEO_Delete_button);
        }

        private void SMOD_DMapShape_Delete_button_Click(object sender, EventArgs e)
        {
            smodList[currentSMOD].shape = null;
            smodList[currentSMOD].deformerMapShapeKey = new TGI();
            SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
            Show_MorphButtonStates(false, false, SMOD_DMapShape_button, SMOD_DMapShape_Delete_button);
        }

        private void SMOD_DMapNormals_Delete_button_Click(object sender, EventArgs e)
        {
            smodList[currentSMOD].normals = null;
            smodList[currentSMOD].deformerMapNormalKey = new TGI();
            SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
            Show_MorphButtonStates(false, false, SMOD_DMapNormals_button, SMOD_DMapNormals_Delete_button);
        }

        private void SMOD_BoneDelta_Delete_button_Click(object sender, EventArgs e)
        {
            smodList[currentSMOD].bond = null;
            smodList[currentSMOD].bonePoseKey = new TGI();
            SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
            Show_MorphButtonStates(false, false, SMOD_BoneDelta_button, SMOD_BoneDelta_Delete_button, SMOD_BoneDelta_Export_button);
            SMODboneEntries_button.Enabled = false;
        }

        private void SMOD_BoneDelta_Export_button_Click(object sender, EventArgs e)
        {
            WriteBONDFile("Save BoneDelta/BOND file", smodList[currentSMOD].bond != null ? smodList[currentSMOD].bond : FetchGameBOND(smodList[currentSMOD].bonePoseKey), smodList[currentSMOD].smodName + "_BOND");
        }

        private void SMOD_BGEO_Export_button_Click(object sender, EventArgs e)
        {
            WriteBgeoFile("Save BGEO as a file", smodList[currentSMOD].bgeo != null ? smodList[currentSMOD].bgeo : FetchGameBGEO(smodList[currentSMOD].BGEOKey[0]), smodList[currentSMOD].smodName + "_BGEO");
        }

        private void SMOD_DMapShape_Export_button_Click(object sender, EventArgs e)
        {
            WriteDMap("Save Shape DMap as a file", smodList[currentSMOD].shape != null ? smodList[currentSMOD].shape : FetchGameDMap(smodList[currentSMOD].deformerMapShapeKey), smodList[currentSMOD].smodName + "_DMapShape");
        }

        private void SMOD_DMapNormals_Export_button_Click(object sender, EventArgs e)
        {
            WriteDMap("Save Normals DMap as a file", smodList[currentSMOD].normals != null ? smodList[currentSMOD].normals : FetchGameDMap(smodList[currentSMOD].deformerMapNormalKey), smodList[currentSMOD].smodName + "_DMapNormals");
        }

        private void SMODboneEntries_button_Click(object sender, EventArgs e)
        {
            BoneEntryEditor boneEntries = new BoneEntryEditor(smodList[currentSMOD].boneEntryList, smodList[currentSMOD].bond != null ? smodList[currentSMOD].bond : FetchGameBOND(smodList[currentSMOD].bonePoseKey), boneHashDict);
            boneEntries.ShowDialog();
            if (boneEntries.DialogResult == DialogResult.OK)
            {
                smodList[currentSMOD].boneEntryList = boneEntries.BoneEntryReturnList;
            }
        }

        private void SMOD_Save_button_Click(object sender, EventArgs e)
        {
            if (smodList.Count > 0)
            {
                SaveSMOD(smodList[currentSMOD]);
                SMODs_dataGridView.Rows[currentSMOD].Cells[0].Value = smodList[currentSMOD].smodName;
                SMODs_dataGridView.Rows[currentSMOD].Cells[1].Value = ListMorphResources(smodList[currentSMOD]);
            }
        }

        private void RegionHelp_button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("A list of custom regions has been added in an attempt to" + Environment.NewLine +
                            "simplify the use of presets for specific body regions." + Environment.NewLine +
                            "Use them in a body preset when you don't want it to affect" + Environment.NewLine +
                            "the entire body. They will still reset the breasts." + Environment.NewLine +
                            "USE CUSTOM REGIONS ONLY FOR BODY PRESETS.");
        }

        private void SaveSMOD(SMOD smod)
        {
            if (String.Compare(SMODmorphName.Text, " ") <= 0)
            {
                MessageBox.Show("You must enter a unique name for the morph!");
                return;
            }
            if ((smod.shape != null & smod.normals == null) | (smod.shape == null & smod.normals != null))
            {
                MessageBox.Show("If using DMaps, you must enter BOTH shape and normals!");
                return;
            }
            bool nameChange = String.Compare(smod.smodName, SMODmorphName.Text) != 0;
            if (smod.publicKey[0].Instance == 0)
            {
                TGI tgi = new TGI((uint)ResourceTypes.SimModifier, 0u, FNVhash.FNV64(smod.smodName) | 0x8000000000000000u);
                smod.publicKey[0] = tgi;
            }
            //if (!tgi.Equals(smod.publicKey[0]))
            //{
            //    foreach (HOTC hotc in hotcList)
            //    {
            //        hotc.ReplaceSmodInstance(smod.publicKey[0].Instance, tgi.Instance);
            //    }
            //    ListHOTC();
            //    foreach (CPRE cpre in presetList)
            //    {
            //        cpre.ReplaceSmodInstance(smod.publicKey[0].Instance, tgi.Instance);
            //    }
            //    ListPresets();
            //    smod.publicKey[0] = tgi;
            //}
            smod.ageGender = GetAgeGender(SMODage_checkedListBox, SMODgender_checkedListBox);
            smod.region = (SimRegion)Enum.Parse(typeof(SimRegion), (string)SMODRegion_comboBox.SelectedItem);
            smod.subRegion = (SimSubRegion)Enum.Parse(typeof(SimSubRegion), (string)SMODSubregion_comboBox.SelectedItem);
            if (nameChange)
            {
                smod.smodName = SMODmorphName.Text;
                ListHOTC();
                ListPresets();
            }
            ShowTreeView();
        }

        private void SMOD_Preview_button_Click(object sender, EventArgs e)
        {
            if (smodList.Count == 0) return;
            SMOD smod = smodList[currentSMOD];
            if ((smod.shape != null & smod.normals == null) | (smod.shape == null & smod.normals != null))
            {
                MessageBox.Show("If using DMaps, you must enter BOTH shape and normals!");
                return;
            }
            BGEO[] bgeo = null;
            DMap[] shape = null;
            DMap[] normals = null;
            BOND[] bond = null;
            if (smod.BGEOKey != null && smod.BGEOKey.Length > 0 && smod.BGEOKey[0] != null && smod.BGEOKey[0].Instance > 0ul)
                bgeo = new BGEO[] { smod.bgeo != null ? smod.bgeo : FetchGameBGEO(smod.BGEOKey[0]) };
            if (smod.deformerMapShapeKey.Instance > 0ul) shape = new DMap[] { smod.shape != null ? smod.shape : FetchGameDMap(smod.deformerMapShapeKey) };
            if (smod.deformerMapNormalKey.Instance > 0ul) normals = new DMap[] { smod.normals != null ? smod.normals : FetchGameDMap(smod.deformerMapNormalKey) };
            if (smod.bonePoseKey.Instance > 0ul) bond = new BOND[] { smod.bond != null ? smod.bond : FetchGameBOND(smod.bonePoseKey) };

            Form previewForm = new EditMorphPreview(GetPreviewSpecies(SMODSpecies_comboBox), smod.ageGender, AgeGender.None, smod.region, smod.subRegion, 
                bgeo, shape, normals, bond, null);
            previewForm.Text = smod.smodName;
            previewForm.Show();
        }


        private void ListSculpt()
        {
            Sculpt_dataGridView.Rows.Clear();
            ClearSculpt();
            for (int i = 0; i < sculptList.Count; i++)
            {
                string[] s = new string[2];
                s[0] = sculptList[i].SculptName;
                s[1] = ListMorphResources(sculptList[i]);
                Sculpt_dataGridView.Rows.Add(s);
            }
            currentSculpt = 0;
            if (sculptList.Count > 0)
            {
                ShowSculpt(sculptList[0]);
                EditSculpts_panel.Enabled = true;
            }
            else
            {
                EditSculpts_panel.Enabled = false;
            }
        }

        private string ListMorphResources(Sculpt sculpt)
        {
            string s = "";
            if (sculpt.BGEOKey != null && sculpt.BGEOKey.Length > 0 && sculpt.BGEOKey[0] != null && sculpt.BGEOKey[0].Instance != 0) s += "BGEO" + (sculpt.bgeo != null ? " (Custom) " : (IsInGamePacks(sculpt.BGEOKey[0]) ? " (EA) " : " (Not found) "));
            if (sculpt.dmapShapeRef != null && sculpt.dmapShapeRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "DMap Shape" + (sculpt.shape != null ? " (Custom) " : (IsInGamePacks(sculpt.dmapShapeRef) ? " (EA) " : " (Not found) "));
            if (sculpt.dmapNormalRef != null && sculpt.dmapNormalRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "DMap Normals" + (sculpt.normals != null ? " (Custom) " : (IsInGamePacks(sculpt.dmapNormalRef) ? " (EA) " : " (Not found) "));
            if (sculpt.boneDeltaRef != null && sculpt.boneDeltaRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "Bone Delta" + (sculpt.bond != null ? " (Custom) " : (IsInGamePacks(sculpt.boneDeltaRef) ? " (EA) " : " (Not found) "));
            if (sculpt.textureRef != null && sculpt.textureRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "Texture" + (sculpt.texture != null ? " (Custom) " : (IsInGamePacks(new TGI((uint)ResourceTypes.LRLE, sculpt.textureRef.Group, sculpt.textureRef.Instance)) ? " (EA) " : " (Not found) "));
            if (sculpt.bumpmapRef != null && sculpt.bumpmapRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "BumpMap" + (sculpt.bumpmap != null ? " (Custom) " : (IsInGamePacks(sculpt.bumpmapRef) ? " (EA) " : " (Not found) "));
            if (sculpt.specularRef != null && sculpt.specularRef.Instance != 0) s += (s.Length > 0 ? Environment.NewLine : "") + "Specular" + (sculpt.specular != null ? " (Custom) " : (IsInGamePacks(sculpt.specularRef) ? " (EA) " : " (Not found) "));
            return s;
        }

        private void Sculpt_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentSculpt)
            {
                SaveSculpt(sculptList[currentSculpt]);
                currentSculpt = e.RowIndex;
            }
            ShowSculpt(sculptList[e.RowIndex]);
            if (e.ColumnIndex == 2)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this Sculpt?", "Delete Sculpt", MessageBoxButtons.YesNo);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    sculptList.RemoveAt(e.RowIndex);
                    ListSculpt();
                }
            }
        }

        private void ShowSculpt(Sculpt sculpt)
        {
            EditSculpts_panel.Enabled = true;
            SculptMorphName.Text = sculpt.SculptName;
            ShowAgeGender(sculpt.ageGender, SculptAge_checkedListBox, SculptGender_checkedListBox);
            SculptRegion_comboBox.SelectedItem = sculpt.region.ToString();
            SculptSubregion_comboBox.SelectedItem = sculpt.subRegion.ToString();

            Show_MorphButtonStates(sculpt.bgeo != null, (sculpt.bgeo == null & sculpt.BGEOKey.Length > 0 ? IsInGamePacks(sculpt.BGEOKey[0]) : false), Sculpt_BGEO_button, Sculpt_BGEO_Delete_button, Sculpt_BGEO_Export_button);
            Show_MorphButtonStates(sculpt.shape != null, (sculpt.shape == null && sculpt.dmapShapeRef.Instance > 0 ? IsInGamePacks(sculpt.dmapShapeRef) : false), Sculpt_DMapShape_button, Sculpt_DMapShape_Delete_button, Sculpt_DMapShape_Export_button);
            Show_MorphButtonStates(sculpt.normals != null, (sculpt.normals == null && sculpt.dmapNormalRef.Instance > 0 ? IsInGamePacks(sculpt.dmapNormalRef) : false), Sculpt_DMapNormals_button, Sculpt_DMapNormals_Delete_button, Sculpt_DMapNormals_Export_button);
            Show_MorphButtonStates(sculpt.bond != null, (sculpt.bond == null && sculpt.boneDeltaRef.Instance > 0 ? IsInGamePacks(sculpt.boneDeltaRef) : false), Sculpt_BoneDelta_button, Sculpt_BoneDelta_Delete_button, Sculpt_BoneDelta_Export_button);
            Show_MorphButtonStates(sculpt.texture != null, (sculpt.texture == null && sculpt.textureRef.Instance > 0 ? IsInGamePacks(new TGI((uint)ResourceTypes.LRLE, sculpt.textureRef.Group, sculpt.textureRef.Instance)) : false), Sculpt_Texture_button, Sculpt_Texture_Delete_button, SculptTexture_Export_button);
            Show_MorphButtonStates(sculpt.bumpmap != null, (sculpt.bumpmap == null && sculpt.bumpmapRef.Instance > 0 ? IsInGamePacks(sculpt.bumpmapRef) : false), Sculpt_BumpMap_button, Sculpt_BumpMap_Delete_button, Sculpt_BumpMap_Export_button);
            Show_MorphButtonStates(sculpt.specular != null, (sculpt.specular == null && sculpt.specularRef.Instance > 0 ? IsInGamePacks(sculpt.specularRef) : false), Sculpt_Specular_button, Sculpt_Specular_Delete_button, Sculpt_Specular_Export_button);
        }

        private void ClearSculpt()
        {
            SculptMorphName.Text = "";
            ShowAgeGender(AgeGender.None, SculptAge_checkedListBox, SculptGender_checkedListBox);
            SculptRegion_comboBox.SelectedIndex = 0;
            SculptSubregion_comboBox.SelectedIndex = 0;

            Show_MorphButtonStates(false, false, Sculpt_BGEO_button, Sculpt_BGEO_Delete_button, Sculpt_BGEO_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_DMapShape_button, Sculpt_DMapShape_Delete_button, Sculpt_DMapShape_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_DMapNormals_button, Sculpt_DMapNormals_Delete_button, Sculpt_DMapNormals_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_BoneDelta_button, Sculpt_BoneDelta_Delete_button, Sculpt_BoneDelta_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_Texture_button, Sculpt_Texture_Delete_button, SculptTexture_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_BumpMap_button, Sculpt_BumpMap_Delete_button, Sculpt_BumpMap_Export_button);
            Show_MorphButtonStates(false, false, Sculpt_Specular_button, Sculpt_Specular_Delete_button, Sculpt_Specular_Export_button);
        }

        private void Sculpt_Clone_button_Click(object sender, EventArgs e)
        {
            ClonePickerForm pick = new ClonePickerForm(gamePackages, ResourceTypes.Sculpt, hotcDictEA, presetDictEA, smodDictEA, dmapDictEA);
            pick.ShowDialog();
            if (pick.DialogResult == DialogResult.OK)
            {
                Cursor curse = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                sculptList.AddRange(pick.clonedSculptList);
                ListSculpt();
                this.Cursor = curse;
                Application.DoEvents();
            }
        }

        private void Sculpt_Add_button_Click(object sender, EventArgs e)
        {
            if (sculptList.Count > 0) SaveSculpt(sculptList[currentSculpt]);
            Sculpt sculpt = new Sculpt();
            sculpt.SculptName = "Custom Sculpt 0x" + ran.Next().ToString();
            int ind = Sculpt_dataGridView.Rows.Add(new string[] { sculpt.SculptName, "" });
            sculptList.Add(sculpt);
            Sculpt_dataGridView.Rows[currentSculpt].Selected = false;
            Sculpt_dataGridView.Rows[ind].Selected = true;
            currentSculpt = ind;
            ShowSculpt(sculptList[currentSculpt]);
        }

        private void Sculpt_BGEO_button_Click(object sender, EventArgs e)
        {
            string bgeoFile = GetFilename("Select .bgeo/.blendgeom file", BGEOfilter);
            if (String.Compare(bgeoFile, " ") > 0)
            {
                BGEO bgeo = null;
                if (!GetBgeoData(bgeoFile, out bgeo))
                {
                    MessageBox.Show("Can't read BGEO file!");
                    return;
                }
                sculptList[currentSculpt].bgeo = bgeo;
                sculptList[currentSculpt].BGEOKey = new TGI[] { new TGI(bgeo.PublicKey) };
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_BGEO_button, Sculpt_BGEO_Delete_button, Sculpt_BGEO_Export_button);
            }
        }

        private void Sculpt_DMapShape_button_Click(object sender, EventArgs e)
        {
            string dmapFile = GetFilename("Select shape .dmap/.deformermap file", DMapfilter);
            if (String.Compare(dmapFile, " ") > 0)
            {
                DMap dmap = null;
                if (!ReadDMap(dmapFile, out dmap))
                {
                    MessageBox.Show("Can't read DMap Shape file!");
                    return;
                }
                if (dmap.ShapeOrNormal != ShapeOrNormals.SHAPE_DEFORMER)
                {
                    MessageBox.Show("You have imported a DMap Normals file, should be a Shape file!");
                    return;
                }
                sculptList[currentSculpt].shape = dmap;
                sculptList[currentSculpt].dmapShapeRef = new TGI((uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_Shape") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_DMapShape_button, Sculpt_DMapShape_Delete_button, Sculpt_DMapShape_Export_button);
            }
        }

        private void Sculpt_DMapNormals_button_Click(object sender, EventArgs e)
        {
            string dmapFile = GetFilename("Select normals .dmap/.deformermap file", DMapfilter);
            if (String.Compare(dmapFile, " ") > 0)
            {
                DMap dmap = null;
                if (!ReadDMap(dmapFile, out dmap))
                {
                    MessageBox.Show("Can't read DMap Shape file!");
                    return;
                }
                if (dmap.ShapeOrNormal != ShapeOrNormals.NORMALS_DEFORMER)
                {
                    MessageBox.Show("You have imported a DMap Shape file, should be a Normals file!");
                    return;
                }
                sculptList[currentSculpt].normals = dmap;
                sculptList[currentSculpt].dmapNormalRef = new TGI((uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_Normals") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_DMapNormals_button, Sculpt_DMapNormals_Delete_button, Sculpt_DMapNormals_Export_button);
            }
        }

        private void Sculpt_Texture_button_Click(object sender, EventArgs e)
        {
            string textureFile = GetFilename("Select texture file", Pngfilter);
            if (String.Compare(textureFile, " ") > 0)
            {
                LRLE lrle;
                if (!ReadTexture(textureFile, out lrle))
                {
                    MessageBox.Show("Can't read Texture file!");
                    return;
                }
                sculptList[currentSculpt].texture = lrle;
                sculptList[currentSculpt].textureRef = new TGI((uint)ResourceTypes.RLE2, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_Texture") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_Texture_button, Sculpt_Texture_Delete_button, SculptTexture_Export_button);
            }
        }

        private void Sculpt_BGEO_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].bgeo = null;
            sculptList[currentSculpt].BGEOKey = new TGI[0];
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_BGEO_button, Sculpt_BGEO_Delete_button);
        }

        private void Sculpt_DMapShape_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].shape = null;
            sculptList[currentSculpt].dmapShapeRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_DMapShape_button, Sculpt_DMapShape_Delete_button);
        }

        private void Sculpt_DMapNormals_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].normals = null;
            sculptList[currentSculpt].dmapNormalRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_DMapNormals_button, Sculpt_DMapNormals_Delete_button);
        }

        private void Sculpt_Texture_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].texture = null;
            sculptList[currentSculpt].textureRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_Texture_button, Sculpt_Texture_Delete_button, SculptTexture_Export_button);
       }

        private void SculptTexture_Export_button_Click(object sender, EventArgs e)
        {
            WriteImage("Save Sculpt Texture", sculptList[currentSculpt].texture != null ? sculptList[currentSculpt].texture : FetchGameLRLE(sculptList[currentSculpt].textureRef), sculptList[currentSculpt].SculptName + "_Texture");
        }

        private void Sculpt_BGEO_Export_button_Click(object sender, EventArgs e)
        {
            WriteBgeoFile("Save BGEO as a file", sculptList[currentSculpt].bgeo != null ? sculptList[currentSculpt].bgeo : FetchGameBGEO(sculptList[currentSculpt].BGEOKey[0]), sculptList[currentSculpt].SculptName + "_BGEO");
        }

        private void Sculpt_DMapShape_Export_button_Click(object sender, EventArgs e)
        {
            WriteDMap("Save Shape DMap as a file", sculptList[currentSculpt].shape != null ? sculptList[currentSculpt].shape : FetchGameDMap(sculptList[currentSculpt].dmapShapeRef), sculptList[currentSculpt].SculptName + "_DMapShape");
        }

        private void Sculpt_DMapNormals_Export_button_Click(object sender, EventArgs e)
        {
            WriteDMap("Save Normals DMap as a file", sculptList[currentSculpt].normals != null ? sculptList[currentSculpt].normals : FetchGameDMap(sculptList[currentSculpt].dmapNormalRef), sculptList[currentSculpt].SculptName + "_DMapNormals");
        }

        private void Sculpt_BumpMap_button_Click(object sender, EventArgs e)
        {
            string imageFile = GetFilename("Select Bumpmap/NormalMap file", DDSfilter);
            if (String.Compare(imageFile, " ") > 0)
            {
                DSTResource dst;
                if (!ReadDST(imageFile, out dst))
                {
                    MessageBox.Show("Can't read Bumpmap/NormalMap file!");
                    return;
                }
                sculptList[currentSculpt].bumpmap = dst;
                sculptList[currentSculpt].bumpmapRef = new TGI((uint)ResourceTypes.DDS, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_Bumpmap") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_BumpMap_button, Sculpt_BumpMap_Delete_button, Sculpt_BumpMap_Export_button);
            }
        }

        private void Sculpt_BumpMap_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].bumpmap = null;
            sculptList[currentSculpt].bumpmapRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_BumpMap_button, Sculpt_BumpMap_Delete_button, Sculpt_BumpMap_Export_button);
        }

        private void Sculpt_BumpMap_Export_button_Click(object sender, EventArgs e)
        {
            WriteDDS("Save Sculpt Bumpmap/Normal Map", sculptList[currentSculpt].bumpmap != null ? sculptList[currentSculpt].bumpmap : FetchGameDST(sculptList[currentSculpt].bumpmapRef), sculptList[currentSculpt].SculptName + "_BumpMap");
        }

        private void Sculpt_Specular_button_Click(object sender, EventArgs e)
        {
            string specFile = GetFilename("Select Specular file", DDSfilter);
            if (String.Compare(specFile, " ") > 0)
            {
                RLEResource rles = new RLEResource(1, null);
                if (!ReadSpecular(specFile, out rles))
                {
                    MessageBox.Show("Can't read Specular file!");
                    return;
                }
                sculptList[currentSculpt].specular = rles;
                sculptList[currentSculpt].specularRef = new TGI((uint)ResourceTypes.RLES, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_Specular") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_Specular_button, Sculpt_Specular_Delete_button, Sculpt_Specular_Export_button);
            }
        }

        private void Sculpt_Specular_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].specular = null;
            sculptList[currentSculpt].specularRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_Specular_button, Sculpt_Specular_Delete_button, Sculpt_Specular_Export_button);
        }

        private void Sculpt_Specular_Export_button_Click(object sender, EventArgs e)
        {
            RLEResource rles = sculptList[currentSculpt].specular != null ? sculptList[currentSculpt].specular : FetchGameRLES(sculptList[currentSculpt].specularRef);
            WriteDDS("Save Sculpt Specular Map", rles, sculptList[currentSculpt].SculptName + "_Specular");
        }


        private void Sculpt_BoneDelta_button_Click(object sender, EventArgs e)
        {
            string bondFile = GetFilename("Select BonePose/BoneDelta file", BONDfilter);
            if (String.Compare(bondFile, " ") > 0)
            {

                BOND bond = null;
                if (!GetBONDData(bondFile, out bond))
                {
                    MessageBox.Show("Can't read BoneDelta file!");
                    return;
                }
                sculptList[currentSculpt].bond = bond;
                sculptList[currentSculpt].boneDeltaRef = new TGI((uint)ResourceTypes.BoneDelta, 0, FNVhash.FNV64(sculptList[currentSculpt].SculptName + "_BOND") | 0x8000000000000000);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
                Show_MorphButtonStates(true, false, Sculpt_BoneDelta_button, Sculpt_BoneDelta_Delete_button, Sculpt_BoneDelta_Export_button);
            }
        }

        private void Sculpt_BoneDelta_Delete_button_Click(object sender, EventArgs e)
        {
            sculptList[currentSculpt].bond = null;
            sculptList[currentSculpt].boneDeltaRef = new TGI();
            Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            Show_MorphButtonStates(false, false, Sculpt_BoneDelta_button, Sculpt_BoneDelta_Delete_button, Sculpt_BoneDelta_Export_button);
        }

        private void Sculpt_BoneDelta_Export_button_Click(object sender, EventArgs e)
        {
            WriteBONDFile("Save BoneDelta/BOND file", sculptList[currentSculpt].bond != null ? sculptList[currentSculpt].bond : FetchGameBOND(sculptList[currentSculpt].boneDeltaRef), sculptList[currentSculpt].SculptName + "_BOND");
        }
        private void Sculpt_Save_button_Click(object sender, EventArgs e)
        {
            if (sculptList.Count > 0)
            {
                SaveSculpt(sculptList[currentSculpt]);
                Sculpt_dataGridView.Rows[currentSculpt].Cells[0].Value = sculptList[currentSculpt].SculptName;
                Sculpt_dataGridView.Rows[currentSculpt].Cells[1].Value = ListMorphResources(sculptList[currentSculpt]);
            }
        }

        private void SaveSculpt(Sculpt sculpt)
        {
            if (String.Compare(SculptMorphName.Text, " ") <= 0)
            {
                MessageBox.Show("You must enter a unique name for the sculpt!");
                return;
            }
            if ((sculpt.shape != null & sculpt.normals == null) | (sculpt.shape == null & sculpt.normals != null))
            {
                MessageBox.Show("If using DMaps, you must enter BOTH shape and normals!");
                return;
            }
            bool nameChange = String.Compare(sculpt.SculptName, SculptMorphName.Text) != 0;
            if (sculpt.publicKey[0].Instance == 0)
            {
                TGI tgi = new TGI((uint)ResourceTypes.Sculpt, 0u, FNVhash.FNV64(sculpt.SculptName) | 0x8000000000000000u);
                sculpt.publicKey[0] = tgi;
            }
            if (nameChange)
            {
                sculpt.SculptName = SculptMorphName.Text;
                ListPresets();
            }
            sculpt.ageGender = GetAgeGender(SculptAge_checkedListBox, SculptGender_checkedListBox);
            sculpt.region = (SimRegion)Enum.Parse(typeof(SimRegion), (string)SculptRegion_comboBox.SelectedItem);
            sculpt.subRegion = (SimSubRegion)Enum.Parse(typeof(SimSubRegion), (string)SculptSubregion_comboBox.SelectedItem);
            ShowTreeView();
        }

        private void Sculpt_Preview_button_Click(object sender, EventArgs e)
        {
            if (sculptList.Count == 0) return;
            Sculpt sculpt = sculptList[currentSculpt];
            if ((sculpt.shape != null & sculpt.normals == null) | (sculpt.shape == null & sculpt.normals != null))
            {
                MessageBox.Show("If using DMaps, you must enter BOTH shape and normals!");
                return;
            }
            BGEO[] bgeo = null;
            DMap[] shape = null;
            DMap[] normals = null;
            BOND[] bond = null;
            Image skinOverlay = null;
            if (sculpt.BGEOKey != null && sculpt.BGEOKey.Length > 0 && sculpt.BGEOKey[0] != null && sculpt.BGEOKey[0].Instance > 0ul)
                bgeo = new BGEO[] { sculpt.bgeo != null ? sculpt.bgeo : FetchGameBGEO(sculpt.BGEOKey[0]) };
            if (sculpt.dmapShapeRef.Instance > 0ul) shape = new DMap[] { sculpt.shape != null ? sculpt.shape : FetchGameDMap(sculpt.dmapShapeRef) };
            if (sculpt.dmapNormalRef.Instance > 0ul) normals = new DMap[] { sculpt.normals != null ? sculpt.normals : FetchGameDMap(sculpt.dmapNormalRef) };
            if (sculpt.boneDeltaRef.Instance > 0ul) bond = new BOND[] { sculpt.bond != null ? sculpt.bond : FetchGameBOND(sculpt.boneDeltaRef) };
            if (sculpt.textureRef.Instance > 0ul)
            {
                Image img = sculpt.texture != null ? sculpt.texture.image : FetchGameLRLE(sculpt.textureRef).image;
                if (img != null) skinOverlay = new Bitmap(img);
            }
            Form previewForm = new EditMorphPreview(GetPreviewSpecies(SculptSpecies_comboBox), sculpt.ageGender, AgeGender.None, sculpt.region, sculpt.subRegion, 
                                                    bgeo, shape, normals, bond, skinOverlay);
            previewForm.Text = sculpt.SculptName;
            previewForm.Show();
        }
    }
}
