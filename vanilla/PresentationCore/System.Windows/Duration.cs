using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows;

/// <summary>Represents the duration of time that a <see cref="T:System.Windows.Media.Animation.Timeline" /> is active.</summary>
[TypeConverter(typeof(DurationConverter))]
public struct Duration
{
	private enum DurationType
	{
		Automatic,
		TimeSpan,
		Forever
	}

	private TimeSpan _timeSpan;

	private DurationType _durationType;

	/// <summary>Gets a value that indicates if this <see cref="T:System.Windows.Duration" /> represents a <see cref="T:System.TimeSpan" /> value.</summary>
	/// <returns>True if this Duration is a <see cref="T:System.TimeSpan" /> value; otherwise, false.</returns>
	public bool HasTimeSpan => _durationType == DurationType.TimeSpan;

	/// <summary>Gets a <see cref="T:System.Windows.Duration" /> value that is automatically determined.</summary>
	/// <returns>A <see cref="T:System.Windows.Duration" /> initialized to an automatic value.</returns>
	public static Duration Automatic
	{
		get
		{
			Duration result = default(Duration);
			result._durationType = DurationType.Automatic;
			return result;
		}
	}

	/// <summary>Gets a <see cref="T:System.Windows.Duration" /> value that represents an infinite interval.</summary>
	/// <returns>A <see cref="T:System.Windows.Duration" /> initialized to a forever value.</returns>
	public static Duration Forever
	{
		get
		{
			Duration result = default(Duration);
			result._durationType = DurationType.Forever;
			return result;
		}
	}

	/// <summary>Gets the <see cref="T:System.TimeSpan" /> value that this <see cref="T:System.Windows.Duration" /> represents.</summary>
	/// <returns>The <see cref="T:System.TimeSpan" /> value that this <see cref="T:System.Windows.Duration" /> represents.</returns>
	/// <exception cref="T:System.InvalidOperationException">Occurs if <see cref="T:System.Windows.Duration" /> is null.</exception>
	public TimeSpan TimeSpan
	{
		get
		{
			if (HasTimeSpan)
			{
				return _timeSpan;
			}
			throw new InvalidOperationException(SR.Format(SR.Timing_NotTimeSpan, this));
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Duration" /> structure with the supplied <see cref="T:System.TimeSpan" /> value.</summary>
	/// <param name="timeSpan">Represents the initial time interval of this duration.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <paramref name="timeSpan" /> is initialized to a negative value.</exception>
	public Duration(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero)
		{
			throw new ArgumentException(SR.Timing_InvalidArgNonNegative, "timeSpan");
		}
		_durationType = DurationType.TimeSpan;
		_timeSpan = timeSpan;
	}

	/// <summary>Implicitly creates a <see cref="T:System.Windows.Duration" /> from a given <see cref="T:System.TimeSpan" />.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Duration" />.</returns>
	/// <param name="timeSpan">
	///   <see cref="T:System.TimeSpan" /> from which an instance of <see cref="T:System.Windows.Duration" /> is implicitly created.</param>
	/// <exception cref="T:System.ArgumentException">Occurs when <see cref="T:System.TimeSpan" /> is negative.</exception>
	public static implicit operator Duration(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero)
		{
			throw new ArgumentException(SR.Timing_InvalidArgNonNegative, "timeSpan");
		}
		return new Duration(timeSpan);
	}

	/// <summary>Adds two instances of <see cref="T:System.Windows.Duration" /> together.</summary>
	/// <returns>If both instances of <see cref="T:System.Windows.Duration" /> have <see cref="T:System.TimeSpan" /> values, this method returns the sum of those two values. If either value is set to <see cref="P:System.Windows.Duration.Automatic" />, the method returns <see cref="P:System.Windows.Duration.Automatic" />. If either value is set to <see cref="P:System.Windows.Duration.Forever" />, the method returns <see cref="P:System.Windows.Duration.Forever" />.If either <paramref name="t1" /> or <paramref name="t2" /> has no value, this method returns null.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to add.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to add.</param>
	public static Duration operator +(Duration t1, Duration t2)
	{
		if (t1.HasTimeSpan && t2.HasTimeSpan)
		{
			return new Duration(t1._timeSpan + t2._timeSpan);
		}
		if (t1._durationType != 0 && t2._durationType != 0)
		{
			return Forever;
		}
		return Automatic;
	}

