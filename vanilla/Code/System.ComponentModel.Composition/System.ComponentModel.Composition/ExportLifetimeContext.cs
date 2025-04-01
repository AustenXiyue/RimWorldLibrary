namespace System.ComponentModel.Composition;

/// <summary>Holds an exported value created by an <see cref="T:System.ComponentModel.Composition.ExportFactory`1" /> object and a reference to a method to release that object.</summary>
/// <typeparam name="T">The type of the exported value.</typeparam>
public sealed class ExportLifetimeContext<T> : IDisposable
{
	private readonly T _value;

	private readonly Action _disposeAction;

	/// <summary>Gets the exported value of a <see cref="T:System.ComponentModel.Composition.ExportFactory`1" /> object.</summary>
	/// <returns>The exported value.</returns>
	public T Value => _value;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Composition.ExportLifetimeContext`1" /> class. </summary>
	/// <param name="value">The exported value.</param>
	/// <param name="disposeAction">A reference to a method to release the object.</param>
	public ExportLifetimeContext(T value, Action disposeAction)
	{
		_value = value;
		_disposeAction = disposeAction;
	}

	/// <summary>Releases all resources used by the current instance of the <see cref="T:System.ComponentModel.Composition.ExportLifetimeContext`1" /> class, including its associated export.</summary>
	public void Dispose()
	{
		if (_disposeAction != null)
		{
			_disposeAction();
		}
	}
}
