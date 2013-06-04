using System;
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.captions;

public delegate void ChemistryListCallback(int selected);

namespace cstari.chemistry.atoms
{
    public class ListBox : Atom
    {
        private ChemistryListCallback callbackHandler;
        private Rectangle InnerRect;

        private int renderPosition;
        private int selected;
        private List<DisplayCaption> lables;
        private int listSpacing;

        public ListBox(Compound gui, Rectangle bounds, List<DisplayCaption> content, ChemistryListCallback call)
            : base(gui,bounds,null)
        {
            callbackHandler = call;

            InnerRect = bounds;
            InnerRect.Width -= Compound.Padding * 2 + Compound.BorderPadding.Width * 2;
            InnerRect.Height -= Compound.Padding * 2 + Compound.BorderPadding.Height * 2;

            List = content;
        }

        public override bool allowFocus()
        {
            return true;
        }

        public int Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public List<DisplayCaption> List
        {
            set
            {
                lables = value;
                
                selected = 0;
                renderPosition = 0;

                listSpacing = 0;
                foreach (DisplayCaption dc in lables)
                {
                    listSpacing = Math.Max(listSpacing, dc.Height);
                }
            }
            get
            {
                return lables;
            }
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            Point draw = offset;

            Rectangle target = Boundaries;

            target.Location = offset;
            InnerRect.Location = offset;
            InnerRect.X += Compound.Padding + Compound.BorderPadding.Width;
            InnerRect.Y += Compound.Padding + Compound.BorderPadding.Height;

            surface.Fill(target, DisplaySettings.atomForeground);
            target.X += Compound.BorderPadding.Width;
            target.Y += Compound.BorderPadding.Height;
            target.Width -= Compound.BorderPadding.Width * 2;
            target.Height -= Compound.BorderPadding.Height * 2;
            surface.Fill(target, DisplaySettings.atomBackground);

            Point p = InnerRect.Location;

            if (lables.Count > 0)
            {
                Rectangle oldClip = surface.ClipRectangle;
                surface.ClipRectangle = InnerRect;

                int maximumPosition = Math.Max(lables.Count * listSpacing - InnerRect.Height, 0);
                int targetPosition = Math.Min(maximumPosition, listSpacing * selected - (InnerRect.Height - listSpacing) / 2);

                if (targetPosition < 0)
                    targetPosition = 0;

                if (targetPosition > renderPosition)
                {
                    renderPosition += (int)Math.Ceiling((targetPosition - renderPosition) / 10.0);
                }
                else
                {
                    renderPosition += (int)Math.Floor((targetPosition - renderPosition) / 10.0);
                }

                p.Y -= renderPosition % listSpacing;

                int DrawStart = renderPosition / listSpacing;
                int DrawEnd = Math.Min(lables.Count - 1, InnerRect.Height / listSpacing + DrawStart + 1);

                for (int i = DrawStart; i <= DrawEnd; i++)
                {
                    if (i == selected )
                    {
                        Rectangle r = new Rectangle(
                            InnerRect.X,
                            p.Y,
                            InnerRect.Width,
                            listSpacing
                            );

                        surface.Fill(r, DisplaySettings.selectedColor);
                    }

                    lables[i].Blit(surface, p);
                    p.Y += listSpacing;
                }

                surface.ClipRectangle = oldClip;
            }
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (lables.Count <= 0 || !e.ButtonPressed)
                return true;

            Rectangle r = new Rectangle(
                Compound.Padding + Compound.BorderPadding.Width,
                Compound.Padding + Compound.BorderPadding.Height,
                Width - (Compound.Padding + Compound.BorderPadding.Width)*2,
                Height - (Compound.Padding + Compound.BorderPadding.Height)*2 );

            if( e.Y < r.Y || e.Y >= r.Y + r.Height ||
                e.X < r.X || e.X >= r.X + r.Width )
                return true;

            selected = (renderPosition + e.Y - r.Y) / listSpacing;

            if (selected >= lables.Count)
                selected = lables.Count - 1;
            if (selected < 0)
                selected = 0;

            if (callbackHandler != null)
                callbackHandler(selected);

            return true;
        }

        public override bool doEvent(object caller, KeyboardEventArgs kbData)
        {
            if (lables.Count <= 0)
                return true;

            if (!kbData.Down)
                return true;

            if (kbData.Unicode != 0)
            {
                int i = selected;

                do
                {
                    i = (i + 1) % lables.Count;

                    if (kbData.UnicodeCharacter == lables[i].Text.Substring(0, 1).ToLower())
                    {
                        selected = i;

                        if (callbackHandler != null)
                            callbackHandler(selected);

                        return true;
                    }
                } while (i != selected);
            }
            
            switch (kbData.Key)
            {
                case Key.UpArrow:
                    if (selected > 0)
                        selected--;
                    break;
                case Key.DownArrow:
                    if (selected < lables.Count - 1)
                        selected++;
                    break;
                case Key.PageUp:
                    selected -= InnerRect.Height / listSpacing;
                    if (selected < 0)
                        selected = 0;
                    break;
                case Key.PageDown:
                    selected += InnerRect.Height / listSpacing;
                    if (selected >= lables.Count)
                        selected = lables.Count - 1;
                    break;
                case Key.Home:
                    selected = 0;
                    break;
                case Key.End:
                    selected = lables.Count - 1;
                    break;
                default:
                    return true;

            }

            if (callbackHandler != null)
                callbackHandler(selected);

            return true;
        }
    }
}
