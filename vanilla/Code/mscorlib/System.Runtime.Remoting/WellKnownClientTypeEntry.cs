using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting;

/// <summary>Holds values for an object type registered on the client as a server-activated type (single call or singleton).</summary>
[ComVisible(true)]
public class WellKnownClientTypeEntry : TypeEntry
{
	private Type obj_type;

	private string obj_url;

	private string app_url;

	/// <summary>Gets or sets the URL of the application to activate the type in.</summary>
	/// <returns>The URL of the application to activate the type in.</returns>
	public string ApplicationUrl
	{
		get
		{
			return app_url;
		}
		set
		{
			app_url = value;
		}
	}

	/// <summary>Gets the <see cref="T:System.Type" /> of the server-activated client type.</summary>
	/// <returns>Gets the <see cref="T:System.Type" /> of the server-activated client type.</returns>
	public Type ObjectType => obj_type;

	/// <summary>Gets the URL of the server-activated client object.</summary>
	/// <returns>The URL of the server-activated client object.</returns>
	public string ObjectUrl => obj_url;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> class with the given type and URL.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the server-activated type. </param>
	/// <param name="objectUrl">The URL of the server-activated type. </param>
	public WellKnownClientTypeEntry(Type type, string objectUrl)
	{
		base.AssemblyName = type.Assembly.FullName;
		base.TypeName = type.FullName;
		obj_type = type;
		obj_url = objectUrl;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.WellKnownClientTypeEntry" /> class with the given type, assembly name, and URL.</summary>
	/// <param name="typeName">The type name of the server-activated type. </param>
	/// <param name="assemblyName">The assembly name of the server-activated type. </param>
	/// <param name="objectUrl">The URL of the server-activated type. </param>
	public WellKnownClientTypeEntry(string typeName, string assemblyName, string objectUrl)
	{
		obj_url = objectUrl;
		base.AssemblyName = assemblyName;
		base.TypeName = typeName;
		Assembly assembly = Assembly.Load(assemblyName);
		obj_type = assembly.GetType(typeName);
		if (obj_type == null)
		{
			throw new RemotingException("Type not found: " + typeName + ", " + assemblyName);
		}
	}

	/// <summary>Returns the full type name, assembly name, and object URL of the server-activated client type as a <see cref="T:System.String" />.</summary>
	/// <returns>The full type name, assembly name, and object URL of the server-activated client type as a <see cref="T:System.String" />.</returns>
	public override string ToString()
	{
		if (ApplicationUrl != null)
		{
			return base.TypeName + base.AssemblyName + ObjectUrl + ApplicationUrl;
		}
		return base.TypeName + base.AssemblyName + ObjectUrl;
	}
}
