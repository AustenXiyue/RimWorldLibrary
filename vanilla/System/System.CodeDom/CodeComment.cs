namespace System.CodeDom;

/// <summary>Represents a comment.</summary>
[Serializable]
public class CodeComment : CodeObject
{
	private string _text;

	/// <summary>Gets or sets a value that indicates whether the comment is a documentation comment.</summary>
	/// <returns>true if the comment is a documentation comment; otherwise, false.</returns>
	public bool DocComment { get; set; }

	/// <summary>Gets or sets the text of the comment.</summary>
	/// <returns>A string containing the comment text.</returns>
	public string Text
	{
		get
		{
			return _text ?? string.Empty;
		}
		set
		{
			_text = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeComment" /> class.</summary>
	public CodeComment()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeComment" /> class with the specified text as contents.</summary>
	/// <param name="text">The contents of the comment. </param>
	public CodeComment(string text)
	{
		Text = text;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeComment" /> class using the specified text and documentation comment flag.</summary>
	/// <param name="text">The contents of the comment. </param>
	/// <param name="docComment">true if the comment is a documentation comment; otherwise, false. </param>
	public CodeComment(string text, bool docComment)
	{
		Text = text;
		DocComment = docComment;
	}
}
