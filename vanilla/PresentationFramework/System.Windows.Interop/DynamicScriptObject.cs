using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Interop;

/// <summary>Enables calls from a XAML browser application (XBAP) to the HTML window that hosts the application. </summary>
public sealed class DynamicScriptObject : DynamicObject
{
	private MS.Win32.UnsafeNativeMethods.IDispatch _scriptObject;

	private MS.Win32.UnsafeNativeMethods.IDispatchEx _scriptObjectEx;

	private Dictionary<string, int> _dispIdCache = new Dictionary<string, int>();

	internal MS.Win32.UnsafeNativeMethods.IDispatch ScriptObject => _scriptObject;

	internal DynamicScriptObject(MS.Win32.UnsafeNativeMethods.IDispatch scriptObject)
	{
		if (scriptObject == null)
		{
			throw new ArgumentNullException("scriptObject");
		}
		_scriptObject = scriptObject;
		_scriptObjectEx = _scriptObject as MS.Win32.UnsafeNativeMethods.IDispatchEx;
	}

	/// <summary>Calls a method on the script object. </summary>
	/// <returns>Always return true. </returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="args">The arguments to pass to the default method.</param>
	/// <param name="result">The method result.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.</exception>
	/// <exception cref="T:System.MissingMethodException">The method does not exist.</exception>
	public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		result = InvokeAndReturn(binder.Name, 1, args);
		return true;
	}

	/// <summary>Gets an member value from the script object.</summary>
	/// <returns>Always returns true. </returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="result">The method result.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.</exception>
	/// <exception cref="T:System.MissingMemberException">The member does not exist.</exception>
	public override bool TryGetMember(GetMemberBinder binder, out object result)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		result = InvokeAndReturn(binder.Name, 2, null);
		return true;
	}

	/// <summary>Sets a member on the script object to the specified value.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="value">The value to set for the member.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.-or-<paramref name="indexes" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="indexes" /> is not equal to 1.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The first <paramref name="indexes" /> value is null.</exception>
	/// <exception cref="T:System.MissingMemberException">The member does not exist.</exception>
	public override bool TrySetMember(SetMemberBinder binder, object value)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		int propertyPutMethod = GetPropertyPutMethod(value);
		InvokeAndReturn(binder.Name, propertyPutMethod, new object[1] { value });
		return true;
	}

	/// <summary>Gets an indexed value from the script object by using the first index value from the <paramref name="indexes" /> collection.</summary>
	/// <returns>Always returns true. </returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="indexes">The index to be retrieved.</param>
	/// <param name="result">The method result.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.-or-<paramref name="indexes" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="indexes" /> is not equal to 1.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The first <paramref name="indexes" /> value is null.</exception>
	/// <exception cref="T:System.MissingMemberException">The member does not exist.</exception>
	public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		if (BrowserInteropHelper.IsHostedInIEorWebOC && TryFindMemberAndInvoke(null, 1, cacheDispId: false, indexes, out result))
		{
			return true;
		}
		if (indexes.Length != 1)
		{
			throw new ArgumentException("indexes", HRESULT.DISP_E_BADPARAMCOUNT.GetException());
		}
		object obj = indexes[0];
		if (obj == null)
		{
			throw new ArgumentOutOfRangeException("indexes");
		}
		result = InvokeAndReturn(obj.ToString(), 2, cacheDispId: false, null);
		return true;
	}

	/// <summary>Sets a member on the script object by using the first index specified in the <paramref name="indexes" /> collection.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="indexes">The index to be retrieved.</param>
	/// <param name="value">The method result</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.-or-<paramref name="indexes" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">The length of <paramref name="indexes" /> is not equal to 1.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The first <paramref name="indexes" /> value is null.</exception>
	/// <exception cref="T:System.MissingMemberException">The member does not exist.</exception>
	public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		if (indexes == null)
		{
			throw new ArgumentNullException("indexes");
		}
		if (indexes.Length != 1)
		{
			throw new ArgumentException("indexes", HRESULT.DISP_E_BADPARAMCOUNT.GetException());
		}
		object obj = indexes[0];
		if (obj == null)
		{
			throw new ArgumentOutOfRangeException("indexes");
		}
		InvokeAndReturn(obj.ToString(), 4, cacheDispId: false, new object[1] { value });
		return true;
	}

	/// <summary>Calls the default script method.</summary>
	/// <returns>Always return true.</returns>
	/// <param name="binder">The binder provided by the call site.</param>
	/// <param name="args">The arguments to pass to the default method.</param>
	/// <param name="result">The method result.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="binder" /> is null.</exception>
	/// <exception cref="T:System.MissingMethodException">The method does not exist.</exception>
	public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
	{
		if (binder == null)
		{
			throw new ArgumentNullException("binder");
		}
		result = InvokeAndReturn(null, 1, args);
		return true;
	}

	/// <summary>Attempts to convert the script object to a string representation.</summary>
	/// <returns>A string representation of the script object, if the object can be converted; otherwise, a string representation of the object's default property or method.</returns>
	public override string ToString()
	{
		_ = Guid.Empty;
		object result = null;
		MS.Win32.NativeMethods.DISPPARAMS dp = new MS.Win32.NativeMethods.DISPPARAMS();
		HRESULT hRESULT;
		if (TryGetDispIdForMember("toString", cacheDispId: true, out var dispid))
		{
			hRESULT = InvokeOnScriptObject(dispid, 1, dp, null, out result);
		}
		else
		{
			dispid = 0;
			hRESULT = InvokeOnScriptObject(dispid, 2, dp, null, out result);
			if (hRESULT.Failed)
			{
				hRESULT = InvokeOnScriptObject(dispid, 1, dp, null, out result);
			}
		}
		if (hRESULT.Succeeded && result != null)
		{
			return result.ToString();
		}
		return base.ToString();
	}

	internal unsafe bool TryFindMemberAndInvokeNonWrapped(string memberName, int flags, bool cacheDispId, object[] args, out object result)
	{
		result = null;
		if (!TryGetDispIdForMember(memberName, cacheDispId, out var dispid))
		{
			return false;
		}
		MS.Win32.NativeMethods.DISPPARAMS dISPPARAMS = new MS.Win32.NativeMethods.DISPPARAMS();
		int num = -3;
		if (flags == 4 || flags == 8)
		{
			dISPPARAMS.cNamedArgs = 1u;
			dISPPARAMS.rgdispidNamedArgs = new IntPtr(&num);
		}
		try
		{
			if (args != null)
			{
				args = (object[])args.Clone();
				Array.Reverse(args);
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i] is DynamicScriptObject dynamicScriptObject)
					{
						args[i] = dynamicScriptObject._scriptObject;
					}
					if (args[i] != null)
					{
						Type type = args[i].GetType();
						if (type.IsArray)
						{
							type = type.GetElementType();
						}
						if (!MarshalLocal.IsTypeVisibleFromCom(type) && !type.IsCOMObject && type != typeof(DateTime))
						{
							throw new ArgumentException(SR.NeedToBeComVisible);
						}
					}
				}
				dISPPARAMS.rgvarg = MS.Win32.UnsafeNativeMethods.ArrayToVARIANTHelper.ArrayToVARIANTVector(args);
				dISPPARAMS.cArgs = (uint)args.Length;
			}
			MS.Win32.NativeMethods.EXCEPINFO eXCEPINFO = new MS.Win32.NativeMethods.EXCEPINFO();
			HRESULT hRESULT = InvokeOnScriptObject(dispid, flags, dISPPARAMS, eXCEPINFO, out result);
			if (hRESULT.Failed)
			{
				if (hRESULT == HRESULT.DISP_E_MEMBERNOTFOUND)
				{
					return false;
				}
				if (hRESULT == HRESULT.SCRIPT_E_REPORTED)
				{
					eXCEPINFO.scode = hRESULT.Code;
					hRESULT = HRESULT.DISP_E_EXCEPTION;
				}
				string text = "[" + (memberName ?? "(default)") + "]";
				Exception exception = hRESULT.GetException();
				if (hRESULT == HRESULT.DISP_E_EXCEPTION)
				{
					int code = ((eXCEPINFO.scode != 0) ? eXCEPINFO.scode : eXCEPINFO.wCode);
					hRESULT = HRESULT.Make(severe: true, Facility.Dispatch, code);
					throw new TargetInvocationException(text + " " + (eXCEPINFO.bstrDescription ?? string.Empty), exception)
					{
						HelpLink = eXCEPINFO.bstrHelpFile,
						Source = eXCEPINFO.bstrSource
					};
				}
				if (hRESULT == HRESULT.DISP_E_BADPARAMCOUNT || hRESULT == HRESULT.DISP_E_PARAMNOTOPTIONAL)
				{
					throw new TargetParameterCountException(text, exception);
				}
				if (hRESULT == HRESULT.DISP_E_OVERFLOW || hRESULT == HRESULT.DISP_E_TYPEMISMATCH)
				{
					throw new ArgumentException(text, new InvalidCastException(exception.Message, hRESULT.Code));
				}
				throw exception;
			}
		}
		finally
		{
			if (dISPPARAMS.rgvarg != IntPtr.Zero)
			{
				MS.Win32.UnsafeNativeMethods.ArrayToVARIANTHelper.FreeVARIANTVector(dISPPARAMS.rgvarg, args.Length);
			}
		}
		return true;
	}

	private object InvokeAndReturn(string memberName, int flags, object[] args)
	{
		return InvokeAndReturn(memberName, flags, cacheDispId: true, args);
	}

	private object InvokeAndReturn(string memberName, int flags, bool cacheDispId, object[] args)
	{
		if (!TryFindMemberAndInvoke(memberName, flags, cacheDispId, args, out var result))
		{
			if (flags == 1)
			{
				throw new MissingMethodException(ToString(), memberName);
			}
			throw new MissingMemberException(ToString(), memberName);
		}
		return result;
	}

	private bool TryFindMemberAndInvoke(string memberName, int flags, bool cacheDispId, object[] args, out object result)
	{
		if (!TryFindMemberAndInvokeNonWrapped(memberName, flags, cacheDispId, args, out result))
		{
			return false;
		}
		if (result != null && Marshal.IsComObject(result))
		{
			result = new DynamicScriptObject((MS.Win32.UnsafeNativeMethods.IDispatch)result);
		}
		return true;
	}

	private bool TryGetDispIdForMember(string memberName, bool cacheDispId, out int dispid)
	{
		dispid = 0;
		if (!string.IsNullOrEmpty(memberName) && (!cacheDispId || !_dispIdCache.TryGetValue(memberName, out dispid)))
		{
			Guid riid = Guid.Empty;
			string[] rgszNames = new string[1] { memberName };
			int[] array = new int[1] { -1 };
			HRESULT iDsOfNames = _scriptObject.GetIDsOfNames(ref riid, rgszNames, array.Length, Thread.CurrentThread.CurrentCulture.LCID, array);
			if (iDsOfNames == HRESULT.DISP_E_UNKNOWNNAME)
			{
				return false;
			}
			iDsOfNames.ThrowIfFailed();
			dispid = array[0];
			if (cacheDispId)
			{
				_dispIdCache[memberName] = dispid;
			}
		}
		return true;
	}

	private HRESULT InvokeOnScriptObject(int dispid, int flags, MS.Win32.NativeMethods.DISPPARAMS dp, MS.Win32.NativeMethods.EXCEPINFO exInfo, out object result)
	{
		if (_scriptObjectEx != null)
		{
			return _scriptObjectEx.InvokeEx(dispid, Thread.CurrentThread.CurrentCulture.LCID, flags, dp, out result, exInfo, BrowserInteropHelper.HostHtmlDocumentServiceProvider);
		}
		Guid riid = Guid.Empty;
		return _scriptObject.Invoke(dispid, ref riid, Thread.CurrentThread.CurrentCulture.LCID, flags, dp, out result, exInfo, null);
	}

	private static int GetPropertyPutMethod(object value)
	{
		if (value == null)
		{
			return 8;
		}
		Type type = value.GetType();
		if (type.IsValueType || type.IsArray || type == typeof(string) || type == typeof(CurrencyWrapper) || type == typeof(DBNull) || type == typeof(Missing))
		{
			return 4;
		}
		return 8;
	}
}
