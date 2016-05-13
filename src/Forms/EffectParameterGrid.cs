using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Tsukikage.XGTGCtrl2.XG;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public partial class EffectParameterGrid : XGParameterGrid
    {
        XGEffectBlockType[] effectBlocks = new XGEffectBlockType[]
        {
          XGEffectBlockType.Reverb, XGEffectBlockType.Chorus, XGEffectBlockType.Variation,
          XGEffectBlockType.Insertion1, XGEffectBlockType.Insertion2, XGEffectBlockType.Insertion3, XGEffectBlockType.Insertion4
        };

        [System.ComponentModel.Browsable(true)]
        public int BlockNumber { get; set; }

        public EffectParameterGrid()
        {
            InitializeComponent();
        }

        ContextMenuStrip[] effectSelectors;
        List<XGEffectParams> Effects = new List<XGEffectParams>();
        List<XGMidiParameter> Parameters = new List<XGMidiParameter>();

        Color valueColor = Color.Red;
        Color labelColor = Color.Maroon;

        protected override void OnSetDevice()
        {
            if (BlockNumber == 0) {
                effectBlocks = new XGEffectBlockType[] { XGEffectBlockType.Reverb, XGEffectBlockType.Chorus, XGEffectBlockType.Variation, };
            }
            if (BlockNumber == 1)
            {
                effectBlocks = new XGEffectBlockType[] { XGEffectBlockType.Insertion1, XGEffectBlockType.Insertion2, XGEffectBlockType.Insertion3, XGEffectBlockType.Insertion4 };
            }
            base.OnSetDevice();
            effectSelectors = Array.ConvertAll(new XGEffectBlockType[] { XGEffectBlockType.Reverb, XGEffectBlockType.Chorus, XGEffectBlockType.Variation,
            XGEffectBlockType.Insertion1, XGEffectBlockType.Insertion2, XGEffectBlockType.Insertion3, XGEffectBlockType.Insertion4 }, CreateMenu);
            ReCreateScreen();
        }

        XGPControlCell AddControl(int x, int y, string label, XGMidiParameter param)
        {
            Parameters.Add(param);
            AddLabelCell(label, x, y, 4, labelColor);
            return AddControlCell(param, x + 4, y, 3, valueColor);
        }

        XGPControlCell AddControl(int x, int y, string label, string param)
        {
            AddLabelCell(label, x, y, 4, labelColor);
            return AddLabelCell(param, x + 4, y, 3, labelColor);
        }

        XGPControlCell AddDisabledControl(int x, int y, string label, string param)
        {
            XGPControlCell ctrl = AddLabelCell(label, x, y, 4, labelColor); ctrl.Enabled = false;
            ctrl = AddLabelCell(param, x + 4, y, 3, labelColor); ctrl.Enabled = false;
            return ctrl;
        }

        void DecideEffect(XGEffectBlockType type, XGEffect efct)
        {
            new XGEffectParams(Device, type).SetEffect(efct);
            ReCreateScreen();
        }

        void PopMenu(XGEffectBlockType type)
        {
            effectSelectors[(int)type].Show(MousePosition);
        }

        ContextMenuStrip CreateMenu(XGEffectBlockType type)
        {
            ContextMenuStrip strip = new ContextMenuStrip();
            EffectSelectorMenu container = new EffectSelectorMenu();
            strip.Items.Add(container);

            for (int i = 0; i < XGEffect.AllEffects.Count; i++)
            {
                XGEffect efct = XGEffect.AllEffects[i];
                if (type >= XGEffectBlockType.Variation
                    || (type == XGEffectBlockType.Reverb && efct.SelectableForReverb)
                    || (type == XGEffectBlockType.Chorus && efct.SelectableForChorus))
                {
                    container.AddButton(XGEffect.AllEffects[i].Name,
                        XGEffect.AllEffects[i].Description,
                        () => { strip.Close(); DecideEffect(type, efct); },
                        efct.SelectableForReverb & efct.SelectableForChorus ? Color.DarkGray :
                        efct.SelectableForReverb ? Color.Maroon :
                        efct.SelectableForChorus ? Color.Teal :
                        Color.Navy
                        );
                }
            }
            strip.LayoutStyle = ToolStripLayoutStyle.Flow;

            return strip;
        }

        void CreateEffectRack(int x, XGEffectParams efctParam)
        {
            XGEffectBlockType efctBlock = efctParam.BlockType;
            efctParam.ReLink();
            XGEffect efctDef = efctParam.Effect ?? XGEffect.AllEffects[0];
            switch (efctBlock)
            {
                case XGEffectBlockType.Reverb: valueColor = Color.Red; labelColor = Color.Maroon; break;
                case XGEffectBlockType.Chorus: valueColor = Color.Cyan; labelColor = Color.Teal; break;
                case XGEffectBlockType.Variation: valueColor = Color.Blue; labelColor = Color.Navy; break;
                case XGEffectBlockType.Insertion1: valueColor = Color.Lime; labelColor = Color.Green; break;
                case XGEffectBlockType.Insertion2: valueColor = Color.Yellow; labelColor = Color.FromArgb(0xFF, 0x8B, 0x8B, 0x00); break;
                case XGEffectBlockType.Insertion3: valueColor = Color.Magenta; labelColor = Color.Purple; break;
                case XGEffectBlockType.Insertion4: valueColor = Color.LightGray; labelColor = Color.Gray; break;
            }
            Parameters.Add(efctParam.EffectType);

            int y = 0;

            AddLabelCell("---- " + efctBlock.ToString(), x, 0, 5, labelColor);
            y++;
            XGPControlCell selectedEffectCell = AddTriggerCell(efctDef.Name, x, y, 7, valueColor, () => { });
            selectedEffectCell.Offset = (v) => PopMenu(efctBlock);
            selectedEffectCell.GetDescriptionFunc = () => "Click to Change Effect " + efctBlock;

            y++;
            AddTriggerCell("[ReSend]", x, y, 2, valueColor, () => efctParam.ReSendAll()).GetDescriptionFunc = () => "DblClick to ReSend All " + efctParam.Name + "s.";
            AddTriggerCell("[Dump]", x + 2, y, 2, valueColor, () => { efctParam.RequestDump(); DoFunc(() => Device.AllDumpRequestHasDone, ReCreateScreen); }).GetDescriptionFunc = () => "DblClick to Request Dump All " + efctParam.Name + "s. ";
            AddTriggerCell(" ", x + 4, y, 3, labelColor, () => { });

            y++;

            bool insertionConnect = efctBlock >= XGEffectBlockType.Insertion1
                || efctBlock == XGEffectBlockType.Variation && efctParam.VariationConnect.Value == 0;

            switch (efctBlock)
            {
                case XGEffectBlockType.Reverb:
                    AddDisabledControl(x, y++, "Connect", "System");
                    AddControl(x, y++, "Return", efctParam.Return);
                    AddControl(x, y++, "Pan", efctParam.Pan);
                        AddDisabledControl(x, y++, "---", "---");
                        AddDisabledControl(x, y++, "---", "---");
                    //AddDisabledControl(x, y++, "Part", "---");
                    break;
                case XGEffectBlockType.Chorus:
                    AddDisabledControl(x, y++, "Connect", "System");
                    AddControl(x, y++, "Return", efctParam.Return);
                    AddControl(x, y++, "Pan", efctParam.Pan);
                    AddControl(x, y++, "Reverb", efctParam.SendToReverb);
                        AddDisabledControl(x, y++, "---", "---");
                    break;
                case XGEffectBlockType.Variation:
                    AddControl(x, y++, "Connect", efctParam.VariationConnect).Offset += v => { efctParam.ReLink(); ReCreateScreen(); };
                    if (insertionConnect)
                    {
                        AddControl(x, y++, "Part", efctParam.PartNumber);
                        AddDisabledControl(x, y++, "---", "---");
                        AddDisabledControl(x, y++, "---", "---");
                        AddDisabledControl(x, y++, "---", "---");
                    }
                    else
                    {
                        //AddDisabledControl(x, y++, "Part", "---");
                        AddControl(x, y++, "Return", efctParam.Return);
                        AddControl(x, y++, "Pan", efctParam.Pan);
                        AddControl(x, y++, "Reverb", efctParam.SendToReverb);
                        AddControl(x, y++, "Chorus", efctParam.SendToChorus);
                    }
                    break;
                case XGEffectBlockType.Insertion1:
                case XGEffectBlockType.Insertion2:
                case XGEffectBlockType.Insertion3:
                case XGEffectBlockType.Insertion4:
                    AddDisabledControl(x, y++, "Connect", "Insertion");
                    //AddDisabledControl(x, y++, "Return", "---");
                    //AddDisabledControl(x, y++, "Pan", "---");
                    //AddDisabledControl(x, y++, "Reverb", "---");
                    //AddDisabledControl(x, y++, "Chorus", "---");
                    AddControl(x, y++, "Part", efctParam.PartNumber);
                        AddDisabledControl(x, y++, "---", "---");
                        AddDisabledControl(x, y++, "---", "---");
                        AddDisabledControl(x, y++, "---", "---");
                    break;
            }

            AddControl(x, y++, "---- Params ----", "---------");

            for (int i = 0; i < 16; i++)
            {
                XGEffect.XGEffectParam p = efctDef.ParameterTypes[i];

                // If "ENS DETUNE" is selected in Chorus Block, some parameters are not available.
                bool notAvailable = (efctBlock == XGEffectBlockType.Chorus && efctDef.EffectValue == 0x5700 && (i >= 10));

                if (p != null)
                {
                    if ((p.InsertionOnly && !insertionConnect) || (notAvailable))
                    {
                        AddDisabledControl(x, y++, p.Name, "---");
                    }
                    else
                    {
                        AddControl(x, y++, p.Name, efctParam.EffectParameters[i]);
                    }
                }
                else
                {
                    AddDisabledControl(x, y++, "---", "---");
                }
            }
        }

        public void ReCreateScreen()
        {
            Effects.Clear();
            XGControls.Clear();
            Parameters.Clear();
            for (int i = 0; i < effectBlocks.Length; i++)
            {
                Effects.Add(new XGEffectParams(Device, effectBlocks[i]));
                CreateEffectRack(7 * i, Effects[i]);
            }
            Invalidate();
        }

        private void buttonDumpAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].RequestDump();
            }
            DoFunc(() => Device.AllDumpRequestHasDone, ReCreateScreen);
        }

        private void buttonSendAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                Effects[i].ReSendAll();
            }
        }

        private void buttonToMML_Click(object sender, EventArgs e)
        {
            StringBuilder mml = new StringBuilder();
            mml.Append(
            @"Function XGXcl1(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val & $7F), $F7; }
Function XGXclV(Int Address, Int _val) { SysEx = $F0, $43,$10,$4C, (Address / 65536 & $7F), (Address / 256 & $7F), (Address & $7F), (_val / 128 & $7F), (_val & $7F), $F7; }
");

            Parameters.ForEach(p =>
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
    }
}
