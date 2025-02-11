namespace System.Windows.Media.TextFormatting;

internal sealed class TextModifierScope
{
	private TextModifierScope _parentScope;

	private TextModifier _modifier;

	private int _cp;

	public TextModifierScope ParentScope => _parentScope;

	public TextModifier TextModifier => _modifier;

	public int TextSourceCharacterIndex => _cp;

	internal TextModifierScope(TextModifierScope parentScope, TextModifier modifier, int cp)
	{
		_parentScope = parentScope;
		_modifier = modifier;
		_cp = cp;
	}

	internal TextRunProperties ModifyProperties(TextRunProperties properties)
	{
		for (TextModifierScope textModifierScope = this; textModifierScope != null; textModifierScope = textModifierScope._parentScope)
		{
			properties = textModifierScope._modifier.ModifyProperties(properties);
		}
		return properties;
	}

	internal TextModifierScope CloneStack()
	{
		TextModifierScope textModifierScope = new TextModifierScope(null, _modifier, _cp);
		TextModifierScope textModifierScope2 = textModifierScope;
		for (TextModifierScope parentScope = _parentScope; parentScope != null; parentScope = parentScope._parentScope)
		{
			textModifierScope2._parentScope = new TextModifierScope(null, parentScope._modifier, parentScope._cp);
			textModifierScope2 = textModifierScope2._parentScope;
		}
		return textModifierScope;
	}
}
