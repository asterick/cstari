using System;
using System.Collections.Generic;
using System.Text;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;
using cstari.chemistry.captions;
using System.Drawing;

namespace cstari.chemistry
{
    static public class DisplaySettings
    {
        static public SdlDotNet.Graphics.Font captionFont;
        static public SdlDotNet.Graphics.Font lableFont;
        static public SdlDotNet.Graphics.Font fixedPointFont;

        static public DisplayCaption defaultCaption;

        static public DisplayIcon minimizeIcon;
        static public DisplayIcon closeIcon;

        static public DisplayIcon checkMatrix;
        static public DisplayIcon uncheckMatrix;

        static public DisplayIcon BLPos;
        static public DisplayIcon M0Pos;
        static public DisplayIcon M1Pos;
        static public DisplayIcon P0Pos;
        static public DisplayIcon P1Pos;
        static public DisplayIcon HPos;

        static public DisplayIcon defaultIcon;

        static public DisplayIcon binaryIcon;
        static public DisplayIcon folderIcon;

        static public DisplayIcon popupIcon;
        static public DisplayIcon radioSelected;
        static public DisplayIcon radioDeselected;
        static public DisplayIcon checkChecked;
        static public DisplayIcon checkUnchecked;
        static public DisplayIcon spinnerArrows;

        static public DisplayIcon scrollUp;
        static public DisplayIcon scrollDown;
        static public DisplayIcon scrollLeft;
        static public DisplayIcon scrollRight;
        static public DisplayIcon scrollMarker;

        static public DisplayIcon topRightGlyph;
        static public DisplayIcon topLeftGlyph;
        static public DisplayIcon bottomRightGlyph;
        static public DisplayIcon bottomLeftGlyph;

        static public Surface desktopImage;
        static public Alignment desktopAlignment;

        static public Color desktopColor;
        static public Color bodyFocused;
        static public Color bodyBlured;
        static public Color titleFocused;
        static public Color titleBlured;
        static public Color focused;
        static public Color captionColor;
        static public Color lableColor;
        static public Color atomHighlight;
        static public Color atomBackground;
        static public Color atomBackgroundDimmed;
        static public Color atomForeground;
        static public Color menuForeground;
        static public Color menuBackground;
        static public Color selectedColor;
        static public Color bodyBorderFocused;
        static public Color bodyBorderBlured;

        static DisplaySettings()
        {
            // SHOULD LOAD FROM FILE IF AVAILABLE
            LoadDefaults();
        }

        private static void LoadDefaults()
        {
            atomBackground = Color.FromArgb(255, 255, 255);
            atomBackgroundDimmed = Color.FromArgb(200, 200, 200);
            atomForeground = Color.FromArgb(0, 0, 0);
            atomHighlight = Color.FromArgb(200, 200, 255);
            bodyBlured = Color.FromArgb(0, 102, 155);
            bodyFocused = Color.FromArgb(0, 150, 255);
            captionColor = Color.FromArgb(0, 0, 0);
            desktopColor = Color.FromArgb(151, 149, 128);
            focused = Color.FromArgb(255, 210, 57);
            lableColor = Color.FromArgb(0, 0, 0);
            menuBackground = Color.FromArgb(255, 255, 255);
            menuForeground = Color.FromArgb(0, 0, 0);
            selectedColor = Color.FromArgb(255, 210, 57);
            titleBlured = Color.FromArgb(255, 210, 57);
            titleFocused = Color.FromArgb(255, 231, 0);
            bodyBorderBlured = Color.FromArgb(0, 0, 0);
            bodyBorderFocused = Color.FromArgb(255, 255, 255);

            lableFont = new SdlDotNet.Graphics.Font(@"freesans.ttf", 12);
            fixedPointFont = new SdlDotNet.Graphics.Font(@"VeraMoBd.ttf", 12);
            captionFont = lableFont;

            desktopAlignment = Alignment.CENTER;
            desktopImage = new Surface(Images.desktop);

            Surface icons = new Surface(Images.alerts);
            popupIcon = new DisplayIcon(icons, new Point(64, 48));

            closeIcon = new DisplayIcon(icons, new Point(0, 0));
            minimizeIcon = new DisplayIcon(icons, new Point(0, 16));
            defaultIcon = new DisplayIcon(icons, new Point(64, 32));

            binaryIcon = new DisplayIcon(icons, new Point(96, 32));
            folderIcon = new DisplayIcon(icons, new Point(112, 32));

            topLeftGlyph = new DisplayIcon(icons, new Point(96, 0));
            topRightGlyph = new DisplayIcon(icons, new Point(108, 0));
            bottomLeftGlyph = new DisplayIcon(icons, new Point(96, 16));
            bottomRightGlyph = new DisplayIcon(icons, new Point(108, 16));

            scrollUp = new DisplayIcon(icons, new Point(48, 0));
            scrollDown = new DisplayIcon(icons, new Point(48, 16));
            scrollLeft = new DisplayIcon(icons, new Point(64, 0));
            scrollRight = new DisplayIcon(icons, new Point(64, 16));
            scrollMarker = new DisplayIcon(icons, new Point(80, 0));

            radioDeselected = new DisplayIcon(icons, new Point(16, 0));
            radioSelected = new DisplayIcon(icons, new Point(16, 16));
            checkUnchecked = new DisplayIcon(icons, new Point(32, 0));
            checkChecked = new DisplayIcon(icons, new Point(32, 16));

            spinnerArrows = new DisplayIcon(icons, new Rectangle(80, 16, 10, 16));

            checkMatrix = new DisplayIcon(icons, new Point(112,48));
            uncheckMatrix = new DisplayIcon(icons, new Point(96, 48));
            
            HPos = new DisplayIcon(icons, new Point(32, 112));
            BLPos = new DisplayIcon(icons, new Point(48, 112));
            P0Pos = new DisplayIcon(icons, new Point(64, 112));
            M0Pos = new DisplayIcon(icons, new Point(80, 112));
            P1Pos = new DisplayIcon(icons, new Point(96, 112));
            M1Pos = new DisplayIcon(icons, new Point(112, 112));
            
            defaultCaption = new DisplayCaption("Unnamed", defaultIcon);
        }

    }
}
