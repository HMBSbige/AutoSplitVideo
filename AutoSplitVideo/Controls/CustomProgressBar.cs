using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace AutoSplitVideo.Controls
{
	public enum ProgressBarDisplayText
	{
		Percentage,
		CustomText
	}

	[ToolboxItem(true)]
	[DesignerCategory(@"Code")]
	public class CustomProgressBar : ProgressBar
	{
		//Property to set to decide whether to print a % or Text

		public ProgressBarDisplayText DisplayStyle { get; set; }

		public Color FontColor { get; set; }

		//Property to hold the custom text
		private string m_CustomText;

		public CustomProgressBar()
		{
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			FontColor = Color.Black;
		}

		public string CustomText
		{
			get => m_CustomText;
			set
			{
				m_CustomText = value;
				Invalidate();
			}
		}

		private const int WM_PAINT = 0x000F;

		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == WM_PAINT)
			{
				var m_Percent = Convert.ToInt32(Convert.ToDouble(Value) / Convert.ToDouble(Maximum) * 100);
				const TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.WordEllipsis;

				using (var g = Graphics.FromHwnd(Handle))
				{
					if (DisplayStyle == ProgressBarDisplayText.CustomText)
					{
						TextRenderer.DrawText(g, CustomText, Font, new Rectangle(0, 0, Width, Height), FontColor, flags);
					}
					else if (DisplayStyle == ProgressBarDisplayText.Percentage)
					{
						TextRenderer.DrawText(g, $@"{m_Percent}%", Font, new Rectangle(0, 0, Width, Height), FontColor, flags);
					}
				}
			}
		}

		#region Avoid flickering text
		//https://stackoverflow.com/a/299983

		[DllImport(@"uxtheme.dll")]
		private static extern int SetWindowTheme(IntPtr hWnd, string appname, string idlist);

		protected override void OnHandleCreated(EventArgs e)
		{
			SetWindowTheme(Handle, string.Empty, string.Empty);
			base.OnHandleCreated(e);
		}

		#endregion
	}
}
