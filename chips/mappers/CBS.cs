using System;
using cstari.chips;

namespace cstari.chips.mappers
{
	/// <summary>
	/// Summary description for cbs.
	/// </summary>
	public class CBS : Mapper
	{
		private byte[] m_Rom;
		private byte[] m_Ram;
		private ushort m_Bank;

		public CBS( byte[] rom )
		{
			m_Rom = rom;
			m_Ram = new byte[0x100];
			m_Bank = 0;
		}

        public override byte access(ushort address, byte data)
		{
            if (address < 0x1000)
                return data;
            else if (address >= 0x1000 && address < 0x1100)
			{
                return m_Ram[address & 0xFF] = data;
            }
			else if( address >= 0x1100 && address < 0x1200 )
			{
                return m_Ram[address & 0xFF];
            }
			else if( address >= 0x1FF8 && address < 0x1FFB )
			{
				m_Bank = (ushort)((address - 0x1FF8) << 12);
			}

			return m_Rom[ (address & 0xFFF) | m_Bank ];
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
					return "CBS 256 byte RAM";
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
				case 1:
					return 0x100;
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
				case 1:
					return m_Ram[ address & 0xFF ];
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
