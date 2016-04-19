using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Tsukikage.XGTGCtrl2.XG
{
    /// <summary>
    /// XGのEffectをあらわす
    /// </summary>
    public class XGEffect
    {
        public static ReadOnlyCollection<XGEffect> AllEffects { get; private set; }
        public static XGEffect GetEffectByTypeValue(int typeValue) { return effectTypeTable.Find(e => e.EffectValue == typeValue); }
        public static XGEffect GetEffectByName(string name) { return effectTypeTable.Find(e => e.Name == name); }

        public int EffectValue { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ReadOnlyCollection<XGEffectParam> ParameterTypes { get; private set; }
        public ReadOnlyCollection<int> InitialValues { get; private set; }
        public bool SelectableForReverb { get; private set; }
        public bool SelectableForChorus { get; private set; }
        public bool ExDataUsed { get; private set; }

        public class XGEffectParam
        {
            public string Name { get; private set; }
            public int MinValue { get; private set; }
            public int MaxValue { get; private set; }
            public ToStringDelegate Conveter { get; private set; }
            public string Unit { get; private set; }
            public string Description { get; private set; }
            public bool InsertionOnly { get; private set; }
            public XGEffectParam(string name, int min, int max, int tableID, string unit, string desc)
            {
                this.Name = name;
                this.MinValue = min;
                this.MaxValue = max;
                this.Conveter = v => valueTypeTable[tableID & 0xFF](v) + unit;
                this.InsertionOnly = (tableID & 0x100) != 0;
                this.Unit = unit;
                this.Description = desc;
            }
        }

        private XGEffect(EffectType effectType, int value, string name, string description, int[] paramTypeIDs, int[] initialValues)
        {
            Debug.Assert(paramTypeIDs.Length == 16);
            Debug.Assert(initialValues.Length == 16);

            this.EffectValue = value;
            this.Name = name;
            this.Description = description;

            this.ParameterTypes = new ReadOnlyCollection<XGEffectParam>(Array.ConvertAll(paramTypeIDs, id => paramTypeTable[id]));
            this.InitialValues = new ReadOnlyCollection<int>(initialValues);
            this.SelectableForChorus = (effectType & EffectType.Chorus) != 0;
            this.SelectableForReverb = (effectType & EffectType.Reverb) != 0;
            this.ExDataUsed = (effectType & EffectType.UseDataMSB) != 0;
        }

        [Flags]
        enum EffectType
        {
            Normal = 0,
            Reverb = 1,
            Chorus = 2,
            UseDataMSB = 4,
        }

        static Dictionary<int, ToStringDelegate> valueTypeTable = new Dictionary<int, ToStringDelegate>();
        static Dictionary<int, XGEffectParam> paramTypeTable = new Dictionary<int, XGEffectParam>();
        static List<XGEffect> effectTypeTable = new List<XGEffect>();

        static ToStringDelegate MakeTableToStringFunc(params string[] bind)
        {
            return (int value) => value >= 0 && value < bind.Length ? bind[value] : "?" + value.ToString();
        }

        static ToStringDelegate MakeCsvTableToStringFunc(string bind)
        {
            return MakeTableToStringFunc(bind.Split(','));
        }

        static ToStringDelegate MakeStepStringFunc(int initial, int step, int count, Converter<int, string> converter)
        {
            string[] table = new string[count];
            for (int i = 0; i < count; i++)
            {
                table[i] = converter(initial + step * i);
            }
            return MakeTableToStringFunc(table);
        }

        static ToStringDelegate MakeStepStringFunc(int initial, int step) { return MakeStepStringFunc(initial, step, 128, v => v.ToString()); }

        static ToStringDelegate MakePlusMinusToStringFunc(string center, string minusPre, string minusPost, string plusPre, string plusPost)
        {
            string[] table = new string[128];
            for (int i = 0; i < 64; i++) { table[i] = minusPre + (64 - i).ToString() + minusPost; }
            table[64] = center;
            for (int i = 65; i < 128; i++) { table[i] = plusPre + (i - 64).ToString() + plusPost; }
            return MakeTableToStringFunc(table);
        }

        static void InitializeAllParams()
        {
            valueTypeTable[0] = MakeStepStringFunc(0, 1);
            valueTypeTable[1] = MakeCsvTableToStringFunc("0.00,0.04,0.08,0.13,0.17,0.21,0.25,0.29,0.34,0.38,0.42,0.46,0.51,0.55,0.59,0.63,0.67,0.72,0.76,0.80,0.84,0.88,0.93,0.97,1.01,1.05,1.09,1.14,1.18,1.22,1.26,1.30,1.35,1.39,1.43,1.47,1.51,1.56,1.60,1.64,1.68,1.72,1.77,1.81,1.85,1.89,1.94,1.98,2.02,2.06,2.10,2.15,2.19,2.23,2.27,2.31,2.36,2.40,2.44,2.48,2.52,2.57,2.61,2.65,2.69,2.78,2.86,2.94,3.03,3.11,3.20,3.28,3.37,3.45,3.53,3.62,3.70,3.87,4.04,4.21,4.37,4.54,4.71,4.88,5.05,5.22,5.38,5.55,5.72,6.06,6.39,6.73,7.07,7.40,7.74,8.08,8.41,8.75,9.08,9.42,9.76,10.1,10.8,11.4,12.1,12.8,13.5,14.1,14.8,15.5,16.2,16.8,17.5,18.2,19.5,20.9,22.2,23.6,24.9,26.2,27.6,28.9,30.3,31.6,33.0,34.3,37.0,39.7");
            valueTypeTable[2] = MakeCsvTableToStringFunc("0.0,0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9,1.0,1.1,1.2,1.3,1.4,1.5,1.6,1.7,1.8,1.9,2.0,2.1,2.2,2.3,2.4,2.5,2.6,2.7,2.8,2.9,3.0,3.1,3.2,3.3,3.4,3.5,3.6,3.7,3.8,3.9,4.0,4.1,4.2,4.3,4.4,4.5,4.6,4.7,4.8,4.9,5.0,5.1,5.2,5.3,5.4,5.5,5.6,5.7,5.8,5.9,6.0,6.1,6.2,6.3,6.4,6.5,6.6,6.7,6.8,6.9,7.0,7.1,7.2,7.3,7.4,7.5,7.6,7.7,7.8,7.9,8.0,8.1,8.2,8.3,8.4,8.5,8.6,8.7,8.8,8.9,9.0,9.1,9.2,9.3,9.4,9.5,9.6,9.7,9.8,9.9,10.0,11.1,12.2,13.3,14.4,15.5,17.1,18.6,20.2,21.8,23.3,24.9,26.5,28.0,29.6,31.2,32.8,34.3,35.9,37.5,39.0,40.6,42.2,43.7,45.3,46.9,48.4,50.0");
            valueTypeTable[3] = MakeCsvTableToStringFunc("20,22,25,28,32,36,40,45,50,56,63,70,80,90,100,110,125,140,160,180,200,225,250,280,315,355,400,450,500,560,630,700,800,900,1.0k,1.1k,1.2k,1.4k,1.6k,1.8k,2.0k,2.2k,2.5k,2.8k,3.2k,3.6k,4.0k,4.5k,5.0k,5.6k,6.3k,7.0k,8.0k,9.0k,10.0k,11.0k,12.0k,14.0k,16.0k,18.0k,20.0k");
            valueTypeTable[4] = MakeCsvTableToStringFunc("0.3,0.4,0.5,0.6,0.7,0.8,0.9,1.0,1.1,1.2,1.3,1.4,1.5,1.6,1.7,1.8,1.9,2.0,2.1,2.2,2.3,2.4,2.5,2.6,2.7,2.8,2.9,3.0,3.1,3.2,3.3,3.4,3.5,3.6,3.7,3.8,3.9,4.0,4.1,4.2,4.3,4.4,4.5,4.6,4.7,4.8,4.9,5.0,5.5,6.0,6.5,7.0,7.5,8.0,8.5,9.0,9.5,10.0,11.0,12.0,13.0,14.0,15.0,16.0,17.0,18.0,19.0,20.0,25.0,30.0");
            valueTypeTable[5] = MakeCsvTableToStringFunc("0.1,1.7,3.2,4.8,6.4,8.0,9.5,11.1,12.7,14.3,15.8,17.4,19.0,20.6,22.1,23.7,25.3,26.9,28.4,30.0,31.6,33.2,34.7,36.3,37.9,39.5,41.0,42.6,44.2,45.7,47.3,48.9,50.5,52.0,53.6,55.2,56.8,58.3,59.9,61.5,63.1,64.6,66.2,67.8,69.4,70.9,72.5,74.1,75.7,77.2,78.8,80.4,81.9,83.5,85.1,86.7,88.2,89.8,91.4,93.0,94.5,96.1,97.7,99.3,100.8,102.4,104.0,105.6,107.1,108.7,110.3,111.9,113.4,115.0,116.6,118.2,119.7,121.3,122.9,124.4,126.0,127.6,129.2,130.7,132.3,133.9,135.5,137.0,138.6,140.2,141.8,143.3,144.9,146.5,148.1,149.6,151.2,152.8,154.4,155.9,157.5,159.1,160.6,162.2,163.8,165.4,166.9,168.5,170.1,171.7,173.2,174.8,176.4,178.0,179.5,181.1,182.7,184.3,185.8,187.4,189.0,190.6,192.1,193.7,195.3,196.9,198.4,200.0");
            valueTypeTable[6] = MakeCsvTableToStringFunc("0.1,0.3,0.4,0.6,0.7,0.9,1.0,1.2,1.4,1.5,1.7,1.8,2.0,2.1,2.3,2.5,2.6,2.8,2.9,3.1,3.2,3.4,3.5,3.7,3.9,4.0,4.2,4.3,4.5,4.6,4.8,5.0,5.1,5.3,5.4,5.6,5.7,5.9,6.1,6.2,6.4,6.5,6.7,6.8,7.0");
            valueTypeTable[7] = MakeCsvTableToStringFunc("0.1,3.2,6.4,9.5,12.7,15.8,19.0,22.1,25.3,28.4,31.6,34.7,37.9,41.0,44.2,47.3,50.5,53.6,56.8,59.9,63.1,66.2,69.4,72.5,75.7,78.8,82.0,85.1,88.3,91.4,94.6,97.7,100.9,104.0,107.2,110.3,113.5,116.6,119.8,122.9,126.1,129.2,132.4,135.5,138.6,141.8,144.9,148.1,151.2,154.4,157.5,160.7,163.8,167.0,170.1,173.3,176.4,179.6,182.7,185.9,189.0,192.2,195.3,198.5,201.6,204.8,207.9,211.1,214.2,217.4,220.5,223.7,226.8,230.0,233.1,236.3,239.4,242.6,245.7,248.9,252.0,255.2,258.3,261.5,264.6,267.7,270.9,274.0,277.2,280.3,283.5,286.6,289.8,292.9,296.1,299.2,302.4,305.5,308.7,311.8,315.0,318.1,321.3,324.4,327.6,330.7,333.9,337.0,340.2,343.3,346.5,349.6,352.8,355.9,359.1,362.2,365.4,368.5,371.7,374.8,378.0,381.1,384.3,387.4,390.6,393.7,396.9,400.0");
            valueTypeTable[8] = MakeCsvTableToStringFunc("1,2,3,4,5,6,7,8,9,10,12,14,16,18,20,23,26,30,35,40");
            valueTypeTable[9] = MakeCsvTableToStringFunc("10,15,25,35,45,55,65,75,85,100,115,140,170,230,340,680");
            valueTypeTable[10] = MakeCsvTableToStringFunc("1.0,1.5,2.0,3.0,5.0,7.0,10.0,20.0");
            valueTypeTable[11] = MakeCsvTableToStringFunc("0.5,0.8,1.0,1.3,1.5,1.8,2.0,2.3,2.6,2.8,3.1,3.3,3.6,3.9,4.1,4.4,4.6,4.9,5.2,5.4,5.7,5.9,6.2,6.5,6.7,7.0,7.2,7.5,7.8,8.0,8.3,8.6,8.8,9.1,9.4,9.6,9.9,10.2,10.4,10.7,11.0,11.2,11.5,11.8,12.1,12.3,12.6,12.9,13.1,13.4,13.7,14.0,14.2,14.5,14.8,15.1,15.4,15.6,15.9,16.2,16.5,16.8,17.1,17.3,17.6,17.9,18.2,18.5,18.8,19.1,19.4,19.7,20.0,20.2,20.5,20.8,21.1,21.4,21.7,22.0,22.4,22.7,23.0,23.3,23.6,23.9,24.2,24.5,24.9,25.2,25.5,25.8,26.1,26.5,26.8,27.1,27.5,27.8,28.1,28.5,28.8,29.2,29.5,29.9,30.2");
            valueTypeTable[12] = MakeCsvTableToStringFunc(",,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,10,15,25,35,45,55,65,75,85,100,115,140,170,230,340,680");
            valueTypeTable[13] = MakeCsvTableToStringFunc("44.1k,22.1k,14.7k,11.0k,8.8k,7.4k,6.3k,5.5k,4.9k,4.4k,4.0k,3.7k,3.4k,3.2k,2.9k,2.8k,2.6k,2.5k,2.3k,2.2k,2.1k,2.0k,1.92k,1.84k,1.76k,1.70k,1.63k,1.58k,1.52k,1.47k,1.42k,1.38k,1.34k,1.30k,1.26k,1.23k,1.19k,1.16k,1.13k,1.10k,1.08k,1.05k,1.03k,1.00k,980.0,959.0,938.0,919.0,900.0,882.0,865.0,848.0,832.0,817.0,802.0,788.0,774.0,760.0,747.0,735.0,723.0,711.0,700.0,689.0,678.0,668.0,658.0,649.0,639.0,630.0,621.0,613.0,604.0,596.0,588.0,580.0,573.0,565.0,558.0,551.0,544.0,538.0,531.0,525.0,519.0,513.0,507.0,501.0,496.0,490.0,485.0,479.0,474.0,469.0,464.0,459.0,455.0,450.0,445.0,441.0,437.0,432.0,428.0,424.0,420.0,416.0,412.0,408.0,405.0,401.0,397.0,394.0,390.0,387.0,383.0,380.0,377.0,374.0,371.0,368.0,364.0,361.0,359.0,356.0,353.0,350.0,347.0,345.0");
            valueTypeTable[14] = MakeCsvTableToStringFunc("64th/3,64th.,32th,32th/3,32th.,16th,16th/3,16th.,8th,8th/3,8th.,4th,4th/3,4th.,2nd,2nd/3,2nd.,4thX4,4thX5,4thX6,4thX7,4thX8,4thX9,4thX10,4thX11,4thX12,4thX13,4thX14,4thX15,4thX16,4thX17,4thX18,4thX19,4thX20,4thX21,4thX22,4thX23,4thX24,4thX25,4thX26,4thX27,4thX28,4thX29,4thX30,4thX31,4thX32,4thX33,4thX34,4thX35,4thX36,4thX37,4thX38,4thX39,4thX40,4thX41,4thX42,4thX43,4thX44,4thX45,4thX46,4thX47,4thX48,4thX49,4thX50,4thX51,4thX52,4thX53,4thX54,4thX55,4thX56,4thX57,4thX58,4thX59,4thX60,4thX61,4thX62,4thX63,4thX64");
            valueTypeTable[15] = MakeCsvTableToStringFunc("0.3,0.9,1.8,2.7,3.6,5.4,7.2,9.0,10,12,14,16,18,20,21,23,25,27,29,30,32,34,36,38,40,41,43,45,47,49,50,52,54,56,58,60,61,63,65,67,69,70,72,74,76,78,80,81,83,85,87,89,90,92,94,96,98,100,101,103,105,107,109,110,112,114,116,118,120,121,123,125,127,129,130,132,134,136,138,140,141,143,145,147,149,150,152,154,156,158,160,161,163,165,167,169,170,172,174,176,178,180,181,183,185,187,189,190,192,194,196,198,200,201,203,205,207,209,210,212,214,216,218,220,221,223,225,227");
            valueTypeTable[16] = MakeCsvTableToStringFunc("2.6,3.0,3.4,3.9,4.3,4.7,5.2,5.6,6.0,6.5,6.9,7.3,7.8,8.2,8.6,13.0,17.3,21.7,26.0,30.4,34.7,39.0,43.4,47.7,52.1,56.4,60.8,65.1,69.4,73.8,78.1,82.5,86.8,91.2,95.5,99.8,104.2,108.5,112.9,117.2,121.6,125.9,130.2,134.6,138.9,143.3,147.6,152.0,156.3,160.6,165.0,169.3,173.7,178.0,182.4,186.7,195.4,217.1,238.8,260.5,282.2,304.0,325.7,347.4,369.1,390.8,412.5,434.2,456.0,477.7,499.4,521.1,542.8,564.5,586.2,608.0,629.7,651.4,673.1,694.8,716.5,738.3,760.0,781.7,803.4,825.1,846.8,868.5,890.3,912.0,933.7,955.4,977.1,998.8,1020.5,1042.3,1064.0,1085.7,1107.4,1129.1,1150.8,1172.5,1194.3,1216.0,1237.7,1259.4,1281.1,1302.8,1346.3,1389.7,1433.1,1476.6,1520.0,1563.4,1606.8,1650.3,1693.7,1737.1,1780.6,1824.0,1867.4,1910.8,1954.3,1997.7,2041.1,2084.6,2128.0,2171.4");
            valueTypeTable[17] = MakeCsvTableToStringFunc("0.7,1.3,2.0,2.7,3.4,4.0,4.7,5.4,6.1,6.7,7.4,8.1,8.7,9.4,10.1,10.8,11.4,12.1,12.8,13.5,14.1,14.8,15.5,16.2,16.8,17.5,18.2,19.5,20.9,21.5,22.9,24.2,25.6,26.9,28.9,30.3,32.3,33.6,35.7,37.7,39.7,42.4,44.4,47.1,49.8,52.5,55.9,59.2,62.6,65.9,70.0,73.3,78.1,82.1,86.8,92.2,96.9,103.0,108.3,115.1,121.1,128.5,135.9,143.3,151.4,160.2,169.6,179.0,189.1,199.9,211.3,223.4,236.2,249.7,263.8,279.3,294.7,311.6,329.7,348.6,368.1,389.6,411.8,435.4,459.6,485.9,514.1,543.1,574.0,607.0,642.0,678.3,717.3,757.7,801.5,847.2,895.0,946.1,1000.7,1057.2,1117.7,1181.7,1249.0,1320.3,1395.7,1475.1,1559.2,1648.7,1742.9,1841.8,1947.5,2058.5,2175.6,2300.1,2431.3,2569.9,2716.6,2871.4,3035.6,3208.5,3391.6,3585.4,3790.0,4006.6,4234.8,4477.0,4732.1,5002.6");
            valueTypeTable[18] = MakeCsvTableToStringFunc("0.1,0.1,0.1,0.2,0.2,0.2,0.2,0.2,0.3,0.3,0.3,0.3,0.4,0.4,0.4,0.4,0.4,0.5,0.5,0.5,0.5,0.6,0.6,0.6,0.7,0.7,0.7,0.8,0.8,0.8,0.9,0.9,1.0,1.0,1.1,1.1,1.2,1.2,1.3,1.4,1.4,1.5,1.6,1.7,1.8,1.8,1.9,2.0,2.1,2.3,2.4,2.5,2.6,2.7,2.9,3.0,3.2,3.3,3.5,3.7,3.9,4.1,4.3,4.5,4.7,5.0,5.2,5.5,5.8,6.0,6.4,6.7,7.0,7.4,7.7,8.1,8.5,9.0,9.4,9.9,10.3,10.7,11.2,11.6,12.1,12.5,12.9,13.4,13.8,14.2,14.7,15.1,15.6,16.0,16.4,16.9,17.3,17.8,18.2,18.6,19.1,19.5,20.0,20.4,20.8,21.3,21.7,22.2,22.6,23.0,23.5,23.9,24.4,24.8,25.2,25.7,26.1,26.5,27.0,27.4,27.9,28.3,28.7,29.2,29.6,30.1,30.5,30.9,31.4,31.8,32.3,32.7,33.1,33.6,34.0,34.5,34.9,35.3,35.8,36.2");
            valueTypeTable[194] = MakeCsvTableToStringFunc("off,wet,wet+dry");
            valueTypeTable[195] = MakeCsvTableToStringFunc("1,1/2,1/4,1/8,1/16,1/32,1/64,1/128");
            valueTypeTable[196] = MakeCsvTableToStringFunc("off,on");
            valueTypeTable[197] = MakeCsvTableToStringFunc("A,B,C,D,E,F,G,H,I,J");
            valueTypeTable[198] = MakeCsvTableToStringFunc("Tri,Sine");
            valueTypeTable[199] = MakeCsvTableToStringFunc("Up,Down");
            valueTypeTable[200] = MakeCsvTableToStringFunc("LPF(12dB),LPF(18dB),LPF(24dB),HPF,BPF,BEF");
            valueTypeTable[201] = MakeCsvTableToStringFunc("off,KeyOnReset,SEQ Start Reset");
            valueTypeTable[202] = MakeCsvTableToStringFunc("Nomal,Low,Mid,High,Low/High,Low/Mid,Mid/High,Full Bit,Wild,Attacky,Low End,Hard");
            valueTypeTable[203] = MakeStepStringFunc(-100, 1);
            valueTypeTable[204] = MakeCsvTableToStringFunc("-180,-158,-135,-113,- 90,- 68,- 45,- 23,+  0,+ 23,+ 45,+ 68,+ 90,+113,+135,+158,+180");
            valueTypeTable[205] = MakeCsvTableToStringFunc("Triangle,Sine,Random");
            valueTypeTable[206] = MakeCsvTableToStringFunc("off,Stack,Combo,Tube,Crunch,Hi Gain,British");
            valueTypeTable[207] = MakeCsvTableToStringFunc("Flat,Stack,Combo,Twin,Radio,Megaphone");
            valueTypeTable[208] = MakeCsvTableToStringFunc("Transistor,Vintage Tube,Dist1,Dist2,Fuzz");
            valueTypeTable[209] = MakeCsvTableToStringFunc("off,on");
            valueTypeTable[210] = MakeCsvTableToStringFunc("Thru,PowerBass,Radio,Telephone,Clean,Low");
            valueTypeTable[211] = MakeStepStringFunc(-6, 1);
            valueTypeTable[212] = MakeCsvTableToStringFunc("a,i,u,e,o");
            valueTypeTable[213] = MakeStepStringFunc(-127, 1);
            valueTypeTable[214] = MakeCsvTableToStringFunc("Off,Stack,Combo,Tube");
            valueTypeTable[215] = MakeCsvTableToStringFunc("L<->R,L->R,L<-R,Lturn,Rturn,L/R");
            valueTypeTable[216] = MakeCsvTableToStringFunc("Slow,Fast");
            valueTypeTable[217] = MakeStepStringFunc(0, 3);
            valueTypeTable[218] = MakeCsvTableToStringFunc("nomal,invers");
            valueTypeTable[219] = MakeStepStringFunc(-192, 3);
            valueTypeTable[220] = MakeCsvTableToStringFunc("mono,stereo");
            valueTypeTable[221] = MakeCsvTableToStringFunc("TypeA,TypeB");
            valueTypeTable[222] = MakeCsvTableToStringFunc("S-H,L-H,Rdm,Rvs,Plt,Spr");
            valueTypeTable[223] = MakeCsvTableToStringFunc("L,R,L&R");
            valueTypeTable[250] = MakePlusMinusToStringFunc(" C ", "L-", "", "R+", "");
            valueTypeTable[251] = MakePlusMinusToStringFunc("L=H", "L", ">H", "L<H", "");
            valueTypeTable[252] = MakePlusMinusToStringFunc("E=R", "E", ">R", "E<R", "");
            valueTypeTable[253] = MakePlusMinusToStringFunc("D=W", "D", ">W", "D<W", "");
            valueTypeTable[254] = MakeStepStringFunc(0, 1, 16384, v => (v / 10) + "." + (v % 10));
            valueTypeTable[255] = MakePlusMinusToStringFunc("+00", "-", "", "+", "");

            paramTypeTable[0] = null;
            paramTypeTable[1] = new XGEffectParam("AEG Phase", 0, 15, 0, "x16th", "AEG の位相");
            paramTypeTable[2] = new XGEffectParam("AM Depth", 0, 127, 0, "", "音量変調の深さ");
            paramTypeTable[3] = new XGEffectParam("AMP Type", 0, 3, 214, "", "シミュレートするアンプタイプの選択");
            paramTypeTable[4] = new XGEffectParam("AMP Type", 0, 6, 214, "", "シミュレートするアンプタイプの選択");
            paramTypeTable[5] = new XGEffectParam("Analog Feel", 0, 10, 0, "", "アナログフランジャーの音質を加味する");
            paramTypeTable[6] = new XGEffectParam("Attack", 0, 19, 8, "", "コンプレッサー効果が効き始めるまでの時間");
            paramTypeTable[7] = new XGEffectParam("Attack", 0, 19, 8, "", "ゲートが開き始めるまでの時間");
            paramTypeTable[8] = new XGEffectParam("Attack Time", 0, 127, 15, "", "エンヴェロープフォロワーの立ち上がり時間");
            paramTypeTable[9] = new XGEffectParam("Auto Pan Depth", 0, 127, 0, "", "オートパンの深さ");
            paramTypeTable[10] = new XGEffectParam("Auto Pan Speed", 0, 127, 1, "Hz", "オートパンのスピード");
            paramTypeTable[11] = new XGEffectParam("Bit Assign", 0, 6, 0, "", "Word Lengthの効き方を調節");
            paramTypeTable[12] = new XGEffectParam("Freq Course", 0, 127, 17, "", "キャリアの周波数");
            paramTypeTable[13] = new XGEffectParam("Freq Fine", 0, 127, 0, "", "キャリアの周波数ファイン");
            paramTypeTable[14] = new XGEffectParam("Cch Delay", 1, 14860, 254, "ms", "センターチャンネルディレイの長さ");
            paramTypeTable[15] = new XGEffectParam("Cch Level", 1, 127, 0, "", "センターチャンネルの音量");
            paramTypeTable[16] = new XGEffectParam("Click Density", 0, 5, 0, "", "クリックの発生頻度");
            paramTypeTable[17] = new XGEffectParam("Click Level", 0, 127, 0, "", "クリックのレベル");
            paramTypeTable[18] = new XGEffectParam("Crossover Freq", 14, 54, 3, "Hz", "高音側スピーカーと低音側スピーカーのクロスオーバー周波数");
            paramTypeTable[19] = new XGEffectParam("CutoffFreqOfst", 0, 127, 0, "", "ワウフィルターを制御する周波数オフセット値");
            paramTypeTable[20] = new XGEffectParam("Delay Mix", 0, 127, 255, "", "ディレイ量のミキシング量");
            paramTypeTable[21] = new XGEffectParam("Delay Offset", 0, 127, 2, "", "ディレイ変調のオフセット値");
            paramTypeTable[22] = new XGEffectParam("Delay Offset", 0, 139, 18, "", "ディレイ変調のオフセット値");
            paramTypeTable[23] = new XGEffectParam("Delay Time", 0, 19, 14, "", "ディレイの長さを音符で指定する");
            paramTypeTable[24] = new XGEffectParam("Delay Time", 0, 127, 2, "ms", "ディレイの長さ");
            paramTypeTable[25] = new XGEffectParam("Delay Time", 0, 127, 7, "ms", "カラオケエコーの反射音の間隔");
            paramTypeTable[26] = new XGEffectParam("Delay Time L>R", 0, 19, 14, "", "左(入力) から右(出力) へのディレイの長さを音符で指定する");
            paramTypeTable[27] = new XGEffectParam("Delay Offset", 0, 127, 0, "", "ディレイ変調のオフセット値");
            paramTypeTable[28] = new XGEffectParam("Delay Time R>L", 0, 19, 14, "", "右(入力) から左(出力) へのディレイの長さを音符で指定する");
            paramTypeTable[29] = new XGEffectParam("Delay2 Level", 0, 127, 0, "", "2本目のディレイの音量");
            paramTypeTable[30] = new XGEffectParam("Density", 0, 4, 0, "", "反射音の密度(値が大きいほどきめ細かくなる)");
            paramTypeTable[31] = new XGEffectParam("Density", 0, 3, 0, "", "反射音の密度(値が大きいほどきめ細かくなる)");
            paramTypeTable[32] = new XGEffectParam("Depth", 0, 104, 11, "m", "シミュレートする部屋の奥行き");
            paramTypeTable[33] = new XGEffectParam("Detune", 14, 114, 255, "ct", "音程をずらす量");
            paramTypeTable[34] = new XGEffectParam("Device", 0, 4, 208, "", "音の歪み方を変化させるデバイスを選ぶ");
            paramTypeTable[35] = new XGEffectParam("Diffusion", 0, 10, 0, "", "拡がり感をコントロールする");
            paramTypeTable[36] = new XGEffectParam("Diffusion", 0, 1, 220, "", "拡がり感をコントロールする");
            paramTypeTable[37] = new XGEffectParam("Direction", 0, 1, 199, "", "エンヴェロープフォロワー変調の向き");
            paramTypeTable[38] = new XGEffectParam("Divide Level", 0, 127, 0, "", "スライスする最小レベル");
            paramTypeTable[39] = new XGEffectParam("Divide Type", 5, 11, 14, "", "スライスするタイミングを音符で指定 5/8/11");
            paramTypeTable[40] = new XGEffectParam("Drive", 0, 127, 0, "", "歪み方の度合");
            paramTypeTable[41] = new XGEffectParam("Drive", 0, 127, 0, "", "エキサイター効果をかける度合");
            paramTypeTable[42] = new XGEffectParam("Drive High", 1, 127, 0, "", "高音側スピーカーの回転による変調の深さ");
            paramTypeTable[43] = new XGEffectParam("Drive Low", 0, 127, 0, "", "低音側スピーカーの回転による変調の深さ");
            paramTypeTable[44] = new XGEffectParam("Dry Level", 0, 127, 0, "", "ドライ音のレベル");
            paramTypeTable[45] = new XGEffectParam("Dry LPF Freq", 34, 60, 3, "Hz", "ドライ音にかけるローパスフィルターで高域をカットする周波数");
            paramTypeTable[46] = new XGEffectParam("Dry SndToNoise", 0, 127, 0, "", "ノイズへのドライ信号の混入");
            paramTypeTable[47] = new XGEffectParam("Dry/Wet", 1, 127, 509, "", "ドライ音とエフェクト音のバランス");
            paramTypeTable[48] = new XGEffectParam("Level Offset", 0, 127, 0, "", "エンヴェロープフォロワー出力に足すオフセット");
            paramTypeTable[49] = new XGEffectParam("Threshold Level", 0, 127, 0, "", "エンヴェロープフォロワーが動き出すレベル");
            paramTypeTable[50] = new XGEffectParam("Edge", 0, 127, 0, "", "歪み方のカーブ (sharp(127)は急に歪みだす、mild(0)は序々に歪む) ");
            paramTypeTable[51] = new XGEffectParam("Emphasis", 0, 1, 209, "", "高域の特性を変化");
            paramTypeTable[52] = new XGEffectParam("EQ High Freq", 25, 58, 3, "Hz", "高域をEQで増減させる周波数");
            paramTypeTable[53] = new XGEffectParam("EQ High Gain", 52, 76, 255, "dB", "高域をEQで増減させるゲイン量");
            paramTypeTable[54] = new XGEffectParam("EQ Low Freq", 4, 40, 3, "Hz", "低域をEQで増減させる周波数");
            paramTypeTable[55] = new XGEffectParam("EQ Low Gain", 52, 76, 255, "dB", "低域をEQで増減させるゲイン量");
            paramTypeTable[56] = new XGEffectParam("EQ Mid Freq", 14, 54, 259, "Hz", "中域をEQで増減させる周波数");
            paramTypeTable[57] = new XGEffectParam("EQ Mid Gain", 52, 76, 511, "dB", "中域をEQで増減させるゲイン");
            paramTypeTable[58] = new XGEffectParam("EQ Mid Width", 10, 120, 510, "", "中域をEQで増減させる範囲の幅");
            paramTypeTable[59] = new XGEffectParam("Er/Rev Balance", 1, 127, 252, "", "初期反射音とリバーブ音のレベルバランス");
            paramTypeTable[60] = new XGEffectParam("F/R Depth", 0, 127, 0, "", "前後のパンの深さ(PAN Direction=Lturn/Rturnの時に有効) ");
            paramTypeTable[61] = new XGEffectParam("Feedback Delay", 1, 14860, 254, "ms", "フィードバックディレイの長さ");
            paramTypeTable[62] = new XGEffectParam("Feedback Dly 1", 1, 14860, 254, "ms", "フィードバックディレイ１の長さ");
            paramTypeTable[63] = new XGEffectParam("Feedback Dly 2", 1, 14860, 254, "ms", "フィードバックディレイ２の長さ");
            paramTypeTable[64] = new XGEffectParam("Feedback Level", 1, 127, 255, "", "フィードバックの量");
            paramTypeTable[65] = new XGEffectParam("Feedback Level", 1, 127, 255, "", "イニシャルディレイのフィードバック量");
            paramTypeTable[66] = new XGEffectParam("Feedback Level", 1, 127, 255, "", "フェイザー出力を再び入力へ戻すレベル(マイナスは位相反転) ");
            paramTypeTable[67] = new XGEffectParam("Feedback Level", 1, 127, 255, "", "ディレイ出力を再び入力へ戻すレベル(マイナスは位相反転)");
            paramTypeTable[68] = new XGEffectParam("Feedback Level", 0, 200, 203, "%", "ディレイ出力を再び入力へ戻すレベル(マイナスは位相反転)");
            paramTypeTable[69] = new XGEffectParam("Feedback Level", 1, 127, 255, "", "反射音の繰り返しの設定");
            paramTypeTable[70] = new XGEffectParam("Filter Type", 0, 5, 200, "", "フィルターのタイプ選択");
            paramTypeTable[71] = new XGEffectParam("Filter Type", 0, 5, 210, "", "音色効果のタイプ設定");
            paramTypeTable[72] = new XGEffectParam("Fine 1", 14, 114, 255, "ct", "1系列目の細かいピッチの設定");
            paramTypeTable[73] = new XGEffectParam("Fine 2", 14, 114, 255, "ct", "2系列目の細かいピッチの設定");
            paramTypeTable[74] = new XGEffectParam("Gate Time", 0, 100, 0, "%", "スライスのゲート時間");
            paramTypeTable[75] = new XGEffectParam("Height", 0, 73, 11, "m", "シミュレートする部屋の高さ");
            paramTypeTable[76] = new XGEffectParam("High Adjust", 0, 25, 0, "", "減衰させる中域の上側の周波数の調整");
            paramTypeTable[77] = new XGEffectParam("High Damp", 1, 10, 254, "", "高域の減衰の調整(値が小さいとき高域が速く減衰する) ");
            paramTypeTable[78] = new XGEffectParam("High GainOfst", 1, 127, 255, "", "各コンプレッサータイプに設定されている高域のゲインオフセット");
            paramTypeTable[79] = new XGEffectParam("High Level", 0, 127, 0, "", "高域のレベル");
            paramTypeTable[80] = new XGEffectParam("High Mute", 0, 1, 209, "", "高域のミュートスイッチ");
            paramTypeTable[81] = new XGEffectParam("Horn Spd Fast", 64, 127, 1, "Hz", "スピーカーの回転するスピード（高域）");
            paramTypeTable[82] = new XGEffectParam("Horn Spd Slow", 0, 63, 1, "Hz", "スピーカーの回転するスピード（高域）");
            paramTypeTable[83] = new XGEffectParam("Horn S/F Time", 0, 127, 0, "", "");
            paramTypeTable[84] = new XGEffectParam("HPF Cutoff", 0, 52, 3, "", "ハイパスフィルターで低域をカットする周波数");
            paramTypeTable[85] = new XGEffectParam("Initial Delay", 0, 127, 5, "ms", "ER(GateReverb)が発音するまでのディレイの長さ");
            paramTypeTable[86] = new XGEffectParam("Initial Delay", 0, 63, 5, "ms", "初期反射音までのディレイタイム");
            paramTypeTable[87] = new XGEffectParam("Initial Delay", 0, 127, 7, "ms", "ディレイの長さ");
            paramTypeTable[88] = new XGEffectParam("Initial Delay", 1, 4600, 254, "ms", "初期ディレイの時間");
            paramTypeTable[89] = new XGEffectParam("Input Level", 0, 127, 0, "", "入力信号のレベル");
            paramTypeTable[90] = new XGEffectParam("Input Mode", 0, 1, 220, "", "入力のモノ/ステレオ切り替え");
            paramTypeTable[91] = new XGEffectParam("Input Select", 0, 2, 223, "", "入力の選択");
            paramTypeTable[92] = new XGEffectParam("L->R Delay", 1, 7430, 254, "ms", "左(入力)から右(出力)へのディレイタイム");
            paramTypeTable[93] = new XGEffectParam("L/R Depth", 0, 127, 0, "", "左右のパンの深さ");
            paramTypeTable[94] = new XGEffectParam("L/R Diffusion", 1, 127, 255, "ms", "広がり感を出すだめの左右のディレイ差");
            paramTypeTable[95] = new XGEffectParam("Lag", 1, 127, 255, "ms", "音符で指定されたディレイにずれをつけるディレイの長さ");
            paramTypeTable[96] = new XGEffectParam("Lch Delay", 1, 14860, 254, "ms", "左チャンネルディレイの長さ");
            paramTypeTable[97] = new XGEffectParam("Lch Delay1", 1, 7430, 254, "ms", "左チャンネル1本目のディレイの長さ");
            paramTypeTable[98] = new XGEffectParam("Lch Delay2", 1, 7430, 254, "ms", "左チャンネル2本目のディレイの長さ");
            paramTypeTable[99] = new XGEffectParam("Lch FB Level", 1, 127, 255, "", "左チャンネルフィードバックの量");
            paramTypeTable[100] = new XGEffectParam("Lch Init Delay", 0, 127, 2, "", "左チャンネルディレイの長さ");
            paramTypeTable[101] = new XGEffectParam("LFO Depth", 0, 127, 0, "", "位相変調の深さ");
            paramTypeTable[102] = new XGEffectParam("LFO Depth", 0, 127, 0, "", "ワウフィルターを制御する深さ");
            paramTypeTable[103] = new XGEffectParam("LFO Depth", 0, 127, 0, "", "ディレイ変調の深さ");
            paramTypeTable[104] = new XGEffectParam("LFO Depth", 0, 127, 0, "", "変調の深さ");
            paramTypeTable[105] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "変調の周波数");
            paramTypeTable[106] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "スピーカーの回転する周波数");
            paramTypeTable[107] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "ディレイ変調の周波数");
            paramTypeTable[108] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "オートパンの周波数");
            paramTypeTable[109] = new XGEffectParam("LFO Frequency", 0, 19, 14, "", "変調スピードを音符で指定する");
            paramTypeTable[110] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "ワウフィルターを制御する周波数");
            paramTypeTable[111] = new XGEffectParam("LFO Frequency", 0, 127, 1, "Hz", "位相変調の周波数");
            paramTypeTable[112] = new XGEffectParam("LFO Phase Diff", 4, 124, 219, "deg", "変調波形のL/R位相差(0deg(=64)で位相差なし)");
            paramTypeTable[113] = new XGEffectParam("LFO Phase Reset", 0, 2, 201, "", "LFO の初期位相のリセット方法");
            paramTypeTable[114] = new XGEffectParam("LFO Wave", 0, 28, 0, "", "パニングカーブを変更する");
            paramTypeTable[115] = new XGEffectParam("LFO Wave", 0, 2, 205, "", "ディレイ変調する波形");
            paramTypeTable[116] = new XGEffectParam("LFO Wave", 0, 1, 198, "", "変調波形の選択");
            paramTypeTable[117] = new XGEffectParam("Liveness", 0, 10, 0, "", "ERの減衰､値が小さいほど減衰が速い");
            paramTypeTable[118] = new XGEffectParam("Low Adjust", 0, 26, 0, "", "減衰させる中域の下側の周波数の調整");
            paramTypeTable[119] = new XGEffectParam("Low Gain Offset", 1, 127, 255, "", "各コンプレッサータイプに設定されている低域のゲインオフセット");
            paramTypeTable[120] = new XGEffectParam("Low Level", 0, 127, 0, "", "低域のレベル");
            paramTypeTable[121] = new XGEffectParam("Low Mute", 0, 1, 209, "", "低域のミュートスイッチ");
            paramTypeTable[122] = new XGEffectParam("Low/High", 1, 127, 251, "", "高音側スピーカーと低音側スピーカーの音量バランス");
            paramTypeTable[123] = new XGEffectParam("LPF Cutoff", 34, 60, 3, "", "ローパスフィルターで高域をカットする周波数");
            paramTypeTable[124] = new XGEffectParam("LPF Resonance", 10, 120, 254, "", "入力のローパスフィルターにくせを付ける");
            paramTypeTable[125] = new XGEffectParam("Mic L-R Angle", 0, 60, 217, "", "出力を取り出すマイクのL/Rの角度");
            paramTypeTable[126] = new XGEffectParam("Mid Gain Offset", 1, 127, 255, "", "各コンプレッサータイプに設定されている中域のゲインオフセット");
            paramTypeTable[127] = new XGEffectParam("Mid Level", 0, 127, 0, "", "中域のレベル");
            paramTypeTable[128] = new XGEffectParam("Mid Mute", 0, 1, 209, "", "中域のミュートスイッチ");
            paramTypeTable[129] = new XGEffectParam("Mix Level", 0, 127, 0, "", "ドライ音にミックスするエフェクト音のレベル");
            paramTypeTable[130] = new XGEffectParam("Mod Delay Ofst", 0, 127, 0, "", "");
            paramTypeTable[131] = new XGEffectParam("Mod Depth", 0, 127, 0, "", "");
            paramTypeTable[132] = new XGEffectParam("Mod Mix Balance", 0, 127, 0, "", "変調した成分のミックスレベル");
            paramTypeTable[133] = new XGEffectParam("Mod Phase", 0, 16, 204, "", "変調波形の位相をコントロールする");
            paramTypeTable[134] = new XGEffectParam("Move Speed", 1, 62, 0, "", "Vowelで設定した音に移る時間");
            paramTypeTable[135] = new XGEffectParam("Noise Level", 0, 127, 0, "", "ノイズのレベル");
            paramTypeTable[136] = new XGEffectParam("Noise LPF Freq", 34, 60, 3, "", "ノイズにかけるローパスフィルターで高域をカットする周波数");
            paramTypeTable[137] = new XGEffectParam("Noise LPF Q", 10, 120, 254, "", "ノイズにかけるローパスフィルターのレゾナンス");
            paramTypeTable[138] = new XGEffectParam("Noise Mod Depth", 0, 127, 0, "", "ノイズの変調の深さ");
            paramTypeTable[139] = new XGEffectParam("Noise Mod Speed", 0, 127, 1, "", "ノイズの変調スピード");
            paramTypeTable[140] = new XGEffectParam("Noise Tone", 0, 6, 0, "", "ノイズの音質");
            paramTypeTable[141] = new XGEffectParam("On/Off Sw", 0, 1, 196, "", "アイソレーターのOn/Off スイッチ");
            paramTypeTable[142] = new XGEffectParam("Output Gain", 0, 18, 211, "dB", "出力のゲイン");
            paramTypeTable[143] = new XGEffectParam("Output Level", 0, 127, 0, "", "出力のレベル");
            paramTypeTable[144] = new XGEffectParam("Output Level", 0, 100, 0, "%", "出力のレベル");
            paramTypeTable[145] = new XGEffectParam("Output Level 1", 0, 127, 0, "", "1系列目の出力のレベル");
            paramTypeTable[146] = new XGEffectParam("Output Level 2", 0, 127, 0, "", "2系列目の出力のレベル");
            paramTypeTable[147] = new XGEffectParam("Output Phase", 0, 1, 218, "", "エフェクト音の位相をL/R入れ換える");
            paramTypeTable[148] = new XGEffectParam("Overdrive", 0, 100, 0, "%", "歪み方の度合");
            paramTypeTable[149] = new XGEffectParam("Pan 1", 1, 127, 250, "", "1系列目のPAN");
            paramTypeTable[150] = new XGEffectParam("Pan 2", 1, 127, 250, "", "2系列目のPAN");
            paramTypeTable[151] = new XGEffectParam("Pan Aeg Level", 0, 127, 0, "", "パンをAEG コントロールする最小レベル");
            paramTypeTable[152] = new XGEffectParam("Pan Aeg Type", 0, 4, 197, "", "パンをAEG コントロールするタイプ選択");
            paramTypeTable[153] = new XGEffectParam("Pan Depth", 1, 127, 255, "", "");
            paramTypeTable[154] = new XGEffectParam("PAN Direction", 0, 5, 215, "", "オートパンのタイプ(L<->Rはサイン波、L/Rは矩形波) ");
            paramTypeTable[155] = new XGEffectParam("Pan Type", 0, 9, 197, "", "");
            paramTypeTable[156] = new XGEffectParam("Phase Inverse R", 0, 2, 194, "", "右チャンネルの位相反転");
            paramTypeTable[157] = new XGEffectParam("Phase Shift", 0, 127, 0, "", "位相変調のオフセット値");
            paramTypeTable[158] = new XGEffectParam("Pitch", 40, 88, 255, "", "半音単位のピッチの設定");
            paramTypeTable[159] = new XGEffectParam("PM Depth", 0, 127, 0, "", "ディレイ変調の深さ");
            paramTypeTable[160] = new XGEffectParam("Presence", 0, 20, 0, "", "ギターアンプなどによくみられるパラメータで、高域をコントロールする");
            paramTypeTable[161] = new XGEffectParam("R->L Delay", 1, 7430, 254, "ms", "右(入力)から左(出力)へのディレイタイム");
            paramTypeTable[162] = new XGEffectParam("Ratio", 0, 7, 10, "", "コンプレッサーの圧縮比");
            paramTypeTable[163] = new XGEffectParam("Rch Delay", 1, 14860, 254, "ms", "右チャンネルディレイの長さ");
            paramTypeTable[164] = new XGEffectParam("Rch Delay1", 1, 7430, 254, "ms", "右チャンネル1本目のディレイの長さ");
            paramTypeTable[165] = new XGEffectParam("Rch Delay2", 1, 7430, 254, "ms", "右チャンネル2本目のディレイの長さ");
            paramTypeTable[166] = new XGEffectParam("Rch FB Level", 1, 127, 254, "ms", "右チャンネルフィードバックの量");
            paramTypeTable[167] = new XGEffectParam("Rch Init Delay", 0, 127, 2, "ms", "右チャンネルディレイの長さ");
            paramTypeTable[168] = new XGEffectParam("Release", 52, 67, 12, "ms", "ワウフィルターの中心周波数が元に戻るまでの時間");
            paramTypeTable[169] = new XGEffectParam("Release", 0, 15, 9, "ms", "コンプレッサー効果から開放されるまでの時間");
            paramTypeTable[170] = new XGEffectParam("Release", 0, 15, 9, "ms", "ゲートが閉じるまでの時間");
            paramTypeTable[171] = new XGEffectParam("Release Curve", 0, 127, 0, "", "エンヴェロープフォロワーのリリースカーブを設定");
            paramTypeTable[172] = new XGEffectParam("Release Time", 0, 127, 16, "", "エンヴェロープフォロワーの収束時間");
            paramTypeTable[173] = new XGEffectParam("Resolution", 0, 7, 195, "", "出力波形のビット精度");
            paramTypeTable[174] = new XGEffectParam("Resonance", 10, 120, 254, "", "ワウフィルターのバンド幅");
            paramTypeTable[175] = new XGEffectParam("Rev Delay", 0, 63, 5, "ms", "初期反射音からリバーブ音までのディレイタイム ");
            paramTypeTable[176] = new XGEffectParam("Reverb Time", 0, 69, 4, "s", "リバーブの長さ");
            paramTypeTable[177] = new XGEffectParam("Room Size", 0, 44, 6, "m", "部屋の大きさ(値が大きいほどERが長くなる)");
            paramTypeTable[178] = new XGEffectParam("Rotor Speed", 0, 127, 1, "Hz", "スピーカーの回転する周波数(DUAL ROTORの場合は低域スピーカー)");
            paramTypeTable[179] = new XGEffectParam("Rotor Spd High", 64, 127, 1, "Hz", "スピーカーの回転する周波数(DUAL ROTORの場合は低域スピーカー)");
            paramTypeTable[180] = new XGEffectParam("Rotor Spd Slow", 0, 63, 1, "Hz", "スピーカーの回転する周波数(DUAL ROTORの場合は低域スピーカー)");
            paramTypeTable[181] = new XGEffectParam("Rotor S/F Time", 0, 127, 0, "", "");
            paramTypeTable[182] = new XGEffectParam("SamlFreq Ctrl", 0, 127, 13, "Hz", "サンプリング周波数のコントロール");
            paramTypeTable[183] = new XGEffectParam("Scratch Depth", 0, 127, 0, "", "スクラッチ変調の深さ");
            paramTypeTable[184] = new XGEffectParam("Scratch Speed", 1, 127, 0, "", "スクラッチ変調のスピード");
            paramTypeTable[185] = new XGEffectParam("Sensitivity", 0, 127, 0, "", "入力の変化に対するワウフィルターの変化の感度");
            paramTypeTable[186] = new XGEffectParam("Sensitivity", 0, 127, 0, "", "入力の変化に対する変調の感度");
            paramTypeTable[187] = new XGEffectParam("Speaker", 0, 5, 207, "", "シミュレートスピーカーの種類を選ぶ");
            paramTypeTable[188] = new XGEffectParam("Speed Control", 0, 1, 216, "", "スピード（Slow/Fast）の切り替え");
            paramTypeTable[189] = new XGEffectParam("Stage", 3, 6, 0, "", "フェイズシフターの段数");
            paramTypeTable[190] = new XGEffectParam("Stage", 4, 6, 0, "", "フェイズシフターの段数");
            paramTypeTable[191] = new XGEffectParam("Stage", 4, 12, 0, "", "フェイズシフターの段数");
            paramTypeTable[192] = new XGEffectParam("Threshold", 55, 97, 213, "dB", "ゲートが開き始める入力レベル");
            paramTypeTable[193] = new XGEffectParam("Threshold", 79, 121, 213, "dB", "効果が効き始める入力レベル");
            paramTypeTable[194] = new XGEffectParam("Threshold Ofst", 32, 96, 255, "", "各コンプレッサータイプに設定されているスレッショルドのオフセット");
            paramTypeTable[195] = new XGEffectParam("Type", 0, 11, 202, "", "COMP のタイプ選択");
            paramTypeTable[196] = new XGEffectParam("Type", 0, 5, 222, "", "タイプ選択");
            paramTypeTable[197] = new XGEffectParam("Type", 0, 1, 221, "", "タイプ選択");
            paramTypeTable[198] = new XGEffectParam("Vowel", 0, 4, 212, "", "母音の選択");
            paramTypeTable[199] = new XGEffectParam("Wah Release", 52, 67, 12, "", "ワウフィルターの中心周波数が元に戻るまでの時間");
            paramTypeTable[200] = new XGEffectParam("Wall Vary", 0, 30, 0, "", "シミュレートする部屋の壁の状態(値が大きいほど乱反射する)");
            paramTypeTable[201] = new XGEffectParam("Width", 0, 37, 11, "m", "シミュレートする部屋の幅");
            paramTypeTable[202] = new XGEffectParam("Word Length", 1, 127, 0, "", "音の粗さの設定");

            effectTypeTable.Add(new XGEffect(EffectType.Reverb | EffectType.Chorus, 0x0000, "NO EFFECT", "エフェクトをOFF にします。", new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4000, "THRU", "エフェクトをかけずにバイパスします。", new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0100, "HALL 1", "ホールでの響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 18, 10, 8, 13, 49, 0, 0, 0, 0, 40, 0, 4, 50, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0101, "HALL 2", "ホールでの響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 25, 10, 28, 6, 46, 0, 0, 0, 0, 40, 13, 3, 74, 7, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0106, "HALL M", "ホールでの響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 18, 10, 8, 13, 49, 0, 0, 0, 0, 40, 0, 4, 50, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0107, "HALL L", "ホールでの響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 18, 10, 28, 6, 46, 0, 0, 0, 0, 40, 13, 3, 74, 7, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0200, "ROOM 1", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 5, 10, 16, 4, 49, 0, 0, 0, 0, 40, 5, 3, 64, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0201, "ROOM 2", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 12, 10, 5, 4, 38, 0, 0, 0, 0, 40, 0, 4, 50, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0202, "ROOM 3", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 9, 10, 47, 5, 36, 0, 0, 0, 0, 40, 0, 4, 60, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0205, "ROOM S", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 11, 10, 5, 4, 38, 0, 0, 0, 0, 40, 0, 4, 50, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0206, "ROOM M", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 13, 10, 16, 4, 49, 0, 0, 0, 0, 40, 5, 3, 64, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0207, "ROOM L", "部屋の響きをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 15, 10, 47, 5, 36, 0, 0, 0, 0, 40, 0, 4, 60, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0300, "STAGE 1", "ソロ楽器に適したリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 19, 10, 16, 7, 54, 0, 0, 0, 0, 40, 0, 3, 64, 6, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0301, "STAGE 2", "ソロ楽器に適したリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 11, 10, 16, 7, 51, 0, 0, 0, 0, 40, 2, 2, 64, 6, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0400, "PLATE", "鉄板リバーブをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 25, 10, 6, 8, 49, 0, 0, 0, 0, 40, 2, 3, 64, 5, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x0407, "GM PLATE", "鉄板リバーブをシミュレートしたリバーブです。", new int[] { 176, 35, 86, 84, 123, 0, 0, 0, 0, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 13, 10, 6, 8, 49, 0, 0, 0, 0, 40, 2, 3, 64, 5, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x0500, "DELAY LCR", "L､R､C の3本のディレイ音を発生するエフェクトです。", new int[] { 96, 163, 14, 61, 64, 15, 77, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 3333, 1667, 5000, 5000, 74, 100, 10, 0, 0, 32, 0, 60, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x0600, "DELAY L/R", "L､R 2本のディレイ音を発生するエフェクトです。２本のフィードバックディレイを持っています。", new int[] { 96, 163, 62, 63, 64, 77, 0, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 2500, 3750, 3752, 3750, 87, 10, 0, 0, 0, 32, 0, 60, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x0700, "ECHO", "L､R 2本のディレイとL､R 独立のフィードバックディレイを持っています。", new int[] { 97, 99, 164, 166, 77, 98, 165, 29, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 1700, 80, 1780, 80, 10, 1700, 1780, 0, 0, 40, 0, 60, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x0800, "CROSSDELAY", "２本のディレイのフィードバックをクロスさせたエフェクトです。", new int[] { 92, 161, 64, 91, 77, 0, 0, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 1700, 1750, 111, 1, 10, 0, 0, 0, 0, 32, 0, 60, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x0900, "ER 1", "リバーブの初期反射音のみを取り出したエフェクトです。", new int[] { 196, 177, 35, 85, 64, 84, 123, 0, 0, 47, 117, 31, 77, 0, 0, 0, }, new int[] { 0, 19, 5, 16, 64, 0, 46, 0, 0, 32, 5, 0, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x0901, "ER 2", "リバーブの初期反射音のみを取り出したエフェクトです。", new int[] { 196, 177, 35, 85, 64, 84, 123, 0, 0, 47, 117, 31, 77, 0, 0, 0, }, new int[] { 2, 7, 10, 16, 64, 3, 46, 0, 0, 32, 5, 2, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x0a00, "GATE REV", "ゲートリバーブをシミュレートしたものです。", new int[] { 197, 177, 35, 85, 64, 84, 123, 0, 0, 47, 117, 31, 77, 0, 0, 0, }, new int[] { 0, 15, 6, 2, 64, 0, 44, 0, 0, 32, 4, 3, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x0b00, "REVRS GATE", "ゲートリバーブの逆再生をシミュレートしたエフェクトです。", new int[] { 197, 177, 35, 85, 64, 84, 123, 0, 0, 47, 117, 31, 77, 0, 0, 0, }, new int[] { 1, 19, 8, 3, 64, 0, 47, 0, 0, 32, 6, 3, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x1000, "WHITE ROOM", "若干のイニシャルディレイを持った独特のショートリバーブです。", new int[] { 176, 35, 86, 84, 123, 201, 75, 32, 200, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 9, 5, 11, 0, 46, 30, 50, 70, 7, 40, 34, 4, 64, 7, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x1100, "TUNNEL", "左右に広がった筒状の空間のシミュレートです。", new int[] { 176, 35, 86, 84, 123, 201, 75, 32, 200, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 48, 6, 19, 0, 44, 33, 52, 70, 16, 40, 20, 4, 64, 7, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x1200, "CANYON", "限りなく広がる幻想的な音の世界をイメージしたものです。", new int[] { 176, 35, 86, 84, 123, 201, 75, 32, 200, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 59, 6, 63, 0, 45, 34, 62, 91, 13, 40, 25, 4, 64, 4, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Reverb, 0x1300, "BASEMENT", "若干のイニシャルディレイの後に、独特の響きを持ったリバーブです。", new int[] { 176, 35, 86, 84, 123, 201, 75, 32, 200, 47, 175, 30, 59, 77, 65, 0, }, new int[] { 3, 6, 3, 0, 34, 26, 29, 59, 15, 40, 32, 4, 64, 8, 64, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1400, "KARAOKE 1", "カラオケ用のエコーです。", new int[] { 25, 69, 201, 75, 0, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 63, 97, 0, 48, 0, 0, 0, 0, 0, 64, 2, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1401, "KARAOKE 2", "カラオケ用のエコーです。", new int[] { 25, 69, 201, 75, 0, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 55, 105, 0, 50, 0, 0, 0, 0, 0, 64, 1, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1402, "KARAOKE 3", "カラオケ用のエコーです。", new int[] { 25, 69, 201, 75, 0, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 43, 110, 14, 53, 0, 0, 0, 0, 0, 64, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1500, "T.DELAY", "テンポ追従するディレイです。", new int[] { 23, 64, 77, 94, 95, 0, 0, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 10, 80, 10, 78, 64, 0, 0, 0, 0, 39, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1508, "T.ECHO", "テンポ追従するエコーです。", new int[] { 23, 64, 77, 94, 95, 0, 0, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 11, 92, 10, 78, 64, 0, 0, 0, 0, 40, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x1600, "TE.CRS DLY", "テンポ追従するクロスディレイです。", new int[] { 26, 28, 64, 91, 77, 95, 0, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 8, 8, 102, 1, 10, 64, 0, 0, 0, 34, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4100, "CHORUS 1", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 6, 54, 77, 106, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4101, "CHORUS 2", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 8, 63, 64, 30, 0, 28, 62, 42, 58, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4102, "CHORUS 3", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 4, 44, 64, 110, 0, 28, 64, 46, 66, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4108, "CHORUS 4", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 9, 32, 69, 104, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 1, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4103, "GM CHORUS1", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 9, 10, 64, 109, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4104, "GM CHORUS2", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 26, 34, 67, 105, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4105, "GM CHORUS3", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 9, 34, 69, 105, 0, 28, 64, 46, 66, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4106, "GM CHORUS4", "一般的なコーラスエフェクトです。音を自然に広げます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 26, 29, 75, 102, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4107, "FB CHORUS", "フィードバックのあるコーラスエフェクトです。（GM Level2 対応）", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 6, 43, 107, 111, 0, 28, 64, 46, 64, 64, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4200, "CELESTE 1", "３相のLFOにより、音にうねりと広がりを与えるエフェクトです。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 12, 32, 64, 0, 0, 28, 64, 46, 64, 127, 40, 68, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4201, "CELESTE 2", "３相のLFOにより、音にうねりと広がりを与えるエフェクトです。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 28, 18, 90, 2, 0, 28, 62, 42, 60, 84, 40, 68, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4202, "CELESTE 3", "３相のLFOにより、音にうねりと広がりを与えるエフェクトです。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 4, 63, 44, 2, 0, 28, 64, 46, 68, 127, 40, 68, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4208, "CELESTE 4", "３相のLFOにより、音にうねりと広がりを与えるエフェクトです。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 90, 0, }, new int[] { 8, 29, 64, 0, 0, 28, 64, 51, 66, 127, 40, 68, 10, 0, 1, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4300, "FLANGER 1", "ジェットサウンドを与えます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 112, 0, 0, }, new int[] { 14, 14, 104, 2, 0, 28, 64, 46, 64, 96, 40, 64, 10, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4301, "FLANGER 2", "ジェットサウンドを与えます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 112, 0, 0, }, new int[] { 32, 17, 26, 2, 0, 28, 64, 46, 60, 96, 40, 64, 10, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4308, "FLANGER 3", "ジェットサウンドを与えます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 112, 0, 0, }, new int[] { 4, 109, 109, 2, 0, 28, 64, 46, 64, 127, 40, 64, 10, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4307, "GM FLANGER", "ジェットサウンドを与えます。", new int[] { 107, 103, 67, 21, 0, 54, 55, 52, 53, 47, 56, 57, 58, 112, 0, 0, }, new int[] { 3, 21, 120, 1, 0, 28, 64, 46, 64, 96, 40, 64, 10, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4400, "SYMPHONIC", "CELESTE の変調をより多重化したものです。", new int[] { 107, 103, 21, 0, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 0, 0, }, new int[] { 12, 25, 16, 0, 0, 28, 64, 46, 64, 127, 46, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4500, "ROTARY SP", "回転スピーカーをシミュレートしたものです。AC1( ｱｻｲﾅﾌﾞﾙｺﾝﾄﾛｰﾗｰ1）などで、回転スピードをコントロールできます。", new int[] { 106, 104, 0, 0, 0, 54, 55, 52, 53, 47, 56, 57, 58, 0, 0, 0, }, new int[] { 81, 35, 0, 0, 0, 24, 60, 45, 54, 127, 33, 52, 30, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4501, "DT+RTRY", "DISTORTION とROTARY SPEAKER を直列に接続したものです。", new int[] { 106, 104, 0, 0, 0, 54, 55, 52, 53, 47, 0, 0, 0, 40, 123, 143, }, new int[] { 6, 92, 0, 0, 0, 26, 68, 56, 52, 127, 0, 0, 0, 5, 49, 55 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4502, "OD+RTRY", "OVER DRIVE とROTARY SPEAKER を直列に接続したものです。", new int[] { 106, 104, 0, 0, 0, 54, 55, 52, 53, 47, 0, 0, 0, 40, 123, 143, }, new int[] { 7, 90, 0, 0, 0, 24, 66, 56, 52, 127, 0, 0, 0, 4, 47, 45 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4503, "AMP+RTRY", "AMP SIMULATOR とR OTARY SPEAKER を直列に接続したものです。", new int[] { 106, 104, 0, 0, 0, 54, 55, 52, 53, 47, 0, 0, 0, 40, 123, 143, }, new int[] { 7, 90, 0, 0, 0, 24, 66, 56, 52, 127, 0, 0, 0, 4, 48, 45 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4600, "TREMOLO", "音量を周期的に変化させるエフェクトです。", new int[] { 105, 2, 159, 0, 0, 54, 55, 52, 53, 0, 56, 57, 58, 112, 90, 0, }, new int[] { 83, 56, 0, 0, 0, 28, 64, 46, 64, 127, 40, 64, 10, 64, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4700, "AUTO PAN", "音像を左右、前後に周期的に移動させるエフェクトです。", new int[] { 108, 93, 60, 154, 0, 54, 55, 52, 53, 0, 56, 57, 58, 0, 0, 0, }, new int[] { 76, 80, 32, 5, 0, 28, 64, 46, 64, 127, 40, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4701, "AUTO PAN 2", "PANNING CURVE を選択可能なAUTO PAN です。", new int[] { 108, 93, 60, 154, 114, 54, 55, 52, 53, 0, 56, 57, 58, 0, 90, 0, }, new int[] { 67, 127, 32, 5, 15, 28, 64, 46, 64, 0, 40, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x4800, "PHASER 1", "位相(フェイズ)を周期的に変化させ音にうねりを持たせます。", new int[] { 111, 101, 157, 66, 0, 54, 55, 52, 53, 47, 191, 36, 0, 0, 0, 0, }, new int[] { 8, 111, 74, 104, 0, 28, 64, 46, 64, 64, 6, 1, 64, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4808, "PHASER 2", "位相(フェイズ)を周期的に変化させ音にうねりを持たせます。", new int[] { 111, 101, 157, 66, 0, 54, 55, 52, 53, 47, 189, 0, 112, 0, 0, 0, }, new int[] { 8, 111, 74, 108, 0, 28, 64, 46, 64, 64, 5, 1, 4, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4900, "DISTORTION", "音にエッジの効いた歪みを与えます。NOISE GATE が入っていますので、A/D 入力にも向いています。", new int[] { 40, 54, 55, 123, 143, 0, 56, 57, 58, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 40, 20, 72, 53, 64, 0, 43, 74, 10, 127, 120, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4901, "COMP+DT", "前段にCOMPRESSOR があるため、入力レベルにかかわらず均等に歪ませることができます。", new int[] { 40, 54, 55, 123, 143, 0, 56, 57, 58, 47, 50, 6, 169, 193, 162, 0, }, new int[] { 40, 20, 72, 53, 48, 0, 43, 74, 10, 127, 120, 6, 2, 100, 4, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4908, "STEREO DT", "ステレオタイプのDISTORTION です。", new int[] { 40, 54, 55, 123, 143, 0, 56, 57, 58, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 18, 27, 71, 48, 84, 0, 32, 66, 10, 127, 105, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4a00, "OVERDRIVE", "音にマイルドな歪みを与えます。NOISE GATE が入っていますので、A/D 入力にも向いています。", new int[] { 40, 54, 55, 123, 143, 0, 56, 57, 58, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 29, 24, 68, 45, 55, 0, 41, 72, 10, 127, 104, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4a08, "STEREO OD", "ステレオタイプのOVER DRIVE です。", new int[] { 40, 54, 55, 123, 143, 0, 56, 57, 58, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 10, 24, 69, 46, 105, 0, 41, 66, 10, 127, 104, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4b00, "AMP SIM", "ギターアンプをシミュレートしたものです。NOISE GATE が入っていますので、A/D 入力にも向いています。", new int[] { 40, 3, 123, 143, 0, 0, 0, 0, 0, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 39, 1, 48, 55, 0, 0, 0, 0, 0, 127, 112, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4b01, "AMP SIM 2", "歪み特性の異なった、新しいAMP タイプです。", new int[] { 40, 4, 123, 143, 0, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 50, 3, 48, 70, 0, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4b08, "STEREO AMP", "ステレオタイプのAMP SIMULATOR です。", new int[] { 40, 3, 123, 143, 0, 0, 0, 0, 0, 47, 50, 0, 0, 0, 0, 0, }, new int[] { 16, 2, 46, 119, 0, 0, 0, 0, 0, 127, 106, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4c00, "3-BAND EQ", "LOW､MID､HIGH のイコライジングが可能なMONO EQです。", new int[] { 55, 56, 57, 58, 53, 54, 52, 0, 0, 0, 0, 0, 0, 0, 90, 0, }, new int[] { 70, 34, 60, 10, 70, 28, 46, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4d00, "2-BAND EQ", "LOW､HIGH のイコライジングが可能なSTEREO EQ です。DRUMPART に最適です。", new int[] { 54, 55, 52, 53, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 28, 70, 46, 70, 0, 0, 0, 0, 0, 127, 34, 64, 10, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4e00, "AUTO WAH", "WAH FILTER の中心周波数を周期的に変化させます。AC1 などでPEDAL WAH としても使えます。", new int[] { 110, 102, 19, 174, 0, 54, 55, 52, 53, 47, 40, 0, 0, 0, 0, 0, }, new int[] { 70, 56, 39, 25, 0, 28, 66, 46, 64, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4e01, "A-WAH+DT", "AUTO WAH の出力をDISTORTION により、歪ませたものです。AC1 などでPEDAL WAH としても使えます。", new int[] { 110, 102, 19, 174, 0, 54, 55, 52, 53, 47, 40, 55, 57, 123, 143, 0, }, new int[] { 40, 73, 26, 29, 0, 28, 66, 46, 64, 127, 30, 72, 74, 53, 48, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x4e02, "A-WAH+OD", "AUTO WAH の出力をOVERDRIVE により、歪ませたものです。AC1 などでPEDAL WAHとしても使えます。", new int[] { 110, 102, 19, 174, 0, 54, 55, 52, 53, 47, 40, 55, 57, 123, 143, 0, }, new int[] { 48, 64, 32, 23, 0, 28, 66, 46, 64, 127, 29, 68, 72, 45, 55, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5000, "PITCH CNG1", "入力信号の音程を変えるエフェクトです。", new int[] { 158, 87, 72, 73, 64, 0, 0, 0, 0, 47, 149, 145, 150, 146, 0, 0, }, new int[] { 64, 0, 74, 54, 64, 0, 0, 0, 0, 64, 1, 127, 127, 127, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5001, "PITCH CNG2", "入力信号の音程を変えるエフェクトです。", new int[] { 158, 87, 72, 73, 64, 0, 0, 0, 0, 47, 149, 145, 150, 146, 0, 0, }, new int[] { 65, 50, 67, 61, 87, 0, 0, 0, 0, 32, 1, 127, 127, 127, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5100, "HM ENHNCER", "入力信号に新たな倍音を付加し音をきわだたせるエフェクトです。", new int[] { 84, 41, 129, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 44, 30, 48, 0, 0, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5200, "TOUCH WAH1", "入力のレベルによりWAH FILTER の中心周波数を変えるプログラムです。AC1 などでPEDAL WAH としても使えます。", new int[] { 185, 19, 174, 0, 0, 54, 55, 52, 53, 47, 40, 0, 0, 0, 0, 0, }, new int[] { 36, 0, 30, 0, 0, 28, 66, 46, 64, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5208, "TOUCH WAH2", "入力のレベルによりWAH FILTER の中心周波数を変えるプログラムです。AC1 などでPEDAL WAH としても使えます。", new int[] { 185, 19, 174, 0, 0, 54, 55, 52, 53, 47, 40, 55, 57, 123, 143, 168, }, new int[] { 68, 18, 60, 0, 0, 28, 66, 46, 64, 127, 0, 72, 74, 53, 57, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5201, "T-WAH+DIST", "TOUCH WAH の出力をDISTORTION により、歪ませたものです。AC1 などでPEDAL WAH としても使えます。", new int[] { 185, 19, 174, 0, 0, 54, 55, 52, 53, 47, 40, 0, 0, 0, 0, 0, }, new int[] { 36, 0, 30, 0, 0, 28, 66, 46, 64, 127, 30, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5202, "T-WAH+ODRV", "TOUCH WAH の出力をOVERDRIVE により、歪ませたものです。AC1 などでPEDAL WAH としても使えます。", new int[] { 185, 19, 174, 0, 0, 54, 55, 52, 53, 47, 40, 55, 57, 123, 143, 168, }, new int[] { 45, 18, 28, 0, 0, 28, 66, 46, 64, 127, 29, 68, 72, 45, 55, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5300, "COMPRESSOR", "設定レベル以上の信号が入力されると出力を抑えます。また、音にアタック感を与えることも出来ます。", new int[] { 6, 169, 193, 162, 143, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 6, 2, 100, 4, 96, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5400, "NOISE GATE", "入力信号が設定レベル以下のになると、入力をゲートします。A/D 入力でノイズを抑えたいときに有効です。", new int[] { 7, 170, 192, 143, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 11, 82, 50, 0, 0, 0, 0, 0, 127, 3, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5500, "VOIC CANCL", "CD などのソースのボーカルパートを減衰させることができます。", new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 118, 76, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 8, 25, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5600, "2WAY ROTRY", "回転スピーカーをシミュレートしたものです。AC1( ｱｻｲﾅﾌﾞﾙｺﾝﾄﾛｰﾗｰ1）などで、回転スピードをコントロールできます。", new int[] { 178, 43, 42, 122, 0, 54, 55, 52, 53, 0, 18, 125, 0, 0, 0, 0, }, new int[] { 16, 26, 35, 70, 0, 24, 60, 45, 54, 127, 31, 45, 32, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5601, "DT +2RTRY", "DISTORTION と2WAY ROTARY SPEAKER を直列に接続したものです。", new int[] { 178, 43, 42, 122, 0, 54, 55, 52, 53, 0, 18, 125, 0, 40, 123, 143, }, new int[] { 6, 28, 30, 64, 0, 24, 66, 56, 59, 127, 36, 60, 0, 3, 48, 60 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5602, "OD +2RTRY", "OVER DRIVE と2WAY ROTARY SPEAKER を直列に接続したものです。", new int[] { 178, 43, 42, 122, 0, 54, 55, 52, 53, 0, 18, 125, 0, 40, 123, 143, }, new int[] { 5, 28, 30, 62, 0, 20, 67, 56, 60, 127, 33, 60, 0, 4, 46, 50 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5603, "AMP+2RTRY", "AMP SIMULATOR と2WAY ROTARY SPEAKER を直列に接続したものです。", new int[] { 178, 43, 42, 122, 0, 54, 55, 52, 53, 0, 18, 125, 3, 40, 123, 143, }, new int[] { 8, 27, 29, 64, 0, 17, 66, 58, 52, 127, 33, 60, 3, 3, 48, 52 }));
            effectTypeTable.Add(new XGEffect(EffectType.Chorus, 0x5700, "ENS DETUNE", "音程をわずかにずらした音を付加することによる、うねりのないコーラスエフェクトです。", new int[] { 33, 100, 167, 0, 0, 0, 0, 0, 0, 47, 54, 55, 52, 53, 0, 0, }, new int[] { 54, 0, 0, 0, 0, 0, 0, 0, 0, 64, 28, 64, 46, 64, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5800, "AMBIENCE", "音の定位をぼかして空間的な広がりを得るエフェクトです。", new int[] { 24, 147, 0, 0, 0, 54, 55, 52, 53, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 114, 0, 0, 0, 0, 28, 64, 46, 64, 64, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5d00, "TALK MOD", "入力信号に母音をつけます。", new int[] { 198, 134, 41, 143, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 2, 60, 6, 54, 5, 10, 1, 1, 0, 127, 0, 0, 0, 0, 1, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x5e00, "LO-FI", "入力信号の音質を粗くします。", new int[] { 182, 202, 142, 123, 71, 124, 11, 51, 0, 47, 0, 0, 0, 0, 90, 0, }, new int[] { 2, 60, 6, 54, 5, 10, 1, 1, 0, 127, 0, 0, 0, 0, 1, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x5f00, "DT+DELAY", "DISTORTION とDELAY を直列に接続したものです。", new int[] { 96, 163, 61, 67, 20, 40, 143, 55, 57, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 2500, 3000, 3750, 74, 70, 40, 48, 72, 74, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x5f01, "OD+DELAY", "OVERDRIVE とD ELAY を直列に接続したものです。", new int[] { 96, 163, 61, 67, 20, 40, 143, 55, 57, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 1900, 1400, 2500, 78, 60, 29, 55, 68, 72, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6000, "CMP+DT+DLY", "COMPRESSOR とD ISTORTION とDELAY を直列に接続したものです。", new int[] { 61, 67, 20, 40, 143, 55, 57, 0, 0, 47, 6, 169, 193, 162, 0, 0, }, new int[] { 3000, 72, 66, 40, 48, 72, 74, 0, 0, 127, 6, 2, 100, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6001, "CMP+OD+DLY", "COMPRESSOR とOVERDRIVE とDELAY を直列に接続したものです。", new int[] { 61, 67, 20, 40, 143, 55, 57, 0, 0, 47, 6, 169, 193, 162, 0, 0, }, new int[] { 3000, 72, 66, 29, 55, 68, 72, 0, 0, 127, 6, 2, 100, 4, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6100, "WAH+DT+DLY", "TOUCH WAH とDISTORTION とDELAY を直列に接続したものです。", new int[] { 61, 67, 20, 40, 143, 55, 57, 0, 0, 47, 185, 19, 174, 168, 0, 0, }, new int[] { 1600, 84, 64, 30, 48, 69, 72, 0, 0, 127, 40, 0, 30, 64, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6101, "WAH+OD+DLY", "TOUCH WAH とOVERDRIVE とDELAY を直列に接続したものです。", new int[] { 61, 67, 20, 40, 143, 55, 57, 0, 0, 47, 185, 19, 174, 168, 0, 0, }, new int[] { 1600, 84, 64, 24, 55, 65, 70, 0, 0, 127, 40, 0, 30, 64, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6200, "V DT HARD", "Vintage Tube やFuzz をシミュレートしたDISTORTION(ハードタイプ) です。", new int[] { 148, 34, 187, 160, 144, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 22, 3, 2, 6, 88, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6201, "V DT H+DLY", "Vintage Tube やFuzz をシミュレートしたDISTORTION(ハードタイプ) とDELAY を直列に接続したものです。", new int[] { 148, 34, 187, 160, 144, 96, 163, 61, 64, 47, 20, 0, 0, 0, 0, 0, }, new int[] { 22, 3, 2, 5, 82, 2500, 5000, 5000, 85, 127, 46, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6202, "V DT SOFT", "Vintage Tube やFuzz をシミュレートしたDISTORTION(ソフトタイプ) です。", new int[] { 148, 34, 187, 160, 144, 0, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 13, 3, 2, 6, 98, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6203, "V DT S+DLY", "Vintage Tube やFuzz をシミュレートしたDISTORTION(ソフトタイプ) とDELAY を直列に接続したものです。", new int[] { 148, 34, 187, 160, 144, 96, 163, 61, 64, 47, 20, 0, 0, 0, 0, 0, }, new int[] { 14, 3, 2, 6, 92, 2500, 5000, 5000, 76, 127, 44, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6300, "DUAL ROTR1", "ROTARY SPEAKER を並列に接続したものです。", new int[] { 180, 82, 179, 81, 83, 181, 43, 42, 122, 0, 54, 55, 52, 53, 125, 188, }, new int[] { 15, 18, 89, 91, 54, 22, 20, 22, 52, 0, 14, 72, 34, 61, 60, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6301, "DUAL ROTR2", "ROTARY SPEAKER を並列に接続したものです。", new int[] { 180, 82, 179, 81, 83, 181, 43, 42, 122, 0, 54, 55, 52, 53, 125, 188, }, new int[] { 14, 18, 91, 95, 54, 22, 22, 29, 64, 0, 34, 64, 34, 64, 60, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x6800, "V-FLANGER", "アナログのニュアンスを再現させたFLANGER です。", new int[] { 107, 103, 115, 22, 68, 54, 55, 52, 53, 47, 56, 57, 58, 133, 77, 5, }, new int[] { 5, 45, 0, 17, 184, 28, 64, 46, 64, 127, 46, 64, 10, 16, 9, 5 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6900, "MULTI COMP", "帯域別コンプレッサーのプリセットタイプです。", new int[] { 195, 194, 119, 126, 78, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 9, 64, 64, 64, 64, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6b00, "T.FLANGER", "テンポに同期するFLANGER です。", new int[] { 109, 103, 67, 21, 113, 54, 55, 52, 53, 47, 56, 57, 58, 112, 0, 0, }, new int[] { 17, 10, 12, 2, 0, 28, 64, 46, 64, 96, 40, 64, 10, 64, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6c00, "T.PHASER", "テンポに同期するPHASER です。", new int[] { 109, 101, 157, 66, 113, 54, 55, 52, 53, 47, 191, 0, 112, 0, 0, 0, }, new int[] { 17, 48, 67, 108, 0, 28, 64, 46, 64, 64, 6, 0, 64, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6d00, "DYNA FLT", "入力信号の振幅に応じてCUTOFF が変化するFILTER です。", new int[] { 70, 186, 48, 174, 8, 172, 171, 37, 49, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 1, 110, 0, 66, 19, 40, 110, 0, 0, 96, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6e00, "DYNA FLANG", "入力信号の振幅に応じてDEPTH が変化するFLANGER です。", new int[] { 186, 22, 64, 8, 172, 171, 37, 49, 48, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 122, 1, 6, 63, 65, 100, 0, 0, 0, 96, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x6f00, "DYNA PHASE", "入力信号の振幅に応じてDEPTH が変化するPHASER です。", new int[] { 186, 48, 64, 8, 172, 171, 37, 49, 0, 47, 190, 0, 54, 55, 52, 53, }, new int[] { 98, 0, 120, 30, 52, 25, 0, 0, 0, 32, 6, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7000, "DYNA RING", "入力信号の振幅に応じてDEPTH が変化するRINGMOD. です。", new int[] { 186, 84, 123, 8, 172, 171, 37, 49, 48, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 80, 0, 60, 12, 58, 70, 0, 0, 10, 64, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7100, "RING MOD", "金属的な音を加味するエフェクトです。", new int[] { 12, 13, 116, 104, 105, 84, 123, 0, 0, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 98, 0, 0, 0, 64, 0, 60, 0, 0, 127, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7200, "SLICE", "入力信号をぶつ切りにするエフェクトです。", new int[] { 39, 74, 152, 151, 153, 38, 155, 40, 1, 47, 0, 0, 54, 55, 52, 53, }, new int[] { 5, 30, 2, 127, 64, 1, 0, 0, 0, 127, 0, 0, 28, 64, 46, 64 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7300, "ISOLATOR", "帯域別に音量をコントロールするエフェクトです。", new int[] { 141, 120, 127, 79, 121, 128, 80, 0, 0, 0, 0, 0, 0, 0, 0, 0, }, new int[] { 1, 64, 64, 64, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7400, "LOW RESO", "RESOLUTION をコントロールし、ローファイ感を出すエフェクトです。", new int[] { 131, 130, 64, 173, 132, 156, 0, 0, 0, 47, 0, 0, 0, 0, 0, 0, }, new int[] { 3, 1, 66, 0, 64, 0, 0, 0, 0, 127, 0, 0, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.Normal, 0x7500, "D.TURNTBL", "TURNTABLE 的なノイズを付加するエフェクトです。", new int[] { 16, 17, 140, 139, 138, 46, 136, 137, 135, 0, 44, 45, 0, 0, 0, 0, }, new int[] { 1, 20, 2, 15, 72, 4, 52, 15, 20, 0, 127, 49, 0, 0, 0, 0 }));
            effectTypeTable.Add(new XGEffect(EffectType.UseDataMSB, 0x7600, "D.SCRATCH", "SCRATCH 効果を付加するエフェクトです。", new int[] { 89, 88, 184, 183, 10, 9, 56, 57, 58, 47, 84, 0, 0, 0, 0, 0, }, new int[] { 80, 1800, 9, 90, 16, 127, 46, 64, 20, 64, 12, 0, 0, 0, 0, 0 }));

            AllEffects = new ReadOnlyCollection<XGEffect>(effectTypeTable);

        }

        static XGEffect()
        {
            InitializeAllParams();
        }
    }
}