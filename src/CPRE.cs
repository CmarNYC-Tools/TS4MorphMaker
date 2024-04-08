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
using s4pi.ImageResource;

namespace MorphTool
{
    public class CPRE
    {
        public uint version;
        public AgeGender ageGender;
        public AgeGender bodyFrameGender;
        public Species species = Species.Human;                // 1 = human
        public SimRegionPreset region;
        public SimSubRegion subRegion;
        public ArchetypeFlags archetype;
        public float displayIndex;
        public uint presetNameKey;
        public uint presetDescKey;
        public SculptLink[] sculpts;
        public Modifier[] modifiers;
        public byte unknown;
        public bool isPhysiqueSet;
        public float heavyValue;
        public float fitValue;
        public float leanValue;
        public float bonyValue;
        public bool isPartSet;
        public ulong partsetInstance;
        public BodyType partsetBodyType;
        public float chanceForRandom;
        public Tag[] tagList;

        public string PresetName;
        public TGI PresetTGI;
        public bool isDefaultReplacement;
        public ThumbnailResource Thumb;

        public CPRE(BinaryReader br)
        {
            this.version = br.ReadUInt32();
            this.ageGender = (AgeGender)br.ReadUInt32();
            if (this.version >= 11)
            {
                this.bodyFrameGender = (AgeGender)br.ReadUInt32();
            }
            if (this.version >= 8)
            {
                this.species = (Species)br.ReadUInt32();
            }
            this.region = (SimRegionPreset)br.ReadUInt32();
            if (this.version >= 9)
            {
                this.subRegion = (SimSubRegion)br.ReadUInt32();
            }
            this.archetype = (ArchetypeFlags)br.ReadUInt32();
            this.displayIndex = br.ReadSingle();
            this.presetNameKey = br.ReadUInt32();
            this.presetDescKey = br.ReadUInt32();
            uint numSculpts = br.ReadUInt32();
            this.sculpts = new SculptLink[numSculpts];
            for (int i = 0; i < numSculpts; i++)
            {
                this.sculpts[i] = new SculptLink(br, this.version);
            }
            uint numModifiers = br.ReadUInt32();
            this.modifiers = new Modifier[numModifiers];
            for (int i = 0; i < numModifiers; i++)
            {
                this.modifiers[i] = new Modifier(br, this.version);
            }
            this.isPhysiqueSet = br.ReadBoolean();
            if (this.isPhysiqueSet)
            {
                this.heavyValue = br.ReadSingle();
                this.fitValue = br.ReadSingle();
                this.leanValue = br.ReadSingle();
                this.bonyValue = br.ReadSingle();
            }
            if (this.version >= 12)
            {
                this.isPartSet = br.ReadBoolean();
                if (isPartSet)
                {
                    this.partsetInstance = br.ReadUInt64();
                    this.partsetBodyType = (BodyType)br.ReadUInt32();
                }
            }
            this.chanceForRandom = br.ReadSingle();
            uint tagCount = br.ReadUInt32();
            this.tagList = new Tag[tagCount];
            for (int i = 0; i < tagCount; i++)
            {
                this.tagList[i] = new Tag(br, this.version);
            }
        }

