namespace System.CodeDom;

/// <summary>Represents a goto statement.</summary>
[Serializable]
public class CodeGotoStatement : CodeStatement
{
	private string _label;

	/// <summary>Gets or sets the name of the label at which to continue program execution.</summary>
	/// <returns>A string that indicates the name of the label at which to continue program execution.</returns>
	/// <exception cref="T:System.ArgumentNullException">The label cannot be set because<paramref name=" value" /> is null or an empty string.</exception>
	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentNullException("value");
			}
			_label = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeGotoStatement" /> class. </summary>
	public CodeGotoStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeGotoStatement" /> class using the specified label name.</summary>
	/// <param name="label">The name of the label at which to continue program execution. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="Label" /> is null.</exception>
	public CodeGotoStatement(string label)
	{
		Label = label;
	}
}
