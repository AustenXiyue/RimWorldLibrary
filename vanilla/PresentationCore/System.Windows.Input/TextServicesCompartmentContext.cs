using System.Collections;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Input;

internal class TextServicesCompartmentContext
{
	private Hashtable _compartmentTable;

	private Hashtable _globalcompartmentTable;

	private MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr _globalcompartmentmanager;

	internal static TextServicesCompartmentContext Current
	{
		get
		{
			if (InputMethod.Current.TextServicesCompartmentContext == null)
			{
				InputMethod.Current.TextServicesCompartmentContext = new TextServicesCompartmentContext();
			}
			return InputMethod.Current.TextServicesCompartmentContext;
		}
	}

	private TextServicesCompartmentContext()
	{
	}

	internal TextServicesCompartment GetCompartment(InputMethodStateType statetype)
	{
		for (int i = 0; i < InputMethodEventTypeInfo.InfoList.Length; i++)
		{
			InputMethodEventTypeInfo inputMethodEventTypeInfo = InputMethodEventTypeInfo.InfoList[i];
			if (inputMethodEventTypeInfo.Type == statetype)
			{
				if (inputMethodEventTypeInfo.Scope == CompartmentScope.Thread)
				{
					return GetThreadCompartment(inputMethodEventTypeInfo.Guid);
				}
				if (inputMethodEventTypeInfo.Scope == CompartmentScope.Global)
				{
					return GetGlobalCompartment(inputMethodEventTypeInfo.Guid);
				}
			}
		}
		return null;
	}

	internal TextServicesCompartment GetThreadCompartment(Guid guid)
	{
		if (!TextServicesLoader.ServicesInstalled || TextServicesContext.DispatcherCurrent == null)
		{
			return null;
		}
		MS.Win32.UnsafeNativeMethods.ITfThreadMgr threadManager = TextServicesContext.DispatcherCurrent.ThreadManager;
		if (threadManager == null)
		{
			return null;
		}
		if (_compartmentTable == null)
		{
			_compartmentTable = new Hashtable();
		}
		TextServicesCompartment textServicesCompartment = _compartmentTable[guid] as TextServicesCompartment;
		if (textServicesCompartment == null)
		{
			textServicesCompartment = new TextServicesCompartment(guid, threadManager as MS.Win32.UnsafeNativeMethods.ITfCompartmentMgr);
			_compartmentTable[guid] = textServicesCompartment;
		}
		return textServicesCompartment;
	}

	internal TextServicesCompartment GetGlobalCompartment(Guid guid)
	{
		if (!TextServicesLoader.ServicesInstalled || TextServicesContext.DispatcherCurrent == null)
		{
			return null;
		}
		if (_globalcompartmentTable == null)
		{
			_globalcompartmentTable = new Hashtable();
		}
		if (_globalcompartmentmanager == null)
		{
			MS.Win32.UnsafeNativeMethods.ITfThreadMgr threadManager = TextServicesContext.DispatcherCurrent.ThreadManager;
			if (threadManager == null)
			{
				return null;
			}
			threadManager.GetGlobalCompartment(out _globalcompartmentmanager);
		}
		TextServicesCompartment textServicesCompartment = null;
		textServicesCompartment = _globalcompartmentTable[guid] as TextServicesCompartment;
		if (textServicesCompartment == null)
		{
			textServicesCompartment = new TextServicesCompartment(guid, _globalcompartmentmanager);
			_globalcompartmentTable[guid] = textServicesCompartment;
		}
		return textServicesCompartment;
	}
}
