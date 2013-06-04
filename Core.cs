using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Drawing.Text;
using System.IO;
using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry;
using cstari.chemistry.menu;
using cstari.chemistry.atoms;
using cstari.chemistry.captions;

using cstari.chips;
using cstari.chips.mappers;
using cstari.ui;

namespace cstari
{
    class Core
    {
        public static string VersionNumber = "20060821";
        public static string ApplicationName = "cstari";
        public static string WindowCaption = ApplicationName + " build (" + VersionNumber + ")";

        private bool Running;

        private Surface screen;
        private Compound gui;

        static void Main(string[] args)
        {
            Core app = new Core();

            app.Run();
            return;
        }

        public Core()
        {
            // Create the display
            screen = Video.SetVideoMode(800, 600);
            Video.WindowCaption = WindowCaption;
            Keyboard.UnicodeEnabled = true;
            
            gui = new Compound(screen);

            // Create our dock
            gui.SetDock(new Dock(gui));

            // Make our menu
            List<MenuItem> mainMenu = new List<MenuItem>();
            List<MenuItem> gameMenu = new List<MenuItem>();
            List<MenuItem> toolMenu = new List<MenuItem>();

            mainMenu.Add(new PopupMenu(new DisplayCaption("Game", DisplaySettings.defaultIcon), gameMenu, null));
            mainMenu.Add(new PopupMenu(new DisplayCaption("Tools"), toolMenu, null));
            mainMenu.Add(new MenuEntry(new DisplayCaption("About"), onAbout));

            toolMenu.Add(new MenuEntry(new DisplayCaption("Locate Duplicates"),null ));
            toolMenu.Add(new MenuEntry(new DisplayCaption("Save Screenshot"),null ));

            gameMenu.Add(new MenuEntry(new DisplayCaption("Load Rom"), onLoadRom ));
            gameMenu.Add(new MenuEntry(new DisplayCaption("Quit"), onQuitMenu ));

            gui.SetMenu(new MenuBar(gui, mainMenu));

            Events.MouseMotion += new EventHandler<MouseMotionEventArgs>(gui.doEvent);
            Events.MouseButtonUp += new EventHandler<MouseButtonEventArgs>(gui.doEvent);
            Events.MouseButtonDown += new EventHandler<MouseButtonEventArgs>(gui.doEvent);
            Events.KeyboardDown += new EventHandler<KeyboardEventArgs>(gui.doEvent);
            Events.KeyboardUp += new EventHandler<KeyboardEventArgs>(gui.doEvent);
            Events.Quit += new EventHandler<QuitEventArgs>(onQuit);

            Running = true;
        }

        public void onLoadRom()
        {
            gui.Add(new LoadRom(gui), true);
        }

        private void onAbout()
        {
            gui.Add(new About(gui), false);
        }

        private void onQuitMenu()
        {
            Running = false;
        }

        private void onQuit(object sender, QuitEventArgs quitArgs)
        {
            Running = false;
        }

        public void Run()
        {
            int frames = 0;
            int previousTicks = 0;

            AudioMixer.StartMixer(44100, 0x800); // 1/5th a second audio buffer

            while( Running )
            {
                while( Events.Poll() );

                gui.Paint();
                Video.Update();

                if (++frames >= 3)
                {
                    int ticks = Timer.TicksElapsed;

                    if (ticks - previousTicks < 50)
                        System.Threading.Thread.Sleep(50 + previousTicks - ticks);
                    else if (ticks - previousTicks > 100)
                    {
                        previousTicks = ticks - 100;
                    }

                    previousTicks += 50;

                    Video.WindowCaption = String.Format("{0} Time skew: {1}ms", WindowCaption, previousTicks - ticks);

                    frames = 0;
                }
            }

            AudioMixer.CloseMixer();
        }
    }
}
