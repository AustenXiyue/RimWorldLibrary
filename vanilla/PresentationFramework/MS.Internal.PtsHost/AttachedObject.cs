using System.Windows;
using MS.Internal.PtsHost.UnsafeNativeMethods;

namespace MS.Internal.PtsHost;

internal class AttachedObject : EmbeddedObject
{
	internal BaseParagraph Para;

	internal override DependencyObject Element => Para.Element;

	internal AttachedObject(int dcp, BaseParagraph para)
		: base(dcp)
	{
		Para = para;
	}

	internal override void Dispose()
	{
		Para.Dispose();
		Para = null;
		base.Dispose();
	}

	internal override void Update(EmbeddedObject newObject)
	{
		AttachedObject attachedObject = newObject as AttachedObject;
		ErrorHandler.Assert(attachedObject != null, ErrorHandler.EmbeddedObjectTypeMismatch);
		ErrorHandler.Assert(attachedObject.Element.Equals(Element), ErrorHandler.EmbeddedObjectOwnerMismatch);
		Dcp = attachedObject.Dcp;
		Para.SetUpdateInfo(PTS.FSKCHANGE.fskchInside, stopAsking: false);
	}
}
