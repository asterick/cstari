using System;
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;
using cstari.chemistry;
using cstari.chemistry.captions;

namespace cstari.chemistry.menu
{
    class PopupMenu : Menu
    {
        private int selected;
        private Size dimensions;

        public PopupMenu( DisplayCaption caption, List<MenuItem> entries, MenuCallback callback)
            : base(caption, entries, callback)
        {
            selected = -1;
        }

        public override bool IsPopup()
        {
            return true;
        }

        override public bool doEvent(object caller, MouseMotionEventArgs mmData)
        {
            if (!visible)
                return false;

            if (SubMenu != null && SubMenu.doEvent(this,mmData))
                return true;

            Point p = new Point(mmData.X - position.X, mmData.Y - position.Y);
            selected = -1;

            if (p.X < 0 ||
                p.Y < 0 ||
                p.X >= dimensions.Width ||
                p.Y >= dimensions.Height)
            {
                // Out of focus, brute force clear
                Clear();
                return false;
            }

            int scanY = 0;
            int index = 0;

            foreach (MenuItem mi in menuEntries)
            {
                if (mi is MenuSeperator)
                {
                    scanY += Compound.Padding;
                }
                else
                {
                    if (scanY + DisplaySettings.lableFont.Height > p.Y)
                    {
                        selected = index;

                        if (mi.IsPopup())
                        {
                            if (SubMenu != null)
                                SubMenu.Clear();

                            SubMenu = (PopupMenu)mi;
                            SubMenu.Position = new Point(dimensions.Width - Compound.Padding * 2 + position.X, scanY + position.Y + Compound.Padding);
                            SubMenu.Show();
                        }

                        return true;
                    }
                    scanY += DisplaySettings.lableFont.Height;
                }
                index++;
            }

            return true;
        }

        override public bool doEvent(object caller, MouseButtonEventArgs mbData)
        {
            if (!mbData.ButtonPressed || !visible)
            {
                return false;
            }

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

        override public void Paint(Surface display)
        {
            if (visible == false)
                return;

            dimensions = new Size(Compound.Padding * 2, Compound.Padding * 2);

            MenuItem sel;
            if (selected >= 0 && selected < menuEntries.Count)
                sel = menuEntries[selected];
            else
                sel = null;


            foreach (MenuItem mi in menuEntries)
            {
                if (mi is MenuSeperator)
                {
                    dimensions.Height += Compound.Padding + 2;
                }
                else
                {
                    DisplayCaption caption = mi.Caption;
                    caption.SizeWithIcon = true;

                    dimensions.Height += caption.Height;
                    dimensions.Width = Math.Max( dimensions.Width, Compound.Padding * 3 + Compound.IconSize.Width + caption.Width );
                }
            }

            Rectangle rect = new Rectangle(position, dimensions);

            display.Fill(rect, DisplaySettings.menuBackground);
            display.Draw(new SdlDotNet.Graphics.Primitives.Box(position, dimensions), DisplaySettings.menuForeground);

            Point p = new Point(0, Compound.Padding + position.Y);

            foreach (MenuItem mi in menuEntries)
            {
                p.X = Compound.Padding + position.X;

                if (mi is MenuSeperator)
                {
                    dimensions.Height = dimensions.Height + SeperatorThickness;

                    Rectangle line = new Rectangle(Compound.Padding + position.X,
                        p.Y + Compound.Padding / 2,
                        dimensions.Width - Compound.Padding * 2,
                        Compound.Seperator);

                    display.Fill(line, DisplaySettings.menuForeground);

                    p.Y += Compound.Padding + 2;
                }
                else
                {
                    DisplayCaption caption = mi.Caption;
                    caption.SizeWithIcon = true;

                    if (sel == mi)
                    {
                        Rectangle selrect = new Rectangle(Compound.Padding + position.X,
                            p.Y,
                            dimensions.Width - Compound.Padding * 2,
                            DisplaySettings.lableFont.Height);

                        display.Fill(selrect, DisplaySettings.selectedColor);
                    }

                    p.X = Compound.Padding + position.X;
                    caption.Blit(display, p);

                    if (mi.IsPopup())
                    {
                        Point p2 = new Point();

                        p2.X = dimensions.Width - Compound.IconSize.Width - Compound.Padding + position.X;
                        p2.Y += p.Y + (DisplaySettings.lableFont.Height - Compound.IconSize.Height) / 2;

                        DisplaySettings.popupIcon.Blit(display, p2);
                    }

                    p.Y += DisplaySettings.lableFont.Height;
                }
            }

            if (SubMenu != null)
                SubMenu.Paint(display);
        }
    }
}
