using System.Windows.Input;

namespace System.Windows.Documents;

/// <summary>  Represents a composition related to text input. You can use this class to find the text position of the composition or the result string.</summary>
public sealed class FrameworkRichTextComposition : FrameworkTextComposition
{
	/// <summary> Gets the start position of the result text of the text input. </summary>
	/// <returns>The start position of the result text of the text input.</returns>
	public TextPointer ResultStart
	{
		get
		{
			if (base._ResultStart != null)
			{
				return (TextPointer)base._ResultStart.GetFrozenPointer(LogicalDirection.Backward);
			}
			return null;
		}
	}

	/// <summary> Gets the end position of the result text of the text input. </summary>
	/// <returns>The end position of the result text of the text input.</returns>
	public TextPointer ResultEnd
	{
		get
		{
			if (base._ResultEnd != null)
			{
				return (TextPointer)base._ResultEnd.GetFrozenPointer(LogicalDirection.Forward);
			}
			return null;
		}
	}

	/// <summary> Gets the start position of the current composition text. </summary>
	/// <returns>The start position of the current composition text.</returns>
	public TextPointer CompositionStart
	{
		get
		{
			if (base._CompositionStart != null)
			{
				return (TextPointer)base._CompositionStart.GetFrozenPointer(LogicalDirection.Backward);
			}
			return null;
		}
	}

	/// <summary> Gets the end position of the current composition text. </summary>
	/// <returns>The end position of the current composition text.</returns>
	public TextPointer CompositionEnd
	{
		get
		{
			if (base._CompositionEnd != null)
			{
				return (TextPointer)base._CompositionEnd.GetFrozenPointer(LogicalDirection.Forward);
			}
			return null;
		}
	}

	internal FrameworkRichTextComposition(InputManager inputManager, IInputElement source, object owner)
		: base(inputManager, source, owner)
	{
	}
}
