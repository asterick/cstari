/*******************************************************************************
**
**  PYTARI - Atari 2600 emulation in python
**
**  Copyright (c) 2000-2005 by Bryon Vandiver
**
**  See the file "license" for information on usage and redistribution of
**  this file, and for a DISCLAIMER OF ALL WARRANTIES.
**
********************************************************************************
**
**   6507 CPU core, atari glue 
**
**   The purpose of this script is to "glue" the component classes
**   encapsulating the various devices that line through out the atari
**
**   They are: CART, RIOT (ram mapped as seperate component), TIA
**
**   Joysticks are handled by the RIOT chip.
**
**   Until the CPU class recieves an insert call with a cart class,
**   it will return nothing but zeros (BRK), causing the atari to do
**   nothing but reset
**
**   Generally speaking, the API for addressing is:
**   fetch access(address, throw)    -   Used for CART bus, read/write at
**                                       the same time
**   value fetch(address)            -   reads a value from the RIOT or TIA
**   throw(address,value)            -   writes a value to the RIOT or TIA
**
**
**   MEMORY MAPPING:
**   
**    C B A 9 8 7 6 5 4 3 2 1 0
**   +-+-+-+-+-+-+-+-+-+-+-+-+-+
**   |A|-|-|B|-|C|-|-|-|-|-|-|-| Addressing Lines
**   +-+-+-+-+-+-+-+-+-+-+-+-+-+
**
**   On A                NONE (Use Cart)
**   On A' and C'        TIA
**   On A' and B and C   RIOT
**   On A' and B' and C  RAM
**
**   Carts are pretty stupid about their place in the world, they
**   recieve addressing calls for reads and even when they are not
**   being accessed.   A lot of banking schemes use this to their
**   advantage.
**
*******************************************************************************/

using System;
using System.Collections.Generic;

namespace cstari.chips
{
    abstract public class CPU
    {
        public abstract void clock(int clock);
        public abstract void poke(byte address, byte data);
        public abstract void poke(ushort address, byte data);
        public abstract byte peek(byte address);
        public abstract byte peek(ushort address);

        protected static Dictionary<int, InstTable> instMode;

        public byte m_S;
        public byte m_A;
        public byte m_X;
        public byte m_Y;
        public ushort m_PC;

        public bool m_FlagZero;
        public bool m_FlagCarry;
        public bool m_FlagDecimal;
        public bool m_FlagOverflow;
        public bool m_FlagNegitive;
        public bool m_FlagInterrupt;

        public struct InstTable
        {
            public string name;
            public AddressMode mode;

            public InstTable(string n, AddressMode m)
            {
                name = n;
                mode = m;
            }
        }

        public enum AddressMode
        {
            Accumulator,
            Implied,
            Relative,
            Immediate,
            ZeroPage,
            ZeroPageIndexedX,
            ZeroPageIndexedY,
            Absolute,
            IndexedX,
            IndexedY,
            Indirect,
            PreIndexedX,
            PostIndexedY
        }

