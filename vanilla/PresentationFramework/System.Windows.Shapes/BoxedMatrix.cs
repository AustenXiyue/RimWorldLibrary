using System.Windows.Media;

namespace System.Windows.Shapes;

internal class BoxedMatrix
{
	public Matrix Value;

	public BoxedMatrix(Matrix value)
	{
		Value = value;
	}
}
