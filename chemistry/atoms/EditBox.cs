using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

public delegate void ChemistryEditCallback(string s);

namespace cstari.chemistry.atoms
{
    public class EditBox : Atom
    {
        private ChemistryEditCallback callbackHandler;
        private string editingValue;

        public EditBox(Compound gui, Rectangle bounds, string val, ChemistryEditCallback call)
            : base(gui, bounds, null)
        {
            callbackHandler = call;
            Value = val;
        }

        public override bool allowFocus()
        {
            return true;
        }

        public string Value
        {
            get { return editingValue; }
            set {
                if (value != null)
                    editingValue = value;
                else
                    editingValue = "";
            }
        }

        override public int Height
        {
            get
            {
                return DisplaySettings.captionFont.Height + Compound.BorderPadding.Height * 2;
            }
            set
            {
            }
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            Surface text = DisplaySettings.captionFont.Render(editingValue, DisplaySettings.captionColor);

            Rectangle r = new Rectangle(offset, new Size(Width,text.Height + Compound.BorderPadding.Height * 2));

            surface.Fill(r, DisplaySettings.atomForeground);
            r.X += Compound.BorderPadding.Width;
            r.Y += Compound.BorderPadding.Height;
            r.Width -= Compound.BorderPadding.Width*2;
            r.Height -= Compound.BorderPadding.Height * 2;
            surface.Fill(r, DisplaySettings.atomBackground);

            r.X += Compound.Padding;
            r.Width -= Compound.Padding * 2;

            Rectangle oldClipper = surface.ClipRectangle;
            surface.ClipRectangle = r;

            if (focused)
            {
                int startOfText;

                if( text.Width + Compound.Padding + Compound.Seperator > r.Width )
                {
                    startOfText = r.X + r.Width - text.Width - Compound.Seperator - Compound.Padding - 1;
                }
                else
                {
                    startOfText = r.X;
                }

                surface.Blit(text, new Point(startOfText, r.Y));

                if ((SdlDotNet.Core.Timer.SecondsElapsed % 2) == 0)
                {
                    r.X = startOfText + 1 + text.Width;
                    r.Width = Compound.Seperator;
                    r.Y += Compound.Padding;
                    r.Height -= Compound.Padding * 2;

                    surface.Fill(r, DisplaySettings.captionColor);
                }
            }
            else
            {
                surface.Blit(text, r.Location);
            }

            surface.ClipRectangle = oldClipper;
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (!e.Down)
                return true;

            if (e.Key == Key.Backspace && editingValue.Length > 0)
            {
                editingValue = editingValue.Substring(0, editingValue.Length - 1);
            }
            else
            {
                editingValue += Keyboard.UnicodeCharacter(e.Unicode);
            }

            if (callbackHandler != null)
            {
                callbackHandler(editingValue);
            }

            return true;
        }
    }
}
