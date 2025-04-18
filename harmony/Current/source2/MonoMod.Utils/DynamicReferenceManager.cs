using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace MonoMod.Utils;

internal static class DynamicReferenceManager
{
	private abstract class Cell
	{
		public readonly nuint Type;

		protected Cell(nuint type)
		{
			Type = type;
		}
	}

	private class RefCell : Cell
	{
		public object? Value;

		public RefCell()
			: base(0u)
		{
		}
	}

	private abstract class ValueCellBase : Cell
	{
		public ValueCellBase()
			: base(1u)
		{
		}

		public abstract object? BoxValue();
	}

	private class ValueCell<T> : ValueCellBase
	{
		public T? Value;

		public override object? BoxValue()
		{
			return Value;
		}
	}

	private sealed class ScopeHandler : ScopeHandlerBase<DynamicReferenceCell>
	{
		public static readonly ScopeHandler Instance = new ScopeHandler();

		public override void EndScope(DynamicReferenceCell data)
		{
			bool lockTaken = false;
			try
			{
				writeLock.Enter(ref lockTaken);
				Cell[] cells = DynamicReferenceManager.cells;
				Cell cell = Volatile.Read(ref cells[data.Index]);
				if (cell != null && cell.GetHashCode() == data.Hash)
				{
					Volatile.Write(ref cells[data.Index], null);
					firstEmptyCell = Math.Min(firstEmptyCell, data.Index);
				}
			}
			finally
			{
				if (lockTaken)
				{
					writeLock.Exit();
				}
			}
		}
	}

	private const nuint RefValueCell = 0u;

	private const nuint ValueTypeCell = 1u;

	private static SpinLock writeLock = new SpinLock(enableThreadOwnerTracking: false);

	private static volatile Cell?[] cells = new Cell[16];

	private static volatile int firstEmptyCell;

	private static readonly MethodInfo Self_GetValue_ii = typeof(DynamicReferenceManager).GetMethod("GetValue", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
	{
		typeof(int),
		typeof(int)
	}, null) ?? throw new InvalidOperationException("GetValue doesn't exist?!?!?!?");

	private static readonly MethodInfo Self_GetValueT_ii = typeof(DynamicReferenceManager).GetMethod("GetValueT", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
	{
		typeof(int),
		typeof(int)
	}, null) ?? throw new InvalidOperationException("GetValueT doesn't exist?!?!?!?");

	private static readonly MethodInfo Self_GetValueTUnsafe_ii = typeof(DynamicReferenceManager).GetMethod("GetValueTUnsafe", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[2]
	{
		typeof(int),
		typeof(int)
	}, null) ?? throw new InvalidOperationException("GetValueTUnsafe doesn't exist?!?!?!?");

	private static DataScope<DynamicReferenceCell> AllocReferenceCore(Cell cell, out DynamicReferenceCell cellRef)
	{
		cellRef = default(DynamicReferenceCell);
		bool lockTaken = false;
		try
		{
			writeLock.Enter(ref lockTaken);
			Cell[] array = cells;
			int i = firstEmptyCell;
			if (i >= array.Length)
			{
				Cell[] destinationArray = new Cell[array.Length * 2];
				Array.Copy(array, destinationArray, array.Length);
				array = (cells = destinationArray);
			}
			int num = i++;
			for (; i < array.Length && array[i] != null; i++)
			{
			}
			firstEmptyCell = i;
			Volatile.Write(ref array[num], cell);
			cellRef = new DynamicReferenceCell(num, cell.GetHashCode());
		}
		finally
		{
			if (lockTaken)
			{
				writeLock.Exit();
			}
		}
		return new DataScope<DynamicReferenceCell>(ScopeHandler.Instance, cellRef);
	}

	private static DataScope<DynamicReferenceCell> AllocReferenceClass(object? value, out DynamicReferenceCell cellRef)
	{
		return AllocReferenceCore(new RefCell
		{
			Value = value
		}, out cellRef);
	}

	private static DataScope<DynamicReferenceCell> AllocReferenceStruct<T>(in T value, out DynamicReferenceCell cellRef)
	{
		return AllocReferenceCore(new ValueCell<T>
		{
			Value = value
		}, out cellRef);
	}

	[MethodImpl((MethodImplOptions)512)]
	public static DataScope<DynamicReferenceCell> AllocReference<T>(in T? value, out DynamicReferenceCell cellRef)
	{
		if (default(T) == null)
		{
			return AllocReferenceClass(Unsafe.As<T, object>(ref Unsafe.AsRef(in value)), out cellRef);
		}
		return AllocReferenceStruct(in value, out cellRef);
	}

	private static Cell GetCell(DynamicReferenceCell cellRef)
	{
		Cell cell = Volatile.Read(ref cells[cellRef.Index]);
		if (cell == null || cell.GetHashCode() != cellRef.Hash)
		{
			throw new ArgumentException("Referenced cell no longer exists", "cellRef");
		}
		return cell;
	}

	public static object? GetValue(DynamicReferenceCell cellRef)
	{
		Cell cell = GetCell(cellRef);
		return (ulong)cell.Type switch
		{
			0uL => Unsafe.As<RefCell>(cell).Value, 
			1uL => Unsafe.As<ValueCellBase>(cell).BoxValue(), 
			_ => throw new InvalidOperationException("Cell is not of valid type"), 
		};
	}

