using System.Collections;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the set of successful matches found by iteratively applying a regular expression pattern to the input string.</summary>
[Serializable]
public class MatchCollection : ICollection, IEnumerable
{
	internal Regex _regex;

	internal ArrayList _matches;

	internal bool _done;

	internal string _input;

	internal int _beginning;

	internal int _length;

	internal int _startat;

	internal int _prevlen;

	private static int infinite = int.MaxValue;

	/// <summary>Gets the number of matches.</summary>
	/// <returns>The number of matches.</returns>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public int Count
	{
		get
		{
			if (_done)
			{
				return _matches.Count;
			}
			GetMatch(infinite);
			return _matches.Count;
		}
	}

	/// <summary>Gets an object that can be used to synchronize access to the collection.</summary>
	/// <returns>An object that can be used to synchronize access to the collection. This property always returns the object itself.</returns>
	public object SyncRoot => this;

	/// <summary>Gets a value indicating whether access to the collection is synchronized (thread-safe).</summary>
	/// <returns>false in all cases.</returns>
	public bool IsSynchronized => false;

	/// <summary>Gets a value that indicates whether the collection is read only.</summary>
	/// <returns>true in all cases. </returns>
	public bool IsReadOnly => true;

	/// <summary>Gets an individual member of the collection.</summary>
	/// <returns>The captured substring at position <paramref name="i" /> in the collection.</returns>
	/// <param name="i">Index into the <see cref="T:System.Text.RegularExpressions.Match" /> collection. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="i" /> is less than 0 or greater than or equal to <see cref="P:System.Text.RegularExpressions.MatchCollection.Count" />. </exception>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public virtual Match this[int i] => GetMatch(i) ?? throw new ArgumentOutOfRangeException("i");

	internal MatchCollection(Regex regex, string input, int beginning, int length, int startat)
	{
		if (startat < 0 || startat > input.Length)
		{
			throw new ArgumentOutOfRangeException("startat", global::SR.GetString("Start index cannot be less than 0 or greater than input length."));
		}
		_regex = regex;
		_input = input;
		_beginning = beginning;
		_length = length;
		_startat = startat;
		_prevlen = -1;
		_matches = new ArrayList();
		_done = false;
	}

	internal Match GetMatch(int i)
	{
		if (i < 0)
		{
			return null;
		}
		if (_matches.Count > i)
		{
			return (Match)_matches[i];
		}
		if (_done)
		{
			return null;
		}
		Match match;
		do
		{
			match = _regex.Run(quick: false, _prevlen, _input, _beginning, _length, _startat);
			if (!match.Success)
			{
				_done = true;
				return null;
			}
			_matches.Add(match);
			_prevlen = match._length;
			_startat = match._textpos;
		}
		while (_matches.Count <= i);
		return match;
	}

	/// <summary>Copies all the elements of the collection to the given array starting at the given index.</summary>
	/// <param name="array">The array the collection is to be copied into. </param>
	/// <param name="arrayIndex">The position in the array where copying is to begin. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="array" /> is a multi-dimensional array.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="arrayIndex" /> is outside the bounds of <paramref name="array" />.-or-<paramref name="arrayIndex" /> plus <see cref="P:System.Text.RegularExpressions.MatchCollection.Count" /> is outside the bounds of <paramref name="array" />.</exception>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public void CopyTo(Array array, int arrayIndex)
	{
		if (array != null && array.Rank != 1)
		{
			throw new ArgumentException(global::SR.GetString("Only single dimensional arrays are supported for the requested action."));
		}
		_ = Count;
		try
		{
			_matches.CopyTo(array, arrayIndex);
		}
		catch (ArrayTypeMismatchException innerException)
		{
			throw new ArgumentException(global::SR.GetString("Target array type is not compatible with the type of items in the collection."), innerException);
		}
	}

	/// <summary>Provides an enumerator that iterates through the collection.</summary>
	/// <returns>An object that contains all <see cref="T:System.Text.RegularExpressions.Match" /> objects within the <see cref="T:System.Text.RegularExpressions.MatchCollection" />.</returns>
	/// <exception cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException">A time-out occurred.</exception>
	public IEnumerator GetEnumerator()
	{
		return new MatchEnumerator(this);
	}

	internal MatchCollection()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
