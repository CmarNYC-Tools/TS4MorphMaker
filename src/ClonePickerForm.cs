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
using System.IO;
using System.Drawing.Imaging;
using System.Reflection;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class ClonePickerForm : Form
    {
        Random ran = new Random(unchecked((int)DateTime.Now.Ticks));
        List<Package> gamePacks;
        ResourceTypes cloneMode;

        public List<HOTC> clonedHotcList = new List<HOTC>();
        public List<CPRE> clonedPresetList = new List<CPRE>();
        public List<Sculpt> clonedSculptList = new List<Sculpt>();
        public List<SMOD> clonedSmodList = new List<SMOD>();
        public bool getDefaultReplacementMorphs = false;

        public ClonePickerForm(List<Package> gamePackages, ResourceTypes resourceType, Dictionary<ulong, string> hotcDictEA,
                        Dictionary<ulong, string> presetDictEA, Dictionary<ulong, string> smodDictEA, Dictionary<ulong, string> dmapDictEA)
        {
            InitializeComponent();
            gamePacks = gamePackages;
            cloneMode = resourceType;
            switch (cloneMode)
            {
                case ResourceTypes.HotSpotControl:
                    CloneListHOTC(hotcDictEA, smodDictEA);
                    break;

                case ResourceTypes.CASPreset:
                    CloneListCPRE(presetDictEA, smodDictEA);
                    break;

                case ResourceTypes.Sculpt:
                    CloneListSculpt(smodDictEA, dmapDictEA);
                    break;

                case ResourceTypes.SimModifier:
                    CloneListSMOD(smodDictEA, dmapDictEA);
                    break;

                default:
                    MessageBox.Show("Unrecognized resource type!");
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                    break;
            }
        }

        private void CloneListHOTC(Dictionary<ulong, string> hotcDictEA, Dictionary<ulong, string> smodDictEA)
        {
            List<ulong> HotSpotInstances = new List<ulong>();
            string[] directions = new string[] { "Left: ", "Right: ", "Up: ", "Down: " };
            CloneDefault_radioButton.Checked = true;

            this.Width += 90;
            DataGridViewColumn level = new DataGridViewTextBoxColumn();
            level.Width = 90;
            level.HeaderText = "Level";
            CloneControl_dataGridView.Columns.Insert(2, level);

            CloneControl_dataGridView.Columns[5].HeaderText = "Restrict to Occult";

            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.HotSpotControl;
            TGI zero = new TGI();

            int sInd = 8;            //index of SMOD information column

            foreach (Package p in gamePacks)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (HotSpotInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        HOTC hotc;
                        try
                        {
                            hotc = new HOTC(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[sInd + 1];
                        str[0] = irie.ToString();
                        if (!hotcDictEA.TryGetValue(irie.Instance, out str[1])) str[1] = "";
                        str[2] = hotc.Level.ToString();
                        str[3] = hotc.Region.ToString().Replace("SIMREGION_", "");
                        str[4] = hotc.Species.ToString();
                        str[5] = hotc.Occult.ToString();
                        string[] ag = hotc.AgeFrame.ToString().Split();
                        foreach (string st in ag)
                        {
                            if (String.Compare(st, "Male") == 0 || String.Compare(st, "Female") == 0 || String.Compare(st, "Unisex") == 0)
                            {
                                str[7] = st;
                                continue;
                            }
                            str[6] += (String.Compare(st, "Toddler,") == 0) ? "P" : st.Substring(0, 1);
                        }
                        if (hotc.RestricttoGender != HOTC.RestrictToGender.None) str[6] += Environment.NewLine + hotc.RestricttoGender.ToString();
                        str[sInd] = "";
                        for (int sl = 0; sl < hotc.sliderDescriptions.Count; sl++)
                        {
                            HOTC.Slider slide = hotc.sliderDescriptions[sl];
                            str[sInd] += "View Angle: " + slide.Angle.ToString() + Environment.NewLine;
                            ulong[] smods = slide.SimModifierInstances;
                            for (int i = 0; i < smods.Length; i++)
                            {
                                if (smods[i] == 0) continue;
                                str[sInd] += directions[i];
                                string s;
                                if (smodDictEA.TryGetValue(smods[i], out s))
                                {
                                    str[sInd] += s;
                                }
                                else
                                {
                                    str[sInd] += "0x" + smods[i].ToString("X16");
                                }
                                str[sInd] += Environment.NewLine;
                            }
                            str[sInd] += Environment.NewLine;
                        }
                        int ind = CloneControl_dataGridView.Rows.Add(str);
                        CloneControl_dataGridView.Rows[ind].Tag = hotc;
                        CloneControl_dataGridView.Rows[ind].Cells[0].Tag = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                        HotSpotInstances.Add(irie.Instance);
                    }
                }
            }

            CloneControl_label.Text = "List of HotSpotControls:";
        }

        private void CloneListCPRE(Dictionary<ulong, string> presetDictEA, Dictionary<ulong, string> smodDictEA)
        {
            List<ulong> presetInstances = new List<ulong>();
            CloneCustom_radioButton.Enabled = true;

            CloneControl_dataGridView.Rows.Clear();
            CloneControl_dataGridView.Columns[1].Width = 100;
            //CloneControl_dataGridView.Columns[3].HeaderText = "Species/Occult";
            //CloneControl_dataGridView.Columns[3].Width = 150;
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.CASPreset;
            TGI zero = new TGI();

            foreach (Package p in gamePacks)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (presetInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        CPRE preset;
                        try
                        {
                            preset = new CPRE(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[8];
                        str[0] = irie.ToString();
                        if (!presetDictEA.TryGetValue(irie.Instance, out str[1])) str[1] = "";
                        str[2] = preset.region.ToString().Replace("SIMREGION_", "");
                        str[3] = preset.species.ToString();
                        str[4] = ListOccultTags(preset);
                        string[] ag = preset.ageGender.ToString().Split();
                        foreach (string st in ag)
                        {
                            str[5] += (String.Compare(st, "Toddler,") == 0) ? "P" : st.Substring(0, 1);
                        }
                        if (preset.bodyFrameGender != AgeGender.None) str[6] = preset.bodyFrameGender.ToString();
                        str[7] = "";
                        foreach (CPRE.SculptLink sculpt in preset.sculpts)
                        {
                            str[7] += "Sculpt: " + "0x" + sculpt.instance.ToString("X16") + Environment.NewLine;
                        }
                        foreach (CPRE.Modifier mod in preset.modifiers)
                        {
                            string s;
                            str[7] += "SimModifier: " + (smodDictEA.TryGetValue(mod.instance, out s) ? s : "0x" + mod.instance.ToString("X16")) + Environment.NewLine;
                        }
                        int ind = CloneControl_dataGridView.Rows.Add(str);
                        CloneControl_dataGridView.Rows[ind].Tag = preset;
                        CloneControl_dataGridView.Rows[ind].Cells[0].Tag = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                        presetInstances.Add(irie.Instance);
                    }
                }
            }

            CloneControl_label.Text = "List of CAS Presets:";
        }

        private static string ListOccultTags(CPRE preset)
        {
            string s = "";
            foreach (CPRE.Tag f in preset.tagList)
            {
                if (f.tagValue == (uint)PresetValueTags.Human || f.tagValue == (uint)PresetValueTags.Alien ||
                    f.tagValue == (uint)PresetValueTags.Vampire || f.tagValue == (uint)PresetValueTags.Mermaid ||
                    f.tagValue == (uint)PresetValueTags.Witch || f.tagValue == (uint)PresetValueTags.Werewolf ||
                    f.tagValue == (uint)PresetValueTags.Fairy)
                    s += ((PresetValueTags)f.tagValue).ToString() + ", ";
            }
            try
            {
                return s.Substring(0, s.LastIndexOf(","));
            }
            catch
            {
                return s;
            }
        }

        private void CloneListSculpt(Dictionary<ulong, string> smodDictEA, Dictionary<ulong, string> dmapDictEA)
        {
            List<ulong> sculptInstances = new List<ulong>();
            CloneCustom_radioButton.Enabled = true;
            CloneControl_dataGridView.Columns.RemoveAt(6);
            CloneControl_dataGridView.Columns.RemoveAt(4);
            CloneControl_dataGridView.Columns.RemoveAt(3);
            CloneControl_dataGridView.Columns.RemoveAt(1);

            CloneControl_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.Sculpt;
            TGI zero = new TGI();

            foreach (Package p in gamePacks)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (sculptInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        Sculpt sculpt;
                        try
                        {
                            sculpt = new Sculpt(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[5];
                        str[0] = irie.ToString();
                        str[1] = sculpt.region.ToString().Replace("SIMREGION_", "");
                        string[] ag = sculpt.ageGender.ToString().Split();
                        foreach (string st in ag)
                        {
                            str[2] += (String.Compare(st, "Toddler,") == 0) ? "P" : st.Substring(0, 1);
                        }
                        str[3] = "";
                        for (int i = 0; i < sculpt.BGEOKey.Length; i++)
                        {
                            string s;
                            str[3] += "BGEO: " + (smodDictEA.TryGetValue(irie.Instance, out s) ? " : " + s : sculpt.BGEOKey[i].ToString()) + Environment.NewLine;
                        }
                        if (!sculpt.dmapShapeRef.Equals(zero) || !sculpt.dmapNormalRef.Equals(zero))
                        {
                            string s;
                            str[3] += "DMAP: " + (dmapDictEA.TryGetValue(sculpt.dmapShapeRef.Instance, out s) ? s : sculpt.dmapShapeRef.ToString() + " (Shape)") + Environment.NewLine;
                            str[3] += "DMAP: " + (dmapDictEA.TryGetValue(sculpt.dmapNormalRef.Instance, out s) ? s : sculpt.dmapNormalRef.ToString() + " (Normals)") + Environment.NewLine;
                        }
                        if (sculpt.textureRef.Instance != 0)
                        {
                            str[3] += "Texture: " + sculpt.textureRef.ToString() + Environment.NewLine;
                        }
                        if (sculpt.bumpmapRef.Instance != 0)
                        {
                            str[3] += "Image: " + sculpt.bumpmapRef.ToString() + Environment.NewLine;
                        }
                        if (sculpt.specularRef.Instance != 0)
                        {
                            str[3] += "Specular: " + sculpt.specularRef.ToString() + Environment.NewLine;
                        }
                        if (sculpt.boneDeltaRef.Instance != 0)
                        {
                            str[3] += "BoneDelta: " + sculpt.boneDeltaRef.ToString() + Environment.NewLine;
                        }
                        int ind = CloneControl_dataGridView.Rows.Add(str);
                        CloneControl_dataGridView.Rows[ind].Tag = sculpt;
                        CloneControl_dataGridView.Rows[ind].Cells[0].Tag = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                        sculptInstances.Add(irie.Instance);
                    }
                }
            }

            CloneControl_label.Text = "List of Sculpts:";
        }

        private void CloneListSMOD(Dictionary<ulong, string> smodDictEA, Dictionary<ulong, string> dmapDictEA)
        {
            List<ulong> smodInstances = new List<ulong>();
            CloneCustom_radioButton.Enabled = true;
            CloneControl_dataGridView.Columns.RemoveAt(6);
            CloneControl_dataGridView.Columns.RemoveAt(4);
            CloneControl_dataGridView.Columns.RemoveAt(3);

            CloneControl_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.SimModifier;
            TGI zero = new TGI();

            foreach (Package p in gamePacks)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (smodInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        SMOD smod;
                        try
                        {
                            smod = new SMOD(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[6];
                        str[0] = irie.ToString();
                        if (!smodDictEA.TryGetValue(irie.Instance, out str[1])) str[1] = "";
                        str[2] = smod.region.ToString().Replace("SIMREGION_", "") + (smod.subRegion != SimSubRegion.None ? "/" + smod.subRegion.ToString() : "");
                        string[] ag = smod.ageGender.ToString().Split();
                        foreach (string st in ag)
                        {
                            str[3] += (String.Compare(st, "Toddler,") == 0) ? "P" : st.Substring(0, 1);
                        }
                        str[4] = "";
                        for (int i = 0; i < smod.BGEOKey.Length; i++)
                        {
                            string s;
                            str[4] += "BGEO: " + (smodDictEA.TryGetValue(irie.Instance, out s) ? " : " + s : smod.BGEOKey[i].ToString()) + Environment.NewLine;
                        }
                        if (!smod.deformerMapShapeKey.Equals(zero) || !smod.deformerMapNormalKey.Equals(zero))
                        {
                            string s;
                            str[4] += "DMAP: " + (dmapDictEA.TryGetValue(smod.deformerMapShapeKey.Instance, out s) ? s : smod.deformerMapShapeKey.ToString() + " (Shape)") + Environment.NewLine;
                            str[4] += "DMAP: " + (dmapDictEA.TryGetValue(smod.deformerMapNormalKey.Instance, out s) ? s : smod.deformerMapNormalKey.ToString() + " (Normals)") + Environment.NewLine;
                        }
                        if (!smod.bonePoseKey.Equals(zero))
                        {
                            str[4] += "BOND: " + smod.bonePoseKey.ToString() + Environment.NewLine;
                        }
                        int ind = CloneControl_dataGridView.Rows.Add(str);
                        CloneControl_dataGridView.Rows[ind].Tag = smod;
                        CloneControl_dataGridView.Rows[ind].Cells[0].Tag = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                        smodInstances.Add(irie.Instance);
                    }
                }
            }

            CloneControl_label.Text = "List of Sim Modifiers:";
        }

        private void CloneControl_Go_button_Click(object sender, EventArgs e)
        {
            bool found = false;
            string name;
            string[] st;
            foreach (DataGridViewRow row in CloneControl_dataGridView.Rows)
            {
                if (Convert.ToBoolean(row.Cells["ControlCloneSelect"].Value) == false) continue;
                switch (cloneMode)
                {
                    case ResourceTypes.HotSpotControl:
                        HOTC hotc = row.Tag as HOTC;
                        name = row.Cells[0].Value.ToString().Replace("HotspotControl", null);
                        if (name.StartsWith("0x")) name = row.Cells[1].Value.ToString();
                        if (name.Length > 0) name += "_";
                        st = row.Cells[4].Value.ToString().Split(Environment.NewLine.ToCharArray());
                        name += (String.Compare(row.Cells[2].Value.ToString(), "Normal") == 0 ? "" : row.Cells[2].Value.ToString() + "_") + 
                            row.Cells[3].Value.ToString() + "_" + st[0] + 
                            row.Cells[5].Value.ToString() + "Frame" + (st.Length > 2 ? st[2] + "Only" : "");
                        if (CloneCustom_radioButton.Checked)
                        {
                            hotc.HotSpotName = name + "_Custom_" + ran.Next().ToString();
                            hotc.HotSpotTGI = new TGI((uint)ResourceTypes.HotSpotControl, 0, FNVhash.FNV64(hotc.HotSpotName) | 0x8000000000000000);
                            hotc.isDefaultReplacement = false;
                        }
                        else
                        {
                            hotc.HotSpotTGI = new TGI(row.Cells[0].Tag as TGI);
                            hotc.HotSpotName = name + "_DefaultReplacement_" + "0x" + hotc.HotSpotTGI.Instance.ToString("X16");
                            hotc.isDefaultReplacement = true;
                        }
                        clonedHotcList.Add(hotc);
                        break;

                    case ResourceTypes.CASPreset:
                        CPRE pre = row.Tag as CPRE;
                        name = row.Cells[0].Value.ToString();
                        if (name.StartsWith("0x")) name = row.Cells[1].Value.ToString();
                        if (name.Length > 0) name += "_";
                        name += row.Cells[2].Value.ToString() + "_" + row.Cells[3].Value.ToString() +
                            row.Cells[4].Value.ToString() + "Frame";
                        if (CloneCustom_radioButton.Checked)
                        {
                            pre.PresetName = name + "_Custom_" + ran.Next().ToString();
                            pre.PresetTGI = new TGI((uint)ResourceTypes.CASPreset, 0, FNVhash.FNV64(pre.PresetName) | 0x8000000000000000);
                            pre.isDefaultReplacement = false;
                        }
                        else
                        {
                            pre.PresetTGI = new TGI(row.Cells[0].Tag as TGI);
                            pre.PresetName = name + "_DefaultReplacement_" + "0x" + pre.PresetTGI.Instance.ToString("X16");
                            pre.isDefaultReplacement = true;
                        }
                        clonedPresetList.Add(pre);
                        break;

                    case ResourceTypes.Sculpt:
                        Sculpt sculpt = row.Tag as Sculpt;
                        name = row.Cells[1].Value.ToString() + "_" + row.Cells[2].Value.ToString();
                        if (CloneCustom_radioButton.Checked)
                        {
                            sculpt.SculptName = name + "_Custom_" + ran.Next().ToString();
                            sculpt.publicKey[0] = new TGI((uint)ResourceTypes.Sculpt, 0, FNVhash.FNV64(sculpt.SculptName) | 0x8000000000000000);
                            sculpt.isDefaultReplacement = false;
                        }
                        else
                        {
                            sculpt.publicKey[0] = new TGI(row.Cells[0].Tag as TGI);
                            sculpt.SculptName = name + "_DefaultReplacement_" + "0x" + sculpt.publicKey[0].Instance.ToString("X16");
                            sculpt.isDefaultReplacement = true;
                        }
                        clonedSculptList.Add(sculpt);
                        break;

                    case ResourceTypes.SimModifier:
                        SMOD smod = row.Tag as SMOD;
                        name = row.Cells[0].Value.ToString();

                        if (name.StartsWith("0x")) name = row.Cells[1].Value.ToString();
                        name += "_" + row.Cells[2].Value.ToString();
                        if (CloneCustom_radioButton.Checked)
                        {
                            smod.smodName = name + "_Custom_" + ran.Next().ToString();
                            smod.publicKey[0] = new TGI((uint)ResourceTypes.SimModifier, 0, FNVhash.FNV64(smod.smodName) | 0x8000000000000000);
                            smod.isDefaultReplacement = false;
                        }
                        else
                        {
                            smod.publicKey[0] = new TGI(row.Cells[0].Tag as TGI);
                            smod.smodName = name + "_DefaultReplacement_" + "0x" + smod.publicKey[0].Instance.ToString("X16");
                            smod.isDefaultReplacement = true;
                        }
                        clonedSmodList.Add(smod);
                        break;

                }
                found = true;
            }
            getDefaultReplacementMorphs = CloneDefaultAll_radioButton.Checked;
            if (!found)
            {
                MessageBox.Show("No controls selected!");
                this.DialogResult = DialogResult.Cancel;
                this.Close();
                return;
            }
            MessageBox.Show("Done! Cloned controls are now visible in the Edit tabs.");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
