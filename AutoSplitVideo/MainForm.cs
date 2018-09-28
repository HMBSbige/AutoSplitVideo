using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.IO;
using System.Windows.Forms;

namespace AutoSplitVideo
{

	public partial class MainForm : MaterialForm
	{
		public MainForm()
		{
			InitializeComponent();

			var materialSkinManager = MaterialSkinManager.Instance;
			materialSkinManager.AddFormToManage(this);
			materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
			materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500, Primary.Indigo700, Primary.Indigo100, Accent.Pink200, TextShade.WHITE);
		}

		private void materialRaisedButton1_Click(object sender, EventArgs e)
		{

		}

		private void InputVideoPath_DragDrop(object sender, DragEventArgs e)
		{
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
			if (Directory.Exists(path))
			{
				OutputVideoPath.Text = $@"{Path.GetFullPath(path)}\";
			}
			else if (File.Exists(path))
			{
				InputVideoPath.Text = Path.GetFullPath(path);
				OutputVideoPath.Text = $@"{Path.GetDirectoryName(path)}\";
			}
		}

		private void FilePath_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Link;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			HintTextBox.SetCueText(InputVideoPath, @"将视频或输出路径拖拽至此");
			HintTextBox.SetCueText(OutputVideoPath, @"将视频或输出路径拖拽至此");
			InputVideoPath.Font = MaterialSkinManager.Instance.ROBOTO_MEDIUM_10;
			OutputVideoPath.Font = MaterialSkinManager.Instance.ROBOTO_MEDIUM_10;
		}
	}
}
