using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

using cstari.chips;
using cstari.chemistry;
using cstari.utility;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

namespace cstari.ui
{
	class Television : Atom
	{
        public Atari atari;
        private Surface display;

        private cstari.input.Joystick j;

        public Television(Compound gui, Point location, Mapper cart, SignalType s)
            : base(gui, new Rectangle(location, new Size(320,264)),null)
        {
            atari = new Atari(cart);
            atari.SetSignal(s);

            j = new cstari.input.Joystick();

            atari.plug(j, null);

            display = new Surface(320, 264);
        }

        public override bool allowFocus()
        {
            return true;
        }

        // Volitile atom, always dirty
        public override bool Dirty
        {
            get { return true; }
        }

        public override int Height
        {
            get
            {
                return atari.Display.Height;
            }
        }

        public override int Width
        {
            get
            {
                return atari.Display.Width;
            }
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    if (e.Down)
                        atari.m_RIOT.AlterPanel(0, 0, RIOT.PanelButtons.Reset);
                    else
                        atari.m_RIOT.AlterPanel(0, RIOT.PanelButtons.Reset, 0);
                    break;
                case Key.F2:
                    if (e.Down)
                        atari.m_RIOT.AlterPanel(0, 0, RIOT.PanelButtons.Select);
                    else
                        atari.m_RIOT.AlterPanel(0, RIOT.PanelButtons.Select, 0);
                    break;
                case Key.F4:
                    if (e.Down)
                        atari.m_RIOT.AlterPanel(RIOT.PanelButtons.Color, 0, 0);
                    break;
                case Key.F5:
                    if (e.Down)
                        atari.m_RIOT.AlterPanel(RIOT.PanelButtons.DifficultyP0, 0, 0);
                    break;
                case Key.F6:
                    if (e.Down)
                        atari.m_RIOT.AlterPanel(RIOT.PanelButtons.DifficultyP1, 0, 0);
                    break;
                default:
                    j.eventDriver(e);
                    break;
            }

            return true;
        }

        public override void paint(SdlDotNet.Graphics.Surface surface, bool focused, System.Drawing.Point offset)
        {
            display.Blit(atari.Display);
            surface.Blit(display, offset);

            atari.execute();
        }

        public void run()
        {
            atari.run();
        }

        public void stop()
        {
            atari.stop();
        }

        public void reset()
        {
            atari.reset();
        }

        public void disasm()
        {
            gui.Add(new Disassembler(gui, atari),true);
        }

        public void tiaview()
        {
            gui.Add(new TiaDebugger(gui, atari.m_TIA), true);
        }
    }
}
