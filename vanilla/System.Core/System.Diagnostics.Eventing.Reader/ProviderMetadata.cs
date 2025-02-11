using System.Collections.Generic;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using Unity;

namespace System.Diagnostics.Eventing.Reader;

/// <summary>Contains static information about an event provider, such as the name and id of the provider, and the collection of events defined in the provider.</summary>
/// <filterpriority>2</filterpriority>
[HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort = true)]
public class ProviderMetadata : IDisposable
{
	/// <summary>Gets the localized name of the event provider.</summary>
	/// <returns>Returns a string that contains the localized name of the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public string DisplayName
	{
		[SecurityCritical]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventMetadata" /> objects, each of which represents an event that is defined in the provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventMetadata" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IEnumerable<EventMetadata> Events
	{
		[SecurityCritical]
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IEnumerable<EventMetadata>)0;
		}
	}

	/// <summary>Gets the base of the URL used to form help requests for the events in this event provider.</summary>
	/// <returns>Returns a <see cref="T:System.Uri" /> value.</returns>
	/// <filterpriority>2</filterpriority>
	public Uri HelpLink
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the globally unique identifier (GUID) for the event provider.</summary>
	/// <returns>Returns the GUID value for the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public Guid Id
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(Guid);
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventKeyword" /> objects, each of which represent an event keyword that is defined in the event provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventKeyword" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventKeyword> Keywords
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventKeyword>)0;
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventLevel" /> objects, each of which represent a level that is defined in the event provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventLevel" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventLevel> Levels
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventLevel>)0;
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventLogLink" /> objects, each of which represent a link to an event log that is used by the event provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventLogLink" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventLogLink> LogLinks
	{
		[SecurityCritical]
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventLogLink>)0;
		}
	}

	/// <summary>Gets the path of the file that contains the message table resource that has the strings associated with the provider metadata.</summary>
	/// <returns>Returns a string that contains the path of the provider message file.</returns>
	/// <filterpriority>2</filterpriority>
	public string MessageFilePath
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the unique name of the event provider.</summary>
	/// <returns>Returns a string that contains the unique name of the event provider.</returns>
	/// <filterpriority>2</filterpriority>
	public string Name
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventOpcode" /> objects, each of which represent an opcode that is defined in the event provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventOpcode" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventOpcode> Opcodes
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventOpcode>)0;
		}
	}

	/// <summary>Gets the path of the file that contains the message table resource that has the strings used for parameter substitutions in event descriptions.</summary>
	/// <returns>Returns a string that contains the path of the file that contains the message table resource that has the strings used for parameter substitutions in event descriptions.</returns>
	/// <filterpriority>2</filterpriority>
	public string ParameterFilePath
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets the path to the file that contains the metadata associated with the provider.</summary>
	/// <returns>Returns a string that contains the path to the file that contains the metadata associated with the provider.</returns>
	/// <filterpriority>2</filterpriority>
	public string ResourceFilePath
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	/// <summary>Gets an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventTask" /> objects, each of which represent a task that is defined in the event provider.</summary>
	/// <returns>Returns an enumerable collection of <see cref="T:System.Diagnostics.Eventing.Reader.EventTask" /> objects.</returns>
	/// <filterpriority>2</filterpriority>
	public IList<EventTask> Tasks
	{
		get
		{
			//IL_0007: Expected O, but got I4
			Unity.ThrowStub.ThrowNotSupportedException();
			return (IList<EventTask>)0;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.ProviderMetadata" /> class by specifying the name of the provider that you want to retrieve information about.</summary>
	/// <param name="providerName">The name of the event provider that you want to retrieve information about.</param>
	/// <filterpriority>2</filterpriority>
	public ProviderMetadata(string providerName)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Eventing.Reader.ProviderMetadata" /> class by specifying the name of the provider that you want to retrieve information about, the event log service that the provider is registered with, and the language that you want to return the information in.</summary>
	/// <param name="providerName">The name of the event provider that you want to retrieve information about.</param>
	/// <param name="session">The <see cref="T:System.Diagnostics.Eventing.Reader.EventLogSession" /> object that specifies whether to get the provider information from a provider on the local computer or a provider on a remote computer.</param>
	/// <param name="targetCultureInfo">The culture that specifies the language that the information should be returned in.</param>
	/// <filterpriority>2</filterpriority>
	public ProviderMetadata(string providerName, EventLogSession session, CultureInfo targetCultureInfo)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases all the resources used by this object.</summary>
	/// <filterpriority>2</filterpriority>
	public void Dispose()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	/// <summary>Releases the unmanaged resources used by this object, and optionally releases the managed resources.</summary>
	/// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
	[SecuritySafeCritical]
	protected virtual void Dispose(bool disposing)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
