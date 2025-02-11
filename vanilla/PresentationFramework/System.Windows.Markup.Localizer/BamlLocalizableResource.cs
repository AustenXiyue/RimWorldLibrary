namespace System.Windows.Markup.Localizer;

/// <summary>Represents a localizable resource in a BAML stream. </summary>
public class BamlLocalizableResource
{
	[Flags]
	private enum LocalizationFlags : byte
	{
		Readable = 1,
		Modifiable = 2
	}

	private string _content;

	private string _comments;

	private LocalizationFlags _flags;

	private LocalizationCategory _category;

	/// <summary>Gets or sets the localizable content.</summary>
	/// <returns>The localizable content string.</returns>
	public string Content
	{
		get
		{
			return _content;
		}
		set
		{
			_content = value;
		}
	}

	/// <summary>Gets or sets the localization comments associated with a resource. </summary>
	/// <returns>The localization comment string.</returns>
	public string Comments
	{
		get
		{
			return _comments;
		}
		set
		{
			_comments = value;
		}
	}

	/// <summary>Gets or sets a value that indicates whether the localizable resource is modifiable. </summary>
	/// <returns>true if the resource is modifiable; otherwise, false.</returns>
	public bool Modifiable
	{
		get
		{
			return (int)(_flags & LocalizationFlags.Modifiable) > 0;
		}
		set
		{
			if (value)
			{
				_flags |= LocalizationFlags.Modifiable;
			}
			else
			{
				_flags &= ~LocalizationFlags.Modifiable;
			}
		}
	}

	/// <summary>Gets or sets whether the resource is visible for translation. </summary>
	/// <returns>true if the resource is visible for translation; otherwise, false.</returns>
	public bool Readable
	{
		get
		{
			return (int)(_flags & LocalizationFlags.Readable) > 0;
		}
		set
		{
			if (value)
			{
				_flags |= LocalizationFlags.Readable;
			}
			else
			{
				_flags &= ~LocalizationFlags.Readable;
			}
		}
	}

	/// <summary>Gets or sets the localization category of a resource. </summary>
	/// <returns>The localization category, as a value of the enumeration.</returns>
	public LocalizationCategory Category
	{
		get
		{
			return _category;
		}
		set
		{
			_category = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResource" /> class. </summary>
	public BamlLocalizableResource()
		: this(null, null, LocalizationCategory.None, modifiable: true, readable: true)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResource" /> class, with the specified localizable value, localization comments, resource category, localization lock status, and visibility of the resource.</summary>
	/// <param name="content">The localizable value.</param>
	/// <param name="comments">Comments used for localizing.</param>
	/// <param name="category">The string category of the resource.</param>
	/// <param name="modifiable">true if the resource should be modifiable; otherwise, false.</param>
	/// <param name="readable">true if the resource should be visible for translation purposes because it is potentially readable as text in the UI; otherwise, false.</param>
	public BamlLocalizableResource(string content, string comments, LocalizationCategory category, bool modifiable, bool readable)
	{
		_content = content;
		_comments = comments;
		_category = category;
		Modifiable = modifiable;
		Readable = readable;
	}

	internal BamlLocalizableResource(BamlLocalizableResource other)
	{
		_content = other._content;
		_comments = other._comments;
		_flags = other._flags;
		_category = other._category;
	}

	/// <summary>Determines whether a specified <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResource" /> object is equal to this object.</summary>
	/// <returns>true if <paramref name="other" /> is equal to this object; otherwise, false.</returns>
	/// <param name="other">The <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResource" /> object test for equality.</param>
	public override bool Equals(object other)
	{
		if (!(other is BamlLocalizableResource bamlLocalizableResource))
		{
			return false;
		}
		if (_content == bamlLocalizableResource._content && _comments == bamlLocalizableResource._comments && _flags == bamlLocalizableResource._flags)
		{
			return _category == bamlLocalizableResource._category;
		}
		return false;
	}

	/// <summary>Returns an integer hash code representing this instance. </summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		return (int)((uint)(((_content != null) ? _content.GetHashCode() : 0) ^ ((_comments != null) ? _comments.GetHashCode() : 0)) ^ (uint)_flags) ^ (int)_category;
	}
}
