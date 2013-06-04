using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

public delegate void ChemistrySpinnerCallback(int val);

namespace cstari.chemistry.atoms
{
    public class Spinner : Atom
    {
        private ChemistrySpinnerCallback callbackHandler;
        private int selection;
        private Range range;

        public Spinner(Compound gui, Rectangle bounds, Range r, ChemistrySpinnerCallback call)
            : base(gui, bounds, null)
        {
            selection = 0;            
            Limits = r;
            callbackHandler = call;
        }

        public Range Limits
        {
            get { return range; }
            set { 
                range = value;
                selection = range.Limit(selection);
            }
        }

        public int Value
        {
            get { return selection; }
            set
            {
                selection = range.Limit(value);
            }
        }

        public override int Height
        {
            get
            {
                return DisplaySettings.spinnerArrows.Height;
            }
            set
            {
                base.Height = value;
            }
        }

        public override bool allowFocus()
        {
            return true;
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            Rectangle r = new Rectangle(offset, new Size(Width, DisplaySettings.spinnerArrows.Height));

            surface.Fill(r, DisplaySettings.atomForeground);
            r.X += Compound.BorderPadding.Width;
            r.Y += Compound.BorderPadding.Height;
            r.Width -= Compound.BorderPadding.Width * 2;
            r.Height -= Compound.BorderPadding.Height * 2;
            surface.Fill(r, DisplaySettings.atomBackground);
            DisplaySettings.spinnerArrows.Blit(surface, offset);

            r.Width -= Compound.Padding;

            Surface text = DisplaySettings.captionFont.Render(selection.ToString(), DisplaySettings.captionColor);

            Point p = offset;
            p.X += Compound.Padding + DisplaySettings.spinnerArrows.Width;
            p.Y += (DisplaySettings.spinnerArrows.Height - text.Height) / 2;

            Rectangle oldClipper = surface.ClipRectangle;
            
            surface.ClipRectangle = r;
            surface.Blit(text, p);
            surface.ClipRectangle = oldClipper;
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (e.Down)
            {
                switch (e.Key)
                {
                    case Key.UpArrow:
                        selection = range.Limit(selection + 1);
                        break;
                    case Key.DownArrow:
                        selection = range.Limit(selection - 1);
                        break;
                    case Key.Home:
                        selection = range.Min;
                        break;
                    case Key.End:
                        selection = range.Min;
                        break;
                }

                if (callbackHandler != null)
                    callbackHandler(selection);
            }
            return true;
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (e.X < DisplaySettings.spinnerArrows.Width && e.ButtonPressed)
            {
                if (e.Y < DisplaySettings.spinnerArrows.Height / 2)
                {
                    selection = range.Limit(selection + 1);
                }
                else
                {
                    selection = range.Limit(selection - 1);
                }
                
                if( callbackHandler != null )
                    callbackHandler(selection);
            }
            return true;
        }
    }
}
