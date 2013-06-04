using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using cstari.chemistry;
using cstari.chemistry.captions;
using cstari.chemistry.atoms;

using cstari.chips;
using cstari.chips.mappers;
using cstari.utility;

namespace cstari.ui
{
    public class LoadRom : Molicule
    {
        private static DirectoryInfo baseDirectory;
        private DirectoryInfo directory;
        private ListBox guiList;
        private Scroll guiScroll;

        static LoadRom()
        {
            baseDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        public LoadRom( Compound gui )
            : base(gui,new Rectangle(), new DisplayCaption("Load Rom"), new List<Atom>())
        {            
            Width = 232;
            Height = 198;

            directory = baseDirectory;

            guiList = new ListBox(gui, new Rectangle(3, 3, 192, 128), new List<DisplayCaption>(), onList);
            guiScroll = new Scroll(gui, new Rectangle(199, 3, 32, 128), new Range(0, 0), onScroll);

            Add(guiScroll);
            Add(guiList);
            Add(new Button(gui, new Rectangle(151, 135, 64, 24), new DisplayCaption("Load"), onLoad));
            Add(new Button(gui, new Rectangle(83, 135, 64, 24), new DisplayCaption("Profile"), onProfile));
            Add(new Button(gui, new Rectangle(3, 135, 64, 24), new DisplayCaption("Up"), onFolderUp));

            BuildList();
        }

        private void BuildList()
        {
            List<DisplayCaption> list = new List<DisplayCaption>();

            foreach (DirectoryInfo di in directory.GetDirectories())
            {
                DisplayCaption dc = new DisplayCaption(di.Name, DisplaySettings.folderIcon);
                dc.UserData = di.Name;
                list.Add(dc);
            }

            foreach (FileInfo fi in directory.GetFiles())
            {
                // Check if it"s a valid rom size
                if ( !MapperFarm.AllowedSize( (int)fi.Length ) )
                    continue;

                try
                {
                    byte[] data = new byte[fi.Length];

                    FileStream fo = File.OpenRead(fi.FullName);
                    fo.Read(data, 0, (int)data.Length);
                    fo.Close();

                    string hash = ProfileManager.GetHash(data);

                    if (ProfileManager.Profiled(hash))
                    {
                        ProfileManager.GameProfile profile = ProfileManager.GetProfile(hash);

                        DisplayCaption dc = new DisplayCaption(profile.name, DisplaySettings.binaryIcon);
                        dc.UserData = fi.Name;

                        list.Add(dc);
                    }
                    else
                    {
                        DisplayCaption dc = new DisplayCaption(fi.Name);
                        dc.UserData = fi.Name;

                        list.Add(dc);
                    }
                }
                catch
                {
                    Console.WriteLine("Failed to test: " + fi.FullName);
                }
            }

            guiList.List = list;
            guiScroll.Max = guiList.List.Count - 1;
        }

        private void onScroll(int Selection)
        {
            guiList.Selected = Selection;
        }

        private void onList(int Selection)
        {
            guiScroll.Value = Selection;
        }

        private void onFolderUp()
        {
            if (directory.Parent == null)
                return;

            directory = directory.Parent;
            BuildList();
        }

        private void onProfile()
        {
            if (guiList.List.Count == 0)
                return;

            string name = Path.Combine(directory.FullName, (string)guiList.List[guiList.Selected].UserData);

            if(File.Exists(name))
            {
                FileStream fi = File.OpenRead(name);
                byte[] data = new byte[fi.Length];
                fi.Read(data, 0, (int)fi.Length);
                fi.Close();

                string hash = ProfileManager.GetHash(data);

                gui.Add(new ConfigureRom(gui, (string)guiList.List[guiList.Selected].UserData, hash, data.Length, guiList.List[guiList.Selected]), true);
            }
        }

        private void onLoad()
        {
            if (guiList.List.Count == 0)
                return;

            string name = Path.Combine(directory.FullName, (string)guiList.List[guiList.Selected].UserData);

            if (Directory.Exists(name))
            {
                directory = new DirectoryInfo(name);
                BuildList();
            }
            else if(File.Exists(name))
            {
                FileStream fi = File.OpenRead(name);
                byte[] data = new byte[fi.Length];
                fi.Read(data, 0, (int)fi.Length);
                fi.Close();

                string hash = ProfileManager.GetHash(data);

                if (ProfileManager.Profiled(hash))
                {
                    ProfileManager.GameProfile profile = ProfileManager.GetProfile(hash);

                    Mapper m = MapperFarm.CreateByName(profile.mapper, data);

                    // TODO: CREATE JOYSTICKS FROM PROFILE
                    // TODO: SET SIGNAL FROM PROFILE

                    baseDirectory = directory;

                    gui.Add(new EmulatorWindow(gui, profile.name, m, profile.signal), true);
                    gui.Remove(this);
                }
                else
                {
                    gui.Add(new ConfigureRom(gui, (string)guiList.List[guiList.Selected].UserData, hash, data.Length, guiList.List[guiList.Selected]), true);
                }
            }
        }
    }
}
