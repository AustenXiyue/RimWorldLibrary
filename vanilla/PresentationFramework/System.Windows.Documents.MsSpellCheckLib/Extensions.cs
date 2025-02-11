using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace System.Windows.Documents.MsSpellCheckLib;

internal static class Extensions
{
	internal static List<string> ToList(this RCW.IEnumString enumString, bool shouldSuppressCOMExceptions = true, bool shouldReleaseCOMObject = true)
	{
		List<string> list = new List<string>();
		if (enumString == null)
		{
			throw new ArgumentNullException("enumString");
		}
		try
		{
			uint pceltFetched = 0u;
			string rgelt = string.Empty;
			do
			{
				enumString.RemoteNext(1u, out rgelt, out pceltFetched);
				if (pceltFetched != 0)
				{
					list.Add(rgelt);
				}
			}
			while (pceltFetched != 0);
		}
		catch (COMException) when (shouldSuppressCOMExceptions)
		{
		}
		finally
		{
			if (shouldReleaseCOMObject)
			{
				Marshal.ReleaseComObject(enumString);
			}
		}
		return list;
	}

	internal static List<SpellChecker.SpellingError> ToList(this RCW.IEnumSpellingError spellingErrors, SpellChecker spellChecker, string text, bool shouldSuppressCOMExceptions = true, bool shouldReleaseCOMObject = true)
	{
		if (spellingErrors == null)
		{
			throw new ArgumentNullException("spellingErrors");
		}
		List<SpellChecker.SpellingError> list = new List<SpellChecker.SpellingError>();
		try
		{
			while (true)
			{
				RCW.ISpellingError spellingError = spellingErrors.Next();
				if (spellingError != null)
				{
					SpellChecker.SpellingError item = new SpellChecker.SpellingError(spellingError, spellChecker, text, shouldSuppressCOMExceptions);
					list.Add(item);
					continue;
				}
				break;
			}
		}
		catch (COMException) when (shouldSuppressCOMExceptions)
		{
		}
		finally
		{
			if (shouldReleaseCOMObject)
			{
				Marshal.ReleaseComObject(spellingErrors);
			}
		}
		return list;
	}

	internal static bool IsClean(this List<SpellChecker.SpellingError> errors)
	{
		if (errors == null)
		{
			throw new ArgumentNullException("errors");
		}
		bool result = true;
		foreach (SpellChecker.SpellingError error in errors)
		{
			if (error.CorrectiveAction != 0)
			{
				result = false;
				break;
			}
		}
		return result;
	}

	internal static bool HasErrors(this RCW.IEnumSpellingError spellingErrors, bool shouldSuppressCOMExceptions = true, bool shouldReleaseCOMObject = true)
	{
		if (spellingErrors == null)
		{
			throw new ArgumentNullException("spellingErrors");
		}
		bool flag = false;
		try
		{
			while (!flag)
			{
				RCW.ISpellingError spellingError = spellingErrors.Next();
				if (spellingError != null)
				{
					if (spellingError.CorrectiveAction != 0)
					{
						flag = true;
					}
					Marshal.ReleaseComObject(spellingError);
					continue;
				}
				break;
			}
		}
		catch (COMException) when (shouldSuppressCOMExceptions)
		{
		}
		finally
		{
			if (shouldReleaseCOMObject)
			{
				Marshal.ReleaseComObject(spellingErrors);
			}
		}
		return flag;
	}
}
