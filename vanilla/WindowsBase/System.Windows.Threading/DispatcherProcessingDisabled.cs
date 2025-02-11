namespace System.Windows.Threading;

/// <summary>Represents the Dispatcher when it is in a disable state and provides a means to re-enable dispatcher processing. </summary>
public struct DispatcherProcessingDisabled : IDisposable
{
	internal Dispatcher _dispatcher;

	/// <summary>Re-enables dispatcher processing.</summary>
	public void Dispose()
	{
		if (_dispatcher != null)
		{
			_dispatcher.VerifyAccess();
			_dispatcher._disableProcessingCount--;
			_dispatcher = null;
		}
	}

	/// <summary>Determines whether the specified <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> object is equal to this <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> object.</summary>
	/// <returns>true if the specified object is equal to this <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> object; otherwise, false.</returns>
	/// <param name="obj">The object to evaluate for equality.</param>
	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is DispatcherProcessingDisabled))
		{
			return false;
		}
		return _dispatcher == ((DispatcherProcessingDisabled)obj)._dispatcher;
	}

	/// <summary>Gets a hash code for this instance. </summary>
	/// <returns>A signed 32-bit integer hash code.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether two <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> objects are equal.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> objects are equal; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator ==(DispatcherProcessingDisabled left, DispatcherProcessingDisabled right)
	{
		return left.Equals(right);
	}

	/// <summary>Determines whether two <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> objects are not equal.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Threading.DispatcherProcessingDisabled" /> objects are not equal; otherwise, false.</returns>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	public static bool operator !=(DispatcherProcessingDisabled left, DispatcherProcessingDisabled right)
	{
		return !left.Equals(right);
	}
}
