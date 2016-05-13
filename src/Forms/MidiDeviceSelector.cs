using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Tsukikage.Util;
using Tsukikage.WinMM.MidiIO;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class MidiDeviceSelector : UserControl
    {
        public MidiDeviceSelector()
        {
            InitializeComponent();
        }

        XGMidiIODevice Device;
        public void SetDevice(XGMidiIODevice device)
        {
            Debug.Assert(this.Device == null);
            this.Device = device;

            comboBoxMidiInSelect.Items.AddRange(MidiIn.GetDeviceNames());
            comboBoxMidiOutSelect.Items.AddRange(MidiOut.GetDeviceNames());

            int inIndex = Array.IndexOf(MidiIn.GetDeviceNames(), Config.lastSelectedMidiIn);
            if (inIndex < 0) { inIndex = 0; }
            if (comboBoxMidiInSelect.Items.Count > 0) { comboBoxMidiInSelect.SelectedIndex = inIndex; }

            int outIndex = Array.IndexOf(MidiOut.GetDeviceNames(), Config.lastSelectedMidiOut);
            if (outIndex < 0) { outIndex = 0; }
            if (comboBoxMidiOutSelect.Items.Count > 0) { comboBoxMidiOutSelect.SelectedIndex = outIndex; }

            pictureBox1.Height = pictureBox1.Width;

            timer1.Interval = 10;
            timer1.Tick += Timer1_Tick;
            timer1.Enabled = true;

            CreateXGPControls();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            bool prev = progressBar1.Style == ProgressBarStyle.Marquee;
            bool curr = !Device.AllDumpRequestHasDone;
            if (prev && !curr)
            {
                progressBar1.Style = ProgressBarStyle.Blocks;
            }
        }

        void CreateXGPControls()
        {

            XGMidiParameter MasterVolume = new XGMidiParameter(Device, "XGMasterVolume", 0x000004, 1, 0, 127, 0);

            XGMidiParameter MasterAttn = new XGMidiParameter(Device, "XGMasterVolume", 0x000005, 1, 0, 127, 127);

            XGMidiParameter MasterTranspose = new XGMidiParameter(Device, "Transpose", 0x000006, 1, 0x28, 0x58, 64);
            MasterTranspose.ToStringConverter = XGMidiParameter.CenterPM;

            XGMidiParameter MasterTune = new XGMidiParameter(Device, "MasterTune", 0x000000, 4, 0, 0x7FF, 0x400);
            MasterTune.ReadValueEncoding = v => v & 0xF | v >> 3 & 0xF0 | v >> 6 & 0xF00;
            MasterTune.WriteValueEncoding = v => v & 0xF | (v & 0xF0) << 3 | (v & 0xF00) << 6;
            MasterTune.ToStringConverter = v => ((v - 1024) / 10).ToString("+000;-000") + "." + ((v - 1024) % 10).ToString();


            int y = -2;
            xgpGrid1.AddTriggerCell("[CONNECTION TEST]", 0, y, 6, Color.Gray, () =>
            {
                Device.WriteXGParam(0x10000, 4, 0);
                Device.WriteXGParam(0x10004, 4, 0);
                Device.WriteXGParam(0x10008, 4, 0);
                Device.WriteXGParam(0x1000C, 4, 0);

                Device.SendXGBulkDumpRequest(0x010000);
                while (!Device.AllDumpRequestHasDone) { Application.DoEvents(); }
                byte[] information = new byte[16];
                for (int i = 0; i < information.Length; i++) { information[i] = (byte)Device.ReadXGParam(0x10000 + i, 1); }
                if (information[0] != 0)
                {
                    int len = Array.IndexOf(information, 0);
                    string modelName = System.Text.Encoding.ASCII.GetString(information, 0, len >= 0 && len < 14 ? len : 14);
                    MessageBox.Show("Connection OK. \nReplied from " + modelName, "Message", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Reqest timed out.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }).GetDescriptionFunc = () => "DblClick: Request DeviceName";
            y++;

            xgpGrid1.AddTriggerCell("[XG SYSTEM ON]", 0, y, 6, Color.Red, () =>
            {
                if (MessageBox.Show("Send XG SYSTEM ON?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                {
                    Device.SendXGParameterByValue(0x00007E, 0, 1);
                    Device.ResetXG();
                    xgpGrid1.RedrawOnRequestComplete();
                }
            }).GetDescriptionFunc = () => "DblClick: Send XG SYSTEM ON.";
            y++;

            xgpGrid1.AddTriggerCell("[Dump]", 0, y, 6, Color.Black, () =>
            {
                MasterVolume.Pick();
                MasterAttn.Pick();
                MasterTune.Pick();
                MasterTranspose.Pick();
                xgpGrid1.RedrawOnRequestComplete();
            }).GetDescriptionFunc = () => "DblClick: Request Dump parameters.";
            y++;

            xgpGrid1.AddTriggerCell("[AllDump]", 0, y, 6, Color.Black, () =>
            {
                if (MessageBox.Show("Dump ALL XG Parameters?\n Including 16 parts, drum S1+S2, effects settings.", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) != DialogResult.OK)
                {
                    return;
                }

                lock(Device)
                {
                    Device.SendXGBulkDumpRequest(0x000000);
                    Device.SendXGBulkDumpRequest(0x010000);
                    Device.SendXGBulkDumpRequest(0x020100);
                    Device.SendXGBulkDumpRequest(0x020110);
                    Device.SendXGBulkDumpRequest(0x020120);
                    Device.SendXGBulkDumpRequest(0x020130);
                    Device.SendXGBulkDumpRequest(0x020140);
                    Device.SendXGBulkDumpRequest(0x020170);
                    Device.SendXGBulkDumpRequest(0x024000);
                    Device.SendXGBulkDumpRequest(0x030000);
                    Device.SendXGBulkDumpRequest(0x030020);
                    Device.SendXGBulkDumpRequest(0x030100);
                    Device.SendXGBulkDumpRequest(0x030120);
                    Device.SendXGBulkDumpRequest(0x030200);
                    Device.SendXGBulkDumpRequest(0x030220);
                    Device.SendXGBulkDumpRequest(0x030300);
                    Device.SendXGBulkDumpRequest(0x030320);
                    Device.SendXGBulkDumpRequest(0x030300);
                    Device.SendXGBulkDumpRequest(0x030320);

                    for (int i = 0; i < 16; i++)
                    {
                        Device.SendXGBulkDumpRequest(0x080000 | i << 8);
                        Device.SendXGBulkDumpRequest(0x080030 | i << 8);
                        Device.SendXGBulkDumpRequest(0x080070 | i << 8);
                        Device.SendXGBulkDumpRequest(0x080074 | i << 8);
                        Device.SendXGBulkDumpRequest(0x0A0020 | i << 8);
                    }

                    for (int i = 0; i < 2; i++)
                    {
                        for (int j = 0x0d; j <= 0x5B; j++)
                        {
                            Device.SendXGBulkDumpRequest(0x300000 | i << 16 | j << 8);
                            Device.SendXGBulkDumpRequest(0x300020 | i << 16 | j << 8);
                            Device.SendXGBulkDumpRequest(0x300050 | i << 16 | j << 8);
                            Device.SendXGBulkDumpRequest(0x300060 | i << 16 | j << 8);
                        }
                    }
                    progressBar1.Style = ProgressBarStyle.Marquee;
                }

                xgpGrid1.RedrawOnRequestComplete();
            }).GetDescriptionFunc = () => "DblClick: Request Dump all XG Params.";
            y++;

            xgpGrid1.AddLabelCell("MasterVolume", 0, y, 4, Color.Green);
            xgpGrid1.AddControlCell(MasterVolume, 4, y, 2, Color.Lime);
            y++;
            xgpGrid1.AddLabelCell("MasterAttn", 0, y, 4, Color.Navy);
            xgpGrid1.AddControlCell(MasterAttn, 4, y, 2, Color.Blue);
            y++;
            xgpGrid1.AddLabelCell("Transpose", 0, y, 4, Color.Olive);
            xgpGrid1.AddControlCell(MasterTranspose, 4, y, 2, Color.Yellow);
            y++;
            xgpGrid1.AddLabelCell("MasterTune", 0, y, 4, Color.Purple);
            xgpGrid1.AddControlCell(MasterTune, 4, y, 2, Color.Magenta);
            y++;
            xgpGrid1.SetDevice(Device);
        }

        private void comboBoxMidiInSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Device != null)
            {
                Trace.WriteLine("Opening MIDI-IN: " + comboBoxMidiInSelect.Items[comboBoxMidiInSelect.SelectedIndex]);
                Device.OpenIn(comboBoxMidiInSelect.SelectedIndex);
                try { Config.lastSelectedMidiIn = MidiIn.GetDeviceNames()[comboBoxMidiInSelect.SelectedIndex]; }
                catch { }
            }
        }

        private void comboBoxMidiOutSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Device != null)
            {
                Trace.WriteLine("Opening MIDI-OUT: " + comboBoxMidiOutSelect.Items[comboBoxMidiOutSelect.SelectedIndex]);
                Device.OpenOut(comboBoxMidiOutSelect.SelectedIndex);
                try { Config.lastSelectedMidiOut = MidiOut.GetDeviceNames()[comboBoxMidiOutSelect.SelectedIndex]; }
                catch { }
            }
        }

        private void buttonOpenConsole_Click(object sender, EventArgs e)
        {
            if (!ConsoleWindow.Allocated) { ConsoleWindow.Allocate(); }
            else if (!ConsoleWindow.Displayed) { ConsoleWindow.Show(); }
            else { ConsoleWindow.Hide(); }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                EntryAssemblyInformation.Title + "\n" +
                EntryAssemblyInformation.Description + "\n" +
                "Version " + EntryAssemblyInformation.Version + "\n" +
                EntryAssemblyInformation.Copyright + "\n"
                , "Version Information"
                , MessageBoxButtons.OK, MessageBoxIcon.Asterisk
                );
        }
    }
}
