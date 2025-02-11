using System;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class IndexingFilterMarshaler : IFilter
{
	internal static Guid PSGUID_STORAGE = new Guid(3072717104u, 18415, 4122, 165, 241, 2, 96, 140, 158, 235, 172);

	internal const int _int16Size = 2;

	private IManagedFilter _implementation;

	private bool _throwOnEndOfChunks = true;

	internal bool ThrowOnEndOfChunks
	{
		get
		{
			return _throwOnEndOfChunks;
		}
		set
		{
			_throwOnEndOfChunks = value;
		}
	}

	internal IndexingFilterMarshaler(IManagedFilter managedFilter)
	{
		if (managedFilter == null)
		{
			throw new ArgumentNullException("managedFilter");
		}
		_implementation = managedFilter;
	}

	internal static ManagedFullPropSpec[] MarshalFullPropSpecArray(uint cAttributes, FULLPROPSPEC[] aAttributes)
	{
		if (cAttributes != 0)
		{
			Invariant.Assert(aAttributes != null);
			ManagedFullPropSpec[] array = new ManagedFullPropSpec[checked((int)cAttributes)];
			for (int i = 0; i < cAttributes; i++)
			{
				array[i] = new ManagedFullPropSpec(aAttributes[i]);
			}
			return array;
		}
		return null;
	}

	internal static void MarshalStringToPtr(string s, ref uint bufCharacterCount, nint p)
	{
		Invariant.Assert(bufCharacterCount != 0);
		if ((uint)s.Length > bufCharacterCount - 1)
		{
			throw new InvalidOperationException(SR.FilterGetTextBufferOverflow);
		}
		bufCharacterCount = (uint)(s.Length + 1);
		Marshal.Copy(s.ToCharArray(), 0, p, s.Length);
		Marshal.WriteInt16(p, s.Length * 2, 0);
	}

	internal static void MarshalPropSpec(ManagedPropSpec propSpec, ref PROPSPEC native)
	{
		native.propType = (uint)propSpec.PropType;
		switch (propSpec.PropType)
		{
		case PropSpecType.Id:
			native.union.propId = propSpec.PropId;
			break;
		case PropSpecType.Name:
			native.union.name = Marshal.StringToCoTaskMemUni(propSpec.PropName);
			break;
		default:
			Invariant.Assert(condition: false);
			break;
		}
	}

	internal static void MarshalFullPropSpec(ManagedFullPropSpec fullPropSpec, ref FULLPROPSPEC native)
	{
		native.guid = fullPropSpec.Guid;
		MarshalPropSpec(fullPropSpec.Property, ref native.property);
	}

	internal static STAT_CHUNK MarshalChunk(ManagedChunk chunk)
	{
		STAT_CHUNK result = default(STAT_CHUNK);
		result.idChunk = chunk.ID;
		Invariant.Assert(chunk.BreakType >= CHUNK_BREAKTYPE.CHUNK_NO_BREAK && chunk.BreakType <= CHUNK_BREAKTYPE.CHUNK_EOC);
		result.breakType = chunk.BreakType;
		Invariant.Assert(chunk.Flags >= (CHUNKSTATE)0 && chunk.Flags <= (CHUNKSTATE.CHUNK_TEXT | CHUNKSTATE.CHUNK_VALUE | CHUNKSTATE.CHUNK_FILTER_OWNED_VALUE));
		result.flags = chunk.Flags;
		result.locale = chunk.Locale;
		result.idChunkSource = chunk.ChunkSource;
		result.cwcStartSource = chunk.StartSource;
		result.cwcLenSource = chunk.LenSource;
		MarshalFullPropSpec(chunk.Attribute, ref result.attribute);
		return result;
	}

	internal static nint MarshalPropVariant(object obj)
	{
		nint num = IntPtr.Zero;
		nint num2 = IntPtr.Zero;
		try
		{
			PROPVARIANT structure;
			if (obj is string)
			{
				num = Marshal.StringToCoTaskMemAnsi((string)obj);
				structure = default(PROPVARIANT);
				structure.vt = VARTYPE.VT_LPSTR;
				structure.union.pszVal = num;
			}
			else
			{
				if (!(obj is DateTime))
				{
					throw new InvalidOperationException(SR.FilterGetValueMustBeStringOrDateTime);
				}
				structure = default(PROPVARIANT);
				structure.vt = VARTYPE.VT_FILETIME;
				long num3 = ((DateTime)obj).ToFileTime();
				structure.union.filetime.dwLowDateTime = (int)num3;
				structure.union.filetime.dwHighDateTime = (int)((num3 >> 32) & 0xFFFFFFFFu);
			}
			num2 = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(PROPVARIANT)));
			Invariant.Assert(num2 != IntPtr.Zero);
			Marshal.StructureToPtr(structure, num2, fDeleteOld: false);
			return num2;
		}
		catch
		{
			if (num != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(num);
			}
			if (num2 != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(num2);
			}
			throw;
		}
	}

	public IFILTER_FLAGS Init(IFILTER_INIT grfFlags, uint cAttributes, FULLPROPSPEC[] aAttributes)
	{
		ManagedFullPropSpec[] aAttributes2 = MarshalFullPropSpecArray(cAttributes, aAttributes);
		return _implementation.Init(grfFlags, aAttributes2);
	}

	public STAT_CHUNK GetChunk()
	{
		ManagedChunk chunk = _implementation.GetChunk();
		if (chunk == null)
		{
			if (ThrowOnEndOfChunks)
			{
				throw new COMException(SR.FilterEndOfChunks, -2147215616);
			}
			STAT_CHUNK result = default(STAT_CHUNK);
			result.idChunk = 0u;
			return result;
		}
		return MarshalChunk(chunk);
	}

	public void GetText(ref uint bufCharacterCount, nint pBuffer)
	{
		MarshalStringToPtr(_implementation.GetText((int)(bufCharacterCount - 1)), ref bufCharacterCount, pBuffer);
	}

	public nint GetValue()
	{
		return MarshalPropVariant(_implementation.GetValue());
	}

	public nint BindRegion(FILTERREGION origPos, ref Guid riid)
	{
		throw new NotImplementedException(SR.FilterBindRegionNotImplemented);
	}
}
