using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using WinRT.Interop;

namespace WinRT;

internal static class ComWrappersSupport
{
	internal class InspectableInfo
	{
		public Guid[] IIDs { get; }

		public string RuntimeClassName => "";

		public InspectableInfo(Type type, Guid[] iids)
		{
			IIDs = iids;
		}
	}

	private static readonly ConcurrentDictionary<string, Func<IInspectable, object>> TypedObjectFactoryCache;

	private static readonly Guid IID_IAgileObject;

	public static readonly ConditionalWeakTable<object, InspectableInfo> InspectableInfoTable;

	private static ComWrappers ComWrappers { get; }

	public static IUnknownVftbl IUnknownVftbl { get; private set; }

	static ComWrappersSupport()
	{
		TypedObjectFactoryCache = new ConcurrentDictionary<string, Func<IInspectable, object>>();
		IID_IAgileObject = Guid.Parse("94ea2b94-e9cc-49e0-c0ff-ee64ca8f5b90");
		InspectableInfoTable = new ConditionalWeakTable<object, InspectableInfo>();
		ComWrappers = new WpfWinRTComWrappers();
		PlatformSpecificInitialize();
	}

	private static void PlatformSpecificInitialize()
	{
		IUnknownVftbl = WpfWinRTComWrappers.IUnknownVftbl;
	}

	public static TReturn MarshalDelegateInvoke<TDelegate, TReturn>(nint thisPtr, Func<TDelegate, TReturn> invoke) where TDelegate : class, Delegate
	{
		TDelegate val = FindObject<TDelegate>(thisPtr);
		if (val != null)
		{
			return invoke(val);
		}
		return default(TReturn);
	}

	public static void MarshalDelegateInvoke<T>(nint thisPtr, Action<T> invoke) where T : class, Delegate
	{
		T val = FindObject<T>(thisPtr);
		if (val != null)
		{
			invoke(val);
		}
	}

