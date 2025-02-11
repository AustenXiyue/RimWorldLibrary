namespace System.Windows.Media.TextFormatting;

/// <summary>Represents a specialized text run that can be used to modify properties of text runs within its scope.</summary>
public abstract class TextModifier : TextRun
{
	/// <summary>Gets the <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" /> for the <see cref="T:System.Windows.Media.TextFormatting.TextModifier" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.Media.TextFormatting.CharacterBufferReference" />.</returns>
	public sealed override CharacterBufferReference CharacterBufferReference => default(CharacterBufferReference);

	/// <summary>Gets a value that indicates whether the <see cref="T:System.Windows.Media.TextFormatting.TextModifier" /> supports <see cref="T:System.Windows.FlowDirection" /> for the current scope of text.</summary>
	/// <returns>true if <see cref="T:System.Windows.Media.TextFormatting.TextModifier" /> supports <see cref="T:System.Windows.FlowDirection" /> for the current scope of text; otherwise, false.</returns>
	public abstract bool HasDirectionalEmbedding { get; }

	/// <summary>Gets the <see cref="T:System.Windows.FlowDirection" /> for the <see cref="T:System.Windows.Media.TextFormatting.TextModifier" />.</summary>
	/// <returns>A value of type <see cref="T:System.Windows.FlowDirection" />.</returns>
	public abstract FlowDirection FlowDirection { get; }

	/// <summary>Retrieves the <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> for a text run.</summary>
	/// <returns>The actual <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> to be used by the <see cref="T:System.Windows.Media.TextFormatting.TextFormatter" />, subject to further modification by <see cref="T:System.Windows.Media.TextFormatting.TextModifier" /> objects at outer scopes.</returns>
	/// <param name="properties">The <see cref="T:System.Windows.Media.TextFormatting.TextRunProperties" /> for a text run, or the return value of <see cref="M:System.Windows.Media.TextFormatting.TextModifier.ModifyProperties(System.Windows.Media.TextFormatting.TextRunProperties)" /> for a nested text modifier.</param>
	public abstract TextRunProperties ModifyProperties(TextRunProperties properties);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Media.TextFormatting.TextModifier" /> class.</summary>
	protected TextModifier()
	{
	}
}
