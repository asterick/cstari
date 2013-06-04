using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet.Input;
using cstari.chemistry.captions;

namespace cstari.chemistry.atoms
{
    public class Dock : Atom
    {
        private List<Atom> docked;

        private int decompressedSize;
        private Atom selectedAtom;

        public Dock(Compound gui)
            : base(gui, new Rectangle(), null)
        {
            docked = new List<Atom>();
            selectedAtom = null;
            decompressedSize = 0;
        }

        public List<Atom> DockedItems
        {
            get
            {
                return docked;
            }
        }

        public void Add(Atom m)
        {
            docked.Add(m);
        }

        public void Add(List<Atom> m)
        {
            docked.InsertRange(0,m);
        }

        public void Remove(Atom m)
        {
            docked.Remove(m);
        }

        public override bool allowFocus()
        {
            return false;
        }

        public override int Width
        {
            get
            {
                int size = 0;

                foreach (Atom a in docked)
                {
                    DisplayCaption dc;
                    if (a == null)
                    {
                        dc = DisplaySettings.defaultCaption;
                    }
                    else
                    {
                        dc = a.Caption;
                        dc.UseDefaultIcon = true;
                    }

                    if (selectedAtom != a)
                    {
                        size += dc.Icon.Width + Compound.Padding;
                    }
                }

                return size + Compound.IconSize.Width + decompressedSize;
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
                int size = 0;

                foreach (Atom a in docked)
                {
                    DisplayCaption dc;
                    if (a == null)
                    {
                        dc = DisplaySettings.defaultCaption;
                    }
                    else
                    {
                        dc = a.Caption;
                        dc.UseDefaultIcon = true;
                    }

                    size = Math.Max( dc.Height,size );
                }

                return size;
            }
            set
            {
                base.Height = value;
            }
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (e.ButtonPressed && selectedAtom != null && caller is Compound)
            {
                Compound gui = (Compound)caller;
                
                docked.Remove(selectedAtom);
                gui.Add(selectedAtom, false);

                selectedAtom = null;
                decompressedSize = 0;
            }

            return true;
        }

        public override bool doEvent(object caller, MouseMotionEventArgs e)
        {
            if (e.X < 0 || e.X > Width ||
                e.Y < 0 || e.Y > Height)
            {
                selectedAtom = null;
                decompressedSize = 0;
                return false;
            }

            Point p = new Point(0, 0);

            foreach (Atom a in docked)
            {
                DisplayCaption dc;

                if (a == null)
                {
                    dc = DisplaySettings.defaultCaption;
                }
                else
                {
                    dc = a.Caption;
                    dc.UseDefaultIcon = true;
                }

                if (selectedAtom != a)
                {
                    p.X += dc.Icon.Width + Compound.Padding;
                }
                else
                {
                    p.X += dc.Width + Compound.Padding;
                }

                if (e.X < p.X)
                {
                    if (selectedAtom != a)
                    {
                        selectedAtom = a;
                        decompressedSize = a.Caption.Icon.Width;
                    }
                    return true;
                }
            }

            selectedAtom = null;
            decompressedSize = 0;

            return true;
        }

        public override void paint(SdlDotNet.Graphics.Surface surface, bool focused, Point offset)
        {
            Point p = offset;

            foreach (Atom a in docked)
            {
                DisplayCaption dc;
                if (a == null)
                {
                    dc = DisplaySettings.defaultCaption;
                }
                else
                {
                    dc = a.Caption;
                    dc.UseDefaultIcon = true;
                }

                if (selectedAtom != a)
                {
                    DisplayIcon i = dc.Icon;

                    p.Y = (Height - i.Height) / 2 + offset.Y;
                    i.Blit(surface, p);

                    p.X += i.Width + Compound.Padding;
                }
                else
                {
                    Rectangle oldClipper = surface.ClipRectangle;
                    Rectangle newClipper = new Rectangle(p, new Size(decompressedSize, dc.Height));

                    newClipper.Intersect(oldClipper);
                    surface.ClipRectangle = newClipper;

                    p.Y = (Height - dc.Height) / 2 + offset.Y;
                    dc.Blit( surface, p );

                    surface.ClipRectangle = oldClipper;
                    p.X += decompressedSize + Compound.Padding;

                    if (decompressedSize < dc.Width)
                    {
                        decompressedSize++;
                    }
                }
            }
        }
    }
}
