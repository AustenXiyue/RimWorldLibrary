using System.Runtime.Serialization;
using System.Text;

namespace System.Xml;

/// <summary>Represents an entry stored in a <see cref="T:System.Xml.XmlDictionary" />.</summary>
public class XmlDictionaryString
{
	private class EmptyStringDictionary : IXmlDictionary
	{
		private XmlDictionaryString empty;

		public XmlDictionaryString EmptyString => empty;

		public EmptyStringDictionary()
		{
			empty = new XmlDictionaryString(this, string.Empty, 0);
		}

		public bool TryLookup(string value, out XmlDictionaryString result)
		{
			if (value == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
			}
			if (value.Length == 0)
			{
				result = empty;
				return true;
			}
			result = null;
			return false;
		}

		public bool TryLookup(int key, out XmlDictionaryString result)
		{
			if (key == 0)
			{
				result = empty;
				return true;
			}
			result = null;
			return false;
		}

		public bool TryLookup(XmlDictionaryString value, out XmlDictionaryString result)
		{
			if (value == null)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
			}
			if (value.Dictionary != this)
			{
				result = null;
				return false;
			}
			result = value;
			return true;
		}
	}

	internal const int MinKey = 0;

	internal const int MaxKey = 536870911;

	private IXmlDictionary dictionary;

	private string value;

	private int key;

	private byte[] buffer;

	private static EmptyStringDictionary emptyStringDictionary = new EmptyStringDictionary();

	/// <summary>Gets an <see cref="T:System.Xml.XmlDictionaryString" /> representing the empty string.</summary>
	/// <returns>An <see cref="T:System.Xml.XmlDictionaryString" /> representing the empty string.</returns>
	public static XmlDictionaryString Empty => emptyStringDictionary.EmptyString;

	/// <summary>Represents the <see cref="T:System.Xml.IXmlDictionary" /> passed to the constructor of this instance of <see cref="T:System.Xml.XmlDictionaryString" />.</summary>
	/// <returns>The <see cref="T:System.Xml.IXmlDictionary" /> for this dictionary entry.</returns>
	public IXmlDictionary Dictionary => dictionary;

	/// <summary>Gets the integer key for this instance of the class.  </summary>
	/// <returns>The integer key for this instance of the class. </returns>
	/// <filterpriority>2</filterpriority>
	public int Key => key;

	/// <summary>Gets the string value for this instance of the class.  </summary>
	/// <returns>The string value for this instance of the class. </returns>
	/// <filterpriority>2</filterpriority>
	public string Value => value;

	/// <summary>Creates an instance of this class. </summary>
	/// <param name="dictionary">The <see cref="T:System.Xml.IXmlDictionary" /> containing this instance.</param>
	/// <param name="value">The string that is the value of the dictionary entry.</param>
	/// <param name="key">The integer that is the key of the dictionary entry.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="dictionary" /> is null.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="key" /> is less than 0 or greater than <see cref="F:System.Int32.MaxValue" /> / 4.</exception>
	public XmlDictionaryString(IXmlDictionary dictionary, string value, int key)
	{
		if (dictionary == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dictionary"));
		}
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));
		}
		if (key < 0 || key > 536870911)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("key", SR.GetString("The value of this argument must fall within the range {0} to {1}.", 0, 536870911)));
		}
		this.dictionary = dictionary;
		this.value = value;
		this.key = key;
	}

	internal static string GetString(XmlDictionaryString s)
	{
		return s?.Value;
	}

	internal byte[] ToUTF8()
	{
		if (buffer == null)
		{
			buffer = Encoding.UTF8.GetBytes(value);
		}
		return buffer;
	}

	/// <summary>Displays a text representation of this object.</summary>
	/// <returns>The string value for this instance of the class. </returns>
	public override string ToString()
	{
		return value;
	}
}
