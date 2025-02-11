namespace System.Windows.Markup.Localizer;

/// <summary>Represents a key that is used to identify localizable resources in a <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</summary>
public class BamlLocalizableResourceKey
{
	private string _uid;

	private string _className;

	private string _propertyName;

	private string _assemblyName;

	/// <summary>Gets the Uid component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</summary>
	/// <returns>The Uid component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</returns>
	public string Uid => _uid;

	/// <summary>Gets the class name component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</summary>
	/// <returns>The class name component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</returns>
	public string ClassName => _className;

	/// <summary>Gets the property name component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</summary>
	/// <returns>The property name component of this <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" />.</returns>
	public string PropertyName => _propertyName;

	/// <summary>Gets the name of the assembly that defines the type of the localizable resource as declared by its <see cref="P:System.Windows.Markup.Localizer.BamlLocalizableResourceKey.ClassName" />.</summary>
	/// <returns>The name of the assembly.</returns>
	public string AssemblyName => _assemblyName;

	internal BamlLocalizableResourceKey(string uid, string className, string propertyName, string assemblyName)
	{
		if (uid == null)
		{
			throw new ArgumentNullException("uid");
		}
		if (className == null)
		{
			throw new ArgumentNullException("className");
		}
		if (propertyName == null)
		{
			throw new ArgumentNullException("propertyName");
		}
		_uid = uid;
		_className = className;
		_propertyName = propertyName;
		_assemblyName = assemblyName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" /> class with the supplied Uid, class name, and property name.</summary>
	/// <param name="uid">The Uid of an element that has a localizable resource.</param>
	/// <param name="className">The class name of a localizable resource in binary XAML (BAML).</param>
	/// <param name="propertyName">The property name of a localizable resource in BAML.</param>
	public BamlLocalizableResourceKey(string uid, string className, string propertyName)
		: this(uid, className, propertyName, null)
	{
	}

	/// <summary>Compares two instances of <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" /> for equality.</summary>
	/// <returns>true if the two instances are equal; otherwise, false.</returns>
	/// <param name="other">The other instance of <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" /> to compare for equality.</param>
	public bool Equals(BamlLocalizableResourceKey other)
	{
		if (other == null)
		{
			return false;
		}
		if (_uid == other._uid && _className == other._className)
		{
			return _propertyName == other._propertyName;
		}
		return false;
	}

	/// <summary>Compares an object to an instance of <see cref="T:System.Windows.Markup.Localizer.BamlLocalizableResourceKey" /> for equality.</summary>
	/// <returns>true if the two instances are equal; otherwise, false.</returns>
	/// <param name="other">The object to compare for equality.</param>
	public override bool Equals(object other)
	{
		return Equals(other as BamlLocalizableResourceKey);
	}

	/// <summary>Returns an integer hash code representing this instance.</summary>
	/// <returns>An integer hash code.</returns>
	public override int GetHashCode()
	{
		return _uid.GetHashCode() ^ _className.GetHashCode() ^ _propertyName.GetHashCode();
	}
}
