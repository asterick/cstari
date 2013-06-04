using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.captions;
using cstari.chemistry.menu;

namespace cstari.chemistry.menu
{
    public class MenuBar
    {
        private int selected;
        private Menu SubMenu;

        private Size displaySize;

        private List<MenuItem> menuEntries;

        public MenuBar(Compound gui, List<MenuItem> entries)
        {
            menuEntries = entries;

            selected = -1;
        }

        public void Clear()
        {
            if (selected >= 0 && selected < menuEntries.Count && menuEntries[selected] is PopupMenu)
            {
                PopupMenu menu = (PopupMenu)menuEntries[selected];

                menu.Clear();
            }
        }

        public bool doEvent(object caller, MouseMotionEventArgs mmData, Point offset )
        {
            if (SubMenu != null && SubMenu.doEvent(this, mmData))
                return true;

            Point p = mmData.Position;
            selected = -1;

            if (p.X < offset.X ||
                p.Y < offset.Y ||
                p.X >= displaySize.Width + offset.X ||
                p.Y >= displaySize.Height + offset.Y )
            {
                // Out of focus, brute force clear
                Clear();
                return false;
            }

            int scanX = Compound.IconSize.Width + Compound.Padding * 2 + offset.X;
            int index = 0;

            foreach (MenuItem mi in menuEntries)
            {
                if (!(mi is MenuSeperator))
                {
                    DisplayCaption caption = mi.Caption;
                    caption.SizeWithIcon = false;

                    int newX = scanX + Compound.Padding * 3 + caption.Width;

                    if (newX > p.X)
                    {
                        selected = index;

                        if (mi.IsPopup())
                        {
                            if (SubMenu != null)
                                SubMenu.Clear();

                            SubMenu = (PopupMenu)mi;
                            SubMenu.Position = new Point(scanX + Compound.Padding * 3, displaySize.Height - Compound.Padding + offset.Y);
                            SubMenu.Show();
                        }

                        return true;
                    }

                    scanX = newX;
                }
                index++;
            }

            return true;
        }

        public bool doEvent(object caller, MouseButtonEventArgs mbData)
        {
            if (!mbData.ButtonPressed)
            {
                return false;
            }

            if (selected < 0)
                return false;

            if (SubMenu != null && SubMenu.doEvent(this, mbData))
            {
                Clear();
                return true;
            }

            if (selected >= 0 && selected <= menuEntries.Count)
                menuEntries[selected].doCallback();
            Clear();

            return true;
        }

        public int Height
        {
            get
            {
                return System.Math.Max(DisplaySettings.lableFont.Height, Compound.IconSize.Height) + Compound.Padding * 1;
            }
        }

        public void paint(Surface surface, Rectangle area)
        {
            displaySize = new Size(area.Width, Height);

            MenuItem sel;
            if (selected >= 0 && selected < menuEntries.Count)
                sel = menuEntries[selected];
            else
                sel = null;

            Rectangle rect = new Rectangle(area.Location, displaySize);

            rect.Height += Compound.BorderPadding.Height;
            surface.Fill(rect, DisplaySettings.menuForeground);
            rect.Height -= Compound.BorderPadding.Height;
            surface.Fill(rect, DisplaySettings.menuBackground);

            Point p = new Point(Compound.Padding + Compound.IconSize.Width + area.X, Compound.Padding + area.Y);

            foreach (MenuItem mi in menuEntries)
            {
                if (!(mi is MenuSeperator))
                {
                    DisplayCaption caption = mi.Caption;
                    caption.SizeWithIcon = false;

                    if (sel == mi)
                    {
                        Rectangle selrect = new Rectangle(p.X - Compound.Padding * 2,
                            p.Y - Compound.Padding,
                            Compound.Padding * 3,
                            displaySize.Height );

                        selrect.Width += Compound.Padding + caption.Width;
                        surface.Fill(selrect, DisplaySettings.selectedColor);
                    }

                    caption.Blit(surface, p);
                    p.X += caption.Width + Compound.Padding;

                    p.X += Compound.Padding * 3;
                }
            }

            if (SubMenu != null)
                SubMenu.Paint(surface);
        }
    }
}
