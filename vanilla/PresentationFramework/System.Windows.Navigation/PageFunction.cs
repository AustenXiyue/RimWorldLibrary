namespace System.Windows.Navigation;

/// <summary>Represents a special type of page that allows you to treat navigation to a page in a similar fashion to calling a method. </summary>
/// <typeparam name="T">The type of value that the <see cref="T:System.Windows.Navigation.PageFunction`1" /> returns to a caller.</typeparam>
public class PageFunction<T> : PageFunctionBase
{
	/// <summary>Occurs when a called <see cref="T:System.Windows.Navigation.PageFunction`1" /> returns, and can only be handled by the calling page.</summary>
	public event ReturnEventHandler<T> Return
	{
		add
		{
			_AddEventHandler(value);
		}
		remove
		{
			_RemoveEventHandler(value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Navigation.PageFunction`1" /> class. </summary>
	public PageFunction()
	{
		base.RaiseTypedEvent += RaiseTypedReturnEvent;
	}

	/// <summary>A <see cref="T:System.Windows.Navigation.PageFunction`1" /> calls <see cref="M:System.Windows.Navigation.PageFunction`1.OnReturn(System.Windows.Navigation.ReturnEventArgs{`0})" /> to return to the caller, passing a return value via a <see cref="T:System.Windows.Navigation.ReturnEventArgs`1" /> object</summary>
	/// <param name="e">A <see cref="T:System.Windows.Navigation.ReturnEventArgs`1" /> object that contains the <see cref="T:System.Windows.Navigation.PageFunction`1" /> return value (<see cref="P:System.Windows.Navigation.ReturnEventArgs`1.Result" />).</param>
	protected virtual void OnReturn(ReturnEventArgs<T> e)
	{
		_OnReturnUnTyped(e);
	}

	internal void RaiseTypedReturnEvent(PageFunctionBase b, RaiseTypedEventArgs args)
	{
		Delegate d = args.D;
		object o = args.O;
		if ((object)d != null)
		{
			ReturnEventArgs<T> e = o as ReturnEventArgs<T>;
			(d as ReturnEventHandler<T>)(this, e);
		}
	}
}
