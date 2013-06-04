using System;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.chemistry.atoms
{
    public class StaticImage : Atom
    {
        private Surface picture;

        public StaticImage(Compound gui,Rectangle bounds, Surface surf)
            : base(gui, bounds, null)
        {
            picture = surf;
        }

        public override bool allowFocus()
        {
            return false;
        }

        public Surface getValue()
        {
            return picture;
        }

        public void setValue(Surface data)
        {
            picture = data;
        }

        public override int Height
        {
            get
            {
                return picture.Height;
            }
        }

        public override int Width
        {
            get
            {
                return picture.Width;
            }
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            surface.Blit(picture, offset);
        }
    }
}
