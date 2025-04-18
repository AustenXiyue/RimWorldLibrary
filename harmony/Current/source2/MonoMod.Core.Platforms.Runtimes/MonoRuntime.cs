using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms.Runtimes;

internal sealed class MonoRuntime : IRuntime
{
	private sealed class PrivateMethodPin
	{
		private readonly MonoRuntime runtime;

		public MethodPinInfo Pin;

		public PrivateMethodPin(MonoRuntime runtime)
		{
			this.runtime = runtime;
		}

		public void UnpinOnce()
		{
			runtime.UnpinOnce(this);
		}
	}

	private sealed class PinHandle : IDisposable
	{
		private readonly PrivateMethodPin pin;

		private bool disposedValue;

		public PinHandle(PrivateMethodPin pin)
		{
			this.pin = pin;
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				pin.UnpinOnce();
				disposedValue = true;
			}
		}

		~PinHandle()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	private struct MethodPinInfo
	{
		public int Count;

		public MethodBase Method;

		public RuntimeMethodHandle Handle;

		public override string ToString()
		{
			return $"(MethodPinInfo: {Count}, {Method}, 0x{(long)Handle.Value:X})";
		}
	}

	private readonly ISystem system;

	private static readonly MethodInfo _DynamicMethod_CreateDynMethod = typeof(DynamicMethod).GetMethod("CreateDynMethod", BindingFlags.Instance | BindingFlags.NonPublic);

	private static readonly FieldInfo _DynamicMethod_mhandle = typeof(DynamicMethod).GetField("mhandle", BindingFlags.Instance | BindingFlags.NonPublic);

	private readonly ConcurrentDictionary<MethodBase, PrivateMethodPin> pinnedMethods = new ConcurrentDictionary<MethodBase, PrivateMethodPin>();

	private readonly ConcurrentDictionary<RuntimeMethodHandle, PrivateMethodPin> pinnedHandles = new ConcurrentDictionary<RuntimeMethodHandle, PrivateMethodPin>();

	public RuntimeKind Target => RuntimeKind.Mono;

	public RuntimeFeature Features => RuntimeFeature.PreciseGC | RuntimeFeature.GenericSharing | RuntimeFeature.DisableInlining | RuntimeFeature.RequiresMethodPinning | RuntimeFeature.RequiresMethodIdentification | RuntimeFeature.RequiresCustomMethodCompile;

	public Abi Abi { get; }

	public event OnMethodCompiledCallback? OnMethodCompiled;

