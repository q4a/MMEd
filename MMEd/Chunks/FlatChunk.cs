using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.ComponentModel;
using MMEd.Util;

// A Flat from within the SHET
// Represents a drivable surface.

namespace MMEd.Chunks
{
    public class FlatChunk : Chunk
    {
        // The values in this enum are valid Idxs for the third 
        // dimension of the TexMetaData array
        public enum TexMetaDataEntries
        {
            Zero = 0, // something which also follows the course. Can't be zeroed out without invalidating the level
            One = 1, // something which seems to always be zero
            Two = 2, // something to do with the corners? No immediately noticeable difference when zeroed out.
            CameraPos = 3, // the camera position idx. Indexes entries in qq
            Four = 4, // some sort of isobars around the path of the course, and ranges 0-4 or so. This seems to be something to do with the restart position for when you die
            Waypoint = 5, // an ascending counter for the position
            Bumpmap = 6, // the bump map. Indexes entries in qq
            Seven = 7 // follows closely the path of the course, except for odd bits 0 elsewhere. It seems to control the restart angle after death, or the offset in the tex square where you restart.        
        }

        // The weapons available in the game
        public enum WeaponType
        {
            Grabber = 1,
            Ommer = 2,
            Molotovs = 3,
            TurboBall = 4, //the power to turn into a white ball
            FlameBall = 5,
            Invisibility = 6,
            Mallet = 7,
            Molotovs2 = 8, //don't know what the difference is here
            FireTrail = 9,
            Missiles = 10,
            Mines = 11,
            GroupGrabber = 12,
            OmniOmmer = 13,
            MultiMallet = 14,
            GroupSpeedUp = 15
        }

        [Description("The number at the start of the Flat. I haven't seen it refrenced anywhere yet")]
        public short DeclaredIdx;

        [Description("The name at the start of the Flat. I haven't seen it refrenced anywhere yet")]
        public string DeclaredName;

        [Description("The position of the top left corner of this sheet, in world co-ordinates")]
        public Short3Coord OriginPosition;

