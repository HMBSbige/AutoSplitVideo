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
			this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
			this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.progressBar = new MaterialSkin.Controls.MaterialProgressBar();
			this.materialLabel3 = new MaterialSkin.Controls.MaterialLabel();
			this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.OutputVideoPath = new MaterialSkin.Controls.MaterialSingleLineTextField();
			this.InputVideoPath = new MaterialSkin.Controls.MaterialSingleLineTextField();
			this.materialRaisedButton1 = new MaterialSkin.Controls.MaterialRaisedButton();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.infoTextBox = new System.Windows.Forms.TextBox();
			this.materialTabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// materialTabSelector1
			// 
			this.materialTabSelector1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
			this.materialTabSelector1.Depth = 0;
			this.materialTabSelector1.Location = new System.Drawing.Point(0, 64);
			this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialTabSelector1.Name = "materialTabSelector1";
			this.materialTabSelector1.Size = new System.Drawing.Size(800, 40);
			this.materialTabSelector1.TabIndex = 0;
			this.materialTabSelector1.Text = "materialTabSelector1";
			// 
			// materialTabControl1
			// 
			this.materialTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.materialTabControl1.Controls.Add(this.tabPage1);
			this.materialTabControl1.Controls.Add(this.tabPage2);
			this.materialTabControl1.Depth = 0;
			this.materialTabControl1.Location = new System.Drawing.Point(10, 108);
			this.materialTabControl1.Margin = new System.Windows.Forms.Padding(1);
			this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialTabControl1.Name = "materialTabControl1";
			this.materialTabControl1.SelectedIndex = 0;
			this.materialTabControl1.Size = new System.Drawing.Size(780, 332);
			this.materialTabControl1.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.AllowDrop = true;
			this.tabPage1.BackColor = System.Drawing.Color.White;
			this.tabPage1.Controls.Add(this.progressBar);
			this.tabPage1.Controls.Add(this.materialLabel3);
			this.tabPage1.Controls.Add(this.materialLabel2);
			this.tabPage1.Controls.Add(this.numericUpDown2);
			this.tabPage1.Controls.Add(this.materialLabel1);
			this.tabPage1.Controls.Add(this.numericUpDown1);
			this.tabPage1.Controls.Add(this.OutputVideoPath);
			this.tabPage1.Controls.Add(this.InputVideoPath);
			this.tabPage1.Controls.Add(this.materialRaisedButton1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(772, 306);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "自动分段";
			this.tabPage1.DragDrop += new System.Windows.Forms.DragEventHandler(this.InputVideoPath_DragDrop);
			this.tabPage1.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// progressBar
			// 
			this.progressBar.Depth = 0;
			this.progressBar.Location = new System.Drawing.Point(8, 111);
			this.progressBar.MouseState = MaterialSkin.MouseState.HOVER;
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(776, 5);
			this.progressBar.TabIndex = 14;
			// 
			// materialLabel3
			// 
			this.materialLabel3.AutoSize = true;
			this.materialLabel3.Depth = 0;
			this.materialLabel3.Font = new System.Drawing.Font("Roboto", 11F);
			this.materialLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.materialLabel3.Location = new System.Drawing.Point(378, 72);
			this.materialLabel3.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialLabel3.Name = "materialLabel3";
			this.materialLabel3.Size = new System.Drawing.Size(41, 19);
			this.materialLabel3.TabIndex = 13;
			this.materialLabel3.Text = "分钟";
			// 
			// materialLabel2
			// 
			this.materialLabel2.AutoSize = true;
			this.materialLabel2.Depth = 0;
			this.materialLabel2.Font = new System.Drawing.Font("Roboto", 11F);
			this.materialLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.materialLabel2.Location = new System.Drawing.Point(175, 72);
			this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialLabel2.Name = "materialLabel2";
			this.materialLabel2.Size = new System.Drawing.Size(108, 19);
			this.materialLabel2.TabIndex = 12;
			this.materialLabel2.Text = "GB，每段截取";
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.DecimalPlaces = 3;
			this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
			this.numericUpDown2.Location = new System.Drawing.Point(104, 71);
			this.numericUpDown2.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(65, 21);
			this.numericUpDown2.TabIndex = 11;
			this.numericUpDown2.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
			// 
			// materialLabel1
			// 
			this.materialLabel1.AutoSize = true;
			this.materialLabel1.Depth = 0;
			this.materialLabel1.Font = new System.Drawing.Font("Roboto", 11F);
			this.materialLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
			this.materialLabel1.Location = new System.Drawing.Point(4, 71);
			this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialLabel1.Name = "materialLabel1";
			this.materialLabel1.Size = new System.Drawing.Size(105, 19);
			this.materialLabel1.TabIndex = 10;
			this.materialLabel1.Text = "如果视频大于";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(289, 71);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(83, 21);
			this.numericUpDown1.TabIndex = 9;
			this.numericUpDown1.Value = new decimal(new int[] {
            90,
            0,
            0,
            0});
			// 
			// OutputVideoPath
			// 
			this.OutputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputVideoPath.Depth = 0;
			this.OutputVideoPath.Hint = "将视频或输出路径拖拽至此";
			this.OutputVideoPath.Location = new System.Drawing.Point(8, 35);
			this.OutputVideoPath.MaxLength = 32767;
			this.OutputVideoPath.MouseState = MaterialSkin.MouseState.HOVER;
			this.OutputVideoPath.Name = "OutputVideoPath";
			this.OutputVideoPath.PasswordChar = '\0';
			this.OutputVideoPath.SelectedText = "";
			this.OutputVideoPath.SelectionLength = 0;
			this.OutputVideoPath.SelectionStart = 0;
			this.OutputVideoPath.Size = new System.Drawing.Size(756, 23);
			this.OutputVideoPath.TabIndex = 8;
			this.OutputVideoPath.TabStop = false;
			this.OutputVideoPath.UseSystemPasswordChar = false;
			// 
			// InputVideoPath
			// 
			this.InputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InputVideoPath.Depth = 0;
			this.InputVideoPath.Hint = "将视频或输出路径拖拽至此";
			this.InputVideoPath.Location = new System.Drawing.Point(8, 6);
			this.InputVideoPath.MaxLength = 32767;
			this.InputVideoPath.MouseState = MaterialSkin.MouseState.HOVER;
			this.InputVideoPath.Name = "InputVideoPath";
			this.InputVideoPath.PasswordChar = '\0';
			this.InputVideoPath.SelectedText = "";
			this.InputVideoPath.SelectionLength = 0;
			this.InputVideoPath.SelectionStart = 0;
			this.InputVideoPath.Size = new System.Drawing.Size(756, 23);
			this.InputVideoPath.TabIndex = 7;
			this.InputVideoPath.TabStop = false;
			this.InputVideoPath.UseSystemPasswordChar = false;
			// 
			// materialRaisedButton1
			// 
			this.materialRaisedButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.materialRaisedButton1.AutoSize = true;
			this.materialRaisedButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.materialRaisedButton1.Depth = 0;
			this.materialRaisedButton1.Icon = null;
			this.materialRaisedButton1.Location = new System.Drawing.Point(713, 71);
			this.materialRaisedButton1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialRaisedButton1.Name = "materialRaisedButton1";
			this.materialRaisedButton1.Primary = true;
			this.materialRaisedButton1.Size = new System.Drawing.Size(51, 36);
			this.materialRaisedButton1.TabIndex = 4;
			this.materialRaisedButton1.Text = "截取";
			this.materialRaisedButton1.UseVisualStyleBackColor = false;
			this.materialRaisedButton1.Click += new System.EventHandler(this.materialRaisedButton1_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.BackColor = System.Drawing.Color.White;
			this.tabPage2.Controls.Add(this.infoTextBox);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(792, 316);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Info";
			// 
			// infoTextBox
			// 
			this.infoTextBox.AllowDrop = true;
			this.infoTextBox.BackColor = System.Drawing.Color.White;
			this.infoTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.infoTextBox.Location = new System.Drawing.Point(3, 3);
			this.infoTextBox.Multiline = true;
			this.infoTextBox.Name = "infoTextBox";
			this.infoTextBox.ReadOnly = true;
			this.infoTextBox.Size = new System.Drawing.Size(786, 310);
			this.infoTextBox.TabIndex = 0;
			this.infoTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.infoTextBox_DragDrop);
			this.infoTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.materialTabControl1);
			this.Controls.Add(this.materialTabSelector1);
			this.MinimumSize = new System.Drawing.Size(800, 450);
			this.Name = "MainForm";
			this.Text = "AutoSplitVideo";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.materialTabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
		private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private MaterialSkin.Controls.MaterialRaisedButton materialRaisedButton1;
		private MaterialSkin.Controls.MaterialSingleLineTextField InputVideoPath;
		private MaterialSkin.Controls.MaterialSingleLineTextField OutputVideoPath;
		private System.Windows.Forms.TextBox infoTextBox;
		private MaterialSkin.Controls.MaterialLabel materialLabel1;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private MaterialSkin.Controls.MaterialLabel materialLabel2;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private MaterialSkin.Controls.MaterialLabel materialLabel3;
		private MaterialSkin.Controls.MaterialProgressBar progressBar;
	}
}

