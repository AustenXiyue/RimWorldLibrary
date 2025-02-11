namespace System.ComponentModel;

/// <summary>Specifies which properties should be reported by type descriptors, specifically the <see cref="M:System.ComponentModel.TypeDescriptor.GetProperties(System.Object)" /> method. This enumeration is used to specify the value of the <see cref="P:System.ComponentModel.PropertyFilterAttribute.Filter" /> property.</summary>
[Flags]
public enum PropertyFilterOptions
{
	/// <summary>Return no properties</summary>
	None = 0,
	/// <summary>Return only those properties that are not valid given the current context of the object. See Remarks.</summary>
	Invalid = 1,
	/// <summary>Return only those properties that have local values currently set.</summary>
	SetValues = 2,
	/// <summary>Return only those properties whose local values are not set, or do not have properties set in an external expression store (such as binding or deferred resource).</summary>
	UnsetValues = 4,
	/// <summary>Return any property that is valid on the  object in the current scope. See Remarks.</summary>
	Valid = 8,
	/// <summary>Return all properties.</summary>
	All = 0xF
}
