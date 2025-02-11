using System;

namespace MonoMod.Core.Platforms;

internal sealed class SimpleNativeDetour : IDisposable
{
	private bool disposedValue;

	private readonly PlatformTriple triple;

	private NativeDetourInfo detourInfo;

	private Memory<byte> backup;

	private IDisposable? AllocHandle;

	public ReadOnlyMemory<byte> DetourBackup => backup;

	public IntPtr Source => detourInfo.From;

	public IntPtr Destination => detourInfo.To;

	internal SimpleNativeDetour(PlatformTriple triple, NativeDetourInfo detourInfo, Memory<byte> backup, IDisposable? allocHandle)
	{
		this.triple = triple;
		this.detourInfo = detourInfo;
		this.backup = backup;
		AllocHandle = allocHandle;
	}

	public void ChangeTarget(IntPtr newTarget)
	{
		CheckDisposed();
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(47, 3, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Retargeting simple detour 0x");
			message.AppendFormatted(Source, "x16");
			message.AppendLiteral(" => 0x");
			message.AppendFormatted(Destination, "x16");
			message.AppendLiteral(" to target 0x");
			message.AppendFormatted(newTarget, "x16");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		NativeDetourInfo retarget = triple.Architecture.ComputeRetargetInfo(detourInfo, newTarget, detourInfo.Size);
		Span<byte> span = stackalloc byte[retarget.Size];
		triple.Architecture.GetRetargetBytes(detourInfo, retarget, span, out IDisposable allocationHandle, out bool needsRepatch, out bool disposeOldAlloc);
		if (needsRepatch)
		{
			byte[] array = null;
			if (retarget.Size > backup.Length)
			{
				array = new byte[retarget.Size];
			}
			triple.System.PatchData(PatchTargetKind.Executable, Source, span, array);
			if (array != null)
			{
				backup.Span.CopyTo(array);
				backup = array;
			}
		}
		detourInfo = retarget;
		IDisposable? allocHandle = AllocHandle;
		IDisposable allocHandle2 = allocationHandle;
		allocationHandle = allocHandle;
		AllocHandle = allocHandle2;
		if (disposeOldAlloc)
		{
			allocationHandle?.Dispose();
		}
	}

	public void Undo()
	{
		CheckDisposed();
		UndoCore(disposing: true);
	}

	private void CheckDisposed()
	{
		if (disposedValue)
		{
			throw new ObjectDisposedException("SimpleNativeDetour");
		}
	}

	private void UndoCore(bool disposing)
	{
		bool isEnabled;
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(30, 2, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("Undoing simple detour 0x");
			message.AppendFormatted(Source, "x16");
			message.AppendLiteral(" => 0x");
			message.AppendFormatted(Destination, "x16");
		}
		_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		triple.System.PatchData(PatchTargetKind.Executable, Source, DetourBackup.Span, default(Span<byte>));
		if (disposing)
		{
			Cleanup();
		}
		disposedValue = true;
	}

	private void Cleanup()
	{
		AllocHandle?.Dispose();
	}

	private void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			UndoCore(disposing);
			disposedValue = true;
		}
	}

	~SimpleNativeDetour()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
