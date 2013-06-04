using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using cstari.chips;
using cstari.chemistry;
using cstari.chemistry.captions;
using cstari.chemistry.atoms;
using cstari.chemistry.menu;
using cstari.utility;

namespace cstari.ui
{
    public class EmulatorWindow : Molicule
    {
        private Television system;

        public EmulatorWindow(Compound gui, string title, Mapper m, SignalType s)
            : base(gui, new Rectangle(0, 0, 328, 332), new DisplayCaption(title), new List<Atom>())
        {
            List<MenuItem> menuItems = new List<MenuItem>();
            List<MenuItem> runtimeItems = new List<MenuItem>();
            List<MenuItem> displayItems = new List<MenuItem>();
            List<MenuItem> debugItems = new List<MenuItem>();

            system = new Television(gui, new Point(0, 0), m, s);

            runtimeItems.Add(new MenuEntry(new DisplayCaption("Run"), system.run));
            runtimeItems.Add(new MenuEntry(new DisplayCaption("Stop"), system.stop));
            runtimeItems.Add(new MenuEntry(new DisplayCaption("Reset"), system.reset));

            debugItems.Add(new MenuEntry(new DisplayCaption("Disassembly"), system.disasm));
            debugItems.Add(new MenuEntry(new DisplayCaption("TIA Debugger"), system.tiaview));

            menuItems.Add(new PopupMenu(new DisplayCaption("Runtime"), runtimeItems, null));
            menuItems.Add(new PopupMenu(new DisplayCaption("Display"), displayItems, null));
            menuItems.Add(new PopupMenu(new DisplayCaption("Debug"), debugItems, null));
            
            SetMenu( new MenuBar( gui, menuItems ));

            Add(system);
        }

        public override int Width
        {
            get
            {
                return CalculateSize(system.Dimensions).Width;
            }
        }

        public override int Height
        {
            get
            {
                return CalculateSize(system.Dimensions).Height;
            }
        }
    }
}
