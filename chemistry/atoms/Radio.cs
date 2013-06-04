using System;
using System.Drawing;
using System.Collections.Generic;
using SdlDotNet;
using SdlDotNet.Graphics;
using SdlDotNet.Input;

using cstari.chemistry.captions;

public delegate void ChemistryRadioCallback(int selected);

namespace cstari.chemistry.atoms
{
    public class Radio : Atom
    {
        private ChemistryRadioCallback callbackHandler;
        private List<DisplayCaption> captions;
        private int selected;

        public Radio(Compound gui, Rectangle bounds, List<DisplayCaption> lables, ChemistryRadioCallback call)
            : base(gui, bounds,null)
        {
            callbackHandler = call;
            captions = lables;
            selected = 0;
        }

        public override bool allowFocus()
        {
            return true;
        }

        public int Selection
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;

                if (selected < 0)
                    selected = 0;
                else if (selected >= captions.Count)
                    selected = captions.Count - 1;
            }
        }

        public List<DisplayCaption> List
        {
            get { return captions; }
            set {
                Selection = selected;
                captions = value; 
            }
        }

        public void setValue(List<DisplayCaption> data)
        {
            captions = data;

            selected = 0;
        }

        public override int Height
        {
            get
            {
                int size = 0;

                for (int i = 0; i < captions.Count; i++)
                {
                    if (i == selected)
                        size += Math.Max(captions[i].Height, DisplaySettings.radioSelected.Height);
                    else
                        size += Math.Max(captions[i].Height, DisplaySettings.radioDeselected.Height);
                }

                return size;
            }
            set
            {
                base.Height = value;
            }
        }

        public override bool doEvent(object caller, KeyboardEventArgs e)
        {
            if (e.Down)
            {
                if (Key.UpArrow == e.Key)
                {
                    selected--;

                    if (selected < 0)
                        selected = captions.Count - 1;
                }
                else if (Key.DownArrow == e.Key)
                {
                    selected = (selected + 1) % captions.Count;
                }
                
                if (callbackHandler != null)
                {
                    callbackHandler(selected);
                }
            }

            return true;
        }

        public override bool doEvent(object caller, MouseButtonEventArgs e)
        {
            Point p = new Point(0, 0);

            if (!e.ButtonPressed)
                return true;

            for (int i = 0; i < captions.Count; i++)
            {
                p.Y += captions[i].Height;

                if (e.X > DisplaySettings.radioDeselected.Width)
                    continue;

                if (e.Y >= p.Y)
                    continue;

                selected = i;

                if (callbackHandler != null)
                    callbackHandler(selected);

                break;
            }

            return true;
        }

        public override void paint(Surface surface, bool focused, Point offset)
        {
            Point pb = offset;
            Point pa = offset;

            pb.X += Math.Max(DisplaySettings.radioSelected.Width, DisplaySettings.radioDeselected.Width) + Compound.Padding;

            for( int i = 0; i < captions.Count; i++ )
            {
                int inc;

                captions[i].SizeWithIcon = false;
                if (i == selected)
                {
                    DisplaySettings.radioSelected.Blit(surface, pa);
                    captions[i].Blit(surface, pb);

                    inc = Math.Max(captions[i].Height, DisplaySettings.radioSelected.Height);
                }
                else
                {
                    DisplaySettings.radioDeselected.Blit(surface, pa);
                    captions[i].Blit(surface, pb);
                    inc = Math.Max(captions[i].Height, DisplaySettings.radioDeselected.Height);
                }

                pa.Y += inc;
                pb.Y += inc;
            }
        }
    }
}
