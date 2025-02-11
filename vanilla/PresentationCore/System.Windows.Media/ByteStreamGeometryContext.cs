using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Media.Composition;
using MS.Internal;
using MS.Internal.PresentationCore;
using MS.Utility;

namespace System.Windows.Media;

internal class ByteStreamGeometryContext : CapacityStreamGeometryContext
{
	private bool _disposed;

	private int _currChunkOffset;

	private FrugalStructList<byte[]> _chunkList;

	private int _currOffset;

	private MIL_PATHGEOMETRY _currentPathGeometryData;

	private MIL_PATHFIGURE _currentPathFigureData;

	private int _currentPathFigureDataOffset = -1;

	private MIL_SEGMENT_POLY _currentPolySegmentData;

	private int _currentPolySegmentDataOffset = -1;

	private uint _lastSegmentSize;

	private uint _lastFigureSize;

	private const int c_defaultChunkSize = 2048;

	private const int c_maxChunkSize = 1048576;

	[ThreadStatic]
	private static byte[] _pooledChunk;

	internal unsafe ByteStreamGeometryContext()
	{
		MIL_PATHGEOMETRY mIL_PATHGEOMETRY = default(MIL_PATHGEOMETRY);
		AppendData((byte*)(&mIL_PATHGEOMETRY), sizeof(MIL_PATHGEOMETRY));
		_currentPathGeometryData.Size = (uint)sizeof(MIL_PATHGEOMETRY);
	}

	public override void Close()
	{
		VerifyApi();
		((IDisposable)this).Dispose();
	}

	public unsafe override void BeginFigure(Point startPoint, bool isFilled, bool isClosed)
	{
		VerifyApi();
		FinishFigure();
		int currOffset = _currOffset;
		MIL_PATHFIGURE mIL_PATHFIGURE = default(MIL_PATHFIGURE);
		AppendData((byte*)(&mIL_PATHFIGURE), sizeof(MIL_PATHFIGURE));
		_currentPathFigureDataOffset = currOffset;
		_currentPathFigureData.StartPoint = startPoint;
		_currentPathFigureData.Flags |= (MilPathFigureFlags)(isFilled ? 8 : 0);
		_currentPathFigureData.Flags |= (MilPathFigureFlags)(isClosed ? 4 : 0);
		_currentPathFigureData.BackSize = _lastFigureSize;
		_currentPathFigureData.Size = (uint)(_currOffset - _currentPathFigureDataOffset);
	}

	public unsafe override void LineTo(Point point, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		Point* ptr = stackalloc Point[1];
		*ptr = point;
		GenericPolyTo(ptr, 1, isStroked, isSmoothJoin, hasCurves: false, MIL_SEGMENT_TYPE.MilSegmentPolyLine);
	}

	public unsafe override void QuadraticBezierTo(Point point1, Point point2, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		Point* ptr = stackalloc Point[2];
		*ptr = point1;
		ptr[1] = point2;
		GenericPolyTo(ptr, 2, isStroked, isSmoothJoin, hasCurves: true, MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier);
	}

	public unsafe override void BezierTo(Point point1, Point point2, Point point3, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		Point* ptr = stackalloc Point[3];
		*ptr = point1;
		ptr[1] = point2;
		ptr[2] = point3;
		GenericPolyTo(ptr, 3, isStroked, isSmoothJoin, hasCurves: true, MIL_SEGMENT_TYPE.MilSegmentPolyBezier);
	}

