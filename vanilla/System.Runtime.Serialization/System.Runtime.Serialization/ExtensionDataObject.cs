using System.Collections.Generic;

namespace System.Runtime.Serialization;

/// <summary>Stores data from a versioned data contract that has been extended by adding new members.</summary>
public sealed class ExtensionDataObject
{
	private IList<ExtensionDataMember> members;

	internal IList<ExtensionDataMember> Members
	{
		get
		{
			return members;
		}
		set
		{
			members = value;
		}
	}

	internal ExtensionDataObject()
	{
	}
}
