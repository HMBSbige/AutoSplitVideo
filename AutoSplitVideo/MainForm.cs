using AutoSplitVideo.Collections;
using AutoSplitVideo.Controls;
using AutoSplitVideo.Model;
using AutoSplitVideo.Properties;
using AutoSplitVideo.Utils;
using AutoSplitVideo.ViewModel;
using MediaToolkit;
using MediaToolkit.Model;
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
			Icon = Resources.Asaki;
			notifyIcon1.Icon = Icon;
			_config.Load();
		}

		private static string ExeName => Assembly.GetExecutingAssembly().GetName().Name;
		private readonly AppConfig _config = new AppConfig($@".\{ExeName}.cfg");

		private FormWindowState DefaultState = FormWindowState.Normal;

		private TimeSpan Duration => TimeSpan.FromMinutes(Convert.ToDouble(numericUpDown2.Value));
		private long Limit => Convert.ToInt64(numericUpDown1.Value * 1024 * 1024 * 8);

		private const int Interval = 15 * 1000;

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

			LoadMainList();
		}

		private void LoadConfig()
		{
			tabControl1.SelectedIndex = _config.TableIndex;
			RecordDirectory.Text = _config.OutputPath;
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
			var runTask = SplitTask(InputVideoPath.Text, OutputVideoPath.Text, checkBox1.Checked);
			runTask.Start();
			runTask.ContinueWith(task => { BeginInvoke(new Action(() => { SetControlEnable(true); })); });
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

		#region VideoInfo

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

		private void SaveConfig()
		{
			_config.TableIndex = tabControl1.SelectedIndex;
			_config.OutputPath = RecordDirectory.Text;
			_config.StreamUrlIndex = GetStreamUrlIndex();
			_config.Rooms = _table.Select(room => room.RealRoomID);
			_config.Save();
		}

		private void Exit()
		{
			SaveConfig();
			StopAllRecordTasks();
			Util.KillFFmpeg();
			Dispose();
			Environment.Exit(0);
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			TriggerMainFormDisplay();
			e.Cancel = true;
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

		private Task SplitTask(string inputVideoPath, string outputDirectoryPath, bool deleteMp4)
		{
			return new Task(() =>
			{
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
			});
		}

		private async void CheckRoomStatus(Rooms room, CancellationTokenSource tokenSource)
		{
			await Task.Run(async () =>
			{
				while (!tokenSource.Token.IsCancellationRequested)
				{
					await room.Refresh().ContinueWith(task =>
					{
						if (!room.IsRecording && room.IsLive)
						{
							room.IsRecording = true;
							RecordTask(room, tokenSource).ContinueWith(task2 =>
							{
								room.IsRecording = false;
							}).Wait();
						}
						else
						{
							Debug.WriteLine($@"No live...Wait {Interval} ms");
							Task.Delay(Interval).Wait();
						}
					});
				}
			});
		}

		private async Task RecordTask(Rooms room, CancellationTokenSource cts)
		{
			var rootPath = RecordDirectory.Text;
			var n = GetStreamUrlIndex();//0,1,2,3
			var dir = Path.Combine(rootPath, $@"{room.RealRoomID}");
			if (!Directory.Exists(dir))
			{
				var dirInfo = Directory.CreateDirectory(dir);
				if (!dirInfo.Exists)
				{
					throw new Exception();
				}
			}

			var iEnumerableUrls = await room.GetLiveUrl();
			var urls = iEnumerableUrls.ToArray();
			if (urls.Length == 0)
			{
				throw new Exception();
			}

			if (n >= urls.Length)
			{
				n = 0;
			}

			var url = urls[n];

			var path = Path.Combine(dir, $@"{DateTime.Now:yyyyMMdd_HHmmss}.flv");

			await Util.FFmpegRecordTask(url, path, cts);
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
				AddRoom(roomId);
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

		private void AddRoom(long roomId)
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
			Debug.WriteLine($@"Add room {room.RealRoomID} for recording");
			var cts = new CancellationTokenSource();
			_recordTasks.Add(room.RealRoomID, cts);
			CheckRoomStatus(room, cts);
		}

		private async void RefreshRooms()
		{
			await Task.Run(() =>
			{
				Parallel.ForEach(_table, async room =>
				{
					await room.Refresh();
				});
			});
		}

		#endregion

		#region 直播间控制

		private void button4_Click(object sender, EventArgs e)
		{
			StopAllRecordTasks();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			StartAllRecordTasks();
		}

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

		private void button7_Click(object sender, EventArgs e)
		{
			var roomId = GetSelectedRoomId();
			if (roomId > 0 && _recordTasks.TryGetValue(roomId, out var cts))
			{
				cts.Cancel();
				_recordTasks.Remove(roomId);
			}
		}

		private void button8_Click(object sender, EventArgs e)
		{
			var roomId = GetSelectedRoomId();
			if (roomId > 0)
			{
				RemoveRoom(roomId);
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
					break;
				}
			}
		}

		#endregion

	}
}
