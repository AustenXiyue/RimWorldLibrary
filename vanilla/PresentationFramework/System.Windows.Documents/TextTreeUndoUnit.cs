using MS.Internal;
using MS.Internal.Documents;

namespace System.Windows.Documents;

internal abstract class TextTreeUndoUnit : IUndoUnit
{
	private readonly TextContainer _tree;

	private readonly int _symbolOffset;

	private int _treeContentHashCode;

	protected TextContainer TextContainer => _tree;

	protected int SymbolOffset => _symbolOffset;

	internal TextTreeUndoUnit(TextContainer tree, int symbolOffset)
	{
		_tree = tree;
		_symbolOffset = symbolOffset;
		_treeContentHashCode = _tree.GetContentHashCode();
	}

	public void Do()
	{
		_tree.BeginChange();
		try
		{
			DoCore();
		}
		finally
		{
			_tree.EndChange();
		}
	}

	public abstract void DoCore();

	public bool Merge(IUndoUnit unit)
	{
		Invariant.Assert(unit != null);
		return false;
	}

	internal void SetTreeHashCode()
	{
		_treeContentHashCode = _tree.GetContentHashCode();
	}

	internal void VerifyTreeContentHashCode()
	{
		if (_tree.GetContentHashCode() != _treeContentHashCode)
		{
			Invariant.Assert(condition: false, "Undo unit is out of sync with TextContainer!");
		}
	}

	internal static PropertyRecord[] GetPropertyRecordArray(DependencyObject d)
	{
		LocalValueEnumerator localValueEnumerator = d.GetLocalValueEnumerator();
		PropertyRecord[] array = new PropertyRecord[localValueEnumerator.Count];
		int num = 0;
		localValueEnumerator.Reset();
		while (localValueEnumerator.MoveNext())
		{
			DependencyProperty property = localValueEnumerator.Current.Property;
			if (!property.ReadOnly)
			{
				array[num].Property = property;
				array[num].Value = d.GetValue(property);
				num++;
			}
		}
		PropertyRecord[] array2;
		if (localValueEnumerator.Count != num)
		{
			array2 = new PropertyRecord[num];
			for (int i = 0; i < num; i++)
			{
				array2[i] = array[i];
			}
		}
		else
		{
			array2 = array;
		}
		return array2;
	}

	internal static LocalValueEnumerator ArrayToLocalValueEnumerator(PropertyRecord[] records)
	{
		DependencyObject dependencyObject = new DependencyObject();
		for (int i = 0; i < records.Length; i++)
		{
			dependencyObject.SetValue(records[i].Property, records[i].Value);
		}
		return dependencyObject.GetLocalValueEnumerator();
	}
}
