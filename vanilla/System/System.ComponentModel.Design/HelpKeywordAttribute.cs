namespace System.ComponentModel.Design;

/// <summary>Specifies the context keyword for a class or member. This class cannot be inherited.</summary>
[Serializable]
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
public sealed class HelpKeywordAttribute : Attribute
{
	/// <summary>Represents the default value for <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" />. This field is read-only.</summary>
	public static readonly HelpKeywordAttribute Default = new HelpKeywordAttribute();

	private string contextKeyword;

	/// <summary>Gets the Help keyword supplied by this attribute.</summary>
	/// <returns>The Help keyword supplied by this attribute.</returns>
	public string HelpKeyword => contextKeyword;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> class. </summary>
	public HelpKeywordAttribute()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> class. </summary>
	/// <param name="keyword">The Help keyword value.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="keyword" /> is null.</exception>
	public HelpKeywordAttribute(string keyword)
	{
		if (keyword == null)
		{
			throw new ArgumentNullException("keyword");
		}
		contextKeyword = keyword;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> class from the given type. </summary>
	/// <param name="t">The type from which the Help keyword will be taken.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="t" /> is null.</exception>
	public HelpKeywordAttribute(Type t)
	{
		if (t == null)
		{
			throw new ArgumentNullException("t");
		}
		contextKeyword = t.FullName;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> instances are equal.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> is equal to the current <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" />; otherwise, false.</returns>
	/// <param name="obj">The <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" /> to compare with the current <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" />.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj != null && obj is HelpKeywordAttribute)
		{
			return ((HelpKeywordAttribute)obj).HelpKeyword == HelpKeyword;
		}
		return false;
	}

	/// <summary>Returns the hash code for this instance.</summary>
	/// <returns>A hash code for the current <see cref="T:System.ComponentModel.Design.HelpKeywordAttribute" />.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Determines whether the Help keyword is null.</summary>
	/// <returns>true if the Help keyword is null; otherwise, false.</returns>
	public override bool IsDefaultAttribute()
	{
		return Equals(Default);
	}
}
