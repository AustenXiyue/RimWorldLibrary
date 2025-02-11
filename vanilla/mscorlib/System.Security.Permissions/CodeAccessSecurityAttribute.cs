using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Specifies the base attribute class for code access security.</summary>
[Serializable]
[ComVisible(true)]
[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract class CodeAccessSecurityAttribute : SecurityAttribute
{
	/// <summary>Initializes a new instance of <see cref="T:System.Security.Permissions.CodeAccessSecurityAttribute" /> with the specified <see cref="T:System.Security.Permissions.SecurityAction" />.</summary>
	/// <param name="action">One of the <see cref="T:System.Security.Permissions.SecurityAction" /> values. </param>
	protected CodeAccessSecurityAttribute(SecurityAction action)
		: base(action)
	{
	}
}
