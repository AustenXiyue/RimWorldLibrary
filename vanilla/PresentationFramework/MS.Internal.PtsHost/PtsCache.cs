using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using System.Windows.Threading;
using MS.Internal.PtsHost.UnsafeNativeMethods;
using MS.Internal.Text;
using MS.Internal.TextFormatting;

namespace MS.Internal.PtsHost;

internal sealed class PtsCache
{
	private class ContextDesc
	{
		internal PtsHost PtsHost;

		internal PTS.FSCONTEXTINFO ContextInfo;

		internal PTS.FSIMETHODS SubtrackParaInfo;

		internal PTS.FSIMETHODS SubpageParaInfo;

		internal PTS.FSFLOATERINIT FloaterInit;

		internal PTS.FSTABLEOBJINIT TableobjInit;

		internal nint InstalledObjects;

		internal TextFormatter TextFormatter;

		internal TextPenaltyModule TextPenaltyModule;

		internal bool IsOptimalParagraphEnabled;

		internal WeakReference Owner;

		internal bool InUse;
	}

	private sealed class PtsCacheShutDownListener : ShutDownListener
	{
		public PtsCacheShutDownListener(PtsCache target)
			: base(target)
		{
		}

		internal override void OnShutDown(object target, object sender, EventArgs e)
		{
			((PtsCache)target).Shutdown();
		}
	}

	private List<ContextDesc> _contextPool;

	private List<PtsContext> _releaseQueue;

	private readonly object _lock = new object();

	private int _disposed;

	internal static PtsHost AcquireContext(PtsContext ptsContext, TextFormattingMode textFormattingMode)
	{
		PtsCache ptsCache = ptsContext.Dispatcher.PtsCache as PtsCache;
		if (ptsCache == null)
		{
			ptsCache = new PtsCache(ptsContext.Dispatcher);
			ptsContext.Dispatcher.PtsCache = ptsCache;
		}
		return ptsCache.AcquireContextCore(ptsContext, textFormattingMode);
	}

	internal static void ReleaseContext(PtsContext ptsContext)
	{
		PtsCache obj = ptsContext.Dispatcher.PtsCache as PtsCache;
		Invariant.Assert(obj != null, "Cannot retrieve PtsCache from PtsContext object.");
		obj.ReleaseContextCore(ptsContext);
	}

	internal static void GetFloaterHandlerInfo(PtsHost ptsHost, nint pobjectinfo)
	{
		PtsCache obj = Dispatcher.CurrentDispatcher.PtsCache as PtsCache;
		Invariant.Assert(obj != null, "Cannot retrieve PtsCache from the current Dispatcher.");
		obj.GetFloaterHandlerInfoCore(ptsHost, pobjectinfo);
	}

	internal static void GetTableObjHandlerInfo(PtsHost ptsHost, nint pobjectinfo)
	{
		PtsCache obj = Dispatcher.CurrentDispatcher.PtsCache as PtsCache;
		Invariant.Assert(obj != null, "Cannot retrieve PtsCache from the current Dispatcher.");
		obj.GetTableObjHandlerInfoCore(ptsHost, pobjectinfo);
	}

	internal static bool IsDisposed()
	{
		bool result = true;
		if (Dispatcher.CurrentDispatcher != null && Dispatcher.CurrentDispatcher.PtsCache is PtsCache ptsCache)
		{
			result = ptsCache._disposed == 1;
		}
		return result;
	}

	private PtsCache(Dispatcher dispatcher)
	{
		_contextPool = new List<ContextDesc>(1);
		new PtsCacheShutDownListener(this);
	}

	~PtsCache()
	{
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
		{
			DestroyPTSContexts();
		}
	}

