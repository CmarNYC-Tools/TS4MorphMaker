/* Xmods Data Library, a library to support tools for The Sims 4,
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
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Linq;

namespace MorphTool
{
    public class GEOM : IComparable<GEOM>
    {
        private int version1, count, ind3, extCount, intCount;
        private TGI dummyTGI;
        private int abspos, meshsize;
        private char[] magic;
        private int version, TGIoff, TGIsize;       // versions: base game 12, current 14
        private uint shaderHash;
        private int MTNFsize;
        private MTNF meshMTNF;
        private int mergeGrp, sortOrd, numVerts, Fcount;
        private vertexForm[] vertform = null;
        private position[] vPositions = null;
        private normal[] vNormals = null;
        private uv[][] vUVs = null;
        private Bones[] vBones = null;
        private tangent[] vTangents = null;
        private tagval[] vTags = null;
        private uint[] vertID = null;
        private int numSubMeshes;
        private byte bytesperfacepnt;
        private int numfacepoints;
        private Face[] meshfaces = null;
        private int skconIndex;  // index to skcon
        private int uvStitchCount;
        private UVStitch[] uvStitches = null;
        private int seamStitchCount;
        private SeamStitch[] seamStitches = null;
        private int slotCount;
        private SlotrayIntersection[] slotrayIntersections = null;
        private int bonehashcount;
        private uint[] bonehasharray = null;
        private int numtgi;
        private TGI[] meshTGIs = null;

        public Vector3[] deltaPosition;

        public bool UpdateToLatestVersion()         //latest version is 14 but meshes included here are V13
        {
            if (this.version < 13)
            {
                this.SetVersion(13);
                return true;
            }
            return false;
        }

        public void SetVersion(int newVersion)
        {
            if (newVersion == 5 && this.version > 5)
            {
                for (int i = 0; i < this.Fcount; i++)
                {
                    if (this.vertform[i].datatype == 5)
                    {
                        this.vertform[i].subtype = 1;
                        this.vertform[i].bytesper = 16;
                    }
                }
            }
            if (newVersion >= 12 & this.version == 5)
            {
                for (int i = 0; i < this.Fcount; i++)
                {
                    if (this.vertform[i].datatype == 5)
                    {
                        this.vertform[i].subtype = 2;
                        this.vertform[i].bytesper = 4;
                    }
                }
            }
            this.version = newVersion;
        }

        public int meshVersion
        {
            get { return this.version; }
        }
        public int numberVertices
        {
            get { return numVerts; }
        }
        public int mergeGroup
        {
            get { return this.mergeGrp; }
        }
        public int sortOrder
        {
            get { return this.sortOrd; }
        }
        public int numberSubMeshes
        {
            get { return this.numSubMeshes; }
        }
        public int bytesPerFacePoint
        {
            get { return this.bytesperfacepnt; }
        }
        public UVStitch[] UVStitches
        {
            get { return this.uvStitches; }
            set
            {
                if (value != null)
                {
                    this.uvStitches = value;
                    this.uvStitchCount = this.uvStitches.Length;
                }
                else
                {
                    this.uvStitches = new UVStitch[0];
                    this.uvStitchCount = 0;
                }
            }
        }
        internal int UVStitches_size
        {
            get
            {
                int tmp = 0;
                foreach (UVStitch a in this.uvStitches)
                {
                    tmp += a.Size;
                }
                return tmp;
            }
        }
        public SeamStitch[] SeamStitches
        {
            get { return this.seamStitches; }
            set
            {
                if (value != null)
                {
                    this.seamStitches = value;
                    this.seamStitchCount = this.seamStitches.Length;
                }
                else
                {
                    this.seamStitches = new SeamStitch[0];
                    this.seamStitchCount = 0;
                }
            }
        }
        public SlotrayIntersection[] SlotrayAdjustments
        {
            get { return this.slotrayIntersections; }
            set
            {
                if (value != null)
                {
                    this.slotrayIntersections = value;
                    this.slotCount = this.slotrayIntersections.Length;
                }
                else
                {
                    this.slotrayIntersections = new SlotrayIntersection[0];
                    this.slotCount = 0;
                }
            }
        }
        public Vector3[] SlotrayTrianglePositions(int slotrayAdjustmentIndex)
        {
            int[] vertexIndices = this.SlotrayAdjustments[slotrayAdjustmentIndex].TrianglePointIndices;
            return new Vector3[] { new Vector3(this.vPositions[vertexIndices[0]].Coordinates), 
                new Vector3(this.vPositions[vertexIndices[1]].Coordinates), new Vector3(this.vPositions[vertexIndices[2]].Coordinates) };
        }
        internal int slotrayAdjustments_size
        {
            get { return this.slotrayIntersections.Length * 63; }
        }

        public bool isValid
        {
            get
            {
                bool b = false;
                if (new string(this.magic) == "GEOM" && this.numVerts > 0 && (this.version == 5 || this.version == 12 || this.version == 13 || this.version == 14 || this.version == 15) && this.Fcount > 2)
                {
                    b = true;
                    int uvInd = 0;
                    for (int i = 0; i < this.vertexFormatList.Length; i++)
                    {
                        switch (this.vertexFormatList[i])
                        {
                            case (1):
                                if (this.vPositions.Length != this.numVerts) b = false;
                                break;
                            case (2):
                                if (this.vNormals.Length != this.numVerts) b = false;
                                break;
                            case (3):
                                if (this.vUVs[uvInd].Length != this.numVerts) b = false;
                                uvInd += 1;
                                break;
                            case (4):
                                if (this.vBones.Length != this.numVerts) b = false;
                                break;
                            case (5):
                                if (this.vBones.Length != this.numVerts) b = false;
                                break;
                            case (6):
                                if (this.vTangents.Length != this.numVerts) b = false;
                                break;
                            case (7):
                                if (this.vTags.Length != this.numVerts) b = false;
                                break;
                            case (10):
                                if (this.vertID.Length != this.numVerts) b = false;
                                break;
                            default:
                                break;
                        }
                    }
                }
                return b;
            }
        }

        public bool isBase
        {
            get { return (this.Fcount > 3); }
        }
        public bool isMorph
        {
            get
            {
                bool b = false;
                if (this.Fcount == 3)
                {
                    if (((this.vertform[0].datatype == 1) && (this.vertform[0].subtype == 1) && (this.vertform[0].bytesper == 12)) &
                        ((this.vertform[1].datatype == 2) && (this.vertform[1].subtype == 1) && (this.vertform[1].bytesper == 12)) &
                        ((this.vertform[2].datatype == 10) && (this.vertform[2].subtype == 4) && (this.vertform[2].bytesper == 4)))
                        b = true;
                }
                return b;
            }
        }
        public bool hasVertexIDs
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 10) > -1 && this.vertID.Length > 0);
            }
        }
        public bool vertexIDsAreSequential
        {
            get
            {
                for (uint i = 0; i < this.numVerts; i++)
                {
                    if (this.vertID[i] != i) return false;
                }
                return true;
            }
        }
        public bool hasPositions
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 1) > -1 && this.vPositions.Length > 0);
            }
        }
        public bool hasNormals
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 2) > -1 && this.vNormals.Length > 0);
            }
        }
        public bool hasUVs
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 3) > -1 && this.vUVs.Length > 0);
            }
        }
        public bool hasBones
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 4) > -1 && this.vBones.Length > 0);
            }
        }
        public bool hasTangents
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 6) > -1 && this.vTangents.Length > 0);
            }
        }
        public bool hasTags
        {
            get
            {
                return (Array.IndexOf(this.vertexFormatList, 7) > -1 && this.vTags.Length > 0);
            }
        }

        public int numberUVsets
        {
            get
            {
                if (vUVs == null)
                {
                    return 0;
                }
                else
                {
                    return vUVs.Length;
                }
            }
        }

        public vertexForm[] vertexFormat
        {
            get
            {
                return this.vertform;
            }
        }
        public int[] vertexFormatList
        {
            get
            {
                int[] tmp = new int[Fcount];
                for (int i = 0; i < Fcount; i++)
                {
                    tmp[i] = this.vertform[i].datatype;
                }
                return tmp;
            }
        }
        public int vertexDataLength
        {
            get
            {
                int vl = 0;
                for (int i = 0; i < Fcount; i++)
                {
                    vl = vl + vertform[i].bytesper;
                }
                return vl;
            }
        }
        public int minVertexID
        {
            get
            {
                if (vertID == null)
                {
                    return -1;
                }
                uint m = vertID[0];
                for (int i = 1; i < this.vertID.Length; i++)
                    if (m > vertID[i]) m = vertID[i];
                return (int)m;
            }
        }
        public int maxVertexID
        {
            get
            {
                if (vertID == null)
                {
                    return -1;
                }
                uint m = 0;
                for (int i = 0; i < this.vertID.Length; i++)
                    if (m < vertID[i]) m = vertID[i];
                return (int)m;
            }
        }
        public int numberFaces
        {
            get { return numfacepoints / 3; }
        }
        public int numberBones
        {
            get { return bonehasharray.Length; }
        }
        public uint[] BoneHashList
        {
            get { return bonehasharray; }
        }
        public int GetBoneIndex(uint boneHash)
        {
            for (int i = 0; i < this.numberBones; i++)
            {
                if (boneHash == this.bonehasharray[i]) return i;
            }
            return -1;
        }
        public int[] GetVertexIndicesAssignedtoBone(uint boneHash)
        {
            int index = GetBoneIndex(boneHash);
            if (index < 0) return new int[] {};
            List<int> verts = new List<int>();
            for (int i = 0; i < this.numberVertices; i++)
            {
                byte[] bones = this.getBones(i);
                byte[] weights = this.getBoneWeights(i);
                for (int j = 0; j < 4; j++)
                {
                    if (weights[j] > 0 && bones[j] == index) verts.Add(i);
                } 
            }
            return verts.ToArray();
        }
        public float GetBoneWeightForVertex(int vertexIndex, uint boneHash)
        {
            byte[] vertBones = this.getBones(vertexIndex);
            byte[] vertWeights = this.getBoneWeights(vertexIndex);
            for (int i = 0; i < 4; i++)
            {
                if (this.bonehasharray[vertBones[i]] == boneHash) return vertWeights[i] / 255f;
            }
            return 0f;
        }
        public bool validBones(int vertexSequenceNumber)
        {
            byte[] abones = this.getBones(vertexSequenceNumber);
            byte[] wbones = this.getBoneWeights(vertexSequenceNumber);
            if (version == 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (wbones[i] > 0 & (abones[i] < 0 | abones[i] >= this.numberBones)) return false;
                }
            }
            else if (version >= 12)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (abones[i] < 0 | abones[i] >= this.numberBones) return false;
                }
            }

            return true;
        }
        public string[] TGIListStr
        {
            get
            {
                string[] tmp = new string[numtgi];
                for (int i = 0; i < numtgi; i++)
                    tmp[i] = meshTGIs[i].ToString();
                return tmp;
            }
        }
        public TGI[] TGIList
        {
            get
            {
                TGI[] tmp = new TGI[numtgi];
                for (int i = 0; i < numtgi; i++)
                    tmp[i] = meshTGIs[i];
                return tmp;
            }
            set
            {
                this.numtgi = value.Length;
                TGI[] tmp = new TGI[this.numtgi];
                for (int i = 0; i < numtgi; i++)
                    tmp[i] = value[i];
                this.meshTGIs = tmp;
            }
        }

        public uint ShaderHash
        {
            get
            {
                return this.shaderHash;
            }
        }
        public MTNF Shader
        {
            get
            {
                return this.meshMTNF;
            }
        }
        public int skeletonIndex
        {
            get { return this.skconIndex; }
        }

        public void setShader(uint shaderHash)
        {
            this.shaderHash = shaderHash;
        }

        public void setShader(uint shaderHash, GEOM.MTNF shader)
        {
            this.shaderHash = shaderHash;
            this.meshMTNF = shader;
        }

        public void setTGI(int index, TGI tgi)
        {
            this.meshTGIs[index] = new TGI(tgi);
        }

        public bool vertexFormatEquals(int[] testFormatList)
        {
            int[] tmp = this.vertexFormatList;
            if (tmp.Length != testFormatList.Length) return false;
            for (int i = 0; i < tmp.Length; i++)
            {
                if (tmp[i] != testFormatList[i]) return false;
            }
            return true;
        }

        public bool hasUVset(int UVsequence)
        {
            return (Array.IndexOf(this.vertexFormatList, 3) > -1 && UVsequence < this.vUVs.Length && this.vUVs[UVsequence] != null);
        }

        public string[] DataString(int lineno)
        {
            int[] f = this.vertexFormatList;
            string[] str = new string[f.Length];
            int uvInd = 0;
            for (int i = 0; i < f.Length; i++)
            {
                switch (f[i])
                {
                    case (1):
                        str[i] = "Position: " + vPositions[lineno].ToString();
                        break;
                    case (2):
                        str[i] = "Normals: " + vNormals[lineno].ToString();
                        break;
                    case (3):
                        str[i] = "UV: " + vUVs[uvInd][lineno].ToString();
                        uvInd += 1;
                        break;
                    case (4):
                        str[i] = "Bones: " + vBones[lineno].ToString();
                        break;
                    case (6):
                        str[i] = "Tangents: " + vTangents[lineno].ToString();
                        break;
                    case (7):
                        str[i] = "TagVals: " + vTags[lineno].ToString();
                        break;
                    case (10):
                        str[i] = "Vertex ID: " + vertID[lineno].ToString();
                        break;
                    default:
                        break;
                }
            }
            return str;
        }
        public string VertexDataString(int vertexSequenceNumber)
        {
            int[] f = this.vertexFormatList;
            string str = "";
            int uvInd = 0;
            string s = " | ";
            for (int i = 0; i < f.Length; i++)
            {
                switch (f[i])
                {
                    case (1):
                        str = str + vPositions[vertexSequenceNumber].ToString() + s;
                        break;
                    case (2):
                        str = str + vNormals[vertexSequenceNumber].ToString() + s;
                        break;
                    case (3):
                        str = str + vUVs[uvInd][vertexSequenceNumber].ToString() + s;
                        uvInd += 1;
                        break;
                    case (4):
                        str = str + vBones[vertexSequenceNumber].ToString() + s;
                        break;
                    case (6):
                        str = str + vTangents[vertexSequenceNumber].ToString() + s;
                        break;
                    case (7):
                        str = str + vTags[vertexSequenceNumber].ToString() + s;
                        break;
                    case (10):
                        str = str + vertID[vertexSequenceNumber].ToString() + s;
                        break;
                    default:
                        break;
                }
            }
            int ind = str.LastIndexOf(s);
            str = str.Remove(ind);
            return str;
        }

        public int getVertexID(int vertexSequenceNumber)
        {
            return (int)this.vertID[vertexSequenceNumber];
        }
        public float[] getPosition(int vertexSequenceNumber)
        {
            return this.vPositions[vertexSequenceNumber].Data();
        }
        public float[] getNormal(int vertexSequenceNumber)
        {
            return this.vNormals[vertexSequenceNumber].Data();
        }
        public float[] getUV(int vertexSequenceNumber, int UVset)
        {
            return this.vUVs[UVset][vertexSequenceNumber].Data();
        }
        public byte[] getBones(int vertexSequenceNumber)
        {
            return this.vBones[vertexSequenceNumber].boneAssignments;
        }
        public float[] getBoneWeightsV5(int vertexSequenceNumber)
        {
            return this.vBones[vertexSequenceNumber].boneWeightsV5;
        }
        public byte[] getBoneWeights(int vertexSequenceNumber)
        {
            return this.vBones[vertexSequenceNumber].boneWeights;
        }
        public float[] getTangent(int vertexSequenceNumber)
        {
            return this.vTangents[vertexSequenceNumber].Data();
        }
        public uint getTagval(int vertexSequenceNumber)
        {
            return this.vTags[vertexSequenceNumber].Data();
        }
        public int[] getFaceIndices(int faceSequenceNumber)
        {
            return new int[] { (int)this.meshfaces[faceSequenceNumber].meshface[0], (int)this.meshfaces[faceSequenceNumber].meshface[1], (int)this.meshfaces[faceSequenceNumber].meshface[2] };
        }
        public Vector3[] getFacePoints(int faceSequenceNumber)
        {
            return new Vector3[] { new Vector3(this.vPositions[this.meshfaces[faceSequenceNumber].meshface[0]].Coordinates), 
                new Vector3(this.vPositions[this.meshfaces[faceSequenceNumber].meshface[1]].Coordinates), 
                new Vector3(this.vPositions[this.meshfaces[faceSequenceNumber].meshface[2]].Coordinates) };
        }

        public void setVertexID(int vertexSequenceNumber, int newVertexID)
        {
            this.vertID[vertexSequenceNumber] = (uint)newVertexID;
        }
        public void setPosition(int vertexSequenceNumber, float[] newPosition)
        {
            this.vPositions[vertexSequenceNumber] = new position(newPosition);
        }
        public void setPosition(int vertexSequenceNumber, float X, float Y, float Z)
        {
            this.vPositions[vertexSequenceNumber] = new position(X, Y, Z);
        }
        public void setNormal(int vertexSequenceNumber, float[] newNormal)
        {
            this.vNormals[vertexSequenceNumber] = new normal(newNormal);
        }
        public void setNormal(int vertexSequenceNumber, float X, float Y, float Z)
        {
            this.vNormals[vertexSequenceNumber] = new normal(X, Y, Z);
        }
        public void setUV(int vertexSequenceNumber, int UVset, float[] newUV)
        {
            this.vUVs[UVset][vertexSequenceNumber] = new uv(newUV[0], newUV[1]);
        }
        public void setUV(int vertexSequenceNumber, int UVset, float U, float V)
        {
            this.vUVs[UVset][vertexSequenceNumber] = new uv(U, V);
        }
        public void setBoneList(uint[] newBoneHashList)
        {
            this.bonehasharray = newBoneHashList;
            this.bonehashcount = newBoneHashList.Length;
        }
        public void setBones(int vertexSequenceNumber, byte[] newBones)
        {
            this.vBones[vertexSequenceNumber].boneAssignments = newBones;
        }
        public void setBones(int vertexSequenceNumber, byte bone0, byte bone1, byte bone2, byte bone3)
        {
            this.vBones[vertexSequenceNumber].boneAssignments = new byte[] { bone0, bone1, bone2, bone3 };
        }
        public void setBoneWeightsV5(int vertexSequenceNumber, float[] newWeights)
        {
            this.vBones[vertexSequenceNumber].boneWeightsV5 = newWeights;
        }
        public void setBoneWeightsV5(int vertexSequenceNumber, float weight0, float weight1, float weight2, float weight3)
        {
            this.vBones[vertexSequenceNumber].boneWeightsV5 = new float[] { weight0, weight1, weight2, weight3 };
        }
        public void setBoneWeights(int vertexSequenceNumber, byte[] newWeights)
        {
            this.vBones[vertexSequenceNumber].boneWeights = newWeights;
        }
        public void setBoneWeights(int vertexSequenceNumber, byte weight0, byte weight1, byte weight2, byte weight3)
        {
            this.vBones[vertexSequenceNumber].boneWeights = new byte[] { weight0, weight1, weight2, weight3 };
        }
        public void setTangent(int vertexSequenceNumber, float[] newTangent)
        {
            this.vTangents[vertexSequenceNumber] = new tangent(newTangent);
        }
        public void setTangent(int vertexSequenceNumber, float X, float Y, float Z)
        {
            this.vTangents[vertexSequenceNumber] = new tangent(X, Y, Z);
        }
        public void setTagval(int vertexSequenceNumber, uint newTag)
        {
            this.vTags[vertexSequenceNumber] = new tagval(newTag);
        }
        public void setBoneHashList(uint[] boneHashList)
        {
            this.bonehasharray = boneHashList;
            this.bonehashcount = boneHashList.Length;
        }

        public List<float[]> GetStitchUVs(int vertexIndex)
        {
            List<float[]> uvList = new List<float[]>();
            foreach (UVStitch s in this.uvStitches)
            {
                if (s.Index == vertexIndex)
                {
                    uvList.AddRange(s.UV1Coordinates);
                    break;
                }
            }
            return uvList;
        }

        public float[] GetHeightandDepth()
        {
            float yMax = 0, zMax = 0;
            foreach (position p in vPositions)
            {
                if (p.Y > yMax) yMax = p.Y;
                if (p.Z > zMax) zMax = p.Z;
            }
            return new float[] { yMax, zMax };
        }

        public GEOM() { }

        public GEOM(GEOM sourceMesh)
        {
            if (!sourceMesh.isValid)
            {
                throw new MeshException("Invalid source mesh, cannot construct new mesh!");
            }
            this.version1 = sourceMesh.version1;
            this.count = sourceMesh.count;
            this.ind3 = sourceMesh.ind3;
            this.extCount = sourceMesh.extCount;
            this.intCount = sourceMesh.intCount;
            this.dummyTGI = new TGI(sourceMesh.dummyTGI);
            this.abspos = sourceMesh.abspos;
            this.magic = sourceMesh.magic;
            this.version = sourceMesh.version;
            this.shaderHash = sourceMesh.shaderHash;
            this.MTNFsize = sourceMesh.MTNFsize;
            if (this.shaderHash > 0)
            {
                this.meshMTNF = new MTNF(sourceMesh.meshMTNF);
            }
            this.mergeGrp = sourceMesh.mergeGrp;
            this.sortOrd = sourceMesh.sortOrd;
            this.numVerts = sourceMesh.numberVertices;
            this.Fcount = sourceMesh.Fcount;
            this.vertform = new vertexForm[sourceMesh.Fcount];
            for (int i = 0; i < sourceMesh.Fcount; i++)
            {
                this.vertform[i] = new vertexForm(sourceMesh.vertform[i]);
            }
            if (sourceMesh.hasBones) this.vBones = new Bones[this.numVerts];
            for (int i = 0; i < this.vertform.Length; i++)
            {
                switch (this.vertform[i].datatype)
                {
                    case (1):
                        this.vPositions = new position[this.numVerts];
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            this.vPositions[j] = new position(sourceMesh.vPositions[j]);
                        }
                        break;
                    case (2):
                        this.vNormals = new normal[this.numVerts];
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            this.vNormals[j] = new normal(sourceMesh.vNormals[j]);
                        }
                        break;
                    case (3):
                        this.vUVs = new uv[sourceMesh.vUVs.Length][];
                        for (int j = 0; j < sourceMesh.vUVs.Length; j++)
                        {
                            this.vUVs[j] = new uv[this.numVerts];
                            for (int k = 0; k < this.numVerts; k++)
                            {
                                this.vUVs[j][k] = new uv(sourceMesh.vUVs[j][k]);
                            }
                        }
                        break;
                    case (4):
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            if (this.vBones[j] == null) this.vBones[j] = new Bones();
                            byte[] tmp = new byte[] { sourceMesh.vBones[j].boneAssignments[0], sourceMesh.vBones[j].boneAssignments[1], 
                                sourceMesh.vBones[j].boneAssignments[2], sourceMesh.vBones[j].boneAssignments[3] };
                            this.vBones[j].boneAssignments = tmp;
                        }
                        break;
                    case (5):
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            if (this.vBones[j] == null) this.vBones[j] = new Bones();
                            byte[] tmp = new byte[] { sourceMesh.vBones[j].boneWeights[0], sourceMesh.vBones[j].boneWeights[1], 
                                sourceMesh.vBones[j].boneWeights[2], sourceMesh.vBones[j].boneWeights[3] };
                            this.vBones[j].boneWeights = tmp;
                        }
                        break;
                    case (6):
                        this.vTangents = new tangent[this.numVerts];
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            this.vTangents[j] = new tangent(sourceMesh.vTangents[j]);
                        }
                        break;
                    case (7):
                        this.vTags = new tagval[this.numVerts];
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            this.vTags[j] = new tagval(sourceMesh.vTags[j]);
                        }
                        break;
                    case (10):
                        this.vertID = new uint[this.numVerts];
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            this.vertID[j] = sourceMesh.vertID[j];
                        }
                        break;
                    default:
                        break;
                }
            }

            this.numSubMeshes = sourceMesh.numSubMeshes;
            this.bytesperfacepnt = sourceMesh.bytesperfacepnt;
            this.numfacepoints = sourceMesh.numfacepoints;
            this.meshfaces = new Face[sourceMesh.meshfaces.Length];
            for (int i = 0; i < sourceMesh.meshfaces.Length; i++)
            {
                this.meshfaces[i] = new Face(sourceMesh.meshfaces[i]);
            }
            if (sourceMesh.version == 5)
            {
                this.skconIndex = sourceMesh.skconIndex;
            }
            else if (sourceMesh.version >= 12)
            {
                this.uvStitchCount = sourceMesh.uvStitchCount;
                uvStitches = new UVStitch[this.uvStitchCount];
                for (int i = 0; i < this.uvStitchCount; i++)
                {
                    uvStitches[i] = new UVStitch(sourceMesh.uvStitches[i]);
                }
                if (sourceMesh.version >= 13)
                {
                    this.seamStitchCount = sourceMesh.seamStitchCount;
                    seamStitches = new SeamStitch[this.seamStitchCount];
                    for (int i = 0; i < this.seamStitchCount; i++)
                    {
                        seamStitches[i] = new SeamStitch(sourceMesh.seamStitches[i]);
                    }
                }
                this.slotCount = sourceMesh.slotCount;
                slotrayIntersections = new SlotrayIntersection[this.slotCount];
                for (int i = 0; i < this.slotCount; i++)
                {
                    slotrayIntersections[i] = new SlotrayIntersection(sourceMesh.slotrayIntersections[i]);
                }
            }
            this.bonehashcount = sourceMesh.bonehashcount;
            this.bonehasharray = new uint[sourceMesh.bonehasharray.Length];
            for (int i = 0; i < sourceMesh.bonehasharray.Length; i++)
            {
                this.bonehasharray[i] = sourceMesh.bonehasharray[i];
            }
            this.numtgi = sourceMesh.numtgi;
            this.meshTGIs = new TGI[sourceMesh.meshTGIs.Length];
            for (int i = 0; i < sourceMesh.meshTGIs.Length; i++)
            {
                this.meshTGIs[i] = new TGI(sourceMesh.meshTGIs[i]);
            }
        }

        public GEOM(GEOM basemesh, Vector3[] delta_positions, Vector3[] delta_normals)
        {
            if (!basemesh.isValid | !basemesh.isBase | basemesh.numVerts <= 0)
            {
                throw new MeshException("Invalid base mesh, cannot construct new mesh!");
            }
            if (basemesh.numberVertices != delta_positions.Length | basemesh.numberVertices != delta_normals.Length)
            {
                throw new MeshException("Lists of positions and normals do not match number of base mesh vertices!");
            }
            this.version1 = basemesh.version1;
            this.count = basemesh.count;
            this.ind3 = basemesh.ind3;
            this.extCount = basemesh.extCount;
            this.intCount = basemesh.intCount;
            this.dummyTGI = new TGI(basemesh.dummyTGI.Type, 0u, 0ul);
            this.abspos = basemesh.abspos;
            this.magic = basemesh.magic;
            this.version = basemesh.version;
            this.shaderHash = 0;
            this.MTNFsize = 0;
            this.mergeGrp = basemesh.mergeGrp;
            this.sortOrd = basemesh.sortOrd;
            this.numVerts = basemesh.numberVertices;
            this.Fcount = 3;
            this.vertform = new vertexForm[3] { new vertexForm(1, 1, 12), new vertexForm(2, 1, 12), new vertexForm(10, 4, 4) };
            this.vPositions = new position[this.numVerts];
            this.vNormals = new normal[this.numVerts];
            this.vertID = basemesh.vertID;
            this.numSubMeshes = basemesh.numSubMeshes;
            this.bytesperfacepnt = basemesh.bytesperfacepnt;
            this.numfacepoints = basemesh.numfacepoints;
            this.meshfaces = basemesh.meshfaces;
            this.skconIndex = 0;
            this.bonehashcount = basemesh.bonehashcount;
            this.bonehasharray = basemesh.bonehasharray;
            this.numtgi = 1;
            this.meshTGIs = new TGI[1] { new TGI(0, 0, 0L) };

            for (int i = 0; i < this.numVerts; i++)
            {
                this.vPositions[i] = new position(delta_positions[i].X, delta_positions[i].Y, delta_positions[i].Z);
                this.vNormals[i] = new normal(delta_normals[i].X, delta_normals[i].Y, delta_normals[i].Z);
            }
        }

        //Converts MS3D to S4 format mesh
        public static GEOM[] GEOMsFromMS3D(MS3D ms3d, GEOM refMesh)
        {
            GEOM[] geomList = new GEOM[ms3d.NumberGroups];

            List<Face>[] groupFaces = new List<Face>[ms3d.NumberGroups];
            List<vertexData>[] groupVertList = new List<vertexData>[ms3d.NumberGroups];
            List<ushort>[] groupVertSequence = new List<ushort>[ms3d.NumberGroups];

            for (int i = 0; i < ms3d.NumberGroups; i++)
            {
                groupFaces[i] = new List<Face>();
                groupVertList[i] = new List<vertexData>();
                groupVertSequence[i] = new List<ushort>();
            }

            for (int groupIndex = 0; groupIndex < ms3d.NumberGroups; groupIndex++)
            {
                for (ushort vInd = 0; vInd < ms3d.VertexArray.Length; vInd++)
                {
                    foreach (ushort f in ms3d.GroupFaceIndices(groupIndex))
                    {
                        bool found = false;
                        for (int v = 0; v < ms3d.getFace(f).VertexIndices.Length; v++)
                        {
                            if (vInd == ms3d.getFace(f).VertexIndices[v])
                            {
                                groupVertSequence[groupIndex].Add(vInd);
                                groupVertList[groupIndex].Add(null);
                                found = true;
                                break;
                            }
                        }
                        if (found) break;
                    }
                }
            }

            for (int groupIndex = 0; groupIndex < ms3d.NumberGroups; groupIndex++)
            {
                foreach (ushort i in ms3d.GroupFaceIndices(groupIndex))
                {
                    ushort[] faceVerts = ms3d.getFace(i).VertexIndices;
                    ushort[] newFaceVerts = new ushort[3];
                    for (int j = 0; j < 3; j++)
                    {
                        sbyte[] tmpBones = ms3d.getBones(faceVerts[j]);
                        byte[] tmpWeights = ms3d.getBoneWeights(faceVerts[j]);
                        byte[] newBones = new byte[4];
                        byte[] newWeights = new byte[4];
                        for (int k = 0; k < 4; k++)
                        {
                            newBones[k] = (byte)Math.Max(0, (int)tmpBones[k]);
                            newWeights[k] = (byte)(Math.Round((double)tmpWeights[k] / 100.0 * 255.0));
                        }
                        vertexData vert = new vertexData(faceVerts[j], new position(ms3d.getVertex(faceVerts[j]).Position), new normal(ms3d.getFace(i).VertexNormals[j]),
                                                            new uv(ms3d.getFace(i).S[j], ms3d.getFace(i).T[j]), new Bones(newBones, newWeights),
                                                            new tagval(ms3d.getVertexExtra(faceVerts[j]).VertexColor), ms3d.getVertexExtra(faceVerts[j]).VertexID);
                        int tmp = groupVertSequence[groupIndex].IndexOf(faceVerts[j]);
                        if (tmp >= 0)
                        {
                            if (groupVertList[groupIndex][tmp] == null)
                            {
                                groupVertList[groupIndex][tmp] = vert;
                                newFaceVerts[j] = (ushort)tmp;
                            }
                            else if (vert.Equals(groupVertList[groupIndex][tmp]))
                            {
                                newFaceVerts[j] = (ushort)tmp;
                            }
                            else
                            {
                                int vertInd = -1;
                                for (int l = 0; l < groupVertList[groupIndex].Count; l++)
                                {
                                    if (groupVertList[groupIndex][l] != null && vert.Equals(groupVertList[groupIndex][l]))
                                    {
                                        vertInd = l;
                                        break;
                                    }
                                }
                                if (vertInd >= 0)
                                {
                                    newFaceVerts[j] = (ushort)vertInd;
                                }
                                else
                                {
                                    newFaceVerts[j] = (ushort)groupVertList[groupIndex].Count;
                                    groupVertList[groupIndex].Add(vert);
                                }
                            }
                        }
                        else
                        {
                            newFaceVerts[j] = (ushort)groupVertList[groupIndex].Count;
                            groupVertList[groupIndex].Add(vert);
                        }
                    }
                    groupFaces[groupIndex].Add(new Face(newFaceVerts));
                }
            }

            for (int i = 0; i < geomList.Length; i++)
            {
                geomList[i] = new GEOM();
                geomList[i].version1 = 3;
                geomList[i].count = 0;
                geomList[i].ind3 = 0;
                geomList[i].extCount = 0;
                geomList[i].intCount = 1;
                geomList[i].dummyTGI = new TGI(0, 0, 0);
                geomList[i].abspos = 0x2C;
                geomList[i].magic = new char[] { 'G', 'E', 'O', 'M' };
                geomList[i].version = refMesh.version;
                geomList[i].shaderHash = refMesh.shaderHash;
                geomList[i].MTNFsize = refMesh.MTNFsize;
                geomList[i].meshMTNF = new MTNF(refMesh.meshMTNF);
                geomList[i].mergeGrp = 0;
                geomList[i].sortOrd = 0;
                geomList[i].numVerts = groupVertList[i].Count;

                geomList[i].MatchFormats(refMesh.vertexFormat);
                List<position> groupPos = new List<position>();
                List<normal> groupNorm = new List<normal>();
                List<uv> groupUV = new List<uv>();
                List<tagval> groupTags = new List<tagval>();
                List<Bones> groupBones = new List<Bones>();
                List<uint> groupIDs = new List<uint>();

                //  groupVertList[i].Sort();
                foreach (vertexData v in groupVertList[i])
                {
                    groupPos.Add(v.p);
                    groupNorm.Add(v.n);
                    groupUV.Add(v.uv);
                    groupTags.Add(v.t);
                    groupBones.Add(v.b);
                    groupIDs.Add(v.id);
                }

                geomList[i].vPositions = groupPos.ToArray();
                geomList[i].vNormals = groupNorm.ToArray();
                geomList[i].vUVs = new uv[1][];
                geomList[i].vUVs[0] = groupUV.ToArray();
                geomList[i].vTags = groupTags.ToArray();
                geomList[i].vBones = groupBones.ToArray();
                geomList[i].vertID = groupIDs.ToArray();
                geomList[i].numSubMeshes = 1;
                geomList[i].bytesperfacepnt = 2;
                geomList[i].numfacepoints = groupFaces[i].Count * 3;
                geomList[i].meshfaces = groupFaces[i].ToArray();
                geomList[i].skconIndex = 0;
                geomList[i].bonehashcount = ms3d.JointArray.Length;
                geomList[i].bonehasharray = new uint[ms3d.JointArray.Length];
                for (int j = 0; j < ms3d.JointArray.Length; j++)
                {
                    geomList[i].bonehasharray[j] = FNVhash.FNV32(ms3d.JointArray[j].JointName);
                }
                geomList[i].numtgi = refMesh.numtgi;
                geomList[i].meshTGIs = refMesh.meshTGIs;
                if (geomList[i].version >= 12)
                {
                    geomList[i].uvStitches = new UVStitch[0];
                    geomList[i].slotrayIntersections = new SlotrayIntersection[0];
                }
                if (geomList[i].version >= 13)
                {
                    geomList[i].seamStitches = new SeamStitch[0];
                }
                geomList[i].vTangents = new tangent[geomList[i].numberVertices];
            }

            return geomList;
        }

        public static GEOM[] GEOMsFromOBJ(OBJ obj, GEOM refMesh)
        {
            obj.Sort();
            GEOM[] geomList = new GEOM[obj.numberGroups];
            for (int groupInd = 0; groupInd < geomList.Length; groupInd++)
            {
                OBJ.Point[] verts = obj.GroupPointArray(groupInd);
                geomList[groupInd] = new GEOM(refMesh);
                geomList[groupInd].numVerts = verts.Length;
                geomList[groupInd].vTags = new tagval[verts.Length];
                geomList[groupInd].vTangents = new tangent[verts.Length];
                geomList[groupInd].vUVs = new uv[1][];
                geomList[groupInd].vUVs[0] = new uv[verts.Length];
                geomList[groupInd].vertID = new uint[0];
                GEOM.position[] pos = new position[verts.Length];
                GEOM.normal[] norm = new normal[verts.Length];
                uint[] id = new uint[verts.Length];
                for (int j = 0; j < verts.Length; j++)
                {
                    pos[j] = new position(verts[j].Position.Coordinates);
                    norm[j] = new normal(verts[j].Normal.Coordinates);
                    id[j] = (uint)(verts[j].ID);
                }
                geomList[groupInd].vPositions = pos;
                geomList[groupInd].vNormals = norm;
                if (obj.hasIDs) geomList[groupInd].vertID = id;
                geomList[groupInd].numVerts = verts.Length;
            }

            return geomList;
        }

        public class GeometryState 
        {
            public uint State { get; set; }

            public int StartIndex { get; set; }
            public int StartFace => StartIndex / 3;

            public int MinVertexIndex { get; set; }

            public int VertexCount { get; set; }

            public int PrimitiveCount { get; set; }

            public void Read(BinaryReader s)
            {
                this.State = s.ReadUInt32();
                this.StartIndex = s.ReadInt32();
                this.MinVertexIndex = s.ReadInt32();
                this.VertexCount = s.ReadInt32();
                this.PrimitiveCount = s.ReadInt32();
            }

            public void Write(BinaryWriter s)
            {
                s.Write(this.State);
                s.Write(this.StartIndex);
                s.Write(this.MinVertexIndex);
                s.Write(this.VertexCount);
                s.Write(this.PrimitiveCount);
            }

        }
        public List<GeometryState> GeometryStates { get; set; } = new();
        public GEOM(BinaryReader br)
        {
            this.ReadFile(br);
        }

        public void ReadFile(BinaryReader br)
        {
            if (br.BaseStream.Length < 12) return;
            br.BaseStream.Position = 0;
            this.version1 = br.ReadInt32();
            this.count = br.ReadInt32();
            this.ind3 = br.ReadInt32();
            this.extCount = br.ReadInt32();
            this.intCount = br.ReadInt32();
            this.dummyTGI = new TGI(br);
            this.abspos = br.ReadInt32();
            this.meshsize = br.ReadInt32();
            this.magic = br.ReadChars(4);
            if (new string(this.magic) != "GEOM")
            {
                throw new MeshException("Not a valid GEOM file.");
            }
            this.version = br.ReadInt32();
            this.TGIoff = br.ReadInt32();
            this.TGIsize = br.ReadInt32();
            this.shaderHash = br.ReadUInt32();
            if (this.shaderHash != 0)
            {
                this.MTNFsize = br.ReadInt32();
                this.meshMTNF = new MTNF(br);
            }
            this.mergeGrp = br.ReadInt32();
            this.sortOrd = br.ReadInt32();
            this.numVerts = br.ReadInt32();
            this.Fcount = br.ReadInt32();
            this.vertform = new vertexForm[this.Fcount];
            for (int i = 0; i < this.Fcount; i++)
            {
                this.vertform[i] = new vertexForm(br);
            }
            int[] f = this.vertexFormatList;
            int uvInd = 0;
            for (int i = 0; i < f.Length; i++)
            {
                switch (f[i])
                {
                    case (1):
                        this.vPositions = new position[this.numVerts];
                        break;
                    case (2):
                        this.vNormals = new normal[this.numVerts];
                        break;
                    case (3):
                        uvInd += 1;
                        break;
                    case (4):
                        this.vBones = new Bones[this.numVerts];
                        break;
                    case (6):
                        this.vTangents = new tangent[this.numVerts];
                        break;
                    case (7):
                        this.vTags = new tagval[this.numVerts];
                        break;
                    case (10):
                        this.vertID = new uint[this.numVerts];
                        break;
                    default:
                        break;
                }
            }
            if (uvInd > 0) this.vUVs = new uv[uvInd][];
            for (int i = 0; i < uvInd; i++)
            {
                this.vUVs[i] = new uv[this.numVerts];
            }
            for (int i = 0; i < this.numVerts; i++)
            {
                int UVind = 0;
                for (int j = 0; j < this.Fcount; j++)
                {
                    switch (this.vertform[j].datatype)
                    {
                        case (1):
                            this.vPositions[i] = new position(br);
                            break;
                        case (2):
                            this.vNormals[i] = new normal(br);
                            break;
                        case (3):
                            this.vUVs[UVind][i] = new uv(br);
                            UVind += 1;
                            break;
                        case (4):
                            if (this.vBones[i] == null) this.vBones[i] = new Bones();
                            this.vBones[i].ReadAssignments(br);
                            break;
                        case (5):
                            if (this.vBones[i] == null) this.vBones[i] = new Bones();
                            this.vBones[i].ReadWeights(br, this.vertform[j].subtype);
                            break;
                        case (6):
                            this.vTangents[i] = new tangent(br);
                            break;
                        case (7):
                            this.vTags[i] = new tagval(br);
                            break;
                        case (10):
                            this.vertID[i] = br.ReadUInt32();
                            break;
                        default:
                            break;
                    }
                }
            }
            this.numSubMeshes = br.ReadInt32();
            this.bytesperfacepnt = br.ReadByte();
            this.numfacepoints = br.ReadInt32();
            this.meshfaces = new Face[this.numfacepoints / 3];
            for (int i = 0; i < this.numfacepoints / 3; i++)
            {
                this.meshfaces[i] = new Face(br, this.bytesperfacepnt);
            }
            if (this.version == 5)
            {
                this.skconIndex = br.ReadInt32();
            }
            else if (this.version >= 12)
            {
                this.uvStitchCount = br.ReadInt32();
                uvStitches = new UVStitch[this.uvStitchCount];
                for (int i = 0; i < this.uvStitchCount; i++)
                {
                    uvStitches[i] = new UVStitch(br);
                }
                if (this.version >= 13)
                {
                    this.seamStitchCount = br.ReadInt32();
                    seamStitches = new SeamStitch[this.seamStitchCount];
                    for (int i = 0; i < this.seamStitchCount; i++)
                    {
                        seamStitches[i] = new SeamStitch(br);
                    }
                }
                this.slotCount = br.ReadInt32();
                slotrayIntersections = new SlotrayIntersection[this.slotCount];
                for (int i = 0; i < this.slotCount; i++)
                {
                    slotrayIntersections[i] = new SlotrayIntersection(br, this.version);
                }
            }
            this.bonehashcount = br.ReadInt32();
            this.bonehasharray = new uint[this.bonehashcount];
            for (int i = 0; i < this.bonehashcount; i++)
            {
                this.bonehasharray[i] = br.ReadUInt32();
            }
            if (br.BaseStream.Length <= br.BaseStream.Position) return;
            if (this.version >= 15)
            {
                var c = br.ReadInt32();
                for (int i = 0; i < c; i++)
                {
                    var gs = new GeometryState();
                    gs.Read(br);
                    this.GeometryStates.Add(gs);
                }
            }
            this.numtgi = br.ReadInt32();
            this.meshTGIs = new TGI[this.numtgi];
            for (int i = 0; i < this.numtgi; i++)
            {
                this.meshTGIs[i] = new TGI(br);
            }
            if (this.isMorph & this.skconIndex >= this.numtgi) this.skconIndex = 0;

            return;
        }

        public void WriteFile(BinaryWriter bw)
        {
            int tmp = 0;
            if (this.meshMTNF != null)
            {
                this.MTNFsize = this.meshMTNF.chunkSize;
            }
            else
            {
                this.MTNFsize = 0;
            }
            if (this.shaderHash != 0) tmp = this.MTNFsize + 4;
            this.TGIoff = 37 + (this.Fcount * 9) + tmp + (this.numVerts * this.vertexDataLength) + (this.numfacepoints * this.bytesperfacepnt) + (this.bonehashcount * 4);
            if (this.version == 5)
            {
                this.TGIoff += 4;
            }
            else if (this.version >= 12)
            {
                this.TGIoff += 8 + this.UVStitches_size + this.slotrayAdjustments_size;
                if (this.version >= 13) this.TGIoff += 4 + (this.seamStitchCount * 6);
            }
            this.meshsize = this.TGIoff + 16 + (this.numtgi * 16);
            this.TGIsize = 4 + (this.numtgi * 16);

            bw.Write(this.version1);
            bw.Write(this.count);
            bw.Write(this.ind3);
            bw.Write(this.extCount);
            bw.Write(this.intCount);
            dummyTGI.Write(bw);
            bw.Write(this.abspos);
            bw.Write(this.meshsize);
            bw.Write(this.magic);
            bw.Write(this.version);
            bw.Write(this.TGIoff);
            bw.Write(this.TGIsize);
            bw.Write(this.shaderHash);
            if (this.shaderHash != 0)
            {
                bw.Write(this.MTNFsize);
                this.meshMTNF.Write(bw);
            }
            bw.Write(this.mergeGrp);
            bw.Write(this.sortOrd);
            bw.Write(this.numVerts);
            bw.Write(this.Fcount);
            for (int i = 0; i < this.Fcount; i++)
            {
                this.vertform[i].vertexformatWrite(bw);
            }

            for (int i = 0; i < this.numVerts; i++)
            {
                int UVind = 0;
                for (int j = 0; j < this.Fcount; j++)
                {
                    switch (this.vertform[j].datatype)
                    {
                        case (1):
                            this.vPositions[i].Write(bw);
                            break;
                        case (2):
                            this.vNormals[i].Write(bw);
                            break;
                        case (3):
                            this.vUVs[UVind][i].Write(bw);
                            UVind += 1;
                            break;
                        case (4):
                            this.vBones[i].WriteAssignments(bw, this.version, this.bonehashcount - 1);
                            break;
                        case (5):
                            this.vBones[i].WriteWeights(bw, this.version);
                            break;
                        case (6):
                            this.vTangents[i].Write(bw);
                            break;
                        case (7):
                            this.vTags[i].Write(bw);
                            break;
                        case (10):
                            bw.Write(this.vertID[i]);
                            break;
                        default:
                            break;
                    }
                }
            }

            bw.Write(this.numSubMeshes);
            bw.Write(this.bytesperfacepnt);
            bw.Write(this.numfacepoints);
            for (int i = 0; i < this.numfacepoints / 3; i++)
            {
                this.meshfaces[i].Write(bw, this.bytesperfacepnt);
            }
            if (this.version == 5)
            {
                bw.Write(this.skconIndex);
            }
            else if (this.version >= 12)
            {
                bw.Write(this.uvStitchCount);
                for (int i = 0; i < this.uvStitchCount; i++)
                {
                    uvStitches[i].Write(bw);
                }
                if (this.version >= 13)
                {
                    bw.Write(this.seamStitchCount);
                    for (int i = 0; i < this.seamStitchCount; i++)
                    {
                        seamStitches[i].Write(bw);
                    }
                }
                bw.Write(this.slotCount);
                for (int i = 0; i < this.slotCount; i++)
                {
                    slotrayIntersections[i].Write(bw);
                }
            }
            bw.Write(this.bonehashcount);
            for (int i = 0; i < this.bonehashcount; i++)
            {
                bw.Write(this.bonehasharray[i]);
            }
            if (this.version >= 15)
            {
                bw.Write(this.GeometryStates.Count);
                foreach (var geometryState in this.GeometryStates)
                {
                    geometryState.Write(bw);
                }
            }
            bw.Write(this.numtgi);
            for (int i = 0; i < this.numtgi; i++)
            {
                this.meshTGIs[i].Write(bw);
            }

            return;
        }

        public void MatchPartSeamStitches()
        {
            if (this.seamStitches == null) return;
            List<ushort> ids = new List<ushort>();
            List<int> numHits = new List<int>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            foreach (SeamStitch s in this.seamStitches)
            {
                int i = ids.IndexOf(s.VertID);
                if (i < 0)
                {
                    Vector3 pos = new Vector3(this.vPositions[s.Index].Coordinates);
                    Vector3 norm = new Vector3(this.vNormals[s.Index].Coordinates);
                    ids.Add(s.VertID);
                    positions.Add(pos);
                    normals.Add(norm);
                    numHits.Add(1);
                }
                else
                {
                    Vector3 pos = new Vector3(this.vPositions[s.Index].Coordinates);
                    Vector3 norm = new Vector3(this.vNormals[s.Index].Coordinates);
                    Vector3 newPos = (pos + (numHits[i] * positions[i])) / (numHits[i] + 1);
                    Vector3 newNorm = (norm + (numHits[i] * normals[i])) / (numHits[i] + 1);
                    positions[i] = newPos;
                    normals[i] = newNorm;
                    numHits[i]++;
                }
            }

            foreach (SeamStitch s in this.seamStitches)
            {
                int i = ids.IndexOf(s.VertID);
                if (i >= 0)
                {
                    this.vPositions[s.Index] = new position(positions[i].Coordinates);
                    this.vNormals[s.Index] = new normal(normals[i].Coordinates);
                }
            }
        }

        public static void MatchSeamStitches(GEOM geom1, GEOM geom2)
        {
            List<ushort> ids = new List<ushort>();
            List<Vector3> positions = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            foreach (SeamStitch s1 in geom1.seamStitches)
            {
                foreach (SeamStitch s2 in geom2.seamStitches)
                {
                    if (s1.VertID == s2.VertID)
                    {
                        int i = ids.IndexOf(s1.VertID);
                        if (i >= 0)
                        {
                            geom1.vPositions[s1.Index] = new position(positions[i].Coordinates);
                            geom2.vPositions[s2.Index] = new position(positions[i].Coordinates);
                            geom1.vNormals[s1.Index] = new normal(normals[i].Coordinates);
                            geom2.vNormals[s2.Index] = new normal(normals[i].Coordinates);
                        }
                        else
                        {
                            Vector3 pos1 = new Vector3(geom1.vPositions[s1.Index].Coordinates);
                            Vector3 pos2 = new Vector3(geom2.vPositions[s2.Index].Coordinates);
                            Vector3 pos = (pos1 + pos2) / 2f;
                            geom1.vPositions[s1.Index] = new position(pos.Coordinates);
                            geom2.vPositions[s2.Index] = new position(pos.Coordinates);
                            Vector3 norm1 = new Vector3(geom1.vNormals[s1.Index].Coordinates);
                            Vector3 norm2 = new Vector3(geom2.vNormals[s2.Index].Coordinates);
                            Vector3 norm = (norm1 + norm2) / 2f;
                            geom1.vNormals[s1.Index] = new normal(norm.Coordinates);
                            geom2.vNormals[s2.Index] = new normal(norm.Coordinates);
                            ids.Add(s1.VertID);
                            positions.Add(pos);
                            normals.Add(norm);
                        }
                    }
                }
            }
        }

        public float GetTotalBoneWeight(int vertexIndex, List<uint> boneHashes)
        {
            byte[] vertBones = this.getBones(vertexIndex);
            byte[] vertWeights = this.getBoneWeights(vertexIndex);
            float weight = 0f;
            for (int i = 0; i < 4; i++)
            {
                if (boneHashes.Contains(this.bonehasharray[vertBones[i]])) weight += vertWeights[i] / 255f;
            }
            return weight;
        }

        internal void SetupDeltas()
        {
            this.deltaPosition = new Vector3[this.vPositions.Length];
            for (int i = 0; i < this.vPositions.Length; i++)
            {
                this.deltaPosition[i] = new Vector3();
            }
        }

        internal void UpdatePositions()
        {
            for (int i = 0; i < this.vPositions.Length; i++)
            {
                this.vPositions[i] = new position((new Vector3(this.getPosition(i)) + this.deltaPosition[i]).Coordinates);
            }
        }

        internal void BoneMorpher(RIG.Bone bone, float weight, Vector3 offset, Vector3 scale, Quaternion rotation)
        {
            if (weight == 0) return;
            Vector3 unit = new Vector3(1f, 1f, 1f);
            List<uint> boneHashes = new List<uint>();
            bone.Rig.GetDescendants(bone.BoneHash, ref boneHashes);

            for (int i = 0; i < this.vPositions.Length; i++)
            {
                float boneWeight = GetTotalBoneWeight(i, boneHashes);
                if (boneWeight == 0f) continue;
                float adjustedWeight = boneWeight * weight;
                Vector3 weightedScale = (scale * adjustedWeight) + unit;
                Vector3 weightedOffset = offset * adjustedWeight;
                Quaternion weightedRotation = rotation * adjustedWeight;
                Matrix4D weightedTransform = weightedRotation.toMatrix4x4(weightedOffset, weightedScale);
                Vector3 vertPos = new Vector3(this.getPosition(i)) - bone.WorldPosition;
                vertPos = weightedTransform * vertPos;
                vertPos += bone.WorldPosition;

                this.deltaPosition[i] += vertPos - new Vector3(this.getPosition(i));

                //  this.setPosition(i, vertPos.Coordinates);

                Matrix4D normalTransform = weightedTransform.Inverse().Transpose();
                Vector3 vertNorm = new Vector3(this.getNormal(i));
                //  vertNorm = (weightedRotation * vertNorm * weightedRotation.Conjugate()).toVector3();
                vertNorm = (normalTransform * vertNorm);
                vertNorm.Normalize();
                //  vertNorm += bone.WorldPosition;
                this.setNormal(i, vertNorm.Coordinates);
            }
        }

        public void MatchFormats(vertexForm[] vertexFormatToMatch)
        {
            int uvInd = 0;
            for (int i = 0; i < vertexFormatToMatch.Length; i++)
            {
                switch (vertexFormatToMatch[i].datatype)
                {
                    case (1):
                        if (this.vPositions == null || this.vPositions.Length != this.numVerts)
                        {
                            this.vPositions = new position[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vPositions[j] = new position();
                            }
                        }
                        break;
                    case (2):
                        if (this.vNormals == null || this.vNormals.Length != this.numVerts)
                        {
                            this.vNormals = new normal[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vNormals[j] = new normal();
                            }
                        }
                        break;
                    case (3):
                        uvInd += 1;
                        break;
                    case (4):
                        if (this.vBones == null || this.vBones.Length != this.numVerts)
                        {
                            this.vBones = new Bones[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vBones[j] = new Bones();
                            }
                        }
                        break;
                    case (5):
                        if (this.vBones == null || this.vBones.Length != this.numVerts)
                        {
                            this.vBones = new Bones[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vBones[j] = new Bones();
                            }
                        }
                        break;
                    case (6):
                        if (!this.hasTangents) this.vTangents = new tangent[this.numVerts];
                        break;
                    case (7):
                        if (this.vTags == null || this.vTags.Length != this.numVerts)
                        {
                            this.vTags = new tagval[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vTags[j] = new tagval();
                            }
                        }
                        break;
                    case (10):
                        if (this.vertID == null || this.vertID.Length != this.numVerts)
                        {
                            this.vertID = new uint[this.numVerts];
                            for (int j = 0; j < this.numVerts; j++)
                            {
                                this.vertID[j] = 0;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            if (uvInd > 0 && this.numberUVsets != uvInd)
            {
                uv[][] newUV = new uv[uvInd][];
                for (int i = 0; i < uvInd; i++)
                {
                    newUV[i] = new uv[this.numVerts];
                    if (this.hasUVset(i))
                    {
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            newUV[i][j] = this.vUVs[i][j];
                        }
                    }
                    else
                    {
                        for (int j = 0; j < this.numVerts; j++)
                        {
                            newUV[i][j] = new uv();
                        }
                    }
                }
                this.vUVs = newUV;
            }
            vertexForm[] newFormat = new vertexForm[vertexFormatToMatch.Length];
            for (int i = 0; i < vertexFormatToMatch.Length; i++)
            {
                newFormat[i] = new vertexForm(vertexFormatToMatch[i].formatDataType, vertexFormatToMatch[i].formatSubType, vertexFormatToMatch[i].formatDataLength);
            }
            this.vertform = newFormat;
            this.Fcount = this.vertform.Length;
        }

        public void AppendMesh(GEOM meshToAppend)
        {
            if (meshToAppend == null) return;
            if (!this.isValid) throw new MeshException("Not a valid mesh!");
            if (!meshToAppend.isValid) throw new MeshException("The mesh to be appended is not a valid mesh!");

            int uvInd = 0;
            for (int i = 0; i < this.vertexFormatList.Length; i++)
            {
                switch (this.vertexFormatList[i])
                {
                    case (1):
                        position[] newPos = new position[this.numVerts + meshToAppend.numVerts];
                        Array.Copy(this.vPositions, 0, newPos, 0, this.numVerts);
                        Array.Copy(meshToAppend.vPositions, 0, newPos, this.numVerts, meshToAppend.numVerts);
                        this.vPositions = newPos;
                        break;
                    case (2):
                        normal[] newNorm = new normal[this.numVerts + meshToAppend.numVerts];
                        Array.Copy(this.vNormals, 0, newNorm, 0, this.numVerts);
                        Array.Copy(meshToAppend.vNormals, 0, newNorm, this.numVerts, meshToAppend.numVerts);
                        this.vNormals = newNorm;
                        break;
                    case (3):
                        uvInd += 1;
                        break;
                    case (4):
                        Bones[] newBones = new Bones[this.numVerts + meshToAppend.numVerts];
                        List<uint> tmpBoneHash = new List<uint>(this.bonehasharray);
                        foreach (uint h in meshToAppend.bonehasharray)
                        {
                            if (tmpBoneHash.IndexOf(h) < 0)
                            {
                                tmpBoneHash.Add(h);         //add bones from second mesh
                            }
                        }
                        uint[] newbonehasharray = tmpBoneHash.ToArray();

                        for (int j = 0; j < this.numVerts; j++)                 //add updated bone assignments for this mesh
                        {
                            byte[] oldBones = this.getBones(j);
                            byte[] oldWeights = this.getBoneWeights(j);
                            byte[] tmpBones = new byte[oldBones.Length];
                            for (int k = 0; k < oldBones.Length; k++)
                            {
                                if (oldWeights[k] > 0 & oldBones[k] < this.bonehasharray.Length)                         // if it's a valid bone
                                {                                               // find index of bone hash in new bone list
                                    tmpBones[k] = (byte)Array.IndexOf(newbonehasharray, this.bonehasharray[oldBones[k]]);
                                }
                                else
                                {
                                    tmpBones[k] = 0;
                                }
                            }
                            newBones[j] = new Bones(tmpBones, oldWeights);
                        }
                        for (int j = 0; j < meshToAppend.numVerts; j++)        //add updated bone assignments for appended mesh
                        {
                            byte[] oldBones = meshToAppend.getBones(j);
                            byte[] oldWeights = meshToAppend.getBoneWeights(j);
                            byte[] tmpBones = new byte[oldBones.Length];
                            for (int k = 0; k < oldBones.Length; k++)
                            {
                                if (oldWeights[k] > 0 & oldBones[k] < meshToAppend.bonehasharray.Length)
                                {
                                    tmpBones[k] = (byte)Array.IndexOf(newbonehasharray, meshToAppend.bonehasharray[oldBones[k]]);
                                }
                                else
                                {
                                    tmpBones[k] = 0;
                                }
                            }
                            newBones[j + this.numVerts] = new Bones(tmpBones, oldWeights);
                        }
                        this.vBones = newBones;
                        this.bonehasharray = newbonehasharray;
                        this.bonehashcount = newbonehasharray.Length;
                        break;
                    case (6):
                        tangent[] newTan = new tangent[this.numVerts + meshToAppend.numVerts];
                        Array.Copy(this.vTangents, 0, newTan, 0, this.numVerts);
                        Array.Copy(meshToAppend.vTangents, 0, newTan, this.numVerts, meshToAppend.numVerts);
                        this.vTangents = newTan;
                        break;
                    case (7):
                        tagval[] newTag = new tagval[this.numVerts + meshToAppend.numVerts];
                        Array.Copy(this.vTags, 0, newTag, 0, this.numVerts);
                        Array.Copy(meshToAppend.vTags, 0, newTag, this.numVerts, meshToAppend.numVerts);
                        this.vTags = newTag;
                        break;
                    case (10):
                        uint[] newIDs = new uint[this.numVerts + meshToAppend.numVerts];
                        Array.Copy(this.vertID, 0, newIDs, 0, this.numVerts);
                        Array.Copy(meshToAppend.vertID, 0, newIDs, this.numVerts, meshToAppend.numVerts);
                        this.vertID = newIDs;
                        break;
                    default:
                        break;
                }
            }
            if (uvInd > 0)
            {
                uv[][] newUV = new uv[uvInd][];
                for (int i = 0; i < uvInd; i++)
                {
                    newUV[i] = new uv[this.numVerts + meshToAppend.numVerts];
                    for (int j = 0; j < this.numVerts; j++)
                    {
                        newUV[i][j] = this.vUVs[i][j];
                    }
                    for (int j = 0; j < meshToAppend.numVerts; j++)
                    {
                        newUV[i][j + this.numVerts] = meshToAppend.vUVs[i][j];
                    }
                }
                this.vUVs = newUV;
            }

            Face[] newFaces = new Face[this.numberFaces + meshToAppend.numberFaces];
            Array.Copy(this.meshfaces, 0, newFaces, 0, this.numberFaces);
            for (int i = 0; i < meshToAppend.numberFaces; i++)
            {
                newFaces[i + this.numberFaces] = new Face(meshToAppend.meshfaces[i].facePoint0 + this.numVerts,
                    meshToAppend.meshfaces[i].facePoint1 + this.numVerts, meshToAppend.meshfaces[i].facePoint2 + this.numVerts);
            }
            this.meshfaces = newFaces;

            if (this.version >= 12)
            {
                UVStitch[] adj = new UVStitch[this.uvStitches.Length + meshToAppend.uvStitches.Length];
                Array.Copy(this.uvStitches, 0, adj, 0, this.uvStitches.Length);
                for (int i = 0; i < meshToAppend.uvStitches.Length; i++)
                {
                    adj[i + this.uvStitches.Length] = new UVStitch(meshToAppend.uvStitches[i]);
                    adj[i + this.uvStitches.Length].Index += this.numVerts;
                }
                this.uvStitches = adj;
                this.uvStitchCount = adj.Length;
                if (this.version >= 13)
                {
                    SeamStitch[] seam = new SeamStitch[this.seamStitches.Length + meshToAppend.seamStitches.Length];
                    Array.Copy(this.seamStitches, 0, seam, 0, this.seamStitches.Length);
                    for (int i = 0; i < meshToAppend.seamStitches.Length; i++)
                    {
                        seam[i + this.seamStitches.Length] = new SeamStitch(meshToAppend.seamStitches[i]);
                        seam[i + this.seamStitches.Length].Index += (uint)this.numVerts;
                    }
                    this.seamStitches = seam;
                    this.seamStitchCount = seam.Length;
                }
                SlotrayIntersection[] adj2 = new SlotrayIntersection[this.slotrayIntersections.Length + meshToAppend.slotrayIntersections.Length];
                Array.Copy(this.slotrayIntersections, 0, adj2, 0, this.slotrayIntersections.Length);
                for (int i = 0; i < meshToAppend.slotrayIntersections.Length; i++)
                {
                    adj2[i + this.slotrayIntersections.Length] = new SlotrayIntersection(meshToAppend.slotrayIntersections[i]);
                    int[] f = meshToAppend.slotrayIntersections[i].TrianglePointIndices;
                    for (int j = 0; j < f.Length; j++)
                    {
                        f[j] += this.numVerts;
                    }
                    adj2[i + this.slotrayIntersections.Length].TrianglePointIndices = f;
                }
                this.slotrayIntersections = adj2;
                this.slotCount = adj2.Length;
            }
            this.numVerts += meshToAppend.numVerts;
            this.numfacepoints += meshToAppend.numfacepoints;
        }

        public int AddIDNumbers(int startnum)
        {
            if (!this.isBase) throw new MeshException("This mesh is not a base mesh!");
            if (!this.hasVertexIDs) this.insertVertexIDinFormatList();
            uint[] newID = new uint[this.numVerts];
            int nextnum = Math.Max(0, startnum);
            bool incrementID;
            for (int i = 0; i < this.numVerts; i++)
            {
                incrementID = true;
                for (int j = 0; j < i; j++)
                {
                    if (this.vPositions[i].Equals(this.vPositions[j]) && this.vNormals[i].Equals(this.vNormals[j]))
                    {
                        newID[i] = newID[j];
                        incrementID = false;
                        break;
                    }
                }
                if (incrementID)
                {
                    newID[i] = (uint)nextnum;
                    nextnum = nextnum + 1;
                }
            }
            this.vertID = newID;
            return nextnum;
        }

        internal void insertVertexIDinFormatList()
        {
            if (Array.IndexOf(this.vertexFormatList, 10) >= 0) return;
            vertexForm[] newFormat = new vertexForm[this.vertexFormatList.Length + 1];
            if (Array.IndexOf(this.vertexFormatList, 5) >= 0)
            {
                int ind = 0;
                for (int i = 0; i < this.vertexFormatList.Length; i++)
                {
                    newFormat[ind] = this.vertform[i];
                    ind += 1;
                    if (this.vertform[i].formatDataType == 5)
                    {
                        newFormat[ind] = new vertexForm(10, 4, 4);
                        ind += 1;
                    }
                }
            }
            else
            {
                Array.Copy(this.vertform, newFormat, this.vertexFormatList.Length);
                newFormat[this.vertexFormatList.Length] = new vertexForm(10, 4, 4);
            }
            this.vertform = newFormat;
            this.Fcount += 1;
        }

        //internal void BoneMorpher(RIG.Bone bone, float weight, Vector3 offset, Vector3 scale, Quaternion rotation)
        //{
        //    Vector3 unit = new Vector3(1f, 1f, 1f);
        //    for (int i = 0; i < this.vPositions.Length; i++)
        //    {
        //        float boneWeight = GetBoneWeightForVertex(i, bone.BoneHash);
        //        if (boneWeight == 0f || weight == 0f) continue;
        //        Vector3 weightedScale = (scale * boneWeight * weight) + unit;
        //        Vector3 weightedOffset = offset * boneWeight * weight;
        //        Quaternion weightedRotation = rotation * boneWeight * weight;
        //        Matrix4D weightedTransform = weightedRotation.toMatrix4x4(weightedOffset, weightedScale);
        //        Vector3 vertPos = new Vector3(this.getPosition(i)) - bone.WorldPosition;
        //        vertPos = weightedTransform * vertPos;
        //        vertPos += bone.WorldPosition;
        //        this.setPosition(i, vertPos.Coordinates);
        //        Vector3 vertNorm = new Vector3(this.getNormal(i)) - bone.WorldPosition;
        //        vertNorm = (weightedRotation * vertNorm * weightedRotation.Conjugate()).toVector3();
        //        vertNorm += bone.WorldPosition;
        //        this.setNormal(i, vertNorm.Coordinates);
        //    }
        //}

        internal class vertexData : IComparable<vertexData>
        {
            internal ushort ind;
            internal position p;
            internal normal n;
            internal uv uv;
            internal Bones b;
            internal tagval t;
            internal uint id;
            internal vertexData(ushort index, position pos, normal norm, uv uv0, Bones bones, tagval color, uint vertID)
            {
                this.ind = index;
                this.p = pos;
                this.n = norm;
                this.uv = uv0;
                this.b = bones;
                this.t = color;
                this.id = vertID;
            }
            public bool Equals(vertexData other)
            {
                return (this.p.Equals(other.p) && this.n.Equals(other.n) && this.uv.Equals(other.uv));
            }
            public int CompareTo(vertexData other)
            {
                if (other == null) throw new ArgumentException("VertexData is null!");
                return this.ind.CompareTo(other.ind);
            }
        }

        internal int vertexIDsearch(uint vertexID)
        {
            return Array.IndexOf(this.vertID, vertexID);
        }

        // helper classes

        public class vertexForm
        {
            internal int datatype, subtype;
            internal byte bytesper;

            public int formatDataType
            {
                get { return this.datatype; }
            }
            public int formatSubType
            {
                get { return this.subtype; }
            }
            public byte formatDataLength
            {
                get { return this.bytesper; }
            }

            public vertexForm() { }

            public vertexForm(vertexForm source)
            {
                this.datatype = source.datatype;
                this.subtype = source.subtype;
                this.bytesper = source.bytesper;
            }

            public vertexForm(int datatype, int subtype, byte bytesper)
            {
                this.datatype = datatype;
                this.subtype = subtype;
                this.bytesper = bytesper;
            }
            internal vertexForm(BinaryReader br)
            {
                this.datatype = br.ReadInt32();
                this.subtype = br.ReadInt32();
                this.bytesper = br.ReadByte();
            }
            internal void vertexformatWrite(BinaryWriter bw)
            {
                bw.Write(this.datatype);
                bw.Write(this.subtype);
                bw.Write(this.bytesper);
            }
            public override string ToString()
            {
                return Enum.GetName(typeof(VertexFormatNames), this.datatype);
            }
        }

        internal class position
        {
            float x, y, z;
            Vector3 workingVector;

            public float[] Coordinates
            {
                get { return new float[3] { this.x, this.y, this.z }; }
            }
            internal float X
            {
                get { return this.x; }
                set { this.x = value; }
            }
            internal float Y
            {
                get { return this.y; }
                set { this.y = value; }
            }
            internal float Z
            {
                get { return this.z; }
                set { this.z = value; }
            }
            internal Vector3 WorkingVector
            {
                get { return this.workingVector; }
                set { this.workingVector = value; }
            }

            internal position()
            {
                this.x = 0f;
                this.y = 0f;
                this.z = 0f;
            }

            internal position(position source)
            {
                this.x = source.x;
                this.y = source.y;
                this.z = source.z;
            }

            internal position(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            internal position(float[] newPosition)
            {
                this.x = newPosition[0];
                this.y = newPosition[1];
                this.z = newPosition[2];
            }
            internal position(BinaryReader br)
            {
                this.x = br.ReadSingle();
                this.y = br.ReadSingle();
                this.z = br.ReadSingle();
            }
            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.x);
                bw.Write(this.y);
                bw.Write(this.z);
            }
            public bool Equals(position comparePosition)
            {
                return (IsEqual(this.x, comparePosition.x) && IsEqual(this.y, comparePosition.y) && IsEqual(this.z, comparePosition.z));
            }
            public override string ToString()
            {
                return this.x.ToString() + ", " + this.y.ToString() + ", " + this.z.ToString();
            }
            public float[] Data()
            {
                return new float[3] { this.x, this.y, this.z };
            }
            public void addDeltas(float[] deltas)
            {
                this.x += deltas[0];
                this.y += deltas[1];
                this.z += deltas[2];
            }
        }
        internal class normal
        {
            float x, y, z;

            public float[] Coordinates
            {
                get { return new float[3] { this.x, this.y, this.z }; }
            }
            internal float X
            {
                get { return this.x; }
                set { this.x = value; }
            }
            internal float Y
            {
                get { return this.y; }
                set { this.y = value; }
            }
            internal float Z
            {
                get { return this.z; }
                set { this.z = value; }
            }

            internal normal()
            {
                this.x = 0f;
                this.y = 0f;
                this.z = 0f;
            }
            internal normal(normal source)
            {
                this.x = source.x;
                this.y = source.y;
                this.z = source.z;
            }
            internal normal(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            internal normal(float[] newNormal)
            {
                this.x = newNormal[0];
                this.y = newNormal[1];
                this.z = newNormal[2];
            }
            internal normal(BinaryReader br)
            {
                this.x = br.ReadSingle();
                this.y = br.ReadSingle();
                this.z = br.ReadSingle();
            }
            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.x);
                bw.Write(this.y);
                bw.Write(this.z);
            }
            public bool Equals(normal compareNormal)
            {
                return (IsEqual(this.x, compareNormal.x) && IsEqual(this.y, compareNormal.y) && IsEqual(this.z, compareNormal.z));
            }
            public override string ToString()
            {
                return this.x.ToString() + ", " + this.y.ToString() + ", " + this.z.ToString();
            }
            public float[] Data()
            {
                return new float[3] { this.x, this.y, this.z };
            }
            public void addDeltas(float[] deltas)
            {
                this.x += deltas[0];
                this.y += deltas[1];
                this.z += deltas[2];
            }
        }

        internal class uv
        {
            float x, y;

            internal float U
            {
                get { return this.x; }
                set { this.x = value; }
            }
            internal float V
            {
                get { return this.y; }
                set { this.y = value; }
            }

            internal uv() { }
            internal uv(uv source)
            {
                this.x = source.x;
                this.y = source.y;
            }
            internal uv(float u, float v)
            {
                this.x = u;
                this.y = v;
            }
            internal uv(float[] newUV)
            {
                this.x = newUV[0];
                this.y = newUV[1];
            }
            internal uv(float[] newUV, bool verticalFlip)
            {
                this.x = newUV[0];
                if (verticalFlip)
                {
                    this.y = 1f - newUV[1];
                }
                else
                {
                    this.y = newUV[1];
                }
            }
            internal uv(BinaryReader br)
            {
                this.x = br.ReadSingle();
                this.y = br.ReadSingle();
            }
            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.x);
                bw.Write(this.y);
            }
            public bool Equals(uv compareUV)
            {
                return (IsEqual(this.x, compareUV.x) && IsEqual(this.y, compareUV.y));
            }
            public bool CloseTo(uv other)
            {
                const float diff = 0.001f;
                return
                (
                   (Math.Abs(this.x - other.x) < diff) &&
                   (Math.Abs(this.y - other.y) < diff)
                );
            }
            public override string ToString()
            {
                return this.x.ToString() + ", " + this.y.ToString();
            }
            public float[] Data()
            {
                return new float[2] { this.x, this.y };
            }
        }
        internal class Bones
        {
            byte[] assignments = new byte[4];
            //float[] weights = new float[4];
            byte[] weights = new byte[4];
            internal Bones() { }
            internal Bones(Bones source)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = source.assignments[i];
                    this.weights[i] = source.weights[i];
                }
            }
            internal Bones(byte[] assignmentsIn, float[] weightsIn)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = assignmentsIn[i];
                    this.weights[i] = (byte)(Math.Min(weightsIn[i] * 255f, 255f));
                }
            }
            internal Bones(int[] assignmentsIn, float[] weightsIn)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = (byte)assignmentsIn[i];
                    this.weights[i] = (byte)(Math.Min(weightsIn[i] * 255f, 255f));
                }
            }
            internal Bones(byte[] assignmentsIn, byte[] weightsIn)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = assignmentsIn[i];
                    this.weights[i] = weightsIn[i];
                    //this.weights[i] = (float)weightsIn[i] / 255f;
                }
            }
            internal Bones(int[] assignmentsIn, byte[] weightsIn)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = (byte)assignmentsIn[i];
                    this.weights[i] = weightsIn[i];
                    //this.weights[i] = (float)weightsIn[i] / 255f;
                }
            }
            internal void ReadAssignments(BinaryReader br)
            {
                for (int i = 0; i < 4; i++)
                {
                    this.assignments[i] = br.ReadByte();
                }
            }
            internal void ReadWeights(BinaryReader br, int subtype)
            {
                if (subtype == 1)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float tmp = br.ReadSingle();
                        this.weights[i] = (byte)(Math.Min(tmp * 255f, 255f));
                    }
                }
                else if (subtype == 2)
                {
                    //for (int i = 2; i >= 0; i--)          // for CAS demo meshes only!
                    //{
                    //    byte tmp = br.ReadByte();
                    //    this.weights[i] = (float)tmp / 255f;
                    //}
                    //byte tmp2 = br.ReadByte();
                    //this.weights[3] = (float)tmp2 / 255f;
                    for (int i = 0; i < 4; i++)
                    {
                        this.weights[i] = br.ReadByte();
                        //this.weights[i] = (float)this.weightsNew[i] / 255f;
                    }
                }
            }
            internal void WriteAssignments(BinaryWriter bw, int version, int maxBoneIndex)
            {
                if (version == 5)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.weights[i] > 0f)
                        {
                            bw.Write(this.assignments[i]);
                        }
                        else
                        {
                            bw.Write((byte)2);
                        }
                    }
                }
                else if (version >= 12)
                {
                    //byte tmp = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        //if (this.weights[i] > 0)
                        //{
                        bw.Write(this.assignments[i]);
                        //}
                        //else
                        //{
                        //    bw.Write(tmp);
                        //    tmp = (byte)Math.Min(tmp++, maxBoneIndex);
                        //}
                    }
                }

            }
            internal void WriteWeights(BinaryWriter bw, int version)
            {
                if (version == 5)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float tmp = (float)this.weights[i] / 255f;
                        bw.Write(tmp);
                    }
                }
                else if (version >= 12)
                {
                    bw.Write(this.weights);
                }
            }
            public bool Equals(Bones compareBones)
            {
                return (this.assignments[0] == compareBones.assignments[0] && this.assignments[1] == compareBones.assignments[1] &&
                    this.assignments[2] == compareBones.assignments[2] && this.assignments[3] == compareBones.assignments[3] &&
                    this.weights[0] == compareBones.weights[0] && this.weights[1] == compareBones.weights[1] &&
                    this.weights[2] == compareBones.weights[2] && this.weights[3] == compareBones.weights[3]);
            }
            public override string ToString()
            {
                return this.assignments[0].ToString() + this.assignments[1].ToString() + this.assignments[2].ToString() + this.assignments[3].ToString() +
                    this.weights[0].ToString() + ", " + this.weights[1].ToString() + ", " + this.weights[2].ToString() + ", " + this.weights[3].ToString();
            }
            internal byte[] boneAssignments
            {
                get { return new byte[] { this.assignments[0], this.assignments[1], this.assignments[2], this.assignments[3] }; }
                set
                {
                    for (int i = 0; i < this.assignments.Length; i++)
                    {
                        this.assignments[i] = value[i];
                    }
                }
            }
            internal float[] boneWeightsV5
            {
                get { return new float[] { this.weights[0], this.weights[1], this.weights[2], this.weights[3] }; }
                set
                {
                    for (int i = 0; i < this.assignments.Length; i++)
                    {
                        //this.weights[i] = value[i];
                        this.weights[i] = (byte)(Math.Min(value[i] * 255f, 255f));
                    }
                }
            }
            internal byte[] boneWeights
            {
                get { return new byte[] { this.weights[0], this.weights[1], this.weights[2], this.weights[3] }; }
                set
                {
                    int tot = 0;
                    for (int i = 0; i < this.assignments.Length; i++)
                    {
                        this.weights[i] = value[i];
                        tot += value[i];
                        //this.weights[i] = (float)value[i] / 255f;
                    }
                    this.weights[0] += (byte)(255 - tot);
                }
            }
            internal void Sort(int version)
            {
                for (int i = this.assignments.Length - 2; i >= 0; i--)
                {
                    for (int j = 0; j <= i; j++)
                    {
                        if (this.weights[j] < this.weights[j + 1])
                        {
                            byte tb = this.assignments[j];
                            this.assignments[j] = this.assignments[j + 1];
                            this.assignments[j + 1] = tb;
                            byte tw = this.weights[j];
                            this.weights[j] = this.weights[j + 1];
                            this.weights[j + 1] = tw;
                        }
                    }
                }
            }
        }

        internal class tangent
        {
            float x, y, z;
            internal tangent() { }
            internal float X
            {
                get { return this.x; }
                set { this.x = value; }
            }
            internal float Y
            {
                get { return this.y; }
                set { this.y = value; }
            }
            internal float Z
            {
                get { return this.z; }
                set { this.z = value; }
            }
            internal tangent(tangent source)
            {
                this.x = source.x;
                this.y = source.y;
                this.z = source.z;
            }
            internal tangent(float x, float y, float z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }
            internal tangent(float[] newTangent)
            {
                this.x = newTangent[0];
                this.y = newTangent[1];
                this.z = newTangent[2];
            }
            internal tangent(BinaryReader br)
            {
                this.x = br.ReadSingle();
                this.y = br.ReadSingle();
                this.z = br.ReadSingle();
            }
            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.x);
                bw.Write(this.y);
                bw.Write(this.z);
            }
            public bool Equals(tangent compareTangent)
            {
                return (IsEqual(this.x, compareTangent.x) && IsEqual(this.y, compareTangent.y) && IsEqual(this.z, compareTangent.z));
            }
            public override string ToString()
            {
                return this.x.ToString() + ", " + this.y.ToString() + ", " + this.z.ToString();
            }
            public float[] Data()
            {
                return new float[3] { this.x, this.y, this.z };
            }
        }
        internal class tagval
        {
            uint tags;
            internal tagval() { }
            internal tagval(tagval source)
            {
                this.tags = source.tags;
            }
            internal tagval(uint tagValue)
            {
                this.tags = tagValue;
            }
            internal tagval(BinaryReader br)
            {
                this.tags = br.ReadUInt32();
            }
            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.tags);
            }
            public bool Equals(tagval compareTagval)
            {
                return (this.tags == compareTagval.tags);
            }
            public override string ToString()
            {
                return Convert.ToString(this.tags, 16).ToUpper().PadLeft(8, '0');
            }
            public uint Data()
            {
                return this.tags;
            }
        }

        public class Face
        {
            uint[] face = new uint[3];
            public int facePoint0
            {
                get { return (int)face[0]; }
            }
            public int facePoint1
            {
                get { return (int)face[1]; }
            }

            public int facePoint2
            {
                get { return (int)face[2]; }
            }
            internal Face() { }
            internal Face(Face source)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.face[i] = source.face[i];
                }
            }
            internal uint[] meshface
            {
                get
                {
                    return new uint[] { face[0], face[1], face[2] };
                }
            }
            internal Face(byte[] face)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.face[i] = (uint)face[i];
                }
            }
            internal Face(ushort[] face)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.face[i] = (uint)face[i];
                }
            }
            internal Face(int[] face)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.face[i] = (uint)face[i];
                }
            }
            internal Face(uint[] face)
            {
                for (int i = 0; i < 3; i++)
                {
                    this.face[i] = face[i];
                }
            }
            public Face(int FacePoint0, int FacePoint1, int FacePoint2)
            {
                this.face[0] = (uint)FacePoint0;
                this.face[1] = (uint)FacePoint1;
                this.face[2] = (uint)FacePoint2;
            }
            public Face(short FacePoint0, short FacePoint1, short FacePoint2)
            {
                this.face[0] = (uint)FacePoint0;
                this.face[1] = (uint)FacePoint1;
                this.face[2] = (uint)FacePoint2;
            }

            internal Face(BinaryReader br, byte bytesperfacepnt)
            {
                for (int i = 0; i < 3; i++)
                {
                    switch (bytesperfacepnt)
                    {
                        case (1):
                            this.face[i] = br.ReadByte();
                            break;
                        case (2):
                            this.face[i] = br.ReadUInt16();
                            break;
                        case (4):
                            this.face[i] = br.ReadUInt32();
                            break;
                        default:
                            break;
                    }

                }
            }
            internal void Write(BinaryWriter bw, byte bytesperfacepnt)
            {
                for (int i = 0; i < 3; i++)
                {
                    switch (bytesperfacepnt)
                    {
                        case (1):
                            bw.Write((byte)this.face[i]);
                            break;
                        case (2):
                            bw.Write((ushort)this.face[i]);
                            break;
                        case (4):
                            bw.Write(this.face[i]);
                            break;
                        default:
                            break;
                    }
                }
            }
            public static Face Reverse(Face source)
            {
                return new Face(source.facePoint2, source.facePoint1, source.facePoint0);
            }
            public void Reverse()
            {
                uint tmp = this.face[0];
                this.face[0] = this.face[2];
                this.face[2] = tmp;
            }
            public bool Equals(Face f)
            {
                return ((this.face[0] == f.face[0]) && (this.face[1] == f.face[1]) && (this.face[2] == f.face[2]));
            }
            public override string ToString()
            {
                return this.face[0].ToString() + ", " + this.face[1].ToString() + ", " + this.face[2].ToString();
            }
        }

        public class UVStitch
        {
            int vertexIndex;
            int count;
            float[][] coordinates;

            public int Index
            {
                get { return this.vertexIndex; }
                set { this.vertexIndex = value; }
            }
            public int Count
            {
                get { return this.count; }
            }
            public List<float[]> UV1Coordinates
            {
                get
                {
                    List<float[]> pairs = new List<float[]>();
                    for (int i = 0; i < coordinates.GetLength(0); i++)
                    {
                        pairs.Add(coordinates[i]);
                    }
                    return pairs;
                }
            }
            public int Size
            {
                get { return 8 + (this.count * 8); }
            }

            internal UVStitch(BinaryReader br)
            {
                this.vertexIndex = br.ReadInt32();
                this.count = br.ReadInt32();
                this.coordinates = new float[this.count][];
                for (int i = 0; i < this.count; i++)
                {
                    this.coordinates[i] = new float[2];
                    this.coordinates[i][0] = br.ReadSingle();
                    this.coordinates[i][1] = br.ReadSingle();
                }
            }

            public UVStitch(UVStitch adjustment)
            {
                this.vertexIndex = adjustment.vertexIndex;
                this.count = adjustment.count;
                this.coordinates = new float[adjustment.count][];
                for (int i = 0; i < this.count; i++)
                {
                    this.coordinates[i] = new float[2];
                    this.coordinates[i][0] = adjustment.coordinates[i][0];
                    this.coordinates[i][1] = adjustment.coordinates[i][1];
                }
            }

            public UVStitch(int vertexIndex, Vector2[] uv1Coordinates)
            {
                this.vertexIndex = vertexIndex;
                this.count = uv1Coordinates.Length;
                this.coordinates = new float[uv1Coordinates.Length][];
                for (int i = 0; i < uv1Coordinates.Length; i++)
                {
                    this.coordinates[i] = new float[2] { uv1Coordinates[i].X, uv1Coordinates[i].Y };
                }
            }

            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.vertexIndex);
                bw.Write(this.count);
                for (int i = 0; i < this.count; i++)
                {
                    bw.Write(this.coordinates[i][0]);
                    bw.Write(this.coordinates[i][1]);
                }
            }
        }

        public class SeamStitch
        {
            uint index;
            UInt16 vertID;

            public uint Index
            {
                get { return this.index; }
                set { this.index = value; }
            }
            public UInt16 VertID
            {
                get { return this.vertID; }
                set { this.vertID = value; }
            }
            public int SeamType
            {
                get { return this.vertID >> 12; }
            }
            public int SeamIndex
            {
                get { return this.vertID & 0x0FFF; }
            }
            public int Size
            {
                get { return 6; }
            }

            internal SeamStitch(BinaryReader br)
            {
                this.index = br.ReadUInt32();
                this.vertID = br.ReadUInt16();
            }

            public SeamStitch(SeamStitch adjustment)
            {
                this.index = adjustment.index;
                this.vertID = adjustment.vertID;
            }

            public SeamStitch(int index, int seam, int vertex)
            {
                this.index = (uint)index;
                this.vertID = (ushort)((seam << 12) + vertex);
            }

            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.index);
                bw.Write(this.vertID);
            }
        }

        public class SlotrayIntersection
        {
            uint slotBone;
            short[] vertIndices;        // short[3] indices of vertices making up face
            float[] coordinates;        // float[2] Barycentric coordinates of the point of intersection
            float distance;             // distance from raycast origin to the intersection point
            float[] offsetFromIntersectionOS;  // Vector3 offset from the intersection point to the slot's average position (if outside geometry) in object space
            float[] slotAveragePosOS;   // Vector3 slot's average position in object space
            float[] transformToLS;     // Quaternion transform from object space to the slot's local space
            uint pivotBone;             // index or hash of the bone that this slot pivots around, 0xFF or 0x00000000 if pivot does not exist

            int parentVersion;

            public uint SlotIndex
            {
                get { return this.slotBone; }
                set { this.slotBone = value; }
            }
            public int[] TrianglePointIndices
            {
                get { return new int[] { this.vertIndices[0], this.vertIndices[1], this.vertIndices[2] }; }
                set { this.vertIndices[0] = (short)value[0]; this.vertIndices[1] = (short)value[1]; this.vertIndices[2] = (short)value[2]; }
            }
            public Vector2 Coordinates
            {
                get { return new Vector2(this.coordinates); }
                set { this.coordinates = value.Coordinates; }
            }
            public float Distance
            {
                get { return this.distance; }
                set { this.distance = value; }
            }
            public Vector3 OffsetFromIntersectionOS
            {
                get { return new Vector3(this.offsetFromIntersectionOS); }
                set { this.offsetFromIntersectionOS = value.Coordinates; }
            }
            public Vector3 SlotAveragePosOS
            {
                get { return new Vector3(this.slotAveragePosOS); }
                set { this.slotAveragePosOS = value.Coordinates; }
            }
            public Quaternion TransformToLS
            {
                get { return new Quaternion(Form1.ArrayToDouble(this.transformToLS)); }
                set { this.transformToLS = new float[] { (float)value.X, (float)value.Y, (float)value.Z, (float)value.W }; }
            }
            public uint PivotBone
            {
                get { return this.pivotBone; }
                set { this.pivotBone = value; }
            }

            internal SlotrayIntersection(BinaryReader br, int version)
            {
                this.parentVersion = version;
                this.slotBone = br.ReadUInt32();
                this.vertIndices = new short[3];
                for (int i = 0; i < 3; i++)
                {
                    this.vertIndices[i] = br.ReadInt16();
                }
                this.coordinates = new float[2];
                for (int i = 0; i < 2; i++)
                {
                    this.coordinates[i] = br.ReadSingle();
                }
                this.distance = br.ReadSingle();
                this.offsetFromIntersectionOS = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    this.offsetFromIntersectionOS[i] = br.ReadSingle();
                }
                this.slotAveragePosOS = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    this.slotAveragePosOS[i] = br.ReadSingle();
                }
                this.transformToLS = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    this.transformToLS[i] = br.ReadSingle();
                }
                if (parentVersion >= 14)
                {
                    this.pivotBone = br.ReadUInt32();
                }
                else
                {
                    this.pivotBone = br.ReadByte();
                }
            }

            public SlotrayIntersection(SlotrayIntersection faceAdjustment)
            {
                this.slotBone = faceAdjustment.slotBone;
                this.vertIndices = new short[3];
                for (int i = 0; i < 3; i++)
                {
                    this.vertIndices[i] = faceAdjustment.vertIndices[i];
                }
                this.coordinates = new float[2];
                for (int i = 0; i < 2; i++)
                {
                    this.coordinates[i] = faceAdjustment.coordinates[i];
                }
                this.distance = faceAdjustment.distance;
                this.offsetFromIntersectionOS = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    this.offsetFromIntersectionOS[i] = faceAdjustment.offsetFromIntersectionOS[i];
                }
                this.slotAveragePosOS = new float[3];
                for (int i = 0; i < 3; i++)
                {
                    this.slotAveragePosOS[i] = faceAdjustment.slotAveragePosOS[i];
                }
                this.transformToLS = new float[4];
                for (int i = 0; i < 4; i++)
                {
                    this.transformToLS[i] = faceAdjustment.transformToLS[i];
                }
                this.pivotBone = faceAdjustment.pivotBone;
            }

            internal void Write(BinaryWriter bw)
            {
                bw.Write(this.slotBone);
                for (int i = 0; i < this.vertIndices.Length; i++)
                {
                    bw.Write(this.vertIndices[i]);
                }
                for (int i = 0; i < this.coordinates.Length; i++)
                {
                    bw.Write(this.coordinates[i]);
                }
                bw.Write(this.distance);
                for (int i = 0; i < this.offsetFromIntersectionOS.Length; i++)
                {
                    bw.Write(this.offsetFromIntersectionOS[i]);
                }
                for (int i = 0; i < this.slotAveragePosOS.Length; i++)
                {
                    bw.Write(this.slotAveragePosOS[i]);
                }
                for (int i = 0; i < this.transformToLS.Length; i++)
                {
                    bw.Write(this.transformToLS[i]);
                }
                if (parentVersion >= 14)
                {
                    bw.Write(this.pivotBone);
                }
                else
                {
                    bw.Write((byte)this.pivotBone);
                }
            }

        }

        public class MTNF
        {
            char[] magic;
            int zero, datasize, paramCount;
            uint[][] paramList;
            object[][] dataList;

            public MTNF() { }

            internal int chunkSize
            {
                get { return 16 + (this.paramCount * 16) + this.datasize; }
            }

            public uint[] getParamsList()
            {
                uint[] tmp = new uint[this.paramCount];
                for (int i = 0; i < this.paramCount; i++)
                {
                    tmp[i] = this.paramList[i][0];
                }
                return tmp;
            }

            public object[] getParamValue(uint parameter, out int valueType)
            {
                object[] tmp = null;
                for (int i = 0; i < this.paramCount; i++)
                {
                    if (this.paramList[i][0] == parameter)
                    {
                        tmp = new object[this.paramList[i][2]];
                        for (int j = 0; j < tmp.Length; j++)
                        {
                            tmp[j] = this.dataList[i][j];
                        }
                        valueType = (int)this.paramList[i][1];
                        return tmp;
                    }
                }
                valueType = 0;
                return null;
            }

            internal MTNF(MTNF source)
            {
                this.magic = source.magic;
                this.zero = source.zero;
                this.datasize = source.datasize;
                this.paramCount = source.paramCount;
                this.paramList = new uint[source.paramList.Length][];
                for (int i = 0; i < source.paramList.Length; i++)
                {
                    this.paramList[i] = new uint[source.paramList[i].Length];
                    for (int j = 0; j < source.paramList[i].Length; j++)
                    {
                        this.paramList[i][j] = source.paramList[i][j];
                    }
                }
                this.dataList = new object[source.dataList.Length][];
                for (int i = 0; i < source.dataList.Length; i++)
                {
                    this.dataList[i] = new object[source.dataList[i].Length];
                    for (int j = 0; j < source.dataList[i].Length; j++)
                    {
                        this.dataList[i][j] = source.dataList[i][j];
                    }
                }
            }

            internal MTNF(BinaryReader br)
            {
                this.magic = br.ReadChars(4);
                this.zero = br.ReadInt32();
                this.datasize = br.ReadInt32();
                this.paramCount = br.ReadInt32();
                this.paramList = new uint[paramCount][];
                for (int i = 0; i < paramCount; i++)
                {
                    this.paramList[i] = new uint[4];
                    for (int j = 0; j < 4; j++)
                    {
                        this.paramList[i][j] = br.ReadUInt32();
                    }
                }
                this.dataList = new object[paramCount][];
                for (int i = 0; i < paramCount; i++)
                {
                    this.dataList[i] = new object[paramList[i][2]];
                    if (paramList[i][1] == 1)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            this.dataList[i][j] = br.ReadSingle();
                        }
                    }
                    else if (paramList[i][1] == 2)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            this.dataList[i][j] = br.ReadInt32();
                        }
                    }
                    else
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            this.dataList[i][j] = br.ReadUInt32();
                        }
                    }
                }
            }

            internal void Write(BinaryWriter bw)
            {
                bw.Write(magic);
                bw.Write(zero);
                bw.Write(datasize);
                bw.Write(paramCount);
                for (int i = 0; i < paramCount; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        bw.Write(paramList[i][j]);
                    }
                }
                for (int i = 0; i < paramCount; i++)
                {
                    if (paramList[i][1] == 1)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            bw.Write((float)dataList[i][j]);
                        }
                    }
                    else if (paramList[i][1] == 2)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            bw.Write((int)dataList[i][j]);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            bw.Write((uint)dataList[i][j]);
                        }
                    }
                }
            }

            public MTNF(uint[] shaderDataArray)
            {
                magic = Encoding.UTF8.GetString(BitConverter.GetBytes(shaderDataArray[0])).ToCharArray();
                zero = (int)shaderDataArray[1];
                datasize = (int)shaderDataArray[2];
                paramCount = (int)shaderDataArray[3];
                paramList = new uint[paramCount][];
                dataList = new object[paramCount][];
                int ind = 4;
                for (int i = 0; i < paramCount; i++)
                {
                    paramList[i] = new uint[4];
                    for (int j = 0; j < 4; j++)
                    {
                        paramList[i][j] = shaderDataArray[ind];
                        ind++;
                    }
                }
                for (int i = 0; i < paramCount; i++)
                {
                    dataList[i] = new object[paramList[i][2]];
                    if (paramList[i][1] == 1)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            byte[] b = BitConverter.GetBytes(shaderDataArray[ind]);
                            dataList[i][j] = BitConverter.ToSingle(b, 0);
                            ind++;
                        }
                    }
                    else if (paramList[i][1] == 2)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            dataList[i][j] = (int)shaderDataArray[ind];
                            ind++;
                        }
                    }
                    else
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            dataList[i][j] = shaderDataArray[ind];
                            ind++;
                        }
                    }
                }
            }

            public uint[] toDataArray()
            {
                List<uint> tmp = new List<uint>();
                tmp.Add(BitConverter.ToUInt32(Encoding.UTF8.GetBytes(magic), 0));
                tmp.Add((uint)zero);
                tmp.Add((uint)datasize);
                tmp.Add((uint)paramCount);
                for (int i = 0; i < paramCount; i++)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        tmp.Add(paramList[i][j]);
                    }
                }
                for (int i = 0; i < paramCount; i++)
                {
                    if (paramList[i][1] == 1)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            byte[] b = BitConverter.GetBytes((float)dataList[i][j]);
                            tmp.Add(BitConverter.ToUInt32(b, 0));
                        }
                    }
                    else if (paramList[i][1] == 2)
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            int t = (int)dataList[i][j];
                            tmp.Add((uint)t);
                        }
                    }
                    else
                    {
                        for (int j = 0; j < paramList[i][2]; j++)
                        {
                            tmp.Add((uint)dataList[i][j]);
                        }
                    }
                }
                return tmp.ToArray();
            }

        }


        [global::System.Serializable]
        public class MeshException : ApplicationException
        {
            public MeshException() { }
            public MeshException(string message) : base(message) { }
            public MeshException(string message, Exception inner) : base(message, inner) { }
            protected MeshException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context)
                : base(info, context) { }
        }

        public enum VertexFormatNames
        {
            Position = 1,
            Normals = 2,
            UV = 3,
            BoneAssignment = 4,
            BoneWeight = 5,
            Tangents = 6,
            TagVal = 7,
            VertexID = 10
        }

        internal static bool IsEqual(float x, float y)
        {
            const float EPSILON = 1e-4f;
            return (Math.Abs(x - y) < EPSILON);
        }

        public enum SeamType
        {
            Ankles = 0,
            Neck = 3,
            Waist = 4,
            WaistAdultFemale = 5,
            WaistAdultMale = 6
        }

        internal static Vector3[][][][] SetupSeamVertexPositions()
        {
            Vector3[][][][] meshSeamVerts = new Vector3[3][][][];   //indices: sex, lod, seam, verts
            meshSeamVerts[0] = new Vector3[4][][];          //male
            meshSeamVerts[1] = new Vector3[4][][];          //female
            meshSeamVerts[2] = new Vector3[4][][];          //child

            meshSeamVerts[0][0] = new Vector3[7][];         //male lod0
            meshSeamVerts[0][0][0] = new Vector3[] { new Vector3(0.10318f, 0.16812f, 0.01464f), new Vector3(0.08145f, 0.16812f, 0.00676f), 
                new Vector3(0.12142f, 0.16812f, 0.00231f), new Vector3(0.1301f, 0.16812f, -0.02376f), new Vector3(0.12235f, 0.16812f, -0.04518f), 
                new Vector3(0.10225f, 0.16812f, -0.06237f), new Vector3(0.06959f, 0.16812f, -0.01289f), new Vector3(0.07157f, 0.16812f, -0.03863f), 
                new Vector3(0.08476f, 0.16812f, -0.05846f), new Vector3(-0.10318f, 0.16812f, 0.01464f), new Vector3(-0.08145f, 0.16812f, 0.00676f), 
                new Vector3(-0.12142f, 0.16812f, 0.00231f), new Vector3(-0.1301f, 0.16812f, -0.02376f), new Vector3(-0.12235f, 0.16812f, -0.04518f), 
                new Vector3(-0.10225f, 0.16812f, -0.06237f), new Vector3(-0.06959f, 0.16812f, -0.01289f), new Vector3(-0.07157f, 0.16812f, -0.03863f), 
                new Vector3(-0.08476f, 0.16812f, -0.05846f) };  //male lod0 ankle
            meshSeamVerts[0][0][1] = new Vector3[0];
            meshSeamVerts[0][0][2] = new Vector3[0];
            meshSeamVerts[0][0][3] = new Vector3[] { new Vector3(0.04994f, 1.65732f, -0.04331f), new Vector3(0.05748f, 1.65212f, -0.02185f), 
                new Vector3(0.02016f, 1.62796f, 0.02991f), new Vector3(-0.0f, 1.62329f, 0.03646f), new Vector3(0.02658f, 1.65984f, -0.06291f), 
                new Vector3(0.04268f, 1.63725f, 0.01346f), new Vector3(0.03073f, 1.63173f, 0.02297f), new Vector3(0.05114f, 1.64436f, -0.00103f), 
                new Vector3(-0.0f, 1.66001f, -0.07078f), new Vector3(-0.04994f, 1.65732f, -0.04331f), new Vector3(-0.05748f, 1.65212f, -0.02185f), 
                new Vector3(-0.02016f, 1.62796f, 0.02991f), new Vector3(-0.02658f, 1.65984f, -0.06291f), new Vector3(-0.04268f, 1.63725f, 0.01346f), 
                new Vector3(-0.03074f, 1.63173f, 0.02296f), new Vector3(-0.05114f, 1.64436f, -0.00103f) };   //male lod0 neck
            meshSeamVerts[0][0][4] = new Vector3[0];
            meshSeamVerts[0][0][5] = new Vector3[0];
            meshSeamVerts[0][0][6] = new Vector3[] {new Vector3(0.13477f, 1.10102f, 0.05168f), new Vector3(0.09531f, 1.09588f, 0.08789f), 
                new Vector3(0.0283f, 1.09001f, 0.11046f), new Vector3(0.06203f, 1.0929f, 0.10109f), new Vector3(-0.0f, 1.08875f, 0.11338f), 
                new Vector3(0.0537f, 1.10483f, -0.079f), new Vector3(0.02362f, 1.10363f, -0.08015f), new Vector3(0.12888f, 1.10734f, -0.03361f), 
                new Vector3(0.14252f, 1.10484f, 0.00736f), new Vector3(0.10903f, 1.10822f, -0.05758f), new Vector3(0.08691f, 1.10752f, -0.0717f), 
                new Vector3(-0.0f, 1.10245f, -0.07855f), new Vector3(-0.13477f, 1.10102f, 0.05168f), new Vector3(-0.09531f, 1.09588f, 0.08789f), 
                new Vector3(-0.0283f, 1.09001f, 0.11046f), new Vector3(-0.06203f, 1.0929f, 0.10109f), new Vector3(-0.0537f, 1.10483f, -0.079f), 
                new Vector3(-0.02362f, 1.10363f, -0.08015f), new Vector3(-0.12888f, 1.10734f, -0.03361f), new Vector3(-0.14252f, 1.10484f, 0.00736f), 
                new Vector3(-0.10903f, 1.10822f, -0.05758f), new Vector3(-0.08691f, 1.10752f, -0.0717f) };  //male lod0 waist

            meshSeamVerts[0][1] = new Vector3[7][];         //male lod1
            meshSeamVerts[0][1][0] = new Vector3[] { new Vector3(0.10318f, 0.16812f, 0.01464f), new Vector3(0.08145f, 0.16812f, 0.00676f), 
                new Vector3(0.12142f, 0.16812f, 0.00231f), new Vector3(0.1301f, 0.16812f, -0.02376f), new Vector3(0.12235f, 0.16812f, -0.04518f), 
                new Vector3(0.09351f, 0.16812f, -0.06042f), new Vector3(0.06959f, 0.16812f, -0.01289f), new Vector3(0.07157f, 0.16812f, -0.03863f), 
                new Vector3(-0.10318f, 0.16812f, 0.01464f), new Vector3(-0.08145f, 0.16812f, 0.00676f), new Vector3(-0.12142f, 0.16812f, 0.00231f), 
                new Vector3(-0.1301f, 0.16812f, -0.02376f), new Vector3(-0.12235f, 0.16812f, -0.04518f), new Vector3(-0.09351f, 0.16812f, -0.06042f), 
                new Vector3(-0.06959f, 0.16812f, -0.01289f), new Vector3(-0.07157f, 0.16812f, -0.03863f) };  //male lod1 ankle
            meshSeamVerts[0][1][1] = new Vector3[0];
            meshSeamVerts[0][1][2] = new Vector3[0];
            meshSeamVerts[0][1][3] = new Vector3[] { new Vector3(0.04994f, 1.65732f, -0.04331f), new Vector3(0.05748f, 1.65212f, -0.02185f), 
                new Vector3(0.02016f, 1.62796f, 0.02991f), new Vector3(-0.0f, 1.62329f, 0.03646f), new Vector3(0.02658f, 1.65984f, -0.06291f), 
                new Vector3(0.04268f, 1.63725f, 0.01346f), new Vector3(0.03074f, 1.63173f, 0.02297f), new Vector3(0.05114f, 1.64436f, -0.00103f), 
                new Vector3(-0.0f, 1.66001f, -0.07078f), new Vector3(-0.04994f, 1.65732f, -0.04331f), new Vector3(-0.05748f, 1.65212f, -0.02185f), 
                new Vector3(-0.02016f, 1.62796f, 0.02991f), new Vector3(-0.02658f, 1.65984f, -0.06291f), new Vector3(-0.04268f, 1.63725f, 0.01346f), 
                new Vector3(-0.03074f, 1.63173f, 0.02296f), new Vector3(-0.05114f, 1.64436f, -0.00103f) };  //male lod1 neck
            meshSeamVerts[0][1][4] = new Vector3[0];
            meshSeamVerts[0][1][5] = new Vector3[0];
            meshSeamVerts[0][1][6] = new Vector3[] { new Vector3(0.13477f, 1.10102f, 0.05168f), new Vector3(0.07867f, 1.09439f, 0.09449f), 
                new Vector3(-0.0f, 1.08875f, 0.11338f), new Vector3(0.03866f, 1.10423f, -0.07958f), new Vector3(0.12888f, 1.10734f, -0.03361f), 
                new Vector3(0.14252f, 1.10484f, 0.00736f), new Vector3(0.09797f, 1.10787f, -0.06464f), new Vector3(-0.0f, 1.10245f, -0.07855f), 
                new Vector3(-0.13477f, 1.10102f, 0.05168f), new Vector3(-0.07867f, 1.09439f, 0.09449f), new Vector3(-0.03866f, 1.10423f, -0.07958f), 
                new Vector3(-0.12888f, 1.10734f, -0.03361f), new Vector3(-0.14252f, 1.10484f, 0.00736f), new Vector3(-0.09797f, 1.10787f, -0.06464f) };
            //male lod1 waist

            meshSeamVerts[0][2] = new Vector3[7][];         //male lod2
            meshSeamVerts[0][2][0] = new Vector3[] { new Vector3(0.1123f, 0.16812f, 0.00848f), new Vector3(0.08145f, 0.16812f, 0.00676f), 
                new Vector3(0.1301f, 0.16812f, -0.02376f), new Vector3(0.12235f, 0.16812f, -0.04518f), new Vector3(0.08254f, 0.16812f, -0.04952f), 
                new Vector3(0.06959f, 0.16812f, -0.01289f), new Vector3(-0.1123f, 0.16812f, 0.00848f), new Vector3(-0.08145f, 0.16812f, 0.00676f), 
                new Vector3(-0.1301f, 0.16812f, -0.02376f), new Vector3(-0.12235f, 0.16812f, -0.04518f), new Vector3(-0.08254f, 0.16812f, -0.04952f), 
                new Vector3(-0.06959f, 0.16812f, -0.01289f) };  //male lod2 ankle
            meshSeamVerts[0][2][1] = new Vector3[0];
            meshSeamVerts[0][2][2] = new Vector3[0];
            meshSeamVerts[0][2][3] = new Vector3[] { new Vector3(0.04994f, 1.65732f, -0.04331f), new Vector3(0.05748f, 1.65212f, -0.02185f), 
                new Vector3(0.02016f, 1.62796f, 0.02991f), new Vector3(-0.0f, 1.62329f, 0.03646f), new Vector3(0.02658f, 1.65984f, -0.06291f), 
                new Vector3(0.04268f, 1.63725f, 0.01346f), new Vector3(0.03074f, 1.63173f, 0.02297f), new Vector3(0.05114f, 1.64436f, -0.00103f), 
                new Vector3(-0.0f, 1.66001f, -0.07078f), new Vector3(-0.04994f, 1.65732f, -0.04331f), new Vector3(-0.05748f, 1.65212f, -0.02185f), 
                new Vector3(-0.02016f, 1.62796f, 0.02991f), new Vector3(-0.02658f, 1.65984f, -0.06291f), new Vector3(-0.04268f, 1.63725f, 0.01346f), 
                new Vector3(-0.03074f, 1.63173f, 0.02296f), new Vector3(-0.05114f, 1.64436f, -0.00103f) };  //male lod2 neck
            meshSeamVerts[0][2][4] = new Vector3[0];
            meshSeamVerts[0][2][5] = new Vector3[0];
            meshSeamVerts[0][2][6] = new Vector3[] { new Vector3(0.13477f, 1.10102f, 0.05168f), new Vector3(0.07867f, 1.09439f, 0.09449f), 
                new Vector3(-0.0f, 1.08875f, 0.11338f), new Vector3(0.06831f, 1.10605f, -0.07211f), new Vector3(0.12888f, 1.10734f, -0.03361f), 
                new Vector3(0.14252f, 1.10484f, 0.00736f), new Vector3(-0.0f, 1.10245f, -0.07855f), new Vector3(-0.13477f, 1.10102f, 0.05168f), 
                new Vector3(-0.07867f, 1.09439f, 0.09449f), new Vector3(-0.06831f, 1.10605f, -0.07211f), new Vector3(-0.12888f, 1.10734f, -0.03361f), 
                new Vector3(-0.14252f, 1.10484f, 0.00736f) };  //male lod2 waist

            meshSeamVerts[0][3] = new Vector3[7][];         //male lod3
            meshSeamVerts[0][3][0] = new Vector3[] { new Vector3(0.10318f, 0.16812f, 0.01464f), new Vector3(0.1301f, 0.16812f, -0.02376f), 
                new Vector3(0.09351f, 0.16812f, -0.06042f), new Vector3(0.06959f, 0.16812f, -0.01289f), new Vector3(-0.10318f, 0.16812f, 0.01464f), 
                new Vector3(-0.1301f, 0.16812f, -0.02376f), new Vector3(-0.09351f, 0.16812f, -0.06042f), new Vector3(-0.06959f, 0.16812f, -0.01289f) };
            //male lod3 ankle
            meshSeamVerts[0][3][1] = new Vector3[0];
            meshSeamVerts[0][3][2] = new Vector3[0];
            meshSeamVerts[0][3][3] = new Vector3[] { new Vector3(0.05748f, 1.65212f, -0.02185f), new Vector3(-0.0f, 1.62329f, 0.03646f), 
                new Vector3(0.03826f, 1.65858f, -0.05311f), new Vector3(0.03074f, 1.63173f, 0.02297f), new Vector3(-0.0f, 1.66001f, -0.07078f), 
                new Vector3(-0.05748f, 1.65212f, -0.02185f), new Vector3(-0.03826f, 1.65858f, -0.05311f), new Vector3(-0.03074f, 1.63173f, 0.02296f) };
            //male lod3 neck
            meshSeamVerts[0][3][4] = new Vector3[0];
            meshSeamVerts[0][3][5] = new Vector3[0];
            meshSeamVerts[0][3][6] = new Vector3[] { new Vector3(0.10672f, 1.09771f, 0.07308f), new Vector3(-0.0f, 1.08875f, 0.11338f), 
                new Vector3(0.0986f, 1.1067f, -0.05286f), new Vector3(0.14252f, 1.10484f, 0.00736f), new Vector3(-0.0f, 1.10245f, -0.07855f), 
                new Vector3(-0.10672f, 1.09771f, 0.07308f), new Vector3(-0.0986f, 1.1067f, -0.05286f), new Vector3(-0.14252f, 1.10484f, 0.00736f) };
            //male lod3 waist

            meshSeamVerts[1][0] = new Vector3[7][];         //female lod0
            meshSeamVerts[1][0][0] = new Vector3[] { new Vector3(0.100610f, 0.178310f, 0.013850f), new Vector3(0.084110f, 0.178310f, 0.010620f),
                new Vector3(0.119550f, 0.178310f, -0.000550f), new Vector3(0.121740f, 0.178310f, -0.023150f), new Vector3(0.113930f, 0.178310f, -0.045780f),
                new Vector3(0.100800f, 0.178310f, -0.058140f), new Vector3(0.073530f, 0.178310f, -0.016510f), new Vector3(0.078090f, 0.178310f, -0.041680f), 
                new Vector3(0.084040f, 0.178310f, -0.053540f), new Vector3(-0.100610f, 0.178310f, 0.013850f), new Vector3(-0.084110f, 0.178310f, 0.010620f),
                new Vector3(-0.119550f, 0.178310f, -0.000550f), new Vector3(-0.121740f, 0.178310f, -0.023150f), new Vector3(-0.113930f, 0.178310f, -0.045780f),
                new Vector3(-0.100780f, 0.178310f, -0.058150f), new Vector3(-0.073530f, 0.178310f, -0.016510f), new Vector3(-0.078090f, 0.178310f, -0.041680f),
                new Vector3(-0.084040f, 0.178310f, -0.053540f) };   //female lod0 ankle
            meshSeamVerts[1][0][1] = new Vector3[0];
            meshSeamVerts[1][0][2] = new Vector3[0];
            meshSeamVerts[1][0][3] = new Vector3[] { new Vector3(0.042280f, 1.657280f, -0.037410f), new Vector3(0.046760f, 1.653580f, -0.018430f), 
                new Vector3(0.015410f, 1.627690f, 0.027820f), new Vector3(0.000000f, 1.624760f, 0.031360f), new Vector3(0.023620f, 1.657860f, -0.051060f), 
                new Vector3(0.035540f, 1.638500f, 0.013000f), new Vector3(0.025860f, 1.632100f, 0.022750f), new Vector3(0.045650f, 1.647510f, -0.002180f),
                new Vector3(0.000000f, 1.658240f, -0.058230f), new Vector3(-0.042280f, 1.657280f, -0.037410f), new Vector3(-0.046760f, 1.653580f, -0.018430f),
                new Vector3(-0.015410f, 1.627690f, 0.027820f), new Vector3(-0.023610f, 1.657860f, -0.051060f), new Vector3(-0.035540f, 1.638500f, 0.013000f),
                new Vector3(-0.025860f, 1.632100f, 0.022750f), new Vector3(-0.045650f, 1.647510f, -0.002180f) };  //female lod0 neck
            meshSeamVerts[1][0][4] = new Vector3[0];
            meshSeamVerts[1][0][5] = new Vector3[] { new Vector3(0.0f, 1.16153f, 0.10832f), new Vector3(0.0f, 1.17486f, -0.05726f), 
                new Vector3(0.11117f, 1.17097f, 0.05975f), new Vector3(0.08326f, 1.16772f, 0.08886f), new Vector3(0.02036f, 1.1624f, 0.10652f), 
                new Vector3(0.05003f, 1.16448f, 0.10046f), new Vector3(0.11914f, 1.17389f, 0.01728f), new Vector3(0.11197f, 1.17412f, -0.01527f), 
                new Vector3(0.09584f, 1.17388f, -0.04124f), new Vector3(0.07065f, 1.1729f, -0.05077f), new Vector3(0.0456f, 1.17315f, -0.05503f), 
                new Vector3(0.01515f, 1.17431f, -0.05672f), new Vector3(-0.11117f, 1.17097f, 0.05975f), new Vector3(-0.08326f, 1.16772f, 0.08886f), 
                new Vector3(-0.02036f, 1.1624f, 0.10652f), new Vector3(-0.05003f, 1.16448f, 0.10046f), new Vector3(-0.11914f, 1.17389f, 0.01728f), 
                new Vector3(-0.11197f, 1.17412f, -0.01527f), new Vector3(-0.09584f, 1.17388f, -0.04124f), new Vector3(-0.07065f, 1.1729f, -0.05077f), 
                new Vector3(-0.0456f, 1.17315f, -0.05503f), new Vector3(-0.01515f, 1.17431f, -0.05672f) }; //female lod0 waist
            meshSeamVerts[1][0][6] = new Vector3[0];

            meshSeamVerts[1][1] = new Vector3[7][];         //female lod1
            meshSeamVerts[1][1][0] = new Vector3[] { new Vector3(0.100610f, 0.178310f, 0.013850f), new Vector3(0.084110f, 0.178310f, 0.010620f), 
                new Vector3(0.119550f, 0.178310f, -0.000550f), new Vector3(0.121740f, 0.178310f, -0.023150f), new Vector3(0.113930f, 0.178310f, -0.045780f), 
                new Vector3(0.092420f, 0.178310f, -0.055840f), new Vector3(0.073530f, 0.178310f, -0.016510f), new Vector3(0.078090f, 0.178310f, -0.041680f), 
                new Vector3(-0.100610f, 0.178310f, 0.013850f), new Vector3(-0.084110f, 0.178310f, 0.010620f), new Vector3(-0.119550f, 0.178310f, -0.000550f), 
                new Vector3(-0.121740f, 0.178310f, -0.023150f), new Vector3(-0.113930f, 0.178310f, -0.045780f), new Vector3(-0.092410f, 0.178310f, -0.055850f), 
                new Vector3(-0.073530f, 0.178310f, -0.016510f), new Vector3(-0.078090f, 0.178310f, -0.041680f) };  //female lod1 ankle
            meshSeamVerts[1][1][1] = new Vector3[0];
            meshSeamVerts[1][1][2] = new Vector3[0];
            meshSeamVerts[1][1][3] = new Vector3[] { new Vector3(0.042280f, 1.657290f, -0.037410f), new Vector3(0.046760f, 1.653590f, -0.018430f), 
                new Vector3(0.015410f, 1.627700f, 0.027820f), new Vector3(0.000000f, 1.624770f, 0.031360f), new Vector3(0.023620f, 1.657860f, -0.051050f), 
                new Vector3(0.035540f, 1.638510f, 0.013000f), new Vector3(0.025860f, 1.632110f, 0.022750f), new Vector3(0.045650f, 1.647510f, -0.002180f), 
                new Vector3(0.000000f, 1.658240f, -0.058220f), new Vector3(-0.042280f, 1.657290f, -0.037410f), new Vector3(-0.046760f, 1.653590f, -0.018430f), 
                new Vector3(-0.015410f, 1.627700f, 0.027820f), new Vector3(-0.023610f, 1.657860f, -0.051050f), new Vector3(-0.035540f, 1.638510f, 0.013000f), 
                new Vector3(-0.025860f, 1.632110f, 0.022750f), new Vector3(-0.045650f, 1.647510f, -0.002180f) };  //female lod1 neck
            meshSeamVerts[1][1][4] = new Vector3[0];
            meshSeamVerts[1][1][5] = new Vector3[] { new Vector3(-0.0f, 1.16153f, 0.10832f), new Vector3(-0.0f, 1.17486f, -0.05726f), 
                new Vector3(0.11117f, 1.17097f, 0.05975f), new Vector3(0.06664f, 1.1661f, 0.09466f), new Vector3(0.11914f, 1.17389f, 0.01728f), 
                new Vector3(0.11197f, 1.17412f, -0.01527f), new Vector3(0.08324f, 1.17339f, -0.04601f), new Vector3(0.0456f, 1.17315f, -0.05503f), 
                new Vector3(-0.11117f, 1.17097f, 0.05975f), new Vector3(-0.06664f, 1.1661f, 0.09466f), new Vector3(-0.11914f, 1.17389f, 0.01728f), 
                new Vector3(-0.11197f, 1.17412f, -0.01527f), new Vector3(-0.08324f, 1.17339f, -0.04601f), new Vector3(-0.0456f, 1.17315f, -0.05503f) };
            //female lod1 waist
            meshSeamVerts[1][1][6] = new Vector3[0];

            meshSeamVerts[1][2] = new Vector3[7][];         //female lod2
            meshSeamVerts[1][2][0] = new Vector3[] { new Vector3(0.110080f, 0.178310f, 0.006650f), new Vector3(0.084110f, 0.178310f, 0.010620f), 
                new Vector3(0.121740f, 0.178310f, -0.023150f), new Vector3(0.113930f, 0.178310f, -0.045780f), new Vector3(0.085250f, 0.178310f, -0.048760f), 
                new Vector3(0.073530f, 0.178310f, -0.016510f), new Vector3(-0.110080f, 0.178310f, 0.006650f), new Vector3(-0.084110f, 0.178310f, 0.010620f), 
                new Vector3(-0.121740f, 0.178310f, -0.023150f), new Vector3(-0.113930f, 0.178310f, -0.045780f), new Vector3(-0.085250f, 0.178310f, -0.048760f), 
                new Vector3(-0.073530f, 0.178310f, -0.016510f) };  //female lod2 ankle
            meshSeamVerts[1][2][1] = new Vector3[0];
            meshSeamVerts[1][2][2] = new Vector3[0];
            meshSeamVerts[1][2][3] = new Vector3[] { new Vector3(0.042280f, 1.657290f, -0.037420f), new Vector3(0.046760f, 1.653580f, -0.018440f), 
                new Vector3(0.015410f, 1.627700f, 0.027820f), new Vector3(0.000000f, 1.624770f, 0.031360f), new Vector3(0.023620f, 1.657860f, -0.051050f), 
                new Vector3(0.035540f, 1.638500f, 0.013000f), new Vector3(0.025860f, 1.632120f, 0.022750f), new Vector3(0.045650f, 1.647510f, -0.002180f), 
                new Vector3(-0.000000f, 1.658240f, -0.058220f), new Vector3(-0.042280f, 1.657290f, -0.037420f), new Vector3(-0.046760f, 1.653580f, -0.018440f), 
                new Vector3(-0.015410f, 1.627700f, 0.027820f), new Vector3(-0.023610f, 1.657860f, -0.051050f), new Vector3(-0.035540f, 1.638510f, 0.013000f), 
                new Vector3(-0.025860f, 1.632120f, 0.022750f), new Vector3(-0.045650f, 1.647510f, -0.002180f) };  //female lod2 neck
            meshSeamVerts[1][2][4] = new Vector3[0];
            meshSeamVerts[1][2][5] = new Vector3[] { new Vector3(-0.0f, 1.16153f, 0.10832f), new Vector3(-0.0f, 1.17486f, -0.05726f), 
                new Vector3(0.08891f, 1.16853f, 0.0772f), new Vector3(0.11914f, 1.17389f, 0.01728f), new Vector3(0.11197f, 1.17412f, -0.01527f), 
                new Vector3(0.06442f, 1.17327f, -0.05052f), new Vector3(-0.08891f, 1.16853f, 0.0772f), new Vector3(-0.11914f, 1.17389f, 0.01728f), 
                new Vector3(-0.11197f, 1.17412f, -0.01527f), new Vector3(-0.06442f, 1.17327f, -0.05052f) };  //female lod2 waist
            meshSeamVerts[1][2][6] = new Vector3[0];

            meshSeamVerts[1][3] = new Vector3[7][];         //female lod3
            meshSeamVerts[1][3][0] = new Vector3[] { new Vector3(0.100610f, 0.178310f, 0.013850f), new Vector3(0.121740f, 0.178310f, -0.023150f), 
                new Vector3(0.092420f, 0.178310f, -0.055840f), new Vector3(0.073530f, 0.178310f, -0.016510f), new Vector3(-0.100610f, 0.178310f, 0.013850f), 
                new Vector3(-0.121740f, 0.178310f, -0.023150f), new Vector3(-0.092410f, 0.178310f, -0.055850f), new Vector3(-0.073530f, 0.178310f, -0.016510f) };
            //female lod3 ankle
            meshSeamVerts[1][3][1] = new Vector3[0];
            meshSeamVerts[1][3][2] = new Vector3[0];
            meshSeamVerts[1][3][3] = new Vector3[] { new Vector3(0.046760f, 1.653580f, -0.018440f), new Vector3(-0.000000f, 1.624770f, 0.031360f), 
                new Vector3(0.032950f, 1.657570f, -0.044230f), new Vector3(0.025830f, 1.632110f, 0.022760f), new Vector3(-0.000000f, 1.658240f, -0.058220f), 
                new Vector3(-0.046760f, 1.653580f, -0.018440f), new Vector3(-0.032950f, 1.657570f, -0.044230f), new Vector3(-0.025860f, 1.632120f, 0.022750f) };
            //female lod3 neck
            meshSeamVerts[1][3][4] = new Vector3[0];
            meshSeamVerts[1][3][5] = new Vector3[] { new Vector3(-0.0f, 1.16153f, 0.10832f), new Vector3(-0.0f, 1.17486f, -0.05726f), 
                new Vector3(0.08891f, 1.16853f, 0.0772f), new Vector3(0.11914f, 1.17389f, 0.01728f), new Vector3(0.0882f, 1.17369f, -0.03289f), 
                new Vector3(-0.08891f, 1.16853f, 0.0772f), new Vector3(-0.11914f, 1.17389f, 0.01728f), new Vector3(-0.0882f, 1.17369f, -0.03289f) };
            //female lod3 waist
            meshSeamVerts[1][3][6] = new Vector3[0];

            meshSeamVerts[2][0] = new Vector3[7][];         //child lod0
            meshSeamVerts[2][0][0] = new Vector3[] { new Vector3(0.07243f, 0.11592f, 0.01694f), new Vector3(0.05307f, 0.11592f, 0.0112f),
                new Vector3(0.09189f, 0.11592f, 0.00385f), new Vector3(0.0952f, 0.11592f, -0.01715f), new Vector3(0.08587f, 0.11592f, -0.03915f),
                new Vector3(0.07431f, 0.11592f, -0.04292f), new Vector3(0.04333f, 0.11592f, -0.00869f), new Vector3(0.04767f, 0.11593f, -0.03418f),
                new Vector3(0.06141f, 0.11592f, -0.04133f), new Vector3(-0.05307f, 0.11592f, 0.0112f), new Vector3(-0.07243f, 0.11592f, 0.01694f),
                new Vector3(-0.09189f, 0.11592f, 0.00385f), new Vector3(-0.0952f, 0.11592f, -0.01715f), new Vector3(-0.08587f, 0.11592f, -0.03915f),
                new Vector3(-0.07431f, 0.11592f, -0.04292f), new Vector3(-0.04333f, 0.11592f, -0.00869f), new Vector3(-0.04767f, 0.11593f, -0.03418f),
                new Vector3(-0.06141f, 0.11592f, -0.04133f) };   //child lod0 ankle
            meshSeamVerts[2][0][1] = new Vector3[0];
            meshSeamVerts[2][0][2] = new Vector3[0];
            meshSeamVerts[2][0][3] = new Vector3[] { new Vector3(-0.03752f, 1.12267f, -0.03273f), new Vector3(-0.02065f, 1.12468f, -0.04379001f), 
                new Vector3(0f, 1.12563f, -0.04968f), new Vector3(0.02065f, 1.12477f, -0.04379001f), new Vector3(0.03752f, 1.12268f, -0.03273f), 
                new Vector3(-0.04215f, 1.11826f, -0.01802f), new Vector3(-0.03877f, 1.11206f, -0.00206f), new Vector3(-0.03276f, 1.10712f, 0.00977f), 
                new Vector3(-0.01436f, 1.09967f, 0.02605f), new Vector3(0f, 1.09771f, 0.029229f), new Vector3(-0.02296f, 1.10289f, 0.01801f), 
                new Vector3(0.04215f, 1.11824f, -0.01801f), new Vector3(0.03877f, 1.11205f, -0.00205f), new Vector3(0.03276f, 1.10712f, 0.00977f), 
                new Vector3(0.01436f, 1.09965f, 0.02605f), new Vector3(0.02295f, 1.10288f, 0.01802f) };  //child lod0 neck
            meshSeamVerts[2][0][4] = new Vector3[] { new Vector3(0.0914f, 0.7533001f, 0.03828f), new Vector3(0.06577f, 0.7498001f, 0.06943001f), 
                new Vector3(0.01949f, 0.74579f, 0.08594f), new Vector3(0.04228f, 0.74777f, 0.08012f), new Vector3(0f, 0.74493f, 0.08794f), 
                new Vector3(0.03662f, 0.7558801f, -0.06594001f), new Vector3(0.01616f, 0.7548701f, -0.06698f), new Vector3(0.09703f, 0.75579f, 0.00505f), 
                new Vector3(0.08787f, 0.75756f, -0.02397f), new Vector3(0.07434f, 0.75819f, -0.04569f), new Vector3(0.05926f, 0.75773f, -0.05923f), 
                new Vector3(0f, 0.75421f, -0.06595f), new Vector3(-0.0914f, 0.7533001f, 0.03828f), new Vector3(-0.06577f, 0.7498001f, 0.06944f), 
                new Vector3(-0.04228f, 0.74777f, 0.08014001f), new Vector3(-0.01949f, 0.74579f, 0.08594f), new Vector3(-0.01616f, 0.7548701f, -0.06698f), 
                new Vector3(-0.03662f, 0.7558801f, -0.06594001f), new Vector3(-0.09703f, 0.75579f, 0.00505f), new Vector3(-0.08787f, 0.75756f, -0.02397f), 
                new Vector3(-0.07434f, 0.7582f, -0.04569f), new Vector3(-0.05926f, 0.75773f, -0.05923f) }; //child lod0 waist
            meshSeamVerts[2][0][5] = new Vector3[0];
            meshSeamVerts[2][0][6] = new Vector3[0];

            meshSeamVerts[2][1] = new Vector3[7][];         //child lod1
            meshSeamVerts[2][1][0] = new Vector3[] { new Vector3(0.053066f, 0.115924f, 0.011202f), new Vector3(0.043329f, 0.115923f, -0.008687f), 
                new Vector3(0.072431f, 0.115924f, 0.016942f), new Vector3(0.09188901f, 0.115924f, 0.003849f), new Vector3(0.095204f, 0.115924f, -0.017147f), 
                new Vector3(0.08587401f, 0.115924f, -0.039149f), new Vector3(0.067858f, 0.115924f, -0.042117f), new Vector3(0.047673f, 0.115932f, -0.034183f), 
                new Vector3(-0.053066f, 0.115924f, 0.011202f), new Vector3(-0.043329f, 0.115923f, -0.008687f), new Vector3(-0.072431f, 0.115924f, 0.016942f), 
                new Vector3(-0.09188901f, 0.115924f, 0.003849f), new Vector3(-0.095204f, 0.115924f, -0.017147f), new Vector3(-0.08587401f, 0.115924f, -0.039149f), 
                new Vector3(-0.067858f, 0.115924f, -0.042117f), new Vector3(-0.047673f, 0.115932f, -0.034183f) };  //child lod1 ankle
            meshSeamVerts[2][1][1] = new Vector3[0];
            meshSeamVerts[2][1][2] = new Vector3[0];
            meshSeamVerts[2][1][3] = new Vector3[] { new Vector3(-0.03752f, 1.12268f, -0.03273f), new Vector3(-0.02065f, 1.12477f, -0.04379001f), 
                new Vector3(0f, 1.12563f, -0.04968f), new Vector3(0.02065f, 1.12477f, -0.04379001f), new Vector3(0.03752f, 1.12268f, -0.03273f), 
                new Vector3(-0.03276f, 1.10712f, 0.00977f), new Vector3(-0.03877f, 1.11205f, -0.002049f), new Vector3(-0.04215f, 1.11824f, -0.01801f), 
                new Vector3(0.04215f, 1.11824f, -0.01801f), new Vector3(0.03877f, 1.11205f, -0.002049f), new Vector3(0.03276f, 1.10712f, 0.00977f), 
                new Vector3(0f, 1.09771f, 0.02923f), new Vector3(-0.01436f, 1.09965f, 0.02605f), new Vector3(-0.02295f, 1.10288f, 0.01802f), 
                new Vector3(0.01436f, 1.09965f, 0.02605f), new Vector3(0.02295f, 1.10288f, 0.01802f) };  //child lod1 neck
            meshSeamVerts[2][1][4] = new Vector3[] { new Vector3(0.0914f, 0.7533001f, 0.03828f), new Vector3(0.05402f, 0.74878f, 0.07478f), 
                new Vector3(0f, 0.74493f, 0.08794f), new Vector3(0.09703f, 0.75579f, 0.00505f), new Vector3(0.08787f, 0.75756f, -0.02397f), 
                new Vector3(0.0668f, 0.75796f, -0.05246f), new Vector3(0.02639f, 0.75537f, -0.06646f), new Vector3(0f, 0.75421f, -0.06595f), 
                new Vector3(-0.0914f, 0.7533001f, 0.03828f), new Vector3(-0.05402f, 0.74878f, 0.07478f), new Vector3(-0.09703f, 0.75579f, 0.00505f), 
                new Vector3(-0.08787f, 0.75756f, -0.02397f), new Vector3(-0.0668f, 0.75796f, -0.05246f), new Vector3(-0.02639f, 0.75537f, -0.06646f) };
            //child lod1 waist
            meshSeamVerts[2][1][5] = new Vector3[0];
            meshSeamVerts[2][1][6] = new Vector3[0];

            meshSeamVerts[2][2] = new Vector3[7][];         //child lod2
            meshSeamVerts[2][2][0] = new Vector3[] { new Vector3(0.053066f, 0.115924f, 0.016333f), new Vector3(), 
                new Vector3(0.043329f, 0.115923f, -0.007374f), new Vector3(0.08216001f, 0.115924f, 0.015517f), new Vector3(0.095204f, 0.115924f, -0.017455f), 
                new Vector3(0.08587401f, 0.115924f, -0.041221f), new Vector3(0.057831f, 0.115938f, -0.038038f), new Vector3(-0.053066f, 0.115924f, 0.016333f), 
                new Vector3(-0.043329f, 0.115923f, -0.007374f), new Vector3(-0.08216001f, 0.115924f, 0.015517f), new Vector3(-0.095204f, 0.115924f, -0.017455f), 
                new Vector3(-0.08587401f, 0.115924f, -0.041221f), new Vector3(-0.057831f, 0.115938f, -0.038038f) };  //child lod2 ankle
            meshSeamVerts[2][2][1] = new Vector3[0];
            meshSeamVerts[2][2][2] = new Vector3[0];
            meshSeamVerts[2][2][3] = new Vector3[] { new Vector3(-0.03752f, 1.12267f, -0.03273f), new Vector3(0f, 1.12563f, -0.04968f), 
                new Vector3(-0.02065f, 1.12468f, -0.04379001f), new Vector3(0.02065f, 1.12468f, -0.04379001f), new Vector3(0.03752f, 1.12267f, -0.03273f), 
                new Vector3(-0.04215f, 1.11826f, -0.01802f), new Vector3(-0.03877f, 1.11206f, -0.002059f), new Vector3(-0.03276f, 1.10712f, 0.00977f), 
                new Vector3(0.03276f, 1.10712f, 0.00977f), new Vector3(0.03877f, 1.11206f, -0.002059f), new Vector3(0.04215f, 1.11826f, -0.01802f), 
                new Vector3(0f, 1.09771f, 0.02923f), new Vector3(-0.01436f, 1.09967f, 0.02605f), new Vector3(-0.02296f, 1.10289f, 0.01801f), 
                new Vector3(0.01436f, 1.09967f, 0.02605f), new Vector3(0.02296f, 1.10289f, 0.01801f) };  //child lod2 neck
            meshSeamVerts[2][2][4] = new Vector3[] { new Vector3(0.0914f, 0.7533001f, 0.03828f), new Vector3(0.05402f, 0.74878f, 0.07478f), 
                new Vector3(0f, 0.74493f, 0.08794f), new Vector3(0.09703f, 0.75579f, 0.00505f), new Vector3(0.08787f, 0.75756f, -0.02397f), 
                new Vector3(0.04613f, 0.75671f, -0.05946f), new Vector3(0f, 0.75421f, -0.06595f), new Vector3(-0.0914f, 0.7533001f, 0.03828f), 
                new Vector3(-0.05402f, 0.74878f, 0.07479f), new Vector3(-0.09703f, 0.75579f, 0.00505f), new Vector3(-0.08787f, 0.75756f, -0.02397f), 
                new Vector3(-0.04613f, 0.75672f, -0.05946f) };  //child lod2 waist
            meshSeamVerts[2][2][5] = new Vector3[0];
            meshSeamVerts[2][2][6] = new Vector3[0];

            meshSeamVerts[2][3] = new Vector3[7][];         //child lod3
            meshSeamVerts[2][3][0] = new Vector3[] { new Vector3(0.07243f, 0.11592f, 0.01694f), new Vector3(0.04333f, 0.11592f, -0.00869f), 
                new Vector3(0.0952f, 0.11592f, -0.01715f), new Vector3(0.06786f, 0.11592f, -0.04212f), new Vector3(-0.07243f, 0.11592f, 0.01694f), 
                new Vector3(-0.04333f, 0.11592f, -0.00869f), new Vector3(-0.0952f, 0.11592f, -0.01715f), new Vector3(-0.06786f, 0.11592f, -0.04212f) };
            //child lod3 ankle
            meshSeamVerts[2][3][1] = new Vector3[0];
            meshSeamVerts[2][3][2] = new Vector3[0];
            meshSeamVerts[2][3][3] = new Vector3[] { new Vector3(0.02908f, 1.12368f, -0.03826001f), new Vector3(0f, 1.12563f, -0.04968f), 
                new Vector3(-0.02908f, 1.12368f, -0.03826001f), new Vector3(-0.04215f, 1.11826f, -0.01802f), new Vector3(0.02296f, 1.10289f, 0.01801f), 
                new Vector3(0.04215f, 1.11826f, -0.01802f), new Vector3(0f, 1.09771f, 0.02923f), new Vector3(-0.02296f, 1.10289f, 0.01801f) };
            //child lod3 neck
            meshSeamVerts[2][3][4] = new Vector3[] { new Vector3(0f, 0.74493f, 0.08794f), new Vector3(0.0721f, 0.7513f, 0.05675f), 
                new Vector3(0.09703f, 0.75579f, 0.00505f), new Vector3(0f, 0.75421f, -0.06595f), new Vector3(-0.0721f, 0.7513f, 0.05675f), 
                new Vector3(-0.09703f, 0.75579f, 0.00505f) };
            //child lod3 waist
            meshSeamVerts[2][3][5] = new Vector3[0];
            meshSeamVerts[2][3][6] = new Vector3[0];

            return meshSeamVerts;
        }

        public int CompareTo(GEOM other)
        {
            if (this.numberVertices > other.numberVertices) return -1;
            else if (this.numberVertices < other.numberVertices) return 1;
            else return 0;
        }
    }
}
