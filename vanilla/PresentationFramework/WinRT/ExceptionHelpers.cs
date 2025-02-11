using System;
using System.Collections;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using ABI.WinRT.Interop;
using WinRT.Interop;

namespace WinRT;

internal static class ExceptionHelpers
{
	internal delegate int GetRestrictedErrorInfo(out nint ppRestrictedErrorInfo);

	internal delegate int SetRestrictedErrorInfo(nint pRestrictedErrorInfo);

	internal delegate int RoOriginateLanguageException(int error, nint message, nint langaugeException);

	internal delegate int RoReportUnhandledError(nint pRestrictedErrorInfo);

	[Serializable]
	internal class __RestrictedErrorObject
	{
		[NonSerialized]
		private readonly IObjectReference _realErrorObject;

		public IObjectReference RealErrorObject => _realErrorObject;

		internal __RestrictedErrorObject(IObjectReference errorObject)
		{
			_realErrorObject = errorObject;
		}
	}

	private const int COR_E_OBJECTDISPOSED = -2146232798;

	private const int RO_E_CLOSED = -2147483629;

	internal const int E_BOUNDS = -2147483637;

	internal const int E_CHANGED_STATE = -2147483636;

	private const int E_ILLEGAL_STATE_CHANGE = -2147483635;

	private const int E_ILLEGAL_METHOD_CALL = -2147483634;

	private const int E_ILLEGAL_DELEGATE_ASSIGNMENT = -2147483624;

	private const int APPMODEL_ERROR_NO_PACKAGE = -2147009196;

	internal const int E_XAMLPARSEFAILED = -2144665590;

	internal const int E_LAYOUTCYCLE = -2144665580;

	internal const int E_ELEMENTNOTENABLED = -2144665570;

	internal const int E_ELEMENTNOTAVAILABLE = -2144665569;

	private static GetRestrictedErrorInfo getRestrictedErrorInfo;

	private static SetRestrictedErrorInfo setRestrictedErrorInfo;

	private static RoOriginateLanguageException roOriginateLanguageException;

	private static RoReportUnhandledError roReportUnhandledError;

	[DllImport("oleaut32.dll")]
	private static extern int SetErrorInfo(uint dwReserved, nint perrinfo);

	static ExceptionHelpers()
	{
		nint num = Platform.LoadLibraryExW("api-ms-win-core-winrt-error-l1-1-1.dll", IntPtr.Zero, 2048u);
		if (num != IntPtr.Zero)
		{
			getRestrictedErrorInfo = Platform.GetProcAddress<GetRestrictedErrorInfo>(num);
			setRestrictedErrorInfo = Platform.GetProcAddress<SetRestrictedErrorInfo>(num);
			roOriginateLanguageException = Platform.GetProcAddress<RoOriginateLanguageException>(num);
			roReportUnhandledError = Platform.GetProcAddress<RoReportUnhandledError>(num);
			return;
		}
		num = Platform.LoadLibraryExW("api-ms-win-core-winrt-error-l1-1-0.dll", IntPtr.Zero, 2048u);
		if (num != IntPtr.Zero)
		{
			getRestrictedErrorInfo = Platform.GetProcAddress<GetRestrictedErrorInfo>(num);
			setRestrictedErrorInfo = Platform.GetProcAddress<SetRestrictedErrorInfo>(num);
		}
	}

	public static void ThrowExceptionForHR(int hr)
	{
		bool restoredExceptionFromGlobalState;
		Exception exceptionForHR = GetExceptionForHR(hr, useGlobalErrorState: true, out restoredExceptionFromGlobalState);
		if (restoredExceptionFromGlobalState)
		{
			ExceptionDispatchInfo.Capture(exceptionForHR).Throw();
		}
		else if (exceptionForHR != null)
		{
			throw exceptionForHR;
		}
	}

	public static Exception GetExceptionForHR(int hr)
	{
		bool restoredExceptionFromGlobalState;
		return GetExceptionForHR(hr, useGlobalErrorState: false, out restoredExceptionFromGlobalState);
	}

