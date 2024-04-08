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
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using s4pi.Interfaces;
using s4pi.Package;
using s4pi.ImageResource;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        int currentHOTC = 0;
        int currentSlider = 0;
        int currentPreset = 0;
        int currentPresetMod = 0;

        private void SetupControlsEditors()
        {
            EditHOTC_cursor_comboBox.Items.AddRange(Enum.GetNames(typeof(HOTC.SliderCursor)));
            EditHOTC_region_comboBox.Items.AddRange(Enum.GetNames(typeof(SimRegion)).Where (s => !s.Contains("CUSTOM_")).ToArray());
            EditHOTC_subregion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimSubRegion)));
            EditPreset_region_comboBox.Items.AddRange(Enum.GetNames(typeof(SimRegionPreset)));
            EditPreset_subregion_comboBox.Items.AddRange(Enum.GetNames(typeof(SimSubRegion)));
            EditPreset_CASPregion_comboBox.Items.AddRange(Enum.GetNames(typeof(BodyType)));
            DataGridViewDisableButtonColumn sculptButtons = new DataGridViewDisableButtonColumn();
            sculptButtons.Text = "Clone";
            sculptButtons.UseColumnTextForButtonValue = true;
            sculptButtons.Width = 70;
            EditPresetSculpt_dataGridView.Columns.Add(sculptButtons);
            DataGridViewDisableButtonColumn modButtons = new DataGridViewDisableButtonColumn();
            modButtons.Text = "Clone";
            modButtons.UseColumnTextForButtonValue = true;
            modButtons.Width = 70;
            EditPresetMod_dataGridView.Columns.Add(modButtons);
        }

        private void EditHOTC_Clone_button_Click(object sender, EventArgs e)
        {
            ClonePickerForm pick = new ClonePickerForm(gamePackages, ResourceTypes.HotSpotControl, hotcDictEA, presetDictEA, smodDictEA, dmapDictEA);
            pick.ShowDialog();
            if (pick.DialogResult == DialogResult.OK)
            {
                Cursor curse = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                hotcList.AddRange(pick.clonedHotcList);
                ListHOTC();
                this.Cursor = curse;
                Application.DoEvents();
            }
        }

        private void ListHOTC()
        {
            EditHOTC_dataGridView.Rows.Clear();
            ClearHOTC();
            for (int i = 0; i < hotcList.Count; i++)
            {
                string[] s = new string[] { hotcList[i].HotSpotName };
                EditHOTC_dataGridView.Rows.Add(s);
            }
            currentHOTC = 0;
            if (hotcList.Count > 0)
            {
                ShowHOTC(hotcList[0]);
                EditHOTC_panel.Enabled = true;
            }
            else
            {
                EditHOTC_panel.Enabled = false;
            }
        }

        private void EditHOTC_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentHOTC)
            {
                if (!SaveHOTC(hotcList[currentHOTC]))
                {
                    EditHOTC_dataGridView.Rows[e.RowIndex].Selected = false;
                    EditHOTC_dataGridView.Rows[currentHOTC].Selected = true;
                    return;
                }
                currentHOTC = e.RowIndex;
            }
            ShowHOTC(hotcList[e.RowIndex]);
            if (e.ColumnIndex == 1)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this HotSpotControl?", "Delete HotSpotControl", MessageBoxButtons.YesNo);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    hotcList.RemoveAt(e.RowIndex);
                    ListHOTC();
                }
            }
        }

        private void EditHOTC_Save_button_Click(object sender, EventArgs e)
        {
            SaveHOTC(hotcList[currentHOTC]);
        }

        private void ShowHOTC(HOTC hotc)
        {
            EditHOTC_name.Text = hotc.HotSpotName;
            EditHOTC_color.Text = hotc.ColorID.ToString();
            EditHOTC_Species.Text = hotc.Species.ToString();
            ShowAgeGender(hotc.AgeFrame, EditHOTC_age_checkedListBox, EditHOTC_frame_checkedListBox);
            EditHOTC_gender_checkedListBox.SetItemChecked(0, false);
            EditHOTC_gender_checkedListBox.SetItemChecked(1, false); 
            if (hotc.RestricttoGender == HOTC.RestrictToGender.Male) EditHOTC_gender_checkedListBox.SetItemChecked(0, true);
            else if (hotc.RestricttoGender == HOTC.RestrictToGender.Female) EditHOTC_gender_checkedListBox.SetItemChecked(1, true);
            EditHOTC_occult_checkedListBox.ClearSelected();
            uint tmp = (uint)hotc.Occult;
            for (int i = 0; i < EditHOTC_occult_checkedListBox.Items.Count; i++)
            {
                EditHOTC_occult_checkedListBox.SetItemChecked(i, (tmp & 1 << i) > 0);
            }
            EditHOTC_level.Text = hotc.Level.ToString();
            EditHOTC_cursor_comboBox.SelectedIndex = (byte)hotc.Cursor;
            EditHOTC_region_comboBox.SelectedItem = hotc.Region.ToString();
            EditHOTC_subregion_comboBox.SelectedItem = hotc.SubRegion.ToString();
            EditSliders_dataGridView.Rows.Clear();
            ClearSlider();
            for (int i = 0; i < hotc.sliderDescriptions.Count; i++)
            {
                int ind = EditSliders_dataGridView.Rows.Add(hotc.sliderDescriptions[i].Angle.ToString());
                EditSliders_dataGridView.Rows[ind].Tag = hotc.sliderDescriptions[i];
            }
            if (hotc.sliderDescriptions.Count > 0)
            {
                currentSlider = 0;
                ShowSlider(0);
            }
            else
            {
                currentSlider = -1;
            }
        }

        private void ClearHOTC()
        {
            EditHOTC_name.Text = "";
            EditHOTC_color.Text = "";
            ShowAgeGender(AgeGender.None, EditHOTC_age_checkedListBox, EditHOTC_frame_checkedListBox);
            EditHOTC_gender_checkedListBox.SetItemChecked(0, false);
            EditHOTC_gender_checkedListBox.SetItemChecked(1, false);
            EditHOTC_occult_checkedListBox.ClearSelected();
            EditHOTC_level.Text = "";
            EditHOTC_cursor_comboBox.SelectedIndex = 0;
            EditHOTC_region_comboBox.SelectedIndex = 0;
            EditHOTC_subregion_comboBox.SelectedIndex = 0;
            EditSliders_dataGridView.Rows.Clear();
            EditSliders_Left.Text = "";
            EditSliders_Right.Text = "";
            EditSliders_Up.Text = "";
            EditSliders_Down.Text = "";
            EditSlidersFlip_checkBox.Checked = false;
            for (int i = 0; i < EditSliders_View_checkedListBox.Items.Count; i++)
            {
                EditSliders_View_checkedListBox.SetItemChecked(i, false);
            }
        }

        private bool SaveHOTC(HOTC hotc)
        {
            if (String.Compare(hotc.HotSpotName, " ") <= 0)
            {
                MessageBox.Show("You must enter a unique name for the HotSpotControl!");
                return false;
            }
            SaveSlider(currentSlider);
            hotc.HotSpotName = EditHOTC_name.Text;
            hotc.AgeFrame = GetAgeGender(EditHOTC_age_checkedListBox, EditHOTC_frame_checkedListBox);
            if (EditHOTC_gender_checkedListBox.GetItemChecked(0)) hotc.RestricttoGender = HOTC.RestrictToGender.Male;
            else if (EditHOTC_gender_checkedListBox.GetItemChecked(1)) hotc.RestricttoGender = HOTC.RestrictToGender.Female;
            else hotc.RestricttoGender = HOTC.RestrictToGender.None;
            uint tmp = 0;
            for (int i = 0; i < EditHOTC_occult_checkedListBox.Items.Count; i++)
            {
                if (EditHOTC_occult_checkedListBox.GetItemChecked(i)) tmp |= 1u << i;
            }
            hotc.Occult = (OccultTypeFlags)tmp;
            hotc.Cursor = (HOTC.SliderCursor) EditHOTC_cursor_comboBox.SelectedIndex;
            hotc.Region = (SimRegion)Enum.Parse(typeof(SimRegion), EditHOTC_region_comboBox.SelectedItem.ToString());
            hotc.SubRegion = (SimSubRegion)Enum.Parse(typeof(SimSubRegion), EditHOTC_subregion_comboBox.SelectedItem.ToString());
            List<HOTC.Slider> sliders = new List<HOTC.Slider>();
            for (int i = 0; i < EditSliders_dataGridView.Rows.Count; i++)
            {
                sliders.Add((HOTC.Slider)EditSliders_dataGridView.Rows[i].Tag);
            }
            hotc.sliderDescriptions = sliders;
            EditHOTC_dataGridView.Rows[currentHOTC].Cells[0].Value = hotc.HotSpotName;
            ShowTreeView();
            return true;
        }

        private void EditSliders_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentSlider)
            {
                SaveSlider(currentSlider);
                currentSlider = e.RowIndex;
            }
            ShowSlider(e.RowIndex);
            if (e.ColumnIndex == 1)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this slider description?", "Delete Slider", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    EditSliders_dataGridView.Rows.RemoveAt(e.RowIndex);
                    if (EditSliders_dataGridView.Rows.Count > 0)
                    {
                        EditSliders_dataGridView.Rows[0].Selected = true;
                        currentSlider = 0;
                        ShowSlider(0);
                    }
                    else
                    {
                        currentSlider = -1;
                    }
                    return;
                }
            }
        }

        private void ShowSlider(int ind)
        {
            if (ind < 0) return;
            HOTC.Slider slider = EditSliders_dataGridView.Rows[ind].Tag as HOTC.Slider;
            for (int i = 0; i < EditSliders_View_checkedListBox.Items.Count; i++)
            {
                EditSliders_View_checkedListBox.SetItemChecked(i, ((byte)slider.Angle & (1 << i)) > 0);
            }
            bool canClone;
            EditSlidersFlip_checkBox.Checked = slider.Flip;
            EditSliders_Left.Text = GetSmodName(slider.SimModifierInstances[0], out canClone);
            EditSliders_Left.Tag = slider.SimModifierInstances[0];
            EditSlidersLeft_Clone_button.Enabled = canClone;
            EditSliders_Right.Text = GetSmodName(slider.SimModifierInstances[1], out canClone);
            EditSliders_Right.Tag = slider.SimModifierInstances[1];
            EditSlidersRight_Clone_button.Enabled = canClone;
            EditSliders_Up.Text = GetSmodName(slider.SimModifierInstances[2], out canClone);
            EditSliders_Up.Tag = slider.SimModifierInstances[2];
            EditSlidersUp_Clone_button.Enabled = canClone;
            EditSliders_Down.Text = GetSmodName(slider.SimModifierInstances[3], out canClone);
            EditSliders_Down.Tag = slider.SimModifierInstances[3];
            EditSlidersDown_Clone_button.Enabled = canClone;
        }

        private void EditSliders_Save_button_Click(object sender, EventArgs e)
        {
            if (EditSliders_dataGridView.Rows.Count > 0 && currentSlider > 0 && currentSlider < EditSliders_dataGridView.Rows.Count) SaveSlider(currentSlider);
        }

        private void SaveSlider(int ind)
        {
            if (ind < 0) return;
            HOTC.Slider slider = new HOTC.Slider();
            byte angle = 0;
            for (int i = 0; i < EditSliders_View_checkedListBox.Items.Count; i++)
            {
                if (EditSliders_View_checkedListBox.GetItemChecked(i)) angle += (byte)(1 << i);
            }
            slider.Angle = (HOTC.ViewAngle)angle;
            slider.Flip = EditSlidersFlip_checkBox.Checked;
            slider.SimModifierInstances[0] = (ulong)EditSliders_Left.Tag;
            slider.SimModifierInstances[1] = (ulong)EditSliders_Right.Tag;
            slider.SimModifierInstances[2] = (ulong)EditSliders_Up.Tag;
            slider.SimModifierInstances[3] = (ulong)EditSliders_Down.Tag;
            EditSliders_dataGridView.Rows[ind].Cells[0].Value = slider.Angle.ToString();
            EditSliders_dataGridView.Rows[ind].Tag = slider;
        }

        private void ClearSlider()
        {
            for (int i = 0; i < EditSliders_View_checkedListBox.Items.Count; i++)
            {
                EditSliders_View_checkedListBox.SetItemChecked(i, false);
            }
            EditSliders_Left.Text = "";
            EditSliders_Right.Text = "";
            EditSliders_Up.Text = "";
            EditSliders_Down.Text = "";
            EditSliders_Left.Tag = 0;
            EditSliders_Right.Tag = 0;
            EditSliders_Up.Tag = 0;
            EditSliders_Down.Tag = 0;
        }

        private void EditSliders_Left_button_Click(object sender, EventArgs e)
        {
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, true))
            {
                EditSliders_Left.Tag = instance;
                EditSliders_Left.Text = name;
                EditSlidersLeft_Clone_button.Enabled = isEA;
            }
        }

        private void EditSliders_Right_button_Click(object sender, EventArgs e)
        {
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, true))
            {
                EditSliders_Right.Tag = instance;
                EditSliders_Right.Text = name;
                EditSlidersRight_Clone_button.Enabled = isEA;
            }
        }

        private void EditSliders_Up_button_Click(object sender, EventArgs e)
        {
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, true))
            {
                EditSliders_Up.Tag = instance;
                EditSliders_Up.Text = name;
                EditSlidersUp_Clone_button.Enabled = isEA;
            }
        }

        private void EditSliders_Down_button_Click(object sender, EventArgs e)
        {
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, true))
            {
                EditSliders_Down.Tag = instance;
                EditSliders_Down.Text = name;
                EditSlidersDown_Clone_button.Enabled = isEA;
            }
        }

        private void EditSlidersLeft_Clone_button_Click(object sender, EventArgs e)
        {
            if (SMODClone(hotcList[currentHOTC].Species, EditSliders_Left)) EditSlidersLeft_Clone_button.Enabled = false;
        }

        private void EditSlidersRight_Clone_button_Click(object sender, EventArgs e)
        {
            if (SMODClone(hotcList[currentHOTC].Species, EditSliders_Right)) EditSlidersRight_Clone_button.Enabled = false;
        }

        private void EditSlidersUp_Clone_button_Click(object sender, EventArgs e)
        {
            if (SMODClone(hotcList[currentHOTC].Species, EditSliders_Up)) EditSlidersUp_Clone_button.Enabled = false;
        }

        private void EditSlidersDown_Clone_button_Click(object sender, EventArgs e)
        {
            if (SMODClone(hotcList[currentHOTC].Species, EditSliders_Down)) EditSlidersDown_Clone_button.Enabled = false;
        }

        private bool SMODClone(Species species, TextBox smodTextbox)
        {
            ulong instance = (ulong)smodTextbox.Tag;
            int ind = ListKeyLookUp(smodList, instance);
            SMOD smod = null;
            if (ind >= 0)
            {
                smod = new SMOD(smodList[ind]);
            }
            else
            {
                smod = FetchGameSMOD(new TGI((uint)ResourceTypes.SimModifier, 0U, instance));
            }
            if (smod != null)
            {
                bool isEA;
                smod.smodName = GetSmodName(smod.publicKey[0].Instance, out isEA).Replace(" (EA)", "") + "_Clone_" + ran.Next().ToString();
                smod.publicKey[0] = new TGI((uint)ResourceTypes.SimModifier, 0, FNVhash.FNV64(smod.smodName) | 0x8000000000000000);
                smod.isDefaultReplacement = false;
                smodTextbox.Tag = smod.publicKey[0].Instance;
                smodTextbox.Text = smod.smodName;
                smodList.Add(smod);
                ListSMOD();
                return true;
            }
            else
            {
                MessageBox.Show("SMOD not found!");
                return false;
            }
        }

        private void EditHOTC_addSlider_button_Click(object sender, EventArgs e)
        {
            if (EditSliders_dataGridView.Rows.Count > 0 && currentSlider < EditSliders_dataGridView.Rows.Count) SaveSlider(currentSlider);
            HOTC.Slider newSlide = new HOTC.Slider();
            int ind = EditSliders_dataGridView.Rows.Add("None");
            EditSliders_dataGridView.Rows[ind].Tag = newSlide;
            EditSliders_dataGridView.Rows[ind].Selected = false;
            EditSliders_dataGridView.Rows[ind].Selected = true;
            currentSlider = ind;
            ShowSlider(ind);
        }

        private void EditHOTC_gender_checkedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.NewValue == CheckState.Checked)
            {
                if (e.Index == 0) EditHOTC_gender_checkedListBox.SetItemChecked(1, false);
                else if (e.Index == 1) EditHOTC_gender_checkedListBox.SetItemChecked(0, false);
            }
        }

        private void EditHotcHelp_button_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The HotSpotControl sliders are used for the frame" + Environment.NewLine +
                            "selected, i.e. Male is used for both males and females" + Environment.NewLine +
                            "in male frame; and Female is used for both males and" + Environment.NewLine +
                            "females in female frame." + Environment.NewLine +
                            "If a Restrict to Gender option is checked, the slider" + Environment.NewLine +
                            "will be used only for the selected frame AND gender." + Environment.NewLine +
                            "For example, Female frame + Restricted to male gender" + Environment.NewLine +
                            "means the slider is used for males in female frame only." + Environment.NewLine +
                            "NOTE: 'Restrict to gender' appears not to work after" + Environment.NewLine +
                            "the Werewolves patch but this needs testing.");
        }


        private bool Modifier_Chooser(string modifierType, out ulong modifierInstance, out string modifierName, out bool isEA, bool showRemoveButton)
        {
            Modifier_Chooser_Form chooser;
            if (string.Compare(modifierType, "smod", true) == 0)
            {
                chooser = new Modifier_Chooser_Form(smodList, smodDictEA, showRemoveButton);
            }
         //   else if (string.Compare(modifierType, "sculpt", true) == 0)
            else
            {
                chooser = new Modifier_Chooser_Form(sculptList, sculptDictEA);
            }
            DialogResult res = chooser.ShowDialog();
            if (res == DialogResult.OK)
            {
                modifierInstance = chooser.ReturnInstance;
                modifierName = chooser.ReturnName;
                isEA = chooser.ReturnIsEA;
                return true;
            }
            else
            {
                modifierInstance = 0;
                modifierName = "";
                isEA = chooser.ReturnIsEA;
                return false;
            }
        }

        private void EditPreset_Add_button_Click(object sender, EventArgs e)
        {
            CPRE pre = new CPRE();
            pre.PresetName = "CustomPreset_" + ran.Next().ToString();
            pre.PresetTGI = new TGI((uint)ResourceTypes.CASPreset, 0, FNVhash.FNV64(pre.PresetName) | 0x8000000000000000);
            pre.isDefaultReplacement = false;
            int ind = presetList.Count;
            presetList.Add(pre);
            ListPresets();
        }

        private void EditPreset_Clone_button_Click(object sender, EventArgs e)
        {
            ClonePickerForm pick = new ClonePickerForm(gamePackages, ResourceTypes.CASPreset, hotcDictEA, presetDictEA, smodDictEA, dmapDictEA);
            pick.ShowDialog();
            if (pick.DialogResult == DialogResult.OK)
            {
                Cursor curse = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                Application.DoEvents();
                presetList.AddRange(pick.clonedPresetList);
                ListPresets();
                this.Cursor = curse;
                Application.DoEvents();
            }
        }

        private void ListPresets()
        {
            EditPreset_dataGridView.Rows.Clear();
            ClearPreset();
            for (int i = 0; i < presetList.Count; i++)
            {
                string[] s = new string[] { presetList[i].PresetName };
                EditPreset_dataGridView.Rows.Add(s);
            }
            currentPreset = 0;
            if (presetList.Count > 0)
            {
                ShowPreset(presetList[0]);
                EditPresets_panel.Enabled = true;
            }
            else
            {
                EditPresets_panel.Enabled = false;
            }
        }

        private void ShowPreset(CPRE preset)
        {
            EditPreset_name.Text = preset.PresetName;
            EditPreset_region_comboBox.SelectedItem = preset.region.ToString();
            EditPreset_subregion_comboBox.SelectedItem = preset.subRegion.ToString();
            EditPreset_Species_comboBox.SelectedIndex = (int)preset.species - 1;
            ShowAgeGender(preset.ageGender, EditPreset_age_checkedListBox, EditPreset_gender_checkedListBox);
            EditPreset_maleFrame_checkBox.Checked = ((preset.bodyFrameGender & AgeGender.Male) > 0);
            EditPreset_femaleFrame_checkBox.Checked = ((preset.bodyFrameGender & AgeGender.Female) > 0);
            ShowArchetype(preset.archetype, EditPreset_Archetype_checkedListBox);
            EditPreset_index.Text = preset.displayIndex.ToString("F", CultureInfo.CurrentCulture);
            EditPreset_random.Text = preset.chanceForRandom.ToString();
            ShowPresetTags(preset.tagList, EditPreset_tagsArchetype_checkedListBox, EditPreset_tagsOccult_checkedListBox);
            EditPreset_physique_checkBox.Checked = preset.isPhysiqueSet;
            ShowPhysiqueSet(preset);
            if (preset.isPartSet)
            {
                EditPreset_CASPid.Text = "0x" + preset.partsetInstance.ToString("X16");
                EditPreset_CASPregion_comboBox.SelectedItem = preset.partsetBodyType.ToString();
            }
            else
            {
                EditPreset_CASPid.Text = "";
                EditPreset_CASPregion_comboBox.SelectedItem = null;
            }
            EditPresetSculpt_dataGridView.Rows.Clear();
            for (int i = 0; i < preset.sculpts.Length; i++)
            {
                string sculptName = "";
                int ind = ListKeyLookUp(sculptList, preset.sculpts[i].instance);
                bool canClone;
                if (ind >= 0) { sculptName = sculptList[ind].SculptName; canClone = false; }
                else { sculptName = "0x" + preset.sculpts[i].instance.ToString("X16") + " (EA)"; canClone = true; }
                ind = EditPresetSculpt_dataGridView.Rows.Add(sculptName);
                EditPresetSculpt_dataGridView.Rows[ind].Tag = preset.sculpts[i];
                EditPresetSculpt_dataGridView.Rows[ind].Cells[2].Tag = canClone;
                DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell)EditPresetSculpt_dataGridView.Rows[ind].Cells[2];
                clone.Enabled = canClone;
            }
            EditPresetMod_dataGridView.Rows.Clear();
            EditPresetMod_SMOD.Text = "";
            for (int i = 0; i < preset.modifiers.Length; i++)
            {
                bool canClone;
                int ind = EditPresetMod_dataGridView.Rows.Add(GetSmodName(preset.modifiers[i].instance, out canClone));
                EditPresetMod_dataGridView.Rows[ind].Tag = preset.modifiers[i];
                EditPresetMod_dataGridView.Rows[ind].Cells[2].Tag = canClone;
                DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell)EditPresetMod_dataGridView.Rows[ind].Cells[2];
                clone.Enabled = canClone;
            }
            if (preset.modifiers.Length > 0)
            {
                currentPresetMod = 0;
                ShowPresetMod(0);
            }
        }

        private void ClearPreset()
        {
            EditPreset_name.Text = "";
            EditPreset_region_comboBox.SelectedIndex = 0;
            EditPreset_subregion_comboBox.SelectedIndex = 0;
            ShowAgeGender(AgeGender.None, EditPreset_age_checkedListBox, EditPreset_gender_checkedListBox);
            EditPreset_maleFrame_checkBox.Checked = false;
            EditPreset_femaleFrame_checkBox.Checked = false;
            ShowArchetype(ArchetypeFlags.None, EditPreset_Archetype_checkedListBox);
            EditPreset_index.Text = "";
            EditPreset_random.Text = "";
            ShowPresetTags(new CPRE.Tag[0], EditPreset_tagsArchetype_checkedListBox, EditPreset_tagsOccult_checkedListBox);
            EditPreset_physique_checkBox.Checked = false;
            EditPreset_CASPid.Text = "";
            EditPreset_CASPregion_comboBox.SelectedItem = null;
            EditPresetSculpt_dataGridView.Rows.Clear();
            EditPresetMod_dataGridView.Rows.Clear();
            EditPresetMod_SMOD.Text = "";
        }

        private void EditPreset_Save_button_Click(object sender, EventArgs e)
        {
            if (presetList.Count > 0) SavePreset(presetList[currentPreset]);
        }

        private bool SavePreset(CPRE preset)
        {
            if (String.Compare(preset.PresetName, " ") <= 0)
            {
                MessageBox.Show("You must enter a unique name for the Preset!");
                return false;
            }
            float newDisplayIndex, newRandom, newHeavy = 0, newFit = 0, newLean = 0, newBony = 0;
            if (!float.TryParse(EditPreset_index.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out newDisplayIndex))
            {
                if (!float.TryParse(EditPreset_index.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out newDisplayIndex))
                {
                    MessageBox.Show("You have not entered a valid Display Index value!");
                    return false;
                }
            }
            if (!float.TryParse(EditPreset_random.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out newRandom))
            {
                if (!float.TryParse(EditPreset_random.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out newRandom))
                {
                    MessageBox.Show("You have not entered a valid Chance for Random value!");
                    return false;
                }
            }
            if (EditPreset_physique_checkBox.Checked)
            {
                if (!float.TryParse(EditPreset_heavy.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out newHeavy))
                {
                    if (!float.TryParse(EditPreset_heavy.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out newHeavy))
                    {
                        MessageBox.Show("You have not entered a valid value for the Heavy weight!");
                        return false;
                    }
                }
                if (!float.TryParse(EditPreset_fit.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out newFit))
                {
                    if (!float.TryParse(EditPreset_fit.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out newFit))
                    {
                        MessageBox.Show("You have not entered a valid value for the Fit weight!");
                        return false;
                    }
                }
                if (!float.TryParse(EditPreset_lean.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out newLean))
                {
                    if (!float.TryParse(EditPreset_lean.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out newLean))
                    {
                        MessageBox.Show("You have not entered a valid value for the Lean weight!");
                        return false;
                    }
                }
                if (!float.TryParse(EditPreset_bony.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out newBony))
                {
                    if (!float.TryParse(EditPreset_bony.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out newBony))
                    {
                        MessageBox.Show("You have not entered a valid value for the Bony weight!");
                        return false;
                    }
                }
            }
            ulong newCASPid = 0;
            preset.isPartSet = string.Compare(EditPreset_CASPid.Text, " ") > 0;
            if (preset.isPartSet)
            {
                string casp = EditPreset_CASPid.Text.Replace("0x", "");
                if (!ulong.TryParse(casp, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out newCASPid))
                {
                    MessageBox.Show("You have not entered a valid hex identifier for CASP ID!");
                    return false;
                }
                if (EditPreset_CASPregion_comboBox.SelectedItem == null)
                {
                    MessageBox.Show("You have not entered a valid CASP Body Type!");
                    return false;
                }
            }
            if (EditPresetMod_dataGridView.Rows.Count > 0)
            {
                if (!SavePresetMod(currentPresetMod)) return false;
            }
            preset.PresetName = EditPreset_name.Text;
            preset.region = (SimRegionPreset)Enum.Parse(typeof(SimRegionPreset), EditPreset_region_comboBox.SelectedItem.ToString());
            preset.subRegion = (SimSubRegion)Enum.Parse(typeof(SimSubRegion), EditPreset_subregion_comboBox.SelectedItem.ToString());
            preset.species = (Species)(EditPreset_Species_comboBox.SelectedIndex + 1);
            preset.ageGender = GetAgeGender(EditPreset_age_checkedListBox, EditPreset_gender_checkedListBox);
            preset.bodyFrameGender = AgeGender.None;
            if (EditPreset_maleFrame_checkBox.Checked) preset.bodyFrameGender = preset.bodyFrameGender | AgeGender.Male;
            if (EditPreset_femaleFrame_checkBox.Checked) preset.bodyFrameGender = preset.bodyFrameGender | AgeGender.Female;
            preset.archetype = GetArchetype(EditPreset_Archetype_checkedListBox);
            preset.displayIndex = newDisplayIndex;
            preset.chanceForRandom = newRandom;
            preset.tagList = GetPresetTags(EditPreset_tagsArchetype_checkedListBox, EditPreset_tagsOccult_checkedListBox, preset.version);
            preset.isPhysiqueSet = EditPreset_physique_checkBox.Checked;
            preset.heavyValue = newHeavy;
            preset.fitValue = newFit;
            preset.leanValue = newLean;
            preset.bonyValue = newBony;
            if (preset.isPartSet)
            {
                preset.partsetInstance = newCASPid;
                preset.partsetBodyType = (BodyType)Enum.Parse(typeof(BodyType), EditPreset_CASPregion_comboBox.SelectedItem.ToString());
            }
            List<CPRE.SculptLink> sculpts = new List<CPRE.SculptLink>();
            for (int i = 0; i < EditPresetSculpt_dataGridView.Rows.Count; i++)
            {
                sculpts.Add((CPRE.SculptLink)EditPresetSculpt_dataGridView.Rows[i].Tag);
            }
            preset.sculpts = sculpts.ToArray();
            List<CPRE.Modifier> mods = new List<CPRE.Modifier>();
            for (int i = 0; i < EditPresetMod_dataGridView.Rows.Count; i++)
            {
                mods.Add((CPRE.Modifier)EditPresetMod_dataGridView.Rows[i].Tag);
            }
            preset.modifiers = mods.ToArray();
            EditPreset_dataGridView.Rows[currentPreset].Cells[0].Value = preset.PresetName;
            ShowTreeView();
            return true;
        }

        private void EditPreset_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.RowIndex != currentPreset)
            {
                if (!SavePreset(presetList[currentPreset]))
                {
                    EditPreset_dataGridView.Rows[e.RowIndex].Selected = false;
                    EditPreset_dataGridView.Rows[currentPreset].Selected = true;
                    return;
                }
                currentPreset = e.RowIndex;
            }
            ShowPreset(presetList[e.RowIndex]);
            if (e.ColumnIndex == 1)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this CAS Preset?", "Delete Preset", MessageBoxButtons.YesNo);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    presetList.RemoveAt(e.RowIndex);
                    ListPresets();
                }
            }
        }

        private void ShowPhysiqueSet(CPRE preset)
        {
            if (EditPreset_physique_checkBox.Checked)
            {
                EditPreset_Physique_panel.Enabled = true;
                EditPreset_heavy.Text = preset.heavyValue.ToString();
                EditPreset_lean.Text = preset.leanValue.ToString();
                EditPreset_fit.Text = preset.fitValue.ToString();
                EditPreset_bony.Text = preset.bonyValue.ToString();
            }
            else
            {
                EditPreset_Physique_panel.Enabled = false;
                EditPreset_heavy.Text = "";
                EditPreset_lean.Text = "";
                EditPreset_fit.Text = "";
                EditPreset_bony.Text = "";
            }
        }

        private void EditPresetSMOD_Add_button_Click(object sender, EventArgs e)
        {
            if (presetList.Count == 0) return;
            if (EditPresetMod_dataGridView.Rows.Count > 0)
            {
                if (!SavePresetMod(currentPresetMod)) return;
            }
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, false))
            {
                EditPresetMod_SMOD.Tag = instance;
                EditPresetMod_SMOD.Text = name;
            }
            else { return; }
            CPRE.Modifier newMod = new CPRE.Modifier(instance, 1f, 
                (SimRegion)Enum.Parse(typeof(SimRegion), EditPreset_region_comboBox.SelectedItem.ToString()), presetList[currentPreset].version);
            int ind = EditPresetMod_dataGridView.Rows.Add(name);
            EditPresetMod_dataGridView.Rows[ind].Tag = newMod;
            EditPresetMod_dataGridView.Rows[ind].Cells[2].Tag = isEA;
            DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell)EditPresetMod_dataGridView.Rows[ind].Cells[2];
            clone.Enabled = isEA;
            EditPresetMod_dataGridView.Rows[currentPresetMod].Selected = false;
            EditPresetMod_dataGridView.Rows[ind].Selected = true;
            currentPresetMod = ind;
            ShowPresetMod(ind);
        }

        private void EditPresetMod_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (e.RowIndex != currentPresetMod)
            {
                if (!SavePresetMod(currentPresetMod))
                {
                    EditPresetMod_dataGridView.Rows[currentPresetMod].Selected = true;
                    EditPresetMod_dataGridView.Rows[e.RowIndex].Selected = false;
                    return;
                }
                currentPresetMod = e.RowIndex;
            }
            ShowPresetMod(e.RowIndex);
            if (e.ColumnIndex == 1)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this preset modification?", "Delete Modification", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    EditPresetMod_dataGridView.Rows.RemoveAt(e.RowIndex);
                    if (EditPresetMod_dataGridView.Rows.Count > 0) 
                    {
                        EditPresetMod_dataGridView.Rows[0].Selected = true;
                        ShowPresetMod(0);
                    }
                    currentPresetMod = 0;
                    return;
                }
            }
            if (e.ColumnIndex == 2)
            {
                if (!(bool)EditPresetMod_dataGridView.Rows[e.RowIndex].Cells[2].Tag) return;
                if (SMODClone(presetList[currentPreset].species, EditPresetMod_SMOD))
                {
                    ulong instance = (ulong)EditPresetMod_SMOD.Tag;
                    CPRE.Modifier newMod = new CPRE.Modifier(instance, 1f,
                        (SimRegion)Enum.Parse(typeof(SimRegion), EditPreset_region_comboBox.SelectedItem.ToString()), presetList[currentPreset].version);
                    EditPresetMod_dataGridView.Rows[e.RowIndex].Tag = newMod;
                    EditPresetMod_dataGridView.Rows[e.RowIndex].Cells[0].Value = EditPresetMod_SMOD.Text;
                    EditPresetMod_dataGridView.Rows[e.RowIndex].Cells[2].Tag = false;
                    DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell) EditPresetMod_dataGridView.Rows[e.RowIndex].Cells[2];
                    clone.Enabled = false;
                }
            }
        }

        private void ShowPresetMod(int ind)
        {
            CPRE.Modifier mod = EditPresetMod_dataGridView.Rows[ind].Tag as CPRE.Modifier;
            EditPresetMod_Weight.Text = mod.weight.ToString();
            bool dummy;
            EditPresetMod_SMOD.Text = GetSmodName(mod.instance, out dummy);
            EditPresetMod_SMOD.Tag = mod.instance;
        }

        private void EditPresetMod_Save_button_Click(object sender, EventArgs e)
        {
            if (EditPresetMod_dataGridView.Rows.Count > 0) SavePresetMod(currentPresetMod);
        }
        
        private bool SavePresetMod(int ind)
        {
            float newWeight;
            if (!float.TryParse(EditPresetMod_Weight.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out newWeight))
            {
                if (!float.TryParse(EditPresetMod_Weight.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out newWeight))
                {
                    MessageBox.Show("You have not entered a valid weight!");
                    return false;
                }
            }
            CPRE.Modifier mod = new CPRE.Modifier((ulong)EditPresetMod_SMOD.Tag, newWeight, 
                (SimRegion)Enum.Parse(typeof(SimRegion), EditPreset_region_comboBox.SelectedItem.ToString()), presetList[currentPreset].version);
            EditPresetMod_dataGridView.Rows[ind].Cells[0].Value = EditPresetMod_SMOD.Text;
            EditPresetMod_dataGridView.Rows[ind].Tag = mod;
            return true;
        }

        private void EditPresetMod_SMOD_Click(object sender, EventArgs e)
        {
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("smod", out instance, out name, out isEA, false))
            {
                EditPresetMod_SMOD.Tag = instance;
                EditPresetMod_SMOD.Text = name;
                CPRE.Modifier mod = (CPRE.Modifier) EditPresetMod_dataGridView.Rows[currentPresetMod].Tag;
                mod.instance = instance;
                EditPresetMod_dataGridView.Rows[currentPresetMod].Cells[0].Value = name;
                EditPresetMod_dataGridView.Rows[currentPresetMod].Cells[2].Tag = isEA;
                DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell)EditPresetMod_dataGridView.Rows[currentPresetMod].Cells[2];
                clone.Enabled = isEA;
            }
        }

        private void EditPresetSculpt_Add_button_Click(object sender, EventArgs e)
        {
            if (presetList.Count == 0) return;
            ulong instance;
            string name;
            bool isEA;
            if (Modifier_Chooser("sculpt", out instance, out name, out isEA, false))
            {
                CPRE.SculptLink newSculpt = new CPRE.SculptLink(instance, 
                    (SimRegion)Enum.Parse(typeof(SimRegion), EditPreset_region_comboBox.SelectedItem.ToString()), presetList[currentPreset].version);
                int ind = EditPresetSculpt_dataGridView.Rows.Add(name);
                EditPresetSculpt_dataGridView.Rows[ind].Tag = newSculpt;
                EditPresetSculpt_dataGridView.Rows[ind].Cells[2].Tag = isEA;
                DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell)EditPresetSculpt_dataGridView.Rows[ind].Cells[2];
                clone.Enabled = isEA;

            }
        }

        private void EditPresetSculpt_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 | e.ColumnIndex < 0) return;
            if (e.ColumnIndex == 1)
            {
                DialogResult res = MessageBox.Show("Are you sure you want to delete this preset sculpt?", "Delete Sculpt", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {
                    EditPresetSculpt_dataGridView.Rows.RemoveAt(e.RowIndex);
                    if (EditPresetSculpt_dataGridView.Rows.Count > 0) EditPresetSculpt_dataGridView.Rows[0].Selected = true;
                }
            }
            if (e.ColumnIndex == 2)
            {
                if (!(bool)EditPresetSculpt_dataGridView.Rows[e.RowIndex].Cells[2].Tag) return;
                CPRE.SculptLink sculptLink = (CPRE.SculptLink)EditPresetSculpt_dataGridView.Rows[e.RowIndex].Tag;
                ulong instance = sculptLink.instance;
                int ind = ListKeyLookUp(sculptList, instance);
                Sculpt sculpt = null;
                if (ind >= 0)
                {
                    sculpt = new Sculpt(sculptList[ind]);
                }
                else
                {
                    sculpt = FetchGameSculpt(new TGI((uint)ResourceTypes.Sculpt, 0U, instance));
                }
                if (sculpt != null)
                {
                    sculpt.SculptName = "0x" + sculpt.publicKey[0].Instance.ToString("X") + "_Clone_" + ran.Next().ToString();
                    sculpt.publicKey[0] = new TGI((uint)ResourceTypes.Sculpt, 0, FNVhash.FNV64(sculpt.SculptName) | 0x8000000000000000);
                    sculpt.isDefaultReplacement = false;
                    sculptList.Add(sculpt);
                    sculptLink.instance = sculpt.publicKey[0].Instance;
                    EditPresetSculpt_dataGridView.Rows[e.RowIndex].Tag = sculptLink;
                    EditPresetSculpt_dataGridView.Rows[e.RowIndex].Cells[0].Value = sculpt.SculptName;
                    EditPresetSculpt_dataGridView.Rows[e.RowIndex].Cells[2].Tag = false;
                    DataGridViewDisableButtonCell clone = (DataGridViewDisableButtonCell) EditPresetSculpt_dataGridView.Rows[e.RowIndex].Cells[2];
                    clone.Enabled = false;
                    ListSculpt();
                }
                else
                {
                    MessageBox.Show("Sculpt not found!");
                }
            }
        }

        private void EditPreset_physique_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            EditPreset_Physique_panel.Enabled = EditPreset_physique_checkBox.Checked;
        }

        private void EditPreset_Preview_button_Click(object sender, EventArgs e)
        {
            if (presetList.Count == 0) return;
            Cursor curse = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            CPRE preset = presetList[currentPreset];
            List<BGEO> bgeo = new List<BGEO>();
            List<DMap> shape = new List<DMap>();
            List<DMap> normals = new List<DMap>();
            List<BOND> bond = new List<BOND>();
            Image skinOverlay = null;
            foreach (CPRE.SculptLink sculptLink in preset.sculpts)
            {
                int ind = ListKeyLookUp(sculptList, sculptLink.instance);
                Sculpt sculpt = null;
                if (ind >= 0)
                {
                    sculpt = sculptList[ind];
                }
                else 
                {
                    sculpt = FetchGameSculpt(new TGI((uint)ResourceTypes.Sculpt, 0U, sculptLink.instance));
                }
                if (sculpt != null)
                {
                    if (sculpt.BGEOKey != null && sculpt.BGEOKey.Length > 0 && sculpt.BGEOKey[0] != null && sculpt.BGEOKey[0].Instance > 0ul)
                        bgeo.Add(sculpt.bgeo != null ? sculpt.bgeo : FetchGameBGEO(sculpt.BGEOKey[0]));
                    if (sculpt.dmapShapeRef.Instance > 0ul) shape.Add(sculpt.shape != null ? sculpt.shape : FetchGameDMap(sculpt.dmapShapeRef));
                    if (sculpt.dmapNormalRef.Instance > 0ul) normals.Add(sculpt.normals != null ? sculpt.normals : FetchGameDMap(sculpt.dmapNormalRef));
                    if (sculpt.boneDeltaRef.Instance > 0ul) bond.Add(sculpt.bond != null ? sculpt.bond : FetchGameBOND(sculpt.boneDeltaRef));
                    if (sculpt.textureRef.Instance > 0ul) 
                    {
                        Image img = sculpt.texture != null ? sculpt.texture.image : FetchGameLRLE(sculpt.textureRef).image;
                        if (img != null) skinOverlay = new Bitmap(img);
                    }
                }
            }
            foreach (CPRE.Modifier mod in preset.modifiers)
            {
                int ind = ListKeyLookUp(smodList, mod.instance);
                SMOD smod = null;
                if (ind >= 0)
                {
                    smod = smodList[ind];
                }
                else
                {
                    smod = FetchGameSMOD(new TGI((uint)ResourceTypes.SimModifier, 0U, mod.instance));
                }
                if (smod != null)
                {
                    if (smod.BGEOKey != null && smod.BGEOKey.Length > 0 && smod.BGEOKey[0] != null && smod.BGEOKey[0].Instance > 0ul)
                    {
                        BGEO b = smod.bgeo != null ? smod.bgeo : FetchGameBGEO(smod.BGEOKey[0]);
                        b.weight = mod.weight;
                        bgeo.Add(b);
                    }
                    if (smod.deformerMapShapeKey.Instance > 0ul)
                    {
                        DMap dmap = smod.shape != null ? smod.shape : FetchGameDMap(smod.deformerMapShapeKey);
                        dmap.weight = mod.weight;
                        shape.Add(dmap);
                    }
                    if (smod.deformerMapNormalKey.Instance > 0ul)
                    {
                        DMap dmap = smod.normals != null ? smod.normals : FetchGameDMap(smod.deformerMapNormalKey);
                        dmap.weight = mod.weight;
                        normals.Add(dmap);
                    }
                    if (smod.bonePoseKey.Instance > 0ul)
                    {
                        BOND b = smod.bond != null ? smod.bond : FetchGameBOND(smod.bonePoseKey);
                        b.weight = mod.weight;
                        bond.Add(b);
                    }
                }
            }
            this.Cursor = curse;
            Application.DoEvents();
            Form previewForm = new EditMorphPreview(preset.species, preset.ageGender, preset.bodyFrameGender, (SimRegion)preset.region, preset.subRegion, 
                                                    bgeo.ToArray(), shape.ToArray(), normals.ToArray(), bond.ToArray(), skinOverlay);
            previewForm.Text = preset.PresetName;
            previewForm.Show();
            for (int i = 0; i < bgeo.Count; i++) { bgeo[i].weight = 1f; }       //reset weights just in case
            for (int i = 0; i < shape.Count; i++) { shape[i].weight = 1f; }
            for (int i = 0; i < normals.Count; i++) { normals[i].weight = 1f; }
            for (int i = 0; i < bond.Count; i++) { bond[i].weight = 1f; }
        }

        private void EditPresetThumb_button_Click(object sender, EventArgs e)
        {
            if (presetList.Count == 0) return;
            ThumbnailResource thumb;
            if (presetList[currentPreset].Thumb == null)
            {
                TGI tgi = new TGI((uint)ResourceTypes.Thumbnail, presetList[currentPreset].region == SimRegionPreset.BODY ? 3u : 2u, presetList[currentPreset].PresetTGI.Instance);
                thumb = FetchGameThumb(tgi);
            }
            else
            {
                thumb = presetList[currentPreset].Thumb;
            }
            ThumbnailManager tman = new ThumbnailManager(thumb, presetList[currentPreset].Thumb != null);
            DialogResult res = tman.ShowDialog();
            if (res == DialogResult.OK) presetList[currentPreset].Thumb = tman.Thumb;
        }
    }
}
