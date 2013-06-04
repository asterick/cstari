/*******************************************************************************
**
**  cstari - Atari 2600 emulation in python
**
**  Copyright (c) 2000-2005 by Bryon Vandiver
**
**  See the file "license" for information on usage and redistribution of
**  this file, and for a DISCLAIMER OF ALL WARRANTIES.
**
********************************************************************************
**
**   Television Interface Adapter Emulation
**
**   This is the "smartest" processor in the whole system,
**   providing I/O to the joysticks, video signal generation,
**   and audio output.  It has very simple, although robust
**   collision detection.  This will be the hardest part of the
**   project to create.
**
**   Addendum: This turned out to be the third hardest part to
**   create
**
**   pygame, as of right now, does not have an API supportive of
**   realtime audio.  as a result of this, no sound emulation
**   will be supplied.
**
**			  [----]	Object Bit Identifier
**			 @  ||		Priority encoder bit	
**			@|  ||		ORed in on the second half of the palette (SCORE mode)
**			||  ||
**	PF	0x41	01000001	Playfield
**	BL	0x42	01000010	Ball
**	P1	0x10	00000100	Player 2
**	M1	0x20	00001000	Player 2 Missle
**	P0	0x04	00010000	Player 1
**	M0	0x08	00100000	Player 1 Missle
**
**
**       The bits are converted to a number, which is used as an index.
**       The way the palette is generated is pretty self explainitory,
**       I made an excel sheet for the mapping, so It would be rather
**       hard to cut and paste, so I"ll spare you.
**
*******************************************************************************/

using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using cstari.input;
using cstari.utility;

namespace cstari.chips
{
    /// <summary>
    /// Summary description for tia.
    /// </summary>
    public class TIA
    {
        // Collision Detection and Drawing Masks

        enum screenMasks : byte
        {
            PF_BIT      = 0x01,
            BL_BIT      = 0x02,
            P0_BIT      = 0x10,
            M0_BIT      = 0x20,
            P1_BIT      = 0x04,
            M1_BIT      = 0x08,
            SIDE_BIT    = 0x40
        };
        public enum collisionMasks : byte
        {
            BL_PF_MASK = TIA.screenMasks.PF_BIT | TIA.screenMasks.BL_BIT,
            P0_PF_MASK = TIA.screenMasks.PF_BIT | TIA.screenMasks.P0_BIT,
            M0_PF_MASK = TIA.screenMasks.PF_BIT | TIA.screenMasks.M0_BIT,
            P1_PF_MASK = TIA.screenMasks.PF_BIT | TIA.screenMasks.P1_BIT,
            M1_PF_MASK = TIA.screenMasks.PF_BIT | TIA.screenMasks.M1_BIT,
            P0_BL_MASK = TIA.screenMasks.BL_BIT | TIA.screenMasks.P0_BIT,
            M0_BL_MASK = TIA.screenMasks.BL_BIT | TIA.screenMasks.M0_BIT,
            P1_BL_MASK = TIA.screenMasks.BL_BIT | TIA.screenMasks.P1_BIT,
            M1_BL_MASK = TIA.screenMasks.BL_BIT | TIA.screenMasks.M1_BIT,
            M0_P0_MASK = TIA.screenMasks.P0_BIT | TIA.screenMasks.M0_BIT,
            P1_P0_MASK = TIA.screenMasks.P0_BIT | TIA.screenMasks.P1_BIT,
            M1_P0_MASK = TIA.screenMasks.P0_BIT | TIA.screenMasks.M1_BIT,
            P1_M0_MASK = TIA.screenMasks.M0_BIT | TIA.screenMasks.P1_BIT,
            M1_M0_MASK = TIA.screenMasks.M0_BIT | TIA.screenMasks.M1_BIT,
            M1_P1_MASK = TIA.screenMasks.P1_BIT | TIA.screenMasks.M1_BIT
        };
        public enum collisionIndicies : int
        {
            BL_PF_INDEX,
            P0_PF_INDEX,
            M0_PF_INDEX,
            P1_PF_INDEX,
            M1_PF_INDEX,
            P0_BL_INDEX,
            M0_BL_INDEX,
            P1_BL_INDEX,
            M1_BL_INDEX,
            M0_P0_INDEX,
            P1_P0_INDEX,
            M1_P0_INDEX,
            P1_M0_INDEX,
            M1_M0_INDEX,
            M1_P1_INDEX
        };
        public enum playfieldModes : byte
        {
            PF_NORMAL = 0,
            PF_SCORE = 2,
            PF_PRI = 4,
            PF_PRI_SCORE = 6
        };

        private static int VideoFrameHeight = 264;

        private SdlDotNet.Graphics.Surface m_VideoFrame;

        // Controller objects
        private Controller m_CtrlA;
        private Controller m_CtrlB;

        // Audio channel classes

        private AudioMixer.AudioChannel m_SndChA;
        private AudioMixer.AudioChannel m_SndChB;

