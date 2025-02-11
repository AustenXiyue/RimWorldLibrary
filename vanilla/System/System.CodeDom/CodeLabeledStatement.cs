namespace System.CodeDom;

/// <summary>Represents a labeled statement or a stand-alone label.</summary>
[Serializable]
public class CodeLabeledStatement : CodeStatement
{
	private string _label;

	/// <summary>Gets or sets the name of the label.</summary>
	/// <returns>The name of the label.</returns>
	public string Label
	{
		get
		{
			return _label ?? string.Empty;
		}
		set
		{
			_label = value;
		}
	}

	/// <summary>Gets or sets the optional associated statement.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeStatement" /> that indicates the statement associated with the label.</returns>
	public CodeStatement Statement { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeLabeledStatement" /> class.</summary>
	public CodeLabeledStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeLabeledStatement" /> class using the specified label name.</summary>
	/// <param name="label">The name of the label. </param>
	public CodeLabeledStatement(string label)
	{
		_label = label;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeLabeledStatement" /> class using the specified label name and statement.</summary>
	/// <param name="label">The name of the label. </param>
	/// <param name="statement">The <see cref="T:System.CodeDom.CodeStatement" /> to associate with the label. </param>
	public CodeLabeledStatement(string label, CodeStatement statement)
	{
		_label = label;
		Statement = statement;
	}
}
