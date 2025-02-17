using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Runtime;

internal struct DependentHandle : IDisposable
{
	private sealed class DependentHolder : CriticalFinalizerObject
	{
		public GCHandle TargetHandle;

		private IntPtr dependent;

		public object? Dependent
		{
			get
			{
				return GCHandle.FromIntPtr(dependent).Target;
			}
			set
			{
				IntPtr value2 = GCHandle.ToIntPtr(GCHandle.Alloc(value, GCHandleType.Normal));
				IntPtr intPtr;
				do
				{
					intPtr = dependent;
				}
				while (Interlocked.CompareExchange(ref dependent, value2, intPtr) == intPtr);
				GCHandle.FromIntPtr(intPtr).Free();
			}
		}

		public DependentHolder(GCHandle targetHandle, object dependent)
		{
			TargetHandle = targetHandle;
			this.dependent = GCHandle.ToIntPtr(GCHandle.Alloc(dependent, GCHandleType.Normal));
		}

		~DependentHolder()
		{
			if (!AppDomain.CurrentDomain.IsFinalizingForUnload() && (!Environment.HasShutdownStarted && (TargetHandle.IsAllocated && TargetHandle.Target != null)))
			{
				GC.ReRegisterForFinalize(this);
			}
			else
			{
				GCHandle.FromIntPtr(dependent).Free();
			}
		}
	}

	private GCHandle dependentHandle;

	private volatile bool allocated;

	public bool IsAllocated => allocated;

	public object? Target
	{
		get
		{
			if (!allocated)
			{
				throw new InvalidOperationException();
			}
			return UnsafeGetTarget();
		}
		set
		{
			if (!allocated || value != null)
			{
				throw new InvalidOperationException();
			}
			UnsafeSetTargetToNull();
		}
	}

	public object? Dependent
	{
		get
		{
			if (!allocated)
			{
				throw new InvalidOperationException();
			}
			return UnsafeGetHolder()?.Dependent;
		}
		set
		{
			if (!allocated)
			{
				throw new InvalidOperationException();
			}
			UnsafeSetDependent(value);
		}
	}

	public (object? Target, object? Dependent) TargetAndDependent
	{
		get
		{
			if (!allocated)
			{
				throw new InvalidOperationException();
			}
			return (Target: UnsafeGetTarget(), Dependent: Dependent);
		}
	}

	public DependentHandle(object? target, object? dependent)
	{
		GCHandle targetHandle = GCHandle.Alloc(target, GCHandleType.WeakTrackResurrection);
		dependentHandle = AllocDepHolder(targetHandle, dependent);
		GC.KeepAlive(target);
		allocated = true;
	}

	private static GCHandle AllocDepHolder(GCHandle targetHandle, object? dependent)
	{
		return GCHandle.Alloc((dependent != null) ? new DependentHolder(targetHandle, dependent) : null, GCHandleType.WeakTrackResurrection);
	}

	private DependentHolder? UnsafeGetHolder()
	{
		return Unsafe.As<DependentHolder>(dependentHandle.Target);
	}

	internal object? UnsafeGetTarget()
	{
		return UnsafeGetHolder()?.TargetHandle.Target;
	}

	internal object? UnsafeGetTargetAndDependent(out object? dependent)
	{
		dependent = null;
		DependentHolder dependentHolder = UnsafeGetHolder();
		if (dependentHolder == null)
		{
			return null;
		}
		object target = dependentHolder.TargetHandle.Target;
		if (target == null)
		{
			return null;
		}
		dependent = dependentHolder.Dependent;
		return target;
	}

	internal void UnsafeSetTargetToNull()
	{
		Free();
	}

	internal void UnsafeSetDependent(object? value)
	{
		DependentHolder dependentHolder = UnsafeGetHolder();
		if (dependentHolder != null)
		{
			if (!dependentHolder.TargetHandle.IsAllocated)
			{
				Free();
			}
			else
			{
				dependentHolder.Dependent = value;
			}
		}
	}

	private void FreeDependentHandle()
	{
		if (allocated)
		{
			UnsafeGetHolder()?.TargetHandle.Free();
			dependentHandle.Free();
		}
		allocated = false;
	}

	private void Free()
	{
		FreeDependentHandle();
	}

	public void Dispose()
	{
		Free();
		allocated = false;
	}
}
