using System.Runtime.InteropServices;

namespace MS.Internal.WindowsRuntime.Windows.Foundation.Collections;

[Guid("6A79E863-4300-459A-9966-CBB660963EE1")]
internal interface IIterator<T>
{
	T _Current { get; }

	bool HasCurrent { get; }

	bool _MoveNext();

	uint GetMany(ref T[] items);
}
