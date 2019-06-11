using System;
using System.Drawing;
using System.Windows.Forms;

namespace JumpKing_Graph {
    public partial class Form2 : Form {
        const UInt16 MAXTHINGS = 130;
        const UInt16 MARGIN = 8;
        const UInt16 GridLineCount = 8;
        float xDiv = 2;

        UInt16[] stagesbuffer = new UInt16[MAXTHINGS];
        Int16 lastBufferSlot = 0;
        UInt16 PB = 0;

        Pen LinePen = new Pen(Color.White, 1);
        Pen LineShadow = new Pen(Color.FromArgb(200, Color.Black), 3);
        Pen GridPen = new Pen(Color.Black, 1);
        SolidBrush GridTextBrush = new SolidBrush(Color.FromArgb(200, 80, 80, 80));
        SolidBrush PBBrush = new SolidBrush(Color.FromArgb(200, 80, 120, 80));
        Font GridFont = new Font("Calibri", 15, FontStyle.Bold);

        Font ErrFont = new Font("Calibri", 20, FontStyle.Bold);
        SolidBrush ErrTextBrush = new SolidBrush(Color.FromArgb(150, 255, 0, 0));

        Random x = new Random();
        JumpKingSDK sdk = new JumpKingSDK();

        public Form2() {
            InitializeComponent();

            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);

            try {
                sdk.Init();
            } catch(Exception e) {

            }
        }

        private void timer1_Tick(object sender, EventArgs e) {
#if !DEBUG
            if (!sdk.IsReady()) {
                try {
                    sdk.Init();
                } catch (Exception ex) {

                }

                this.Invalidate();

                return;
            }
#endif

            UInt16 currStage;
#if DEBUG
            Int16 currStageI = (Int16)(x.Next(-2, 3) + stagesbuffer[lastBufferSlot]);

            if(currStageI < 0)
                currStageI = 0;

            currStage = (UInt16)currStageI;
#else
            currStage = sdk.GetCurrentScreen();
#endif

            if(PB < currStage)
                PB = currStage;

            if(stagesbuffer[lastBufferSlot] != currStage) {
                if(lastBufferSlot == 0)
                    stagesbuffer[lastBufferSlot] = currStage;

                if(lastBufferSlot < MAXTHINGS - 1)
                    lastBufferSlot++;
                else
                    Array.Copy(stagesbuffer, 1, stagesbuffer, 0, stagesbuffer.Length - 1);

                stagesbuffer[lastBufferSlot] = currStage;

                this.Invalidate();
            }
        }

        private void DrawGraph(Graphics g) {
            SizeF strDim;

            if(!sdk.IsReady()) {
                String err = sdk.HasProcess() ? "Screen Mem-location not found" : "JumpKing Process not found!";

                strDim = g.MeasureString(err, GridFont);

                g.DrawString(err, GridFont, ErrTextBrush, (ClientSize.Width / 2) - (strDim.Width / 2), (ClientSize.Height / 2) - (strDim.Height / 2));
            }

            UInt16 yMin = UInt16.MaxValue;

            for(UInt16 i = 0; i <= lastBufferSlot; i++) {
                if(stagesbuffer[i] < yMin)
                    yMin = stagesbuffer[i];
            }

            float yDiv = ((float)(ClientSize.Height - (MARGIN * 2)) / (float)(PB - yMin));

            float UsePB = Math.Max(GridLineCount, PB);

            UInt16 SplitLines = (UInt16)Math.Round((UsePB - yMin) / (float)GridLineCount);
            if(SplitLines < 1)
                SplitLines = 1;

            //Grid
            for(UInt16 i = yMin; i < UsePB; i += SplitLines) {
                String iString = i.ToString();

                UInt16 yHeight = (UInt16)(ClientSize.Height - Math.Round(yDiv * (i - yMin), 0) - MARGIN);

                g.DrawLine(GridPen, 0, yHeight, ClientSize.Width, yHeight);

                strDim = g.MeasureString(iString, GridFont);

                g.DrawString(iString, GridFont, GridTextBrush, (ClientSize.Width / 2) - (strDim.Width / 2), yHeight - (strDim.Height / 2));
            }
            
            strDim = g.MeasureString("git.io/fjg0c", GridFont);

            g.DrawString("git.io/fjg0c", GridFont, GridTextBrush, (ClientSize.Width / 2) - (strDim.Width / 2), (int)Math.Round((float)ClientRectangle.Height * 0.91) - (strDim.Height / 2));


            if(lastBufferSlot < 1)
                return;

            Point[] points = new Point[lastBufferSlot + 1];
            UInt16 pt = 0;

            //Graph
            for(UInt16 i = 0; i < points.Length; i++) {
                UInt16 yPos = (UInt16)(ClientSize.Height - Math.Round(yDiv * (float)(stagesbuffer[i] - yMin), 0) - MARGIN);

                points[pt++] = new Point((UInt16)Math.Round((float)i * xDiv) + MARGIN, yPos);
            }

            g.DrawCurve(LineShadow, points, 0.2F);
            g.DrawCurve(LinePen, points, 0.2F);

            g.DrawString(String.Format("Highest: {0}", PB), GridFont, PBBrush, 0, 0);
        }

        private void Form2_Paint(object sender, PaintEventArgs e) {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            DrawGraph(e.Graphics);
        }

        private void Form2_VisibleChanged(object sender, EventArgs e) {
            xDiv = ((float)(ClientRectangle.Width - (MARGIN * 2)) / (float)MAXTHINGS);
        }
    }
}
