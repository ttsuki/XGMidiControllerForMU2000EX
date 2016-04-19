using System;
using System.Collections.Generic;
using System.Diagnostics;
using Tsukikage.WinMM.MidiIO;

namespace Tsukikage.XGTGCtrl2.XG
{
    public class MidiIODevice : IDisposable
    {
        MidiOut midiOut;
        MidiIn midiIn;

        void OnLongMessage(byte[] data) { if (LongMessage != null) { LongMessage(data); } }
        void OnShortMessage(uint data) { if (ShortMessage != null) { ShortMessage(data); } }

        public void OpenOut(int index)
        {
            CloseOut();
            try
            {
                midiOut = new MidiOut(index);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error occured: " + ex.ToString());
            }
        }

        public void CloseOut()
        {
            if (midiOut != null)
            {
                midiOut.Close();
                midiOut = null;
            }
        }

        public void OpenIn(int index)
        {
            CloseIn();
            try
            {
                midiIn = new MidiIn(index);
                midiIn.OnLongMsg += OnLongMessage;
                midiIn.OnShortMsg += OnShortMessage;
                midiIn.Start();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error occured: " + ex.ToString());
            }
        }

        public void CloseIn()
        {
            if (midiIn != null)
            {
                midiIn.Stop();
                midiIn.OnLongMsg -= OnLongMessage;
                midiIn.OnShortMsg -= OnShortMessage;
                midiIn.Close();
                midiIn = null;
            }
        }

        public void Dispose()
        {
            CloseOut();
            System.Threading.Thread.Sleep(100);
            CloseIn();
        }

        public void Write(int message) { if (midiOut != null) { midiOut.ShortMessage((uint)message); } }
        public void Write(params byte[] message) { if (midiOut != null) { midiOut.Write(message); } }
        public void Write(byte[] array, int start, int count) { if (midiOut != null) { midiOut.Write(array, start, count); } }
        public event MidiIn.MidiInLongMessageHandler LongMessage;
        public event MidiIn.MidiInShortMessageHandler ShortMessage;
    }


    public class XGMidiIODevice : MidiIODevice
    {
        ParameterMemory ParameterMemoryData;

        Queue<KeyValuePair<int, byte[]>> dumpRequestQueue = new Queue<KeyValuePair<int, byte[]>>();
        List<KeyValuePair<int, DateTime>> requestQueue = new List<KeyValuePair<int, DateTime>>();

        public XGMidiIODevice()
        {
            this.LongMessage += OnExclusive;
            this.ParameterMemoryData = new ParameterMemory(0x400000);
            ResetXG();
        }

        public void SendXGParameterRequest(int address)
        {
            byte address0 = (byte)(address >> 16 & 0x7F);
            byte address1 = (byte)(address >> 8 & 0x7F);
            byte address2 = (byte)(address >> 0 & 0x7F);
            Request(address, 0xF0, 0x43, 0x30, 0x4C, address0, address1, address2, 0xF7);
        }

        public void SendXGParameterByValue(int address, int value, int count)
        {
            byte address0 = (byte)(address >> 16 & 0x7F);
            byte address1 = (byte)(address >> 8 & 0x7F);
            byte address2 = (byte)(address >> 0 & 0x7F);

            byte value0 = (byte)(value >> 21 & 0x7F);
            byte value1 = (byte)(value >> 14 & 0x7F);
            byte value2 = (byte)(value >> 7 & 0x7F);
            byte value3 = (byte)(value >> 0 & 0x7F);

            switch (count)
            {
                case 1: Write(0xF0, 0x43, 0x10, 0x4C, address0, address1, address2, value3, 0xF7); break;
                case 2: Write(0xF0, 0x43, 0x10, 0x4C, address0, address1, address2, value2, value3, 0xF7); break;
                case 4: Write(0xF0, 0x43, 0x10, 0x4C, address0, address1, address2, value0, value1, value2, value3, 0xF7); break;
                default: Debug.Fail("count != {1,2,4}"); break;
            }
        }

        public void SendXGBulkDumpRequest(int address)
        {
            byte address0 = (byte)(address >> 16 & 0x7F);
            byte address1 = (byte)(address >> 8 & 0x7F);
            byte address2 = (byte)(address >> 0 & 0x7F);
            Request(address, 0xF0, 0x43, 0x20, 0x4C, address0, address1, address2, 0xF7);
        }

        void OnExclusive(byte[] data)
        {
            if (data[0] != 0xF0) { return; }

            if (data[1] != 0x43) { return; }
            if (data[3] != 0x4C) { return; }

            switch (data[2] & 0x70)
            {
                case 0x00: OnBulkDump(data); break;
                case 0x10: OnParameter(data); break;
                default: Trace.Write("unknown exclusive."); break;
            }
        }

