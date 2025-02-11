using System.Windows;
using System.Windows.Media.Media3D;

namespace MS.Internal;

internal interface IVisual3DContainer
{
	void AddChild(Visual3D child);

	void RemoveChild(Visual3D child);

	int GetChildrenCount();

	Visual3D GetChild(int index);

	void VerifyAPIReadOnly();

	void VerifyAPIReadOnly(DependencyObject other);

	void VerifyAPIReadWrite();

	void VerifyAPIReadWrite(DependencyObject other);
}
