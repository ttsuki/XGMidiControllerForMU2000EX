namespace Tsukikage.XGTGCtrl2.Forms
{
    partial class DrumParameterGrid
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

        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonDumpAll = new System.Windows.Forms.Button();
            this.buttonSendAll = new System.Windows.Forms.Button();
            this.buttonToMML = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.labelNotes = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.labelPart = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonDumpAll
            // 
            this.buttonDumpAll.Location = new System.Drawing.Point(3, 3);
            this.buttonDumpAll.Name = "buttonDumpAll";
            this.buttonDumpAll.Size = new System.Drawing.Size(75, 23);
            this.buttonDumpAll.TabIndex = 0;
            this.buttonDumpAll.Text = "DumpAll";
            this.buttonDumpAll.UseVisualStyleBackColor = true;
            this.buttonDumpAll.Click += new System.EventHandler(this.buttonDumpAll_Click);
            // 
            // buttonSendAll
            // 
            this.buttonSendAll.Location = new System.Drawing.Point(84, 3);
            this.buttonSendAll.Name = "buttonSendAll";
            this.buttonSendAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSendAll.TabIndex = 1;
            this.buttonSendAll.Text = "SendAll";
            this.buttonSendAll.UseVisualStyleBackColor = true;
            this.buttonSendAll.Click += new System.EventHandler(this.buttonSendAll_Click);
            // 
            // buttonToMML
            // 
            this.buttonToMML.Location = new System.Drawing.Point(165, 3);
            this.buttonToMML.Name = "buttonToMML";
            this.buttonToMML.Size = new System.Drawing.Size(75, 23);
            this.buttonToMML.TabIndex = 2;
            this.buttonToMML.Text = "ToMML";
            this.buttonToMML.UseVisualStyleBackColor = true;
            this.buttonToMML.Click += new System.EventHandler(this.buttonToMML_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(363, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(218, 19);
            this.textBox1.TabIndex = 3;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // labelNotes
            // 
            this.labelNotes.AutoSize = true;
            this.labelNotes.Location = new System.Drawing.Point(320, 8);
            this.labelNotes.Name = "labelNotes";
            this.labelNotes.Size = new System.Drawing.Size(37, 12);
            this.labelNotes.TabIndex = 4;
            this.labelNotes.Text = "Notes:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(275, 6);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(39, 19);
            this.numericUpDown1.TabIndex = 5;
            this.numericUpDown1.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.ValueChanged += new System.EventHandler(this.numericUpDown1_ValueChanged);
            // 
            // labelPart
            // 
            this.labelPart.AutoSize = true;
            this.labelPart.Location = new System.Drawing.Point(246, 8);
            this.labelPart.Name = "labelPart";
            this.labelPart.Size = new System.Drawing.Size(23, 12);
            this.labelPart.TabIndex = 4;
            this.labelPart.Text = "CH:";
            // 
            // DrumParameterGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.labelPart);
            this.Controls.Add(this.labelNotes);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.buttonToMML);
            this.Controls.Add(this.buttonSendAll);
            this.Controls.Add(this.buttonDumpAll);
            this.Name = "DrumParameterGrid";
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonDumpAll;
        private System.Windows.Forms.Button buttonSendAll;
        private System.Windows.Forms.Button buttonToMML;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label labelNotes;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelPart;


    }
}
