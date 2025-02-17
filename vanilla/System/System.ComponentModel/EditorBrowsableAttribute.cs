namespace System.ComponentModel;

/// <summary>Specifies that a property or method is viewable in an editor. This class cannot be inherited.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
public sealed class EditorBrowsableAttribute : Attribute
{
	private EditorBrowsableState browsableState;

	/// <summary>Gets the browsable state of the property or method.</summary>
	/// <returns>An <see cref="T:System.ComponentModel.EditorBrowsableState" /> that is the browsable state of the property or method.</returns>
	public EditorBrowsableState State => browsableState;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorBrowsableAttribute" /> class with an <see cref="T:System.ComponentModel.EditorBrowsableState" />.</summary>
	/// <param name="state">The <see cref="T:System.ComponentModel.EditorBrowsableState" /> to set <see cref="P:System.ComponentModel.EditorBrowsableAttribute.State" /> to. </param>
	public EditorBrowsableAttribute(EditorBrowsableState state)
	{
		browsableState = state;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.EditorBrowsableAttribute" /> class with <see cref="P:System.ComponentModel.EditorBrowsableAttribute.State" /> set to the default state.</summary>
	public EditorBrowsableAttribute()
		: this(EditorBrowsableState.Always)
	{
	}

	/// <summary>Returns whether the value of the given object is equal to the current <see cref="T:System.ComponentModel.EditorBrowsableAttribute" />.</summary>
	/// <returns>true if the value of the given object is equal to that of the current; otherwise, false.</returns>
	/// <param name="obj">The object to test the value equality of. </param>
	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is EditorBrowsableAttribute editorBrowsableAttribute)
		{
			return editorBrowsableAttribute.browsableState == browsableState;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
