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
**   RIOT (CMOS 6532) Emulation
**
**   This code emulates the RIOT chip, minus the 128 bytes
**   of ram which suprisingly enough provides all of the
**   memory in the atari, minus a few registers (most which
**   are read or write only)
**
**   Since the (r) and (iot) are sperated, only the IO and
**   Timer is implemented
**
**   The RIOT is mapped in the atari thusly:
**
**     Address
**   +-+-+-+-+-+
**   |4|3|2|1|0|     Read    Write   
**   +-+-+-+-+-+
**   
**    - - 0 S 0      Output  Output  S = Port
**    - - 0 S 1      DDR     DDR     S = Port
**    - A 1 - 0      Timer   ------  A = Enable Timer IRQ
**    - - 1 - 1      IRQ     ------
**    0 - 1 B C      ------  Edge    B = Enable PA7 IRQ      C = +/- edge
**    1 A 1 B B      ------  Timer   A = Enable Timer IRQ    B = Interval
**
**   Since the 6507 has no IRQ lines, the emulation does not handle the
**   Enable Lines.  Reading the IRQ Flag register clears the PA7 IRQ,
**   addressing the timer register causes the timer IRQ to reset
**   The edge register gains data from the addressing lines, not thrown
**   data.
**
**   How output PB is handled is a mystery to me, it very well could be
**   used to mask the port, but not actually be written to (in the case
**   of a diode farm)
**
**   JOYSTICK API:
**
**       Clocks are randomly added to the joystick, it is it"s
**       responcibility to know when it has changed.
**
**       <-Transition()  - Joystick must call this when PA7 has changed
**       fetch()         - Poll for status of the fields, packed
**       throw(value)    - Forces output to the joystick ( masked )
**       buttons()       - Fetches the POTS and FIRE button, used by TIA
**
*******************************************************************************/

using System;
using cstari.input;

/*
class RIOT:
*/

namespace cstari.chips
{
    /// <summary>
    /// Summary description for riot.
    /// </summary>

    public class RIOT
    {
        private Controller m_CtrlA;
        private Controller m_CtrlB;

        private int m_Clocks;
        private int m_Timer;

        private byte m_DDRA;
        private byte m_DDRB;
        private byte m_OUTA;
        private byte m_OUTB;

        private byte m_IRQ;
        private byte m_Edge;

        private int m_Shift;

        public PanelButtons m_Panel;

        public enum PanelButtons : byte
        {
            Reset           = 1,
            Select          = 2,
            Color           = 8,
            DifficultyP0    = 64,
            DifficultyP1    = 128
        }

        public RIOT(Controller CtrlA, Controller CtrlB)
        {
            m_Clocks = 0xCD;					// Riot gains a random start position
            m_Timer = 0x7FFF;					// Timer is reset

            m_DDRA = 0x00;					// Initially, everything is a read
            m_DDRB = 0x00;					// ONLY HOLDS DATA!  This might also mask data
            m_OUTA = 0x00;					// Only A allows output
            m_OUTB = 0x00;					// THIS MIGHT BE DEPRECIATED, since the port is read only

            m_IRQ = 0x00;					// No events have occured, but they could later
            m_Edge = 0;						// Interrupt occurs on a negitive edge
            m_Shift = 10;						// There is no inverval shift
            m_Clocks = 0;						// RIOT Timing

            m_Panel = PanelButtons.Reset | PanelButtons.Color | PanelButtons.Select;

            m_CtrlA = CtrlA;					// Start off with dummy controllers, these do nothing
            m_CtrlB = CtrlB;					// but return OFF states, only player one gets an IRQ
        }

        public void reset()
        {
            m_Timer = 0x177DB;					// Timer is reset
            m_Shift = 10;						// single cycle counting
        }

        public void plug(Controller CtrlA, Controller CtrlB)
        {
            if (CtrlA != null)
            {
                CtrlA.transitions(this);
            }
            if (CtrlB != null)
            {
                CtrlB.transitions(this);
            }

            m_CtrlA = CtrlA;
            m_CtrlB = CtrlB;
        }

        public void transisition(byte edge)
        {
            if (edge == m_Edge)	// Does the edge match
            {
                m_IRQ = (byte)(m_IRQ | 0x40);
            }
        }

