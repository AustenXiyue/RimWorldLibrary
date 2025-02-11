using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

/// <summary>Implements x:Array support for .NET Framework XAML Services</summary>
[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
[ContentProperty("Items")]
[MarkupExtensionReturnType(typeof(Array))]
public class ArrayExtension : MarkupExtension
{
	private ArrayList _arrayList = new ArrayList();

	private Type _arrayType;

	/// <summary>Gets or sets the type of array to be created when calling <see cref="M:System.Windows.Markup.ArrayExtension.ProvideValue(System.IServiceProvider)" />.</summary>
	/// <returns>The type of the array.</returns>
	[ConstructorArgument("type")]
	public Type Type
	{
		get
		{
			return _arrayType;
		}
		set
		{
			_arrayType = value;
		}
	}

	/// <summary>Gets the contents of the array. Settable in XAML through XAML collection syntax.</summary>
	/// <returns>The array contents.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	public IList Items => _arrayList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ArrayExtension" /> class. This creates an empty array. </summary>
	public ArrayExtension()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ArrayExtension" /> class and initializes the type of the array. </summary>
	/// <param name="arrayType">The object type of the new array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="arrayType" /> is null.</exception>
	public ArrayExtension(Type arrayType)
	{
		_arrayType = arrayType ?? throw new ArgumentNullException("arrayType");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.ArrayExtension" /> class based on the provided raw array.</summary>
	/// <param name="elements">The array content that populates the created array.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="elements" /> is null.</exception>
	public ArrayExtension(Array elements)
	{
		ArgumentNullException.ThrowIfNull(elements, "elements");
		_arrayList.AddRange(elements);
		_arrayType = elements.GetType().GetElementType();
	}

	/// <summary>Appends the supplied object to the end of the array. </summary>
	/// <param name="value">The object to add to the end of the array.</param>
	public void AddChild(object value)
	{
		_arrayList.Add(value);
	}

	/// <summary>Adds a text node as a new array item.</summary>
	/// <param name="text">The text to add to the end of the array.</param>
	public void AddText(string text)
	{
		AddChild(text);
	}

	/// <summary>Returns an array that is sized to the number of objects supplied in the <see cref="P:System.Windows.Markup.ArrayExtension.Items" /> values. </summary>
	/// <returns>The created array, or null.</returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension.</param>
	/// <exception cref="T:System.InvalidOperationException">Processed an array that did not provide a valid <see cref="P:System.Windows.Markup.ArrayExtension.Type" />.-or-There is a type mismatch between the declared <see cref="P:System.Windows.Markup.ArrayExtension.Type" /> of the array and one or more of its <see cref="P:System.Windows.Markup.ArrayExtension.Items" /> values. </exception>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_arrayType == null)
		{
			throw new InvalidOperationException(System.SR.MarkupExtensionArrayType);
		}
		object obj = null;
		try
		{
			return _arrayList.ToArray(_arrayType);
		}
		catch (InvalidCastException)
		{
			throw new InvalidOperationException(System.SR.Format(System.SR.MarkupExtensionArrayBadType, _arrayType.Name));
		}
	}
}
