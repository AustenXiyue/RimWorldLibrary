using System;
using Microsoft.Win32;
using MS.Internal.WindowsBase;

namespace MS.Utility;

internal static class EventTrace
{
	public enum LayoutSource : byte
	{
		LayoutManager,
		HwndSource_SetLayoutSize,
		HwndSource_WMSIZE
	}

	internal enum Level : byte
	{
		LogAlways = 0,
		Critical = 1,
		Error = 2,
		Warning = 3,
		Info = 4,
		Verbose = 5,
		PERF = 16,
		PERF_LOW = 17,
		PERF_MED = 18,
		PERF_HIGH = 19
	}

	[Flags]
	internal enum Keyword
	{
		KeywordGeneral = 1,
		KeywordPerf = 2,
		KeywordText = 4,
		KeywordInput = 8,
		KeywordAnnotation = 0x10,
		KeywordXamlBaml = 0x20,
		KeywordXPS = 0x40,
		KeywordAnimation = 0x80,
		KeywordLayout = 0x100,
		KeywordHosting = 0x400,
		KeywordHeapMeter = 0x800,
		KeywordGraphics = 0x1000,
		KeywordDispatcher = 0x2000
	}

	internal enum Event : ushort
	{
		WClientCreateVisual = 1,
		WClientAppCtor = 2,
		WClientAppRun = 3,
		WClientString = 4,
		WClientStringBegin = 5,
		WClientStringEnd = 6,
		WClientPropParentCheck = 7,
		UpdateVisualStateStart = 8,
		UpdateVisualStateEnd = 9,
		PerfElementIDName = 10,
		PerfElementIDAssignment = 11,
		WClientFontCache = 1001,
		WClientInputMessage = 2001,
		StylusEventQueued = 2002,
		TouchDownReported = 2003,
		TouchMoveReported = 2004,
		TouchUpReported = 2005,
		ManipulationReportFrame = 2006,
		ManipulationEventRaised = 2007,
		PenThreadPoolThreadAcquisition = 2008,
		CreateStickyNoteBegin = 3001,
		CreateStickyNoteEnd = 3002,
		DeleteTextNoteBegin = 3003,
		DeleteTextNoteEnd = 3004,
		DeleteInkNoteBegin = 3005,
		DeleteInkNoteEnd = 3006,
		CreateHighlightBegin = 3007,
		CreateHighlightEnd = 3008,
		ClearHighlightBegin = 3009,
		ClearHighlightEnd = 3010,
		LoadAnnotationsBegin = 3011,
		LoadAnnotationsEnd = 3012,
		AddAnnotationBegin = 3013,
		AddAnnotationEnd = 3014,
		DeleteAnnotationBegin = 3015,
		DeleteAnnotationEnd = 3016,
		GetAnnotationByIdBegin = 3017,
		GetAnnotationByIdEnd = 3018,
		GetAnnotationByLocBegin = 3019,
		GetAnnotationByLocEnd = 3020,
		GetAnnotationsBegin = 3021,
		GetAnnotationsEnd = 3022,
		SerializeAnnotationBegin = 3023,
		SerializeAnnotationEnd = 3024,
		DeserializeAnnotationBegin = 3025,
		DeserializeAnnotationEnd = 3026,
		UpdateAnnotationWithSNCBegin = 3027,
		UpdateAnnotationWithSNCEnd = 3028,
		UpdateSNCWithAnnotationBegin = 3029,
		UpdateSNCWithAnnotationEnd = 3030,
		AnnotationTextChangedBegin = 3031,
		AnnotationTextChangedEnd = 3032,
		AnnotationInkChangedBegin = 3033,
		AnnotationInkChangedEnd = 3034,
		AddAttachedSNBegin = 3035,
		AddAttachedSNEnd = 3036,
		RemoveAttachedSNBegin = 3037,
		RemoveAttachedSNEnd = 3038,
		AddAttachedHighlightBegin = 3039,
		AddAttachedHighlightEnd = 3040,
		RemoveAttachedHighlightBegin = 3041,
		RemoveAttachedHighlightEnd = 3042,
		AddAttachedMHBegin = 3043,
		AddAttachedMHEnd = 3044,
		RemoveAttachedMHBegin = 3045,
		RemoveAttachedMHEnd = 3046,
		WClientParseBamlBegin = 4001,
		WClientParseBamlEnd = 4002,
		WClientParseXmlBegin = 4003,
		WClientParseXmlEnd = 4004,
		WClientParseFefCrInstBegin = 4005,
		WClientParseFefCrInstEnd = 4006,
		WClientParseInstVisTreeBegin = 4007,
		WClientParseInstVisTreeEnd = 4008,
		WClientParseRdrCrInstBegin = 4009,
		WClientParseRdrCrInstEnd = 4010,
		WClientParseRdrCrInFTypBegin = 4011,
		WClientParseRdrCrInFTypEnd = 4012,
		WClientResourceFindBegin = 4013,
		WClientResourceFindEnd = 4014,
		WClientResourceCacheValue = 4015,
		WClientResourceCacheNull = 4016,
		WClientResourceCacheMiss = 4017,
		WClientResourceStock = 4018,
		WClientResourceBamlAssembly = 4019,
		WClientParseXamlBegin = 4020,
		WClientParseXamlBamlInfo = 4021,
		WClientParseXamlEnd = 4022,
		WClientDRXFlushPageStart = 5001,
		WClientDRXFlushPageStop = 5002,
		WClientDRXSerializeTreeStart = 5003,
		WClientDRXSerializeTreeEnd = 5004,
		WClientDRXGetVisualStart = 5005,
		WClientDRXGetVisualEnd = 5006,
		WClientDRXReleaseWriterStart = 5007,
		WClientDRXReleaseWriterEnd = 5008,
		WClientDRXGetPrintCapStart = 5009,
		WClientDRXGetPrintCapEnd = 5010,
		WClientDRXPTProviderStart = 5011,
		WClientDRXPTProviderEnd = 5012,
		WClientDRXRasterStart = 5013,
		WClientDRXRasterEnd = 5014,
		WClientDRXOpenPackageBegin = 5015,
		WClientDRXOpenPackageEnd = 5016,
		WClientDRXGetStreamBegin = 5017,
		WClientDRXGetStreamEnd = 5018,
		WClientDRXPageVisible = 5019,
		WClientDRXPageLoaded = 5020,
		WClientDRXInvalidateView = 5021,
		WClientDRXStyleCreated = 5022,
		WClientDRXFindBegin = 5023,
		WClientDRXFindEnd = 5024,
		WClientDRXZoom = 5025,
		WClientDRXEnsureOMBegin = 5026,
		WClientDRXEnsureOMEnd = 5027,
		WClientDRXTreeFlattenBegin = 5028,
		WClientDRXTreeFlattenEnd = 5029,
		WClientDRXAlphaFlattenBegin = 5030,
		WClientDRXAlphaFlattenEnd = 5031,
		WClientDRXGetDevModeBegin = 5032,
		WClientDRXGetDevModeEnd = 5033,
		WClientDRXStartDocBegin = 5034,
		WClientDRXStartDocEnd = 5035,
		WClientDRXEndDocBegin = 5036,
		WClientDRXEndDocEnd = 5037,
		WClientDRXStartPageBegin = 5038,
		WClientDRXStartPageEnd = 5039,
		WClientDRXEndPageBegin = 5040,
		WClientDRXEndPageEnd = 5041,
		WClientDRXCommitPageBegin = 5042,
		WClientDRXCommitPageEnd = 5043,
		WClientDRXConvertFontBegin = 5044,
		WClientDRXConvertFontEnd = 5045,
		WClientDRXConvertImageBegin = 5046,
		WClientDRXConvertImageEnd = 5047,
		WClientDRXSaveXpsBegin = 5048,
		WClientDRXSaveXpsEnd = 5049,
		WClientDRXLoadPrimitiveBegin = 5050,
		WClientDRXLoadPrimitiveEnd = 5051,
		WClientDRXSavePageBegin = 5052,
		WClientDRXSavePageEnd = 5053,
		WClientDRXSerializationBegin = 5054,
		WClientDRXSerializationEnd = 5055,
		WClientDRXReadStreamBegin = 5056,
		WClientDRXReadStreamEnd = 5057,
		WClientDRXGetPageBegin = 5058,
		WClientDRXGetPageEnd = 5059,
		WClientDRXLineDown = 5060,
		WClientDRXPageDown = 5061,
		WClientDRXPageJump = 5062,
		WClientDRXLayoutBegin = 5063,
		WClientDRXLayoutEnd = 5064,
		WClientDRXInstantiated = 5065,
		WClientTimeManagerTickBegin = 6001,
		WClientTimeManagerTickEnd = 6002,
		WClientLayoutBegin = 7001,
		WClientLayoutEnd = 7002,
		WClientMeasureBegin = 7005,
		WClientMeasureAbort = 7006,
		WClientMeasureEnd = 7007,
		WClientMeasureElementBegin = 7008,
		WClientMeasureElementEnd = 7009,
		WClientArrangeBegin = 7010,
		WClientArrangeAbort = 7011,
		WClientArrangeEnd = 7012,
		WClientArrangeElementBegin = 7013,
		WClientArrangeElementEnd = 7014,
		WClientLayoutAbort = 7015,
		WClientLayoutFireSizeChangedBegin = 7016,
		WClientLayoutFireSizeChangedEnd = 7017,
		WClientLayoutFireLayoutUpdatedBegin = 7018,
		WClientLayoutFireLayoutUpdatedEnd = 7019,
		WClientLayoutFireAutomationEventsBegin = 7020,
		WClientLayoutFireAutomationEventsEnd = 7021,
		WClientLayoutException = 7022,
		WClientLayoutInvalidated = 7023,
		WpfHostUm_WinMainStart = 9003,
		WpfHostUm_WinMainEnd = 9004,
		WpfHostUm_InvokingBrowser = 9005,
		WpfHostUm_LaunchingRestrictedProcess = 9006,
		WpfHostUm_EnteringMessageLoop = 9007,
		WpfHostUm_ClassFactoryCreateInstance = 9008,
		WpfHostUm_ReadingDeplManifestStart = 9009,
		WpfHostUm_ReadingDeplManifestEnd = 9010,
		WpfHostUm_ReadingAppManifestStart = 9011,
		WpfHostUm_ReadingAppManifestEnd = 9012,
		WpfHostUm_ParsingMarkupVersionStart = 9013,
		WpfHostUm_ParsingMarkupVersionEnd = 9014,
		WpfHostUm_IPersistFileLoad = 9015,
		WpfHostUm_IPersistMonikerLoadStart = 9016,
		WpfHostUm_IPersistMonikerLoadEnd = 9017,
		WpfHostUm_BindProgress = 9018,
		WpfHostUm_OnStopBinding = 9019,
		WpfHostUm_VersionAttach = 9020,
		WpfHostUm_VersionActivateStart = 9021,
		WpfHostUm_VersionActivateEnd = 9022,
		WpfHostUm_StartingCLRStart = 9023,
		WpfHostUm_StartingCLREnd = 9024,
		WpfHostUm_IHlinkTargetNavigateStart = 9025,
		WpfHostUm_IHlinkTargetNavigateEnd = 9026,
		WpfHostUm_ReadyStateChanged = 9027,
		WpfHostUm_InitDocHostStart = 9028,
		WpfHostUm_InitDocHostEnd = 9029,
		WpfHostUm_MergingMenusStart = 9030,
		WpfHostUm_MergingMenusEnd = 9031,
		WpfHostUm_UIActivationStart = 9032,
		WpfHostUm_UIActivationEnd = 9033,
		WpfHostUm_LoadingResourceDLLStart = 9034,
		WpfHostUm_LoadingResourceDLLEnd = 9035,
		WpfHostUm_OleCmdQueryStatusStart = 9036,
		WpfHostUm_OleCmdQueryStatusEnd = 9037,
		WpfHostUm_OleCmdExecStart = 9038,
		WpfHostUm_OleCmdExecEnd = 9039,
		WpfHostUm_ProgressPageShown = 9040,
		WpfHostUm_AdHocProfile1Start = 9041,
		WpfHostUm_AdHocProfile1End = 9042,
		WpfHostUm_AdHocProfile2Start = 9043,
		WpfHostUm_AdHocProfile2End = 9044,
		WpfHost_DocObjHostCreated = 9045,
		WpfHost_XappLauncherAppStartup = 9046,
		WpfHost_XappLauncherAppExit = 9047,
		WpfHost_DocObjHostRunApplicationStart = 9048,
		WpfHost_DocObjHostRunApplicationEnd = 9049,
		WpfHost_ClickOnceActivationStart = 9050,
		WpfHost_ClickOnceActivationEnd = 9051,
		WpfHost_InitAppProxyStart = 9052,
		WpfHost_InitAppProxyEnd = 9053,
		WpfHost_AppProxyCtor = 9054,
		WpfHost_RootBrowserWindowSetupStart = 9055,
		WpfHost_RootBrowserWindowSetupEnd = 9056,
		WpfHost_AppProxyRunStart = 9057,
		WpfHost_AppProxyRunEnd = 9058,
		WpfHost_AppDomainManagerCctor = 9059,
		WpfHost_ApplicationActivatorCreateInstanceStart = 9060,
		WpfHost_ApplicationActivatorCreateInstanceEnd = 9061,
		WpfHost_DetermineApplicationTrustStart = 9062,
		WpfHost_DetermineApplicationTrustEnd = 9063,
		WpfHost_FirstTimeActivation = 9064,
		WpfHost_GetDownloadPageStart = 9065,
		WpfHost_GetDownloadPageEnd = 9066,
		WpfHost_DownloadDeplManifestStart = 9067,
		WpfHost_DownloadDeplManifestEnd = 9068,
		WpfHost_AssertAppRequirementsStart = 9069,
		WpfHost_AssertAppRequirementsEnd = 9070,
		WpfHost_DownloadApplicationStart = 9071,
		WpfHost_DownloadApplicationEnd = 9072,
		WpfHost_DownloadProgressUpdate = 9073,
		WpfHost_XappLauncherAppNavigated = 9074,
		WpfHost_UpdateBrowserCommandsStart = 9077,
		WpfHost_UpdateBrowserCommandsEnd = 9078,
		WpfHost_PostShutdown = 9079,
		WpfHost_AbortingActivation = 9080,
		WpfHost_IBHSRunStart = 9081,
		WpfHost_IBHSRunEnd = 9082,
		Wpf_NavigationAsyncWorkItem = 9083,
		Wpf_NavigationWebResponseReceived = 9084,
		Wpf_NavigationEnd = 9085,
		Wpf_NavigationContentRendered = 9086,
		Wpf_NavigationStart = 9087,
		Wpf_NavigationLaunchBrowser = 9088,
		Wpf_NavigationPageFunctionReturn = 9089,
		DrawBitmapInfo = 11001,
		BitmapCopyInfo = 11002,
		SetClipInfo = 11003,
		DWMDraw_ClearStart = 11004,
		DWMDraw_ClearEnd = 11005,
		DWMDraw_BitmapStart = 11006,
		DWMDraw_BitmapEnd = 11007,
		DWMDraw_RectangleStart = 11008,
		DWMDraw_RectangleEnd = 11009,
		DWMDraw_GeometryStart = 11010,
		DWMDraw_GeometryEnd = 11011,
		DWMDraw_ImageStart = 11012,
		DWMDraw_ImageEnd = 11013,
		DWMDraw_GlyphRunStart = 11014,
		DWMDraw_GlyphRunEnd = 11015,
		DWMDraw_BeginLayerStart = 11016,
		DWMDraw_BeginLayerEnd = 11017,
		DWMDraw_EndLayerStart = 11018,
		DWMDraw_EndLayerEnd = 11019,
		DWMDraw_ClippedBitmapStart = 11020,
		DWMDraw_ClippedBitmapEnd = 11021,
		DWMDraw_Info = 11022,
		LayerEventStart = 11023,
		LayerEventEnd = 11024,
		WClientDesktopRTCreateBegin = 11025,
		WClientDesktopRTCreateEnd = 11026,
		WClientUceProcessQueueBegin = 11027,
		WClientUceProcessQueueEnd = 11028,
		WClientUceProcessQueueInfo = 11029,
		WClientUcePrecomputeBegin = 11030,
		WClientUcePrecomputeEnd = 11031,
		WClientUceRenderBegin = 11032,
		WClientUceRenderEnd = 11033,
		WClientUcePresentBegin = 11034,
		WClientUcePresentEnd = 11035,
		WClientUceResponse = 11036,
		WClientUceCheckDeviceStateInfo = 11037,
		VisualCacheAlloc = 11038,
		VisualCacheUpdate = 11039,
		CreateChannel = 11040,
		CreateOrAddResourceOnChannel = 11041,
		CreateWpfGfxResource = 11042,
		ReleaseOnChannel = 11043,
		UnexpectedSoftwareFallback = 11044,
		WClientInterlockedRenderBegin = 11045,
		WClientInterlockedRenderEnd = 11046,
		WClientRenderHandlerBegin = 11047,
		WClientRenderHandlerEnd = 11048,
		WClientAnimRenderHandlerBegin = 11049,
		WClientAnimRenderHandlerEnd = 11050,
		WClientMediaRenderBegin = 11051,
		WClientMediaRenderEnd = 11052,
		WClientPostRender = 11053,
		WClientQPCFrequency = 11054,
		WClientPrecomputeSceneBegin = 11055,
		WClientPrecomputeSceneEnd = 11056,
		WClientCompileSceneBegin = 11057,
		WClientCompileSceneEnd = 11058,
		WClientUIResponse = 11059,
		WClientUICommitChannel = 11060,
		WClientUceNotifyPresent = 11061,
		WClientScheduleRender = 11062,
		WClientOnRenderBegin = 11063,
		WClientOnRenderEnd = 11064,
		WClientCreateIRT = 11065,
		WClientPotentialIRTResource = 11066,
		WClientUIContextDispatchBegin = 12001,
		WClientUIContextDispatchEnd = 12002,
		WClientUIContextPost = 12003,
		WClientUIContextAbort = 12004,
		WClientUIContextPromote = 12005,
		WClientUIContextIdle = 12006
	}

