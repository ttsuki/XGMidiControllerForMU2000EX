using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Tsukikage.XGTGCtrl2.XG
{
    public delegate string ToStringDelegate(int value);
    public delegate int EncodeValueDelegate(int value);


    public class XGMidiParameter
    {
        public static readonly string[] freqTable = { "20", "22", "25", "28", "32", "36", "40", "45", "50", "56", "63", "70", "80", "90", "100", "110", "125", "140", "160", "180", "200", "225", "250", "280", "315", "355", "400", "450", "500", "560", "630", "700", "800", "900", "1.0k", "1.1k", "1.2k", "1.4k", "1.6k", "1.8k", "2.0k", "2.2k", "2.5k", "2.8k", "3.2k", "3.6k", "4.0k", "4.5k", "5.0k", "5.6k", "6.3k", "7.0k", "8.0k", "9.0k", "10k", "11k", "12k", "14k", "16k", "18k", "20k", };

        public static ToStringDelegate MakeTableToStringFunc(params string[] bind)
        {
            return (int value) => value >= 0 && value < bind.Length ? bind[value] : "?" + value.ToString();
        }

        public static ToStringDelegate MakePlusMinusToStringFunc(string minusPre, string minusPost, string center, string plusPre, string plusPost, string zero)
        {
            string[] table = new string[128];
            for (int i = 0; i < 64; i++) { table[i] = minusPre + (64 - i).ToString().PadLeft(2) + minusPost; }
            table[64] = center.PadLeft(3);
            for (int i = 65; i < 128; i++) { table[i] = plusPre + (i - 64).ToString().PadLeft(2) + plusPost; }
            if (!string.IsNullOrEmpty(zero)) { table[0] = zero; }
            return MakeTableToStringFunc(table);
        }
        public static ToStringDelegate MakeStepToStringFunc(int initial, int step, int count)
        {
            string[] table = new string[count];
            for (int i = 0; i < table.Length; i++) { table[i] = string.Format("{0,3}", initial + i * step); }
            return MakeTableToStringFunc(table);
        }

        public static readonly ToStringDelegate DefaultToString = MakeStepToStringFunc(0, 1, 128);
        public static readonly ToStringDelegate ProgramNumber = MakeStepToStringFunc(1, 1, 128);
        public static readonly ToStringDelegate FreqToString = MakeTableToStringFunc(freqTable);
        public static readonly ToStringDelegate CenterPM = MakePlusMinusToStringFunc("-", "", "  0", "+", "", null);
        public static readonly ToStringDelegate PanpotPM = MakePlusMinusToStringFunc("L", "", " C ", "R", "", "RND");
        public static readonly ToStringDelegate OnOff = MakeTableToStringFunc("Off", "On");

        public XGMidiIODevice Host { get; private set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Address { get; set; }
        public int Count { get; set; }

        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public int CenterValue { get; set; }

        public ToStringDelegate ToStringConverter;
        public EncodeValueDelegate WriteValueEncoding;
        public EncodeValueDelegate ReadValueEncoding;
        //public ToStringDelegate ToMMLConverter;
        public int Value
        {
            get { return ReadValue(); }
            set { WriteValue(value); Resend(); }
        }

        public string ValueString
        {
            get { return ToStringConverter(Value); }
        }

        public XGMidiParameter(XGMidiIODevice host, string name, int address, int count, int min, int max, int center)
        {
            this.Host = host;
            this.Name = name;
            this.Address = address;
            this.Count = count;

            this.MinValue = min;
            this.MaxValue = max;
            this.CenterValue = center;

            this.ToStringConverter = DefaultToString;
            this.WriteValueEncoding = v => v;
            this.ReadValueEncoding = v => v;
        }


        public virtual void OffsetValue(int ofst)
        {
            int val = Value;
            val += ofst;
            if (val >= MaxValue) { val = MaxValue; }
            if (val <= MinValue) { val = MinValue; }
            Value = val;
        }

        public void WriteValue(int val)
        {
            Host.WriteXGParam(Address, Count, WriteValueEncoding(val));
        }

        public int ReadValue()
        {
            return ReadValueEncoding(Host.ReadXGParam(Address, Count));
        }

        public void Resend()
        {
            Host.SendXGParameterByValue(Address, WriteValueEncoding(Value), Count);
        }

        public void Pick()
        {
            Host.SendXGParameterRequest(Address);
        }

        public virtual void Inc() { OffsetValue(1); }
        public virtual void Dec() { OffsetValue(-1); }

    }

    public class ParameterCollection
    {
        protected ParameterCollection(XGMidiIODevice host, string name, int baseAddress)
        {
            this.Host = host;
            this.Name = name;
            this.BaseAddress = baseAddress;
            this.Parameters = new List<XGMidiParameter>();
        }
        public XGMidiIODevice Host { get; private set; }
        public string Name { get; private set; }
        public int BaseAddress { get; private set; }
        public List<XGMidiParameter> Parameters { get; private set; }

        public virtual void RequestDump() { }

        public virtual void ReSendAll()
        {
            for (int i = 0; i < Parameters.Count; i++)
            {
                Parameters[i].Value = Parameters[i].Value;
            }
        }

        protected XGMidiParameter AddParameter(string name, int offset, int min, int max, int center)
        {
            XGMidiParameter p = new XGMidiParameter(Host, this.Name + " " + name, BaseAddress + offset, 1, min, max, center);
            Parameters.Add(p);
            return p;
        }

        protected XGMidiParameter AddParameter(string name, int offset, int min, int max, int center, ToStringDelegate valStringConverter)
        {
            XGMidiParameter p = AddParameter(name, offset, min, max, center);
            p.ToStringConverter = valStringConverter;
            return p;
        }
    }


    class XGPartParams : ParameterCollection
    {
        public int Channel { get; private set; }

        public XGMidiParameter ProgramMSB { get; private set; }
        public XGMidiParameter ProgramLSB { get; private set; }
        public XGMidiParameter ProgramNumber { get; private set; }
        public XGMidiParameter RcvNoteMessage { get; private set; }

        //public XGMidiParameter Modulation { get; private set; }
        public XGMidiParameter Pan { get; private set; }
        public XGMidiParameter Volume { get; private set; }
        public XGMidiParameter Reverb { get; private set; }
        public XGMidiParameter Chorus { get; private set; }
        public XGMidiParameter Variation { get; private set; }

        public XGMidiParameter DryLevel { get; private set; }

        public XGMidiParameter LPFCutoffFreq { get; private set; }
        public XGMidiParameter LPFResonance { get; private set; }
        public XGMidiParameter HPFCutoffFreq { get; private set; }

        public XGMidiParameter EGAttack { get; private set; }
        public XGMidiParameter EGDecay { get; private set; }
        public XGMidiParameter EGRelease { get; private set; }

        public XGMidiParameter VibRate { get; private set; }
        public XGMidiParameter VibDepth { get; private set; }
        public XGMidiParameter VibDelay { get; private set; }

        public XGMidiParameter EQBassFreq { get; private set; }
        public XGMidiParameter EQBassGain { get; private set; }
        public XGMidiParameter EQTrebleFreq { get; private set; }
        public XGMidiParameter EQTrebleGain { get; private set; }

        public XGMidiParameter MWPitchControl { get; private set; }
        public XGMidiParameter MWLPFControl { get; private set; }
        public XGMidiParameter MWAmpControl { get; private set; }
        public XGMidiParameter MWLFOPModDepth { get; private set; }
        public XGMidiParameter MWLFOFModDepth { get; private set; }
        public XGMidiParameter MWLFOAModDepth { get; private set; }

        public XGPartParams(XGMidiIODevice host, int channel)
            : base(host, "Part[" + (channel + 1) + "]", 0x080000 | channel << 8)
        {
            Channel = channel;
            ProgramMSB = AddParameter("ProgramMSB", 0x01, 0x00, 0x7f, 0x00);
            ProgramLSB = AddParameter("ProgramLSB", 0x02, 0x00, 0x7f, 0x00);
            ProgramNumber = AddParameter("ProgramNumber", 0x03, 0x00, 0x7f, 0x00, XGMidiParameter.ProgramNumber);

            Pan = AddParameter("Pan", 0x0E, 0x00, 0x7f, 0x40, XGMidiParameter.PanpotPM);
            Volume = AddParameter("Volume", 0x0B, 0x00, 0x7f, 0x00);
            Reverb = AddParameter("Reverb", 0x13, 0x00, 0x7f, 0x00);
            Chorus = AddParameter("Chorus", 0x12, 0x00, 0x7f, 0x00);
            Variation = AddParameter("Variation", 0x14, 0x00, 0x7f, 0x00);
            DryLevel = AddParameter("DryLevel", 0x11, 0x00, 0x7f, 0x00);

            LPFCutoffFreq = AddParameter("LPFCutoffFreq", 0x18, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            LPFResonance = AddParameter("LPFResonance", 0x19, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            HPFCutoffFreq = AddParameter("HPFCutoffFreq", 0x020020, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            RcvNoteMessage = AddParameter("RcvNoteMessage", 0x35, 0x00, 0x01, 0x00, XGMidiParameter.OnOff);

            EGAttack = AddParameter("EGAttack", 0x1A, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EGDecay = AddParameter("EGDecay", 0x1B, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EGRelease = AddParameter("EGRelease", 0x1C, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);

            VibRate = AddParameter("VibRate", 0x15, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            VibDepth = AddParameter("VibDepth", 0x16, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            VibDelay = AddParameter("VibDelay", 0x17, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);

            EQBassFreq = AddParameter("EQBassFreq", 0x76, 0x04, 0x28, 0x0C, XGMidiParameter.FreqToString);
            EQBassGain = AddParameter("EQBassGain", 0x72, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EQTrebleFreq = AddParameter("EQTrebleFreq", 0x77, 0x1C, 0x3A, 0x36, XGMidiParameter.FreqToString);
            EQTrebleGain = AddParameter("EQTrebleGain", 0x73, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);

            MWPitchControl = AddParameter("MWPitchControl", 0x1D, 0x28, 0x58, 0x40, XGMidiParameter.CenterPM);
            MWLPFControl = AddParameter("MWLPFControl", 0x1E, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            MWAmpControl = AddParameter("MWAmpControl", 0x1F, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            MWLFOPModDepth = AddParameter("MWLFOPModDepth", 0x20, 0x00, 0x7f, 0x00);
            MWLFOFModDepth = AddParameter("MWLFOFModDepth", 0x21, 0x00, 0x7f, 0x00);
            MWLFOAModDepth = AddParameter("MWLFOAModDepth", 0x22, 0x00, 0x7f, 0x00);
        }

        public override void RequestDump()
        {
            base.RequestDump();
            Host.SendXGBulkDumpRequest(BaseAddress);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x30);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x70);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x74);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x020020);
        }
    }

    public class XGDrumParams : ParameterCollection
    {
        public int SetupNumber { get; private set; }
        public int NoteNumber { get; private set; }


        public XGMidiParameter PitchCoarse { get; private set; }
        public XGMidiParameter PitchFine { get; private set; }
        public XGMidiParameter Volume { get; private set; }
        public XGMidiParameter Pan { get; private set; }

        public XGMidiParameter Reverb { get; private set; }
        public XGMidiParameter Chorus { get; private set; }
        public XGMidiParameter Variation { get; private set; }

        public XGMidiParameter LPFCutoffFreq { get; private set; }
        public XGMidiParameter LPFResonance { get; private set; }
        public XGMidiParameter HPFCutoffFreq { get; private set; }

        public XGMidiParameter EGAttackRate { get; private set; }
        public XGMidiParameter EGDecay1Rate { get; private set; }
        public XGMidiParameter EGDecay2Rate { get; private set; }

        public XGMidiParameter EQBassGain { get; private set; }
        public XGMidiParameter EQTrebleGain { get; private set; }
        public XGMidiParameter EQBassFreq { get; private set; }
        public XGMidiParameter EQTrebleFreq { get; private set; }

        public XGMidiParameter AltGroup { get; private set; }
        public XGMidiParameter KeyAssign { get; private set; }
        public XGMidiParameter RcvNoteOff { get; private set; }
        public XGMidiParameter RcvNoteOn { get; private set; }

        public XGMidiParameter VelocitySensePitch { get; private set; }
        public XGMidiParameter VelocitySenseLPFCutoff { get; private set; }

        public XGDrumParams(XGMidiIODevice host, int setup, int note)
            : base(host, "DRUM SETUP S" + (setup + 1) + "N" + note, 0x300000 | setup << 16 | note << 8)
        {
            this.SetupNumber = setup;
            this.NoteNumber = note;

            PitchCoarse = AddParameter("PitchCoarse", 0x00, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            PitchFine = AddParameter("PitchFine", 0x01, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            Volume = AddParameter("Volume", 0x02, 0x00, 0x7f, 0x00);
            AltGroup = AddParameter("AltGroup", 0x03, 0x00, 0x7f, 0x00);
            Pan = AddParameter("Pan", 0x04, 0x00, 0x7f, 0x40, XGMidiParameter.PanpotPM);
            Reverb = AddParameter("Reverb", 0x05, 0x00, 0x7f, 0x00);
            Chorus = AddParameter("Chorus", 0x06, 0x00, 0x7f, 0x00);
            Variation = AddParameter("Variation", 0x07, 0x00, 0x7f, 0x00);
            KeyAssign = AddParameter("KeyAssign", 0x08, 0x00, 0x7f, 0x00);
            RcvNoteOff = AddParameter("RcvNoteOff", 0x09, 0x00, 0x01, 0x00, XGMidiParameter.OnOff);
            RcvNoteOn = AddParameter("RcvNoteOn", 0x0A, 0x00, 0x01, 0x00, XGMidiParameter.OnOff);

            LPFCutoffFreq = AddParameter("LPFCutoffFreq", 0x0B, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            LPFResonance = AddParameter("LPFResonance", 0x0C, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EGAttackRate = AddParameter("EGAttackRate", 0x0D, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EGDecay1Rate = AddParameter("EGDecay1Rate", 0x0E, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EGDecay2Rate = AddParameter("EGDecay2Rate", 0x0F, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);

            EQBassGain = AddParameter("EQBassGain", 0x20, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EQTrebleGain = AddParameter("EQTrebleGain", 0x21, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);
            EQBassFreq = AddParameter("EQBassFreq", 0x24, 0x04, 0x28, 0x04, XGMidiParameter.FreqToString);
            EQTrebleFreq = AddParameter("EQTrebleFreq", 0x25, 0x1C, 0x3A, 0x1C, XGMidiParameter.FreqToString);

            HPFCutoffFreq = AddParameter("HPFCutoffFreq", 0x50, 0x00, 0x7f, 0x40, XGMidiParameter.CenterPM);

            VelocitySensePitch = AddParameter("VelocitySensePitch", 0x60, 0x30, 0x50, 0x40, XGMidiParameter.CenterPM);
            VelocitySenseLPFCutoff = AddParameter("VelocitySenseLPFCutoff", 0x61, 0x30, 0x50, 0x40, XGMidiParameter.CenterPM);
        }

        public override void RequestDump()
        {
            base.RequestDump();
            Host.SendXGBulkDumpRequest(BaseAddress);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x20);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x50);
            Host.SendXGBulkDumpRequest(BaseAddress + 0x60);
        }
    }



    public class XGMultiEQParams : ParameterCollection
    {
        public XGMidiParameter Type { get; private set; }
        
        public XGMidiParameter Gain1 { get; private set; }
        public XGMidiParameter Freq1 { get; private set; }
        public XGMidiParameter Q1 { get; private set; }
        public XGMidiParameter Shape1 { get; private set; }

        public XGMidiParameter Gain2 { get; private set; }
        public XGMidiParameter Freq2 { get; private set; }
        public XGMidiParameter Q2 { get; private set; }

        public XGMidiParameter Gain3 { get; private set; }
        public XGMidiParameter Freq3 { get; private set; }
        public XGMidiParameter Q3 { get; private set; }

        public XGMidiParameter Gain4 { get; private set; }
        public XGMidiParameter Freq4 { get; private set; }
        public XGMidiParameter Q4 { get; private set; }

        public XGMidiParameter Gain5 { get; private set; }
        public XGMidiParameter Freq5 { get; private set; }
        public XGMidiParameter Q5 { get; private set; }
        public XGMidiParameter Shape5 { get; private set; }

        public class BandParameter
        {
            public XGMidiParameter Gain { get; private set; }
            public XGMidiParameter Frequency { get; private set; }
            public XGMidiParameter Q { get; private set; }
            public XGMidiParameter Shape { get; private set; }
            public XGMidiParameter LowerShape { get; private set; }
            public XGMidiParameter HigherShape { get; private set; }

            static int[] freqTable =
            {
                20, 22, 25, 28, 32, 36, 40, 45, 50, 56, 63, 70, 80, 90, 100,
                110, 125, 140, 160, 180, 200, 225, 250, 280, 315, 355, 400, 450, 500, 560, 630, 700, 800, 900, 1000,
                1100, 1200, 1400, 1600, 1800, 2000, 2200, 2500, 2800, 3200, 3600, 4000, 4500, 5000, 5600, 6300, 7000, 8000, 9000, 10000,
                11000, 12000, 14000, 16000, 18000, 20000
            };
            public double ActualFrequency { get { return freqTable[Frequency.Value]; } }
            public double ActualQ { get { return Q.Value / 10.0; } }
            public double ActualGain { get { return Gain.Value - 0x40; } }
            public bool LowerShelving { get { return LowerShape != null ? LowerShape.Value == 0 : false; } }
            public bool HigherShelving { get { return HigherShape != null ? HigherShape.Value == 0 : false; } }

            public BandParameter(XGMidiParameter gain, XGMidiParameter freq, XGMidiParameter q, XGMidiParameter lowerShape, XGMidiParameter higherShape)
            {
                Debug.Assert(!(this.LowerShape != null && this.HigherShape != null)); // 両方が!nullであってはならぬ。
                this.Gain = gain;
                this.Frequency = freq;
                this.Q = q;
                this.LowerShape = lowerShape;
                this.HigherShape = higherShape;
                this.Shape = this.LowerShape ?? this.HigherShape;
            }
        }

        BandParameter[] bandParams;
        public ReadOnlyCollection<BandParameter> Bands;

        public XGMultiEQParams(XGMidiIODevice host)
            : base(host, "MULTI EQ", 0x024000)
        {
            bandParams = new BandParameter[5];

            ToStringDelegate dBStringFunc = XGMidiParameter.MakePlusMinusToStringFunc("-", "dB", "+0dB", "+", "dB", null);
            ToStringDelegate QStringFunc = v => (v * 0.1).ToString("0.0");
            Type = AddParameter("Type", 0, 0, 4, 0, XGMidiParameter.MakeTableToStringFunc("FLAT", "JAZZ", "POPS", "ROCK", "CLASSIC"));
            Gain1 = AddParameter("Gain1", 1, 0x34, 0x4C, 0x40, dBStringFunc);
            Freq1 = AddParameter("Freq1", 2, 0x04, 0x28, 0x04, XGMidiParameter.FreqToString);
            Q1 = AddParameter("Q1", 3, 0x01, 0x78, 0x07, QStringFunc);
            Shape1 = AddParameter("Shape1", 4, 0, 1, 0, XGMidiParameter.MakeTableToStringFunc("SHELVING", "PEAKING"));
            bandParams[0] = new BandParameter(Gain1, Freq1, Q1, Shape1, null);

            Gain2 = AddParameter("Gain2", 5, 0x34, 0x4C, 0x40, dBStringFunc);
            Freq2 = AddParameter("Freq2", 6, 0x0E, 0x36, 0x0E, XGMidiParameter.FreqToString);
            Q2 = AddParameter("Q2", 7, 0x01, 0x78, 0x07, QStringFunc);
            bandParams[1] = new BandParameter(Gain2, Freq2, Q2, null, null);

            Gain3 = AddParameter("Gain3", 9, 0x34, 0x4C, 0x40, dBStringFunc);
            Freq3 = AddParameter("Freq3", 10, 0x0E, 0x36, 0x0E, XGMidiParameter.FreqToString);
            Q3 = AddParameter("Q3", 11, 0x01, 0x78, 0x07, QStringFunc);
            bandParams[2] = new BandParameter(Gain3, Freq3, Q3, null, null);

            Gain4 = AddParameter("Gain4", 13, 0x34, 0x4C, 0x40, dBStringFunc);
            Freq4 = AddParameter("Freq4", 14, 0x0E, 0x36, 0x0E, XGMidiParameter.FreqToString);
            Q4 = AddParameter("Q4", 15, 0x01, 0x78, 0x07, QStringFunc);
            bandParams[3] = new BandParameter(Gain4, Freq4, Q4, null, null);

            Gain5 = AddParameter("Gain5", 17, 0x34, 0x4C, 0x40, dBStringFunc);
            Freq5 = AddParameter("Freq5", 18, 0x1C, 0x3A, 0x1C, XGMidiParameter.FreqToString);
            Q5 = AddParameter("Q5", 19, 0x01, 0x78, 0x07, QStringFunc);
            Shape5 = AddParameter("Shape5", 20, 0, 1, 0, XGMidiParameter.MakeTableToStringFunc("SHELVING", "PEAKING"));
            bandParams[4] = new BandParameter(Gain5, Freq5, Q5, null, Shape5);

            Bands = new ReadOnlyCollection<BandParameter>(bandParams);
        }

        public override void RequestDump()
        {
            base.RequestDump();
            Host.SendXGBulkDumpRequest(BaseAddress);
        }
    }

    public enum XGEffectBlockType
    {
        Reverb,
        Chorus,
        Variation,
        Insertion1,
        Insertion2,
        Insertion3,
        Insertion4,
    }

    public class XGEffectParams : ParameterCollection
    {
        public XGEffect effect_;
        public XGEffect Effect { get { return effect_; } set { SetEffect(value); } }

        public XGMidiParameter EffectType { get; private set; }
        public XGMidiParameter Return { get; private set; }
        public XGMidiParameter Pan { get; private set; }
        public XGMidiParameter SendToReverb { get; private set; }
        public XGMidiParameter SendToChorus { get; private set; }
        public XGMidiParameter VariationConnect { get; private set; }
        public XGMidiParameter PartNumber { get; private set; }

        public XGMidiParameter Parameter1 { get { return efctParams[0]; } }
        public XGMidiParameter Parameter2 { get { return efctParams[1]; } }
        public XGMidiParameter Parameter3 { get { return efctParams[2]; } }
        public XGMidiParameter Parameter4 { get { return efctParams[3]; } }
        public XGMidiParameter Parameter5 { get { return efctParams[4]; } }
        public XGMidiParameter Parameter6 { get { return efctParams[5]; } }
        public XGMidiParameter Parameter7 { get { return efctParams[6]; } }
        public XGMidiParameter Parameter8 { get { return efctParams[7]; } }
        public XGMidiParameter Parameter9 { get { return efctParams[8]; } }
        public XGMidiParameter Parameter10 { get { return efctParams[9]; } }
        public XGMidiParameter Parameter11 { get { return efctParams[10]; } }
        public XGMidiParameter Parameter12 { get { return efctParams[11]; } }
        public XGMidiParameter Parameter13 { get { return efctParams[12]; } }
        public XGMidiParameter Parameter14 { get { return efctParams[13]; } }
        public XGMidiParameter Parameter15 { get { return efctParams[14]; } }
        public XGMidiParameter Parameter16 { get { return efctParams[15]; } }

        XGMidiParameter[] efctParams;
        public ReadOnlyCollection<XGMidiParameter> EffectParameters;
        public static int GetEffectBlockAddress(XGEffectBlockType type)
        {
            switch (type)
            {
                case XGEffectBlockType.Reverb: return 0x020100;
                case XGEffectBlockType.Chorus: return 0x020120;
                case XGEffectBlockType.Variation: return 0x020140;
                case XGEffectBlockType.Insertion1: return 0x030000;
                case XGEffectBlockType.Insertion2: return 0x030100;
                case XGEffectBlockType.Insertion3: return 0x030200;
                case XGEffectBlockType.Insertion4: return 0x030300;
            }
            throw new ArgumentOutOfRangeException();
        }

        public XGEffectBlockType BlockType { get; private set; }
        public XGEffectParams(XGMidiIODevice host, XGEffectBlockType type)
            : base(host, "Effect[" + type + "]", GetEffectBlockAddress(type))
        {
            BlockType = type;
            efctParams = new XGMidiParameter[16];

            int address = 0;
            EffectType = AddParameter("EffectType", address++, 0x00, 0x00, 0x00);
            EffectType.ToStringConverter = v => XGEffect.GetEffectByTypeValue(v << 1 & 0x7F00 | v & 0x7F).Name;
            EffectType.Count = 2;
            address++;

            bool isSystemEffect = type <= XGEffectBlockType.Variation;
            bool isInsEffect = type >= XGEffectBlockType.Variation;
            bool hasSendToRev = type == XGEffectBlockType.Chorus || type == XGEffectBlockType.Variation;
            bool hasSendToCho = type == XGEffectBlockType.Variation;
            bool hasVarConnect = type == XGEffectBlockType.Variation;

            for (int i = 0; i < 10; i++)
            {
                if (type != XGEffectBlockType.Variation)
                {
                    efctParams[i] = AddParameter("Parameter " + i, address, 0, 127, 0);
                    address += 1;
                }
                else
                {
                    efctParams[i] = AddParameter("Parameter " + i, address, 0, 16384, 0);
                    efctParams[i].Count = 2;
                    address += 2;
                }
            }

            Return = isSystemEffect ? AddParameter("Return", address++, 0x00, 0x7f, 0x00) : null;
            Pan = isSystemEffect ? AddParameter("Pan", address++, 0x00, 0x7f, 0x40, XGMidiParameter.PanpotPM) : null;
            SendToReverb = hasSendToRev ? AddParameter("SendToReverb", address++, 0x00, 0x7f, 0x00) : null;
            SendToChorus = hasSendToCho ? AddParameter("SendToChorus", address++, 0x00, 0x7f, 0x00) : null;
            VariationConnect = hasVarConnect ? AddParameter("VariationConnect", address++, 0x00, 0x01, 0x00, v => v == 0 ? "Insertion" : v == 1 ? "System" : "? " + v) : null;
            if (isInsEffect)
            {
                PartNumber = AddParameter("PartNumber", address++, 0, 66, 0);
                PartNumber.ToStringConverter = v => v == 66 ? "OFF" : v < 64 ? (v + 1).ToString() : v == 64 ? "AD1" : v == 65 ? "AD2" : "? " + v;
                PartNumber.ReadValueEncoding = v => v != 127 ? v : 66;
                PartNumber.WriteValueEncoding = v => v != 66 ? v : 127;
            }

            for (int i = 0; i < 6; i++)
            {
                int subBlock =
                    type == XGEffectBlockType.Reverb ? 0x10 :
                    type == XGEffectBlockType.Chorus ? 0x10 :
                    type == XGEffectBlockType.Variation ? 0x30 :
                    type >= XGEffectBlockType.Insertion1 ? 0x20 :
                    0;

                efctParams[10 + i] = AddParameter("---", subBlock + i, 0, 127, 0);
                address += 1;
            }

            EffectParameters = new ReadOnlyCollection<XGMidiParameter>(efctParams);

            ReLink();
        }

        public void ReLink()
        {
            int v = EffectType.Value;
            XGEffect effect = XGEffect.GetEffectByTypeValue(v & 0x7f | v << 1 & 0x7f00);
            if (effect == null) { return; }

            this.effect_ = effect;


            if (this.BlockType >= XGEffectBlockType.Insertion1)
            {
                for (int i = 0; i < 10; i++)
                {
                    efctParams[i].Address = effect.ExDataUsed ? BaseAddress + 0x30 + i * 2 : BaseAddress + 0x02 + i * 1;
                    efctParams[i].Count = effect.ExDataUsed ? 2 : 1;
                }
            }

            XGEffect.XGEffectParam dummy = new XGEffect.XGEffectParam("---", 0, 0, 0, "", "---");
            for (int i = 0; i < 16; i++)
            {
                XGEffect.XGEffectParam param = effect.ParameterTypes[i] ?? dummy;
                efctParams[i].Name = this.Name + " " + param.Name;
                efctParams[i].Description = param.Description;
                efctParams[i].MinValue = param.MinValue;
                efctParams[i].MaxValue = param.MaxValue;
                efctParams[i].ToStringConverter = param.Conveter;
                efctParams[i].CenterValue = param.MinValue;
            }
        }

        public void SetEffect(XGEffect effect)
        {
            int v = effect.EffectValue;
            EffectType.Value = v & 0x7f | v >> 1 & 0x3f80;
            ReLink();
            for (int i = 0; i < 16; i++) { efctParams[i].Value = effect.InitialValues[i]; }
        }

        public void WriteEffect(XGEffect effect)
        {
            int v = effect.EffectValue;
            EffectType.WriteValue(v & 0x7f | v >> 1 & 0x3f80);
            ReLink();
            for (int i = 0; i < 16; i++) { efctParams[i].WriteValue(effect.InitialValues[i]); }
        }

        public override void RequestDump()
        {
            base.RequestDump();
            Host.SendXGBulkDumpRequest(BaseAddress);

            switch (BlockType)
            {
                case XGEffectBlockType.Reverb:
                case XGEffectBlockType.Chorus:
                    Host.SendXGBulkDumpRequest(BaseAddress + 0x10);
                    break;
                case XGEffectBlockType.Variation:
                    Host.SendXGBulkDumpRequest(BaseAddress + 0x30);
                    break;
                case XGEffectBlockType.Insertion1:
                case XGEffectBlockType.Insertion2:
                case XGEffectBlockType.Insertion3:
                case XGEffectBlockType.Insertion4:
                    Host.SendXGBulkDumpRequest(BaseAddress + 0x20);
                    break;
                default:
                    Debug.Fail("");
                    break;
            }

        }
    }
}
