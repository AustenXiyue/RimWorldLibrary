using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using MS.Internal;
using MS.Win32;

namespace System.Windows.Documents;

internal class NLGSpellerInterop : SpellerInteropBase
{
	private struct STextRange : ITextRange
	{
		private readonly int _start;

		private readonly int _length;

		public int Start => _start;

		public int Length => _length;
	}

	private enum RangeRole
	{
		ecrrSimpleSegment,
		ecrrAlternativeForm,
		ecrrIncorrect,
		ecrrAutoReplaceForm,
		ecrrCorrectForm,
		ecrrPreferredForm,
		ecrrNormalizedForm,
		ecrrCompoundSegment,
		ecrrPhraseSegment,
		ecrrNamedEntity,
		ecrrCompoundWord,
		ecrrPhrase,
		ecrrUnknownWord,
		ecrrContraction,
		ecrrHyphenatedWord,
		ecrrContractionSegment,
		ecrrHyphenatedSegment,
		ecrrCapitalization,
		ecrrAccent,
		ecrrRepeated,
		ecrrDefinition,
		ecrrOutOfContext
	}

	private class SpellerSegment : ISpellerSegment, IDisposable
	{
		private STextRange? _sTextRange;

		private int _subSegmentCount;

		private IReadOnlyList<ISpellerSegment> _subSegments;

		private IReadOnlyList<string> _suggestions;

		private RangeRole? _rangeRole;

		private ITextSegment _textSegment;

		private bool _disposed;

		public string SourceString { get; }

		public string Text => SourceString?.Substring(TextRange.Start, TextRange.Length);

		public IReadOnlyList<ISpellerSegment> SubSegments
		{
			get
			{
				if (_subSegments == null)
				{
					EnumerateSubSegments();
				}
				return _subSegments;
			}
		}

		public ITextRange TextRange
		{
			get
			{
				if (!_sTextRange.HasValue)
				{
					_textSegment.get_Range(out var val);
					_sTextRange = val;
				}
				return _sTextRange.Value;
			}
		}

		public IReadOnlyList<string> Suggestions
		{
			get
			{
				if (_suggestions == null)
				{
					EnumerateSuggestions();
				}
				return _suggestions;
			}
		}

		public bool IsClean => RangeRole != RangeRole.ecrrIncorrect;

		public RangeRole RangeRole
		{
			get
			{
				if (!_rangeRole.HasValue)
				{
					_textSegment.get_Role(out var val);
					_rangeRole = val;
				}
				return _rangeRole.Value;
			}
		}

		public SpellerSegment(ITextSegment textSegment)
		{
			_textSegment = textSegment;
		}

		private void EnumerateSuggestions()
		{
			List<string> list = new List<string>();
			_textSegment.get_Suggestions(out var val);
			if (val == null)
			{
				_suggestions = list.AsReadOnly();
				return;
			}
			try
			{
				MS.Win32.NativeMethods.VARIANT vARIANT = new MS.Win32.NativeMethods.VARIANT();
				int[] array = new int[1];
				while (true)
				{
					vARIANT.Clear();
					if (EnumVariantNext(val, vARIANT, array) != 0 || array[0] == 0)
					{
						break;
					}
					list.Add(Marshal.PtrToStringUni(vARIANT.data1.Value));
				}
			}
			finally
			{
				Marshal.ReleaseComObject(val);
			}
			_suggestions = list.AsReadOnly();
		}

		private void EnumerateSubSegments()
		{
			_textSegment.get_Count(out _subSegmentCount);
			List<ISpellerSegment> list = new List<ISpellerSegment>();
			for (int i = 0; i < _subSegmentCount; i++)
			{
				_textSegment.get_Item(i, out var val);
				list.Add(new SpellerSegment(val));
			}
			_subSegments = list.AsReadOnly();
		}

