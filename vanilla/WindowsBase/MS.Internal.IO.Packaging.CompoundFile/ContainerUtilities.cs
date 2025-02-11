using System;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal static class ContainerUtilities
{
	private static readonly int _int16Size = SizeOfInt16();

	private static readonly int _int32Size = SizeOfInt32();

	private static readonly byte[] _paddingBuf = new byte[4];

	private static readonly int _int64Size = SizeOfInt64();

	internal static readonly char PathSeparator = Path.DirectorySeparatorChar;

	internal static readonly string PathSeparatorAsString = new string(PathSeparator, 1);

	private static readonly CaseInsensitiveOrdinalStringComparer _stringCaseInsensitiveComparer = new CaseInsensitiveOrdinalStringComparer();

	internal static int Int16Size => _int16Size;

	internal static int Int32Size => _int32Size;

	internal static int Int64Size => _int64Size;

	internal static CaseInsensitiveOrdinalStringComparer StringCaseInsensitiveComparer => _stringCaseInsensitiveComparer;

	internal static int CalculateDWordPadBytesLength(int length)
	{
		int num = length & 3;
		if (num > 0)
		{
			num = 4 - num;
		}
		return num;
	}

	internal static int WriteByteLengthPrefixedDWordPaddedUnicodeString(BinaryWriter writer, string outputString)
	{
		int num = 0;
		checked
		{
			if (outputString != null)
			{
				num = outputString.Length * 2;
			}
			if (writer != null)
			{
				writer.Write(num);
				if (num != 0)
				{
					writer.Write(outputString.ToCharArray());
				}
			}
			if (num != 0)
			{
				int num2 = CalculateDWordPadBytesLength(num);
				if (num2 != 0)
				{
					num += num2;
					writer?.Write(_paddingBuf, 0, num2);
				}
			}
			return num + _int32Size;
		}
	}

	internal static string ReadByteLengthPrefixedDWordPaddedUnicodeString(BinaryReader reader)
	{
		int bytesRead;
		return ReadByteLengthPrefixedDWordPaddedUnicodeString(reader, out bytesRead);
	}

	internal static string ReadByteLengthPrefixedDWordPaddedUnicodeString(BinaryReader reader, out int bytesRead)
	{
		bytesRead = 0;
		CheckAgainstNull(reader, "reader");
		bytesRead = reader.ReadInt32();
		string text = null;
		if (bytesRead > 0)
		{
			try
			{
				if (reader.BaseStream.Length < bytesRead / 2)
				{
					throw new FileFormatException(SR.InvalidStringFormat);
				}
			}
			catch (NotSupportedException)
			{
			}
			text = new string(reader.ReadChars(bytesRead / 2));
			if (text.Length != bytesRead / 2)
			{
				throw new FileFormatException(SR.InvalidStringFormat);
			}
		}
		else
		{
			if (bytesRead != 0)
			{
				throw new FileFormatException(SR.InvalidStringFormat);
			}
			text = string.Empty;
		}
		int num = CalculateDWordPadBytesLength(bytesRead);
		checked
		{
			if (num > 0)
			{
				if (reader.ReadBytes(num).Length != num)
				{
					throw new FileFormatException(SR.InvalidStringFormat);
				}
				bytesRead += num;
			}
			bytesRead += _int32Size;
			return text;
		}
	}

	internal static void CheckAgainstNull(object paramRef, string testStringIdentifier)
	{
		if (paramRef == null)
		{
			throw new ArgumentNullException(testStringIdentifier);
		}
	}

	private static int SizeOfInt16()
	{
		return Marshal.SizeOf(typeof(short));
	}

	private static int SizeOfInt32()
	{
		return Marshal.SizeOf(typeof(int));
	}

	private static int SizeOfInt64()
	{
		return Marshal.SizeOf(typeof(long));
	}

	internal static string[] ConvertBackSlashPathToStringArrayPath(string backSlashPath)
	{
		if (backSlashPath == null || backSlashPath.Length == 0)
		{
			return Array.Empty<string>();
		}
		if (char.IsWhiteSpace(backSlashPath[0]) || char.IsWhiteSpace(backSlashPath[backSlashPath.Length - 1]))
		{
			throw new ArgumentException(SR.MalformedCompoundFilePath);
		}
		string[] array = backSlashPath.Split(PathSeparator);
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text.Length == 0)
			{
				throw new ArgumentException(SR.PathHasEmptyElement, "backSlashPath");
			}
		}
		return array;
	}

	internal static string ConvertStringArrayPathToBackSlashPath(IList arrayPath)
	{
		if (arrayPath == null || 1 > arrayPath.Count)
		{
			return string.Empty;
		}
		if (1 == arrayPath.Count)
		{
			return (string)arrayPath[0];
		}
		CheckStringForEmbeddedPathSeparator((string)arrayPath[0], "Path array element");
		StringBuilder stringBuilder = new StringBuilder((string)arrayPath[0]);
		for (int i = 1; i < arrayPath.Count; i++)
		{
			CheckStringForEmbeddedPathSeparator((string)arrayPath[i], "Path array element");
			stringBuilder.Append(PathSeparator);
			stringBuilder.Append((string)arrayPath[i]);
		}
		return stringBuilder.ToString();
	}

	internal static string ConvertStringArrayPathToBackSlashPath(IList storages, string streamName)
	{
		string text = ConvertStringArrayPathToBackSlashPath(storages);
		if (text.Length > 0)
		{
			return text + PathSeparator + streamName;
		}
		return streamName;
	}

	internal static void CheckStringAgainstNullAndEmpty(string testString, string testStringIdentifier)
	{
		if (testString == null)
		{
			throw new ArgumentNullException(testStringIdentifier);
		}
		if (testString.Length == 0)
		{
			throw new ArgumentException(SR.StringEmpty, testStringIdentifier);
		}
	}

	internal static void CheckStringAgainstReservedName(string nameString, string nameStringIdentifier)
	{
		if (IsReservedName(nameString))
		{
			throw new ArgumentException(SR.Format(SR.StringCanNotBeReservedName, nameStringIdentifier));
		}
	}

	internal static bool IsReservedName(string nameString)
	{
		CheckStringAgainstNullAndEmpty(nameString, "nameString");
		if (nameString[0] >= '\u0001')
		{
			return nameString[0] <= '\u001f';
		}
		return false;
	}

	internal static void CheckStringForEmbeddedPathSeparator(string testString, string testStringIdentifier)
	{
		CheckStringAgainstNullAndEmpty(testString, testStringIdentifier);
		if (testString.IndexOf(PathSeparator) != -1)
		{
			throw new ArgumentException(SR.Format(SR.NameCanNotHaveDelimiter, testStringIdentifier, PathSeparator), "testString");
		}
	}
}
