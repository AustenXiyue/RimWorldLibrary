namespace System.Runtime.CompilerServices;

/// <summary>Holds a reference to a value.</summary>
/// <typeparam name="T">The type of the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</typeparam>
/// <filterpriority>2</filterpriority>
public class StrongBox<T> : IStrongBox
{
	/// <summary>Represents the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</summary>
	public T Value;

	/// <summary>Gets or sets the value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</summary>
	/// <returns>The value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> references.</returns>
	object IStrongBox.Value
	{
		get
		{
			return Value;
		}
		set
		{
			Value = (T)value;
		}
	}

	/// <summary>Initializes a new StrongBox which can receive a value when used in a reference call.</summary>
	public StrongBox()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> class by using the supplied value. </summary>
	/// <param name="value">A value that the <see cref="T:System.Runtime.CompilerServices.StrongBox`1" /> will reference.</param>
	/// <filterpriority>2</filterpriority>
	public StrongBox(T value)
	{
		Value = value;
	}
}
