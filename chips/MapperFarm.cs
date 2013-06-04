using System;
using System.Collections.Generic;
using System.Text;

using cstari.chips.mappers;

/*******************************************************************************
**
**  PYTARI - Atari 2600 emulation in python
**
**  Copyright (c) 2000-2005 by Bryon Vandiver
**
**  See the file "license" for information on usage and redistribution of
**  this file, and for a DISCLAIMER OF ALL WARRANTIES.
**
********************************************************************************
**
**   Cartridge Mappers, Interfaces a UInt8 Numeric array
**
**   This creates a glue from a numeric array to the atari
**   bus, with a dictionary containing the mappers.
**
**   The Cart API is very simple, consisting of only one call,
**   access, which does read and write at the same time, over
**   all of the atari"s addressing space
**
**   It can be assumed that only the first 13 bits of the address
**   will be passed
**
**
********************************************************************************/

/*
    ;  10 -- Compumate
    ;  15 -- Starpath
 */

namespace cstari.chips
{
    static public class MapperFarm
    {
        private delegate Mapper CartCreator( byte[] data );
        private static Dictionary<int,string[]> mappersBySize;
        private static Dictionary<string, CartCreator> mapperCreate;
        public static int MaxRomSize = 0x10000;

        static MapperFarm()
        {
            mappersBySize = new Dictionary<int,string[]>();

            mappersBySize[0x800] = new string[] {
                "2k Mirrored",
                "Commavid"
            };

            mappersBySize[0x1000] = new string[] {
                "4k Standard"
            };

            mappersBySize[0x2000] = new string[] {
                "Standard Mapping", 
                "Super Chip Mapping", 
                "Parker Brothers", 
                "Tiger Vision", 
                "Reversed Standard",
                "Activision",
                "UA Limited"
            };

            // Trimmed rom support
            mappersBySize[0x2800] = new string[] {
                "Pitfall II"
            };

            mappersBySize[0x28FF] = new string[] {
                "Pitfall II"
            };

            mappersBySize[12288] = new string[] {
                "CBS"
            };

            mappersBySize[0x4000] = new string[] { 
                "Standard Mapping", 
                "Super Chip Mapping", 
                "M-Network"
            };

            mappersBySize[0x8000] = new string[] { 
                "Standard Mapping", 
                "Super Chip Mapping",
                "Tigervision Extended"
            };


            mappersBySize[0x10000] = new string[] { 
                "Standard Mapping", 
                "Super Chip Mapping",
                "Megaboy",
                "Tigervision Extended"
            };

            mappersBySize[0x20000] = new string[] {
                "Tigervision Extended"
            };

            mappersBySize[0x40000] = new string[] {
                "Tigervision Extended"
            };

            mappersBySize[0x80000] = new string[] {
                "Tigervision Extended"
            };

            mapperCreate = new Dictionary<string, CartCreator>();

            mapperCreate["2k Mirrored"] = CreateTwoK;
            mapperCreate["4k Standard"] = CreateFourK;
            mapperCreate["Standard Mapping"] = CreateStandard;
            mapperCreate["Super Chip Mapping"] = CreateSuperChip;
            mapperCreate["Parker Brothers"] = CreateParkerBrothers;
            mapperCreate["Tiger Vision"] = CreateTigerVision;
            mapperCreate["Activision"] = CreateActivision;
            mapperCreate["M-Network"] = CreateMNetwork;
            mapperCreate["Megaboy"] = CreateMegaboy;
            mapperCreate["Pitfall II"] = CreatePitfallII;
            mapperCreate["CBS"] = CreateCBS;
            mapperCreate["Reversed Standard"] = CreateReversedStandard;
            mapperCreate["Commavid"] = CreateCommavid;
            mapperCreate["Tigervision Extended"] = CreateTigerVisionExtended;
            mapperCreate["UA Limited"] = CreateUALimited;
        }

        public static bool AllowedSize( int size )
        {
            return mappersBySize.ContainsKey(size);
        }               

        public static string[] MappersBySize(int size)
        {
            string[] s = mappersBySize[size];

            if (s != null)
                return s;

            return null;
        }

        public static Mapper CreateByName(string name, byte[] data)
        {
            CartCreator cc = mapperCreate[name];

            if (cc != null)
            {
                return cc(data);
            }

            return null;
        }

        // DELEGATES FOR NEW STATEMENTS

        static private Mapper CreateUALimited(byte[] data)
        {
            return new UALimited(data);
        }
        
        static private Mapper CreateTwoK(byte[] data)
        {
            return new TwoK(data);
        }

        static private Mapper CreateCommavid(byte[] data)
        {
            return new Commavid(data);
        }

        static private Mapper CreateFourK(byte[] data)
        {
            return new FourK(data);
        }

        static private Mapper CreatePitfallII(byte[] data)
        {
            return new PitfallII(data);
        }

        static private Mapper CreateStandard(byte[] data)
        {
            return new Standard(data);
        }

        static private Mapper CreateReversedStandard(byte[] data)
        {
            return new ReversedStandard(data);
        }

        static private Mapper CreateSuperChip(byte[] data)
        {
            return new SuperChip(data);
        }

        static private Mapper CreateParkerBrothers(byte[] data)
        {
            return new ParkerBrothers(data);
        }

        static private Mapper CreateTigerVision(byte[] data)
        {
            return new TigerVision(data);
        }

        static private Mapper CreateTigerVisionExtended(byte[] data)
        {
            return new TigerVisionExtended(data);
        }

        static private Mapper CreateActivision(byte[] data)
        {
            return new Activision(data);
        }

        static private Mapper CreateMNetwork(byte[] data)
        {
            return new MNetwork(data);        
        }

        static private Mapper CreateCBS(byte[] data)
        {
            return new CBS(data);
        }

        static private Mapper CreateMegaboy(byte[] data)
        {
            return new Megaboy(data);
        }
    }
}
