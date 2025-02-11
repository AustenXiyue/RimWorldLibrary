using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MS.Internal;

internal abstract class CharacterBuffer : IList<char>, ICollection<char>, IEnumerable<char>, IEnumerable
{
	public abstract char this[int index] { get; set; }

	public abstract int Count { get; }

	public bool IsReadOnly => true;

	public unsafe abstract char* GetCharacterPointer();

	public abstract nint PinAndGetCharacterPointer(int offset, out GCHandle gcHandle);

	public abstract void UnpinCharacterPointer(GCHandle gcHandle);

	public abstract void AppendToStringBuilder(StringBuilder stringBuilder, int characterOffset, int length);

	public int IndexOf(char item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (item == this[i])
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, char item)
	{
		throw new NotSupportedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	public void Add(char item)
	{
		throw new NotSupportedException();
	}

	public void Clear()
	{
		throw new NotSupportedException();
	}

	public bool Contains(char item)
	{
		return IndexOf(item) != -1;
	}

	public void CopyTo(char[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[arrayIndex + i] = this[i];
		}
	}

	public bool Remove(char item)
	{
		throw new NotSupportedException();
	}

	IEnumerator<char> IEnumerable<char>.GetEnumerator()
	{
		int i = 0;
		while (i < Count)
		{
			yield return this[i];
			int num = i + 1;
			i = num;
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<char>)this).GetEnumerator();
	}
}
