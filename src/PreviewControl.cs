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
        ResourceManager rm = Properties.Resources.ResourceManager;
        bool skipLODcheck = false;
        bool skipModelUpdate = false;
        GEOM CurrentHead = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfHead_lod0)));
        GEOM CurrentHeadMorph = null;
        int currentLOD = 0;
        GEOM CurrentBody = new GEOM(new BinaryReader(new MemoryStream(Properties.Resources.yfBodyComplete_lod0)));
        GEOM CurrentBodyMorph = null;
        GEOM CurrentEars, CurrentTail;
        GEOM CurrentEarsMorph, CurrentTailMorph;
        Image CurrentSkin = Properties.Resources.FemaleSkinTan;
        GEOM CurrentCustom = null;
        GEOM CurrentCustomMorph = null;
        Species currentSpecies = Species.Human;
        AgeGender currentAge = AgeGender.Adult;
        AgeGender currentGender = AgeGender.Female;
        RIG previewRig = GetTS4Rig(Species.Human, AgeGender.Adult);
        List<BGEO> morphBGEO = new List<BGEO>();
        List<MorphMap> morphShape = new List<MorphMap>();
        List<MorphMap> morphNormals = new List<MorphMap>();
        List<BOND> morphBOND = new List<BOND>();

        private void SelectBoneDelta_button_Click(object sender, EventArgs e)
        {
            PreviewBONDfile.Text = GetFilename("Select BoneDelta/Bone Pose file to preview", BONDfilter);
            if (String.Compare(PreviewBONDfile.Text, " ") <= 0) return;
            BOND tmp;
            if (GetBONDData(PreviewBONDfile.Text, out tmp))
            {
                float factor;
                if (!float.TryParse(PreviewBondFactor.Text, out factor))
                {
                    MessageBox.Show("Please enter a valid number in the Weight field!");
                    return;
                }
                tmp.weight = factor;
               // RIG rig = GetTS4Rig(currentSpecies, currentAge);
                RIG rig = previewRig;
                CurrentHeadMorph = LoadBONDMorph(CurrentHeadMorph, tmp, rig);
                CurrentBodyMorph = LoadBONDMorph(CurrentBodyMorph, tmp, rig);
                CurrentEarsMorph = LoadBONDMorph(CurrentEarsMorph, tmp, rig);
                CurrentTailMorph = LoadBONDMorph(CurrentTailMorph, tmp, rig);
                CurrentCustomMorph = LoadBONDMorph(CurrentCustomMorph, tmp, rig);
                morphBOND.Add(tmp);
                if (PreviewShow_checkBox.Checked)
                {
                    morphPreview1.Stop_Mesh();
                    if (PreviewShowModel_checkBox.Checked)
                    {
                        if (PreviewShow_Skin_checkBox.Checked)
                            morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, CurrentCustomMorph, CurrentSkin, false);
                        else
                            morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, CurrentCustomMorph, null, false);
                    }
                    else
                    {
                        morphPreview1.Start_Mesh(null, null, null, null, CurrentCustomMorph, null, false);
                    }
                }
            }
        }

        //public static GEOM LoadBONDMorph(GEOM baseMesh, BOND boneDelta, RIG rig)
        //{
        //    if (baseMesh == null) return null;
        //    if (boneDelta == null) return baseMesh;
        //    GEOM morphMesh = new GEOM(baseMesh);
        //    Vector3 unit = new Vector3(1f, 1f, 1f);
        //    string missingBones = "";
        //    foreach (BOND.BoneAdjust delta in boneDelta.adjustments)
        //    {
        //        RIG.Bone bone = rig.GetBone(delta.slotHash);
        //        if (bone == null) 
        //        {
        //            missingBones += Environment.NewLine + "Bone not found: " + delta.slotHash.ToString("X8");
        //            continue;
        //        }
        //        Vector3 scale = (bone.MorphRotation.toMatrix3x3() * Matrix3x3.FromScale(new Vector3(delta.scaleX, delta.scaleY, delta.scaleZ) + unit)).Scale - unit;
        //        Vector3 offset = (bone.MorphRotation * new Vector3(delta.offsetX, delta.offsetY, delta.offsetZ) * bone.MorphRotation.Conjugate()).toVector3();
        //        Quaternion rotation = new Quaternion(delta.quatX, delta.quatY, delta.quatZ, delta.quatW);
        //        if (rotation.isEmpty) rotation = Quaternion.Identity;
        //        if (!rotation.isNormalized) rotation.Balance();
        //        rotation = bone.MorphRotation * rotation * bone.MorphRotation.Conjugate();
        //        RecurseChildren(bone, scale, offset, rotation, rig, morphMesh);
        //        morphMesh.BoneMorpher(bone.WorldPosition, boneDelta.weight, offset, scale, rotation);
        //        morphMesh.BoneReset();
        //    }
        //    if (missingBones.Length > 0)
        //    {
        //        MessageBox.Show(missingBones + Environment.NewLine + Environment.NewLine + "Are you using the right rig?");
        //    }

        //    return morphMesh;
        //}

        //public static void RecurseChildren(RIG.Bone bone, Vector3 scale, Vector3 offset, Quaternion rotation, RIG rig, GEOM geom)
        //{
        //    //   MessageBox.Show(bone.BoneName + ", " + bone.WorldPosition.ToString());
        //    int[] verts = geom.GetVertexIndicesAssignedtoBone(bone.BoneHash);
        //    geom.AddBoneWeights(bone.BoneHash, verts);
        //    RIG.Bone[] childBones = rig.GetChildren(bone.BoneHash);
        //    foreach (RIG.Bone child in childBones)
        //    {
        //        child.BoneMover(bone, scale, offset, rotation);
        //        RecurseChildren(child, scale, offset, Quaternion.Identity, rig, geom);
        //    }
        //}

        //public static GEOM LoadBONDMorph(GEOM baseMesh, BOND boneDelta, RIG rig)
        //{
        //    if (baseMesh == null) return null;
        //    if (boneDelta == null) return baseMesh;
        //    GEOM morphMesh = new GEOM(baseMesh);
        //    Vector3 unit = new Vector3(1f, 1f, 1f);
        //    string missingBones = "";
        //    float weight = boneDelta.weight;
        //    foreach (BOND.BoneAdjust delta in boneDelta.adjustments)
        //    {
        //        RIG.Bone bone = rig.GetBone(delta.slotHash);
        //        if (bone == null)
        //        {
        //            missingBones += Environment.NewLine + "Bone not found: " + delta.slotHash.ToString("X8");
        //            continue;
        //        }
        //        Vector3 localScale = new Vector3(delta.scaleX, delta.scaleY, delta.scaleZ);
        //        Vector3 localOffset = new Vector3(delta.offsetX, delta.offsetY, delta.offsetZ);
        //        Quaternion localRotation = new Quaternion(delta.quatX, delta.quatY, delta.quatZ, delta.quatW);
        //        if (localRotation.isEmpty) localRotation = Quaternion.Identity;
        //        if (!localRotation.isNormalized) localRotation.Balance();

        //        Vector3 worldScale = (bone.MorphRotation.toMatrix3x3() * Matrix3x3.FromScale(localScale + unit)).Scale - unit;
        //        Vector3 worldOffset = (bone.MorphRotation * localOffset * bone.MorphRotation.Conjugate()).toVector3();
        //        Quaternion worldRotation = bone.MorphRotation * localRotation * bone.MorphRotation.Conjugate();
        //        BoneMorpher(morphMesh, rig, bone, weight, worldOffset, worldScale, worldRotation);
        //        rig.BoneMorpher(bone.BoneHash, weight, localScale, localOffset, localRotation);
        //    }
        //    if (missingBones.Length > 0)
        //    {
        //        MessageBox.Show(missingBones + Environment.NewLine + Environment.NewLine + "Are you using the right rig?");
        //    }

        //    return morphMesh;
        //}

        //internal static void BoneMorpher(GEOM morphMesh, RIG rig, RIG.Bone bone, float weight, Vector3 offset, Vector3 scale, Quaternion rotation)
        //{
        //    morphMesh.BoneMorpher(bone, weight, offset, scale, rotation);
        //    RIG.Bone[] childBones = rig.GetChildren(bone.BoneHash);
        //    foreach (RIG.Bone child in childBones)
        //    {
        //        BoneMorpher(morphMesh, rig, child, weight, offset, scale, rotation);
        //    }
        //}

        public static GEOM LoadBONDMorph(GEOM baseMesh, BOND boneDelta, RIG rig)
        {
            if (baseMesh == null) return null;
            if (boneDelta == null) return baseMesh;
            GEOM morphMesh = new GEOM(baseMesh);
            Vector3 unit = new Vector3(1f, 1f, 1f);
            string missingBones = "";
            float weight = boneDelta.weight;

            morphMesh.SetupDeltas();

            // for (int i = boneDelta.adjustments.Length - 1; i >= 0; i--)
            foreach (BOND.BoneAdjust delta in boneDelta.adjustments)
            {
                //    BOND.BoneAdjust delta = boneDelta.adjustments[i];

                RIG.Bone bone = rig.GetBone(delta.slotHash);
                if (bone == null)
                {
                    missingBones += "Bone not found: " + delta.slotHash.ToString("X8") + ", ";
                    continue;
                }
                Vector3 localScale = new Vector3(delta.scaleX, delta.scaleY, delta.scaleZ);
                Vector3 localOffset = new Vector3(delta.offsetX, delta.offsetY, delta.offsetZ);
                Quaternion localRotation = new Quaternion(delta.quatX, delta.quatY, delta.quatZ, delta.quatW);
                if (localRotation.isEmpty) localRotation = Quaternion.Identity;
                if (!localRotation.isNormalized) localRotation.Balance();

                Vector3 worldScale = (bone.MorphRotation.toMatrix3D() * Matrix3D.FromScale(localScale + unit)).Scale - unit;
                Vector3 worldOffset = (bone.MorphRotation * localOffset * bone.MorphRotation.Conjugate()).toVector3();
                Quaternion worldRotation = bone.MorphRotation * localRotation * bone.MorphRotation.Conjugate();

                //BoneMorpher2(true, morphMesh, rig, bone, weight, localOffset, localScale, localRotation);
                morphMesh.BoneMorpher(bone, weight, worldOffset, worldScale, worldRotation);
                //rig.BoneMorpher(bone, weight, localScale, localOffset, localRotation);
                // BoneMorpher(morphMesh, rig, bone, weight, worldOffset, worldScale, worldRotation);
            }

            morphMesh.UpdatePositions();

            foreach (BOND.BoneAdjust delta in boneDelta.adjustments)
            {
                RIG.Bone bone = rig.GetBone(delta.slotHash);
                if (bone == null)
                {
                    continue;
                }
                Vector3 localScale = new Vector3(delta.scaleX, delta.scaleY, delta.scaleZ);
                Vector3 localOffset = new Vector3(delta.offsetX, delta.offsetY, delta.offsetZ);
                Quaternion localRotation = new Quaternion(delta.quatX, delta.quatY, delta.quatZ, delta.quatW);
                if (localRotation.isEmpty) localRotation = Quaternion.Identity;
                if (!localRotation.isNormalized) localRotation.Balance();

                rig.BoneMorpher(bone, weight, localScale, localOffset, localRotation);
            }

            if (missingBones.Length > 0)
            {
                MessageBox.Show(missingBones + Environment.NewLine + Environment.NewLine + "Are you using the right rig?");
            }
            return morphMesh;
        }

        private void SelectBGEO_button_Click(object sender, EventArgs e)
        {
            PreviewBGEOfile.Text = GetFilename("Select BGEO file to preview", BGEOfilter);
            if (String.Compare(PreviewBGEOfile.Text, " ") <= 0) return;
            float factor;
            BGEO tmp;
            if (GetBgeoData(PreviewBGEOfile.Text, out tmp))
            {
                if (!float.TryParse(PreviewBGEOFactor.Text, out factor))
                {
                    MessageBox.Show("Please enter a valid number in the Weight field!");
                    return;
                }
                tmp.weight = factor;
                CurrentHeadMorph = LoadBGEOMorph(CurrentHeadMorph, tmp, currentLOD);
                int customLod = CustomLOD_comboBox.SelectedIndex;
                CurrentCustomMorph = LoadBGEOMorph(CurrentCustomMorph, tmp, customLod);
                morphBGEO.Add(tmp);
                if (PreviewShow_checkBox.Checked)
                {
                    morphPreview1.Stop_Mesh();
                    if (PreviewShowModel_checkBox.Checked)
                    {
                        if (PreviewShow_Skin_checkBox.Checked)
                            morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, CurrentCustomMorph, CurrentSkin, false);
                        else
                            morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, CurrentCustomMorph, null, false);
                    }
                    else
                    {
                        morphPreview1.Start_Mesh(null, null, null, null, CurrentCustomMorph, null, false);
                    }
                }
            }
        }

        public static GEOM LoadBGEOMorph(GEOM baseMesh, BGEO morph, int lod)
        {
            if (baseMesh == null) return null;
            if (morph == null) return new GEOM(baseMesh);
            GEOM morphMesh = new GEOM(baseMesh);
            float weight = morph.weight;
            uint startID = morph.LodData[lod].IndexBase;
            uint startIndex = 0;
            for (int i = 0; i < lod; i++)
            {
                startIndex += morph.LodData[i].NumberVertices;
            }
            for (int i = 0; i < morphMesh.numberVertices; i++)
            {
             //   if (morphMesh.getVertexID(i) >= 6000) MessageBox.Show(morphMesh.getVertexID(i).ToString() + " : " + morph.LodData[lod].IndexBase.ToString() + " - " + morph.LodData[lod].NumberVertices.ToString());
                if (morphMesh.getVertexID(i) >= morph.LodData[lod].IndexBase &&
                        morphMesh.getVertexID(i) < morph.LodData[lod].IndexBase + morph.LodData[lod].NumberVertices)
                {
                    BGEO.Blend blend = morph.BlendMap[startIndex + (morphMesh.getVertexID(i) - startID)];
                    if (blend.PositionDelta)
                    {
                        Vector3 pos = new Vector3(morphMesh.getPosition(i));
                        Vector3 delta = new Vector3(morph.VectorData[blend.Index].TranslatedVector);
                        morphMesh.setPosition(i, (pos + (delta * weight)).Coordinates);
                    }
                    if (blend.NormalDelta)
                    {
                        Vector3 norm = new Vector3(morphMesh.getNormal(i));
                        Vector3 delta = new Vector3(morph.VectorData[blend.Index + (blend.PositionDelta ? 1 : 0)].TranslatedVector);
                        morphMesh.setNormal(i, (norm + (delta * weight)).Coordinates);
                    }
                }
            }
            return morphMesh;
        }

        private void SelectDMapShape_button_Click(object sender, EventArgs e)
        {
            PreviewShapeFile.Text = GetFilename("Select Shape DMap", DMapfilter);
        }

        private void SelectDMapNormals_button_Click(object sender, EventArgs e)
        {
            PreviewNormalsFile.Text = GetFilename("Select Normals DMap", DMapfilter);
        }

        private void PreviewDMapGo_button_Click(object sender, EventArgs e)
        {
            DMap shape = null;
            DMap normals = null;
            if (String.Compare(PreviewShapeFile.Text, " ") > 0 && !ReadDMap(PreviewShapeFile.Text, out shape))
            {
                MessageBox.Show("Can't read Shape DMap!");
                return;
            }
            if (String.Compare(PreviewNormalsFile.Text, " ") > 0 && !ReadDMap(PreviewNormalsFile.Text, out normals))
            {
                MessageBox.Show("Can't read Normals DMap!");
                return;
            }
            if (shape != null && shape.ShapeOrNormal != ShapeOrNormals.SHAPE_DEFORMER)
            {
                MessageBox.Show("You've loaded a normals DMap into the shape field!");
                return;
            }
            if (normals != null && normals.ShapeOrNormal != ShapeOrNormals.NORMALS_DEFORMER)
            {
                MessageBox.Show("You've loaded a shape DMap into the normals field!");
                return;
            }
            float factor;
            if (!float.TryParse(PreviewDMapFactor.Text, out factor))
            {
                MessageBox.Show("Please enter a valid number in the Weight field!");
                return;
            }
            MorphMap stmp = shape != null ? shape.ToMorphMap() : null;
            MorphMap ntmp = normals != null ? normals.ToMorphMap() : null;
            if (stmp != null) stmp.weight = factor;
            if (ntmp != null) ntmp.weight = factor;
            if (shape != null || normals != null)
            {
                morphShape.Add(stmp);
                morphNormals.Add(ntmp);
                CurrentHeadMorph = LoadDMapMorph(CurrentHeadMorph, stmp, ntmp);
                CurrentBodyMorph = LoadDMapMorph(CurrentBodyMorph, stmp, ntmp);
                CurrentEarsMorph = LoadDMapMorph(CurrentEarsMorph, stmp, ntmp);
                CurrentTailMorph = LoadDMapMorph(CurrentTailMorph, stmp, ntmp);
                CurrentCustomMorph = LoadDMapMorph(CurrentCustomMorph, stmp, ntmp);
                if (CurrentHeadMorph != null) CurrentHeadMorph.MatchPartSeamStitches();
                if (CurrentBodyMorph != null) CurrentBodyMorph.MatchPartSeamStitches();
                if (CurrentEarsMorph != null) CurrentEarsMorph.MatchPartSeamStitches();
                if (CurrentTailMorph != null) CurrentTailMorph.MatchPartSeamStitches();
                if (CurrentCustomMorph != null) CurrentCustomMorph.MatchPartSeamStitches();
                if (CurrentHeadMorph != null && CurrentBodyMorph != null) GEOM.MatchSeamStitches(CurrentHeadMorph, CurrentBodyMorph);
                if (CurrentHeadMorph != null && CurrentEarsMorph != null) GEOM.MatchSeamStitches(CurrentHeadMorph, CurrentEarsMorph);
                if (CurrentBodyMorph != null && CurrentTailMorph != null) GEOM.MatchSeamStitches(CurrentBodyMorph, CurrentTailMorph);
            }
            if (PreviewShow_checkBox.Checked)
            {
                morphPreview1.Stop_Mesh();
                ShowMesh(false);
            }
        }

        public static GEOM LoadDMapMorph(GEOM baseMesh, DMap dmapShape, DMap dmapNormals)
        {
            MorphMap mapShape = dmapShape != null ? dmapShape.ToMorphMap() : null;
            MorphMap mapNormals = dmapNormals != null ? dmapNormals.ToMorphMap() : null;
            return LoadDMapMorph(baseMesh, mapShape, mapNormals);
        }

        //public static GEOM LoadDMapMorph(GEOM baseMesh, MorphMap mapShape, MorphMap mapNormals)
        //{
        //    if (baseMesh == null) return null;
        //    if (mapShape == null & mapNormals == null)
        //    {
        //        return new GEOM(baseMesh);
        //    }
        //    GEOM morphMesh = new GEOM(baseMesh);
        //    for (int i = 0; i < morphMesh.numberVertices; i++)
        //    {
        //        float[] pos = morphMesh.getPosition(i);
        //        float[] norm = morphMesh.getNormal(i);
        //        if (mapShape != null && morphMesh.hasUVset(1))
        //        {
        //            List<float[]> stitchList = morphMesh.GetStitchUVs(i);
        //            Vector3 shapeVector = new Vector3();
        //            Vector3 normVector = new Vector3();
        //            float[] uv1 = morphMesh.getUV(i, 1);
        //            if (stitchList.Count > 0)
        //            {
        //                foreach (float[] stitch in stitchList)
        //                {
        //                    int x = (int)(Math.Abs((mapShape.MapWidth - 1) * stitch[0]) - mapShape.MinCol);
        //                    int y = (int)(((mapShape.MapHeight - 1) * stitch[1]) - mapShape.MinRow);
        //                    if (x >= 0 && x < (mapShape.MaxCol - mapShape.MinCol + 1) &&
        //                        y >= 0 && y < (mapShape.MaxRow - mapShape.MinRow + 1))
        //                    {
        //                        if (mapShape != null)
        //                        {
        //                            Vector3 deltaShape = mapShape.GetAdjustedDelta(x, y, uv1[0] < 0, (byte)(morphMesh.getTagval(i) & 0xFF));
        //                            shapeVector += deltaShape;
        //                        }
        //                        if (mapNormals != null)
        //                        {
        //                            Vector3 deltaNorm = mapNormals.GetAdjustedDelta(x, y, uv1[0] < 0, (byte)(morphMesh.getTagval(i) & 0xFF));
        //                            normVector += deltaNorm;
        //                        }
        //                    }
        //                }
        //                shapeVector = shapeVector / (float)stitchList.Count;
        //                normVector = normVector / (float)stitchList.Count;
        //            }
        //            else
        //            {
        //                int x = (int)(Math.Abs(mapShape.MapWidth * uv1[0]) - mapShape.MinCol);
        //                int y = (int)((mapShape.MapHeight * uv1[1]) - mapShape.MinRow);
        //                if (x >= 0 && x < (mapShape.MaxCol - mapShape.MinCol + 1) &&
        //                    y >= 0 && y < (mapShape.MaxRow - mapShape.MinRow + 1))
        //                {
        //                    if (mapShape != null)
        //                    {
        //                        shapeVector = mapShape.GetAdjustedDelta(x, y, uv1[0] < 0, (byte)(morphMesh.getTagval(i) & 0xFF));
        //                    }
        //                    if (mapNormals != null)
        //                    {
        //                        normVector = mapNormals.GetAdjustedDelta(x, y, uv1[0] < 0, (byte)(morphMesh.getTagval(i) & 0xFF));
        //                    }
        //                }
        //            }
        //            pos[0] -= shapeVector.X * mapShape.weight;
        //            pos[1] -= shapeVector.Y * mapShape.weight;
        //            pos[2] -= shapeVector.Z * mapShape.weight;
        //            norm[0] -= normVector.X * mapNormals.weight;
        //            norm[1] -= normVector.Y * mapNormals.weight;
        //            norm[2] -= normVector.Z * mapNormals.weight;
        //        }
        //        morphMesh.setPosition(i, pos);
        //        morphMesh.setNormal(i, norm);
        //    }

        //    return morphMesh;
        //}

        //public static GEOM LoadDMapMorph(GEOM baseMesh, MorphMap morphShape, MorphMap morphNormals)
        //{
        //    if (baseMesh == null) return null;
        //    if (morphShape == null & morphNormals == null)
        //    {
        //        return new GEOM(baseMesh);
        //    }
        //    GEOM morphMesh = new GEOM(baseMesh);
        //    for (int map = 1; map <= 2; map++)
        //    {
        //        if (morphShape != null && morphMesh.hasUVset(map))
        //        {
        //            for (int i = 0; i < morphMesh.numberVertices; i++)
        //            {
        //                float[] pos = morphMesh.getPosition(i);
        //                float[] norm = morphMesh.getNormal(i);
        //                List<float[]> stitchList = morphMesh.GetStitchUVs(i);
        //                Vector3 shapeVector = new Vector3();
        //                Vector3 normVector = new Vector3();
        //                if (stitchList.Count > 0)
        //                {
        //                    foreach (float[] stitch in stitchList)
        //                    {
        //                        int x = (int)(Math.Abs((morphShape.MapWidth - 1) * stitch[0]) - morphShape.MinCol);
        //                        int y = (int)(((morphShape.MapHeight - 1) * stitch[1]) - morphShape.MinRow);
        //                        if (x >= 0 && x < (morphShape.MaxCol - morphShape.MinCol) &&
        //                            y >= 0 && y < (morphShape.MaxRow - morphShape.MinRow))
        //                        {
        //                            // Vector3 deltaShape = morphShape.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(g.getTagval(i) & 0xFF));
        //                            Vector3 deltaShape = morphShape.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));  //as of Cats & Dogs
        //                            shapeVector += deltaShape;
        //                            if (morphNormals != null)
        //                            {
        //                                //  Vector3 deltaNorm = morphNormals.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(g.getTagval(i) & 0xFF));
        //                                Vector3 deltaNorm = morphNormals.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
        //                                normVector += deltaNorm;
        //                            }
        //                        }
        //                    }
        //                    shapeVector = shapeVector / (float)stitchList.Count;
        //                    normVector = normVector / (float)stitchList.Count;
        //                }
        //                else
        //                {
        //                    float[] uv1 = morphMesh.getUV(i, map);
        //                    int x = (int)(Math.Abs(morphShape.MapWidth * uv1[0]) - morphShape.MinCol);
        //                    int y = (int)((morphShape.MapHeight * uv1[1]) - morphShape.MinRow);
        //                    if (x >= 0 && x < (morphShape.MaxCol - morphShape.MinCol) &&
        //                        y >= 0 && y < (morphShape.MaxRow - morphShape.MinRow))
        //                    {
        //                        shapeVector = morphShape.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
        //                        if (morphNormals != null)
        //                        {
        //                            normVector = morphNormals.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
        //                        }
        //                    }
        //                }
        //                pos[0] -= shapeVector.X * morphShape.weight;
        //                pos[1] -= shapeVector.Y * morphShape.weight;
        //                pos[2] -= shapeVector.Z * morphShape.weight;
        //                morphMesh.setPosition(i, pos);
        //                if (morphNormals != null)
        //                {
        //                    norm[0] -= normVector.X * morphNormals.weight;
        //                    norm[1] -= normVector.Y * morphNormals.weight;
        //                    norm[2] -= normVector.Z * morphNormals.weight;
        //                    morphMesh.setNormal(i, norm);
        //                }
        //            }
        //        }
        //    }

        //    return morphMesh;
        //}

        //public static GEOM LoadDMapMorph(GEOM baseMesh, MorphMap morphShape, MorphMap morphNormals)
        //{
        //    if (baseMesh == null) return null;
        //    if (morphShape == null & morphNormals == null)
        //    {
        //        return new GEOM(baseMesh);
        //    }
        //    if (!baseMesh.hasTags || !baseMesh.hasUVset(1)) return new GEOM(baseMesh);
        //    GEOM morphMesh = new GEOM(baseMesh);
        //    Vector3 empty = new Vector3(0, 0, 0);
        //    for (int map = 1; map < 2; map++)
        //    {
        //        if (morphShape != null && morphMesh.hasUVset(map))
        //        {
        //            for (int i = 0; i < morphMesh.numberVertices; i++)
        //            {
        //                float[] pos = morphMesh.getPosition(i);
        //                float[] norm = morphMesh.getNormal(i);
        //                List<float[]> stitchList = morphMesh.GetStitchUVs(i);
        //                int x, y;
        //                Vector3 shapeVector = new Vector3();
        //                Vector3 normVector = new Vector3();
        //                if (stitchList.Count > 0)
        //                {
        //                    float[] uv1 = stitchList[stitchList.Count - 1];
        //                    x = (int)(Math.Abs(morphShape.MapWidth * uv1[0]) - morphShape.MinCol - 0.5f);
        //                    y = (int)((morphShape.MapHeight * uv1[1]) - morphShape.MinRow - 0.5f);
        //                }
        //                else
        //                {
        //                    float[] uv1 = morphMesh.getUV(i, map);
        //                    x = (int)(Math.Abs(morphShape.MapWidth * uv1[0]) - morphShape.MinCol - 0.5f);
        //                    y = (int)((morphShape.MapHeight * uv1[1]) - morphShape.MinRow - 0.5f);
        //                }

        //                //if (x >= 0 && x <= (morphShape.MaxCol - morphShape.MinCol) &&
        //                //    y >= 0 && y <= (morphShape.MaxRow - morphShape.MinRow))
        //                //if (x >= 0 && y >= 0)
        //                //{
        //                x = Math.Max(x, 0);
        //                y = Math.Max(y, 0);
        //                x = (int)Math.Min(x, morphShape.MaxCol - morphShape.MinCol);
        //                y = (int)Math.Min(y, morphShape.MaxRow - morphShape.MinRow);
        //                shapeVector = morphShape.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
        //                if (morphNormals != null)
        //                {
        //                    normVector = morphNormals.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
        //                }
        //                //}

        //                if (shapeVector != empty)
        //                {
        //                    // float vertWeight = 1f;
        //                    float vertWeight = ((morphMesh.getTagval(i) & 0xFF00) >> 8) / 255f;
        //                    pos[0] -= shapeVector.X * morphShape.weight * vertWeight;
        //                    pos[1] -= shapeVector.Y * morphShape.weight * vertWeight;
        //                    pos[2] -= shapeVector.Z * morphShape.weight * vertWeight;
        //                    morphMesh.setPosition(i, pos);
        //                    if (morphNormals != null)
        //                    {
        //                        norm[0] -= normVector.X * morphNormals.weight * vertWeight;
        //                        norm[1] -= normVector.Y * morphNormals.weight * vertWeight;
        //                        norm[2] -= normVector.Z * morphNormals.weight * vertWeight;
        //                        morphMesh.setNormal(i, norm);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return morphMesh;
        //}

        public static GEOM LoadDMapMorph(GEOM baseMesh, MorphMap morphShape, MorphMap morphNormals)
        {
            if (baseMesh == null) return null;
            if (morphShape == null & morphNormals == null) return new GEOM(baseMesh);
            if (morphShape.weight == 0) return new GEOM(baseMesh);
            if (!baseMesh.hasTags || !baseMesh.hasUVset(1)) return new GEOM(baseMesh);
            GEOM morphMesh = new GEOM(baseMesh);
            Vector3 empty = new Vector3(0, 0, 0);

            if (morphShape != null && morphMesh.hasUVset(1))
            {
                for (int i = 0; i < morphMesh.numberVertices; i++)
                {
                    float[] pos = morphMesh.getPosition(i);
                    float[] norm = morphMesh.getNormal(i);
                    List<float[]> stitchList = morphMesh.GetStitchUVs(i);
                    int x, y;
                    Vector3 shapeVector = new Vector3();
                    Vector3 normVector = new Vector3();
                    if (stitchList.Count > 0)
                    {
                        float[] uv1 = stitchList[0];
                        x = (int)(Math.Abs(morphShape.MapWidth * uv1[0]) - morphShape.MinCol - 0.5f);
                        y = (int)((morphShape.MapHeight * uv1[1]) - morphShape.MinRow - 0.5f);
                    }
                    else
                    {
                        float[] uv1 = morphMesh.getUV(i, 1);
                        x = (int)(Math.Abs(morphShape.MapWidth * uv1[0]) - morphShape.MinCol - 0.5f);
                        y = (int)((morphShape.MapHeight * uv1[1]) - morphShape.MinRow - 0.5f);
                    }

                    if (y > morphShape.MaxRow - morphShape.MinRow) y = (int)(morphShape.MaxRow - morphShape.MinRow - 0.5f); //not sure about this

                    if (x >= 0 && x <= (morphShape.MaxCol - morphShape.MinCol) &&
                        y >= 0 && y <= (morphShape.MaxRow - morphShape.MinRow))
                    {
                        shapeVector = morphShape.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
                        if (morphNormals != null)
                        {
                            normVector = morphNormals.GetAdjustedDelta(x, y, pos[0] < 0, (byte)(morphMesh.getTagval(i) & 0x3F));
                        }
                    }

                    if (shapeVector != empty)
                    {
                        float vertWeight = Math.Min(((morphMesh.getTagval(i) & 0xFF00) >> 8) / 127f, 1f);
                        pos[0] -= shapeVector.X * morphShape.weight * vertWeight;
                        pos[1] -= shapeVector.Y * morphShape.weight * vertWeight;
                        pos[2] -= shapeVector.Z * morphShape.weight * vertWeight;
                        morphMesh.setPosition(i, pos);
                        if (morphNormals != null)
                        {
                            norm[0] -= normVector.X * morphNormals.weight * vertWeight;
                            norm[1] -= normVector.Y * morphNormals.weight * vertWeight;
                            norm[2] -= normVector.Z * morphNormals.weight * vertWeight;
                            morphMesh.setNormal(i, norm);
                        }
                    }
                }
            }

            return morphMesh;
        }

        private void PreviewShow_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            morphPreview1.Stop_Mesh();
            ShowMesh(false);
        }

        private void PreviewRemove_button_Click(object sender, EventArgs e)
        {
            PreviewBGEOfile.Text = "";
            PreviewShapeFile.Text = "";
            PreviewNormalsFile.Text = "";
            PreviewBONDfile.Text = "";
            morphBGEO = new List<BGEO>();
            morphShape = new List<MorphMap>();
            morphNormals = new List<MorphMap>();
            morphBOND = new List<BOND>();
            CurrentHeadMorph = CurrentHead != null ? new GEOM(CurrentHead) : null;
            CurrentBodyMorph = CurrentBody != null ? new GEOM(CurrentBody) : null;
            CurrentEarsMorph = CurrentEars != null ? new GEOM(CurrentEars) : null;
            CurrentTailMorph = CurrentTail != null ? new GEOM(CurrentTail) : null;

            if (CurrentCustom != null) CurrentCustomMorph = new GEOM(CurrentCustom);
            morphPreview1.Stop_Mesh();
            ShowMesh(false);
        }
        
        private void PreviewSpecies_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skipModelUpdate = true;
            currentSpecies = (Species)(PreviewSpecies_listBox.SelectedIndex + 1);
            if (currentSpecies == Species.Human)
            {
                currentGender = AgeGender.Female;
                PreviewAgeGender_listBox.Items.Clear();
                PreviewAgeGender_listBox.Items.AddRange(new string[] { "Infant", "Toddler", "Child", "Adult Male", "Adult Female" });
                PreviewAgeGender_listBox.SelectedIndex = 3;
                PreviewType1_listBox.Items.Clear();
                PreviewType1_listBox.Items.AddRange(new string[] { "Male Frame", "Female Frame" });
                PreviewType2_listBox.Items.Clear();
                PreviewType2_listBox.Items.AddRange(new string[] { "Skintight", "Robe" });
                SaveHeadMorph_button.Enabled = true;
            }
            else if (currentSpecies == Species.Werewolf)
            {
                currentGender = AgeGender.Male;
                PreviewAgeGender_listBox.Items.Clear();
                PreviewAgeGender_listBox.Items.AddRange(new string[] { "Adult Male", "Adult Female" });
                PreviewAgeGender_listBox.SelectedIndex = 0;
                PreviewType1_listBox.Items.Clear();
                PreviewType1_listBox.Items.AddRange(new string[] { "Male Frame", "Female Frame" });
                PreviewType2_listBox.Items.Clear();
                PreviewType2_listBox.Items.AddRange(new string[] { "Werewolf Morph", "Base" });
                SaveHeadMorph_button.Enabled = true;
            }
            else
            {
                currentGender = AgeGender.Unisex;
                PreviewAgeGender_listBox.Items.Clear();
                PreviewType1_listBox.Items.Clear();
                if (currentSpecies == Species.Horse)
                {
                    PreviewType1_listBox.Items.AddRange(new string[] { "Ears Up" });
                }else{
                    PreviewType1_listBox.Items.AddRange(new string[] { "Ears Up", "Ears Down" });
                }
                PreviewType1_listBox.SelectedIndex = 0;
                PreviewType2_listBox.Items.Clear();
                if (currentSpecies == Species.Horse)
                {
                    PreviewType2_listBox.Items.AddRange(new string[] { "Tail" });
                    PreviewAgeGender_listBox.Items.AddRange(new string[] { "Foal", "Adult" });
                }
                else if (currentSpecies == Species.Cat)
                {
                    PreviewType2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub" });
                    PreviewAgeGender_listBox.Items.AddRange(new string[] { "Kitten", "Adult" });
                }
                else
                {
                    PreviewType2_listBox.Items.AddRange(new string[] { "Tail Long", "Tail Stub", "Tail Ring", "Tail Screw" });
                    PreviewAgeGender_listBox.Items.AddRange(new string[] { "Puppy", "Adult" });
                }
                PreviewType2_listBox.SelectedIndex = 0;
                PreviewAgeGender_listBox.SelectedIndex = 1;
                SaveHeadMorph_button.Enabled = false;
            }
            SetPreviewAgeGender();
            skipModelUpdate = false;
            Set_AgeGenderFrame();
        }

        private void PreviewAgeGender_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            skipModelUpdate = true;
            SetPreviewAgeGender();
            skipModelUpdate = false;
            Set_AgeGenderFrame();
        }

        private void SetPreviewAgeGender()
        {
            if (currentSpecies == Species.Human)
            {
                if (PreviewAgeGender_listBox.SelectedIndex == 0) currentAge = AgeGender.Infant;
                else if (PreviewAgeGender_listBox.SelectedIndex == 1) currentAge = AgeGender.Toddler;
                else if (PreviewAgeGender_listBox.SelectedIndex == 2) currentAge = AgeGender.Child;
                else currentAge = AgeGender.Adult;
            }
            else if (currentSpecies == Species.Werewolf)
            {
                currentAge = AgeGender.Adult;
            }
            else
            {
                if (PreviewAgeGender_listBox.SelectedIndex == 0) currentAge = AgeGender.Child;
                else currentAge = AgeGender.Adult;
            }
            if (currentSpecies == Species.Human && currentAge == AgeGender.Adult)
            {
                if (PreviewAgeGender_listBox.SelectedIndex == 3) currentGender = AgeGender.Male;
                else currentGender = AgeGender.Female;
            }
            else if (currentSpecies == Species.Werewolf)
            {
                if (PreviewAgeGender_listBox.SelectedIndex == 0) currentGender = AgeGender.Male;
                else currentGender = AgeGender.Female;
            }
            else
            {
                currentGender = AgeGender.Unisex;
            } 
            if (currentSpecies == Species.Human)
            {
                if (currentGender == AgeGender.Male)
                {
                    PreviewType1_listBox.Enabled = true;
                    PreviewType1_listBox.SelectedIndex = 0;
                }
                else if (currentGender == AgeGender.Female)
                {
                    PreviewType1_listBox.Enabled = true;
                    PreviewType1_listBox.SelectedIndex = 1;
                }
            }
            else if (currentSpecies == Species.Werewolf)
            {
                PreviewType1_listBox.Enabled = false;
            }
            if (currentSpecies == Species.Human || currentSpecies == Species.Werewolf)
            {
                if (currentGender == AgeGender.Male)
                {
                    PreviewType2_listBox.Enabled = true;
                    PreviewType2_listBox.SelectedIndex = 0;
                }
                else if (currentGender == AgeGender.Female)
                {
                    PreviewType2_listBox.Enabled = true;
                    PreviewType2_listBox.SelectedIndex = 0;
                }
                else
                {
                    PreviewType1_listBox.ClearSelected();
                    PreviewType1_listBox.Enabled = false;
                    PreviewType2_listBox.ClearSelected();
                    PreviewType2_listBox.Enabled = false;
                }
            }
        }

        private void PreviewType1_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Set_AgeGenderFrame();
        }

        private void PreviewType2_listBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Set_AgeGenderFrame();
        }

        private void Set_AgeGenderFrame()
        {
            if (skipModelUpdate) return;
            currentLOD = 0;
            string prefix = GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            if (currentSpecies == Species.Human)
            {
                CurrentHead = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0"))));
                string bodyType = (currentAge == AgeGender.Adult && PreviewType2_listBox.SelectedIndex == 1) ? "Robe" : "Complete";
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Body" + bodyType + "_lod0"))));
                CurrentEars = null;
                CurrentTail = null;
                if (currentAge == AgeGender.Adult && currentGender == AgeGender.Male)
                {
                    if (PreviewType1_listBox.SelectedIndex == 1)
                    {
                        DMap shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_shape)));
                        DMap normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_normals)));
                        CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                    CurrentSkin = Properties.Resources.MaleSkinTan;
                }
                else if (currentAge == AgeGender.Adult && currentGender == AgeGender.Female)
                {
                    if (PreviewType1_listBox.SelectedIndex == 0)
                    {
                        DMap shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_shape)));
                        DMap normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_normals)));
                        CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                    CurrentSkin = Properties.Resources.FemaleSkinTan;
                }
                else if (currentAge == AgeGender.Child)
                {
                    CurrentSkin = Properties.Resources.ChildSkinTan;
                }
                else
                {
                    CurrentSkin = Properties.Resources.ToddlerSkinTan;
                }
            }
            else if (currentSpecies == Species.Werewolf)
            {
                CurrentHead = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod0"))));
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                CurrentEars = null;
                CurrentTail = null;
                DMap shape = null;
                DMap normals = null;
                if (currentAge == AgeGender.Adult && currentGender == AgeGender.Male)
                {
                    //if (PreviewType1_listBox.SelectedIndex == 1)
                    //{
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ym_to_female_normals)));
                    //    CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                    //    CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Normals)));
                    //}
                    //else
                    //{
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Normals)));
                    //}
                    if (PreviewType2_listBox.SelectedIndex == 0)
                    {
                        shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Shape)));
                        normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Normals)));
                        CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                }
                else if (currentAge == AgeGender.Adult && currentGender == AgeGender.Female)
                {
                    //if (PreviewType1_listBox.SelectedIndex == 0)
                    //{
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yf_to_male_normals)));
                    //    CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                    //    CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.ymWerewolf_Normals)));
                    //}
                    //else
                    //{
                    //    shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Shape)));
                    //    normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Normals)));
                    //}
                    if (PreviewType2_listBox.SelectedIndex == 0)
                    {
                        shape = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Shape)));
                        normals = new DMap(new BinaryReader(new MemoryStream(Properties.Resources.yfWerewolf_Normals)));
                        CurrentHead = LoadDMapMorph(CurrentHead, shape.ToMorphMap(), normals.ToMorphMap());
                        CurrentBody = LoadDMapMorph(CurrentBody, shape.ToMorphMap(), normals.ToMorphMap());
                    }
                }
                CurrentSkin = Properties.Resources.WerewolfSkin;
            }
            else if(currentSpecies == Species.Horse)
            {
                CurrentHead = null;
                CurrentSkin = Properties.Resources.HorseSkin;
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));     
                CurrentEars = null;
                string[] tailType = new string[] { "Tail" };
                CurrentTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[PreviewType2_listBox.SelectedIndex] + "_lod0"))));
            }
            else
            {
                CurrentHead = null;
                CurrentSkin = (currentSpecies == Species.Cat) ? Properties.Resources.CatSkin : Properties.Resources.DogSkin;
                CurrentBody = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "BodyComplete_lod0"))));
                string earType = PreviewType1_listBox.SelectedIndex == 0 ? "Ears" : "EarsDown";
                CurrentEars = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + earType + "_lod0"))));
                string[] tailType = new string[] { "Tail", "TailStub", "TailRing", "TailScrew" };
                CurrentTail = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + tailType[PreviewType2_listBox.SelectedIndex] + "_lod0"))));
            }

            ApplyAllMorphs(true, true, false);
            morphPreview1.Stop_Mesh();
            ShowMesh(true);
            skipLODcheck = true;
            PreviewLOD_comboBox.SelectedIndex = 0;
            skipLODcheck = false;
        }

        private void ApplyAllMorphs(bool head, bool body, bool custom)
        {
            if (head) CurrentHeadMorph = (CurrentHead != null) ? new GEOM(CurrentHead) : null;
            if (body)
            {
                CurrentBodyMorph = (CurrentBody != null) ? new GEOM(CurrentBody) : null;
                CurrentEarsMorph = (CurrentEars != null) ? new GEOM(CurrentEars) : null;
                CurrentTailMorph = (CurrentTail != null) ? new GEOM(CurrentTail) : null;
            }
            if (custom) CurrentCustomMorph = (CurrentCustom != null) ? new GEOM(CurrentCustom) : null;
           // RIG rig = GetTS4Rig(currentSpecies, currentAge);
            RIG rig = previewRig;
            foreach (BOND b in morphBOND)
            {
                if (head) CurrentHeadMorph = LoadBONDMorph(CurrentHeadMorph, b, rig);
                if (body) CurrentBodyMorph = LoadBONDMorph(CurrentBodyMorph, b, rig);
                if (custom) CurrentCustomMorph = LoadBONDMorph(CurrentCustomMorph, b, rig);
            }
            foreach (BGEO b in morphBGEO)
            {
                if (head) CurrentHeadMorph = LoadBGEOMorph(CurrentHeadMorph, b, currentLOD);
                if (custom) CurrentCustomMorph = LoadBGEOMorph(CurrentCustomMorph, b, currentLOD);
            }
            for (int i = 0; i < morphShape.Count; i++)
            {
                if (head) CurrentHeadMorph = LoadDMapMorph(CurrentHeadMorph, morphShape[i], morphNormals[i]);
                if (body)
                {
                    CurrentBodyMorph = LoadDMapMorph(CurrentBodyMorph, morphShape[i], morphNormals[i]);
                    CurrentEarsMorph = LoadDMapMorph(CurrentEarsMorph, morphShape[i], morphNormals[i]);
                    CurrentTailMorph = LoadDMapMorph(CurrentTailMorph, morphShape[i], morphNormals[i]);
                }
                if (custom) CurrentCustomMorph = LoadDMapMorph(CurrentCustomMorph, morphShape[i], morphNormals[i]);
            }
        }

        private void ShowMesh(bool resetView)
        {
            if (PreviewShow_checkBox.Checked)
            {
                if (PreviewShowModel_checkBox.Checked)
                {
                    morphPreview1.Start_Mesh(CurrentHeadMorph, CurrentBodyMorph, CurrentEarsMorph, CurrentTailMorph, CurrentCustomMorph, PreviewShow_Skin_checkBox.Checked ? CurrentSkin : null, resetView);

                }
                else
                {
                    morphPreview1.Start_Mesh(null, null, null, null, CurrentCustomMorph, PreviewShow_Skin_checkBox.Checked ? CurrentSkin : null, resetView);
                }
            }
            else
            {
                if (PreviewShowModel_checkBox.Checked)
                {
                    morphPreview1.Start_Mesh(CurrentHead, CurrentBody, CurrentEars, CurrentTail, CurrentCustom, PreviewShow_Skin_checkBox.Checked ? CurrentSkin : null, resetView);
                }
                else
                {
                    morphPreview1.Start_Mesh(null, null, null, null, CurrentCustom, PreviewShow_Skin_checkBox.Checked ? CurrentSkin : null, resetView);
                }
            }
        }

        private void PreviewLOD_comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (skipLODcheck) return;
            Set_LOD(PreviewLOD_comboBox.SelectedIndex);
        }
        private void Set_LOD(int lod)
        {
            if (currentSpecies != Species.Human) return;
            string prefix = GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            CurrentHead = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + "Head_lod" + lod.ToString()))));
            currentLOD = lod;
            ApplyAllMorphs(true, false, false);
            morphPreview1.Stop_Mesh();
            ShowMesh(false);
        }

        private void Skin_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            morphPreview1.Stop_Mesh();
            ShowMesh(false);
        }

        private void PreviewCustom_button_Click(object sender, EventArgs e)
        {
            PreviewCustom.Text = GetFilename("Select custom mesh", GEOMfilter);
            if (String.Compare(PreviewCustom.Text, " ") <= 0) return;
            GEOM geom;
            if (ReadGEOM(PreviewCustom.Text, out geom))
            {
                CurrentCustom = new GEOM(geom);
                CurrentCustomMorph = new GEOM(geom);
                PreviewShowModel_checkBox.Checked = false;
            }
            morphPreview1.Stop_Mesh();
            ShowMesh(true);
        }

        private void PreviewCustomRemove_button_Click(object sender, EventArgs e)
        {
            CurrentCustom = null;
            CurrentCustomMorph = null;
            PreviewCustom.Text = "";
            morphPreview1.Stop_Mesh();
            ShowMesh(true);
        }

        private void PreviewShowModel_checkBox_CheckedChanged(object sender, EventArgs e)
        {
            morphPreview1.Stop_Mesh();
            ShowMesh(false);
        }

        public static GEOM[] GetMorphedHeadMeshes(ResourceManager rm, string meshResourcePrefix, RIG rig, BGEO[] bgeos, MorphMap[] dmapShapes, MorphMap[] dmapNormals,
            BOND[] boneDeltas)
        {
            GEOM[] geoms = new GEOM[4];
            for (int i = 0; i < geoms.Length; i++)
            {
                geoms[i] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(meshResourcePrefix + "Head_lod" + i.ToString()))));
            }
            foreach (BGEO b in bgeos)
            {
                for (int j = 0; j < geoms.Length; j++)
                {
                    geoms[j] = LoadBGEOMorph(geoms[j], b, j);
                }
            }
            for (int i = 0; i < dmapShapes.Length; i++)
            {
                for (int j = 0; j < geoms.Length; j++)
                {
                    geoms[j] = LoadDMapMorph(geoms[j], dmapShapes[i], dmapNormals[i]);
                }
            }
            for (int i = 0; i < boneDeltas.Length; i++)
            {
                for (int j = 0; j < geoms.Length; j++)
                {
                    geoms[j] = LoadBONDMorph(geoms[j], boneDeltas[i], rig);
                }
            }
            return geoms;
        }

        public static GEOM[] GetMorphedBodyMeshes(ResourceManager rm, string prefix, string[] partNames, MorphMap[] dmapShapes, MorphMap[] dmapNormals)
        {
            GEOM[] geoms = new GEOM[partNames.Length];
            for (int i = 0; i < geoms.Length; i++)
            {
                geoms[i] = new GEOM(new BinaryReader(new MemoryStream((byte[])rm.GetObject(prefix + partNames[i] + "_lod0"))));
            }
            for (int i = 0; i < dmapShapes.Length; i++)
            {
                for (int j = 0; j < geoms.Length; j++)
                {
                    geoms[j] = LoadDMapMorph(geoms[j], dmapShapes[i], dmapNormals[i]);
                }
            }
            //for (int i = 0; i < boneDeltas.Length; i++)
            //{
            //    for (int j = 0; j < geoms.Length; j++)
            //    {
            //        geoms[j] = LoadBONDMorph(geoms[j], boneDeltas[i], rig);
            //    }
            //}
            return geoms;
        }

        private void SaveHeadMorph_button_Click(object sender, EventArgs e)
        {
            if (currentSpecies != Species.Human || currentSpecies == Species.Werewolf) return;
            if (Form.ModifierKeys == Keys.Control)
            {
                WriteGEOM("Save GEOM of morphed head", CurrentHeadMorph);
            }
            else
            {
                string prefix = GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
                GEOM[] lods = GetMorphedHeadMeshes(rm, prefix, previewRig, morphBGEO.ToArray(), morphShape.ToArray(), morphNormals.ToArray(), morphBOND.ToArray());

                if (Form.ModifierKeys == Keys.Shift)
                {
                    MS3D ms3d = new MS3D(lods, SelectRig(currentAge), 0, new string[] { "lod0Morph", "lod1Morph", "lod2Morph", "lod3Morph" });
                    WriteMS3D("Save MS3D of morphed head", ms3d, "");
                }
                else
                {
                    OBJ obj = new OBJ(lods, 0, true, new string[] { "lod0Morph", "lod1Morph", "lod2Morph", "lod3Morph" });
                    WriteOBJFile("Save OBJ of morphed head", obj, "");
                }
            }
        }

        private void SaveBodyMorph_button_Click(object sender, EventArgs e)
        {
            string prefix = GetBodyCompletePrefix(currentSpecies, currentAge, currentGender);
            List<GEOM> allMorph = new List<GEOM>();
            allMorph.Add(new GEOM(CurrentBodyMorph));
            List<string> allNames = new List<string>();
            allNames.Add(prefix + "MorphedBody");
            if (currentSpecies == Species.Human || currentSpecies == Species.Werewolf)
            {
                allMorph.Add(CurrentHeadMorph);
                allNames.Add("MorphedHead");
            }
            else
            {
                allMorph.Add(CurrentEarsMorph);
                allNames.Add("MorphedEars");
                allMorph.Add(CurrentTailMorph);
                allNames.Add("MorphedTail");
            }

            if (Form.ModifierKeys == Keys.Control)
            {
                for (int i = 1; i < allMorph.Count; i++)
                {
                    allMorph[0].AppendMesh(allMorph[i]);
                }
                WriteGEOM("Save GEOM of morphed model", allMorph[0]);
            }
            else if (Form.ModifierKeys == Keys.Shift)
            {
                MS3D ms3d = new MS3D(allMorph.ToArray(), SelectRig(currentAge, currentSpecies), 1, allNames.ToArray());
                WriteMS3D("Save MS3D of morphed model", ms3d, "");
            }
            else
            {
                OBJ obj = new OBJ(allMorph.ToArray(), 1, false, allNames.ToArray());
                WriteOBJFile("Save OBJ of morphed model", obj, "");
            }
        }

        private void SaveCustomMorph_button_Click(object sender, EventArgs e)
        {
            if (Form.ModifierKeys == Keys.Control)
            {
                WriteGEOM("Save GEOM of morphed custom mesh", CurrentCustomMorph);
            }
            else if (Form.ModifierKeys == Keys.Shift)
            {
                MS3D ms3d = new MS3D(new GEOM[] { CurrentCustomMorph }, SelectRig(currentAge), 0, new string[] { "CustomMorph" });
                WriteMS3D("Save MS3D of morphed custom mesh", ms3d, "");
            }
            else
            {
                OBJ obj = new OBJ(new GEOM[] { CurrentCustomMorph }, 0, false, new string[] { "CustomMorph" });
                WriteOBJFile("Save OBJ of morphed custom mesh", obj, "");
            }
        }

    }
}
