using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using MS.Internal.PresentationCore;
using MS.Win32;
using MS.Win32.Recognizer;

namespace MS.Internal.Ink.GestureRecognition;

internal sealed class NativeRecognizer : IDisposable
{
	private enum RECO_TYPE : ushort
	{
		RECO_TYPE_WSTRING,
		RECO_TYPE_WCHAR
	}

	private const string GestureRecognizerPath = "SOFTWARE\\MICROSOFT\\TPG\\SYSTEM RECOGNIZERS\\{BED9A940-7D48-48E3-9A68-F4887A5A1B2E}";

	private const string GestureRecognizerFullPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\MICROSOFT\\TPG\\SYSTEM RECOGNIZERS\\{BED9A940-7D48-48E3-9A68-F4887A5A1B2E}";

	private const string GestureRecognizerValueName = "RECOGNIZER DLL";

	private const string GestureRecognizerGuid = "{BED9A940-7D48-48E3-9A68-F4887A5A1B2E}";

	private const ushort MAX_GESTURE_COUNT = 256;

	private const ushort GESTURE_NULL = 61440;

	private const ushort IRAS_DefaultCount = 10;

	private const ushort MaxStylusPoints = 10000;

	private static readonly Guid GUID_CONFIDENCELEVEL;

	private bool _disposed;

	private ContextSafeHandle _hContext;

	private static readonly object _syncRoot;

	private static RecognizerSafeHandle s_hRec;

	private static Guid s_Gesture;

	private static readonly bool s_isSupported;

	private static bool s_GetAlternateListExists;

	private static RecognizerSafeHandle RecognizerHandleSingleton
	{
		get
		{
			if (s_isSupported && s_hRec == null)
			{
				lock (_syncRoot)
				{
					if (s_isSupported && s_hRec == null && HRESULT.Failed(UnsafeNativeMethods.CreateRecognizer(ref s_Gesture, out s_hRec)))
					{
						s_hRec = null;
					}
				}
			}
			return s_hRec;
		}
	}

	static NativeRecognizer()
	{
		GUID_CONFIDENCELEVEL = new Guid("{7DFE11A7-FB5D-4958-8765-154ADF0D833F}");
		_syncRoot = new object();
		s_Gesture = new Guid("{BED9A940-7D48-48E3-9A68-F4887A5A1B2E}");
		s_isSupported = LoadRecognizerDll();
	}

	private NativeRecognizer()
	{
		if (HRESULT.Failed(UnsafeNativeMethods.CreateContext(RecognizerHandleSingleton, out _hContext)))
		{
			throw new InvalidOperationException(SR.UnspecifiedGestureConstructionException);
		}
		_hContext.AddReferenceOnRecognizer(RecognizerHandleSingleton);
	}

	internal static NativeRecognizer CreateInstance()
	{
		if (RecognizerHandleSingleton != null)
		{
			return new NativeRecognizer();
		}
		return null;
	}

