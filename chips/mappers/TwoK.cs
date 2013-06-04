using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for twok.
    /// </summary>
    public class TwoK : Mapper
    {
        private byte[] m_Rom;

        public TwoK(byte[] rom)
        {
            m_Rom = rom;
        }

        public override byte access(ushort address, byte data)
        {
            if (address < 0x1000)
                return data;
            return m_Rom[address & 0x7FF];
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
                    return 0x800;
                default:
                    return -1;
            }
        }

        public override byte getBlockData(int block, ushort address)
        {
            switch (block)
            {
                case 0:
                    return m_Rom[address & 0x7FF];
                default:
                    return 0;
            }
        }

        public override byte read(ushort address)
        {
            return m_Rom[address & 0x7FF];
        }
    }
}
