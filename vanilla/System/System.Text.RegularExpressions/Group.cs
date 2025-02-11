using System.Runtime.Serialization;
using Unity;

namespace System.Text.RegularExpressions;

/// <summary>Represents the results from a single capturing group. </summary>
[Serializable]
public class Group : Capture
{
	internal static Group _emptygroup = new Group(string.Empty, new int[0], 0, string.Empty);

	internal int[] _caps;

	internal int _capcount;

	internal CaptureCollection _capcoll;

	[OptionalField]
	internal string _name;

	/// <summary>Gets a value indicating whether the match is successful.</summary>
	/// <returns>true if the match is successful; otherwise, false.</returns>
	public bool Success => _capcount != 0;

	public string Name => _name;

	/// <summary>Gets a collection of all the captures matched by the capturing group, in innermost-leftmost-first order (or innermost-rightmost-first order if the regular expression is modified with the <see cref="F:System.Text.RegularExpressions.RegexOptions.RightToLeft" /> option). The collection may have zero or more items.</summary>
	/// <returns>The collection of substrings matched by the group.</returns>
	public CaptureCollection Captures
	{
		get
		{
			if (_capcoll == null)
			{
				_capcoll = new CaptureCollection(this);
			}
			return _capcoll;
		}
	}

	internal Group(string text, int[] caps, int capcount, string name)
		: base(text, (capcount != 0) ? caps[(capcount - 1) * 2] : 0, (capcount != 0) ? caps[capcount * 2 - 1] : 0)
	{
		_caps = caps;
		_capcount = capcount;
		_name = name;
	}

	/// <summary>Returns a Group object equivalent to the one supplied that is safe to share between multiple threads.</summary>
	/// <returns>A regular expression Group object. </returns>
	/// <param name="inner">The input <see cref="T:System.Text.RegularExpressions.Group" /> object.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="inner" /> is null.</exception>
	public static Group Synchronized(Group inner)
	{
		if (inner == null)
		{
			throw new ArgumentNullException("inner");
		}
		CaptureCollection captures = inner.Captures;
		if (inner._capcount > 0)
		{
			_ = captures[0];
		}
		return inner;
	}

	internal Group()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
