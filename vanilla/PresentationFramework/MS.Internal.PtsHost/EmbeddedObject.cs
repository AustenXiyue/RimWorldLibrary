using System.Windows;

namespace MS.Internal.PtsHost;

internal abstract class EmbeddedObject
{
	internal int Dcp;

	internal abstract DependencyObject Element { get; }

	protected EmbeddedObject(int dcp)
	{
		Dcp = dcp;
	}

	internal virtual void Dispose()
	{
	}

	internal abstract void Update(EmbeddedObject newObject);
}
