using System.Runtime.InteropServices;

namespace System.IO;

/// <summary>Specifies whether to search the current directory, or the current directory and all subdirectories. </summary>
[Serializable]
[ComVisible(true)]
public enum SearchOption
{
	/// <summary>Includes only the current directory in a search operation.</summary>
	TopDirectoryOnly,
	/// <summary>Includes the current directory and all its subdirectories in a search operation. This option includes reparse points such as mounted drives and symbolic links in the search.</summary>
	AllDirectories
}
