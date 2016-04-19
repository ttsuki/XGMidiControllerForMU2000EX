using System.Drawing;
using System.Windows.Forms;

namespace Tsukikage.XGTGCtrl2.Forms
{
    public class EffectSelectorMenu : ToolStripControlHost
    {
        Label label;
        FlowLayoutPanel Panel;

        public EffectSelectorMenu()
            : base(new FlowLayoutPanel())
        {
            Panel = new FlowLayoutPanel();
            Panel.FlowDirection = FlowDirection.TopDown;
            Panel.Size = new System.Drawing.Size(600, 480);

            label = new Label();
            label.Font = f;
            label.Size = new Size(600, 48);
            
            FlowLayoutPanel parent = Control as FlowLayoutPanel;
            parent.BackColor = Color.Transparent;
            parent.FlowDirection = FlowDirection.TopDown;
            parent.Controls.Add(Panel);
            parent.Controls.Add(label);
            parent.Size = new System.Drawing.Size(600, 528);
            this.Size = parent.Size;
        }

        //FlowLayoutPanel Panel { get { return Control as FlowLayoutPanel; } }
        Font f = new Font(Config.FontFace, Config.FontSize);
        public void AddButton(string text, string desc, Action click, Color color)
        {
            Button b = new Button();
            b.Font = f;
            b.Text = text;
            b.Width = 100;
            b.Height = 24;
            b.Margin = new System.Windows.Forms.Padding(0);
            //b.BackColor = SystemColors.Control;
            b.ForeColor = color;
            b.TextAlign = ContentAlignment.MiddleLeft;
            Panel.Controls.Add(b);
            b.Click += (s, e) =>
            {
                if (click != null) { click(); }
            };
            b.MouseEnter += (s, e) =>
            {
                label.Text = desc;
                label.ForeColor = color;
                if ((Application.VisualStyleState & System.Windows.Forms.VisualStyles.VisualStyleState.ClientAreaEnabled) == 0)
                {
                    b.BackColor = color;
                    b.ForeColor = Color.White;
                }
            };
            b.MouseLeave += (s, e) =>
            {
                if ((Application.VisualStyleState & System.Windows.Forms.VisualStyles.VisualStyleState.ClientAreaEnabled) == 0)
                {
                    b.BackColor = SystemColors.Control;
                    b.ForeColor = color;
                }
            };
        }

        protected override void OnSubscribeControlEvents(Control control)
        {
            base.OnSubscribeControlEvents(control);
        }
    }
}
