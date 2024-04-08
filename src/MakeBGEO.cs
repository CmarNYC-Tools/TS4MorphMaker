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

        private void ExportFace_button_Click(object sender, EventArgs e)
        {
            ExportFace(true);
        }

        private void FaceExport_OBJ_button_Click(object sender, EventArgs e)
        {
            ExportFace(false);
        }

        private void ExportFace(bool exportFormatMS3D)
        {
            GEOM[] tmp = new GEOM[4];
            AgeGender age = AgeGender.Adult;
            if (ExportFaceAgeGender_listBox.SelectedIndex == 0)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.iuHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.iuHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.iuHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.iuHead_lod3)));
                age = AgeGender.Infant;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 1)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.puHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.puHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.puHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.puHead_lod3)));
                age = AgeGender.Toddler;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 2)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.cuHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.cuHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.cuHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.cuHead_lod3)));
                age = AgeGender.Child;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 3)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymHead_lod3)));
                age = AgeGender.Adult;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 4)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod3)));
                age = AgeGender.Adult;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 5)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfwHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfwHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfwHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfwHead_lod3)));
                age = AgeGender.Adult;
            }
            else if (ExportFaceAgeGender_listBox.SelectedIndex == 6)
            {
                tmp[0] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymwHead_lod0)));
                tmp[1] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymwHead_lod1)));
                tmp[2] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymwHead_lod2)));
                tmp[3] = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.ymwHead_lod3)));
                age = AgeGender.Adult;
            }
            if (exportFormatMS3D)
            {
                MS3D ms3d = new MS3D(tmp, SelectRig(age), HeadMeshUV0_radioButton.Checked ? 0 : 1,
                            new string[] { "TS4Head_lod0", "TS4Head_lod1", "TS4Head_lod2", "TS4Head_lod3" });
                WriteMS3D("Save Base Milkshape mesh", ms3d, "");
            }
            else
            {
                OBJ obj = new OBJ(tmp, HeadMeshUV0_radioButton.Checked ? 0 : 1, true, 
                            new string[] { "TS4Head_lod0", "TS4Head_lod1", "TS4Head_lod2", "TS4Head_lod3" });
                WriteOBJFile("Save Base OBJ head mesh", obj, "");
            }
        }

        private void ImportFaceBase_button_Click(object sender, EventArgs e)
        {
            ImportFaceBase.Text = GetFilename("Select Base Mesh", Meshfilter);
        }

        private void ImportFaceMorph_button_Click(object sender, EventArgs e)
        {
            ImportFaceMorph.Text = GetFilename("Select Morphed Mesh", Meshfilter);
        }

        private void BGEOgo_button_Click(object sender, EventArgs e)
        {
            if ((string.Compare(ImportFaceBase.Text, " ") <= 0) || (string.Compare(ImportFaceMorph.Text, " ") <= 0))
            {
                MessageBox.Show("You must select both a base and morph mesh!");
                return;
            }
            if (string.Compare(BGEOmorphName.Text, " ") <= 0)
            {
                MessageBox.Show("You must enter a unique name for the morph so the BGEO is assigned a TGI!");
                return;
            }
            GEOM[] baseGeom;
            GEOM[] morphGeom;

            if (string.Compare(Path.GetExtension(ImportFaceBase.Text), ".ms3d", true) == 0)
            {
                MS3D baseMesh;
                MS3D morphMesh;
                if (!ReadMS3D(ImportFaceBase.Text, out baseMesh))
                {
                    MessageBox.Show("Can't read base Milkshape mesh!");
                    return;
                }
                if (!ReadMS3D(ImportFaceMorph.Text, out morphMesh))
                {
                    MessageBox.Show("Can't read morph Milkshape mesh!");
                    return;
                }
                baseGeom = GEOM.GEOMsFromMS3D(baseMesh, CurrentHead);
                morphGeom = GEOM.GEOMsFromMS3D(morphMesh, CurrentHead);
            }
            else
            {
                OBJ baseMesh;
                OBJ morphMesh;
                if (!GetOBJData(ImportFaceBase.Text, out baseMesh))
                {
                    MessageBox.Show("Can't read base OBJ mesh!");
                    return;
                }
                if (!GetOBJData(ImportFaceMorph.Text, out morphMesh))
                {
                    MessageBox.Show("Can't read morph OBJ mesh!");
                    return;
                }
                if (!morphMesh.hasNormals) 
                {
                    MessageBox.Show("The morphed mesh does not have normals. Please export it with normals.");
                    return;
                }
                baseGeom = GEOM.GEOMsFromOBJ(baseMesh, CurrentHead);
                morphGeom = GEOM.GEOMsFromOBJ(morphMesh, CurrentHead);
                if (morphGeom[0].numberVertices == 0 && morphGeom.Length == baseGeom.Length + 1)        //Blender weirdness
                {
                    GEOM[] tmp = new GEOM[baseGeom.Length];
                    Array.Copy(morphGeom, 1, tmp, 0, baseGeom.Length);
                    morphGeom = tmp;
                }
                bool sortme = false;                                                                       //more Blender weirdness
                for (int i = 0; i < morphGeom.Length - 1; i++)
                {
                    if (morphGeom[i].numberVertices < morphGeom[i + 1].numberVertices)
                    {
                        sortme = true;
                        break;
                    }
                }
                if (sortme)
                {
                    List<GEOM> tmp = new List<GEOM>(morphGeom);
                    tmp.Sort();
                    morphGeom = tmp.ToArray();
                }
            }

            BGEOwait_label.Visible = true;

            BGEO.LOD[] lods = new BGEO.LOD[4];
            List<BGEO.Blend> blends = new List<BGEO.Blend>();
            List<BGEO.Vector> vectors = new List<BGEO.Vector>();
            int vectorIndex = 0;
            int previousNumberVectors = 0;
            Vector3 zero = new Vector3();
            int lastIndex = 0;
            for (int i = 0; i < baseGeom.Length; i++)
            {
                if (baseGeom[i].numberVertices != morphGeom[i].numberVertices)
                {
                    MessageBox.Show("The number of vertices in the base mesh and morph mesh do not match.");
                    BGEOwait_label.Visible = false;
                    return;
                }
                int startVert = baseGeom[i].minVertexID;
                int numberVerts = baseGeom[i].maxVertexID - baseGeom[i].minVertexID + 1;
                int numberVectors = 0;
                SortableBlend[] sortBlends = new SortableBlend[numberVerts];
                for (int j = 0; j < sortBlends.Length; j++)
                {
                    sortBlends[j] = new SortableBlend(false, false, 0);
                }
                bool mirror = BGEOmirror_checkBox.Checked;
                for (int j = 0; j < baseGeom[i].numberVertices; j++)
                {
                    int ind = j;
                    int id = baseGeom[i].getVertexID(j);
                    if (morphGeom[i].hasVertexIDs && id != morphGeom[i].getVertexID(j))
                    {
                        ind = morphGeom[i].vertexIDsearch((uint)baseGeom[i].getVertexID(j));
                    }
                    if (ind >= 0)
                    {
                        Vector3 basePos = new Vector3(baseGeom[i].getPosition(j));
                        Vector3 baseNorm = new Vector3(baseGeom[i].getNormal(j));
                        Vector3 morphPos, morphNorm, deltaPos, deltaNorm;
                        morphPos = new Vector3(morphGeom[i].getPosition(ind));
                        morphNorm = new Vector3(morphGeom[i].getNormal(ind));
                        if (mirror && basePos.X < 0)
                        {
                            Vector3 baseMirror = new Vector3(basePos);
                            baseMirror.X = -basePos.X;
                            for (int m = 0; m < baseGeom[i].numberVertices; m++)
                            {
                                if (baseMirror == new Vector3(baseGeom[i].getPosition(m)))
                                {
                                    int ind2;
                                    if (baseGeom[i].getVertexID(m) == morphGeom[i].getVertexID(m))
                                    {
                                        ind2 = m;
                                    }
                                    else
                                    {
                                        ind2 = morphGeom[i].vertexIDsearch((uint)baseGeom[i].getVertexID(m));
                                    }
                                    morphPos = new Vector3(morphGeom[i].getPosition(ind2));
                                    morphPos.X = -morphPos.X;
                                    morphNorm = new Vector3(morphGeom[i].getNormal(ind2));
                                    morphNorm.X = -morphNorm.X;
                                }
                            }   
                        }
                        bool hasPos = false, hasNorm = false;
                        deltaPos = morphPos - basePos;
                        deltaNorm = morphNorm - baseNorm;
                        hasPos = (deltaPos != zero);
                        if (!NoNormalsBGEO_checkBox.Checked) hasNorm = (deltaNorm != zero);
                        vectorIndex = vectors.Count;
                        if (hasPos)
                        {
                            vectors.Add(new BGEO.Vector(deltaPos.Coordinates));
                            numberVectors++;
                        }
                        if (hasNorm)
                        {
                            vectors.Add(new BGEO.Vector(deltaNorm.Coordinates));
                            numberVectors++;
                        }
                        if (hasPos || hasNorm) 
                            sortBlends[id - startVert] = new SortableBlend(hasPos, hasNorm, vectorIndex);
                    }
                }
                lods[i] = new BGEO.LOD((uint)startVert, (uint)numberVerts, (uint)numberVectors);
                lastIndex += previousNumberVectors;
                for (int j = 0; j < sortBlends.Length; j++)
                {
                    if (sortBlends[j].hasPos || sortBlends[j].hasNorm)
                    {
                        blends.Add(new BGEO.Blend(sortBlends[j].hasPos, sortBlends[j].hasNorm, (short)(sortBlends[j].vectorIndex - lastIndex)));
                        lastIndex = sortBlends[j].vectorIndex;
                    }
                    else
                    {
                        blends.Add(new BGEO.Blend(false, false, 0));
                    }
                }
                previousNumberVectors = numberVectors;
            }

            uint objectSize = 20U + (uint)(lods.Length * 12) + (uint)(2 * blends.Count) + (uint)(6 * vectors.Count);
            ulong instance = FNVhash.FNV64(BGEOmorphName.Text) | 0x8000000000000000;
            BGEO bgeo = new BGEO(3U, new TGI[] { new TGI(0x067CAA11U, 0, instance) }, null, null,
                new BGEO.ObjectData[] { new BGEO.ObjectData(44U, objectSize) }, 0x600, 
                 lods, blends.ToArray(), vectors.ToArray());

            BGEOwait_label.Visible = false;

            WriteBgeoFile("Save morph BGEO", bgeo, "S4_067CAA11_00000000_" + instance.ToString("X16") +"_" + BGEOmorphName.Text);
        }

        public class SortableBlend
        {
            internal bool hasPos;
            internal bool hasNorm;
            internal int vectorIndex;

            public SortableBlend(bool hasPos, bool hasNorm, int vectInd)
            {
                this.hasPos = hasPos;
                this.hasNorm = hasNorm;
                this.vectorIndex = vectInd;
            }
        }

        private void NumberGEOM_button_Click(object sender, EventArgs e)
        {
            string geomFile = GetFilename("Select custom GEOM mesh to have ID numbers added", GEOMfilter);
            if (String.Compare(geomFile, " ") <= 0) return;
            if (!File.Exists(geomFile)) return;
            GEOM geom = null;
            if (ReadGEOM(geomFile, out geom))
            {
                GetIDStartNumber id = new GetIDStartNumber();
                id.ShowDialog();
                if (id.DialogResult == DialogResult.OK)
                {
                    int nextNum = geom.AddIDNumbers(id.StartID);
                    string savedGeom = WriteGEOM("Save numbered GEOM mesh", geom);
                    string baseName = Path.GetFileNameWithoutExtension(savedGeom);
                    OBJ obj = new OBJ(new GEOM[] { geom }, 0, true, new string[] { baseName });
                    WriteOBJFile("Save numbered OBJ mesh to use as base for BGEO", obj, baseName);
                    MS3D ms3d = new MS3D(new GEOM[] { geom }, adultRig, 0, new string[] { baseName });
                    WriteMS3D("Save numbered MS3D mesh to use as base for BGEO", ms3d, baseName);
                    MessageBox.Show("The last ID number used was " + (nextNum - 1).ToString());
                }
            }
            else
            {
                MessageBox.Show("Unable to read file: " + geomFile);
            }
        }
    }
}
