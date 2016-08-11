using System;
using System.Drawing;
using System.Text;
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

        string CreateMmlText()
        {
            StringBuilder mml = new StringBuilder();
            mml.AppendLine(@" // Functions // 
Function XGXcl1(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val & $7F), $F7; }
Function XGXclN(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val / 4096 & $7F), (_val / 256 & $7F), (_val / 16 & $7F), (_val & $7F), $F7; }
Function XGXclV(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val / 128 & $7F), (_val & $7F), $F7; }
Function XGNrpn(Int _msb, Int _lsb, Int _data) { y99,_msb y98,_lsb y6,_data }
Function ProgramChange(Int _data, Int _msb, Int _lsb) { y0,_msb y32,_lsb @_data }
");
            mml.AppendLine("// Global //");
            mml.AppendLine("XGXcl1($00007E,$00); r8 // XG SYSTEM ON");
            mml.AppendLine(this.midiDeviceSelector1.GetMmlText());
            mml.AppendLine("// Effect RCV //");
            mml.AppendLine(this.effectParameterGrid1.GetMmlText());
            mml.AppendLine("// Effect Insertion 1-4 //");
            mml.AppendLine(this.effectParameterGrid2.GetMmlText());
            mml.AppendLine("// Effect EQ //");
            mml.AppendLine(this.multiEQParameterGrid1.GetMmlText());
            mml.AppendLine("// Parts //");
            mml.AppendLine(this.partParameterGrid1.GetMmlText());
            mml.AppendLine("// Drums //");
            mml.AppendLine(this.drumParameterGrid1.GetMmlFuncText());
            mml.AppendLine(this.drumParameterGrid1.GetMmlDataText());
            mml.AppendLine(this.drumParameterGrid2.GetMmlDataText());
            mml.AppendLine(this.drumParameterGrid3.GetMmlDataText());
            mml.AppendLine(this.drumParameterGrid4.GetMmlDataText());

            return mml.ToString();
        }

        private void copyToClipboardAsMMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string mml = CreateMmlText();
            Clipboard.SetText(mml);
            MessageBox.Show("Copied.");
        }
    }
}
