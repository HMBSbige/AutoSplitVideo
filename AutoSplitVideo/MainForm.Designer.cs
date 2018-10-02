namespace AutoSplitVideo
{
	partial class MainForm
	{
		/// <summary>
		/// 必需的设计器变量。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清理所有正在使用的资源。
		/// </summary>
		/// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows 窗体设计器生成的代码

		/// <summary>
		/// 设计器支持所需的方法 - 不要修改
		/// 使用代码编辑器修改此方法的内容。
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.button1 = new System.Windows.Forms.Button();
			this.OutputVideoPath = new System.Windows.Forms.TextBox();
			this.InputVideoPath = new System.Windows.Forms.TextBox();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.infoTextBox = new System.Windows.Forms.TextBox();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.AllowDrop = true;
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(800, 450);
			this.tabControl1.TabIndex = 0;
			this.tabControl1.DragOver += new System.Windows.Forms.DragEventHandler(this.tabControl1_DragOver);
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label3);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.progressBar);
			this.tabPage1.Controls.Add(this.numericUpDown2);
			this.tabPage1.Controls.Add(this.numericUpDown1);
			this.tabPage1.Controls.Add(this.button1);
			this.tabPage1.Controls.Add(this.OutputVideoPath);
			this.tabPage1.Controls.Add(this.InputVideoPath);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(792, 424);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "自动分段";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("宋体", 12F);
			this.label3.Location = new System.Drawing.Point(365, 71);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(40, 16);
			this.label3.TabIndex = 8;
			this.label3.Text = "分钟";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("宋体", 12F);
			this.label2.Location = new System.Drawing.Point(201, 71);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(104, 16);
			this.label2.TabIndex = 7;
			this.label2.Text = "GB，每段截取";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("宋体", 12F);
			this.label1.Location = new System.Drawing.Point(6, 71);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "如果视频大于";
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.progressBar.Location = new System.Drawing.Point(8, 108);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(778, 23);
			this.progressBar.TabIndex = 5;
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.Location = new System.Drawing.Point(311, 70);
			this.numericUpDown2.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(48, 21);
			this.numericUpDown2.TabIndex = 4;
			this.numericUpDown2.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.DecimalPlaces = 3;
			this.numericUpDown1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.numericUpDown1.Location = new System.Drawing.Point(116, 70);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(79, 21);
			this.numericUpDown1.TabIndex = 3;
			this.numericUpDown1.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(711, 70);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "截取";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.Button1_Click);
			// 
			// OutputVideoPath
			// 
			this.OutputVideoPath.AllowDrop = true;
			this.OutputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputVideoPath.Location = new System.Drawing.Point(8, 33);
			this.OutputVideoPath.Name = "OutputVideoPath";
			this.OutputVideoPath.Size = new System.Drawing.Size(778, 21);
			this.OutputVideoPath.TabIndex = 1;
			this.OutputVideoPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.InputVideoPath_DragDrop);
			this.OutputVideoPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// InputVideoPath
			// 
			this.InputVideoPath.AllowDrop = true;
			this.InputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InputVideoPath.Location = new System.Drawing.Point(8, 6);
			this.InputVideoPath.Name = "InputVideoPath";
			this.InputVideoPath.Size = new System.Drawing.Size(778, 21);
			this.InputVideoPath.TabIndex = 0;
			this.InputVideoPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.InputVideoPath_DragDrop);
			this.InputVideoPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.infoTextBox);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(792, 424);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Info";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// infoTextBox
			// 
			this.infoTextBox.AllowDrop = true;
			this.infoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoTextBox.Location = new System.Drawing.Point(3, 3);
			this.infoTextBox.Multiline = true;
			this.infoTextBox.Name = "infoTextBox";
			this.infoTextBox.ReadOnly = true;
			this.infoTextBox.Size = new System.Drawing.Size(786, 418);
			this.infoTextBox.TabIndex = 0;
			this.infoTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.infoTextBox_DragDrop);
			this.infoTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.tabControl1);
			this.Name = "MainForm";
			this.Text = "AutoSplitVideo";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TextBox infoTextBox;
		private System.Windows.Forms.TextBox OutputVideoPath;
		private System.Windows.Forms.TextBox InputVideoPath;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
	}
}

