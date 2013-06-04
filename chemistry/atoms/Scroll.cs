using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry;

public delegate void ChemistryScrollCallback(int val);

namespace cstari.chemistry.atoms
{
    public class Scroll : Atom
    {
        private ChemistryScrollCallback callbackHandler;

        private Range range;
        private int selection;
        private bool dragging;

        public Scroll(Compound gui, Rectangle bounds, Range r, ChemistryScrollCallback call)
            : base(gui,bounds,null)
        {
            selection = 0;
            Limits = r;
            callbackHandler = call;

            dragging = false;
        }

        public Range Limits
        {
            get { return range; }
            set
            {
                range = value;
                selection = range.Limit(selection);
            }
        }

        public int Min
        {
            get { return range.Min; }
            set
            {
                range.Min = value;
                selection = range.Limit(selection);
            }
        }

        public int Max
        {
            get { return range.Max; }
            set
            {
                range.Max = value;
                selection = range.Limit(selection);
            }
        }

        public int Value
        {
            get { return selection; }
            set
            {
                selection = value;
                selection = range.Limit(selection);
            }
        }

        public override int Width
        {
            get
            {
                if (base.Width < base.Height)
                {
                    return DisplaySettings.scrollUp.Width;
                }
                return base.Width;
            }
            set
            {
                base.Width = value;
            }
        }

        public override int Height
        {
            get
            {
                if (base.Width > base.Height)
                {
                    return DisplaySettings.scrollLeft.Height;
                }
                return base.Height;
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
            if (Width >= Height)
            {
                // HORIZONTAL SCROLL BAR

                Rectangle r = new Rectangle(offset, new Size(Width, DisplaySettings.scrollLeft.Height));

                surface.Fill(r, DisplaySettings.atomForeground);
                r.X += Compound.BorderPadding.Width;
                r.Y += Compound.BorderPadding.Height;
                r.Width -= Compound.BorderPadding.Width * 2;
                r.Height -= Compound.BorderPadding.Height * 2;
                surface.Fill(r, DisplaySettings.atomBackground);

                Point p = new Point(offset.X + Width - DisplaySettings.scrollRight.Width, offset.Y);
                DisplaySettings.scrollLeft.Blit(surface, offset);
                DisplaySettings.scrollRight.Blit(surface, p);

                int AvailableSpace = Width - (DisplaySettings.scrollLeft.Width +
                    DisplaySettings.scrollRight.Width +
                    DisplaySettings.scrollMarker.Width);

                p = new Point(
                        (int)(AvailableSpace * range.RelativeLocation(selection))
                        + offset.X + DisplaySettings.scrollLeft.Width,
                        r.Y + (r.Height - DisplaySettings.scrollMarker.Height) / 2
                    );

                DisplaySettings.scrollMarker.Blit(surface, p);
            }
            else
            {
                // VERTICAL SCROLL BAR
                Rectangle r = new Rectangle(offset, new Size(DisplaySettings.scrollUp.Width, Height));

                surface.Fill(r, DisplaySettings.atomForeground);
                r.X += Compound.BorderPadding.Width;
                r.Y += Compound.BorderPadding.Height;
                r.Width -= Compound.BorderPadding.Width * 2;
                r.Height -= Compound.BorderPadding.Height * 2;
                surface.Fill(r, DisplaySettings.atomBackground);

                Point p = new Point(offset.X, offset.Y + Height - DisplaySettings.scrollDown.Height);
                DisplaySettings.scrollUp.Blit(surface, offset);
                DisplaySettings.scrollDown.Blit(surface, p);

                int AvailableSpace = Height - (DisplaySettings.scrollUp.Height +
                    DisplaySettings.scrollDown.Height +
                    DisplaySettings.scrollMarker.Height);

                p = new Point(
                        r.X + (r.Width - DisplaySettings.scrollMarker.Width) / 2,
                        (int)(AvailableSpace * range.RelativeLocation(selection))
                        + offset.Y + DisplaySettings.scrollUp.Height
                    );

                DisplaySettings.scrollMarker.Blit(surface, p);
            }
        }

        public override bool doEvent(object caller, KeyboardEventArgs keyboardArgs)
        {
            if (keyboardArgs.Down == false)
                return true;

            switch (keyboardArgs.Key)
            {
                case Key.RightArrow:
                    selection = range.Limit(selection + 1);
                    break;
                case Key.DownArrow:
                    selection = range.Limit(selection + 1);
                    break;
                case Key.LeftArrow:
                    selection = range.Limit(selection - 1);
                    break;
                case Key.UpArrow:
                    selection = range.Limit(selection - 1);
                    break;
                case Key.Home:
                    selection = range.Min;
                    break;
                case Key.End:
                    selection = range.Max;
                    break;
            }

            if (callbackHandler != null)
                callbackHandler(selection);

            return true;
         }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {

            if (!e.ButtonPressed)
            {
                dragging = false;
            }
            else if (Width >= Height)
            {
                // HORIZONTAL SCROLL BAR
                if (e.X < DisplaySettings.scrollLeft.Width)
                {
                    selection = range.Limit(selection - 1);
                }
                else if (e.X > Width - DisplaySettings.scrollRight.Width)
                {
                    selection = range.Limit(selection + 1);
                }
                else
                {
                    dragging = true;
                    return true;
                }
            }
            else
            {
                // HORIZONTAL SCROLL BAR
                if (e.Y < DisplaySettings.scrollUp.Height)
                {
                    selection = range.Limit(selection - 1);
                }
                else if (e.Y > Height - DisplaySettings.scrollDown.Height)
                {
                    selection = range.Limit(selection + 1);
                }
                else
                {
                    dragging = true;
                    return true;
                }
            }

            if (callbackHandler != null)
                callbackHandler(selection);

            return true;
        }

        public override bool doEvent(object caller, MouseMotionEventArgs e)
        {
            if (dragging)
            {
                float pos;
                if (Width >= Height)
                {
                    pos = (float)(e.X - DisplaySettings.scrollLeft.Width - DisplaySettings.scrollMarker.Width / 2) /
                        (float)(Width - DisplaySettings.scrollLeft.Width - DisplaySettings.scrollRight.Width - DisplaySettings.scrollMarker.Width);
                }
                else
                {
                    pos = (float)(e.Y - DisplaySettings.scrollUp.Height - DisplaySettings.scrollMarker.Height / 2) /
                        (float)(Height - DisplaySettings.scrollUp.Height - DisplaySettings.scrollDown.Height - DisplaySettings.scrollMarker.Height);
                }

                selection = range.Limit((int)(pos * (range.Max - range.Min) + range.Min));

                if (callbackHandler != null)
                    callbackHandler(selection);
            }

            return true;
        }
    }
}
