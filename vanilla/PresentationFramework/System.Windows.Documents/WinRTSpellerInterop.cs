using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Documents.MsSpellCheckLib;
using System.Windows.Documents.Tracing;
using System.Windows.Input;
using System.Windows.Threading;
using MS.Internal.WindowsRuntime.Windows.Data.Text;

namespace System.Windows.Documents;

internal class WinRTSpellerInterop : SpellerInteropBase
{
	internal readonly struct TextRange : ITextRange
	{
		private readonly int _start;

		private readonly int _length;

		public int Start => _start;

		public int Length => _length;

		public TextRange(MS.Internal.WindowsRuntime.Windows.Data.Text.TextSegment textSegment)
		{
			_length = (int)textSegment.Length;
			_start = (int)textSegment.StartPosition;
		}

		public TextRange(int start, int length)
		{
			_start = start;
			_length = length;
		}

		public TextRange(ITextRange textRange)
			: this(textRange.Start, textRange.Length)
		{
		}

		public static explicit operator TextRange(MS.Internal.WindowsRuntime.Windows.Data.Text.TextSegment textSegment)
		{
			return new TextRange(textSegment);
		}
	}

	[DebuggerDisplay("SubSegments.Count = {SubSegments.Count} TextRange = {TextRange.Start},{TextRange.Length}")]
	internal class SpellerSegment : ISpellerSegment
	{
		private SpellChecker _spellChecker;

		private IReadOnlyList<string> _suggestions;

		private bool? _isClean;

		private static readonly IReadOnlyList<ISpellerSegment> _empty;

		public string SourceString { get; }

		public string Text => SourceString?.Substring(TextRange.Start, TextRange.Length);

		public IReadOnlyList<ISpellerSegment> SubSegments => _empty;

		public ITextRange TextRange { get; }

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

		public bool IsClean
		{
			get
			{
				if (!_isClean.HasValue)
				{
					EnumerateSuggestions();
				}
				return _isClean.Value;
			}
		}

		internal WinRTSpellerInterop Owner { get; }

		public SpellerSegment(string sourceString, ITextRange textRange, SpellChecker spellChecker, WinRTSpellerInterop owner)
		{
			_spellChecker = spellChecker;
			_suggestions = null;
			Owner = owner;
			SourceString = sourceString;
			TextRange = textRange;
		}

		static SpellerSegment()
		{
			_empty = new List<ISpellerSegment>().AsReadOnly();
		}

		private void EnumerateSuggestions()
		{
			List<string> list = new List<string>();
			_isClean = true;
			if (_spellChecker == null)
			{
				_suggestions = list.AsReadOnly();
				return;
			}
			List<SpellChecker.SpellingError> list2 = null;
			using (new SpellerCOMActionTraceLogger(Owner, SpellerCOMActionTraceLogger.Actions.ComprehensiveCheck))
			{
				list2 = ((Text != null) ? _spellChecker.ComprehensiveCheck(Text) : null);
			}
			if (list2 == null)
			{
				_suggestions = list.AsReadOnly();
				return;
			}
			foreach (SpellChecker.SpellingError item in list2)
			{
				list.AddRange(item.Suggestions);
				if (item.CorrectiveAction != 0)
				{
					_isClean = false;
				}
			}
			_suggestions = list.AsReadOnly();
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
	}

	[DebuggerDisplay("Sentence = {_sentence}")]
	private class SpellerSentence : ISpellerSentence
	{
		private string _sentence;

		private WordsSegmenter _wordBreaker;

		private SpellChecker _spellChecker;

		private IReadOnlyList<SpellerSegment> _segments;

		private WinRTSpellerInterop _owner;

		public IReadOnlyList<ISpellerSegment> Segments
		{
			get
			{
				if (_segments == null)
				{
					_segments = _wordBreaker.ComprehensiveGetTokens(_sentence, _spellChecker, _owner);
				}
				return _segments;
			}
		}

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

		public SpellerSentence(string sentence, WordsSegmenter wordBreaker, SpellChecker spellChecker, WinRTSpellerInterop owner)
		{
			_sentence = sentence;
			_wordBreaker = wordBreaker;
			_spellChecker = spellChecker;
			_segments = null;
			_owner = owner;
		}
	}

	private bool _isDisposed;

	private SpellerMode _mode;

