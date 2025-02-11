using MS.Internal;

namespace System.Windows.Media;

internal class MatrixStack
{
	private Matrix[] _items;

	private int _size;

	private static readonly int s_initialSize = 40;

	private const int s_growFactor = 2;

	private const int s_shrinkFactor = 3;

	private int _highWaterMark;

	private int _observeCount;

	private const int s_trimCount = 10;

	public bool IsEmpty => _size == 0;

	public MatrixStack()
	{
		_items = new Matrix[s_initialSize];
	}

	private void EnsureCapacity()
	{
		if (_size == _items.Length)
		{
			Matrix[] array = new Matrix[2 * _size];
			Array.Copy(_items, array, _size);
			_items = array;
		}
	}

	public void Push(ref Matrix matrix, bool combine)
	{
		EnsureCapacity();
		if (combine && _size > 0)
		{
			_items[_size] = matrix;
			MatrixUtil.MultiplyMatrix(ref _items[_size], ref _items[_size - 1]);
		}
		else
		{
			_items[_size] = matrix;
		}
		_size++;
		_highWaterMark = Math.Max(_highWaterMark, _size);
	}

	public void Push(Transform transform, bool combine)
	{
		EnsureCapacity();
		if (combine && _size > 0)
		{
			transform.MultiplyValueByMatrix(ref _items[_size], ref _items[_size - 1]);
		}
		else
		{
			_items[_size] = transform.Value;
		}
		_size++;
		_highWaterMark = Math.Max(_highWaterMark, _size);
	}

	public void Push(Vector offset, bool combine)
	{
		EnsureCapacity();
		if (combine && _size > 0)
		{
			_items[_size] = _items[_size - 1];
		}
		else
		{
			_items[_size] = Matrix.Identity;
		}
		MatrixUtil.PrependOffset(ref _items[_size], offset.X, offset.Y);
		_size++;
		_highWaterMark = Math.Max(_highWaterMark, _size);
	}

	public void Pop()
	{
		_size--;
	}

	public Matrix Peek()
	{
		return _items[_size - 1];
	}

	public void Optimize()
	{
		if (_observeCount == 10)
		{
			int num = Math.Max(_highWaterMark, s_initialSize);
			if (num * 3 <= _items.Length)
			{
				_items = new Matrix[num];
			}
			_highWaterMark = 0;
			_observeCount = 0;
		}
		else
		{
			_observeCount++;
		}
	}
}
