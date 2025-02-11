using System;
using System.Windows.Threading;
using MS.Internal.Documents;

namespace MS.Internal.PtsHost;

internal sealed class BackgroundFormatInfo
{
	private double _viewportHeight;

	private bool _doesFinalDTRCoverRestOfText;

	private int _lastCPUninterruptible;

	private DateTime _backgroundFormatStopTime;

	private int _cchAllText;

	private int _cpInterrupted;

	private static bool _isBackgroundFormatEnabled = true;

	private StructuralCache _structuralCache;

	private DateTime _throttleTimeout = DateTime.UtcNow;

	private DispatcherTimer _throttleBackgroundTimer;

	private IFlowDocumentFormatter _pendingBackgroundFormatter;

	private const uint _throttleBackgroundSeconds = 2u;

	private const uint _stopTimeDelta = 200u;

	internal int LastCPUninterruptible => _lastCPUninterruptible;

	internal DateTime BackgroundFormatStopTime => _backgroundFormatStopTime;

	internal int CchAllText => _cchAllText;

	internal static bool IsBackgroundFormatEnabled => _isBackgroundFormatEnabled;

	internal bool DoesFinalDTRCoverRestOfText => _doesFinalDTRCoverRestOfText;

	internal int CPInterrupted
	{
		get
		{
			return _cpInterrupted;
		}
		set
		{
			_cpInterrupted = value;
		}
	}

	internal double ViewportHeight
	{
		get
		{
			return _viewportHeight;
		}
		set
		{
			_viewportHeight = value;
		}
	}

	internal BackgroundFormatInfo(StructuralCache structuralCache)
	{
		_structuralCache = structuralCache;
	}

	internal void UpdateBackgroundFormatInfo()
	{
		_cpInterrupted = -1;
		_lastCPUninterruptible = 0;
		_doesFinalDTRCoverRestOfText = false;
		_cchAllText = _structuralCache.TextContainer.SymbolCount;
		if (_structuralCache.DtrList != null)
		{
			int num = 0;
			for (int i = 0; i < _structuralCache.DtrList.Length - 1; i++)
			{
				num += _structuralCache.DtrList[i].PositionsAdded - _structuralCache.DtrList[i].PositionsRemoved;
			}
			DirtyTextRange dirtyTextRange = _structuralCache.DtrList[_structuralCache.DtrList.Length - 1];
			if (dirtyTextRange.StartIndex + num + dirtyTextRange.PositionsAdded >= _cchAllText)
			{
				_doesFinalDTRCoverRestOfText = true;
				_lastCPUninterruptible = dirtyTextRange.StartIndex + num;
			}
		}
		else
		{
			_doesFinalDTRCoverRestOfText = true;
		}
		_backgroundFormatStopTime = DateTime.UtcNow.AddMilliseconds(200.0);
	}

	internal void ThrottleBackgroundFormatting()
	{
		if (_throttleBackgroundTimer == null)
		{
			_throttleBackgroundTimer = new DispatcherTimer(DispatcherPriority.Background);
			_throttleBackgroundTimer.Interval = new TimeSpan(0, 0, 2);
			_throttleBackgroundTimer.Tick += OnThrottleBackgroundTimeout;
		}
		else
		{
			_throttleBackgroundTimer.Stop();
		}
		_throttleBackgroundTimer.Start();
	}

	internal void BackgroundFormat(IFlowDocumentFormatter formatter, bool ignoreThrottle)
	{
		if (_throttleBackgroundTimer == null)
		{
			formatter.OnContentInvalidated(affectsLayout: true);
		}
		else if (ignoreThrottle)
		{
			OnThrottleBackgroundTimeout(null, EventArgs.Empty);
		}
		else
		{
			_pendingBackgroundFormatter = formatter;
		}
	}

	private void OnThrottleBackgroundTimeout(object sender, EventArgs e)
	{
		_throttleBackgroundTimer.Stop();
		_throttleBackgroundTimer = null;
		if (_pendingBackgroundFormatter != null)
		{
			BackgroundFormat(_pendingBackgroundFormatter, ignoreThrottle: true);
			_pendingBackgroundFormatter = null;
		}
	}
}
