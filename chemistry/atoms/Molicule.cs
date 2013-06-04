using System;
using System.Collections.Generic;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.captions;
using cstari.chemistry.menu;

namespace cstari.chemistry.atoms
{
    public class Molicule : Atom
    {
        private List<Atom> windowAtoms;

        private Atom activeAtom;
        private Atom draggingAtom;
        private MenuBar menu;
        private bool dragging;

        public Molicule(Compound gui, Rectangle r, DisplayCaption cap, List<Atom> atoms)
            : base(gui, r, cap)
        {
            windowAtoms = atoms;
            draggingAtom = null;
            dragging = false;
            menu = null;

            if (atoms.Count > 0)
                activeAtom = atoms[atoms.Count-1];
            else
                activeAtom = null;
        }

        public void SetMenu(MenuBar m)
        {
            menu = m;
        }
        
        protected void Add(Atom m)
        {
            windowAtoms.Add(m);
        }

        protected void Remove(Atom m)
        {
            windowAtoms.Remove(m);
        }

        public void Focus(Atom m)
        {
            if (windowAtoms.Contains(m))
            {
                windowAtoms.Remove(m);
                windowAtoms.Add(m);
                activeAtom = m;
            }
        }

        public Rectangle ClientRect
        {
            get
            {
                Rectangle target = new Rectangle(
                                Compound.BorderPadding.Width,
                                Compound.BorderPadding.Height + Caption.Height,
                                Width - Compound.BorderPadding.Width * 2,
                                Height - Compound.BorderPadding.Height * 3 - Caption.Height);

                if (menu != null)
                {
                    target.Y += menu.Height;
                    target.Height -= menu.Height;
                }

                target.X += Compound.Padding;
                target.Y += Compound.Padding + Compound.Padding;
                target.Width -= Compound.Padding * 2;
                target.Height -= Compound.Padding * 2;

                return target;
            }
        }

        public Size CalculateSize(Size s)
        {
            s.Width += Compound.BorderPadding.Width * 2 + Compound.Padding * 3;
            s.Height += Compound.BorderPadding.Height * 2 + Caption.Height + Compound.Padding * 2;

            if (menu != null)
            {
                s.Height += menu.Height;
            }

            return s;
        }

        override public bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (e.Down == true)
            {
                if (windowAtoms.Count > 0 && e.Key == Key.Tab)
                {
                    for (int i = 0; i < windowAtoms.Count; i++)
                    {
                        if (windowAtoms[i].allowFocus())
                        {
                            Focus(windowAtoms[i]);
                            return true;
                        }
                    }
                }
            }

            if( activeAtom != null )
                return activeAtom.doEvent(this, e);

            return true;
        }