        static CPU()
        {
            instMode = new Dictionary<int, InstTable>();

            instMode[0xca] = new InstTable("DEX", AddressMode.Implied);
            instMode[0x88] = new InstTable("DEY", AddressMode.Implied);
            instMode[0xaa] = new InstTable("TAX", AddressMode.Implied);
            instMode[0x10] = new InstTable("BPL", AddressMode.Relative);
            instMode[0x90] = new InstTable("BCC", AddressMode.Relative);
            instMode[0xec] = new InstTable("CPX", AddressMode.Absolute);
            instMode[0xe0] = new InstTable("CPX", AddressMode.Immediate);
            instMode[0xe4] = new InstTable("CPX", AddressMode.ZeroPage);
            instMode[0xcc] = new InstTable("CPY", AddressMode.Absolute);
            instMode[0xc0] = new InstTable("CPY", AddressMode.Immediate);
            instMode[0xc4] = new InstTable("CPY", AddressMode.ZeroPage);
            instMode[0x8] = new InstTable("PHP", AddressMode.Implied);
            instMode[0x81] = new InstTable("STA", AddressMode.PreIndexedX);
            instMode[0x9d] = new InstTable("STA", AddressMode.IndexedX);
            instMode[0x85] = new InstTable("STA", AddressMode.ZeroPage);
            instMode[0x95] = new InstTable("STA", AddressMode.ZeroPageIndexedX);
            instMode[0x8d] = new InstTable("STA", AddressMode.Absolute);
            instMode[0x91] = new InstTable("STA", AddressMode.PostIndexedY);
            instMode[0x99] = new InstTable("STA", AddressMode.IndexedY);
            instMode[0xba] = new InstTable("TSX", AddressMode.Implied);
            instMode[0xce] = new InstTable("DEC", AddressMode.Absolute);
            instMode[0xde] = new InstTable("DEC", AddressMode.IndexedX);
            instMode[0xc6] = new InstTable("DEC", AddressMode.ZeroPage);
            instMode[0xd6] = new InstTable("DEC", AddressMode.ZeroPageIndexedX);
            instMode[0xc1] = new InstTable("CMP", AddressMode.PreIndexedX);
            instMode[0xdd] = new InstTable("CMP", AddressMode.IndexedX);
            instMode[0xc5] = new InstTable("CMP", AddressMode.ZeroPage);
            instMode[0xc9] = new InstTable("CMP", AddressMode.Immediate);
            instMode[0xd5] = new InstTable("CMP", AddressMode.ZeroPageIndexedX);
            instMode[0xcd] = new InstTable("CMP", AddressMode.Absolute);
            instMode[0xd1] = new InstTable("CMP", AddressMode.PostIndexedY);
            instMode[0xd9] = new InstTable("CMP", AddressMode.IndexedY);
            instMode[0xa1] = new InstTable("LDA", AddressMode.PreIndexedX);
            instMode[0xbd] = new InstTable("LDA", AddressMode.IndexedX);
            instMode[0xa5] = new InstTable("LDA", AddressMode.ZeroPage);
            instMode[0xa9] = new InstTable("LDA", AddressMode.Immediate);
            instMode[0xb5] = new InstTable("LDA", AddressMode.ZeroPageIndexedX);
            instMode[0xad] = new InstTable("LDA", AddressMode.Absolute);
            instMode[0xb1] = new InstTable("LDA", AddressMode.PostIndexedY);
            instMode[0xb9] = new InstTable("LDA", AddressMode.IndexedY);
            instMode[0xf0] = new InstTable("BEQ", AddressMode.Relative);
            instMode[0x2a] = new InstTable("ROL", AddressMode.Accumulator);
            instMode[0x2e] = new InstTable("ROL", AddressMode.Absolute);
            instMode[0x3e] = new InstTable("ROL", AddressMode.IndexedX);
            instMode[0x26] = new InstTable("ROL", AddressMode.ZeroPage);
            instMode[0x36] = new InstTable("ROL", AddressMode.ZeroPageIndexedX);
            instMode[0x8c] = new InstTable("STY", AddressMode.Absolute);
            instMode[0x84] = new InstTable("STY", AddressMode.ZeroPage);
            instMode[0x94] = new InstTable("STY", AddressMode.ZeroPageIndexedX);
            instMode[0x6c] = new InstTable("JMP", AddressMode.Indirect);
            instMode[0x4c] = new InstTable("JMP", AddressMode.Absolute);
            instMode[0x30] = new InstTable("BMI", AddressMode.Relative);
            instMode[0x40] = new InstTable("RTI", AddressMode.Implied);
            instMode[0xa8] = new InstTable("TAY", AddressMode.Implied);
            instMode[0x8a] = new InstTable("TXA", AddressMode.Implied);
            instMode[0x60] = new InstTable("RTS", AddressMode.Implied);
            instMode[0xf8] = new InstTable("SED", AddressMode.Implied);
            instMode[0x4a] = new InstTable("LSR", AddressMode.Accumulator);
            instMode[0x4e] = new InstTable("LSR", AddressMode.Absolute);
            instMode[0x5e] = new InstTable("LSR", AddressMode.IndexedX);
            instMode[0x46] = new InstTable("LSR", AddressMode.ZeroPage);
            instMode[0x56] = new InstTable("LSR", AddressMode.ZeroPageIndexedX);
            instMode[0x20] = new InstTable("JSR", AddressMode.Absolute);
            instMode[0x38] = new InstTable("SEC", AddressMode.Implied);
            instMode[0x2c] = new InstTable("BIT", AddressMode.Absolute);
            instMode[0x24] = new InstTable("BIT", AddressMode.ZeroPage);
            instMode[0xb0] = new InstTable("BCS", AddressMode.Relative);
            instMode[0x9a] = new InstTable("TXS", AddressMode.Implied);
            instMode[0x78] = new InstTable("SEI", AddressMode.Implied);
            instMode[0xa] = new InstTable("ASL", AddressMode.Accumulator);
            instMode[0xe] = new InstTable("ASL", AddressMode.Absolute);
            instMode[0x1e] = new InstTable("ASL", AddressMode.IndexedX);
            instMode[0x6] = new InstTable("ASL", AddressMode.ZeroPage);
            instMode[0x16] = new InstTable("ASL", AddressMode.ZeroPageIndexedX);
            instMode[0x70] = new InstTable("BVS", AddressMode.Relative);
            instMode[0x58] = new InstTable("CLI", AddressMode.Implied);
            instMode[0x6a] = new InstTable("ROR", AddressMode.Accumulator);
            instMode[0x6e] = new InstTable("ROR", AddressMode.Absolute);
            instMode[0x7e] = new InstTable("ROR", AddressMode.IndexedX);
            instMode[0x66] = new InstTable("ROR", AddressMode.ZeroPage);
            instMode[0x76] = new InstTable("ROR", AddressMode.ZeroPageIndexedX);
            instMode[0xd8] = new InstTable("CLD", AddressMode.Implied);
            instMode[0x18] = new InstTable("CLC", AddressMode.Implied);
            instMode[0x61] = new InstTable("ADC", AddressMode.PreIndexedX);
            instMode[0x7d] = new InstTable("ADC", AddressMode.IndexedX);
            instMode[0x65] = new InstTable("ADC", AddressMode.ZeroPage);
            instMode[0x69] = new InstTable("ADC", AddressMode.Immediate);
            instMode[0x75] = new InstTable("ADC", AddressMode.ZeroPageIndexedX);
            instMode[0x6d] = new InstTable("ADC", AddressMode.Absolute);
            instMode[0x71] = new InstTable("ADC", AddressMode.PostIndexedY);
            instMode[0x79] = new InstTable("ADC", AddressMode.IndexedY);
            instMode[0xb8] = new InstTable("CLV", AddressMode.Implied);
            instMode[0x8e] = new InstTable("STX", AddressMode.Absolute);
            instMode[0x86] = new InstTable("STX", AddressMode.ZeroPage);
            instMode[0x96] = new InstTable("STX", AddressMode.ZeroPageIndexedY);
            instMode[0xbc] = new InstTable("LDY", AddressMode.IndexedX);
            instMode[0xac] = new InstTable("LDY", AddressMode.Absolute);
            instMode[0xa0] = new InstTable("LDY", AddressMode.Immediate);
            instMode[0xa4] = new InstTable("LDY", AddressMode.ZeroPage);
            instMode[0xb4] = new InstTable("LDY", AddressMode.ZeroPageIndexedX);
            instMode[0xd0] = new InstTable("BNE", AddressMode.Relative);
            instMode[0x21] = new InstTable("AND", AddressMode.PreIndexedX);
            instMode[0x3d] = new InstTable("AND", AddressMode.IndexedX);
            instMode[0x25] = new InstTable("AND", AddressMode.ZeroPage);
            instMode[0x29] = new InstTable("AND", AddressMode.Immediate);
            instMode[0x35] = new InstTable("AND", AddressMode.ZeroPageIndexedX);
            instMode[0x2d] = new InstTable("AND", AddressMode.Absolute);
            instMode[0x31] = new InstTable("AND", AddressMode.PostIndexedY);
            instMode[0x39] = new InstTable("AND", AddressMode.IndexedY);
            instMode[0xbe] = new InstTable("LDX", AddressMode.IndexedY);
            instMode[0xae] = new InstTable("LDX", AddressMode.Absolute);
            instMode[0xa2] = new InstTable("LDX", AddressMode.Immediate);
            instMode[0xa6] = new InstTable("LDX", AddressMode.ZeroPage);
            instMode[0xb6] = new InstTable("LDX", AddressMode.ZeroPageIndexedY);
            instMode[0xe8] = new InstTable("INX", AddressMode.Implied);
            instMode[0xc8] = new InstTable("INY", AddressMode.Implied);
            instMode[0x41] = new InstTable("EOR", AddressMode.PreIndexedX);
            instMode[0x5d] = new InstTable("EOR", AddressMode.IndexedX);
            instMode[0x45] = new InstTable("EOR", AddressMode.ZeroPage);
            instMode[0x49] = new InstTable("EOR", AddressMode.Immediate);
            instMode[0x55] = new InstTable("EOR", AddressMode.ZeroPageIndexedX);
            instMode[0x4d] = new InstTable("EOR", AddressMode.Absolute);
            instMode[0x51] = new InstTable("EOR", AddressMode.PostIndexedY);
            instMode[0x59] = new InstTable("EOR", AddressMode.IndexedY);
            instMode[0x48] = new InstTable("PHA", AddressMode.Implied);
            instMode[0x28] = new InstTable("PLP", AddressMode.Implied);
            instMode[0x98] = new InstTable("TYA", AddressMode.Implied);
            instMode[0x50] = new InstTable("BVC", AddressMode.Relative);
            instMode[0xe1] = new InstTable("SBC", AddressMode.PreIndexedX);
            instMode[0xfd] = new InstTable("SBC", AddressMode.IndexedX);
            instMode[0xe5] = new InstTable("SBC", AddressMode.ZeroPage);
            instMode[0xe9] = new InstTable("SBC", AddressMode.Immediate);
            instMode[0xf5] = new InstTable("SBC", AddressMode.ZeroPageIndexedX);
            instMode[0xed] = new InstTable("SBC", AddressMode.Absolute);
            instMode[0xf1] = new InstTable("SBC", AddressMode.PostIndexedY);
            instMode[0xf9] = new InstTable("SBC", AddressMode.IndexedY);
            instMode[0x0] = new InstTable("BRK", AddressMode.Implied);
            instMode[0x68] = new InstTable("PLA", AddressMode.Implied);
            instMode[0x1] = new InstTable("ORA", AddressMode.PreIndexedX);
            instMode[0x1d] = new InstTable("ORA", AddressMode.IndexedX);
            instMode[0x5] = new InstTable("ORA", AddressMode.ZeroPage);
            instMode[0x9] = new InstTable("ORA", AddressMode.Immediate);
            instMode[0x15] = new InstTable("ORA", AddressMode.ZeroPageIndexedX);
            instMode[0xd] = new InstTable("ORA", AddressMode.Absolute);
            instMode[0x11] = new InstTable("ORA", AddressMode.PostIndexedY);
            instMode[0x19] = new InstTable("ORA", AddressMode.IndexedY);
            instMode[0xea] = new InstTable("NOP", AddressMode.Implied);
            instMode[0xee] = new InstTable("INC", AddressMode.Absolute);
            instMode[0xfe] = new InstTable("INC", AddressMode.IndexedX);
            instMode[0xe6] = new InstTable("INC", AddressMode.ZeroPage);
            instMode[0xf6] = new InstTable("INC", AddressMode.ZeroPageIndexedX);
        }

