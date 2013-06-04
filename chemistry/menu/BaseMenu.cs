using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.captions;

namespace cstari.chemistry.menu
{
    public delegate void MenuCallback();

    public abstract class MenuItem
    {
        abstract public DisplayCaption Caption
        {
            get;
            set;
        }

        abstract public void doCallback();
        abstract public bool IsPopup();
    }

    public class MenuSeperator : MenuItem
    {
        public override DisplayCaption Caption
        {
            get
            {
                return null;
            }
            set
            {
            }
        }


        public override bool IsPopup()
        {
            return false;
        }

        public override void doCallback()
        {
        }
    }

    public class MenuEntry : MenuItem
    {
        protected MenuCallback callbackEvent;
        protected DisplayCaption menuCaption;

        public MenuEntry( DisplayCaption caption, MenuCallback callback)
        {
            menuCaption = caption; 
            callbackEvent = callback;
        }

        public override bool IsPopup()
        {
            return false;
        }

        public override void doCallback()
        {
            if (callbackEvent != null)
            {
                callbackEvent();
            }
        }

        public override DisplayCaption Caption
        {
            get
            {
                return menuCaption;
            }
            set
            {
                menuCaption = value;
            }
        }
    }

    abstract public class Menu : MenuEntry
    {
        public static int SeperatorThickness = 2;

        protected List<MenuItem> menuEntries;
        protected Menu SubMenu;

        protected Point position;
        protected bool visible;

        public Menu(DisplayCaption caption, List<MenuItem> entries, MenuCallback callback)
            : base(caption, callback)
        {
            SubMenu = null;
            menuEntries = entries;
            Clear();
        }

        public Point Position
        {
            set
            {
                position = value;
            }
            get
            {
                return position;
            }
        }

        public void Clear()
        {
            visible = false;
        }

        public void Show()
        {
            visible = true;
        }

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

        abstract public void Paint(Surface display);
    }
}
