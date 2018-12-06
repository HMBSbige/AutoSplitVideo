using System.ComponentModel;
using System.Windows.Forms;

namespace AutoSplitVideo.Controls
{
	[ToolboxItem(true)]
	[DesignerCategory(@"Code")]
	public class DoubleBufferedDataGridView : DataGridView
	{
		public DoubleBufferedDataGridView()
		{
			AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
			CellBorderStyle = DataGridViewCellBorderStyle.Single;
			BackgroundColor = DefaultCellStyle.BackColor;
			MultiSelect = false;
			StandardTab = true;
			SelectionMode = DataGridViewSelectionMode.FullRowSelect;
			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
			UpdateStyles();
		}
	}
}