	private Dictionary<CultureInfo, Tuple<WordsSegmenter, SpellChecker>> _spellCheckers;

	private CultureInfo _defaultCulture;

	private CultureInfo _culture;

	private Dictionary<string, List<string>> _customDictionaryFiles;

	private readonly WeakReference<Dispatcher> _dispatcher;

	internal override SpellerMode Mode
	{
		set
		{
			_mode = value;
		}
	}

	internal override bool MultiWordMode
	{
		set
		{
		}
	}

	private CultureInfo Culture
	{
		get
		{
			return _culture;
		}
		set
		{
			_culture = value;
			EnsureWordBreakerAndSpellCheckerForCulture(_culture);
		}
	}

	private WordsSegmenter CurrentWordBreaker
	{
		get
		{
			if (Culture == null)
			{
				return null;
			}
			EnsureWordBreakerAndSpellCheckerForCulture(Culture);
			return _spellCheckers[Culture]?.Item1;
		}
	}

	private WordsSegmenter DefaultCultureWordBreaker
	{
		get
		{
			if (_defaultCulture == null)
			{
				return null;
			}
			return _spellCheckers[_defaultCulture]?.Item1;
		}
	}

	private SpellChecker CurrentSpellChecker
	{
		get
		{
			if (Culture == null)
			{
				return null;
			}
			EnsureWordBreakerAndSpellCheckerForCulture(Culture);
			return _spellCheckers[Culture]?.Item2;
		}
	}

	internal WinRTSpellerInterop()
	{
		try
		{
			SpellCheckerFactory.Create();
		}
		catch (Exception ex) when (ex is InvalidCastException || ex is COMException)
		{
			Dispose();
			throw new PlatformNotSupportedException(string.Empty, ex);
		}
		_spellCheckers = new Dictionary<CultureInfo, Tuple<WordsSegmenter, SpellChecker>>();
		_customDictionaryFiles = new Dictionary<string, List<string>>();
		_defaultCulture = InputLanguageManager.Current?.CurrentInputLanguage ?? Thread.CurrentThread.CurrentCulture;
		_culture = null;
		try
		{
			EnsureWordBreakerAndSpellCheckerForCulture(_defaultCulture, throwOnError: true);
		}
		catch (Exception ex2) when (ex2 is ArgumentException || ex2 is NotSupportedException || ex2 is PlatformNotSupportedException)
		{
			_spellCheckers = null;
			Dispose();
			if (ex2 is PlatformNotSupportedException || ex2 is NotSupportedException)
			{
				throw;
			}
			throw new NotSupportedException(string.Empty, ex2);
		}
		_dispatcher = new WeakReference<Dispatcher>(Dispatcher.CurrentDispatcher);
		WeakEventManager<AppDomain, UnhandledExceptionEventArgs>.AddHandler(AppDomain.CurrentDomain, "UnhandledException", ProcessUnhandledException);
	}

	~WinRTSpellerInterop()
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
		try
		{
			if (BeginInvokeOnUIThread(new Action<bool>(Dispose), DispatcherPriority.Normal, disposing) == null)
			{
				ReleaseAllResources(disposing);
				_isDisposed = true;
			}
		}
		catch (InvalidOperationException)
		{
		}
	}

	internal override void SetLocale(CultureInfo culture)
	{
		Culture = culture;
	}

	internal override void SetReformMode(CultureInfo culture, SpellingReform spellingReform)
	{
	}

	internal override bool CanSpellCheck(CultureInfo culture)
	{
		if (!_isDisposed)
		{
			return EnsureWordBreakerAndSpellCheckerForCulture(culture);
		}
		return false;
	}

	internal override void UnloadDictionary(object token)
	{
		if (!_isDisposed)
		{
			Tuple<string, string> obj = (Tuple<string, string>)token;
			string item = obj.Item1;
			string item2 = obj.Item2;
			using (new SpellerCOMActionTraceLogger(this, SpellerCOMActionTraceLogger.Actions.UnregisterUserDictionary))
			{
				SpellCheckerFactory.UnregisterUserDictionary(item2, item);
			}
			FileHelper.DeleteTemporaryFile(item2);
		}
	}

	internal override object LoadDictionary(string lexiconFilePath)
	{
		if (!_isDisposed)
		{
			return LoadDictionaryImpl(lexiconFilePath);
		}
		return null;
	}

