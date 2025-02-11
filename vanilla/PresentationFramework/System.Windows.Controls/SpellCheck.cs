using System.Collections;
using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using MS.Internal;

namespace System.Windows.Controls;

/// <summary>Provides real-time spell-checking functionality to text-editing controls, such as <see cref="T:System.Windows.Controls.TextBox" /> and <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
public sealed class SpellCheck
{
	internal class DictionaryCollectionFactory : DefaultValueFactory
	{
		internal override object DefaultValue => null;

		internal DictionaryCollectionFactory()
		{
		}

		internal override object CreateDefaultValue(DependencyObject owner, DependencyProperty property)
		{
			return new CustomDictionarySources(owner as TextBoxBase);
		}
	}

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.SpellCheck.IsEnabled" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.SpellCheck.IsEnabled" /> dependency property.</returns>
	public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SpellCheck), new FrameworkPropertyMetadata(OnIsEnabledChanged));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.SpellCheck.SpellingReform" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.SpellCheck.SpellingReform" /> dependency property.</returns>
	public static readonly DependencyProperty SpellingReformProperty = DependencyProperty.RegisterAttached("SpellingReform", typeof(SpellingReform), typeof(SpellCheck), new FrameworkPropertyMetadata((Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName == "de") ? SpellingReform.Postreform : SpellingReform.PreAndPostreform, OnSpellingReformChanged));

	private static readonly DependencyPropertyKey CustomDictionariesPropertyKey = DependencyProperty.RegisterAttachedReadOnly("CustomDictionaries", typeof(IList), typeof(SpellCheck), new FrameworkPropertyMetadata(new DictionaryCollectionFactory()));

	/// <summary>Identifies the <see cref="P:System.Windows.Controls.SpellCheck.CustomDictionaries" /> dependency property.</summary>
	/// <returns>The identifier for the <see cref="P:System.Windows.Controls.SpellCheck.CustomDictionaries" /> dependency property.</returns>
	public static readonly DependencyProperty CustomDictionariesProperty = CustomDictionariesPropertyKey.DependencyProperty;

	private readonly TextBoxBase _owner;

	/// <summary>Gets or sets a value that determines whether the spelling checker is enabled on this text-editing control, such as <see cref="T:System.Windows.Controls.TextBox" /> or <see cref="T:System.Windows.Controls.RichTextBox" />. </summary>
	/// <returns>true if the spelling checker is enabled on the control; otherwise, false. The default value is false.</returns>
	public bool IsEnabled
	{
		get
		{
			return (bool)_owner.GetValue(IsEnabledProperty);
		}
		set
		{
			_owner.SetValue(IsEnabledProperty, value);
		}
	}

	/// <summary>Gets or sets the spelling reform rules that are used by the spelling checker. </summary>
	/// <returns>The spelling reform rules that are used by the spelling checker. The default value is <see cref="F:System.Windows.Controls.SpellingReform.PreAndPostreform" /> for French and <see cref="F:System.Windows.Controls.SpellingReform.Postreform" /> for German.</returns>
	public SpellingReform SpellingReform
	{
		get
		{
			return (SpellingReform)_owner.GetValue(SpellingReformProperty);
		}
		set
		{
			_owner.SetValue(SpellingReformProperty, value);
		}
	}

	/// <summary>Gets the collection of lexicon file locations that are used for custom spell checking.</summary>
	/// <returns>The collection of lexicon file locations.</returns>
	public IList CustomDictionaries => (IList)_owner.GetValue(CustomDictionariesProperty);

	internal SpellCheck(TextBoxBase owner)
	{
		_owner = owner;
	}

	/// <summary>Enables or disables the spelling checker on the specified text-editing control, such as <see cref="T:System.Windows.Controls.TextBox" /> or <see cref="T:System.Windows.Controls.RichTextBox" />.</summary>
	/// <param name="textBoxBase">The text-editing control on which to enable or disable the spelling checker. Example controls include <see cref="T:System.Windows.Controls.TextBox" /> and <see cref="T:System.Windows.Controls.RichTextBox" />.</param>
	/// <param name="value">A Boolean value that specifies whether the spelling checker is enabled on the text-editing control.</param>
	public static void SetIsEnabled(TextBoxBase textBoxBase, bool value)
	{
		if (textBoxBase == null)
		{
			throw new ArgumentNullException("textBoxBase");
		}
		textBoxBase.SetValue(IsEnabledProperty, value);
	}

	/// <summary>Returns a value that indicates whether the spelling checker is enabled on the specified text-editing control.</summary>
	/// <returns>true if the spelling checker is enabled on the text-editing control; otherwise, false.</returns>
	/// <param name="textBoxBase">The text-editing control to check. Example controls include <see cref="T:System.Windows.Controls.TextBox" /> and <see cref="T:System.Windows.Controls.RichTextBox" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="textBoxBase " />is null.</exception>
	public static bool GetIsEnabled(TextBoxBase textBoxBase)
	{
		if (textBoxBase == null)
		{
			throw new ArgumentNullException("textBoxBase");
		}
		return (bool)textBoxBase.GetValue(IsEnabledProperty);
	}

	/// <summary>Determines the spelling reform rules that are used by the spelling checker. </summary>
	/// <param name="textBoxBase">The text-editing control to which the spelling checker is applied. Example controls include <see cref="T:System.Windows.Controls.TextBox" /> and <see cref="T:System.Windows.Controls.RichTextBox" />.</param>
	/// <param name="value">The <see cref="P:System.Windows.Controls.SpellCheck.SpellingReform" /> value that determines the spelling reform rules.</param>
	public static void SetSpellingReform(TextBoxBase textBoxBase, SpellingReform value)
	{
		if (textBoxBase == null)
		{
			throw new ArgumentNullException("textBoxBase");
		}
		textBoxBase.SetValue(SpellingReformProperty, value);
	}

	/// <summary>Gets the collection of lexicon file locations that are used for custom spelling checkers on a specified text-editing control. </summary>
	/// <returns>The collection of lexicon file locations.</returns>
	/// <param name="textBoxBase">The text-editing control whose collection of lexicon files is retrieved.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="textBoxBase " />is null.</exception>
	public static IList GetCustomDictionaries(TextBoxBase textBoxBase)
	{
		if (textBoxBase == null)
		{
			throw new ArgumentNullException("textBoxBase");
		}
		return (IList)textBoxBase.GetValue(CustomDictionariesProperty);
	}

	private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (!(d is TextBoxBase element))
		{
			return;
		}
		TextEditor textEditor = TextEditor._GetTextEditor(element);
		if (textEditor != null)
		{
			textEditor.SetSpellCheckEnabled((bool)e.NewValue);
			if ((bool)e.NewValue != (bool)e.OldValue)
			{
				textEditor.SetCustomDictionaries((bool)e.NewValue);
			}
		}
	}

	private static void OnSpellingReformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is TextBoxBase element)
		{
			TextEditor._GetTextEditor(element)?.SetSpellingReform((SpellingReform)e.NewValue);
		}
	}
}
