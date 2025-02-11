using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

namespace MS.Internal.Data;

internal sealed class DifferencingCollection : ObservableCollection<object>
{
	private enum Change
	{
		None,
		Add,
		Remove,
		Move,
		Replace,
		Reset
	}

	private static object Unset = new object();

	internal DifferencingCollection(IEnumerable enumerable)
	{
		LoadItems(enumerable);
	}

	internal void Update(IEnumerable enumerable)
	{
		IList<object> list = base.Items;
		int num = -1;
		int newIndex = -1;
		int count = list.Count;
		Change change = Change.None;
		int num2 = 0;
		object obj = Unset;
		foreach (object item in enumerable)
		{
			if (num2 < count && ItemsControl.EqualsEx(item, list[num2]))
			{
				num2++;
				continue;
			}
			switch (change)
			{
			case Change.None:
				if (num2 + 1 < count && ItemsControl.EqualsEx(item, list[num2 + 1]))
				{
					change = Change.Remove;
					num = num2;
					obj = list[num2];
					num2 += 2;
				}
				else
				{
					change = Change.Add;
					num = num2;
					obj = item;
				}
				break;
			case Change.Add:
				if (num2 + 1 < count && ItemsControl.EqualsEx(item, list[num2 + 1]))
				{
					if (!ItemsControl.EqualsEx(obj, list[num2]))
					{
						change = ((num2 >= count || num2 != num) ? Change.Reset : Change.Replace);
					}
					else
					{
						change = Change.Move;
						newIndex = num;
						num = num2;
					}
					num2 += 2;
				}
				else
				{
					change = Change.Reset;
				}
				break;
			case Change.Remove:
				if (ItemsControl.EqualsEx(item, obj))
				{
					change = Change.Move;
					newIndex = num2 - 1;
				}
				else
				{
					change = Change.Reset;
				}
				break;
			default:
				change = Change.Reset;
				break;
			}
			if (change == Change.Reset)
			{
				break;
			}
		}
		if (num2 == count - 1)
		{
			switch (change)
			{
			case Change.None:
				change = Change.Remove;
				num = num2;
				break;
			case Change.Add:
				if (ItemsControl.EqualsEx(obj, list[num2]))
				{
					change = Change.Move;
					newIndex = num;
					num = num2;
				}
				else
				{
					change = ((num != count - 1) ? Change.Reset : Change.Replace);
				}
				break;
			default:
				change = Change.Reset;
				break;
			}
		}
		else if (num2 != count)
		{
			change = Change.Reset;
		}
		switch (change)
		{
		case Change.Add:
			Invariant.Assert(obj != Unset);
			Insert(num, obj);
			break;
		case Change.Remove:
			RemoveAt(num);
			break;
		case Change.Move:
			Move(num, newIndex);
			break;
		case Change.Replace:
			Invariant.Assert(obj != Unset);
			base[num] = obj;
			break;
		case Change.Reset:
			Reload(enumerable);
			break;
		case Change.None:
			break;
		}
	}

	private void LoadItems(IEnumerable enumerable)
	{
		foreach (object item in enumerable)
		{
			base.Items.Add(item);
		}
	}

	private void Reload(IEnumerable enumerable)
	{
		base.Items.Clear();
		LoadItems(enumerable);
		OnPropertyChanged(new PropertyChangedEventArgs("Count"));
		OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}
}
