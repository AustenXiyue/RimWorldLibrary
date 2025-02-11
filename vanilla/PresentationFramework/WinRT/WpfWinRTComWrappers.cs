using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;
using MS.Internal.WindowsRuntime.Windows.Data.Text;
using WinRT.Interop;

namespace WinRT;

internal class WpfWinRTComWrappers : ComWrappers
{
	private class VtableEntriesCleanupScout
	{
		private unsafe readonly ComInterfaceEntry* _data;

		public unsafe VtableEntriesCleanupScout(ComInterfaceEntry* data)
		{
			_data = data;
		}

		unsafe ~VtableEntriesCleanupScout()
		{
			Marshal.FreeCoTaskMem((nint)_data);
		}
	}

	private static ConditionalWeakTable<object, VtableEntriesCleanupScout> ComInterfaceEntryCleanupTable;

	public static IUnknownVftbl IUnknownVftbl { get; }

	static WpfWinRTComWrappers()
	{
		ComInterfaceEntryCleanupTable = new ConditionalWeakTable<object, VtableEntriesCleanupScout>();
		ComWrappers.GetIUnknownImpl(out var fpQueryInterface, out var fpAddRef, out var fpRelease);
		IUnknownVftbl = new IUnknownVftbl
		{
			QueryInterface = Marshal.GetDelegateForFunctionPointer<IUnknownVftbl._QueryInterface>(fpQueryInterface),
			AddRef = Marshal.GetDelegateForFunctionPointer<IUnknownVftbl._AddRef>(fpAddRef),
			Release = Marshal.GetDelegateForFunctionPointer<IUnknownVftbl._Release>(fpRelease)
		};
	}

	protected unsafe override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
	{
		List<ComInterfaceEntry> interfaceTableEntries = ComWrappersSupport.GetInterfaceTableEntries(obj);
		if (flags.HasFlag(CreateComInterfaceFlags.CallerDefinedIUnknown))
		{
			interfaceTableEntries.Add(new ComInterfaceEntry
			{
				IID = typeof(IUnknownVftbl).GUID,
				Vtable = IUnknownVftbl.AbiToProjectionVftblPtr
			});
		}
		interfaceTableEntries.Add(new ComInterfaceEntry
		{
			IID = typeof(IInspectable).GUID,
			Vtable = IInspectable.Vftbl.AbiToProjectionVftablePtr
		});
		count = interfaceTableEntries.Count;
		ComInterfaceEntry* ptr = (ComInterfaceEntry*)Marshal.AllocCoTaskMem(sizeof(ComInterfaceEntry) * count);
		for (int i = 0; i < count; i++)
		{
			ptr[i] = interfaceTableEntries[i];
		}
		ComInterfaceEntryCleanupTable.Add(obj, new VtableEntriesCleanupScout(ptr));
		return ptr;
	}

	protected override object CreateObject(nint externalComObject, CreateObjectFlags flags)
	{
		IObjectReference objectReferenceForInterface = ComWrappersSupport.GetObjectReferenceForInterface(externalComObject);
		if (objectReferenceForInterface.TryAs(out ObjectReference<IInspectable.Vftbl> objRef) == 0)
		{
			IInspectable inspectable = new IInspectable(objRef);
			if (inspectable.GetRuntimeClassName(noThrow: true) == "Windows.Data.Text.WordSegment")
			{
				return new MS.Internal.WindowsRuntime.Windows.Data.Text.WordSegment(new MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordSegment(objectReferenceForInterface));
			}
			return inspectable;
		}
		return null;
	}

	protected override void ReleaseObjects(IEnumerable objects)
	{
		foreach (object @object in objects)
		{
			if (ComWrappersSupport.TryUnwrapObject(@object, out var objRef))
			{
				objRef.Dispose();
				continue;
			}
			throw new InvalidOperationException("Cannot release objects that are not runtime wrappers of native WinRT objects.");
		}
	}
}
