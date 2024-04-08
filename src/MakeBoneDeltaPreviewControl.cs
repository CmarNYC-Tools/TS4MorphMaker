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
using System.Resources;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        GEOM bondHead = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod0)));
        GEOM bondHeadMorph = null;
        GEOM bondBody = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfBodyComplete_lod0)));
        GEOM bondBodyMorph = null;
        GEOM bondEars, bondTail;
        GEOM bondEarsMorph, bondTailMorph;
        Image bondSkin = Properties.Resources.FemaleSkinTan;
        Species bondSpecies = Species.Human;
        AgeGender bondAge = AgeGender.Adult;
        AgeGender bondGender = AgeGender.Female;

        internal class AgeGenderSpecies
        {
            internal AgeGender age;
            internal AgeGender gender;
            internal Species species;
            internal AgeGenderSpecies(AgeGender age, AgeGender gender, Species species)
            {
                this.age = age;
                this.gender = gender;
                this.species = species;
            }
            internal AgeGenderSpecies(AgeGender age, Species species)
            {
                this.age = age;
                this.gender = AgeGender.Unisex;
                this.species = species;
            }
            public override string ToString()
            {
                return (species == Species.Human) ? (age > AgeGender.Child && age <= AgeGender.Elder ? gender.ToString() : age.ToString()) :
                    (age > AgeGender.Child ? species.ToString() : (species == Species.Horse? "Foal" : species == Species.Cat ? "Kitten" : "Puppy"));
            }
        }

        private void BoneDeltaPreviewSetup()
        {
            object[] models = new object[] { new AgeGenderSpecies(AgeGender.Adult, AgeGender.Male, Species.Human), 
                new AgeGenderSpecies(AgeGender.Adult, AgeGender.Female, Species.Human), new AgeGenderSpecies(AgeGender.Child, AgeGender.Unisex, Species.Human),
                new AgeGenderSpecies(AgeGender.Toddler, AgeGender.Unisex, Species.Human), new AgeGenderSpecies(AgeGender.Infant, AgeGender.Unisex, Species.Human), 
                new AgeGenderSpecies(AgeGender.Adult, AgeGender.Unisex, Species.Cat),
                new AgeGenderSpecies(AgeGender.Adult, AgeGender.Unisex, Species.Dog), new AgeGenderSpecies(AgeGender.Adult, AgeGender.Unisex, Species.LittleDog),
                new AgeGenderSpecies(AgeGender.Adult, AgeGender.Unisex, Species.Horse),
                new AgeGenderSpecies(AgeGender.Child, AgeGender.Unisex, Species.Cat), new AgeGenderSpecies(AgeGender.Child, AgeGender.Unisex, Species.Dog), new AgeGenderSpecies(AgeGender.Child, AgeGender.Unisex, Species.Horse) };
            BONDmodel_comboBox.Items.AddRange(models);
            BONDmodel_comboBox.SelectedIndex = 1;
            BONDmodelEars_comboBox.Items.AddRange(new object[] { EarType.Up, EarType.Down });
            BONDmodelEars_comboBox.SelectedIndex = 0;
            BONDmodelEars_comboBox.Enabled = false;
            BONDmodelTail_comboBox.Items.AddRange(new object[] { TailType.Long, TailType.Ring, TailType.Screw, TailType.Stub });
            BONDmodelTail_comboBox.SelectedIndex = 0;
            BONDmodelTail_comboBox.Enabled = false;
            morphPreview2.Start_Mesh(bondHead, bondBody, bondEars, bondTail, null, bondSkin, true);
        }

        private void BoneDeltaPreviewUpdate(bool reset)
        {
            currentRig = new RIG(rigsList[BONDRigs_comboBox.SelectedIndex]);
            bondHeadMorph = LoadBONDMorph(bondHead, currentBOND, currentRig);
            bondBodyMorph = LoadBONDMorph(bondBody, currentBOND, currentRig);
            bondEarsMorph = LoadBONDMorph(bondEars, currentBOND, currentRig);
            bondTailMorph = LoadBONDMorph(bondTail, currentBOND, currentRig);
            morphPreview2.Stop_Mesh();
            morphPreview2.Start_Mesh(bondHeadMorph, bondBodyMorph, bondEarsMorph, bondTailMorph, null, bondSkin, reset);
        }

        private void BONDmodel_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            BONDset_AgeGenderSpecies();
        }

        private void BONDmodelEars_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwapPetParts();
        }

        private void BONDmodelTail_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            SwapPetParts();
        }

        private void BONDset_AgeGenderSpecies()
        {
            AgeGenderSpecies ags = (AgeGenderSpecies) BONDmodel_comboBox.SelectedItem;
            bondSpecies = ags.species;
            bondAge = ags.age;
            bondGender = ags.gender;

            string prefix = GetBodyCompletePrefix(bondSpecies, bondAge, bondGender);
            if (bondSpecies == Species.Human)
            {
                bondHead = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0"))));
                bondBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete" + "_lod0"))));
                bondEars = null;
                bondTail = null;
                bondSkin = (bondAge == AgeGender.Adult) ? 
                    (bondGender == AgeGender.Male ? Properties.Resources.MaleSkinTan : Properties.Resources.FemaleSkinTan) : 
                    (bondAge == AgeGender.Child ? Properties.Resources.ChildSkinTan : Properties.Resources.ToddlerSkinTan);
                BONDmodelEars_comboBox.Enabled = false;
                BONDmodelTail_comboBox.Enabled = false;
                if (bondAge > AgeGender.Child) currentModelGender = bondGender;
            }
            else
            {
                bondHead = null;
                bondBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                if(bondSpecies != Species.Horse){
                     BONDmodelEars_comboBox.Enabled = true;
                     string earType = BONDmodelEars_comboBox.SelectedIndex == 0 ? "Ears" : "EarsDown";
                     bondEars = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + earType + "_lod0"))));
                }else{
                    bondEars = null;
                }
                BONDmodelTail_comboBox.Items.Clear();
                BONDmodelTail_comboBox.Items.AddRange(
                    bondSpecies == Species.Horse? new object[]{ TailType.Long}:
                    bondSpecies == Species.Cat ? new object[] { TailType.Long, TailType.Stub } :
                                                        new object[] { TailType.Long, TailType.Stub, TailType.Ring, TailType.Screw });
                if (BONDmodelTail_comboBox.SelectedIndex < 0 || BONDmodelTail_comboBox.SelectedIndex > BONDmodelTail_comboBox.Items.Count) BONDmodelTail_comboBox.SelectedIndex = 0;
                BONDmodelTail_comboBox.Enabled = true;

                string[] tailType = bondSpecies == Species.Horse ? new string[] { "Tail" } : bondSpecies == Species.Cat ? new string[] { "Tail", "TailStub" } : new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
                bondTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[BONDmodelTail_comboBox.SelectedIndex] + "_lod0"))));
                bondSkin = bondSpecies == Species.Horse ?  Properties.Resources.HorseSkin :
                    bondSpecies == Species.Cat ? Properties.Resources.CatSkin : Properties.Resources.DogSkin;
            }

            if (bondHead != null) bondHeadMorph = new GEOM(bondHead);
            else bondHeadMorph = null;
            bondBodyMorph = new GEOM(bondBody);
            if (bondEars != null) bondEarsMorph = new GEOM(bondEars);
            else bondEarsMorph = null;
            if (bondTail != null) bondTailMorph = new GEOM(bondTail);
            else bondTailMorph = null;
            if (currentBOND != null)
            {
                bondHeadMorph = LoadBONDMorph(bondHeadMorph, currentBOND, currentRig);
                bondBodyMorph = LoadBONDMorph(bondBodyMorph, currentBOND, currentRig);
                bondEarsMorph = LoadBONDMorph(bondEarsMorph, currentBOND, currentRig);
                bondTailMorph = LoadBONDMorph(bondTailMorph, currentBOND, currentRig);
            }
            morphPreview2.Stop_Mesh();
            morphPreview2.Start_Mesh(bondHeadMorph, bondBodyMorph, bondEarsMorph, bondTailMorph, null, bondSkin, true);
        }

        private void SwapPetParts()
        {
            if (bondSpecies == Species.Human) return;
            string prefix = GetBodyCompletePrefix(bondSpecies, bondAge, bondGender);
            if(bondSpecies != Species.Horse){
                string earType = BONDmodelEars_comboBox.SelectedIndex == 0 ? "Ears" : "EarsDown";
                bondEars = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + earType + "_lod0"))));
            }
            string[] tailType = new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
            bondTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[BONDmodelTail_comboBox.SelectedIndex] + "_lod0"))));

            if (bondEars != null) bondEarsMorph = new GEOM(bondEars);
            else bondEarsMorph = null;
            if (bondTail != null) bondTailMorph = new GEOM(bondTail);
            else bondTailMorph = null;
            currentRig = new RIG(rigsList[BONDRigs_comboBox.SelectedIndex]);
            if (currentBOND != null)
            {
                bondHeadMorph = LoadBONDMorph(bondHeadMorph, currentBOND, currentRig);
                bondBodyMorph = LoadBONDMorph(bondBodyMorph, currentBOND, currentRig);
                bondEarsMorph = LoadBONDMorph(bondEarsMorph, currentBOND, currentRig);
                bondTailMorph = LoadBONDMorph(bondTailMorph, currentBOND, currentRig);
            }
            morphPreview2.Stop_Mesh();
            morphPreview2.Start_Mesh(bondHeadMorph, bondBodyMorph, bondEarsMorph, bondTailMorph, null, bondSkin, true);
        }

        private void BONDshowMorph_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowStuff();
        }

        private void BONDshowSkin_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            ShowStuff();
        }

        private void ShowStuff()
        {
            morphPreview2.Stop_Mesh();
            if (BONDshowMorph_checkBox.Checked)
            {
                if (BONDshowSkin_checkBox.Checked)
                {
                    morphPreview2.Start_Mesh(bondHeadMorph, bondBodyMorph, bondEarsMorph, bondTailMorph, null, bondSkin, false);
                }
                else
                {
                    morphPreview2.Start_Mesh(bondHeadMorph, bondBodyMorph, bondEarsMorph, bondTailMorph, null, null, false);
                }
            }
            else
            {
                if (BONDshowSkin_checkBox.Checked)
                {
                    morphPreview2.Start_Mesh(bondHead, bondBody, bondEars, bondTail, null, bondSkin, false);
                }
                else
                {
                    morphPreview2.Start_Mesh(bondHead, bondBody, bondEars, bondTail, null, null, false);
                }
            }
        }
    }
}
