using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Windows.Input;

/// <summary>Contains a collection of <see cref="T:System.Windows.Input.StylusButton" /> objects.</summary>
public class StylusButtonCollection : ReadOnlyCollection<StylusButton>
{
	internal StylusButtonCollection(StylusButton[] buttons)
		: base((IList<StylusButton>)new List<StylusButton>(buttons))
	{
	}

	internal StylusButtonCollection(List<StylusButton> buttons)
		: base((IList<StylusButton>)buttons)
	{
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusButton" /> that the specified GUID identifies.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusButton" /> of the specified GUID.</returns>
	/// <param name="guid">The <see cref="T:System.Guid" /> that specifies the desired <see cref="T:System.Windows.Input.StylusButton" />.</param>
	public StylusButton GetStylusButtonByGuid(Guid guid)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].Guid == guid)
			{
				return base[i];
			}
		}
		return null;
	}
}
