using System;
using System.IO;
using System.Threading;
using MS.Internal.WindowsBase;

namespace MS.Internal;

internal static class Verify
{
	public static void IsApartmentState(ApartmentState requiredState)
	{
		if (Thread.CurrentThread.GetApartmentState() != requiredState)
		{
			throw new InvalidOperationException(SR.Format(SR.Verify_ApartmentState, requiredState));
		}
	}

	public static void IsNeitherNullNorEmpty(string value, string name)
	{
		if (value == null)
		{
			throw new ArgumentNullException(name, SR.Verify_NeitherNullNorEmpty);
		}
		if (value == "")
		{
			throw new ArgumentException(SR.Verify_NeitherNullNorEmpty, name);
		}
	}

	public static void IsNotNull<T>(T obj, string name) where T : class
	{
		if (obj == null)
		{
			throw new ArgumentNullException(name);
		}
	}

	public static void IsTrue(bool expression, string name, string message)
	{
		if (!expression)
		{
			throw new ArgumentException(message, name);
		}
	}

	public static void AreNotEqual<T>(T actual, T notExpected, string parameterName, string message)
	{
		if (notExpected == null)
		{
			if (actual == null || actual.Equals(notExpected))
			{
				throw new ArgumentException(SR.Format(SR.Verify_AreNotEqual, notExpected), parameterName);
			}
		}
		else if (notExpected.Equals(actual))
		{
			throw new ArgumentException(SR.Format(SR.Verify_AreNotEqual, notExpected), parameterName);
		}
	}

	public static void FileExists(string filePath, string parameterName)
	{
		IsNeitherNullNorEmpty(filePath, parameterName);
		if (!File.Exists(filePath))
		{
			throw new ArgumentException(SR.Format(SR.Verify_FileExists, filePath), parameterName);
		}
	}
}
