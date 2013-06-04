using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for parkerbrothers.
	/// </summary>
	public class ParkerBrothers : Mapper
	{
		private byte[] m_Rom;
		private ushort[] m_Page;

		public ParkerBrothers( byte[] rom )
		{
			m_Rom = rom;
			m_Page = new ushort[4] {0,0,0,0x1C00};
		}

        // 1110000000000
        //    1234567890

        public override byte access(ushort address, byte data)
		{
            if (address < 0x1000)
                return data;
            else if (address >= 0x1FE0 && address <= 0x1FF7)
			{
				m_Page[(address >> 3)&3] = (ushort)((address & 0x7) << 10);
			}

			return m_Rom[ (address & 0x3FF) | m_Page[(address>>10)&3] ];
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
					return m_Rom[ (address & 0x3FF) | m_Page[(address>>10)&3] ];
				default:
					return 0;
			}
		}

        public override byte read(ushort address)
		{
			return m_Rom[ (address & 0x3FF) | m_Page[(address>>10)&3] ];
		}
	}
}
