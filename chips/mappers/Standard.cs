using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for standard.
    /// </summary>
    public class Standard : Mapper
    {
        private byte[] m_Rom;
        private ushort m_Bank;
        private int m_Banks;
        private int m_Base;

        public Standard(byte[] rom)
        {
            m_Rom = rom;
            m_Bank = 0;

            m_Banks = m_Rom.Length / 0x1000;
            switch (m_Banks)
            {
                case 2:
                    m_Base = 0x1FF8;
                    break;
                case 4:
                    m_Base = 0x1FF6;
                    break;
                case 8:
                    m_Base = 0x1FF4;
                    break;
                case 16:
                    m_Base = 0x1FF0;
                    break;
            }
        }

        public override byte access(ushort address, byte data)
        {
            if (address < 0x1000)
                return data;
            else if (address >= m_Base && address < m_Base + m_Banks)
            {
                m_Bank = (ushort)((address - m_Base) << 12);
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
