using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MS.Internal;

internal sealed class CharArrayCharacterBuffer : CharacterBuffer
{
	private char[] _characterArray;

	public override char this[int characterOffset]
	{
		get
		{
			return _characterArray[characterOffset];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override int Count => _characterArray.Length;

	public CharArrayCharacterBuffer(char[] characterArray)
	{
		if (characterArray == null)
		{
			throw new ArgumentNullException("characterArray");
		}
		_characterArray = characterArray;
	}

	public unsafe override char* GetCharacterPointer()
	{
		return null;
	}

	public unsafe override nint PinAndGetCharacterPointer(int offset, out GCHandle gcHandle)
	{
		gcHandle = GCHandle.Alloc(_characterArray, GCHandleType.Pinned);
		return new IntPtr((byte*)((IntPtr)gcHandle.AddrOfPinnedObject()).ToPointer() + (nint)offset * (nint)2);
	}

	public override void UnpinCharacterPointer(GCHandle gcHandle)
	{
		gcHandle.Free();
	}

	public override void AppendToStringBuilder(StringBuilder stringBuilder, int characterOffset, int characterLength)
	{
		if (characterLength < 0 || characterOffset + characterLength > _characterArray.Length)
		{
			characterLength = _characterArray.Length - characterOffset;
		}
		stringBuilder.Append(_characterArray, characterOffset, characterLength);
	}
}
