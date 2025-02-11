using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace System.Windows.Input;

/// <summary>Represents information related to the scope of data provided by an input method.</summary>
[TypeConverter("System.Windows.Input.InputScopeConverter, PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
public class InputScope
{
	private IList<InputScopeName> _scopeNames = new List<InputScopeName>();

	private IList<InputScopePhrase> _phraseList = new List<InputScopePhrase>();

	private string _regexString;

	private string _srgsMarkup;

	/// <summary>Gets or sets the input scope name.</summary>
	/// <returns>A member of the <see cref="T:System.Windows.Input.InputScopeName" /> enumeration specifying a name for this input scope.The default value is <see cref="F:System.Windows.Input.InputScopeNameValue.Default" />.</returns>
	/// <exception cref="T:System.ArgumentException">Raised when an attempt is made to set this property to any value other than a valid member of the <see cref="T:System.Windows.Input.InputScopeName" /> enumeration.</exception>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public IList Names => (IList)_scopeNames;

	/// <summary>Gets or sets a string that specifies any Speech Recognition Grammar Specification (SRGS) markup to be used as a suggested input pattern by input processors.</summary>
	/// <returns>A string that specifies any SRGS markup to be used as a suggested input pattern by input processors.This property has no default value.</returns>
	/// <exception cref="T:System.ArgumentNullException">Raised when an attempt is made to set this property to null.</exception>
	[DefaultValue(null)]
	public string SrgsMarkup
	{
		get
		{
			return _srgsMarkup;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_srgsMarkup = value;
		}
	}

	/// <summary>Gets or sets a regular expression to be used as a suggested text input pattern by input processors.</summary>
	/// <returns>A string that defines a regular expression to be used as a suggested text input pattern by input processors.This property has no default value.</returns>
	/// <exception cref="T:System.ArgumentNullException">Raised when an attempt is made to set this property to null.</exception>
	[DefaultValue(null)]
	public string RegularExpression
	{
		get
		{
			return _regexString;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			_regexString = value;
		}
	}

	/// <summary>Gets a collection of phrases to be used as suggested input patterns by input processors.</summary>
	/// <returns>An object containing a collection of phrases to be used as suggested input patterns by input processors.  This object implements the <see cref="T:System.Collections.IList" /> interface.This property has no default value.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public IList PhraseList => (IList)_phraseList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScope" /> class.</summary>
	public InputScope()
	{
	}
}
