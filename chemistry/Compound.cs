/*******************************************************************************
**
**  PYTARI - Atari 2600 emulation in python
**
**  Copyright (c) 2000-2005 by Bryon Vandiver
**
**  See the file "license" for information on usage and redistribution of
**  this file, and for a DISCLAIMER OF ALL WARRANTIES.
**
********************************************************************************
**
**   Chemistry:  A Micro GUI designed for use with pygame
**
**   Chemistry was designed for use with pytari, although
**   it is self contained and can be exported for use with
**   any project.
**   
**   On a side note, this is infact inspired by MacOS (TM),
**   as well as Amiga Workbench (TM). I, however, own
**   neither system.
**
**   Currently, the code is very messy, and unoptomized.
**   I"m writting this with a nasty case of the flu, and
**   like any code monkey, I would rather sit at my terminal
**   than get the much needed bed rest that is required
**
**   Optomizations and Clean ups will be done in parallel
**   development of the GUI, for things I think needs to
**   be revised, look in the known issues section
**
**   Current Atoms
**       Button  List    Lable   Image
**       Edit    Radio   Check   Scroll
**       Spinner
**
**   Notes:
**   o   Mouse event positions are relative to the location the
**       atom was when the MOUSEBUTTONDOWN event occured
**
**   KNOWN ISSUES:
**   o   The focusing window with mouse click has issues
**   o   Need a ClientRect -> WindowSize function
**   o   Need to Move Window Size and Pos properties to diffrent dict keys
**   o   NEED TO COMMENT LOCATIONS OF MAGIC NUMBERS
**   o   Need to remove MAGIC NUMBERS
**   o   There are a lot of order dependant functions
**   o   I would like to see the code a little more modular, as in
**       breaking down the event and paint loops some more, it is
**       currently a tad more bulky than I would like
**
*******************************************************************************/

using System;
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.menu;
using cstari.chemistry.atoms;
using cstari.chemistry.captions;

namespace cstari.chemistry
{
    public enum Alignment
    {
        TOP_LEFT = 0x0101,
        TOP = 0x0001,
        TOP_RIGHT = 0x1001,
        LEFT = 0x0100,
        CENTER = 0x0000,
        RIGHT = 0x1000,
        BOTTOM_LEFT = 0x0110,
        BOTTOM = 0x0010,
        BOTTOM_RIGHT = 0x1010
    }

    public class Compound
    {
        public static Size IconSize = new Size(16, 16);
        public static Size BorderPadding = new Size(2, 2);
        public static int Seperator = 1;
        public static int Padding = 4;

        private static Point SpawnDefault = new Point(32, 32);

        private List<Atom> desktopAtoms;
        private Surface displaySurface;
        private MenuBar menu;
        private Dock dock;
        private Atom activeAtom;
        private Point SpawnPoint;

        private Atom draggingAtom;

        public Compound(Surface surface)
        {
            displaySurface = surface;
            activeAtom = null;
            desktopAtoms = new List<Atom>();
            SpawnPoint = SpawnDefault;
            menu = null;
            draggingAtom = null;
            dock = null;

            /*
            self.hidden = false
            self.iconified = []
            self.selectedIcon = None
            self.selectedShown = 0
             */
        }

        public void Add(Atom m, bool AutoPosition)
        {
            activeAtom = m;

            if( AutoPosition )
            {
                Size s = m.Dimensions;

                if (s.Width + SpawnPoint.X > displaySurface.Width ||
                    s.Height + SpawnPoint.Y > displaySurface.Height)
                {
                    SpawnPoint = SpawnDefault;
                }

                m.Position = SpawnPoint;

                SpawnPoint.X += Compound.Padding * 3;
                SpawnPoint.Y += Compound.Padding * 3;
            }

            desktopAtoms.Add(m);
        }

        public void Remove(Atom m)
        {
            desktopAtoms.Remove(m);

            if (activeAtom == m)
            {
                if (desktopAtoms.Count > 0)
                    activeAtom = desktopAtoms[desktopAtoms.Count-1];
                else
                    activeAtom = null;
            }

            if (draggingAtom == m)
                draggingAtom = null;
        }

        public void Minimize(Atom m)
        {
            if (dock != null)
            {
                dock.Add(m);
                Remove(m);
            }
        }

        public void Focus(Atom m)
        {
            if (desktopAtoms.Contains(m))
            {
                desktopAtoms.Remove(m);
                desktopAtoms.Add(m);
                activeAtom = m;
            }
        }

        public void SetMenu(MenuBar m)
        {
            if (menu != null)
                menu.Clear();

            menu = m;
        }

        public void SetDock(Dock d)
        {
            if (d == null)
                return;

            if (dock != null)
            {
                d.Add(dock.DockedItems);
            }

            dock = d;
        }

        public void doEvent(object sender, KeyboardEventArgs e)
        {            
//            if (menu != null && menu.doEvent(this, e))
//                return;

            if (e.Down == true)
            {
                if ( desktopAtoms.Count > 0 && e.Key == Key.Tab &&
                    ((e.Mod & ModifierKeys.LeftControl) != 0) || 
                    ((e.Mod & ModifierKeys.RightControl) != 0))
                {
                    for (int i = 0; i < desktopAtoms.Count; i++)
                    {
                        if (desktopAtoms[i].allowFocus())
                        {
                            Focus(desktopAtoms[i]);
                            return;
                        }
                    }

                    return;
                }
            }

            if( activeAtom != null )
                activeAtom.doEvent(this, e);
        }

