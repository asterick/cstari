using System;
using cstari.input;
using cstari.chips;

namespace cstari.input
{
    /// <summary>
    /// Summary description for dummy.
    /// </summary>
    public class Dummy : Controller
    {
        public Dummy()
        {
        }

        public void transitions(RIOT host)
        {
        }

        public void ground(bool state)
        {
        }

        public void latch(bool state)
        {
        }

        public void clock(int cycles)
        {
        }

        public void poke(byte data)
        {
        }

        public byte peek()
        {
            return 0xF;
        }

        public byte pot1()
        {
            return 0x80;
        }

        public byte pot2()
        {
            return 0x80;
        }

        public byte fire()
        {
            return 0x80;
        }
    }
}
