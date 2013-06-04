using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.chemistry.atoms
{
    public class Lable : Atom
    {
        private SdlDotNet.Graphics.Font font;
        private Alignment align;
        private Color color;

        private int MaxTextWidth;

        private string[] caption;
        private Surface[] captionRendered;

        public Lable(Compound gui, Rectangle bounds, string text, Alignment a, Color c, SdlDotNet.Graphics.Font f)
            : base(gui, bounds, null)
        {
            Text = text;

            font = f;
            align = a;
            color = c;
        }

        public Color Color
        {
            set
            {
                color = value;
                captionRendered = null;
            }
        }

        public SdlDotNet.Graphics.Font Font
        {
            set
            {
                font = value;
                captionRendered = null;
            }
        }

        public Alignment Alignment
        {
            set
            {
                align = value;
            }
        }

        public override bool allowFocus()
        {
            return false;
        }

        public string Text
        {
            set
            {
                caption = value.Split(new char[] { '\n' });
                captionRendered = null;
            }
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            if (captionRendered == null)
            {
                captionRendered = new Surface[caption.Length];
                MaxTextWidth = 0;

                for (int i = 0; i < caption.Length; i++)
                {
                    captionRendered[i] = font.Render(caption[i], color);
                    MaxTextWidth = System.Math.Max(captionRendered[i].Width, MaxTextWidth);
                }
            }

            Point p = offset;

            for (int i = 0; i < captionRendered.Length; i++)
            {
                if (align == Alignment.RIGHT)
                {
                    p.X = Width - captionRendered[i].Width + offset.X;
                }
                else if (align == Alignment.CENTER)
                {
                    p.X = (Width - captionRendered[i].Width) / 2 + offset.X;
                }

                surface.Blit(captionRendered[i], p);
                p.Y += captionRendered[i].Height;
            }
        }
    }
}
