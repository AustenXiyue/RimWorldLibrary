using System.ComponentModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Media.Animation;

/// <summary>During the relative course of an animation, a <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance specifies the precise timing when a particular key frame should take place. </summary>
[TypeConverter(typeof(KeyTimeConverter))]
public struct KeyTime : IEquatable<KeyTime>
{
	private object _value;

	private KeyTimeType _type;

	/// <summary>Gets the <see cref="P:System.Windows.Media.Animation.KeyTime.Uniform" /> value which divides the allotted time of the animation evenly between key frames.</summary>
	/// <returns>A <see cref="P:System.Windows.Media.Animation.KeyTime.Uniform" /> value.</returns>
	public static KeyTime Uniform
	{
		get
		{
			KeyTime result = default(KeyTime);
			result._type = KeyTimeType.Uniform;
			return result;
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Media.Animation.KeyTime.Paced" /> value which creates timing behavior resulting in an animation that interpolates at a constant rate.</summary>
	/// <returns>A <see cref="P:System.Windows.Media.Animation.KeyTime.Paced" /> value.</returns>
	public static KeyTime Paced
	{
		get
		{
			KeyTime result = default(KeyTime);
			result._type = KeyTimeType.Paced;
			return result;
		}
	}

	/// <summary>Gets the time when the key frame ends expressed as a time relative to the beginning of the animation.</summary>
	/// <returns>A <see cref="P:System.Windows.Media.Animation.KeyTime.TimeSpan" /> value.</returns>
	/// <exception cref="T:System.InvalidOperationException">If this instance is not of type <see cref="P:System.Windows.Media.Animation.KeyTime.TimeSpan" />.</exception>
	public TimeSpan TimeSpan
	{
		get
		{
			if (_type == KeyTimeType.TimeSpan)
			{
				return (TimeSpan)_value;
			}
			throw new InvalidOperationException();
		}
	}

	/// <summary>Gets the time when the key frame ends expressed as a percentage of the total duration of the animation. </summary>
	/// <returns>A <see cref="P:System.Windows.Media.Animation.KeyTime.Percent" /> value.</returns>
	/// <exception cref="T:System.InvalidOperationException">If this instance is not of type <see cref="P:System.Windows.Media.Animation.KeyTime.Percent" />.</exception>
	public double Percent
	{
		get
		{
			if (_type == KeyTimeType.Percent)
			{
				return (double)_value;
			}
			throw new InvalidOperationException();
		}
	}

	/// <summary>Gets the <see cref="P:System.Windows.Media.Animation.KeyTime.Type" /> value this instance represents. </summary>
	/// <returns>A <see cref="P:System.Windows.Media.Animation.KeyTime.Type" /> value.  </returns>
	public KeyTimeType Type => _type;

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance, with the <see cref="T:System.Windows.Media.Animation.KeyTimeType" /> property initialized to the value of the specified parameter. </summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance, initialized to the value of <paramref name="percent" />. </returns>
	/// <param name="percent">The value of the new <see cref="T:System.Windows.Media.Animation.KeyTime" />.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="percent" /> is less than 0.0 or greater than 1.0.</exception>
	public static KeyTime FromPercent(double percent)
	{
		if (percent < 0.0 || percent > 1.0)
		{
			throw new ArgumentOutOfRangeException("percent", SR.Format(SR.Animation_KeyTime_InvalidPercentValue, percent));
		}
		KeyTime result = default(KeyTime);
		result._value = percent;
		result._type = KeyTimeType.Percent;
		return result;
	}

	/// <summary>Creates a new <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance, with the <see cref="T:System.Windows.Media.Animation.KeyTimeType" /> property initialized to the value of the specified parameter.</summary>
	/// <returns>A new <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance, initialized to the value of <paramref name="timeSpan" />.</returns>
	/// <param name="timeSpan">The value of the new <see cref="T:System.Windows.Media.Animation.KeyTime" />.</param>
	public static KeyTime FromTimeSpan(TimeSpan timeSpan)
	{
		if (timeSpan < TimeSpan.Zero)
		{
			throw new ArgumentOutOfRangeException("timeSpan", SR.Format(SR.Animation_KeyTime_LessThanZero, timeSpan));
		}
		KeyTime result = default(KeyTime);
		result._value = timeSpan;
		result._type = KeyTimeType.TimeSpan;
		return result;
	}

	/// <summary>Indicates whether the two specified <see cref="T:System.Windows.Media.Animation.KeyTime" /> structures are equal.</summary>
	/// <returns>true if the values of <paramref name="keyTime1" /> and <paramref name="keyTime2" /> are equal; otherwise false.</returns>
	/// <param name="keyTime1">The first value to compare.</param>
	/// <param name="keyTime2">The second value to compare.</param>
	public static bool Equals(KeyTime keyTime1, KeyTime keyTime2)
	{
		if (keyTime1._type == keyTime2._type)
		{
			switch (keyTime1._type)
			{
			case KeyTimeType.Percent:
				if ((double)keyTime1._value != (double)keyTime2._value)
				{
					return false;
				}
				break;
			case KeyTimeType.TimeSpan:
				if ((TimeSpan)keyTime1._value != (TimeSpan)keyTime2._value)
				{
					return false;
				}
				break;
			}
			return true;
		}
		return false;
	}

	/// <summary>Overloaded operator that compares two <see cref="T:System.Windows.Media.Animation.KeyTime" /> structures for equality.</summary>
	/// <returns>true if <paramref name="keyTime1" /> and <paramref name="keyTime2" /> are equal; otherwise, false.</returns>
	/// <param name="keyTime1">The first value to compare.</param>
	/// <param name="keyTime2">The second value to compare.</param>
	public static bool operator ==(KeyTime keyTime1, KeyTime keyTime2)
	{
		return Equals(keyTime1, keyTime2);
	}

	/// <summary>Overloaded operator that compares two <see cref="T:System.Windows.Media.Animation.KeyTime" /> structures for inequality.</summary>
	/// <returns>true if <paramref name="keyTime1" /> and <paramref name="keyTime2" /> are not equal; otherwise, false. </returns>
	/// <param name="keyTime1">The first value to compare.</param>
	/// <param name="keyTime2">The second value to compare.</param>
	public static bool operator !=(KeyTime keyTime1, KeyTime keyTime2)
	{
		return !Equals(keyTime1, keyTime2);
	}

	/// <summary>Indicates whether this instance is equal to the specified <see cref="T:System.Windows.Media.Animation.KeyTime" />.</summary>
	/// <returns>true if <paramref name="value" /> is equal to this instance; otherwise, false.</returns>
	/// <param name="value">The object to compare with this instance.</param>
	public bool Equals(KeyTime value)
	{
		return Equals(this, value);
	}

	/// <summary>Indicates whether this instance equals the specified object.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Windows.Media.Animation.KeyTime" /> that represents the same length of time as this instance; otherwise false.</returns>
	/// <param name="value">The object to compare with this instance.</param>
	public override bool Equals(object value)
	{
		if (value == null || !(value is KeyTime))
		{
			return false;
		}
		return this == (KeyTime)value;
	}

	/// <summary>Returns an integer hash code representing this instance.</summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		if (_value != null)
		{
			return _value.GetHashCode();
		}
		return _type.GetHashCode();
	}

	/// <summary>Returns a string representing this <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance. </summary>
	/// <returns>A string representation of this instance.</returns>
	public override string ToString()
	{
		return new KeyTimeConverter().ConvertToString(this);
	}

	/// <summary>Overloaded operator that implicitly converts a <see cref="P:System.Windows.Media.Animation.KeyTime.TimeSpan" /> to a <see cref="T:System.Windows.Media.Animation.KeyTime" />.</summary>
	/// <returns>The new <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance.</returns>
	/// <param name="timeSpan">The <see cref="P:System.Windows.Media.Animation.KeyTime.TimeSpan" /> value to convert.</param>
	public static implicit operator KeyTime(TimeSpan timeSpan)
	{
		return FromTimeSpan(timeSpan);
	}
}
