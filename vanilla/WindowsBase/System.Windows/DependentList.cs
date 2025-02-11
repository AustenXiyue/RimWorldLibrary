using MS.Utility;

namespace System.Windows;

internal class DependentList : FrugalObjectList<Dependent>
{
	public bool IsEmpty
	{
		get
		{
			for (int num = base.Count - 1; num >= 0; num--)
			{
				if (base[num].IsValid())
				{
					return false;
				}
			}
			return true;
		}
	}

	public void Add(DependencyObject d, DependencyProperty dp, Expression expr)
	{
		if (base.Count == base.Capacity)
		{
			CleanUpDeadWeakReferences();
		}
		Dependent value = new Dependent(d, dp, expr);
		Add(value);
	}

	public void Remove(DependencyObject d, DependencyProperty dp, Expression expr)
	{
		Dependent value = new Dependent(d, dp, expr);
		Remove(value);
	}

	public void InvalidateDependents(DependencyObject source, DependencyPropertyChangedEventArgs sourceArgs)
	{
		Dependent[] array = ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			Expression expr = array[i].Expr;
			if (expr == null)
			{
				continue;
			}
			expr.OnPropertyInvalidation(source, sourceArgs);
			if (!expr.ForwardsInvalidations)
			{
				DependencyObject dO = array[i].DO;
				DependencyProperty dP = array[i].DP;
				if (dO != null && dP != null)
				{
					dO.InvalidateProperty(dP);
				}
			}
		}
	}

	private void CleanUpDeadWeakReferences()
	{
		int num = 0;
		for (int num2 = base.Count - 1; num2 >= 0; num2--)
		{
			if (base[num2].IsValid())
			{
				num++;
			}
		}
		if (num == base.Count)
		{
			return;
		}
		Compacter compacter = new Compacter(this, num);
		int start = 0;
		bool flag = false;
		int i = 0;
		for (int count = base.Count; i < count; i++)
		{
			if (flag != base[i].IsValid())
			{
				if (flag)
				{
					compacter.Include(start, i);
				}
				start = i;
				flag = !flag;
			}
		}
		if (flag)
		{
			compacter.Include(start, base.Count);
		}
		compacter.Finish();
	}
}
