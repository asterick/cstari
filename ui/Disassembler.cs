using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using cstari.chemistry;
using cstari.chemistry.atoms;
using cstari.chemistry.captions;

using cstari.chips;

namespace cstari.ui
{
    public class Disassembler : Molicule
    {
        private Atari atari;

        private Lable PCLable;
        private Lable ALable;
        private Lable XLable;
        private Lable YLable;
        private Lable SLable;
        private Lable PLable;   
     
        private Lable address;
        private Lable instruction;
        private Lable data;

        private Scroll scrollBar;
        private EditBox modEdit;

        private bool followPC;

        private ushort focusAddress;

        public Disassembler(Compound gui, Atari a)
            : base(gui, new Rectangle(32, 32, 395 + 32 + 16, 208 + 36 + 32), new DisplayCaption("Disassembler"), new List<Atom>())
        {
            atari = a;

            List<DisplayCaption> radioList = new List<DisplayCaption>();
            radioList.Add( new DisplayCaption("Cartridge"));
            radioList.Add( new DisplayCaption("RIOT"));

            followPC = true;

            // Create other thingers here

            Add(new CheckBox(gui, new Rectangle(0, 0, 128, 16), new DisplayCaption("Follow PC"), followPC, follow));
            Add(new CheckBox(gui, new Rectangle(0, 20, 128, 16), new DisplayCaption("Dump Trace"), false, atari.setTrace));
            Add(scrollBar = new Scroll(gui, new Rectangle(395 + 16, 0, 16, 208), new Range(0, 0), addressChanged));

            Add(address = new Lable(gui, new Rectangle(128, 0, 44,208), "", Alignment.RIGHT, Color.Black, DisplaySettings.fixedPointFont));
            Add(instruction = new Lable(gui, new Rectangle(176, 0, 136, 208), "", Alignment.LEFT, Color.White, DisplaySettings.fixedPointFont));
            Add(data = new Lable(gui, new Rectangle(316, 0, 79, 208), "", Alignment.LEFT, Color.LightBlue, DisplaySettings.fixedPointFont));

            Add(modEdit = new EditBox(gui, new Rectangle(304, 215, 64, 20), "", null));

            Add(new Radio(gui, new Rectangle(0, 40, 100, 16), radioList, baseChanged));
            Add(PCLable = new Lable(gui, new Rectangle(40, 84, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(ALable = new Lable(gui, new Rectangle(40, 104, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(XLable = new Lable(gui, new Rectangle(40, 124, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(YLable = new Lable(gui, new Rectangle(40, 144, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(SLable = new Lable(gui, new Rectangle(40, 164, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(PLable = new Lable(gui, new Rectangle(40, 184, 80, 16), "", Alignment.LEFT, Color.Black, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 84, 32, 16), "PC", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 104, 32, 16), "A", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 124, 32, 16), "X", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 144, 32, 16), "Y", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 164, 32, 16), "S", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Lable(gui, new Rectangle(0, 184, 32, 16), "P", Alignment.RIGHT, Color.White, DisplaySettings.fixedPointFont));
            Add(new Button(gui, new Rectangle(0, 212, 48, 26), new DisplayCaption("Run"), atari.run));
            Add(new Button(gui, new Rectangle(52, 212, 48, 26), new DisplayCaption("Stop"), atari.stop));
            Add(new Button(gui, new Rectangle(104, 212, 48, 26), new DisplayCaption("Step"), step));
            Add(new Button(gui, new Rectangle(156, 212, 72, 26), new DisplayCaption("Jump To"), jumpTo));
            Add(new Button(gui, new Rectangle(232, 212, 64, 26), new DisplayCaption("Run To"), runTo));

            focusAddress = atari.m_PC;

            baseChanged(0);
        }

        public void follow(bool f)
        {
            followPC = f;
        }

        public void baseChanged(int b)
        {
            if (b == 0)
                scrollBar.Limits = new Range(0x1000, 0x1FFF);
            else
                scrollBar.Limits = new Range(0x80, 0xFF);
        }

        private void runTo()
        {
            ushort address = Convert.ToUInt16(modEdit.Value,16);
            atari.runTo(address);
        }

        private void step()
        {
            atari.step();
        }

        private void jumpTo()
        {
            ushort address = Convert.ToUInt16(modEdit.Value, 16);
            scrollBar.Value = address;
            addressChanged(address);
        }

        public void addressChanged(int i)
        {
            focusAddress = (ushort)i;
            modEdit.Value = String.Format("{0:X4}", focusAddress);
        }

        public override void paint(SdlDotNet.Graphics.Surface display, bool focused, Point offset)
        {
            PCLable.Text = String.Format("{0:X4}", atari.m_PC);
            ALable.Text = String.Format("{0:X2}", atari.m_A);
            XLable.Text = String.Format("{0:X2}", atari.m_X);
            YLable.Text = String.Format("{0:X2}", atari.m_Y);
            SLable.Text = String.Format("{0:X2}", atari.m_S);

            string flags = "CZIDB1VS";
            string p = "";
            byte mp = atari.BuildStatus(false);

            for( int i = 0; i < 8; i++ )
            {
                if ((mp & 1 << i) != 0)
                {
                    p += flags[i];
                }
                else
                {
                    p+= "-";
                }
            }
            PLable.Text = String.Format(p);

            if (followPC)
            {
                addressChanged(atari.m_PC);
            }
            
            ushort pos = focusAddress;

            string adr = "";
            string dat = "";
            string ins = "";

            for (int i = 0; i < 13; i++)
            {
                adr += String.Format("{0:x4}\n", pos);
                ins += ((pos == atari.m_PC)?"--> ":"    ") + atari.instructionAsm(pos) + "\n";

                byte[] d = atari.instructionData(pos);
                for (int b = 0; b < d.Length; b++)
                {
                    dat += String.Format("{0:x2} ", d[b]);
                }
                dat += "\n";

                pos += (ushort)d.Length;
            }

            address.Text = adr;
            instruction.Text = ins;
            data.Text = dat;

            base.paint(display, focused, offset);
        }
    }
}
