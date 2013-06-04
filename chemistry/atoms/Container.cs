using System;
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

namespace cstari.chemistry.atoms
{
    public class Container : Atom
    {
        private List<Atom> filler;

        private Point internalOffset;

        public Container(Compound gui, Rectangle bounds, List<Atom> content)
            : base(gui,bounds,null)
        {
            filler = content;
        }

        public override bool allowFocus()
        {
            return false;
        }

        public List<Atom> getValue()
        {
            return filler;
        }

        public Point ContainerOffset
        {
            get { return internalOffset; }
            set { internalOffset = value; }
        }
        
        public void setValue(Atom a)
        {
            filler.Add(a);
        }

        public void setValue(List<Atom> atoms)
        {
            filler.AddRange(atoms);
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            // TODO: FORWARD TO INTERNAL WIDGETS

            return true;
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            // TODO: FORWARD TO INTERNAL WIDGETS

            return true;
        }

        public override bool doEvent(object caller, MouseMotionEventArgs e)
        {
            // TODO: FORWARD TO INTERNAL WIDGETS

            return true;
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            if (filler.Count <= 0)
                return;

            Rectangle container = Boundaries;

            surface.Fill(container, DisplaySettings.atomForeground);
            container.X += Compound.BorderPadding.Width;
            container.Y += Compound.BorderPadding.Height;
            container.Width += Compound.BorderPadding.Width*2;
            container.Height += Compound.BorderPadding.Height*2;
            surface.Fill(container, focused ? DisplaySettings.bodyFocused : DisplaySettings.bodyBlured);

            Rectangle oldClipper = surface.ClipRectangle;
            Rectangle newClipper;

            container.Intersect(oldClipper);

            foreach (Atom a in filler)
            {
                newClipper = a.Boundaries;

                newClipper.X += offset.X - internalOffset.X;
                newClipper.Y += offset.Y - internalOffset.Y;

                newClipper.Intersect(container);

                surface.ClipRectangle = newClipper;

                // TODO: ALLOW FOR FOCUS
                a.paint(surface, false, newClipper.Location);
            }

            surface.ClipRectangle = oldClipper;
        }
    }
}
