using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for megaboy.
	/// </summary>
	public class Megaboy : Mapper
	{
		private byte[] m_Rom;
		private ushort m_Bank;
		private byte m_Page;

		public Megaboy( byte[] rom )
		{
			m_Rom = rom;
			m_Bank = 0;
			m_Page = 0;
		}

        public override byte access(ushort address, byte data)
		{
            if (address < 0x1000)
                return data;
            else if (address == 0x1FF0)
			{
				m_Page = (byte)((m_Page+1) & 0x0F);
				m_Bank = (ushort)(m_Page << 12);
			}
			else if( address == 0x1FEC )
			{
				return m_Page;
			}

			return m_Rom[ m_Bank | (address & 0xFFF) ];
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