		public void EnumSubSegments(EnumTextSegmentsCallback segmentCallback, object data)
		{
			bool flag = true;
			int num = 0;
			while (flag && num < SubSegments.Count)
			{
				flag = segmentCallback(SubSegments[num], data);
				num++;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("NLGSpellerInterop.SpellerSegment");
			}
			if (_subSegments != null)
			{
				foreach (SpellerSegment subSegment in _subSegments)
				{
					subSegment.Dispose();
				}
				_subSegments = null;
			}
			if (_textSegment != null)
			{
				Marshal.ReleaseComObject(_textSegment);
				_textSegment = null;
			}
			_disposed = true;
		}

		~SpellerSegment()
		{
			Dispose(disposing: false);
		}
	}

	private class SpellerSentence : ISpellerSentence, IDisposable
	{
		private IReadOnlyList<ISpellerSegment> _segments;

		private bool _disposed;

		public IReadOnlyList<ISpellerSegment> Segments => _segments;

		public int EndOffset
		{
			get
			{
				int result = -1;
				if (Segments.Count > 0)
				{
					ITextRange textRange = Segments[Segments.Count - 1].TextRange;
					result = textRange.Start + textRange.Length;
				}
				return result;
			}
		}

		public SpellerSentence(ISentence sentence)
		{
			_disposed = false;
			try
			{
				sentence.get_Count(out var val);
				List<ISpellerSegment> list = new List<ISpellerSegment>();
				for (int i = 0; i < val; i++)
				{
					sentence.get_Item(i, out var val2);
					list.Add(new SpellerSegment(val2));
				}
				_segments = list.AsReadOnly();
				Invariant.Assert(_segments.Count == val);
			}
			finally
			{
				Marshal.ReleaseComObject(sentence);
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("NLGSpellerInterop.SpellerSentence");
			}
			if (_segments != null)
			{
				foreach (SpellerSegment segment in _segments)
				{
					segment.Dispose();
				}
				_segments = null;
			}
			_disposed = true;
		}

		~SpellerSentence()
		{
			Dispose(disposing: false);
		}
	}

	private static class UnsafeNlMethods
	{
		[DllImport("PresentationNative_cor3.dll", PreserveSig = false)]
		internal static extern void NlLoad();

		[DllImport("PresentationNative_cor3.dll")]
		internal static extern void NlUnload();

		[DllImport("PresentationNative_cor3.dll", PreserveSig = false)]
		internal static extern void NlGetClassObject(ref Guid clsid, ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object classObject);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("004CD7E2-8B63-4ef9-8D46-080CDBBE47AF")]
	internal interface ILexicon
	{
		void ReadFrom([MarshalAs(UnmanagedType.BStr)] string fileName);

		void stub_WriteTo();

		void stub_GetEnumerator();

		void stub_IndexOf();

		void stub_TagFor();

		void stub_ContainsPrefix();

		void stub_Add();

		void stub_Remove();

		void stub_Version();

		void stub_Count();

		void stub__NewEnum();

		void stub_get_Item();

		void stub_set_Item();

		void stub_get_ItemByName();

		void stub_set_ItemByName();

		void stub_get0_PropertyCount();

		void stub_get1_Property();

		void stub_set_Property();

		void stub_get_IsSealed();

		void stub_get_IsReadOnly();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("B6797CC0-11AE-4047-A438-26C0C916EB8D")]
	private interface ITextContext
	{
		void stub_get_PropertyCount();

		void stub_get_Property();

		void stub_put_Property();

		void stub_get_DefaultDialectCount();

		void stub_get_DefaultDialect();

		void stub_AddDefaultDialect();

		void stub_RemoveDefaultDialect();

		void get_LexiconCount([MarshalAs(UnmanagedType.I4)] out int lexiconCount);

		void get_Lexicon(int index, [MarshalAs(UnmanagedType.Interface)] out ILexicon lexicon);

		void AddLexicon([In][MarshalAs(UnmanagedType.Interface)] ILexicon lexicon);

		void RemoveLexicon([In][MarshalAs(UnmanagedType.Interface)] ILexicon lexicon);

		void stub_get_Version();

		void stub_get_ResourceLoader();

		void stub_put_ResourceLoader();

