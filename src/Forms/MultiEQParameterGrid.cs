using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class MultiEQParameterGrid : XGParameterGrid
    {
        XGMultiEQParams EQParams;
        public MultiEQParameterGrid()
        {
            MidiProgramNumber.Init();
            InitializeComponent();
        }

        Color[] bandColors = new Color[5] { Color.Red, Color.Cyan, Color.Blue, Color.Lime, Color.Magenta };

        protected override void OnSetDevice()
        {
            base.OnSetDevice();
            EQParams = new XGMultiEQParams(Device);

            AddLabelCell("EQType", 0, 0, 3, Color.Olive);
            var typeCtrl = AddControlCell(EQParams.Type, 3, 0, 3, Color.Yellow);
            typeCtrl.Offset += v => { EQParams.RequestDump(); RedrawOnRequestComplete(); };

            int baseY = 8, y, x = 0;

            y = baseY;
            AddLabelCell("", x, y++, 3, Color.DarkGray);
            AddLabelCell("Shape", x, y++, 3, Color.DarkGray);
            AddLabelCell("Frequency", x, y++, 3, Color.DarkGray);
            AddLabelCell("Q", x, y++, 3, Color.DarkGray);
            AddLabelCell("Gain", x, y++, 3, Color.DarkGray);
            x += 3;

            for (int i = 0; i < 5; i++)
            {
                y = baseY;

                var band = EQParams.Bands[i];
                var brightColor = bandColors[i];
                var darkColor = Color.FromArgb(brightColor.A, brightColor.R / 2, brightColor.G / 2, brightColor.B / 2);

                AddLabelCell("Band " + (i + 1), x, y++, 3, darkColor);
                if (EQParams.Bands[i].Shape != null) { AddControlCell(band.Shape, x, y++, 3, brightColor).Offset += v => Invalidate(); }
                else { AddLabelCell("-", x, y++, 3, darkColor); }
                AddControlCell(band.Frequency, x, y++, 3, brightColor).Offset += v => Invalidate();
                AddControlCell(band.Q, x, y++, 3, brightColor).Offset += v => Invalidate();
                AddControlCell(band.Gain, x, y++, 3, brightColor).Offset += v => Invalidate();
                x += 3;
            }

            AdjustWindowSize();
        }

        private void buttonDumpAll_Click(object sender, EventArgs e)
        {
            EQParams.RequestDump();
            RedrawOnRequestComplete();
        }


        private void buttonSendAll_Click(object sender, EventArgs e)
        {
            EQParams.ReSendAll();
        }

        private void buttonToMML_Click(object sender, EventArgs e)
        {
            StringBuilder mml = new StringBuilder();
            mml.AppendLine(@"Function XGXcl1(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val & $7F), $F7; }");

            EQParams.Parameters.ForEach(p =>
            {
                if (p == null) { return; }
                mml.Append("r%1 ");
                mml.Append(p.Count == 1 ? "XGXcl1($" : "XGXclV($");
                mml.Append(p.Address.ToString("X06"));
                mml.Append(", $");
                mml.Append(p.Value.ToString(p.Count == 1 ? "X02" : "X04"));
                mml.Append("); // ");
                mml.Append(p.Name);
                mml.Append(" = ");
                mml.Append(p.ValueString);
                mml.AppendLine();
            });
            CopyToClipboard(mml.ToString());
        }

        static double window(double centerFreq, double q, double input, double gain, bool shelvingLow, bool shelvingHigh)
        {
            // だいたいそれっぽいうそかんすう
            if (shelvingLow && input < centerFreq) { return gain; }
            if (shelvingHigh && input > centerFreq) { return gain; }
            double minFreq = centerFreq - centerFreq / q * 2 / 3;
            double maxFreq = centerFreq + centerFreq / q * 4 / 3;
            if (input < minFreq) { return 0; }
            if (input > maxFreq) { return 0; }
            if (input < centerFreq) { return (1 - (centerFreq - input) / (centerFreq - minFreq)) * gain; }
            else { return (1 - (centerFreq - input) / (centerFreq - maxFreq)) * gain; }
        }


        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            if (EQParams != null)
            {
                Dictionary<int, string> line = new Dictionary<int, string>();
                for (int i = 1; i < 10; i++) { line.Add(i * 10, (i == 1 || i == 2 || i == 5) ? (i * 10).ToString() : null); }
                for (int i = 1; i < 10; i++) { line.Add(i * 100, (i == 1 || i == 2 || i == 5) ? (i * 100).ToString() : null); }
                for (int i = 1; i < 10; i++) { line.Add(i * 1000, (i == 1 || i == 2 || i == 5) ? (i).ToString() + "k" : null); }
                for (int i = 1; i < 10; i++) { line.Add(i * 10000, (i == 1 || i == 2 || i == 5) ? (i * 10).ToString() + "k" : null); }

                Graphics g = e.Graphics;
                using (Font font = new Font(Config.FontFace, Config.FontSize))
                using (Pen pen = new Pen(Brushes.Gray))
                using (Pen p = new Pen(Brushes.Bisque))
                {
                    var Bands = EQParams.Bands;
                    Rectangle eqImageBox = new Rectangle(6, Config.GridHeight * 3 + 6, ClientSize.Width - 12, Config.GridHeight * 6);
                    g.FillRectangle(Brushes.AliceBlue, eqImageBox);

                    double fStart = 19;
                    double fEnd = 21000;
                    double fBase = Math.Pow(fEnd / fStart, 1.0 / eqImageBox.Width);
                    double maxDecibel = 15;
                    int dotRadius = 4;

                    int Y = eqImageBox.Top;
                    int H = eqImageBox.Height;
                    int halfH = H / 2;
                    int centerY = Y + H / 2;

                    for (int x = eqImageBox.Left; x < eqImageBox.Right; x++)
                    {
                        double f = fStart * Math.Pow(fBase, x - eqImageBox.Left);
                        double db = 0;
                        for (int j = 0; j < Bands.Count; j++)
                        {
                            db += window(Bands[j].ActualFrequency, Bands[j].ActualQ, f, Bands[j].ActualGain, Bands[j].LowerShelving, Bands[j].HigherShelving);
                        }
                        db = Math.Min(Math.Max(db, -maxDecibel), maxDecibel);
                        g.DrawLine(p, x, Y + H, x, centerY + (int)(db / maxDecibel * -halfH));
                    }

                    double pf = fStart;
                    for (int x = eqImageBox.Left; x < eqImageBox.Right; x++)
                    {
                        double f = fStart * Math.Pow(fBase, x - eqImageBox.Left);
                       
                        // draw band ●
                        for (int j = 0; j < Bands.Count; j++)
                        {
                            if (pf < Bands[j].ActualFrequency && f >= Bands[j].ActualFrequency)
                            {
                                using (Brush b = new SolidBrush(bandColors[j]))
                                {
                                    Point pos = new Point(x, centerY + (int)(Bands[j].ActualGain / maxDecibel * -halfH));
                                    g.FillEllipse(b, new Rectangle(pos - new Size(dotRadius, dotRadius), new Size(dotRadius * 2, dotRadius * 2)));
                                }
                            }
                        }

                        // datum lines
                        foreach (var fr in line)
                        {
                            if (pf < fr.Key && f >= fr.Key)
                            {
                                g.DrawLine((fr.Value != null ? Pens.Black : Pens.LightGray), x, Y, x, Y + H);
                                if (fr.Value != null) { g.DrawString(fr.Value, font, Brushes.Black, x - 8, Y + H); }
                            }
                        }

                        pf = f;
                    }

                    g.DrawLine(Pens.Gray, eqImageBox.Left, centerY, eqImageBox.Right, centerY);
                    for (int i = 1; i <= maxDecibel / 3; i++)
                    {
                        int yy = (int)((i * 3) / maxDecibel * -halfH);
                        g.DrawLine(Pens.LightGray, eqImageBox.Left, centerY + yy, eqImageBox.Right, centerY + yy);
                        g.DrawLine(Pens.LightGray, eqImageBox.Left, centerY - yy, eqImageBox.Right, centerY - yy);
                    }

                    g.DrawString("EQ Image", font, Brushes.LightGray, eqImageBox.Location - new Size(2,2));
                    g.DrawString("EQ Image", font, Brushes.Black, eqImageBox.Location);

                }
            }

            base.OnPaint(e);
        }
    }
}
