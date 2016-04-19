using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class XGParameterGrid : UserControl
    {
        protected List<XGPControlCell> XGControls;
        protected XGMidiIODevice Device;
        protected string Status;
        int CellW = Config.GridWidth, CellH = Config.GridHeight;

        public XGParameterGrid()
        {
            InitializeComponent();
            XGControls = new List<XGPControlCell>();

            this.DoubleBuffered = true;
            timer1.Enabled = true;
            //timer1.Tick += (s, e) => Invalidate();
        }

        public void SetDevice(XGMidiIODevice device)
        {
            Debug.Assert(this.Device == null);
            this.Device = device;

            OnSetDevice();
            AdjustWindowSize();
        }

        protected virtual void OnSetDevice() { }

        protected void DoFunc(Predicate pred, Action func)
        {
            EventHandler handler = null;
            handler = (object sender, EventArgs e) =>
            {
                if (pred())
                {
                    timer1.Tick -= handler;
                    func();
                }
            };
            timer1.Tick += handler;
        }

        private void PartParameter_Paint(object sender, PaintEventArgs e)
        {
            PaintScreen(e.Graphics);
        }

        void PaintScreen(Graphics g)
        {
            foreach (var ctrl in XGControls) { ctrl.Paint(g); }

            {
                XGPControlCell ctrl = HitTest(mousePos);
                string status = "";
                Color color = Color.LightGray;
                if (ctrl != null)
                {
                    color = ctrl.BaseColor;
                    status = ctrl.Description;
                }

                SizeF sz = g.MeasureString(status, XGPControlCell.Font);
                Color darkColor = Color.FromArgb(color.A, color.R / 4, color.G / 4, color.B / 4);
                using (Brush darkBrush = new SolidBrush(darkColor))
                {
                    //g.FillRectangle(darkBrush, new RectangleF(new Point(6, ClientSize.Height - 24), sz));
                    g.DrawString(Status, XGPControlCell.Font, darkBrush, new Point(6, ClientSize.Height - 24));
                }

            }
        }

        public XGPControlCell AddLabelCell(string label, int x, int y, int w, Color c)
        {
            XGPControlCell ctrl = AddControlCell(null, x, y, w, c);
            ctrl.Text = label;
            return ctrl;
        }

        public XGPControlCell AddTriggerCell(string label, int x, int y, int w, Color c, Action act)
        {
            XGPControlCell ctrl = AddControlCell(null, x, y, w, c);
            ctrl.Text = label;
            ctrl.Trigger = act;
            return ctrl;
        }

        public XGPControlCell AddControlCell(XGMidiParameter target, int x, int y, int w, Color c)
        {
            y += 2;
            XGPControlCell ctrl = new XGPControlCell()
            {
                Location = new Point(CellW * x + 6, CellH * y + 6),
                Size = new Size(CellW * w, CellH),
                BaseColor = c,
                TargetParameter = target,
            };
            XGControls.Add(ctrl);
            return ctrl;
        }

        public void AdjustWindowSize()
        {
            int maxX = 0, maxY = 0;
            for (int i = 0; i < XGControls.Count; i++)
            {
                maxX = Math.Max(maxX, XGControls[i].Location.X + XGControls[i].Size.Width);
                maxY = Math.Max(maxY, XGControls[i].Location.Y + XGControls[i].Size.Height);
            }
            maxY += 24;
            this.ClientSize = new Size(maxX + 6, maxY + 6);
        }

        protected void CopyToClipboard(string text)
        {
            try
            {
                Clipboard.SetText(text.ToString());
                MessageBox.Show("クリップボードにコピーしました\n" + text.ToString(), "Succeeded.", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                MessageBox.Show("クリップボードにコピー失敗", "Failed.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }


        #region mouse

        int mouseRepeatCount = 0;
        int mouseOffset = 0;
        Point mousePos = Point.Empty;
        XGPControlCell lastHitObject = null;
        private void PartParameter_MouseDown(object sender, MouseEventArgs e)
        {
            mouseRepeatCount = 0;
            mousePos = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.Left) { if (mouseOffset == -1) { mouseOffset = -10; } else { mouseOffset = 1; } }
            if (e.Button == MouseButtons.Right) { if (mouseOffset == 1) { mouseOffset = 10; } else { mouseOffset = -1; } }

            if (e.Button == MouseButtons.Middle)
            {
                XGPControlCell ctrl = HitTest(e.Location);
                if (ctrl!=null && ctrl.TargetParameter!=null)
                {
                    ctrl.TargetParameter.Pick();
                    RedrawOnRequestComplete();
                }
            }
        }

        public void RedrawOnRequestComplete()
        {
            DoFunc(() => Device.AllDumpRequestHasDone, Invalidate);
        }

        private void PartParameter_MouseUp(object sender, MouseEventArgs e)
        {
            mouseRepeatCount = 0;
            mousePos = new Point(e.X, e.Y);
            if (e.Button == MouseButtons.Left) { if (mouseOffset == 10 || mouseOffset == -10) { mouseOffset = -1; } else { mouseOffset = 0; } }
            if (e.Button == MouseButtons.Right) { if (mouseOffset == 10 || mouseOffset == -10) { mouseOffset = 1; } else { mouseOffset = 0; } }
        }

        private void PartParameter_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos = new Point(e.X, e.Y);
            string s = HittestAndDescription(mousePos);
            if (s != Status) { Status = s; }
        }

        private void XGPGrid_DoubleClick(object sender, EventArgs e)
        {
            HittestAndTrigger(mousePos);
        }

        int TickMouse()
        {
            int ret = 0;
            int repeatInterval = 7;
            switch (mouseOffset)
            {
                case 1: if (mouseRepeatCount == 0 || mouseRepeatCount >= repeatInterval * 2 && mouseRepeatCount % repeatInterval == 0) { ret = 1; } break;
                case -1: if (mouseRepeatCount == 0 || mouseRepeatCount >= repeatInterval * 2 && mouseRepeatCount % repeatInterval == 0) { ret = -1; } break;
                case 10: if (mouseRepeatCount < 150) { ret = 1; } else { ret = 30; } break;
                case -10: if (mouseRepeatCount < 150) { ret = -1; } else { ret = -30; } break;
            }
            if (mouseOffset == 0)
            {
                mouseRepeatCount = 0;
            }
            else
            {
                mouseRepeatCount++;
            }
            return ret;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            HitTest(mousePos);
            int p = TickMouse();
            if (p != 0) { HittestAndOffsetValue(mousePos, p); }

            //Invalidate();
        }


        #endregion

        protected XGPControlCell HitTest(Point p)
        {
            XGPControlCell hit = XGControls.Find(c => c.HitTestRect.Contains(p));
            
            if (lastHitObject != hit)
            {
                mouseRepeatCount = 0;
                lastHitObject = hit;
                Invalidate(new Rectangle(0, ClientSize.Height - 24, ClientSize.Width, 24));
            }
            return hit;
        }

        protected void HittestAndOffsetValue(Point p, int val)
        {
            XGPControlCell c = HitTest(p);
            if (c != null)
            {
                if (val == 1) { c.Increment(); }
                else if (val == -1) { c.Decrement(); }
                else { c.Offset(val); }
                Invalidate(c.HitTestRect);
                for (int i = 0; i < c.RelativeXGP.Count; i++)
                {
                    Invalidate(c.RelativeXGP[i].HitTestRect);
                }
            }
        }

        protected string HittestAndDescription(Point p)
        {
            XGPControlCell c = HitTest(p);
            return c != null ? c.Description : "";
        }

        protected void HittestAndTrigger(Point p)
        {
            XGPControlCell c = HitTest(p);
            if (c != null && c.Trigger != null) { c.Trigger.Invoke(); }
        }
        
        public class XGPControlCell
        {
            public Point Location;
            public Size Size;
            public Color BaseColor;
            public bool Enabled;
            public static Font Font;
            public static StringFormat Format;
            public static Dictionary<Color, Brush> Brushes = new Dictionary<Color, Brush>();

            public static Brush GetBrush(Color color)
            {
                lock (Brushes)
                {
                    Brush brush;
                    if (Brushes.TryGetValue(color, out brush)) { return brush; }
                    return Brushes[color] = new SolidBrush(color);
                }
            }

            public XGMidiParameter TargetParameter;
            public List<XGPControlCell> RelativeXGP;
            public string Text = "---";

            public Getter<string> GetTextFunc;
            public Getter<string> GetDescriptionFunc;
            public Action Trigger;
            public Action<int> Offset;
            public Action Increment;
            public Action Decrement;
            public Rectangle HitTestRect { get { return new Rectangle(Location, Size); } }

            public string Description { get { return GetDescriptionFunc(); } }

            static XGPControlCell()
            {
                Font = new Font(Config.FontFace, Config.FontSize);
                Format = new StringFormat(StringFormat.GenericTypographic);
                Format.Alignment = StringAlignment.Far;
                Format.LineAlignment = StringAlignment.Far;
                Format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            }

            public XGPControlCell()
            {
                RelativeXGP = new List<XGPControlCell>();
                Enabled = true;

                Offset = ofst =>
                {
                    if (TargetParameter == null) { return; }
                    TargetParameter.OffsetValue(ofst);
                };
                GetTextFunc = () => TargetParameter != null ? TargetParameter.ValueString : Text;
                GetDescriptionFunc = () => TargetParameter != null ? (TargetParameter.Description ?? TargetParameter.Name ?? "") : "";
                Increment = () => Offset(1);
                Decrement = () => Offset(-1);
                Trigger = () => { };
            }

            public virtual void Paint(Graphics g)
            {
                Color darkColor = Color.FromArgb(BaseColor.A, BaseColor.R / 2, BaseColor.G / 2, BaseColor.B / 2);
                Brush brightBrush = GetBrush(BaseColor);
                Brush darkBrush = GetBrush(darkColor);
                Brush textBrush = Enabled ? GetBrush(Color.White) : GetBrush(Color.Gray);

                g.FillRectangle(darkBrush, new Rectangle(Location, Size));

                string valString = GetTextFunc();
                Rectangle textRect = new Rectangle(Location.X - 20, Location.Y, Size.Width - 2 + 20, Size.Height - 2);
                g.DrawString(valString, Font, textBrush, textRect, Format);

                // bar
                int w = (int)((Size.Width - 2) * GetRate());
                int c = (int)((Size.Width - 2) * GetCenterRate());
                int s = Math.Min(w, c);
                int e = Math.Max(w, c);
                if (TargetParameter != null)
                {
                    if (s == e)
                    {
                        e = s + 1;
                        if (GetRate() == GetCenterRate())
                        {
                            brightBrush = GetBrush(Color.Gray);
                        }
                    }
                }
                g.FillRectangle(brightBrush, 
                    new Rectangle(Location + new Size(s + 1, this.Size.Height - 3),
                    new Size(e - s, 2)));
            }

            float GetRate()
            {
                if (TargetParameter == null) { return 0f; }
                return (TargetParameter.Value - TargetParameter.MinValue) / (float)(TargetParameter.MaxValue - TargetParameter.MinValue) + 0.0001f;
            }

            float GetCenterRate()
            {
                if (TargetParameter == null) { return 0f; }
                return (TargetParameter.CenterValue - TargetParameter.MinValue) / (float)(TargetParameter.MaxValue - TargetParameter.MinValue) + 0.0001f;
            }
        }
    }
}
