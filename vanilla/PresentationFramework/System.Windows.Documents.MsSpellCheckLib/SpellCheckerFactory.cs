using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using MS.Internal;

namespace System.Windows.Documents.MsSpellCheckLib;

internal class SpellCheckerFactory
{
	private class SpellCheckerCreationHelper
	{
		private static Dictionary<string, SpellCheckerCreationHelper> _instances = new Dictionary<string, SpellCheckerCreationHelper>();

		private static ReaderWriterLockSlimWrapper _lock = new ReaderWriterLockSlimWrapper();

		private string _language;

		public static RCW.ISpellCheckerFactory ComFactory => Singleton.ComFactory;

		private SpellCheckerCreationHelper(string language)
		{
			_language = language;
		}

		private static void CreateForLanguage(string language)
		{
			_instances[language] = new SpellCheckerCreationHelper(language);
		}

		public static SpellCheckerCreationHelper Helper(string language)
		{
			if (!_instances.ContainsKey(language))
			{
				_lock.WithWriteLock(CreateForLanguage, language);
			}
			return _instances[language];
		}

		public RCW.ISpellChecker CreateSpellChecker()
		{
			return ComFactory?.CreateSpellChecker(_language);
		}

		public bool CreateSpellCheckerRetryPreamble(out Func<RCW.ISpellChecker> func)
		{
			func = null;
			bool num = Reinitalize();
			if (num)
			{
				func = Helper(_language).CreateSpellChecker;
			}
			return num;
		}
	}

	private static ReaderWriterLockSlimWrapper _factoryLock;

	private static Dictionary<bool, List<Type>> SuppressedExceptions;

	internal RCW.ISpellCheckerFactory ComFactory { get; private set; }

	internal static SpellCheckerFactory Singleton { get; private set; }

	public static SpellCheckerFactory Create(bool shouldSuppressCOMExceptions = false)
	{
		SpellCheckerFactory result = null;
		bool result2 = false;
		if (_factoryLock.WithWriteLock(CreateLockFree, shouldSuppressCOMExceptions, arg2: false, out result2) && result2)
		{
			result = Singleton;
		}
		return result;
	}

	private SpellCheckerFactory()
	{
	}

	static SpellCheckerFactory()
	{
		_factoryLock = new ReaderWriterLockSlimWrapper(LockRecursionPolicy.SupportsRecursion);
		SuppressedExceptions = new Dictionary<bool, List<Type>>
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
		Singleton = new SpellCheckerFactory();
		bool result = false;
		_factoryLock.WithWriteLock(CreateLockFree, arg1: true, arg2: true, out result);
	}

	private static bool Reinitalize()
	{
		bool result = false;
		return _factoryLock.WithWriteLock(CreateLockFree, arg1: false, arg2: false, out result) && result;
	}

	private static bool CreateLockFree(bool suppressCOMExceptions = true, bool suppressOtherExceptions = true)
	{
		if (Singleton.ComFactory != null)
		{
			try
			{
				Marshal.ReleaseComObject(Singleton.ComFactory);
			}
			catch
			{
			}
			Singleton.ComFactory = null;
		}
		bool flag = false;
		RCW.ISpellCheckerFactory comFactory = null;
		try
		{
			comFactory = new RCW.SpellCheckerFactoryCoClass();
			flag = true;
		}
		catch (COMException) when (suppressCOMExceptions)
		{
		}
		catch (UnauthorizedAccessException inner)
		{
			if (!suppressCOMExceptions)
			{
				throw new COMException(string.Empty, inner);
			}
		}
		catch (InvalidCastException inner2)
		{
			if (!suppressCOMExceptions)
			{
				throw new COMException(string.Empty, inner2);
			}
		}
		catch (Exception ex2) when (suppressOtherExceptions && !(ex2 is COMException) && !(ex2 is UnauthorizedAccessException))
		{
		}
		if (flag)
		{
			Singleton.ComFactory = comFactory;
		}
		return flag;
	}

	private List<string> SupportedLanguagesImpl()
	{
		RCW.IEnumString enumString = ComFactory?.SupportedLanguages;
		List<string> result = null;
		if (enumString != null)
		{
			result = enumString.ToList();
		}
		return result;
	}

