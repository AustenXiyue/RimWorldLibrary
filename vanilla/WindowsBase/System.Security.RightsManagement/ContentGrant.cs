namespace System.Security.RightsManagement;

/// <summary>Represents a right granted to a user to access information in a rights managed document.</summary>
public class ContentGrant
{
	private ContentUser _user;

	private ContentRight _right;

	private DateTime _validFrom;

	private DateTime _validUntil;

	/// <summary>Gets the user who is granted the access <see cref="P:System.Security.RightsManagement.ContentGrant.Right" />.</summary>
	/// <returns>The user that the access <see cref="P:System.Security.RightsManagement.ContentGrant.Right" /> is granted to, as specified to the <see cref="M:System.Security.RightsManagement.ContentGrant.#ctor(System.Security.RightsManagement.ContentUser,System.Security.RightsManagement.ContentRight)" /> constructor.</returns>
	public ContentUser User => _user;

	/// <summary>Gets the <see cref="T:System.Security.RightsManagement.ContentRight" /> that is granted.</summary>
	/// <returns>The access right that is granted to the <see cref="P:System.Security.RightsManagement.ContentGrant.User" />, as specified to the <see cref="M:System.Security.RightsManagement.ContentGrant.#ctor(System.Security.RightsManagement.ContentUser,System.Security.RightsManagement.ContentRight)" /> constructor.</returns>
	public ContentRight Right => _right;

	/// <summary>Gets the starting date and time that the granted <see cref="P:System.Security.RightsManagement.ContentGrant.Right" /> begins.</summary>
	/// <returns>The start date and time that the granted <see cref="P:System.Security.RightsManagement.ContentGrant.Right" /> begins, or <see cref="T:System.DateTime" />.<see cref="F:System.DateTime.MinValue" /> if there is there is no starting limitation.</returns>
	public DateTime ValidFrom => _validFrom;

	/// <summary>Gets the ending date and time that the granted <see cref="P:System.Security.RightsManagement.ContentGrant.Right" /> expires.</summary>
	/// <returns>The end date and time that the granted <see cref="P:System.Security.RightsManagement.ContentGrant.Right" /> expires, or <see cref="T:System.DateTime" />.<see cref="F:System.DateTime.MaxValue" /> if there is no ending limitation.</returns>
	public DateTime ValidUntil => _validUntil;

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.ContentGrant" /> class that grants a specified <see cref="T:System.Security.RightsManagement.ContentUser" /> a specified <see cref="T:System.Security.RightsManagement.ContentRight" />.</summary>
	/// <param name="user">The user the access right is granted to.</param>
	/// <param name="right">The access right that is granted.</param>
	public ContentGrant(ContentUser user, ContentRight right)
		: this(user, right, DateTime.MinValue, DateTime.MaxValue)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.RightsManagement.ContentGrant" /> class that grants a specified <see cref="T:System.Security.RightsManagement.ContentUser" /> a specified <see cref="T:System.Security.RightsManagement.ContentRight" /> for a specified <see cref="T:System.DateTime" /> duration.</summary>
	/// <param name="user">The user the access right is granted to.</param>
	/// <param name="right">The access right that is granted.</param>
	/// <param name="validFrom">The starting date and time that the right begins.</param>
	/// <param name="validUntil">The ending date and time that the right expires.</param>
	public ContentGrant(ContentUser user, ContentRight right, DateTime validFrom, DateTime validUntil)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (right != 0 && right != ContentRight.Edit && right != ContentRight.Print && right != ContentRight.Extract && right != ContentRight.ObjectModel && right != ContentRight.Owner && right != ContentRight.ViewRightsData && right != ContentRight.Forward && right != ContentRight.Reply && right != ContentRight.ReplyAll && right != ContentRight.Sign && right != ContentRight.DocumentEdit && right != ContentRight.Export)
		{
			throw new ArgumentOutOfRangeException("right");
		}
		if (validFrom > validUntil)
		{
			throw new ArgumentOutOfRangeException("validFrom");
		}
		_user = user;
		_right = right;
		_validFrom = validFrom;
		_validUntil = validUntil;
	}
}
