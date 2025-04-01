using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting;

/// <summary>Holds values for an object type registered on the client end as a type that can be activated on the server.</summary>
[ComVisible(true)]
public class ActivatedClientTypeEntry : TypeEntry
{
	private string applicationUrl;

	private Type obj_type;

	/// <summary>Gets the URL of the application to activate the type in.</summary>
	/// <returns>The URL of the application to activate the type in.</returns>
	public string ApplicationUrl => applicationUrl;

	/// <summary>Gets or sets the context attributes for the client-activated type.</summary>
	/// <returns>The context attributes for the client activated type.</returns>
	public IContextAttribute[] ContextAttributes
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	/// <summary>Gets the <see cref="T:System.Type" /> of the client-activated type.</summary>
	/// <returns>Gets the <see cref="T:System.Type" /> of the client-activated type.</returns>
	public Type ObjectType => obj_type;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> class with the given <see cref="T:System.Type" /> and application URL.</summary>
	/// <param name="type">The <see cref="T:System.Type" /> of the client activated type. </param>
	/// <param name="appUrl">The URL of the application to activate the type in. </param>
	public ActivatedClientTypeEntry(Type type, string appUrl)
	{
		base.AssemblyName = type.Assembly.FullName;
		base.TypeName = type.FullName;
		applicationUrl = appUrl;
		obj_type = type;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Remoting.ActivatedClientTypeEntry" /> class with the given type name, assembly name, and application URL.</summary>
	/// <param name="typeName">The type name of the client activated type. </param>
	/// <param name="assemblyName">The assembly name of the client activated type. </param>
	/// <param name="appUrl">The URL of the application to activate the type in. </param>
	public ActivatedClientTypeEntry(string typeName, string assemblyName, string appUrl)
	{
		base.AssemblyName = assemblyName;
		base.TypeName = typeName;
		applicationUrl = appUrl;
		Assembly assembly = Assembly.Load(assemblyName);
		obj_type = assembly.GetType(typeName);
		if (obj_type == null)
		{
			throw new RemotingException("Type not found: " + typeName + ", " + assemblyName);
		}
	}

	/// <summary>Returns the type name, assembly name, and application URL of the client-activated type as a <see cref="T:System.String" />.</summary>
	/// <returns>The type name, assembly name, and application URL of the client-activated type as a <see cref="T:System.String" />.</returns>
	public override string ToString()
	{
		return base.TypeName + base.AssemblyName + ApplicationUrl;
	}
}
