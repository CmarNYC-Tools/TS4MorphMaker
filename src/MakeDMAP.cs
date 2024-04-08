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
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        DateTime time;
        BackgroundWorker bgWorker;

        private void ExportBodyGo_button_Click(object sender, EventArgs e)
        {
            GEOM[] tmp;
            Species species = (Species)(ExportBodySpecies_listBox.SelectedIndex + 1);
            AgeGender age;
            if (species == Species.Human)
            {
                if (ExportBodyAgeGender_listBox.SelectedIndex == 0) age = AgeGender.Infant;
                else if (ExportBodyAgeGender_listBox.SelectedIndex == 1) age = AgeGender.Toddler;
                else if (ExportBodyAgeGender_listBox.SelectedIndex == 2) age = AgeGender.Child;
                else age = AgeGender.Adult;
            }
            else if (species == Species.Werewolf)
            {
                age = AgeGender.Adult;
            }
            else
            {
                if (ExportBodyAgeGender_listBox.SelectedIndex == 0) age = AgeGender.Child;
                else age = AgeGender.Adult;
            }
            AgeGender gender;
            if (species == Species.Human && age == AgeGender.Adult)
            {
                if (ExportBodyAgeGender_listBox.SelectedIndex == 3) gender = AgeGender.Male;
                else gender = AgeGender.Female;
            }
            else if (species == Species.Werewolf)
            {
                if (ExportBodyAgeGender_listBox.SelectedIndex == 0) gender = AgeGender.Male;
                else gender = AgeGender.Female;
            }
            else
            {
                gender = AgeGender.Unisex;
            }
            string prefix = GetBodyCompletePrefix(species, age, gender);
            string[] groupnames;

            System.Resources.ResourceManager rm = Properties.Resources.ResourceManager;
            if (species == Species.Human)
            {
                tmp = new GEOM[1];
                string bodyType = (age == AgeGender.Adult && ExportBodyType_listBox.SelectedIndex == 1) ? "Robe" : "Complete";
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Body" + bodyType + "_lod0"))));
                tmp[0].AppendMesh(new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0")))));
                groupnames = new string[] { prefix + (ExportBodyType_listBox.SelectedIndex == 1 ? "Robe" : "Body") };
            }
            else if (species == Species.Werewolf)
            {
                tmp = new GEOM[1];
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                tmp[0].AppendMesh(new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0")))));
                groupnames = new string[] { prefix + "Body" };
            }
            else
            {
                if (species == Species.Cat)  groupnames = new string[] { prefix + "Body", "Ears", "EarsDown", "Tail", "TailStub" };
                else groupnames = new string[] { prefix + "Body", "Ears", "EarsDown", "Tail", "TailStub", "TailRing", "TailScrew" }; 
                tmp = new GEOM[groupnames.Length];
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                for (int i = 1; i < groupnames.Length; i++)
                {
                    tmp[i] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + groupnames[i] + "_lod0"))));
                }
            }

            OBJ obj = new OBJ(tmp, 1, false, groupnames);
            WriteOBJFile("Save Base OBJ mesh", obj, "");
        }

        public static string GetBodyCompletePrefix(Species species, AgeGender age, AgeGender gender)
        {
            string specifier = "";
            if (age == AgeGender.Infant) specifier = (species == Species.Human ? "i" : "c");
            else if (age == AgeGender.Toddler) specifier = (species == Species.Human ? "p" : "c");
            else if (age == AgeGender.Child) specifier = "c";
            else specifier = ((species == Species.Human || species == Species.Werewolf) ? "y" : "a");
            if (species == Species.Werewolf) specifier += ((gender & AgeGender.Male) > 0 ? "m" : "f") + "w";
            else if (species != Species.Human) specifier +=
                (age == AgeGender.Child && species == Species.LittleDog) ? "d" :
                species.ToString().Substring(0, 1).ToLower();
            else if (age <= AgeGender.Child || age == AgeGender.Infant) specifier += "u";
            else specifier += (gender == AgeGender.Male || gender == AgeGender.Female) ? gender.ToString().Substring(0, 1).ToLower() : "m";
            return specifier;
        }

        private void ExportBodyType_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExportBodyType_listBox.SelectedIndex == 1)
            {
                if (ExportBodyAgeGender_listBox.SelectedIndex <= 2)
                {
                    MessageBox.Show("There is no robe base mesh for infants, toddlers or children." + Environment.NewLine + 
                        "Please use your own or ask for one to be included.");
                    ExportBodyType_listBox.SelectedIndex = 0;
                }
            }
        }

        private void ExportBodySpecies_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ExportBodySpecies_listBox.SelectedIndex == 0)
            {
                ExportBodyAgeGender_listBox.Items.Clear();
                ExportBodyAgeGender_listBox.Items.AddRange(new string[] { "Infant", "Toddler", "Child", "Adult Male", "Adult Female" });
                ExportBodyAgeGender_listBox.SelectedIndex = 3;
                ExportBodyType_listBox.Visible = true;
                ExportBodyType_listBox.Items.Clear();
                ExportBodyType_listBox.Items.AddRange(new string[] { "Skintight", "Robe" });
                ExportBodyType_listBox.SelectedIndex = 0;
            }
            else
            {
                ExportBodyAgeGender_listBox.Items.Clear();
                ExportBodyType_listBox.Visible = false;
                if (ExportBodySpecies_listBox.SelectedIndex == 2) // cat
                {
                    ExportBodyAgeGender_listBox.Items.AddRange(new string[] { "Kitten", "Adult" });
                }
                else if (ExportBodySpecies_listBox.SelectedIndex == 1 || ExportBodySpecies_listBox.SelectedIndex == 3) //dog, little dog
                {
                    ExportBodyAgeGender_listBox.Items.AddRange(new string[] { "Puppy", "Adult" });
                }
                else if (ExportBodySpecies_listBox.SelectedIndex == 4) // werewolf
                {
                    ExportBodyAgeGender_listBox.Items.AddRange(new string[] { "Adult Male", "Adult Female" });
                }
                ExportBodyAgeGender_listBox.SelectedIndex = 1;
            }
        }

        private void ImportSkinBase_button_Click(object sender, EventArgs e)
        {
            ImportSkinBase.Text = GetFilename("Select base (unchanged) skintight OBJ mesh", OBJfilter);
        }

        private void ImportSkinMorph_button_Click(object sender, EventArgs e)
        {
            ImportSkinMorph.Text = GetFilename("Select morph (edited) skintight OBJ mesh", OBJfilter);
        }

        private void ImportRobeBase_button_Click(object sender, EventArgs e)
        {
            ImportRobeBase.Text = GetFilename("Select base (unchanged) robe OBJ mesh", OBJfilter);
        }

        private void ImportRobeMorph_button_Click(object sender, EventArgs e)
        {
            ImportRobeMorph.Text = GetFilename("Select morph (edited) robetight OBJ mesh", OBJfilter);
        }

        private void ImportRobeBase_TextChanged(object sender, EventArgs e)
        {
            DMapCopySkin_checkBox.Enabled = (string.Compare(ImportRobeBase.Text, " ") <= 0);
        }

        private void DmapHuman_RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            Set_DMapSpecies();
        }

        private void DMapDog_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Set_DMapSpecies();
        }

        private void DMapCat_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Set_DMapSpecies();
        }

        private void DMapLittleDog_radioButton_CheckedChanged(object sender, EventArgs e)
        {
            Set_DMapSpecies();
        }
        
        private void Set_DMapSpecies()
        {
            if (DmapHuman_RadioButton.Checked)
            {
                DMapInfant_checkBox.Enabled = true;
                DMapToddler_checkBox.Enabled = true;
                DMapChild_checkBox.Text = "Child";
            }
            else
            {
                DMapInfant_checkBox.Checked = false;
                DMapInfant_checkBox.Enabled = false;
                DMapToddler_checkBox.Checked = false;
                DMapToddler_checkBox.Enabled = false;
                DMapMale_checkBox.Checked = true;
                DMapFemale_checkBox.Checked = true;
                if (DMapCat_radioButton.Checked) DMapChild_checkBox.Text = "Kitten";
                else DMapChild_checkBox.Text = "Puppy";
            }
        }

        private void DMapGo_button_Click(object sender, EventArgs e)
        {
            if ((string.Compare(ImportSkinBase.Text, " ") <= 0) || (string.Compare(ImportSkinMorph.Text, " ") <= 0))
            {
                MessageBox.Show("You must select both a base and morph skintight mesh!");
                return;
            }
            bool gotRobe = (string.Compare(ImportRobeBase.Text, " ") > 0) || (string.Compare(ImportRobeMorph.Text, " ") > 0);
            if (gotRobe)
            {
                if ((string.Compare(ImportRobeBase.Text, " ") <= 0) || (string.Compare(ImportRobeMorph.Text, " ") <= 0))
                {
                    MessageBox.Show("If you include a robe morph, you must select both a base and morph mesh!");
                    return;
                }
            }

            OBJ baseSkinMesh = null;
            OBJ morphSkinMesh = null;
            OBJ baseRobeMesh = null;
            OBJ morphRobeMesh = null;
            if (!GetOBJData(ImportSkinBase.Text, out baseSkinMesh))
            {
                MessageBox.Show("Can't read base OBJ skintight mesh!");
                return;
            }
            if (!GetOBJData(ImportSkinMorph.Text, out morphSkinMesh))
            {
                MessageBox.Show("Can't read morph OBJ skintight mesh!");
                return;
            }
            if (!morphSkinMesh.hasNormals)
            {
                MessageBox.Show("The morphed skintight mesh does not have normals. Please export it with normals.");
                return;
            }
            if (baseSkinMesh.positionArray.Length != morphSkinMesh.positionArray.Length)
            {
                MessageBox.Show("Number of vertices in base and morph skintight meshes do not match!");
                return;
            }

            if (gotRobe)
            {
                if (!GetOBJData(ImportRobeBase.Text, out baseRobeMesh))
                {
                    MessageBox.Show("Can't read base robe OBJ mesh!");
                    return;
                }
                if (!GetOBJData(ImportRobeMorph.Text, out morphRobeMesh))
                {
                    MessageBox.Show("Can't read morph robe OBJ mesh!");
                    return;
                }
                if (!morphRobeMesh.hasNormals)
                {
                    MessageBox.Show("The morphed robe mesh does not have normals. Please export it with normals.");
                    return;
                }
                if (baseRobeMesh.positionArray.Length != morphRobeMesh.positionArray.Length)
                {
                    MessageBox.Show("Number of vertices in base and morph robe meshes do not match!");
                    return;
                }
            }

            time = System.DateTime.Now;

            DMap_progressBar.Visible = true;
            DMap_progressBar.Step = 1;
            DMap_progressBar.Value = 1;
            DMap_progressBar.Maximum = gotRobe ? 24 : 14;
            StatusBar1.Visible = true;
            StatusBar1.Text = "Please wait," + Environment.NewLine + "this may take" + Environment.NewLine + "a long time......." +
                                Environment.NewLine + "Setting Up";

            StatusBar1.Refresh();
            DMapCancel_button.Visible = true;
            DMapGo_button.Enabled = false;
            List<object> argList = new List<object>();
            argList.Add(baseSkinMesh);
            argList.Add(morphSkinMesh);
            argList.Add(baseRobeMesh);
            argList.Add(morphRobeMesh);

            Species species = Species.Human;
            if (DMapDog_radioButton.Checked) species = Species.Dog;
            else if (DMapCat_radioButton.Checked) species = Species.Cat;
            else if (DMapLittleDog_radioButton.Checked) species = Species.LittleDog;
            argList.Add(species);

            AgeGender ageGender = AgeGender.None;
            if (DMapInfant_checkBox.Checked) ageGender = ageGender | AgeGender.Infant;
            if (DMapToddler_checkBox.Checked) ageGender = ageGender | AgeGender.Toddler;
            if (DMapChild_checkBox.Checked) ageGender = ageGender | AgeGender.Child;
            if (DMapAdult_checkBox.Checked) ageGender = ageGender | AgeGender.Teen | AgeGender.YoungAdult | AgeGender.Adult | AgeGender.Elder;
            if (DMapMale_checkBox.Checked) ageGender = ageGender | AgeGender.Male;
            if (DMapFemale_checkBox.Checked) ageGender = ageGender | AgeGender.Female;
            argList.Add(ageGender);

            Physiques physique;
            if (Physiques_comboBox.SelectedIndex == 10)
            {
                physique = Physiques.BODYBLENDTYPE_AVERAGE;
            }
            else
            {
                physique = (Physiques)Physiques_comboBox.SelectedIndex;
            }
            argList.Add(physique);
            //RobeChannel robe = (RobeChannel)RobeOptions_comboBox.SelectedIndex;
            //argList.Add(robe);
            DMapSize size = DMapBodySmall_radioButton.Checked ? DMapSize.Small : DMapSize.Medium;
            //else if (DMapBodyFull_radioButton.Checked)
            //{
            //    size = DMapSize.Large;
            //}
            argList.Add(size);
            bool robeIsCopy = DMapCopySkin_checkBox.Checked;
            argList.Add(robeIsCopy);
            bool ignoreNormals = NoNormalsDmap_checkBox.Checked;
            argList.Add(ignoreNormals);
            bgWorker = new BackgroundWorker();
            bgWorker.WorkerSupportsCancellation = true;
            bgWorker.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            bgWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
            bgWorker.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            bgWorker.RunWorkerAsync(argList);
        }
        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
           // DateTime time = DateTime.Now;
            //Bitmap map = MakeMap(baseSkinMesh, morphSkinMesh);
            //WriteBitmap("Save bitmap image of DMap", map);

            BackgroundWorker worker = sender as BackgroundWorker;
            List<object> argList = e.Argument as List<object>;
            e.Result = MakeMap((OBJ)argList[0], (OBJ)argList[1], (OBJ)argList[2], (OBJ)argList[3], (Species)argList[4], (AgeGender)argList[5], 
                (Physiques)argList[6], (DMapSize)argList[7], (bool)argList[8], (bool)argList[9],
                worker);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 2) StatusBar1.Text += Environment.NewLine + "Processing Skintight Data";
            if (e.ProgressPercentage == 12) StatusBar1.Text += Environment.NewLine + "Processing Robe Data";
            if (e.ProgressPercentage == 22) StatusBar1.Text += Environment.NewLine + "Creating DMaps";
            DMap_progressBar.PerformStep();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else if (e.Cancelled || e.Result == null)
            {
                MessageBox.Show("Cancelled by user");
                StatusBar1.Visible = false;
                DMapCancel_button.Visible = false;
                DMapGo_button.Enabled = true;
                DMap_progressBar.Visible = false;
                bgWorker.Dispose();
            }
            else
            {
                List<DMap> dmap = e.Result as List<DMap>;
                if (CompressDMap_checkBox.Checked)
                {
                    DMap_progressBar.PerformStep();
                    StatusBar1.Text += Environment.NewLine + "Compressing Data...";
                    dmap[0].Compress();
                    dmap[1].Compress();
                }
                StatusBar1.Text = "Completed in: " + ((DateTime.Now - time).ToString()).Substring(0, 8);
                DMapCancel_button.Visible = false;
                DMapGo_button.Enabled = true;
                DMap_progressBar.Visible = false;
                WriteDMap("Save Shape DMap as a file", dmap[0], "");
                WriteDMap("Save Normals DMap as a file", dmap[1], "");
                bgWorker.Dispose();
            }
        }

        private void DMapCancel_button_Click(object sender, EventArgs e)
        {
            bgWorker.CancelAsync();
        }

        private List<DMap> MakeMap(OBJ baseSkin, OBJ morphSkin, OBJ baseRobe, OBJ morphRobe, Species species, AgeGender ageGender, 
            Physiques physique, DMapSize size, bool robeIsCopy, bool ignoreNormals, BackgroundWorker worker)
        {
            int progress = 1;
            worker.WorkerReportsProgress = true;
            worker.ReportProgress(progress);

            float workWidth;
            float workHeight;
            if (species == Species.Human)
            {
                if (size == DMapSize.Small)
                {
                    workWidth = 256;
                    workHeight = 512;
                }
                else if (size == DMapSize.Medium)
                {
                    workWidth = 512;
                    workHeight = 1024;
                }
                else
                {
                    workWidth = 1024;
                    workHeight = 2048;
                }
            }
            else
            {
                if (size == DMapSize.Small)
                {
                    workWidth = 256;
                    workHeight = 256;
                }
                else if (size == DMapSize.Medium)
                {
                    workWidth = 512;
                    workHeight = 512;
                }
                else
                {
                    workWidth = 1024;
                    workHeight = 1024;
                }
            }
            RobeChannel robe;
            Vector3 zero = new Vector3(0f, 0f, 0f);

            OBJ deltaSkin = OBJ.DeltaOBJ(baseSkin, morphSkin);
            OBJ.Point[] vertsSkin = deltaSkin.PointArray;

            uint minCol = (uint)workWidth;
            uint maxCol = 0;
            uint minRow = (uint)workHeight;
            uint maxRow = 0;
            Vector3[][] deltasSkin = null;
            Vector3[][] normalsSkin = null;
            progress = 2;

            if (!GetDeltas(vertsSkin, workWidth, workHeight, worker, progress, out deltasSkin, out normalsSkin, ignoreNormals, 
                    deltaSkin.TrianglesUV1(workWidth, workHeight))) return null;
            for (uint h = 0; h < deltasSkin.Length; h++)
            {
                for (uint w = 0; w < deltasSkin[h].Length; w++)
                {
                    if (deltasSkin[h][w] != zero)
                    {
                        if (w < minCol) minCol = w;
                        if (w > maxCol) maxCol = w;
                        if (h < minRow) minRow = h;
                        if (h > maxRow) maxRow = h;
                    }
                }
            }

            Vector3[][] deltasRobe = null;
            Vector3[][] normalsRobe = null;
            if (baseRobe != null && morphRobe != null)
            {
                OBJ deltaRobe = OBJ.DeltaOBJ(baseRobe, morphRobe);
                OBJ.Point[] vertsRobe = deltaRobe.PointArray;
                progress = 12;
                if (!GetDeltas(vertsRobe, workWidth, workHeight, worker, progress, out deltasRobe, out normalsRobe, ignoreNormals, 
                            deltaRobe.TrianglesUV1(workWidth, workHeight))) return null;

                for (uint h = 0; h < deltasRobe.Length; h++)
                {
                    for (uint w = 0; w < deltasRobe[h].Length; w++)
                    {
                        if (deltasRobe[h][w] != zero)
                        {
                            if (w < minCol) minCol = w;
                            if (w > maxCol) maxCol = w;
                            if (h < minRow) minRow = h;
                            if (h > maxRow) maxRow = h;
                        }
                    }
                }
                robe = RobeChannel.ROBECHANNEL_PRESENT;
            }
            else 
            {
                robe = robeIsCopy ? RobeChannel.ROBECHANNEL_ISCOPY : RobeChannel.ROBECHANNEL_DROPPED;
            }

            progress = 22;
            if (worker.CancellationPending) return null;
            worker.ReportProgress(progress);

            DMap dshape = new DMap(species, ageGender, physique, robe, 
                ShapeOrNormals.SHAPE_DEFORMER, minCol, maxCol, minRow, maxRow, 
                deltasSkin, deltasRobe, true);
            DMap dnorm = new DMap(species, ageGender, physique, robe,
                ShapeOrNormals.NORMALS_DEFORMER, minCol, maxCol, minRow, maxRow, 
                normalsSkin, normalsRobe, true);

            List<DMap> dmaps = new List<DMap>();
            dmaps.Add(dshape);
            dmaps.Add(dnorm);
            return dmaps;
            //Bitmap bmp = new Bitmap(dmap.ToBitMap(DMap.OutputType.Skin));
            //return bmp;
        }

        internal bool GetDeltas(OBJ.Point[] verts, float workWidth, float workHeight, BackgroundWorker worker, int progress,
            out Vector3[][] deltas, out Vector3[][] normals, bool ignoreNormals, List<OBJ.GroupTriangle> facesUV1)
        {
            worker.ReportProgress(progress);
            deltas = new Vector3[(int)workHeight][];
            normals = new Vector3[(int)workHeight][];
            float maxDistance = workWidth / 100f;
            if (worker.CancellationPending) return false;
            //List<DeltaPoint> deltaPoints = new List<DeltaPoint>();
            //for (int i = 0; i < verts.Length; i++)
            //{
            //    float uvX = (verts[i].UV.X * 2f) - 1f;
            //    if (uvX < 0) continue;

            //    int x = (int)(workWidth * uvX);
            //    int y = (int)(workHeight * verts[i].UV.Y);
            //    deltaPoints.Add(new DeltaPoint(x, y, verts[i].Group, verts[i].Position, verts[i].Normal));
            //}

            float minUVx = float.MaxValue;
            float maxUVx = float.MinValue;
            float minUVy = float.MaxValue;
            float maxUVy = 0f;
            Vector3 zero = new Vector3(0f, 0f, 0f);
            foreach (OBJ.Point p in verts)
            {
                if (p.Position != zero || p.Normal != zero)
                {
                    if (p.UV.X < minUVx) minUVx = p.UV.X;
                    if (p.UV.X > maxUVx) maxUVx = p.UV.X;
                    if (p.UV.Y < minUVy) minUVy = p.UV.Y;
                    if (p.UV.Y > maxUVy) maxUVy = p.UV.Y;
                }
            }
            minUVx = Math.Max((((minUVx * 2f) - 1f) * workWidth) - (workWidth / 20f), 0f);
            maxUVx = Math.Min((((maxUVx * 2f) - 1f) * workWidth) + (workWidth / 20f), workWidth);
            minUVy = Math.Max((minUVy * workHeight) - (workHeight / 20f), 0f);
            maxUVy = Math.Min((maxUVy * workHeight) + (workHeight / 20f), workHeight);

            int tmp = (int)workHeight / 9;
            int counter = 0, steps = 1;
            for (int r = 0; r < workHeight; r++)
            {
                counter++;
                if (counter >= tmp && steps < 10)
                {
                    if (worker.CancellationPending) return false;
                    progress++;
                    worker.ReportProgress(progress);
                    counter = 0;
                    steps++;
                }
                deltas[r] = new Vector3[(int)workWidth];
                normals[r] = new Vector3[(int)workWidth];
                for (int c = 0; c < workWidth; c++)
                {
                    if (r < minUVy || r > maxUVy || c < minUVx || c > maxUVx)
                    {
                    //    MessageBox.Show(r.ToString() + " " + minUVy.ToString() + " " + maxUVy.ToString());
                        deltas[r][c] = new Vector3();
                        normals[r][c] = new Vector3();
                        continue;
                    }
                    Vector3[] tmpVectors = PointInOrNearMesh(c, r, facesUV1, maxDistance);
                    if (tmpVectors == null)              //point not inside or near triangles of UV1 points of this mesh
                    {
                        deltas[r][c] = new Vector3();
                        normals[r][c] = new Vector3();
                    }
                    else
                    {
                     //   Vector3[] tmpVectors = GetInterpolatedDeltas(c, r, deltaPoints, ReferencePointsNumber);
                        deltas[r][c] = tmpVectors[0];
                        normals[r][c] = ignoreNormals ? new Vector3() : tmpVectors[1];
                    }
                }
            }
            return true;
        }

        internal Vector3[] PointInOrNearMesh(int w, int h, List<OBJ.GroupTriangle> facesUV1, float maxDistance)
        {
            float x = (float)w;
            float y = (float)h;
            float[] weights;
            float padding = 0.75f;
            float lowval = 0f - padding;
            float highval = 1f + padding;
            float[] tmp = new float[3];
            Vector2 point = new Vector2(x, y);
            List<DeltaPoint> refVerts = new List<DeltaPoint>();

            foreach (OBJ.GroupTriangle face in facesUV1)
            {
                if (face.Triangle.PointInUV1Triangle(point, out weights))
                {
                    Vector3 pos = new Vector3();
                    Vector3 norm = new Vector3();
                    for (int i = 0; i < 3; i++)
                    {
                        pos += face.Positions[i] * weights[i];
                        norm += face.Normals[i] * weights[i];
                    }
                    return new Vector3[] { pos, norm };
                }
                else if (lowval <= weights[0] && weights[0] <= highval && lowval <= weights[1] && weights[1] <= highval && lowval <= weights[2] && weights[2] <= highval)
                {
                    List<DeltaPoint> faceVerts = new List<DeltaPoint>();
                    float pointDistance;
                    for (int i = 0; i < 3; i++)
                    {
                        if (weights[i] >= 0f) faceVerts.Add(new DeltaPoint(face.UV1[i], face.Group, face.Positions[i], face.Normals[i]));
                    }
                    if (faceVerts.Count == 2)
                    {
                        int endpointIndex;
                        if (!point.DistanceFromLineRestricted(faceVerts[0].uv, faceVerts[1].uv, out pointDistance, out endpointIndex))
                        {
                            DeltaPoint d = faceVerts[endpointIndex];
                            faceVerts = new List<DeltaPoint>();
                            faceVerts.Add(d);
                        }
                    }
                    else if (faceVerts.Count == 1)
                    {
                        pointDistance = point.Distance(faceVerts[0].uv);
                    }
                    else
                    {
                        return null;
                    }
                    if (pointDistance < maxDistance)
                    {
                        if (refVerts.Count == 2 && faceVerts.Count == 1) continue;
                        if (faceVerts.Count == 2 && refVerts.Count == 1) refVerts = new List<DeltaPoint>();
                        foreach (DeltaPoint p in faceVerts)
                        {
                            if (!refVerts.Contains(p)) refVerts.Add(p);
                        }
                    }
                }
            }
            if (refVerts.Count > 0)
            {
                return GetInterpolatedDeltas(point, refVerts);
            }
            return null;
        }

        internal Vector3[] GetInterpolatedDeltas(Vector2 point, List<DeltaPoint> deltaPoints)
        {
            if (deltaPoints.Count == 1) return new Vector3[] { deltaPoints[0].deltaPosition, deltaPoints[0].deltaNormal }; 
            List<ReferencePoint> refPoints = new List<ReferencePoint>();
            foreach (DeltaPoint p in deltaPoints)
            {
                //  if (p.group != group) continue;
                float distance = p.uv.Distance(point);
                if (distance < 0.001) return new Vector3[] { p.deltaPosition, p.deltaNormal };
                ReferencePoint rp = new ReferencePoint(distance, p.deltaPosition, p.deltaNormal, p.uv);
                if (!refPoints.Contains(rp)) refPoints.Add(rp);
            }
            refPoints.Sort();

            int numRefs = 2;
            float[] d = new float[numRefs];
            float dt = 0;
            for (int i = 0; i < numRefs; i++)
            {
                d[i] = 1f / (float)Math.Pow(refPoints[i].distance, 3d);
                dt += d[i];
            }

            Vector3 pos = new Vector3();
            Vector3 norm = new Vector3();
            float[] weights = new float[3];
            for (int i = 0; i < numRefs; i++)
            {
                weights[i] = d[i] / dt;
                pos += refPoints[i].deltaPosition * weights[i];
                norm += refPoints[i].deltaNormal * weights[i];
            }
            return new Vector3[] { pos, norm };
        }

        internal class DeltaPoint : IEquatable<DeltaPoint>
        {
            internal Vector2 uv;
            internal int group;
            internal Vector3 deltaPosition;
            internal Vector3 deltaNormal;
            internal DeltaPoint(Vector2 uv, int group, Vector3 deltaPosition, Vector3 deltaNormal)
            {
                this.uv = new Vector2(uv);
                this.group = group;
                this.deltaPosition = new Vector3(deltaPosition);
                this.deltaNormal = new Vector3(deltaNormal);
            }
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                if (obj is DeltaPoint)
                {
                    return base.Equals(obj as DeltaPoint);
                }
                else
                {
                    return false;
                }
            }
            public bool Equals(DeltaPoint other)
            {
                return this.uv.Equals(other.uv);
            }
        }

        internal class ReferencePoint : IComparable<ReferencePoint>
        {
            internal float distance;
            internal Vector3 deltaPosition;
            internal Vector3 deltaNormal;
            internal Vector2 uv;
            internal ReferencePoint(float distance, Vector3 deltaPosition, Vector3 deltaNormal, Vector2 uv1)
            {
                this.distance = distance;
                this.deltaPosition = new Vector3(deltaPosition);
                this.deltaNormal = new Vector3(deltaNormal);
                this.uv = new Vector2(uv1);
            }
            public int CompareTo(ReferencePoint refp)
            {
                return this.distance.CompareTo(refp.distance);
            }
        }

        internal class ReferenceFace : IComparable<ReferenceFace>
        {
            internal float distance;
            internal OBJ.GroupTriangle face;
            internal ReferenceFace(float distance, OBJ.GroupTriangle triangle)
            {
                this.distance = distance;
                this.face = new OBJ.GroupTriangle(triangle);
            }
            public int CompareTo(ReferenceFace refFace)
            {
                return this.distance.CompareTo(refFace.distance);
            }
        }

        internal enum DMapSize
        {
            Small = 0,
            Medium = 1,
            Large = 2
        }
    }
}
