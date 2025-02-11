namespace System.Security.RightsManagement;

/// <summary>Specifies rights that can be granted to users for accessing content in a rights managed document.</summary>
public enum ContentRight
{
	/// <summary>The user can view the protected content.</summary>
	View,
	/// <summary>The user can edit and encrypt the protected content.</summary>
	Edit,
	/// <summary>The user can print the protected content.</summary>
	Print,
	/// <summary>The user can extract (copy and paste) the protected content.</summary>
	Extract,
	/// <summary>The user can control programmed access to the protected content.</summary>
	ObjectModel,
	/// <summary>The user is the content owner.  The owner can edit and encrypt the protected content, and decrypt the signed <see cref="T:System.Security.RightsManagement.PublishLicense" />.</summary>
	Owner,
	/// <summary>The user can decrypt and view the rights specified in the signed <see cref="T:System.Security.RightsManagement.PublishLicense" />.</summary>
	ViewRightsData,
	/// <summary>The user can forward the protected content to another user.</summary>
	Forward,
	/// <summary>The user can reply to the sender of the protected content.</summary>
	Reply,
	/// <summary>The user can "reply all" to recipients of the protected content.</summary>
	ReplyAll,
	/// <summary>The user can digitally sign the protected content.</summary>
	Sign,
	/// <summary>The user can edit the document that contains the protected content.</summary>
	DocumentEdit,
	/// <summary>The user can export the protected content.</summary>
	Export
}
