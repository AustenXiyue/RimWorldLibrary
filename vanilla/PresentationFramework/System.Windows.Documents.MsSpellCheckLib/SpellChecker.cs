using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Documents.MsSpellCheckLib;

internal class SpellChecker : IDisposable
{
	internal class OptionDescription
	{
		private List<string> _labels;

		internal string Id { get; private set; }

		internal string Heading { get; private set; }

		internal string Description { get; private set; }

		internal IReadOnlyCollection<string> Labels => _labels.AsReadOnly();

		private OptionDescription(string id, string heading, string description, List<string> labels = null)
		{
			Id = id;
			Heading = heading;
			Description = description;
			_labels = labels ?? new List<string>();
		}

		internal static OptionDescription Create(RCW.IOptionDescription optionDescription, bool shouldSuppressCOMExceptions = true, bool shouldReleaseCOMObject = true)
		{
			if (optionDescription == null)
			{
				throw new ArgumentNullException("optionDescription");
			}
			OptionDescription optionDescription2 = new OptionDescription(optionDescription.Id, optionDescription.Heading, optionDescription.Description);
			try
			{
				optionDescription2._labels = optionDescription.Labels.ToList();
			}
			catch (COMException) when (shouldSuppressCOMExceptions)
			{
			}
			finally
			{
				if (shouldReleaseCOMObject)
				{
					Marshal.ReleaseComObject(optionDescription);
				}
			}
			return optionDescription2;
		}
	}

	private class HasErrorsResult : Tuple<string, bool>
	{
		public string Text => base.Item1;

		public bool HasErrors => base.Item2;

		public HasErrorsResult(string text, bool hasErrors)
			: base(text, hasErrors)
		{
		}
	}

	internal class SpellCheckerChangedEventArgs : EventArgs
	{
		internal SpellChecker SpellChecker { get; private set; }

		internal SpellCheckerChangedEventArgs(SpellChecker spellChecker)
		{
			SpellChecker = spellChecker;
		}
	}

	private class SpellCheckerChangedEventHandler : RCW.ISpellCheckerChangedEventHandler
	{
		private SpellCheckerChangedEventArgs _eventArgs;

		private SpellChecker _spellChecker;

		internal SpellCheckerChangedEventHandler(SpellChecker spellChecker)
		{
			_spellChecker = spellChecker;
			_eventArgs = new SpellCheckerChangedEventArgs(_spellChecker);
		}

		public void Invoke(RCW.ISpellChecker sender)
		{
			if (sender == _spellChecker?._speller?.Value)
			{
				_spellChecker?.OnChanged(_eventArgs);
			}
		}
	}

	internal enum CorrectiveAction
	{
		None,
		GetSuggestions,
		Replace,
		Delete
	}

	internal class SpellingError
	{
		private List<string> _suggestions;

		internal uint StartIndex { get; }

		internal uint Length { get; }

		internal CorrectiveAction CorrectiveAction { get; }

		internal string Replacement { get; }

		internal IReadOnlyCollection<string> Suggestions => _suggestions.AsReadOnly();

		internal SpellingError(RCW.ISpellingError error, SpellChecker spellChecker, string text, bool shouldSuppressCOMExceptions = true, bool shouldReleaseCOMObject = true)
		{
			if (error == null)
			{
				throw new ArgumentNullException("error");
			}
			StartIndex = error.StartIndex;
			Length = error.Length;
			CorrectiveAction = (CorrectiveAction)error.CorrectiveAction;
			Replacement = error.Replacement;
			PopulateSuggestions(error, spellChecker, text, shouldSuppressCOMExceptions, shouldReleaseCOMObject);
		}

		private void PopulateSuggestions(RCW.ISpellingError error, SpellChecker spellChecker, string text, bool shouldSuppressCOMExceptions, bool shouldReleaseCOMObject)
		{
			try
			{
				_suggestions = new List<string>();
				if (CorrectiveAction == CorrectiveAction.GetSuggestions)
				{
					List<string> collection = spellChecker.Suggest(text, shouldSuppressCOMExceptions);
					_suggestions.AddRange(collection);
				}
				else if (CorrectiveAction == CorrectiveAction.Replace)
				{
					_suggestions.Add(Replacement);
				}
			}
			finally
			{
				if (shouldReleaseCOMObject)
				{
					Marshal.ReleaseComObject(error);
				}
			}
		}
	}