	private static TypeClassification LinuxAmd64Classifier(Type type, bool isReturn)
	{
		if (type.IsEnum)
		{
			type = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).First().FieldType;
		}
		switch (Type.GetTypeCode(type))
		{
		case TypeCode.Empty:
			return TypeClassification.InRegister;
		case TypeCode.Object:
		case TypeCode.DBNull:
		case TypeCode.String:
			return TypeClassification.InRegister;
		case TypeCode.Boolean:
		case TypeCode.Char:
		case TypeCode.SByte:
		case TypeCode.Byte:
		case TypeCode.Int16:
		case TypeCode.UInt16:
		case TypeCode.Int32:
		case TypeCode.UInt32:
		case TypeCode.Int64:
		case TypeCode.UInt64:
			return TypeClassification.InRegister;
		case TypeCode.Single:
		case TypeCode.Double:
			return TypeClassification.InRegister;
		default:
			if (type.IsPointer)
			{
				return TypeClassification.InRegister;
			}
			if (type.IsByRef)
			{
				return TypeClassification.InRegister;
			}
			if (type == typeof(IntPtr) || type == typeof(UIntPtr))
			{
				return TypeClassification.InRegister;
			}
			if (type == typeof(void))
			{
				return TypeClassification.InRegister;
			}
			Helpers.Assert(type.IsValueType, null, "type.IsValueType");
			return ClassifyValueType(type, isReturn: true);
		}
	}

	private static TypeClassification ClassifyValueType(Type type, bool isReturn)
	{
		int managedSize = type.GetManagedSize();
		bool flag = (!isReturn || managedSize != 8) && (isReturn || managedSize > 16);
		if (managedSize == 0)
		{
			return TypeClassification.InRegister;
		}
		if (flag)
		{
			if (!isReturn)
			{
				return TypeClassification.OnStack;
			}
			return TypeClassification.ByReference;
		}
		int num = ((managedSize <= 8) ? 1 : 2);
		int num2 = 1;
		int num3 = 1;
		if (isReturn && num != 1)
		{
			num2 = (num3 = 2);
		}
		if (num2 == 2 || num3 == 2)
		{
			num2 = 2;
		}
		return num2 switch
		{
			1 => TypeClassification.InRegister, 
			2 => TypeClassification.OnStack, 
			_ => throw new InvalidOperationException(), 
		};
	}

	private static IEnumerable<FieldInfo> NestedValutypeFields(Type type)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			if (fieldInfo.FieldType.IsValueType)
			{
				foreach (FieldInfo item in NestedValutypeFields(fieldInfo.FieldType))
				{
					yield return item;
				}
			}
			else
			{
				yield return fieldInfo;
			}
		}
	}

	public MonoRuntime(ISystem system)
	{
		this.system = system;
		Abi? defaultAbi = system.DefaultAbi;
		if (defaultAbi.HasValue)
		{
			Abi abi = defaultAbi.GetValueOrDefault();
			OSKind kernel = PlatformDetection.OS.GetKernel();
			if (((kernel == OSKind.OSX || kernel == OSKind.Linux) ? true : false) && PlatformDetection.Architecture == ArchitectureKind.x86_64)
			{
				abi = abi with
				{
					Classifier = LinuxAmd64Classifier
				};
			}
			kernel = PlatformDetection.OS;
			bool flag = ((kernel == OSKind.Windows || kernel == OSKind.Wine) ? true : false);
			if (flag)
			{
				ArchitectureKind architecture = PlatformDetection.Architecture;
				flag = (uint)(architecture - 2) <= 1u;
			}
			if (flag)
			{
				abi = abi with
				{
					ArgumentOrder = new SpecialArgumentKind[3]
					{
						SpecialArgumentKind.ThisPointer,
						SpecialArgumentKind.ReturnBuffer,
						SpecialArgumentKind.UserArguments
					}
				};
			}
			Abi = abi;
			return;
		}
		throw new InvalidOperationException("Cannot use Mono system, because the underlying system doesn't provide a default ABI!");
	}

	public unsafe void DisableInlining(MethodBase method)
	{
		ushort* ptr = (ushort*)((long)GetMethodHandle(method).Value + 2);
		*ptr |= 8;
	}

	public RuntimeMethodHandle GetMethodHandle(MethodBase method)
	{
		if (method is DynamicMethod)
		{
			_DynamicMethod_CreateDynMethod?.Invoke(method, ArrayEx.Empty<object>());
			if (_DynamicMethod_mhandle != null)
			{
				return (RuntimeMethodHandle)_DynamicMethod_mhandle.GetValue(method);
			}
		}
		return method.MethodHandle;
	}

	public IDisposable? PinMethodIfNeeded(MethodBase method)
	{
		method = GetIdentifiable(method);
		PrivateMethodPin orAdd = pinnedMethods.GetOrAdd(method, delegate(MethodBase m)
		{
			PrivateMethodPin privateMethodPin = new PrivateMethodPin(this)
			{
				Pin = 
				{
					Method = m
				}
			};
			RuntimeMethodHandle key = (privateMethodPin.Pin.Handle = GetMethodHandle(m));
			pinnedHandles[key] = privateMethodPin;
			DisableInlining(method);
			_ = method.DeclaringType?.IsGenericType ?? false;
			return privateMethodPin;
		});
		Interlocked.Increment(ref orAdd.Pin.Count);
		return new PinHandle(orAdd);
	}

	private void UnpinOnce(PrivateMethodPin pin)
	{
		if (Interlocked.Decrement(ref pin.Pin.Count) <= 0)
		{
			pinnedMethods.TryRemove(pin.Pin.Method, out PrivateMethodPin value);
			pinnedHandles.TryRemove(pin.Pin.Handle, out value);
		}
	}

	public MethodBase GetIdentifiable(MethodBase method)
	{
		if (!pinnedHandles.TryGetValue(GetMethodHandle(method), out PrivateMethodPin value))
		{
			return method;
		}
		return value.Pin.Method;
	}

	public IntPtr GetMethodEntryPoint(MethodBase method)
	{
		if (pinnedMethods.TryGetValue(method, out PrivateMethodPin value))
		{
			return value.Pin.Handle.GetFunctionPointer();
		}
		return GetMethodHandle(method).GetFunctionPointer();
	}

	public void Compile(MethodBase method)
	{
		GetMethodHandle(method).GetFunctionPointer();
	}
}