	/// <summary>Subtracts the value of one instance of <see cref="T:System.Windows.Duration" /> from another.</summary>
	/// <returns>If both instances of <see cref="T:System.Windows.Duration" /> have values, an instance of <see cref="T:System.Windows.Duration" /> that represents the value of <paramref name="t1" /> minus <paramref name="t2" />. If <paramref name="t1" /> has a value of <see cref="P:System.Windows.Duration.Forever" /> and <paramref name="t2" /> has a value of <see cref="P:System.Windows.Duration.TimeSpan" />, this method returns <see cref="P:System.Windows.Duration.Forever" />. Otherwise this method returns null.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" />.</param>
	/// <param name="t2">The instance of <see cref="T:System.Windows.Duration" /> to subtract.</param>
	public static Duration operator -(Duration t1, Duration t2)
	{
		if (t1.HasTimeSpan && t2.HasTimeSpan)
		{
			return new Duration(t1._timeSpan - t2._timeSpan);
		}
		if (t1._durationType == DurationType.Forever && t2.HasTimeSpan)
		{
			return Forever;
		}
		return Automatic;
	}

	/// <summary>Determines whether two instances of <see cref="T:System.Windows.Duration" /> are equal.</summary>
	/// <returns>true if both instances of <see cref="T:System.Windows.Duration" /> have values and are equal, or if both instances of <see cref="T:System.Windows.Duration" /> are null. Otherwise, this method returns false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator ==(Duration t1, Duration t2)
	{
		return t1.Equals(t2);
	}

	/// <summary>Determines if two instances of <see cref="T:System.Windows.Duration" /> are not equal.</summary>
	/// <returns>true if exactly one of <paramref name="t1" /> or <paramref name="t2" /> represent a value, or if they both represent values that are not equal; otherwise, false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator !=(Duration t1, Duration t2)
	{
		return !t1.Equals(t2);
	}

	/// <summary>Determines if one instance of <see cref="T:System.Windows.Duration" /> is greater than another.</summary>
	/// <returns>true if both <paramref name="t1" /> and <paramref name="t2" /> have values and <paramref name="t1" /> is greater than <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator >(Duration t1, Duration t2)
	{
		if (t1.HasTimeSpan && t2.HasTimeSpan)
		{
			return t1._timeSpan > t2._timeSpan;
		}
		if (t1.HasTimeSpan && t2._durationType == DurationType.Forever)
		{
			return false;
		}
		if (t1._durationType == DurationType.Forever && t2.HasTimeSpan)
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether an instance of <see cref="T:System.Windows.Duration" /> is greater than or equal to another instance.</summary>
	/// <returns>true if both <paramref name="t1" /> and <paramref name="t2" /> have values and <paramref name="t1" /> is greater than or equal to <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator >=(Duration t1, Duration t2)
	{
		if (t1._durationType == DurationType.Automatic && t2._durationType == DurationType.Automatic)
		{
			return true;
		}
		if (t1._durationType == DurationType.Automatic || t2._durationType == DurationType.Automatic)
		{
			return false;
		}
		return !(t1 < t2);
	}

	/// <summary>Determines if the value of one instance of <see cref="T:System.Windows.Duration" /> is less than the value of another instance.</summary>
	/// <returns>true if both <paramref name="t1" /> and <paramref name="t2" /> have values and <paramref name="t1" /> is less than <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator <(Duration t1, Duration t2)
	{
		if (t1.HasTimeSpan && t2.HasTimeSpan)
		{
			return t1._timeSpan < t2._timeSpan;
		}
		if (t1.HasTimeSpan && t2._durationType == DurationType.Forever)
		{
			return true;
		}
		if (t1._durationType == DurationType.Forever)
		{
			_ = t2.HasTimeSpan;
			return false;
		}
		return false;
	}

	/// <summary>Determines if the value of one instance of <see cref="T:System.Windows.Duration" /> is less than or equal to the value of another instance.</summary>
	/// <returns>true if both <paramref name="t1" /> and <paramref name="t2" /> have values and <paramref name="t1" /> is less than or equal to <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool operator <=(Duration t1, Duration t2)
	{
		if (t1._durationType == DurationType.Automatic && t2._durationType == DurationType.Automatic)
		{
			return true;
		}
		if (t1._durationType == DurationType.Automatic || t2._durationType == DurationType.Automatic)
		{
			return false;
		}
		return !(t1 > t2);
	}

	/// <summary>Compares one <see cref="T:System.Windows.Duration" /> value to another.</summary>
	/// <returns>If <paramref name="t1" /> is less than <paramref name="t2" />, a negative value that represents the difference. If <paramref name="t1" /> is equal to <paramref name="t2" />, zero. If <paramref name="t1" /> is greater than <paramref name="t2" />, a positive value that represents the difference.</returns>
	/// <param name="t1">The first instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">The second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static int Compare(Duration t1, Duration t2)
	{
		if (t1._durationType == DurationType.Automatic)
		{
			if (t2._durationType == DurationType.Automatic)
			{
				return 0;
			}
			return -1;
		}
		if (t2._durationType == DurationType.Automatic)
		{
			return 1;
		}
		if (t1 < t2)
		{
			return -1;
		}
		if (t1 > t2)
		{
			return 1;
		}
		return 0;
	}

	/// <summary>Returns the specified instance of <see cref="T:System.Windows.Duration" />.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Duration" />.</returns>
	/// <param name="duration">The instance of <see cref="T:System.Windows.Duration" /> to get.</param>
	public static Duration Plus(Duration duration)
	{
		return duration;
	}

	/// <summary>Returns the specified instance of <see cref="T:System.Windows.Duration" />.</summary>
	/// <returns>An instance of <see cref="T:System.Windows.Duration" />.</returns>
	/// <param name="duration">The instance of <see cref="T:System.Windows.Duration" /> to get.</param>
	public static Duration operator +(Duration duration)
	{
		return duration;
	}

	/// <summary>Adds the value of the specified instance of <see cref="T:System.Windows.Duration" /> to the value of the current instance.</summary>
	/// <returns>If both instances of <see cref="T:System.Windows.Duration" /> have values, an instance of <see cref="T:System.Windows.Duration" /> that represents the combined values. Otherwise this method returns null.</returns>
	/// <param name="duration">An instance of <see cref="T:System.Windows.Duration" /> that represents the value of the current instance plus <paramref name="duration" />.</param>
	public Duration Add(Duration duration)
	{
		return this + duration;
	}

	/// <summary>Determines whether a specified object is equal to an instance of <see cref="T:System.Windows.Duration" />.</summary>
	/// <returns>true if value is equal to the current instance of Duration; otherwise, false.</returns>
	/// <param name="value">Object to check for equality.</param>
	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		if (value is Duration)
		{
			return Equals((Duration)value);
		}
		return false;
	}