        public byte[] m_ScanLine;
        public byte[] m_LastScanLine;
        private uint[] m_ScanLineBlitted;

        // Collision masks and flags
        private static byte[] m_CollisionMasks = new byte[15] {
                (byte)collisionMasks.BL_PF_MASK, 
                (byte)collisionMasks.P0_PF_MASK, 
                (byte)collisionMasks.M0_PF_MASK,
                (byte)collisionMasks.P1_PF_MASK, 
                (byte)collisionMasks.M1_PF_MASK, 
                (byte)collisionMasks.P0_BL_MASK,
                (byte)collisionMasks.M0_BL_MASK, 
                (byte)collisionMasks.P1_BL_MASK, 
                (byte)collisionMasks.M1_BL_MASK,
                (byte)collisionMasks.M0_P0_MASK, 
                (byte)collisionMasks.P1_P0_MASK, 
                (byte)collisionMasks.M1_P0_MASK,
                (byte)collisionMasks.P1_M0_MASK, 
                (byte)collisionMasks.M1_M0_MASK, 
                (byte)collisionMasks.M1_P1_MASK
            };

        public byte[] m_Collisions;

        // Random Constant Arrays
        private static byte[] m_MirrorByte = new byte[256] {
			  0, 128,  64, 192,  32, 160,  96, 224,  16, 144,  80, 208,  48, 176, 112, 240,
			  8, 136,  72, 200,  40, 168, 104, 232,  24, 152,  88, 216,  56, 184, 120, 248,
			  4, 132,  68, 196,  36, 164, 100, 228,  20, 148,  84, 212,  52, 180, 116, 244,  
			 12, 140,  76, 204,  44, 172, 108, 236,  28, 156,  92, 220,  60, 188, 124, 252, 
			  2, 130,  66, 194,  34, 162,  98, 226,  18, 146,  82, 210,  50, 178, 114, 242, 
			 10, 138,  74, 202,  42, 170, 106, 234,  26, 154,  90, 218,  58, 186, 122, 250, 
			  6, 134,  70, 198,  38, 166, 102, 230,  22, 150,  86, 214,  54, 182, 118, 246, 
			 14, 142,  78, 206,  46, 174, 110, 238,  30, 158,  94, 222,  62, 190, 126, 254,  
			  1, 129,  65, 193,  33, 161,  97, 225,  17, 145,  81, 209,  49, 177, 113, 241, 
			  9, 137,  73, 201,  41, 169, 105, 233,  25, 153,  89, 217,  57, 185, 121, 249, 
			  5, 133,  69, 197,  37, 165, 101, 229,  21, 149,  85, 213,  53, 181, 117, 245, 
		     13, 141,  77, 205,  45, 173, 109, 237,  29, 157,  93, 221,  61, 189, 125, 253,
			  3, 131,  67, 195,  35, 163,  99, 227,  19, 147,  83, 211,  51, 179, 115, 243,
			 11, 139,  75, 203,  43, 171, 107, 235,  27, 155,  91, 219,  59, 187, 123, 251,
		      7, 135,  71, 199,  39, 167, 103, 231,  23, 151,  87, 215,  55, 183, 119, 247, 
			 15, 143,  79, 207,  47, 175, 111, 239,  31, 159,  95, 223,  63, 191, 127, 255 };

        private static int[] m_Repeats = new int[8] { 0, 1, 1, 2, 1, 0, 2, 0 };
        private static int[] m_Stretch = new int[8] { 1, 1, 1, 1, 1, 2, 1, 4 };
        private static int[] m_GapSize = new int[8] { 0, 16, 32, 16, 64, 0, 32, 0 };
        private uint[] m_Palette;

        // Misc Registers
        private bool m_Blanking;
        public bool m_Syncing;

        private int m_Clock;
        public int m_vPos;
        public int m_hPos;
        private int m_hDirty;
        private int m_BlankPeriod;

        // Draw Mode Registers
        public byte m_DrawMode;

        // Color Registers
        private uint[] m_ColorTranslate;
        private byte m_ChromaPF;
        private byte m_ChromaP0;
        private byte m_ChromaP1;
        private byte m_ChromaBK;

        // HMOVE Registers
        public int m_MoveP0;
        public int m_MoveP1;
        public int m_MoveM0;
        public int m_MoveM1;
        public int m_MoveBL;

        // Object positions
        public int m_P0Pos;
        public int m_P1Pos;
        public int m_M0Pos;
        public int m_M1Pos;
        public int m_BLPos;

        // Object sizes and spacing
        public int m_P0MidDelay;
        public int m_P1MidDelay;
        public int m_M0Size;
        public int m_M1Size;
        public int m_BLSize;

        public int m_GapP0;
        public int m_StretchP0;
        public int m_RepeatP0;
        public int m_GapP1;
        public int m_StretchP1;
        public int m_RepeatP1;