        public CPU()
        {
            m_S = 0xFF;
            m_A = 0;
            m_X = 0;
            m_Y = 0;
            m_PC = 0;
        }

        public virtual void reset()
        {
            m_S = 0xFF;        // Reset Stack
            m_PC = (ushort)((peek(0xFFFD) << 8) | peek(0xFFFC));
        }

        public void stepOperation()
        {
            clock(1);
            byte op = peek(m_PC++);

            switch (op)
            {
                case 0x08: inst_PHP_IMP(); break;
                case 0x0a: inst_ASL_ACC(); break;
                case 0x0e: inst_ASL_ABS(); break;
                case 0x06: inst_ASL_ZZZ(); break;
                case 0x00: inst_BRK_IMP(); break;
                case 0x01: inst_ORA_PRE(); break;
                case 0x05: inst_ORA_ZZZ(); break;
                case 0x09: inst_ORA_IMM(); break;
                case 0x0d: inst_ORA_ABS(); break;
                case 0xca: inst_DEX_IMP(); break;
                case 0x88: inst_DEY_IMP(); break;
                case 0xaa: inst_TAX_IMP(); break;
                case 0x10: inst_BPL_REL(); break;
                case 0x90: inst_BCC_REL(); break;
                case 0xec: inst_CPX_ABS(); break;
                case 0xe0: inst_CPX_IMM(); break;
                case 0xe4: inst_CPX_ZZZ(); break;
                case 0xcc: inst_CPY_ABS(); break;
                case 0xc0: inst_CPY_IMM(); break;
                case 0xc4: inst_CPY_ZZZ(); break;
                case 0x81: inst_STA_PRE(); break;
                case 0x9d: inst_STA_INX(); break;
                case 0x85: inst_STA_ZZZ(); break;
                case 0x95: inst_STA_ZIX(); break;
                case 0x8d: inst_STA_ABS(); break;
                case 0x91: inst_STA_PST(); break;
                case 0x99: inst_STA_INY(); break;
                case 0xba: inst_TSX_IMP(); break;
                case 0xce: inst_DEC_ABS(); break;
                case 0xde: inst_DEC_INX(); break;
                case 0xc6: inst_DEC_ZZZ(); break;
                case 0xd6: inst_DEC_ZIX(); break;
                case 0xc1: inst_CMP_PRE(); break;
                case 0xdd: inst_CMP_INX(); break;
                case 0xc5: inst_CMP_ZZZ(); break;
                case 0xc9: inst_CMP_IMM(); break;
                case 0xd5: inst_CMP_ZIX(); break;
                case 0xcd: inst_CMP_ABS(); break;
                case 0xd1: inst_CMP_PST(); break;
                case 0xd9: inst_CMP_INY(); break;
                case 0xa1: inst_LDA_PRE(); break;
                case 0xbd: inst_LDA_INX(); break;
                case 0xa5: inst_LDA_ZZZ(); break;
                case 0xa9: inst_LDA_IMM(); break;
                case 0xb5: inst_LDA_ZIX(); break;
                case 0xad: inst_LDA_ABS(); break;
                case 0xb1: inst_LDA_PST(); break;
                case 0xb9: inst_LDA_INY(); break;
                case 0xf0: inst_BEQ_REL(); break;
                case 0x2a: inst_ROL_ACC(); break;
                case 0x2e: inst_ROL_ABS(); break;
                case 0x3e: inst_ROL_INX(); break;
                case 0x26: inst_ROL_ZZZ(); break;
                case 0x36: inst_ROL_ZIX(); break;
                case 0x8c: inst_STY_ABS(); break;
                case 0x84: inst_STY_ZZZ(); break;
                case 0x94: inst_STY_ZIX(); break;
                case 0x6c: inst_JMP_IND(); break;
                case 0x4c: inst_JMP_ABS(); break;
                case 0x30: inst_BMI_REL(); break;
                case 0x40: inst_RTI_IMP(); break;
                case 0xa8: inst_TAY_IMP(); break;
                case 0x8a: inst_TXA_IMP(); break;
                case 0x60: inst_RTS_IMP(); break;
                case 0xf8: inst_SED_IMP(); break;
                case 0x4a: inst_LSR_ACC(); break;
                case 0x4e: inst_LSR_ABS(); break;
                case 0x5e: inst_LSR_INX(); break;
                case 0x46: inst_LSR_ZZZ(); break;
                case 0x56: inst_LSR_ZIX(); break;
                case 0x20: inst_JSR_ABS(); break;
                case 0x38: inst_SEC_IMP(); break;
                case 0x2c: inst_BIT_ABS(); break;
                case 0x24: inst_BIT_ZZZ(); break;
                case 0xb0: inst_BCS_REL(); break;
                case 0x9a: inst_TXS_IMP(); break;
                case 0x78: inst_SEI_IMP(); break;
                case 0x1e: inst_ASL_INX(); break;
                case 0x16: inst_ASL_ZIX(); break;
                case 0x70: inst_BVS_REL(); break;
                case 0x58: inst_CLI_IMP(); break;
                case 0x6a: inst_ROR_ACC(); break;
                case 0x6e: inst_ROR_ABS(); break;
                case 0x7e: inst_ROR_INX(); break;
                case 0x66: inst_ROR_ZZZ(); break;
                case 0x76: inst_ROR_ZIX(); break;
                case 0xd8: inst_CLD_IMP(); break;
                case 0x18: inst_CLC_IMP(); break;
                case 0xb8: inst_CLV_IMP(); break;
                case 0x8e: inst_STX_ABS(); break;
                case 0x86: inst_STX_ZZZ(); break;
                case 0x96: inst_STX_ZIY(); break;
                case 0xbc: inst_LDY_INX(); break;
                case 0xac: inst_LDY_ABS(); break;
                case 0xa0: inst_LDY_IMM(); break;
                case 0xa4: inst_LDY_ZZZ(); break;
                case 0xb4: inst_LDY_ZIX(); break;
                case 0xd0: inst_BNE_REL(); break;
                case 0x21: inst_AND_PRE(); break;
                case 0x3d: inst_AND_INX(); break;
                case 0x25: inst_AND_ZZZ(); break;
                case 0x29: inst_AND_IMM(); break;
                case 0x35: inst_AND_ZIX(); break;
                case 0x2d: inst_AND_ABS(); break;
                case 0x31: inst_AND_PST(); break;
                case 0x39: inst_AND_INY(); break;
                case 0xbe: inst_LDX_INY(); break;
                case 0xae: inst_LDX_ABS(); break;
                case 0xa2: inst_LDX_IMM(); break;
                case 0xa6: inst_LDX_ZZZ(); break;
                case 0xb6: inst_LDX_ZIY(); break;
                case 0xe8: inst_INX_IMP(); break;
                case 0xc8: inst_INY_IMP(); break;
                case 0x41: inst_EOR_PRE(); break;
                case 0x5d: inst_EOR_INX(); break;
                case 0x45: inst_EOR_ZZZ(); break;
                case 0x49: inst_EOR_IMM(); break;
                case 0x55: inst_EOR_ZIX(); break;
                case 0x4d: inst_EOR_ABS(); break;
                case 0x51: inst_EOR_PST(); break;
                case 0x59: inst_EOR_INY(); break;
                case 0x48: inst_PHA_IMP(); break;
                case 0x28: inst_PLP_IMP(); break;
                case 0x98: inst_TYA_IMP(); break;
                case 0x50: inst_BVC_REL(); break;
                case 0x68: inst_PLA_IMP(); break;
                case 0x1d: inst_ORA_INX(); break;
                case 0x15: inst_ORA_ZIX(); break;
                case 0x11: inst_ORA_PST(); break;
                case 0x19: inst_ORA_INY(); break;
                case 0xee: inst_INC_ABS(); break;
                case 0xfe: inst_INC_INX(); break;
                case 0xe6: inst_INC_ZZZ(); break;
                case 0xf6: inst_INC_ZIX(); break;

                // --- UNTESTED OPCODES ---------

                case 0x61: inst_ADC_PRE(); break;
                case 0x7d: inst_ADC_INX(); break;
                case 0x65: inst_ADC_ZZZ(); break;
                case 0x69: inst_ADC_IMM(); break;
                case 0x75: inst_ADC_ZIX(); break;
                case 0x6d: inst_ADC_ABS(); break;
                case 0x71: inst_ADC_PST(); break;
                case 0x79: inst_ADC_INY(); break;
                case 0xe1: inst_SBC_PRE(); break;
                case 0xfd: inst_SBC_INX(); break;
                case 0xe5: inst_SBC_ZZZ(); break;
                case 0xe9: inst_SBC_IMM(); break;
                case 0xf5: inst_SBC_ZIX(); break;
                case 0xed: inst_SBC_ABS(); break;
                case 0xf1: inst_SBC_PST(); break;
                case 0xf9: inst_SBC_INY(); break;

                case 0xea: clock(1) /*NoOp*/ ; break;
                default: inst_Undefined(op); break;
            }
        }

