using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Xml;

/// <summary>Enables optimized strings to be managed in a dynamic way.</summary>
/// <filterpriority>2</filterpriority>
public class XmlBinaryReaderSession : IXmlDictionary
{
	private const int MaxArrayEntries = 2048;

	private XmlDictionaryString[] strings;

	private Dictionary<int, XmlDictionaryString> stringDict;

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.XmlBinaryReaderSession" /> class.</summary>
	public XmlBinaryReaderSession()
	{
	}

	/// <summary>Creates an <see cref="T:System.Xml.XmlDictionaryString" /> from the input parameters and adds it to an internal collection.</summary>
	/// <returns>The newly created <see cref="T:System.Xml.XmlDictionaryString" /> that is added to an internal collection.</returns>
	/// <param name="id">The key value.</param>
	/// <param name="value">The value.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="id" /> is less than zero.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">An entry with key = <paramref name="id" /> already exists.</exception>
	public XmlDictionaryString Add(int id, string value)
	{
		if (id < 0)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException(SR.GetString("ID must be >= 0.")));
		}
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
		}
		if (TryLookup(id, out var result))
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("ID already defined.")));
		}
		result = new XmlDictionaryString(this, value, id);
		if (id >= 2048)
		{
			if (stringDict == null)
			{
				stringDict = new Dictionary<int, XmlDictionaryString>();
			}
			stringDict.Add(id, result);
		}
		else
		{
			if (strings == null)
			{
				strings = new XmlDictionaryString[Math.Max(id + 1, 16)];
			}
			else if (id >= strings.Length)
			{
				XmlDictionaryString[] destinationArray = new XmlDictionaryString[Math.Min(Math.Max(id + 1, strings.Length * 2), 2048)];
				Array.Copy(strings, destinationArray, strings.Length);
				strings = destinationArray;
			}
			strings[id] = result;
		}
		return result;
	}

	/// <summary>Checks whether the internal collection contains an entry matching a key.</summary>
	/// <returns>true if an entry matching the <paramref name="key" /> was found; otherwise, false.</returns>
	/// <param name="key">The key to search on.</param>
	/// <param name="result">When this method returns, contains a string if an entry is found; otherwise, null. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="key" /> is null.</exception>
	public bool TryLookup(int key, out XmlDictionaryString result)
	{
		if (strings != null && key >= 0 && key < strings.Length)
		{
			result = strings[key];
			return result != null;
		}
		if (key >= 2048 && stringDict != null)
		{
			return stringDict.TryGetValue(key, out result);
		}
		result = null;
		return false;
	}

	/// <summary>Checks whether the internal collection contains an entry matching a value.</summary>
	/// <returns>true if an entry matching the <paramref name="value" /> was found; otherwise, false.</returns>
	/// <param name="value">The value to search for.</param>
	/// <param name="result">When this method returns, contains a string if an entry is found; otherwise, null. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
	public bool TryLookup(string value, out XmlDictionaryString result)
	{
		if (value == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
		}
		if (strings != null)
		{
			for (int i = 0; i < strings.Length; i++)
			{
				XmlDictionaryString xmlDictionaryString = strings[i];
				if (xmlDictionaryString != null && xmlDictionaryString.Value == value)
				{
					result = xmlDictionaryString;
					return true;
				}
			}
		}
		if (stringDict != null)
		{
			foreach (XmlDictionaryString value2 in stringDict.Values)
			{
				if (value2.Value == value)
				{
					result = value2;
					return true;
				}
			}
		}
		result = null;
		return false;
	}

	/// <summary>Checks whether the internal collection contains an entry matching a value.</summary>
	/// <returns>true if an entry matching the <paramref name="value" /> was found; otherwise, false.</returns>
	/// <param name="value">The value to search for.</param>
	/// <param name="result">When this method returns, contains a string if an entry is found; otherwise, null. This parameter is passed uninitialized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="value" /> is null.</exception>
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

	/// <summary>Clears the internal collection of all contents.</summary>
	public void Clear()
	{
		if (strings != null)
		{
			Array.Clear(strings, 0, strings.Length);
		}
		if (stringDict != null)
		{
			stringDict.Clear();
		}
	}
}