	[MethodImpl((MethodImplOptions)512)]
	private static ref T? GetValueRef<T>(DynamicReferenceCell cellRef)
	{
		Cell cell = GetCell(cellRef);
		switch (cell.Type)
		{
		case 0uL:
		{
			Helpers.Assert(default(T) == null, null, "default(T) == null");
			RefCell refCell = Unsafe.As<RefCell>(cell);
			object value = refCell.Value;
			bool value2 = ((value == null || value is T) ? true : false);
			Helpers.Assert(value2, null, "c.Value is null or T");
			return ref Unsafe.As<object, T>(ref refCell.Value);
		}
		case 1uL:
			Helpers.Assert(default(T) != null, null, "default(T) != null");
			return ref ((ValueCell<T>)cell).Value;
		default:
			throw new InvalidOperationException("Cell is not of valid type");
		}
	}

	[MethodImpl((MethodImplOptions)512)]
	private static ref T? GetValueRefUnsafe<T>(DynamicReferenceCell cellRef)
	{
		Cell cell = GetCell(cellRef);
		if (default(T) == null)
		{
			return ref Unsafe.As<object, T>(ref Unsafe.As<RefCell>(cell).Value);
		}
		return ref Unsafe.As<ValueCell<T>>(cell).Value;
	}

	public static T? GetValue<T>(DynamicReferenceCell cellRef)
	{
		return GetValueRef<T>(cellRef);
	}

	internal static object? GetValue(int index, int hash)
	{
		return GetValue(new DynamicReferenceCell(index, hash));
	}

	internal static T? GetValueT<T>(int index, int hash)
	{
		return GetValue<T>(new DynamicReferenceCell(index, hash));
	}

	internal static T? GetValueTUnsafe<T>(int index, int hash)
	{
		return GetValueRefUnsafe<T>(new DynamicReferenceCell(index, hash));
	}

	public static void SetValue<T>(DynamicReferenceCell cellRef, in T? value)
	{
		GetValueRef<T>(cellRef) = value;
	}

	public static void EmitLoadReference(this ILProcessor il, DynamicReferenceCell cellRef)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValue_ii));
	}

	public static void EmitLoadReference(this ILCursor il, DynamicReferenceCell cellRef)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValue_ii));
	}

	public static void EmitLoadReference(this ILGenerator il, DynamicReferenceCell cellRef)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(System.Reflection.Emit.OpCodes.Call, Self_GetValue_ii);
	}

	public static void EmitLoadTypedReference(this ILProcessor il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValueT_ii.MakeGenericMethod(type)));
	}

	public static void EmitLoadTypedReference(this ILCursor il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValueT_ii.MakeGenericMethod(type)));
	}

	public static void EmitLoadTypedReference(this ILGenerator il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(System.Reflection.Emit.OpCodes.Call, Self_GetValueT_ii.MakeGenericMethod(type));
	}

	internal static void EmitLoadTypedReferenceUnsafe(this ILProcessor il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValueTUnsafe_ii.MakeGenericMethod(type)));
	}

	internal static void EmitLoadTypedReferenceUnsafe(this ILCursor il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(Mono.Cecil.Cil.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(Mono.Cecil.Cil.OpCodes.Call, il.Body.Method.Module.ImportReference(Self_GetValueTUnsafe_ii.MakeGenericMethod(type)));
	}

	internal static void EmitLoadTypedReferenceUnsafe(this ILGenerator il, DynamicReferenceCell cellRef, Type type)
	{
		Helpers.ThrowIfArgumentNull(il, "il");
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Index);
		il.Emit(System.Reflection.Emit.OpCodes.Ldc_I4, cellRef.Hash);
		il.Emit(System.Reflection.Emit.OpCodes.Call, Self_GetValueTUnsafe_ii.MakeGenericMethod(type));
	}

	public static DataScope<DynamicReferenceCell> EmitNewReference(this ILProcessor il, object? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadReference(cellRef);
		return result;
	}

	public static DataScope<DynamicReferenceCell> EmitNewReference(this ILCursor il, object? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadReference(cellRef);
		return result;
	}

	public static DataScope<DynamicReferenceCell> EmitNewReference(this ILGenerator il, object? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadReference(cellRef);
		return result;
	}

	public static DataScope<DynamicReferenceCell> EmitNewTypedReference<T>(this ILProcessor il, T? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadTypedReferenceUnsafe(cellRef, typeof(T));
		return result;
	}

	public static DataScope<DynamicReferenceCell> EmitNewTypedReference<T>(this ILCursor il, T? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadTypedReferenceUnsafe(cellRef, typeof(T));
		return result;
	}

	public static DataScope<DynamicReferenceCell> EmitNewTypedReference<T>(this ILGenerator il, T? value, out DynamicReferenceCell cellRef)
	{
		DataScope<DynamicReferenceCell> result = AllocReference(in value, out cellRef);
		il.EmitLoadTypedReferenceUnsafe(cellRef, typeof(T));
		return result;
	}
}
