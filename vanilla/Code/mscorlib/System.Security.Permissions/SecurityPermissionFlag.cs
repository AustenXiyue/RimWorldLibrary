using System.Runtime.InteropServices;

namespace System.Security.Permissions;

/// <summary>Specifies access flags for the security permission object.</summary>
[Serializable]
[Flags]
[ComVisible(true)]
public enum SecurityPermissionFlag
{
	/// <summary>No security access.</summary>
	NoFlags = 0,
	/// <summary>Ability to assert that all this code's callers have the requisite permission for the operation.</summary>
	Assertion = 1,
	/// <summary>Ability to call unmanaged code.</summary>
	UnmanagedCode = 2,
	/// <summary>Ability to skip verification of code in this assembly. Code that is unverifiable can be run if this permission is granted.</summary>
	SkipVerification = 4,
	/// <summary>Permission for the code to run. Without this permission, managed code will not be executed.</summary>
	Execution = 8,
	/// <summary>Ability to use certain advanced operations on threads.</summary>
	ControlThread = 0x10,
	/// <summary>Ability to provide evidence, including the ability to alter the evidence provided by the common language runtime.</summary>
	ControlEvidence = 0x20,
	/// <summary>Ability to view and modify policy.</summary>
	ControlPolicy = 0x40,
	/// <summary>Ability to provide serialization services. Used by serialization formatters.</summary>
	SerializationFormatter = 0x80,
	/// <summary>Ability to specify domain policy.</summary>
	ControlDomainPolicy = 0x100,
	/// <summary>Ability to manipulate the principal object.</summary>
	ControlPrincipal = 0x200,
	/// <summary>Ability to create and manipulate an <see cref="T:System.AppDomain" />.</summary>
	ControlAppDomain = 0x400,
	/// <summary>Permission to configure Remoting types and channels.</summary>
	RemotingConfiguration = 0x800,
	/// <summary>Permission to plug code into the common language runtime infrastructure, such as adding Remoting Context Sinks, Envoy Sinks and Dynamic Sinks.</summary>
	Infrastructure = 0x1000,
	/// <summary>Permission to perform explicit binding redirection in the application configuration file. This includes redirection of .NET Framework assemblies that have been unified as well as other assemblies found outside the .NET Framework.</summary>
	BindingRedirects = 0x2000,
	/// <summary>The unrestricted state of the permission.</summary>
	AllFlags = 0x3FFF
}
