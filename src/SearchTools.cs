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
using System.Globalization;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        private void HotSpotGo_button_Click(object sender, EventArgs e)
        {
            List<ulong> HotSpotInstances = new List<ulong>();
            string[] directions = new string[] { "Left: ", "Right: ", "Up: ", "Down: " };

            HOTCsearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.HotSpotControl;
            TGI zero = new TGI();

            int sInd = 5;            //index of SMOD information column

            foreach (Package p in gamePackages)
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
                        string[] str = new string[6];
                        str[0] = irie.ToString();
                        string sh;
                        if (hotcDictEA.TryGetValue(irie.Instance, out sh)) str[0] += Environment.NewLine + sh;
                        string[] ag = hotc.AgeFrame.ToString().Split();
                        foreach (string st in ag)
                        {
                            str[1] += (String.Compare(st, "Toddler,") == 0) ? "P" : st.Substring(0, 1);
                            if (String.Compare(st, "Male") == 0 || String.Compare(st, "Female") == 0 || String.Compare(st, "Unisex") == 0)
                            {
                                str[1] += Environment.NewLine;
                                if (hotc.RestricttoGender != HOTC.RestrictToGender.None) str[1] += hotc.RestricttoGender.ToString() + "/";
                                str[1] += st + " Frame";
                            }
                        }
                        str[2] = hotc.Level.ToString();
                        str[3] = hotc.ColorID.ToString("X2");
                        str[4] = hotc.Region.ToString().Replace("SIMREGION_", "");

                        str[sInd] = "";
                        int numValid = 0;
                        bool listIt = SearchHOTC_Empty_checkBox.Checked;
                        List<HOTC.Slider> slides = hotc.sliderDescriptions;
                        foreach (HOTC.Slider slide in hotc.sliderDescriptions)
                        {
                            if (SearchHOTC_SMODs_checkBox.Checked) str[sInd] += "View Angle: " + slide.Angle.ToString() + " : Flip Directions?: " + slide.Flip.ToString()
                                + Environment.NewLine;
                            ulong[] smods = slide.SimModifierInstances;
                            for (int i = 0; i < smods.Length; i++)
                            {
                                if (smods[i] > 0) listIt = true;
                                if (SearchHOTC_SMODs_checkBox.Checked)
                                {
                                    string smod = "0x" + smods[i].ToString("X16");
                                    str[sInd] += directions[i] + smod;
                                    string s;
                                    if (smodDictEA.TryGetValue(smods[i], out s)) str[sInd] += String.Compare(smod, s) != 0 ? " : " + s : "";
                                    str[sInd] += Environment.NewLine;
                                }
                                else
                                {
                                    if (smods[i] > 0) numValid++;
                                }
                            }
                        }
                        if (listIt & ((HotSpotSearchHuman_radioButton.Checked && hotc.Species == Species.Human) 
                                   || (HotSpotSearchCD_radioButton.Checked && hotc.Species != Species.Human)))
                        {
                            if (!SearchHOTC_SMODs_checkBox.Checked) str[sInd] = numValid.ToString() + " valid SMODs";
                            int ind = HOTCsearch_dataGridView.Rows.Add(str);
                            HOTCsearch_dataGridView.Rows[ind].Tag = hotc;
                        }
                        HotSpotInstances.Add(irie.Instance);
                    }
                }
            }
        }
        
        private void SMODGo_button_Click(object sender, EventArgs e)
        {
            List<ulong> modifierInstances = new List<ulong>();

            SMODsearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.SimModifier;
            TGI zero = new TGI();

            foreach (Package p in gamePackages)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (modifierInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        SMOD cntx;
                        try
                        {
                            cntx = new SMOD(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[4];
                        str[0] = irie.Instance.ToString("X16");
                        str[1] = cntx.ageGender.ToString();
                        smodDictEA.TryGetValue(irie.Instance, out str[2]);
                        str[3] = "";
                        for (int i = 0; i < cntx.BGEOKey.Length; i++)
                        {
                            str[3] += "BGEO: " + cntx.BGEOKey[i].ToString() + Environment.NewLine;
                        }
                        if (!cntx.bonePoseKey.Equals(zero))
                        {
                            str[3] += "BONE: " + cntx.bonePoseKey.ToString() + Environment.NewLine;
                        }
                        if (!cntx.deformerMapShapeKey.Equals(zero) || !cntx.deformerMapNormalKey.Equals(zero))
                        {
                            str[3] += "DMAP: " + cntx.deformerMapShapeKey.ToString();
                            string s;
                            if (dmapDictEA.TryGetValue(cntx.deformerMapShapeKey.Instance, out s))
                            {
                                str[3] += " : " + s + Environment.NewLine;
                            }
                            else
                            {
                                str[3] += " (Shape)" + Environment.NewLine;
                            }
                            str[2] += "DMAP: " + cntx.deformerMapNormalKey.ToString();
                            if (dmapDictEA.TryGetValue(cntx.deformerMapNormalKey.Instance, out s))
                            {
                                str[3] += " : " + s + Environment.NewLine;
                            }
                            else
                            {
                                str[3] += " (Normals)" + Environment.NewLine;
                            }
                        }
                        int ind = SMODsearch_dataGridView.Rows.Add(str);
                        SMODsearch_dataGridView.Rows[ind].Tag = cntx;
                        modifierInstances.Add(irie.Instance);
                    }
                }
            }
        }

        private void SculptSearch_Go_button_Click(object sender, EventArgs e)
        {
            List<ulong> sculptInstances = new List<ulong>();

            SculptSearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.Sculpt;
            TGI zero = new TGI();

            foreach (Package p in gamePackages)
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
                        string[] str = new string[4];
                        str[0] = irie.ToString();
                        str[1] = sculpt.ageGender.ToString();
                        str[2] = sculpt.region.ToString();
                        str[3] = "";
                        for (int i = 0; i < sculpt.BGEOKey.Length; i++)
                        {
                            string s;
                            str[3] += "BGEO: " + sculpt.BGEOKey[i].ToString() + (smodDictEA.TryGetValue(irie.Instance, out s) ? " : " + s : "") + Environment.NewLine;
                        }
                        if (!sculpt.dmapShapeRef.Equals(zero) || !sculpt.dmapNormalRef.Equals(zero))
                        {
                            str[3] += "DMAP: " + sculpt.dmapShapeRef.ToString();
                            string s;
                            if (dmapDictEA.TryGetValue(sculpt.dmapShapeRef.Instance, out s))
                            {
                                str[3] += " : " + s + Environment.NewLine;
                            }
                            else
                            {
                                str[3] += " (Shape)" + Environment.NewLine;
                            }
                            str[3] += "DMAP: " + sculpt.dmapNormalRef.ToString();
                            if (dmapDictEA.TryGetValue(sculpt.dmapNormalRef.Instance, out s))
                            {
                                str[3] += " : " + s + Environment.NewLine;
                            }
                            else
                            {
                                str[3] += " (Normals)" + Environment.NewLine;
                            }
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
                        int ind = SculptSearch_dataGridView.Rows.Add(str);
                        SculptSearch_dataGridView.Rows[ind].Tag = sculpt;
                        sculptInstances.Add(irie.Instance);
                    }
                }
            }
        }


        /// <summary>
        /// Reads text list of TGI and name, ex: c5f6763e:00000000:8088d1e4b2013f98   yfheadNose_TipWide
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="morphTGIs"></param>
        /// <param name="morphNames"></param>
        private void ParseTextResourcesList(string filename, out List<TGI> tgiList, out List<string> nameList)
        {
            string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string resourcePath = Path.Combine(executingPath, filename);
            if (!File.Exists(resourcePath))
            {
                MessageBox.Show(string.Format("'{0}' not found in CAS Tools directory '{1}'; TGI and resource name list cannot be loaded.", filename, executingPath));
                tgiList = null;
                nameList = null;
                return;
            }
            string line = "";
            System.IO.StreamReader file = new System.IO.StreamReader(resourcePath);
            List<TGI> tgiTmp = new List<TGI>();
            List<string> nameTmp = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                string[] s = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                tgiTmp.Add(new TGI(s[0]));
                nameTmp.Add(s[1]);
            }
            file.Close();

            uint type = tgiTmp[0].Type;
            List<string> tmpNames = new List<string>();
            List<TGI> tmpTGI = new List<TGI>();
            foreach (string s in nameTmp)
            {
                if (s.StartsWith("yf") || s.StartsWith("yu"))
                {
                    string cu = s.Replace("yf", "cu").Replace("yu", "cu");
                    tmpNames.Add(cu);
                    tmpTGI.Add(new TGI(type, 0, FNVhash.FNV64(cu)));
                    string pu = s.Replace("yf", "pu").Replace("yu", "pu");
                    tmpNames.Add(pu);
                    tmpTGI.Add(new TGI(type, 0, FNVhash.FNV64(pu)));
                }
            }
            nameTmp.AddRange(tmpNames);
            tgiTmp.AddRange(tmpTGI);

            tgiList = tgiTmp;
            nameList = nameTmp;
            return;
        }

        private Dictionary<ulong, string> ParseTextResourcesList(string filename, List<IResourceIndexEntry> resourceEntries)
        {
            string executingPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string resourcePath = Path.Combine(executingPath, filename);
            if (!File.Exists(resourcePath))
            {
                MessageBox.Show(string.Format("'{0}' not found in CAS Tools directory '{1}'; TGI and resource name list cannot be loaded.", filename, executingPath));
                return null;
            }
            string line = "";
            System.IO.StreamReader file = new System.IO.StreamReader(resourcePath);
            Dictionary<TGI, string> dictTmp1 = new Dictionary<TGI, string>();
            while ((line = file.ReadLine()) != null)
            {
                string[] s = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                dictTmp1.Add((new TGI(s[0])), s[1]);
            }
            file.Close();

            Dictionary<ulong, string> dictTmp2 = new Dictionary<ulong, string>();
            foreach (KeyValuePair<TGI, string> pair in dictTmp1)
            {
                dictTmp2.Add(pair.Key.Instance, pair.Value);
            }
            uint type;
            if (resourceEntries.Count > 0) type = resourceEntries[0].ResourceType;
            else type = 0;
            foreach (KeyValuePair<TGI, string> pair in dictTmp1)
            {
                if (pair.Value.StartsWith("yf") || pair.Value.StartsWith("yu"))
                {
                    string cu = pair.Value.Replace("yf", "cu").Replace("yu", "cu");
                    ulong cuHash = FNVhash.FNV64(cu);
                    IResourceKey irie = new TGIBlock(1, null, type, 0U, cuHash);
                    if (resourceEntries.Contains(irie))
                    {
                        if (!dictTmp2.ContainsKey(cuHash) & !dictTmp2.ContainsValue(cu)) dictTmp2.Add(cuHash, cu);
                    }
                    string pu = pair.Value.Replace("yf", "pu").Replace("yu", "pu");
                    ulong puHash = FNVhash.FNV64(pu);
                    IResourceKey irie2 = new TGIBlock(1, null, type, 0U, puHash);
                    if (resourceEntries.Contains(irie2))
                    {
                        if (!dictTmp2.ContainsKey(puHash) & !dictTmp2.ContainsValue(pu)) dictTmp2.Add(puHash, pu);
                    }
                }
            }
            return dictTmp2;
        }

        private void CPREGo_button_Click(object sender, EventArgs e)
        {
            List<ulong> presetInstances = new List<ulong>();

            CPREsearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.CASPreset;
            TGI zero = new TGI();

            foreach (Package p in gamePackages)
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
                        if (PresetSearchHuman_radioButton.Checked && preset.species != Species.Human) continue;
                        if (PresetSearchCD_radioButton.Checked && preset.species == Species.Human) continue;
                        string[] str = new string[6];
                        str[0] = irie.Instance.ToString("X16");
                        str[1] = Enum.GetName(typeof(SimRegion), preset.region);
                        str[2] = preset.ageGender.ToString() + Environment.NewLine + "Frame: " + preset.bodyFrameGender.ToString();
                        str[3] = preset.species.ToString();
                        str[4] = ListOccultTags(preset);
                        str[5] = "";
                        foreach (CPRE.SculptLink sculpt in preset.sculpts)
                        {
                            str[5] += "Sculpt: " + sculpt.instance.ToString("X16") + Environment.NewLine;
                        }
                        foreach (CPRE.Modifier mod in preset.modifiers)
                        {
                            string smod = "0x" + mod.instance.ToString("X16");
                            string s;
                            str[5] += "SimModifier: " + smod;
                            if (smodDictEA.TryGetValue(mod.instance, out s)) str[5] += (String.Compare(smod, s) != 0 ? " (" + s + ")" : "") + Environment.NewLine;
                        }
                        if ((PresetSearchHuman_radioButton.Checked && preset.species == Species.Human) ||
                            (PresetSearchCD_radioButton.Checked && preset.species != Species.Human))
                        {
                            int ind = CPREsearch_dataGridView.Rows.Add(str);
                            CPREsearch_dataGridView.Rows[ind].Tag = preset;
                        }
                        presetInstances.Add(irie.Instance);
                    }
                }
            }
        }

        private static string ListOccultTags(CPRE preset)
        {
            string s = "";
            foreach (CPRE.Tag f in preset.tagList)
            {
                if (f.tagValue == (uint)PresetValueTags.Human || f.tagValue == (uint)PresetValueTags.Alien ||
                    f.tagValue == (uint)PresetValueTags.Vampire || f.tagValue == (uint)PresetValueTags.Mermaid ||
                    f.tagValue == (uint)PresetValueTags.Witch || f.tagValue == (uint)PresetValueTags.Werewolf) 
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

        private void BONDGo_button_Click(object sender, EventArgs e)
        {
            List<ulong> bondInstances = new List<ulong>();

            BONDsearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.BoneDelta;
            TGI zero = new TGI();

            foreach (Package p in gamePackages)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (bondInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        BOND bond;
                        try
                        {
                            bond = new BOND(br);
                        }
                        catch
                        {
                            continue;
                        }
                        string[] str = new string[1];
                        str[0] = irie.ToString();
                        string s;
                        str[0] += (smodDictEA.TryGetValue(irie.Instance, out s) ? " (" + s + ")" : "");
                        int ind = BONDsearch_dataGridView.Rows.Add(str);
                        BONDsearch_dataGridView.Rows[ind].Tag = bond;
                        bondInstances.Add(irie.Instance);
                    }
                }
            }
        }

        private void BONDsearch_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 1)
            {
                BOND[] bond = new BOND[] { (BOND)BONDsearch_dataGridView.Rows[e.RowIndex].Tag };
                Species species;
                if (bond[0].AllBonesInRig(adultRig)) species = Species.Human;
                else if (bond[0].AllBonesInRig(adultDogRig)) species = Species.Dog;
                else species = Species.Cat;
                Form previewForm = new EditMorphPreview(species, AgeGender.Adult | AgeGender.Male, AgeGender.None, SimRegion.BODY, SimSubRegion.None,
                        null, null, null, bond, null);
                previewForm.Text = "BoneDelta preview";
                previewForm.Show();
            }
            if (e.ColumnIndex == 2)
            {
                BOND bond = (BOND)BONDsearch_dataGridView.Rows[e.RowIndex].Tag;
                WriteBONDFile("Save BoneDelta", bond, (string)BONDsearch_dataGridView.Rows[e.RowIndex].Cells[0].Value);
            }
        }

        private void searchForID_button_Click(object sender, EventArgs e)
        {
            UInt32 type = 0;
            if (string.Compare(searchForTypeID.Text, " ") > 0)
            {
                try
                {
                    type = UInt32.Parse(searchForTypeID.Text.Replace("0x", "").Replace("0X", ""), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid hex type ID!");
                    return;
                }
            }
            UInt32 group = 0;
            if (string.Compare(searchForGroupID.Text, " ") > 0)
            {
                try
                {
                    group = UInt32.Parse(searchForGroupID.Text.Replace("0x", "").Replace("0X", ""), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid hex group ID!");
                    return;
                }
            }
            UInt64 instance = 0;
            if (string.Compare(searchForInstanceID.Text, " ") > 0)
            {
                try
                {
                    instance = UInt64.Parse(searchForInstanceID.Text.Replace("0x", "").Replace("0X", ""), System.Globalization.NumberStyles.HexNumber);
                }
                catch
                {
                    MessageBox.Show("Please enter a valid hex instance ID!");
                    return;
                }
            }
            string TS4FilesPath = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Maxis\\The Sims 4", "Install Dir", null);
            string[] paths = Directory.GetFiles(TS4FilesPath, "*.package", SearchOption.AllDirectories);
            if (paths.Length == 0)
            {
                MessageBox.Show("Can't find game packages! Please go to File / Change Settings and correct the game packages path or make it blank to reset, then restart.");
                return;
            }
            string output = "";
            int count = 0;
            foreach (string s in paths)
            {
                Package p = OpenPackage(s, false);
                if (p == null)
                {
                    MessageBox.Show("Can't read package: " + s);
                    return;
                }
                Predicate<IResourceIndexEntry> getID = r => (String.Compare(searchForTypeID.Text, " ") > 0 ? r.ResourceType == type : true) &&
                    (String.Compare(searchForGroupID.Text, " ") > 0 ? r.ResourceGroup == group : true) &&
                    (String.Compare(searchForInstanceID.Text, " ") > 0 ? r.Instance == instance : true);

                List<IResourceIndexEntry> ires = p.FindAll(getID);
                foreach (IResourceIndexEntry i in ires)
                {
                    output += s + " : " + i.ResourceType.ToString("X8") + "_" + i.ResourceGroup.ToString("X8") + "_" + i.Instance.ToString("X16") + Environment.NewLine;
                    if (SearchLimit_checkBox.Checked && count >= 100) break;
                    count++;
                }
            }
            searchForInstanceID_output.Text = output;
            if (count == 0) searchForInstanceID_output.Text += "None found";
        }

        private void DMapSearchGo_button_Click(object sender, EventArgs e)
        {
            List<ulong> DMapInstances = new List<ulong>();

            DMapSearch_dataGridView.Rows.Clear();
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.DeformerMap;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            foreach (Package p in gamePackages)
            {
                List<IResourceIndexEntry> iries = p.FindAll(pred);
                foreach (IResourceIndexEntry irie in iries)
                {
                    if (DMapInstances.IndexOf(irie.Instance) >= 0) continue;
                    using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                    {
                        if (br.BaseStream.Length == 0) continue;
                        DMap dmap;
                        try
                        {
                            dmap = new DMap(br);
                        }
                        catch
                        {
                            continue;
                        }
                        if (DMapSearchHuman_radioButton.Checked && dmap.Species != Species.Human && dmap.Species != Species.Werewolf) continue;
                        if (DMapSearchPets_radioButton.Checked && dmap.Species == Species.Human || dmap.Species == Species.Werewolf || dmap.Species == 0) continue;
                        if (DMapSearchPhysique_comboBox.SelectedIndex != -1)
                        {
                            if (!(DMapSearchPhysique_comboBox.SelectedIndex == 11 ||
                                 (DMapSearchPhysique_comboBox.SelectedIndex == 10 & dmap.Physique == Physiques.BODYBLENDTYPE_AVERAGE) ||
                                 DMapSearchPhysique_comboBox.SelectedIndex == (uint)dmap.Physique)) continue;
                        }
                        string[] str = new string[6];
                        str[0] = irie.ToString();
                        string tmp = dmap.ShapeOrNormal.ToString().Replace("_DEFORMER", "").ToLower();
                        str[1] = textInfo.ToTitleCase(tmp);
                        str[2] = dmap.Species.ToString();
                        str[3] = dmap.AgeGender.ToString();
                        str[4] = dmap.Physique.ToString();
                        string s = "";
                        if (dmapDictEA.TryGetValue(irie.Instance, out s)) str[5] = s;
                        int ind = DMapSearch_dataGridView.Rows.Add(str);
                        DMapSearch_dataGridView.Rows[ind].Tag = dmap;
                        if (dmap.ShapeOrNormal == ShapeOrNormals.NORMALS_DEFORMER)
                        {
                            DMapSearch_dataGridView.Rows[ind].Cells[6] = new DataGridViewTextBoxCell();
                            DMapSearch_dataGridView.Rows[ind].Cells[7] = new DataGridViewTextBoxCell();
                        }
                        DMapInstances.Add(irie.Instance);
                    }
                }
            }
        }

        private void DMapSearch_dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == 6)
            {
                DMap[] shape = new DMap[] { (DMap)DMapSearch_dataGridView.Rows[e.RowIndex].Tag };
                Form previewForm = new EditMorphPreview(shape[0].Species, shape[0].AgeGender, AgeGender.None, SimRegion.BODY, SimSubRegion.None,
                        null, shape, null, null, null);
                previewForm.Text = "DMap preview";
                previewForm.Show();
            }
            if (e.ColumnIndex == 7)
            {
                DMap shape = (DMap)DMapSearch_dataGridView.Rows[e.RowIndex].Tag;
                WriteDMap("Save Shape DMap", shape, (string)DMapSearch_dataGridView.Rows[e.RowIndex].Cells[0].Value);
                for (int i = 0; i < DMapSearch_dataGridView.Rows.Count; i++)
                {
                    DMap dmap = (DMap)DMapSearch_dataGridView.Rows[i].Tag;
                    if (dmap.ShapeOrNormal == ShapeOrNormals.SHAPE_DEFORMER) continue;
                    if (!dmap.HasData) continue;
                    if ((dmap.AgeGender == shape.AgeGender) && (dmap.Height == shape.Height) && (dmap.MaxCol == shape.MaxCol) && (dmap.MaxRow == shape.MaxRow) &&
                        (dmap.MinCol == shape.MinCol) && (dmap.MinRow == shape.MinRow) && (dmap.Physique == shape.Physique) && (dmap.Species == shape.Species) &&
                        (dmap.Width == shape.Width))
                    {
                        WriteDMap("Save Normals DMap", dmap, (string)DMapSearch_dataGridView.Rows[i].Cells[0].Value);
                    }
                }
            }
        }
    }
}
