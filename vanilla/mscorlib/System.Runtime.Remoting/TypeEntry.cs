using System.Runtime.InteropServices;

namespace System.Runtime.Remoting;

/// <summary>Implements a base class that holds the configuration information used to activate an instance of a remote type.</summary>
[ComVisible(true)]
public class TypeEntry
{
	private string assembly_name;

	private string type_name;

	/// <summary>Gets the assembly name of the object type configured to be a remote-activated type.</summary>
	/// <returns>The assembly name of the object type configured to be a remote-activated type.</returns>
	public string AssemblyName
	{
		get
		{
			return assembly_name;
		}
		set
		{
			assembly_name = value;
		}
	}

	/// <summary>Gets the full type name of the object type configured to be a remote-activated type.</summary>
	/// <returns>The full type name of the object type configured to be a remote-activated type.</returns>
	public string TypeName
	{
		get
		{
			return type_name;
		}
		set
		{
			type_name = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.TypeEntry" /> class.</summary>
	protected TypeEntry()
	{
	}
}
