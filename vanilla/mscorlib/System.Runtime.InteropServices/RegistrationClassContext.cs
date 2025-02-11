namespace System.Runtime.InteropServices;

/// <summary>Specifies the set of execution contexts in which a class object will be made available for requests to construct instances.</summary>
[Flags]
public enum RegistrationClassContext
{
	/// <summary>Disables activate-as-activator (AAA) activations for this activation only.</summary>
	DisableActivateAsActivator = 0x8000,
	/// <summary>Enables activate-as-activator (AAA) activations for this activation only.</summary>
	EnableActivateAsActivator = 0x10000,
	/// <summary>Allows the downloading of code from the Directory Service or the Internet.</summary>
	EnableCodeDownload = 0x2000,
	/// <summary>Begin this activation from the default context of the current apartment.</summary>
	FromDefaultContext = 0x20000,
	/// <summary>The code that manages objects of this class is an in-process handler.</summary>
	InProcessHandler = 2,
	/// <summary>Not used.</summary>
	InProcessHandler16 = 0x20,
	/// <summary>The code that creates and manages objects of this class is a DLL that runs in the same process as the caller of the function specifying the class context.</summary>
	InProcessServer = 1,
	/// <summary>Not used.</summary>
	InProcessServer16 = 8,
	/// <summary>The EXE code that creates and manages objects of this class runs on same machine but is loaded in a separate process space.</summary>
	LocalServer = 4,
	/// <summary>Disallows the downloading of code from the Directory Service or the Internet.</summary>
	NoCodeDownload = 0x400,
	/// <summary>Specifies whether activation fails if it uses custom marshaling.</summary>
	NoCustomMarshal = 0x1000,
	/// <summary>Overrides the logging of failures.</summary>
	NoFailureLog = 0x4000,
	/// <summary>A remote machine context.</summary>
	RemoteServer = 0x10,
	/// <summary>Not used.</summary>
	Reserved1 = 0x40,
	/// <summary>Not used.</summary>
	Reserved2 = 0x80,
	/// <summary>Not used.</summary>
	Reserved3 = 0x100,
	/// <summary>Not used.</summary>
	Reserved4 = 0x200,
	/// <summary>Not used.</summary>
	Reserved5 = 0x800
}
