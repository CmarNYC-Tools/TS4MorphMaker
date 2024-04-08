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
using s4pi.ImageResource;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        string version = "4.5.2.0";
        static string Meshfilter = "MS3D or OBJ files (*.ms3d; *.obj)|*.ms3d; *.obj|All files (*.*)|*.*";
        static string OBJfilter = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*";
        static string DMapfilter = "DMap files (*.dmap; *.deformermap)|*.dmap; *.deformermap|All files (*.*)|*.*";
        static string Bitmapfilter = "Bitmap files (*.bmp)|*.bmp|All files (*.*)|*.*";
        static string DDSfilter = "DDS files (*.dds)|*.dds|All files (*.*)|*.*";
        static string Imagefilter = "DDS files (*.dds)|*.dds|PNG files (*.png)|*.png|All files (*.*)|*.*";
        static string Pngfilter = "PNG files (*.png)|*.png|All files (*.*)|*.*";
        static string BGEOfilter = "BGEO files (*.blendgeom; *.bgeo)|*.blendgeom;*.bgeo|All files (*.*)|*.*";
        static string GEOMfilter = "GEOM files (*.simgeom; *.geom)|*.simgeom;*.geom|All files (*.*)|*.*";
        static string MS3Dfilter = "Milkshape MS3D files (*.ms3d)|*.ms3d|All files (*.*)|*.*";
        static string BONDfilter = "BoneDelta/SlotAdjust/BonePose files (*.bonedelta)|*.bonedelta|All files (*.*)|*.*";
        static string RIGfilter = "RIG files (*.rig, *._rig, *.grannyrig)|*.*rig|All files (*.*)|*.*";
        static string Packagefilter = "Package files (*.package)|*.package|All files (*.*)|*.*";
        static RIG adultRig;
        static RIG childRig;
        static RIG toddlerRig;
        static RIG infantRig;
        static RIG adultCatRig;
        static RIG adultDogRig;
        static RIG adultLittleDogRig;
        static RIG childCatRig;
        static RIG childDogRig;
        List<RIG> rigsList;
        string[] rigNames;

        Dictionary<uint, string> boneHashDict = new Dictionary<uint,string>();
        SortedDictionary<string, uint> boneNameDict = new SortedDictionary<string, uint>();
        Dictionary<ulong, string> hotcDictEA = new Dictionary<ulong, string>();
        Dictionary<ulong, string> smodDictEA = new Dictionary<ulong, string>();
        Dictionary<ulong, string> dmapDictEA = new Dictionary<ulong, string>();
        Dictionary<ulong, string> sculptDictEA = new Dictionary<ulong, string>();
        Dictionary<ulong, string> presetDictEA = new Dictionary<ulong, string>();
        List<Package> gamePackages = new List<Package>();
        List<Package> gameThumbPacks = null;
        Random ran = new Random(unchecked((int)DateTime.Now.Ticks));

        public Form1()
        {
            InitializeComponent();
            this.Text = "TS4 MorphMaker version " + version;

            while (!DetectFilePaths())
            {
                DialogResult res = MessageBox.Show("Can't find game and/or user files. Do you want to set them manually?", "Files not found", MessageBoxButtons.RetryCancel);
                if (res == DialogResult.Cancel) break;
                Form f = new PathsPrompt(Properties.Settings.Default.TS4Path, Properties.Settings.Default.TS4UserPath);
                f.ShowDialog();
            }

            string[] thumbs = null;
            string[] localthumbs = null;

            try
            {
                string TS4FilesPath = Properties.Settings.Default.TS4Path;
                //   string TS4FilesPath = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Maxis\\The Sims 4", "Install Dir", null);
                string[] paths = Directory.GetFiles(TS4FilesPath, "*Build*.package", SearchOption.AllDirectories);
                if (paths.Length == 0)
                {
                    MessageBox.Show("Can't find game packages: can't clone or search game files!");
                }
                else
                {
                    foreach (string s in paths)
                    {
                        Package p = OpenPackage(s, false);
                        if (p == null)
                        {
                            MessageBox.Show("Can't read package: " + s);
                        }
                        else
                        {
                            gamePackages.Add(p);
                        }
                    }
                }
                thumbs = Directory.GetFiles(TS4FilesPath, "thumbnails.package", SearchOption.AllDirectories);
            }
            catch
            {
                MessageBox.Show("Can't find game packages: can't clone or search game files!");
            }

            //string tmp = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\The Sims 4";
            //if (tmp != null && !Directory.Exists(tmp))
            //{
            //    string[] tmp2 = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts", "localthumbcache.package", SearchOption.AllDirectories);
            //    if (tmp2.Length > 0)
            //    {
            //        tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
            //    }
            //}
            try
            {
      //          localthumbs = Directory.GetFiles(tmp, "localthumbcache.package", SearchOption.TopDirectoryOnly);
                localthumbs = Directory.GetFiles(Properties.Settings.Default.TS4UserPath, "localthumbcache.package", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                MessageBox.Show("The path to your user Sims 4 folder in Documents is invalid! You will not be able to work with game-generated thumbnails.");
            }
            List<string> thumbPaths = new List<string>();
            if (thumbs != null && thumbs.Length > 0) thumbPaths.AddRange(thumbs);
            if (localthumbs != null && localthumbs.Length > 0) thumbPaths.AddRange(localthumbs);
            thumbPaths.Sort();
            gameThumbPacks = new List<Package>();
            for (int i = 0; i < thumbPaths.Count; i++)
            {
                gameThumbPacks.Add(OpenPackage(thumbPaths[i], false));
            }

            Physiques_comboBox.Items.AddRange(Enum.GetNames(typeof(Physiques)));

            morphPreview1.Start_Mesh(CurrentHead, CurrentBody, null, null, null, CurrentSkin, true);
            PreviewSpecies_listBox.SelectedIndex = 0;
            PreviewAgeGender_listBox.SelectedIndex = 4;
            PreviewType1_listBox.SelectedIndex = 1;
            PreviewType2_listBox.SelectedIndex = 0;
            CurrentHeadMorph = new GEOM(CurrentHead);
            CurrentBodyMorph = new GEOM(CurrentBody);

            BoneDeltaPreviewSetup();

            ExportBodySpecies_listBox.SelectedIndex = 0;
            ExportBodyAgeGender_listBox.SelectedIndex = 3;
            ExportBodyType_listBox.SelectedIndex = 0;
            DMapAdult_checkBox.Checked = true;
            DMapFemale_checkBox.Checked = true;
            Physiques_comboBox.SelectedIndex = 9;

            ExportFaceAgeGender_listBox.SelectedIndex = 3;
            adultRig = GetTS4Rig(Species.Human, AgeGender.Adult);
            childRig = GetTS4Rig(Species.Human, AgeGender.Child);
            toddlerRig = GetTS4Rig(Species.Human, AgeGender.Toddler);
            infantRig = GetTS4Rig(Species.Human, AgeGender.Infant);
            adultCatRig = GetTS4Rig(Species.Cat, AgeGender.Adult);
            adultDogRig = GetTS4Rig(Species.Dog, AgeGender.Adult);
            adultLittleDogRig = GetTS4Rig(Species.LittleDog, AgeGender.Adult);
            childCatRig = GetTS4Rig(Species.Cat, AgeGender.Child);
            childDogRig = GetTS4Rig(Species.Dog, AgeGender.Child);
            rigsList = new List<RIG> { adultRig, childRig, toddlerRig, infantRig, adultCatRig, adultDogRig, adultLittleDogRig, childCatRig, childDogRig };
            rigNames = new string[] { "Adult Rig", "Child Rig", "Toddler Rig", "Infant Rig", "Cat Rig", "Dog Rig", "Little Dog Rig", "Kitten Rig", "Puppy Rig" };
            foreach (RIG rig in rigsList)
            {
                foreach (RIG.Bone bone in rig.Bones)
                {
                    if (!boneHashDict.ContainsKey(bone.BoneHash))
                    {
                        boneHashDict.Add(bone.BoneHash, bone.BoneName);
                        boneNameDict.Add(bone.BoneName, bone.BoneHash);
                    }
                }
            }
            BONDRigs_comboBox.Items.AddRange(rigNames);
            BONDRigs_comboBox.SelectedIndex = 0;
            LoadRigBones(adultRig);

            Predicate<IResourceIndexEntry> predHotSpot = r => r.ResourceType == (uint)ResourceTypes.HotSpotControl;
            Predicate<IResourceIndexEntry> predPreset = r => r.ResourceType == (uint)ResourceTypes.CASPreset;
            Predicate<IResourceIndexEntry> predDMap = r => r.ResourceType == (uint)ResourceTypes.DeformerMap;
            Predicate<IResourceIndexEntry> predSMod = r => r.ResourceType == (uint)ResourceTypes.SimModifier;
            Predicate<IResourceIndexEntry> predSculpt = r => r.ResourceType == (uint)ResourceTypes.Sculpt;
            List<IResourceIndexEntry> listHotSpotResource = new List<IResourceIndexEntry>();
            List<IResourceIndexEntry> listPresetResource = new List<IResourceIndexEntry>();
            List<IResourceIndexEntry> listDMapResource = new List<IResourceIndexEntry>();
            List<IResourceIndexEntry> listSModResource = new List<IResourceIndexEntry>();
            List<IResourceIndexEntry> listSculptResource = new List<IResourceIndexEntry>();
            foreach (Package p in gamePackages)
            {
                listHotSpotResource.AddRange(p.FindAll(predHotSpot));
                listPresetResource.AddRange(p.FindAll(predPreset));
                listDMapResource.AddRange(p.FindAll(predDMap));
                listSModResource.AddRange(p.FindAll(predSMod));
                listSculptResource.AddRange(p.FindAll(predSculpt));
            }

            hotcDictEA = ParseTextResourcesList("HotSpotList.txt", listHotSpotResource);
            dmapDictEA = ParseTextResourcesList("DMapList.txt", listDMapResource);
            smodDictEA = ParseTextResourcesList("ModifierList.txt", listSModResource);
            presetDictEA = ParseTextResourcesList("PresetList.txt", listPresetResource);
            foreach (IResourceIndexEntry irie in listSModResource)
            {
                if (!smodDictEA.ContainsKey(irie.Instance)) smodDictEA.Add(irie.Instance, "0x" + irie.Instance.ToString("X16"));
            }
            foreach (IResourceIndexEntry irie in listSculptResource)
            {
                if (!sculptDictEA.ContainsKey(irie.Instance)) sculptDictEA.Add(irie.Instance, "0x" + irie.Instance.ToString("X16"));
            }

            MorphTreeSetup();
            SetupControlsEditors();
            SetupMorphsEditors();
        }

        private bool DetectFilePaths()
        {
            try
            {
                if (String.Compare(Properties.Settings.Default.TS4Path, " ") <= 0)
                {
                    string tmp = (string)Microsoft.Win32.Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Maxis\\The Sims 4", "Install Dir", null);
                    if (tmp != null) Properties.Settings.Default.TS4Path = tmp;
                    //MessageBox.Show(tmp);
                    Properties.Settings.Default.Save();
                }
                if (String.Compare(Properties.Settings.Default.TS4UserPath, " ") <= 0)
                {
                    string tmp = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts\\The Sims 4";
                    if (tmp != null)
                    {
                        if (!Directory.Exists(tmp))
                        {
                            string[] tmp2 = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Personal) + "\\Electronic Arts", "localthumbcache.package", SearchOption.AllDirectories);
                            if (tmp2.Length > 0)
                            {
                                tmp = Path.GetDirectoryName(tmp2[0]) + Path.DirectorySeparatorChar;
                            }
                        }
                        Properties.Settings.Default.TS4UserPath = tmp;
                    }
                    //MessageBox.Show(tmp);
                    Properties.Settings.Default.Save();
                    if (Properties.Settings.Default.TS4Path == null | Properties.Settings.Default.TS4UserPath == null) return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return (Directory.Exists(Properties.Settings.Default.TS4Path) & Directory.Exists(Properties.Settings.Default.TS4UserPath));
        }

        public static RIG GetTS4Rig(Species species, AgeGender age)
        {
            BinaryReader br = null;
            RIG rig = null;
            string path = "";
            if (species == Species.Human)
            {
                path = Application.StartupPath + "\\S4_" + Enum.GetName(typeof(AgeGender), age) + "_RIG.grannyrig";
            }
            else if (species == Species.Werewolf)
            {
                path = Application.StartupPath + "\\S4_Werewolf_RIG.grannyrig";
            }
            else
            {
                path = Application.StartupPath + "\\S4_" + Enum.GetName(typeof(AgeGender), age) + Enum.GetName(typeof(Species), species) + "_RIG.grannyrig";
            }
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
                MessageBox.Show("Can't open " + age.ToString() + species.ToString() + "RIG file!");
                return null;
            }
            return rig;
        }

        internal static RIG SelectRig(AgeGender age)
        {
            return SelectRig(age, Species.Human);
        }

        internal static RIG SelectRig(AgeGender age, Species species)
        {
            if (species == Species.Cat)
            {
                if (age == AgeGender.Child) return childCatRig;
                else return adultCatRig;
            }
            else if (species == Species.Dog)
            {
                if (age == AgeGender.Child) return childDogRig;
                else return adultDogRig;
            }
            else if (species == Species.LittleDog)
            {
                if (age == AgeGender.Child) return childDogRig;
                else return adultLittleDogRig;
            }
            else
            {
                if (age == AgeGender.Infant) return infantRig;
                else if (age == AgeGender.Toddler) return toddlerRig;
                else if (age == AgeGender.Child) return childRig;
                else return adultRig;
            }
        }

        internal string GetFilename(string title, string filter)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = filter;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.CheckFileExists = true;
            openFileDialog1.Title = title;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool GetOBJData(string file, out OBJ outOBJ)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            OBJ newOBJ = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        MemoryStream ms = new MemoryStream();
                        myStream.Position = 0;
                        myStream.CopyTo(ms);
                        ms.Position = 0;
                        StreamReader sr = new StreamReader(ms);
                        newOBJ = new OBJ(sr);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                outOBJ = newOBJ;
                return false;
            }
            outOBJ = newOBJ;
            return true;
        }

        internal static string WriteOBJFile(string title, OBJ myOBJ, string defaultFilename)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = OBJfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "obj";
            saveFileDialog1.OverwritePrompt = true;
            if (String.Compare(defaultFilename, " ") > 0) saveFileDialog1.FileName = defaultFilename;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            StreamWriter sw = new StreamWriter(myStream);
                            myOBJ.Write(sw);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool ReadDMap(string file, out DMap dmap)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            dmap = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        BinaryReader br = new BinaryReader(myStream);
                        dmap = new DMap(br);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal string WriteDMap(string title, DMap dmap, string defaultFilename)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = DMapfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            if (String.Compare(defaultFilename, " ") > 0) saveFileDialog1.FileName = defaultFilename;
            saveFileDialog1.DefaultExt = "deformermap";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            BinaryWriter bw = new BinaryWriter(myStream);
                            dmap.Write(bw);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool ReadMS3D(string file, out MS3D ms3d)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            ms3d = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        BinaryReader br = new BinaryReader(myStream);
                        ms3d = new MS3D(br);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal static string WriteMS3D(string title, MS3D ms3d, string defaultFilename)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = MS3Dfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            if (String.Compare(defaultFilename, " ") > 0) saveFileDialog1.FileName = defaultFilename;
            saveFileDialog1.DefaultExt = "ms3d";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                if ((myStream = saveFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        BinaryWriter bw = new BinaryWriter(myStream);
                        ms3d.Write(bw);
                    }
                    myStream.Close();
                }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool ReadGEOM(string file, out GEOM geom)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            geom = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        BinaryReader br = new BinaryReader(myStream);
                        geom = new GEOM(br);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal static string WriteGEOM(string title, GEOM geom)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = GEOMfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "simgeom";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            BinaryWriter bw = new BinaryWriter(myStream);
                            geom.WriteFile(bw);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal void WriteBitmap(string title, Bitmap map)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = Bitmapfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "bmp";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    map.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }
            }
        }

        internal void WriteDDS(string title, DSTResource dst, string defaultFilename)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = defaultFilename;
            saveFileDialog1.Filter = DDSfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "dds";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream myStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                    {
                        dst.ToDDS().CopyTo(myStream);
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }
            }
        }

        internal void WriteDDS(string title, RLEResource rle, string defaultFilename)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = defaultFilename;
            saveFileDialog1.Filter = DDSfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "dds";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream myStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                    {
                        rle.ToDDS().CopyTo(myStream);
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }
            }
        }

        internal void WriteImage(string title, LRLE lrle, string defaultFilename)     //Save texture in png
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = defaultFilename;
            saveFileDialog1.Filter = Pngfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (FileStream myStream = new FileStream(saveFileDialog1.FileName, FileMode.Create, FileAccess.Write))
                    {
                        lrle.image.Save(myStream, ImageFormat.Png);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }
            }
        }

        internal bool GetBgeoData(string file, out BGEO bgeo)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            BGEO newBGEO = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        MemoryStream ms = new MemoryStream();
                        myStream.Position = 0;
                        myStream.CopyTo(ms);
                        ms.Position = 0;
                        BinaryReader br = new BinaryReader(ms);
                        newBGEO = new BGEO(br);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                bgeo = newBGEO;
                return false;
            }
            bgeo = newBGEO;
            return true;
        }

        internal string WriteBgeoFile(string title, BGEO bgeo, string filename)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = BGEOfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.FileName = filename;
            saveFileDialog1.DefaultExt = "blendgeom";
            saveFileDialog1.OverwritePrompt = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            BinaryWriter bw = new BinaryWriter(myStream);
                            bgeo.Write(bw);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool GetBONDData(string file, out BOND bond)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            BOND newBond = null;
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        BinaryReader br = new BinaryReader(myStream);
                        myStream.Position = 0;
                        newBond = new BOND(br);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                bond = null;
                return false;
            }
            bond = newBond;
            return true;
        }

        internal string WriteBONDFile(string title, BOND bond, string defaultFilename)
        {
            Stream myStream = null;
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = BONDfilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "bonedelta";
            saveFileDialog1.OverwritePrompt = true;
            if (String.CompareOrdinal(defaultFilename, " ") > 0) saveFileDialog1.FileName = defaultFilename;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = saveFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            BinaryWriter br = new BinaryWriter(myStream);
                            bond.Write(br);
                        }
                        myStream.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    myStream.Close();
                }
                return saveFileDialog1.FileName;
            }
            else
            {
                return "";
            }
        }

        internal bool ReadTexture(string file, out LRLE lrle)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            lrle = new LRLE();
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        Bitmap img = new Bitmap(myStream);
                        lrle = new LRLE(img);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal bool ReadDST(string file, out DSTResource dst)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            dst = new DSTResource(1, null);
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        dst.ImportToDST(myStream);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal bool ReadSpecular(string file, out RLEResource rles)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = file;
            rles = new RLEResource(1, null);
            try
            {
                if ((myStream = openFileDialog1.OpenFile()) != null)
                {
                    using (myStream)
                    {
                        rles.ImportToRLE(myStream, RLEResource.RLEVersion.RLES);
                    }
                    myStream.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file " + openFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                myStream.Close();
                return false;
            }
            return true;
        }

        internal Package OpenPackage(string packagePath, bool readwrite)
        {
            try
            {
                Package package = (Package)Package.OpenPackage(0, packagePath, readwrite);
                return package;
            }
            catch
            {
                MessageBox.Show("Unable to read valid package data from " + packagePath);
                return null;
            }
        }

        internal bool WritePackage(string title, Package pack, string defaultFilename)
        {
            string tmp;
            return WritePackage(title, pack, defaultFilename, out tmp);
        }

        internal bool WritePackage(string title, Package pack, string defaultFilename, out string newFilename)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = Packagefilter;
            saveFileDialog1.FilterIndex = 1;
            saveFileDialog1.Title = title;
            saveFileDialog1.AddExtension = true;
            saveFileDialog1.CheckPathExists = true;
            saveFileDialog1.DefaultExt = "package";
            saveFileDialog1.OverwritePrompt = true;
            if (String.CompareOrdinal(defaultFilename, " ") > 0) saveFileDialog1.FileName = defaultFilename;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    pack.SaveAs(saveFileDialog1.FileName);
                    newFilename = saveFileDialog1.FileName;
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not write file " + saveFileDialog1.FileName + ". Original error: " + ex.Message + Environment.NewLine + ex.StackTrace.ToString());
                    newFilename = saveFileDialog1.FileName;
                    return false;
                }
            }
            newFilename = "";
            return false;
        }

        private void hashStripMenuItem_Click(object sender, EventArgs e)
        {
            FNVhashForm fnv = new FNVhashForm();
            fnv.Show();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TS MorphMaker V" + version +", by cmar" + Environment.NewLine + "Freeware available from modthesims.info");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setupStripMenuItem_Click(object sender, EventArgs e)
        {
            Form f = new PathsPrompt(Properties.Settings.Default.TS4Path, Properties.Settings.Default.TS4UserPath);
            f.ShowDialog();
        }

        public static double[] ArrayToDouble(float[] array)
        {
            double[] result = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i];
            }
            return result;
        }
    }
}