	private static Exception GetExceptionForHR(int hr, bool useGlobalErrorState, out bool restoredExceptionFromGlobalState)
	{
		restoredExceptionFromGlobalState = false;
		if (hr >= 0)
		{
			return null;
		}
		ObjectReference<ABI.WinRT.Interop.IErrorInfo.Vftbl> objRef = null;
		IObjectReference restrictedErrorObject = null;
		string description = null;
		string restrictedDescription = null;
		string restrictedErrorReference = null;
		string capabilitySid = null;
		bool hasRestrictedLanguageErrorObject = false;
		Exception ex;
		if (useGlobalErrorState && getRestrictedErrorInfo != null)
		{
			Marshal.ThrowExceptionForHR(getRestrictedErrorInfo(out var ppRestrictedErrorInfo));
			if (ppRestrictedErrorInfo != IntPtr.Zero)
			{
				IObjectReference objectReference = ObjectReference<ABI.WinRT.Interop.IRestrictedErrorInfo.Vftbl>.Attach(ref ppRestrictedErrorInfo);
				restrictedErrorObject = objectReference.As<ABI.WinRT.Interop.IRestrictedErrorInfo.Vftbl>();
				ABI.WinRT.Interop.IRestrictedErrorInfo restrictedErrorInfo = new ABI.WinRT.Interop.IRestrictedErrorInfo(objectReference);
				restrictedErrorInfo.GetErrorDetails(out description, out var error, out restrictedDescription, out capabilitySid);
				restrictedErrorReference = restrictedErrorInfo.GetReference();
				if (objectReference.TryAs(out ObjectReference<ABI.WinRT.Interop.ILanguageExceptionErrorInfo.Vftbl> objRef2) >= 0)
				{
					using IObjectReference objectReference2 = ((WinRT.Interop.ILanguageExceptionErrorInfo)new ABI.WinRT.Interop.ILanguageExceptionErrorInfo(objRef2)).GetLanguageException();
					if (objectReference2 != null)
					{
						if (objectReference2.IsReferenceToManagedObject)
						{
							ex = ComWrappersSupport.FindObject<Exception>(objectReference2.ThisPtr);
							if (GetHRForException(ex) == hr)
							{
								restoredExceptionFromGlobalState = true;
								return ex;
							}
						}
						else
						{
							hasRestrictedLanguageErrorObject = true;
						}
					}
				}
				else if (hr == error)
				{
					objectReference.TryAs(out objRef);
				}
			}
		}
		using (objRef)
		{
			ex = (((uint)(hr - -2147483635) > 1u && hr != -2147483624 && hr != -2147009196) ? Marshal.GetExceptionForHR(hr, objRef?.ThisPtr ?? (-1)) : new InvalidOperationException(description));
		}
		ex.AddExceptionDataForRestrictedErrorInfo(description, restrictedDescription, restrictedErrorReference, capabilitySid, restrictedErrorObject, hasRestrictedLanguageErrorObject);
		return ex;
	}

	public unsafe static void SetErrorInfo(Exception ex)
	{
		if (getRestrictedErrorInfo != null && setRestrictedErrorInfo != null && roOriginateLanguageException != null)
		{
			if (ex.TryGetRestrictedLanguageErrorObject(out var restrictedErrorObject))
			{
				using (restrictedErrorObject)
				{
					setRestrictedErrorInfo(restrictedErrorObject.ThisPtr);
					return;
				}
			}
			string text = ex.Message;
			if (string.IsNullOrEmpty(text))
			{
				text = ex.GetType().FullName;
			}
			nint zero = default(nint);
			if (Platform.WindowsCreateString(text, text.Length, &zero) != 0)
			{
				zero = IntPtr.Zero;
			}
			using IObjectReference objectReference2 = ComWrappersSupport.CreateCCWForObject(ex);
			roOriginateLanguageException(GetHRForException(ex), zero, objectReference2.ThisPtr);
			return;
		}
		using IObjectReference objectReference3 = ComWrappersSupport.CreateCCWForObject(new ManagedExceptionErrorInfo(ex));
		SetErrorInfo(0u, objectReference3.ThisPtr);
	}

