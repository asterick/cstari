using System;
using System.Collections.Generic;
using cstari.input;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.chips
{
    /// <summary>
    /// Summary description for atari.
    /// </summary>
    public class Atari : CPU
    {
        private int freqOverSixty;

        private Controller m_CtrlA;
        private Controller m_CtrlB;
        private Mapper m_Cart;
        public RIOT m_RIOT;
        public TIA m_TIA;

        public delegate void ExecuteStep();
        public delegate void SmallStep();

        public ExecuteStep execute;
        public SmallStep step;

        private ushort m_BreakAddress;

        private byte[] m_Ram;

        private int m_Clocks;
        private byte m_BusTrash;

        private static Dictionary<AddressMode, int> instSize;
        private static Dictionary<AddressMode, string> instFormat;

        static Atari()
        {
            instSize = new Dictionary<AddressMode, int>();
            instSize[AddressMode.Accumulator] = 1;
            instSize[AddressMode.Implied] = 1;
            instSize[AddressMode.Relative] = 2;
            instSize[AddressMode.Immediate] = 2;
            instSize[AddressMode.ZeroPage] = 2;
            instSize[AddressMode.ZeroPageIndexedX] = 2;
            instSize[AddressMode.ZeroPageIndexedY] = 2;
            instSize[AddressMode.Absolute] = 3;
            instSize[AddressMode.IndexedX] = 3;
            instSize[AddressMode.IndexedY] = 3;
            instSize[AddressMode.Indirect] = 3;
            instSize[AddressMode.PreIndexedX] = 2;
            instSize[AddressMode.PostIndexedY] = 2;

            instFormat = new Dictionary<AddressMode, string>();
            instFormat[AddressMode.Accumulator] = " A";
            instFormat[AddressMode.Implied] = " ";
            instFormat[AddressMode.Relative] = " ${0,4:X}";
            instFormat[AddressMode.Immediate] = " #${2,2:X}";
            instFormat[AddressMode.ZeroPage] = " ${2,2:X}";
            instFormat[AddressMode.ZeroPageIndexedX] = " ${2,2:X}, X";
            instFormat[AddressMode.ZeroPageIndexedY] = " ${2,2:X}, Y";
            instFormat[AddressMode.Absolute] = " ${1,4:X}";
            instFormat[AddressMode.IndexedX] = " ${1,4:X}, X";
            instFormat[AddressMode.IndexedY] = " ${1,4:X}, Y";
            instFormat[AddressMode.Indirect] = " (${1,4:X})";
            instFormat[AddressMode.PreIndexedX] = " (${2,2:X}, X)";
            instFormat[AddressMode.PostIndexedY] = " (${2,2:X}), Y";
        }

        public Atari(Mapper cart)
        {
            m_Ram = new byte[128];

            m_CtrlA = new Dummy();
            m_CtrlB = new Dummy();
            m_TIA = new TIA(m_CtrlA, m_CtrlB);
            m_RIOT = new RIOT(m_CtrlA, m_CtrlB);

            m_Cart = cart;

            setTrace(false);
            reset();
            run();

            m_Clocks = 0;
            m_BusTrash = 0;
        }

        public Surface Display
        {
            get
            {
                return m_TIA.Display;
            }
        }

        public void plug(Controller CtrlA, Controller CtrlB)
        {
            m_CtrlA = CtrlA;
            m_CtrlB = CtrlB;

            m_TIA.plug(CtrlA, CtrlB);
            m_RIOT.plug(CtrlA, CtrlB);
        }

        public override void reset()
        {
            if (m_Cart != null)
            {
                base.reset();
            }
        }

        public void stop()
        {
            execute = new ExecuteStep(runStopped);
        }

        public void run()
        {
            execute = new ExecuteStep(runFrame);
        }

        public void setTrace(bool trace)
        {
            if (trace)
            {
                step = new SmallStep(stepWithDump);
            }
            else
            {
                step = new SmallStep(stepOperation);
            }
        }

        public void SetSignal(cstari.utility.SignalType s)
        {
            if (s == cstari.utility.SignalType.VIDEO_NTSC)
            {
                freqOverSixty = 59736;
            }
            else
            {
                freqOverSixty = 59280;
            }

            m_TIA.SetSignal(s);
        }

        private void stepWithDump()
        {
            m_TIA.ClockCatchUp();
            DumpInstruction();
            stepOperation();
        }

        public void runTo(ushort address)
        {
            m_BreakAddress = address;
            execute = new ExecuteStep(runRunTo);
        }

        private void runStopped()
        {
        }

        private void runRunTo()
        {
            if (m_Cart == null)
            {
                return;
            }

            while (m_Clocks < freqOverSixty)
            {
                if (m_PC != m_BreakAddress)
                {
                    execute = new ExecuteStep(runStopped);
                }
                step();
            }
            if (m_Clocks >= freqOverSixty)
            {
                m_Clocks -= freqOverSixty;
            }
        }

        private void runFrame()
        {
            if (m_Cart == null)
            {
                return;
            }

            while (m_Clocks < freqOverSixty)
                step();
            if (m_Clocks >= freqOverSixty)
                m_Clocks -= freqOverSixty;

            m_TIA.ClockCatchUp();
        }

        public override void clock(int clocks)
        {
            // The CPU runs at 1/3rd the speed of the rest of the system
            // Extra cycles of data from the TIA (WSYNC)
            clocks = clocks * 3 + m_TIA.SyncCycles;

            m_Clocks += clocks;
            m_TIA.clock(clocks);
            m_RIOT.clock(clocks);
            m_Cart.clock(clocks);

            if (m_CtrlA != null)
                m_CtrlA.clock(clocks);
            if (m_CtrlB != null)
                m_CtrlB.clock(clocks);
        }

        public override void poke(ushort address, byte data)
        {
            m_Cart.access((ushort)(address & 0x1FFF), data);

            if ((address & 0x1000) == 0)
            {
                ushort index = (ushort)(address & 0x7F);
                if ((address & 0x80) != 0)
                {
                    if ((address & 0x200) != 0)
                        m_RIOT.poke(index, data);
                    else
                        m_Ram[index] = data;
                }
                else
                {
                    m_TIA.poke(index, data);
                }
            }
        }

        public override void poke(byte address, byte data)
        {
            m_Cart.access(address, data);

            ushort index = (ushort)(address & 0x7F);

            if ((address & 0x80) != 0)
            {
                m_Ram[index] = data;
            }
            else
            {
                m_TIA.poke(index, data);
            }
        }

        public override byte peek(byte address)
        {
            byte access;
            access = m_Cart.access(address, m_BusTrash);

            ushort index = (ushort)(address & 0x7F);

            if ((address & 0x80) != 0)
            {
                return m_BusTrash = m_Ram[index];
            }
            else
            {
                return m_BusTrash = m_TIA.peek(index, m_BusTrash);
            }
        }

        public override byte peek(ushort address)
        {
            address = (ushort)(address & 0x1FFF);

            if ((address & 0x1000) == 0)
            {
                ushort index = (ushort)(address & 0x7F);
                if ((address & 0x80) != 0)
                    if ((address & 0x200) != 0)
                    {
                        m_BusTrash = m_RIOT.peek(index, m_BusTrash);
                    }
                    else
                    {
                        m_BusTrash = m_Ram[index];
                    }
                else
                {
                    m_BusTrash = m_TIA.peek(index, m_BusTrash);
                }
            }

            return m_BusTrash = m_Cart.access(address, m_BusTrash);
        }

        public byte read(ushort address)
        {
            if ((address & 0x1000) != 0)
            {
                return m_Cart.read((ushort)(address & 0xFFF));
            }
            else if ((address & 0x200) == 0 && (address & 0x80) != 0)
            {
                return m_Ram[address & 0x7F];
            }
            else
            {
                return 0;
            }
        }

        private void DumpInstruction()
        {
            char[,] flagsMap = new char[8, 2] { { '-', 'C' }, { '-', 'Z' }, { '-', 'I' }, { '-', 'D' }, { '-', 'B' }, { '-', '1' }, { '-', 'V' }, { '-', 'S' } };

            string flags = "";
            byte p = BuildStatus(false);

            for (int i = 0; i < 8; i++)
            {
                flags += flagsMap[i, (p >> i) & 1];
            }

            Console.Write("{0,4:X}; A: {1,2:X} X: {2,2:X} Y: {3,2:X} S: {4,2:X} P: {5,8} ", new object[] { m_PC, m_A, m_X, m_Y, m_S, flags });

            Console.Write("({0,3} {1,3}:{2,3} {3,3} {4,3} {5,3} {6,3}) {7} ", new object[] { 
                m_TIA.m_vPos % 999, 
                m_TIA.m_hPos, 
                m_TIA.m_BLPos, 
                m_TIA.m_M0Pos, 
                m_TIA.m_P0Pos, 
                m_TIA.m_M1Pos, 
                m_TIA.m_P1Pos,
                m_TIA.m_Syncing});

            Console.WriteLine(instructionAsm(m_PC));
        }

        public string instructionAsm(ushort address)
        {
            byte data = read(address);
            byte imm1 = read((ushort)(address + 1));
            byte imm2 = read((ushort)(address + 2));

            if (!instMode.ContainsKey(data))
                return "---";

            InstTable mode = instMode[data];

            return String.Format(mode.name + instFormat[mode.mode], new object[] { 
                (ushort)(address+2+(sbyte)imm1),
                imm1 | (imm2<<8),
                imm1 });
        }

        public byte[] instructionData(ushort address)
        {
            byte data = read(address);

            if (!instMode.ContainsKey(data))
                return new byte[] { data };

            InstTable mode = instMode[data];

            if (instSize[mode.mode] == 1)
                return new byte[] { data };

            byte imm1 = read((ushort)(address + 1));
            if (instSize[mode.mode] == 2)
                return new byte[] { data, imm1 };

            byte imm2 = read((ushort)(address + 2));
            return new byte[] { data, imm1, imm2 };
        }
    }
}