        private void inst_ADC_BODY(byte value)
        {
            int tmp = value + m_A + (m_FlagCarry ? 1 : 0);

            if (m_FlagDecimal)
            {
                int tmp_a = (m_A & 0xf) + (value & 0xf) + (m_FlagCarry ? 1 : 0);

                if (tmp_a > 0x9)
                    tmp_a += 0x6;

                if (tmp_a <= 0x0f)
                    tmp_a = (tmp_a & 0xf) + (m_A & 0xf0) + (value & 0xf0);
                else
                    tmp_a = (tmp_a & 0xf) + (m_A & 0xf0) + (value & 0xf0) + 0x10;

                set_zero((byte)tmp);
                set_negitive((byte)tmp_a);
                set_overflow(((m_A ^ tmp_a) & ~(m_A ^ value) & 0x80) != 0);

                if ((tmp_a & 0x1f0) > 0x90)
                    tmp_a += 0x60;

                set_carry((tmp_a & 0xff0) > 0xf0);

                m_A = (byte)tmp_a;
            }
            else
            {
                m_A = (byte)tmp;

                set_overflow(((m_A ^ tmp) & ~(m_A ^ value) & 0x80) != 0);
                set_carry(tmp > 0xff);
                set_negitive(m_A);
                set_zero(m_A);
            }

        }

        private void inst_SBC_BODY(byte value)
        {
            ushort tmp;

            tmp = (ushort)(m_A - value - (m_FlagCarry ? 0 : 1));

            if (m_FlagDecimal)
            {
                ushort tmp_a = (ushort)((m_A & 0xf) - (value & 0xf) - (m_FlagCarry ? 0 : 1));

                if ((tmp_a & 0x10)!= 0)
                    tmp_a = (ushort)(((tmp_a - 6) & 0xf) | ((m_A & 0xf0) - (value & 0xf0) - 0x10));
                else
                    tmp_a = (ushort)((tmp_a & 0xf) | ((m_A & 0xf0) - (value & 0xf0)));

                if ((tmp_a & 0x100)!= 0)
                    tmp_a -= 0x60;

                m_A = (byte)tmp_a;
            }
            else
            {
                m_A = (byte)tmp;
            }

            set_carry(tmp < 0x100);
            set_zero((byte)tmp);
            set_negitive((byte)tmp);
            set_overflow(((m_A ^ tmp) & (m_A ^ value) & 0x80) != 0);
        }
        #region CPU Opcode Helpers

        /*******************************************************************************
		**
		**  PYTARI - Atari 2600 emulation in python
		**
		**  Copyright (c) 2000-2005 by Bryon Vandiver
		**
		**  See the file "license" for information on usage and redistribution of
		**  this file, and for a DISCLAIMER OF ALL WARRANTIES.
		**
		********************************************************************************
		**
		**   6507 CPU core, interpretive opcode set
		**   
		**   Requirements
		**       Register Set { S, A, X, Y, P, PC, PRE }
		**       Bus Driver
		**           push()      push_es a value to the stack
		**           pull()      Pulls a value from the stack
		**           peek()     Grabs a value off the bus
		**           poke()     Writes a value to the bus
		**   
		**   Pulls and push_es both preform their becrementation
		**   post read / write, to conform to the perceived
		**   method laid out by the cyclic timing chart
		**
		*******************************************************************************/


