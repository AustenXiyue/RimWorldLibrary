using System.Windows.Markup;

namespace System.Windows.Input;

/// <summary>Represents a suggested input text pattern.</summary>
[ContentProperty("Name")]
public class InputScopePhrase : IAddChild
{
	private string _phraseName;

	/// <summary>Gets or sets a descriptive name associated with the text input pattern for this <see cref="T:System.Windows.Input.InputScopePhrase" />.</summary>
	/// <returns>A string containing the descriptive name for this <see cref="T:System.Windows.Input.InputScopePhrase" />.</returns>
	public string Name
	{
		get
		{
			return _phraseName;
		}
		set
		{
			_phraseName = value;
		}
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	public InputScopePhrase()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScopePhrase" /> class, taking a string specifying the <see cref="P:System.Windows.Input.InputScopePhrase.Name" /> of the input scope phrase.</summary>
	/// <param name="name">A string specifying the initial value for the <see cref="P:System.Windows.Input.InputScopePhrase.Name" /> property.  This value cannot be null.</param>
	/// <exception cref="T:System.ArgumentNullException">Raised if <paramref name="name" /> is null.</exception>
	public InputScopePhrase(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_phraseName = name;
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="value">An object to add as a child. </param>
	public void AddChild(object value)
	{
		throw new NotImplementedException();
	}

	/// <summary>This type or member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	/// <param name="name">A string to add. </param>
	public void AddText(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		_phraseName = name;
	}
}