	private static readonly Dictionary<bool, List<Type>> SuppressedExceptions = new Dictionary<bool, List<Type>>
	{
		{
			false,
			new List<Type>()
		},
		{
			true,
			new List<Type>
			{
				typeof(COMException),
				typeof(UnauthorizedAccessException)
			}
		}
	};

	private ChangeNotifyWrapper<RCW.ISpellChecker> _speller;

	private string _languageTag;

	private SpellCheckerChangedEventHandler _spellCheckerChangedEventHandler;

	private uint? _eventCookie;

	private List<HasErrorsResult> _hasErrorsCache;

	private const int HasErrorsCacheCapacity = 10;

	private bool _disposed;

	public event EventHandler<SpellCheckerChangedEventArgs> Changed
	{
		add
		{
			lock (this._changed)
			{
				if (this._changed == null)
				{
					_eventCookie = add_SpellCheckerChanged(_spellCheckerChangedEventHandler);
				}
				_changed += value;
			}
		}
		remove
		{
			lock (this._changed)
			{
				_changed -= value;
				if (this._changed == null && _eventCookie.HasValue)
				{
					remove_SpellCheckerChanged(_eventCookie.Value);
					_eventCookie = null;
				}
			}
		}
	}

	private event EventHandler<SpellCheckerChangedEventArgs> _changed;

	public SpellChecker(string languageTag)
	{
		_speller = new ChangeNotifyWrapper<RCW.ISpellChecker>();
		_languageTag = languageTag;
		_spellCheckerChangedEventHandler = new SpellCheckerChangedEventHandler(this);
		if (Init(shouldSuppressCOMExceptions: false))
		{
			_speller.PropertyChanged += SpellerInstanceChanged;
		}
	}

	private bool Init(bool shouldSuppressCOMExceptions = true)
	{
		_speller.Value = SpellCheckerFactory.CreateSpellChecker(_languageTag, shouldSuppressCOMExceptions);
		return _speller.Value != null;
	}

	public string GetLanguageTag()
	{
		if (!_disposed)
		{
			return _languageTag;
		}
		return null;
	}

	public List<string> SuggestImpl(string word)
	{
		return _speller.Value.Suggest(word)?.ToList(shouldSuppressCOMExceptions: false);
	}

	public List<string> SuggestImplWithRetries(string word, bool shouldSuppressCOMExceptions = true)
	{
		List<string> result = null;
		if (!RetryHelper.TryExecuteFunction(() => SuggestImpl(word), out result, () => Init(shouldSuppressCOMExceptions), SuppressedExceptions[shouldSuppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public List<string> Suggest(string word, bool shouldSuppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return SuggestImplWithRetries(word, shouldSuppressCOMExceptions);
		}
		return null;
	}

	private void AddImpl(string word)
	{
		_speller.Value.Add(word);
	}

	private void AddImplWithRetries(string word, bool shouldSuppressCOMExceptions = true)
	{
		RetryHelper.TryCallAction(delegate
		{
			AddImpl(word);
		}, () => Init(shouldSuppressCOMExceptions), SuppressedExceptions[shouldSuppressCOMExceptions]);
	}

	public void Add(string word, bool shouldSuppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			AddImplWithRetries(word, shouldSuppressCOMExceptions);
		}
	}

	private void IgnoreImpl(string word)
	{
		_speller.Value.Ignore(word);
	}

	public void IgnoreImplWithRetries(string word, bool shouldSuppressCOMExceptions = true)
	{
		RetryHelper.TryCallAction(delegate
		{
			IgnoreImpl(word);
		}, () => Init(shouldSuppressCOMExceptions), SuppressedExceptions[shouldSuppressCOMExceptions]);
	}

	public void Ignore(string word, bool shouldSuppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			IgnoreImplWithRetries(word, shouldSuppressCOMExceptions);
		}
	}

	private void AutoCorrectImpl(string from, string to)
	{
		_speller.Value.AutoCorrect(from, to);
	}

	private void AutoCorrectImplWithRetries(string from, string to, bool suppressCOMExceptions = true)
	{
		RetryHelper.TryCallAction(delegate
		{
			AutoCorrectImpl(from, to);
		}, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]);
	}

