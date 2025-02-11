using System.ComponentModel;
using System.Runtime.Serialization;

namespace System.Xml;

/// <summary>Contains configurable quota values for XmlDictionaryReaders.</summary>
public sealed class XmlDictionaryReaderQuotas
{
	private bool readOnly;

	private int maxStringContentLength;

	private int maxArrayLength;

	private int maxDepth;

	private int maxNameTableCharCount;

	private int maxBytesPerRead;

	private XmlDictionaryReaderQuotaTypes modifiedQuotas;

	private const int DefaultMaxDepth = 32;

	private const int DefaultMaxStringContentLength = 8192;

	private const int DefaultMaxArrayLength = 16384;

	private const int DefaultMaxBytesPerRead = 4096;

	private const int DefaultMaxNameTableCharCount = 16384;

	private static XmlDictionaryReaderQuotas defaultQuota = new XmlDictionaryReaderQuotas(32, 8192, 16384, 4096, 16384, (XmlDictionaryReaderQuotaTypes)0);

	private static XmlDictionaryReaderQuotas maxQuota = new XmlDictionaryReaderQuotas(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, XmlDictionaryReaderQuotaTypes.MaxDepth | XmlDictionaryReaderQuotaTypes.MaxStringContentLength | XmlDictionaryReaderQuotaTypes.MaxArrayLength | XmlDictionaryReaderQuotaTypes.MaxBytesPerRead | XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount);

	/// <summary>Gets an instance of this class with all properties set to maximum values.</summary>
	/// <returns>An instance of <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> with properties set to <see cref="F:System.Int32.MaxValue" />.</returns>
	public static XmlDictionaryReaderQuotas Max => maxQuota;

	/// <summary>Gets and sets the maximum string length returned by the reader.</summary>
	/// <returns>The maximum string length returned by the reader. The default is 8192.</returns>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value, but quota values are read-only for this instance.</exception>
	/// <exception cref="T:System.ArgumentException">Trying to set the value to less than zero.</exception>
	[DefaultValue(8192)]
	public int MaxStringContentLength
	{
		get
		{
			return maxStringContentLength;
		}
		set
		{
			if (readOnly)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The '{0}' quota is readonly.", "MaxStringContentLength")));
			}
			if (value <= 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Quota must be a positive value."), "value"));
			}
			maxStringContentLength = value;
			modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxStringContentLength;
		}
	}

	/// <summary>Gets and sets the maximum allowed array length.</summary>
	/// <returns>The maximum allowed array length. The default is 16384.</returns>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value, but quota values are read-only for this instance.</exception>
	/// <exception cref="T:System.ArgumentException">Trying to set the value to less than zero.</exception>
	[DefaultValue(16384)]
	public int MaxArrayLength
	{
		get
		{
			return maxArrayLength;
		}
		set
		{
			if (readOnly)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The '{0}' quota is readonly.", "MaxArrayLength")));
			}
			if (value <= 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Quota must be a positive value."), "value"));
			}
			maxArrayLength = value;
			modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxArrayLength;
		}
	}

	/// <summary>Gets and sets the maximum allowed bytes returned for each read.</summary>
	/// <returns>The maximum allowed bytes returned for each read. The default is 4096.</returns>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value, but quota values are read-only for this instance.</exception>
	/// <exception cref="T:System.ArgumentException">Trying to set the value to less than zero.</exception>
	[DefaultValue(4096)]
	public int MaxBytesPerRead
	{
		get
		{
			return maxBytesPerRead;
		}
		set
		{
			if (readOnly)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The '{0}' quota is readonly.", "MaxBytesPerRead")));
			}
			if (value <= 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Quota must be a positive value."), "value"));
			}
			maxBytesPerRead = value;
			modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxBytesPerRead;
		}
	}

	/// <summary>Gets and sets the maximum nested node depth.</summary>
	/// <returns>The maximum nested node depth. The default is 32;</returns>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value and quota values are read-only for this instance.</exception>
	/// <exception cref="T:System.ArgumentException">Trying to set the value is less than zero.</exception>
	[DefaultValue(32)]
	public int MaxDepth
	{
		get
		{
			return maxDepth;
		}
		set
		{
			if (readOnly)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The '{0}' quota is readonly.", "MaxDepth")));
			}
			if (value <= 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Quota must be a positive value."), "value"));
			}
			maxDepth = value;
			modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxDepth;
		}
	}

	/// <summary>Gets and sets the maximum characters allowed in a table name.</summary>
	/// <returns>The maximum characters allowed in a table name. The default is 16384.</returns>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value, but quota values are read-only for this instance.</exception>
	/// <exception cref="T:System.ArgumentException">Trying to set the value to less than zero.</exception>
	[DefaultValue(16384)]
	public int MaxNameTableCharCount
	{
		get
		{
			return maxNameTableCharCount;
		}
		set
		{
			if (readOnly)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("The '{0}' quota is readonly.", "MaxNameTableCharCount")));
			}
			if (value <= 0)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(SR.GetString("Quota must be a positive value."), "value"));
			}
			maxNameTableCharCount = value;
			modifiedQuotas |= XmlDictionaryReaderQuotaTypes.MaxNameTableCharCount;
		}
	}

	/// <summary>Gets the modified quotas for the <see cref="T:System.Xml.XmlDictionaryReaderQuotas" />.</summary>
	/// <returns>The modified quotas for the <see cref="T:System.Xml.XmlDictionaryReaderQuotas" />.</returns>
	public XmlDictionaryReaderQuotaTypes ModifiedQuotas => modifiedQuotas;

	/// <summary>Creates a new instance of this class. </summary>
	public XmlDictionaryReaderQuotas()
	{
		defaultQuota.CopyTo(this);
	}

	private XmlDictionaryReaderQuotas(int maxDepth, int maxStringContentLength, int maxArrayLength, int maxBytesPerRead, int maxNameTableCharCount, XmlDictionaryReaderQuotaTypes modifiedQuotas)
	{
		this.maxDepth = maxDepth;
		this.maxStringContentLength = maxStringContentLength;
		this.maxArrayLength = maxArrayLength;
		this.maxBytesPerRead = maxBytesPerRead;
		this.maxNameTableCharCount = maxNameTableCharCount;
		this.modifiedQuotas = modifiedQuotas;
		MakeReadOnly();
	}

	/// <summary>Sets the properties on a passed-in quotas instance, based on the values in this instance.</summary>
	/// <param name="quotas">The <see cref="T:System.Xml.XmlDictionaryReaderQuotas" /> instance to which to copy values.</param>
	/// <exception cref="T:System.InvalidOperationException">Trying to set the value, but quota values are read-only for the passed in instance.</exception>
	/// <exception cref="T:System.ArgumentNullException">Passed in target<paramref name=" quotas" /> is null.</exception>
	/// <filterpriority>2</filterpriority>
	public void CopyTo(XmlDictionaryReaderQuotas quotas)
	{
		if (quotas == null)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("quotas"));
		}
		if (quotas.readOnly)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Cannot copy XmlDictionaryReaderQuotas. Target is readonly.")));
		}
		InternalCopyTo(quotas);
	}

	internal void InternalCopyTo(XmlDictionaryReaderQuotas quotas)
	{
		quotas.maxStringContentLength = maxStringContentLength;
		quotas.maxArrayLength = maxArrayLength;
		quotas.maxDepth = maxDepth;
		quotas.maxNameTableCharCount = maxNameTableCharCount;
		quotas.maxBytesPerRead = maxBytesPerRead;
		quotas.modifiedQuotas = modifiedQuotas;
	}

	internal void MakeReadOnly()
	{
		readOnly = true;
	}
}
