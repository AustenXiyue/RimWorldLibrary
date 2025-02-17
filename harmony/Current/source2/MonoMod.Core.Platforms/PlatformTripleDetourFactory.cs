using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms;

internal sealed class PlatformTripleDetourFactory : IDetourFactory
{
	private abstract class DetourBase : ICoreDetourBase, IDisposable
	{
		protected abstract class DetourBoxBase
		{
			public SimpleNativeDetour? Detour;

			protected readonly PlatformTriple Triple;

			protected readonly object Sync = new object();

			private bool applyDetours;

			private bool isApplying;

			public bool IsApplied
			{
				get
				{
					return Volatile.Read(ref applyDetours);
				}
				set
				{
					Volatile.Write(ref applyDetours, value);
					Thread.MemoryBarrier();
				}
			}

			public bool IsApplying
			{
				get
				{
					return Volatile.Read(ref isApplying);
				}
				set
				{
					Volatile.Write(ref isApplying, value);
					Thread.MemoryBarrier();
				}
			}

			protected DetourBoxBase(PlatformTriple triple)
			{
				Triple = triple;
				applyDetours = false;
				isApplying = false;
			}
		}

		protected readonly PlatformTriple Triple;

		protected DetourBoxBase DetourBox;

		private bool disposedValue;

		public bool IsApplied => DetourBox.IsApplied;

		protected DetourBase(PlatformTriple triple)
		{
			Triple = triple;
			DetourBox = null;
		}

		protected TBox GetDetourBox<TBox>() where TBox : DetourBoxBase
		{
			return Unsafe.As<TBox>(DetourBox);
		}

		protected static void ReplaceDetourInLock(DetourBoxBase nativeDetour, SimpleNativeDetour? newDetour, out SimpleNativeDetour? oldDetour)
		{
			Thread.MemoryBarrier();
			oldDetour = Interlocked.Exchange(ref nativeDetour.Detour, newDetour);
		}

		protected abstract SimpleNativeDetour CreateDetour();

		public void Apply()
		{
			lock (DetourBox)
			{
				if (IsApplied)
				{
					throw new InvalidOperationException("Cannot apply a detour which is already applied");
				}
				try
				{
					DetourBox.IsApplying = true;
					DetourBox.IsApplied = true;
					ReplaceDetourInLock(DetourBox, CreateDetour(), out SimpleNativeDetour _);
				}
				catch
				{
					DetourBox.IsApplied = false;
					throw;
				}
				finally
				{
					DetourBox.IsApplying = false;
				}
			}
		}

		protected abstract void BeforeUndo();

		protected abstract void AfterUndo();

		public void Undo()
		{
			lock (DetourBox)
			{
				if (!IsApplied)
				{
					throw new InvalidOperationException("Cannot undo a detour which is not applied");
				}
				try
				{
					DetourBox.IsApplying = true;
					UndoCore(out SimpleNativeDetour oldDetour);
					oldDetour?.Dispose();
				}
				finally
				{
					DetourBox.IsApplying = false;
				}
			}
		}

		private void UndoCore(out SimpleNativeDetour? oldDetour)
		{
			BeforeUndo();
			DetourBox.IsApplied = false;
			ReplaceDetourInLock(DetourBox, null, out oldDetour);
			AfterUndo();
		}