        override public bool doEvent(object caller, MouseButtonEventArgs e)
        {
            if (e.ButtonPressed && e.Button == MouseButton.PrimaryButton)
            {
                Rectangle titleBar = new Rectangle(
                    Compound.BorderPadding.Width,
                    Compound.BorderPadding.Height,
                    Width - Compound.BorderPadding.Width * 2,
                    Caption.Height + Compound.Padding - Compound.BorderPadding.Height );

                if (e.X >= titleBar.X && e.X < titleBar.X + titleBar.Width &&
                    e.Y >= titleBar.Y && e.Y < titleBar.Y + titleBar.Height)
                {
                    Point p = titleBar.Location;

                    p.X += titleBar.Width - DisplaySettings.minimizeIcon.Width -
                        DisplaySettings.closeIcon.Width - Compound.Padding;

                    if (e.X >= p.X && e.X < p.X + DisplaySettings.minimizeIcon.Width && caller is Compound)
                    {
                        Compound gui = (Compound)caller;

                        gui.Minimize(this);
                        return true;
                    }

                    p.X += DisplaySettings.minimizeIcon.Width + Compound.Padding;

                    if (e.X >= p.X && e.X < p.X + DisplaySettings.closeIcon.Width && caller is Compound)
                    {
                        Compound gui = (Compound)caller;

                        gui.Remove(this);
                        return true;
                    }

                    dragging = true;
                }
            }
            else
            {
                dragging = false;
            }

            Rectangle target = new Rectangle(
                Compound.BorderPadding.Width,
                Compound.BorderPadding.Height + Caption.Height,
                Width - Compound.BorderPadding.Width * 2,
                Height - Compound.BorderPadding.Height * 3 - Caption.Height);


            if (menu != null)
            {
                if (menu.doEvent(this, e))
                    return true;

                target.Y += menu.Height;
                target.Height -= menu.Height;
            }

            target.X += Compound.Padding;
            target.Y += Compound.Padding*2;
            target.Width -= Compound.Padding * 2;
            target.Height -= Compound.Padding * 2;

            if (draggingAtom != null)
            {
                MouseButtonEventArgs te = new MouseButtonEventArgs(
                    e.Button,
                    e.ButtonPressed,
                    (short)(e.X - draggingAtom.Position.X - target.X),
                    (short)(e.Y - draggingAtom.Position.Y - target.Y));

                draggingAtom.doEvent(this, te);
            }

            if (e.X < target.X || e.X >= target.X + target.Width ||
                e.Y < target.Y || e.Y >= target.Y + target.Height)
                return true;

            if (e.Button == MouseButton.PrimaryButton && !e.ButtonPressed)
            {
                draggingAtom = null;
            }

            for (int i = windowAtoms.Count - 1; i >= 0; i--)
            {
                Rectangle r = windowAtoms[i].Boundaries;

                r.X += target.X;
                r.Y += target.Y;

                if (e.X < r.X || e.X >= r.X + r.Width ||
                    e.Y < r.Y || e.Y >= r.Y + r.Height)
                {
                    continue;
                }

                if (e.Button == MouseButton.PrimaryButton && e.ButtonPressed)
                {
                    draggingAtom = windowAtoms[i];
                }

                MouseButtonEventArgs te = new MouseButtonEventArgs(
                    e.Button,
                    e.ButtonPressed,
                    (short)(e.X - r.X),
                    (short)(e.Y - r.Y));

                Atom wa = windowAtoms[i];

                wa.doEvent(this, te);
                Focus(wa);

                return true;
            }

            return true;
        }        

        override public bool doEvent(object caller, MouseMotionEventArgs e)
        {
            if (dragging)
            {
                this.X += e.RelativeX;
                this.Y += e.RelativeY;

                return true;
            }

            Rectangle target = new Rectangle(
                Compound.BorderPadding.Width,
                Compound.BorderPadding.Height + Caption.Height,
                Width - Compound.BorderPadding.Width * 2,
                Height - Compound.BorderPadding.Height * 3 - Caption.Height);

            if (menu != null)
            {
                Point p = this.Position;

                MouseMotionEventArgs te = new MouseMotionEventArgs(
                    e.ButtonPressed,
                    e.Button,
                    (short)(e.X + this.X),
                    (short)(e.Y + this.Y),
                    e.RelativeX,
                    e.RelativeY);

                p.X += target.X;
                p.Y += target.Y;

                if (menu.doEvent(this, te, p))
                    return true;

                target.Y += menu.Height;
                target.Height -= menu.Height;
            }

            target.X += Compound.Padding;
            target.Y += Compound.Padding * 2;
            target.Width -= Compound.Padding * 2;
            target.Height -= Compound.Padding * 2;

            if (draggingAtom != null)
            {
                MouseMotionEventArgs te = new MouseMotionEventArgs(
                    e.ButtonPressed,
                    e.Button,
                    (short)(e.X - draggingAtom.Position.X - target.X),
                    (short)(e.Y - draggingAtom.Position.Y - target.Y),
                    e.RelativeX,
                    e.RelativeY);

                draggingAtom.doEvent(this, te);
            }


            if (e.X < target.X || e.X >= target.X + target.Width ||
                e.Y < target.Y || e.Y >= target.Y + target.Height)
                return true;

            for (int i = windowAtoms.Count - 1; i >= 0; i--)
            {
                Rectangle r = windowAtoms[i].Boundaries;

                r.X += target.X;
                r.Y += target.Y;

                if (e.X < r.X || e.X >= r.X + r.Width ||
                    e.Y < r.Y || e.Y >= r.Y + r.Height)
                {
                    continue;
                }

                if (windowAtoms[i] == draggingAtom)
                {
                    break;
                }

                MouseMotionEventArgs te = new MouseMotionEventArgs(
                    e.ButtonPressed,
                    e.Button,
                    (short)(e.X - r.X),
                    (short)(e.Y - r.Y),
                    e.RelativeX,
                    e.RelativeY);

                windowAtoms[i].doEvent(this, te);

                return true;
            }

            return true;
        }

