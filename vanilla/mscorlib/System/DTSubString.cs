namespace System;

internal struct DTSubString
{
	internal string s;

	internal int index;

	internal int length;

	internal DTSubStringType type;

	internal int value;

	internal char this[int relativeIndex] => s[index + relativeIndex];
}
