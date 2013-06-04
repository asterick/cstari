using System;
using System.Collections.Generic;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.chemistry.captions
{
    public class DisplayCaption
    {
        private Surface caption;
        private string textCaption;
        private DisplayIcon icon;
        private bool sizeWithIcon;
        private bool defaultIcon;
        private object userData;

        public object UserData
        {
            set { userData = value; }
            get { return userData; }
        }

        public DisplayIcon Icon
        {
            get {
                return (icon == null && defaultIcon) ? DisplaySettings.defaultIcon : icon; 
            }
            set
            {
                icon = value;
            }
        }

        public string Text
        {
            get
            {
                return textCaption;
            }
            set
            {
                textCaption = value;
                caption = null;
            }
        }

        public bool SizeWithIcon
        {
            set { sizeWithIcon = value; }
        }

        public bool UseDefaultIcon
        {
            set { defaultIcon = value; }
        }

        public int Width
        {
            get {
                Prerender();

                DisplayIcon i = icon;

                if (i == null && defaultIcon)
                {
                    i = DisplaySettings.defaultIcon;
                }

                int width = 0;

                if (caption != null && (i != null || sizeWithIcon))
                {
                    width += Compound.Padding;
                }

                if( caption != null )
                    width += caption.Width;

                if (i != null)
                {
                    width += i.Width;
                }
                else if (sizeWithIcon)
                {
                    width += Compound.IconSize.Width;
                }

                return width; 
            }
        }

        public int Height
        {
            get
            {
                Prerender();

                int height = 0;

                DisplayIcon i = icon;

                if (i == null && defaultIcon)
                    i = DisplaySettings.defaultIcon;

                if (i == null && sizeWithIcon)
                    height = Compound.IconSize.Height;
                else if (i != null)
                    height = i.Height;

                if (caption != null)
                    height = Math.Max(height, caption.Height);

                return height;
            }
        }

        public DisplayCaption(string title, Surface src, Rectangle r)
        {
            icon = new DisplayIcon(src, r);
            textCaption = title;
        }

        public DisplayCaption(string title, Surface src, Point p)
        {
            icon = new DisplayIcon(src, p);
            textCaption = title;
        }

        public DisplayCaption(string title, DisplayIcon i)
        {
            icon = i;
            textCaption = title;
        }

        public DisplayCaption(Surface src, Rectangle r)
        {
            icon = new DisplayIcon(src, r);
            textCaption = null;
        }

        public DisplayCaption(Surface src, Point p)
        {
            icon = new DisplayIcon(src, p);
            textCaption = null;
        }

        public DisplayCaption(DisplayIcon i)
        {
            icon = i;
            textCaption = null;
        }

        public DisplayCaption(string title)
        {
            icon = null;
            textCaption = title;
        }

        private void Prerender()
        {
            if (caption == null && textCaption != null)
                caption = DisplaySettings.captionFont.Render(textCaption, DisplaySettings.captionColor);
        }

        // ----------------------------------------------------------

        public void Blit(Surface src, Point p)
        {
            Prerender();

            DisplayIcon i = icon;

            if( i == null && defaultIcon )
                i = DisplaySettings.defaultIcon;

            if (i != null)
            {
                Point pI = p;

                if( caption != null )
                    pI.Y += (caption.Height - i.Height) / 2;
                i.Blit(src, pI);                
                p.X += Compound.Padding + i.Width;
            }
            else if (sizeWithIcon)
            {
                p.X += Compound.Padding + Compound.IconSize.Width;
            }

            if( caption != null )
                src.Blit(caption, p);
        }
    }
}
