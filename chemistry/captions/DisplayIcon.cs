using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.chemistry.captions
{
    public class DisplayIcon
    {
        private Rectangle rect;
        private Surface icon;

        public int Width
        {
            get { return rect.Width; }
        }

        public int Height
        {
            get { return rect.Height; }
        }

        public Size Dimensions
        {
            get { return rect.Size; }
        }

        public DisplayIcon(string filename)
        {
            icon = new Surface(filename);
            rect = new Rectangle(new Point(0, 0), icon.Size);
        }

        public DisplayIcon(Surface src, Point p)
        {
            icon = src;
            rect = new Rectangle(p, Compound.IconSize);
        }

        public DisplayIcon(Surface src, Rectangle r)
        {
            icon = src;
            rect = r;
        }

        public DisplayIcon(DisplayIcon i)
        {
            icon = i.icon;
            rect = i.rect;
        }

        public void Blit(Surface surf, Point o)
        {
            surf.Blit(icon, o, rect);
        }
    }
}
