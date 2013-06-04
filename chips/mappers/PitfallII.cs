using System;
using cstari.chips;

namespace cstari.chips.mappers
{
    /// <summary>
    /// Summary description for standard.
    /// </summary>
    public class PitfallII : Mapper
    {
        private byte[] m_Rom;
        private ushort m_Bank;

        ushort[] m_Counters;
        byte[] m_Flags;
        byte[] m_Tops;
        byte[] m_Bottoms;

        int m_RandomSreg;

        public PitfallII(byte[] rom)
        {
            m_Counters = new ushort[8];
            m_Flags = new byte[8];
            m_Tops = new byte[8];
            m_Bottoms = new byte[8];
            m_RandomSreg = 1;

            m_Rom = rom;
            m_Bank = 0;
        }
        
        // --- READ ADDRESSES --------------------------------------

        public byte ReadRandom(int counter, byte data)
        {
            int newBit = (m_RandomSreg & 1) ^ 
                ((m_RandomSreg & 4) >> 2) ^ 
                ((m_RandomSreg & 8) >> 3) ^ 
                ((m_RandomSreg & 16) >> 4);

            m_RandomSreg = (m_RandomSreg >> 1) | (newBit << 7);

            return (byte) m_RandomSreg;
        }

        public byte ReadSound(int counter, byte data)
        {
            // TODO: Read Sound

            return 0;
        }

        public byte ReadDataCounted(int counter, byte data)
        {
            byte o = m_Rom[ m_Counters[ counter ] ];

            ushort count = (ushort)((m_Counters[counter] - 1) & 0x7FF);
            m_Counters[counter] = count;

            // Test the byte for top / bottom
            if (o == m_Tops[counter])
                m_Flags[counter] = 0xFF;
            else if (o == m_Bottoms[counter])
                m_Flags[counter] = 0;

            return m_Rom[0x27FE - count];
        }

        public byte ReadDataCountedMasked(int counter, byte data)
        {
            return (byte)(ReadDataCounted(counter, data) & m_Flags[counter]);
        }

        public byte ReadFlags(int counter, byte data)
        {
            return m_Flags[counter];
        }

        // --- WRITE ADDRESSES -------------------------------------

        public byte WriteTop(int counter, byte data)
        {
            m_Tops[counter] = data;
            return data;
        }

        public byte WriteBottom(int counter, byte data)
        {
            m_Bottoms[counter] = data;
            return data;
        }

        public byte WriteCounterLow(int counter, byte data)
        {
            m_Counters[counter] = (ushort)((m_Counters[counter] & 0x700) | data);

            // TODO: This screws with the audio
            return data;
        }

        public byte WriteCounterHigh(int counter, byte data)
        {
            m_Counters[counter] = (ushort)((m_Counters[counter] & 0xFF) | ((data & 0x7) << 8));
            m_Flags[counter] = 0;
            return data;
        }

        public byte WriteResetRandom(int counter, byte data)
        {
            m_RandomSreg = 1;

            return 0;
        }


        // --- MAPPER INTERFACE ------------------------------------

        public override byte access(ushort address, byte data)
        {
            if (address < 0x1000)
                return data;
            if (address >= 0x1000 && address <= 0x1003)
                return ReadRandom(address & 3, data);
            if (address >= 0x1004 && address <= 0x1007)
                return ReadSound(address & 3, data);
            if (address >= 0x1008 && address <= 0x100F)
                return ReadDataCounted(address & 7, data);
            if (address >= 0x1010 && address <= 0x1017)
                return ReadDataCountedMasked(address & 7, data);
            if (address >= 0x1038 && address <= 0x103F)
                return ReadFlags(address & 7, data);

            if (address >= 0x1040 && address <= 0x1047)
                return WriteTop(address & 7, data);
            if (address >= 0x1048 && address <= 0x104F)
                return WriteBottom(address & 7, data);
            if (address >= 0x1050 && address <= 0x1057)
                return WriteCounterLow(address & 7, data);
            if (address >= 0x1058 && address <= 0x105f)
                return WriteCounterHigh(address & 7, data);
            if (address >= 0x1070 && address <= 0x1077)
                return WriteResetRandom(address & 7, data);

            // No IO
            if (address >= 0x1000 && address <= 0x107F)
                return 0;

            if (address >= 0x1FF8 && address < 0x1FFA)
            {
                m_Bank = (ushort)((address - 0x1FF8) << 12);
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