        public void clock(int clock)
        {
            m_Clocks += clock;
        }

        public void AlterPanel(PanelButtons toggle, PanelButtons release, PanelButtons press)
        {
            m_Panel ^= toggle;
            m_Panel |= release;
            m_Panel &= ~press;
        }

        public byte peek(ushort address, byte data)
        {
            m_Timer -= m_Clocks / 3;
            m_Clocks %= 3;					            // The riot runs 1/3rd the max speed

            if (m_Timer < 0)							// Timer rollover
            {
                m_Timer = m_Timer & 0xFF;				// Roll over
                m_Shift = 0;							// Unshifted
                m_IRQ = (byte)(m_IRQ | 0x80);			// Timer IRQ occured
            }

            if ((address & 0x4) != 0)					// Timer and IRQ
            {
                if ((address & 0x1) != 0)				// IRQ status
                {
                    byte oldIRQ = m_IRQ;
                    m_IRQ &= 0x80;		                // Clear PA7 flag
                    return oldIRQ;						// Pre-clear value
                }
                else									// Read Timer
                {
                    m_IRQ &= 0x40;		                // Clear timer flag
                    return (byte)(m_Timer >> m_Shift);	// Return "fixed" value
                }
            }

            else										// Ports
            {
                if ((address & 0x1) != 0)				// Direction selection
                {
                    if ((~address & 0x02) != 0)			// Read port A Status
                    {
                        return m_DDRA;
                    }
                    else								// Read port B Status
                    {
                        return m_DDRB;
                    }
                }
                else									// Read input ORed with the output
                {
                    if ((~address & 0x02) != 0)			// Read port A Status (Controllers)
                    {
                        byte input = 0;

                        if (m_CtrlA != null)
                            input = (byte)(m_CtrlA.peek() << 4);
                        if (m_CtrlB != null)
                            input |= m_CtrlB.peek();

                        return (byte)((m_OUTA & m_DDRA) | (input & ~m_DDRA));
                    }
                    else								// Read port B status
                    {
                        return (byte)((byte)m_Panel & ~m_DDRB);
                    }
                }
            }
        }

        public void poke(ushort address, byte data)
        {
            m_Timer -= m_Clocks / 3;
            m_Clocks %= 3;

            if (m_Timer < 0)							// Timer rollover
            {
                m_Timer = m_Timer & 0xFF;				// Roll over
                m_Shift = 0;							// Unshifted
                m_IRQ = (byte)(m_IRQ | 0x80);			// Timer IRQ occured
            }

            if ((address & 0x4) != 0)					// Timer and Edge
            {
                if ((address & 0x10) != 0)			// Write timer
                {
                    int interval = address & 0x3;		// Gain the interval
                    m_IRQ &= 0x40;		                // Clear timer IRQ

                    if (interval == 3)
                    {
                        m_Shift = 10;					// shift value is usually a multiple of 3
                    }
                    else
                    {
                        m_Shift = interval * 3;
                    }

                    m_Timer = (data << m_Shift);
                }
                else									// Write edge
                {
                    m_Edge = (byte)(address & 0x1);
                }
            }
            else										// Ports
            {
                if ((address & 0x1) != 0)				// Direction selection
                {
                    if ((address & 0x02) == 0)			// Read port A Status
                    {
                        m_DDRA = data;					// Data direction changed, give joys new data          

                        if (m_CtrlA != null)
                            m_CtrlA.poke((byte)((data & m_OUTA) >> 4));
                        if (m_CtrlB != null)
                            m_CtrlB.poke((byte)(data & m_OUTB & 0xF));
                    }
                    else								// Read port B Status
                    {
                        m_DDRB = data;					// Data direction changed, but co
                    }
                }
                else
                {										// Write output
                    if ((address & 0x02) == 0)			// Read port A Status
                    {
                        m_OUTA = data;					// Output to the controllers changed

                        if (m_CtrlA != null)
                            m_CtrlA.poke((byte)((data & m_OUTA) >> 4));
                        if (m_CtrlB != null)
                            m_CtrlB.poke((byte)(data & m_OUTB & 0xF));

                        return;
                    }
                    else								// Read port B Status
                    {
                        m_OUTB = data;					// Output to PortB changed, this may be completely useless
                    }
                }
            }
        }
    }
}
