using System.IO;

namespace MS.Internal.IO.Packaging;

internal class TrackingMemoryStreamFactory : ITrackingMemoryStreamFactory
{
	private long _bufferedMemoryConsumption;

	internal long CurrentMemoryConsumption => _bufferedMemoryConsumption;

	public MemoryStream Create()
	{
		return new TrackingMemoryStream(this);
	}

	public MemoryStream Create(int capacity)
	{
		return new TrackingMemoryStream(this, capacity);
	}

	public void ReportMemoryUsageDelta(int delta)
	{
		checked
		{
			_bufferedMemoryConsumption += delta;
		}
	}
}
