using System.ComponentModel;
using System.Windows.Markup;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Defines a name for text input patterns.</summary>
[ContentProperty("NameValue")]
[TypeConverter("System.Windows.Input.InputScopeNameConverter, PresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, Custom=null")]
public class InputScopeName : IAddChild
{
	private InputScopeNameValue _nameValue;

	/// <summary>Gets or sets the input scope name value which modifies how input from alternative input methods is interpreted.</summary>
	/// <returns>The input scope name value which modifies how input from alternative input methods is interpreted.</returns>
	public InputScopeNameValue NameValue
	{
		get
		{
			return _nameValue;
		}
		set
		{
			if (!IsValidInputScopeNameValue(value))
			{
				throw new ArgumentException(SR.Format(SR.InputScope_InvalidInputScopeName, "value"));
			}
			_nameValue = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScopeName" /> class.</summary>
	public InputScopeName()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.InputScopeName" /> class with the specified <see cref="P:System.Windows.Input.InputScopeName.NameValue" />.</summary>
	/// <param name="nameValue">The input scope name which modifies how input from alternative input methods is interpreted.</param>
	public InputScopeName(InputScopeNameValue nameValue)
	{
		_nameValue = nameValue;
	}

	/// <summary>Adds a child object to this <see cref="T:System.Windows.Input.InputScopeName" />.</summary>
	/// <param name="value">The object to be added as the child of this <see cref="T:System.Windows.Input.InputScopeName" />.</param>
	public void AddChild(object value)
	{
		throw new NotImplementedException();
	}

	/// <summary>Adds a text string as a child of this <see cref="T:System.Windows.Input.InputScopeName" />.</summary>
	/// <param name="name">The text added to the <see cref="T:System.Windows.Input.InputScopeName" />.</param>
	public void AddText(string name)
	{
	}

	private bool IsValidInputScopeNameValue(InputScopeNameValue name)
	{
		switch (name)
		{
		default:
			return false;
		case InputScopeNameValue.Xml:
		case InputScopeNameValue.Srgs:
		case InputScopeNameValue.RegularExpression:
		case InputScopeNameValue.PhraseList:
		case InputScopeNameValue.Default:
		case InputScopeNameValue.Url:
		case InputScopeNameValue.FullFilePath:
		case InputScopeNameValue.FileName:
		case InputScopeNameValue.EmailUserName:
		case InputScopeNameValue.EmailSmtpAddress:
		case InputScopeNameValue.LogOnName:
		case InputScopeNameValue.PersonalFullName:
		case InputScopeNameValue.PersonalNamePrefix:
		case InputScopeNameValue.PersonalGivenName:
		case InputScopeNameValue.PersonalMiddleName:
		case InputScopeNameValue.PersonalSurname:
		case InputScopeNameValue.PersonalNameSuffix:
		case InputScopeNameValue.PostalAddress:
		case InputScopeNameValue.PostalCode:
		case InputScopeNameValue.AddressStreet:
		case InputScopeNameValue.AddressStateOrProvince:
		case InputScopeNameValue.AddressCity:
		case InputScopeNameValue.AddressCountryName:
		case InputScopeNameValue.AddressCountryShortName:
		case InputScopeNameValue.CurrencyAmountAndSymbol:
		case InputScopeNameValue.CurrencyAmount:
		case InputScopeNameValue.Date:
		case InputScopeNameValue.DateMonth:
		case InputScopeNameValue.DateDay:
		case InputScopeNameValue.DateYear:
		case InputScopeNameValue.DateMonthName:
		case InputScopeNameValue.DateDayName:
		case InputScopeNameValue.Digits:
		case InputScopeNameValue.Number:
		case InputScopeNameValue.OneChar:
		case InputScopeNameValue.Password:
		case InputScopeNameValue.TelephoneNumber:
		case InputScopeNameValue.TelephoneCountryCode:
		case InputScopeNameValue.TelephoneAreaCode:
		case InputScopeNameValue.TelephoneLocalNumber:
		case InputScopeNameValue.Time:
		case InputScopeNameValue.TimeHour:
		case InputScopeNameValue.TimeMinorSec:
		case InputScopeNameValue.NumberFullWidth:
		case InputScopeNameValue.AlphanumericHalfWidth:
		case InputScopeNameValue.AlphanumericFullWidth:
		case InputScopeNameValue.CurrencyChinese:
		case InputScopeNameValue.Bopomofo:
		case InputScopeNameValue.Hiragana:
		case InputScopeNameValue.KatakanaHalfWidth:
		case InputScopeNameValue.KatakanaFullWidth:
		case InputScopeNameValue.Hanja:
			return true;
		}
	}
}
