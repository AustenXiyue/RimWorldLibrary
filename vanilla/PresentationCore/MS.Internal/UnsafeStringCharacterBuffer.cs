using System;
using System.Runtime.InteropServices;
using System.Text;
using MS.Internal.PresentationCore;

namespace MS.Internal;

internal sealed class UnsafeStringCharacterBuffer : CharacterBuffer
{
	private unsafe char* _unsafeString;

	private int _length;

	public unsafe override char this[int characterOffset]
	{
		get
		{
			if (characterOffset >= _length || characterOffset < 0)
			{
				throw new ArgumentOutOfRangeException("characterOffset", SR.Format(SR.ParameterMustBeBetween, 0, _length));
			}
			return _unsafeString[characterOffset];
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public override int Count => _length;

	public unsafe UnsafeStringCharacterBuffer(char* characterString, int length)
	{
		if (characterString == null)
		{
			throw new ArgumentNullException("characterString");
		}
		if (length <= 0)
		{
			throw new ArgumentOutOfRangeException("length", SR.ParameterValueMustBeGreaterThanZero);
		}
		_unsafeString = characterString;
		_length = length;
	}

	public unsafe override char* GetCharacterPointer()
	{
		return _unsafeString;
	}

	public unsafe override nint PinAndGetCharacterPointer(int offset, out GCHandle gcHandle)
	{
		gcHandle = default(GCHandle);
		return new IntPtr(_unsafeString + offset);
	}

	public override void UnpinCharacterPointer(GCHandle gcHandle)
	{
	}

	public unsafe override void AppendToStringBuilder(StringBuilder stringBuilder, int characterOffset, int characterLength)
	{
		if (characterOffset >= _length || characterOffset < 0)
		{
			throw new ArgumentOutOfRangeException("characterOffset", SR.Format(SR.ParameterMustBeBetween, 0, _length));
		}
		if (characterLength < 0 || characterOffset + characterLength > _length)
		{
			throw new ArgumentOutOfRangeException("characterLength", SR.Format(SR.ParameterMustBeBetween, 0, _length - characterOffset));
		}
		stringBuilder.Append(new string(_unsafeString, characterOffset, characterLength));
	}
}
