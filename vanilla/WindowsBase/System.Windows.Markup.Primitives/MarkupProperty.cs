using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup.Primitives;

/// <summary>Abstract class that provides a property description to be used while writing to markup which encapsulates access to properties and their values. </summary>
public abstract class MarkupProperty
{
	/// <summary>When overridden in a derived class, gets a name that is used for diagnostics and error reporting. </summary>
	/// <returns>The identifier property name.</returns>
	public abstract string Name { get; }

	/// <summary>When overridden in a derived class, gets the CLR type of the property.</summary>
	/// <returns>The CLR type.</returns>
	public abstract Type PropertyType { get; }

	internal bool IsCollectionProperty
	{
		get
		{
			Type propertyType = PropertyType;
			if (!typeof(IList).IsAssignableFrom(propertyType) && !typeof(IDictionary).IsAssignableFrom(propertyType))
			{
				return propertyType.IsArray;
			}
			return true;
		}
	}

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.ComponentModel.PropertyDescriptor" /> for the markup property. </summary>
	/// <returns>The property descriptor. </returns>
	public virtual PropertyDescriptor PropertyDescriptor => null;

	/// <summary>When overridden in a derived class, gets the <see cref="T:System.Windows.DependencyProperty" /> identifier for the markup property if the property is implemented as a dependency property</summary>
	/// <returns>The dependency property identifier.</returns>
	public virtual DependencyProperty DependencyProperty => null;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> is an attached <see cref="T:System.Windows.DependencyProperty" />. </summary>
	/// <returns>true if the property is an attached <see cref="T:System.Windows.DependencyProperty" />; otherwise, false.</returns>
	public virtual bool IsAttached => false;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> represents a constructor argument.</summary>
	/// <returns>true if this property represents a constructor argument; otherwise, false.</returns>
	public virtual bool IsConstructorArgument => false;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> represents text which is passed to a type converter to create an instance of the property or if a constructor should be used.</summary>
	/// <returns>true, if this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> represents a string; otherwise, false.</returns>
	public virtual bool IsValueAsString => false;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> represents direct content of a collection.</summary>
	/// <returns>true if the property represents direct content; otherwise, false.</returns>
	public virtual bool IsContent => false;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> represents the key used by the <see cref="T:System.Windows.Markup.Primitives.MarkupObject" /> to store the item in a dictionary.</summary>
	/// <returns>true if this property represents a key; otherwise, false.</returns>
	public virtual bool IsKey => false;

	/// <summary>When overridden in a derived class, determines whether this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> is a composite property. </summary>
	/// <returns>true is this property is a composite property; otherwise, false.</returns>
	public virtual bool IsComposite => false;

	/// <summary>When overridden in a derived class, gets the current value of this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" />.</summary>
	/// <returns>The current value.</returns>
	public abstract object Value { get; }

	/// <summary>When overridden in a derived class, gets the string value of this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" />.</summary>
	/// <returns>The string value.</returns>
	public abstract string StringValue { get; }

	/// <summary>When overridden in a derived class, gets the set of types that this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" /> will reference when it serializes its value as a string.</summary>
	/// <returns>The set of types.</returns>
	public abstract IEnumerable<Type> TypeReferences { get; }

	/// <summary>When overridden in a derived class, gets the items that make up the value of this property. </summary>
	/// <returns>The items that make up the value of this property.</returns>
	public abstract IEnumerable<MarkupObject> Items { get; }

	/// <summary>When overridden in a derived class, gets the attributes associated with this <see cref="T:System.Windows.Markup.Primitives.MarkupProperty" />.</summary>
	/// <returns>The collection of attributes.</returns>
	public abstract AttributeCollection Attributes { get; }

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal MarkupProperty()
	{
	}

	internal virtual void VerifyOnlySerializableTypes()
	{
	}
}
