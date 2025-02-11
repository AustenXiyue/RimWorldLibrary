using System.ComponentModel;
using System.Text;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>Describes how a <see cref="T:System.Windows.Media.Animation.Timeline" /> repeats its simple duration.</summary>
[TypeConverter(typeof(RepeatBehaviorConverter))]
public struct RepeatBehavior : IFormattable
{
	private enum RepeatBehaviorType
	{
		IterationCount,
		RepeatDuration,
		Forever
	}

	private double _iterationCount;

	private TimeSpan _repeatDuration;

	private RepeatBehaviorType _type;

	/// <summary>Gets a <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> that specifies an infinite number of repetitions.  </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> that specifies an infinite number of repetitions.   </returns>
	public static RepeatBehavior Forever
	{
		get
		{
			RepeatBehavior result = default(RepeatBehavior);
			result._type = RepeatBehaviorType.Forever;
			return result;
		}
	}

	/// <summary>Gets a value that indicates whether the repeat behavior has a specified iteration count.    </summary>
	/// <returns>true if the specified type refers to an iteration count; otherwise, false. </returns>
	public bool HasCount => _type == RepeatBehaviorType.IterationCount;

	/// <summary>Gets a value that indicates whether the repeat behavior has a specified repeat duration. </summary>
	/// <returns>true if the specified type refers to a repeat duration; otherwise, false.</returns>
	public bool HasDuration => _type == RepeatBehaviorType.RepeatDuration;

	/// <summary>Gets the number of times a <see cref="T:System.Windows.Media.Animation.Timeline" /> should repeat. </summary>
	/// <returns>The numeric value representing the number of iterations to repeat.</returns>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> describes a repeat duration, not an iteration count.</exception>
	public double Count
	{
		get
		{
			if (_type != 0)
			{
				throw new InvalidOperationException(SR.Format(SR.Timing_RepeatBehaviorNotIterationCount, this));
			}
			return _iterationCount;
		}
	}

