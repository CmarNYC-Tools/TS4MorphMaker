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
    public partial class EditMorphPreview : Form
    {
        System.Resources.ResourceManager rm = Properties.Resources.ResourceManager;
        Species currentSpecies;
        AgeGender currentAge;
        AgeGender currentGender;
        GEOM CurrentHead;
        GEOM CurrentBody;
        GEOM CurrentHeadMorph;
        GEOM CurrentBodyMorph;
        GEOM CurrentEars, CurrentTail;
        GEOM CurrentEarsMorph, CurrentTailMorph;
        BGEO[] morphBGEO;
        MorphMap[] morphShape;
        MorphMap[] morphNormals;
        BOND[] morphBOND;
        Image skinOverlay;
        bool skipModelUpdate = false;

        public EditMorphPreview(Species species, AgeGender ageGender, AgeGender frame, SimRegion region, SimSubRegion subRegion, 
                                BGEO[] bgeo, DMap[] dmapShape, DMap[] dmapNormals, BOND[] bond, Image skin)
        {
            InitializeComponent();
            Species_listBox.SelectedIndexChanged -= Species_listBox_SelectedIndexChanged;
            AgeGender_listBox.SelectedIndexChanged -= AgeGender_listBox_SelectedIndexChanged;
            Type1_listBox.SelectedIndexChanged -= Type1_listBox_SelectedIndexChanged;
            Type2_listBox.SelectedIndexChanged -= Type2_listBox_SelectedIndexChanged;
            morphBGEO = bgeo;
            if (dmapShape != null)
            {
                morphShape = new MorphMap[dmapShape.Length];
                morphNormals = dmapNormals != null ? new MorphMap[dmapNormals.Length] : new MorphMap[dmapShape.Length];
                for (int i = 0; i < dmapShape.Length; i++)
                {
                    morphShape[i] = dmapShape[i].ToMorphMap();
                    morphNormals[i] = dmapNormals != null && dmapNormals[i] != null ? dmapNormals[i].ToMorphMap() : null;
                }
            }
            else
            {
                morphShape = null;
                morphNormals = null;
            }
            morphBOND = bond;
            skinOverlay = skin;

            Species_listBox.SelectedIndex = (int)species - 1;
            bool isAdult = ((ageGender & AgeGender.Teen) > 0) || ((ageGender & AgeGender.YoungAdult) > 0) || ((ageGender & AgeGender.Adult) > 0) || ((ageGender & AgeGender.Elder) > 0);
            if (species == Species.Human)
            {
                if (isAdult)
                {
                    if ((ageGender & AgeGender.Male) > 0) AgeGender_listBox.SelectedIndex = 3;
                    else AgeGender_listBox.SelectedIndex = 4;
                    if ((frame & AgeGender.Male) > 0) Type1_listBox.SelectedIndex = 0;
                    else if ((frame & AgeGender.Female) > 0) Type1_listBox.SelectedIndex = 1;
                    Type2_listBox.SelectedIndex = 0;
                }
                else
                {
                    if ((ageGender & AgeGender.Child) > 0) AgeGender_listBox.SelectedIndex = 2;
                    else if ((ageGender & AgeGender.Toddler) > 0) AgeGender_listBox.SelectedIndex = 1;
                    else AgeGender_listBox.SelectedIndex = 0;
                }
            }
            else if (species == Species.Werewolf)
            {
                if ((ageGender & AgeGender.Male) > 0) AgeGender_listBox.SelectedIndex = 0;
                else AgeGender_listBox.SelectedIndex = 1;
                if ((frame & AgeGender.Male) > 0) Type1_listBox.SelectedIndex = 0;
                else if ((frame & AgeGender.Female) > 0) Type1_listBox.SelectedIndex = 1;
                Type2_listBox.SelectedIndex = 0;
            }
            else
            {
                AgeGender_listBox.Items.Clear();
                if (species == Species.Cat) AgeGender_listBox.Items.AddRange(new string[] { "Kitten", "Adult" });
                else AgeGender_listBox.Items.AddRange(new string[] { "Puppy", "Adult" });
                if (isAdult) AgeGender_listBox.SelectedIndex = 1;
                else AgeGender_listBox.SelectedIndex = 0;
                Type1_listBox.Items.Clear();
                Type1_listBox.Items.AddRange(new string[] { "Ears Up", "Ears Down" });
                Type1_listBox.SelectedIndex = subRegion == SimSubRegion.EarsDown ? 1 : 0;
                Type2_listBox.Items.Clear();
                if (species == Species.Cat) Type2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub" });
                else Type2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub", "Tail Ring", "Tail Screw" });
                int tmp = (int)subRegion - 3;
                if (tmp < 0) tmp = 0; if (tmp > Type2_listBox.Items.Count - 1) tmp = Type2_listBox.Items.Count - 1;
                Type2_listBox.SelectedIndex = tmp;
            }

            Set_AgeGenderFrame();
            Species_listBox.SelectedIndexChanged += Species_listBox_SelectedIndexChanged;
            AgeGender_listBox.SelectedIndexChanged += AgeGender_listBox_SelectedIndexChanged;
            Type1_listBox.SelectedIndexChanged += Type1_listBox_SelectedIndexChanged;
            Type2_listBox.SelectedIndexChanged += Type2_listBox_SelectedIndexChanged;
        }

        private void Species_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AgeGender_listBox.SelectedIndexChanged -= AgeGender_listBox_SelectedIndexChanged;
            skipModelUpdate = true;
            Species species = (Species)(Species_listBox.SelectedIndex + 1);
            if (species == Species.Human)
            {
                AgeGender_listBox.Items.Clear();
                AgeGender_listBox.Items.AddRange(new string[] { "Infant", "Toddler", "Child", "Adult Male", "Adult Female" });
                AgeGender_listBox.SelectedIndex = 3;
                Type1_listBox.Items.Clear();
                Type1_listBox.Items.AddRange(new string[] { "Male Frame", "Female Frame" });
                Type1_listBox.SelectedIndex = 0;
                Type2_listBox.Items.Clear();
                Type2_listBox.Items.AddRange(new string[] { "Skintight", "Robe" });
                Type2_listBox.SelectedIndex = 0;
                SaveHead_button.Enabled = true;
            }
            else if (species == Species.Werewolf)
            {
                AgeGender_listBox.Items.Clear();
                AgeGender_listBox.Items.AddRange(new string[] { "Adult Male", "Adult Female" });
                AgeGender_listBox.SelectedIndex = 0;
                Type1_listBox.Items.Clear();
                Type1_listBox.Items.AddRange(new string[] { "Male Frame", "Female Frame" });
                Type1_listBox.SelectedIndex = 0;
                Type2_listBox.Items.Clear();
                Type2_listBox.Items.AddRange(new string[] { "Skintight" });
                Type2_listBox.SelectedIndex = 0;
                SaveHead_button.Enabled = true;
            }
            else
            {
                AgeGender_listBox.Items.Clear();
                Type1_listBox.Items.Clear();
                Type1_listBox.Items.AddRange(new string[] { "Ears Up", "Ears Down" });
                Type1_listBox.SelectedIndex = 0;
                Type2_listBox.Items.Clear();
                if (species == Species.Cat)
                {
                    Type2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub" });
                    AgeGender_listBox.Items.AddRange(new string[] { "Kitten", "Adult" });
                }
                else
                {
                    Type2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub", "Tail Ring", "Tail Screw" });
                    AgeGender_listBox.Items.AddRange(new string[] { "Puppy", "Adult" });
                }
                Type2_listBox.SelectedIndex = 0;
                AgeGender_listBox.SelectedIndex = 1;
                SaveHead_button.Enabled = false;
            }
            skipModelUpdate = false;
            Set_AgeGenderFrame();
            AgeGender_listBox.SelectedIndexChanged += AgeGender_listBox_SelectedIndexChanged;
        }

        private void AgeGender_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skipModelUpdate = true;
            if (currentSpecies == Species.Human && currentAge == AgeGender.Adult)
            {
                Type1_listBox.SelectedIndex = AgeGender_listBox.SelectedIndex - 3;
                Type2_listBox.SelectedIndex = 0;
            }
            else if (currentSpecies == Species.Werewolf)
            {
                Type1_listBox.SelectedIndex = AgeGender_listBox.SelectedIndex;
                Type2_listBox.SelectedIndex = 0;
            }
            skipModelUpdate = false;
            Set_AgeGenderFrame();
        }

        private void Type1_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentSpecies == Species.Human || currentSpecies == Species.Werewolf)
            {
                Set_AgeGenderFrame();
            }
            else
            {
                SwapPetParts();
            }
        }

        private void Type2_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (currentSpecies == Species.Human || currentSpecies == Species.Werewolf)
            {
                Set_AgeGenderFrame();
            }
            else
            {
                SwapPetParts();
            }
        }

        private void Set_AgeGenderFrame()
        {
            if (skipModelUpdate) return;
            currentSpecies = (Species)(Species_listBox.SelectedIndex + 1);
            if (currentSpecies == Species.Human)
            {
                if (AgeGender_listBox.SelectedIndex == 0) currentAge = AgeGender.Infant;
                else if (AgeGender_listBox.SelectedIndex == 1) currentAge = AgeGender.Toddler;
                else if (AgeGender_listBox.SelectedIndex == 2) currentAge = AgeGender.Child;
                else currentAge = AgeGender.Adult;
            }
            else if (currentSpecies == Species.Werewolf)
            {
                currentAge = AgeGender.Adult;
            }
            else
            {
                if (AgeGender_listBox.SelectedIndex == 0) currentAge = AgeGender.Child;
                else currentAge = AgeGender.Adult;
            }
            if (currentSpecies == Species.Human)
            {
                if (currentAge == AgeGender.Adult)
                {
                    if (AgeGender_listBox.SelectedIndex == 3) currentGender = AgeGender.Male;
                    else currentGender = AgeGender.Female;
                }
                else
                {
                    currentGender = AgeGender.Unisex;
                }
            }
            else if (currentSpecies == Species.Werewolf)
            {
                if (AgeGender_listBox.SelectedIndex == 0) currentGender = AgeGender.Male;
                else currentGender = AgeGender.Female;
            }
            else
            {
                currentGender = AgeGender.Unisex;
            }

            string prefix = Form1.GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            if (currentSpecies == Species.Human || currentSpecies == Species.Werewolf)
            {
                CurrentHead = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0"))));
                string bodyType = (currentAge == AgeGender.Adult && Type2_listBox.SelectedIndex == 1) ? "Robe" : "Complete";
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Body" + bodyType + "_lod0"))));
                CurrentEars = null;
                CurrentTail = null;
                if (currentAge == AgeGender.Adult && currentGender == AgeGender.Male)
                {
                    if (Type1_listBox.SelectedIndex == 1)
                    {
                        DMap shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_shape)));
                        DMap normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_normals)));
                        CurrentHead = Form1.LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = Form1.LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                }
                else if (currentAge == AgeGender.Adult && currentGender == AgeGender.Female)
                {
                    if (Type1_listBox.SelectedIndex == 0)
                    {
                        DMap shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_shape)));
                        DMap normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_normals)));
                        CurrentHead = Form1.LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = Form1.LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                }
                SaveHead_button.Enabled = true;
            }
            else
            {
                CurrentHead = null;
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                string earType = Type1_listBox.SelectedIndex == 0 ? "Ears" : "EarsDown";
                CurrentEars = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + earType + "_lod0"))));
                string[] tailType = new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
                CurrentTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[Type2_listBox.SelectedIndex] + "_lod0"))));
                SaveHead_button.Enabled = false;
            }

            if (CurrentHead != null) CurrentHeadMorph = new GEOM(CurrentHead);
            else CurrentHeadMorph = null;
            CurrentBodyMorph = new GEOM(CurrentBody);
            if (CurrentEars != null) CurrentEarsMorph = new GEOM(CurrentEars);
            else CurrentEarsMorph = null;
            if (CurrentTail != null) CurrentTailMorph = new GEOM(CurrentTail);
            else CurrentTailMorph = null;
            if (morphBOND != null)
            {
                RIG rig = Form1.GetTS4Rig(currentSpecies, currentAge);
                for (int i = 0; i < morphBOND.Length; i++)
                {
                    CurrentHeadMorph = Form1.LoadBONDMorph(CurrentHeadMorph, morphBOND[i], rig);
                    CurrentBodyMorph = Form1.LoadBONDMorph(CurrentBodyMorph, morphBOND[i], rig);
                    CurrentEarsMorph = Form1.LoadBONDMorph(CurrentEarsMorph, morphBOND[i], rig);
                    CurrentTailMorph = Form1.LoadBONDMorph(CurrentTailMorph, morphBOND[i], rig);
                }
            }
            if (morphBGEO != null)
            {
                for (int i = 0; i < morphBGEO.Length; i++)
                {
                    CurrentHeadMorph = Form1.LoadBGEOMorph(CurrentHeadMorph, morphBGEO[i], 0);
                }
            }
            if (morphShape != null)
            {
                for (int i = 0; i < morphShape.Length; i++)
                {
                    CurrentHeadMorph = Form1.LoadDMapMorph(CurrentHeadMorph, morphShape[i], morphNormals[i]);
                    CurrentBodyMorph = Form1.LoadDMapMorph(CurrentBodyMorph, morphShape[i], morphNormals[i]);
                    CurrentEarsMorph = Form1.LoadDMapMorph(CurrentEarsMorph, morphShape[i], morphNormals[i]);
                    CurrentTailMorph = Form1.LoadDMapMorph(CurrentTailMorph, morphShape[i], morphNormals[i]);
                    if (CurrentHeadMorph != null) CurrentHeadMorph.MatchPartSeamStitches();
                    if (CurrentBodyMorph != null) CurrentBodyMorph.MatchPartSeamStitches();
                    if (CurrentEarsMorph != null) CurrentEarsMorph.MatchPartSeamStitches();
                    if (CurrentTailMorph != null) CurrentTailMorph.MatchPartSeamStitches();
                    if (CurrentHeadMorph != null && CurrentBodyMorph != null) GEOM.MatchSeamStitches(CurrentHeadMorph, CurrentBodyMorph);
                    if (CurrentHeadMorph != null && CurrentEarsMorph != null) GEOM.MatchSeamStitches(CurrentHeadMorph, CurrentEarsMorph);
                    if (CurrentBodyMorph != null && CurrentTailMorph != null) GEOM.MatchSeamStitches(CurrentBodyMorph, CurrentTailMorph);
                }
            }
            morphPreview1.Stop_Mesh();
            if (ShowMorph_checkBox.Checked)
            {
                morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, null, skinOverlay, true);
            }
            else
            {
                morphPreview1.Start_Mesh(CurrentHead, CurrentBody, CurrentEars, CurrentTail, null, skinOverlay, true);
            }
        }

        private void SwapPetParts()
        {
            if (skipModelUpdate) return;
            string prefix = Form1.GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            string earType = Type1_listBox.SelectedIndex == 0 ? "Ears" : "EarsDown";
            CurrentEars = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + earType + "_lod0"))));
            string[] tailType = new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
            CurrentTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[Type2_listBox.SelectedIndex] + "_lod0"))));

            if (CurrentEars != null) CurrentEarsMorph = new GEOM(CurrentEars);
            else CurrentEarsMorph = null;
            if (CurrentTail != null) CurrentTailMorph = new GEOM(CurrentTail);
            else CurrentTailMorph = null;
            if (morphBOND != null)
            {
                RIG rig = Form1.GetTS4Rig(currentSpecies, currentAge);
                for (int i = 0; i < morphBOND.Length; i++)
                {
                    CurrentEarsMorph = Form1.LoadBONDMorph(CurrentEarsMorph, morphBOND[i], rig);
                    CurrentTailMorph = Form1.LoadBONDMorph(CurrentTailMorph, morphBOND[i], rig);
                }
            }
            if (morphShape != null)
            {
                for (int i = 0; i < morphShape.Length; i++)
                {
                    CurrentEarsMorph = Form1.LoadDMapMorph(CurrentEarsMorph, morphShape[i], morphNormals[i]);
                    CurrentTailMorph = Form1.LoadDMapMorph(CurrentTailMorph, morphShape[i], morphNormals[i]);
                }
            }
            morphPreview1.Stop_Mesh();
            if (ShowMorph_checkBox.Checked)
            {
                morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, null, skinOverlay, true);
            }
            else
            {
                morphPreview1.Start_Mesh(CurrentHead, CurrentBody, CurrentEars, CurrentTail, null, skinOverlay, true);
            }
        }

        private void ShowMorph_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowStuff();
        }

        private void ShowOverlay_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowStuff();
        }

        private void ShowStuff()
        {
            morphPreview1.Stop_Mesh();
            if (ShowMorph_checkBox.Checked)
            {
                if (ShowOverlay_checkBox.Checked)
                {
                    morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, null, skinOverlay, false);
                }
                else
                {
                    morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, null, null, false);
                }
            }
            else
            {
                morphPreview1.Start_Mesh(CurrentHead, CurrentBody, CurrentEars, CurrentTail, null, null, false);
            }
        }
        
        private void SaveHeadMorph_button_Click(object sender, EventArgs e)
        {
            if (currentSpecies != Species.Human) return;
            if (Form.ModifierKeys == Keys.Control)
            {
                Form1.WriteGEOM("Save GEOM of morphed head", CurrentHeadMorph);
            }
            else
            {
                RIG rig = Form1.GetTS4Rig(currentSpecies, currentAge);
                string prefix = Form1.GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
                GEOM[] lods = Form1.GetMorphedHeadMeshes(rm, prefix, rig, morphBGEO.ToArray(), morphShape.ToArray(), morphNormals.ToArray(), morphBOND.ToArray());

                if (Form.ModifierKeys == Keys.Shift)
                {
                    MS3D ms3d = new MS3D(lods, Form1.SelectRig(currentAge), 0, new string[] { "lod0Morph", "lod1Morph", "lod2Morph", "lod3Morph" });
                    Form1.WriteMS3D("Save MS3D of morphed head", ms3d, "");
                }
                else
                {
                    OBJ obj = new OBJ(lods, 0, true, new string[] { "lod0Morph", "lod1Morph", "lod2Morph", "lod3Morph" });
                    Form1.WriteOBJFile("Save OBJ of morphed head", obj, "");
                }
            }
        }

        private void SaveBodyMorph_button_Click(object sender, EventArgs e)
        {
            string prefix = Form1.GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            List<GEOM> allMorph = new List<GEOM>();
            allMorph.Add(new GEOM(CurrentBodyMorph));
            List<string> allNames = new List<string>();
            allNames.Add(prefix + "MorphedBody");
            if (currentSpecies == Species.Human)
            {
                allMorph[0].AppendMesh(CurrentHeadMorph);
                if (Form.ModifierKeys == Keys.Control)
                {
                    Form1.WriteGEOM("Save GEOM of morphed model", allMorph[0]);
                    return;
                }
            }
            else
            {
                if (Form.ModifierKeys == Keys.Control)
                {
                    allMorph[0].AppendMesh(CurrentEarsMorph);
                    allMorph[0].AppendMesh(CurrentTailMorph);
                    Form1.WriteGEOM("Save GEOM of morphed model", allMorph[0]);
                    return;
                }
                string[] ears = new string[] { "Ears", "EarsDown" };
                allMorph.AddRange(Form1.GetMorphedBodyMeshes(rm, prefix, ears, morphShape, morphNormals));
                allNames.AddRange(ears);
                string[] tails = (currentSpecies == Species.Cat) ? new string[] { "Tail", "TailStub" } : 
                    new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
                allMorph.AddRange(Form1.GetMorphedBodyMeshes(rm, prefix, tails, morphShape, morphNormals));
                allNames.AddRange(tails);
            }

            if (Form.ModifierKeys == Keys.Shift)
            {
                MS3D ms3d = new MS3D(allMorph.ToArray(), Form1.SelectRig(currentAge, currentSpecies), 1, allNames.ToArray());
                Form1.WriteMS3D("Save MS3D of morphed model", ms3d, "");
            }
            else
            {
                OBJ obj = new OBJ(allMorph.ToArray(), 1, false, allNames.ToArray());
                Form1.WriteOBJFile("Save OBJ of morphed model", obj, "");
            }
        }
    }
}
