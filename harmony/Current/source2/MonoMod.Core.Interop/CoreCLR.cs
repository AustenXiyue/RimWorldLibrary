using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MonoMod.Core.Interop.Attributes;
using MonoMod.Utils;

namespace MonoMod.Core.Interop;

internal static class CoreCLR
{
	public enum CorJitResult
	{
		CORJIT_OK
	}

	public readonly struct InvokeCompileMethodPtr
	{
		private readonly IntPtr methodPtr;

		public unsafe delegate*<IntPtr, IntPtr, IntPtr, V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> InvokeCompileMethod => (delegate*<IntPtr, IntPtr, IntPtr, V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult>)(void*)methodPtr;

		public unsafe InvokeCompileMethodPtr(delegate*<IntPtr, IntPtr, IntPtr, V21.CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> ptr)
		{
			methodPtr = (IntPtr)ptr;
		}
	}

	public class V21
	{
		public struct CORINFO_SIG_INST
		{
			public uint classInstCount;

			public unsafe IntPtr* classInst;

			public uint methInstCount;

			public unsafe IntPtr* methInst;
		}

		public struct CORINFO_SIG_INFO
		{
			public int callConv;

			public IntPtr retTypeClass;

			public IntPtr retTypeSigClass;

			public byte retType;

			public byte flags;

			public ushort numArgs;

			public CORINFO_SIG_INST sigInst;

			public IntPtr args;

			public IntPtr pSig;

			public uint sbSig;

			public IntPtr scope;

			public uint token;
		}

		public struct CORINFO_METHOD_INFO
		{
			public IntPtr ftn;

			public IntPtr scope;

			public unsafe byte* ILCode;

			public uint ILCodeSize;

			public uint maxStack;

			public uint EHcount;

			public int options;

			public int regionKind;

			public CORINFO_SIG_INFO args;

			public CORINFO_SIG_INFO locals;
		}

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		public unsafe delegate CorJitResult CompileMethodDelegate(IntPtr thisPtr, IntPtr corJitInfo, CORINFO_METHOD_INFO* methodInfo, uint flags, byte** nativeEntry, uint* nativeSizeOfCode);

		public unsafe static InvokeCompileMethodPtr InvokeCompileMethodPtr => new InvokeCompileMethodPtr((delegate*<IntPtr, IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult>)(&InvokeCompileMethod));

		public unsafe static CorJitResult InvokeCompileMethod(IntPtr functionPtr, IntPtr thisPtr, IntPtr corJitInfo, CORINFO_METHOD_INFO* methodInfo, uint flags, byte** pNativeEntry, uint* pNativeSizeOfCode)
		{
			if (functionPtr == IntPtr.Zero)
			{
				*pNativeEntry = null;
				*pNativeSizeOfCode = 0u;
				return CorJitResult.CORJIT_OK;
			}
			delegate* unmanaged[Stdcall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> delegate_002A = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult>)(void*)functionPtr;
			delegate* unmanaged[Stdcall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> delegate_002A2 = delegate_002A;
			/*Error near IL_002b: Handle with invalid row number.*/;
		}
	}

	public class V30 : V21
	{
	}

	public class V31 : V30
	{
	}

	public class V50 : V31
	{
	}

	public class V60 : V50
	{
		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public new unsafe delegate CorJitResult CompileMethodDelegate(IntPtr thisPtr, IntPtr corJitInfo, CORINFO_METHOD_INFO* methodInfo, uint flags, byte** nativeEntry, uint* nativeSizeOfCode);

		public enum MethodClassification
		{
			IL,
			FCall,
			NDirect,
			EEImpl,
			Array,
			Instantiated,
			ComInterop,
			Dynamic
		}

		[Flags]
		public enum MethodDescClassification : ushort
		{
			ClassificationMask = 7,
			HasNonVtableSlot = 8,
			MethodImpl = 0x10,
			HasNativeCodeSlot = 0x20,
			HasComPlusCallInfo = 0x40,
			Static = 0x80,
			Duplicate = 0x400,
			VerifiedState = 0x800,
			Verifiable = 0x1000,
			NotInline = 0x2000,
			Synchronized = 0x4000,
			RequiresFullSlotNumber = 0x8000
		}

		public struct RelativePointer
		{
			private nint m_delta;

			public unsafe void* Value
			{
				get
				{
					nint delta = m_delta;
					if (delta != 0)
					{
						return Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref this, delta));
					}
					return null;
				}
			}

			public RelativePointer(nint delta)
			{
				m_delta = delta;
			}
		}

		public struct RelativeFixupPointer
		{
			private nint m_delta;

			public const nint FIXUP_POINTER_INDIRECTION = 1;

