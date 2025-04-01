using System.Runtime.Serialization;

namespace System.Text.RegularExpressions;

/// <summary>Provides information about a regular expression that is used to compile a regular expression to a stand-alone assembly. </summary>
[Serializable]
public class RegexCompilationInfo
{
	private string pattern;

	private RegexOptions options;

	private string name;

	private string nspace;

	private bool isPublic;

	[OptionalField(VersionAdded = 2)]
	private TimeSpan matchTimeout;

	/// <summary>Gets or sets the regular expression to compile.</summary>
	/// <returns>The regular expression to compile.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value for this property is null.</exception>
	public string Pattern
	{
		get
		{
			return pattern;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			pattern = value;
		}
	}

	/// <summary>Gets or sets the options to use when compiling the regular expression.</summary>
	/// <returns>A bitwise combination of the enumeration values.</returns>
	public RegexOptions Options
	{
		get
		{
			return options;
		}
		set
		{
			options = value;
		}
	}

	/// <summary>Gets or sets the name of the type that represents the compiled regular expression.</summary>
	/// <returns>The name of the new type.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value for this property is null.</exception>
	/// <exception cref="T:System.ArgumentException">The value for this property is an empty string.</exception>
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value.Length == 0)
			{
				throw new ArgumentException(global::SR.GetString("Argument {0} cannot be null or zero-length.", "value"), "value");
			}
			name = value;
		}
	}

	/// <summary>Gets or sets the namespace to which the new type belongs.</summary>
	/// <returns>The namespace of the new type.</returns>
	/// <exception cref="T:System.ArgumentNullException">The value for this property is null.</exception>
	public string Namespace
	{
		get
		{
			return nspace;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			nspace = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the compiled regular expression has public visibility.</summary>
	/// <returns>true if the regular expression has public visibility; otherwise, false.</returns>
	public bool IsPublic
	{
		get
		{
			return isPublic;
		}
		set
		{
			isPublic = value;
		}
	}

	/// <summary>Gets or sets the regular expression's default time-out interval.</summary>
	/// <returns>The default maximum time interval that can elapse in a pattern-matching operation before a <see cref="T:System.Text.RegularExpressions.RegexMatchTimeoutException" /> is thrown, or <see cref="F:System.Text.RegularExpressions.Regex.InfiniteMatchTimeout" /> if time-outs are disabled.</returns>
	public TimeSpan MatchTimeout
	{
		get
		{
			return matchTimeout;
		}
		set
		{
			Regex.ValidateMatchTimeout(value);
			matchTimeout = value;
		}
	}

	[OnDeserializing]
	private void InitMatchTimeoutDefaultForOldVersionDeserialization(StreamingContext unusedContext)
	{
		matchTimeout = Regex.DefaultMatchTimeout;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.RegularExpressions.RegexCompilationInfo" /> class that contains information about a regular expression to be included in an assembly. </summary>
	/// <param name="pattern">The regular expression to compile. </param>
	/// <param name="options">The regular expression options to use when compiling the regular expression. </param>
	/// <param name="name">The name of the type that represents the compiled regular expression. </param>
	/// <param name="fullnamespace">The namespace to which the new type belongs. </param>
	/// <param name="ispublic">true to make the compiled regular expression publicly visible; otherwise, false. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pattern" /> is null.-or-<paramref name="name" /> is null.-or-<paramref name="fullnamespace" /> is null.</exception>
	public RegexCompilationInfo(string pattern, RegexOptions options, string name, string fullnamespace, bool ispublic)
		: this(pattern, options, name, fullnamespace, ispublic, Regex.DefaultMatchTimeout)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Text.RegularExpressions.RegexCompilationInfo" /> class that contains information about a regular expression with a specified time-out value to be included in an assembly.</summary>
	/// <param name="pattern">The regular expression to compile.</param>
	/// <param name="options">The regular expression options to use when compiling the regular expression.</param>
	/// <param name="name">The name of the type that represents the compiled regular expression.</param>
	/// <param name="fullnamespace">The namespace to which the new type belongs.</param>
	/// <param name="ispublic">true to make the compiled regular expression publicly visible; otherwise, false.</param>
	/// <param name="matchTimeout">The default time-out interval for the regular expression.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="name" /> is <see cref="F:System.String.Empty" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="pattern" /> is null.-or-<paramref name="name" /> is null.-or-<paramref name="fullnamespace" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="matchTimeout" /> is negative, zero, or greater than approximately 24 days.</exception>
	public RegexCompilationInfo(string pattern, RegexOptions options, string name, string fullnamespace, bool ispublic, TimeSpan matchTimeout)
	{
		Pattern = pattern;
		Name = name;
		Namespace = fullnamespace;
		this.options = options;
		isPublic = ispublic;
		MatchTimeout = matchTimeout;
	}
}