        //
        //   The flag getters and setters, this is purely for readability
        //   of code, these will most likely be depreciated eventually, and
        //   simply inlined.
        //

        public byte BuildStatus(bool Break)
        {
            return (byte)( 0x20 |
                (m_FlagCarry        ? 0x01 : 0x00) |
                (m_FlagZero         ? 0x02 : 0x00) |
                (Break              ? 0x04 : 0x00) |
                (m_FlagDecimal      ? 0x08 : 0x00) |
                (m_FlagInterrupt    ? 0x10 : 0x00) |
                (m_FlagOverflow     ? 0x40 : 0x00) |
                (m_FlagNegitive     ? 0x80 : 0x00) );
        }

        private void SetStatus(byte Value)
        {
            m_FlagCarry     = ( (Value & 0x01) != 0 );
            m_FlagZero      = ( (Value & 0x02) != 0 );
            m_FlagDecimal   = ( (Value & 0x08) != 0 );
            m_FlagInterrupt = ( (Value & 0x10) != 0 );
            m_FlagOverflow  = ( (Value & 0x40) != 0 );
            m_FlagNegitive  = ( (Value & 0x80) != 0 );
        }

        private void set_carry(bool condition)
        {
            m_FlagCarry = condition;
        }

        private void set_zero(byte data)
        {
            m_FlagZero = (data == 0);
        }

        private void set_interrupt(bool condition)
        {
            m_FlagInterrupt = condition;
        }

        private void set_decimal(bool condition)
        {
            m_FlagDecimal = condition;
        }

        private void set_overflow(bool condition)
        {
            m_FlagOverflow = condition;
        }

        private void set_negitive(byte data)
        {
            m_FlagNegitive = ((data & 0x80) != 0);
        }
        #endregion
        #region CPU Opcode Definitions

        private void inst_Undefined(byte op)
        {
            Console.WriteLine("Encountered invalid instruction {0,2:X} at ~{0,4:X}", op, m_PC);
//            reset();
        }

        private void inst_BRK_IMP()
        {
            clock(6);

            poke((ushort)(m_S-- | 0x100), (byte)(m_PC >> 8));
            poke((ushort)(m_S-- | 0x100), (byte)m_PC);

            poke((ushort)(m_S-- | 0x100), BuildStatus(true));

            set_interrupt(true);
            m_PC = peek(0xFFFE);
            m_PC = (ushort)(m_PC | peek(0xFFFF) << 8);
        }

        private void inst_RTI_IMP()
        {
            clock(5);

            SetStatus(peek((ushort)(++m_S | 0x100)));
            m_PC = peek((ushort)(++m_S|0x100));
            m_PC = (ushort)(m_PC | peek((ushort)(m_S|0x100)) << 8);
        }

        private void inst_RTS_IMP()
        {
            clock(5);

            m_PC = peek((ushort)(++m_S|0x100));
            m_PC |= (ushort)(peek((ushort)(++m_S|0x100)) << 8);
            m_PC++;
        }

        private void inst_PHA_IMP()
        {
            clock(2);
            poke((ushort)(m_S--|0x100), m_A);
        }

        private void inst_PHP_IMP()
        {
            clock(2);
            poke((ushort)(m_S--|0x100), BuildStatus(false));
        }

