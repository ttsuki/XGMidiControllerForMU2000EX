namespace Tsukikage.XGTGCtrl2.Forms
{
    partial class PartParameterGrid
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
            this.buttonToMML = new System.Windows.Forms.Button();
            this.buttonSendAll = new System.Windows.Forms.Button();
            this.buttonDumpAll = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonToMML
            // 
            this.buttonToMML.Location = new System.Drawing.Point(165, 3);
            this.buttonToMML.Name = "buttonToMML";
            this.buttonToMML.Size = new System.Drawing.Size(75, 23);
            this.buttonToMML.TabIndex = 5;
            this.buttonToMML.Text = "ToMML";
            this.buttonToMML.UseVisualStyleBackColor = true;
            this.buttonToMML.Click += new System.EventHandler(this.buttonToMML_Click);
            // 
            // buttonSendAll
            // 
            this.buttonSendAll.Location = new System.Drawing.Point(84, 3);
            this.buttonSendAll.Name = "buttonSendAll";
            this.buttonSendAll.Size = new System.Drawing.Size(75, 23);
            this.buttonSendAll.TabIndex = 4;
            this.buttonSendAll.Text = "SendAll";
            this.buttonSendAll.UseVisualStyleBackColor = true;
            this.buttonSendAll.Click += new System.EventHandler(this.buttonSendAll_Click);
            // 
            // buttonDumpAll
            // 
            this.buttonDumpAll.Location = new System.Drawing.Point(3, 3);
            this.buttonDumpAll.Name = "buttonDumpAll";
            this.buttonDumpAll.Size = new System.Drawing.Size(75, 23);
            this.buttonDumpAll.TabIndex = 3;
            this.buttonDumpAll.Text = "DumpAll";
            this.buttonDumpAll.UseVisualStyleBackColor = true;
            this.buttonDumpAll.Click += new System.EventHandler(this.buttonDumpAll_Click);
            // 
            // PartParameterGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonToMML);
            this.Controls.Add(this.buttonSendAll);
            this.Controls.Add(this.buttonDumpAll);
            this.Name = "PartParameterGrid";
            this.Size = new System.Drawing.Size(606, 467);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonToMML;
        private System.Windows.Forms.Button buttonSendAll;
        private System.Windows.Forms.Button buttonDumpAll;
    }
}