	public void AutoCorrect(string from, string to, bool suppressCOMExceptions = true)
	{
		AutoCorrectImplWithRetries(from, to, suppressCOMExceptions);
	}

	private byte GetOptionValueImpl(string optionId)
	{
		return _speller.Value.GetOptionValue(optionId);
	}

	private byte GetOptionValueImplWithRetries(string optionId, bool suppressCOMExceptions = true)
	{
		if (!RetryHelper.TryExecuteFunction(() => GetOptionValueImpl(optionId), out var result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return 0;
		}
		return result;
	}

	public byte GetOptionValue(string optionId, bool suppressCOMExceptions = true)
	{
		return GetOptionValueImplWithRetries(optionId, suppressCOMExceptions);
	}

	private List<string> GetOptionIdsImpl()
	{
		return _speller.Value.OptionIds?.ToList(shouldSuppressCOMExceptions: false);
	}

	private List<string> GetOptionIdsImplWithRetries(bool suppressCOMExceptions)
	{
		List<string> result = null;
		if (!RetryHelper.TryExecuteFunction(GetOptionIdsImpl, out result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public List<string> GetOptionIds(bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return GetOptionIdsImplWithRetries(suppressCOMExceptions);
		}
		return null;
	}

	private string GetIdImpl()
	{
		return _speller.Value.Id;
	}

	private string GetIdImplWithRetries(bool suppressCOMExceptions)
	{
		string result = null;
		if (!RetryHelper.TryExecuteFunction(GetIdImpl, out result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	private string GetId(bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return GetIdImplWithRetries(suppressCOMExceptions);
		}
		return null;
	}

	private string GetLocalizedNameImpl()
	{
		return _speller.Value.LocalizedName;
	}

	private string GetLocalizedNameImplWithRetries(bool suppressCOMExceptions)
	{
		string result = null;
		if (!RetryHelper.TryExecuteFunction(GetLocalizedNameImpl, out result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public string GetLocalizedName(bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return GetLocalizedNameImplWithRetries(suppressCOMExceptions);
		}
		return null;
	}

	private OptionDescription GetOptionDescriptionImpl(string optionId)
	{
		RCW.IOptionDescription optionDescription = _speller.Value.GetOptionDescription(optionId);
		if (optionDescription == null)
		{
			return null;
		}
		return OptionDescription.Create(optionDescription, shouldSuppressCOMExceptions: false);
	}

	private OptionDescription GetOptionDescriptionImplWithRetries(string optionId, bool suppressCOMExceptions)
	{
		OptionDescription result = null;
		if (!RetryHelper.TryExecuteFunction(() => GetOptionDescriptionImpl(optionId), out result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public OptionDescription GetOptionDescription(string optionId, bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return GetOptionDescriptionImplWithRetries(optionId, suppressCOMExceptions);
		}
		return null;
	}

	private List<SpellingError> CheckImpl(string text)
	{
		return _speller.Value.Check(text)?.ToList(this, text, shouldSuppressCOMExceptions: false);
	}

	private List<SpellingError> CheckImplWithRetries(string text, bool suppressCOMExceptions)
	{
		List<SpellingError> result = null;
		if (!RetryHelper.TryExecuteFunction(() => CheckImpl(text), out result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public List<SpellingError> Check(string text, bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return CheckImplWithRetries(text, suppressCOMExceptions);
		}
		return null;
	}

	public List<SpellingError> ComprehensiveCheckImpl(string text)
	{
		return _speller.Value.ComprehensiveCheck(text)?.ToList(this, text, shouldSuppressCOMExceptions: false);
	}

	public List<SpellingError> ComprehensiveCheckImplWithRetries(string text, bool shouldSuppressCOMExceptions = true)
	{
		List<SpellingError> result = null;
		if (!RetryHelper.TryExecuteFunction(() => ComprehensiveCheckImpl(text), out result, () => Init(shouldSuppressCOMExceptions), SuppressedExceptions[shouldSuppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	public List<SpellingError> ComprehensiveCheck(string text, bool shouldSuppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return ComprehensiveCheckImplWithRetries(text, shouldSuppressCOMExceptions);
		}
		return null;
	}

	public bool HasErrorsImpl(string text)
	{
		return _speller.Value.ComprehensiveCheck(text)?.HasErrors(shouldSuppressCOMExceptions: false) ?? false;
	}

	public bool HasErrorsImplWithRetries(string text, bool shouldSuppressCOMExceptions = true)
	{
		bool result = false;
		if (!RetryHelper.TryExecuteFunction(() => HasErrorsImpl(text), out result, () => Init(shouldSuppressCOMExceptions), SuppressedExceptions[shouldSuppressCOMExceptions]))
		{
			return false;
		}
		return result;
	}

	public bool HasErrors(string text, bool shouldSuppressCOMExceptions = true)
	{
		if (_disposed || string.IsNullOrWhiteSpace(text))
		{
			return false;
		}
		List<HasErrorsResult> list = _hasErrorsCache;
		int num = list?.Count ?? 0;
		int i;
		for (i = 0; i < num && !(text == list[i].Text); i++)
		{
		}
		HasErrorsResult hasErrorsResult;
		if (i < num)
		{
			hasErrorsResult = list[i];
		}
		else
		{
			hasErrorsResult = new HasErrorsResult(text, HasErrorsImplWithRetries(text, shouldSuppressCOMExceptions));
			if (list == null)
			{
				list = (_hasErrorsCache = new List<HasErrorsResult>(10));
			}
			if (num < 10)
			{
				list.Add(hasErrorsResult);
			}
			else
			{
				i = 9;
			}
		}
		while (i > 0)
		{
			list[i] = list[i - 1];
			i--;
		}
		list[0] = hasErrorsResult;
		return hasErrorsResult.HasErrors;
	}

	private uint? add_SpellCheckerChangedImpl(RCW.ISpellCheckerChangedEventHandler handler)
	{
		if (handler == null)
		{
			return _speller.Value.add_SpellCheckerChanged(handler);
		}
		return null;
	}

	private uint? addSpellCheckerChangedImplWithRetries(RCW.ISpellCheckerChangedEventHandler handler, bool suppressCOMExceptions)
	{
		if (!RetryHelper.TryExecuteFunction(() => add_SpellCheckerChangedImpl(handler), out var result, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	private uint? add_SpellCheckerChanged(RCW.ISpellCheckerChangedEventHandler handler, bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			return addSpellCheckerChangedImplWithRetries(handler, suppressCOMExceptions);
		}
		return null;
	}

	private void remove_SpellCheckerChangedImpl(uint eventCookie)
	{
		_speller.Value.remove_SpellCheckerChanged(eventCookie);
	}

	private void remove_SpellCheckerChangedImplWithRetries(uint eventCookie, bool suppressCOMExceptions = true)
	{
		RetryHelper.TryCallAction(delegate
		{
			remove_SpellCheckerChangedImpl(eventCookie);
		}, () => Init(suppressCOMExceptions), SuppressedExceptions[suppressCOMExceptions]);
	}

	private void remove_SpellCheckerChanged(uint eventCookie, bool suppressCOMExceptions = true)
	{
		if (!_disposed)
		{
			remove_SpellCheckerChangedImplWithRetries(eventCookie, suppressCOMExceptions);
		}
	}

	private void SpellerInstanceChanged(object sender, PropertyChangedEventArgs args)
	{
		_hasErrorsCache = null;
		if (this._changed == null)
		{
			return;
		}
		lock (this._changed)
		{
			if (this._changed != null)
			{
				_eventCookie = add_SpellCheckerChanged(_spellCheckerChangedEventHandler);
			}
		}
	}

	internal virtual void OnChanged(SpellCheckerChangedEventArgs e)
	{
		_hasErrorsCache = null;
		this._changed?.Invoke(this, e);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}
		_disposed = true;
		if (_speller?.Value != null)
		{
			try
			{
				Marshal.ReleaseComObject(_speller.Value);
			}
			catch
			{
			}
			_speller = null;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~SpellChecker()
	{
		Dispose(disposing: false);
	}
}
