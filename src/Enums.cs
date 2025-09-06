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

namespace MorphTool
{
    public enum Species : uint
    {
        Human = 1,
        Dog = 2,
        Cat = 3,
        LittleDog = 4,
        Werewolf = 5,
        Horse = 6
    }

    [Flags]
    public enum OccultTypeFlags : uint
    {
        Fairy = 1 << 6,
        Werewolf = 1 << 5,
        Spellcaster = 1 << 4,
        Mermaid = 1 << 3,
        Vampire = 1 << 2,
        Alien = 1 << 1,
        Human = 1,
        None = 0
    }
    
    [Flags]
    public enum AgeGender : uint
    {
        None = 0,
        Baby = 0x00000001,
        Toddler = 0x00000002,
        Child = 0x00000004,
        Teen = 0x00000008,
        YoungAdult = 0x00000010,
        Adult = 0x00000020,
        Elder = 0x00000040,
        Infant = 0x00000080,
        Male = 0x00001000,
        Female = 0x00002000,
        Unisex = 0x00003000
    }

    public enum Physiques : byte
    {
        BODYBLENDTYPE_HEAVY = 0,
        BODYBLENDTYPE_FIT = 1,
        BODYBLENDTYPE_LEAN = 2,
        BODYBLENDTYPE_BONY = 3,
        BODYBLENDTYPE_PREGNANT = 4,
        BODYBLENDTYPE_HIPS_WIDE = 5,
        BODYBLENDTYPE_HIPS_NARROW = 6,
        BODYBLENDTYPE_WAIST_WIDE = 7,
        BODYBLENDTYPE_WAIST_NARROW = 8,
        BODYBLENDTYPE_IGNORE = 9,   // Assigned to deformation maps associated with sculpts or modifiers, instead of a physique.
        BODYBLENDTYPE_AVERAGE = 100, // Special case used to indicate an "average" deformation map always applied for a given age
    }

    public enum ShapeOrNormals : byte
    {
        SHAPE_DEFORMER = 0,     // This resource contains positional deltas
        NORMALS_DEFORMER = 1    // This resource contains normal deltas
    }

    public enum RobeChannel : byte
    {
        ROBECHANNEL_PRESENT = 0,
        ROBECHANNEL_DROPPED = 1,
        ROBECHANNEL_ISCOPY = 2,     // Robe data not present but is the same as skin tight data.
    }

    public enum SimRegion : uint 
    {
        EYES = 0,
        NOSE,
        MOUTH,
        CHEEKS,
        CHIN,
        JAW,
        FOREHEAD,

        // Modifier-only face regions
        BROWS = 8,
        EARS,
        HEAD,

        // Other face regions
        FULLFACE = 12,

        // Modifier body regions
        CHEST = 14,
        UPPERCHEST,
        NECK,
        SHOULDERS,
        UPPERARM,
        LOWERARM,
        HANDS,
        WAIST,
        HIPS,
        BELLY,
        BUTT,
        THIGHS,
        LOWERLEG,
        FEET,

        // Other body regions
        BODY,
        UPPERBODY,
        LOWERBODY,
        TAIL,
        FUR,
        FORELEGS,
        HINDLEGS,

      //  ALL = LOWERBODY + 1,     // all

        CUSTOM_MaleParts = 50,
        CUSTOM_FemaleParts,
        CUSTOM_Breasts,
        CUSTOM_Chest,
        CUSTOM_UpperChest,
        CUSTOM_Back,
        CUSTOM_Neck,
        CUSTOM_Shoulders,
        CUSTOM_UpperArm,
        CUSTOM_LowerArm,
        CUSTOM_Hands,
        CUSTOM_Waist,
        CUSTOM_Hips,
        CUSTOM_Belly,
        CUSTOM_Butt,
        CUSTOM_Thighs,
        CUSTOM_LowerLeg,
        CUSTOM_Feet,
        CUSTOM_Misc1,
        CUSTOM_Misc2,
        CUSTOM_Misc3,
        CUSTOM_Misc4,
        CUSTOM_Misc5
    }

    public enum SimSubRegion
    {
        None = 0,
        EarsUp = 1,
        EarsDown = 2,
        TailLong = 3,
        TailRing = 4,
        TailScrew = 5,
        TailStub = 6
    }

    public enum SimRegionPreset : uint
    {
        EYES = 0,
        NOSE,
        MOUTH,
        CHEEKS,
        CHIN,
        JAW,
        FOREHEAD,

        // Modifier-only face regions
        BROWS = 8,
        EARS,
        HEAD,

        // Other face regions
        FULLFACE = 12,

        // Other body regions
        BODY = 28,
       // TAIL = 31,
        FUR = 32
    }

    public enum ArchetypeFlags : uint       //used in CASPreset
    {
        None = 0x00000000,

        Caucasian = 0x00000001,
        African = 0x00000002,
        Asian = 0x00000004,
        MiddleEastern= 0x00000008,
        NativeAmerican = 0x00000010,

        All = 0xffffffff
    }

    public enum PresetCategoryTags : ushort
    {
        Archetype = 0x0045,
        Occult = 0x006D
    }