        public bool m_RefPF;
        public bool m_RefP0;
        public bool m_RefP1;

        // Missle Lock registers
        public bool m_M0Lock;
        public bool m_M1Lock;

        // Enable and graphic registers
        public bool m_EnaM0;
        public bool m_EnaM1;
        public bool m_EnaBL;
        public bool m_EnaBLDel;
        public byte m_P0Grp;
        public byte m_P0GrpDel;
        public byte m_P1Grp;
        public byte m_P1GrpDel;
        public byte m_PF0Grp;
        public byte m_PF1Grp;
        public byte m_PF2Grp;

        // Delay Registers
        public bool m_VDelP0;
        public bool m_VDelP1;
        public bool m_VDelBL;

        // Extra cycles value (for riot)
        private int m_CyclesAdded;

        public TIA(Controller CtrlA, Controller CtrlB)
        {
            // Initialize fast collision masking

            m_Collisions = new byte[15];

            m_ColorTranslate = new uint[192];
            m_ScanLine = new byte[224];			// NOTE: extra +64 bytes for overflow
            m_LastScanLine = new byte[224];     // 

            m_CtrlA = CtrlA;
            m_CtrlB = CtrlB;

            m_Blanking = false;
            m_Syncing = false;

            m_Clock = 0;
            m_vPos = 0;
            m_hPos = 0;
            m_hDirty = 0;
            m_BlankPeriod = 0;

            m_DrawMode = 0;

            m_ChromaPF = 0;
            m_ChromaP0 = 0;
            m_ChromaP1 = 0;
            m_ChromaBK = 0;

            m_MoveP0 = 0;
            m_MoveP1 = 0;
            m_MoveM0 = 0;
            m_MoveM1 = 0;
            m_MoveBL = 0;

            m_P0Pos = 0;
            m_P1Pos = 0;
            m_M0Pos = 0;
            m_M1Pos = 0;
            m_BLPos = 0;

            m_P0MidDelay = 0;
            m_P1MidDelay = 0;
            m_M0Size = 0;
            m_M1Size = 0;
            m_BLSize = 0;

            m_GapP0 = 0;
            m_StretchP0 = 0;
            m_RepeatP0 = 0;
            m_GapP1 = 0;
            m_StretchP1 = 0;
            m_RepeatP1 = 0;

            m_RefPF = false;
            m_RefP0 = false;
            m_RefP1 = false;

            // Missle Lock registers
            m_M0Lock = false;
            m_M1Lock = false;

            // Enable and graphic registers
            m_EnaM0 = false;
            m_EnaM1 = false;
            m_EnaBL = false;
            m_EnaBLDel = false;
            m_P0Grp = 0;
            m_P0GrpDel = 0;
            m_P1Grp = 0;
            m_P1GrpDel = 0;
            m_PF0Grp = 0;
            m_PF1Grp = 0;
            m_PF2Grp = 0;

            // Delay Registers
            m_VDelP0 = false;
            m_VDelP1 = false;
            m_VDelBL = false;

            // Create audio channels
            m_SndChA = AudioMixer.OpenChannel();
            m_SndChB = AudioMixer.OpenChannel();

            m_VideoFrame = new SdlDotNet.Graphics.Surface(320, VideoFrameHeight, 32);
            m_VideoFrame.AlphaBlending = true;
            m_VideoFrame.Alpha = 127;

            m_ScanLineBlitted = new uint[160*VideoFrameHeight];
        }

        ~TIA()
        {
            AudioMixer.ReleaseChannel(m_SndChA);
            AudioMixer.ReleaseChannel(m_SndChB);
        }

        public Surface Display
        {
            get { return m_VideoFrame; }
        }

        public void plug(Controller CtrlA, Controller CtrlB)
        {
            m_CtrlA = CtrlA;
            m_CtrlB = CtrlB;
        }

        public int SyncCycles
        {
            get
            {
                int cycles = m_CyclesAdded;
                m_CyclesAdded = 0;
                return cycles;
            }
        }

        public Color ChromaBK
        {
            get
            {
                return Color.FromArgb((int)m_Palette[m_ChromaBK]);
            }
        }

        public Color ChromaPF
        {
            get
            {
                return Color.FromArgb((int)m_Palette[m_ChromaPF]);
            }
        }

        public Color ChromaP0
        {
            get
            {
                return Color.FromArgb((int)m_Palette[m_ChromaP0]);
            }
        }

        public Color ChromaP1
        {
            get
            {
                return Color.FromArgb((int)m_Palette[m_ChromaP1]);
            }
        }

        public int Playfield
        {
            get
            {
                return (m_PF0Grp << 16) | (m_PF1Grp << 8) | m_PF2Grp;
            }
        }

        public Color GetColor( bool previous, int x )
        {
            byte[] line = previous ? m_LastScanLine : m_ScanLine;

            return Color.FromArgb((int)m_ColorTranslate[line[x]]);
        }

