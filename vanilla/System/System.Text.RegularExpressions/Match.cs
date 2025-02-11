using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the results from a single regular expression match.</summary>
[Serializable]
public class Match : Group
{
	internal static Match _empty = new Match(null, 1, string.Empty, 0, 0, 0);

	internal GroupCollection _groupcoll;

	internal Regex _regex;

	internal int _textbeg;

	internal int _textpos;

	internal int _textend;

	internal int _textstart;

	internal int[][] _matches;

	internal int[] _matchcount;

	internal bool _balancing;

	/// <summary>Gets the empty group. All failed matches return this empty match.</summary>
	/// <returns>An empty match.</returns>
	public static Match Empty => _empty;

	/// <summary>Gets a collection of groups matched by the regular expression.</summary>
	/// <returns>The character groups matched by the pattern.</returns>
	public virtual GroupCollection Groups
	{
		get
		{
			if (_groupcoll == null)
			{
				_groupcoll = new GroupCollection(this, null);
			}
			return _groupcoll;
		}
	}

	internal Match(Regex regex, int capcount, string text, int begpos, int len, int startpos)
		: base(text, new int[2], 0, "0")
	{
		_regex = regex;
		_matchcount = new int[capcount];
		_matches = new int[capcount][];
		_matches[0] = _caps;
		_textbeg = begpos;
		_textend = begpos + len;
		_textstart = startpos;
		_balancing = false;
	}

	internal virtual void Reset(Regex regex, string text, int textbeg, int textend, int textstart)
	{
		_regex = regex;
		_text = text;
		_textbeg = textbeg;
		_textend = textend;
		_textstart = textstart;
		for (int i = 0; i < _matchcount.Length; i++)
		{
			_matchcount[i] = 0;
		}
		_balancing = false;
	}

	/// <summary>Returns a new <see cref="T:System.Text.RegularExpressions.Match" /> object with the results for the next match, starting at the position at which the last match ended (at the character after the last matched character).</summary>
	/// <returns>The next regular expression match.</returns>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public Match NextMatch()
	{
		if (_regex == null)
		{
			return this;
		}
		return _regex.Run(quick: false, _length, _text, _textbeg, _textend - _textbeg, _textpos);
	}

	/// <summary>Returns the expansion of the specified replacement pattern. </summary>
	/// <returns>The expanded version of the <paramref name="replacement" /> parameter.</returns>
	/// <param name="replacement">The replacement pattern to use. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="replacement" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">Expansion is not allowed for this pattern.</exception>
	public virtual string Result(string replacement)
	{
		if (replacement == null)
		{
			throw new ArgumentNullException("replacement");
		}
		if (_regex == null)
		{
			throw new NotSupportedException(global::SR.GetString("Result cannot be called on a failed Match."));
		}
		RegexReplacement regexReplacement = (RegexReplacement)_regex.replref.Get();
		if (regexReplacement == null || !regexReplacement.Pattern.Equals(replacement))
		{
			regexReplacement = RegexParser.ParseReplacement(replacement, _regex.caps, _regex.capsize, _regex.capnames, _regex.roptions);
			_regex.replref.Cache(regexReplacement);
		}
		return regexReplacement.Replacement(this);
	}

	internal virtual string GroupToStringImpl(int groupnum)
	{
		int num = _matchcount[groupnum];
		if (num == 0)
		{
			return string.Empty;
		}
		int[] array = _matches[groupnum];
		return _text.Substring(array[(num - 1) * 2], array[num * 2 - 1]);
	}

	internal string LastGroupToStringImpl()
	{
		return GroupToStringImpl(_matchcount.Length - 1);
	}

	/// <summary>Returns a <see cref="T:System.Text.RegularExpressions.Match" /> instance equivalent to the one supplied that is suitable to share between multiple threads.</summary>
	/// <returns>A regular expression match that is suitable to share between multiple threads. </returns>
	/// <param name="inner">A regular expression match equivalent to the one expected. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inner" /> is null.</exception>
	public static Match Synchronized(Match inner)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		int num = inner._matchcount.Length;
		for (int i = 0; i < num; i++)
		{
			Group.Synchronized(inner.Groups[i]);
		}
		return inner;
	}

	internal virtual void AddMatch(int cap, int start, int len)
	{
		if (_matches[cap] == null)
		{
			_matches[cap] = new int[2];
		}
		int num = _matchcount[cap];
		if (num * 2 + 2 > _matches[cap].Length)
		{
			int[] array = _matches[cap];
			int[] array2 = new int[num * 8];
			for (int i = 0; i < num * 2; i++)
			{
				array2[i] = array[i];
			}
			_matches[cap] = array2;
		}
		_matches[cap][num * 2] = start;
		_matches[cap][num * 2 + 1] = len;
		_matchcount[cap] = num + 1;
	}

	internal virtual void BalanceMatch(int cap)
	{
		_balancing = true;
		int num = _matchcount[cap] * 2 - 2;
		if (_matches[cap][num] < 0)
		{
			num = -3 - _matches[cap][num];
		}
		num -= 2;
		if (num >= 0 && _matches[cap][num] < 0)
		{
			AddMatch(cap, _matches[cap][num], _matches[cap][num + 1]);
		}
		else
		{
			AddMatch(cap, -3 - num, -4 - num);
		}
	}

	internal virtual void RemoveMatch(int cap)
	{
		_matchcount[cap]--;
	}

	internal virtual bool IsMatched(int cap)
	{
		if (cap < _matchcount.Length && _matchcount[cap] > 0)
		{
			return _matches[cap][_matchcount[cap] * 2 - 1] != -2;
		}
		return false;
	}

	internal virtual int MatchIndex(int cap)
	{
		int num = _matches[cap][_matchcount[cap] * 2 - 2];
		if (num >= 0)
		{
			return num;
		}
		return _matches[cap][-3 - num];
	}

	internal virtual int MatchLength(int cap)
	{
		int num = _matches[cap][_matchcount[cap] * 2 - 1];
		if (num >= 0)
		{
			return num;
		}
		return _matches[cap][-3 - num];
	}

	internal virtual void Tidy(int textpos)
	{
		int[] array = _matches[0];
		_index = array[0];
		_length = array[1];
		_textpos = textpos;
		_capcount = _matchcount[0];
		if (!_balancing)
		{
			return;
		}
		for (int i = 0; i < _matchcount.Length; i++)
		{
			int num = _matchcount[i] * 2;
			int[] array2 = _matches[i];
			int num2 = 0;
			for (num2 = 0; num2 < num && array2[num2] >= 0; num2++)
			{
			}
			int num3 = num2;
			for (; num2 < num; num2++)
			{
				if (array2[num2] < 0)
				{
					num3--;
					continue;
				}
				if (num2 != num3)
				{
					array2[num3] = array2[num2];
				}
				num3++;
			}
			_matchcount[i] = num3 / 2;
		}
		_balancing = false;
	}

	internal Match()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