		void get_Options([MarshalAs(UnmanagedType.Interface)] out IProcessingOptions val);

		void get_Capabilities(int locale, [MarshalAs(UnmanagedType.Interface)] out IProcessingOptions val);

		void stub_get_Lexicons();

		void stub_put_Lexicons();

		void stub_get_MaxSentences();

		void stub_put_MaxSentences();

		void stub_get_IsSingleLanguage();

		void stub_put_IsSingleLanguage();

		void stub_get_IsSimpleWordBreaking();

		void stub_put_IsSimpleWordBreaking();

		void stub_get_UseRelativeTimes();

		void stub_put_UseRelativeTimes();

		void stub_get_IgnorePunctuation();

		void stub_put_IgnorePunctuation();

		void stub_get_IsCaching();

		void stub_put_IsCaching();

		void stub_get_IsShowingGaps();

		void stub_put_IsShowingGaps();

		void stub_get_IsShowingCharacterNormalizations();

		void stub_put_IsShowingCharacterNormalizations();

		void stub_get_IsShowingWordNormalizations();

		void stub_put_IsShowingWordNormalizations();

		void stub_get_IsComputingCompounds();

		void stub_put_IsComputingCompounds();

		void stub_get_IsComputingInflections();

		void stub_put_IsComputingInflections();

		void stub_get_IsComputingLemmas();

		void stub_put_IsComputingLemmas();

		void stub_get_IsComputingExpansions();

		void stub_put_IsComputingExpansions();

		void stub_get_IsComputingBases();

		void stub_put_IsComputingBases();

		void stub_get_IsComputingPartOfSpeechTags();

		void stub_put_IsComputingPartOfSpeechTags();

		void stub_get_IsFindingDefinitions();

		void stub_put_IsFindingDefinitions();

		void stub_get_IsFindingDateTimeMeasures();

		void stub_put_IsFindingDateTimeMeasures();

		void stub_get_IsFindingPersons();

		void stub_put_IsFindingPersons();

		void stub_get_IsFindingLocations();

		void stub_put_IsFindingLocations();

		void stub_get_IsFindingOrganizations();

		void stub_put_IsFindingOrganizations();

		void stub_get_IsFindingPhrases();

		void stub_put_IsFindingPhrases();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("549F997E-0EC3-43d4-B443-2BF8021010CF")]
	private interface ITextChunk
	{
		void stub_get_InputText();

		void stub_put_InputText();

		void SetInputArray([In] nint inputArray, int size);

		void stub_RegisterEngine();

		void stub_UnregisterEngine();

		void stub_get_InputArray();

		void stub_get_InputArrayRange();

		void stub_put_InputArrayRange();

		void get_Count(out int val);

		void get_Item(int index, [MarshalAs(UnmanagedType.Interface)] out ISentence val);

		void stub_get__NewEnum();

		void get_Sentences([MarshalAs(UnmanagedType.Interface)] out MS.Win32.UnsafeNativeMethods.IEnumVariant val);

		void stub_get_PropertyCount();

		void stub_get_Property();

		void stub_put_Property();

		void get_Context([MarshalAs(UnmanagedType.Interface)] out ITextContext val);

		void put_Context([MarshalAs(UnmanagedType.Interface)] ITextContext val);

		void stub_get_Locale();

		void put_Locale(int val);

		void stub_get_IsLocaleReliable();

		void stub_put_IsLocaleReliable();

		void stub_get_IsEndOfDocument();

		void stub_put_IsEndOfDocument();

		void GetEnumerator([MarshalAs(UnmanagedType.Interface)] out MS.Win32.UnsafeNativeMethods.IEnumVariant val);

		void stub_ToString();

		void stub_ProcessStream();

		void get_ReuseObjects(out bool val);

		void put_ReuseObjects(bool val);
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("F0C13A7A-199B-44be-8492-F91EAA50F943")]
	private interface ISentence
	{
		void stub_get_PropertyCount();

		void stub_get_Property();

		void stub_put_Property();

		void get_Count(out int val);

