using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains an event opcode that is defined in an event provider. An opcode defines a numeric value that identifies the activity or a point within an activity that the application was performing when it raised the event.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public sealed class EventOpcode
{
	/// <summary>Gets the localized name for an event opcode.</summary>
	/// <returns>Returns a string that contains the localized name for an event opcode.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the non-localized name for an event opcode.</summary>
	/// <returns>Returns a string that contains the non-localized name for an event opcode.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the numeric value associated with the event opcode.</summary>
	/// <returns>Returns an integer value.</returns>
	/// <filterpriority>2</filterpriority>
	public int Value
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	internal EventOpcode()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
