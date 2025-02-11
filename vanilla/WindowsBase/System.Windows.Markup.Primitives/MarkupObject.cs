using System.Collections.Generic;
using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows.Markup.Primitives;

/// <summary>Abstract class that represents an object that can be used to navigate a tree of objects.</summary>
public abstract class MarkupObject
{
	/// <summary>When overridden in a derived class, gets the type of the <see cref="T:System.Windows.Markup.Primitives.MarkupObject" /> instance.</summary>
	/// <returns>The type of the object. </returns>
	public abstract Type ObjectType { get; }

	/// <summary>When overridden in a derived class, gets the instance of the object represented by this <see cref="T:System.Windows.Markup.Primitives.MarkupObject" />.</summary>
	/// <returns>The instance of the object</returns>
	public abstract object Instance { get; }

	/// <summary>When overridden in a derived class, gets the properties of this <see cref="T:System.Windows.Markup.Primitives.MarkupObject" /> instance that should be written to XAML.</summary>
	/// <returns>The properties.  </returns>
	public virtual IEnumerable<MarkupProperty> Properties => GetProperties(mapToConstructorArgs: true);

	/// <summary>When overridden in a derived class, gets the attributes associated with this <see cref="T:System.Windows.Markup.Primitives.MarkupObject" />.  </summary>
	/// <returns>The collection of attributes. </returns>
	public abstract AttributeCollection Attributes { get; }

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal MarkupObject()
	{
	}

	internal abstract IEnumerable<MarkupProperty> GetProperties(bool mapToConstructorArgs);

	/// <summary>When overridden in a derived class, assigns a root context for <see cref="T:System.Windows.Markup.ValueSerializer" /> classes.</summary>
	/// <param name="context">The <see cref="T:System.Windows.Markup.IValueSerializerContext" /> to assign a root context for.</param>
	public abstract void AssignRootContext(IValueSerializerContext context);
}
