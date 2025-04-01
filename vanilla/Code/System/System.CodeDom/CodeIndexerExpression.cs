namespace System.CodeDom;

/// <summary>Represents a reference to an indexer property of an object.</summary>
[Serializable]
public class CodeIndexerExpression : CodeExpression
{
	private CodeExpressionCollection _indices;

	/// <summary>Gets or sets the target object that can be indexed.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpression" /> that indicates the indexer object.</returns>
	public CodeExpression TargetObject { get; set; }

	/// <summary>Gets the collection of indexes of the indexer expression.</summary>
	/// <returns>A <see cref="T:System.CodeDom.CodeExpressionCollection" /> that indicates the index or indexes of the indexer expression.</returns>
	public CodeExpressionCollection Indices => _indices ?? (_indices = new CodeExpressionCollection());

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeIndexerExpression" /> class.</summary>
	public CodeIndexerExpression()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.CodeDom.CodeIndexerExpression" /> class using the specified target object and index.</summary>
	/// <param name="targetObject">The target object. </param>
	/// <param name="indices">The index or indexes of the indexer expression. </param>
	public CodeIndexerExpression(CodeExpression targetObject, params CodeExpression[] indices)
	{
		TargetObject = targetObject;
		Indices.AddRange(indices);
	}
}
