using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using cstari.chemistry;
using cstari.chemistry.atoms;
using cstari.chemistry.captions;

using cstari.chips;
using SdlDotNet;
using SdlDotNet.Graphics;

namespace cstari.ui
{

    public class TiaDebugger : Molicule
    {
        private TIA tia;

        private Surface ColorBK;
        private Surface ColorPF;
        private Surface ColorP0;
        private Surface ColorP1;

        private Surface GRPF;
        private Surface GRP0;
        private Surface GRP1;
        private Surface GRP0A;
        private Surface GRP1A;

        private Surface GRBL;
        private Surface GRM0;
        private Surface GRM1;
        private Surface GRBLA;

        private Surface ScanLine;

        private Lable PosP0t;
        private Lable PosP1t;
        private Lable PosBLt;
        private Lable PosM0t;
        private Lable PosM1t;
        private Lable HPost;
        private Lable VPost;

        private Lable P0Del;
        private Lable P1Del;
        private Lable BLDel;

        private Lable moveP0;
        private Lable moveP1;
        private Lable moveBL;
        private Lable moveM0;
        private Lable moveM1;
        private Lable PFModes;

        static private Surface matrix;
        static private Dictionary<int, Point> matrixIcons;

        static TiaDebugger()
        {
            matrix = new Surface(Images.matrix);
            matrixIcons = new Dictionary<int,Point>();

            matrixIcons[(int)TIA.collisionIndicies.BL_PF_INDEX] = new Point(1 * 16, 5 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M0_BL_INDEX] = new Point(2 * 16, 5 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P0_BL_INDEX] = new Point(3 * 16, 5 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M1_BL_INDEX] = new Point(4 * 16, 5 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P1_BL_INDEX] = new Point(5 * 16, 5 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M0_PF_INDEX] = new Point(2 * 16, 4 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P0_PF_INDEX] = new Point(3 * 16, 4 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M1_PF_INDEX] = new Point(4 * 16, 4 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P1_PF_INDEX] = new Point(5 * 16, 4 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M0_P0_INDEX] = new Point(3 * 16, 3 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M1_M0_INDEX] = new Point(4 * 16, 3 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P1_M0_INDEX] = new Point(5 * 16, 3 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M1_P0_INDEX] = new Point(4 * 16, 2 * 16);
            matrixIcons[(int)TIA.collisionIndicies.P1_P0_INDEX] = new Point(5 * 16, 2 * 16);
            matrixIcons[(int)TIA.collisionIndicies.M1_P1_INDEX] = new Point(5 * 16, 1 * 16);        
        }