		protected abstract void BeforeDispose();

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				BeforeDispose();
				lock (DetourBox)
				{
					UndoCore(out SimpleNativeDetour oldDetour);
					oldDetour?.Dispose();
				}
				disposedValue = true;
			}
		}

		~DetourBase()
		{
			Dispose(disposing: false);
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}

	private sealed class Detour : DetourBase, ICoreDetour, ICoreDetourBase, IDisposable
	{
		private sealed class ManagedDetourBox : DetourBoxBase
		{
			private readonly MethodBase src;

			private readonly MethodBase target;

			public ManagedDetourBox(PlatformTriple triple, MethodBase src, MethodBase target)
				: base(triple)
			{
				this.src = src;
				this.target = target;
				Detour = null;
			}

			public void SubscribeCompileMethod()
			{
				AddRelatedDetour(src, this);
			}

			public void UnsubscribeCompileMethod()
			{
				RemoveRelatedDetour(src, this);
			}

			public void OnMethodCompiled(MethodBase method, IntPtr codeStart, IntPtr codeStartRw, ulong codeSize)
			{
				if (!base.IsApplied)
				{
					return;
				}
				method = Triple.GetIdentifiable(method);
				lock (Sync)
				{
					if (!base.IsApplied || base.IsApplying)
					{
						return;
					}
					bool isEnabled;
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(43, 4, out isEnabled);
					if (isEnabled)
					{
						message.AppendLiteral("Updating detour from ");
						message.AppendFormatted(src);
						message.AppendLiteral(" to ");
						message.AppendFormatted(target);
						message.AppendLiteral(" (recompiled ");
						message.AppendFormatted(method);
						message.AppendLiteral(" to ");
						message.AppendFormatted(codeStart, "x16");
						message.AppendLiteral(")");
					}
					_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
					try
					{
						base.IsApplying = true;
						SimpleNativeDetour detour = Detour;
						IntPtr to;
						IntPtr from;
						IntPtr fromRw;
						if (detour != null)
						{
							_ = detour.Source;
							to = detour.Destination;
							from = codeStart;
							fromRw = codeStartRw;
						}
						else
						{
							from = codeStart;
							fromRw = codeStartRw;
							to = Triple.Runtime.GetMethodHandle(target).GetFunctionPointer();
						}
						SimpleNativeDetour newDetour = Triple.CreateSimpleDetour(from, to, (int)codeSize, fromRw);
						DetourBase.ReplaceDetourInLock(this, newDetour, out SimpleNativeDetour _);
					}
					finally
					{
						base.IsApplying = false;
					}
				}
			}
		}

		private sealed class RelatedDetourBag
		{
			public readonly MethodBase Method;

			public readonly List<ManagedDetourBox> RelatedDetours = new List<ManagedDetourBox>();

			public bool IsValid = true;

			public RelatedDetourBag(MethodBase method)
			{
				Method = method;
			}
		}

		private readonly MethodBase realTarget;

		private static readonly object subLock = new object();

		private static bool hasSubscribed;

		private static readonly ConcurrentDictionary<MethodBase, RelatedDetourBag> relatedDetours = new ConcurrentDictionary<MethodBase, RelatedDetourBag>();

		private IDisposable? srcPin;

		private IDisposable? dstPin;

		private new ManagedDetourBox DetourBox => GetDetourBox<ManagedDetourBox>();

		public MethodBase Source { get; }

		public MethodBase Target { get; }

		public Detour(PlatformTriple triple, MethodBase src, MethodBase dst)
			: base(triple)
		{
			Source = triple.GetIdentifiable(src);
			Target = dst;
			realTarget = triple.GetRealDetourTarget(src, dst);
			base.DetourBox = new ManagedDetourBox(triple, Source, realTarget);
			if (triple.SupportedFeatures.Has(RuntimeFeature.CompileMethodHook))
			{
				EnsureSubscribed(triple);
				DetourBox.SubscribeCompileMethod();
			}
		}

		private static void EnsureSubscribed(PlatformTriple triple)
		{
			if (Volatile.Read(ref hasSubscribed))
			{
				return;
			}
			lock (subLock)
			{
				if (!Volatile.Read(ref hasSubscribed))
				{
					Volatile.Write(ref hasSubscribed, value: true);
					triple.Runtime.OnMethodCompiled += OnMethodCompiled;
				}
			}
		}

		private static void AddRelatedDetour(MethodBase m, ManagedDetourBox cmh)
		{
			while (true)
			{
				RelatedDetourBag orAdd = relatedDetours.GetOrAdd(m, (MethodBase m) => new RelatedDetourBag(m));
				lock (orAdd)
				{
					if (!orAdd.IsValid)
					{
						continue;
					}
					orAdd.RelatedDetours.Add(cmh);
					if (orAdd.RelatedDetours.Count > 1)
					{
						bool isEnabled;
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(115, 1, out isEnabled);
						if (isEnabled)
						{
							message.AppendLiteral("Multiple related detours for method ");
							message.AppendFormatted(m);
							message.AppendLiteral("! This means that the method has been detoured twice. Detour cleanup will fail.");
						}
						_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
					}
					break;
				}
			}
		}

		private static void RemoveRelatedDetour(MethodBase m, ManagedDetourBox cmh)
		{
			if (relatedDetours.TryGetValue(m, out RelatedDetourBag value))
			{
				lock (value)
				{
					value.RelatedDetours.Remove(cmh);
					if (value.RelatedDetours.Count == 0)
					{
						value.IsValid = false;
						Helpers.Assert(relatedDetours.TryRemove(value.Method, out RelatedDetourBag _), null, "relatedDetours.TryRemove(related.Method, out _)");
					}
					return;
				}
			}
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(79, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Attempted to remove a related detour from method ");
				message.AppendFormatted(m);
				message.AppendLiteral(" which has no RelatedDetourBag");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
		}

		private static void OnMethodCompiled(RuntimeMethodHandle methodHandle, MethodBase? method, IntPtr codeStart, IntPtr codeStartRw, ulong codeSize)
		{
			if ((object)method == null)
			{
				return;
			}
			method = PlatformTriple.Current.GetIdentifiable(method);
			if (!relatedDetours.TryGetValue(method, out RelatedDetourBag value))
			{
				return;
			}
			lock (value)
			{
				foreach (ManagedDetourBox relatedDetour in value.RelatedDetours)
				{
					relatedDetour.OnMethodCompiled(method, codeStart, codeStartRw, codeSize);
				}
			}
		}

		protected override SimpleNativeDetour CreateDetour()
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(33, 2, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Applying managed detour from ");
				message.AppendFormatted(Source);
				message.AppendLiteral(" to ");
				message.AppendFormatted(realTarget);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
			srcPin = Triple.PinMethodIfNeeded(Source);
			dstPin = Triple.PinMethodIfNeeded(realTarget);
			Triple.Compile(Source);
			IntPtr nativeMethodBody = Triple.GetNativeMethodBody(Source);
			Triple.Compile(realTarget);
			IntPtr functionPointer = Triple.Runtime.GetMethodHandle(realTarget).GetFunctionPointer();
			return Triple.CreateSimpleDetour(nativeMethodBody, functionPointer, -1, (IntPtr)0);
		}

		protected override void BeforeUndo()
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(32, 2, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Undoing managed detour from ");
				message.AppendFormatted(Source);
				message.AppendLiteral(" to ");
				message.AppendFormatted(realTarget);
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		}

		protected override void AfterUndo()
		{
			Interlocked.Exchange(ref srcPin, null)?.Dispose();
			Interlocked.Exchange(ref dstPin, null)?.Dispose();
		}

		protected override void BeforeDispose()
		{
			if (Triple.SupportedFeatures.Has(RuntimeFeature.CompileMethodHook))
			{
				DetourBox.UnsubscribeCompileMethod();
			}
		}
	}

	private sealed class NativeDetour : DetourBase, ICoreNativeDetour, ICoreDetourBase, IDisposable
	{
		private sealed class NativeDetourBox : DetourBoxBase
		{
			public readonly IntPtr From;

			public readonly IntPtr To;

			public NativeDetourBox(PlatformTriple triple, IntPtr from, IntPtr to)
				: base(triple)
			{
				From = from;
				To = to;
			}
		}

		private IDisposable? origHandle;

		public IntPtr Source => DetourBox.From;

		public IntPtr Target => DetourBox.To;

		public bool HasOrigEntrypoint => OrigEntrypoint != IntPtr.Zero;

		public IntPtr OrigEntrypoint { get; private set; }

		private new NativeDetourBox DetourBox => GetDetourBox<NativeDetourBox>();

		public NativeDetour(PlatformTriple triple, IntPtr from, IntPtr to)
			: base(triple)
		{
			base.DetourBox = new NativeDetourBox(triple, from, to);
		}

		protected override SimpleNativeDetour CreateDetour()
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(32, 2, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Applying native detour from ");
				message.AppendFormatted(Source, "x16");
				message.AppendLiteral(" to ");
				message.AppendFormatted(Target, "x16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
			Triple.CreateNativeDetour(Source, Target, -1, (IntPtr)0).Deconstruct(out SimpleNativeDetour Simple, out IntPtr AltEntry, out IDisposable AltHandle);
			SimpleNativeDetour result = Simple;
			IntPtr origEntrypoint = AltEntry;
			IDisposable disposable = AltHandle;
			IDisposable? disposable2 = origHandle;
			AltHandle = disposable;
			disposable = disposable2;
			origHandle = AltHandle;
			OrigEntrypoint = origEntrypoint;
			return result;
		}

		protected override void BeforeUndo()
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogTraceStringHandler(31, 2, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Undoing native detour from ");
				message.AppendFormatted(Source, "x16");
				message.AppendLiteral(" to ");
				message.AppendFormatted(Target, "x16");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Trace(ref message);
		}

		protected override void AfterUndo()
		{
			OrigEntrypoint = IntPtr.Zero;
			origHandle?.Dispose();
			origHandle = null;
		}

		protected override void BeforeDispose()
		{
		}
	}

	private readonly PlatformTriple triple;

	public PlatformTripleDetourFactory(PlatformTriple triple)
	{
		this.triple = triple;
	}

	public ICoreDetour CreateDetour(CreateDetourRequest request)
	{
		Helpers.ThrowIfArgumentNull(request.Source, "request.Source");
		Helpers.ThrowIfArgumentNull(request.Target, "request.Target");
		if (!triple.TryDisableInlining(request.Source))
		{
			bool isEnabled;
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler message = new _003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.DebugLogWarningStringHandler(66, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("Could not disable inlining of method ");
				message.AppendFormatted(request.Source);
				message.AppendLiteral("; detours may not be reliable");
			}
			_003C027f1d0e_002D6e0b_002D4adc_002Dbc2b_002Da5d0603c6ea8_003EMMDbgLog.Warning(ref message);
		}
		Detour detour = new Detour(triple, request.Source, request.Target);
		if (request.ApplyByDefault)
		{
			detour.Apply();
		}
		return detour;
	}

	public ICoreNativeDetour CreateNativeDetour(CreateNativeDetourRequest request)
	{
		NativeDetour nativeDetour = new NativeDetour(triple, request.Source, request.Target);
		if (request.ApplyByDefault)
		{
			nativeDetour.Apply();
		}
		return nativeDetour;
	}
}