        void OnBulkDump(byte[] data)
        {
            int length = data[4] << 7 | data[5];
            int address = data[6] << 16 | data[7] << 8 | data[8];

            int excLen = 1 + 3 + 2 + 3 + length + 1 + 1; // F0 43004C LLLL ADADAD data CS F7 
            if (data[excLen - 1] != 0xF7) { Trace.WriteLine("bulkdump length error."); return; }

            int checksum = 0;
            for (int i = 0; i < length + 6; i++) { checksum += data[i + 4]; }
            if ((checksum & 0x7F) != 0) { Trace.WriteLine("bulkdump checksum error."); return; }

            for (int i = 0; i < length; i++)
            {
                ParameterMemoryData[address + i] = data[i + 9];
            }

            OnBulkDumpChain(address);

            Arrival(address);
        }

        void OnBulkDumpChain(int address)
        {
            // Insertion Effect
            if ((address & 0xFFFCFF) == 0x030000)
            {
                int v = ParameterMemoryData.Read2(address);
                XGEffect fx = XGEffect.GetEffectByTypeValue(v & 0x7f | v << 1 & 0x7f00);
                if (fx != null && fx.ExDataUsed)
                {
                    SendXGBulkDumpRequest(address + 0x30);
                }
            }
        }


        void OnParameter(byte[] data)
        {
            int address = data[4] << 16 | data[5] << 8 | data[6];
            int excLen = Array.IndexOf<byte>(data, 0xF7) + 1;
            int datalen = excLen - (1 + 3 + 3 + 1); // F0 43104C  ADADAD data F7 

            // to Value
            int value = 0;
            for (int i = 0; i < datalen; i++) { value = value << 7 | data[7 + i]; }
            Arrival(address);

            try
            {
                WriteXGParam(address, datalen, value);
            }
            catch
            {
                Trace.Write("parameter set error");
            }
        }

        class ParameterMemory
        {
            byte[] memory;
            public int Length { get { return memory.Length; } }
            public ParameterMemory(int count) { this.memory = new byte[count]; }
            public byte this[int addr] { get { return ReadByte(addr); } set { WriteByte(addr, value); } }
            byte ReadByte(int address) { return address >= 0 && address < memory.Length ? memory[address] : (byte)0; }
            void WriteByte(int address, byte value) { if (address >= 0 && address < memory.Length) { memory[address] = (byte)(value & 0x7F); } }

            public int Read1(int addr) { return ReadByte(addr); }
            public int Read2(int addr) { return Read1(addr) << 7 | Read1(addr + 1); }
            public int Read4(int addr) { return Read2(addr) << 14 | Read2(addr + 2); }
            public void Write1(int addr, int value) { WriteByte(addr, (byte)(value & 0x7F)); }
            public void Write2(int addr, int value) { Write1(addr, value >> 7); Write1(addr + 1, value); }
            public void Write4(int addr, int value) { Write2(addr, value >> 14); Write2(addr + 2, value); }
        };

        public int ReadXGParam(int addr, int count)
        {
            switch (count)
            {
                case 1: return ParameterMemoryData.Read1(addr);
                case 2: return ParameterMemoryData.Read2(addr);
                case 4: return ParameterMemoryData.Read4(addr);
                default: throw new NotImplementedException();
            }
        }

        public void WriteXGParam(int addr, int count, int value)
        {
            switch (count)
            {
                case 1: ParameterMemoryData.Write1(addr, value); break;
                case 2: ParameterMemoryData.Write2(addr, value); break;
                case 4: ParameterMemoryData.Write4(addr, value); break;
                default: throw new NotImplementedException();
            }
        }

        public bool AllDumpRequestHasDone
        {
            get { return MaintainQueue(); }
        }

        bool MaintainQueue()
        {
            lock (this)
            {
                bool x = requestQueue.Count != 0;
                requestQueue.RemoveAll(kvp =>
                {
                    bool ret = kvp.Value < DateTime.Now;
                    Trace.WriteLineIf(ret, "Request Timed Out: Address[$" + kvp.Key.ToString("X06") + "]");
                    return ret;
                });

                Trace.WriteLineIf(x && dumpRequestQueue.Count == 0 && requestQueue.Count == 0, "All request has done.");

                while (requestQueue.Count < 8 && dumpRequestQueue.Count > 0)
                {
                    var r = dumpRequestQueue.Dequeue();
                    Request(r.Key, r.Value);
                }

                return requestQueue.Count == 0;
            }
        }