        public TiaDebugger(Compound gui, TIA t)
            : base(gui, new Rectangle(32, 32, 488, 300), new DisplayCaption("TIA Registers"), new List<Atom>())
        {
            tia = t;

            ColorBK = new Surface(new Size(16, 16));
            ColorPF = new Surface(new Size(16, 16));
            ColorP0 = new Surface(new Size(16, 16));
            ColorP1 = new Surface(new Size(16, 16));

            Add(PosP0t = new Lable(gui, new Rectangle(64, 140, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(PosP1t = new Lable(gui, new Rectangle(64, 160, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(PosBLt = new Lable(gui, new Rectangle(64, 180, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(PosM0t = new Lable(gui, new Rectangle(64, 200, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(PosM1t = new Lable(gui, new Rectangle(64, 220, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(HPost = new Lable(gui, new Rectangle(64, 40, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(VPost = new Lable(gui, new Rectangle(96, 40, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));

            Add(P0Del = new Lable(gui, new Rectangle(128, 140, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(P1Del = new Lable(gui, new Rectangle(128, 160, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(BLDel = new Lable(gui, new Rectangle(128, 180, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));

            GRPF = new Surface(new Size(320, 16));
            GRP0 = new Surface(new Size(128, 16));
            GRP1 = new Surface(new Size(128, 16));
            GRP0A = new Surface(new Size(128, 16));
            GRP1A = new Surface(new Size(128, 16));

            GRBL = new Surface(new Size(16, 16));
            GRM0 = new Surface(new Size(16, 16));
            GRM1 = new Surface(new Size(16, 16));
            GRBLA = new Surface(new Size(16, 16));

            ScanLine = new Surface(new Size(456, 4));

            for (int i = 0; i < 136; i++)
            {
                Rectangle r = new Rectangle(i, 0, 0, 3);

                ScanLine.Fill(r, Color.FromArgb(i * 255 / 136, i * 255 / 136, i * 255 / 136));
            }

            Add(moveP0 = new Lable(gui, new Rectangle(96, 140, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(moveP1 = new Lable(gui, new Rectangle(96, 160, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(moveBL = new Lable(gui, new Rectangle(96, 180, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(moveM0 = new Lable(gui, new Rectangle(96, 200, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(moveM1 = new Lable(gui, new Rectangle(96, 220, 48, 32), "0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));

            Add(new Lable(gui, new Rectangle(0, 80,64,48), "BK", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new Lable(gui, new Rectangle(64, 80, 64, 48), "PF", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(32, 80,0,0), ColorBK));
            Add(new StaticImage(gui, new Rectangle(32 + 64, 80, 0, 0), ColorPF));
            Add(new StaticImage(gui, new Rectangle(64 + 64, 80, 0, 0), GRPF));
            Add(new StaticImage(gui, new Rectangle(360 + 16, 204 - 8 - 32, 0, 0), matrix));
            Add(new Lable(gui, new Rectangle(0, 140, 64, 48), "P0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(32, 140, 0, 0), ColorP0));
            Add(new StaticImage(gui, new Rectangle(96 + 64, 140, 0, 0), GRP0));
            Add(new StaticImage(gui, new Rectangle(240 + 64, 140, 0, 0), GRP0A));
            Add(new Lable(gui, new Rectangle(0, 160, 64, 48), "P1", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(32, 160, 0, 0), ColorP1));
            Add(new StaticImage(gui, new Rectangle(96 + 64, 160, 0, 0), GRP1));
            Add(new StaticImage(gui, new Rectangle(240 + 64, 160, 0, 0), GRP1A));
            Add(new Lable(gui, new Rectangle(0, 180, 64, 48), "BL", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(96 + 64, 180, 0, 0), GRBL));
            Add(new StaticImage(gui, new Rectangle(128 + 64, 180, 0, 0), GRBLA));
            Add(new Lable(gui, new Rectangle(0, 200, 64, 48), "M0", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(96 + 64, 200, 0, 0), GRM0));
            Add(new Lable(gui, new Rectangle(0, 220, 64, 48), "M1", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(96 + 64, 220, 0, 0), GRM1));
            Add(new Lable(gui, new Rectangle(0, 40, 64, 48), "Raster", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new Lable(gui, new Rectangle(128, 120, 64, 48), "Del", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new Lable(gui, new Rectangle(96, 120, 64, 48), "HM", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new Lable(gui, new Rectangle(64, 120, 64, 48), "Pos", Alignment.LEFT, Color.White, DisplaySettings.captionFont));
            Add(new StaticImage(gui, new Rectangle(8, 16, 0, 0), ScanLine));
            Add(PFModes = new Lable(gui, new Rectangle(144, 80, 200, 32), "", Alignment.LEFT, Color.FromArgb(160, 160, 160), DisplaySettings.captionFont));
        }

        private void FillSurface(Surface bitmap, int source )
        {
            Rectangle r = new Rectangle(0,0,15,15);
            for (int i = 0; i < 32; i++)
            {
                if ((source & (0x80 >> i)) != 0)
                {
                    bitmap.Fill(r, Color.White);
                }
                r.X += 16;
            }
        }

        public override void paint(Surface display, bool focused, Point offset)
        {
            tia.ClockCatchUp();
            tia.ForceScanDraw();

            ColorBK.Fill(tia.ChromaBK);
            ColorPF.Fill(tia.ChromaPF);
            ColorP0.Fill(tia.ChromaP0);
            ColorP1.Fill(tia.ChromaP1);

            PosBLt.Text = Convert.ToString(tia.m_BLPos);
            PosP0t.Text = Convert.ToString(tia.m_P0Pos);
            PosM0t.Text = Convert.ToString(tia.m_M0Pos);
            PosP1t.Text = Convert.ToString(tia.m_P1Pos);
            PosM1t.Text = Convert.ToString(tia.m_M1Pos);
            HPost.Text = Convert.ToString(tia.m_hPos);
            VPost.Text = Convert.ToString(tia.m_vPos);

            string modes = "";
            if( tia.m_RefPF )
                modes = "Reflected  ";
            if ((tia.m_DrawMode & (int)TIA.playfieldModes.PF_SCORE) != 0)
                modes += "Score  ";
            if ((tia.m_DrawMode & (int)TIA.playfieldModes.PF_PRI) != 0)
                modes += "Priority";
            
            PFModes.Text = modes;

            if (tia.m_EnaBL)
                GRBL.Fill(Color.FromArgb(255, 255, 255));
            else
                GRBL.Fill(Color.FromArgb(0, 0, 0));

            if (tia.m_EnaM0)
                GRM0.Fill(Color.FromArgb(255, 255, 255));
            else
                GRM0.Fill(Color.FromArgb(0, 0, 0));

            if (tia.m_EnaM1)
                GRM1.Fill(Color.FromArgb(255, 255, 255));
            else
                GRM1.Fill(Color.FromArgb(0, 0, 0));

            if (tia.m_EnaBLDel)
                GRBLA.Fill(Color.FromArgb(255, 255, 255));
            else
                GRBLA.Fill(Color.FromArgb(0, 0, 0));

            if (tia.m_VDelP0)
                P0Del.Text = "X";
            else
                P0Del.Text = "";

            if (tia.m_VDelP1)
                P1Del.Text = "X";
            else
                P1Del.Text = "";

            if (tia.m_VDelP0)
                BLDel.Text = "X";
            else
                BLDel.Text = "";

            moveBL.Text = Convert.ToString(-tia.m_MoveBL);
            moveP0.Text = Convert.ToString(-tia.m_MoveP0);
            moveM0.Text = Convert.ToString(-tia.m_MoveM0);
            moveP1.Text = Convert.ToString(-tia.m_MoveP1);
            moveM1.Text = Convert.ToString(-tia.m_MoveM1);

            base.paint(display, focused, offset);

            Rectangle client = ClientRect;

            client.X += offset.X;
            client.Y += offset.Y;

            foreach (int i in matrixIcons.Keys)
            {
                Point p = matrixIcons[i];

                if (tia.m_Collisions[i] != 0)
                {
                    DisplaySettings.checkMatrix.Blit(matrix, p);
                }
                else
                {
                    DisplaySettings.uncheckMatrix.Blit(matrix, p);
                }
            }

            for (int i = 0; i < 160; i++)
            {
                Rectangle r = new Rectangle(136 + i * 2, 0, 2, 2);
                ScanLine.Fill(r, tia.GetColor(true, i));
                r.Y += 2;
                ScanLine.Fill(r, tia.GetColor(false, i));
            }

            GRPF.Fill(Color.Black);
            GRP0.Fill(Color.Black);
            GRP1.Fill(Color.Black);
            GRP0A.Fill(Color.Black);
            GRP1A.Fill(Color.Black);

            FillSurface(GRP0, tia.m_P0Grp);
            FillSurface(GRP1, tia.m_P1Grp);
            FillSurface(GRP0A, tia.m_P0GrpDel);
            FillSurface(GRP1A, tia.m_P1GrpDel);

            Rectangle rect = new Rectangle(0, 0, 16, 16);

            for (int i = 0; i < 4; i++)
            {
                if ((tia.m_PF0Grp & (0x10 << i))!=0)
                    GRPF.Fill(rect, Color.White);
                rect.X += 16;
            }
            for (int i = 0; i < 8; i++)
            {
                if ((tia.m_PF1Grp & (0x80 >> i))!=0)
                    GRPF.Fill(rect, Color.White);
                rect.X += 16;
            }
            for (int i = 0; i < 8; i++)
            {
                if ((tia.m_PF2Grp & (0x01 << i))!=0)
                    GRPF.Fill(rect, Color.White);
                rect.X += 16;
            }


            DisplaySettings.BLPos.Blit(display, new Point(8 + (68 + tia.m_BLPos) * 2 + client.X, client.Y));
            DisplaySettings.M0Pos.Blit(display, new Point(8 + (68 + tia.m_M0Pos) * 2 + client.X, client.Y));
            DisplaySettings.M1Pos.Blit(display, new Point(8 + (68 + tia.m_M1Pos) * 2 + client.X, client.Y));            
            DisplaySettings.P0Pos.Blit(display, new Point(8 + (68 + tia.m_P0Pos) * 2 + client.X, client.Y));
            DisplaySettings.P1Pos.Blit(display, new Point(8 + (68 + tia.m_P1Pos) * 2 + client.X, client.Y));
            DisplaySettings.HPos.Blit(display, new Point(8 + (68 + tia.m_hPos) * 2 + client.X, client.Y + 20));
        }
    }
}
