using System;

namespace MS.Win32.Recognizer;

internal struct RECO_LATTICE_PROPERTY
{
	public Guid guidProperty;

	public ushort cbPropertyValue;

	public nint pPropertyValue;
}
