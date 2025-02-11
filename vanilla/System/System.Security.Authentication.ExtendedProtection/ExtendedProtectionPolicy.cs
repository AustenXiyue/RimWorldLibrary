using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Security.Authentication.ExtendedProtection;

/// <summary>The <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class represents the extended protection policy used by the server to validate incoming client connections. </summary>
[Serializable]
[System.MonoTODO]
[TypeConverter(typeof(ExtendedProtectionPolicyTypeConverter))]
public class ExtendedProtectionPolicy : ISerializable
{
	/// <summary>Gets a custom channel binding token (CBT) to use for validation.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> that contains a custom channel binding to use for validation.</returns>
	public ChannelBinding CustomChannelBinding
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the custom Service Provider Name (SPN) list used to match against a client's SPN.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ServiceNameCollection" /> that contains the custom SPN list that is used to match against a client's SPN.</returns>
	public ServiceNameCollection CustomServiceNames
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Indicates whether the operating system supports integrated windows authentication with extended protection.</summary>
	/// <returns>true if the operating system supports integrated windows authentication with extended protection, otherwise false.</returns>
	public static bool OSSupportsExtendedProtection
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets when the extended protection policy should be enforced.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced. </returns>
	public PolicyEnforcement PolicyEnforcement
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Gets the kind of protection enforced by the extended protection policy.</summary>
	/// <returns>A <see cref="T:System.Security.Authentication.ExtendedProtection.ProtectionScenario" /> value that indicates the kind of protection enforced by the policy. </returns>
	public ProtectionScenario ProtectionScenario
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class that specifies when the extended protection policy should be enforced.</summary>
	/// <param name="policyEnforcement">A <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced.</param>
	[System.MonoTODO("Not implemented.")]
	public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class that specifies when the extended protection policy should be enforced and the channel binding token (CBT) to be used.</summary>
	/// <param name="policyEnforcement">A <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced.</param>
	/// <param name="customChannelBinding">A <see cref="T:System.Security.Authentication.ExtendedProtection.ChannelBinding" /> that contains a custom channel binding to use for validation.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="policyEnforcement" /> is specified as <see cref="F:System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customChannelBinding " />is null. </exception>
	public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, ChannelBinding customChannelBinding)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class that specifies when the extended protection policy should be enforced, the kind of protection enforced by the policy, and a custom Service Provider Name (SPN) list that is used to match against a client's SPN.</summary>
	/// <param name="policyEnforcement">A <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced.</param>
	/// <param name="protectionScenario">A <see cref="T:System.Security.Authentication.ExtendedProtection.ProtectionScenario" /> value that indicates the kind of protection enforced by the policy.</param>
	/// <param name="customServiceNames">A <see cref="T:System.Collections.ICollection" /> that contains the custom SPN list that is used to match against a client's SPN.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="policyEnforcement" /> is specified as <see cref="F:System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customServiceNames " />is nullor an empty list. </exception>
	public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, ProtectionScenario protectionScenario, ICollection customServiceNames)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class that specifies when the extended protection policy should be enforced, the kind of protection enforced by the policy, and a custom Service Provider Name (SPN) list that is used to match against a client's SPN.</summary>
	/// <param name="policyEnforcement">A <see cref="T:System.Security.Authentication.ExtendedProtection.PolicyEnforcement" /> value that indicates when the extended protection policy should be enforced.</param>
	/// <param name="protectionScenario">A <see cref="T:System.Security.Authentication.ExtendedProtection.ProtectionScenario" /> value that indicates the kind of protection enforced by the policy.</param>
	/// <param name="customServiceNames">A <see cref="T:System.Security.Authentication.ExtendedProtection.ServiceNameCollection" /> that contains the custom SPN list that is used to match against a client's SPN.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="policyEnforcement" /> is specified as <see cref="F:System.Security.Authentication.ExtendedProtection.PolicyEnforcement.Never" />.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="customServiceNames " />is nullor an empty list. </exception>
	public ExtendedProtectionPolicy(PolicyEnforcement policyEnforcement, ProtectionScenario protectionScenario, ServiceNameCollection customServiceNames)
	{
		throw new NotImplementedException();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> class from a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that contains the required data to populate the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" />.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> instance that contains the information that is required to serialize the new <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> instance.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the source of the serialized stream that is associated with the new <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> instance.</param>
	protected ExtendedProtectionPolicy(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}

	/// <summary>Gets a string representation for the extended protection policy instance.</summary>
	/// <returns>A <see cref="T:System.String" /> instance that contains the representation of the <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> instance.</returns>
	[System.MonoTODO]
	public override string ToString()
	{
		return base.ToString();
	}

	/// <summary>Populates a <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the required data to serialize an <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> object.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object that holds the serialized data for an <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" /> object.</param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains the destination of the serialized stream that is associated with the new <see cref="T:System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy" />.</param>
	[SecurityPermission(SecurityAction.LinkDemand, SerializationFormatter = true)]
	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		throw new NotImplementedException();
	}
}