        public void clock(int cycles)
        {
            m_Clock += cycles;
        }

        public void SetSignal(SignalType signal)
        {
            int sampleRate;

            m_Palette = Palette.SetSignal(signal);

            if (signal == SignalType.VIDEO_NTSC)
            {
                sampleRate = 31440;
                m_BlankPeriod = -40;
            }
            else if (signal == SignalType.VIDEO_PAL)
            {
                sampleRate = 31200;
                m_BlankPeriod = -48;
            }
            else
            {
                sampleRate = 31200;
                m_BlankPeriod = -48;
            }

            m_SndChA.SetInputRate(sampleRate);
            m_SndChB.SetInputRate(sampleRate);
        }

        private void ScanIn(int loc, byte pattern, byte mask, int stretch, bool mirror)
        {
            if (loc >= 160)
                return;

            if (mirror)
                pattern = m_MirrorByte[pattern];

            while ( pattern != 0 )
            {
                if ((pattern & 0x80) != 0)
                {
                    for (int rep = stretch; rep > 0; rep--)
                        m_ScanLine[loc++] |= mask;
                }
                else
                {
                    for (int rep = stretch; rep > 0; rep--)
                        m_ScanLine[loc++] &= (byte)~mask;
                }

                pattern <<= 1;
            }

            // Mask rest of the scanline
            while (loc < 160)
            {
                m_ScanLine[loc++] &= (byte)~mask;
            }
        }

        private void LatchSeralizers(int start, int end)
        {
            int opos;
            int i;

            // --- TIA AUDIO SERIALIZERS ---
            
            if (start <= 0 && 0 < end)
            {
                m_SndChA.Tick();
                m_SndChB.Tick();
            }
            if (start <= 114 && 114 < end)
            {
                m_SndChA.Tick();
                m_SndChB.Tick();
            }

            // --- BLANKING PERIOD HAS NO SCANNERS ---
            if (m_Syncing || m_Blanking)
                return;

            // --- SCAN IN THE PLAYFIELD -----

            if (start <= 0 && 0 < end)
                ScanIn(0, (byte)(m_PF0Grp >> 4), (byte)screenMasks.PF_BIT, 4, true);
            if (start <= 16 && 16 < end)
                ScanIn(16, m_PF1Grp, (byte)screenMasks.PF_BIT, 4, false);
            if (start <= 48 && 48 < end)
                ScanIn(48, m_PF2Grp, (byte)screenMasks.PF_BIT, 4, true);

            if (m_RefPF)
            {
                if (start <= 80 && 80 < end)
                    ScanIn(80, m_PF2Grp, (byte)screenMasks.PF_BIT, 4, false);
                if (start <= 112 && 112 < end)
                    ScanIn(112, m_PF1Grp, (byte)screenMasks.PF_BIT, 4, true);
                if (start <= 144 && 144 < end)
                    ScanIn(144, m_PF0Grp, (byte)screenMasks.PF_BIT, 4, false);
            }
            else
            {
                if (start <= 80 && 80 < end)
                    ScanIn(80, (byte)(m_PF0Grp >> 4), (byte)screenMasks.PF_BIT, 4, true);
                if (start <= 96 && 96 < end)
                    ScanIn(96, m_PF1Grp, (byte)screenMasks.PF_BIT, 4, false);
                if (start <= 128 && 128 < end)
                    ScanIn(128, m_PF2Grp, (byte)screenMasks.PF_BIT, 4, true);
            }

            // --- SCAN IN THE BALL ----------        

            if (start <= m_BLPos && m_BLPos < end && (m_VDelBL ? m_EnaBLDel : m_EnaBL))
                ScanIn(m_BLPos, (byte)0x80, (byte)screenMasks.BL_BIT, m_BLSize, false);

            // --- SCAN IN THE MISSLES -------

            if (m_EnaM0 && !m_M0Lock)
            {
                for (i = 0; i <= m_RepeatP0; i++)
                {
                    opos = m_M0Pos + i * m_GapP0;
                    if (opos >= 160) opos -= 160;

                    if (start < opos && opos <= end)
                        ScanIn(opos, (byte)0x80, (byte)screenMasks.M0_BIT, m_M0Size, false);
                }
            }

            if (m_EnaM1 && !m_M1Lock)
            {
                for (i = 0; i <= m_RepeatP1; i++)
                {
                    opos = m_M1Pos + i * m_GapP1;
                    if (opos >= 160) opos -= 160;

                    if (start < opos && opos <= end)
                        ScanIn(opos, (byte)0x80, (byte)screenMasks.M1_BIT, m_M1Size, false);
                }
            }

            // --- SCAN IN PLAYER GRAPHICS ---
            for (i = 0; i <= m_RepeatP0; i++)
            {
                opos = m_P0Pos + i * m_GapP0;
                if (opos >= 160) opos -= 160;

                if (start < opos && opos <= end)
                    ScanIn(opos, (byte)(m_VDelP0 ? m_P0GrpDel : m_P0Grp), (byte)screenMasks.P0_BIT, m_StretchP0, m_RefP0);
            }

            for (i = 0; i <= m_RepeatP1; i++)
            {
                opos = m_P1Pos + i * m_GapP1;
                if (opos >= 160) opos -= 160;

                if (start < opos && opos <= end)
                    ScanIn(opos, (byte)(m_VDelP1 ? m_P1GrpDel : m_P1Grp), (byte)screenMasks.P1_BIT, m_StretchP1, m_RefP1);
            }
        }

