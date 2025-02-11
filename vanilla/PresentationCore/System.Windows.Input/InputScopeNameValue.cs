namespace System.Windows.Input;

/// <summary>Specifies the input scope name which modifies how input from alternative input methods is interpreted.</summary>
public enum InputScopeNameValue
{
	/// <summary>The default handling of input commands.</summary>
	Default = 0,
	/// <summary>The text input pattern for a Uniform Resource Locator (URL).</summary>
	Url = 1,
	/// <summary>The text input pattern for the full path of a file.</summary>
	FullFilePath = 2,
	/// <summary>The text input pattern for a file name.</summary>
	FileName = 3,
	/// <summary>The text input pattern for an email user name.</summary>
	EmailUserName = 4,
	/// <summary>The text input pattern for a Simple Mail Transfer Protocol (SMTP) email address.</summary>
	EmailSmtpAddress = 5,
	/// <summary>The text input pattern for a log on name.</summary>
	LogOnName = 6,
	/// <summary>The text input pattern for a person's full name.</summary>
	PersonalFullName = 7,
	/// <summary>The text input pattern for the prefix of a person's name.</summary>
	PersonalNamePrefix = 8,
	/// <summary>The text input pattern for a person's given name.</summary>
	PersonalGivenName = 9,
	/// <summary>The text input pattern for a person's middle name.</summary>
	PersonalMiddleName = 10,
	/// <summary>The text input pattern for a person's surname.</summary>
	PersonalSurname = 11,
	/// <summary>The text input pattern for the suffix of a person's name.</summary>
	PersonalNameSuffix = 12,
	/// <summary>The text input pattern for a postal address.</summary>
	PostalAddress = 13,
	/// <summary>The text input pattern for a postal code.</summary>
	PostalCode = 14,
	/// <summary>The text input pattern for a street address.</summary>
	AddressStreet = 15,
	/// <summary>The text input pattern for a state or province.</summary>
	AddressStateOrProvince = 16,
	/// <summary>The text input pattern for a city address.</summary>
	AddressCity = 17,
	/// <summary>The text input pattern for the name of a country.</summary>
	AddressCountryName = 18,
	/// <summary>The text input pattern for the abbreviated name of a country.</summary>
	AddressCountryShortName = 19,
	/// <summary>The text input pattern for amount and symbol of currency.</summary>
	CurrencyAmountAndSymbol = 20,
	/// <summary>The text input pattern for amount of currency.</summary>
	CurrencyAmount = 21,
	/// <summary>The text input pattern for a calendar date.</summary>
	Date = 22,
	/// <summary>The text input pattern for the numeric month in a calendar date.</summary>
	DateMonth = 23,
	/// <summary>The text input pattern for the numeric day in a calendar date.</summary>
	DateDay = 24,
	/// <summary>The text input pattern for the year in a calendar date.</summary>
	DateYear = 25,
	/// <summary>The text input pattern for the name of the month in a calendar date.</summary>
	DateMonthName = 26,
	/// <summary>The text input pattern for the name of the day in a calendar date.</summary>
	DateDayName = 27,
	/// <summary>The text input pattern for digits.</summary>
	Digits = 28,
	/// <summary>The text input pattern for a number.</summary>
	Number = 29,
	/// <summary>The text input pattern for one character.</summary>
	OneChar = 30,
	/// <summary>The text input pattern for a password.</summary>
	Password = 31,
	/// <summary>The text input pattern for a telephone number.</summary>
	TelephoneNumber = 32,
	/// <summary>The text input pattern for a telephone country code.</summary>
	TelephoneCountryCode = 33,
	/// <summary>The text input pattern for a telephone area code.</summary>
	TelephoneAreaCode = 34,
	/// <summary>The text input pattern for a telephone local number.</summary>
	TelephoneLocalNumber = 35,
	/// <summary>The text input pattern for the time.</summary>
	Time = 36,
	/// <summary>The text input pattern for the hour of the time.</summary>
	TimeHour = 37,
	/// <summary>The text input pattern for the minutes or seconds of time.</summary>
	TimeMinorSec = 38,
	/// <summary>The text input pattern for a full-width number.</summary>
	NumberFullWidth = 39,
	/// <summary>The text input pattern for alphanumeric half-width characters.</summary>
	AlphanumericHalfWidth = 40,
	/// <summary>The text input pattern for alphanumeric full-width characters.</summary>
	AlphanumericFullWidth = 41,
	/// <summary>The text input pattern for Chinese currency.</summary>
	CurrencyChinese = 42,
	/// <summary>The text input pattern for the Bopomofo Mandarin Chinese phonetic transcription system.</summary>
	Bopomofo = 43,
	/// <summary>The text input pattern for the Hiragana writing system.</summary>
	Hiragana = 44,
	/// <summary>The text input pattern for half-width Katakana characters.</summary>
	KatakanaHalfWidth = 45,
	/// <summary>The text input pattern for full-width Katakana characters.</summary>
	KatakanaFullWidth = 46,
	/// <summary>The text input pattern for Hanja characters.</summary>
	Hanja = 47,
	/// <summary>The text input pattern for a phrase list.</summary>
	PhraseList = -1,
	/// <summary>The text input pattern for a regular expression.</summary>
	RegularExpression = -2,
	/// <summary>The text input pattern for the Speech Recognition Grammar Specification (SRGS).</summary>
	Srgs = -3,
	/// <summary>The text input pattern for XML.</summary>
	Xml = -4
}
