using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for activision.
	/// </summary>
	public class Activision : Mapper
	{
		private byte[] m_Rom;
		private ushort m_Bank;
        private int m_AccessCount;

		public Activision( byte[] rom )
		{
			m_Rom	= rom;
			m_Bank	= 0x0000;
		}

        public override byte access(ushort address, byte data)
		{
            byte dout = m_Rom[ (address & 0xFFF) | m_Bank ];

            if (address >= 0x100 && address <= 0x1FF)
            {
                m_AccessCount++;

                if ((address & 1) == 1 && m_AccessCount == 2)
                {
                    m_Bank = ((data & 0x20) == 0) ? (ushort)0x1000 : (ushort)0x0000;
                    m_AccessCount = 0;
                }
            }
            else if (m_AccessCount == 2)
            {
                m_Bank = ((dout & 0x20) == 0) ? (ushort)0x1000 : (ushort)0x0000;
                m_AccessCount = 0;
            }
            else
            {
                m_AccessCount = 0;
            }

            if (address < 0x1000)
                return data;

            return dout;
        }

        public override int getBlockCount()
		{
			return 1;
		}

        public override string getBlockName(int block)
		{
			switch( block )
			{
				case 0:
					return "Program ROM";
				default:
					return null;
			}
		}

        public override int getBlockLength(int block)
		{
			switch( block )
			{
				case 0:
					return 0x1000;
				default:
					return -1;
			}
		}

        public override byte getBlockData(int block, ushort address)
		{
			switch( block )
			{
				case 0:
					return m_Rom[ (address & 0xFFF) | m_Bank ];
				default:
					return 0;
			}
		}

        public override byte read(ushort address)
		{
			return m_Rom[ (address & 0xFFF) | m_Bank ];
		}	
	}
}