	public override void PolyLineTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		GenericPolyTo(points, isStroked, isSmoothJoin, hasCurves: false, 1, MIL_SEGMENT_TYPE.MilSegmentPolyLine);
	}

	public override void PolyQuadraticBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		GenericPolyTo(points, isStroked, isSmoothJoin, hasCurves: true, 2, MIL_SEGMENT_TYPE.MilSegmentPolyQuadraticBezier);
	}

	public override void PolyBezierTo(IList<Point> points, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		GenericPolyTo(points, isStroked, isSmoothJoin, hasCurves: true, 3, MIL_SEGMENT_TYPE.MilSegmentPolyBezier);
	}

	public unsafe override void ArcTo(Point point, Size size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked, bool isSmoothJoin)
	{
		VerifyApi();
		if (_currentPathFigureDataOffset == -1)
		{
			throw new InvalidOperationException(SR.StreamGeometry_NeedBeginFigure);
		}
		FinishSegment();
		MIL_SEGMENT_ARC mIL_SEGMENT_ARC = default(MIL_SEGMENT_ARC);
		mIL_SEGMENT_ARC.Type = MIL_SEGMENT_TYPE.MilSegmentArc;
		mIL_SEGMENT_ARC.Flags |= (MILCoreSegFlags)((!isStroked) ? 4 : 0);
		mIL_SEGMENT_ARC.Flags |= (MILCoreSegFlags)(isSmoothJoin ? 8 : 0);
		mIL_SEGMENT_ARC.Flags |= MILCoreSegFlags.SegIsCurved;
		mIL_SEGMENT_ARC.BackSize = _lastSegmentSize;
		mIL_SEGMENT_ARC.Point = point;
		mIL_SEGMENT_ARC.Size = size;
		mIL_SEGMENT_ARC.XRotation = rotationAngle;
		mIL_SEGMENT_ARC.LargeArc = (isLargeArc ? 1u : 0u);
		mIL_SEGMENT_ARC.Sweep = ((sweepDirection == SweepDirection.Clockwise) ? 1u : 0u);
		int currOffset = _currOffset;
		AppendData((byte*)(&mIL_SEGMENT_ARC), sizeof(MIL_SEGMENT_ARC));
		_lastSegmentSize = (uint)sizeof(MIL_SEGMENT_ARC);
		_currentPathFigureData.Flags |= ((!isStroked) ? MilPathFigureFlags.HasGaps : ((MilPathFigureFlags)0));
		_currentPathFigureData.Flags |= MilPathFigureFlags.HasCurves;
		_currentPathFigureData.Count++;
		_currentPathFigureData.Size = (uint)(_currOffset - _currentPathFigureDataOffset);
		_currentPathFigureData.OffsetToLastSegment = (uint)(currOffset - _currentPathFigureDataOffset);
	}

	internal byte[] GetData()
	{
		ShrinkToFit();
		return _chunkList[0];
	}

	internal override void SetClosedState(bool isClosed)
	{
		if (_currentPathFigureDataOffset == -1)
		{
			throw new InvalidOperationException(SR.StreamGeometry_NeedBeginFigure);
		}
		_currentPathFigureData.Flags &= ~MilPathFigureFlags.IsClosed;
		_currentPathFigureData.Flags |= (MilPathFigureFlags)(isClosed ? 4 : 0);
	}

	private void VerifyApi()
	{
		VerifyAccess();
		if (_disposed)
		{
			throw new ObjectDisposedException("ByteStreamGeometryContext");
		}
	}

	protected virtual void CloseCore(byte[] geometryData)
	{
	}

	internal unsafe override void DisposeCore()
	{
		if (!_disposed)
		{
			FinishFigure();
			fixed (MIL_PATHGEOMETRY* currentPathGeometryData = &_currentPathGeometryData)
			{
				OverwriteData((byte*)currentPathGeometryData, 0, sizeof(MIL_PATHGEOMETRY));
			}
			ShrinkToFit();
			CloseCore(_chunkList[0]);
			_disposed = true;
		}
	}

	private unsafe void ReadData(byte* pbData, int bufferOffset, int cbDataSize)
	{
		Invariant.Assert(cbDataSize >= 0);
		Invariant.Assert(bufferOffset >= 0);
		Invariant.Assert(_currOffset >= checked(bufferOffset + cbDataSize));
		ReadWriteData(reading: true, pbData, cbDataSize, 0, ref bufferOffset);
	}

	private unsafe void OverwriteData(byte* pbData, int bufferOffset, int cbDataSize)
	{
		Invariant.Assert(cbDataSize >= 0);
		Invariant.Assert(checked(bufferOffset + cbDataSize) <= _currOffset);
		ReadWriteData(reading: false, pbData, cbDataSize, 0, ref bufferOffset);
	}

	private unsafe void AppendData(byte* pbData, int cbDataSize)
	{
		Invariant.Assert(cbDataSize >= 0);
		int currOffset = checked(_currOffset + cbDataSize);
		if (_chunkList.Count == 0)
		{
			byte[] value = AcquireChunkFromPool();
			_chunkList.Add(value);
		}
		ReadWriteData(reading: false, pbData, cbDataSize, _chunkList.Count - 1, ref _currChunkOffset);
		_currOffset = currOffset;
	}

	internal unsafe void ShrinkToFit()
	{
		if (_chunkList.Count > 1 || _chunkList[0].Length != _currOffset)
		{
			byte[] array = new byte[_currOffset];
			fixed (byte* pbData = array)
			{
				ReadData(pbData, 0, _currOffset);
			}
			ReturnChunkToPool(_chunkList[0]);
			if (_chunkList.Count == 1)
			{
				_chunkList[0] = array;
				return;
			}
			_chunkList = default(FrugalStructList<byte[]>);
			_chunkList.Add(array);
		}
	}

	private unsafe void ReadWriteData(bool reading, byte* pbData, int cbDataSize, int currentChunk, ref int bufferOffset)
	{
		Invariant.Assert(cbDataSize >= 0);
		while (bufferOffset > _chunkList[currentChunk].Length)
		{
			bufferOffset -= _chunkList[currentChunk].Length;
			currentChunk++;
		}
		while (cbDataSize > 0)
		{
			int num = Math.Min(cbDataSize, _chunkList[currentChunk].Length - bufferOffset);
			if (num > 0)
			{
				Invariant.Assert(_chunkList[currentChunk] != null && _chunkList[currentChunk].Length >= bufferOffset + num);
				Invariant.Assert(_chunkList[currentChunk].Length != 0);
				if (reading)
				{
					Marshal.Copy(_chunkList[currentChunk], bufferOffset, (nint)pbData, num);
				}
				else
				{
					Marshal.Copy((nint)pbData, _chunkList[currentChunk], bufferOffset, num);
				}
				cbDataSize -= num;
				pbData += num;
				bufferOffset += num;
			}
			if (cbDataSize > 0)
			{
				currentChunk = checked(currentChunk + 1);
				if (_chunkList.Count == currentChunk)
				{
					Invariant.Assert(!reading);
					int num2 = Math.Min(2 * _chunkList[_chunkList.Count - 1].Length, 1048576);
					_chunkList.Add(new byte[num2]);
				}
				bufferOffset = 0;
			}
		}
	}

	private unsafe void FinishFigure()
	{
		if (_currentPathFigureDataOffset != -1)
		{
			FinishSegment();
			fixed (MIL_PATHFIGURE* currentPathFigureData = &_currentPathFigureData)
			{
				OverwriteData((byte*)currentPathFigureData, _currentPathFigureDataOffset, sizeof(MIL_PATHFIGURE));
			}
			_currentPathGeometryData.Flags |= (MilPathGeometryFlags)(((_currentPathFigureData.Flags & MilPathFigureFlags.HasCurves) != 0) ? 1 : 0);
			_currentPathGeometryData.Flags |= (MilPathGeometryFlags)(((_currentPathFigureData.Flags & MilPathFigureFlags.HasGaps) != 0) ? 4 : 0);
			_currentPathGeometryData.Flags |= (MilPathGeometryFlags)(((_currentPathFigureData.Flags & MilPathFigureFlags.IsFillable) == 0) ? 8 : 0);
			_currentPathGeometryData.FigureCount++;
			_currentPathGeometryData.Size = (uint)_currOffset;
			_lastFigureSize = _currentPathFigureData.Size;
			_currentPathFigureDataOffset = -1;
			_currentPathFigureData = default(MIL_PATHFIGURE);
			_lastSegmentSize = 0u;
		}
	}

	private unsafe void FinishSegment()
	{
		if (_currentPolySegmentDataOffset != -1)
		{
			fixed (MIL_SEGMENT_POLY* currentPolySegmentData = &_currentPolySegmentData)
			{
				OverwriteData((byte*)currentPolySegmentData, _currentPolySegmentDataOffset, sizeof(MIL_SEGMENT_POLY));
			}
			_lastSegmentSize = (uint)(sizeof(MIL_SEGMENT_POLY) + sizeof(Point) * _currentPolySegmentData.Count);
			if ((_currentPolySegmentData.Flags & MILCoreSegFlags.SegIsAGap) != 0)
			{
				_currentPathFigureData.Flags |= MilPathFigureFlags.HasGaps;
			}
			if ((_currentPolySegmentData.Flags & MILCoreSegFlags.SegIsCurved) != 0)
			{
				_currentPathFigureData.Flags |= MilPathFigureFlags.HasCurves;
			}
			_currentPathFigureData.Count++;
			_currentPathFigureData.Size = (uint)(_currOffset - _currentPathFigureDataOffset);
			_currentPathFigureData.OffsetToLastSegment = (uint)(_currentPolySegmentDataOffset - _currentPathFigureDataOffset);
			_currentPolySegmentDataOffset = -1;
			_currentPolySegmentData = default(MIL_SEGMENT_POLY);
		}
	}

	private unsafe void GenericPolyTo(IList<Point> points, bool isStroked, bool isSmoothJoin, bool hasCurves, int pointCountMultiple, MIL_SEGMENT_TYPE segmentType)
	{
		if (_currentPathFigureDataOffset == -1)
		{
			throw new InvalidOperationException(SR.StreamGeometry_NeedBeginFigure);
		}
		if (points == null)
		{
			return;
		}
		int count = points.Count;
		count -= count % pointCountMultiple;
		if (count > 0)
		{
			GenericPolyToHelper(isStroked, isSmoothJoin, hasCurves, segmentType);
			for (int i = 0; i < count; i++)
			{
				Point point = points[i];
				AppendData((byte*)(&point), sizeof(Point));
				_currentPolySegmentData.Count++;
			}
		}
	}

	private unsafe void GenericPolyTo(Point* points, int count, bool isStroked, bool isSmoothJoin, bool hasCurves, MIL_SEGMENT_TYPE segmentType)
	{
		if (_currentPathFigureDataOffset == -1)
		{
			throw new InvalidOperationException(SR.StreamGeometry_NeedBeginFigure);
		}
		GenericPolyToHelper(isStroked, isSmoothJoin, hasCurves, segmentType);
		AppendData((byte*)points, sizeof(Point) * count);
		_currentPolySegmentData.Count += (uint)count;
	}

	private unsafe void GenericPolyToHelper(bool isStroked, bool isSmoothJoin, bool hasCurves, MIL_SEGMENT_TYPE segmentType)
	{
		if (_currentPolySegmentDataOffset != -1 && (_currentPolySegmentData.Type != segmentType || (_currentPolySegmentData.Flags & MILCoreSegFlags.SegIsAGap) == 0 != isStroked || (_currentPolySegmentData.Flags & MILCoreSegFlags.SegSmoothJoin) != 0 != isSmoothJoin))
		{
			FinishSegment();
		}
		if (_currentPolySegmentDataOffset == -1)
		{
			int currOffset = _currOffset;
			MIL_SEGMENT_POLY mIL_SEGMENT_POLY = default(MIL_SEGMENT_POLY);
			AppendData((byte*)(&mIL_SEGMENT_POLY), sizeof(MIL_SEGMENT_POLY));
			_currentPolySegmentDataOffset = currOffset;
			_currentPolySegmentData.Type = segmentType;
			_currentPolySegmentData.Flags |= (MILCoreSegFlags)((!isStroked) ? 4 : 0);
			_currentPolySegmentData.Flags |= (MILCoreSegFlags)(hasCurves ? 32 : 0);
			_currentPolySegmentData.Flags |= (MILCoreSegFlags)(isSmoothJoin ? 8 : 0);
			_currentPolySegmentData.BackSize = _lastSegmentSize;
		}
	}

	private static byte[] AcquireChunkFromPool()
	{
		byte[] pooledChunk = _pooledChunk;
		if (pooledChunk == null)
		{
			return new byte[2048];
		}
		_pooledChunk = null;
		return pooledChunk;
	}

	private static void ReturnChunkToPool(byte[] chunk)
	{
		if (chunk.Length == 2048)
		{
			_pooledChunk = chunk;
		}
	}
}
