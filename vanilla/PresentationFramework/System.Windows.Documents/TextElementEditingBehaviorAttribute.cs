namespace System.Windows.Documents;

/// <summary>Specifies how a <see cref="T:System.Windows.Controls.RichTextBox" /> should handle a custom text element.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class TextElementEditingBehaviorAttribute : Attribute
{
	private bool _isMergeable;

	private bool _isTypographicOnly;

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.Windows.Controls.RichTextBox" /> can merge two adjacent text elements.</summary>
	/// <returns>true if a <see cref="T:System.Windows.Controls.RichTextBox" /> is free to merge adjacent custom text elements that have identical property values; otherwise, false.</returns>
	public bool IsMergeable
	{
		get
		{
			return _isMergeable;
		}
		set
		{
			_isMergeable = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the text element provides formatting on a character basis, or if the formatting applies to the entire element. </summary>
	/// <returns>true if formatting should apply to the individual characters; false if the formatting should apply to the entire element.</returns>
	public bool IsTypographicOnly
	{
		get
		{
			return _isTypographicOnly;
		}
		set
		{
			_isTypographicOnly = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Documents.TextElementEditingBehaviorAttribute" /> class. </summary>
	public TextElementEditingBehaviorAttribute()
	{
	}
}
