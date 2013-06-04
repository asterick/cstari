using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for standard.
    /// </summary>
    public class ReversedStandard : Mapper
    {
        private byte[] m_Rom;
        private ushort m_Bank;

        public ReversedStandard(byte[] rom)
        {
            m_Rom = rom;
            m_Bank = 0;
        }

        public override byte access(ushort address, byte data)
        {
            if (address < 0x1000)
                return data;
            else if (address >= 0x1FF8 && address <= 0x1FF9 )
            {
                m_Bank = (address == 0x1FF8) ? (ushort)0x1000 : (ushort)0;
            }

            return m_Rom[(address & 0xFFF) | m_Bank];
        }

        public override int getBlockCount()
        {
            return 1;
        }

        public override string getBlockName(int block)
        {
            switch (block)
            {
                case 0:
                    return "Program ROM";
                default:
                    return null;
            }
        }

        public override int getBlockLength(int block)
        {
            switch (block)
            {
                case 0:
                    return 0x1000;
                default:
                    return -1;
            }
        }

        public override byte getBlockData(int block, ushort address)
        {
            switch (block)
            {
                case 0:
                    return m_Rom[(address & 0xFFF) | m_Bank];
                default:
                    return 0;
            }
        }

        public override byte read(ushort address)
        {
            return m_Rom[(address & 0xFFF) | m_Bank];
        }
    }
}
