using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using MS.Internal.Annotations;
using MS.Internal.Annotations.Anchoring;

namespace System.Windows.Annotations;

/// <summary>Represents a set of name/value pairs that identify an item of content.</summary>
public sealed class ContentLocatorPart : INotifyPropertyChanged2, INotifyPropertyChanged, IOwnedObject
{
	private bool _owned;

	private XmlQualifiedName _type;

	private ObservableDictionary _nameValues;

	/// <summary>Gets a collection of the name/value pairs that define this part.</summary>
	/// <returns>The collection of the name/value pairs that define this <see cref="T:System.Windows.Annotations.ContentLocatorPart" />.</returns>
	public IDictionary<string, string> NameValuePairs => _nameValues;

	/// <summary>Gets the type name and namespace of the part.</summary>
	/// <returns>The type name and namespace of the part.</returns>
	public XmlQualifiedName PartType => _type;

	bool IOwnedObject.Owned
	{
		get
		{
			return _owned;
		}
		set
		{
			_owned = value;
		}
	}

	/// <summary>This event supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. </summary>
	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add
		{
			_propertyChanged += value;
		}
		remove
		{
			_propertyChanged -= value;
		}
	}

	private event PropertyChangedEventHandler _propertyChanged;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> class with a specified type name and namespace.</summary>
	/// <param name="partType">The type name and namespace for the <see cref="T:System.Windows.Annotations.ContentLocatorPart" />.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="partType" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The strings <paramref name="partType" />.<see cref="P:System.Xml.XmlQualifiedName.Name" /> or <paramref name="partType" />.<see cref="P:System.Xml.XmlQualifiedName.Namespace" /> (or both) are null or empty.</exception>
	public ContentLocatorPart(XmlQualifiedName partType)
	{
		if (partType == null)
		{
			throw new ArgumentNullException("partType");
		}
		if (string.IsNullOrEmpty(partType.Name))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "partType.Name");
		}
		if (string.IsNullOrEmpty(partType.Namespace))
		{
			throw new ArgumentException(SR.TypeNameMustBeSpecified, "partType.Namespace");
		}
		_type = partType;
		_nameValues = new ObservableDictionary();
		_nameValues.PropertyChanged += OnPropertyChanged;
	}

	/// <summary>Returns a value that indicates whether a given <see cref="T:System.Windows.Annotations.ContentLocatorPart" /> is identical to this <see cref="T:System.Windows.Annotations.ContentLocatorPart" />.</summary>
	/// <returns>true if the <see cref="P:System.Windows.Annotations.ContentLocatorPart.NameValuePairs" /> within both parts are identical; otherwise, false.</returns>
	/// <param name="obj">The part to compare for equality.</param>
	public override bool Equals(object obj)
	{
		ContentLocatorPart contentLocatorPart = obj as ContentLocatorPart;
		if (contentLocatorPart == this)
		{
			return true;
		}
		if (contentLocatorPart == null)
		{
			return false;
		}
		if (!_type.Equals(contentLocatorPart.PartType))
		{
			return false;
		}
		if (contentLocatorPart.NameValuePairs.Count != _nameValues.Count)
		{
			return false;
		}
		foreach (KeyValuePair<string, string> nameValue in _nameValues)
		{
			if (!contentLocatorPart._nameValues.TryGetValue(nameValue.Key, out var value))
			{
				return false;
			}
			if (nameValue.Value != value)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the hash code for this part.</summary>
	/// <returns>The hash code for this part.</returns>
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	/// <summary>Creates a modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorPart" />.</summary>
	/// <returns>A modifiable deep copy clone of this <see cref="T:System.Windows.Annotations.ContentLocatorPart" />.</returns>
	public object Clone()
	{
		ContentLocatorPart contentLocatorPart = new ContentLocatorPart(_type);
		foreach (KeyValuePair<string, string> nameValue in _nameValues)
		{
			contentLocatorPart.NameValuePairs.Add(nameValue.Key, nameValue.Value);
		}
		return contentLocatorPart;
	}

	internal bool Matches(ContentLocatorPart part)
	{
		bool result = false;
		_nameValues.TryGetValue("IncludeOverlaps", out var value);
		if (bool.TryParse(value, out result) && result)
		{
			if (part == this)
			{
				return true;
			}
			if (!_type.Equals(part.PartType))
			{
				return false;
			}
			TextSelectionProcessor.GetMaxMinLocatorPartValues(this, out var startOffset, out var endOffset);
			TextSelectionProcessor.GetMaxMinLocatorPartValues(part, out var startOffset2, out var endOffset2);
			if (startOffset == startOffset2 && endOffset == endOffset2)
			{
				return true;
			}
			if (startOffset == int.MinValue)
			{
				return false;
			}
			if ((startOffset2 >= startOffset && startOffset2 <= endOffset) || (startOffset2 < startOffset && endOffset2 >= startOffset))
			{
				return true;
			}
			return false;
		}
		return Equals(part);
	}

	internal string GetQueryFragment(XmlNamespaceManager namespaceManager)
	{
		bool result = false;
		_nameValues.TryGetValue("IncludeOverlaps", out var value);
		if (bool.TryParse(value, out result) && result)
		{
			return GetOverlapQueryFragment(namespaceManager);
		}
		return GetExactQueryFragment(namespaceManager);
	}

	private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (this._propertyChanged != null)
		{
			this._propertyChanged(this, new PropertyChangedEventArgs("NameValuePairs"));
		}
	}

	private string GetOverlapQueryFragment(XmlNamespaceManager namespaceManager)
	{
		string text = namespaceManager.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core");
		string text2 = namespaceManager.LookupPrefix(PartType.Namespace);
		string text3 = ((text2 == null) ? "" : (text2 + ":"));
		text3 = text3 + TextSelectionProcessor.CharacterRangeElementName.Name + "/" + text + ":Item";
		TextSelectionProcessor.GetMaxMinLocatorPartValues(this, out var startOffset, out var endOffset);
		string text4 = startOffset.ToString(NumberFormatInfo.InvariantInfo);
		string text5 = endOffset.ToString(NumberFormatInfo.InvariantInfo);
		return text3 + "[starts-with(@Name, \"Segment\") and  ((substring-before(@Value,\",\") >= " + text4 + " and substring-before(@Value,\",\") <= " + text5 + ") or   (substring-before(@Value,\",\") < " + text4 + " and substring-after(@Value,\",\") >= " + text4 + "))]";
	}

	private string GetExactQueryFragment(XmlNamespaceManager namespaceManager)
	{
		string text = namespaceManager.LookupPrefix("http://schemas.microsoft.com/windows/annotations/2003/11/core");
		string text2 = namespaceManager.LookupPrefix(PartType.Namespace);
		string text3 = ((text2 == null) ? "" : (text2 + ":"));
		text3 += PartType.Name;
		bool flag = false;
		foreach (KeyValuePair<string, string> nameValuePair in NameValuePairs)
		{
			if (flag)
			{
				text3 = text3 + "/parent::*/" + text + ":Item[";
			}
			else
			{
				flag = true;
				text3 = text3 + "/" + text + ":Item[";
			}
			text3 = text3 + "@Name=\"" + nameValuePair.Key + "\" and @Value=\"" + nameValuePair.Value + "\"]";
		}
		if (flag)
		{
			text3 += "/parent::*";
		}
		return text3;
	}
}
