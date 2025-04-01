using System.Collections;
using System.Runtime.InteropServices;

namespace System.Resources;

/// <summary>Provides the base functionality for reading data from resource files.</summary>
[ComVisible(true)]
public interface IResourceReader : IEnumerable, IDisposable
{
	/// <summary>Closes the resource reader after releasing any resources associated with it.</summary>
	void Close();

	/// <summary>Returns a dictionary enumerator of the resources for this reader.</summary>
	/// <returns>A dictionary enumerator for the resources for this reader.</returns>
	new IDictionaryEnumerator GetEnumerator();
}