	internal override object LoadDictionary(Uri item, string trustedFolder)
	{
		if (_isDisposed)
		{
			return null;
		}
		return LoadDictionaryImpl(item.LocalPath);
	}

	internal override void ReleaseAllLexicons()
	{
		if (!_isDisposed)
		{
			ClearDictionaries();
		}
	}

	private bool EnsureWordBreakerAndSpellCheckerForCulture(CultureInfo culture, bool throwOnError = false)
	{
		if (_isDisposed || culture == null)
		{
			return false;
		}
		if (!_spellCheckers.ContainsKey(culture))
		{
			WordsSegmenter wordsSegmenter = null;
			try
			{
				wordsSegmenter = WordsSegmenter.Create(culture.Name, shouldPreferNeutralSegmenter: true);
			}
			catch when (!throwOnError)
			{
				wordsSegmenter = null;
			}
			if (wordsSegmenter == null)
			{
				_spellCheckers[culture] = null;
				return false;
			}
			SpellChecker spellChecker = null;
			try
			{
				using (new SpellerCOMActionTraceLogger(this, SpellerCOMActionTraceLogger.Actions.SpellCheckerCreation))
				{
					spellChecker = new SpellChecker(culture.Name);
				}
			}
			catch (Exception ex)
			{
				spellChecker = null;
				if (throwOnError && ex is ArgumentException)
				{
					throw new NotSupportedException(string.Empty, ex);
				}
			}
			if (spellChecker == null)
			{
				_spellCheckers[culture] = null;
			}
			else
			{
				_spellCheckers[culture] = new Tuple<WordsSegmenter, SpellChecker>(wordsSegmenter, spellChecker);
			}
		}
		return _spellCheckers[culture] != null;
	}

	internal override int EnumTextSegments(char[] text, int count, EnumSentencesCallback sentenceCallback, EnumTextSegmentsCallback segmentCallback, object data)
	{
		if (_isDisposed)
		{
			return 0;
		}
		WordsSegmenter wordsSegmenter = CurrentWordBreaker ?? DefaultCultureWordBreaker;
		SpellChecker currentSpellChecker = CurrentSpellChecker;
		bool flag = _mode.HasFlag(SpellerMode.SpellingErrors) || _mode.HasFlag(SpellerMode.Suggestions);
		if (wordsSegmenter == null || (flag && currentSpellChecker == null))
		{
			return 0;
		}
		int num = 0;
		bool flag2 = true;
		string[] array = new string[1] { string.Join(string.Empty, text) };
		for (int i = 0; i < array.Length; i++)
		{
			SpellerSentence spellerSentence = new SpellerSentence(array[i], wordsSegmenter, CurrentSpellChecker, this);
			num += spellerSentence.Segments.Count;
			if (segmentCallback != null)
			{
				int num2 = 0;
				while (flag2 && num2 < spellerSentence.Segments.Count)
				{
					flag2 = segmentCallback(spellerSentence.Segments[num2], data);
					num2++;
				}
			}
			if (sentenceCallback != null)
			{
				flag2 = sentenceCallback(spellerSentence, data);
			}
			if (!flag2)
			{
				break;
			}
		}
		return num;
	}

	private Tuple<string, string> LoadDictionaryImpl(string lexiconFilePath)
	{
		if (_isDisposed)
		{
			return new Tuple<string, string>(null, null);
		}
		if (!File.Exists(lexiconFilePath))
		{
			throw new ArgumentException(SR.Format(SR.CustomDictionaryFailedToLoadDictionaryUri, lexiconFilePath));
		}
		bool flag = false;
		string filePath = null;
		try
		{
			CultureInfo cultureInfo = null;
			using (FileStream stream = new FileStream(lexiconFilePath, FileMode.Open, FileAccess.Read))
			{
				using StreamReader streamReader = new StreamReader(stream);
				cultureInfo = TryParseLexiconCulture(streamReader.ReadLine());
			}
			string ietfLanguageTag = cultureInfo.IetfLanguageTag;
			using (FileStream targetStream = FileHelper.CreateAndOpenTemporaryFile(out filePath, FileAccess.Write, FileOptions.None, "dic"))
			{
				CopyToUnicodeFile(lexiconFilePath, targetStream);
				flag = true;
			}
			if (!_customDictionaryFiles.ContainsKey(ietfLanguageTag))
			{
				_customDictionaryFiles[ietfLanguageTag] = new List<string>();
			}
			_customDictionaryFiles[ietfLanguageTag].Add(filePath);
			using (new SpellerCOMActionTraceLogger(this, SpellerCOMActionTraceLogger.Actions.RegisterUserDictionary))
			{
				SpellCheckerFactory.RegisterUserDictionary(filePath, ietfLanguageTag);
			}
			return new Tuple<string, string>(ietfLanguageTag, filePath);
		}
		catch (Exception ex) when (ex is ArgumentException || !flag)
		{
			if (filePath != null)
			{
				FileHelper.DeleteTemporaryFile(filePath);
			}
			throw new ArgumentException(SR.Format(SR.CustomDictionaryFailedToLoadDictionaryUri, lexiconFilePath), ex);
		}
	}

