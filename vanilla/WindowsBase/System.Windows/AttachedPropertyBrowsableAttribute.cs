using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>Provides a base class for .NET Framework attributes that report the use scope of attached properties.</summary>
public abstract class AttachedPropertyBrowsableAttribute : Attribute
{
	internal virtual bool UnionResults => false;

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal abstract bool IsBrowsable(DependencyObject d, DependencyProperty dp);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.AttachedPropertyBrowsableAttribute" /> class.</summary>
	protected AttachedPropertyBrowsableAttribute()
	{
	}
}
