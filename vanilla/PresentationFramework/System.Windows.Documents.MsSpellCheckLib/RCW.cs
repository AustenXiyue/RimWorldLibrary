using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Windows.Documents.MsSpellCheckLib;

internal class RCW
{
	internal enum WORDLIST_TYPE
	{
		WORDLIST_TYPE_IGNORE,
		WORDLIST_TYPE_ADD,
		WORDLIST_TYPE_EXCLUDE,
		WORDLIST_TYPE_AUTOCORRECT
	}

	internal enum CORRECTIVE_ACTION
	{
		CORRECTIVE_ACTION_NONE,
		CORRECTIVE_ACTION_GET_SUGGESTIONS,
		CORRECTIVE_ACTION_REPLACE,
		CORRECTIVE_ACTION_DELETE
	}

	[ComImport]
	[Guid("B7C82D61-FBE8-4B47-9B27-6C0D2E0DE0A3")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISpellingError
	{
		uint StartIndex
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			get;
		}

		uint Length
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			get;
		}

		CORRECTIVE_ACTION CorrectiveAction
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			get;
		}

		string Replacement
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("803E3BD4-2828-4410-8290-418D1D73C762")]
	internal interface IEnumSpellingError
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		ISpellingError Next();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("00000101-0000-0000-C000-000000000046")]
	internal interface IEnumString
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RemoteNext([In] uint celt, [MarshalAs(UnmanagedType.LPWStr)] out string rgelt, out uint pceltFetched);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Skip([In] uint celt);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Reset();

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Clone([MarshalAs(UnmanagedType.Interface)] out IEnumString ppenum);
	}

	[ComImport]
	[Guid("432E5F85-35CF-4606-A801-6F70277E1D7A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IOptionDescription
	{
		string Id
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string Heading
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string Description
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		IEnumString Labels
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}
	}

	[ComImport]
	[Guid("0B83A5B0-792F-4EAB-9799-ACF52C5ED08A")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISpellCheckerChangedEventHandler
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Invoke([In][MarshalAs(UnmanagedType.Interface)] ISpellChecker sender);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("B6FD0B71-E2BC-4653-8D05-F197E412770B")]
	internal interface ISpellChecker
	{
		string languageTag
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumSpellingError Check([In][MarshalAs(UnmanagedType.LPWStr)] string text);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumString Suggest([In][MarshalAs(UnmanagedType.LPWStr)] string word);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Add([In][MarshalAs(UnmanagedType.LPWStr)] string word);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void Ignore([In][MarshalAs(UnmanagedType.LPWStr)] string word);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void AutoCorrect([In][MarshalAs(UnmanagedType.LPWStr)] string from, [In][MarshalAs(UnmanagedType.LPWStr)] string to);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		byte GetOptionValue([In][MarshalAs(UnmanagedType.LPWStr)] string optionId);

		IEnumString OptionIds
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		string Id
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		string LocalizedName
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.LPWStr)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		uint add_SpellCheckerChanged([In][MarshalAs(UnmanagedType.Interface)] ISpellCheckerChangedEventHandler handler);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void remove_SpellCheckerChanged([In] uint eventCookie);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IOptionDescription GetOptionDescription([In][MarshalAs(UnmanagedType.LPWStr)] string optionId);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		IEnumSpellingError ComprehensiveCheck([In][MarshalAs(UnmanagedType.LPWStr)] string text);
	}

	[ComImport]
	[Guid("8E018A9D-2415-4677-BF08-794EA61F94BB")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface ISpellCheckerFactory
	{
		IEnumString SupportedLanguages
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int IsSupported([In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		ISpellChecker CreateSpellChecker([In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);
	}

	[ComImport]
	[Guid("AA176B85-0E12-4844-8E1A-EEF1DA77F586")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	internal interface IUserDictionariesRegistrar
	{
		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void RegisterUserDictionary([In][MarshalAs(UnmanagedType.LPWStr)] string dictionaryPath, [In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		void UnregisterUserDictionary([In][MarshalAs(UnmanagedType.LPWStr)] string dictionaryPath, [In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);
	}

	[ComImport]
	[Guid("7AB36653-1796-484B-BDFA-E74F1DB7C1DC")]
	[TypeLibType(TypeLibTypeFlags.FCanCreate)]
	[ClassInterface(ClassInterfaceType.None)]
	internal class SpellCheckerFactoryCoClass : ISpellCheckerFactory, SpellCheckerFactoryClass, IUserDictionariesRegistrar
	{
		public virtual extern IEnumString SupportedLanguages
		{
			[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
			[return: MarshalAs(UnmanagedType.Interface)]
			get;
		}

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		[return: MarshalAs(UnmanagedType.Interface)]
		public virtual extern ISpellChecker CreateSpellChecker([In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern int IsSupported([In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void RegisterUserDictionary([In][MarshalAs(UnmanagedType.LPWStr)] string dictionaryPath, [In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);

		[MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		public virtual extern void UnregisterUserDictionary([In][MarshalAs(UnmanagedType.LPWStr)] string dictionaryPath, [In][MarshalAs(UnmanagedType.LPWStr)] string languageTag);
	}

	[ComImport]
	[CoClass(typeof(SpellCheckerFactoryCoClass))]
	[Guid("8E018A9D-2415-4677-BF08-794EA61F94BB")]
	internal interface SpellCheckerFactoryClass : ISpellCheckerFactory
	{
	}
}