        void Request(int address, params byte[] message)
        {
            lock (this)
            {

                if (requestQueue.Count >= 8)
                {
                    Trace.WriteLine("Queued: Request Address[$" + address.ToString("X06") + "]");
                    dumpRequestQueue.Enqueue(new KeyValuePair<int, byte[]>(address, message));
                }
                else
                {
                    const int timeoutSec = 1;
                    Trace.WriteLine("Requested: Address[$" + address.ToString("X06") + "]");
                    requestQueue.Add(new KeyValuePair<int, DateTime>(address, DateTime.Now + TimeSpan.FromSeconds(timeoutSec)));
                    Write(message);
                }
            }
        }

        void Arrival(int address)
        {
            lock (this)
            {
                Trace.WriteLine("Recieved: Address[$" + address.ToString("X06") + "]");
                int i = requestQueue.FindIndex(kvp => kvp.Key == address);
                if (i >= 0) { requestQueue.RemoveAt(i); }
                Trace.WriteLineIf(requestQueue.Count == 0 && dumpRequestQueue.Count == 0, "All request has done.");
                MaintainQueue();
            }
        }

        void ResetEffect(XGEffectBlockType blockType, string effectName)
        {
            Action<XGMidiParameter, int> writeValue = (p, v) => { if (p != null) { p.WriteValue(v); } };

            var eff = new XGEffectParams(this, blockType);
            eff.WriteEffect(XGEffect.GetEffectByName(effectName));
            writeValue(eff.SendToChorus, 0);
            writeValue(eff.SendToReverb, 0);
            writeValue(eff.Return, 0x40);
            writeValue(eff.Pan, 0x40);
            writeValue(eff.VariationConnect, 0);
            writeValue(eff.PartNumber, 0x7F);
        }

