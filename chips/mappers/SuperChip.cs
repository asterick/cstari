using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for superchip.
    /// </summary>
    public class SuperChip : Mapper
    {
        private byte[] m_Rom;
        private byte[] m_Ram;
        private ushort m_Bank;
        private int m_Banks;
        private int m_Base;

        public SuperChip(byte[] rom)
        {
            m_Rom = rom;
            m_Ram = new byte[128];
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
            else if (address >= 0x1000 && address < 0x1080)
            {
                return m_Ram[address & 0x7F] = data;
            }
            else if (address >= 0x1080 && address < 0x1100)
            {
                return m_Ram[address & 0x7F];
            }
            else if (address >= m_Base && address < m_Base + m_Banks)
            {
                m_Bank = (ushort)((address - m_Base) << 12);
            }

            return m_Rom[(address & 0xFFF) | m_Bank];
        }

        public override int getBlockCount()
        {
            return 2;
        }

        public override string getBlockName(int block)
        {
            switch (block)
            {
                case 0:
                    return "Program ROM";
                case 1:
                    return "Super-chip RAM";
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
                case 1:
                    return 0x80;
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
                case 1:
                    return m_Ram[address & 0x7F];
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
