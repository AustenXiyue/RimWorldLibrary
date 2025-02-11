namespace System.Runtime.InteropServices;

/// <summary>Specifies which <see cref="T:System.Type" /> exclusively uses an interface. This class cannot be inherited.</summary>
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Interface, Inherited = false)]
public sealed class TypeLibImportClassAttribute : Attribute
{
	internal string _importClassName;

	/// <summary>Gets the name of a <see cref="T:System.Type" /> object that exclusively uses an interface.</summary>
	/// <returns>The name of a <see cref="T:System.Type" /> object that exclusively uses an interface.</returns>
	public string Value => _importClassName;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.InteropServices.TypeLibImportClassAttribute" /> class specifying the <see cref="T:System.Type" /> that exclusively uses an interface. </summary>
	/// <param name="importClass">The <see cref="T:System.Type" /> object that exclusively uses an interface.</param>
	public TypeLibImportClassAttribute(Type importClass)
	{
		_importClassName = importClass.ToString();
	}
}