        private void inst_PLA_IMP()
        {
            clock(3);

            m_A = peek((ushort)(++m_S|0x100));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_PLP_IMP()
        {
            clock(3);

            SetStatus(peek((ushort)(++m_S | 0x100)));
        }

        private void inst_JSR_ABS()
        {
            clock(5);

            byte PRE = peek(m_PC++);

            poke((ushort)(m_S-- | 0x100), (byte)(m_PC >> 8));
            poke((ushort)(m_S-- | 0x100), (byte)m_PC);

            m_PC = (ushort)((peek(m_PC) << 8) | PRE);
        }

        private void inst_JMP_ABS()
        {
            clock(2);

            byte PRE = peek(m_PC++);
            m_PC = (ushort)(peek(m_PC) << 8 | PRE);
        }

        private void inst_JMP_IND()
        {
            clock(4);

            ushort src = peek(m_PC++);
            src |= (ushort)(peek(m_PC) << 8);
            
            m_PC = peek(src);
            m_PC |= (ushort)(peek((ushort)(((src + 1) & 0xFF) | (src & 0xFF00))) << 8);
        }

        private void inst_DEX_IMP()
        {
            clock(1);
            m_X--;
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_DEY_IMP()
        {
            clock(1);
            m_Y--;
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        #endregion
        #region Template Opcodes

        private void inst_BPL_REL()
        {
            clock(1);
            ushort src;
            sbyte offset = (sbyte)peek(m_PC++);

            if (!m_FlagNegitive)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_BCC_REL()
        {
            ushort src;

            clock(1);
            sbyte offset = (sbyte)peek(m_PC++);
            if ((!m_FlagCarry))
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_CPX_IMM()
        {
            short fetch;

            clock(1);
            fetch = (short)(m_X - peek(m_PC++));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CPX_ZZZ()
        {
            clock(2);

            short fetch = (short)(m_X - peek(peek(m_PC++)));

            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CPX_ABS()
        {
            short fetch;
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = (short)(m_X - peek(src));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_EOR_IMM()
        {
            clock(1);

            m_A ^= peek(m_PC++);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_ZZZ()
        {
            clock(2);

            m_A ^= peek(peek(m_PC++));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_ZIX()
        {
            clock(3);

            m_A ^= peek((byte)(peek(m_PC++) + m_X));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_ABS()
        {
            clock(3);

            byte PRE = peek(m_PC++);
            m_A ^= peek((ushort)((peek(m_PC++) << 8) | PRE));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_INX()
        {
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            
            if ( offset > 0xFF )
            {
                clock(1);
            }

            m_A ^= peek((ushort)(offset + src));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_INY()
        {
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)(peek(m_PC++) << 8);

            if ((offset & 0x100) != 0)
            {
                clock(1);
            }

            m_A ^= peek((ushort)(offset + src));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_PRE()
        {
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            m_A ^= peek(addr);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_EOR_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);
            
            if (offset > 0xFF)
            {
                clock(1);
            }

            m_A ^= peek((ushort)(addr+offset)); 
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_TSX_IMP()
        {
            clock(1);
            m_X = m_S;
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_DEC_ZZZ()
        {
            byte fetch;

            clock(4);

            byte src = peek(m_PC++);
            fetch = peek(src);
            poke(src, fetch);
            fetch--;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_DEC_ZIX()
        {
            byte fetch;
            byte src;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            fetch = peek(src);
            poke(src, fetch);
            fetch--;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_DEC_ABS()
        {
            byte fetch;
            ushort src;

            clock(5);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = peek(src);
            poke(src, fetch);
            fetch--;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_DEC_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(6);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            src = (ushort)(offset + src);

            fetch = peek(src);
            poke(src, fetch);
            fetch--;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_CMP_IMM()
        {
            clock(1);

            short fetch = (short)(m_A - peek(m_PC++));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_ZZZ()
        {
            short fetch;

            clock(2);

            fetch = (short)(m_A - peek(peek(m_PC++)));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_ZIX()
        {
            short fetch;

            clock(3);

            fetch = (short)(m_A - peek((byte)(peek(m_PC++) + m_X)));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_ABS()
        {
            short fetch;
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)((peek(m_PC++) << 8));
            fetch = (short)(m_A - peek(src));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_INX()
        {
            short fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ( offset > 0xFF )
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }

            fetch = (short)(m_A - fetch);
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_INY()
        {
            short fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ((offset & 0x100) != 0)
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }
            fetch = (short)(m_A - fetch);

            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_PRE()
        {
            short fetch;
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            fetch = (short)(m_A - peek(addr));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CMP_PST()
        {
            short fetch;

            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);
            
            if (offset > 0xFF)
            {
                clock(1);
            }

            fetch = (short)(m_A - peek((ushort)(addr + offset)));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_LDA_IMM()
        {
            clock(1);

            m_A = peek(m_PC++);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_ZZZ()
        {
            clock(2);

            m_A = peek(peek(m_PC++));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_ZIX()
        {
            clock(3);

            m_A = peek((byte)(peek(m_PC++) + m_X));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_ABS()
        {
            clock(3);

            byte PRE = peek(m_PC++);
            m_A = peek((ushort)((peek(m_PC++) << 8) | PRE));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_INX()
        {
            ushort src;
            ushort offset;
            byte PRE = peek(m_PC++);

            clock(3);

            src = (ushort)(peek(m_PC++) << 8);
            offset = (ushort)(PRE + m_X);

            if ((offset & 0xFF00) != 0)
            {
                clock(1);
            }

            m_A = peek((ushort)(offset + src));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_INY()
        {
            ushort src;
            ushort offset;

            clock(3);

            byte PRE = peek(m_PC++);
            src = (ushort)(peek(m_PC++) << 8);
            offset = (ushort)(PRE + m_Y);

            if ((offset & 0xFF00) != 0)
            {
                clock(1);
            }

            m_A = peek((ushort)(offset + src));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_PRE()
        {
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            m_A = peek(addr);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LDA_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);

            if (offset > 0xFF)
            {
                clock(1);
            }

            m_A = peek((ushort)(offset+addr));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_BEQ_REL()
        {
            clock(1);

            sbyte offset = (sbyte)peek(m_PC++);
            ushort src;

            if (m_FlagZero)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_ROL_ACC()
        {
            byte fetch;
            byte temp;

            clock(1);
            
            temp = m_A;
            fetch = (byte)((m_A << 1) | (m_FlagCarry ? 1 : 0));
            set_carry((temp & 0x80) != 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
            m_A = (byte)fetch;
        }

        private void inst_ROL_ZZZ()
        {
            byte fetch;
            byte temp;

            clock(4);

            byte PRE = peek(m_PC++);
            
            fetch = peek(PRE);
            poke(PRE, (byte)fetch);
            temp = fetch;

            fetch = (byte)((fetch << 1) | (m_FlagCarry ? 1 : 0));
            set_carry((temp & 0x80) != 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
            poke(PRE, (byte)fetch);
        }

        private void inst_ROL_ZIX()
        {
            byte fetch;
            byte src;
            byte temp;

            clock(5);

            byte PRE = peek(m_PC++);
            src = (byte)(PRE + m_X);
            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch << 1) | (m_FlagCarry ? 1 : 0));
            set_carry((temp & 0x80) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ROL_ABS()
        {
            byte fetch;
            ushort src;
            byte temp;

            clock(5);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch << 1) | (m_FlagCarry ? 1 : 0));
            set_carry((temp & 0x80) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ROL_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;
            byte temp;

            clock(6);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            src = (ushort)(offset + src);

            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch << 1) | (m_FlagCarry ? 1 : 0));
            set_carry((temp & 0x80) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_BMI_REL()
        {
            ushort src;

            clock(1);

            sbyte offset = (sbyte)peek(m_PC++);
            if (m_FlagNegitive)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_TAX_IMP()
        {
            clock(1);
            m_X = m_A;
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_TAY_IMP()
        {
            clock(1);
            m_Y = m_A;
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_TXA_IMP()
        {
            clock(1);
            m_A = m_X;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_STY_ZZZ()
        {
            clock(2);

            poke(peek(m_PC++), m_Y);
        }

        private void inst_STY_ZIX()
        {
            clock(3);
            poke((byte)(peek(m_PC++) + m_X), m_Y);
        }

        private void inst_STY_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            poke(src, m_Y);
        }

        private void inst_SED_IMP()
        {
            clock(1);
            set_decimal(true);
        }

        private void inst_SEC_IMP()
        {
            clock(1);
            set_carry(true);
        }

        private void inst_LSR_ACC()
        {
            clock(1);
            set_carry((m_A & 0x01) != 0);
            m_A >>= 1;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_LSR_ZZZ()
        {
            byte fetch;

            clock(4);

            byte src = peek(m_PC++);
            fetch = peek(src);
            poke(src, fetch);
            set_carry((fetch & 0x01) != 0);
            fetch >>= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_LSR_ZIX()
        {
            byte fetch;
            byte src;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            fetch = peek(src);
            poke(src, fetch);
            set_carry((fetch & 0x01) != 0);
            fetch >>= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_LSR_ABS()
        {
            byte fetch;
            ushort src;

            clock(5);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);

            fetch = peek(src);
            poke(src, fetch);
            set_carry((fetch & 0x01) != 0);
            fetch >>= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_LSR_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;


            clock(6);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            src = (ushort)(offset + src);
            fetch = peek(src);

            poke(src, fetch);
            set_carry((fetch & 0x01) != 0);
            fetch >>= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_LDY_IMM()
        {
            clock(1);
            m_Y = peek(m_PC++);
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_LDY_ZZZ()
        {
            clock(2);
            m_Y = peek(peek(m_PC++));
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_LDY_ZIX()
        {
            clock(3);

            m_Y = peek((byte)(peek(m_PC++) + m_X));
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_LDY_ABS()
        {
            clock(3);

            byte PRE = peek(m_PC++);
            m_Y = peek((ushort)((peek(m_PC++) << 8) | PRE));
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_LDY_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ( offset > 0xFF)
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }

            m_Y = fetch;
            set_zero(fetch);
            set_negitive(fetch);
        }

        private void inst_LDX_IMM()
        {
            clock(1);
            m_X = peek(m_PC++);
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_LDX_ZZZ()
        {
            clock(2);

            m_X = peek(peek(m_PC++));
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_LDX_ZIY()
        {
            byte fetch;
            byte src;

            clock(3);

            src = (byte)(peek(m_PC++) + m_Y);
            fetch = peek(src);
            m_X = fetch;
            set_zero(fetch);
            set_negitive(fetch);
        }

        private void inst_LDX_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            m_X = peek(src);
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_LDX_INY()
        {
            ushort fetch;
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src += m_Y;
            if (src > 0xFF)
            {
                clock(1);
            }
            src += (ushort)(peek(m_PC++) << 8);
            fetch = peek(src);

            m_X = (byte)fetch;
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_TXS_IMP()
        {
            clock(1);
            m_S = m_X;
        }

        private void inst_SEI_IMP()
        {
            clock(1);
            set_interrupt(true);
        }

        private void inst_ASL_ACC()
        {
            clock(1);
            set_carry((m_A & 0x80) != 0);
            m_A <<= 1;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ASL_ZZZ()
        {
            byte fetch;

            clock(4);

            byte PRE = peek(m_PC++);
            fetch = peek(PRE);
            poke(PRE, fetch);
            
            set_carry((fetch & 0x80) != 0);
            fetch <<= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(PRE, fetch);
        }

        private void inst_ASL_ZIX()
        {
            byte fetch;
            byte src;


            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            fetch = peek(src);
            poke(src, fetch);
            set_carry((fetch & 0x80) != 0);
            fetch <<= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ASL_ABS()
        {
            byte fetch;
            ushort src;


            clock(5);

            byte PRE = peek(m_PC++);
            src = (ushort)((peek(m_PC++) << 8) | PRE);
            
            fetch = peek(src);            
            poke(src, fetch);            
            set_carry((fetch & 0x80) != 0);            
            fetch <<= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ASL_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(6);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            src = (ushort)(offset + src);
            
            fetch = peek(src);
            poke(src, fetch);            
            set_carry((fetch & 0x80) != 0);            
            fetch <<= 1;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_BVS_REL()
        {
            ushort src;

            clock(1);
            sbyte offset = (sbyte)peek(m_PC++);
            if (m_FlagOverflow)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_CLI_IMP()
        {
            clock(1);
            set_interrupt(false);
        }

        private void inst_CLD_IMP()
        {
            clock(1);
            set_decimal(false);
        }

        private void inst_CLC_IMP()
        {
            clock(1);
            set_carry(false);
        }

        private void inst_BCS_REL()
        {
            ushort src;

            clock(1);
            sbyte offset = (sbyte)peek(m_PC++);

            if (m_FlagCarry)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_BIT_ZZZ()
        {
            byte fetch;

            clock(2);

            byte PRE = peek(m_PC++);
            fetch = peek(PRE);
            
            set_negitive(fetch);
            set_overflow((fetch & 0x40) != 0);

            set_zero((byte)(fetch & m_A));
        }

        private void inst_BIT_ABS()
        {
            byte fetch;

            clock(3);

            byte PRE = peek(m_PC++);
            fetch = peek((ushort)((peek(m_PC++) << 8) | PRE));
            set_negitive(fetch);
            set_overflow((fetch & 0x40) != 0);
            set_zero((byte)(fetch & m_A));
        }

        private void inst_ADC_IMM()
        {
            clock(1);

            inst_ADC_BODY(peek(m_PC++));
        }

        private void inst_ADC_ZZZ()
        {
            clock(2);

            inst_ADC_BODY(peek(peek(m_PC++)));
        }

        private void inst_ADC_ZIX()
        {
            clock(3);

            inst_ADC_BODY(peek((byte)(peek(m_PC++) + m_X)));
        }

        private void inst_ADC_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);

            inst_ADC_BODY(peek(src));
        }

        private void inst_ADC_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)((peek(m_PC++) << 8)+offset);

            if ( offset > 0x1FF )
            {
                clock(1);
            }
            fetch = peek(src);

            inst_ADC_BODY(fetch);
        }

        private void inst_ADC_INY()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)((peek(m_PC++) << 8)+offset);
            if (offset > 0x100)
            {
                clock(1);
            }
            fetch = peek(src);

            inst_ADC_BODY(fetch);
        }

        private void inst_ADC_PRE()
        {
            clock(5);

            byte src = (byte)(peek(m_PC++) + m_X);
            ushort addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);
           
            inst_ADC_BODY(peek(addr));
        }

        private void inst_ADC_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);

            if (offset > 0xFF)
            {
                clock(1);
            }

            inst_ADC_BODY(peek((ushort)(addr + offset)));
        }

        private void inst_CLV_IMP()
        {
            clock(1);
            set_overflow(false);
        }

        private void inst_STX_ZZZ()
        {
            clock(2);
            poke(peek(m_PC++), m_X);
        }

        private void inst_STX_ZIY()
        {
            clock(3);

            poke((byte)(peek(m_PC++) + m_Y), m_X);
        }

        private void inst_STX_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            poke(src, m_X);
        }

        private void inst_ROR_ACC()
        {
            byte temp;

            clock(1);
            temp = m_A;
            m_A = (byte)((m_A >> 1) | (m_FlagCarry ? 0x80 : 0));
            set_carry((temp & 0x01) != 0);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ROR_ZZZ()
        {
            byte fetch;
            byte temp;

            clock(4);

            byte src = peek(m_PC++);
            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch >> 1) | (m_FlagCarry ? 0x80 : 0));
            set_carry((temp & 0x01) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ROR_ZIX()
        {
            byte fetch;
            byte src;
            byte temp;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch >> 1) | (m_FlagCarry ? 0x80 : 0));
            set_carry((temp & 0x01) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ROR_ABS()
        {
            byte fetch;
            ushort src;
            byte temp;

            clock(5);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = peek(src);
            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch >> 1) | (m_FlagCarry ? 0x80 : 0));
            set_carry((temp & 0x01) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_ROR_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;
            byte temp;

            clock(6);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            src = (ushort)(offset + src);
            fetch = peek(src);

            poke(src, fetch);
            temp = fetch;
            fetch = (byte)((fetch >> 1) | (m_FlagCarry ? 0x80 : 0));
            set_carry((temp & 0x01) != 0);
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_BNE_REL()
        {
            clock(1);

            sbyte offset = (sbyte)peek(m_PC++);
            ushort src;
            
            if (!m_FlagZero)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_AND_IMM()
        {
            clock(1);
            m_A &= peek(m_PC++);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_ZZZ()
        {
            clock(2);

            m_A &= peek(peek(m_PC++));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_ZIX()
        {
            clock(3);

            m_A &= peek((byte)(peek(m_PC++) + m_X));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            m_A &= peek(src);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ((offset & 0x100) != 0)
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }
            m_A &= fetch;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_INY()
        {
            byte fetch;
            ushort src;
            ushort offset;


            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));

            if ( offset > 0xFF )
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }
            m_A &= fetch;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_PRE()
        {
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            m_A &= peek(addr);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_AND_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);
            if (offset > 0xFF)
            {
                clock(1);
            }
            m_A &= peek((ushort)(addr + offset));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_INX_IMP()
        {
            clock(1);
            m_X++;
            set_zero(m_X);
            set_negitive(m_X);
        }

        private void inst_INY_IMP()
        {
            clock(1);
            m_Y++;
            set_zero(m_Y);
            set_negitive(m_Y);
        }

        private void inst_CPY_IMM()
        {
            short fetch;

            clock(1);
            fetch = (short)(m_Y - peek(m_PC++));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CPY_ZZZ()
        {
            short fetch;

            clock(2);

            fetch = (short)(m_Y - peek(peek(m_PC++)));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_CPY_ABS()
        {
            short fetch;
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = (short)(m_Y - peek(src));
            set_carry(fetch >= 0);
            set_zero((byte)fetch);
            set_negitive((byte)fetch);
        }

        private void inst_STA_ZZZ()
        {
            clock(2);
            poke(peek(m_PC++), m_A);
        }

        private void inst_STA_ZIX()
        {
            clock(3);

            poke((byte)(peek(m_PC++) + m_X), m_A);
        }

        private void inst_STA_ABS()
        {
            ushort src;

            clock(3);

            byte PRE = peek(m_PC++);
            src = (ushort)((peek(m_PC++) << 8) | PRE);
            poke(src, m_A);
        }

        private void inst_STA_INX()
        {
            clock(4);
            byte PRE = peek(m_PC++);
            poke((ushort)((peek(m_PC++) << 8) + PRE + m_X), m_A);
        }

        private void inst_STA_INY()
        {
            ushort src;

            clock(4);

            src = peek(m_PC++);
            src += (ushort)((peek(m_PC++) << 8) | m_Y);
            poke(src, m_A);
        }

        private void inst_STA_PRE()
        {
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            peek(addr);
            poke(addr, m_A);
        }

        private void inst_STA_PST()
        {
            clock(5);

            byte src = peek(m_PC++);
            ushort addr = (ushort)(peek(src++) + m_Y);
            addr += (ushort)(peek(src) << 8);
            poke(addr, m_A);
        }

        private void inst_TYA_IMP()
        {
            clock(1);
            m_A = m_Y;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_BVC_REL()
        {
            ushort src;

            clock(1);

            sbyte offset = (sbyte)peek(m_PC++);
            if (!m_FlagOverflow)
            {
                clock(1);
                src = (ushort)(m_PC + offset);
                if (((src ^ m_PC) & 0xFF00) != 0)
                {
                    clock(1);
                }
                m_PC = src;
            }
        }

        private void inst_SBC_IMM()
        {
            clock(1);

            inst_SBC_BODY(peek(m_PC++));
        }

        private void inst_SBC_ZZZ()
        {
            clock(2);

            inst_SBC_BODY(peek(peek(m_PC++)));
        }

        private void inst_SBC_ZIX()
        {
            clock(3);

            inst_SBC_BODY(peek((byte)(peek(m_PC++) + m_X)));
        }

        private void inst_SBC_ABS()
        {
            ushort src;

            clock(3);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);

            inst_SBC_BODY(peek(src));
        }

        private void inst_SBC_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)((peek(m_PC++) << 8)+offset);

            if ( offset > 0x1FF )
            {
                clock(1);
            }
            fetch = peek(src);

            inst_SBC_BODY(fetch);
        }

        private void inst_SBC_INY()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)((peek(m_PC++) << 8)+offset);
            if (offset > 0x100)
            {
                clock(1);
            }
            fetch = peek(src);

            inst_SBC_BODY(fetch);
        }

        private void inst_SBC_PRE()
        {
            clock(5);

            byte src = (byte)(peek(m_PC++) + m_X);
            ushort addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);
           
            inst_SBC_BODY(peek(addr));
        }

        private void inst_SBC_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);

            if (offset > 0xFF)
            {
                clock(1);
            }

            inst_SBC_BODY(peek((ushort)(addr + offset)));
        }

        private void inst_INC_ZZZ()
        {
            clock(4);

            byte addr = peek(m_PC++);
            byte fetch = peek(addr);
            poke(addr, fetch);
            fetch++;
            set_zero(fetch);
            set_negitive(fetch);
            poke(addr, fetch);
        }

        private void inst_INC_ZIX()
        {
            byte fetch;
            byte src;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            
            fetch = peek(src);
            poke(src, fetch);
            fetch++;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_INC_ABS()
        {
            byte fetch;
            ushort src;

            clock(5);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            fetch = peek(src);
            poke(src, fetch);
            fetch++;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }

        private void inst_INC_INX()
        {
            byte fetch;
            ushort src;

            clock(6);

            src = peek(m_PC++);
            src |= (ushort)(peek(m_PC++) << 8);
            src += m_X;
            
            fetch = peek(src);
            poke(src, fetch);
            fetch++;
            set_zero(fetch);
            set_negitive(fetch);
            poke(src, fetch);
        }


        private void inst_ORA_IMM()
        {
            clock(1);

            m_A |= peek(m_PC++);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_ZZZ()
        {
            clock(2);

            m_A |= peek(peek(m_PC++));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_ZIX()
        {
            clock(3);

            m_A |= peek((byte)(peek(m_PC++) + m_X));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_ABS()
        {
            clock(3);

            byte PRE = peek(m_PC++);

            m_A |= peek((ushort)((peek(m_PC++) << 8) | PRE));
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_INX()
        {
            byte fetch;
            ushort src;
            ushort offset;

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_X);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ((offset & 0x100) != 0)
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }

            m_A |= fetch;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_INY()
        {
            byte fetch;
            ushort src;
            ushort offset;

            // !!!

            clock(3);

            offset = (ushort)(peek(m_PC++) + m_Y);
            src = (ushort)(peek(m_PC++) << 8);
            fetch = peek((ushort)(offset & 0xFF | src));
            if ((offset & 0x100) != 0)
            {
                clock(1);
                fetch = peek((ushort)(offset + src));
            }

            m_A |= fetch;
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_PRE()
        {
            byte src;
            ushort addr;

            clock(5);

            src = (byte)(peek(m_PC++) + m_X);
            addr = peek(src++);
            addr |= (ushort)(peek(src) << 8);

            m_A |= peek(addr);
            set_zero(m_A);
            set_negitive(m_A);
        }

        private void inst_ORA_PST()
        {
            clock(4);

            byte src = peek(m_PC++);
            ushort offset = (ushort)(peek(src++) + m_Y);
            ushort addr = (ushort)(peek(src) << 8);

            if (offset > 0xFF)
            {
                clock(1);
            }
            m_A |= peek((ushort)(offset + addr));
            set_zero(m_A);
            set_negitive(m_A);
        }
        #endregion
    }
}

