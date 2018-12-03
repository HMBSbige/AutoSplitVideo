using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AutoSplitVideo.Collections
{
	/// <inheritdoc />
	/// <summary>
	/// 提供支持数据绑定的泛型集合
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class BindingCollection<T> : BindingList<T>
	{
		private bool _isSortedCore = true;
		private ListSortDirection _sortDirectionCore = ListSortDirection.Ascending;
		private PropertyDescriptor _sortPropertyCore = null;

		/// <inheritdoc />
		/// <summary>
		/// 是否已排序
		/// </summary>
		protected override bool IsSortedCore => _isSortedCore;

		/// <inheritdoc />
		/// <summary>
		/// 获取列表的排序方向
		/// </summary>
		protected override ListSortDirection SortDirectionCore => _sortDirectionCore;

		/// <inheritdoc />
		/// <summary>
		/// 获取用于对列表排序的属性说明符
		/// </summary>
		protected override PropertyDescriptor SortPropertyCore => _sortPropertyCore;

		/// <inheritdoc />
		/// <summary>
		/// 是否支持排序
		/// </summary>
		protected override bool SupportsSortingCore => true;

		/// <inheritdoc />
		/// <summary>
		/// 是否支持搜索
		/// </summary>
		protected override bool SupportsSearchingCore => true;

		/// <inheritdoc />
		/// <summary>
		/// 构造函数
		/// </summary>
		public BindingCollection()
		{ }

		/// <inheritdoc />
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="list"></param>
		public BindingCollection(IList<T> list)
			: base(list)
		{ }

		/// <inheritdoc />
		/// <summary>
		/// 对项排序
		/// </summary>
		/// <param name="property"></param>
		/// <param name="direction"></param>
		protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
		{
			if (Items is List<T> items)
			{
				var pc = new ObjectPropertyCompare<T>(property, direction);
				items.Sort(pc);
				_isSortedCore = true;
				_sortDirectionCore = direction;
				_sortPropertyCore = property;
			}
			else
			{
				_isSortedCore = false;
			}

			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}

		/// <inheritdoc />
		/// <summary>
		/// 移除排序
		/// </summary>
		protected override void RemoveSortCore()
		{
			_isSortedCore = false;
			OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
		}
	}

	public class ObjectPropertyCompare<T> : IComparer<T>
	{
		/// <summary>
		/// 属性
		/// </summary>
		public PropertyDescriptor Property { get; set; }
		/// <summary>
		/// 排序方向
		/// </summary>
		public ListSortDirection Direction { get; set; }

		/// <summary>
		/// 构造函数
		/// </summary>
		public ObjectPropertyCompare()
		{

		}

		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="property"></param>
		/// <param name="direction"></param>
		public ObjectPropertyCompare(PropertyDescriptor property, ListSortDirection direction)
		{
			Property = property;
			Direction = direction;
		}

		/// <summary>
		/// 比较两个对象
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public int Compare(T x, T y)
		{
			var xValue = x.GetType().GetProperty(Property.Name).GetValue(x, null);
			var yValue = y.GetType().GetProperty(Property.Name).GetValue(y, null);

			int returnValue;

			if (xValue == null && yValue == null)
			{
				returnValue = 0;
			}
			else if (xValue == null)
			{
				returnValue = -1;
			}
			else if (yValue == null)
			{
				returnValue = 1;
			}
			else if (xValue is IComparable comparable)
			{
				returnValue = comparable.CompareTo(yValue);
			}
			else if (xValue.Equals(yValue))
			{
				returnValue = 0;
			}
			else
			{
				returnValue = string.Compare(xValue.ToString(), yValue.ToString(), StringComparison.Ordinal);
			}

			if (Direction == ListSortDirection.Ascending)
			{
				return returnValue;
			}
			return returnValue * -1;
		}
	}
}
