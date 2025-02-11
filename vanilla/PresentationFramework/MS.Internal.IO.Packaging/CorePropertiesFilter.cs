using System;
using System.Globalization;
using System.IO.Packaging;
using System.Runtime.InteropServices;
using System.Windows;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class CorePropertiesFilter : IManagedFilter
{
	private class PropertyChunk : ManagedChunk
	{
		internal PropertyChunk(uint chunkId, Guid guid, uint propId)
			: base(chunkId, CHUNK_BREAKTYPE.CHUNK_EOS, new ManagedFullPropSpec(guid, propId), (uint)CultureInfo.InvariantCulture.LCID, CHUNKSTATE.CHUNK_VALUE)
		{
		}
	}

	private IFILTER_INIT _grfFlags;

	private ManagedFullPropSpec[] _aAttributes;

	private uint _chunkID;

	private bool _pendingGetValue;

	private CorePropertyEnumerator _corePropertyEnumerator;

	private PackageProperties _coreProperties;

	private CorePropertyEnumerator CorePropertyEnumerator
	{
		get
		{
			if (_corePropertyEnumerator == null)
			{
				_corePropertyEnumerator = new CorePropertyEnumerator(_coreProperties, _grfFlags, _aAttributes);
			}
			return _corePropertyEnumerator;
		}
	}

	internal CorePropertiesFilter(PackageProperties coreProperties)
	{
		if (coreProperties == null)
		{
			throw new ArgumentNullException("coreProperties");
		}
		_coreProperties = coreProperties;
	}

	public IFILTER_FLAGS Init(IFILTER_INIT grfFlags, ManagedFullPropSpec[] aAttributes)
	{
		_grfFlags = grfFlags;
		_aAttributes = aAttributes;
		_corePropertyEnumerator = new CorePropertyEnumerator(_coreProperties, _grfFlags, _aAttributes);
		return IFILTER_FLAGS.IFILTER_FLAGS_NONE;
	}

	public ManagedChunk GetChunk()
	{
		_pendingGetValue = false;
		if (!CorePropertyEnumerator.MoveNext())
		{
			return null;
		}
		PropertyChunk result = new PropertyChunk(AllocateChunkID(), CorePropertyEnumerator.CurrentGuid, CorePropertyEnumerator.CurrentPropId);
		_pendingGetValue = true;
		return result;
	}

	public string GetText(int bufferCharacterCount)
	{
		throw new COMException(SR.FilterGetTextNotSupported, -2147215611);
	}

	public object GetValue()
	{
		if (!_pendingGetValue)
		{
			throw new COMException(SR.FilterGetValueAlreadyCalledOnCurrentChunk, -2147215614);
		}
		_pendingGetValue = false;
		return CorePropertyEnumerator.CurrentValue;
	}

	private uint AllocateChunkID()
	{
		if (_chunkID == uint.MaxValue)
		{
			_chunkID = 1u;
		}
		else
		{
			_chunkID++;
		}
		return _chunkID;
	}
}
