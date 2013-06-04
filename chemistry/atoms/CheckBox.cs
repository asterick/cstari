using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Input;
using cstari.chemistry.captions;

public delegate void ChemistryCheckboxCallback(bool check);

namespace cstari.chemistry.atoms
{
    public class CheckBox : Atom
    {
        private ChemistryCheckboxCallback callbackHandler;
        private bool boxChecked;

        public CheckBox(Compound gui, Rectangle bounds, DisplayCaption dc, bool atomChecked, ChemistryCheckboxCallback callback)
            : base(gui,bounds,dc)
        {
            callbackHandler = callback;
            boxChecked = atomChecked;
        }

        public override bool allowFocus()
        {
            return true;
        }

        public bool getValue()
        {
            return boxChecked;
        }

        public void setValue(bool b)
        {
            boxChecked = b;
        }

        public override int Height
        {
            get
            {
                DisplayIcon box = boxChecked ? DisplaySettings.checkChecked : DisplaySettings.checkUnchecked;
              
                return Math.Min( box.Height, Caption.Height );
            }
            set
            {
                base.Height = value;
            }
        }

        override public void paint(SdlDotNet.Graphics.Surface surface, bool focused, Point offset)
        {
            Point p = offset;

            Caption.SizeWithIcon = false;

            DisplayIcon box = boxChecked ? DisplaySettings.checkChecked : DisplaySettings.checkUnchecked;

            if (Caption.Height > box.Height)
            {
                p.Y += (Caption.Height - box.Height) / 2;

                box.Blit(surface, p);

                p.X += box.Width + Compound.Padding;
                p.Y = offset.Y;

                Caption.Blit(surface, p);
            }
            else
            {

                box.Blit(surface, p);

                p.X += box.Width + Compound.Padding;
                p.Y += (box.Height - Caption.Height) / 2;

                Caption.Blit(surface, p);
            }
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (!e.Down)
                return true;

            if (e.Key == Key.Return || e.Key == Key.Space)
            {
                boxChecked = !boxChecked;

                if( callbackHandler != null )
                    callbackHandler(boxChecked);
            }

            return true;
        }

        override public bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (!e.ButtonPressed)
                return true;

            DisplayIcon box = boxChecked ? DisplaySettings.checkChecked : DisplaySettings.checkUnchecked;
            Rectangle hit = new Rectangle(new Point(0, (Caption.Height - box.Height) / 2), box.Dimensions);

            if (e.X < hit.X || e.X > hit.Y + hit.Width ||
                e.Y < hit.Y || e.Y > hit.Y + hit.Height)
                return true;

            boxChecked = !boxChecked;

            if (callbackHandler != null)
                callbackHandler(boxChecked);

            return true;
        }
    }
}
