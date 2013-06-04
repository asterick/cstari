using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using cstari.chemistry;
using cstari.chemistry.atoms;
using cstari.chemistry.captions;

using SdlDotNet.Graphics;

namespace cstari.ui
{
    public class About : Molicule
    {
        private static Surface aboutIcon;
        private Lable about;

        static About()
        {
            aboutIcon = new Surface(Images.logo);
        }

        public About(Compound gui)
            : base(gui, new Rectangle(32, 32, 480, 240), new DisplayCaption("About CStari"), new List<Atom>())
        {
            string aboutCaption = "CStari: " + Core.VersionNumber + "\n\n" +
                "Bryon Vandiver (asterick@buxx.com): Code\n" +
                "Kevin Horton: Public Domain Information\n" +
                "Dan Boris: Public Domain Information\n" +
                "Steve Wright: Stella Programmer's Guide";

            Add(new StaticImage(gui,new Rectangle(16,16,128,128),aboutIcon));

            about = new Lable(gui, new Rectangle(140, 16, 340, 200), aboutCaption, Alignment.CENTER, Color.White, DisplaySettings.captionFont);

            Add( about );
        }
    }
}
