using System.Configuration;
using System.IO;

namespace System.Xml.Serialization.Configuration;

/// <summary>Validates the rules governing the use of the tempFilesLocation configuration switch. </summary>
public class RootedPathValidator : ConfigurationValidatorBase
{
	/// <summary>Determines whether the type of the object can be validated.</summary>
	/// <returns>true if the <paramref name="type" /> parameter matches a valid XMLSerializer object; otherwise, false.</returns>
	/// <param name="type">The type of the object.</param>
	public override bool CanValidate(Type type)
	{
		return type == typeof(string);
	}

	/// <summary>Determines whether the value of an object is valid.</summary>
	/// <param name="value">The value of an object.</param>
	public override void Validate(object value)
	{
		string text = value as string;
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		text = text.Trim();
		if (!string.IsNullOrEmpty(text))
		{
			if (!Path.IsPathRooted(text))
			{
				throw new ConfigurationErrorsException();
			}
			char c = text[0];
			if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar)
			{
				throw new ConfigurationErrorsException();
			}
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Xml.Serialization.Configuration.RootedPathValidator" /> class. </summary>
	public RootedPathValidator()
	{
	}
}