	public static bool TryUnwrapObject(object o, out IObjectReference objRef)
	{
		if (o is Delegate @delegate)
		{
			return TryUnwrapObject(@delegate.Target, out objRef);
		}
		Type type = o.GetType();
		ObjectReferenceWrapperAttribute customAttribute = type.GetCustomAttribute<ObjectReferenceWrapperAttribute>();
		if (customAttribute != null)
		{
			objRef = (IObjectReference)type.GetField(customAttribute.ObjectReferenceField, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(o);
			return true;
		}
		ProjectedRuntimeClassAttribute customAttribute2 = type.GetCustomAttribute<ProjectedRuntimeClassAttribute>();
		if (customAttribute2 != null)
		{
			return TryUnwrapObject(type.GetProperty(customAttribute2.DefaultInterfaceProperty, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(o), out objRef);
		}
		objRef = null;
		return false;
	}

	public static IObjectReference GetObjectReferenceForInterface(nint externalComObject)
	{
		using ObjectReference<IUnknownVftbl> objectReference = ObjectReference<IUnknownVftbl>.FromAbi(externalComObject);
		if (objectReference.TryAs(IID_IAgileObject, out ObjectReference<IUnknownVftbl> objRef) >= 0)
		{
			objRef.Dispose();
			return objectReference.As<IUnknownVftbl>();
		}
		return new ObjectReferenceWithContext<IUnknownVftbl>(objectReference.GetRef(), Context.GetContextCallback());
	}

	public static List<ComWrappers.ComInterfaceEntry> GetInterfaceTableEntries(object obj)
	{
		List<ComWrappers.ComInterfaceEntry> list = new List<ComWrappers.ComInterfaceEntry>();
		Type[] interfaces = obj.GetType().GetInterfaces();
		foreach (Type type in interfaces)
		{
			if (Projections.IsTypeWindowsRuntimeType(type))
			{
				Type type2 = type.FindHelperType();
				list.Add(new ComWrappers.ComInterfaceEntry
				{
					IID = GuidGenerator.GetIID(type2),
					Vtable = (nint)type2.FindVftblType().GetField("AbiToProjectionVftablePtr", BindingFlags.Static | BindingFlags.Public).GetValue(null)
				});
			}
			if (type.IsConstructedGenericType && Projections.TryGetCompatibleWindowsRuntimeTypeForVariantType(type, out var compatibleType))
			{
				Type type3 = compatibleType.FindHelperType();
				list.Add(new ComWrappers.ComInterfaceEntry
				{
					IID = GuidGenerator.GetIID(type3),
					Vtable = (nint)type3.FindVftblType().GetField("AbiToProjectionVftablePtr", BindingFlags.Static | BindingFlags.Public).GetValue(null)
				});
			}
		}
		if (obj is Delegate)
		{
			list.Add(new ComWrappers.ComInterfaceEntry
			{
				IID = GuidGenerator.GetIID(obj.GetType()),
				Vtable = (nint)obj.GetType().GetHelperType().GetField("AbiToProjectionVftablePtr", BindingFlags.Static | BindingFlags.Public)
					.GetValue(null)
			});
		}
		list.Add(new ComWrappers.ComInterfaceEntry
		{
			IID = IID_IAgileObject,
			Vtable = IUnknownVftbl.AbiToProjectionVftblPtr
		});
		return list;
	}

	public static (InspectableInfo inspectableInfo, List<ComWrappers.ComInterfaceEntry> interfaceTableEntries) PregenerateNativeTypeInformation(object obj)
	{
		List<ComWrappers.ComInterfaceEntry> interfaceTableEntries = GetInterfaceTableEntries(obj);
		Guid[] array = new Guid[interfaceTableEntries.Count];
		for (int i = 0; i < interfaceTableEntries.Count; i++)
		{
			array[i] = interfaceTableEntries[i].IID;
		}
		Type type = obj.GetType();
		if (type.FullName.StartsWith("ABI."))
		{
			type = Projections.FindCustomPublicTypeForAbiType(type) ?? type.Assembly.GetType(type.FullName.Substring("ABI.".Length)) ?? type;
		}
		return (inspectableInfo: new InspectableInfo(type, array), interfaceTableEntries: interfaceTableEntries);
	}

	public static InspectableInfo GetInspectableInfo(nint pThis)
	{
		object key = FindObject<object>(pThis);
		return InspectableInfoTable.GetValue(key, (object o) => PregenerateNativeTypeInformation(o).inspectableInfo);
	}

	public static object CreateRcwForComObject(nint ptr)
	{
		return ComWrappers.GetOrCreateObjectForComInstance(ptr, CreateObjectFlags.TrackerObject);
	}

	public static void RegisterObjectForInterface(object obj, nint thisPtr)
	{
		TryRegisterObjectForInterface(obj, thisPtr);
	}

	public static object TryRegisterObjectForInterface(object obj, nint thisPtr)
	{
		return ComWrappers.GetOrRegisterObjectForComInstance(thisPtr, CreateObjectFlags.TrackerObject, obj);
	}

	public static IObjectReference CreateCCWForObject(object obj)
	{
		nint thisPtr = ComWrappers.GetOrCreateComInterfaceForObject(obj, CreateComInterfaceFlags.TrackerSupport);
		return ObjectReference<IUnknownVftbl>.Attach(ref thisPtr);
	}

	public unsafe static T FindObject<T>(nint ptr) where T : class
	{
		return ComWrappers.ComInterfaceDispatch.GetInstance<T>((ComWrappers.ComInterfaceDispatch*)ptr);
	}

	private static T FindDelegate<T>(nint thisPtr) where T : class, Delegate
	{
		return FindObject<T>(thisPtr);
	}

	public static nint AllocateVtableMemory(Type vtableType, int size)
	{
		return RuntimeHelpers.AllocateTypeAssociatedMemory(vtableType, size);
	}
}
