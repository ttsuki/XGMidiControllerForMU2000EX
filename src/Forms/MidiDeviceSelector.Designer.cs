namespace Tsukikage.XGTGCtrl2.Forms
{
    partial class MidiDeviceSelector
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        class PictureBoxX : System.Windows.Forms.PictureBox
        {
            protected override void OnPaint(System.Windows.Forms.PaintEventArgs pe)
            {
                pe.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                pe.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

                base.OnPaint(pe);
            }
        }

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboBoxMidiOutSelect = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxMidiInSelect = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.xgpGrid1 = new Tsukikage.XGTGCtrl2.Forms.XGParameterGrid();
            this.pictureBox1 = new Tsukikage.XGTGCtrl2.Forms.MidiDeviceSelector.PictureBoxX();
            this.buttonOpenConsole = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxMidiOutSelect
            // 
            this.comboBoxMidiOutSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMidiOutSelect.FormattingEnabled = true;
            this.comboBoxMidiOutSelect.Location = new System.Drawing.Point(30, 29);
            this.comboBoxMidiOutSelect.Name = "comboBoxMidiOutSelect";
            this.comboBoxMidiOutSelect.Size = new System.Drawing.Size(346, 20);
            this.comboBoxMidiOutSelect.TabIndex = 9;
            this.comboBoxMidiOutSelect.SelectedIndexChanged += new System.EventHandler(this.comboBoxMidiOutSelect_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(23, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "Out";
            // 
            // comboBoxMidiInSelect
            // 
            this.comboBoxMidiInSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMidiInSelect.FormattingEnabled = true;
            this.comboBoxMidiInSelect.Location = new System.Drawing.Point(30, 3);
            this.comboBoxMidiInSelect.Name = "comboBoxMidiInSelect";
            this.comboBoxMidiInSelect.Size = new System.Drawing.Size(346, 20);
            this.comboBoxMidiInSelect.TabIndex = 7;
            this.comboBoxMidiInSelect.SelectedIndexChanged += new System.EventHandler(this.comboBoxMidiInSelect_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(14, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "In";
            // 
            // xgpGrid1
            // 
            this.xgpGrid1.Location = new System.Drawing.Point(102, 52);
            this.xgpGrid1.Name = "xgpGrid1";
            this.xgpGrid1.Size = new System.Drawing.Size(274, 96);
            this.xgpGrid1.TabIndex = 11;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Tsukikage.XGTGCtrl2.Properties.Resources.icon1;
            this.pictureBox1.Location = new System.Drawing.Point(5, 55);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(96, 96);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // buttonOpenConsole
            // 
            this.buttonOpenConsole.Location = new System.Drawing.Point(5, 157);
            this.buttonOpenConsole.Name = "buttonOpenConsole";
            this.buttonOpenConsole.Size = new System.Drawing.Size(96, 23);
            this.buttonOpenConsole.TabIndex = 13;
            this.buttonOpenConsole.Text = "MIDI Log";
            this.buttonOpenConsole.UseVisualStyleBackColor = true;
            this.buttonOpenConsole.Click += new System.EventHandler(this.buttonOpenConsole_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(5, 181);
            this.progressBar1.MarqueeAnimationSpeed = 1;
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(96, 10);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar1.TabIndex = 14;
            // 
            // MidiDeviceSelector
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.buttonOpenConsole);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.xgpGrid1);
            this.Controls.Add(this.comboBoxMidiOutSelect);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comboBoxMidiInSelect);
            this.Controls.Add(this.label1);
            this.Name = "MidiDeviceSelector";
            this.Size = new System.Drawing.Size(626, 207);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBoxMidiOutSelect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxMidiInSelect;
        private System.Windows.Forms.Label label1;
        private XGParameterGrid xgpGrid1;
        private MidiDeviceSelector.PictureBoxX pictureBox1;
        private System.Windows.Forms.Button buttonOpenConsole;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Timer timer1;
    }
}
