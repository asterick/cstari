using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for tigervision.
    /// </summary>
    public class TigerVisionExtended : Mapper
    {
        private byte[] m_Rom;
        
        private int m_Bank;        
        private int m_LastBank;

        public TigerVisionExtended(byte[] rom)
        {
            m_Rom = rom;
            m_Bank = 0;

            m_LastBank = (rom.Length / 0x800 - 1) << 11;
        }

        public override byte access(ushort address, byte data)
        {            
            if (address < 0x40)
            {
                m_Bank = (ushort)(data << 11);
            }
            
            if (address < 0x1000)
                return data;
            else if (address >= 0x1800)
            {
                return m_Rom[address | m_LastBank];
            }

            return m_Rom[(address & 0x7FF) | m_Bank];
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
                    return m_Rom[(address & 0x7FF) | (address < 0x1800 ? (int)m_Bank : 0x1800)];
                default:
                    return 0;
            }
        }

        public override byte read(ushort address)
        {
            return m_Rom[(address & 0x7FF) | (address < 0x1800 ? (int)m_Bank : 0x1800)];
        }
    }
}
