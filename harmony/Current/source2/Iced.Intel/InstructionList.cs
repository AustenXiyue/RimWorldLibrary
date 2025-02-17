using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Iced.Intel;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(InstructionListDebugView))]
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class InstructionList : IList<Instruction>, ICollection<Instruction>, IEnumerable<Instruction>, IEnumerable, IReadOnlyList<Instruction>, IReadOnlyCollection<Instruction>, IList, ICollection
{
	public struct Enumerator : IEnumerator<Instruction>, IDisposable, IEnumerator
	{
		private readonly InstructionList list;

		private int index;

		public ref Instruction Current => ref list.elements[index];

		Instruction IEnumerator<Instruction>.Current => list.elements[index];

		object IEnumerator.Current => list.elements[index];

		internal Enumerator(InstructionList list)
		{
			this.list = list;
			index = -1;
		}

		public bool MoveNext()
		{
			index++;
			return index < list.count;
		}

		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		public void Dispose()
		{
		}
	}

	private Instruction[] elements;

	private int count;

	public int Count => count;

	int ICollection<Instruction>.Count => count;

	int ICollection.Count => count;

	int IReadOnlyCollection<Instruction>.Count => count;

	public int Capacity => elements.Length;

	bool ICollection<Instruction>.IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool IList.IsFixedSize => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	public ref Instruction this[int index] => ref elements[index];

	Instruction IList<Instruction>.this[int index]
	{
		get
		{
			return elements[index];
		}
		set
		{
			elements[index] = value;
		}
	}

	Instruction IReadOnlyList<Instruction>.this[int index] => elements[index];

	object? IList.this[int index]
	{
		get
		{
			return elements[index];
		}
		set
		{
			if (value == null)
			{
				ThrowHelper.ThrowArgumentNullException_value();
			}
			if (!(value is Instruction))
			{
				ThrowHelper.ThrowArgumentException();
			}
			elements[index] = (Instruction)value;
		}
	}

	public InstructionList()
	{
		elements = Array2.Empty<Instruction>();
	}

	public InstructionList(int capacity)
	{
		if (capacity < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_capacity();
		}
		elements = ((capacity == 0) ? Array2.Empty<Instruction>() : new Instruction[capacity]);
	}

	public InstructionList(InstructionList list)
	{
		if (list == null)
		{
			ThrowHelper.ThrowArgumentNullException_list();
		}
		int num = list.count;
		if (num == 0)
		{
			elements = Array2.Empty<Instruction>();
			return;
		}
		Instruction[] destinationArray = (elements = new Instruction[num]);
		count = num;
		Array.Copy(list.elements, 0, destinationArray, 0, num);
	}

	public InstructionList(IEnumerable<Instruction> collection)
	{
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException_collection();
		}
		if (collection is ICollection<Instruction> { Count: var num } collection2)
		{
			if (num == 0)
			{
				elements = Array2.Empty<Instruction>();
				return;
			}
			collection2.CopyTo(elements = new Instruction[num], 0);
			count = num;
			return;
		}
		elements = Array2.Empty<Instruction>();
		foreach (Instruction item in collection)
		{
			Add(item);
		}
	}

	private void SetMinCapacity(int minCapacity)
	{
		Instruction[] array = elements;
		uint num = (uint)array.Length;
		if (minCapacity > (int)num)
		{
			uint num2 = num * 2;
			if (num2 < 4)
			{
				num2 = 4u;
			}
			if (num2 < (uint)minCapacity)
			{
				num2 = (uint)minCapacity;
			}
			if (num2 > 2146435071)
			{
				num2 = 2146435071u;
			}
			Instruction[] destinationArray = new Instruction[num2];
			Array.Copy(array, 0, destinationArray, 0, count);
			elements = destinationArray;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref Instruction AllocUninitializedElement()
	{
		int num = count;
		Instruction[] array = elements;
		if (num == array.Length)
		{
			SetMinCapacity(num + 1);
			array = elements;
		}
		count = num + 1;
		return ref array[num];
	}

	private void MakeRoom(int index, int extraLength)
	{
		SetMinCapacity(count + extraLength);
		int num = count - index;
		if (num != 0)
		{
			Instruction[] array = elements;
			Array.Copy(array, index, array, index + extraLength, num);
		}
	}

	public void Insert(int index, in Instruction instruction)
	{
		int num = count;
		if ((uint)index > (uint)num)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		MakeRoom(index, 1);
		elements[index] = instruction;
		count = num + 1;
	}

	void IList<Instruction>.Insert(int index, Instruction instruction)
	{
		Insert(index, in instruction);
	}

	void IList.Insert(int index, object value)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException_value();
		}
		if (!(value is Instruction))
		{
			ThrowHelper.ThrowArgumentException();
		}
		Insert(index, (Instruction)value);
	}

	public void RemoveAt(int index)
	{
		int num = count;
		if ((uint)index >= (uint)num)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		int num2 = (count = num - 1) - index;
		if (num2 != 0)
		{
			Instruction[] array = elements;
			Array.Copy(array, index + 1, array, index, num2);
		}
	}

	void IList<Instruction>.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}

	public void AddRange(IEnumerable<Instruction> collection)
	{
		InsertRange(count, collection);
	}

	public void InsertRange(int index, IEnumerable<Instruction> collection)
	{
		if ((uint)index > (uint)count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (collection == null)
		{
			ThrowHelper.ThrowArgumentNullException_collection();
		}
		if (collection is InstructionList { count: var num } instructionList)
		{
			if (num != 0)
			{
				MakeRoom(index, num);
				count += num;
				Array.Copy(instructionList.elements, 0, elements, index, num);
			}
			return;
		}
		if (collection is IList<Instruction> { Count: var num2 } list)
		{
			if (num2 != 0)
			{
				MakeRoom(index, num2);
				count += num2;
				Instruction[] array = elements;
				for (int i = 0; i < num2; i++)
				{
					array[index + i] = list[i];
				}
			}
			return;
		}
		if (collection is IReadOnlyList<Instruction> { Count: var num3 } readOnlyList)
		{
			if (num3 != 0)
			{
				MakeRoom(index, num3);
				count += num3;
				Instruction[] array2 = elements;
				for (int j = 0; j < num3; j++)
				{
					array2[index + j] = readOnlyList[j];
				}
			}
			return;
		}
		foreach (Instruction item in collection)
		{
			Instruction instruction = item;
			Insert(index++, in instruction);
		}
	}

	public void RemoveRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		if ((uint)(index + count) > (uint)this.count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		int num = this.count;
		int num2 = (this.count = num - count) - index;
		if (num2 != 0)
		{
			Instruction[] array = elements;
			Array.Copy(array, index + count, array, index, num2);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Add(in Instruction instruction)
	{
		int num = count;
		Instruction[] array = elements;
		if (num == array.Length)
		{
			SetMinCapacity(num + 1);
			array = elements;
		}
		array[num] = instruction;
		count = num + 1;
	}

	void ICollection<Instruction>.Add(Instruction instruction)
	{
		Add(in instruction);
	}

	int IList.Add(object value)
	{
		if (value == null)
		{
			ThrowHelper.ThrowArgumentNullException_value();
		}
		if (!(value is Instruction))
		{
			ThrowHelper.ThrowArgumentException();
		}
		Add((Instruction)value);
		return count - 1;
	}

	public void Clear()
	{
		count = 0;
	}

	void ICollection<Instruction>.Clear()
	{
		Clear();
	}

	void IList.Clear()
	{
		Clear();
	}

	public bool Contains(in Instruction instruction)
	{
		return IndexOf(in instruction) >= 0;
	}

	bool ICollection<Instruction>.Contains(Instruction instruction)
	{
		return Contains(in instruction);
	}

	bool IList.Contains(object value)
	{
		if (value is Instruction instruction)
		{
			return Contains(in instruction);
		}
		return false;
	}

	public int IndexOf(in Instruction instruction)
	{
		Instruction[] array = elements;
		int num = count;
		for (int i = 0; i < num; i++)
		{
			if (array[i] == instruction)
			{
				return i;
			}
		}
		return -1;
	}

	int IList<Instruction>.IndexOf(Instruction instruction)
	{
		return IndexOf(in instruction);
	}

	int IList.IndexOf(object value)
	{
		if (value is Instruction instruction)
		{
			return IndexOf(in instruction);
		}
		return -1;
	}

	public int IndexOf(in Instruction instruction, int index)
	{
		int num = count;
		if ((uint)index > (uint)num)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction[] array = elements;
		for (int i = index; i < num; i++)
		{
			if (array[i] == instruction)
			{
				return i;
			}
		}
		return -1;
	}

	public int IndexOf(in Instruction instruction, int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		int num = index + count;
		if ((uint)num > (uint)this.count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		Instruction[] array = elements;
		for (int i = index; i < num; i++)
		{
			if (array[i] == instruction)
			{
				return i;
			}
		}
		return -1;
	}

	public int LastIndexOf(in Instruction instruction)
	{
		for (int num = count - 1; num >= 0; num--)
		{
			if (elements[num] == instruction)
			{
				return num;
			}
		}
		return -1;
	}

	public int LastIndexOf(in Instruction instruction, int index)
	{
		int num = count;
		if ((uint)index > (uint)num)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		Instruction[] array = elements;
		for (int num2 = num - 1; num2 >= index; num2--)
		{
			if (array[num2] == instruction)
			{
				return num2;
			}
		}
		return -1;
	}

	public int LastIndexOf(in Instruction instruction, int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		int num = index + count;
		if ((uint)num > (uint)this.count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		Instruction[] array = elements;
		for (int num2 = num - 1; num2 >= index; num2--)
		{
			if (array[num2] == instruction)
			{
				return num2;
			}
		}
		return -1;
	}

	public bool Remove(in Instruction instruction)
	{
		int num = IndexOf(in instruction);
		if (num >= 0)
		{
			RemoveAt(num);
		}
		return num >= 0;
	}

	bool ICollection<Instruction>.Remove(Instruction instruction)
	{
		return Remove(in instruction);
	}

	void IList.Remove(object value)
	{
		if (value is Instruction instruction)
		{
			Remove(in instruction);
		}
	}

	public void CopyTo(Instruction[] array)
	{
		CopyTo(array, 0);
	}

	public void CopyTo(Instruction[] array, int arrayIndex)
	{
		Array.Copy(elements, 0, array, arrayIndex, count);
	}

	void ICollection<Instruction>.CopyTo(Instruction[] array, int arrayIndex)
	{
		CopyTo(array, arrayIndex);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		if (array == null)
		{
			ThrowHelper.ThrowArgumentNullException_array();
		}
		else if (array is Instruction[] array2)
		{
			CopyTo(array2, index);
		}
		else
		{
			ThrowHelper.ThrowArgumentException();
		}
	}

	public void CopyTo(int index, Instruction[] array, int arrayIndex, int count)
	{
		Array.Copy(elements, index, array, arrayIndex, count);
	}

	public InstructionList GetRange(int index, int count)
	{
		if (index < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_index();
		}
		if (count < 0)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		if ((uint)(index + count) > (uint)this.count)
		{
			ThrowHelper.ThrowArgumentOutOfRangeException_count();
		}
		InstructionList instructionList = new InstructionList(count);
		Array.Copy(elements, index, instructionList.elements, 0, count);
		instructionList.count = count;
		return instructionList;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<Instruction> IEnumerable<Instruction>.GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(this);
	}

	public ReadOnlyCollection<Instruction> AsReadOnly()
	{
		return new ReadOnlyCollection<Instruction>(this);
	}

	public Instruction[] ToArray()
	{
		int num = count;
		if (num == 0)
		{
			return Array2.Empty<Instruction>();
		}
		Instruction[] array = new Instruction[num];
		Array.Copy(elements, 0, array, 0, array.Length);
		return array;
	}
}