	/// <summary>Gets the total length of time a <see cref="T:System.Windows.Media.Animation.Timeline" /> should play. </summary>
	/// <returns>The total length of time a timeline should play. </returns>
	/// <exception cref="T:System.InvalidOperationException">This <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> does not describe a repeat duration; it describes an iteration count.</exception>
	public TimeSpan Duration
	{
		get
		{
			if (_type != RepeatBehaviorType.RepeatDuration)
			{
				throw new InvalidOperationException(SR.Format(SR.Timing_RepeatBehaviorNotRepeatDuration, this));
			}
			return _repeatDuration;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> structure with the specified iteration count. </summary>
	/// <param name="count">A number greater than or equal to 0 that specifies the number of iterations to make. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="count" /> evaluates to infinity, a value that is not a number, or is negative.</exception>
	public RepeatBehavior(double count)
	{
		if (double.IsInfinity(count) || double.IsNaN(count) || count < 0.0)
		{
			throw new ArgumentOutOfRangeException("count", SR.Format(SR.Timing_RepeatBehaviorInvalidIterationCount, count));
		}
		_repeatDuration = new TimeSpan(0L);
		_iterationCount = count;
		_type = RepeatBehaviorType.IterationCount;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> structure with the specified repeat duration. </summary>
	/// <param name="duration">The total length of time that the <see cref="T:System.Windows.Media.Animation.Timeline" /> should play (its active duration). </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="duration" /> evaluates to a negative number.</exception>
	public RepeatBehavior(TimeSpan duration)
	{
		if (duration < new TimeSpan(0L))
		{
			throw new ArgumentOutOfRangeException("duration", SR.Format(SR.Timing_RepeatBehaviorInvalidRepeatDuration, duration));
		}
		_iterationCount = 0.0;
		_repeatDuration = duration;
		_type = RepeatBehaviorType.RepeatDuration;
	}

	/// <summary>Indicates whether this instance is equal to the specified object. </summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> that represents the same repeat behavior as this instance; otherwise, false.</returns>
	/// <param name="value">The object to compare with this instance.</param>
	public override bool Equals(object value)
	{
		if (value is RepeatBehavior)
		{
			return Equals((RepeatBehavior)value);
		}
		return false;
	}

	/// <summary>Returns a value that indicates whether this instance is equal to the specified <see cref="T:System.Windows.Media.Animation.RepeatBehavior" />. </summary>
	/// <returns>true if both the type and repeat behavior of <paramref name="repeatBehavior" /> are equal to this instance; otherwise, false.</returns>
	/// <param name="repeatBehavior">The value to compare to this instance.</param>
	public bool Equals(RepeatBehavior repeatBehavior)
	{
		if (_type == repeatBehavior._type)
		{
			return _type switch
			{
				RepeatBehaviorType.Forever => true, 
				RepeatBehaviorType.IterationCount => _iterationCount == repeatBehavior._iterationCount, 
				RepeatBehaviorType.RepeatDuration => _repeatDuration == repeatBehavior._repeatDuration, 
				_ => false, 
			};
		}
		return false;
	}

	/// <summary>Indicates whether the two specified <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> structures are equal. </summary>
	/// <returns>true if both the type and repeat behavior of <paramref name="repeatBehavior1" /> are equal to that of <paramref name="repeatBehavior2" />; otherwise, false.</returns>
	/// <param name="repeatBehavior1">The first value to compare.</param>
	/// <param name="repeatBehavior2">The second value to compare.</param>
	public static bool Equals(RepeatBehavior repeatBehavior1, RepeatBehavior repeatBehavior2)
	{
		return repeatBehavior1.Equals(repeatBehavior2);
	}

	/// <summary> Returns the hash code of this instance.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return _type switch
		{
			RepeatBehaviorType.IterationCount => _iterationCount.GetHashCode(), 
			RepeatBehaviorType.RepeatDuration => _repeatDuration.GetHashCode(), 
			RepeatBehaviorType.Forever => 2147483605, 
			_ => base.GetHashCode(), 
		};
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> instance. </summary>
	/// <returns>A string representation of this instance.</returns>
	public override string ToString()
	{
		return InternalToString(null, null);
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> instance with the specified format. </summary>
	/// <returns>A string representation of this instance.</returns>
	/// <param name="formatProvider">The format used to construct the return value.</param>
	public string ToString(IFormatProvider formatProvider)
	{
		return InternalToString(null, formatProvider);
	}

	/// <summary>Formats the value of the current instance using the specified format.</summary>
	/// <returns>The value of the current instance in the specified format.</returns>
	/// <param name="format">The format to use.-or- A null reference (Nothing in Visual Basic) to use the default format defined for the type of the <see cref="T:System.IFormattable" /> implementation. </param>
	/// <param name="formatProvider">The provider to use to format the value.-or- A null reference (Nothing in Visual Basic) to obtain the numeric format information from the current locale setting of the operating system. </param>
	string IFormattable.ToString(string format, IFormatProvider formatProvider)
	{
		return InternalToString(format, formatProvider);
	}

	internal string InternalToString(string format, IFormatProvider formatProvider)
	{
		switch (_type)
		{
		case RepeatBehaviorType.Forever:
			return "Forever";
		case RepeatBehaviorType.IterationCount:
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat(formatProvider, "{0:" + format + "}x", _iterationCount);
			return stringBuilder.ToString();
		}
		case RepeatBehaviorType.RepeatDuration:
			return _repeatDuration.ToString();
		default:
			return null;
		}
	}

	/// <summary>Indicates whether the two specified <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> instances are equal. </summary>
	/// <returns>true if both the type and repeat behavior of <paramref name="repeatBehavior1" /> are equal to that of <paramref name="repeatBehavior2" />; otherwise, false.</returns>
	/// <param name="repeatBehavior1">The first value to compare.</param>
	/// <param name="repeatBehavior2">The second value to compare.</param>
	public static bool operator ==(RepeatBehavior repeatBehavior1, RepeatBehavior repeatBehavior2)
	{
		return repeatBehavior1.Equals(repeatBehavior2);
	}

	/// <summary>Indicates whether the two <see cref="T:System.Windows.Media.Animation.RepeatBehavior" /> instances are not equal. </summary>
	/// <returns>true if <paramref name="repeatBehavior1" /> and <paramref name="repeatBehavior2" /> are different types or the repeat behavior properties are not equal; otherwise false.</returns>
	/// <param name="repeatBehavior1">The first value to compare.</param>
	/// <param name="repeatBehavior2">The second value to compare.</param>
	public static bool operator !=(RepeatBehavior repeatBehavior1, RepeatBehavior repeatBehavior2)
	{
		return !repeatBehavior1.Equals(repeatBehavior2);
	}
}
