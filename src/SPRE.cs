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
using System.Linq;
using System.Text;
using System.IO;

namespace MorphTool
{
    public class SPRE
    {
        private uint version;
        private AgeGender ageGender;
        private Species species;
        private uint isCASPreset;
        private float sortPriority;
        private ushort unknown2;
        private uint presetID;
        private uint unknown3;
        private uint[] coatSwatches;
        private ulong coatPatternLink;
        private ulong simOutfitLink;
        private List<Tag> flagList;

        public SPRE(BinaryReader r)
        {
            this.version = r.ReadUInt32();
            this.ageGender = (AgeGender)r.ReadUInt32();
            this.species = (Species)r.ReadUInt32();
            this.isCASPreset = r.ReadUInt32();
            this.sortPriority = r.ReadSingle();
            this.unknown2 = r.ReadUInt16();
            this.presetID = r.ReadUInt32();
            this.unknown3 = r.ReadUInt32();
            byte count = r.ReadByte();
            this.coatSwatches = new uint[count];
            for (int i = 0; i < count; i++)
            {
                this.coatSwatches[i] = r.ReadUInt32();
            }
            this.coatPatternLink = r.ReadUInt64();
            this.simOutfitLink = r.ReadUInt64();
            int flagCount = r.ReadInt32();
            this.flagList = new List<Tag>();
            for (int i = 0; i < flagCount; i++)
            {
                this.flagList.Add(new Tag(r, this.version));
            }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.version);
            w.Write((uint)this.ageGender);
            w.Write((uint)this.species);
            w.Write(this.isCASPreset);
            w.Write(this.sortPriority);
            w.Write(this.unknown2);
            w.Write(this.presetID);
            w.Write(this.unknown3);
            w.Write((byte)this.coatSwatches.Length);
            for (int i = 0; i < this.coatSwatches.Length; i++)
            {
                w.Write(this.coatSwatches[i]);
            }
            w.Write(this.coatPatternLink);
            w.Write(this.simOutfitLink);
            w.Write(this.flagList.Count);
            for (int i = 0; i < flagList.Count; i++)
            {
                this.flagList[i].Write(w);
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
                if (version >= 5)
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

            public Tag(Tag other)
            {
                this.parentVersion = other.parentVersion;
                this.tagType = other.tagType;
                this.tagValue = other.tagValue;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.tagType);
                if (parentVersion >= 5)
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
