using System.Collections;

namespace MS.Internal.Controls;

internal class EmptyEnumerable : IEnumerable
{
	private static IEnumerable _instance;

	public static IEnumerable Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new EmptyEnumerable();
			}
			return _instance;
		}
	}

	private EmptyEnumerable()
	{
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return EmptyEnumerator.Instance;
	}
}