		void stub_get_Parent();

		void get_Item(int index, [MarshalAs(UnmanagedType.Interface)] out ITextSegment val);

		void stub_get__NewEnum();

		void stub_get_Segments();

		void stub_GetEnumerator();

		void stub_get_IsEndOfParagraph();

		void stub_get_IsUnfinished();

		void stub_get_IsUnfinishedAtEnd();

		void stub_get_Locale();

		void stub_get_IsLocaleReliable();

		void stub_get_Range();

		void stub_get_RequiresNormalization();

		void stub_ToString();

		void stub_CopyToString();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("AF4656B8-5E5E-4fb2-A2D8-1E977E549A56")]
	private interface ITextSegment
	{
		void stub_get_IsSurfaceString();

		void get_Range([MarshalAs(UnmanagedType.Struct)] out STextRange val);

		void stub_get_Identifier();

		void stub_get_Unit();

		void get_Count(out int val);

		void get_Item(int index, [MarshalAs(UnmanagedType.Interface)] out ITextSegment val);

		void stub_get_Expansions();

		void stub_get_Bases();

		void stub_get_SuggestionScores();

		void stub_get_PropertyCount();

		void stub_get_Property();

		void stub_put_Property();

		void stub_CopyToString();

		void get_Role(out RangeRole val);

		void stub_get_PrimaryType();

		void stub_get_SecondaryType();

		void stub_get_SpellingVariations();

		void stub_get_CharacterNormalizations();

		void stub_get_Representations();

		void stub_get_Inflections();

		void get_Suggestions([MarshalAs(UnmanagedType.Interface)] out MS.Win32.UnsafeNativeMethods.IEnumVariant val);

		void stub_get_Lemmas();

		void stub_get_SubSegments();

		void stub_get_Alternatives();

		void stub_ToString();

		void stub_get_IsPossiblePhraseStart();

		void stub_get_SpellingScore();

		void stub_get_IsPunctuation();

		void stub_get_IsEndPunctuation();

		void stub_get_IsSpace();

		void stub_get_IsAbbreviation();

		void stub_get_IsSmiley();
	}

	[ComImport]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	[Guid("C090356B-A6A5-442a-A204-CFD5415B5902")]
	private interface IProcessingOptions
	{
		void stub_get__NewEnum();

		void stub_GetEnumerator();

		void stub_get_Locale();

		void stub_get_Count();

		void stub_get_Name();

		void stub_get_Item();

		void put_Item(object index, object val);

		void stub_get_IsReadOnly();
	}

	private ITextChunk _textChunk;

	private bool _isDisposed;

	private SpellerMode _mode;

	private bool _multiWordMode;

	private static readonly Guid CLSID_ITextContext = new Guid(859728164, 17235, 18740, 167, 190, 95, 181, 189, 221, 178, 214);

	private static readonly Guid IID_ITextContext = new Guid(3061415104u, 4526, 16455, 164, 56, 38, 192, 201, 22, 235, 141);

	private static readonly Guid CLSID_ITextChunk = new Guid(2313837402u, 53276, 17760, 168, 116, 159, 201, 42, 251, 14, 250);

	private static readonly Guid IID_ITextChunk = new Guid(1419745662, 3779, 17364, 180, 67, 43, 248, 2, 16, 16, 207);

	private static readonly Guid CLSID_Lexicon = new Guid("D385FDAD-D394-4812-9CEC-C6575C0B2B38");

	private static readonly Guid IID_ILexicon = new Guid("004CD7E2-8B63-4ef9-8D46-080CDBBE47AF");

	internal override SpellerMode Mode
	{
		set
		{
			_mode = value;
			if (_mode.HasFlag(SpellerMode.SpellingErrors))
			{
				SetContextOption("IsSpellChecking", true);
				if (_mode.HasFlag(SpellerMode.Suggestions))
				{
					SetContextOption("IsSpellVerifyOnly", false);
				}
				else
				{
					SetContextOption("IsSpellVerifyOnly", true);
				}
			}
			else if (_mode.HasFlag(SpellerMode.WordBreaking))
			{
				SetContextOption("IsSpellChecking", false);
			}
		}
	}

