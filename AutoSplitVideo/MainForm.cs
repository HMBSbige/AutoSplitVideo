using AutoSplitVideo.Collections;
using AutoSplitVideo.Controller;
using AutoSplitVideo.Controls;
using AutoSplitVideo.Flv;
using AutoSplitVideo.Model;
using AutoSplitVideo.Properties;
using AutoSplitVideo.Utils;
using AutoSplitVideo.ViewModel;
using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoSplitVideo
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
			_config.Load();
			Icon = Resources.Asaki;
			notifyIcon1.Icon = Icon;
		}

		private static string ExeName => Assembly.GetExecutingAssembly().GetName().Name;
		private readonly AppConfig _config = new AppConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"{ExeName}.cfg"));

		private FormWindowState DefaultState = FormWindowState.Normal;

		private TimeSpan Duration => TimeSpan.FromMinutes(Convert.ToDouble(numericUpDown2.Value));
		private long Limit => Convert.ToInt64(numericUpDown1.Value * 1024 * 1024 * 8);

		private const long RecordFileMinSize = 13 * 1024;//13KB

		private const int Interval = 10 * 1000;

		private const string ConvertCommand = @"-i ""{0}"" -c copy ""{1}""";
		private const string SplitCommand = @"-ss {0} -t {1} -accurate_seek -i ""{2}"" -codec copy -avoid_negative_ts 1 ""{3}""";
		private const string MuxCommand = @"-i ""{0}"" -i ""{1}"" -vcodec copy -acodec copy ""{2}""";

		#region MainForm

		private void MainForm_Load(object sender, EventArgs e)
		{
			HintTextBox.SetCueText(InputVideoPath, @"视频路径");
			HintTextBox.SetCueText(OutputVideoPath, @"输出路径");
			LoadConfig();
			if (!Directory.Exists(RecordDirectory.Text))
			{
				RecordDirectory.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
			}
			timer1.Start();
			AutoStartupCheckBox.Checked = AutoStartup.Check();

			AutoStartupCheckBox.Click += AutoStartupCheckBox_CheckedChanged;
			NotifyCheckBox.Click += NotifyCheckBox_Click;
			radioButton1.CheckedChanged += radioButton_CheckedChanged;
			radioButton2.CheckedChanged += radioButton_CheckedChanged;
			radioButton3.CheckedChanged += radioButton_CheckedChanged;
			radioButton4.CheckedChanged += radioButton_CheckedChanged;
			RecordDirectory.TextChanged += RecordDirectory_TextChanged;
			tabControl1.SelectedIndexChanged += tabControl1_SelectedIndexChanged;
			AutoConvert.CheckedChanged += AutoConvert_CheckedChanged;
			checkBox1.CheckedChanged += CheckBox1_CheckedChanged;
			checkBox2.CheckedChanged += CheckBox2_CheckedChanged;
			checkBox3.CheckedChanged += CheckBox3_CheckedChanged;
			checkBox4.CheckedChanged += CheckBox4_CheckedChanged;
			checkBox5.CheckedChanged += CheckBox5_CheckedChanged;
			checkBox6.CheckedChanged += CheckBox6_CheckedChanged;
			numericUpDown1.ValueChanged += NumericUpDown1_ValueChanged;
			numericUpDown2.ValueChanged += NumericUpDown2_ValueChanged;

			CheckBox1Changed();
			CheckBox2Changed();
			CheckBox5Changed();

			LoadMainList();
		}

		private void AutoStartupCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!AutoStartup.Set(AutoStartupCheckBox.Checked))
			{
				MessageBox.Show(@"设置失败", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				AutoStartupCheckBox.Checked = !AutoStartupCheckBox.Checked;
			}
		}

		private void LoadConfig()
		{
			tabControl1.SelectedIndex = _config.TableIndex;
			RecordDirectory.Text = _config.OutputPath;
			NotifyCheckBox.Checked = _config.IsNotify != 0;
			AutoConvert.Checked = _config.IsAutoConvertFlv != 0;

			checkBox1.Checked = _config.DeleteFlv != 0;
			checkBox2.Checked = _config.OnlyConvert != 0;
			checkBox3.Checked = _config.IsSkipSameMp4 != 0;
			checkBox4.Checked = _config.IsSendToRecycleBin != 0;
			checkBox5.Checked = _config.OutputSameAsInput != 0;
			checkBox6.Checked = _config.UseFlvFix != 0;

			numericUpDown1.Value = _config.N1;
			numericUpDown2.Value = _config.N2;

			switch (_config.StreamUrlIndex)
			{
				case 0:
					radioButton1.Checked = true;
					break;
				case 1:
					radioButton2.Checked = true;
					break;
				case 2:
					radioButton3.Checked = true;
					break;
				case 3:
					radioButton4.Checked = true;
					break;
			}

			foreach (var roomId in _config.Rooms)
			{
				AddRoom(roomId);
			}
		}

		private VideoConvertConfig GetVideoConvertConfig()
		{
			return new VideoConvertConfig
			{
				DeleteFlv = checkBox1.Checked,
				OnlyConvert = checkBox2.Checked,
				IsSkipSameMp4 = checkBox3.Checked,
				IsSendToRecycleBin = checkBox4.Checked,
				OutputSameAsInput = checkBox5.Checked,
				UseFlvFix = checkBox6.Checked
			};
		}

		private void LoadMainList()
		{
			MainList.AutoGenerateColumns = false;
			MainList.DataSource = Table;

			MainList.Columns[0].HeaderText = @"房间号";
			MainList.Columns[1].HeaderText = @"主播";
			MainList.Columns[2].HeaderText = @"直播标题";
			MainList.Columns[3].HeaderText = @"直播状态";
			MainList.Columns[4].HeaderText = @"录制状态";

			MainList.Columns[0].DataPropertyName = @"RealRoomID";
			MainList.Columns[1].DataPropertyName = @"AnchorName";
			MainList.Columns[2].DataPropertyName = @"Title";
			MainList.Columns[3].DataPropertyName = @"LiveStatus";
			MainList.Columns[4].DataPropertyName = @"RecordingStatus";
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
			Task runTask;
			var config = GetVideoConvertConfig();
			if (File.Exists(InputVideoPath.Text))
			{
				runTask = SplitTaskFile(InputVideoPath.Text, OutputVideoPath.Text, config, true);
			}
			else if (Directory.Exists(InputVideoPath.Text))
			{
				runTask = SplitTaskDirectory(InputVideoPath.Text, OutputVideoPath.Text, config);
			}
			else
			{
				MessageBox.Show(@"视频路径出错", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
				SetControlEnable(true);
				return;
			}
			if (runTask != null)
			{
				runTask.Start();
				runTask.ContinueWith(task => { BeginInvoke(new Action(() => { SetControlEnable(true); })); });
			}
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

		private void button9_Click(object sender, EventArgs e)
		{
			var path = RecordDirectory.Text;
			if (Directory.Exists(path))
			{
				Process.Start(path);
			}
		}

		#endregion

		#region MediaInfo

		private void ShowVideoInfo(string path, bool isComplete = false)
		{
			var sb = new StringBuilder();
			try
			{
				using (var mi = new MediaInfo.MediaInfo())
				{
					mi.Open(path);
					if (isComplete)
					{
						mi.Option(@"Complete", @"1");
					}
					else
					{
						mi.Option(@"Complete");
					}
					sb.AppendLine(mi.Inform());
				}
			}
			catch
			{
				sb.AppendLine(@"读取文件失败，可能是 MediaInfo.dll 加载错误");
			}

			infoTextBox.Invoke(new Action(() => { infoTextBox.Text = sb.ToString(); }));
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
			checkBox4.Enabled = b;
			checkBox1.Enabled = b;
			checkBox2.Enabled = b;
			checkBox3.Enabled = b;
			checkBox5.Enabled = b;
			checkBox6.Enabled = b;
		}

		private void CheckBox2Changed()
		{
			if (checkBox2.Checked)
			{
				numericUpDown1.Enabled = false;
				numericUpDown2.Enabled = false;
			}
			else if (checkBox2.Enabled)
			{
				numericUpDown1.Enabled = true;
				numericUpDown2.Enabled = true;
			}
		}

		private void CheckBox2_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox2Changed();
		}

		private void CheckBox2_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox2Changed();
			SaveConfig();
		}

		private void CheckBox1Changed()
		{
			if (checkBox1.Enabled)
			{
				checkBox4.Enabled = checkBox1.Checked;
			}
		}

		private void CheckBox1_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox1Changed();
			SaveConfig();
		}

		private void CheckBox1_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox1Changed();
		}

		private void CheckBox5Changed()
		{
			if (checkBox5.Enabled)
			{
				OutputVideoPath.Enabled = !checkBox5.Checked;
				OutputVideoPath.Text = string.Empty;
			}
		}

		private void CheckBox5_EnabledChanged(object sender, EventArgs e)
		{
			CheckBox5Changed();
		}

		private void CheckBox5_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox5Changed();
			SaveConfig();
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

		private void SetLabel4(int num)
		{
			label4.Invoke(new Action(() => { label4.Text = $@"队列中的视频文件：{num} 个"; }));
		}

		private void MainList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
			if (e.ColumnIndex == 3 || e.ColumnIndex == 4)
			{
				var cell = MainList.Rows[e.RowIndex].Cells[e.ColumnIndex];
				if (cell.Value is string oldValue)
				{
					cell.Style.ForeColor = oldValue.EndsWith(@"...") ? Color.Green : Color.Red;
				}
			}
		}

		private void List_MouseDown(object sender, MouseEventArgs e)
		{
			var dgv = (DoubleBufferedDataGridView)sender;

			var rowIndex = dgv.HitTest(e.X, e.Y).RowIndex;

			if (rowIndex == -1)
			{
				dgv.ClearSelection();
			}
		}

		private void MainList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var rows = MainList.SelectedRows;
			if (rows.Count == 1)
			{
				if (rows[0].Cells[0].Value is long value)
				{
					Process.Start($@"https://live.bilibili.com/{value}");
				}
			}
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
				path = Path.GetFullPath(path);
				if (!path.EndsWith(@"\"))
				{
					path += @"\";
				}
				OutputVideoPath.Text = OutputVideoPath.Enabled ? path : string.Empty;
				InputVideoPath.Text = path;
			}
			else if (File.Exists(path))
			{
				InputVideoPath.Text = Path.GetFullPath(path);
				var outputPath = $@"{Path.GetDirectoryName(path)}";
				if (!outputPath.EndsWith(@"\"))
				{
					outputPath += @"\";
				}
				OutputVideoPath.Text = OutputVideoPath.Enabled ? outputPath : string.Empty;
			}
		}

		private void OutputVideoPath_DragDrop(object sender, DragEventArgs e)
		{
			var path = ((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
			if (Directory.Exists(path))
			{
				path = Path.GetFullPath(path);
				if (!path.EndsWith(@"\"))
				{
					path += @"\";
				}
				OutputVideoPath.Text = path;
			}
			else if (File.Exists(path))
			{
				InputVideoPath.Text = Path.GetFullPath(path);
				OutputVideoPath.Text = $@"{Path.GetDirectoryName(path)}";
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

		#region 保存配置

		private void SaveConfig()
		{
			_config.TableIndex = tabControl1.SelectedIndex;
			_config.OutputPath = RecordDirectory.Text;
			_config.StreamUrlIndex = GetStreamUrlIndex();
			_config.Rooms = _table.Select(room => room.RealRoomID);
			_config.IsNotify = NotifyCheckBox.Checked ? 1 : 0;
			_config.IsAutoConvertFlv = AutoConvert.Checked ? 1 : 0;

			_config.DeleteFlv = checkBox1.Checked ? 1 : 0;
			_config.OnlyConvert = checkBox2.Checked ? 1 : 0;
			_config.IsSkipSameMp4 = checkBox3.Checked ? 1 : 0;
			_config.IsSendToRecycleBin = checkBox4.Checked ? 1 : 0;
			_config.OutputSameAsInput = checkBox5.Checked ? 1 : 0;
			_config.UseFlvFix = checkBox6.Checked ? 1 : 0;

			_config.N1 = numericUpDown1.Value;
			_config.N2 = numericUpDown2.Value;

			_config.Save();
		}

		private void RecordDirectory_TextChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void radioButton_CheckedChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void NotifyCheckBox_Click(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void AutoConvert_CheckedChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void NumericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void NumericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void CheckBox3_CheckedChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void CheckBox4_CheckedChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		private void CheckBox6_CheckedChanged(object sender, EventArgs e)
		{
			SaveConfig();
		}

		#endregion

		#region 程序退出

		private void Exit()
		{
			var dr = MessageBox.Show($@"确定退出 {ExeName}？", @"退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
			if (dr == DialogResult.OK)
			{
				SaveConfig();
				StopAllRecordTasks();
				Util.KillFFmpeg();
				Dispose();
				Environment.Exit(0);
			}
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (e.CloseReason == CloseReason.UserClosing)
			{
				TriggerMainFormDisplay();
				e.Cancel = true;
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
			if (e.Button == MouseButtons.Left)
			{
				TriggerMainFormDisplay();
			}
		}

		private void ShowHideMenuItem_Click(object sender, EventArgs e)
		{
			TriggerMainFormDisplay();
		}

		private void ExitMenuItem_Click(object sender, EventArgs e)
		{
			Exit();
		}

		#endregion

		#region Async、Task

		private Task SplitTaskFile(string inputVideoPath, string outputDirectoryPath, VideoConvertConfig config, bool isNotify)
		{
			return new Task(() =>
			{
				ShowVideoInfo(inputVideoPath);
				if (isNotify)
				{
					SetProgressBar(0);
					SetLabel4(1);
				}
				using (var engine = new Engine())
				{
					var inputFile = new MediaFile(inputVideoPath);
					var outputFile = new MediaFile();
					if (config.OutputSameAsInput || string.IsNullOrWhiteSpace(outputDirectoryPath) || !Directory.Exists(outputDirectoryPath))
					{
						outputDirectoryPath = Path.GetDirectoryName(inputVideoPath);
					}
					var mp4File = new MediaFile(Path.Combine(outputDirectoryPath ?? throw new ArgumentNullException(nameof(outputDirectoryPath)), $@"{Path.GetFileNameWithoutExtension(inputVideoPath)}.mp4"));

					//flv转封装成MP4
					if (config.IsSkipSameMp4 && File.Exists(mp4File.Filename))
					{
						//do nothing
					}
					else if (Util.IsMp4(inputFile.Filename))
					{
						mp4File.Filename = inputVideoPath;
					}
					else if (config.UseFlvFix)
					{
						using (var flv = new FlvFile(inputFile.Filename))
						{
							flv.ExtractStreams(true, true, false, true);

							engine.CustomCommand(string.Format(MuxCommand, flv.VideoPath, flv.AudioPath, mp4File.Filename));

							File.Delete(flv.VideoPath);
							File.Delete(flv.AudioPath);
						}
					}
					else
					{
						engine.CustomCommand(string.Format(ConvertCommand, inputFile.Filename, mp4File.Filename));
					}

					if (isNotify)
					{
						SetProgressBar(50);
					}

					if (!config.OnlyConvert)
					{
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

								outputFile.Filename = Path.Combine(outputDirectoryPath, $@"{Path.GetFileNameWithoutExtension(mp4File.Filename)}_{i + 1}.mp4");

								engine.CustomCommand(string.Format(SplitCommand, now, t, mp4File.Filename, outputFile.Filename));

								engine.GetMetadata(outputFile);
								now += t;

								if (isNotify)
								{
									SetProgressBar(50 + Convert.ToInt32(Convert.ToDouble(now.Ticks) / duration.Ticks * 50));
								}
							}
						}
					}

					if (config.DeleteFlv && Util.IsFlv(inputFile.Filename))
					{
						if (config.IsSendToRecycleBin)
						{
							FileSystem.DeleteFile(inputFile.Filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
						}
						else
						{
							File.Delete(inputFile.Filename);
						}
					}
				}

				if (isNotify)
				{
					SetProgressBar(100);
					MessageBox.Show($@"{Path.GetFileName(inputVideoPath)} 完成！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
				else
				{
					Logging.Info($@"{Path.GetFullPath(inputVideoPath)} 转封装完成！");
				}
			});
		}

		private Task SplitTaskDirectory(string inputVideoPath, string outputDirectoryPath, VideoConvertConfig config)
		{
			return new Task(() =>
			{
				var progress = 0;
				SetProgressBar(progress);
				var flvs = Util.GetAllFlvList(inputVideoPath);
				SetLabel4(flvs.Count);
				if (flvs.Count > 0)
				{
					var halfProgress = 50 / flvs.Count;
					using (var engine = new Engine())
					{
						foreach (var flv in flvs)
						{
							var inputFile = new MediaFile(flv);
							var outputFile = new MediaFile();
							if (config.OutputSameAsInput || string.IsNullOrWhiteSpace(outputDirectoryPath) || !Directory.Exists(outputDirectoryPath))
							{
								outputDirectoryPath = Path.GetDirectoryName(flv);
							}
							var mp4File = new MediaFile(Path.Combine(outputDirectoryPath ?? throw new ArgumentNullException(nameof(outputDirectoryPath)), $@"{Path.GetFileNameWithoutExtension(flv)}.mp4"));

							//flv转封装成MP4
							if (config.IsSkipSameMp4 && File.Exists(mp4File.Filename))
							{
								//do nothing
							}
							else if (config.UseFlvFix)
							{
								using (var flvFile = new FlvFile(inputFile.Filename))
								{
									flvFile.ExtractStreams(true, true, false, true);

									engine.CustomCommand(string.Format(MuxCommand, flvFile.VideoPath, flvFile.AudioPath, mp4File.Filename));

									File.Delete(flvFile.VideoPath);
									File.Delete(flvFile.AudioPath);
								}
							}
							else
							{
								engine.CustomCommand(string.Format(ConvertCommand, inputFile.Filename, mp4File.Filename));
							}

							progress += halfProgress;
							SetProgressBar(progress);

							if (!config.OnlyConvert)
							{
								//分段
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

										outputFile.Filename = Path.Combine(outputDirectoryPath, $@"{Path.GetFileNameWithoutExtension(mp4File.Filename)}_{i + 1}.mp4");

										engine.CustomCommand(string.Format(SplitCommand, now, t, mp4File.Filename, outputFile.Filename));

										engine.GetMetadata(outputFile);
										now += t;

										progress += Convert.ToInt32(Convert.ToDouble(now.Ticks) / duration.Ticks * halfProgress);
										SetProgressBar(progress);
									}
								}
							}
							else
							{
								progress += halfProgress;
								SetProgressBar(progress);
							}

							if (config.DeleteFlv && Util.IsFlv(inputFile.Filename))
							{
								if (config.IsSendToRecycleBin)
								{
									FileSystem.DeleteFile(inputFile.Filename, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
								}
								else
								{
									File.Delete(inputFile.Filename);
								}
							}
						}
					}
				}

				SetProgressBar(100);
				MessageBox.Show($@"{inputVideoPath} 完成！", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

			});
		}

		private async void CheckRoomStatus(Rooms room, CancellationTokenSource tokenSource)
		{
			room.IsRecordTaskStarted = true;
			var lastStatus = false;
			await Task.Run(async () =>
			{
				while (!tokenSource.Token.IsCancellationRequested)
				{
					await room.Refresh().ContinueWith(task =>
					{
						if (!room.IsRecording && room.IsLive)
						{
							Logging.Info($@"{room.RealRoomID}:Live");
							if (!lastStatus && NotifyCheckBox.Checked)
							{
								notifyIcon1.ShowBalloonTip(0, room.Title, $@"{room.AnchorName} 开播了！", ToolTipIcon.Info);
							}
							RecordTask(room, tokenSource).ContinueWith(task2 =>
							{
								room.IsRecording = false;
							}).Wait();
						}
						else
						{
							Debug.WriteLine($@"{room.RealRoomID}:No live...Wait {Interval} ms");
							Task.Delay(Interval).Wait();
						}

						lastStatus = room.IsLive;
					});
				}
			});
		}

		private async Task RecordTask(Rooms room, CancellationTokenSource cts)
		{
			var rootPath = RecordDirectory.Text;
			var n = GetStreamUrlIndex();//0,1,2,3
			var httpWay = RecordWay2.Checked;
			var isConvert2Mp4 = AutoConvert.Checked;

			var dir = Path.Combine(rootPath, $@"{room.AnchorName}_{room.RealRoomID}");
			if (!Directory.Exists(dir))
			{
				var dirInfo = Directory.CreateDirectory(dir);
				if (!dirInfo.Exists)
				{
					throw new Exception($@"{room.AnchorName}_{room.RealRoomID}:存储目录创建失败");
				}
			}

			var iEnumerableUrls = await room.GetLiveUrl();
			var urls = iEnumerableUrls.ToArray();
			if (urls.Length == 0)
			{
				throw new Exception($@"{room.AnchorName}_{room.RealRoomID}:直播流获取失败");
			}

			if (n >= urls.Length)
			{
				n = 0;
			}

			var url = urls[n];
			Logging.Info($@"{room.AnchorName}_{room.RealRoomID}:{url}");

			try
			{
				var isConnected = await room.TestHttpOk(url);
				if (!isConnected)
				{
					Logging.Error($@"{room.AnchorName}_{room.RealRoomID}:直播流错误...");
					return;
				}
			}
			catch (TaskCanceledException)
			{
				Logging.Error($@"{room.AnchorName}_{room.RealRoomID}:直播流检测超时...");
				return;
			}

			Logging.Info($@"{room.AnchorName}_{room.RealRoomID}:录制开始");

			var path = Path.Combine(dir, $@"{DateTime.Now:yyyyMMdd_HHmmss}_{room.Title}.flv");

			room.IsRecording = true;
			if (httpWay)
			{
				await MyTask.HttpDownLoadRecordTask(url, path, cts);
			}
			else
			{
				await MyTask.FFmpegRecordTask(url, path, cts);
			}

			Logging.Info($@"{room.RealRoomID}:录制结束=>{path}");

			if (File.Exists(path))
			{
				var size = Util.GetFileSize(path);
				if (size < RecordFileMinSize)
				{
					File.Delete(path);
					Logging.Info($@"{room.RealRoomID}:因文件过小删除：{path}");
				}
				else if (isConvert2Mp4)
				{
					Logging.Info($@"{room.RealRoomID}:开始转封装：{path}");
					// 异步执行
					SplitTaskFile(path, OutputVideoPath.Text, GetVideoConvertConfig(), false).Start();
				}
			}
		}

		#endregion

		#region DataForRecord

		private readonly BindingCollection<Rooms> Table = new BindingCollection<Rooms>();
		private readonly ConcurrentList<Rooms> _table = new ConcurrentList<Rooms>();
		private readonly Dictionary<long, CancellationTokenSource> _recordTasks = new Dictionary<long, CancellationTokenSource>();

		#endregion

		#region 录播

		private void NewRoomId_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				button3.PerformClick();
				e.SuppressKeyPress = true;
			}
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (long.TryParse(NewRoomId.Text, out var roomId))
			{
				AddRoom(roomId, true);
			}
			else
			{
				MessageBox.Show(@"房间号错误", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private int GetStreamUrlIndex()
		{
			if (radioButton1.Checked)
			{
				return 0;
			}

			if (radioButton2.Checked)
			{
				return 1;
			}

			if (radioButton3.Checked)
			{
				return 2;
			}

			if (radioButton4.Checked)
			{
				return 3;
			}

			return 0;
		}

		private void AddRoom(long roomId, bool isSave = false)
		{
			var room = new Rooms(roomId);
			room.Refresh().ContinueWith(task =>
			{
				if (task.IsFaulted)
				{
					MessageBox.Show(room.Message, @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				MainList.Invoke(new Action(() =>
				{
					if (_table.All(x => x.RealRoomID != room.RealRoomID))
					{
						_table.Add(room);
						Table.Add(room);

						AddCheckRoomStatusTask(room);

						if (isSave)
						{
							SaveConfig();
						}

						NewRoomId.Clear();
					}
					else
					{
						MessageBox.Show($@"已添加房间 {room.RealRoomID}", @"提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}));
			});
		}

		private void AddCheckRoomStatusTask(Rooms room)
		{
			Logging.Info($@"Add room {room.RealRoomID} for recording");
			var cts = new CancellationTokenSource();
			cts.Token.Register(() => { room.IsRecordTaskStarted = false; });
			_recordTasks.Add(room.RealRoomID, cts);
			CheckRoomStatus(room, cts);
		}

		#endregion

		#region 直播间控制

		//停止所有录制
		private void button4_Click(object sender, EventArgs e)
		{
			StopAllRecordTasks();
		}

		//开始所有录制
		private void button5_Click(object sender, EventArgs e)
		{
			StartAllRecordTasks();
		}

		//开始某房间录制
		private void button6_Click(object sender, EventArgs e)
		{
			var roomId = GetSelectedRoomId();
			if (roomId > 0 && !_recordTasks.ContainsKey(roomId))
			{
				foreach (var room in _table)
				{
					if (room.RealRoomID == roomId)
					{
						AddCheckRoomStatusTask(room);
					}
				}
			}
		}

		//停止某房间录制
		private void button7_Click(object sender, EventArgs e)
		{
			var roomId = GetSelectedRoomId();
			if (roomId > 0 && _recordTasks.TryGetValue(roomId, out var cts))
			{
				cts.Cancel();
				_recordTasks.Remove(roomId);
			}
		}

		//移除某房间
		private void button8_Click(object sender, EventArgs e)
		{
			var roomId = GetSelectedRoomId();
			if (roomId > 0)
			{
				var dr = MessageBox.Show($@"不再录制 {roomId} ?", @"询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if (dr == DialogResult.OK)
				{
					RemoveRoom(roomId);
				}
			}
		}

		private void StopAllRecordTasks()
		{
			foreach (var task in _recordTasks)
			{
				if (!task.Value.IsCancellationRequested)
				{
					task.Value.Cancel();
				}
			}

			_recordTasks.Clear();
		}

		private void StartAllRecordTasks()
		{
			foreach (var room in _table)
			{
				if (!_recordTasks.ContainsKey(room.RealRoomID))
				{
					AddCheckRoomStatusTask(room);
				}
			}
		}

		private long GetSelectedRoomId()
		{
			var rows = MainList.SelectedRows;
			if (rows.Count == 1)
			{
				if (long.TryParse(rows[0].Cells[0].Value.ToString(), out var roomId))
				{
					return roomId;
				}
			}
			return 0;
		}

		private void RemoveRoom(long roomId)
		{
			foreach (var x in _table)
			{
				if (x.RealRoomID == roomId)
				{
					_table.Remove(x);
					Table.Remove(x);
					if (_recordTasks.TryGetValue(x.RealRoomID, out var cts))
					{
						cts.Cancel();
						_recordTasks.Remove(x.RealRoomID);
					}
					SaveConfig();
					break;
				}
			}
		}

		#endregion

		private void Timer1_Tick(object sender, EventArgs e)
		{
			var (availableFreeSpace, totalSize) = Util.GetDiskUsage(RecordDirectory.Text);
			if (totalSize != 0)
			{
				DiskUsage.CustomText = $@"已使用 {Util.CountSize(totalSize - availableFreeSpace)}/{Util.CountSize(totalSize)} 剩余 {Util.CountSize(availableFreeSpace)}";
				var percentage = (totalSize - availableFreeSpace) / (double)totalSize;
				DiskUsage.Value = Convert.ToInt32(percentage * DiskUsage.Maximum);
				DiskUsage.ForeColor = percentage >= 0.9 ? Color.Red : SystemColors.Highlight;
			}
			else
			{
				DiskUsage.CustomText = string.Empty;
				DiskUsage.Value = 0;
			}
		}
	}
}
