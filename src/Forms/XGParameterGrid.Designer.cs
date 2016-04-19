namespace Tsukikage.XGTGCtrl2.Forms
{
    partial class XGParameterGrid
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
        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // XGPGrid
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "XGPGrid";
            this.Size = new System.Drawing.Size(978, 505);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PartParameter_Paint);
            this.DoubleClick += new System.EventHandler(this.XGPGrid_DoubleClick);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PartParameter_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PartParameter_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PartParameter_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.Timer timer1;

    }
}