    public enum PresetValueTags : uint
    {
        African = 0x0049,
        Asian = 0x004B,
        Caucasian = 0x004C,
        Latin = 0x0138,
        MiddleEastern = 0x004A,
        NorthAmerican = 0x0059,
        SouthAsian = 0x0058,
        Human = 0x051E,
        Alien = 0x301F,
        Vampire = 0x068D,
        Mermaid = 0x08A0,
        Witch = 0x08E7,
        Werewolf = 0x0ADB,
        Fairy = 0x00000CE9,
    }

    public enum ResourceTypes : uint
    {
        BlendGeometry = 0x067CAA11U,
        RLE2 = 0x3453CF95U,
        RLES = 0xBA856C78,
        DDS = 0x00B2D882,
        BoneDelta = 0x0355E0A6U,
        DeformerMap = 0xDB43E069U,
        HotSpotControl = 0x8B18FF6EU,
        NameMap = 0x0166038CU,
        SimModifier = 0xC5F6763EU,
        CASPreset = 0xEAA32ADDU,
        Sculpt = 0x9D1AB874U,
        Thumbnail = 0x5B282D45U,
        LRLE = 0x2BC04EDFU
    }

    public enum BgeoLinkTag : uint
    {
        NoBGEO = 0,
        UseBGEO = 0x30000001U
    }

    public enum EarType : int
    {
        Up = 0,
        Down = 1
    }
    public enum TailType : int
    {
        Long = 0,
        Stub = 1,
        Ring = 2,
        Screw = 3
    }

    public enum BodyType        //used in CASP
    {
        All = 0,
        Hat = 1,
        Hair = 2,
        Head = 3,
        Face = 4,
        Body = 5,
        Top = 6,
        Bottom = 7,
        Shoes = 8,
        Accessories = 9,
        Earrings = 0x0A,
        Glasses = 0x0B,
        Necklace = 0x0C,
        Gloves = 0x0D,
        BraceletLeft = 0x0E,
        BraceletRight = 0x0F,
        LipRingLeft = 0x10,
        LipRingRight = 0x11,
        NoseRingLeft = 0x12,
        NoseRingRight = 0x13,
        BrowRingLeft = 0x14,
        BrowRingRight = 0x15,
        RingIndexLeft = 0x16,
        RingIndexRight = 0x17,
        RingThirdLeft = 0x18,
        RingThirdRight = 0x19,
        RingMidLeft = 0x1A,
        RingMidRight = 0x1B,
        FacialHair = 0x1C,
        Lipstick = 0x1D,
        Eyeshadow = 0x1E,
        Eyeliner = 0x1F,
        Blush = 0x20,
        Facepaint = 0x21,
        Eyebrows = 0x22,
        Eyecolor = 0x23,
        Socks = 0x24,
        Mascara = 0x25,
        ForeheadCrease = 0x26,
        Freckles = 0x27,
        DimpleLeft = 0x28,
        DimpleRight = 0x29,
        Tights = 0x2A,
        MoleLeftLip = 0x2B,
        MoleRightLip = 0x2C,
        TattooArmLowerLeft = 0x2D,
        TattooArmUpperLeft = 0x2E,
        TattooArmLowerRight = 0x2F,
        TattooArmUpperRight = 0x30,
        TattooLegLeft = 0x31,
        TattooLegRight = 0x32,
        TattooTorsoBackLower = 0x33,
        TattooTorsoBackUpper = 0x34,
        TattooTorsoFrontLower = 0x35,
        TattooTorsoFrontUpper = 0x36,
        MoleLeftCheek = 0x37,
        MoleRightCheek = 0x38,
        MouthCrease = 0x39,
        SkinOverlay = 0x3A,
        Fur = 0x3B,
        AnimalEars = 0x3C,
        Tail = 0x3D,
        NoseColor = 0x3E,
        SecondaryEyeColor = 0x3F,
        OccultBrow = 0x40,
        OccultEyeSocket = 0x41,
        OccultEyeLid = 0x42,
        OccultMouth = 0x43,
        OccultLeftCheek = 0x44,
        OccultRightCheek = 0x45,
        OccultNeckScar = 0x46,
        SkinDetailScar = 0x47,
        SkinDetailAcne = 0x48,
        Fingernails = 0x49,
        Toenails = 0x4A,
        HairColor = 0x4B,
        Bite = 0x4C,
        BodyFreckles = 0x4D,
        BodyHairArm = 0x4E,
        BodyHairLeg = 0x4F,
        BodyHairTorsoFront = 0x50,
        BodyHairTorsoBack = 0x51,
        BodyScarArmLeft = 0x52,
        BodyScarArmRight = 0x53,
        BodyScarTorsoFront = 0x54,
        BodyScarTorsoBack = 0x55,
        BodyScarLegLeft = 0x56,
        BodyScarLegRight = 0x57,
        AttachmentBack = 0x58,
        TeenAcne = 0x59,
        ScarFace = 0x5A,
        BirthmarkFace = 0x5B,
        BirthmarkTorsoBack = 0x5C,
        BirthmarkTorsoFront = 0x5D,
        BirthmarkArms = 0x5E,
        MoleFace,
        MoleChestUpper,
        MoleBackUpper,
        BirthmarkLegs,
        StretchMarksFront,
        StretchMarksBack,
        Saddle,
        Bridle,
        Reins,
        Blanket,
        SkinDetailHoofColor,
        HairMane,
        HairTail,
        HairForelock,
        HairFeathers,
        Horn,
        TailBase,
        BirthmarkOccult,
        TattooHead,
        Wings,
        HeadDeco,
        SkinSpecularity,
    }
}
