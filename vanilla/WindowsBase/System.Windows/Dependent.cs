namespace System.Windows;

internal struct Dependent
{
	private DependencyProperty _DP;

	private WeakReference _wrDO;

	private WeakReference _wrEX;

	public DependencyObject DO
	{
		get
		{
			if (_wrDO == null)
			{
				return null;
			}
			return (DependencyObject)_wrDO.Target;
		}
	}

	public DependencyProperty DP => _DP;

	public Expression Expr
	{
		get
		{
			if (_wrEX == null)
			{
				return null;
			}
			return (Expression)_wrEX.Target;
		}
	}

	public bool IsValid()
	{
		if (!_wrEX.IsAlive)
		{
			return false;
		}
		if (_wrDO != null && !_wrDO.IsAlive)
		{
			return false;
		}
		return true;
	}

	public Dependent(DependencyObject o, DependencyProperty p, Expression e)
	{
		_wrEX = ((e == null) ? null : new WeakReference(e));
		_DP = p;
		_wrDO = ((o == null) ? null : new WeakReference(o));
	}

	public override bool Equals(object o)
	{
		if (!(o is Dependent dependent))
		{
			return false;
		}
		if (!IsValid() || !dependent.IsValid())
		{
			return false;
		}
		if (_wrEX.Target != dependent._wrEX.Target)
		{
			return false;
		}
		if (_DP != dependent._DP)
		{
			return false;
		}
		if (_wrDO != null && dependent._wrDO != null)
		{
			if (_wrDO.Target != dependent._wrDO.Target)
			{
				return false;
			}
		}
		else if (_wrDO != null || dependent._wrDO != null)
		{
			return false;
		}
		return true;
	}

	public static bool operator ==(Dependent first, Dependent second)
	{
		return first.Equals(second);
	}

	public static bool operator !=(Dependent first, Dependent second)
	{
		return !first.Equals(second);
	}

	public override int GetHashCode()
	{
		int num = ((Expression)_wrEX.Target)?.GetHashCode() ?? 0;
		if (_wrDO != null)
		{
			num += ((DependencyObject)_wrDO.Target)?.GetHashCode() ?? 0;
		}
		return num + ((_DP != null) ? _DP.GetHashCode() : 0);
	}
}
