using System.ComponentModel;
using System.IO;

namespace System.Xml;

/// <summary>Represents an application resource stream resolver.</summary>
[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IApplicationResourceStreamResolver
{
	/// <summary>Returns an application resource stream from the specified URI.</summary>
	/// <returns>An application resource stream.</returns>
	/// <param name="relativeUri">The relative URI.</param>
	[Obsolete("This API supports the .NET Framework infrastructure and is not intended to be used directly from your code.", true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	Stream GetApplicationResourceStream(Uri relativeUri);
}
