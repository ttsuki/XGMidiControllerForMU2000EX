﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class PartParameterGrid : XGParameterGrid
    {
        public PartParameterGrid()
        {
            MidiProgramNumber.Init();
            InitializeComponent();
        }

        List<XGPartParams> Channels = new List<XGPartParams>();
        protected override void OnSetDevice()
        {
            base.OnSetDevice();
            {
                int x = 0;
                int y0 = 0;
                int y1 = 1;
                AddLabelCell("Part", x, y0, 3, Color.Black);
                AddLabelCell("CH#", x, y1, 1, Color.Black); x++;
                AddLabelCell("ON", x, y1, 1, Color.DarkGray); x++;
                AddLabelCell("Md", x, y1, 1, Color.DarkGray); x++;

                AddLabelCell("Program", x, y0, 6, Color.Green);
                AddLabelCell("MSB", x, y1, 1, Color.Green); x++;
                AddLabelCell("LSB", x, y1, 1, Color.Green); x++;
                AddLabelCell("PG#", x, y1, 1, Color.Green); x++;
                AddLabelCell("", x, y1, 3, Color.Green); x += 3;

                AddLabelCell("Part/Effect", x, y0, 6, Color.Gray);
                AddLabelCell("VOL", x, y1, 1, Color.Green); x++;
                AddLabelCell("PAN", x, y1, 1, Color.Olive); x++;
                //AddLabelCell("MOD", x, y1, 1, Color.Blue); x++;

                AddLabelCell("REV", x, y1, 1, Color.Maroon); x++;
                AddLabelCell("CHO", x, y1, 1, Color.Teal); x++;
                AddLabelCell("VAR", x, y1, 1, Color.Navy); x++;
                AddLabelCell("DRY", x, y1, 1, Color.Olive); x++;

                AddLabelCell("Fillter", x, y0, 3, Color.Green);
                AddLabelCell("LPF", x, y1, 1, Color.Green); x++;
                AddLabelCell("Rsn", x, y1, 1, Color.Green); x++;
                AddLabelCell("HPF", x, y1, 1, Color.Green); x++;

                AddLabelCell("EG", x, y0, 3, Color.Teal);
                AddLabelCell("Atk", x, y1, 1, Color.Teal); x++;
                AddLabelCell("Dcy1", x, y1, 1, Color.Teal); x++;
                AddLabelCell("Rls", x, y1, 1, Color.Teal); x++;

                AddLabelCell("VIBRATO", x, y0, 3, Color.Maroon);
                AddLabelCell("Rat", x, y1, 1, Color.Maroon); x++;
                AddLabelCell("Dpt", x, y1, 1, Color.Maroon); x++;
                AddLabelCell("Dly1", x, y1, 1, Color.Maroon); x++;

                AddLabelCell("EQ", x, y0, 4, Color.Olive);
                AddLabelCell("BFr", x, y1, 1, Color.Olive); x++;
                AddLabelCell("BGa", x, y1, 1, Color.Olive); x++;
                AddLabelCell("TFr", x, y1, 1, Color.Olive); x++;
                AddLabelCell("TGa", x, y1, 1, Color.Olive); x++;

                //AddLabelCell("MW", x, y0, 6, Color.Purple);
                //AddLabelCell("Pit", x, y1, 1, Color.Purple); x++;
                //AddLabelCell("LPF", x, y1, 1, Color.Purple); x++;
                //AddLabelCell("Amp", x, y1, 1, Color.Purple); x++;
                //AddLabelCell("PMd", x, y1, 1, Color.Purple); x++;
                //AddLabelCell("FMd", x, y1, 1, Color.Purple); x++;
                //AddLabelCell("AMd", x, y1, 1, Color.Purple); x++;
            }

            for (int i = 0; i < 16; i++)
            {
                int x = 0;
                int y = i + 2;
                int channel = i;
                XGPControlCell cell;
                XGPartParams param = new XGPartParams(Device, channel);
                Channels.Add(param);

                // #xx
                cell = AddTriggerCell((i + 1).ToString(), x, y, 1, Color.Black, () => param.ReSendAll()); x++;
                bool noteOn = false;
                cell.Decrement = () =>
                {
                    if (!noteOn)
                    {
                        noteOn = true;
                        int noteNum = param.ProgramMSB.Value == 126 || param.ProgramMSB.Value == 127 ? 38 : 0x3C;
                        Device.Write(0x640090 | noteNum << 8 | channel);
                        while ((System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Right) != 0)
                        {
                            System.Threading.Thread.Sleep(0); System.Windows.Forms.Application.DoEvents();
                        }
                        Device.Write(0x000080 | noteNum << 8 | channel);
                        noteOn = false;
                    }
                };
                cell.GetDescriptionFunc = () => "DblClick: ReSend / RightClick: AUDITION";


                // ON/OFF
                cell = AddControlCell(param.RcvNoteMessage, x, y, 1, Color.LightGray); x++;
                cell.Decrement += () => { Device.Write(0x0078B0 + channel); }; // all sound off
                cell.Trigger = () =>
                    {
                        for (int j = 0; j < 16; j++)
                        {
                            if (j != param.Channel)
                            {
                                Device.Write(0x0078B0 + j);
                            }
                            Device.WriteXGParam(0x080035 | j << 8, 1, j == param.Channel ? 1 : 0);
                            Device.SendXGParameterByValue(0x080035 | j << 8, j == param.Channel ? 1 : 0, 1);
                        }
                        Invalidate();
                    };


                // #mode
                cell = AddControlCell(param.PartMode, x, y, 1, Color.LightGray); x++;

                var msbCell = AddControlCell(param.ProgramMSB, x, y, 1, Color.Lime); x++;
                var lsbCell = AddControlCell(param.ProgramLSB, x, y, 1, Color.Lime); x++;
                var pgnCell = AddControlCell(param.ProgramNumber, x, y, 1, Color.Lime); x++;
                var pgnameCell = AddLabelCell("", x, y, 3, Color.Lime); x += 3;

                Getter<MidiProgramNumber> getCurrentProgram = () => new MidiProgramNumber(msbCell.TargetParameter.Value, lsbCell.TargetParameter.Value, pgnCell.TargetParameter.Value);
                Action<MidiProgramNumber> setCurrentProgram = pn =>
                {
                    msbCell.TargetParameter.Value = pn.BankMSB;
                    lsbCell.TargetParameter.Value = pn.BankLSB;
                    pgnCell.TargetParameter.Value = pn.ProgramNumber;
                };

                pgnameCell.GetTextFunc = () => MidiProgramNumber.GetToneName(getCurrentProgram());
                pgnameCell.Increment = () => setCurrentProgram(MidiProgramNumber.FindSameMSBNextTone(getCurrentProgram()));
                pgnameCell.Decrement = () => setCurrentProgram(MidiProgramNumber.FindSameMSBPreviousTone(getCurrentProgram()));
                pgnameCell.GetDescriptionFunc = () => "Left/Right Click: Set (Next or Previous) Tone";

                pgnameCell.RelativeXGP.Add(msbCell);
                pgnameCell.RelativeXGP.Add(lsbCell);
                pgnameCell.RelativeXGP.Add(pgnCell);
                msbCell.RelativeXGP.Add(pgnameCell);
                lsbCell.RelativeXGP.Add(pgnameCell);
                pgnCell.RelativeXGP.Add(pgnameCell);
                msbCell.Increment += () => pgnCell.Offset(0);
                msbCell.Decrement += () => pgnCell.Offset(0);
                lsbCell.Increment += () => pgnCell.Offset(0);
                lsbCell.Decrement += () => pgnCell.Offset(0);

                AddControlCell(param.Volume, x, y, 1, Color.Lime); x++;
                AddControlCell(param.Pan, x, y, 1, Color.Yellow); x++;
                //cell = AddControlCell(param.Modulation, x, y, 1, Color.Blue); x++;

                AddControlCell(param.Reverb, x, y, 1, Color.Red); x++;
                AddControlCell(param.Chorus, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.Variation, x, y, 1, Color.Blue); x++;
                AddControlCell(param.DryLevel, x, y, 1, Color.Yellow); x++;

                AddControlCell(param.LPFCutoffFreq, x, y, 1, Color.Lime); x++;
                AddControlCell(param.LPFResonance, x, y, 1, Color.Lime); x++;
                AddControlCell(param.HPFCutoffFreq, x, y, 1, Color.Lime); x++;

                AddControlCell(param.EGAttack, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.EGDecay, x, y, 1, Color.Cyan); x++;
                AddControlCell(param.EGRelease, x, y, 1, Color.Cyan); x++;

                AddControlCell(param.VibRate, x, y, 1, Color.Red); x++;
                AddControlCell(param.VibDepth, x, y, 1, Color.Red); x++;
                AddControlCell(param.VibDelay, x, y, 1, Color.Red); x++;

                AddControlCell(param.EQBassFreq, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQBassGain, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQTrebleFreq, x, y, 1, Color.Yellow); x++;
                AddControlCell(param.EQTrebleGain, x, y, 1, Color.Yellow); x++;

                //AddControlCell(param.MWPitchControl, x, y, 1, Color.Magenta); x++;
                //AddControlCell(param.MWLPFControl, x, y, 1, Color.Magenta); x++;
                //AddControlCell(param.MWAmpControl, x, y, 1, Color.Magenta); x++;
                //AddControlCell(param.MWLFOPModDepth, x, y, 1, Color.Magenta); x++;
                //AddControlCell(param.MWLFOFModDepth, x, y, 1, Color.Magenta); x++;
                //AddControlCell(param.MWLFOAModDepth, x, y, 1, Color.Magenta); x++;
            }
        }

        private void buttonDumpAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i].RequestDump();
            }
            RedrawOnRequestComplete();
        }


        private void buttonSendAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Channels.Count; i++)
            {
                Channels[i].ReSendAll();
            }
        }

        public string GetMmlText()
        {
            StringBuilder mml = new StringBuilder();
            mml.Append(

@"
Function TrackParam(_ch,_md, _pgm,_pgl,_pgc, _vol=100, _pan=0, _rev=40,_cho=0,_var=0,_dry=0, _lpf=0,_rsn=0,_hpf=0, _ega=0,_egd=0,_egr=0, _vrt=0,_vdp=0,_vdl=0, _eqbf=$0C, _eqbg=0, _eqtf=$36, _eqtg=0)
{
    CH = _ch;
    XGXcl1($080007 | (_ch*256), _md) // Mode
    ProgramChange(_pgc, _pgm, _pgl)
    MainVolume(_vol)
    Panpot(64+_pan)
    Reverb(_rev)
    Chorus(_cho)
    VAR(_var)
    XGXcl1($080011 | (_ch*256), _dry) // Dry Send Level

    XGNrpn(1,$20,_lpf+64) // LPF Freq
    XGNrpn(1,$21,_rsn+64) // LPF Reso
    XGNrpn(1,$24,_hpf+64) // HPF Freq

    XGNrpn(1,$63,_ega+64) // EG Atk
    XGNrpn(1,$64,_egd+64) // EQ Dcy
    XGNrpn(1,$66,_egr+64) // EQ Rls

    XGNrpn(1,$08,_vrt+64) // Vib Rat
    XGNrpn(1,$09,_vdp+64) // Vib Dpt
    XGNrpn(1,$0A,_vdl+64) // Vib Dly

    XGNrpn(1,$34,_eqbf+12) // EQ Bass Freq
    XGNrpn(1,$30,_eqbg+64) // EQ Bass Gain
    XGNrpn(1,$35,_eqtf+54) // EQ Treble Freq
    XGNrpn(1,$31,_eqtg+64) // EQ Treble Gain
    r%1
}

//ackParam(CH,M,	MSB,LSB,PG#,	VOL,PAN,	REV,CHO,VAR,DRY,	LPF,RSN,HPF,	EGA,EGD,EGR,	VRt,VDp,VDl,	BFq,BGa,TFq,TGa
");

            for (int i = 0; i < Channels.Count; i++)
            {
                XGPartParams p = Channels[i];
                mml.AppendFormat(
                    "TrackParam({0,2},{23,1},\t{1,3},{2,3},{3,3},\t{4,3},{5,3:+#0;-#0;##0},\t"
                    + "{6,3},{7,3},{8,3},{9,3},\t"
                    + "{10,3:+##;-##;##0},{11,3:+##;-##;##0},{12,3:+##;-##;##0},\t"
                    + "{13,3:+##;-##;##0},{14,3:+##;-##;##0},{15,3:+##;-##;##0},\t"
                    + "{16,3:+##;-##;##0},{17,3:+##;-##;##0},{18,3:+##;-##;##0},\t"
                    + "{19:+##;-##;###},{20:+##;-##;###},{21:+##;-##;###},{22:+##;-##;###}"
                    + ");\n",
                    p.Channel + 1,
                    p.ProgramMSB.Value, p.ProgramLSB.Value, p.ProgramNumber.Value + 1,
                    p.Volume.Value, p.Pan.Value - 64,
                    p.Reverb.Value, p.Chorus.Value, p.Variation.Value, p.DryLevel.Value,
                    p.LPFCutoffFreq.Value - 64, p.LPFResonance.Value - 64, p.HPFCutoffFreq.Value - 64,
                    p.EGAttack.Value - 64, p.EGDecay.Value - 64, p.EGRelease.Value - 64,
                    p.VibRate.Value - 64, p.VibDepth.Value - 64, p.VibDelay.Value - 64,
                    p.EQBassFreq.Value != 0x0C ? p.EQBassFreq.Value - 0x0C : 0, p.EQBassGain.Value - 64,
                    p.EQTrebleFreq.Value != 0x36 ? p.EQTrebleFreq.Value - 0x36 : 0, p.EQTrebleGain.Value - 64,
                    p.PartMode.Value
                    );
            }
            return mml.ToString();
        }
    }
}
