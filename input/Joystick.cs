using System;
using cstari.input;
using cstari.chips;
using SdlDotNet.Input;

namespace cstari.input
{
    /// <summary>
    /// Summary description for dummy.
    /// </summary>
    public class Joystick : Controller
    {
        private byte direction;
        private bool firing;

        public Joystick()
        {
            direction = 0xF;
            firing = false;
        }

        public void eventDriver(KeyboardEventArgs e)
        {
            byte mask;
            switch (e.Key)
            {
                case Key.UpArrow:
                    mask = 1;
                    break;
                case Key.DownArrow:
                    mask = 2;
                    break;
                case Key.LeftArrow:
                    mask = 4;
                    break;
                case Key.RightArrow:
                    mask = 8;
                    break;
                case Key.LeftShift:
                    firing = e.Down;
                    return ;
                default:
                    return;
            }

            if (e.Down)
                direction &= (byte)~mask;
            else
                direction |= mask;
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
            return direction;
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
            return firing ? (byte)0 : (byte)0x80;
        }
    }
}
