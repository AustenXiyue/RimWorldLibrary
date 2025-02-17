using Unity;

namespace System.Text;

/// <summary>Provides basic information about an encoding.</summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public sealed class EncodingInfo
{
	private int iCodePage;

	private string strEncodingName;

	private string strDisplayName;

	/// <summary>Gets the code page identifier of the encoding.</summary>
	/// <returns>The code page identifier of the encoding.</returns>
	/// <filterpriority>2</filterpriority>
	public int CodePage => iCodePage;

	/// <summary>Gets the name registered with the Internet Assigned Numbers Authority (IANA) for the encoding.</summary>
	/// <returns>The IANA name for the encoding. For more information about the IANA, see www.iana.org.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name => strEncodingName;

	/// <summary>Gets the human-readable description of the encoding.</summary>
	/// <returns>The human-readable description of the encoding.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName => strDisplayName;

	internal EncodingInfo(int codePage, string name, string displayName)
	{
		iCodePage = codePage;
		strEncodingName = name;
		strDisplayName = displayName;
	}

	/// <summary>Returns a <see cref="T:System.Text.Encoding" /> object that corresponds to the current <see cref="T:System.Text.EncodingInfo" /> object.</summary>
	/// <returns>A <see cref="T:System.Text.Encoding" /> object that corresponds to the current <see cref="T:System.Text.EncodingInfo" /> object.</returns>
	/// <filterpriority>1</filterpriority>
	public Encoding GetEncoding()
	{
		return Encoding.GetEncoding(iCodePage);
	}

	/// <summary>Gets a value indicating whether the specified object is equal to the current <see cref="T:System.Text.EncodingInfo" /> object.</summary>
	/// <returns>true if <paramref name="value" /> is a <see cref="T:System.Text.EncodingInfo" /> object and is equal to the current <see cref="T:System.Text.EncodingInfo" /> object; otherwise, false.</returns>
	/// <param name="value">An object to compare to the current <see cref="T:System.Text.EncodingInfo" /> object.</param>
	/// <filterpriority>1</filterpriority>
	public override bool Equals(object value)
	{
		if (value is EncodingInfo encodingInfo)
		{
			return CodePage == encodingInfo.CodePage;
		}
		return false;
	}

	/// <summary>Returns the hash code for the current <see cref="T:System.Text.EncodingInfo" /> object.</summary>
	/// <returns>A 32-bit signed integer hash code.</returns>
	/// <filterpriority>1</filterpriority>
	public override int GetHashCode()
	{
		return CodePage;
	}

	internal EncodingInfo()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