	private PtsHost AcquireContextCore(PtsContext ptsContext, TextFormattingMode textFormattingMode)
	{
		int i;
		for (i = 0; i < _contextPool.Count && (_contextPool[i].InUse || _contextPool[i].IsOptimalParagraphEnabled != ptsContext.IsOptimalParagraphEnabled); i++)
		{
		}
		if (i == _contextPool.Count)
		{
			_contextPool.Add(new ContextDesc());
			_contextPool[i].IsOptimalParagraphEnabled = ptsContext.IsOptimalParagraphEnabled;
			_contextPool[i].PtsHost = new PtsHost();
			_contextPool[i].PtsHost.Context = CreatePTSContext(i, textFormattingMode);
		}
		if (_contextPool[i].IsOptimalParagraphEnabled)
		{
			ptsContext.TextFormatter = _contextPool[i].TextFormatter;
		}
		_contextPool[i].InUse = true;
		_contextPool[i].Owner = new WeakReference(ptsContext);
		return _contextPool[i].PtsHost;
	}

	private void ReleaseContextCore(PtsContext ptsContext)
	{
		lock (_lock)
		{
			if (_disposed == 0)
			{
				if (_releaseQueue == null)
				{
					_releaseQueue = new List<PtsContext>();
					ptsContext.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnPtsContextReleased), null);
				}
				_releaseQueue.Add(ptsContext);
			}
		}
	}

	private void GetFloaterHandlerInfoCore(PtsHost ptsHost, nint pobjectinfo)
	{
		int i;
		for (i = 0; i < _contextPool.Count && _contextPool[i].PtsHost != ptsHost; i++)
		{
		}
		Invariant.Assert(i < _contextPool.Count, "Cannot find matching PtsHost in the Context pool.");
		PTS.Validate(PTS.GetFloaterHandlerInfo(ref _contextPool[i].FloaterInit, pobjectinfo));
	}

	private void GetTableObjHandlerInfoCore(PtsHost ptsHost, nint pobjectinfo)
	{
		int i;
		for (i = 0; i < _contextPool.Count && _contextPool[i].PtsHost != ptsHost; i++)
		{
		}
		Invariant.Assert(i < _contextPool.Count, "Cannot find matching PtsHost in the context pool.");
		PTS.Validate(PTS.GetTableObjHandlerInfo(ref _contextPool[i].TableobjInit, pobjectinfo));
	}

	private void Shutdown()
	{
		GC.WaitForPendingFinalizers();
		if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
		{
			OnPtsContextReleased(cleanContextPool: false);
			DestroyPTSContexts();
		}
	}

	private void DestroyPTSContexts()
	{
		int num = 0;
		while (num < _contextPool.Count)
		{
			if (_contextPool[num].Owner.Target is PtsContext ptsContext)
			{
				Invariant.Assert(_contextPool[num].PtsHost.Context == ptsContext.Context, "PTS Context mismatch.");
				_contextPool[num].Owner = new WeakReference(null);
				_contextPool[num].InUse = false;
				Invariant.Assert(!ptsContext.Disposed, "PtsContext has been already disposed.");
				ptsContext.Dispose();
			}
			if (!_contextPool[num].InUse)
			{
				Invariant.Assert(_contextPool[num].PtsHost.Context != IntPtr.Zero, "PTS Context handle is not valid.");
				PTS.IgnoreError(PTS.DestroyDocContext(_contextPool[num].PtsHost.Context));
				Invariant.Assert(_contextPool[num].InstalledObjects != IntPtr.Zero, "Installed Objects handle is not valid.");
				PTS.IgnoreError(PTS.DestroyInstalledObjectsInfo(_contextPool[num].InstalledObjects));
				if (_contextPool[num].TextPenaltyModule != null)
				{
					_contextPool[num].TextPenaltyModule.Dispose();
				}
				_contextPool.RemoveAt(num);
			}
			else
			{
				num++;
			}
		}
	}

	private object OnPtsContextReleased(object args)
	{
		OnPtsContextReleased(cleanContextPool: true);
		return null;
	}

	private void OnPtsContextReleased(bool cleanContextPool)
	{
		int i;
		lock (_lock)
		{
			if (_releaseQueue != null)
			{
				foreach (PtsContext item in _releaseQueue)
				{
					for (i = 0; i < _contextPool.Count; i++)
					{
						if (_contextPool[i].PtsHost.Context == item.Context)
						{
							_contextPool[i].Owner = new WeakReference(null);
							_contextPool[i].InUse = false;
							break;
						}
					}
					Invariant.Assert(i < _contextPool.Count, "PtsContext not found in the context pool.");
					Invariant.Assert(!item.Disposed, "PtsContext has been already disposed.");
					item.Dispose();
				}
				_releaseQueue = null;
			}
		}
		if (!cleanContextPool || _contextPool.Count <= 4)
		{
			return;
		}
		i = 4;
		while (i < _contextPool.Count)
		{
			if (!_contextPool[i].InUse)
			{
				Invariant.Assert(_contextPool[i].PtsHost.Context != IntPtr.Zero, "PTS Context handle is not valid.");
				PTS.Validate(PTS.DestroyDocContext(_contextPool[i].PtsHost.Context));
				Invariant.Assert(_contextPool[i].InstalledObjects != IntPtr.Zero, "Installed Objects handle is not valid.");
				PTS.Validate(PTS.DestroyInstalledObjectsInfo(_contextPool[i].InstalledObjects));
				if (_contextPool[i].TextPenaltyModule != null)
				{
					_contextPool[i].TextPenaltyModule.Dispose();
				}
				_contextPool.RemoveAt(i);
			}
			else
			{
				i++;
			}
		}
	}

	private nint CreatePTSContext(int index, TextFormattingMode textFormattingMode)
	{
		PtsHost ptsHost = _contextPool[index].PtsHost;
		Invariant.Assert(ptsHost != null);
		InitInstalledObjectsInfo(ptsHost, ref _contextPool[index].SubtrackParaInfo, ref _contextPool[index].SubpageParaInfo, out var installedObjects, out var installedObjectsCount);
		_contextPool[index].InstalledObjects = installedObjects;
		InitGenericInfo(ptsHost, index + 1, installedObjects, installedObjectsCount, ref _contextPool[index].ContextInfo);
		InitFloaterObjInfo(ptsHost, ref _contextPool[index].FloaterInit);
		InitTableObjInfo(ptsHost, ref _contextPool[index].TableobjInit);
		if (_contextPool[index].IsOptimalParagraphEnabled)
		{
			TextFormatterContext textFormatterContext = new TextFormatterContext();
			TextPenaltyModule textPenaltyModule = textFormatterContext.GetTextPenaltyModule();
			nint ptsPenaltyModule = textPenaltyModule.DangerousGetHandle();
			_contextPool[index].TextPenaltyModule = textPenaltyModule;
			_contextPool[index].ContextInfo.ptsPenaltyModule = ptsPenaltyModule;
			_contextPool[index].TextFormatter = TextFormatter.CreateFromContext(textFormatterContext, textFormattingMode);
			GC.SuppressFinalize(_contextPool[index].TextPenaltyModule);
		}
		PTS.Validate(PTS.CreateDocContext(ref _contextPool[index].ContextInfo, out var pfscontext));
		return pfscontext;
	}

	private unsafe void InitGenericInfo(PtsHost ptsHost, nint clientData, nint installedObjects, int installedObjectsCount, ref PTS.FSCONTEXTINFO contextInfo)
	{
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		Invariant.Assert(condition: true);
		contextInfo.version = 0u;
		contextInfo.fsffi = 320u;
		contextInfo.drMinColumnBalancingStep = TextDpi.ToTextDpi(10.0);
		contextInfo.cInstalledObjects = installedObjectsCount;
		contextInfo.pInstalledObjects = installedObjects;
		contextInfo.pfsclient = clientData;
		contextInfo.pfnAssertFailed = ptsHost.AssertFailed;
		contextInfo.fscbk.cbkfig.pfnGetFigureProperties = ptsHost.GetFigureProperties;
		contextInfo.fscbk.cbkfig.pfnGetFigurePolygons = ptsHost.GetFigurePolygons;
		contextInfo.fscbk.cbkfig.pfnCalcFigurePosition = ptsHost.CalcFigurePosition;
		contextInfo.fscbk.cbkgen.pfnFSkipPage = ptsHost.FSkipPage;
		contextInfo.fscbk.cbkgen.pfnGetPageDimensions = ptsHost.GetPageDimensions;
		contextInfo.fscbk.cbkgen.pfnGetNextSection = ptsHost.GetNextSection;
		contextInfo.fscbk.cbkgen.pfnGetSectionProperties = ptsHost.GetSectionProperties;
		contextInfo.fscbk.cbkgen.pfnGetJustificationProperties = ptsHost.GetJustificationProperties;
		contextInfo.fscbk.cbkgen.pfnGetMainTextSegment = ptsHost.GetMainTextSegment;
		contextInfo.fscbk.cbkgen.pfnGetHeaderSegment = ptsHost.GetHeaderSegment;
		contextInfo.fscbk.cbkgen.pfnGetFooterSegment = ptsHost.GetFooterSegment;
		contextInfo.fscbk.cbkgen.pfnUpdGetSegmentChange = ptsHost.UpdGetSegmentChange;
		contextInfo.fscbk.cbkgen.pfnGetSectionColumnInfo = ptsHost.GetSectionColumnInfo;
		contextInfo.fscbk.cbkgen.pfnGetSegmentDefinedColumnSpanAreaInfo = ptsHost.GetSegmentDefinedColumnSpanAreaInfo;
		contextInfo.fscbk.cbkgen.pfnGetHeightDefinedColumnSpanAreaInfo = ptsHost.GetHeightDefinedColumnSpanAreaInfo;
		contextInfo.fscbk.cbkgen.pfnGetFirstPara = ptsHost.GetFirstPara;
		contextInfo.fscbk.cbkgen.pfnGetNextPara = ptsHost.GetNextPara;
		contextInfo.fscbk.cbkgen.pfnUpdGetFirstChangeInSegment = ptsHost.UpdGetFirstChangeInSegment;
		contextInfo.fscbk.cbkgen.pfnUpdGetParaChange = ptsHost.UpdGetParaChange;
		contextInfo.fscbk.cbkgen.pfnGetParaProperties = ptsHost.GetParaProperties;
		contextInfo.fscbk.cbkgen.pfnCreateParaclient = ptsHost.CreateParaclient;
		contextInfo.fscbk.cbkgen.pfnTransferDisplayInfo = ptsHost.TransferDisplayInfo;
		contextInfo.fscbk.cbkgen.pfnDestroyParaclient = ptsHost.DestroyParaclient;
		contextInfo.fscbk.cbkgen.pfnFInterruptFormattingAfterPara = ptsHost.FInterruptFormattingAfterPara;
		contextInfo.fscbk.cbkgen.pfnGetEndnoteSeparators = ptsHost.GetEndnoteSeparators;
		contextInfo.fscbk.cbkgen.pfnGetEndnoteSegment = ptsHost.GetEndnoteSegment;
		contextInfo.fscbk.cbkgen.pfnGetNumberEndnoteColumns = ptsHost.GetNumberEndnoteColumns;
		contextInfo.fscbk.cbkgen.pfnGetEndnoteColumnInfo = ptsHost.GetEndnoteColumnInfo;
		contextInfo.fscbk.cbkgen.pfnGetFootnoteSeparators = ptsHost.GetFootnoteSeparators;
		contextInfo.fscbk.cbkgen.pfnFFootnoteBeneathText = ptsHost.FFootnoteBeneathText;
		contextInfo.fscbk.cbkgen.pfnGetNumberFootnoteColumns = ptsHost.GetNumberFootnoteColumns;
		contextInfo.fscbk.cbkgen.pfnGetFootnoteColumnInfo = ptsHost.GetFootnoteColumnInfo;
		contextInfo.fscbk.cbkgen.pfnGetFootnoteSegment = ptsHost.GetFootnoteSegment;
		contextInfo.fscbk.cbkgen.pfnGetFootnotePresentationAndRejectionOrder = ptsHost.GetFootnotePresentationAndRejectionOrder;
		contextInfo.fscbk.cbkgen.pfnFAllowFootnoteSeparation = ptsHost.FAllowFootnoteSeparation;
		contextInfo.fscbk.cbkobj.pfnDuplicateMcsclient = ptsHost.DuplicateMcsclient;
		contextInfo.fscbk.cbkobj.pfnDestroyMcsclient = ptsHost.DestroyMcsclient;
		contextInfo.fscbk.cbkobj.pfnFEqualMcsclient = ptsHost.FEqualMcsclient;
		contextInfo.fscbk.cbkobj.pfnConvertMcsclient = ptsHost.ConvertMcsclient;
		contextInfo.fscbk.cbkobj.pfnGetObjectHandlerInfo = ptsHost.GetObjectHandlerInfo;
		contextInfo.fscbk.cbktxt.pfnCreateParaBreakingSession = ptsHost.CreateParaBreakingSession;
		contextInfo.fscbk.cbktxt.pfnDestroyParaBreakingSession = ptsHost.DestroyParaBreakingSession;
		contextInfo.fscbk.cbktxt.pfnGetTextProperties = ptsHost.GetTextProperties;
		contextInfo.fscbk.cbktxt.pfnGetNumberFootnotes = ptsHost.GetNumberFootnotes;
		contextInfo.fscbk.cbktxt.pfnGetFootnotes = ptsHost.GetFootnotes;
		contextInfo.fscbk.cbktxt.pfnFormatDropCap = ptsHost.FormatDropCap;
		contextInfo.fscbk.cbktxt.pfnGetDropCapPolygons = ptsHost.GetDropCapPolygons;
		contextInfo.fscbk.cbktxt.pfnDestroyDropCap = ptsHost.DestroyDropCap;
		contextInfo.fscbk.cbktxt.pfnFormatBottomText = ptsHost.FormatBottomText;
		contextInfo.fscbk.cbktxt.pfnFormatLine = ptsHost.FormatLine;
		contextInfo.fscbk.cbktxt.pfnFormatLineForced = ptsHost.FormatLineForced;
		contextInfo.fscbk.cbktxt.pfnFormatLineVariants = ptsHost.FormatLineVariants;
		contextInfo.fscbk.cbktxt.pfnReconstructLineVariant = ptsHost.ReconstructLineVariant;
		contextInfo.fscbk.cbktxt.pfnDestroyLine = ptsHost.DestroyLine;
		contextInfo.fscbk.cbktxt.pfnDuplicateLineBreakRecord = ptsHost.DuplicateLineBreakRecord;
		contextInfo.fscbk.cbktxt.pfnDestroyLineBreakRecord = ptsHost.DestroyLineBreakRecord;
		contextInfo.fscbk.cbktxt.pfnSnapGridVertical = ptsHost.SnapGridVertical;
		contextInfo.fscbk.cbktxt.pfnGetDvrSuppressibleBottomSpace = ptsHost.GetDvrSuppressibleBottomSpace;
		contextInfo.fscbk.cbktxt.pfnGetDvrAdvance = ptsHost.GetDvrAdvance;
		contextInfo.fscbk.cbktxt.pfnUpdGetChangeInText = ptsHost.UpdGetChangeInText;
		contextInfo.fscbk.cbktxt.pfnUpdGetDropCapChange = ptsHost.UpdGetDropCapChange;
		contextInfo.fscbk.cbktxt.pfnFInterruptFormattingText = ptsHost.FInterruptFormattingText;
		contextInfo.fscbk.cbktxt.pfnGetTextParaCache = ptsHost.GetTextParaCache;
		contextInfo.fscbk.cbktxt.pfnSetTextParaCache = ptsHost.SetTextParaCache;
		contextInfo.fscbk.cbktxt.pfnGetOptimalLineDcpCache = ptsHost.GetOptimalLineDcpCache;
		contextInfo.fscbk.cbktxt.pfnGetNumberAttachedObjectsBeforeTextLine = ptsHost.GetNumberAttachedObjectsBeforeTextLine;
		contextInfo.fscbk.cbktxt.pfnGetAttachedObjectsBeforeTextLine = ptsHost.GetAttachedObjectsBeforeTextLine;
		contextInfo.fscbk.cbktxt.pfnGetNumberAttachedObjectsInTextLine = ptsHost.GetNumberAttachedObjectsInTextLine;
		contextInfo.fscbk.cbktxt.pfnGetAttachedObjectsInTextLine = ptsHost.GetAttachedObjectsInTextLine;
		contextInfo.fscbk.cbktxt.pfnUpdGetAttachedObjectChange = ptsHost.UpdGetAttachedObjectChange;
		contextInfo.fscbk.cbktxt.pfnGetDurFigureAnchor = ptsHost.GetDurFigureAnchor;
	}

	private unsafe void InitInstalledObjectsInfo(PtsHost ptsHost, ref PTS.FSIMETHODS subtrackParaInfo, ref PTS.FSIMETHODS subpageParaInfo, out nint installedObjects, out int installedObjectsCount)
	{
		subtrackParaInfo.pfnCreateContext = ptsHost.SubtrackCreateContext;
		subtrackParaInfo.pfnDestroyContext = ptsHost.SubtrackDestroyContext;
		subtrackParaInfo.pfnFormatParaFinite = ptsHost.SubtrackFormatParaFinite;
		subtrackParaInfo.pfnFormatParaBottomless = ptsHost.SubtrackFormatParaBottomless;
		subtrackParaInfo.pfnUpdateBottomlessPara = ptsHost.SubtrackUpdateBottomlessPara;
		subtrackParaInfo.pfnSynchronizeBottomlessPara = ptsHost.SubtrackSynchronizeBottomlessPara;
		subtrackParaInfo.pfnComparePara = ptsHost.SubtrackComparePara;
		subtrackParaInfo.pfnClearUpdateInfoInPara = ptsHost.SubtrackClearUpdateInfoInPara;
		subtrackParaInfo.pfnDestroyPara = ptsHost.SubtrackDestroyPara;
		subtrackParaInfo.pfnDuplicateBreakRecord = ptsHost.SubtrackDuplicateBreakRecord;
		subtrackParaInfo.pfnDestroyBreakRecord = ptsHost.SubtrackDestroyBreakRecord;
		subtrackParaInfo.pfnGetColumnBalancingInfo = ptsHost.SubtrackGetColumnBalancingInfo;
		subtrackParaInfo.pfnGetNumberFootnotes = ptsHost.SubtrackGetNumberFootnotes;
		subtrackParaInfo.pfnGetFootnoteInfo = ptsHost.SubtrackGetFootnoteInfo;
		subtrackParaInfo.pfnGetFootnoteInfoWord = IntPtr.Zero;
		subtrackParaInfo.pfnShiftVertical = ptsHost.SubtrackShiftVertical;
		subtrackParaInfo.pfnTransferDisplayInfoPara = ptsHost.SubtrackTransferDisplayInfoPara;
		subpageParaInfo.pfnCreateContext = ptsHost.SubpageCreateContext;
		subpageParaInfo.pfnDestroyContext = ptsHost.SubpageDestroyContext;
		subpageParaInfo.pfnFormatParaFinite = ptsHost.SubpageFormatParaFinite;
		subpageParaInfo.pfnFormatParaBottomless = ptsHost.SubpageFormatParaBottomless;
		subpageParaInfo.pfnUpdateBottomlessPara = ptsHost.SubpageUpdateBottomlessPara;
		subpageParaInfo.pfnSynchronizeBottomlessPara = ptsHost.SubpageSynchronizeBottomlessPara;
		subpageParaInfo.pfnComparePara = ptsHost.SubpageComparePara;
		subpageParaInfo.pfnClearUpdateInfoInPara = ptsHost.SubpageClearUpdateInfoInPara;
		subpageParaInfo.pfnDestroyPara = ptsHost.SubpageDestroyPara;
		subpageParaInfo.pfnDuplicateBreakRecord = ptsHost.SubpageDuplicateBreakRecord;
		subpageParaInfo.pfnDestroyBreakRecord = ptsHost.SubpageDestroyBreakRecord;
		subpageParaInfo.pfnGetColumnBalancingInfo = ptsHost.SubpageGetColumnBalancingInfo;
		subpageParaInfo.pfnGetNumberFootnotes = ptsHost.SubpageGetNumberFootnotes;
		subpageParaInfo.pfnGetFootnoteInfo = ptsHost.SubpageGetFootnoteInfo;
		subpageParaInfo.pfnShiftVertical = ptsHost.SubpageShiftVertical;
		subpageParaInfo.pfnTransferDisplayInfoPara = ptsHost.SubpageTransferDisplayInfoPara;
		PTS.Validate(PTS.CreateInstalledObjectsInfo(ref subtrackParaInfo, ref subpageParaInfo, out installedObjects, out installedObjectsCount));
	}

	private unsafe void InitFloaterObjInfo(PtsHost ptsHost, ref PTS.FSFLOATERINIT floaterInit)
	{
		floaterInit.fsfloatercbk.pfnGetFloaterProperties = ptsHost.GetFloaterProperties;
		floaterInit.fsfloatercbk.pfnFormatFloaterContentFinite = ptsHost.FormatFloaterContentFinite;
		floaterInit.fsfloatercbk.pfnFormatFloaterContentBottomless = ptsHost.FormatFloaterContentBottomless;
		floaterInit.fsfloatercbk.pfnUpdateBottomlessFloaterContent = ptsHost.UpdateBottomlessFloaterContent;
		floaterInit.fsfloatercbk.pfnGetFloaterPolygons = ptsHost.GetFloaterPolygons;
		floaterInit.fsfloatercbk.pfnClearUpdateInfoInFloaterContent = ptsHost.ClearUpdateInfoInFloaterContent;
		floaterInit.fsfloatercbk.pfnCompareFloaterContents = ptsHost.CompareFloaterContents;
		floaterInit.fsfloatercbk.pfnDestroyFloaterContent = ptsHost.DestroyFloaterContent;
		floaterInit.fsfloatercbk.pfnDuplicateFloaterContentBreakRecord = ptsHost.DuplicateFloaterContentBreakRecord;
		floaterInit.fsfloatercbk.pfnDestroyFloaterContentBreakRecord = ptsHost.DestroyFloaterContentBreakRecord;
		floaterInit.fsfloatercbk.pfnGetFloaterContentColumnBalancingInfo = ptsHost.GetFloaterContentColumnBalancingInfo;
		floaterInit.fsfloatercbk.pfnGetFloaterContentNumberFootnotes = ptsHost.GetFloaterContentNumberFootnotes;
		floaterInit.fsfloatercbk.pfnGetFloaterContentFootnoteInfo = ptsHost.GetFloaterContentFootnoteInfo;
		floaterInit.fsfloatercbk.pfnTransferDisplayInfoInFloaterContent = ptsHost.TransferDisplayInfoInFloaterContent;
		floaterInit.fsfloatercbk.pfnGetMCSClientAfterFloater = ptsHost.GetMCSClientAfterFloater;
		floaterInit.fsfloatercbk.pfnGetDvrUsedForFloater = ptsHost.GetDvrUsedForFloater;
	}

	private unsafe void InitTableObjInfo(PtsHost ptsHost, ref PTS.FSTABLEOBJINIT tableobjInit)
	{
		tableobjInit.tableobjcbk.pfnGetTableProperties = ptsHost.GetTableProperties;
		tableobjInit.tableobjcbk.pfnAutofitTable = ptsHost.AutofitTable;
		tableobjInit.tableobjcbk.pfnUpdAutofitTable = ptsHost.UpdAutofitTable;
		tableobjInit.tableobjcbk.pfnGetMCSClientAfterTable = ptsHost.GetMCSClientAfterTable;
		tableobjInit.tableobjcbk.pfnGetDvrUsedForFloatTable = IntPtr.Zero;
		tableobjInit.tablecbkfetch.pfnGetFirstHeaderRow = ptsHost.GetFirstHeaderRow;
		tableobjInit.tablecbkfetch.pfnGetNextHeaderRow = ptsHost.GetNextHeaderRow;
		tableobjInit.tablecbkfetch.pfnGetFirstFooterRow = ptsHost.GetFirstFooterRow;
		tableobjInit.tablecbkfetch.pfnGetNextFooterRow = ptsHost.GetNextFooterRow;
		tableobjInit.tablecbkfetch.pfnGetFirstRow = ptsHost.GetFirstRow;
		tableobjInit.tablecbkfetch.pfnGetNextRow = ptsHost.GetNextRow;
		tableobjInit.tablecbkfetch.pfnUpdFChangeInHeaderFooter = ptsHost.UpdFChangeInHeaderFooter;
		tableobjInit.tablecbkfetch.pfnUpdGetFirstChangeInTable = ptsHost.UpdGetFirstChangeInTable;
		tableobjInit.tablecbkfetch.pfnUpdGetRowChange = ptsHost.UpdGetRowChange;
		tableobjInit.tablecbkfetch.pfnUpdGetCellChange = ptsHost.UpdGetCellChange;
		tableobjInit.tablecbkfetch.pfnGetDistributionKind = ptsHost.GetDistributionKind;
		tableobjInit.tablecbkfetch.pfnGetRowProperties = ptsHost.GetRowProperties;
		tableobjInit.tablecbkfetch.pfnGetCells = ptsHost.GetCells;
		tableobjInit.tablecbkfetch.pfnFInterruptFormattingTable = ptsHost.FInterruptFormattingTable;
		tableobjInit.tablecbkfetch.pfnCalcHorizontalBBoxOfRow = ptsHost.CalcHorizontalBBoxOfRow;
		tableobjInit.tablecbkcell.pfnFormatCellFinite = ptsHost.FormatCellFinite;
		tableobjInit.tablecbkcell.pfnFormatCellBottomless = ptsHost.FormatCellBottomless;
		tableobjInit.tablecbkcell.pfnUpdateBottomlessCell = ptsHost.UpdateBottomlessCell;
		tableobjInit.tablecbkcell.pfnCompareCells = ptsHost.CompareCells;
		tableobjInit.tablecbkcell.pfnClearUpdateInfoInCell = ptsHost.ClearUpdateInfoInCell;
		tableobjInit.tablecbkcell.pfnSetCellHeight = ptsHost.SetCellHeight;
		tableobjInit.tablecbkcell.pfnDestroyCell = ptsHost.DestroyCell;
		tableobjInit.tablecbkcell.pfnDuplicateCellBreakRecord = ptsHost.DuplicateCellBreakRecord;
		tableobjInit.tablecbkcell.pfnDestroyCellBreakRecord = ptsHost.DestroyCellBreakRecord;
		tableobjInit.tablecbkcell.pfnGetCellNumberFootnotes = ptsHost.GetCellNumberFootnotes;
		tableobjInit.tablecbkcell.pfnGetCellFootnoteInfo = IntPtr.Zero;
		tableobjInit.tablecbkcell.pfnGetCellFootnoteInfoWord = IntPtr.Zero;
		tableobjInit.tablecbkcell.pfnGetCellMinColumnBalancingStep = ptsHost.GetCellMinColumnBalancingStep;
		tableobjInit.tablecbkcell.pfnTransferDisplayInfoCell = ptsHost.TransferDisplayInfoCell;
	}
}
