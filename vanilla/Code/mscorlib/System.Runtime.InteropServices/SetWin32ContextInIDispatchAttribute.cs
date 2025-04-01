namespace System.Runtime.InteropServices;

/// <summary>This attribute has been deprecated. </summary>
[AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
[ComVisible(true)]
[Obsolete("This attribute has been deprecated.  Application Domains no longer respect Activation Context boundaries in IDispatch calls.", false)]
public sealed class SetWin32ContextInIDispatchAttribute : Attribute
{
	/// <summary>This attribute has been deprecated.  </summary>
	public SetWin32ContextInIDispatchAttribute()
	{
	}
}