        public void ClockCatchUp()
        {
            LatchSeralizers(m_hPos, m_hPos + m_Clock);
            ForceScanDraw();

            m_hPos = m_hPos + m_Clock;
            m_Clock = 0;

            // Rollover clock, scanning any additional lines that may have been continued
            while (m_hPos >= 160)
            {                
                ForceScanDraw();

                m_hPos -= 160 + 68;
                m_vPos += 1;
                m_hDirty = 0;

                byte[] last = m_ScanLine;
                m_ScanLine = m_LastScanLine;
                m_LastScanLine = last;

                int i;
                for (i = 0; i < 80; i++)
                    m_ScanLine[i] = 0;
                for (; i < 160; i++)
                    m_ScanLine[i] = (byte)screenMasks.SIDE_BIT;

                LatchSeralizers(-68, m_hPos);
            }
        }

        public unsafe void FrameCopy(Surface surf)
        {
            surf.Lock();
            uint* pixels = (uint*)surf.Pixels.ToPointer();
            
            int increase = surf.Pitch / surf.BytesPerPixel - surf.Width;
            int p = 0;

            for (int y = 0; y < VideoFrameHeight; y++)
            {
                for (int i = 0; i < 160; i++)
                {
                    uint c = m_ScanLineBlitted[p++];

                    *(pixels++) = c;
                    *(pixels++) = c;
                }
                pixels += increase;
            }
            surf.Unlock();
        }

        public void ForceScanDraw()
        {
            if (m_hPos <= 0)
            {
                return;
            }

            if (m_vPos >= VideoFrameHeight || m_vPos < 0)
                return;

            int end = (m_hPos > 160) ? 160 : m_hPos;
            FindCollisions(m_hDirty, end);

            int i = m_hDirty;
            int p = m_vPos * 160 + m_hDirty;
                        
            while( i < end )
            {
                m_ScanLineBlitted[p++] = m_ColorTranslate[m_ScanLine[i++]];
            }

            m_hDirty = m_hPos;
        }

        private void FindCollisions(int start, int end)
        {
            if (end >= 160)
                end = 159;

            for(int m = 0; m < m_CollisionMasks.Length; m++)
            {
                byte mask = m_CollisionMasks[m];

                if (m_Collisions[m] != 0)
                    continue;

                for (int x = start; x <= end; x++)
                {
                    if ((m_ScanLine[x] & mask) == mask)
                    {
                        m_Collisions[m] = 0x40;
                        break;
                    }
                }
            }
        }

        #region Scan Control Registers

        private void VSYNC(byte data)
        {
            bool newSync = (data & 0x2) != 0;

//            if (!m_Syncing && newSync)
//            {
//                FrameCopy(m_VideoFrame);
//                m_vPos = m_BlankPeriod;
//            }

            m_Syncing = newSync;
        }

        private void VBLANK(byte data)
        {
            bool oldBlanking = m_Blanking;

            m_Blanking = (data & 0x2) != 0;

            if (oldBlanking && !m_Blanking)
            {
                FrameCopy(m_VideoFrame);
                m_vPos = 0;
            }

            if (m_CtrlA != null)
            {
                m_CtrlA.ground((data & 0x80) != 0);
                m_CtrlA.latch((data & 0x40) != 0);
            }
            if (m_CtrlB != null)
            {
                m_CtrlB.ground((data & 0x80) != 0);
                m_CtrlB.latch((data & 0x40) != 0);
            }
        }

        private void WSYNC(byte data)
        {
            if (m_hPos != -68)
            {
                m_CyclesAdded += 160 - m_hPos;
            }
        }

        private void RSYNC(byte data)
        {
            m_hPos = 0;
            m_Clock = 0;
            m_hDirty = 0;
        }
        #endregion
        #region Rendering Options Registers

        private void CTRLPF(byte data)
        {
            m_RefPF = (data & 0x1) != 0;
            m_DrawMode = (byte)(data & 0x6);
            m_BLSize = 1 << ((data >> 4) & 0x3);

            UpdatePalette();
        }

        private void REFP0(byte data)
        {
            m_RefP0 = (data & 0x8) != 0;
        }

        private void REFP1(byte data)
        {
            m_RefP1 = (data & 0x8) != 0;
        }

        #endregion
        #region Object Size Registers