	private void ClearDictionaries(bool disposing = false)
	{
		if (_isDisposed)
		{
			return;
		}
		if (_customDictionaryFiles != null)
		{
			foreach (KeyValuePair<string, List<string>> customDictionaryFile in _customDictionaryFiles)
			{
				string key = customDictionaryFile.Key;
				foreach (string item in customDictionaryFile.Value)
				{
					try
					{
						using (new SpellerCOMActionTraceLogger(this, SpellerCOMActionTraceLogger.Actions.UnregisterUserDictionary))
						{
							SpellCheckerFactory.UnregisterUserDictionary(item, key);
						}
						FileHelper.DeleteTemporaryFile(item);
					}
					catch
					{
					}
				}
			}
			_customDictionaryFiles.Clear();
		}
		if (disposing)
		{
			_customDictionaryFiles = null;
		}
	}

	private static CultureInfo TryParseLexiconCulture(string line)
	{
		RegexOptions options = RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;
		CultureInfo invariantCulture = CultureInfo.InvariantCulture;
		if (line == null)
		{
			return invariantCulture;
		}
		string[] array = Regex.Split(line.Trim(), "\\s*\\#LID\\s+(\\d+)\\s*", options);
		if (array.Length != 3)
		{
			return invariantCulture;
		}
		string text = array[0];
		string s = array[1];
		string text2 = array[2];
		if (text != string.Empty || text2 != string.Empty || !int.TryParse(s, out var result))
		{
			return invariantCulture;
		}
		try
		{
			return new CultureInfo(result);
		}
		catch (CultureNotFoundException)
		{
			return CultureInfo.InvariantCulture;
		}
	}

	private static void CopyToUnicodeFile(string sourcePath, FileStream targetStream)
	{
		using FileStream fileStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
		bool num = fileStream.ReadByte() == 255 && fileStream.ReadByte() == 254;
		fileStream.Seek(0L, SeekOrigin.Begin);
		if (num)
		{
			fileStream.CopyTo(targetStream);
			return;
		}
		using StreamReader streamReader = new StreamReader(fileStream);
		using StreamWriter streamWriter = new StreamWriter(targetStream, Encoding.Unicode);
		string text = null;
		while ((text = streamReader.ReadLine()) != null)
		{
			streamWriter.WriteLine(text);
		}
	}

	private void ProcessUnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		bool flag = false;
		try
		{
			if (BeginInvokeOnUIThread(new Action<bool>(ClearDictionaries), DispatcherPriority.Normal, flag) == null)
			{
				ClearDictionaries(flag);
			}
		}
		catch (InvalidOperationException)
		{
		}
	}

	private void ReleaseAllResources(bool disposing)
	{
		if (_spellCheckers != null)
		{
			foreach (Tuple<WordsSegmenter, SpellChecker> value in _spellCheckers.Values)
			{
				(value?.Item2)?.Dispose();
			}
			_spellCheckers = null;
		}
		ClearDictionaries(disposing);
	}

	private DispatcherOperation BeginInvokeOnUIThread(Delegate method, DispatcherPriority priority, params object[] args)
	{
		Dispatcher target = null;
		if (_dispatcher == null || !_dispatcher.TryGetTarget(out target) || target == null)
		{
			throw new InvalidOperationException();
		}
		if (!target.CheckAccess())
		{
			return target.BeginInvoke(method, priority, args);
		}
		return null;
	}
}
