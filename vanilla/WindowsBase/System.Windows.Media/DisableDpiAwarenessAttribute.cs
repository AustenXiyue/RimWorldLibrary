namespace System.Windows.Media;

/// <summary>Allows WPF applications to disable dots per inch (dpi) awareness for all user interface elements.</summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class DisableDpiAwarenessAttribute : Attribute
{
	/// <summary>Initializes a new instance of <see cref="T:System.Windows.Media.DisableDpiAwarenessAttribute" />.</summary>
	public DisableDpiAwarenessAttribute()
	{
	}
}
