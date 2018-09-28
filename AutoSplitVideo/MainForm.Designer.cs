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
			this.materialRaisedButton1 = new MaterialSkin.Controls.MaterialRaisedButton();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.InputVideoPath = new System.Windows.Forms.TextBox();
			this.OutputVideoPath = new System.Windows.Forms.TextBox();
			this.materialTabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
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
			this.materialTabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.materialTabControl1.Controls.Add(this.tabPage1);
			this.materialTabControl1.Controls.Add(this.tabPage2);
			this.materialTabControl1.Depth = 0;
			this.materialTabControl1.Location = new System.Drawing.Point(12, 105);
			this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialTabControl1.Name = "materialTabControl1";
			this.materialTabControl1.SelectedIndex = 0;
			this.materialTabControl1.Size = new System.Drawing.Size(776, 174);
			this.materialTabControl1.TabIndex = 1;
			// 
			// tabPage1
			// 
			this.tabPage1.BackColor = System.Drawing.Color.White;
			this.tabPage1.Controls.Add(this.OutputVideoPath);
			this.tabPage1.Controls.Add(this.InputVideoPath);
			this.tabPage1.Controls.Add(this.materialRaisedButton1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(768, 148);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "自动分段";
			// 
			// materialRaisedButton1
			// 
			this.materialRaisedButton1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.materialRaisedButton1.AutoSize = true;
			this.materialRaisedButton1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.materialRaisedButton1.Depth = 0;
			this.materialRaisedButton1.Icon = null;
			this.materialRaisedButton1.Location = new System.Drawing.Point(713, 68);
			this.materialRaisedButton1.MouseState = MaterialSkin.MouseState.HOVER;
			this.materialRaisedButton1.Name = "materialRaisedButton1";
			this.materialRaisedButton1.Primary = true;
			this.materialRaisedButton1.Size = new System.Drawing.Size(55, 36);
			this.materialRaisedButton1.TabIndex = 4;
			this.materialRaisedButton1.Text = "截取";
			this.materialRaisedButton1.UseVisualStyleBackColor = false;
			this.materialRaisedButton1.Click += new System.EventHandler(this.materialRaisedButton1_Click);
			// 
			// tabPage2
			// 
			this.tabPage2.BackColor = System.Drawing.Color.White;
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(768, 148);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Info";
			// 
			// InputVideoPath
			// 
			this.InputVideoPath.AllowDrop = true;
			this.InputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.InputVideoPath.Font = new System.Drawing.Font("宋体", 11F);
			this.InputVideoPath.Location = new System.Drawing.Point(0, 8);
			this.InputVideoPath.Name = "InputVideoPath";
			this.InputVideoPath.ReadOnly = true;
			this.InputVideoPath.Size = new System.Drawing.Size(768, 24);
			this.InputVideoPath.TabIndex = 5;
			this.InputVideoPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.InputVideoPath_DragDrop);
			this.InputVideoPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
			// 
			// OutputVideoPath
			// 
			this.OutputVideoPath.AllowDrop = true;
			this.OutputVideoPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.OutputVideoPath.Font = new System.Drawing.Font("宋体", 11F);
			this.OutputVideoPath.Location = new System.Drawing.Point(0, 38);
			this.OutputVideoPath.Name = "OutputVideoPath";
			this.OutputVideoPath.Size = new System.Drawing.Size(768, 24);
			this.OutputVideoPath.TabIndex = 6;
			this.OutputVideoPath.DragDrop += new System.Windows.Forms.DragEventHandler(this.InputVideoPath_DragDrop);
			this.OutputVideoPath.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilePath_DragEnter);
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
			this.ResumeLayout(false);

		}

		#endregion

		private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
		private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private MaterialSkin.Controls.MaterialRaisedButton materialRaisedButton1;
		private System.Windows.Forms.TextBox InputVideoPath;
		private System.Windows.Forms.TextBox OutputVideoPath;
	}
}