	private List<string> SupportedLanguagesImplWithRetries(bool shouldSuppressCOMExceptions)
	{
		List<string> result = null;
		if (!RetryHelper.TryExecuteFunction(SupportedLanguagesImpl, out result, () => Reinitalize(), SuppressedExceptions[shouldSuppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	private List<string> GetSupportedLanguagesPrivate(bool shouldSuppressCOMExceptions = true)
	{
		List<string> result = null;
		if (!_factoryLock.WithWriteLock(SupportedLanguagesImplWithRetries, shouldSuppressCOMExceptions, out result))
		{
			return null;
		}
		return result;
	}

	internal static List<string> GetSupportedLanguages(bool shouldSuppressCOMExceptions = true)
	{
		return Singleton?.GetSupportedLanguagesPrivate(shouldSuppressCOMExceptions);
	}

	private bool IsSupportedImpl(string languageTag)
	{
		if (ComFactory != null)
		{
			return ComFactory.IsSupported(languageTag) != 0;
		}
		return false;
	}

	private bool IsSupportedImplWithRetries(string languageTag, bool suppressCOMExceptions = true)
	{
		bool result = false;
		if (!RetryHelper.TryExecuteFunction(() => IsSupportedImpl(languageTag), out result, () => Reinitalize(), SuppressedExceptions[suppressCOMExceptions]))
		{
			return false;
		}
		return result;
	}

	private bool IsSupportedPrivate(string languageTag, bool suppressCOMExceptons = true)
	{
		bool result = false;
		if (!_factoryLock.WithWriteLock(IsSupportedImplWithRetries, languageTag, suppressCOMExceptons, out result))
		{
			return false;
		}
		return result;
	}

	internal static bool IsSupported(string languageTag, bool suppressCOMExceptons = true)
	{
		if (Singleton != null)
		{
			return Singleton.IsSupportedPrivate(languageTag, suppressCOMExceptons);
		}
		return false;
	}

	private RCW.ISpellChecker CreateSpellCheckerImpl(string languageTag)
	{
		return SpellCheckerCreationHelper.Helper(languageTag).CreateSpellChecker();
	}

	private RCW.ISpellChecker CreateSpellCheckerImplWithRetries(string languageTag, bool suppressCOMExceptions = true)
	{
		RCW.ISpellChecker result = null;
		if (!RetryHelper.TryExecuteFunction(SpellCheckerCreationHelper.Helper(languageTag).CreateSpellChecker, out result, SpellCheckerCreationHelper.Helper(languageTag).CreateSpellCheckerRetryPreamble, SuppressedExceptions[suppressCOMExceptions]))
		{
			return null;
		}
		return result;
	}

	private RCW.ISpellChecker CreateSpellCheckerPrivate(string languageTag, bool suppressCOMExceptions = true)
	{
		RCW.ISpellChecker result = null;
		if (!_factoryLock.WithWriteLock(CreateSpellCheckerImplWithRetries, languageTag, suppressCOMExceptions, out result))
		{
			return null;
		}
		return result;
	}

	internal static RCW.ISpellChecker CreateSpellChecker(string languageTag, bool suppressCOMExceptions = true)
	{
		return Singleton?.CreateSpellCheckerPrivate(languageTag, suppressCOMExceptions);
	}

	private void RegisterUserDicionaryImpl(string dictionaryPath, string languageTag)
	{
		((RCW.IUserDictionariesRegistrar)ComFactory)?.RegisterUserDictionary(dictionaryPath, languageTag);
	}

	private void RegisterUserDictionaryImplWithRetries(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		if (dictionaryPath == null)
		{
			throw new ArgumentNullException("dictionaryPath");
		}
		if (languageTag == null)
		{
			throw new ArgumentNullException("languageTag");
		}
		RetryHelper.TryCallAction(delegate
		{
			RegisterUserDicionaryImpl(dictionaryPath, languageTag);
		}, () => Reinitalize(), SuppressedExceptions[suppressCOMExceptions]);
	}

	private void RegisterUserDictionaryPrivate(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		_factoryLock.WithWriteLock(delegate
		{
			RegisterUserDictionaryImplWithRetries(dictionaryPath, languageTag, suppressCOMExceptions);
		});
	}

	internal static void RegisterUserDictionary(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		Singleton?.RegisterUserDictionaryPrivate(dictionaryPath, languageTag, suppressCOMExceptions);
	}

	private void UnregisterUserDictionaryImpl(string dictionaryPath, string languageTag)
	{
		((RCW.IUserDictionariesRegistrar)ComFactory)?.UnregisterUserDictionary(dictionaryPath, languageTag);
	}

	private void UnregisterUserDictionaryImplWithRetries(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		if (dictionaryPath == null)
		{
			throw new ArgumentNullException("dictionaryPath");
		}
		if (languageTag == null)
		{
			throw new ArgumentNullException("languageTag");
		}
		RetryHelper.TryCallAction(delegate
		{
			UnregisterUserDictionaryImpl(dictionaryPath, languageTag);
		}, () => Reinitalize(), SuppressedExceptions[suppressCOMExceptions]);
	}

	private void UnregisterUserDictionaryPrivate(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		_factoryLock.WithWriteLock(delegate
		{
			UnregisterUserDictionaryImplWithRetries(dictionaryPath, languageTag, suppressCOMExceptions);
		});
	}

	internal static void UnregisterUserDictionary(string dictionaryPath, string languageTag, bool suppressCOMExceptions = true)
	{
		Singleton?.UnregisterUserDictionaryPrivate(dictionaryPath, languageTag, suppressCOMExceptions);
	}
}
