using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using cstari.chemistry.atoms;
using cstari.chemistry.captions;
using cstari.chemistry;

using cstari.chips;
using cstari.utility;

namespace cstari.ui
{
    public class ConfigureRom : Molicule
    {
        private string gameHash;

        private EditBox name;
        private ListBox mapper;
        private ListBox ctrl_a;
        private ListBox ctrl_b;
        private Radio signal;

        private Lable aLable;
        private Lable bLable;

        private DisplayCaption configCaption;

        private string filename;

        public ConfigureRom(Compound gui, string file, string hash, int romSize, DisplayCaption caption )
            :
            base(gui, new Rectangle(0, 0, 273, 224), new DisplayCaption("Configure rom"), new List<Atom>())
        {
            gameHash = hash;

            List<DisplayCaption> controllers = new List<DisplayCaption>();
            List<DisplayCaption> mappers = new List<DisplayCaption>();
            List<DisplayCaption> signals = new List<DisplayCaption>();

            configCaption = caption;
            filename = file;

            DisplayCaption s;

            s = new DisplayCaption("NTSC");
            s.UserData = SignalType.VIDEO_NTSC;
            signals.Add(s);

            s = new DisplayCaption("PAL");
            s.UserData = SignalType.VIDEO_PAL;
            signals.Add(s);

            s = new DisplayCaption("SECAM");
            s.UserData = SignalType.VIDEO_SECAM;
            signals.Add(s);

            foreach (string m in MapperFarm.MappersBySize(romSize))
            {
                mappers.Add(new DisplayCaption(m));
            }
            
            // TODO: Retrieve from input farm
            controllers.Add(new DisplayCaption("Joystick"));

            name = new EditBox(gui, new Rectangle(0, 0, 256, 24), file, null);
            mapper = new ListBox(gui, new Rectangle(124, 28, 132, 40), mappers, null);
            ctrl_a = new ListBox(gui, new Rectangle(124, 72, 132, 40), controllers, null);
            ctrl_b = new ListBox(gui, new Rectangle(124, 116, 132, 40), controllers, null);
            signal = new Radio(gui, new Rectangle(16, 44, 100, 0), signals, null);

            Add(new Button(gui, new Rectangle(192,160,64,24),new DisplayCaption("Update"),doProfile));
            Add(new Button(gui, new Rectangle(192-68,160,64,24),new DisplayCaption("Remove"),removeProfile));


            aLable = new Lable(gui, new Rectangle(110, 126, 0, 0), "B", Alignment.LEFT, DisplaySettings.captionColor, DisplaySettings.captionFont);
            bLable = new Lable(gui, new Rectangle(110, 84, 0, 0), "A", Alignment.LEFT, DisplaySettings.captionColor, DisplaySettings.captionFont);

            Add(aLable);
            Add(bLable);

            if (ProfileManager.Profiled(gameHash))
            {
                ProfileManager.GameProfile profile = ProfileManager.GetProfile(gameHash);

                name.Value = profile.name;

                for (int i = 0; i < signals.Count; i++)
                {
                    if ((SignalType)signals[i].UserData == profile.signal)
                    {
                        signal.Selection = i;
                        break;
                    }
                }

                for (int i = 0; i < ctrl_a.List.Count; i++)
                {
                    if (ctrl_a.List[i].Text == profile.controller_a)
                    {
                        ctrl_a.Selected = i;
                        break;
                    }
                }

                for (int i = 0; i < ctrl_b.List.Count; i++)
                {
                    if (ctrl_b.List[i].Text == profile.controller_b)
                    {
                        ctrl_b.Selected = i;
                        break;
                    }
                }

                for (int i = 0; i < mappers.Count; i++)
                {
                    if (mappers[i].Text == profile.mapper)
                    {
                        mapper.Selected = i;
                        break;
                    }
                }                
            }
            
            Add(name);
            Add(mapper);
            Add(ctrl_a);
            Add(ctrl_b);
            Add(signal);
        }

        public void doProfile()
        {
            ProfileManager.GameProfile gp = new ProfileManager.GameProfile();

            gp.name = name.Value;
            gp.mapper = mapper.List[mapper.Selected].Text;
            gp.controller_a = ctrl_a.List[ctrl_a.Selected].Text;
            gp.controller_b = ctrl_b.List[ctrl_b.Selected].Text;
            gp.signal = (SignalType) signal.List[signal.Selection].UserData;            

            ProfileManager.AddProfile(gameHash, gp);

            configCaption.Icon = DisplaySettings.binaryIcon;
            configCaption.Text = name.Value;

            gui.Remove(this);
        }

        public void removeProfile()
        {
            ProfileManager.RemoveProfile(gameHash);

            configCaption.Icon = null;
            configCaption.Text = filename;

            gui.Remove(this);
        }
    }
}
