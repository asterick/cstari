using System;
using System.Drawing;

using SdlDotNet;
using SdlDotNet.Input;

using cstari.chemistry.captions;

namespace cstari.chemistry
{
    public abstract class Atom
    {
        abstract public bool allowFocus();
        protected bool dirty;
        protected Compound gui;
        private DisplayCaption caption;

        private int x;
        private int y;
        private int width;
        private int height;

        public Atom(Compound GUI, Rectangle bound, DisplayCaption caption)
        {
            gui = GUI;
            Boundaries = bound;
            Caption = caption;
        }

        public virtual DisplayCaption Caption
        {
            set
            {
                caption = value;
            }
            get
            {
                return caption;
            }
        }

        public virtual Rectangle Boundaries
        {
            get
            {
                return new Rectangle(Position,Dimensions);
            }
            set
            {
                X = value.X;
                Y = value.Y;
                Width = value.Width;
                Height = value.Height;
            }
        }

        public virtual Size Dimensions
        {
            get
            {
                return new Size(Width,Height);
            }
            set
            {
                Width = value.Width;
                Height = value.Height;
            }
        }

        public virtual Point Position
        {
            get
            {
                return new Point(X,Y);
            }
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public virtual bool Dirty
        {
            get
            {
                return dirty;
            }

        }

        public virtual int X
        {
            get { return x; }
            set { x = value; }
        }

        public virtual int Y
        {
            get { return y; }
            set { y = value; }
        }

        public virtual int Width
        {
            get { return width; }
            set { width = value; }
        }

        public virtual int Height
        {
            get { return height; }
            set { height = value; }
        }

        abstract public void paint(SdlDotNet.Graphics.Surface surface, bool focused, Point offset);

        virtual public bool doEvent(object caller, KeyboardEventArgs e)
        {
            return false;
        }

        virtual public bool doEvent(object caller, MouseButtonEventArgs e)
        {
            return false;
        }

        virtual public bool doEvent(object caller, MouseMotionEventArgs e)
        {
            return false;
        }
    }
}
