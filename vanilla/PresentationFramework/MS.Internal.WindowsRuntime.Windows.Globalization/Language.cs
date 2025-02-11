using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using MS.Internal.WindowsRuntime.ABI.Windows.Globalization;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.Windows.Globalization;

[WindowsRuntimeType]
[ProjectedRuntimeClass("_default")]
internal sealed class Language : ICustomQueryInterface, IEquatable<Language>
{
	internal class _ILanguageFactory : MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguageFactory
	{
		private static WeakLazy<_ILanguageFactory> _instance = new WeakLazy<_ILanguageFactory>();

		public static _ILanguageFactory Instance => _instance.Value;

		public _ILanguageFactory()
			: base(ActivationFactory<Language>.As<Vftbl>())
		{
		}

		public new nint CreateLanguage(string languageTag)
		{
			MarshalString m = null;
			nint result = 0;
			try
			{
				m = MarshalString.CreateMarshaler(languageTag);
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.CreateLanguage_0(base.ThisPtr, MarshalString.GetAbi(m), out result));
				return result;
			}
			finally
			{
				MarshalString.DisposeMarshaler(m);
			}
		}
	}

	internal class _ILanguageStatics : MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguageStatics
	{
		private static WeakLazy<_ILanguageStatics> _instance = new WeakLazy<_ILanguageStatics>();

		public static ILanguageStatics Instance => _instance.Value;

		public _ILanguageStatics()
			: base(new BaseActivationFactory("Windows.Globalization", "Windows.Globalization.Language")._As<Vftbl>())
		{
		}
	}

	internal class _ILanguageStatics2 : MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguageStatics2
	{
		private static WeakLazy<_ILanguageStatics2> _instance = new WeakLazy<_ILanguageStatics2>();

		public static ILanguageStatics2 Instance => _instance.Value;

		public _ILanguageStatics2()
			: base(new BaseActivationFactory("Windows.Globalization", "Windows.Globalization.Language")._As<Vftbl>())
		{
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct InterfaceTag<I>
	{
	}

	private IObjectReference _inner;

	private readonly Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage> _defaultLazy;

	public nint ThisPtr => _default.ThisPtr;

	private MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage _default => _defaultLazy.Value;

	public static string CurrentInputMethodLanguageTag => _ILanguageStatics.Instance.CurrentInputMethodLanguageTag;

	public string DisplayName => _default.DisplayName;

	public string LanguageTag => _default.LanguageTag;

	public LanguageLayoutDirection LayoutDirection => AsInternal(default(InterfaceTag<ILanguage2>)).LayoutDirection;

	public string NativeName => _default.NativeName;

	public string Script => _default.Script;

	public Language(string languageTag)
		: this(((Func<MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage>)delegate
		{
			nint num = _ILanguageFactory.Instance.CreateLanguage(languageTag);
			try
			{
				return new MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage(ComWrappersSupport.GetObjectReferenceForInterface(num));
			}
			finally
			{
				MarshalInspectable.DisposeAbi(num);
			}
		})())
	{
		ComWrappersSupport.RegisterObjectForInterface(this, ThisPtr);
	}

	public static bool IsWellFormed(string languageTag)
	{
		return _ILanguageStatics.Instance.IsWellFormed(languageTag);
	}

	public static bool TrySetInputMethodLanguageTag(string languageTag)
	{
		return _ILanguageStatics2.Instance.TrySetInputMethodLanguageTag(languageTag);
	}

	public static Language FromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		object obj = MarshalInspectable.FromAbi(thisPtr);
		if (!(obj is Language))
		{
			return new Language((MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage)obj);
		}
		return (Language)obj;
	}

	public Language(MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage ifc)
	{
		_defaultLazy = new Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage>(() => ifc);
	}

	public static bool operator ==(Language x, Language y)
	{
		return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
	}

	public static bool operator !=(Language x, Language y)
	{
		return !(x == y);
	}

	public bool Equals(Language other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is Language language)
		{
			return this == language;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((IntPtr)ThisPtr).GetHashCode();
	}

	private IObjectReference GetDefaultReference<T>()
	{
		return _default.AsInterface<T>();
	}

	private IObjectReference GetReferenceForQI()
	{
		return _inner ?? _default.ObjRef;
	}

	private ILanguage AsInternal(InterfaceTag<ILanguage> _)
	{
		return _default;
	}

	private ILanguageExtensionSubtags AsInternal(InterfaceTag<ILanguageExtensionSubtags> _)
	{
		return new MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguageExtensionSubtags(GetReferenceForQI());
	}

	public IReadOnlyList<string> GetExtensionSubtags(string singleton)
	{
		return AsInternal(default(InterfaceTag<ILanguageExtensionSubtags>)).GetExtensionSubtags(singleton);
	}

	private ILanguage2 AsInternal(InterfaceTag<ILanguage2> _)
	{
		return new MS.Internal.WindowsRuntime.ABI.Windows.Globalization.ILanguage2(GetReferenceForQI());
	}

	private bool IsOverridableInterface(Guid iid)
	{
		return false;
	}

	CustomQueryInterfaceResult ICustomQueryInterface.GetInterface(ref Guid iid, out nint ppv)
	{
		ppv = IntPtr.Zero;
		if (IsOverridableInterface(iid) || typeof(IInspectable).GUID == iid)
		{
			return CustomQueryInterfaceResult.NotHandled;
		}
		if (GetReferenceForQI().TryAs(iid, out ObjectReference<IUnknownVftbl> objRef) >= 0)
		{
			using (objRef)
			{
				ppv = objRef.GetRef();
				return CustomQueryInterfaceResult.Handled;
			}
		}
		return CustomQueryInterfaceResult.NotHandled;
	}
}