        private void NUSIZ0(byte data)
        {
            m_M0Size = 1 << ((data >> 4) & 0x3);

            int nusiz = data & 0x7;
            m_StretchP0 = m_Stretch[nusiz];
            m_GapP0 = m_GapSize[nusiz];
            m_RepeatP0 = m_Repeats[nusiz];

            m_P0MidDelay = m_StretchP0 * 3;

            if (m_M0Lock)
            {
                m_M0Pos = m_P0MidDelay + m_P0Pos;
            }
        }

        private void NUSIZ1(byte data)
        {
            m_M1Size = 1 << ((data >> 4) & 0x3);

            int nusiz = data & 0x7;
            m_StretchP1 = m_Stretch[nusiz];
            m_GapP1 = m_GapSize[nusiz];
            m_RepeatP1 = m_Repeats[nusiz];

            m_P1MidDelay = m_StretchP0 * 3;

            if (m_M1Lock)
            {
                m_M1Pos = m_P1MidDelay + m_P1Pos;
            }
        }

        // MISSLE MOVEMENT LOCK
        public void RESMP0(byte data)
        {
            m_M0Lock = (data & 0x2) != 0;

            if (m_M0Lock)
            {
                m_M0Pos = m_P0MidDelay + m_P0Pos;
            }
        }
        public void RESMP1(byte data)
        {
            m_M1Lock = (data & 0x2) != 0;

            if (m_M1Lock)
            {
                m_M1Pos = m_P1MidDelay + m_P1Pos;
            }
        }

        // OBJECT POSITION REGISTERS
        private void RESP0(byte data)
        {
            m_P0Pos = (m_hPos < 0 ? 0 : m_hPos) + 5;

            if (m_M0Lock)
                m_M0Pos = m_P0MidDelay + m_P0Pos;
        }

        private void RESP1(byte data)
        {
            m_P1Pos = (m_hPos < 0 ? 0 : m_hPos) + 5;

            if (m_M1Lock)
                m_M1Pos = m_P1MidDelay + m_P1Pos;
        }

        private void RESM0(byte data)
        {
            if (m_M0Lock)
                return;

            m_M0Pos = (m_hPos < 0 ? 0 : m_hPos) + 4;
        }

        private void RESM1(byte data)
        {
            if (m_M1Lock)
                return;

            m_M1Pos = (m_hPos < 0 ? 0 : m_hPos) + 4;
        }

        private void RESBL(byte data)
        {
            m_BLPos = (m_hPos < 0 ? 0 : m_hPos) + 4;
        }
        #endregion
        #region Palette Registers

