using System;
using System.ComponentModel;

namespace WinRT;

[EditorBrowsable(EditorBrowsableState.Never)]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class ObjectReferenceWrapperAttribute : Attribute
{
	public string ObjectReferenceField { get; }

	public ObjectReferenceWrapperAttribute(string objectReferenceField)
	{
		ObjectReferenceField = objectReferenceField;
	}
}
