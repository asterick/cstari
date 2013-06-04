using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for fourk.
	/// </summary>
	public class FourK : Mapper
	{
		private byte[] m_Rom;

		public FourK( byte[] rom )
		{
			m_Rom = rom;
		}

        public override byte access(ushort address, byte data)
		{
            if (address < 0x1000)
                return data;
            return m_Rom[address & 0xFFF];
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
					return m_Rom[ address & 0xFFF ];
				default:
					return 0;
			}
		}

        public override byte read(ushort address)
		{
			return m_Rom[ address & 0xFFF ];
		}	
	}
}
