using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows.Threading;

internal struct PriorityRange
{
	public static readonly PriorityRange All = new PriorityRange(DispatcherPriority.Inactive, DispatcherPriority.Send, ignored: true);

	public static readonly PriorityRange None = new PriorityRange(DispatcherPriority.Invalid, DispatcherPriority.Invalid, ignored: true);

	private DispatcherPriority _min;

	private bool _isMinInclusive;

	private DispatcherPriority _max;

	private bool _isMaxInclusive;

	public DispatcherPriority Min => _min;

	public DispatcherPriority Max => _max;

	public bool IsMinInclusive => _isMinInclusive;

	public bool IsMaxInclusive => _isMaxInclusive;

	public bool IsValid
	{
		get
		{
			if (_min > DispatcherPriority.Invalid && _min <= DispatcherPriority.Send && _max > DispatcherPriority.Invalid)
			{
				return _max <= DispatcherPriority.Send;
			}
			return false;
		}
	}

	public PriorityRange(DispatcherPriority min, DispatcherPriority max)
	{
		this = default(PriorityRange);
		Initialize(min, isMinInclusive: true, max, isMaxInclusive: true);
	}

	public PriorityRange(DispatcherPriority min, bool isMinInclusive, DispatcherPriority max, bool isMaxInclusive)
	{
		this = default(PriorityRange);
		Initialize(min, isMinInclusive, max, isMaxInclusive);
	}

	public bool Contains(DispatcherPriority priority)
	{
		if (priority <= DispatcherPriority.Invalid || priority > DispatcherPriority.Send)
		{
			return false;
		}
		if (!IsValid)
		{
			return false;
		}
		bool flag = false;
		flag = ((!_isMinInclusive) ? (priority > _min) : (priority >= _min));
		if (flag)
		{
			flag = ((!_isMaxInclusive) ? (priority < _max) : (priority <= _max));
		}
		return flag;
	}

	public bool Contains(PriorityRange priorityRange)
	{
		if (!priorityRange.IsValid)
		{
			return false;
		}
		if (!IsValid)
		{
			return false;
		}
		bool flag = false;
		if (priorityRange._isMinInclusive)
		{
			flag = Contains(priorityRange.Min);
		}
		else if (priorityRange.Min >= _min && priorityRange.Min < _max)
		{
			flag = true;
		}
		if (flag)
		{
			if (priorityRange._isMaxInclusive)
			{
				flag = Contains(priorityRange.Max);
			}
			else if (priorityRange.Max > _min && priorityRange.Max <= _max)
			{
				flag = true;
			}
		}
		return flag;
	}

	public override bool Equals(object o)
	{
		if (o is PriorityRange)
		{
			return Equals((PriorityRange)o);
		}
		return false;
	}

	public bool Equals(PriorityRange priorityRange)
	{
		if (priorityRange._min == _min && priorityRange._isMinInclusive == _isMinInclusive && priorityRange._max == _max)
		{
			return priorityRange._isMaxInclusive == _isMaxInclusive;
		}
		return false;
	}

	public static bool operator ==(PriorityRange priorityRange1, PriorityRange priorityRange2)
	{
		return priorityRange1.Equals(priorityRange2);
	}

	public static bool operator !=(PriorityRange priorityRange1, PriorityRange priorityRange2)
	{
		return !(priorityRange1 == priorityRange2);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	private void Initialize(DispatcherPriority min, bool isMinInclusive, DispatcherPriority max, bool isMaxInclusive)
	{
		switch (min)
		{
		default:
			throw new InvalidEnumArgumentException("min", (int)min, typeof(DispatcherPriority));
		case DispatcherPriority.Inactive:
			throw new ArgumentException(SR.InvalidPriority, "min");
		case DispatcherPriority.Invalid:
		case DispatcherPriority.SystemIdle:
		case DispatcherPriority.ApplicationIdle:
		case DispatcherPriority.ContextIdle:
		case DispatcherPriority.Background:
		case DispatcherPriority.Input:
		case DispatcherPriority.Loaded:
		case DispatcherPriority.Render:
		case DispatcherPriority.DataBind:
		case DispatcherPriority.Normal:
		case DispatcherPriority.Send:
			switch (max)
			{
			default:
				throw new InvalidEnumArgumentException("max", (int)max, typeof(DispatcherPriority));
			case DispatcherPriority.Inactive:
				throw new ArgumentException(SR.InvalidPriority, "max");
			case DispatcherPriority.Invalid:
			case DispatcherPriority.SystemIdle:
			case DispatcherPriority.ApplicationIdle:
			case DispatcherPriority.ContextIdle:
			case DispatcherPriority.Background:
			case DispatcherPriority.Input:
			case DispatcherPriority.Loaded:
			case DispatcherPriority.Render:
			case DispatcherPriority.DataBind:
			case DispatcherPriority.Normal:
			case DispatcherPriority.Send:
				if (max < min)
				{
					throw new ArgumentException(SR.InvalidPriorityRangeOrder);
				}
				_min = min;
				_isMinInclusive = isMinInclusive;
				_max = max;
				_isMaxInclusive = isMaxInclusive;
				break;
			}
			break;
		}
	}

	private PriorityRange(DispatcherPriority min, DispatcherPriority max, bool ignored)
	{
		_min = min;
		_isMinInclusive = true;
		_max = max;
		_isMaxInclusive = true;
	}
}
