using System.IO;

namespace MS.Internal.IO.Packaging;

internal interface ITrackingMemoryStreamFactory
{
	MemoryStream Create();

	MemoryStream Create(int capacity);

	void ReportMemoryUsageDelta(int delta);
}
