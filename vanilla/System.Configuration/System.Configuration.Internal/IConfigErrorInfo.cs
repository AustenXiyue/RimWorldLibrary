namespace System.Configuration.Internal;

/// <summary>Defines an interface used by the .NET Framework to support creating error configuration records.</summary>
public interface IConfigErrorInfo
{
	/// <summary>Gets a string specifying the file name related to the configuration details.</summary>
	/// <returns>A string specifying a filename.</returns>
	string Filename { get; }

	/// <summary>Gets an integer specifying the line number related to the configuration details.</summary>
	/// <returns>An integer specifying a line number.</returns>
	int LineNumber { get; }
}
