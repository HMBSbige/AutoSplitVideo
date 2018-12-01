using AutoSplitVideo.Properties;
using AutoSplitVideo.Utils;
using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoSplitVideo
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			Icon = Resources.Asaki;
			notifyIcon1.Icon = Icon;
		}

		private FormWindowState DefaultState = FormWindowState.Normal;

		private TimeSpan Duration => TimeSpan.FromMinutes(Convert.ToDouble(numericUpDown2.Value));
		private long Limit => Convert.ToInt64(numericUpDown1.Value * 1024 * 1024 * 8);

		#region MainForm

		private void MainForm_Load(object sender, EventArgs e)
		{
			HintTextBox.SetCueText(InputVideoPath, @"视频路径");
			HintTextBox.SetCueText(OutputVideoPath, @"输出路径");
			HintTextBox.SetCueText(RecordDirectory, @"录播输出路径");
			RecordDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
		}

		private void MainForm_Resize(object sender, EventArgs e)
		{
			if (WindowState == FormWindowState.Minimized)
			{
				TriggerMainFormDisplay();
			}
			else
			{
				DefaultState = WindowState;
			}
		}

		private void Button1_Click(object sender, EventArgs e)
		{
			SetControlEnable(false);
			var runTask = SplitTask(InputVideoPath.Text, OutputVideoPath.Text, checkBox1.Checked);
			runTask.Start();
			runTask.ContinueWith(task =>
			{
				BeginInvoke(new Action(() =>
				{
					SetControlEnable(true);
				}));
			});
		}

		private void ShowVideoInfo(string path)
		{
			var inputFile = new MediaFile { Filename = path };

			using (var engine = new Engine())
			{
				engine.GetMetadata(inputFile);
			}

			var sb = new StringBuilder();
			sb.AppendLine($@"文件：{inputFile.Filename}");
			try
			{
				sb.AppendLine($@"时长：{inputFile.Metadata.Duration}");

				sb.AppendLine($@"音频码率：{inputFile.Metadata.AudioData.BitRateKbs}");
				sb.AppendLine($@"音频格式：{inputFile.Metadata.AudioData.Format}");
				sb.AppendLine($@"声道：{inputFile.Metadata.AudioData.ChannelOutput}");
				sb.AppendLine($@"采样率：{inputFile.Metadata.AudioData.SampleRate}");

				sb.AppendLine($@"视频码率：{inputFile.Metadata.VideoData.BitRateKbs}");
				sb.AppendLine($@"视频格式：{inputFile.Metadata.VideoData.Format}");
				sb.AppendLine($@"颜色模型：{inputFile.Metadata.VideoData.ColorModel}");
				sb.AppendLine($@"帧率：{inputFile.Metadata.VideoData.Fps}");
				sb.AppendLine($@"分辨率：{inputFile.Metadata.VideoData.FrameSize}");

			}
			catch
			{
				sb.AppendLine(@"读取视频文件失败，可能是错误的视频文件");
			}

			infoTextBox.Invoke(new Action(() => { infoTextBox.Text = sb.ToString(); }));
		}

		private void button2_Click(object sender, EventArgs e)
		{
			var dir = Util.SelectPath();
			if (string.IsNullOrWhiteSpace(dir) || !Directory.Exists(dir))
			{
				return;
			}
			RecordDirectory.Text = dir;
		}

		#endregion

		#region 设置控件状态

		private void SetControlEnable(bool b)
		{
			button1.Enabled = b;
			InputVideoPath.Enabled = b;
			OutputVideoPath.Enabled = b;
			numericUpDown1.Enabled = b;
			numericUpDown2.Enabled = b;
			checkBox1.Enabled = b;
		}

		private void SetProgressBar(int i)
		{
			progressBar.Invoke(new Action(() =>
			{
				if (i > progressBar.Maximum)
				{
					i = progressBar.Maximum;
				}
				progressBar.Value = i;
			}));
		}

		#endregion

		#region 拖拽

		private void tabControl1_DragOver(object sender, DragEventArgs e)
		{
			e.Effect = DragDropEffects.All;

			var clientPoint = tabControl1.PointToClient(new Point(e.X, e.Y));

			for (var i = 0; i < tabControl1.TabCount; i++)
			{
				if (tabControl1.SelectedIndex != i && tabControl1.GetTabRect(i).Contains(clientPoint))
				{
					tabControl1.SelectedIndex = i;
				}
			}
		}

		private void infoTextBox_DragDrop(object sender, DragEventArgs e)
		{
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
			ShowVideoInfo(path);
		}

		private void InputVideoPath_DragDrop(object sender, DragEventArgs e)
		{
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
			if (Directory.Exists(path))
			{
				OutputVideoPath.Text = $@"{Path.GetFullPath(path)}";
				if (!OutputVideoPath.Text.EndsWith(@"\"))
				{
					OutputVideoPath.Text += @"\";
				}
			}
			else if (File.Exists(path))
			{
				InputVideoPath.Text = Path.GetFullPath(path);
				OutputVideoPath.Text = $@"{Path.GetDirectoryName(path)}\";
				if (!OutputVideoPath.Text.EndsWith(@"\"))
				{
					OutputVideoPath.Text += @"\";
				}
			}
		}

		private void FilePath_DragEnter(object sender, DragEventArgs e)
		{
			e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Link : DragDropEffects.None;
		}

		#endregion

		#region 程序退出

		private void Exit()
		{
			Util.StopFFmpeg();
			Dispose();
			Environment.Exit(0);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				var dr = MessageBox.Show(@"是否退出？", @"是否退出？", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (dr == DialogResult.Yes)
				{
					Exit();
				}
				else
				{
					e.Cancel = true;
				}
			}
			else
			{
				Exit();
			}
		}

		#endregion

		#region 托盘图标

		private void TriggerMainFormDisplay()
		{
			Visible = !Visible;
			if (Visible)
			{
				if (WindowState == FormWindowState.Minimized)
				{
					WindowState = DefaultState;
				}

				TopMost = true;
				TopMost = false;
			}
		}

		private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
		{
			TriggerMainFormDisplay();
		}

		#endregion

		#region Task

		private Task SplitTask(string inputVideoPath, string outputDirectoryPath, bool deleteMp4)
		{
			return new Task(() =>
			{
				//try
				//{
				ShowVideoInfo(inputVideoPath);
				SetProgressBar(0);
				using (var engine = new Engine())
				{
					var inputFile = new MediaFile(inputVideoPath);
					var outputFile = new MediaFile();
					var mp4File = new MediaFile($@"{outputDirectoryPath}{Path.GetFileNameWithoutExtension(inputVideoPath)}.mp4");

					//flv转封装成MP4
					var ismp4 = true; //原档是否为mp4
					if (File.Exists(mp4File.Filename))
					{
						//do nothing
					}
					else if (Path.GetExtension(inputVideoPath) == @"mp4")
					{
						mp4File.Filename = inputVideoPath;
					}
					else
					{
						ismp4 = false;
						engine.CustomCommand($@"-i ""{inputFile.Filename}"" -c copy -copyts ""{mp4File.Filename}""");
					}

					SetProgressBar(50);

					//分段
					ShowVideoInfo(mp4File.Filename);
					if (Util.GetFileSize(mp4File.Filename) > Limit * 1024 / 8)
					{
						engine.GetMetadata(mp4File);
						var vb = mp4File.Metadata.VideoData.BitRateKbs ?? 0;
						var ab = mp4File.Metadata.AudioData.BitRateKbs;
						var maxDuration = TimeSpan.FromSeconds(Convert.ToDouble(Limit) / (vb + ab));
						var duration = mp4File.Metadata.Duration;
						var now = TimeSpan.Zero;
						for (var i = 0; now < duration; ++i)
						{
							var t = Duration;
							if (now + maxDuration >= duration)
							{
								t = duration - now;
							}

							outputFile.Filename = $@"{outputDirectoryPath}{Path.GetFileNameWithoutExtension(mp4File.Filename)}_{i + 1}.mp4";

							engine.CustomCommand($@"-ss {now} -t {t} -accurate_seek -i ""{mp4File.Filename}"" -codec copy -avoid_negative_ts 1 ""{outputFile.Filename}""");

							engine.GetMetadata(outputFile);
							now += t;

							SetProgressBar(50 + Convert.ToInt32(Convert.ToDouble(now.Ticks) / duration.Ticks * 50));
						}

						if (!ismp4 && deleteMp4)
						{
							File.Delete(mp4File.Filename);
						}
					}
				}

				SetProgressBar(100);
				MessageBox.Show(@"完成！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
				//}
				//catch (Exception ex)
				//{
				//	MessageBox.Show(ex.Message, @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				//}
			});
		}

		private Task FFmpegRecordTask(string url, string path)
		{
			return new Task(() =>
			{
				using (var engine = new Engine())
				{
					engine.CustomCommand($@"-y -i ""{url}"" ""{path}""");
				}
			});
		}

		#endregion

	}
}
