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
using System.Linq;
using System.Text;
using System.IO;

namespace MorphTool
{
    public class HOTC
    {
        private uint version;       //0x0000000F
        private AgeGender ageFrame;
        private Species species;
        private OccultTypeFlags occult;
        private HotSpotLevel level;
        private byte sliderID;
        private SliderCursor cursor;
        private SimRegion region;
        private SimSubRegion subRegion;
        private RestrictToGender gender;
        private ulong textureReference;
        private uint unknown4;
        public List<Slider> sliderDescriptions;

        public string HotSpotName;
        public TGI HotSpotTGI;
        public bool isDefaultReplacement;
        public uint CurrentVersion = 0x0F;

        public uint Version { get { return this.version; } set { this.version = value; } }
        public Species Species { get { return this.species; } set { this.species = value; } }
        public OccultTypeFlags Occult { get { return this.occult; } set { this.occult = value; } }
        public AgeGender AgeFrame { get { return this.ageFrame; } set { this.ageFrame = value; } }
        public HotSpotLevel Level { get { return this.level; } set { this.level = value; } }
        public byte ColorID { get { return this.sliderID; } set { this.sliderID = value; } }
        public SliderCursor Cursor { get { return this.cursor; } set { this.cursor = value; } }
        public SimRegion Region { get { return this.region; } set { this.region = value; } }
        public SimSubRegion SubRegion { get { return this.subRegion; } set { this.subRegion = value; } }
        public RestrictToGender RestricttoGender { get { return this.gender; } set { this.gender = value; } }
        public ulong TextureReference { get { return this.textureReference; } set { this.textureReference = value; } }

        public HOTC(BinaryReader br)
        {
            this.version = br.ReadUInt32();
            this.ageFrame = (AgeGender)br.ReadUInt32();
            this.species = (Species)br.ReadUInt32();
            if (version >= 0x0F) this.occult = (OccultTypeFlags)br.ReadUInt32();
            this.level = (HotSpotLevel)br.ReadByte();
            this.sliderID = br.ReadByte();
            this.cursor = (SliderCursor)br.ReadByte();
            this.region = (SimRegion)br.ReadUInt32();
            this.subRegion = (SimSubRegion)br.ReadUInt32();
            this.gender = (RestrictToGender)br.ReadByte();
            this.textureReference = br.ReadUInt64();
            this.unknown4 = br.ReadUInt32();
            byte sliderCount = br.ReadByte();
            this.sliderDescriptions = new List<Slider>();
            for (int i = 0; i < sliderCount; i++)
            {
                this.sliderDescriptions.Add(new Slider(br));
            }
        }

        public HOTC(HOTC other)
        {
            this.version = other.version;
            this.ageFrame = other.ageFrame;
            this.species = other.species;
            this.occult = other.occult;
            this.level = other.level;
            this.sliderID = other.sliderID;
            this.cursor = other.cursor;
            this.region = other.region;
            this.subRegion = other.subRegion;
            this.gender = other.gender;
            this.textureReference = other.textureReference;
            this.unknown4 = other.unknown4;
            this.sliderDescriptions = new List<Slider>();
            for (int i = 0; i < other.sliderDescriptions.Count; i++)
            {
                this.sliderDescriptions.Add(new Slider(other.sliderDescriptions[i]));
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(this.version);
            bw.Write((uint)this.ageFrame);
            bw.Write((uint)this.species);
            if (version >= 0x0F) bw.Write((uint)this.occult);
            bw.Write((byte)this.level);
            bw.Write(this.sliderID);
            bw.Write((byte)this.cursor);
            bw.Write((uint)this.region);
            bw.Write((uint)this.subRegion);
            bw.Write((byte)this.gender);
            bw.Write(this.textureReference);
            bw.Write(this.unknown4);
            if (sliderDescriptions == null) sliderDescriptions = new List<Slider>();
            bw.Write((byte)this.sliderDescriptions.Count);
            for (int i = 0; i < this.sliderDescriptions.Count; i++)
            {
                this.sliderDescriptions[i].Write(bw);
            }
        }

        public void ReplaceSmodInstance(ulong oldInstance, ulong newInstance)
        {
            for (int i = 0; i < this.sliderDescriptions.Count; i++)
            {
                for (int j = 0; j < this.sliderDescriptions[i].simModifierReference.Length; j++)
                {
                    if (this.sliderDescriptions[i].simModifierReference[j] == 0) continue;
                    if (this.sliderDescriptions[i].simModifierReference[j] == oldInstance)
                            this.sliderDescriptions[i].simModifierReference[j] = newInstance;
                }
            }
        }

        public class Slider
        {
            private ViewAngle angle;
            private bool flip;              // reverse directions in left profile
            private float[] unknown7;       //2 floats
            internal ulong[] simModifierReference;      //4 Instance IDs, Left, Right, Up, Down

            public ViewAngle Angle { get { return this.angle; } set { this.angle = value; } }
            public bool Flip { get { return this.flip; } set { this.flip = value; } }
            public ulong[] SimModifierInstances { get { return this.simModifierReference; } set { this.simModifierReference = value; } }

            public Slider()
            {
                this.angle = HOTC.ViewAngle.None;
                this.flip = false;
                this.unknown7 = new float[] { 1, 1 };
                this.simModifierReference = new ulong[] { 0, 0, 0, 0 };
            }

            public Slider(BinaryReader br)
            {
                this.angle = (ViewAngle)br.ReadByte();
                this.flip = br.ReadBoolean();
                this.unknown7 = new float[2];
                for (int i = 0; i < 2; i++) { this.unknown7[i] = br.ReadSingle(); }
                this.simModifierReference = new ulong[4];
                for (int i = 0; i < 4; i++) { this.simModifierReference[i] = br.ReadUInt64(); }
            }

            public Slider(Slider other)
            {
                this.angle = other.angle;
                this.flip = other.flip;
                this.unknown7 = new float[2];
                if (other.unknown7 != null)
                {
                    for (int i = 0; i < 2; i++) { this.unknown7[i] = other.unknown7[i]; }
                }
                this.simModifierReference = new ulong[4];
                if (other.simModifierReference != null)
                {
                    for (int i = 0; i < 4; i++) { this.simModifierReference[i] = other.simModifierReference[i]; }
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write((byte)this.angle);
                bw.Write(this.flip);
                for (int i = 0; i < 2; i++) { bw.Write(this.unknown7[i]); }
                for (int i = 0; i < 4; i++) { bw.Write(this.simModifierReference[i]); }
            }
        }

        public enum HotSpotLevel : byte
        {
            Normal = 0,
            Micro = 1,
            TopLevel = 2
        }

        [Flags]
        public enum ViewAngle : byte
        {
            None = 0,
            Front = 1 << 0,
            Threequarter_right = 1 << 1,
            Threequarter_left = 1 << 2,
            Profile_right = 1 << 3,
            Profile_left = 1 << 4,
            Back = 1 << 5,
        }

        public enum RestrictToGender : byte
        {
            None = 0,
            Male = 1,
            Female = 2
        }

        public enum SliderCursor : byte
        {
            None = 0,
            HorizontalAndVerticalArrows = 1,
            HorizontalArrows = 2,
            VerticalArrows = 3,
            Diagonal = 4,
            Rotation = 5
        }

    }
}