	public static void ReportUnhandledError(Exception ex)
	{
		SetErrorInfo(ex);
		if (getRestrictedErrorInfo != null && roReportUnhandledError != null)
		{
			Marshal.ThrowExceptionForHR(getRestrictedErrorInfo(out var ppRestrictedErrorInfo));
			using ObjectReference<IUnknownVftbl> objectReference = ObjectReference<IUnknownVftbl>.Attach(ref ppRestrictedErrorInfo);
			roReportUnhandledError(objectReference.ThisPtr);
		}
	}

	public static int GetHRForException(Exception ex)
	{
		int error = ex.HResult;
		if (ex.TryGetRestrictedLanguageErrorObject(out var restrictedErrorObject))
		{
			restrictedErrorObject.AsType<ABI.WinRT.Interop.IRestrictedErrorInfo>().GetErrorDetails(out var _, out error, out var _, out var _);
		}
		if (error == -2146232798)
		{
			return -2147483629;
		}
		return error;
	}

	internal static void AddExceptionDataForRestrictedErrorInfo(this Exception ex, string description, string restrictedError, string restrictedErrorReference, string restrictedCapabilitySid, IObjectReference restrictedErrorObject, bool hasRestrictedLanguageErrorObject = false)
	{
		IDictionary data = ex.Data;
		if (data != null)
		{
			data.Add("Description", description);
			data.Add("RestrictedDescription", restrictedError);
			data.Add("RestrictedErrorReference", restrictedErrorReference);
			data.Add("RestrictedCapabilitySid", restrictedCapabilitySid);
			data.Add("__RestrictedErrorObjectReference", (restrictedErrorObject == null) ? null : new __RestrictedErrorObject(restrictedErrorObject));
			data.Add("__HasRestrictedLanguageErrorObject", hasRestrictedLanguageErrorObject);
		}
	}

	internal static bool TryGetRestrictedLanguageErrorObject(this Exception ex, out IObjectReference restrictedErrorObject)
	{
		restrictedErrorObject = null;
		IDictionary data = ex.Data;
		if (data != null && data.Contains("__HasRestrictedLanguageErrorObject"))
		{
			if (data.Contains("__RestrictedErrorObjectReference") && data["__RestrictedErrorObjectReference"] is __RestrictedErrorObject _RestrictedErrorObject)
			{
				restrictedErrorObject = _RestrictedErrorObject.RealErrorObject;
			}
			return (bool)data["__HasRestrictedLanguageErrorObject"];
		}
		return false;
	}

	public static Exception AttachRestrictedErrorInfo(Exception e)
	{
		if (e != null)
		{
			try
			{
				Marshal.ThrowExceptionForHR(getRestrictedErrorInfo(out var ppRestrictedErrorInfo));
				if (ppRestrictedErrorInfo != IntPtr.Zero)
				{
					IObjectReference objectReference = ObjectReference<ABI.WinRT.Interop.IRestrictedErrorInfo.Vftbl>.Attach(ref ppRestrictedErrorInfo);
					ABI.WinRT.Interop.IRestrictedErrorInfo restrictedErrorInfo = new ABI.WinRT.Interop.IRestrictedErrorInfo(objectReference);
					restrictedErrorInfo.GetErrorDetails(out var description, out var error, out var restrictedDescription, out var capabilitySid);
					if (e.HResult == error)
					{
						e.AddExceptionDataForRestrictedErrorInfo(description, restrictedDescription, restrictedErrorInfo.GetReference(), capabilitySid, objectReference.As<ABI.WinRT.Interop.IRestrictedErrorInfo.Vftbl>());
					}
				}
			}
			catch
			{
			}
		}
		return e;
	}
}
