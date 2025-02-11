using System.ComponentModel;

namespace System.Windows.Data;

/// <summary>Declares a mapping between a uniform resource identifier (URI) and a prefix.</summary>
public class XmlNamespaceMapping : ISupportInitialize
{
	private string _prefix;

	private Uri _uri;

	private bool _initializing;

	/// <summary>Gets or sets the prefix to use in Extensible Application Markup Language (XAML).</summary>
	/// <returns>The prefix to associate with the URI. The default is an empty string("").</returns>
	public string Prefix
	{
		get
		{
			return _prefix;
		}
		set
		{
			if (!_initializing)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsInitializeOnly, "Prefix", GetType().Name));
			}
			if (_prefix != null && _prefix != value)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsImmutable, "Prefix", GetType().Name));
			}
			_prefix = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Uri" /> of the namespace for which to create a mapping.</summary>
	/// <returns>The URI of the namespace. The default is null.</returns>
	public Uri Uri
	{
		get
		{
			return _uri;
		}
		set
		{
			if (!_initializing)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsInitializeOnly, "Uri", GetType().Name));
			}
			if (_uri != null && _uri != value)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsImmutable, "Uri", GetType().Name));
			}
			_uri = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> class.</summary>
	public XmlNamespaceMapping()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> class with the specified prefix and uniform resource identifier (URI).</summary>
	/// <param name="prefix">The prefix to use in Extensible Application Markup Language (XAML).</param>
	/// <param name="uri">The <see cref="T:System.Uri" /> of the namespace to create the mapping for.</param>
	public XmlNamespaceMapping(string prefix, Uri uri)
	{
		_prefix = prefix;
		_uri = uri;
	}

	/// <summary>Returns a value that indicates whether this <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> is equivalent to the specified instance.</summary>
	/// <returns>true if the two instances are the same; otherwise, false.</returns>
	/// <param name="obj">The instance to compare for equality.</param>
	public override bool Equals(object obj)
	{
		return this == obj as XmlNamespaceMapping;
	}

	/// <summary>Performs equality comparison by value.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects are the same; otherwise, false.</returns>
	/// <param name="mappingA">The first <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to compare.</param>
	/// <param name="mappingB">The second <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to compare.</param>
	public static bool operator ==(XmlNamespaceMapping mappingA, XmlNamespaceMapping mappingB)
	{
		if ((object)mappingA == null)
		{
			return (object)mappingB == null;
		}
		if ((object)mappingB == null)
		{
			return false;
		}
		if (mappingA.Prefix == mappingB.Prefix)
		{
			return mappingA.Uri == mappingB.Uri;
		}
		return false;
	}

	/// <summary>Performs inequality comparison by value.</summary>
	/// <returns>true if the two <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> objects are not the same; otherwise, false.</returns>
	/// <param name="mappingA">The first <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to compare.</param>
	/// <param name="mappingB">The second <see cref="T:System.Windows.Data.XmlNamespaceMapping" /> object to compare.</param>
	public static bool operator !=(XmlNamespaceMapping mappingA, XmlNamespaceMapping mappingB)
	{
		return !(mappingA == mappingB);
	}

	/// <summary>Returns the hash code for this <see cref="T:System.Windows.Data.XmlNamespaceMapping" />.</summary>
	/// <returns>The hash code for this <see cref="T:System.Windows.Data.XmlNamespaceMapping" />.</returns>
	public override int GetHashCode()
	{
		int num = 0;
		if (_prefix != null)
		{
			num = _prefix.GetHashCode();
		}
		if (_uri != null)
		{
			return num + _uri.GetHashCode();
		}
		return num;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.BeginInit()
	{
		_initializing = true;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.EndInit()
	{
		if (_prefix == null)
		{
			throw new InvalidOperationException(SR.Format(SR.PropertyMustHaveValue, "Prefix", GetType().Name));
		}
		if (_uri == null)
		{
			throw new InvalidOperationException(SR.Format(SR.PropertyMustHaveValue, "Uri", GetType().Name));
		}
		_initializing = false;
	}
}