	internal override bool MultiWordMode
	{
		set
		{
			_multiWordMode = value;
			SetContextOption("IsSpellSuggestingMWEs", _multiWordMode);
		}
	}

	internal NLGSpellerInterop()
	{
		UnsafeNlMethods.NlLoad();
		bool flag = true;
		try
		{
			_textChunk = CreateTextChunk();
			ITextContext textContext = CreateTextContext();
			try
			{
				_textChunk.put_Context(textContext);
			}
			finally
			{
				Marshal.ReleaseComObject(textContext);
			}
			_textChunk.put_ReuseObjects(val: true);
			Mode = SpellerMode.None;
			MultiWordMode = false;
			flag = false;
		}
		finally
		{
			if (flag)
			{
				if (_textChunk != null)
				{
					Marshal.ReleaseComObject(_textChunk);
					_textChunk = null;
				}
				UnsafeNlMethods.NlUnload();
			}
		}
	}

	~NLGSpellerInterop()
	{
		Dispose(disposing: false);
	}

	public override void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected override void Dispose(bool disposing)
	{
		if (_isDisposed)
		{
			throw new ObjectDisposedException(SR.TextEditorSpellerInteropHasBeenDisposed);
		}
		if (_textChunk != null)
		{
			Marshal.ReleaseComObject(_textChunk);
			_textChunk = null;
		}
		UnsafeNlMethods.NlUnload();
		_isDisposed = true;
	}

	internal override void SetLocale(CultureInfo culture)
	{
		_textChunk.put_Locale(culture.LCID);
	}

	private void SetContextOption(string option, object value)
	{
		_textChunk.get_Context(out var val);
		if (val == null)
		{
			return;
		}
		try
		{
			val.get_Options(out var val2);
			if (val2 != null)
			{
				try
				{
					val2.put_Item(option, value);
					return;
				}
				finally
				{
					Marshal.ReleaseComObject(val2);
				}
			}
		}
		finally
		{
			Marshal.ReleaseComObject(val);
		}
	}

	internal override int EnumTextSegments(char[] text, int count, EnumSentencesCallback sentenceCallback, EnumTextSegmentsCallback segmentCallback, object data)
	{
		int num = 0;
		nint num2 = Marshal.AllocHGlobal(count * 2);
		try
		{
			Marshal.Copy(text, 0, num2, count);
			_textChunk.SetInputArray(num2, count);
			_textChunk.GetEnumerator(out var val);
			try
			{
				MS.Win32.NativeMethods.VARIANT vARIANT = new MS.Win32.NativeMethods.VARIANT();
				int[] array = new int[1];
				bool flag = true;
				val.Reset();
				do
				{
					vARIANT.Clear();
					if (EnumVariantNext(val, vARIANT, array) != 0 || array[0] == 0)
					{
						break;
					}
					using SpellerSentence spellerSentence = new SpellerSentence((ISentence)vARIANT.ToObject());
					num += spellerSentence.Segments.Count;
					if (segmentCallback != null)
					{
						int num3 = 0;
						while (flag && num3 < spellerSentence.Segments.Count)
						{
							flag = segmentCallback(spellerSentence.Segments[num3], data);
							num3++;
						}
					}
					if (sentenceCallback != null)
					{
						flag = sentenceCallback(spellerSentence, data);
					}
				}
				while (flag);
				vARIANT.Clear();
				return num;
			}
			finally
			{
				Marshal.ReleaseComObject(val);
			}
		}
		finally
		{
			Marshal.FreeHGlobal(num2);
		}
	}

	internal override void UnloadDictionary(object dictionary)
	{
		ILexicon lexicon = dictionary as ILexicon;
		Invariant.Assert(lexicon != null);
		ITextContext val = null;
		try
		{
			_textChunk.get_Context(out val);
			val.RemoveLexicon(lexicon);
		}
		finally
		{
			Marshal.ReleaseComObject(lexicon);
			if (val != null)
			{
				Marshal.ReleaseComObject(val);
			}
		}
	}

