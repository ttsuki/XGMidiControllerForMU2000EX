using System;
using System.Drawing;
using System.Windows.Forms;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class MainForm : Form
    {
        private XGMidiIODevice Device { get; set; }
        private readonly int x_;
        private readonly int y_;

        public MainForm()
        {
            this.Font = new System.Drawing.Font(Config.FontFace, Config.FontSize);
            this.Text = Util.EntryAssemblyInformation.Title;

            Device = new XGMidiIODevice();
            Device.ResetXG();
            InitializeComponent();
            this.midiDeviceSelector1.SetDevice(Device); this.midiDeviceSelector1.SizeChanged += (s, e) => FitClientSize();
            this.partParameterGrid1.SetDevice(Device); this.partParameterGrid1.SizeChanged += (s, e) => FitClientSize();
            this.drumParameterGrid1.SetDevice(Device); this.drumParameterGrid1.SizeChanged += (s, e) => FitClientSize();
            this.drumParameterGrid2.SetDevice(Device); this.drumParameterGrid2.SizeChanged += (s, e) => FitClientSize();
            this.drumParameterGrid3.SetDevice(Device); this.drumParameterGrid3.SizeChanged += (s, e) => FitClientSize();
            this.drumParameterGrid4.SetDevice(Device); this.drumParameterGrid4.SizeChanged += (s, e) => FitClientSize();
            this.effectParameterGrid1.SetDevice(Device); this.effectParameterGrid1.SizeChanged += (s, e) => FitClientSize();
            this.effectParameterGrid2.SetDevice(Device); this.effectParameterGrid2.SizeChanged += (s, e) => FitClientSize();
            this.multiEQParameterGrid1.SetDevice(Device); this.multiEQParameterGrid1.SizeChanged += (s, e) => FitClientSize();
            
            x_ = this.Size.Width - tabControl1.SelectedTab.ClientSize.Width;
            y_ = this.Size.Height - tabControl1.SelectedTab.ClientSize.Height;
            FitClientSize();

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Device != null)
            {
                Device.Dispose();
                Device = null;
            }
        }

        void FitClientSize()
        {
            Size sz = tabControl1.SelectedTab.Controls[0].Size + new Size(12, 12);
            this.Size = sz + new Size(x_, y_);
            effectParameterGrid1.ReCreateScreen();
            effectParameterGrid2.ReCreateScreen();
            tabControl1.Invalidate();

        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            FitClientSize();
        }

        private void midiDeviceSelector1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
