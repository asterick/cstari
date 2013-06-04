using System;
using cstari.chips;

namespace cstari.input
{
    /// <summary>
    /// Summary description for controller.
    /// </summary>
    public interface Controller
    {
        void transitions(RIOT riot);
        void ground(bool state);
        void latch(bool state);
        void clock(int cycles);

        void poke(byte data);
        byte peek();

        byte pot1();
        byte pot2();
        byte fire();
    }
}