	internal override object LoadDictionary(string lexiconFilePath)
	{
		return AddLexicon(lexiconFilePath);
	}

	internal override object LoadDictionary(Uri item, string trustedFolder)
	{
		return LoadDictionary(item.LocalPath);
	}

	internal override void ReleaseAllLexicons()
	{
		ITextContext val = null;
		try
		{
			_textChunk.get_Context(out val);
			int lexiconCount = 0;
			val.get_LexiconCount(out lexiconCount);
			while (lexiconCount > 0)
			{
				ILexicon lexicon = null;
				val.get_Lexicon(0, out lexicon);
				val.RemoveLexicon(lexicon);
				Marshal.ReleaseComObject(lexicon);
				lexiconCount--;
			}
		}
		finally
		{
			if (val != null)
			{
				Marshal.ReleaseComObject(val);
			}
		}
	}

	internal override void SetReformMode(CultureInfo culture, SpellingReform spellingReform)
	{
		string twoLetterISOLanguageName = culture.TwoLetterISOLanguageName;
		string text = ((twoLetterISOLanguageName == "de") ? "GermanReform" : ((!(twoLetterISOLanguageName == "fr")) ? null : "FrenchReform"));
		if (text == null)
		{
			return;
		}
		switch (spellingReform)
		{
		case SpellingReform.Prereform:
			SetContextOption(text, 1);
			break;
		case SpellingReform.Postreform:
			SetContextOption(text, 2);
			break;
		case SpellingReform.PreAndPostreform:
			if (text == "GermanReform")
			{
				SetContextOption(text, 2);
			}
			else
			{
				SetContextOption(text, 0);
			}
			break;
		}
	}

	internal override bool CanSpellCheck(CultureInfo culture)
	{
		switch (culture.TwoLetterISOLanguageName)
		{
		case "en":
		case "de":
		case "fr":
		case "es":
			return true;
		default:
			return false;
		}
	}

	private ILexicon AddLexicon(string lexiconFilePath)
	{
		ITextContext val = null;
		ILexicon lexicon = null;
		bool flag = true;
		bool flag2 = false;
		try
		{
			flag2 = true;
			lexicon = CreateLexicon();
			lexicon.ReadFrom(lexiconFilePath);
			_textChunk.get_Context(out val);
			val.AddLexicon(lexicon);
			flag = false;
		}
		catch (Exception innerException)
		{
			if (flag2)
			{
				throw new ArgumentException(SR.Format(SR.CustomDictionaryFailedToLoadDictionaryUri, lexiconFilePath), innerException);
			}
			throw;
		}
		finally
		{
			if (flag && lexicon != null)
			{
				Marshal.ReleaseComObject(lexicon);
			}
			if (val != null)
			{
				Marshal.ReleaseComObject(val);
			}
		}
		return lexicon;
	}

	private static object CreateInstance(Guid clsid, Guid iid)
	{
		UnsafeNlMethods.NlGetClassObject(ref clsid, ref iid, out var classObject);
		return classObject;
	}

	private static ITextContext CreateTextContext()
	{
		return (ITextContext)CreateInstance(CLSID_ITextContext, IID_ITextContext);
	}

	private static ITextChunk CreateTextChunk()
	{
		return (ITextChunk)CreateInstance(CLSID_ITextChunk, IID_ITextChunk);
	}

	private static ILexicon CreateLexicon()
	{
		return (ILexicon)CreateInstance(CLSID_Lexicon, IID_ILexicon);
	}

	private unsafe static int EnumVariantNext(MS.Win32.UnsafeNativeMethods.IEnumVariant variantEnumerator, MS.Win32.NativeMethods.VARIANT variant, int[] fetched)
	{
		int result;
		fixed (short* vt = &variant.vt)
		{
			void* rgvar = vt;
			result = variantEnumerator.Next(1, (nint)rgvar, fetched);
		}
		return result;
	}
}
