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
        Package morphPack = null;
        NameMap nmap = null;
        List<HOTC> hotcList = new List<HOTC>();
        List<CPRE> presetList = new List<CPRE>();
        List<SMOD> smodList = new List<SMOD>();
        List<Sculpt> sculptList = new List<Sculpt>();

        private void EditNewPackage_button_Click(object sender, EventArgs e)
        {
            if (morphPack != null || hotcList.Count > 0 || presetList.Count > 0 || smodList.Count > 0 || sculptList.Count > 0)
            {
                DialogResult res = MessageBox.Show("Do you want to discard the current package and start a new one?", "Start Package", MessageBoxButtons.OKCancel);
                if (res == DialogResult.Cancel) return;
            }
            nmap = null;
            hotcList = new List<HOTC>();
            EditHOTC_dataGridView.Rows.Clear();
            ClearHOTC();
            currentHOTC = 0;
            currentSlider = 0;
            presetList = new List<CPRE>();
            EditPreset_dataGridView.Rows.Clear();
            ClearPreset();
            currentPreset = 0;
            currentPresetMod = 0;
            smodList = new List<SMOD>();
            SMODs_dataGridView.Rows.Clear();
            ClearSMOD();
            currentSMOD = 0;
            sculptList = new List<Sculpt>();
            Sculpt_dataGridView.Rows.Clear();
            ClearSculpt();
            currentSculpt = 0;
            EditPackageFile.Text = "";
            AppendPackageFileSelect_button.Enabled = false;
            morphs_treeView.Nodes.Clear();
        }

        private void EditPackageFileSelect_button_Click(object sender, EventArgs e)
        {
            if (morphPack != null || hotcList.Count > 0 || presetList.Count > 0 || smodList.Count > 0 || sculptList.Count > 0)
            {
                DialogResult res = MessageBox.Show("Do you want to discard the current package and open a new one?", "Open Package", MessageBoxButtons.OKCancel);
                if (res == DialogResult.Cancel) return;
            }
            EditPackageFile.Text = GetFilename("Select Slider/Preset Package to edit", Packagefilter);
            morphPack = OpenPackage(EditPackageFile.Text, false);
            if (morphPack == null)
            {
                MessageBox.Show("Can't open package!");
                return;
            }
            Cursor curse = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            nmap = null;
            hotcList = new List<HOTC>();
            presetList = new List<CPRE>();
            smodList = new List<SMOD>();
            sculptList = new List<Sculpt>();

            LoadPackage(morphPack);

            ListHOTC();
            ListPresets();
            ListSMOD();
            ListSculpt();
            ShowTreeView();
            this.Cursor = curse;
            AppendPackageFileSelect_button.Enabled = true;
            Application.DoEvents();
        }

        private void AppendPackageFileSelect_button_Click(object sender, EventArgs e)
        {
            if (morphPack == null)
            {
                DialogResult res = MessageBox.Show("There is no open package to append to!", "Append Package", MessageBoxButtons.OKCancel);
                if (res == DialogResult.Cancel) return;
            }
            string appendPack = GetFilename("Select Slider/Preset Package to append", Packagefilter);
            Package pack = OpenPackage(appendPack, false);
            if (pack == null)
            {
                MessageBox.Show("Can't open package!");
                return;
            }
            Cursor curse = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();

            LoadPackage(pack);

            ListHOTC();
            ListPresets();
            ListSMOD();
            ListSculpt();
            ShowTreeView();
            this.Cursor = curse;
            Application.DoEvents();
        }

        private void LoadPackage(Package pack)
        {
            Predicate<IResourceIndexEntry> predNMap = r => r.ResourceType == (uint)ResourceTypes.NameMap;
            IResourceIndexEntry irieNames = pack.Find(predNMap);
            if (irieNames != null)
            {
                using (BinaryReader br = new BinaryReader(pack.GetResource(irieNames)))
                {
                    if (nmap == null)
                    {
                        nmap = new NameMap(br);
                    }
                    else
                    {
                        nmap.addNmap(new NameMap(br));
                    }
                }
            }

            Predicate<IResourceIndexEntry> predHOTC = r => r.ResourceType == (uint)ResourceTypes.HotSpotControl;
            Predicate<IResourceIndexEntry> predPreset = r => r.ResourceType == (uint)ResourceTypes.CASPreset;
            Predicate<IResourceIndexEntry> predSculpt = r => r.ResourceType == (uint)ResourceTypes.Sculpt;
            Predicate<IResourceIndexEntry> predMod = r => r.ResourceType == (uint)ResourceTypes.SimModifier;

            List<IResourceIndexEntry> iries = pack.FindAll(predHOTC);
            bool? update = null;
            foreach (IResourceIndexEntry irie in iries)
            {
                HOTC hotc;
                using (BinaryReader br = new BinaryReader(pack.GetResource(irie)))
                {
                    hotc = new HOTC(br);
                }
                if (hotc.Version < hotc.CurrentVersion)
                {
                    if (update == null)
                    {
                        DialogResult res = MessageBox.Show("Update HotSpotControls to the latest version?", "Old version of HotSpotControls found", MessageBoxButtons.YesNo);
                        update = res == DialogResult.Yes;
                    }
                    if (update == true) hotc.Version = hotc.CurrentVersion;
                }
                if (nmap != null) hotc.HotSpotName = nmap.getName(irie.Instance);
                if (string.Compare(hotc.HotSpotName, " ") <= 0)
                    if (!hotcDictEA.TryGetValue(irie.Instance, out hotc.HotSpotName))
                        hotc.HotSpotName = irie.ToString();
                hotc.HotSpotTGI = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                hotc.isDefaultReplacement = IsInGamePacks(irie);
                hotcList.Add(hotc);
            }
            iries = pack.FindAll(predPreset);
            foreach (IResourceIndexEntry irie in iries)
            {
                CPRE cpre;
                using (BinaryReader br = new BinaryReader(pack.GetResource(irie)))
                {
                    cpre = new CPRE(br);
                }
                if (nmap != null) cpre.PresetName = nmap.getName(irie.Instance);
                if (string.Compare(cpre.PresetName, " ") <= 0) cpre.PresetName = irie.ToString();
                cpre.PresetTGI = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                cpre.isDefaultReplacement = IsInGamePacks(irie);
                Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.Thumbnail & r.Instance == irie.Instance;
                IResourceIndexEntry ir = pack.Find(pred);
                if (ir != null)
                {
                    using (Stream s = pack.GetResource(ir))
                    {
                        ThumbnailResource thumb = new ThumbnailResource(1, s);
                        cpre.Thumb = thumb;
                    }
                }

                presetList.Add(cpre);
            }
            iries = pack.FindAll(predSculpt);
            foreach (IResourceIndexEntry irie in iries)
            {
                Sculpt sculpt;
                using (BinaryReader br = new BinaryReader(pack.GetResource(irie)))
                {
                    sculpt = new Sculpt(br);
                }
                if (nmap != null) sculpt.SculptName = nmap.getName(irie.Instance);
                if (string.Compare(sculpt.SculptName, " ") <= 0) sculpt.SculptName = irie.ToString();
                sculpt.isDefaultReplacement = IsInGamePacks(irie);
                TGI tgi = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                if (!tgi.Equals(sculpt.publicKey[0])) sculpt.publicKey[0] = tgi;
                if (sculpt.BGEOKey != null && sculpt.BGEOKey.Length > 0 && sculpt.BGEOKey[0].Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.BGEOKey[0]);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            BGEO bgeo = new BGEO(br);
                            sculpt.bgeo = bgeo;
                        }
                    }
                }
                if (sculpt.textureRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.LRLE & r.ResourceGroup == sculpt.textureRef.Group & r.Instance == sculpt.textureRef.Instance;
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (Stream s = pack.GetResource(ir))
                        {
                            sculpt.texture = new LRLE(new BinaryReader(s));
                        }
                    }
                    else
                    {
                        pred = r => r.ResourceType == sculpt.textureRef.Type & r.ResourceGroup == sculpt.textureRef.Group & r.Instance == sculpt.textureRef.Instance;
                        ir = pack.Find(pred);
                        if (ir != null)
                        {
                            using (Stream s = pack.GetResource(ir))
                            {
                                try
                                {
                                    sculpt.texture = new LRLE(new BinaryReader(s));
                                }
                                catch
                                {
                                    try
                                    {
                                        RLEResource rle = new RLEResource(1, s);
                                        DdsFile dds = new DdsFile();
                                        dds.Load(rle.ToDDS(), false);
                                        sculpt.texture = new LRLE(dds.Image);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Can't read texture: " + sculpt.textureRef.ToString() + Environment.NewLine + ex.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                if (sculpt.bumpmapRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.bumpmapRef);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (Stream s = pack.GetResource(ir))
                        {
                            DSTResource dst = new DSTResource(1, s);
                            sculpt.bumpmap = dst;
                        }
                    }
                }
                if (sculpt.specularRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.specularRef);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (Stream s = pack.GetResource(ir))
                        {
                            sculpt.specular = new RLEResource(1, s);
                        }
                    }
                }
                if (sculpt.dmapShapeRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.dmapShapeRef);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            DMap dmap = new DMap(br);
                            sculpt.shape = dmap;
                        }
                    }
                }
                if (sculpt.dmapNormalRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.dmapNormalRef);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            DMap dmap = new DMap(br);
                            sculpt.normals = dmap;
                        }
                    }
                }
                if (sculpt.boneDeltaRef.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(sculpt.boneDeltaRef);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            BOND bond = new BOND(br);
                            sculpt.bond = bond;
                        }
                    }
                }
                sculptList.Add(sculpt);
            }
            iries = pack.FindAll(predMod);
            foreach (IResourceIndexEntry irie in iries)
            {
                SMOD smod;
                using (BinaryReader br = new BinaryReader(pack.GetResource(irie)))
                {
                    smod = new SMOD(br);
                }
                if (nmap != null) smod.smodName = nmap.getName(irie.Instance);
                if (string.Compare(smod.smodName, " ") <= 0) smod.smodName = irie.ToString();
                smod.isDefaultReplacement = IsInGamePacks(irie);
                TGI tgi = new TGI(irie.ResourceType, irie.ResourceGroup, irie.Instance);
                if (!tgi.Equals(smod.publicKey[0])) smod.publicKey[0] = tgi;
                if (smod.BGEOKey != null && smod.BGEOKey.Length > 0 && smod.BGEOKey[0].Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(smod.BGEOKey[0]);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            BGEO bgeo = new BGEO(br);
                            smod.bgeo = bgeo;
                        }
                    }
                }
                if (smod.bonePoseKey.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(smod.bonePoseKey);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            BOND bond = new BOND(br);
                            smod.bond = bond;
                        }
                    }
                }
                if (smod.deformerMapShapeKey.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(smod.deformerMapShapeKey);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            DMap dmap = new DMap(br);
                            smod.shape = dmap;
                        }
                    }
                }
                if (smod.deformerMapNormalKey.Instance > 0)
                {
                    Predicate<IResourceIndexEntry> pred = GetPredicate(smod.deformerMapNormalKey);
                    IResourceIndexEntry ir = pack.Find(pred);
                    if (ir != null)
                    {
                        using (BinaryReader br = new BinaryReader(pack.GetResource(ir)))
                        {
                            DMap dmap = new DMap(br);
                            smod.normals = dmap;
                        }
                    }
                }
                smodList.Add(smod);
            }
        }

        private Predicate<IResourceIndexEntry> GetPredicate(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            return pred;
        }

        private void SavePackage_Go_button_Click(object sender, EventArgs e)
        {
            Package morphPackage = (Package)Package.NewPackage(1);
            nmap = new NameMap();

            foreach (HOTC hotc in hotcList)
            {
                TGIBlock hotcRef;
                if (hotc.isDefaultReplacement)
                {
                    hotcRef = new TGIBlock(1, null, hotc.HotSpotTGI.Type, hotc.HotSpotTGI.Group, hotc.HotSpotTGI.Instance);
                }
                else
                {
                    hotcRef = new TGIBlock(1, null, (uint)ResourceTypes.HotSpotControl, 0, FNVhash.FNV64(hotc.HotSpotName) | 0x8000000000000000);
                }
                Stream s = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(s);
                hotc.Write(bw);
                morphPackage.AddResource(hotcRef, s, true);
                nmap.addName(hotcRef.Instance, hotc.HotSpotName, false);
            }

            foreach (CPRE preset in presetList)
            {
                TGIBlock preRef;
                if (preset.isDefaultReplacement)
                {
                    preRef = new TGIBlock(1, null, preset.PresetTGI.Type, preset.PresetTGI.Group, preset.PresetTGI.Instance);
                }
                else
                {
                    preRef = new TGIBlock(1, null, (uint)ResourceTypes.CASPreset, 0, FNVhash.FNV64(preset.PresetName) | 0x8000000000000000);
                }
                Stream s = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(s);
                preset.Write(bw);
                morphPackage.AddResource(preRef, s, true);
                if (preset.Thumb != null)
                {
                    TGIBlock thumbRef = new TGIBlock(1, null, (uint)ResourceTypes.Thumbnail, 
                        (preset.region == SimRegionPreset.BODY || preset.region == SimRegionPreset.FUR ? 3u : 2u), preRef.Instance);
                    morphPackage.AddResource(thumbRef, preset.Thumb.ToJFIF(), true);
                }
                nmap.addName(preRef.Instance, preset.PresetName, false);
            }

            foreach (SMOD smod in smodList)
            {
                //TGIBlock modRef = new TGIBlock(1, null, (uint)ResourceTypes.SimModifier, 0, FNVhash.FNV64(smod.smodName) | 0x8000000000000000);
                //smod.publicKey[0] = new TGI(modRef.ResourceType, modRef.ResourceGroup, modRef.Instance);
                TGIBlock modRef = new TGIBlock(1, null, (uint)ResourceTypes.SimModifier, 0, smod.publicKey[0].Instance);
                nmap.addName(smod.publicKey[0].Instance, smod.smodName, false);

                if (smod.bgeo != null)
                {
                    TGIBlock bgeoRef = new TGIBlock(1, null, (uint)ResourceTypes.BlendGeometry, 0, smod.publicKey[0].Instance);
                    smod.bgeo.PublicKey = new TGI(bgeoRef.ResourceType, bgeoRef.ResourceGroup, bgeoRef.Instance);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    smod.bgeo.Write(bwm);
                    morphPackage.AddResource(bgeoRef, sm, true);
                    smod.BGEOKey = new TGI[] { new TGI(smod.bgeo.PublicKey) };
                }
                if (smod.shape != null)
                {
                    TGIBlock dmapRef = new TGIBlock(1, null, (uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(smod.smodName + "_Shape") | 0x8000000000000000);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    smod.shape.Write(bwm);
                    morphPackage.AddResource(dmapRef, sm, true);
                    nmap.addName(dmapRef.Instance, smod.smodName + "_Shape", false);
                    smod.deformerMapShapeKey = new TGI(dmapRef.ResourceType, dmapRef.ResourceGroup, dmapRef.Instance);
                }
                if (smod.normals != null)
                {
                    TGIBlock dmapRef = new TGIBlock(1, null, (uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(smod.smodName + "_Normals") | 0x8000000000000000);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    smod.normals.Write(bwm);
                    morphPackage.AddResource(dmapRef, sm, true);
                    nmap.addName(dmapRef.Instance, smod.smodName + "_Normals", false);
                    smod.deformerMapNormalKey = new TGI(dmapRef.ResourceType, dmapRef.ResourceGroup, dmapRef.Instance);
                }
                if (smod.bond != null)
                {
                    TGIBlock bondRef = new TGIBlock(1, null, (uint)ResourceTypes.BoneDelta, 0, smod.publicKey[0].Instance);
                    smod.bond.publicKey[0] = new TGI(bondRef.ResourceType, bondRef.ResourceGroup, bondRef.Instance);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    smod.bond.Write(bwm);
                    morphPackage.AddResource(bondRef, sm, true);
                    smod.bonePoseKey = new TGI(smod.bond.publicKey[0]);
                }

                Stream s = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(s);
                smod.Write(bw);
                morphPackage.AddResource(modRef, s, true);
            }

            foreach (Sculpt sculpt in sculptList)
            {
                //TGIBlock sculptRef = new TGIBlock(1, null, (uint)ResourceTypes.Sculpt, 0, FNVhash.FNV64(sculpt.SculptName) | 0x8000000000000000);
                //sculpt.publicKey[0] = new TGI(sculptRef.ResourceType, sculptRef.ResourceGroup, sculptRef.Instance);
                TGIBlock sculptRef = new TGIBlock(1, null, (uint)ResourceTypes.Sculpt, 0, sculpt.publicKey[0].Instance);
                nmap.addName(sculptRef.Instance, sculpt.SculptName, false);

                if (sculpt.bgeo != null)
                {
                    TGIBlock bgeoRef = new TGIBlock(1, null, (uint)ResourceTypes.BlendGeometry, 0, sculptRef.Instance);
                    sculpt.bgeo.PublicKey = new TGI(bgeoRef.ResourceType, bgeoRef.ResourceGroup, bgeoRef.Instance);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    sculpt.bgeo.Write(bwm);
                    morphPackage.AddResource(bgeoRef, sm, true);
                    sculpt.BGEOKey = new TGI[] { new TGI(sculpt.bgeo.PublicKey) };
                }
                if (sculpt.shape != null)
                {
                    TGIBlock dmapRef = new TGIBlock(1, null, (uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(sculpt.SculptName + "_Shape") | 0x8000000000000000);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    sculpt.shape.Write(bwm);
                    morphPackage.AddResource(dmapRef, sm, true);
                    nmap.addName(dmapRef.Instance, sculpt.SculptName + "_Shape", false);
                    sculpt.dmapShapeRef = new TGI(dmapRef.ResourceType, dmapRef.ResourceGroup, dmapRef.Instance);
                }
                if (sculpt.normals != null)
                {
                    TGIBlock dmapRef = new TGIBlock(1, null, (uint)ResourceTypes.DeformerMap, 0, FNVhash.FNV64(sculpt.SculptName + "_Normals") | 0x8000000000000000);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    sculpt.normals.Write(bwm);
                    morphPackage.AddResource(dmapRef, sm, true);
                    nmap.addName(dmapRef.Instance, sculpt.SculptName + "_Normals", false);
                    sculpt.dmapNormalRef = new TGI(dmapRef.ResourceType, dmapRef.ResourceGroup, dmapRef.Instance);
                }
                if (sculpt.texture != null)
                {
                    TGIBlock textRef = new TGIBlock(1, null, (uint)ResourceTypes.LRLE, 0, sculptRef.Instance);
                    morphPackage.AddResource(textRef, sculpt.texture.Stream, true);
                    sculpt.textureRef = new TGI((uint)ResourceTypes.RLE2, textRef.ResourceGroup, textRef.Instance);
                }
                if (sculpt.bumpmap != null)
                {
                    TGIBlock bumpRef = new TGIBlock(1, null, (uint)ResourceTypes.DDS, 0, sculptRef.Instance);
                    morphPackage.AddResource(bumpRef, sculpt.bumpmap.Stream, true);
                    sculpt.bumpmapRef = new TGI(bumpRef.ResourceType, bumpRef.ResourceGroup, bumpRef.Instance);
                }
                if (sculpt.specular != null)
                {
                    TGIBlock specRef = new TGIBlock(1, null, (uint)ResourceTypes.RLES, 0, sculptRef.Instance);
                    morphPackage.AddResource(specRef, sculpt.specular.Stream, true);
                    sculpt.specularRef = new TGI(specRef.ResourceType, specRef.ResourceGroup, specRef.Instance);
                }
                if (sculpt.bond != null)
                {
                    TGIBlock bondRef = new TGIBlock(1, null, (uint)ResourceTypes.BoneDelta, 0, sculptRef.Instance);
                    Stream sm = new MemoryStream();
                    BinaryWriter bwm = new BinaryWriter(sm);
                    sculpt.bond.Write(bwm);
                    morphPackage.AddResource(bondRef, sm, true);
                    sculpt.boneDeltaRef = new TGI(bondRef.ResourceType, bondRef.ResourceGroup, bondRef.Instance);
                }

                Stream s = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(s);
                sculpt.Write(bw);
                morphPackage.AddResource(sculptRef, s, true);
            }

            TGIBlock nameRef = new TGIBlock(1, null, (uint)ResourceTypes.NameMap, 0, 0);
            Stream sn = new MemoryStream();
            BinaryWriter bwn = new BinaryWriter(sn);
            nmap.Write(bwn);
            morphPackage.AddResource(nameRef, sn, true);

            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = Packagefilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = "Save package";
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "package";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            morphPackage.SaveAs(myStream);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write package file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                    return;
                }
            }
            EditPackageFile.Text = saveFileDialog1.FileName;
        }

        private int ListKeyLookUp(List<SMOD> resourceList, ulong instance)
        {
            for (int i = 0; i < resourceList.Count; i++)
            {
                if (resourceList[i].publicKey[0].Instance == instance) return i;
            }
            return -1;
        }
        private int ListKeyLookUp(List<Sculpt> resourceList, ulong instance)
        {
            for (int i = 0; i < resourceList.Count; i++)
            {
                if (resourceList[i].publicKey[0].Instance == instance) return i;
            }
            return -1;
        }

        private void ShowAgeGender(AgeGender ageGender, CheckedListBox ageList, CheckedListBox genderList)
        {
            ageList.SetItemChecked(0, ((ageGender & AgeGender.Infant) > 0));
            ageList.SetItemChecked(1, ((ageGender & AgeGender.Toddler) > 0));
            ageList.SetItemChecked(2, ((ageGender & AgeGender.Child) > 0));
            ageList.SetItemChecked(3, ((ageGender & AgeGender.Teen) > 0));
            ageList.SetItemChecked(4, ((ageGender & AgeGender.YoungAdult) > 0));
            ageList.SetItemChecked(5, ((ageGender & AgeGender.Adult) > 0));
            ageList.SetItemChecked(6, ((ageGender & AgeGender.Elder) > 0));

            genderList.SetItemChecked(0, ((ageGender & AgeGender.Male) > 0));
            genderList.SetItemChecked(1, ((ageGender & AgeGender.Female) > 0)); 
        }

        private AgeGender GetAgeGender(CheckedListBox ageList, CheckedListBox genderList)
        {
            AgeGender ageGender = AgeGender.None;
            if (ageList.GetItemChecked(0)) ageGender = ageGender | AgeGender.Infant;
            if (ageList.GetItemChecked(1)) ageGender = ageGender | AgeGender.Toddler;
            if (ageList.GetItemChecked(2)) ageGender = ageGender | AgeGender.Child;
            if (ageList.GetItemChecked(3)) ageGender = ageGender | AgeGender.Teen;
            if (ageList.GetItemChecked(4)) ageGender = ageGender | AgeGender.YoungAdult;
            if (ageList.GetItemChecked(5)) ageGender = ageGender | AgeGender.Adult;
            if (ageList.GetItemChecked(6)) ageGender = ageGender | AgeGender.Elder;

            if (genderList.GetItemChecked(0)) ageGender = ageGender | AgeGender.Male;
            if (genderList.GetItemChecked(1)) ageGender = ageGender | AgeGender.Female;

            return ageGender;
        }

        private void ShowArchetype(ArchetypeFlags archetype, CheckedListBox archetypeList)
        {
            archetypeList.SetItemChecked(0, ((archetype & ArchetypeFlags.None) > 0));
            archetypeList.SetItemChecked(1, ((archetype & ArchetypeFlags.Caucasian) > 0));
            archetypeList.SetItemChecked(2, ((archetype & ArchetypeFlags.African) > 0));
            archetypeList.SetItemChecked(3, ((archetype & ArchetypeFlags.Asian) > 0));
            archetypeList.SetItemChecked(4, ((archetype & ArchetypeFlags.MiddleEastern) > 0));
            archetypeList.SetItemChecked(5, ((archetype & ArchetypeFlags.NativeAmerican) > 0));
        }

        private ArchetypeFlags GetArchetype(CheckedListBox archtypeList)
        {
            ArchetypeFlags archtype = ArchetypeFlags.None;
            if (archtypeList.GetItemChecked(1)) archtype = archtype | ArchetypeFlags.Caucasian;
            if (archtypeList.GetItemChecked(2)) archtype = archtype | ArchetypeFlags.African;
            if (archtypeList.GetItemChecked(3)) archtype = archtype | ArchetypeFlags.Asian;
            if (archtypeList.GetItemChecked(4)) archtype = archtype | ArchetypeFlags.MiddleEastern;
            if (archtypeList.GetItemChecked(5)) archtype = archtype | ArchetypeFlags.NativeAmerican;
            return archtype;
        }

        private void ShowPresetTags(CPRE.Tag[] flagList, CheckedListBox archetypeTagList, CheckedListBox occultTagList)
        {
            for (int i = 0; i < archetypeTagList.Items.Count; i++)
            {
                archetypeTagList.SetItemChecked(i, false);
            }
            for (int i = 0; i < occultTagList.Items.Count; i++)
            {
                occultTagList.SetItemChecked(i, false);
            }
            foreach (CPRE.Tag f in flagList)
            {
                if (f.tagValue == (uint)PresetValueTags.African) archetypeTagList.SetItemChecked(0, true);
                if (f.tagValue == (uint)PresetValueTags.Asian) archetypeTagList.SetItemChecked(1, true);
                if (f.tagValue == (uint)PresetValueTags.Caucasian) archetypeTagList.SetItemChecked(2, true);
                if (f.tagValue == (uint)PresetValueTags.Latin) archetypeTagList.SetItemChecked(3, true);
                if (f.tagValue == (uint)PresetValueTags.MiddleEastern) archetypeTagList.SetItemChecked(4, true);
                if (f.tagValue == (uint)PresetValueTags.NorthAmerican) archetypeTagList.SetItemChecked(5, true);
                if (f.tagValue == (uint)PresetValueTags.SouthAsian) archetypeTagList.SetItemChecked(6, true);

                if (f.tagValue == (uint)PresetValueTags.Human) occultTagList.SetItemChecked(0, true);
                if (f.tagValue == (uint)PresetValueTags.Alien) occultTagList.SetItemChecked(1, true);
                if (f.tagValue == (uint)PresetValueTags.Vampire) occultTagList.SetItemChecked(2, true);
                if (f.tagValue == (uint)PresetValueTags.Mermaid) occultTagList.SetItemChecked(3, true);
                if (f.tagValue == (uint)PresetValueTags.Witch) occultTagList.SetItemChecked(4, true);
                if (f.tagValue == (uint)PresetValueTags.Werewolf) occultTagList.SetItemChecked(5, true);
            }
        }

        private CPRE.Tag[] GetPresetTags(CheckedListBox archetypeTagList, CheckedListBox occultTagList, uint presetVersion)
        {
            List<CPRE.Tag> tagList = new List<CPRE.Tag>();
            if (archetypeTagList.GetItemChecked(0)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.African, presetVersion));
            if (archetypeTagList.GetItemChecked(1)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.Asian, presetVersion));
            if (archetypeTagList.GetItemChecked(2)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.Caucasian, presetVersion));
            if (archetypeTagList.GetItemChecked(3)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.Latin, presetVersion));
            if (archetypeTagList.GetItemChecked(4)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.MiddleEastern, presetVersion));
            if (archetypeTagList.GetItemChecked(5)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.NorthAmerican, presetVersion));
            if (archetypeTagList.GetItemChecked(6)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Archetype, PresetValueTags.SouthAsian, presetVersion));
            if (occultTagList.GetItemChecked(0)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Human, presetVersion));
            if (occultTagList.GetItemChecked(1)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Alien, presetVersion));
            if (occultTagList.GetItemChecked(2)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Vampire, presetVersion));
            if (occultTagList.GetItemChecked(3)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Mermaid, presetVersion));
            if (occultTagList.GetItemChecked(4)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Witch, presetVersion));
            if (occultTagList.GetItemChecked(5)) tagList.Add(new CPRE.Tag(PresetCategoryTags.Occult, PresetValueTags.Werewolf, presetVersion));
            return tagList.ToArray();
        }

        private bool IsInGamePacks(TGI tgi)
        {
            if (tgi == null) return false;
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            return IsInGamePacks(pred);
        }

        private bool IsInGamePacks(IResourceIndexEntry irie)
        {
            if (irie == null) return false;
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == irie.ResourceType & r.ResourceGroup == irie.ResourceGroup & r.Instance == irie.Instance;
            return IsInGamePacks(pred);
        }

        private bool IsInGamePacks(Predicate<IResourceIndexEntry> pred)
        {
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry itest = p.Find(pred);
                if (itest != null)
                {
                    return true;
                }
            }
            return false;
        }

        private Sculpt FetchGameSculpt(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            Sculpt sculpt = new Sculpt(br);
                            return sculpt;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read Sculpt: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }
        private SMOD FetchGameSMOD(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            SMOD smod = new SMOD(br);
                            return smod;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read SMOD: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }
        private BGEO FetchGameBGEO(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            BGEO bgeo = new BGEO(br);
                            return bgeo;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read BGEO: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }
        private DMap FetchGameDMap(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            DMap dmap = new DMap(br);
                            return dmap;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read DMap: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }

        private BOND FetchGameBOND(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (BinaryReader br = new BinaryReader(p.GetResource(irie)))
                        {
                            BOND bond = new BOND(br);
                            return bond;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read BOND: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }

        private RLEResource FetchGameRLES(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        RLEResource rle;
                        using (Stream s = p.GetResource(irie))
                        {
                            rle = new RLEResource(1, s);
                        }
                        return rle;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read RLES: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }

        private LRLE FetchGameLRLE(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == (uint)ResourceTypes.LRLE & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (Stream s = p.GetResource(irie))
                        {
                            LRLE lrle = new LRLE(new BinaryReader(s));
                            return lrle;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read LRLE: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return new LRLE();
        }

        private DSTResource FetchGameDST(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gamePackages)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        using (Stream s = p.GetResource(irie))
                        {
                            DSTResource dds = new DSTResource(1, s);
                            return dds;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read DST: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }

        private ThumbnailResource FetchGameThumb(TGI tgi)
        {
            Predicate<IResourceIndexEntry> pred = r => r.ResourceType == tgi.Type & r.ResourceGroup == tgi.Group & r.Instance == tgi.Instance;
            foreach (Package p in gameThumbPacks)
            {
                IResourceIndexEntry irie = p.Find(pred);
                if (irie != null)
                {
                    try
                    {
                        ThumbnailResource thumb;
                        using (Stream s = p.GetResource(irie))
                        {
                            thumb = new ThumbnailResource(1, s);
                        }
                        return thumb;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Can't read Thumbnail: " + irie.ToString() + Environment.NewLine + e.Message);
                    }
                }
            }
            return null;
        }
    }
}