			public unsafe void* Value
			{
				get
				{
					nint delta = m_delta;
					if (delta == 0)
					{
						return null;
					}
					nint num = (nint)Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref this, delta));
					if ((num & 1) != 0)
					{
						num = *(nint*)(num - 1);
					}
					return (void*)num;
				}
			}
		}

		public struct MethodDesc
		{
			[Flags]
			public enum Flags3 : ushort
			{
				TokenRemainderMask = 0x3FFF,
				HasForwardedValuetypeParameter = 0x4000,
				ValueTypeParametersWalked = 0x4000,
				DoesNotHaveEquivalentValuetypeParameters = 0x8000
			}

			[Flags]
			public enum Flags2 : byte
			{
				HasStableEntryPoint = 1,
				HasPrecode = 2,
				IsUnboxingStub = 4,
				IsJitIntrinsic = 0x10,
				IsEligibleForTieredCompilation = 0x20,
				RequiresCovariantReturnTypeChecking = 0x40
			}

			public static readonly nuint Alignment = (nuint)((IntPtr.Size == 8) ? 8 : 4);

			public Flags3 m_wFlags3AndTokenRemainder;

			public byte m_chunkIndex;

			public Flags2 m_bFlags2;

			public const ushort PackedSlot_SlotMask = 1023;

			public const ushort PackedSlot_NameHashMask = 64512;

			public ushort m_wSlotNumber;

			public MethodDescClassification m_wFlags;

			private unsafe static readonly nuint[] s_ClassificationSizeTable = new UIntPtr[8]
			{
				(nuint)sizeof(MethodDesc),
				(nuint)FCallMethodDescPtr.CurrentSize,
				(nuint)NDirectMethodDescPtr.CurrentSize,
				(nuint)EEImplMethodDescPtr.CurrentSize,
				(nuint)ArrayMethodDescPtr.CurrentSize,
				(nuint)sizeof(InstantiatedMethodDesc),
				(nuint)sizeof(ComPlusCallMethodDesc),
				(nuint)DynamicMethodDescPtr.CurrentSize
			};

			public ushort SlotNumber
			{
				get
				{
					if (!m_wFlags.Has(MethodDescClassification.RequiresFullSlotNumber))
					{
						return (ushort)(m_wSlotNumber & 0x3FF);
					}
					return m_wSlotNumber;
				}
			}

			public MethodClassification Classification => (MethodClassification)(m_wFlags & MethodDescClassification.ClassificationMask);

			public unsafe MethodDescChunk* MethodDescChunk => (MethodDescChunk*)((byte*)Unsafe.AsPointer(ref this) - (nuint)((nint)sizeof(MethodDescChunk) + (nint)(m_chunkIndex * Alignment)));

			public unsafe MethodTable* MethodTable => MethodDescChunk->m_methodTable;

			public bool HasNonVtableSlot => m_wFlags.Has(MethodDescClassification.HasNonVtableSlot);

			public bool HasStableEntryPoint => m_bFlags2.Has(Flags2.HasStableEntryPoint);

			public bool HasPrecode => m_bFlags2.Has(Flags2.HasPrecode);

			public bool HasNativeCodeSlot => m_wFlags.Has(MethodDescClassification.HasNativeCodeSlot);

			public bool IsUnboxingStub => m_bFlags2.Has(Flags2.IsUnboxingStub);

			public unsafe bool HasMethodInstantiation
			{
				get
				{
					if (TryAsInstantiated(out var md))
					{
						return md->IMD_HasMethodInstantiation;
					}
					return false;
				}
			}

			public unsafe bool IsGenericMethodDefinition
			{
				get
				{
					if (TryAsInstantiated(out var md))
					{
						return md->IMD_IsGenericMethodDefinition;
					}
					return false;
				}
			}

			public unsafe bool IsInstantiatingStub
			{
				get
				{
					if (!IsUnboxingStub && TryAsInstantiated(out var md))
					{
						return md->IMD_IsWrapperStubWithInstantiations;
					}
					return false;
				}
			}

			public bool IsWrapperStub
			{
				get
				{
					if (!IsUnboxingStub)
					{
						return IsInstantiatingStub;
					}
					return true;
				}
			}

			public bool IsTightlyBoundToMethodTable
			{
				get
				{
					if (!HasNonVtableSlot)
					{
						return true;
					}
					if (HasMethodInstantiation)
					{
						return IsGenericMethodDefinition;
					}
					if (IsWrapperStub)
					{
						return false;
					}
					return true;
				}
			}

			public unsafe void* GetMethodEntryPoint()
			{
				if (HasNonVtableSlot)
				{
					nuint baseSize = GetBaseSize();
					byte* ptr = (byte*)Unsafe.AsPointer(ref this) + baseSize;
					if (!MethodDescChunk->m_flagsAndTokenRange.Has(V60.MethodDescChunk.Flags.IsZapped))
					{
						return *(void**)ptr;
					}
					return new RelativePointer((nint)ptr).Value;
				}
				return MethodTable->GetSlot(SlotNumber);
			}

			public unsafe bool TryAsFCall(out FCallMethodDescPtr md)
			{
				if (Classification == MethodClassification.FCall)
				{
					md = new FCallMethodDescPtr(Unsafe.AsPointer(ref this), FCallMethodDescPtr.CurrentVtable);
					return true;
				}
				md = default(FCallMethodDescPtr);
				return false;
			}

			public unsafe bool TryAsNDirect(out NDirectMethodDescPtr md)
			{
				if (Classification == MethodClassification.NDirect)
				{
					md = new NDirectMethodDescPtr(Unsafe.AsPointer(ref this), NDirectMethodDescPtr.CurrentVtable);
					return true;
				}
				md = default(NDirectMethodDescPtr);
				return false;
			}

			public unsafe bool TryAsEEImpl(out EEImplMethodDescPtr md)
			{
				if (Classification == MethodClassification.EEImpl)
				{
					md = new EEImplMethodDescPtr(Unsafe.AsPointer(ref this), EEImplMethodDescPtr.CurrentVtable);
					return true;
				}
				md = default(EEImplMethodDescPtr);
				return false;
			}

			public unsafe bool TryAsArray(out ArrayMethodDescPtr md)
			{
				if (Classification == MethodClassification.Array)
				{
					md = new ArrayMethodDescPtr(Unsafe.AsPointer(ref this), ArrayMethodDescPtr.CurrentVtable);
					return true;
				}
				md = default(ArrayMethodDescPtr);
				return false;
			}

			public unsafe bool TryAsInstantiated(out InstantiatedMethodDesc* md)
			{
				if (Classification == MethodClassification.Instantiated)
				{
					md = (InstantiatedMethodDesc*)Unsafe.AsPointer(ref this);
					return true;
				}
				md = default(InstantiatedMethodDesc*);
				return false;
			}

			public unsafe bool TryAsComPlusCall(out ComPlusCallMethodDesc* md)
			{
				if (Classification == MethodClassification.ComInterop)
				{
					md = (ComPlusCallMethodDesc*)Unsafe.AsPointer(ref this);
					return true;
				}
				md = default(ComPlusCallMethodDesc*);
				return false;
			}

			public unsafe bool TryAsDynamic(out DynamicMethodDescPtr md)
			{
				if (Classification == MethodClassification.Dynamic)
				{
					md = new DynamicMethodDescPtr(Unsafe.AsPointer(ref this), DynamicMethodDescPtr.CurrentVtable);
					return true;
				}
				md = default(DynamicMethodDescPtr);
				return false;
			}

			public unsafe nuint SizeOf(bool includeNonVtable = true, bool includeMethodImpl = true, bool includeComPlus = true, bool includeNativeCode = true)
			{
				nuint num = (nuint)((nint)GetBaseSize() + (nint)((includeNonVtable && m_wFlags.Has(MethodDescClassification.HasNonVtableSlot)) ? sizeof(void*) : 0) + ((includeMethodImpl && m_wFlags.Has(MethodDescClassification.MethodImpl)) ? ((nint)sizeof(void*) * (nint)2) : 0) + ((includeComPlus && m_wFlags.Has(MethodDescClassification.HasComPlusCallInfo)) ? sizeof(void*) : 0) + ((includeNativeCode && m_wFlags.Has(MethodDescClassification.HasNativeCodeSlot)) ? sizeof(void*) : 0));
				if (includeNativeCode && HasNativeCodeSlot)
				{
					num += (nuint)((((nuint)GetAddrOfNativeCodeSlot() & (nuint)1u) != 0) ? sizeof(void*) : 0);
				}
				return num;
			}

			public unsafe void* GetNativeCode()
			{
				if (HasNativeCodeSlot)
				{
					void* ptr = *(void**)((nuint)GetAddrOfNativeCodeSlot() & (nuint)(~(nint)1));
					if (ptr != null)
					{
						return ptr;
					}
				}
				if (!HasStableEntryPoint || HasPrecode)
				{
					return null;
				}
				return GetStableEntryPoint();
			}

			public unsafe void* GetStableEntryPoint()
			{
				return GetMethodEntryPoint();
			}

			public unsafe static MethodDesc* FindTightlyBoundWrappedMethodDesc(MethodDesc* pMD)
			{
				if (pMD->IsUnboxingStub && pMD->TryAsInstantiated(out var md))
				{
					pMD = md->IMD_GetWrappedMethodDesc();
				}
				if (!pMD->IsTightlyBoundToMethodTable)
				{
					pMD = pMD->GetCanonicalMethodTable()->GetParallelMethodDesc(pMD);
				}
				if (pMD->IsUnboxingStub)
				{
					pMD = GetNextIntroducedMethod(pMD);
				}
				return pMD;
			}

			public unsafe static MethodDesc* GetNextIntroducedMethod(MethodDesc* pMD)
			{
				MethodDescChunk* methodDescChunk = pMD->MethodDescChunk;
				nuint num = (nuint)((byte*)pMD + pMD->SizeOf());
				nuint num2 = (nuint)((byte*)methodDescChunk + methodDescChunk->SizeOf);
				if (num < num2)
				{
					return (MethodDesc*)num;
				}
				methodDescChunk = methodDescChunk->m_next;
				if (methodDescChunk != null)
				{
					return methodDescChunk->FirstMethodDesc;
				}
				return null;
			}

			public unsafe MethodTable* GetCanonicalMethodTable()
			{
				return MethodTable->GetCanonicalMethodTable();
			}

			public unsafe void* GetAddrOfNativeCodeSlot()
			{
				nuint byteOffset = SizeOf(includeNonVtable: true, includeMethodImpl: true, includeComPlus: false, includeNativeCode: false);
				return Unsafe.AsPointer(ref Unsafe.AddByteOffset(ref this, byteOffset));
			}

			public nuint GetBaseSize()
			{
				return GetBaseSize(Classification);
			}

			public static nuint GetBaseSize(MethodClassification classification)
			{
				return s_ClassificationSizeTable[(int)classification];
			}
		}

		public struct MethodDescChunk
		{
			[Flags]
			public enum Flags : ushort
			{
				TokenRangeMask = 0x3FF,
				HasCompactEntrypoints = 0x4000,
				IsZapped = 0x8000
			}

			public unsafe MethodTable* m_methodTable;

			public unsafe MethodDescChunk* m_next;

			public byte m_size;

			public byte m_count;

			public Flags m_flagsAndTokenRange;

			public unsafe MethodDesc* FirstMethodDesc => (MethodDesc*)((byte*)Unsafe.AsPointer(ref this) + sizeof(MethodDescChunk));

			public uint Size => (uint)(m_size + 1);

			public uint Count => (uint)(m_count + 1);

			public unsafe nuint SizeOf => (nuint)sizeof(MethodDescChunk) + Size * MethodDesc.Alignment;
		}

		[FatInterface]
		public struct StoredSigMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (IntPtr.Size == 8) ? StoredSigMethodDesc_64.FatVtable_ : StoredSigMethodDesc_32.FatVtable_;

			public unsafe static int CurrentSize { get; } = (IntPtr.Size == 8) ? sizeof(StoredSigMethodDesc_64) : sizeof(StoredSigMethodDesc_32);

			public unsafe void* m_pSig
			{
				[FatInterfaceIgnore]
				get
				{
					return GetPSig();
				}
			}

			public uint m_cSig
			{
				[FatInterfaceIgnore]
				get
				{
					return GetCSig();
				}
			}

			private unsafe void* GetPSig()
			{
				return ((delegate*<void*, void*>)(void*)vtbl_[0])(ptr_);
			}

			private unsafe uint GetCSig()
			{
				return ((delegate*<void*, uint>)(void*)vtbl_[0])(ptr_);
			}

			public unsafe StoredSigMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		[FatInterfaceImpl(typeof(StoredSigMethodDescPtr))]
		public struct StoredSigMethodDesc_64
		{
			public MethodDesc @base;

			public unsafe void* m_pSig;

			public uint m_cSig;

			public uint m_dwExtendedFlags;

			private static IntPtr[]? fatVtable_;

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[2]
						{
							(IntPtr)(delegate*<void*, void*>)(&S_GetPSig_0),
							(IntPtr)(delegate*<void*, uint>)(&S_GetCSig_1)
						};
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static uint S_GetCSig_1(void* ptr__)
					{
						return ((StoredSigMethodDesc_64*)ptr__)->GetCSig();
					}
					unsafe static void* S_GetPSig_0(void* ptr__)
					{
						return ((StoredSigMethodDesc_64*)ptr__)->GetPSig();
					}
				}
			}

			private unsafe void* GetPSig()
			{
				return m_pSig;
			}

			private uint GetCSig()
			{
				return m_cSig;
			}
		}

		[FatInterfaceImpl(typeof(StoredSigMethodDescPtr))]
		public struct StoredSigMethodDesc_32
		{
			public MethodDesc @base;

			public unsafe void* m_pSig;

			public uint m_cSig;

			private static IntPtr[]? fatVtable_;

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[2]
						{
							(IntPtr)(delegate*<void*, void*>)(&S_GetPSig_0),
							(IntPtr)(delegate*<void*, uint>)(&S_GetCSig_1)
						};
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static uint S_GetCSig_1(void* ptr__)
					{
						return ((StoredSigMethodDesc_32*)ptr__)->GetCSig();
					}
					unsafe static void* S_GetPSig_0(void* ptr__)
					{
						return ((StoredSigMethodDesc_32*)ptr__)->GetPSig();
					}
				}
			}

			private unsafe void* GetPSig()
			{
				return m_pSig;
			}

			private uint GetCSig()
			{
				return m_cSig;
			}
		}

		[FatInterface]
		public struct FCallMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (IntPtr.Size == 8) ? FCallMethodDesc_64.FatVtable_ : FCallMethodDesc_32.FatVtable_;

			public unsafe static int CurrentSize { get; } = (IntPtr.Size == 8) ? sizeof(FCallMethodDesc_64) : sizeof(FCallMethodDesc_32);

			public uint m_dwECallID
			{
				[FatInterfaceIgnore]
				get
				{
					return GetECallID();
				}
			}

			private unsafe uint GetECallID()
			{
				return ((delegate*<void*, uint>)(void*)vtbl_[0])(ptr_);
			}

			public unsafe FCallMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		[FatInterfaceImpl(typeof(FCallMethodDescPtr))]
		public struct FCallMethodDesc_64
		{
			public MethodDesc @base;

			public uint m_dwECallID;

			public uint m_padding;

			private static IntPtr[]? fatVtable_;

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[1] { (IntPtr)(delegate*<void*, uint>)(&S_GetECallID_0) };
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static uint S_GetECallID_0(void* ptr__)
					{
						return ((FCallMethodDesc_64*)ptr__)->GetECallID();
					}
				}
			}

			private uint GetECallID()
			{
				return m_dwECallID;
			}
		}

		[FatInterfaceImpl(typeof(FCallMethodDescPtr))]
		public struct FCallMethodDesc_32
		{
			public MethodDesc @base;

			public uint m_dwECallID;

			private static IntPtr[]? fatVtable_;

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[1] { (IntPtr)(delegate*<void*, uint>)(&S_GetECallID_0) };
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static uint S_GetECallID_0(void* ptr__)
					{
						return ((FCallMethodDesc_32*)ptr__)->GetECallID();
					}
				}
			}

			private uint GetECallID()
			{
				return m_dwECallID;
			}
		}

		public struct DynamicResolver
		{
		}

		[Flags]
		public enum DynamicMethodDesc_ExtendedFlags
		{
			Attrs = 0xFFFF,
			ILStubAttrs = 0x17,
			MemberAccessMask = 7,
			ReverseStub = 8,
			Static = 0x10,
			CALLIStub = 0x20,
			DelegateStub = 0x40,
			StructMarshalStub = 0x80,
			Unbreakable = 0x100,
			SignatureNeedsResture = 0x400,
			StubNeedsCOMStarted = 0x800,
			MulticastStub = 0x1000,
			UnboxingILStub = 0x2000,
			WrapperDelegateStub = 0x4000,
			UnmanagedCallersOnlyStub = 0x8000,
			ILStub = 0x10000,
			LCGMethod = 0x20000,
			StackArgSize = 0xFFC0000
		}

		[FatInterface]
		public struct DynamicMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (IntPtr.Size == 8) ? DynamicMethodDesc_64.FatVtable_ : DynamicMethodDesc_32.FatVtable_;

			public unsafe static int CurrentSize { get; } = (IntPtr.Size == 8) ? sizeof(DynamicMethodDesc_64) : sizeof(DynamicMethodDesc_32);

			public DynamicMethodDesc_ExtendedFlags Flags => GetFlags();

			private unsafe DynamicMethodDesc_ExtendedFlags GetFlags()
			{
				return ((delegate*<void*, DynamicMethodDesc_ExtendedFlags>)(void*)vtbl_[0])(ptr_);
			}

			public unsafe DynamicMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		[FatInterfaceImpl(typeof(DynamicMethodDescPtr))]
		public struct DynamicMethodDesc_64
		{
			public StoredSigMethodDesc_64 @base;

			public unsafe byte* m_pszMethodName;

			public unsafe DynamicResolver* m_pResolver;

			private static IntPtr[]? fatVtable_;

			public DynamicMethodDesc_ExtendedFlags Flags => GetFlags();

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[1] { (IntPtr)(delegate*<void*, DynamicMethodDesc_ExtendedFlags>)(&S_GetFlags_0) };
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static DynamicMethodDesc_ExtendedFlags S_GetFlags_0(void* ptr__)
					{
						return ((DynamicMethodDesc_64*)ptr__)->GetFlags();
					}
				}
			}

			private DynamicMethodDesc_ExtendedFlags GetFlags()
			{
				return (DynamicMethodDesc_ExtendedFlags)@base.m_dwExtendedFlags;
			}
		}

		[FatInterfaceImpl(typeof(DynamicMethodDescPtr))]
		public struct DynamicMethodDesc_32
		{
			public StoredSigMethodDesc_32 @base;

			public unsafe byte* m_pszMethodName;

			public unsafe DynamicResolver* m_pResolver;

			public uint m_dwExtendedFlags;

			private static IntPtr[]? fatVtable_;

			public DynamicMethodDesc_ExtendedFlags Flags => GetFlags();

			public unsafe static IntPtr[] FatVtable_
			{
				get
				{
					object obj = fatVtable_;
					if (obj == null)
					{
						obj = new IntPtr[1] { (IntPtr)(delegate*<void*, DynamicMethodDesc_ExtendedFlags>)(&S_GetFlags_0) };
						fatVtable_ = (IntPtr[]?)obj;
					}
					return (IntPtr[])obj;
					unsafe static DynamicMethodDesc_ExtendedFlags S_GetFlags_0(void* ptr__)
					{
						return ((DynamicMethodDesc_32*)ptr__)->GetFlags();
					}
				}
			}

			private DynamicMethodDesc_ExtendedFlags GetFlags()
			{
				return (DynamicMethodDesc_ExtendedFlags)m_dwExtendedFlags;
			}
		}

		[FatInterface]
		public struct ArrayMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (IntPtr.Size == 8) ? ArrayMethodDesc_64.FatVtable_ : ArrayMethodDesc_32.FatVtable_;

			public unsafe static int CurrentSize { get; } = (IntPtr.Size == 8) ? sizeof(ArrayMethodDesc_64) : sizeof(ArrayMethodDesc_32);

			public unsafe ArrayMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		public enum ArrayFunc
		{
			Get,
			Set,
			Address,
			Ctor
		}

		[FatInterfaceImpl(typeof(ArrayMethodDescPtr))]
		public struct ArrayMethodDesc_64
		{
			public StoredSigMethodDesc_64 @base;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		[FatInterfaceImpl(typeof(ArrayMethodDescPtr))]
		public struct ArrayMethodDesc_32
		{
			public StoredSigMethodDesc_32 @base;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		public struct NDirectWriteableData
		{
		}

		[Flags]
		public enum NDirectMethodDesc_Flags : ushort
		{
			EarlyBound = 1,
			HasSuppressUnmanagedCodeAccess = 2,
			DefaultDllImportSearchPathIsCached = 4,
			IsMarshalingRequiredCached = 0x10,
			CachedMarshalingRequired = 0x20,
			NativeAnsi = 0x40,
			LastError = 0x80,
			NativeNoMangle = 0x100,
			VarArgs = 0x200,
			StdCall = 0x400,
			ThisCall = 0x800,
			IsQCall = 0x1000,
			DefaultDllImportSearchPathsStatus = 0x2000,
			NDirectPopulated = 0x8000
		}

		[FatInterface]
		public struct NDirectMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (PlatformDetection.Architecture == ArchitectureKind.x86) ? NDirectMethodDesc_x86.FatVtable_ : NDirectMethodDesc_other.FatVtable_;

			public unsafe static int CurrentSize { get; } = (PlatformDetection.Architecture == ArchitectureKind.x86) ? sizeof(NDirectMethodDesc_x86) : sizeof(NDirectMethodDesc_other);

			public unsafe NDirectMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		[FatInterfaceImpl(typeof(NDirectMethodDescPtr))]
		public struct NDirectMethodDesc_other
		{
			public struct NDirect
			{
				public unsafe void* m_pNativeNDirectTarget;

				public unsafe byte* m_pszEntrypointName;

				public nuint union_pszLibName_dwECallID;

				public unsafe NDirectWriteableData* m_pWriteableData;

				public unsafe void* m_pImportThunkGlue;

				public uint m_DefaultDllImportSearchPathsAttributeValue;

				public NDirectMethodDesc_Flags m_wFlags;

				public unsafe MethodDesc* m_pStubMD;
			}

			public MethodDesc @base;

			private NDirect ndirect;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		[FatInterfaceImpl(typeof(NDirectMethodDescPtr))]
		public struct NDirectMethodDesc_x86
		{
			public struct NDirect
			{
				public unsafe void* m_pNativeNDirectTarget;

				public unsafe byte* m_pszEntrypointName;

				public nuint union_pszLibName_dwECallID;

				public unsafe NDirectWriteableData* m_pWriteableData;

				public unsafe void* m_pImportThunkGlue;

				public uint m_DefaultDllImportSearchPathsAttributeValue;

				public NDirectMethodDesc_Flags m_wFlags;

				public ushort m_cbStackArgumentSize;

				public unsafe MethodDesc* m_pStubMD;
			}

			public MethodDesc @base;

			private NDirect ndirect;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		[FatInterface]
		public struct EEImplMethodDescPtr
		{
			private unsafe readonly void* ptr_;

			private readonly IntPtr[] vtbl_;

			public static IntPtr[] CurrentVtable { get; } = (IntPtr.Size == 8) ? EEImplMethodDesc_64.FatVtable_ : EEImplMethodDesc_32.FatVtable_;

			public unsafe static int CurrentSize { get; } = (IntPtr.Size == 8) ? sizeof(EEImplMethodDesc_64) : sizeof(EEImplMethodDesc_32);

			public unsafe EEImplMethodDescPtr(void* ptr, IntPtr[] vtbl)
			{
				ptr_ = ptr;
				vtbl_ = vtbl;
			}
		}

		[FatInterfaceImpl(typeof(EEImplMethodDescPtr))]
		public struct EEImplMethodDesc_64
		{
			public StoredSigMethodDesc_64 @base;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		[FatInterfaceImpl(typeof(EEImplMethodDescPtr))]
		public struct EEImplMethodDesc_32
		{
			public StoredSigMethodDesc_32 @base;

			private static IntPtr[]? fatVtable_;

			public static IntPtr[] FatVtable_ => fatVtable_ ?? (fatVtable_ = new IntPtr[0]);
		}

		public struct ComPlusCallMethodDesc
		{
			public MethodDesc @base;

			public unsafe void* m_pComPlusCallInfo;
		}

		public struct InstantiatedMethodDesc
		{
			[Flags]
			public enum Flags : ushort
			{
				KindMask = 7,
				GenericMethodDefinition = 0,
				UnsharedMethodInstantiation = 1,
				SharedMethodInstantiation = 2,
				WrapperStubWithInstantiations = 3,
				EnCAddedMethod = 7,
				Unrestored = 8,
				HasComPlusCallInfo = 0x10
			}

			public MethodDesc @base;

			public unsafe void* union_pDictLayout_pWrappedMethodDesc;

			public unsafe Dictionary* m_pPerInstInfo;

			public Flags m_wFlags2;

			public ushort m_wNumGenericArgs;

			public unsafe bool IMD_HasMethodInstantiation
			{
				get
				{
					if (!IMD_IsGenericMethodDefinition)
					{
						return m_pPerInstInfo != null;
					}
					return true;
				}
			}

			public bool IMD_IsGenericMethodDefinition => (m_wFlags2 & Flags.KindMask) == 0;

			public bool IMD_IsWrapperStubWithInstantiations => (m_wFlags2 & Flags.KindMask) == Flags.WrapperStubWithInstantiations;

			public unsafe MethodDesc* IMD_GetWrappedMethodDesc()
			{
				Helpers.Assert(IMD_IsWrapperStubWithInstantiations, null, "IMD_IsWrapperStubWithInstantiations");
				return (MethodDesc*)union_pDictLayout_pWrappedMethodDesc;
			}
		}

		public struct Dictionary
		{
		}

		public struct Module
		{
		}

		public struct MethodTableWriteableData
		{
		}

		public struct VTableIndir2_t
		{
			public unsafe void* pCode;

			public unsafe void* Value => pCode;
		}

		public struct VTableIndir_t
		{
			public unsafe VTableIndir2_t* Value;
		}

		private static class MultipurposeSlotHelpers
		{
			public unsafe static byte OffsetOfMp1()
			{
				MethodTable methodTable = default(MethodTable);
				return (byte)((byte*)(&methodTable.union_pPerInstInfo_ElementTypeHnd_pMultipurposeSlot1) - (byte*)(&methodTable));
			}

			public unsafe static byte OffsetOfMp2()
			{
				MethodTable methodTable = default(MethodTable);
				return (byte)((byte*)(&methodTable.union_p_InterfaceMap_pMultipurposeSlot2) - (byte*)(&methodTable));
			}

			public unsafe static byte RegularOffset(int index)
			{
				return (byte)(sizeof(MethodTable) + index * IntPtr.Size - 2 * IntPtr.Size);
			}
		}

		public struct MethodTable
		{
			[Flags]
			public enum Flags2 : ushort
			{
				MultipurposeSlotsMask = 0x1F,
				HasPerInstInfo = 1,
				HasInterfaceMap = 2,
				HasDispatchMapSlot = 4,
				HasNonVirtualSlots = 8,
				HasModuleOverride = 0x10,
				IsZapped = 0x20,
				IsPreRestored = 0x40,
				HasModuleDependencies = 0x80,
				IsIntrinsicType = 0x100,
				RequiresDispatchTokenFat = 0x200,
				HasCctor = 0x400,
				HasVirtualStaticMethods = 0x800,
				REquiresAlign8 = 0x1000,
				HasBoxedRegularStatics = 0x2000,
				HasSingleNonVirtualSlot = 0x4000,
				DependsOnEquivalentOrForwardedStructs = 0x8000
			}

			public enum UnionLowBits
			{
				EEClass,
				Invalid,
				MethodTable,
				Indirection
			}

			public uint m_dwFlags;

			public uint m_BaseSize;

			public Flags2 m_wFlags2;

			public ushort m_wToken;

			public ushort m_wNumVirtuals;

			public ushort m_wNumInterfaces;

			private unsafe void* m_pParentMethodTable;

			public unsafe Module* m_pLoaderModule;

			public unsafe MethodTableWriteableData* m_pWriteableData;

			public unsafe void* union_pEEClass_pCanonMT;

			public unsafe void* union_pPerInstInfo_ElementTypeHnd_pMultipurposeSlot1;

			public unsafe void* union_p_InterfaceMap_pMultipurposeSlot2;

			public const int VTABLE_SLOTS_PER_CHUNK = 8;

			public const int VTABLE_SLOTS_PER_CHUNK_LOG2 = 3;

			private static readonly byte[] c_NonVirtualSlotsOffsets = GetNonVirtualSlotsOffsets();

			public bool IsInterface => (m_dwFlags & 0xF0000) == 786432;

			public bool HasIndirectParent => (m_dwFlags & 0x800000) != 0;

			public bool HasSingleNonVirtualSlot => m_wFlags2.Has(Flags2.HasSingleNonVirtualSlot);

			public unsafe MethodTable* GetCanonicalMethodTable()
			{
				nuint num = (nuint)union_pEEClass_pCanonMT;
				if ((num & 2) == 0)
				{
					return (MethodTable*)num;
				}
				if ((num & 1) != 0)
				{
					return *(MethodTable**)(num - 3);
				}
				return (MethodTable*)(num - 2);
			}

			public unsafe MethodDesc* GetParallelMethodDesc(MethodDesc* pDefMD)
			{
				return GetMethodDescForSlot(pDefMD->SlotNumber);
			}

			public unsafe MethodDesc* GetMethodDescForSlot(uint slotNumber)
			{
				if (IsInterface)
				{
					GetNumVirtuals();
				}
				throw new NotImplementedException();
			}

			public unsafe void* GetRestoredSlot(uint slotNumber)
			{
				MethodTable* ptr = (MethodTable*)Unsafe.AsPointer(ref this);
				void* slot;
				while (true)
				{
					ptr = ptr->GetCanonicalMethodTable();
					slot = ptr->GetSlot(slotNumber);
					if (slot != null)
					{
						break;
					}
					ptr = ptr->GetParentMethodTable();
				}
				return slot;
			}

			public unsafe MethodTable* GetParentMethodTable()
			{
				void* pParentMethodTable = m_pParentMethodTable;
				if (HasIndirectParent)
				{
					return *(MethodTable**)pParentMethodTable;
				}
				return (MethodTable*)pParentMethodTable;
			}

			public unsafe void* GetSlot(uint slotNumber)
			{
				nint slotPtrRaw = GetSlotPtrRaw(slotNumber);
				if (slotNumber < GetNumVirtuals())
				{
					return ((VTableIndir2_t*)slotPtrRaw)->Value;
				}
				if ((m_wFlags2 & Flags2.IsZapped) != 0 && slotNumber >= GetNumVirtuals())
				{
					return ((RelativePointer*)slotPtrRaw)->Value;
				}
				return *(void**)slotPtrRaw;
			}

			public unsafe nint GetSlotPtrRaw(uint slotNum)
			{
				if (slotNum < GetNumVirtuals())
				{
					uint indexOfVtableIndirection = GetIndexOfVtableIndirection(slotNum);
					return (nint)(VTableIndir_t__GetValueMaybeNullAtPtr((nint)(GetVtableIndirections() + indexOfVtableIndirection)) + GetIndexAfterVtableIndirection(slotNum));
				}
				if (HasSingleNonVirtualSlot)
				{
					return GetNonVirtualSlotsPtr();
				}
				return (nint)(GetNonVirtualSlotsArray() + (slotNum - GetNumVirtuals()));
			}

			public ushort GetNumVirtuals()
			{
				return m_wNumVirtuals;
			}

			public static uint GetIndexOfVtableIndirection(uint slotNum)
			{
				return slotNum >> 3;
			}

			public unsafe VTableIndir_t* GetVtableIndirections()
			{
				return (VTableIndir_t*)((byte*)Unsafe.AsPointer(ref this) + sizeof(MethodTable));
			}

			public unsafe static VTableIndir2_t* VTableIndir_t__GetValueMaybeNullAtPtr(nint @base)
			{
				return (VTableIndir2_t*)@base;
			}

			public static uint GetIndexAfterVtableIndirection(uint slotNum)
			{
				return slotNum & 7;
			}

			[MultipurposeSlotOffsetTable(3, typeof(MultipurposeSlotHelpers))]
			private static byte[] GetNonVirtualSlotsOffsets()
			{
				return new byte[8]
				{
					MultipurposeSlotHelpers.OffsetOfMp1(),
					MultipurposeSlotHelpers.OffsetOfMp2(),
					MultipurposeSlotHelpers.OffsetOfMp1(),
					MultipurposeSlotHelpers.RegularOffset(2),
					MultipurposeSlotHelpers.OffsetOfMp2(),
					MultipurposeSlotHelpers.RegularOffset(2),
					MultipurposeSlotHelpers.RegularOffset(2),
					MultipurposeSlotHelpers.RegularOffset(3)
				};
			}

			public nint GetNonVirtualSlotsPtr()
			{
				return GetMultipurposeSlotPtr(Flags2.HasNonVirtualSlots, c_NonVirtualSlotsOffsets);
			}

			public unsafe nint GetMultipurposeSlotPtr(Flags2 flag, byte[] offsets)
			{
				nint num = offsets[(uint)(m_wFlags2 & (flag - 1))];
				if (num >= sizeof(MethodTable))
				{
					num += (nint)GetNumVTableIndirections() * (nint)sizeof(VTableIndir_t);
				}
				return (nint)((byte*)Unsafe.AsPointer(ref this) + num);
			}

			public unsafe void*** GetNonVirtualSlotsArray()
			{
				return (void***)GetNonVirtualSlotsPtr();
			}

			public uint GetNumVTableIndirections()
			{
				return GetNumVtableIndirections(GetNumVirtuals());
			}

			public static uint GetNumVtableIndirections(uint numVirtuals)
			{
				return numVirtuals + 7 >> 3;
			}
		}

		public new unsafe static InvokeCompileMethodPtr InvokeCompileMethodPtr => new InvokeCompileMethodPtr((delegate*<IntPtr, IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult>)(&InvokeCompileMethod));

		public new unsafe static CorJitResult InvokeCompileMethod(IntPtr functionPtr, IntPtr thisPtr, IntPtr corJitInfo, CORINFO_METHOD_INFO* methodInfo, uint flags, byte** nativeEntry, uint* nativeSizeOfCode)
		{
			if (functionPtr == IntPtr.Zero)
			{
				*nativeEntry = null;
				*nativeSizeOfCode = 0u;
				return CorJitResult.CORJIT_OK;
			}
			delegate* unmanaged[Thiscall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> delegate_002A = (delegate* unmanaged[Thiscall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult>)(void*)functionPtr;
			delegate* unmanaged[Thiscall]<IntPtr, IntPtr, CORINFO_METHOD_INFO*, uint, byte**, uint*, CorJitResult> delegate_002A2 = delegate_002A;
			/*Error near IL_002b: Handle with invalid row number.*/;
		}
	}

	public readonly struct InvokeAllocMemPtr
	{
		private readonly IntPtr methodPtr;

		public unsafe delegate*<IntPtr, IntPtr, V70.AllocMemArgs*, void> InvokeAllocMem => (delegate*<IntPtr, IntPtr, V70.AllocMemArgs*, void>)(void*)methodPtr;

		public unsafe InvokeAllocMemPtr(delegate*<IntPtr, IntPtr, V70.AllocMemArgs*, void> ptr)
		{
			methodPtr = (IntPtr)ptr;
		}
	}

	public class V70 : V60
	{
		public static class ICorJitInfoVtable
		{
			public const int AllocMemIndex = 159;

			public const int TotalVtableCount = 175;
		}

		public struct AllocMemArgs
		{
			public uint hotCodeSize;

			public uint coldCodeSize;

			public uint roDataSize;

			public uint xcptnsCount;

			public int flag;

			public IntPtr hotCodeBlock;

			public IntPtr hotCodeBlockRW;

			public IntPtr coldCodeBlock;

			public IntPtr coldCodeBlockRW;

			public IntPtr roDataBlock;

			public IntPtr roDataBlockRW;
		}

		[UnmanagedFunctionPointer(CallingConvention.ThisCall)]
		public unsafe delegate void AllocMemDelegate(IntPtr thisPtr, AllocMemArgs* args);

		public unsafe static InvokeAllocMemPtr InvokeAllocMemPtr => new InvokeAllocMemPtr((delegate*<IntPtr, IntPtr, AllocMemArgs*, void>)(&InvokeAllocMem));

		public unsafe static void InvokeAllocMem(IntPtr functionPtr, IntPtr thisPtr, AllocMemArgs* args)
		{
			if (functionPtr == IntPtr.Zero)
			{
				return;
			}
			delegate* unmanaged[Thiscall]<IntPtr, AllocMemArgs*, void> delegate_002A = (delegate* unmanaged[Thiscall]<IntPtr, AllocMemArgs*, void>)(void*)functionPtr;
			delegate* unmanaged[Thiscall]<IntPtr, AllocMemArgs*, void> delegate_002A2 = delegate_002A;
			/*Error near IL_001a: Handle with invalid row number.*/;
		}
	}

	public class V80 : V70
	{
		public static class ICorJitInfoVtableV80
		{
			public const int AllocMemIndex = 154;

			public const int TotalVtableCount = 170;
		}
	}
}
