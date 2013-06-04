using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Input;

using cstari.chemistry.captions;

public delegate void ChemistryButtonCallback();

namespace cstari.chemistry.atoms
{
    public class Button : Atom
    {
        private ChemistryButtonCallback callbackHandler;
        private bool pressed;

        public Button(Compound gui,Rectangle bounds, DisplayCaption atomCaption, ChemistryButtonCallback callback)
            : base(gui, bounds, atomCaption)
        {
            callbackHandler = callback;

            pressed = false;
        }

        override public bool allowFocus()
        {
            return true;
        }

        override public void paint(SdlDotNet.Graphics.Surface surface, bool focused, Point offset)
        {
            Caption.SizeWithIcon = false;

            Point dest = new Point((Width - Caption.Width) / 2 + offset.X,
                                    (Height - Caption.Height) / 2 + offset.Y);

            Rectangle rect = new Rectangle(offset, Dimensions);

            surface.Fill(rect, DisplaySettings.atomForeground);
            rect.Width -= Compound.BorderPadding.Width * 2;
            rect.Height -= Compound.BorderPadding.Height * 2;
            rect.X += Compound.BorderPadding.Width;
            rect.Y += Compound.BorderPadding.Height;
            surface.Fill(rect, pressed ? DisplaySettings.atomBackgroundDimmed : DisplaySettings.atomBackground);
            rect.Width -= Compound.BorderPadding.Width * 2;
            rect.Height -= Compound.BorderPadding.Height * 2;
            rect.X += Compound.BorderPadding.Width;
            rect.Y += Compound.BorderPadding.Height;

            Rectangle oldClipper = surface.ClipRectangle;
            surface.ClipRectangle = rect;

            Caption.Blit(surface, dest);

            surface.ClipRectangle = oldClipper;
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (callbackHandler == null)
                return true;

            if (e.ButtonPressed)
            {
                pressed = true;
            }
            else if( pressed )
            {
                pressed = false;

                if (e.X > 0 && e.X < Width &&
                    e.Y > 0 && e.Y < Height)
                {
                    callbackHandler();
                }
            }

            return true;
        }

        override public bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (callbackHandler == null || !e.Down)
                return true;

            if (e.Key == Key.Return || e.Key == Key.Space)
            {
                callbackHandler();
            }

            return true;
        }
    }
}