        [Description(@"A 3-rotation vector for this Flat. X measures the positive rotation about
            the X-axis, with 1024 being 90 degrees, and so on for Y and Z. The order of rotation
            is cruicial. qq: write it in here, then!")]
        public Short3Coord RotationVector;

        [Description("The width, in texture squares of this Flat")]
        public short Width;

        [Description("The height, in texture squares of this Flat")]
        public short Height;

        [Description("The width of each texture square, in world co-ordinates")]
        public short ScaleX;

        [Description("The height of each texture square, in world co-ordinates")]
        public short ScaleY;

        [Description(@"An array of the texture associated with each square in the tex
 array. This indexes the ""tiles"" NamedImageGroup.")]
        public short[][] TextureIds;

        [Description(@"An array of the height of the top left corner of each square in 
the Flat. Negative is ""up""")]
        public short[][] TerrainHeight;

        [Description("Short Flag: determines whether the Flat is solid and has a metadata array")]
        public bool FlgA;

        [Description("Short Flag: unk")]
        public bool FlgB;

        [Description("Short Flag: unk")]
        public bool FlgC;

        [Description(@"Short Flag: seems to control whether the Flat is visible. 
        May be dependent on A. Sometimes ignored")]
        public bool FlgD;

        [Description("Short Flag: many ramps have this set to true")]
        public bool FlgE;

        [Description("No idea. Length is determined by flagA")]
        public byte[] NextN;

        [Description(@"One metadata entry for each tex square.
See enum TexMetaDataEntries. Arry dimensions are Width*Height*8. Only Flats with FlgA have this")]
        public byte[][][] TexMetaData;

        [Description("The objects on this Flat")]
        public ObjectEntry[] Objects;

        [Description("The weapons on this Flat")]
        public WeaponEntry[] Weapons;

        public void Deserialise(BinaryReader bin)
        {
            //header
            DeclaredIdx = bin.ReadInt16();
            DeclaredName = StreamUtils.ReadASCIINullTermString(bin.BaseStream);
            if (DeclaredName.Length != 8)
                throw new DeserialisationException("Expecting name to be length 8 & null-terminated", bin.BaseStream.Position);

            //more header:
            OriginPosition = Short3Coord.ReadShort3Coord64(bin);
            RotationVector = Short3Coord.ReadShort3Coord64(bin);
            Width = bin.ReadInt16();
            Height = bin.ReadInt16();
            ScaleX = bin.ReadInt16();
            ScaleY = bin.ReadInt16();

            //every flat has a tex array:
            TextureIds = new short[Width][]; //not allowed 2d initalisers!!!
            TerrainHeight = new short[Width][];
            for (int x = 0; x < Width; x++)
            {
                TextureIds[x] = new short[Height];
                TerrainHeight[x] = new short[Height];
                for (int y = 0; y < Height; y++)
                {
                    TextureIds[x][y] = bin.ReadInt16();
                    TerrainHeight[x][y] = bin.ReadInt16();
                }
            }

            //General flags
            FlgA = StreamUtils.ReadShortFlag(bin);
            FlgB = StreamUtils.ReadShortFlag(bin);
            FlgC = StreamUtils.ReadShortFlag(bin);
            FlgD = StreamUtils.ReadShortFlag(bin);
            FlgE = StreamUtils.ReadByteFlag(bin);  //qq that looks wrong to me, but it works!

            //what do these mean?
            NextN = bin.ReadBytes(FlgA ? 2 : 6); 

            //unless (FlgA), we're done...
            if (FlgA)
            {
                //load the tex metadata
                TexMetaData = new Byte[Width][][];
                for (int x = 0; x < Width; x++)
                {
                    TexMetaData[x] = new Byte[Height][];
                    for (int y = 0; y < Height; y++)
                    {
                        TexMetaData[x][y] = bin.ReadBytes(8);
                    }
                }

                //objects:
                short objectCount = bin.ReadInt16();
                Objects = new ObjectEntry[objectCount];
                for (int i = 0; i < objectCount; i++)
                {
                    Objects[i] = new ObjectEntry(bin);
                }

                //weapons:
                short weapCount = bin.ReadInt16();
                Weapons = new WeaponEntry[weapCount];
                for (int i = 0; i < weapCount; i++)
                {
                    Weapons[i] = new WeaponEntry(bin);
                }
            }
        }

        public override void Deserialise(System.IO.Stream inStr)
        {
            Deserialise(new BinaryReader(inStr));
        }

        public override void Serialise(Stream outStr)
        {
            BinaryWriter bout = new BinaryWriter(outStr);

            //header
            bout.Write(DeclaredIdx);
            StreamUtils.WriteASCIINullTermString(outStr, DeclaredName);
            OriginPosition.WriteShort3Coord64(bout);
            RotationVector.WriteShort3Coord64(bout);
            bout.Write(Width);
            bout.Write(Height);
            bout.Write(ScaleX);
            bout.Write(ScaleY);
 
            //every flat has a tex array:
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    bout.Write(TextureIds[x][y]);
                    bout.Write(TerrainHeight[x][y]);
                }
            }

            //General flags
            StreamUtils.WriteShortFlag(bout, FlgA);
            StreamUtils.WriteShortFlag(bout, FlgB);
            StreamUtils.WriteShortFlag(bout, FlgC);
            StreamUtils.WriteShortFlag(bout, FlgD);
            StreamUtils.WriteByteFlag(bout, FlgE); //N.B. byte here. See Deserialise.

            //what do these mean?
            bout.Write(NextN);

            //unless (FlgA), we're done...
            if (FlgA)
            {
                //load the tex metadata
                for (int x = 0; x < Width; x++)
                {
                     for (int y = 0; y < Height; y++)
                    {
                        bout.Write(TexMetaData[x][y]);
                    }
                }

                //objects:
                bout.Write((short)Objects.Length);
                foreach (ObjectEntry o in Objects)
                {
                    o.WriteToStream(bout);
                }
                
                //weapons:
                bout.Write((short)Weapons.Length);
                foreach (WeaponEntry w in Weapons)
                {
                    w.WriteToStream(bout);
                }
            }
        }

        public override string Name
        {
            get
            {
                return string.Format("[{0}] {1}", DeclaredIdx, DeclaredName);
            }
         }

        public class ObjectEntry
        {
            [Description("The position of the origin of the object, in the co-ordinate space of the parent Flat")]
            public Short3Coord OriginPosition;

            [Description("A 3-rotation vector for this object. See Flat.RotationVector")]
            public Short3Coord RotationVector;

            [Description("The type of the object. Indexes the OBJT array (TODO)")]
            public short ObjtType;

            [Description("A byte flag of unknown meaning")]
            public bool FlagUnknown;

            [Description("Whether this object is solid. May not always be honoured.")]
            public bool IsSolid;

            [Description("dunno")]
            public short ShortUnknown;

            public ObjectEntry() { }
            public ObjectEntry(BinaryReader bin)
            {
                RotationVector = Short3Coord.ReadShort3Coord64(bin);
                OriginPosition = Short3Coord.ReadShort3Coord64(bin);
                ObjtType = bin.ReadInt16();
                FlagUnknown = StreamUtils.ReadByteFlag(bin);
                IsSolid = StreamUtils.ReadByteFlag(bin);
                ShortUnknown = bin.ReadInt16();
            }

            public void WriteToStream(BinaryWriter bout)
            {
                RotationVector.WriteShort3Coord64(bout);
                OriginPosition.WriteShort3Coord64(bout);
                bout.Write(ObjtType);
                StreamUtils.WriteByteFlag(bout, FlagUnknown);
                StreamUtils.WriteByteFlag(bout, IsSolid);
                bout.Write(ShortUnknown);
            }
        }

        public class WeaponEntry
        {
            [Description("The type of the weapon. See enum WeaponType")]
            public WeaponType WeaponType;

            [Description("dunno")]
            public short ShortUnknown;

            [Description("The position of the weapon, in the co-ordinate space of the parent Flat")]
            public Short3Coord Position;

            public WeaponEntry() { }
            public WeaponEntry(BinaryReader bin)
            {
                try
                {
                    WeaponType = (WeaponType)bin.ReadInt16();
                }
                catch (InvalidCastException e)
                {
                    throw new DeserialisationException("Unrecognised weapon type: " + e, bin.BaseStream.Position);
                }
                ShortUnknown = bin.ReadInt16();
                Position = Short3Coord.ReadShort3Coord64(bin);
            }

            public void WriteToStream(BinaryWriter bout)
            {
                 bout.Write((short)WeaponType);
                 bout.Write(ShortUnknown);
                 Position.WriteShort3Coord64(bout);
            }
        }

        public FlatChunk() { }
        public FlatChunk(BinaryReader bin) { Deserialise(bin); }

        public override void ReplaceChild(Chunk xiFrom, Chunk xiTo)
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}