	/// <summary>Determines whether a specified <see cref="T:System.Windows.Duration" /> is equal to this instance of <see cref="T:System.Windows.Duration" />.</summary>
	/// <returns>true if <paramref name="duration" /> is equal to the current instance of <see cref="T:System.Windows.Duration" />; otherwise, false.</returns>
	/// <param name="duration">Instance of <see cref="T:System.Windows.Duration" /> to check for equality.</param>
	public bool Equals(Duration duration)
	{
		if (HasTimeSpan)
		{
			if (duration.HasTimeSpan)
			{
				return _timeSpan == duration._timeSpan;
			}
			return false;
		}
		return _durationType == duration._durationType;
	}

	/// <summary>Determines whether two instances of <see cref="T:System.Windows.Duration" /> are equal.</summary>
	/// <returns>true if <paramref name="t1" /> is equal to <paramref name="t2" />; otherwise, false.</returns>
	/// <param name="t1">First instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	/// <param name="t2">Second instance of <see cref="T:System.Windows.Duration" /> to compare.</param>
	public static bool Equals(Duration t1, Duration t2)
	{
		return t1.Equals(t2);
	}

	/// <summary>Gets a hash code for this instance.</summary>
	/// <returns>A signed 32-bit integer hash code.</returns>
	public override int GetHashCode()
	{
		if (HasTimeSpan)
		{
			return _timeSpan.GetHashCode();
		}
		return _durationType.GetHashCode() + 17;
	}

	/// <summary>Subtracts the value of the specified instance of <see cref="T:System.Windows.Duration" /> from this instance.</summary>
	/// <returns>A new instance of <see cref="T:System.Windows.Duration" /> whose value is the result of this instance minus the value of <paramref name="duration" />.</returns>
	/// <param name="duration">The instance of <see cref="T:System.Windows.Duration" /> to subtract from the current instance.</param>
	public Duration Subtract(Duration duration)
	{
		return this - duration;
	}

	/// <summary>Converts an instance of <see cref="T:System.Windows.Duration" /> to a <see cref="T:System.String" /> representation.</summary>
	/// <returns>A <see cref="T:System.String" /> representation of this instance of <see cref="T:System.Windows.Duration" />.</returns>
	public override string ToString()
	{
		if (HasTimeSpan)
		{
			return TypeDescriptor.GetConverter(_timeSpan).ConvertToString(_timeSpan);
		}
		if (_durationType == DurationType.Forever)
		{
			return "Forever";
		}
		return "Automatic";
	}
}
