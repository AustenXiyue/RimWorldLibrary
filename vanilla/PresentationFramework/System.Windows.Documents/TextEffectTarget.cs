using System.Windows.Media;
using MS.Internal.Text;

namespace System.Windows.Documents;

/// <summary>Result from using <see cref="T:System.Windows.Documents.TextEffectResolver" /> to set an effect on text. This consists of the <see cref="T:System.Windows.Media.TextEffect" /> created and the <see cref="T:System.Windows.DependencyObject" /> to which the <see cref="T:System.Windows.Media.TextEffect" /> should be set. </summary>
public class TextEffectTarget
{
	private DependencyObject _element;

	private TextEffect _effect;

	/// <summary> Gets the <see cref="T:System.Windows.DependencyObject" /> that the <see cref="T:System.Windows.Media.TextEffect" /> is targeting. </summary>
	/// <returns>The <see cref="T:System.Windows.DependencyObject" /> that the <see cref="T:System.Windows.Media.TextEffect" /> is targeting. </returns>
	public DependencyObject Element => _element;

	/// <summary> Gets the <see cref="T:System.Windows.Media.TextEffect" /> of the <see cref="T:System.Windows.Documents.TextEffectTarget" />. </summary>
	/// <returns>The <see cref="T:System.Windows.Media.TextEffect" /> of the <see cref="T:System.Windows.Documents.TextEffectTarget" />.</returns>
	public TextEffect TextEffect => _effect;

	/// <summary>Gets a value that determines whether the text effect is enabled on the target element </summary>
	/// <returns>true if the <see cref="T:System.Windows.Media.TextEffect" />is enabled on the target; otherwise, false.</returns>
	public bool IsEnabled
	{
		get
		{
			TextEffectCollection textEffects = DynamicPropertyReader.GetTextEffects(_element);
			if (textEffects != null)
			{
				for (int i = 0; i < textEffects.Count; i++)
				{
					if (textEffects[i] == _effect)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	internal TextEffectTarget(DependencyObject element, TextEffect effect)
	{
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		if (effect == null)
		{
			throw new ArgumentNullException("effect");
		}
		_element = element;
		_effect = effect;
	}

	/// <summary>Enables the <see cref="T:System.Windows.Media.TextEffect" /> on the target text. </summary>
	public void Enable()
	{
		TextEffectCollection textEffectCollection = DynamicPropertyReader.GetTextEffects(_element);
		if (textEffectCollection == null)
		{
			textEffectCollection = new TextEffectCollection();
			_element.SetValue(TextElement.TextEffectsProperty, textEffectCollection);
		}
		for (int i = 0; i < textEffectCollection.Count; i++)
		{
			if (textEffectCollection[i] == _effect)
			{
				return;
			}
		}
		textEffectCollection.Add(_effect);
	}

	/// <summary> Disables the <see cref="T:System.Windows.Media.TextEffect" /> on the effect target. </summary>
	public void Disable()
	{
		TextEffectCollection textEffects = DynamicPropertyReader.GetTextEffects(_element);
		if (textEffects == null)
		{
			return;
		}
		for (int i = 0; i < textEffects.Count; i++)
		{
			if (textEffects[i] == _effect)
			{
				textEffects.RemoveAt(i);
				break;
			}
		}
	}
}
