using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using Tsukikage.SharpJson;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class DrumParameterGrid : XGParameterGrid
    {
        public DrumParameterGrid()
        {
            InitializeComponent();
            TargetMap = 1;
        }

        
        /// <summary>
        /// ターゲット
        /// </summary>
        [Browsable(true)]
        public int TargetMap { get; set; }

        [Browsable(true)]
        public int TargetChannel { get; set; }

        List<XGDrumParams> Drums = new List<XGDrumParams>();
        protected override void OnSetDevice()
        {
            TargetChannel = 10;
            if (TargetMap == 1) { TargetChannel = 10; }
            if (TargetMap == 2) { TargetChannel = 9; }
            if (TargetMap == 3) { TargetChannel = 26; }

            textBox1.Text = Config.drumMapInitial;

            numericUpDown1.Value = TargetChannel;
            textBox1_TextChanged(null, null);
            //ReCreateScreen(noteNumbers);
        }

        void ReCreateScreen(int[] noteNumbers)
        {
            XGControls.Clear();
            Drums.Clear();
            Array.Reverse(noteNumbers);
            {
                int x = 0;
                int y = 0;

                AddLabelCell("N#", x, y, 1, Color.Black); x++;

                AddLabelCell("ON", x, y, 1, Color.Gray); x++;

                AddLabelCell("NoteName", x, y, 4, Color.Gray);x += 4;
                AddLabelCell("PCs", x, y, 1, Color.Olive); x++;
                AddLabelCell("PTn", x, y, 1, Color.Purple); x++;

                AddLabelCell("VOL", x, y, 1, Color.Green); x++;
                AddLabelCell("PAN", x, y, 1, Color.Olive); x++;

                AddLabelCell("REV", x, y, 1, Color.Maroon); x++;
                AddLabelCell("CHO", x, y, 1, Color.Teal); x++;
                AddLabelCell("VAR", x, y, 1, Color.Navy); x++;

                AddLabelCell("LPF", x, y, 1, Color.Green); x++;
                AddLabelCell("Rsn", x, y, 1, Color.Green); x++;
                AddLabelCell("HPF", x, y, 1, Color.Green); x++;

                AddLabelCell("Atk", x, y, 1, Color.Teal); x++;
                AddLabelCell("Dc1", x, y, 1, Color.Teal); x++;
                AddLabelCell("Dc2", x, y, 1, Color.Teal); x++;

                AddLabelCell("Bf", x, y, 1, Color.Olive); x++;
                AddLabelCell("Bg", x, y, 1, Color.Olive); x++;
                AddLabelCell("Tf", x, y, 1, Color.Olive); x++;
                AddLabelCell("Tg", x, y, 1, Color.Olive); x++;
            }

            for (int i = 0; i < noteNumbers.Length; i++)
            {
                int x = 0;
                int y = i + 1;
                int n = noteNumbers[i];
                if (n < 13 || n > 91)
                {
                    AddTriggerCell(n.ToString(), x, y, 1, Color.Black, () => { }); x++;
                    continue;
                }
                    
                XGDrumParams param = new XGDrumParams(Device, TargetMap - 1, n);
                Drums.Add(param);

                XGPControlCell noteOnCell = AddTriggerCell(n.ToString(), x, y, 1, Color.Black, () => param.ReSendAll()); x++;
                bool noteOn = false;
                noteOnCell.Decrement = () =>
                {
                    if (!noteOn)
                    {
                        noteOn = true;
                        int noteNum = n;
                        int channel = TargetChannel - 1;
                        Device.Write(0x640090 | noteNum << 8 | channel);
                        while ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Right) != 0) { System.Threading.Thread.Sleep(0); System.Windows.Forms.Application.DoEvents(); }
                        Device.Write(0x000080 | noteNum << 8 | channel);
                        noteOn = false;
                    }
                };
                noteOnCell.GetDescriptionFunc = () => "DblClick: ReSend / RightClick: AUDITION";

                AddControlCell(param.RcvNoteOn, x, y, 1, Color.LightGray); x++;
                XGPControlCell toneNameCell =  AddLabelCell("", x, y, 4, Color.LightGray); x += 4;
                toneNameCell.GetTextFunc = () => {
                    var part = new XGPartParams(Device, TargetChannel - 1);
                    int bankMSB = part.ProgramMSB.Value;
                    int progNum = part.ProgramNumber.Value;
                    return MidiProgramNumber.GetDrumToneName(bankMSB, progNum, n);
                };



                AddControlCell(param.PitchCoarse, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.PitchFine, x, y, 1, Color.Magenta); x++;

                AddControlCell(param.Volume, x, y, 1, Color.Lime); x++;
                AddControlCell(param.Pan, x, y, 1, Color.Yellow); x++;

                AddControlCell(param.Reverb, x, y, 1, Color.Red); x++;
                AddControlCell(param.Chorus, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.Variation, x, y, 1, Color.Blue); x++;


                AddControlCell(param.LPFCutoffFreq, x, y, 1, Color.Lime); x++;
                AddControlCell(param.LPFResonance, x, y, 1, Color.Lime); x++;
                AddControlCell(param.HPFCutoffFreq, x, y, 1, Color.Lime); x++;

                AddControlCell(param.EGAttackRate, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.EGDecay1Rate, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.EGDecay2Rate, x, y, 1, Color.Cyan); x++;

                AddControlCell(param.EQBassFreq, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQBassGain, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQTrebleFreq, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQTrebleGain, x, y, 1, Color.Yellow); x++;
            }
            AdjustWindowSize();
            textBox1.Width = (this.ClientSize.Width - textBox1.Left - 12);
        }

        private void buttonDumpAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Drums.Count; i++)
            {
                Drums[i].RequestDump();
            }
            RedrawOnRequestComplete();
        }


        private void buttonSendAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Drums.Count; i++)
            {
                Drums[i].ReSendAll();
            }
        }

        private void buttonToMML_Click(object sender, EventArgs e)
        {
            StringBuilder mml = new StringBuilder();
            mml.Append(
@"Function DrumParam(Int _n, Int _pco, Int _pft, Int _vol, Int _pan, Int _rev, Int _cho, Int _var,
    Int _lpf, Int _rsn, Int _hpf, Int _atk, Int _dc1, Int _dc2, Int _bfr, Int _bga, Int _tfr, Int _tga)
{
    NRPN($18,_n,_pco) NRPN($19,_n,_pft) NRPN($1A,_n,_vol) NRPN($1C,_n,_pan)
    NRPN($1D,_n,_rev) NRPN($1E,_n,_cho) NRPN($1F,_n,_var) 
    NRPN($14,_n,_lpf) NRPN($15,_n,_rsn) NRPN($24,_n,_hpf) NRPN($16,_n,_atk) NRPN($17,_n,_dc1)
    NRPN($34,_n,_bfr) NRPN($30,_n,_bga) NRPN($35,_n,_tfr) NRPN($31,_n,_tga)
}

");
            for (int i = 0; i < Drums.Count; i++)
            {
                XGDrumParams p = Drums[i];
                int[] arr = { p.NoteNumber, 
                             p.PitchCoarse.Value, p.PitchFine.Value,
                             p.Volume.Value,p.Pan.Value,p.Reverb.Value,p.Chorus.Value,p.Variation.Value,
                             p.LPFCutoffFreq.Value, p.LPFResonance.Value, p.HPFCutoffFreq.Value,
                             p.EGAttackRate.Value, p.EGDecay1Rate.Value, p.EGDecay2Rate.Value,
                             p.EQBassFreq.Value, p.EQBassGain.Value, p.EQTrebleFreq.Value, p.EQTrebleGain.Value };

                mml.AppendLine("DrumParam(" + string.Join(",", Array.ConvertAll(arr, v => v.ToString())) + ");");
            }
            CopyToClipboard(mml.ToString());
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Var v = Var.FromFormattedString("[" + textBox1.Text + "]");
                ReCreateScreen((int[])v);
                Invalidate();
                textBox1.BackColor = Color.White;
            }
            catch
            {
                textBox1.BackColor = Color.MistyRose;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (numericUpDown1.Value >= 1 && numericUpDown1.Value < 64)
            {
                TargetChannel = (int)numericUpDown1.Value;
                Invalidate();
                textBox1.BackColor = Color.White;
            }
            else
            {
                numericUpDown1.BackColor = Color.MistyRose;

            }
        }
    }
}
