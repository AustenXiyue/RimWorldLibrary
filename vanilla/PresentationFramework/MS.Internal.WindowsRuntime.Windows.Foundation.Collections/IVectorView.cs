using System.Runtime.InteropServices;

namespace MS.Internal.WindowsRuntime.Windows.Foundation.Collections;

[Guid("BBE1FA4C-B0E3-4583-BAEF-1F1B2E483E56")]
internal interface IVectorView<T> : IIterable<T>
{
	uint Size { get; }

	T GetAt(uint index);

	bool IndexOf(T value, out uint index);

	uint GetMany(uint startIndex, ref T[] items);
}