	internal static readonly TraceProvider EventProvider;

	internal static void EasyTraceEvent(Keyword keywords, Event eventID)
	{
		if (IsEnabled(keywords, Level.Info))
		{
			EventProvider.TraceEvent(eventID, keywords, Level.Info);
		}
	}

	internal static void EasyTraceEvent(Keyword keywords, Level level, Event eventID)
	{
		if (IsEnabled(keywords, level))
		{
			EventProvider.TraceEvent(eventID, keywords, level);
		}
	}

	internal static void EasyTraceEvent<T1>(Keyword keywords, Event eventID, T1 param1)
	{
		if (IsEnabled(keywords, Level.Info))
		{
			EventProvider.TraceEvent(eventID, keywords, Level.Info, param1);
		}
	}

	internal static void EasyTraceEvent<T1>(Keyword keywords, Level level, Event eventID, T1 param1)
	{
		if (IsEnabled(keywords, level))
		{
			EventProvider.TraceEvent(eventID, keywords, level, param1);
		}
	}

	internal static void EasyTraceEvent<T1, T2>(Keyword keywords, Event eventID, T1 param1, T2 param2)
	{
		if (IsEnabled(keywords, Level.Info))
		{
			EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2);
		}
	}

	internal static void EasyTraceEvent<T1, T2>(Keyword keywords, Level level, Event eventID, T1 param1, T2 param2)
	{
		if (IsEnabled(keywords, Level.Info))
		{
			EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2);
		}
	}

	internal static void EasyTraceEvent<T1, T2, T3>(Keyword keywords, Event eventID, T1 param1, T2 param2, T3 param3)
	{
		if (IsEnabled(keywords, Level.Info))
		{
			EventProvider.TraceEvent(eventID, keywords, Level.Info, param1, param2, param3);
		}
	}

	internal static bool IsEnabled(Keyword flag, Level level)
	{
		return EventProvider.IsEnabled(flag, level);
	}

	static EventTrace()
	{
		Guid providerGuid = new Guid("E13B77A8-14B6-11DE-8069-001B212B5009");
		if (Environment.OSVersion.Version.Major < 6 || IsClassicETWRegistryEnabled())
		{
			EventProvider = new ClassicTraceProvider();
		}
		else
		{
			EventProvider = new ManifestTraceProvider();
		}
		EventProvider.Register(providerGuid);
	}

	private static bool IsClassicETWRegistryEnabled()
	{
		string keyName = "HKEY_CURRENT_USER\\Software\\Microsoft\\Avalon.Graphics\\";
		return object.Equals(1, Registry.GetValue(keyName, "ClassicETW", 0));
	}

	internal static Guid GetGuidForEvent(Event arg)
	{
		switch (arg)
		{
		case Event.WClientCreateVisual:
			return new Guid(767479650, 20970, 18746, 141, 208, 75, 238, 28, 203, 232, 170);
		case Event.WClientAppCtor:
			return new Guid(4193274054u, 8209, 19722, 129, 42, 35, 164, 164, 216, 1, 245);
		case Event.WClientAppRun:
			return new Guid(145168854u, 60025, 19132, 151, 153, 56, 237, 237, 96, 33, 51);
		case Event.WClientString:
		case Event.WClientStringBegin:
		case Event.WClientStringEnd:
			return new Guid(1799094872u, 40411, 17785, 134, 96, 65, 195, 173, 162, 92, 52);
		case Event.WClientPropParentCheck:
			return new Guid(2199644679u, 23084, 17228, 142, 248, 126, 186, 65, 200, 129, 251);
		case Event.UpdateVisualStateStart:
		case Event.UpdateVisualStateEnd:
			return new Guid(128441699u, 46381, 20223, 172, 63, 36, 72, 218, 249, 116, 153);
		case Event.PerfElementIDName:
		case Event.PerfElementIDAssignment:
			return new Guid(2690701696u, 19480, 18771, 129, 223, 207, 223, 211, 69, 197, 202);
		case Event.WClientFontCache:
			return new Guid(4080410886u, 47201, 18816, 154, 172, 177, 239, 11, 171, 117, 170);
		case Event.WClientInputMessage:
			return new Guid(1254595500, 32251, 17410, 169, 16, 253, 175, 225, 111, 41, 178);
		case Event.StylusEventQueued:
			return new Guid(1106039032u, 62886, 19118, 158, 133, 202, 236, 225, 25, 184, 83);
		case Event.TouchDownReported:
			return new Guid(2205864826u, 36079, 19468, 148, 74, 174, 59, 31, 28, 37, 87);
		case Event.TouchMoveReported:
			return new Guid(4252077631u, 21602, 16935, 166, 16, 117, 213, 191, 137, 103, 162);
		case Event.TouchUpReported:
			return new Guid(3266086307u, 57707, 19719, 144, 222, 30, 104, 99, 148, 184, 49);
		case Event.ManipulationReportFrame:
			return new Guid(3783643286u, 28345, 16830, 129, 244, 117, 217, 36, 66, 88, 114);
		case Event.ManipulationEventRaised:
			return new Guid(1375110635u, 45329, 16397, 179, 227, 70, 2, 47, 102, 168, 148);
		case Event.PenThreadPoolThreadAcquisition:
			return new Guid(1815239734, 19807, 17192, 177, 198, 225, 100, 121, 109, 254, 43);
		case Event.CreateStickyNoteBegin:
		case Event.CreateStickyNoteEnd:
			return new Guid(3822845868u, 7826, 20296, 166, 90, 194, 144, 189, 95, 95, 21);
		case Event.DeleteTextNoteBegin:
		case Event.DeleteTextNoteEnd:
			return new Guid(1982243577u, 39521, 17315, 183, 204, 187, 132, 194, 73, 58, 167);
		case Event.DeleteInkNoteBegin:
		case Event.DeleteInkNoteEnd:
			return new Guid(3212716691u, 40298, 17726, 186, 219, 63, 143, 96, 7, 92, 242);
		case Event.CreateHighlightBegin:
		case Event.CreateHighlightEnd:
			return new Guid(3265654200u, 44147, 16879, 169, 67, 168, 164, 159, 162, 132, 177);
		case Event.ClearHighlightBegin:
		case Event.ClearHighlightEnd:
			return new Guid(3785724231u, 53901, 19551, 185, 128, 105, 27, 226, 253, 66, 8);
		case Event.LoadAnnotationsBegin:
		case Event.LoadAnnotationsEnd:
			return new Guid(3476695102u, 49156, 20093, 179, 185, 204, 155, 88, 42, 74, 95);
		case Event.AddAnnotationBegin:
		case Event.AddAnnotationEnd:
			return new Guid(2404069290u, 9430, 20194, 153, 53, 187, 248, 69, 247, 88, 162);
		case Event.DeleteAnnotationBegin:
		case Event.DeleteAnnotationEnd:
			return new Guid(1300439600u, 38186, 17508, 128, 175, 170, 178, 172, 134, 23, 3);
		case Event.GetAnnotationByIdBegin:
		case Event.GetAnnotationByIdEnd:
			return new Guid(1025996095u, 60298, 20085, 157, 91, 130, 251, 165, 92, 222, 209);
		case Event.GetAnnotationByLocBegin:
		case Event.GetAnnotationByLocEnd:
			return new Guid(1947877820u, 36557, 17361, 167, 241, 210, 250, 202, 115, 98, 239);
		case Event.GetAnnotationsBegin:
		case Event.GetAnnotationsEnd:
			return new Guid(3449774103u, 32356, 19553, 185, 237, 92, 47, 200, 196, 216, 73);
		case Event.SerializeAnnotationBegin:
		case Event.SerializeAnnotationEnd:
			return new Guid(21533259, 23530, 17385, 179, 237, 57, 156, 161, 59, 53, 235);
		case Event.DeserializeAnnotationBegin:
		case Event.DeserializeAnnotationEnd:
			return new Guid(775078485u, 55003, 19943, 158, 98, 149, 134, 55, 119, 120, 213);
		case Event.UpdateAnnotationWithSNCBegin:
		case Event.UpdateAnnotationWithSNCEnd:
			return new Guid(543033944, 15485, 18781, 179, 237, 24, 195, 251, 56, 146, 63);
		case Event.UpdateSNCWithAnnotationBegin:
		case Event.UpdateSNCWithAnnotationEnd:
			return new Guid(1505966030u, 40130, 19078, 155, 250, 6, 31, 233, 84, 8, 107);
		case Event.AnnotationTextChangedBegin:
		case Event.AnnotationTextChangedEnd:
			return new Guid(2344161977u, 14813, 16904, 173, 98, 190, 102, 254, 91, 123, 165);
		case Event.AnnotationInkChangedBegin:
		case Event.AnnotationInkChangedEnd:
			return new Guid(304669012u, 61809, 17006, 182, 114, 94, 225, 155, 117, 94, 223);
		case Event.AddAttachedSNBegin:
		case Event.AddAttachedSNEnd:
			return new Guid(2628149494u, 36220, 19088, 169, 47, 116, 72, 45, 156, 193, 207);
		case Event.RemoveAttachedSNBegin:
		case Event.RemoveAttachedSNEnd:
			return new Guid(2353818103u, 4485, 18143, 165, 245, 227, 26, 199, 233, 108, 7);
		case Event.AddAttachedHighlightBegin:
		case Event.AddAttachedHighlightEnd:
			return new Guid(1456655077, 24256, 17659, 152, 194, 69, 62, 135, 160, 135, 123);
		case Event.RemoveAttachedHighlightBegin:
		case Event.RemoveAttachedHighlightEnd:
			return new Guid(1283576976u, 36868, 18897, 135, 215, 40, 157, 83, 163, 20, 239);
		case Event.AddAttachedMHBegin:
		case Event.AddAttachedMHEnd:
			return new Guid(2124535112u, 51735, 51735, 161, 168, 241, 133, 125, 182, 48, 46);
		case Event.RemoveAttachedMHBegin:
		case Event.RemoveAttachedMHEnd:
			return new Guid(694974817u, 47477, 17675, 137, 117, 191, 134, 43, 108, 113, 89);
		case Event.WClientParseBamlBegin:
		case Event.WClientParseBamlEnd:
			return new Guid(2317236981u, 14957, 17794, 134, 209, 89, 1, 71, 30, 187, 222);
		case Event.WClientParseXmlBegin:
		case Event.WClientParseXmlEnd:
			return new Guid(3213288895u, 16308, 17455, 163, 74, 178, 7, 163, 177, 156, 59);
		case Event.WClientParseFefCrInstBegin:
		case Event.WClientParseFefCrInstEnd:
			return new Guid(4149563745u, 27674, 18962, 130, 141, 132, 146, 167, 105, 154, 73);
		case Event.WClientParseInstVisTreeBegin:
		case Event.WClientParseInstVisTreeEnd:
			return new Guid(2831399360u, 22059, 17673, 190, 203, 160, 142, 72, 26, 114, 115);
		case Event.WClientParseRdrCrInstBegin:
		case Event.WClientParseRdrCrInstEnd:
			return new Guid(2343105820u, 1909, 19167, 158, 237, 177, 101, 76, 160, 136, 245);
		case Event.WClientParseRdrCrInFTypBegin:
		case Event.WClientParseRdrCrInFTypEnd:
			return new Guid(228679000u, 50087, 16606, 145, 19, 114, 219, 12, 74, 147, 81);
		case Event.WClientResourceFindBegin:
		case Event.WClientResourceFindEnd:
			return new Guid(579702997, 32281, 17536, 158, 86, 58, 242, 233, 15, 141, 166);
		case Event.WClientResourceCacheValue:
			return new Guid(992296493, 29349, 18590, 140, 101, 86, 193, 230, 200, 89, 181);
		case Event.WClientResourceCacheNull:
			return new Guid(2019993179, 12088, 17334, 171, 210, 223, 67, 59, 188, 160, 115);
		case Event.WClientResourceCacheMiss:
			return new Guid(69236063u, 54294, 20245, 147, 159, 62, 44, 211, 252, 234, 35);
		case Event.WClientResourceStock:
			return new Guid(116457188, 29405, 18434, 189, 61, 9, 133, 19, 159, 169, 26);
		case Event.WClientResourceBamlAssembly:
			return new Guid(434062195, 26240, 18948, 140, 119, 210, 246, 128, 156, 167, 3);
		case Event.WClientParseXamlBegin:
			return new Guid(828646778u, 51646, 19510, 157, 143, 9, 177, 138, 200, 128, 166);
		case Event.WClientParseXamlBamlInfo:
			return new Guid(12654544u, 33332, 20218, 172, 227, 115, 186, 28, 101, 95, 40);
		case Event.WClientParseXamlEnd:
			return new Guid(828646778u, 51646, 19510, 157, 143, 9, 177, 138, 200, 128, 166);
		case Event.WClientDRXFlushPageStart:
		case Event.WClientDRXFlushPageStop:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 88);
		case Event.WClientDRXSerializeTreeStart:
		case Event.WClientDRXSerializeTreeEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 90);
		case Event.WClientDRXGetVisualStart:
		case Event.WClientDRXGetVisualEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 89);
		case Event.WClientDRXReleaseWriterStart:
		case Event.WClientDRXReleaseWriterEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 91);
		case Event.WClientDRXGetPrintCapStart:
		case Event.WClientDRXGetPrintCapEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 92);
		case Event.WClientDRXPTProviderStart:
		case Event.WClientDRXPTProviderEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 93);
		case Event.WClientDRXRasterStart:
		case Event.WClientDRXRasterEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 94);
		case Event.WClientDRXOpenPackageBegin:
		case Event.WClientDRXOpenPackageEnd:
			return new Guid(730822131u, 63737, 16501, 185, 20, 90, 232, 83, 199, 98, 118);
		case Event.WClientDRXGetStreamBegin:
		case Event.WClientDRXGetStreamEnd:
			return new Guid(1061490923u, 40680, 19328, 158, 199, 119, 94, 254, 177, 186, 114);
		case Event.WClientDRXPageVisible:
			return new Guid(719832577, 2796, 19609, 186, 128, 46, 202, 113, 45, 27, 151);
		case Event.WClientDRXPageLoaded:
			return new Guid(1711441477u, 57378, 19856, 167, 189, 168, 204, 218, 205, 178, 225);
		case Event.WClientDRXInvalidateView:
			return new Guid(1004762127, 2609, 19746, 162, 163, 77, 75, 109, 58, 184, 153);
		case Event.WClientDRXStyleCreated:
			return new Guid(1769176117, 5686, 17342, 163, 82, 66, 140, 163, 109, 27, 44);
		case Event.WClientDRXFindBegin:
		case Event.WClientDRXFindEnd:
			return new Guid(4287560564u, 61354, 16973, 144, 34, 238, 141, 33, 173, 128, 78);
		case Event.WClientDRXZoom:
			return new Guid(777012641u, 36268, 19600, 153, 149, 50, 96, 222, 22, 108, 143);
		case Event.WClientDRXEnsureOMBegin:
		case Event.WClientDRXEnsureOMEnd:
			return new Guid(686008507u, 44731, 18664, 134, 182, 50, 117, 155, 71, 252, 190);
		case Event.WClientDRXTreeFlattenBegin:
		case Event.WClientDRXTreeFlattenEnd:
			return new Guid(3025499220u, 8491, 20311, 185, 202, 43, 169, 213, 130, 115, 179);
		case Event.WClientDRXAlphaFlattenBegin:
		case Event.WClientDRXAlphaFlattenEnd:
			return new Guid(808387305u, 61477, 16515, 171, 213, 44, 227, 170, 169, 163, 207);
		case Event.WClientDRXGetDevModeBegin:
		case Event.WClientDRXGetDevModeEnd:
			return new Guid(1463724252u, 56172, 17088, 145, 248, 150, 78, 57, 203, 106, 112);
		case Event.WClientDRXStartDocBegin:
		case Event.WClientDRXStartDocEnd:
			return new Guid(4093355622u, 64015, 17543, 184, 70, 159, 32, 72, 17, 191, 61);
		case Event.WClientDRXEndDocBegin:
		case Event.WClientDRXEndDocEnd:
			return new Guid(1950208975u, 48078, 20073, 164, 219, 133, 34, 110, 198, 164, 69);
		case Event.WClientDRXStartPageBegin:
		case Event.WClientDRXStartPageEnd:
			return new Guid(1392760146, 10411, 19884, 139, 205, 15, 125, 86, 117, 161, 87);
		case Event.WClientDRXEndPageBegin:
		case Event.WClientDRXEndPageEnd:
			return new Guid(3792690676u, 6054, 20063, 134, 147, 61, 215, 203, 4, 148, 34);
		case Event.WClientDRXCommitPageBegin:
		case Event.WClientDRXCommitPageEnd:
			return new Guid(2105467277u, 44709, 18751, 158, 242, 187, 219, 54, 252, 170, 120);
		case Event.WClientDRXConvertFontBegin:
		case Event.WClientDRXConvertFontEnd:
			return new Guid(2298228034u, 45534, 17800, 140, 59, 220, 91, 236, 3, 169, 172);
		case Event.WClientDRXConvertImageBegin:
		case Event.WClientDRXConvertImageEnd:
			return new Guid(402513884u, 41406, 17331, 178, 238, 245, 232, 155, 123, 27, 38);
		case Event.WClientDRXSaveXpsBegin:
		case Event.WClientDRXSaveXpsEnd:
			return new Guid(3120767189u, 8852, 16487, 139, 25, 239, 156, 221, 173, 75, 26);
		case Event.WClientDRXLoadPrimitiveBegin:
		case Event.WClientDRXLoadPrimitiveEnd:
			return new Guid(3501657241u, 17678, 18546, 162, 212, 251, 251, 29, 199, 151, 250);
		case Event.WClientDRXSavePageBegin:
		case Event.WClientDRXSavePageEnd:
			return new Guid(2967725963u, 39623, 18236, 137, 3, 181, 210, 18, 57, 158, 59);
		case Event.WClientDRXSerializationBegin:
		case Event.WClientDRXSerializationEnd:
			return new Guid(86452076u, 54260, 17043, 184, 140, 236, 223, 124, 172, 68, 48);
		case Event.WClientDRXReadStreamBegin:
		case Event.WClientDRXReadStreamEnd:
			return new Guid(3266400293u, 30738, 20036, 139, 104, 125, 115, 67, 3, 67, 138);
		case Event.WClientDRXGetPageBegin:
		case Event.WClientDRXGetPageEnd:
			return new Guid(2697032281u, 50865, 18512, 169, 171, 19, 101, 159, 230, 220, 88);
		case Event.WClientDRXLineDown:
			return new Guid(3061494060u, 10687, 16416, 182, 120, 240, 67, 146, 91, 130, 53);
		case Event.WClientDRXPageDown:
			return new Guid(3620596562u, 23459, 19970, 177, 20, 56, 90, 97, 231, 186, 157);
		case Event.WClientDRXPageJump:
			return new Guid(4033392951u, 31497, 17569, 132, 208, 79, 241, 89, 46, 10, 193);
		case Event.WClientDRXLayoutBegin:
		case Event.WClientDRXLayoutEnd:
			return new Guid(888924736, 568, 18831, 177, 42, 99, 31, 90, 142, 249, 165);
		case Event.WClientDRXInstantiated:
			return new Guid(2649126881u, 37194, 17004, 188, 217, 44, 205, 234, 54, 72, 223);
		case Event.WClientTimeManagerTickBegin:
		case Event.WClientTimeManagerTickEnd:
			return new Guid(3929754470u, 45663, 20061, 139, 212, 236, 98, 187, 68, 88, 62);
		case Event.WClientLayoutBegin:
		case Event.WClientLayoutEnd:
			return new Guid(2750265104u, 8700, 20369, 151, 244, 172, 43, 13, 241, 194, 15);
		case Event.WClientMeasureBegin:
		case Event.WClientMeasureAbort:
		case Event.WClientMeasureEnd:
		case Event.WClientMeasureElementBegin:
		case Event.WClientMeasureElementEnd:
			return new Guid(805693051, 4764, 19693, 188, 170, 145, 215, 215, 59, 21, 68);
		case Event.WClientArrangeBegin:
		case Event.WClientArrangeAbort:
		case Event.WClientArrangeEnd:
		case Event.WClientArrangeElementBegin:
		case Event.WClientArrangeElementEnd:
			return new Guid(1259271121, 3259, 18503, 185, 143, 22, 64, 142, 126, 131, 243);
		case Event.WClientLayoutAbort:
		case Event.WClientLayoutFireSizeChangedBegin:
		case Event.WClientLayoutFireSizeChangedEnd:
		case Event.WClientLayoutFireLayoutUpdatedBegin:
		case Event.WClientLayoutFireLayoutUpdatedEnd:
		case Event.WClientLayoutFireAutomationEventsBegin:
		case Event.WClientLayoutFireAutomationEventsEnd:
		case Event.WClientLayoutException:
		case Event.WClientLayoutInvalidated:
			return new Guid(2750265104u, 8700, 20369, 151, 244, 172, 43, 13, 241, 194, 15);
		case Event.WpfHostUm_WinMainStart:
		case Event.WpfHostUm_WinMainEnd:
		case Event.WpfHostUm_InvokingBrowser:
		case Event.WpfHostUm_LaunchingRestrictedProcess:
		case Event.WpfHostUm_EnteringMessageLoop:
		case Event.WpfHostUm_ClassFactoryCreateInstance:
		case Event.WpfHostUm_ReadingDeplManifestStart:
		case Event.WpfHostUm_ReadingDeplManifestEnd:
		case Event.WpfHostUm_ReadingAppManifestStart:
		case Event.WpfHostUm_ReadingAppManifestEnd:
		case Event.WpfHostUm_ParsingMarkupVersionStart:
		case Event.WpfHostUm_ParsingMarkupVersionEnd:
		case Event.WpfHostUm_IPersistFileLoad:
		case Event.WpfHostUm_IPersistMonikerLoadStart:
		case Event.WpfHostUm_IPersistMonikerLoadEnd:
		case Event.WpfHostUm_BindProgress:
		case Event.WpfHostUm_OnStopBinding:
		case Event.WpfHostUm_VersionAttach:
		case Event.WpfHostUm_VersionActivateStart:
		case Event.WpfHostUm_VersionActivateEnd:
		case Event.WpfHostUm_StartingCLRStart:
		case Event.WpfHostUm_StartingCLREnd:
		case Event.WpfHostUm_IHlinkTargetNavigateStart:
		case Event.WpfHostUm_IHlinkTargetNavigateEnd:
		case Event.WpfHostUm_ReadyStateChanged:
		case Event.WpfHostUm_InitDocHostStart:
		case Event.WpfHostUm_InitDocHostEnd:
		case Event.WpfHostUm_MergingMenusStart:
		case Event.WpfHostUm_MergingMenusEnd:
		case Event.WpfHostUm_UIActivationStart:
		case Event.WpfHostUm_UIActivationEnd:
		case Event.WpfHostUm_LoadingResourceDLLStart:
		case Event.WpfHostUm_LoadingResourceDLLEnd:
		case Event.WpfHostUm_OleCmdQueryStatusStart:
		case Event.WpfHostUm_OleCmdQueryStatusEnd:
		case Event.WpfHostUm_OleCmdExecStart:
		case Event.WpfHostUm_OleCmdExecEnd:
		case Event.WpfHostUm_ProgressPageShown:
		case Event.WpfHostUm_AdHocProfile1Start:
		case Event.WpfHostUm_AdHocProfile1End:
		case Event.WpfHostUm_AdHocProfile2Start:
		case Event.WpfHostUm_AdHocProfile2End:
			return new Guid(3978631008u, 31676, 19237, 131, 40, 205, 127, 39, 31, 238, 137);
		case Event.WpfHost_DocObjHostCreated:
		case Event.WpfHost_XappLauncherAppStartup:
		case Event.WpfHost_XappLauncherAppExit:
		case Event.WpfHost_DocObjHostRunApplicationStart:
		case Event.WpfHost_DocObjHostRunApplicationEnd:
		case Event.WpfHost_ClickOnceActivationStart:
		case Event.WpfHost_ClickOnceActivationEnd:
		case Event.WpfHost_InitAppProxyStart:
		case Event.WpfHost_InitAppProxyEnd:
		case Event.WpfHost_AppProxyCtor:
		case Event.WpfHost_RootBrowserWindowSetupStart:
		case Event.WpfHost_RootBrowserWindowSetupEnd:
		case Event.WpfHost_AppProxyRunStart:
		case Event.WpfHost_AppProxyRunEnd:
		case Event.WpfHost_AppDomainManagerCctor:
		case Event.WpfHost_ApplicationActivatorCreateInstanceStart:
		case Event.WpfHost_ApplicationActivatorCreateInstanceEnd:
		case Event.WpfHost_DetermineApplicationTrustStart:
		case Event.WpfHost_DetermineApplicationTrustEnd:
		case Event.WpfHost_FirstTimeActivation:
		case Event.WpfHost_GetDownloadPageStart:
		case Event.WpfHost_GetDownloadPageEnd:
		case Event.WpfHost_DownloadDeplManifestStart:
		case Event.WpfHost_DownloadDeplManifestEnd:
		case Event.WpfHost_AssertAppRequirementsStart:
		case Event.WpfHost_AssertAppRequirementsEnd:
		case Event.WpfHost_DownloadApplicationStart:
		case Event.WpfHost_DownloadApplicationEnd:
		case Event.WpfHost_DownloadProgressUpdate:
		case Event.WpfHost_XappLauncherAppNavigated:
		case Event.WpfHost_UpdateBrowserCommandsStart:
		case Event.WpfHost_UpdateBrowserCommandsEnd:
		case Event.WpfHost_PostShutdown:
		case Event.WpfHost_AbortingActivation:
		case Event.WpfHost_IBHSRunStart:
		case Event.WpfHost_IBHSRunEnd:
			return new Guid(1610003845, 32697, 16777, 190, 179, 84, 200, 44, 228, 215, 209);
		case Event.Wpf_NavigationAsyncWorkItem:
		case Event.Wpf_NavigationWebResponseReceived:
		case Event.Wpf_NavigationEnd:
		case Event.Wpf_NavigationContentRendered:
		case Event.Wpf_NavigationStart:
		case Event.Wpf_NavigationLaunchBrowser:
		case Event.Wpf_NavigationPageFunctionReturn:
			return new Guid(1878760485, 23690, 16529, 152, 156, 91, 89, 106, 178, 134, 160);
		case Event.DrawBitmapInfo:
			return new Guid(2817650589u, 39865, 19581, 147, 173, 17, 145, 155, 18, 47, 162);
		case Event.BitmapCopyInfo:
			return new Guid(1543685679u, 44737, 20236, 180, 167, 81, 29, 40, 1, 132, 253);
		case Event.SetClipInfo:
			return new Guid(1791686128u, 54080, 17267, 168, 81, 254, 161, 38, 122, 162, 16);
		case Event.DWMDraw_ClearStart:
		case Event.DWMDraw_ClearEnd:
			return new Guid(3365275952u, 48937, 19462, 133, 116, 212, 190, 128, 63, 19, 249);
		case Event.DWMDraw_BitmapStart:
		case Event.DWMDraw_BitmapEnd:
		case Event.DWMDraw_RectangleStart:
		case Event.DWMDraw_RectangleEnd:
		case Event.DWMDraw_GeometryStart:
		case Event.DWMDraw_GeometryEnd:
		case Event.DWMDraw_ImageStart:
		case Event.DWMDraw_ImageEnd:
		case Event.DWMDraw_GlyphRunStart:
		case Event.DWMDraw_GlyphRunEnd:
		case Event.DWMDraw_BeginLayerStart:
		case Event.DWMDraw_BeginLayerEnd:
		case Event.DWMDraw_EndLayerStart:
		case Event.DWMDraw_EndLayerEnd:
		case Event.DWMDraw_ClippedBitmapStart:
		case Event.DWMDraw_ClippedBitmapEnd:
		case Event.DWMDraw_Info:
			return new Guid(3303600999u, 15265, 19573, 185, 133, 250, 203, 180, 39, 77, 215);
		case Event.LayerEventStart:
		case Event.LayerEventEnd:
			return new Guid(3940132123u, 54227, 19211, 141, 37, 228, 145, 78, 212, 193, 237);
		case Event.WClientDesktopRTCreateBegin:
		case Event.WClientDesktopRTCreateEnd:
			return new Guid(778224575, 31825, 17403, 140, 220, 145, 93, 74, 188, 9, 221);
		case Event.WClientUceProcessQueueBegin:
		case Event.WClientUceProcessQueueEnd:
		case Event.WClientUceProcessQueueInfo:
			return new Guid(3083335314u, 62132, 17530, 181, 223, 250, 108, 49, 72, 137, 174);
		case Event.WClientUcePrecomputeBegin:
		case Event.WClientUcePrecomputeEnd:
			return new Guid(3729895008u, 18093, 19648, 154, 41, 66, 106, 135, 232, 142, 159);
		case Event.WClientUceRenderBegin:
		case Event.WClientUceRenderEnd:
			return new Guid(2462732300u, 26545, 17535, 148, 151, 207, 214, 213, 42, 91, 14);
		case Event.WClientUcePresentBegin:
		case Event.WClientUcePresentEnd:
			return new Guid(1279842031u, 44052, 19844, 186, 55, 73, 169, 75, 168, 210, 175);
		case Event.WClientUceResponse:
			return new Guid(1277508388, 29232, 20385, 151, 72, 172, 76, 89, 207, 40, 140);
		case Event.WClientUceCheckDeviceStateInfo:
			return new Guid(1986010477u, 50900, 20109, 172, 110, 63, 155, 79, 23, 69, 224);
		case Event.VisualCacheAlloc:
			return new Guid(2246796534u, 56452, 17350, 177, 76, 59, 214, 7, 244, 44, 13);
		case Event.VisualCacheUpdate:
			return new Guid(2768089687u, 61782, 18678, 176, 245, 196, 169, 68, 181, 83, 251);
		case Event.CreateChannel:
			return new Guid(474045442, 5190, 18444, 168, 30, 178, 150, 126, 231, 226, 10);
		case Event.CreateOrAddResourceOnChannel:
			return new Guid(2850974682u, 61663, 20013, 163, 221, 37, 202, 143, 179, 159, 31);
		case Event.CreateWpfGfxResource:
			return new Guid(2648880491u, 31140, 18812, 136, 242, 213, 190, 220, 4, 42, 157);
		case Event.ReleaseOnChannel:
			return new Guid(2321647371u, 42900, 18302, 144, 147, 40, 46, 9, 234, 190, 89);
		case Event.UnexpectedSoftwareFallback:
			return new Guid(2100069176u, 49468, 19548, 134, 122, 197, 108, 152, 3, 84, 228);
		case Event.WClientInterlockedRenderBegin:
		case Event.WClientInterlockedRenderEnd:
			return new Guid(2146001677u, 37853, 17841, 148, 89, 33, 199, 164, 17, 49, 116);
		case Event.WClientRenderHandlerBegin:
		case Event.WClientRenderHandlerEnd:
			return new Guid(1998837943, 18571, 20352, 176, 137, 70, 164, 198, 172, 161, 196);
		case Event.WClientAnimRenderHandlerBegin:
		case Event.WClientAnimRenderHandlerEnd:
			return new Guid(1377574029u, 64170, 17243, 173, 140, 29, 100, 68, 43, 253, 112);
		case Event.WClientMediaRenderBegin:
		case Event.WClientMediaRenderEnd:
			return new Guid(1747444807, 3598, 19294, 174, 129, 183, 154, 0, 236, 131, 73);
		case Event.WClientPostRender:
			return new Guid(4218015045u, 49165, 19491, 151, 101, 105, 192, 3, 68, 178, 197);
		case Event.WClientQPCFrequency:
			return new Guid(820904087, 2124, 16523, 144, 56, 115, 190, 208, 71, 152, 115);
		case Event.WClientPrecomputeSceneBegin:
		case Event.WClientPrecomputeSceneEnd:
			return new Guid(858866191, 31291, 17078, 141, 254, 170, 191, 71, 40, 1, 218);
		case Event.WClientCompileSceneBegin:
		case Event.WClientCompileSceneEnd:
			return new Guid(2939616437u, 22757, 18640, 136, 208, 216, 244, 220, 181, 106, 18);
		case Event.WClientUIResponse:
			return new Guid(2871613531u, 18324, 17509, 145, 230, 157, 245, 134, 28, 136, 197);
		case Event.WClientUICommitChannel:
			return new Guid(4190123822u, 24765, 18121, 188, 100, 148, 254, 95, 211, 31, 228);
		case Event.WClientUceNotifyPresent:
			return new Guid(617419894u, 57669, 20058, 139, 252, 80, 195, 107, 189, 249, 204);
		case Event.WClientScheduleRender:
			return new Guid(1834674931u, 42035, 19882, 139, 49, 216, 174, 73, 207, 107, 209);
		case Event.WClientOnRenderBegin:
		case Event.WClientOnRenderEnd:
			return new Guid(977755375, 3626, 17563, 152, 110, 239, byte.MaxValue, 93, 98, 96, 231);
		case Event.WClientCreateIRT:
			return new Guid(3580787486u, 57932, 19211, 156, 74, 136, 129, 247, 0, 86, 51);
		case Event.WClientPotentialIRTResource:
			return new Guid(1079360470u, 47681, 19408, 188, 13, 107, 103, 150, 82, 41, 190);
		case Event.WClientUIContextDispatchBegin:
		case Event.WClientUIContextDispatchEnd:
			return new Guid(612475764u, 39327, 19154, 159, 34, 107, 124, 142, 42, 93, 176);
		case Event.WClientUIContextPost:
			return new Guid(1982364399u, 63092, 16481, 166, 10, 118, 249, 85, 80, 239, 235);
		case Event.WClientUIContextAbort:
			return new Guid(960515497, 16703, 17793, 160, 161, 71, 21, 22, 139, 90, 216);
		case Event.WClientUIContextPromote:
			return new Guid(1663913630u, 47496, 19250, 171, 42, 179, 122, 163, 73, 39, 238);
		case Event.WClientUIContextIdle:
			return new Guid(3324439535u, 1920, 18559, 129, 215, 56, 211, 240, 166, 240, 94);
		default:
			throw new ArgumentException(SR.InvalidEvent, "arg");
		}
	}

	internal static ushort GetTaskForEvent(Event arg)
	{
		switch (arg)
		{
		case Event.WClientCreateVisual:
			return 28;
		case Event.WClientAppCtor:
			return 48;
		case Event.WClientAppRun:
			return 49;
		case Event.WClientString:
		case Event.WClientStringBegin:
		case Event.WClientStringEnd:
			return 51;
		case Event.WClientPropParentCheck:
			return 85;
		case Event.UpdateVisualStateStart:
		case Event.UpdateVisualStateEnd:
			return 129;
		case Event.PerfElementIDName:
		case Event.PerfElementIDAssignment:
			return 143;
		case Event.WClientFontCache:
			return 52;
		case Event.WClientInputMessage:
			return 29;
		case Event.StylusEventQueued:
			return 132;
		case Event.TouchDownReported:
			return 133;
		case Event.TouchMoveReported:
			return 134;
		case Event.TouchUpReported:
			return 135;
		case Event.ManipulationReportFrame:
			return 136;
		case Event.ManipulationEventRaised:
			return 137;
		case Event.PenThreadPoolThreadAcquisition:
			return 147;
		case Event.CreateStickyNoteBegin:
		case Event.CreateStickyNoteEnd:
			return 92;
		case Event.DeleteTextNoteBegin:
		case Event.DeleteTextNoteEnd:
			return 93;
		case Event.DeleteInkNoteBegin:
		case Event.DeleteInkNoteEnd:
			return 94;
		case Event.CreateHighlightBegin:
		case Event.CreateHighlightEnd:
			return 95;
		case Event.ClearHighlightBegin:
		case Event.ClearHighlightEnd:
			return 96;
		case Event.LoadAnnotationsBegin:
		case Event.LoadAnnotationsEnd:
			return 97;
		case Event.AddAnnotationBegin:
		case Event.AddAnnotationEnd:
			return 99;
		case Event.DeleteAnnotationBegin:
		case Event.DeleteAnnotationEnd:
			return 100;
		case Event.GetAnnotationByIdBegin:
		case Event.GetAnnotationByIdEnd:
			return 101;
		case Event.GetAnnotationByLocBegin:
		case Event.GetAnnotationByLocEnd:
			return 102;
		case Event.GetAnnotationsBegin:
		case Event.GetAnnotationsEnd:
			return 103;
		case Event.SerializeAnnotationBegin:
		case Event.SerializeAnnotationEnd:
			return 104;
		case Event.DeserializeAnnotationBegin:
		case Event.DeserializeAnnotationEnd:
			return 105;
		case Event.UpdateAnnotationWithSNCBegin:
		case Event.UpdateAnnotationWithSNCEnd:
			return 106;
		case Event.UpdateSNCWithAnnotationBegin:
		case Event.UpdateSNCWithAnnotationEnd:
			return 107;
		case Event.AnnotationTextChangedBegin:
		case Event.AnnotationTextChangedEnd:
			return 108;
		case Event.AnnotationInkChangedBegin:
		case Event.AnnotationInkChangedEnd:
			return 109;
		case Event.AddAttachedSNBegin:
		case Event.AddAttachedSNEnd:
			return 110;
		case Event.RemoveAttachedSNBegin:
		case Event.RemoveAttachedSNEnd:
			return 111;
		case Event.AddAttachedHighlightBegin:
		case Event.AddAttachedHighlightEnd:
			return 112;
		case Event.RemoveAttachedHighlightBegin:
		case Event.RemoveAttachedHighlightEnd:
			return 113;
		case Event.AddAttachedMHBegin:
		case Event.AddAttachedMHEnd:
			return 114;
		case Event.RemoveAttachedMHBegin:
		case Event.RemoveAttachedMHEnd:
			return 115;
		case Event.WClientParseBamlBegin:
		case Event.WClientParseBamlEnd:
			return 41;
		case Event.WClientParseXmlBegin:
		case Event.WClientParseXmlEnd:
			return 43;
		case Event.WClientParseFefCrInstBegin:
		case Event.WClientParseFefCrInstEnd:
			return 44;
		case Event.WClientParseInstVisTreeBegin:
		case Event.WClientParseInstVisTreeEnd:
			return 45;
		case Event.WClientParseRdrCrInstBegin:
		case Event.WClientParseRdrCrInstEnd:
			return 46;
		case Event.WClientParseRdrCrInFTypBegin:
		case Event.WClientParseRdrCrInFTypEnd:
			return 47;
		case Event.WClientResourceFindBegin:
		case Event.WClientResourceFindEnd:
			return 86;
		case Event.WClientResourceCacheValue:
			return 87;
		case Event.WClientResourceCacheNull:
			return 88;
		case Event.WClientResourceCacheMiss:
			return 89;
		case Event.WClientResourceStock:
			return 90;
		case Event.WClientResourceBamlAssembly:
			return 91;
		case Event.WClientParseXamlBegin:
			return 42;
		case Event.WClientParseXamlBamlInfo:
			return 144;
		case Event.WClientParseXamlEnd:
			return 42;
		case Event.WClientDRXFlushPageStart:
		case Event.WClientDRXFlushPageStop:
			return 121;
		case Event.WClientDRXSerializeTreeStart:
		case Event.WClientDRXSerializeTreeEnd:
			return 123;
		case Event.WClientDRXGetVisualStart:
		case Event.WClientDRXGetVisualEnd:
			return 122;
		case Event.WClientDRXReleaseWriterStart:
		case Event.WClientDRXReleaseWriterEnd:
			return 124;
		case Event.WClientDRXGetPrintCapStart:
		case Event.WClientDRXGetPrintCapEnd:
			return 125;
		case Event.WClientDRXPTProviderStart:
		case Event.WClientDRXPTProviderEnd:
			return 126;
		case Event.WClientDRXRasterStart:
		case Event.WClientDRXRasterEnd:
			return 127;
		case Event.WClientDRXOpenPackageBegin:
		case Event.WClientDRXOpenPackageEnd:
			return 53;
		case Event.WClientDRXGetStreamBegin:
		case Event.WClientDRXGetStreamEnd:
			return 55;
		case Event.WClientDRXPageVisible:
			return 56;
		case Event.WClientDRXPageLoaded:
			return 57;
		case Event.WClientDRXInvalidateView:
			return 58;
		case Event.WClientDRXStyleCreated:
			return 64;
		case Event.WClientDRXFindBegin:
		case Event.WClientDRXFindEnd:
			return 65;
		case Event.WClientDRXZoom:
			return 66;
		case Event.WClientDRXEnsureOMBegin:
		case Event.WClientDRXEnsureOMEnd:
			return 67;
		case Event.WClientDRXTreeFlattenBegin:
		case Event.WClientDRXTreeFlattenEnd:
			return 69;
		case Event.WClientDRXAlphaFlattenBegin:
		case Event.WClientDRXAlphaFlattenEnd:
			return 70;
		case Event.WClientDRXGetDevModeBegin:
		case Event.WClientDRXGetDevModeEnd:
			return 71;
		case Event.WClientDRXStartDocBegin:
		case Event.WClientDRXStartDocEnd:
			return 72;
		case Event.WClientDRXEndDocBegin:
		case Event.WClientDRXEndDocEnd:
			return 73;
		case Event.WClientDRXStartPageBegin:
		case Event.WClientDRXStartPageEnd:
			return 74;
		case Event.WClientDRXEndPageBegin:
		case Event.WClientDRXEndPageEnd:
			return 75;
		case Event.WClientDRXCommitPageBegin:
		case Event.WClientDRXCommitPageEnd:
			return 76;
		case Event.WClientDRXConvertFontBegin:
		case Event.WClientDRXConvertFontEnd:
			return 77;
		case Event.WClientDRXConvertImageBegin:
		case Event.WClientDRXConvertImageEnd:
			return 78;
		case Event.WClientDRXSaveXpsBegin:
		case Event.WClientDRXSaveXpsEnd:
			return 79;
		case Event.WClientDRXLoadPrimitiveBegin:
		case Event.WClientDRXLoadPrimitiveEnd:
			return 80;
		case Event.WClientDRXSavePageBegin:
		case Event.WClientDRXSavePageEnd:
			return 81;
		case Event.WClientDRXSerializationBegin:
		case Event.WClientDRXSerializationEnd:
			return 82;
		case Event.WClientDRXReadStreamBegin:
		case Event.WClientDRXReadStreamEnd:
			return 54;
		case Event.WClientDRXGetPageBegin:
		case Event.WClientDRXGetPageEnd:
			return 68;
		case Event.WClientDRXLineDown:
			return 59;
		case Event.WClientDRXPageDown:
			return 60;
		case Event.WClientDRXPageJump:
			return 61;
		case Event.WClientDRXLayoutBegin:
		case Event.WClientDRXLayoutEnd:
			return 62;
		case Event.WClientDRXInstantiated:
			return 63;
		case Event.WClientTimeManagerTickBegin:
		case Event.WClientTimeManagerTickEnd:
			return 50;
		case Event.WClientLayoutBegin:
		case Event.WClientLayoutEnd:
			return 25;
		case Event.WClientMeasureBegin:
		case Event.WClientMeasureAbort:
		case Event.WClientMeasureEnd:
		case Event.WClientMeasureElementBegin:
		case Event.WClientMeasureElementEnd:
			return 26;
		case Event.WClientArrangeBegin:
		case Event.WClientArrangeAbort:
		case Event.WClientArrangeEnd:
		case Event.WClientArrangeElementBegin:
		case Event.WClientArrangeElementEnd:
			return 27;
		case Event.WClientLayoutAbort:
		case Event.WClientLayoutFireSizeChangedBegin:
		case Event.WClientLayoutFireSizeChangedEnd:
		case Event.WClientLayoutFireLayoutUpdatedBegin:
		case Event.WClientLayoutFireLayoutUpdatedEnd:
		case Event.WClientLayoutFireAutomationEventsBegin:
		case Event.WClientLayoutFireAutomationEventsEnd:
		case Event.WClientLayoutException:
		case Event.WClientLayoutInvalidated:
			return 25;
		case Event.WpfHostUm_WinMainStart:
		case Event.WpfHostUm_WinMainEnd:
		case Event.WpfHostUm_InvokingBrowser:
		case Event.WpfHostUm_LaunchingRestrictedProcess:
		case Event.WpfHostUm_EnteringMessageLoop:
		case Event.WpfHostUm_ClassFactoryCreateInstance:
		case Event.WpfHostUm_ReadingDeplManifestStart:
		case Event.WpfHostUm_ReadingDeplManifestEnd:
		case Event.WpfHostUm_ReadingAppManifestStart:
		case Event.WpfHostUm_ReadingAppManifestEnd:
		case Event.WpfHostUm_ParsingMarkupVersionStart:
		case Event.WpfHostUm_ParsingMarkupVersionEnd:
		case Event.WpfHostUm_IPersistFileLoad:
		case Event.WpfHostUm_IPersistMonikerLoadStart:
		case Event.WpfHostUm_IPersistMonikerLoadEnd:
		case Event.WpfHostUm_BindProgress:
		case Event.WpfHostUm_OnStopBinding:
		case Event.WpfHostUm_VersionAttach:
		case Event.WpfHostUm_VersionActivateStart:
		case Event.WpfHostUm_VersionActivateEnd:
		case Event.WpfHostUm_StartingCLRStart:
		case Event.WpfHostUm_StartingCLREnd:
		case Event.WpfHostUm_IHlinkTargetNavigateStart:
		case Event.WpfHostUm_IHlinkTargetNavigateEnd:
		case Event.WpfHostUm_ReadyStateChanged:
		case Event.WpfHostUm_InitDocHostStart:
		case Event.WpfHostUm_InitDocHostEnd:
		case Event.WpfHostUm_MergingMenusStart:
		case Event.WpfHostUm_MergingMenusEnd:
		case Event.WpfHostUm_UIActivationStart:
		case Event.WpfHostUm_UIActivationEnd:
		case Event.WpfHostUm_LoadingResourceDLLStart:
		case Event.WpfHostUm_LoadingResourceDLLEnd:
		case Event.WpfHostUm_OleCmdQueryStatusStart:
		case Event.WpfHostUm_OleCmdQueryStatusEnd:
		case Event.WpfHostUm_OleCmdExecStart:
		case Event.WpfHostUm_OleCmdExecEnd:
		case Event.WpfHostUm_ProgressPageShown:
		case Event.WpfHostUm_AdHocProfile1Start:
		case Event.WpfHostUm_AdHocProfile1End:
		case Event.WpfHostUm_AdHocProfile2Start:
		case Event.WpfHostUm_AdHocProfile2End:
			return 116;
		case Event.WpfHost_DocObjHostCreated:
		case Event.WpfHost_XappLauncherAppStartup:
		case Event.WpfHost_XappLauncherAppExit:
		case Event.WpfHost_DocObjHostRunApplicationStart:
		case Event.WpfHost_DocObjHostRunApplicationEnd:
		case Event.WpfHost_ClickOnceActivationStart:
		case Event.WpfHost_ClickOnceActivationEnd:
		case Event.WpfHost_InitAppProxyStart:
		case Event.WpfHost_InitAppProxyEnd:
		case Event.WpfHost_AppProxyCtor:
		case Event.WpfHost_RootBrowserWindowSetupStart:
		case Event.WpfHost_RootBrowserWindowSetupEnd:
		case Event.WpfHost_AppProxyRunStart:
		case Event.WpfHost_AppProxyRunEnd:
		case Event.WpfHost_AppDomainManagerCctor:
		case Event.WpfHost_ApplicationActivatorCreateInstanceStart:
		case Event.WpfHost_ApplicationActivatorCreateInstanceEnd:
		case Event.WpfHost_DetermineApplicationTrustStart:
		case Event.WpfHost_DetermineApplicationTrustEnd:
		case Event.WpfHost_FirstTimeActivation:
		case Event.WpfHost_GetDownloadPageStart:
		case Event.WpfHost_GetDownloadPageEnd:
		case Event.WpfHost_DownloadDeplManifestStart:
		case Event.WpfHost_DownloadDeplManifestEnd:
		case Event.WpfHost_AssertAppRequirementsStart:
		case Event.WpfHost_AssertAppRequirementsEnd:
		case Event.WpfHost_DownloadApplicationStart:
		case Event.WpfHost_DownloadApplicationEnd:
		case Event.WpfHost_DownloadProgressUpdate:
		case Event.WpfHost_XappLauncherAppNavigated:
		case Event.WpfHost_UpdateBrowserCommandsStart:
		case Event.WpfHost_UpdateBrowserCommandsEnd:
		case Event.WpfHost_PostShutdown:
		case Event.WpfHost_AbortingActivation:
		case Event.WpfHost_IBHSRunStart:
		case Event.WpfHost_IBHSRunEnd:
			return 117;
		case Event.Wpf_NavigationAsyncWorkItem:
		case Event.Wpf_NavigationWebResponseReceived:
		case Event.Wpf_NavigationEnd:
		case Event.Wpf_NavigationContentRendered:
		case Event.Wpf_NavigationStart:
		case Event.Wpf_NavigationLaunchBrowser:
		case Event.Wpf_NavigationPageFunctionReturn:
			return 118;
		case Event.DrawBitmapInfo:
			return 1;
		case Event.BitmapCopyInfo:
			return 2;
		case Event.SetClipInfo:
			return 3;
		case Event.DWMDraw_ClearStart:
		case Event.DWMDraw_ClearEnd:
			return 5;
		case Event.DWMDraw_BitmapStart:
		case Event.DWMDraw_BitmapEnd:
		case Event.DWMDraw_RectangleStart:
		case Event.DWMDraw_RectangleEnd:
		case Event.DWMDraw_GeometryStart:
		case Event.DWMDraw_GeometryEnd:
		case Event.DWMDraw_ImageStart:
		case Event.DWMDraw_ImageEnd:
		case Event.DWMDraw_GlyphRunStart:
		case Event.DWMDraw_GlyphRunEnd:
		case Event.DWMDraw_BeginLayerStart:
		case Event.DWMDraw_BeginLayerEnd:
		case Event.DWMDraw_EndLayerStart:
		case Event.DWMDraw_EndLayerEnd:
		case Event.DWMDraw_ClippedBitmapStart:
		case Event.DWMDraw_ClippedBitmapEnd:
		case Event.DWMDraw_Info:
			return 8;
		case Event.LayerEventStart:
		case Event.LayerEventEnd:
			return 9;
		case Event.WClientDesktopRTCreateBegin:
		case Event.WClientDesktopRTCreateEnd:
			return 12;
		case Event.WClientUceProcessQueueBegin:
		case Event.WClientUceProcessQueueEnd:
		case Event.WClientUceProcessQueueInfo:
			return 13;
		case Event.WClientUcePrecomputeBegin:
		case Event.WClientUcePrecomputeEnd:
			return 14;
		case Event.WClientUceRenderBegin:
		case Event.WClientUceRenderEnd:
			return 15;
		case Event.WClientUcePresentBegin:
		case Event.WClientUcePresentEnd:
			return 16;
		case Event.WClientUceResponse:
			return 17;
		case Event.WClientUceCheckDeviceStateInfo:
			return 19;
		case Event.VisualCacheAlloc:
			return 130;
		case Event.VisualCacheUpdate:
			return 131;
		case Event.CreateChannel:
			return 141;
		case Event.CreateOrAddResourceOnChannel:
			return 139;
		case Event.CreateWpfGfxResource:
			return 140;
		case Event.ReleaseOnChannel:
			return 142;
		case Event.UnexpectedSoftwareFallback:
			return 128;
		case Event.WClientInterlockedRenderBegin:
		case Event.WClientInterlockedRenderEnd:
			return 138;
		case Event.WClientRenderHandlerBegin:
		case Event.WClientRenderHandlerEnd:
			return 30;
		case Event.WClientAnimRenderHandlerBegin:
		case Event.WClientAnimRenderHandlerEnd:
			return 31;
		case Event.WClientMediaRenderBegin:
		case Event.WClientMediaRenderEnd:
			return 32;
		case Event.WClientPostRender:
			return 33;
		case Event.WClientQPCFrequency:
			return 34;
		case Event.WClientPrecomputeSceneBegin:
		case Event.WClientPrecomputeSceneEnd:
			return 35;
		case Event.WClientCompileSceneBegin:
		case Event.WClientCompileSceneEnd:
			return 36;
		case Event.WClientUIResponse:
			return 37;
		case Event.WClientUICommitChannel:
			return 38;
		case Event.WClientUceNotifyPresent:
			return 39;
		case Event.WClientScheduleRender:
			return 40;
		case Event.WClientOnRenderBegin:
		case Event.WClientOnRenderEnd:
			return 120;
		case Event.WClientCreateIRT:
			return 145;
		case Event.WClientPotentialIRTResource:
			return 146;
		case Event.WClientUIContextDispatchBegin:
		case Event.WClientUIContextDispatchEnd:
			return 20;
		case Event.WClientUIContextPost:
			return 21;
		case Event.WClientUIContextAbort:
			return 22;
		case Event.WClientUIContextPromote:
			return 23;
		case Event.WClientUIContextIdle:
			return 24;
		default:
			throw new ArgumentException(SR.InvalidEvent, "arg");
		}
	}

	internal static byte GetOpcodeForEvent(Event arg)
	{
		switch (arg)
		{
		case Event.WClientCreateVisual:
		case Event.WClientAppCtor:
		case Event.WClientAppRun:
		case Event.WClientString:
		case Event.WClientPropParentCheck:
		case Event.PerfElementIDAssignment:
		case Event.WClientFontCache:
		case Event.WClientInputMessage:
		case Event.StylusEventQueued:
		case Event.TouchDownReported:
		case Event.TouchMoveReported:
		case Event.TouchUpReported:
		case Event.ManipulationReportFrame:
		case Event.ManipulationEventRaised:
		case Event.PenThreadPoolThreadAcquisition:
		case Event.WClientResourceCacheValue:
		case Event.WClientResourceCacheNull:
		case Event.WClientResourceCacheMiss:
		case Event.WClientResourceStock:
		case Event.WClientResourceBamlAssembly:
		case Event.WClientParseXamlBamlInfo:
		case Event.WClientDRXPageVisible:
		case Event.WClientDRXPageLoaded:
		case Event.WClientDRXInvalidateView:
		case Event.WClientDRXStyleCreated:
		case Event.WClientDRXZoom:
		case Event.WClientDRXLineDown:
		case Event.WClientDRXPageDown:
		case Event.WClientDRXPageJump:
		case Event.WClientDRXInstantiated:
		case Event.DrawBitmapInfo:
		case Event.BitmapCopyInfo:
		case Event.SetClipInfo:
		case Event.DWMDraw_Info:
		case Event.WClientUceProcessQueueInfo:
		case Event.WClientUceResponse:
		case Event.WClientUceCheckDeviceStateInfo:
		case Event.VisualCacheAlloc:
		case Event.VisualCacheUpdate:
		case Event.CreateChannel:
		case Event.CreateOrAddResourceOnChannel:
		case Event.CreateWpfGfxResource:
		case Event.ReleaseOnChannel:
		case Event.UnexpectedSoftwareFallback:
		case Event.WClientPostRender:
		case Event.WClientQPCFrequency:
		case Event.WClientUIResponse:
		case Event.WClientUICommitChannel:
		case Event.WClientUceNotifyPresent:
		case Event.WClientScheduleRender:
		case Event.WClientCreateIRT:
		case Event.WClientPotentialIRTResource:
		case Event.WClientUIContextPost:
		case Event.WClientUIContextAbort:
		case Event.WClientUIContextPromote:
		case Event.WClientUIContextIdle:
			return 0;
		case Event.WClientStringBegin:
		case Event.UpdateVisualStateStart:
		case Event.CreateStickyNoteBegin:
		case Event.DeleteTextNoteBegin:
		case Event.DeleteInkNoteBegin:
		case Event.CreateHighlightBegin:
		case Event.ClearHighlightBegin:
		case Event.LoadAnnotationsBegin:
		case Event.AddAnnotationBegin:
		case Event.DeleteAnnotationBegin:
		case Event.GetAnnotationByIdBegin:
		case Event.GetAnnotationByLocBegin:
		case Event.GetAnnotationsBegin:
		case Event.SerializeAnnotationBegin:
		case Event.DeserializeAnnotationBegin:
		case Event.UpdateAnnotationWithSNCBegin:
		case Event.UpdateSNCWithAnnotationBegin:
		case Event.AnnotationTextChangedBegin:
		case Event.AnnotationInkChangedBegin:
		case Event.AddAttachedSNBegin:
		case Event.RemoveAttachedSNBegin:
		case Event.AddAttachedHighlightBegin:
		case Event.RemoveAttachedHighlightBegin:
		case Event.AddAttachedMHBegin:
		case Event.RemoveAttachedMHBegin:
		case Event.WClientParseBamlBegin:
		case Event.WClientParseXmlBegin:
		case Event.WClientParseFefCrInstBegin:
		case Event.WClientParseInstVisTreeBegin:
		case Event.WClientParseRdrCrInstBegin:
		case Event.WClientParseRdrCrInFTypBegin:
		case Event.WClientResourceFindBegin:
		case Event.WClientParseXamlBegin:
		case Event.WClientDRXFlushPageStart:
		case Event.WClientDRXSerializeTreeStart:
		case Event.WClientDRXGetVisualStart:
		case Event.WClientDRXReleaseWriterStart:
		case Event.WClientDRXGetPrintCapStart:
		case Event.WClientDRXPTProviderStart:
		case Event.WClientDRXRasterStart:
		case Event.WClientDRXOpenPackageBegin:
		case Event.WClientDRXGetStreamBegin:
		case Event.WClientDRXFindBegin:
		case Event.WClientDRXEnsureOMBegin:
		case Event.WClientDRXTreeFlattenBegin:
		case Event.WClientDRXAlphaFlattenBegin:
		case Event.WClientDRXGetDevModeBegin:
		case Event.WClientDRXStartDocBegin:
		case Event.WClientDRXEndDocBegin:
		case Event.WClientDRXStartPageBegin:
		case Event.WClientDRXEndPageBegin:
		case Event.WClientDRXCommitPageBegin:
		case Event.WClientDRXConvertFontBegin:
		case Event.WClientDRXConvertImageBegin:
		case Event.WClientDRXSaveXpsBegin:
		case Event.WClientDRXLoadPrimitiveBegin:
		case Event.WClientDRXSavePageBegin:
		case Event.WClientDRXSerializationBegin:
		case Event.WClientDRXReadStreamBegin:
		case Event.WClientDRXGetPageBegin:
		case Event.WClientDRXLayoutBegin:
		case Event.WClientTimeManagerTickBegin:
		case Event.WClientLayoutBegin:
		case Event.WClientMeasureBegin:
		case Event.WClientArrangeBegin:
		case Event.DWMDraw_ClearStart:
		case Event.LayerEventStart:
		case Event.WClientDesktopRTCreateBegin:
		case Event.WClientUceProcessQueueBegin:
		case Event.WClientUcePrecomputeBegin:
		case Event.WClientUceRenderBegin:
		case Event.WClientUcePresentBegin:
		case Event.WClientInterlockedRenderBegin:
		case Event.WClientRenderHandlerBegin:
		case Event.WClientAnimRenderHandlerBegin:
		case Event.WClientMediaRenderBegin:
		case Event.WClientPrecomputeSceneBegin:
		case Event.WClientCompileSceneBegin:
		case Event.WClientOnRenderBegin:
		case Event.WClientUIContextDispatchBegin:
			return 1;
		case Event.WClientStringEnd:
		case Event.UpdateVisualStateEnd:
		case Event.CreateStickyNoteEnd:
		case Event.DeleteTextNoteEnd:
		case Event.DeleteInkNoteEnd:
		case Event.CreateHighlightEnd:
		case Event.ClearHighlightEnd:
		case Event.LoadAnnotationsEnd:
		case Event.AddAnnotationEnd:
		case Event.DeleteAnnotationEnd:
		case Event.GetAnnotationByIdEnd:
		case Event.GetAnnotationByLocEnd:
		case Event.GetAnnotationsEnd:
		case Event.SerializeAnnotationEnd:
		case Event.DeserializeAnnotationEnd:
		case Event.UpdateAnnotationWithSNCEnd:
		case Event.UpdateSNCWithAnnotationEnd:
		case Event.AnnotationTextChangedEnd:
		case Event.AnnotationInkChangedEnd:
		case Event.AddAttachedSNEnd:
		case Event.RemoveAttachedSNEnd:
		case Event.AddAttachedHighlightEnd:
		case Event.RemoveAttachedHighlightEnd:
		case Event.AddAttachedMHEnd:
		case Event.RemoveAttachedMHEnd:
		case Event.WClientParseBamlEnd:
		case Event.WClientParseXmlEnd:
		case Event.WClientParseFefCrInstEnd:
		case Event.WClientParseInstVisTreeEnd:
		case Event.WClientParseRdrCrInstEnd:
		case Event.WClientParseRdrCrInFTypEnd:
		case Event.WClientResourceFindEnd:
		case Event.WClientParseXamlEnd:
		case Event.WClientDRXFlushPageStop:
		case Event.WClientDRXSerializeTreeEnd:
		case Event.WClientDRXGetVisualEnd:
		case Event.WClientDRXReleaseWriterEnd:
		case Event.WClientDRXGetPrintCapEnd:
		case Event.WClientDRXPTProviderEnd:
		case Event.WClientDRXRasterEnd:
		case Event.WClientDRXOpenPackageEnd:
		case Event.WClientDRXGetStreamEnd:
		case Event.WClientDRXFindEnd:
		case Event.WClientDRXEnsureOMEnd:
		case Event.WClientDRXTreeFlattenEnd:
		case Event.WClientDRXAlphaFlattenEnd:
		case Event.WClientDRXGetDevModeEnd:
		case Event.WClientDRXStartDocEnd:
		case Event.WClientDRXEndDocEnd:
		case Event.WClientDRXStartPageEnd:
		case Event.WClientDRXEndPageEnd:
		case Event.WClientDRXCommitPageEnd:
		case Event.WClientDRXConvertFontEnd:
		case Event.WClientDRXConvertImageEnd:
		case Event.WClientDRXSaveXpsEnd:
		case Event.WClientDRXLoadPrimitiveEnd:
		case Event.WClientDRXSavePageEnd:
		case Event.WClientDRXSerializationEnd:
		case Event.WClientDRXReadStreamEnd:
		case Event.WClientDRXGetPageEnd:
		case Event.WClientDRXLayoutEnd:
		case Event.WClientTimeManagerTickEnd:
		case Event.WClientLayoutEnd:
		case Event.WClientMeasureEnd:
		case Event.WClientArrangeEnd:
		case Event.DWMDraw_ClearEnd:
		case Event.LayerEventEnd:
		case Event.WClientDesktopRTCreateEnd:
		case Event.WClientUceProcessQueueEnd:
		case Event.WClientUcePrecomputeEnd:
		case Event.WClientUceRenderEnd:
		case Event.WClientUcePresentEnd:
		case Event.WClientInterlockedRenderEnd:
		case Event.WClientRenderHandlerEnd:
		case Event.WClientAnimRenderHandlerEnd:
		case Event.WClientMediaRenderEnd:
		case Event.WClientPrecomputeSceneEnd:
		case Event.WClientCompileSceneEnd:
		case Event.WClientOnRenderEnd:
		case Event.WClientUIContextDispatchEnd:
			return 2;
		case Event.PerfElementIDName:
		case Event.WClientMeasureAbort:
		case Event.WClientArrangeAbort:
		case Event.WClientLayoutAbort:
		case Event.WpfHost_DocObjHostCreated:
		case Event.Wpf_NavigationStart:
			return 10;
		case Event.WClientMeasureElementBegin:
		case Event.WClientArrangeElementBegin:
		case Event.WClientLayoutFireSizeChangedBegin:
		case Event.WpfHost_IBHSRunStart:
		case Event.Wpf_NavigationAsyncWorkItem:
			return 11;
		case Event.WClientMeasureElementEnd:
		case Event.WClientArrangeElementEnd:
		case Event.WClientLayoutFireSizeChangedEnd:
		case Event.WpfHost_IBHSRunEnd:
		case Event.Wpf_NavigationWebResponseReceived:
			return 12;
		case Event.WClientLayoutFireLayoutUpdatedBegin:
		case Event.WpfHost_XappLauncherAppStartup:
		case Event.Wpf_NavigationLaunchBrowser:
			return 13;
		case Event.WClientLayoutFireLayoutUpdatedEnd:
		case Event.WpfHost_XappLauncherAppExit:
		case Event.Wpf_NavigationEnd:
			return 14;
		case Event.WClientLayoutFireAutomationEventsBegin:
		case Event.WpfHost_DocObjHostRunApplicationStart:
		case Event.Wpf_NavigationContentRendered:
			return 15;
		case Event.WClientLayoutFireAutomationEventsEnd:
		case Event.WpfHost_DocObjHostRunApplicationEnd:
		case Event.Wpf_NavigationPageFunctionReturn:
			return 16;
		case Event.WClientLayoutException:
		case Event.WpfHost_ClickOnceActivationStart:
			return 17;
		case Event.WClientLayoutInvalidated:
		case Event.WpfHost_ClickOnceActivationEnd:
			return 18;
		case Event.WpfHost_InitAppProxyStart:
			return 19;
		case Event.WpfHost_InitAppProxyEnd:
			return 20;
		case Event.WpfHostUm_WinMainStart:
		case Event.WpfHost_AppProxyCtor:
			return 30;
		case Event.WpfHostUm_WinMainEnd:
		case Event.WpfHost_RootBrowserWindowSetupStart:
			return 31;
		case Event.WpfHostUm_InvokingBrowser:
		case Event.WpfHost_RootBrowserWindowSetupEnd:
			return 32;
		case Event.WpfHostUm_LaunchingRestrictedProcess:
		case Event.WpfHost_AppProxyRunStart:
			return 33;
		case Event.WpfHostUm_EnteringMessageLoop:
		case Event.WpfHost_AppProxyRunEnd:
			return 34;
		case Event.WpfHostUm_ClassFactoryCreateInstance:
			return 35;
		case Event.WpfHostUm_ReadingDeplManifestStart:
		case Event.WpfHost_AppDomainManagerCctor:
			return 40;
		case Event.WpfHostUm_ReadingDeplManifestEnd:
		case Event.WpfHost_ApplicationActivatorCreateInstanceStart:
			return 41;
		case Event.WpfHostUm_ReadingAppManifestStart:
		case Event.WpfHost_ApplicationActivatorCreateInstanceEnd:
			return 42;
		case Event.WpfHostUm_ReadingAppManifestEnd:
		case Event.WpfHost_DetermineApplicationTrustStart:
			return 43;
		case Event.WpfHostUm_ParsingMarkupVersionStart:
		case Event.WpfHost_DetermineApplicationTrustEnd:
			return 44;
		case Event.WpfHostUm_ParsingMarkupVersionEnd:
			return 45;
		case Event.WpfHostUm_IPersistFileLoad:
		case Event.WpfHost_FirstTimeActivation:
			return 50;
		case Event.WpfHostUm_IPersistMonikerLoadStart:
		case Event.WpfHost_GetDownloadPageStart:
			return 51;
		case Event.WpfHostUm_IPersistMonikerLoadEnd:
		case Event.WpfHost_GetDownloadPageEnd:
			return 52;
		case Event.WpfHostUm_BindProgress:
		case Event.WpfHost_DownloadDeplManifestStart:
			return 53;
		case Event.WpfHostUm_OnStopBinding:
		case Event.WpfHost_DownloadDeplManifestEnd:
			return 54;
		case Event.WpfHost_AssertAppRequirementsStart:
			return 55;
		case Event.WpfHost_AssertAppRequirementsEnd:
		case Event.DWMDraw_BitmapStart:
			return 56;
		case Event.WpfHost_DownloadApplicationStart:
		case Event.DWMDraw_BitmapEnd:
			return 57;
		case Event.WpfHost_DownloadApplicationEnd:
		case Event.DWMDraw_RectangleStart:
			return 58;
		case Event.WpfHost_DownloadProgressUpdate:
		case Event.DWMDraw_RectangleEnd:
			return 59;
		case Event.WpfHostUm_VersionAttach:
		case Event.WpfHost_XappLauncherAppNavigated:
		case Event.DWMDraw_GeometryStart:
			return 60;
		case Event.WpfHostUm_VersionActivateStart:
		case Event.DWMDraw_GeometryEnd:
			return 61;
		case Event.WpfHostUm_VersionActivateEnd:
		case Event.DWMDraw_ImageStart:
			return 62;
		case Event.DWMDraw_ImageEnd:
			return 63;
		case Event.DWMDraw_GlyphRunStart:
			return 64;
		case Event.DWMDraw_GlyphRunEnd:
			return 65;
		case Event.DWMDraw_BeginLayerStart:
			return 68;
		case Event.DWMDraw_BeginLayerEnd:
			return 69;
		case Event.WpfHost_UpdateBrowserCommandsStart:
		case Event.DWMDraw_EndLayerStart:
			return 70;
		case Event.WpfHost_UpdateBrowserCommandsEnd:
		case Event.DWMDraw_EndLayerEnd:
			return 71;
		case Event.DWMDraw_ClippedBitmapStart:
			return 78;
		case Event.DWMDraw_ClippedBitmapEnd:
			return 79;
		case Event.WpfHost_PostShutdown:
			return 80;
		case Event.WpfHost_AbortingActivation:
			return 81;
		case Event.WpfHostUm_StartingCLRStart:
			return 90;
		case Event.WpfHostUm_StartingCLREnd:
			return 91;
		case Event.WpfHostUm_IHlinkTargetNavigateStart:
			return 95;
		case Event.WpfHostUm_IHlinkTargetNavigateEnd:
			return 96;
		case Event.WpfHostUm_ReadyStateChanged:
			return 97;
		case Event.WpfHostUm_InitDocHostStart:
			return 98;
		case Event.WpfHostUm_InitDocHostEnd:
			return 99;
		case Event.WpfHostUm_MergingMenusStart:
			return 100;
		case Event.WpfHostUm_MergingMenusEnd:
			return 101;
		case Event.WpfHostUm_UIActivationStart:
			return 102;
		case Event.WpfHostUm_UIActivationEnd:
			return 103;
		case Event.WpfHostUm_LoadingResourceDLLStart:
			return 104;
		case Event.WpfHostUm_LoadingResourceDLLEnd:
			return 105;
		case Event.WpfHostUm_OleCmdQueryStatusStart:
			return 106;
		case Event.WpfHostUm_OleCmdQueryStatusEnd:
			return 107;
		case Event.WpfHostUm_OleCmdExecStart:
			return 108;
		case Event.WpfHostUm_OleCmdExecEnd:
			return 109;
		case Event.WpfHostUm_ProgressPageShown:
			return 110;
		case Event.WpfHostUm_AdHocProfile1Start:
			return 152;
		case Event.WpfHostUm_AdHocProfile1End:
			return 153;
		case Event.WpfHostUm_AdHocProfile2Start:
			return 154;
		case Event.WpfHostUm_AdHocProfile2End:
			return 155;
		default:
			throw new ArgumentException(SR.InvalidEvent, "arg");
		}
	}

	internal static byte GetVersionForEvent(Event arg)
	{
		if (arg <= Event.WClientParseXamlEnd)
		{
			if (arg <= Event.WClientInputMessage)
			{
				if (arg <= Event.PerfElementIDAssignment)
				{
					if (arg - 1 <= Event.WClientStringEnd)
					{
						goto IL_02a5;
					}
					if (arg - 8 <= Event.WClientAppRun)
					{
						goto IL_02a3;
					}
				}
				else if (arg == Event.WClientFontCache || arg == Event.WClientInputMessage)
				{
					goto IL_02a5;
				}
			}
			else if (arg <= Event.RemoveAttachedMHEnd)
			{
				if (arg - 2002 <= Event.WClientStringEnd)
				{
					goto IL_02a3;
				}
				if (arg - 3001 <= (Event)45)
				{
					goto IL_02a5;
				}
			}
			else
			{
				if (arg - 4001 <= (Event)18)
				{
					goto IL_02a5;
				}
				if (arg - 4020 <= Event.WClientAppCtor)
				{
					goto IL_02a3;
				}
			}
			goto IL_02a9;
		}
		if (arg <= Event.WClientLayoutInvalidated)
		{
			switch (arg)
			{
			case Event.WClientDRXFlushPageStart:
			case Event.WClientDRXFlushPageStop:
			case Event.WClientDRXSerializeTreeStart:
			case Event.WClientDRXSerializeTreeEnd:
			case Event.WClientDRXGetVisualStart:
			case Event.WClientDRXGetVisualEnd:
			case Event.WClientDRXReleaseWriterStart:
			case Event.WClientDRXReleaseWriterEnd:
			case Event.WClientDRXGetPrintCapStart:
			case Event.WClientDRXGetPrintCapEnd:
			case Event.WClientDRXPTProviderStart:
			case Event.WClientDRXPTProviderEnd:
			case Event.WClientDRXRasterStart:
			case Event.WClientDRXRasterEnd:
			case Event.WClientMeasureAbort:
			case Event.WClientMeasureElementBegin:
			case Event.WClientMeasureElementEnd:
			case Event.WClientArrangeElementBegin:
			case Event.WClientArrangeElementEnd:
			case Event.WClientLayoutAbort:
			case Event.WClientLayoutFireSizeChangedBegin:
			case Event.WClientLayoutFireSizeChangedEnd:
			case Event.WClientLayoutFireLayoutUpdatedBegin:
			case Event.WClientLayoutFireLayoutUpdatedEnd:
			case Event.WClientLayoutFireAutomationEventsBegin:
			case Event.WClientLayoutFireAutomationEventsEnd:
			case Event.WClientLayoutException:
			case Event.WClientLayoutInvalidated:
				break;
			case Event.WClientDRXOpenPackageBegin:
			case Event.WClientDRXOpenPackageEnd:
			case Event.WClientDRXGetStreamBegin:
			case Event.WClientDRXGetStreamEnd:
			case Event.WClientDRXPageVisible:
			case Event.WClientDRXPageLoaded:
			case Event.WClientDRXInvalidateView:
			case Event.WClientDRXStyleCreated:
			case Event.WClientDRXFindBegin:
			case Event.WClientDRXFindEnd:
			case Event.WClientDRXZoom:
			case Event.WClientDRXEnsureOMBegin:
			case Event.WClientDRXEnsureOMEnd:
			case Event.WClientDRXTreeFlattenBegin:
			case Event.WClientDRXTreeFlattenEnd:
			case Event.WClientDRXAlphaFlattenBegin:
			case Event.WClientDRXAlphaFlattenEnd:
			case Event.WClientDRXGetDevModeBegin:
			case Event.WClientDRXGetDevModeEnd:
			case Event.WClientDRXStartDocBegin:
			case Event.WClientDRXStartDocEnd:
			case Event.WClientDRXEndDocBegin:
			case Event.WClientDRXEndDocEnd:
			case Event.WClientDRXStartPageBegin:
			case Event.WClientDRXStartPageEnd:
			case Event.WClientDRXEndPageBegin:
			case Event.WClientDRXEndPageEnd:
			case Event.WClientDRXCommitPageBegin:
			case Event.WClientDRXCommitPageEnd:
			case Event.WClientDRXConvertFontBegin:
			case Event.WClientDRXConvertFontEnd:
			case Event.WClientDRXConvertImageBegin:
			case Event.WClientDRXConvertImageEnd:
			case Event.WClientDRXSaveXpsBegin:
			case Event.WClientDRXSaveXpsEnd:
			case Event.WClientDRXLoadPrimitiveBegin:
			case Event.WClientDRXLoadPrimitiveEnd:
			case Event.WClientDRXSavePageBegin:
			case Event.WClientDRXSavePageEnd:
			case Event.WClientDRXSerializationBegin:
			case Event.WClientDRXSerializationEnd:
			case Event.WClientDRXReadStreamBegin:
			case Event.WClientDRXReadStreamEnd:
			case Event.WClientDRXGetPageBegin:
			case Event.WClientDRXGetPageEnd:
			case Event.WClientDRXLineDown:
			case Event.WClientDRXPageDown:
			case Event.WClientDRXPageJump:
			case Event.WClientDRXLayoutBegin:
			case Event.WClientDRXLayoutEnd:
			case Event.WClientDRXInstantiated:
			case Event.WClientTimeManagerTickBegin:
			case Event.WClientTimeManagerTickEnd:
			case Event.WClientLayoutEnd:
			case Event.WClientMeasureBegin:
			case Event.WClientMeasureEnd:
			case Event.WClientArrangeBegin:
			case Event.WClientArrangeAbort:
			case Event.WClientArrangeEnd:
				goto IL_02a5;
			case Event.WClientLayoutBegin:
				goto IL_02a7;
			default:
				goto IL_02a9;
			}
		}
		else
		{
			if (arg <= Event.Wpf_NavigationPageFunctionReturn)
			{
				if (arg - 9003 <= (Event)71 || arg - 9077 <= (Event)12)
				{
					goto IL_02a5;
				}
				goto IL_02a9;
			}
			switch (arg)
			{
			case Event.DrawBitmapInfo:
			case Event.BitmapCopyInfo:
			case Event.SetClipInfo:
			case Event.DWMDraw_ClearStart:
			case Event.DWMDraw_ClearEnd:
			case Event.DWMDraw_BitmapStart:
			case Event.DWMDraw_BitmapEnd:
			case Event.DWMDraw_RectangleStart:
			case Event.DWMDraw_RectangleEnd:
			case Event.DWMDraw_GeometryStart:
			case Event.DWMDraw_GeometryEnd:
			case Event.DWMDraw_ImageStart:
			case Event.DWMDraw_ImageEnd:
			case Event.DWMDraw_GlyphRunStart:
			case Event.DWMDraw_GlyphRunEnd:
			case Event.DWMDraw_BeginLayerStart:
			case Event.DWMDraw_BeginLayerEnd:
			case Event.DWMDraw_EndLayerStart:
			case Event.DWMDraw_EndLayerEnd:
			case Event.DWMDraw_ClippedBitmapStart:
			case Event.DWMDraw_ClippedBitmapEnd:
			case Event.DWMDraw_Info:
			case Event.VisualCacheAlloc:
			case Event.VisualCacheUpdate:
			case Event.CreateChannel:
			case Event.CreateOrAddResourceOnChannel:
			case Event.CreateWpfGfxResource:
			case Event.ReleaseOnChannel:
			case Event.UnexpectedSoftwareFallback:
			case Event.WClientOnRenderBegin:
			case Event.WClientOnRenderEnd:
			case Event.WClientCreateIRT:
			case Event.WClientPotentialIRTResource:
				break;
			case Event.LayerEventStart:
			case Event.LayerEventEnd:
			case Event.WClientDesktopRTCreateBegin:
			case Event.WClientDesktopRTCreateEnd:
			case Event.WClientUceProcessQueueBegin:
			case Event.WClientUceProcessQueueEnd:
			case Event.WClientUceProcessQueueInfo:
			case Event.WClientUcePrecomputeBegin:
			case Event.WClientUcePrecomputeEnd:
			case Event.WClientUceRenderBegin:
			case Event.WClientUceRenderEnd:
			case Event.WClientUcePresentBegin:
			case Event.WClientUcePresentEnd:
			case Event.WClientUceResponse:
			case Event.WClientUceCheckDeviceStateInfo:
			case Event.WClientInterlockedRenderBegin:
			case Event.WClientInterlockedRenderEnd:
			case Event.WClientRenderHandlerBegin:
			case Event.WClientRenderHandlerEnd:
			case Event.WClientAnimRenderHandlerBegin:
			case Event.WClientAnimRenderHandlerEnd:
			case Event.WClientMediaRenderBegin:
			case Event.WClientMediaRenderEnd:
			case Event.WClientPostRender:
			case Event.WClientQPCFrequency:
			case Event.WClientPrecomputeSceneBegin:
			case Event.WClientPrecomputeSceneEnd:
			case Event.WClientCompileSceneBegin:
			case Event.WClientCompileSceneEnd:
			case Event.WClientUIResponse:
			case Event.WClientUICommitChannel:
			case Event.WClientUceNotifyPresent:
			case Event.WClientScheduleRender:
			case Event.WClientUIContextDispatchEnd:
			case Event.WClientUIContextIdle:
				goto IL_02a5;
			case Event.WClientUIContextDispatchBegin:
			case Event.WClientUIContextPost:
			case Event.WClientUIContextAbort:
			case Event.WClientUIContextPromote:
				goto IL_02a7;
			default:
				goto IL_02a9;
			}
		}
		goto IL_02a3;
		IL_02a9:
		throw new ArgumentException(SR.InvalidEvent, "arg");
		IL_02a5:
		return 2;
		IL_02a3:
		return 0;
		IL_02a7:
		return 3;
	}
}
