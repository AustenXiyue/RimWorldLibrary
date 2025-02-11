using System.Collections.Generic;

namespace System.Windows.Media.Media3D;

internal class Matrix3DStack
{
	private readonly List<Matrix3D> _stack = new List<Matrix3D>();

	public int Count => _stack.Count;

	public bool IsEmpty => _stack.Count == 0;

	public Matrix3D Top => _stack[_stack.Count - 1];

	public void Clear()
	{
		_stack.Clear();
	}

	public Matrix3D Pop()
	{
		Matrix3D top = Top;
		_stack.RemoveAt(_stack.Count - 1);
		return top;
	}

	public void Push(Matrix3D matrix)
	{
		if (_stack.Count > 0)
		{
			matrix.Append(Top);
		}
		_stack.Add(matrix);
	}
}