	internal ApplicationGesture[] SetEnabledGestures(IEnumerable<ApplicationGesture> applicationGestures)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("NativeRecognizer");
		}
		ApplicationGesture[] applicationGestureArrayAndVerify = GetApplicationGestureArrayAndVerify(applicationGestures);
		if (HRESULT.Failed(SetEnabledGestures(_hContext, applicationGestureArrayAndVerify)))
		{
			throw new InvalidOperationException(SR.UnspecifiedSetEnabledGesturesException);
		}
		return applicationGestureArrayAndVerify;
	}

	internal GestureRecognitionResult[] Recognize(StrokeCollection strokes)
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("NativeRecognizer");
		}
		if (strokes == null)
		{
			throw new ArgumentNullException("strokes");
		}
		if (strokes.Count > 2)
		{
			throw new ArgumentException(SR.StrokeCollectionCountTooBig, "strokes");
		}
		GestureRecognitionResult[] result = Array.Empty<GestureRecognitionResult>();
		if (strokes.Count == 0)
		{
			return result;
		}
		int hr = 0;
		try
		{
			hr = UnsafeNativeMethods.ResetContext(_hContext);
			if (HRESULT.Failed(hr))
			{
				return result;
			}
			hr = AddStrokes(_hContext, strokes);
			if (HRESULT.Failed(hr))
			{
				return result;
			}
			hr = UnsafeNativeMethods.Process(_hContext, out var _);
			if (HRESULT.Succeeded(hr))
			{
				result = ((!s_GetAlternateListExists) ? InvokeGetLatticePtr() : InvokeGetAlternateList());
			}
		}
		finally
		{
			if (HRESULT.Failed(hr))
			{
				throw new InvalidOperationException(SR.UnspecifiedGestureException);
			}
		}
		return result;
	}

	internal static ApplicationGesture[] GetApplicationGestureArrayAndVerify(IEnumerable<ApplicationGesture> applicationGestures)
	{
		if (applicationGestures == null)
		{
			throw new ArgumentNullException("applicationGestures");
		}
		uint num = 0u;
		if (applicationGestures is ICollection<ApplicationGesture> collection)
		{
			num = (uint)collection.Count;
		}
		else
		{
			foreach (ApplicationGesture applicationGesture in applicationGestures)
			{
				_ = applicationGesture;
				num++;
			}
		}
		if (num == 0)
		{
			throw new ArgumentException(SR.ApplicationGestureArrayLengthIsZero, "applicationGestures");
		}
		bool flag = false;
		List<ApplicationGesture> list = new List<ApplicationGesture>();
		foreach (ApplicationGesture applicationGesture2 in applicationGestures)
		{
			if (!ApplicationGestureHelper.IsDefined(applicationGesture2))
			{
				throw new ArgumentException(SR.ApplicationGestureIsInvalid, "applicationGestures");
			}
			if (applicationGesture2 == ApplicationGesture.AllGestures)
			{
				flag = true;
			}
			if (list.Contains(applicationGesture2))
			{
				throw new ArgumentException(SR.DuplicateApplicationGestureFound, "applicationGestures");
			}
			list.Add(applicationGesture2);
		}
		if (flag && list.Count != 1)
		{
			throw new ArgumentException(SR.AllGesturesMustExistAlone, "applicationGestures");
		}
		return list.ToArray();
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_hContext.Dispose();
			_disposed = true;
		}
	}

	private static bool LoadRecognizerDll()
	{
		string text = null;
		RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\MICROSOFT\\TPG\\SYSTEM RECOGNIZERS\\{BED9A940-7D48-48E3-9A68-F4887A5A1B2E}");
		if (registryKey != null)
		{
			try
			{
				text = registryKey.GetValue("RECOGNIZER DLL") as string;
				if (text == null)
				{
					return false;
				}
			}
			finally
			{
				registryKey.Close();
			}
			if (text != null)
			{
				nint num = MS.Win32.UnsafeNativeMethods.LoadLibrary(text);
				s_GetAlternateListExists = false;
				if (num != IntPtr.Zero)
				{
					s_GetAlternateListExists = MS.Win32.UnsafeNativeMethods.GetProcAddressNoThrow(new HandleRef(null, num), "GetAlternateList") != IntPtr.Zero;
				}
				return num != IntPtr.Zero;
			}
			return false;
		}
		return false;
	}

	private int SetEnabledGestures(ContextSafeHandle recContext, ApplicationGesture[] enabledGestures)
	{
		uint num = (uint)enabledGestures.Length;
		CHARACTER_RANGE[] array = new CHARACTER_RANGE[num];
		if (num == 1 && enabledGestures[0] == ApplicationGesture.AllGestures)
		{
			array[0].cChars = 256;
			array[0].wcLow = 61440;
		}
		else
		{
			for (int i = 0; i < num; i++)
			{
				array[i].cChars = 1;
				array[i].wcLow = (ushort)enabledGestures[i];
			}
		}
		return UnsafeNativeMethods.SetEnabledUnicodeRanges(recContext, num, array);
	}

	private int AddStrokes(ContextSafeHandle recContext, StrokeCollection strokes)
	{
		foreach (Stroke stroke in strokes)
		{
			PACKET_DESCRIPTION packetDescription = default(PACKET_DESCRIPTION);
			nint packets = IntPtr.Zero;
			try
			{
				GetPacketData(stroke, out packetDescription, out var countOfBytes, out packets, out var xForm);
				if (packets == IntPtr.Zero)
				{
					return -2147483640;
				}
				int num = UnsafeNativeMethods.AddStroke(recContext, ref packetDescription, (uint)countOfBytes, packets, xForm);
				if (HRESULT.Failed(num))
				{
					return num;
				}
			}
			finally
			{
				ReleaseResourcesinPacketDescription(packetDescription, packets);
			}
		}
		return UnsafeNativeMethods.EndInkInput(recContext);
	}

	private unsafe void GetPacketData(Stroke stroke, out PACKET_DESCRIPTION packetDescription, out int countOfBytes, out nint packets, out MS.Win32.NativeMethods.XFORM xForm)
	{
		countOfBytes = 0;
		packets = IntPtr.Zero;
		packetDescription = default(PACKET_DESCRIPTION);
		Matrix identity = Matrix.Identity;
		xForm = new MS.Win32.NativeMethods.XFORM((float)identity.M11, (float)identity.M12, (float)identity.M21, (float)identity.M22, (float)identity.OffsetX, (float)identity.OffsetY);
		StylusPointCollection stylusPointCollection = stroke.StylusPoints;
		if (stylusPointCollection.Count != 0)
		{
			if (stylusPointCollection.Description.PropertyCount > 3)
			{
				StylusPointDescription subsetToReformatTo = new StylusPointDescription(new StylusPointPropertyInfo[3]
				{
					new StylusPointPropertyInfo(StylusPointProperties.X),
					new StylusPointPropertyInfo(StylusPointProperties.Y),
					stylusPointCollection.Description.GetPropertyInfo(StylusPointProperties.NormalPressure)
				});
				stylusPointCollection = stylusPointCollection.Reformat(subsetToReformatTo);
			}
			if (stylusPointCollection.Count > 10000)
			{
				stylusPointCollection = stylusPointCollection.Clone(10000);
			}
			Guid[] array = new Guid[3]
			{
				StylusPointPropertyIds.X,
				StylusPointPropertyIds.Y,
				StylusPointPropertyIds.NormalPressure
			};
			packetDescription.cbPacketSize = (uint)(array.Length * Marshal.SizeOf(typeof(int)));
			packetDescription.cPacketProperties = (uint)array.Length;
			StylusPointPropertyInfo[] array2 = new StylusPointPropertyInfo[3]
			{
				StylusPointPropertyInfoDefaults.X,
				StylusPointPropertyInfoDefaults.Y,
				stylusPointCollection.Description.GetPropertyInfo(StylusPointProperties.NormalPressure)
			};
			PACKET_PROPERTY[] array3 = new PACKET_PROPERTY[packetDescription.cPacketProperties];
			for (int i = 0; i < packetDescription.cPacketProperties; i++)
			{
				array3[i].guid = array[i];
				StylusPointPropertyInfo stylusPointPropertyInfo = array2[i];
				PROPERTY_METRICS propertyMetrics = default(PROPERTY_METRICS);
				propertyMetrics.nLogicalMin = stylusPointPropertyInfo.Minimum;
				propertyMetrics.nLogicalMax = stylusPointPropertyInfo.Maximum;
				propertyMetrics.Units = (int)stylusPointPropertyInfo.Unit;
				propertyMetrics.fResolution = stylusPointPropertyInfo.Resolution;
				array3[i].PropertyMetrics = propertyMetrics;
			}
			int cb = (int)(Marshal.SizeOf(typeof(PACKET_PROPERTY)) * packetDescription.cPacketProperties);
			packetDescription.pPacketProperties = Marshal.AllocCoTaskMem(cb);
			PACKET_PROPERTY* ptr = (PACKET_PROPERTY*)((IntPtr)packetDescription.pPacketProperties).ToPointer();
			PACKET_PROPERTY* ptr2 = ptr;
			for (int i = 0; i < packetDescription.cPacketProperties; i++)
			{
				Marshal.StructureToPtr(array3[i], new IntPtr(ptr2), fDeleteOld: false);
				ptr2++;
			}
			int[] array4 = stylusPointCollection.ToHiMetricArray();
			int num = array4.Length;
			if (num != 0)
			{
				countOfBytes = num * Marshal.SizeOf(typeof(int));
				packets = Marshal.AllocCoTaskMem(countOfBytes);
				Marshal.Copy(array4, 0, packets, num);
			}
		}
	}

	private unsafe void ReleaseResourcesinPacketDescription(PACKET_DESCRIPTION pd, nint packets)
	{
		if (pd.pPacketProperties != IntPtr.Zero)
		{
			PACKET_PROPERTY* ptr = (PACKET_PROPERTY*)((IntPtr)pd.pPacketProperties).ToPointer();
			PACKET_PROPERTY* ptr2 = ptr;
			for (int i = 0; i < pd.cPacketProperties; i++)
			{
				Marshal.DestroyStructure(new IntPtr(ptr2), typeof(PACKET_PROPERTY));
				ptr2++;
			}
			Marshal.FreeCoTaskMem(pd.pPacketProperties);
			pd.pPacketProperties = IntPtr.Zero;
		}
		if (pd.pguidButtons != IntPtr.Zero)
		{
			Marshal.FreeCoTaskMem(pd.pguidButtons);
			pd.pguidButtons = IntPtr.Zero;
		}
		if (packets != IntPtr.Zero)
		{
			Marshal.FreeCoTaskMem(packets);
			packets = IntPtr.Zero;
		}
	}

	private GestureRecognitionResult[] InvokeGetAlternateList()
	{
		GestureRecognitionResult[] result = Array.Empty<GestureRecognitionResult>();
		RECO_RANGE recoRange = default(RECO_RANGE);
		recoRange.iwcBegin = 0u;
		recoRange.cCount = 1u;
		uint cAlts = 10u;
		nint[] array = new nint[10];
		try
		{
			if (HRESULT.Succeeded(UnsafeNativeMethods.GetAlternateList(_hContext, ref recoRange, ref cAlts, array, ALT_BREAKS.ALT_BREAKS_SAME)) && cAlts != 0)
			{
				List<GestureRecognitionResult> list = new List<GestureRecognitionResult>();
				for (int i = 0; i < cAlts; i++)
				{
					uint size = 1u;
					StringBuilder stringBuilder = new StringBuilder(1);
					if (!HRESULT.Failed(UnsafeNativeMethods.GetString(array[i], out recoRange, ref size, stringBuilder)) && !HRESULT.Failed(UnsafeNativeMethods.GetConfidenceLevel(array[i], out recoRange, out var confidenceLevel)))
					{
						ApplicationGesture applicationGesture = (ApplicationGesture)stringBuilder[0];
						if (ApplicationGestureHelper.IsDefined(applicationGesture))
						{
							list.Add(new GestureRecognitionResult(confidenceLevel, applicationGesture));
						}
					}
				}
				result = list.ToArray();
			}
		}
		finally
		{
			for (int j = 0; j < cAlts; j++)
			{
				if (array[j] != IntPtr.Zero)
				{
					UnsafeNativeMethods.DestroyAlternate(array[j]);
					array[j] = IntPtr.Zero;
				}
			}
		}
		return result;
	}

	private unsafe GestureRecognitionResult[] InvokeGetLatticePtr()
	{
		GestureRecognitionResult[] result = Array.Empty<GestureRecognitionResult>();
		nint pRecoLattice = IntPtr.Zero;
		if (HRESULT.Succeeded(UnsafeNativeMethods.GetLatticePtr(_hContext, ref pRecoLattice)))
		{
			RECO_LATTICE* ptr = (RECO_LATTICE*)pRecoLattice;
			uint ulBestResultColumnCount = ptr->ulBestResultColumnCount;
			if (ulBestResultColumnCount != 0 && ptr->pLatticeColumns != IntPtr.Zero)
			{
				List<GestureRecognitionResult> list = new List<GestureRecognitionResult>();
				RECO_LATTICE_COLUMN* pLatticeColumns = (RECO_LATTICE_COLUMN*)ptr->pLatticeColumns;
				ulong* pulBestResultColumns = (ulong*)ptr->pulBestResultColumns;
				for (uint num = 0u; num < ulBestResultColumnCount; num++)
				{
					ulong num2 = pulBestResultColumns[num];
					RECO_LATTICE_COLUMN rECO_LATTICE_COLUMN = pLatticeColumns[num2];
					for (int i = 0; i < rECO_LATTICE_COLUMN.cLatticeElements; i++)
					{
						RECO_LATTICE_ELEMENT rECO_LATTICE_ELEMENT = *(RECO_LATTICE_ELEMENT*)(rECO_LATTICE_COLUMN.pLatticeElements + (nint)i * (nint)sizeof(RECO_LATTICE_ELEMENT));
						if (rECO_LATTICE_ELEMENT.type != 1)
						{
							continue;
						}
						RecognitionConfidence confidence = RecognitionConfidence.Poor;
						RECO_LATTICE_PROPERTIES epProp = rECO_LATTICE_ELEMENT.epProp;
						uint cProperties = epProp.cProperties;
						RECO_LATTICE_PROPERTY** apProps = (RECO_LATTICE_PROPERTY**)epProp.apProps;
						for (int j = 0; j < cProperties; j++)
						{
							RECO_LATTICE_PROPERTY* ptr2 = apProps[j];
							if (ptr2->guidProperty == GUID_CONFIDENCELEVEL)
							{
								RecognitionConfidence pPropertyValue = *(RecognitionConfidence*)ptr2->pPropertyValue;
								if (pPropertyValue >= RecognitionConfidence.Strong && pPropertyValue <= RecognitionConfidence.Poor)
								{
									confidence = pPropertyValue;
								}
								break;
							}
						}
						ApplicationGesture applicationGesture = (ApplicationGesture)(ushort)rECO_LATTICE_ELEMENT.pData;
						if (ApplicationGestureHelper.IsDefined(applicationGesture))
						{
							list.Add(new GestureRecognitionResult(confidence, applicationGesture));
						}
					}
				}
				result = list.ToArray();
			}
		}
		return result;
	}
}
