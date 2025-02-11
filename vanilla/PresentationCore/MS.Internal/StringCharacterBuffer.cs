using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MS.Internal;

internal sealed class StringCharacterBuffer : CharacterBuffer
{
	private string _string;

	public override char this[int characterOffset]
	{
		get
		{
			return _string[characterOffset];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override int Count => _string.Length;

	public StringCharacterBuffer(string characterString)
	{
		if (characterString == null)
		{
			throw new ArgumentNullException("characterString");
		}
		_string = characterString;
	}

	public unsafe override char* GetCharacterPointer()
	{
		return null;
	}

	public unsafe override nint PinAndGetCharacterPointer(int offset, out GCHandle gcHandle)
	{
		gcHandle = GCHandle.Alloc(_string, GCHandleType.Pinned);
		return new IntPtr((byte*)((IntPtr)gcHandle.AddrOfPinnedObject()).ToPointer() + (nint)offset * (nint)2);
	}

	public override void UnpinCharacterPointer(GCHandle gcHandle)
	{
		gcHandle.Free();
	}

	public override void AppendToStringBuilder(StringBuilder stringBuilder, int characterOffset, int characterLength)
	{
		if (characterLength < 0 || characterOffset + characterLength > _string.Length)
		{
			characterLength = _string.Length - characterOffset;
		}
		stringBuilder.Append(_string, characterOffset, characterLength);
	}
}
