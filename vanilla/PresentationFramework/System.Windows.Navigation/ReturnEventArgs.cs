namespace System.Windows.Navigation;

/// <summary>Provides data for the <see cref="E:System.Windows.Navigation.PageFunction`1.Return" /> event. </summary>
/// <typeparam name="T">The type of the return value.</typeparam>
public class ReturnEventArgs<T> : EventArgs
{
	private T _result;

	/// <summary>Gets or sets the value that is returned by the page function.</summary>
	/// <returns>The value that is returned by the page function.</returns>
	public T Result
	{
		get
		{
			return _result;
		}
		set
		{
			_result = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.ReturnEventArgs`1" /> class. </summary>
	public ReturnEventArgs()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.ReturnEventArgs`1" /> class with the return value.</summary>
	/// <param name="result">The value to be returned.</param>
	public ReturnEventArgs(T result)
	{
		_result = result;
	}
}