        public override bool allowFocus()
        {
            return true;
        }

        public override void paint(Surface display, bool focused, Point offset)
        {
            Rectangle target = new Rectangle( offset, Dimensions );
            Rectangle titleBar = target;

            titleBar.Width = titleBar.Width;
            titleBar.Height = Caption.Height + Compound.Padding;
            target.Width--;
            target.Height--;

            Caption.UseDefaultIcon = true;
            
            Rectangle windowFiller = target;

            display.Fill(target, focused ? DisplaySettings.bodyBorderFocused : DisplaySettings.bodyBorderBlured);

            target.Width -= Compound.BorderPadding.Width * 2;
            target.Height -= Compound.BorderPadding.Height * 2;
            target.X = offset.X + Compound.BorderPadding.Width;
            target.Y = offset.Y + Compound.BorderPadding.Height;

            display.Fill(target, focused ? DisplaySettings.bodyFocused : DisplaySettings.bodyBlured);

            display.Fill(titleBar, focused ? DisplaySettings.bodyBorderFocused : DisplaySettings.bodyBorderBlured);
            titleBar.Width -= Compound.BorderPadding.Width * 2;
            titleBar.Height -= Compound.BorderPadding.Height * 2;
            titleBar.X += Compound.BorderPadding.Width;
            titleBar.Y += Compound.BorderPadding.Height;
            display.Fill(titleBar, focused ? DisplaySettings.titleFocused : DisplaySettings.titleBlured);

            Point p = new Point(titleBar.X+Compound.Padding, titleBar.Y);
            p.Y += (titleBar.Height - Math.Max(DisplaySettings.minimizeIcon.Height, DisplaySettings.closeIcon.Height)) / 2;
            p.X = titleBar.X + titleBar.Width - DisplaySettings.minimizeIcon.Width - DisplaySettings.closeIcon.Width - Compound.Padding;

            Rectangle oldClipper = display.ClipRectangle;
            Rectangle newClipper = new Rectangle(
                titleBar.X + Compound.Padding,
                titleBar.Y,
                p.X - titleBar.X - Compound.Padding*2,
                titleBar.Height);

            display.ClipRectangle = newClipper;
            Caption.Blit(display, newClipper.Location);
            display.ClipRectangle = oldClipper;

            DisplaySettings.minimizeIcon.Blit(display, p);
            p.X += DisplaySettings.minimizeIcon.Width + Compound.Padding;
            DisplaySettings.closeIcon.Blit(display, p);
            
            newClipper = new Rectangle(new Point(target.X,
                target.Y + Caption.Height),
                new Size(target.Width,
                target.Height - Compound.BorderPadding.Height - Caption.Height));

            Rectangle menuRect = newClipper;

            if (menu != null)
            {
                newClipper.Y += menu.Height;
                newClipper.Height -= menu.Height;
            }

            newClipper.X += Compound.Padding;
            newClipper.Y += Compound.Padding *2 ;
            newClipper.Width -= Compound.Padding * 2;
            newClipper.Height -= Compound.Padding * 2;

            if (activeAtom != null && activeAtom.allowFocus())
            {
                p = newClipper.Location;

                Rectangle widgetBox = activeAtom.Boundaries;
                widgetBox.X += p.X;
                widgetBox.Y += p.Y;

                widgetBox.Inflate(Compound.BorderPadding);
                display.Fill(widgetBox, DisplaySettings.focused);
            }

            display.ClipRectangle = newClipper;

            foreach (Atom a in windowAtoms)
            {
                p = newClipper.Location;

                Rectangle widgetClip = a.Boundaries;
                widgetClip.X += p.X;
                widgetClip.Y += p.Y;
                widgetClip.Width++;
                widgetClip.Height++;

                widgetClip.Intersect(newClipper);
                display.ClipRectangle = widgetClip;

                a.paint(display, a == activeAtom && focused, widgetClip.Location);
            }

            display.ClipRectangle = oldClipper;

            if (menu != null)
                menu.paint(display, menuRect);
        }
    }
}
