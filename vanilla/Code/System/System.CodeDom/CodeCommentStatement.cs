namespace System.CodeDom;

/// <summary>Represents a statement consisting of a single comment.</summary>
[Serializable]
public class CodeCommentStatement : CodeStatement
{
	/// <summary>Gets or sets the contents of the comment.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeComment" /> that indicates the comment.</returns>
	public CodeComment Comment { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatement" /> class.</summary>
	public CodeCommentStatement()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatement" /> class using the specified comment.</summary>
	/// <param name="comment">A <see cref="T:System.CodeDom.CodeComment" /> that indicates the comment. </param>
	public CodeCommentStatement(CodeComment comment)
	{
		Comment = comment;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatement" /> class using the specified text as contents.</summary>
	/// <param name="text">The contents of the comment. </param>
	public CodeCommentStatement(string text)
	{
		Comment = new CodeComment(text);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeCommentStatement" /> class using the specified text and documentation comment flag.</summary>
	/// <param name="text">The contents of the comment. </param>
	/// <param name="docComment">true if the comment is a documentation comment; otherwise, false. </param>
	public CodeCommentStatement(string text, bool docComment)
	{
		Comment = new CodeComment(text, docComment);
	}
}
