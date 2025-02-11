using System.IO;

namespace MS.Internal.IO.Packaging;

internal sealed class TrackingMemoryStream : MemoryStream
{
	private ITrackingMemoryStreamFactory _memoryStreamFactory;

	private int _lastReportedHighWaterMark;

	internal TrackingMemoryStream(ITrackingMemoryStreamFactory memoryStreamFactory)
	{
		_memoryStreamFactory = memoryStreamFactory;
		ReportIfNeccessary();
	}

	internal TrackingMemoryStream(ITrackingMemoryStreamFactory memoryStreamFactory, int capacity)
		: base(capacity)
	{
		_memoryStreamFactory = memoryStreamFactory;
		ReportIfNeccessary();
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		int result = base.Read(buffer, offset, count);
		ReportIfNeccessary();
		return result;
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		base.Write(buffer, offset, count);
		ReportIfNeccessary();
	}

	public override void SetLength(long value)
	{
		base.SetLength(value);
		ReportIfNeccessary();
	}

	protected override void Dispose(bool disposing)
	{
		try
		{
			if (disposing && _memoryStreamFactory != null)
			{
				SetLength(0L);
				Capacity = 0;
				ReportIfNeccessary();
				_memoryStreamFactory = null;
			}
		}
		finally
		{
			base.Dispose(disposing);
		}
	}

	private void ReportIfNeccessary()
	{
		if (Capacity != _lastReportedHighWaterMark)
		{
			_memoryStreamFactory.ReportMemoryUsageDelta(checked(Capacity - _lastReportedHighWaterMark));
			_lastReportedHighWaterMark = Capacity;
		}
	}
}