        private void UpdatePalette()
        {
            uint PF_Left, PF_Right;

            ForceScanDraw();

            if ((m_DrawMode & (byte)playfieldModes.PF_SCORE) != 0)
            {
                PF_Left     = m_Palette[m_ChromaP0];
                PF_Right    = m_Palette[m_ChromaP1];
            }
            else
            {
                PF_Left     = m_Palette[m_ChromaPF];
                PF_Right    = m_Palette[m_ChromaPF];
            }

            if ((m_DrawMode & (byte)playfieldModes.PF_PRI) != 0)
            {
                for (int i = 1; i < 0x40; i++)
                {
                    if ((i & ((int)screenMasks.PF_BIT | (int)screenMasks.BL_BIT)) != 0)
                    {
                        m_ColorTranslate[i] = PF_Left;
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] = PF_Right;
                    }
                    else if ((i & ((int)screenMasks.P0_BIT | (int)screenMasks.M0_BIT)) != 0)
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] =
                        m_ColorTranslate[i] = m_Palette[m_ChromaP0];
                    else if ((i & ((int)screenMasks.P1_BIT | (int)screenMasks.M1_BIT)) != 0)
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] =
                        m_ColorTranslate[i] = m_Palette[m_ChromaP1];
                }
            }
            else
            {
                for (int i = 1; i < 0x40; i++)
                {
                    if ((i & ((int)screenMasks.P0_BIT | (int)screenMasks.M0_BIT)) != 0)
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] =
                        m_ColorTranslate[i] = m_Palette[m_ChromaP0];
                    else if ((i & ((int)screenMasks.P1_BIT | (int)screenMasks.M1_BIT)) != 0)
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] =
                        m_ColorTranslate[i] = m_Palette[m_ChromaP1];
                    else if ((i & ((int)screenMasks.PF_BIT | (int)screenMasks.BL_BIT)) != 0)
                    {
                        m_ColorTranslate[i] = PF_Left;
                        m_ColorTranslate[i | (int)screenMasks.SIDE_BIT] = PF_Right;
                    }
                }
            }
        }

        private void COLUP0(byte data)
        {
            m_ChromaP0 = (byte)(data >> 1);
            UpdatePalette();
        }

        private void COLUP1(byte data)
        {
            m_ChromaP1 = (byte)(data >> 1);
            UpdatePalette();
        }

        private void COLUPF(byte data)
        {
            m_ChromaPF = (byte)(data >> 1);
            UpdatePalette();
        }

        private void COLUBK(byte data)
        {
            ForceScanDraw();
            m_ChromaBK = (byte)(data >> 1);
            m_ColorTranslate[0x00] = m_Palette[m_ChromaBK];
            m_ColorTranslate[0x40] = m_Palette[m_ChromaBK];
        }
        #endregion
        #region Graphic Registers

        private void PF0(byte data)
        {
            m_PF0Grp = data;
        }

        private void PF1(byte data)
        {
            m_PF1Grp = data;
        }

        private void PF2(byte data)
        {
            m_PF2Grp = data;
        }

        private void GRP0(byte data)
        {
            m_P0Grp = data;
            m_P1GrpDel = m_P1Grp;
        }

        private void GRP1(byte data)
        {
            m_P1Grp = data;
            m_P0GrpDel = m_P0Grp;
            m_EnaBLDel = m_EnaBL;
        }

        private void ENAM0(byte data)
        {
            m_EnaM0 = (data & 0x2) != 0;
        }

        private void ENAM1(byte data)
        {
            m_EnaM1 = (data & 0x2) != 0;
        }

        private void ENABL(byte data)
        {
            m_EnaBL = (data & 0x2) != 0;
        }
        #endregion
        #region Delay Registers
        private void VDELP0(byte data)
        {
            m_VDelP0 = (data & 0x1) != 0;
        }

        private void VDELP1(byte data)
        {
            m_VDelP1 = (data & 0x1) != 0;
        }

        private void VDELBL(byte data)
        {
            m_VDelBL = (data & 0x1) != 0;
        }

        #endregion
        #region Movement Registers

        private void HMP0(byte data)
        {
            m_MoveP0 = (sbyte)data >> 4;
        }

        private void HMP1(byte data)
        {
            m_MoveP1 = (sbyte)data >> 4;
        }

        private void HMM0(byte data)
        {
            m_MoveM0 = (sbyte)data >> 4;
        }

        private void HMM1(byte data)
        {
            m_MoveM1 = (sbyte)data >> 4;
        }

        private void HMBL(byte data)
        {
            m_MoveBL = (sbyte)data >> 4;
        }

        private void HMOVE(byte data)
        {
            // TODO: FIX SO IT FREAKS OUT IN NON-HBLANK
            // TODO: HANDLE COSMIC ARK BUG
            // Currently assuming that it occurs in hblank
            m_BLPos = (m_BLPos - m_MoveBL + 160) % 160;
            m_P0Pos = (m_P0Pos - m_MoveP0 + 160) % 160;
            m_P1Pos = (m_P1Pos - m_MoveP1 + 160) % 160;

            if (m_M0Lock)
                m_M0Pos = m_P0MidDelay + m_P0Pos;
            else
                m_M0Pos = (m_M0Pos - m_MoveM0 + 160) % 160;

            if (m_M1Lock)
                m_M1Pos = m_P1MidDelay + m_P1Pos;
            else
                m_M1Pos = (m_M1Pos - m_MoveM1 + 160) % 160;


            // Render hmove artifact
            for (int i = 0; i < 8; i++)
            {
                m_ScanLine[i] |= 0x80;
            }
        }

        private void HMCLR(byte data)
        {
            m_MoveP0 = 0;
            m_MoveP1 = 0;
            m_MoveM0 = 0;
            m_MoveM1 = 0;
            m_MoveBL = 0;
        }

        #endregion
        #region Collision Registers
        // COLLISION REGISTERS

        private void CXCLR(byte data)
        {
            ForceScanDraw();
            for (int i = 0; i < m_Collisions.Length; i++)
                m_Collisions[i] = 0;
        }

        private byte CXM0P(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.P1_M0_INDEX] << 1) | m_Collisions[(int)collisionIndicies.M0_P0_INDEX]);
        }

        private byte CXM1P(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.M1_P1_INDEX] << 1) | m_Collisions[(int)collisionIndicies.M1_P0_INDEX]);
        }

        private byte CXP0FB(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.P0_PF_INDEX] << 1) | m_Collisions[(int)collisionIndicies.P0_BL_INDEX]);
        }

        private byte CXP1FB(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.P1_PF_INDEX] << 1) | m_Collisions[(int)collisionIndicies.P1_BL_INDEX]);
        }

        private byte CXM0FB(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.M0_PF_INDEX] << 1) | m_Collisions[(int)collisionIndicies.M0_BL_INDEX]);
        }

        private byte CXM1FB(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.M1_PF_INDEX] << 1) | m_Collisions[(int)collisionIndicies.M1_BL_INDEX]);
        }

        private byte CXBLPF(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x7F) | (m_Collisions[(int)collisionIndicies.BL_PF_INDEX] << 1));
        }

        private byte CXPPMM(byte trash)
        {
            ForceScanDraw();
            return (byte)((trash & 0x3F) | (m_Collisions[(int)collisionIndicies.P1_P0_INDEX] << 1) | m_Collisions[(int)collisionIndicies.M1_M0_INDEX]);
        }
        #endregion
        #region Input Registers

        private byte INPT0(byte trash)
        {
            if (m_CtrlA != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlA.pot1());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }

        private byte INPT1(byte trash)
        {
            if (m_CtrlA != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlA.pot2());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }

        private byte INPT2(byte trash)
        {
            if (m_CtrlB != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlB.pot1());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }

        private byte INPT3(byte trash)
        {
            if (m_CtrlB != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlB.pot2());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }

        private byte INPT4(byte trash)
        {
            if (m_CtrlA != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlA.fire());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }

        private byte INPT5(byte trash)
        {
            if (m_CtrlB != null)
            {
                return (byte)((trash & 0x7F) | m_CtrlB.fire());
            }
            else
            {
                return (byte)((trash & 0x7F) | 0x80);
            }
        }
        #endregion
        #region Data Bus Commands

        public byte peek(ushort address, byte data)
        {
            ClockCatchUp();

            switch (address & 0x0F)
            {
                case 0x0:
                    return CXM0P(data);
                case 0x1:
                    return CXM1P(data);
                case 0x2:
                    return CXP0FB(data);
                case 0x3:
                    return CXP1FB(data);
                case 0x4:
                    return CXM0FB(data);
                case 0x5:
                    return CXM1FB(data);
                case 0x6:
                    return CXBLPF(data);
                case 0x7:
                    return CXPPMM(data);
                case 0x8:
                    return INPT0(data);
                case 0x9:
                    return INPT1(data);
                case 0xA:
                    return INPT2(data);
                case 0xB:
                    return INPT3(data);
                case 0xC:
                    return INPT4(data);
                case 0xD:
                    return INPT5(data);
                default:
                    return data;
            }
        }

        public void poke(ushort address, byte data)
        {
            ClockCatchUp();

            switch (address & 0x3F)
            {
                case 0x00:
                    VSYNC(data);
                    break;
                case 0x01:
                    VBLANK(data);
                    break;
                case 0x02:
                    WSYNC(data);
                    break;
                case 0x03:
                    RSYNC(data);
                    break;
                case 0x04:
                    NUSIZ0(data);
                    break;
                case 0x05:
                    NUSIZ1(data);
                    break;
                case 0x06:
                    COLUP0(data);
                    break;
                case 0x07:
                    COLUP1(data);
                    break;
                case 0x08:
                    COLUPF(data);
                    break;
                case 0x09:
                    COLUBK(data);
                    break;
                case 0x0A:
                    CTRLPF(data);
                    break;
                case 0x0B:
                    REFP0(data);
                    break;
                case 0x0C:
                    REFP1(data);
                    break;
                case 0x0D:
                    PF0(data);
                    break;
                case 0x0E:
                    PF1(data);
                    break;
                case 0x0F:
                    PF2(data);
                    break;
                case 0x10:
                    RESP0(data);
                    break;
                case 0x11:
                    RESP1(data);
                    break;
                case 0x12:
                    RESM0(data);
                    break;
                case 0x13:
                    RESM1(data);
                    break;
                case 0x14:
                    RESBL(data);
                    break;
                case 0x15:
                    m_SndChA.AUDC(data);
                    break;
                case 0x16:
                    m_SndChB.AUDC(data);
                    break;
                case 0x17:
                    m_SndChA.AUDF(data);
                    break;
                case 0x18:
                    m_SndChB.AUDF(data);
                    break;
                case 0x19:
                    m_SndChA.AUDV(data);
                    break;
                case 0x1A:
                    m_SndChB.AUDV(data);
                    break;
                case 0x1B:
                    GRP0(data);
                    break;
                case 0x1C:
                    GRP1(data);
                    break;
                case 0x1D:
                    ENAM0(data);
                    break;
                case 0x1E:
                    ENAM1(data);
                    break;
                case 0x1F:
                    ENABL(data);
                    break;
                case 0x20:
                    HMP0(data);
                    break;
                case 0x21:
                    HMP1(data);
                    break;
                case 0x22:
                    HMM0(data);
                    break;
                case 0x23:
                    HMM1(data);
                    break;
                case 0x24:
                    HMBL(data);
                    break;
                case 0x25:
                    VDELP0(data);
                    break;
                case 0x26:
                    VDELP1(data);
                    break;
                case 0x27:
                    VDELBL(data);
                    break;
                case 0x28:
                    RESMP0(data);
                    break;
                case 0x29:
                    RESMP1(data);
                    break;
                case 0x2A:
                    HMOVE(data);
                    break;
                case 0x2B:
                    HMCLR(data);
                    break;
                case 0x2C:
                    CXCLR(data);
                    break;
            }
        }
        #endregion
    }
}
