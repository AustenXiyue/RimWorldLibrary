namespace System.ComponentModel;

/// <summary>Specifies when a component property can be bound to an application setting.</summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class SettingsBindableAttribute : Attribute
{
	/// <summary>Specifies that a property is appropriate to bind settings to.</summary>
	public static readonly SettingsBindableAttribute Yes = new SettingsBindableAttribute(bindable: true);

	/// <summary>Specifies that a property is not appropriate to bind settings to.</summary>
	public static readonly SettingsBindableAttribute No = new SettingsBindableAttribute(bindable: false);

	private bool _bindable;

	/// <summary>Gets a value indicating whether a property is appropriate to bind settings to. </summary>
	/// <returns>true if the property is appropriate to bind settings to; otherwise, false.</returns>
	public bool Bindable => _bindable;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.SettingsBindableAttribute" /> class. </summary>
	/// <param name="bindable">true to specify that a property is appropriate to bind settings to; otherwise, false.</param>
	public SettingsBindableAttribute(bool bindable)
	{
		_bindable = bindable;
	}

	/// <summary>Determines whether two <see cref="T:System.ComponentModel.SettingsBindableAttribute" /> objects are equal.</summary>
	/// <returns>true if <paramref name="obj" /> equals the type and value of this instance; otherwise, false.</returns>
	/// <param name="obj">The value to compare to.</param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj != null && obj is SettingsBindableAttribute)
		{
			return ((SettingsBindableAttribute)obj).Bindable == _bindable;
		}
		return false;
	}

	/// <returns>A 32-bit signed integer hash code.</returns>
	public override int GetHashCode()
	{
		return _bindable.GetHashCode();
	}
}