        public void doEvent(object sender, MouseButtonEventArgs e)
        {
            if (menu != null && menu.doEvent(this, e))
                return;

            if (dock != null &&
                e.X >= displaySurface.Width - dock.Width &&
                e.Y >= displaySurface.Height - dock.Height)
            {
                MouseButtonEventArgs te = new MouseButtonEventArgs(
                    e.Button,
                    e.ButtonPressed,
                    (short)(e.X - dock.Position.X),
                    (short)(e.Y - dock.Position.Y));

                if( dock.doEvent(this, te) )
                    return ;
            }


            if (draggingAtom != null)
            {
                MouseButtonEventArgs te = new MouseButtonEventArgs(
                    e.Button,
                    e.ButtonPressed,
                    (short)(e.X - draggingAtom.Position.X),
                    (short)(e.Y - draggingAtom.Position.Y));

                draggingAtom.doEvent(this, te);
            }

            if (e.Button == MouseButton.PrimaryButton && !e.ButtonPressed )
            {
                draggingAtom = null;
            }

            for (int i = desktopAtoms.Count - 1; i >= 0; i--)
            {
                Rectangle r = desktopAtoms[i].Boundaries;

                if (e.X < r.X || e.X >= r.X + r.Width ||
                    e.Y < r.Y || e.Y >= r.Y + r.Height)
                {
                    continue;
                }

                if (e.Button == MouseButton.PrimaryButton && e.ButtonPressed)
                {
                    draggingAtom = desktopAtoms[i];
                }

                MouseButtonEventArgs te = new MouseButtonEventArgs(
                    e.Button,
                    e.ButtonPressed,
                    (short)(e.X - r.X),
                    (short)(e.Y - r.Y));

                Atom da = desktopAtoms[i];
                da.doEvent(this, te);
                Focus(da);

                return;
            }
        }

        public void doEvent(object sender, MouseMotionEventArgs e)
        {
            if (menu != null && menu.doEvent(this, e, new Point(0,0)))
                return ;

            dock.X = displaySurface.Width - dock.Width;
            dock.Y = displaySurface.Height - dock.Height;

            if (dock != null)
            {
                MouseMotionEventArgs te = new MouseMotionEventArgs(
                    e.ButtonPressed,
                    e.Button,
                    (short)(e.X - dock.X),
                    (short)(e.Y - dock.Y),
                    e.RelativeX,
                    e.RelativeY);

                if( dock.doEvent(this, te) )
                    return ;
            }

            if (draggingAtom != null)
            {
                MouseMotionEventArgs te = new MouseMotionEventArgs(
                    e.ButtonPressed,
                    e.Button,
                    (short)(e.X - activeAtom.Position.X),
                    (short)(e.Y - activeAtom.Position.Y),
                    e.RelativeX,
                    e.RelativeY);

                draggingAtom.doEvent(this, te);
            }

            for (int i = desktopAtoms.Count - 1; i >= 0; i--)
            {
                Rectangle r = desktopAtoms[i].Boundaries;

                if (e.X < r.X || e.X >= r.X + r.Width ||
                    e.Y < r.Y || e.Y >= r.Y + r.Height)
                {
                    continue;
                }

                if (desktopAtoms[i] == draggingAtom)
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

                desktopAtoms[i].doEvent(this, te);

                return;
            }
        }

        public void Paint()
        {
            // Fill the desktop
            displaySurface.Fill(DisplaySettings.desktopColor);

            // Draw the desktop wallpaper
            Point p = new Point();

            if ((DisplaySettings.desktopAlignment & Alignment.BOTTOM) != 0)
                p.Y = displaySurface.Height - DisplaySettings.desktopImage.Height;
            else if ((DisplaySettings.desktopAlignment & Alignment.TOP) != 0)
                p.Y = 0;
            else
                p.Y = (displaySurface.Height - DisplaySettings.desktopImage.Height) / 2;

            if ((DisplaySettings.desktopAlignment & Alignment.RIGHT) != 0)
                p.X = displaySurface.Width - DisplaySettings.desktopImage.Width;
            else if ((DisplaySettings.desktopAlignment & Alignment.LEFT) != 0)
                p.X = 0;
            else
                p.X = (displaySurface.Width - DisplaySettings.desktopImage.Width) / 2;

            displaySurface.Blit(DisplaySettings.desktopImage, p);

            // Paint the windows
            Rectangle oldClipper = displaySurface.ClipRectangle;

            foreach (Molicule m in desktopAtoms)
            {
                Rectangle newClipper = m.Boundaries;
                newClipper.Intersect(oldClipper);

                displaySurface.ClipRectangle = newClipper;

                m.paint(displaySurface, activeAtom == m, newClipper.Location);
            }

            // Paint the dock
            if( dock != null )
            {
                int width = dock.Width;
                int height = dock.Height;


                Rectangle clipper = new Rectangle(
                    displaySurface.Width - width,
                    displaySurface.Height - height,
                    width,
                    height );

                displaySurface.ClipRectangle = clipper;

                dock.paint(displaySurface, false, clipper.Location);
            }

            displaySurface.ClipRectangle = oldClipper;

            // Paint the menu (top level widget)
            if (menu != null)
                menu.paint(displaySurface, displaySurface.Rectangle);

            DisplaySettings.topLeftGlyph.Blit(displaySurface, new Point(0, 0));
            DisplaySettings.topRightGlyph.Blit(displaySurface, new Point(displaySurface.Width - Compound.IconSize.Width, 0));
            DisplaySettings.bottomLeftGlyph.Blit(displaySurface, new Point(0, displaySurface.Height - Compound.IconSize.Height));
            DisplaySettings.bottomRightGlyph.Blit(displaySurface, new Point(displaySurface.Width - Compound.IconSize.Width, displaySurface.Height - Compound.IconSize.Height));
        }
    }
}
