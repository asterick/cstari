using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for mnetwork.
	/// </summary>
	public class MNetwork : Mapper
	{
		private byte[] m_Rom;
		private byte[] m_Ram;
		private ushort m_RomBank;
		private ushort m_RamBank;

		public MNetwork( byte[] rom )
		{
			m_Rom = rom;
			m_Ram = new byte[0x800];

			m_RomBank = 0;
			m_RamBank = 0;
		}
		
        public override byte access(ushort address, byte data)
		{
            if (address < 0x1000)
                return data;

            else if (address >= 0x1FF8 && address < 0x1FFC)
			{
				m_RamBank = (ushort)((address & 3) << 8);
			}
			else if( address >= 0x1FE0 && address < 0x1FE8 )
			{
				m_RomBank = (ushort)((address & 7) << 11);
			}
			else if( address >= 0x1000 && address < 0x1800 )
			{
				if( m_RomBank == 0x3800 )
				{
					if( address < 0x1400 )
					{
						return m_Ram[ (address&0x3FF) | 0x400 ] = data;
					}
					else
					{
						return m_Ram[ (address&0x3FF) | 0x400 ];
					}
				}
				else
				{
					return m_Rom[ m_RomBank | (address & 0x7FF) ];
				}
			}
            else if( address >= 0x1800 && address < 0x1900 )
			{
				return m_Ram[ m_RamBank | (address & 0xFF) ] = data;
			}
			else if( address >= 0x1900 && address < 0x1A00 )
			{
				return m_Ram[ m_RamBank | (address & 0xFF) ];
			}

			return m_Rom[ address | 0x3800 ];
		}

        public override int getBlockCount()
		{
			return 2;
		}

        public override string getBlockName(int block)
		{
			switch( block )
			{
				case 0:
					return "Program ROM";
				case 1:
					return "M-Network 2k RAM";
				default:
					return null;
			}
		}

        public override int getBlockLength(int block)
		{
			switch( block )
			{
				case 0:
					return 4096;
				case 1:
					return 2048;
				default:
					return 0;
			}
		}

        public override byte getBlockData(int block, ushort address)
		{
			switch( block )
			{
				case 0:
					if( address < 0x800 )
					{
						return m_Rom[ (address & 0x7FF) | m_RomBank ];
					}
					return m_Rom[ (address & 0x7FF) | 0x3800 ];
				case 1:
					return m_Ram[ address & 0x7FF ];
				default:
					return 0;
			}
		}

        public override byte read(ushort address)
		{
			if( address < 0x800 )
			{
				return m_Rom[ (address & 0x7FF) | m_RomBank ];
			}
			return m_Rom[ (address & 0x7FF) | 0x3800 ];
		}
	}
}
