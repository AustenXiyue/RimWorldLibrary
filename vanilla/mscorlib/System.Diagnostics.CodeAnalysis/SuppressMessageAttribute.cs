namespace System.Diagnostics.CodeAnalysis;

/// <summary>Suppresses reporting of a specific static analysis tool rule violation, allowing multiple suppressions on a single code artifact.</summary>
[Conditional("CODE_ANALYSIS")]
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class SuppressMessageAttribute : Attribute
{
	private string category;

	private string justification;

	private string checkId;

	private string scope;

	private string target;

	private string messageId;

	/// <summary>Gets the category identifying the classification of the attribute.</summary>
	/// <returns>The category identifying the attribute.</returns>
	public string Category => category;

	/// <summary>Gets the identifier of the static analysis tool rule to be suppressed.</summary>
	/// <returns>The identifier of the static analysis tool rule to be suppressed.</returns>
	public string CheckId => checkId;

	/// <summary>Gets or sets the scope of the code that is relevant for the attribute.</summary>
	/// <returns>The scope of the code that is relevant for the attribute.</returns>
	public string Scope
	{
		get
		{
			return scope;
		}
		set
		{
			scope = value;
		}
	}

	/// <summary>Gets or sets a fully qualified path that represents the target of the attribute.</summary>
	/// <returns>A fully qualified path that represents the target of the attribute.</returns>
	public string Target
	{
		get
		{
			return target;
		}
		set
		{
			target = value;
		}
	}

	/// <summary>Gets or sets an optional argument expanding on exclusion criteria.</summary>
	/// <returns>A string containing the expanded exclusion criteria.</returns>
	public string MessageId
	{
		get
		{
			return messageId;
		}
		set
		{
			messageId = value;
		}
	}

	/// <summary>Gets or sets the justification for suppressing the code analysis message.</summary>
	/// <returns>The justification for suppressing the message.</returns>
	public string Justification
	{
		get
		{
			return justification;
		}
		set
		{
			justification = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.CodeAnalysis.SuppressMessageAttribute" /> class, specifying the category of the static analysis tool and the identifier for an analysis rule. </summary>
	/// <param name="category">The category for the attribute.</param>
	/// <param name="checkId">The identifier of the analysis tool rule the attribute applies to.</param>
	public SuppressMessageAttribute(string category, string checkId)
	{
		this.category = category;
		this.checkId = checkId;
	}
}