        public CPRE()
        {
            this.version = 12;
            this.ageGender = AgeGender.None;
            this.bodyFrameGender = AgeGender.Male & AgeGender.Female;
            this.species = Species.Human;
            this.region = SimRegionPreset.EYES;
            this.subRegion = SimSubRegion.None;
            this.archetype = ArchetypeFlags.None;
            this.displayIndex = 0;
            this.presetNameKey = 0x811C9DC5;
            this.presetDescKey = 0x811C9DC5;
            uint numSculpts = 0;
            this.sculpts = new SculptLink[numSculpts];
            uint numModifiers = 0;
            this.modifiers = new Modifier[numModifiers];
            this.isPhysiqueSet = false;
            this.isPartSet = false;
            this.chanceForRandom = 0;
            uint tagCount = 0;
            this.tagList = new Tag[tagCount];
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(this.version);
            bw.Write((uint)this.ageGender);
            if (this.version >= 11)
            {
                bw.Write((uint)this.bodyFrameGender);
            }
            if (this.version >= 8)
            {
                bw.Write((uint)this.species);
            }
            bw.Write((uint)this.region);
            if (this.version >= 9)
            {
                bw.Write((uint)this.subRegion);
            }
            bw.Write((uint)this.archetype);
            bw.Write(this.displayIndex);
            bw.Write(this.presetNameKey);
            bw.Write(this.presetDescKey);
            if (this.sculpts == null) this.sculpts = new SculptLink[0];
            bw.Write(sculpts.Length);
            for (int i = 0; i < sculpts.Length; i++)
            {
                this.sculpts[i].Write(bw);
            }
            if (this.modifiers == null) this.modifiers = new Modifier[0];
            bw.Write(modifiers.Length);
            for (int i = 0; i < modifiers.Length; i++)
            {
                this.modifiers[i].Write(bw);
            }
            bw.Write(this.isPhysiqueSet);
            if (this.isPhysiqueSet)
            {
                bw.Write(this.heavyValue);
                bw.Write(this.fitValue);
                bw.Write(this.leanValue);
                bw.Write(this.bonyValue);
            }
            if (this.version >= 12)
            {
                bw.Write(this.isPartSet);
                if (this.isPartSet)
                {
                    bw.Write(this.partsetInstance);
                    bw.Write((uint)this.partsetBodyType);
                }
            }
            bw.Write(this.chanceForRandom);
            if (this.tagList == null) this.tagList = new Tag[0];
            bw.Write(tagList.Length);
            for (int i = 0; i < tagList.Length; i++)
            {
                this.tagList[i].Write(bw);
            }
        }

        public void ReplaceSculptInstance(ulong oldInstance, ulong newInstance)
        {
            for (int i = 0; i < this.sculpts.Length; i++)
            {
                if (this.sculpts[i].instance == 0) continue;
                if (this.sculpts[i].instance == oldInstance) this.sculpts[i].instance = newInstance;
            }
        }

        public void ReplaceSmodInstance(ulong oldInstance, ulong newInstance)
        {
            for (int i = 0; i < this.modifiers.Length; i++)
            {
                if (this.modifiers[i].instance == 0) continue;
                if (this.modifiers[i].instance == oldInstance) this.modifiers[i].instance = newInstance;
            }
        }


        public class SculptLink
        {
            private uint parentVersion;
            public ulong instance;
            public uint region;     //deprecated

            public SculptLink(BinaryReader br, uint version)
            {
                this.parentVersion = version;
                this.instance = br.ReadUInt64();
                if (version < 9)
                {
                    this.region = br.ReadUInt32();
                }
            }

            public SculptLink(ulong instance, SimRegion region, uint version)
            {
                this.parentVersion = version;
                this.instance = instance;
                if (version < 9)
                {
                    this.region = (uint)region;
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.instance);
                if (parentVersion < 9)
                {
                    bw.Write(this.region);
                }
            }
        }

        public class Modifier
        {
            private uint parentVersion;
            public ulong instance;
            public float weight;
            public uint region;     //deprecated

            public Modifier(BinaryReader br, uint version)
            {
                this.parentVersion = version;
                this.instance = br.ReadUInt64();
                this.weight = br.ReadSingle();
                if (version < 9)
                {
                    this.region = br.ReadUInt32();
                }
            }

            public Modifier(ulong SIMO_instance, float weight, SimRegion region, uint version)
            {
                this.parentVersion = version;
                this.instance = SIMO_instance;
                this.weight = weight;
                this.region = (uint)region;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.instance);
                bw.Write(this.weight);
                if (parentVersion < 9)
                {
                    bw.Write(this.region);
                }
            }
        }

        public class Tag
        {
            uint parentVersion;
            public ushort tagType;
            public uint tagValue;

            public Tag(BinaryReader br, uint version)
            {
                this.parentVersion = version;
                this.tagType = br.ReadUInt16();
                if (version >= 10)
                {
                    this.tagValue = br.ReadUInt32();
                }
                else
                {
                    this.tagValue = br.ReadUInt16();
                }
            }

            public Tag(PresetCategoryTags category, PresetValueTags value, uint version)
            {
                this.parentVersion = version;
                this.tagType = (ushort)category;
                this.tagValue = (uint)value;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.tagType);
                if (parentVersion >= 10)
                {
                    bw.Write(this.tagValue);
                }
                else
                {
                    bw.Write((ushort)this.tagValue);
                }
            }
        }
    }
}
