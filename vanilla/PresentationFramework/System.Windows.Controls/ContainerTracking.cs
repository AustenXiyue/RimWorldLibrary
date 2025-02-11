using System.Diagnostics;

namespace System.Windows.Controls;

internal class ContainerTracking<T>
{
	private T _container;

	private ContainerTracking<T> _next;

	private ContainerTracking<T> _previous;

	internal T Container => _container;

	internal ContainerTracking<T> Next => _next;

	internal ContainerTracking<T> Previous => _previous;

	internal ContainerTracking(T container)
	{
		_container = container;
	}

	internal void StartTracking(ref ContainerTracking<T> root)
	{
		if (root != null)
		{
			root._previous = this;
		}
		_next = root;
		root = this;
	}

	internal void StopTracking(ref ContainerTracking<T> root)
	{
		if (_previous != null)
		{
			_previous._next = _next;
		}
		if (_next != null)
		{
			_next._previous = _previous;
		}
		if (root == this)
		{
			root = _next;
		}
		_previous = null;
		_next = null;
	}

	[Conditional("DEBUG")]
	internal void Debug_AssertIsInList(ContainerTracking<T> root)
	{
	}

	[Conditional("DEBUG")]
	internal void Debug_AssertNotInList(ContainerTracking<T> root)
	{
	}
}