        public void ResetXG()
        {
            WriteXGParam(0x000000, 4, 0x10000);// MASTER TUNE = 0
            WriteXGParam(0x000004, 1, 0x7F);// MASTER VOLUME
            WriteXGParam(0x000005, 1, 0x00);// MASTER ATTR
            WriteXGParam(0x000006, 1, 0x40);// TRANSPOSE

            ResetEffect(XGEffectBlockType.Reverb, "HALL 1");
            ResetEffect(XGEffectBlockType.Chorus, "CHORUS 1");
            ResetEffect(XGEffectBlockType.Variation, "DELAY LCR");
            ResetEffect(XGEffectBlockType.Insertion1, "DISTORTION");
            ResetEffect(XGEffectBlockType.Insertion2, "DISTORTION");
            ResetEffect(XGEffectBlockType.Insertion3, "DISTORTION");
            ResetEffect(XGEffectBlockType.Insertion4, "DISTORTION");

            for (int i = 0; i < 64; i++)
            {
                var part = new XGPartParams(this, i);
                part.ProgramMSB.WriteValue(i % 16 == 9 ? 127 : 0);
                part.ProgramLSB.WriteValue(0);
                part.ProgramNumber.WriteValue(0);
                part.RcvNoteMessage.WriteValue(1);
                part.Pan.WriteValue(64);
                part.Volume.WriteValue(100);
                part.Reverb.WriteValue(40);
                part.Chorus.WriteValue(0);
                part.Variation.WriteValue(0);
                part.DryLevel.WriteValue(127);
                part.LPFCutoffFreq.WriteValue(64);
                part.LPFResonance.WriteValue(64);
                part.HPFCutoffFreq.WriteValue(64);
                part.EGAttack.WriteValue(64);
                part.EGDecay.WriteValue(64);
                part.EGRelease.WriteValue(64);
                part.VibRate.WriteValue(64);
                part.VibDepth.WriteValue(64);
                part.VibDelay.WriteValue(64);
                part.EQBassFreq.WriteValue(12);
                part.EQBassGain.WriteValue(64);
                part.EQTrebleFreq.WriteValue(54);
                part.EQTrebleGain.WriteValue(64);
                part.MWPitchControl.WriteValue(64);
                part.MWLPFControl.WriteValue(64);
                part.MWAmpControl.WriteValue(64);
                part.MWLFOPModDepth.WriteValue(10);
                part.MWLFOFModDepth.WriteValue(0);
                part.MWLFOAModDepth.WriteValue(0);
            }

            ulong[] data = { // StandKit#
                0x6603335F5F004040, 0x7903335F5F004040, 0x3F00337F7F004040, 0x7F00337F7F004040, 0x5D04343F3F004040, 0x7404343F3F004040, 0x7F00404B00004040, 0x7F00407F7F004040,
                0x5E00403F3F004040, 0x6200403F3F004040, 0x5C00407F7F004040, 0x7700407F7F004040, 0x3100407F7F004040, 0x2F00407F7F014040, 0x3400407F7F004040, 0x2D00407F7F014040,
                0x4F00407F7F014040, 0x7F00403F3F004040, 0x4B00407F7F004040, 0x7F00407F7F004040, 0x7400402020004040, 0x7700407F7F004042, 0x6600402020004040, 0x7F00402020004040,
                0x5D00407F7F004040, 0x7F00407F7F004040, 0x6E00407F7F004040, 0x7B00407F7F004040, 0x6F00187F7F004040, 0x5B014D2020004040, 0x7100277F7F004040, 0x5C014D2020004040,
                0x6300347F7F004040, 0x60014D2020004040, 0x5700407F7F004040, 0x6300537F7F004040, 0x7F00457F7F004040, 0x7400687F7F004040, 0x6900227F7F004041, 0x7800227F7F004040,
                0x6B002E7F7F004040, 0x7400403F3F004040, 0x7F00407F7F004040, 0x76004D3F3F004040, 0x7F00337F7F004040, 0x6A00197F7F004040, 0x6E002E7F7F004040, 0x6E006E5F5F004040,
                0x57006E5F5F004040, 0x6900277F7F004040, 0x6B00197F7F004040, 0x7300405F5F004040, 0x5B00407F7F004040, 0x5F00407F7F004040, 0x6C00226464004040, 0x6C00226464004040,
                0x5A001C3F3F004040, 0x6300153F3F004040, 0x6700657F7F014040, 0x6E00657F7F014040, 0x7C005F3F3F004040, 0x6A006E3F3F014040, 0x5800405F5F004040, 0x6B00685F5F004040,
                0x6000685F5F004040, 0x6100157F7F004040, 0x6B00227F7F004040, 0x7F02195F5F004040, 0x7F02197F7F004040, 0x6A00533F3F004040, 0x7B00697F7F004040, 0x4400407F7F004040,
                0x7F00407F7F004040, 0x7F00407F7F004040, 0x7F00407F7F004040, 0x7F00407F7F004040, 0x7F00407F7F004040, 0x7F00407F7F004040, 0x7F00407F7F004040,  
            };

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0x0d; j <= 0x5b; j++)
                {
                    var drum = new XGDrumParams(this, i, j);
                    drum.PitchCoarse.WriteValue(64);
                    drum.PitchFine.WriteValue(64);
                    drum.Volume.WriteValue((int)(data[j - 0x0d] >> 56 & 0x7F));
                    drum.Pan.WriteValue((int)(data[j - 0x0d] >> 40 & 0x7F));
                    drum.Reverb.WriteValue((int)(data[j - 0x0d] >> 32 & 0x7F));
                    drum.Chorus.WriteValue((int)(data[j - 0x0d] >> 24 & 0x7F));
                    drum.Variation.WriteValue(0x7F);
                    drum.LPFCutoffFreq.WriteValue(0x40);
                    drum.LPFResonance.WriteValue(0x40);
                    drum.HPFCutoffFreq.WriteValue(0x40);
                    drum.EGAttackRate.WriteValue(0x40);
                    drum.EGDecay1Rate.WriteValue(0x40);
                    drum.EGDecay2Rate.WriteValue(0x40);
                    drum.EQBassGain.WriteValue(0x40);
                    drum.EQTrebleGain.WriteValue(0x40);
                    drum.EQBassFreq.WriteValue(0x0C);
                    drum.EQTrebleFreq.WriteValue(0x36);
                    drum.AltGroup.WriteValue((int)(data[j - 0x0d] >> 48 & 0x7F));
                    drum.KeyAssign.WriteValue(0x00);
                    drum.RcvNoteOff.WriteValue((int)(data[j - 0x0d] >> 16 & 0x7F));
                    drum.RcvNoteOn.WriteValue(0x01);
                    drum.VelocitySensePitch.WriteValue((int)(data[j - 0x0d] >> 8 & 0x7F));
                    drum.VelocitySenseLPFCutoff.WriteValue((int)(data[j - 0x0d] >> 0 & 0x7F));
                }
            }

            {
                var eq = new XGMultiEQParams(this);

                eq.Type.WriteValue(0);
                eq.Gain1.WriteValue(0x40);
                eq.Freq1.WriteValue(0x0c);
                eq.Q1.WriteValue(0x07);
                eq.Shape1.WriteValue(0x00);
                eq.Gain2.WriteValue(0x40);
                eq.Freq2.WriteValue(0x1C);
                eq.Q2.WriteValue(0x07);
                eq.Gain3.WriteValue(0x40);
                eq.Freq3.WriteValue(0x22);
                eq.Q3.WriteValue(0x07);
                eq.Gain4.WriteValue(0x40);
                eq.Freq4.WriteValue(0x2E);
                eq.Q4.WriteValue(0x07);
                eq.Gain5.WriteValue(0x40);
                eq.Freq5.WriteValue(0x34);
                eq.Q5.WriteValue(0x07);
                eq.Shape5.WriteValue(0x00);
            }
        }
    }
}
