using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using MS.Internal.PresentationFramework.Interop;
using MS.Internal.WindowsRuntime.ABI.Windows.Data.Text;
using MS.Internal.WindowsRuntime.Windows.Globalization;
using WinRT;
using WinRT.Interop;

namespace MS.Internal.WindowsRuntime.Windows.Data.Text;

[WindowsRuntimeType]
[ProjectedRuntimeClass("_default")]
internal sealed class WordsSegmenter : ICustomQueryInterface, IEquatable<WordsSegmenter>
{
	internal class _IWordsSegmenterFactory : MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenterFactory
	{
		private static WeakLazy<_IWordsSegmenterFactory> _instance = new WeakLazy<_IWordsSegmenterFactory>();

		public static _IWordsSegmenterFactory Instance => _instance.Value;

		public _IWordsSegmenterFactory()
			: base(ActivationFactory<WordsSegmenter>.As<Vftbl>())
		{
		}

		public new nint CreateWithLanguage(string language)
		{
			MarshalString m = null;
			nint result = 0;
			try
			{
				m = MarshalString.CreateMarshaler(language);
				ExceptionHelpers.ThrowExceptionForHR(_obj.Vftbl.CreateWithLanguage_0(base.ThisPtr, MarshalString.GetAbi(m), out result));
				return result;
			}
			finally
			{
				MarshalString.DisposeMarshaler(m);
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct InterfaceTag<I>
	{
	}

	private IObjectReference _inner;

	private readonly Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter> _defaultLazy;

	public static readonly string Undetermined = "und";

	public nint ThisPtr => _default.ThisPtr;

	private MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter _default => _defaultLazy.Value;

	public string ResolvedLanguage => _default.ResolvedLanguage;

	private static List<string> ScriptCodesRequiringDedicatedSegmenter { get; } = new List<string>
	{
		"Bopo", "Brah", "Egyp", "Goth", "Hang", "Hani", "Ital", "Java", "Kana", "Khar",
		"Laoo", "Lisu", "Mymr", "Talu", "Thai", "Tibt", "Xsux", "Yiii"
	};

	public WordsSegmenter(string language)
		: this(((Func<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter>)delegate
		{
			nint num = _IWordsSegmenterFactory.Instance.CreateWithLanguage(language);
			try
			{
				return new MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter(ComWrappersSupport.GetObjectReferenceForInterface(num));
			}
			finally
			{
				MarshalInspectable.DisposeAbi(num);
			}
		})())
	{
		ComWrappersSupport.RegisterObjectForInterface(this, ThisPtr);
	}

	public static WordsSegmenter FromAbi(nint thisPtr)
	{
		if (thisPtr == IntPtr.Zero)
		{
			return null;
		}
		object obj = MarshalInspectable.FromAbi(thisPtr);
		if (!(obj is WordsSegmenter))
		{
			return new WordsSegmenter((MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter)obj);
		}
		return (WordsSegmenter)obj;
	}

	public WordsSegmenter(MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter ifc)
	{
		_defaultLazy = new Lazy<MS.Internal.WindowsRuntime.ABI.Windows.Data.Text.IWordsSegmenter>(() => ifc);
	}

	public static bool operator ==(WordsSegmenter x, WordsSegmenter y)
	{
		return (x?.ThisPtr ?? IntPtr.Zero) == (y?.ThisPtr ?? IntPtr.Zero);
	}

	public static bool operator !=(WordsSegmenter x, WordsSegmenter y)
	{
		return !(x == y);
	}

	public bool Equals(WordsSegmenter other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is WordsSegmenter wordsSegmenter)
		{
			return this == wordsSegmenter;
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

	private IWordsSegmenter AsInternal(InterfaceTag<IWordsSegmenter> _)
	{
		return _default;
	}

	public WordSegment GetTokenAt(string text, uint startIndex)
	{
		return _default.GetTokenAt(text, startIndex);
	}

	public IReadOnlyList<WordSegment> GetTokens(string text)
	{
		return _default.GetTokens(text);
	}

	public void Tokenize(string text, uint startIndex, WordSegmentsTokenizingHandler handler)
	{
		_default.Tokenize(text, startIndex, handler);
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

	public static WordsSegmenter Create(string language, bool shouldPreferNeutralSegmenter = false)
	{
		if (!OSVersionHelper.IsOsWindows8Point1OrGreater)
		{
			throw new PlatformNotSupportedException();
		}
		if (shouldPreferNeutralSegmenter && !ShouldUseDedicatedSegmenter(language))
		{
			language = Undetermined;
		}
		return new WordsSegmenter(language);
	}

	private static bool ShouldUseDedicatedSegmenter(string languageTag)
	{
		bool result = true;
		try
		{
			Language language = new Language(languageTag);
			string script = language.Script;
			if (ScriptCodesRequiringDedicatedSegmenter.FindIndex((string s) => s.Equals(script, StringComparison.InvariantCultureIgnoreCase)) == -1)
			{
				result = false;
			}
		}
		catch (Exception ex) when (ex is NotSupportedException || ex is ArgumentException || ex is TargetInvocationException || ex is MissingMemberException)
		{
		}
		return result;
	}
}
