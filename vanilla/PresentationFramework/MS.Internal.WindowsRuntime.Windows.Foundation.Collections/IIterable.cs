using System.Runtime.InteropServices;

namespace MS.Internal.WindowsRuntime.Windows.Foundation.Collections;

[Guid("FAA585EA-6214-4217-AFDA-7F46DE5869B3")]
internal interface IIterable<T>
{
	IIterator<T> First();
}
