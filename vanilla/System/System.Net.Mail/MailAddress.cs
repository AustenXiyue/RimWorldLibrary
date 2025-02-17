using System.Text;

namespace System.Net.Mail;

/// <summary>Represents the address of an electronic mail sender or recipient.</summary>
public class MailAddress
{
	private string address;

	private string displayName;

	private string host;

	private string user;

	private string to_string;

	/// <summary>Gets the e-mail address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the e-mail address.</returns>
	public string Address => address;

	/// <summary>Gets the display name composed from the display name and address information specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the display name; otherwise, <see cref="F:System.String.Empty" /> ("") if no display name information was specified when this instance was created.</returns>
	public string DisplayName
	{
		get
		{
			if (displayName == null)
			{
				return string.Empty;
			}
			return displayName;
		}
	}

	/// <summary>Gets the host portion of the address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the host computer that accepts e-mail for the <see cref="P:System.Net.Mail.MailAddress.User" /> property.</returns>
	public string Host => host;

	/// <summary>Gets the user information from the address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the user name portion of the <see cref="P:System.Net.Mail.MailAddress.Address" />.</returns>
	public string User => user;

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address. </summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.</exception>
	public MailAddress(string address)
		: this(address, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address and display name.</summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <param name="displayName">A <see cref="T:System.String" /> that contains the display name associated with <paramref name="address" />. This parameter can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.-or-<paramref name="address" /> contains non-ASCII characters.</exception>
	public MailAddress(string address, string displayName)
		: this(address, displayName, Encoding.UTF8)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address, display name, and encoding.</summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <param name="displayName">A <see cref="T:System.String" /> that contains the display name associated with <paramref name="address" />.</param>
	/// <param name="displayNameEncoding">The <see cref="T:System.Text.Encoding" /> that defines the character set used for <paramref name="displayName" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.-or-<paramref name="displayName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").-or-<paramref name="displayName" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.-or-<paramref name="address" /> contains non-ASCII characters.</exception>
	[System.MonoTODO("We don't do anything with displayNameEncoding")]
	public MailAddress(string address, string displayName, Encoding displayNameEncoding)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address.Length == 0)
		{
			throw new ArgumentException("address");
		}
		if (displayName != null)
		{
			this.displayName = displayName.Trim();
		}
		ParseAddress(address);
	}

	private void ParseAddress(string address)
	{
		address = address.Trim();
		int num = address.IndexOf('"');
		if (num != -1)
		{
			if (num != 0 || address.Length == 1)
			{
				throw CreateFormatException();
			}
			int num2 = address.LastIndexOf('"');
			if (num2 == num)
			{
				throw CreateFormatException();
			}
			if (displayName == null)
			{
				displayName = address.Substring(num + 1, num2 - num - 1).Trim();
			}
			address = address.Substring(num2 + 1).Trim();
		}
		num = address.IndexOf('<');
		if (num >= 0)
		{
			if (displayName == null)
			{
				displayName = address.Substring(0, num).Trim();
			}
			if (address.Length - 1 == num)
			{
				throw CreateFormatException();
			}
			int num3 = address.IndexOf('>', num + 1);
			if (num3 == -1)
			{
				throw CreateFormatException();
			}
			address = address.Substring(num + 1, num3 - num - 1).Trim();
		}
		this.address = address;
		num = address.IndexOf('@');
		if (num <= 0)
		{
			throw CreateFormatException();
		}
		if (num != address.LastIndexOf('@'))
		{
			throw CreateFormatException();
		}
		user = address.Substring(0, num).Trim();
		if (user.Length == 0)
		{
			throw CreateFormatException();
		}
		host = address.Substring(num + 1).Trim();
		if (host.Length == 0)
		{
			throw CreateFormatException();
		}
	}

	/// <summary>Compares two mail addresses.</summary>
	/// <returns>true if the two mail addresses are equal; otherwise, false.</returns>
	/// <param name="value">A <see cref="T:System.Net.Mail.MailAddress" /> instance to compare to the current instance.</param>
	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		return string.Compare(ToString(), value.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
	}

	/// <summary>Returns a hash value for a mail address.</summary>
	/// <returns>An integer hash value.</returns>
	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	/// <summary>Returns a string representation of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the contents of this <see cref="T:System.Net.Mail.MailAddress" />.</returns>
	public override string ToString()
	{
		if (to_string != null)
		{
			return to_string;
		}
		if (!string.IsNullOrEmpty(displayName))
		{
			to_string = $"\"{DisplayName}\" <{Address}>";
		}
		else
		{
			to_string = address;
		}
		return to_string;
	}

	private static FormatException CreateFormatException()
	{
		return new FormatException("The specified string is not in the form required for an e-mail address.");
	}
}
