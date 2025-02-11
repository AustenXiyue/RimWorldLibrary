namespace System.Windows.Media.Animation;

/// <summary>An <see cref="T:System.Windows.Media.Animation.IKeyFrame" /> interface implementation provides un-typed access to <see cref="T:System.Windows.Media.Animation.KeyTime" /> properties. </summary>
public interface IKeyFrame
{
	/// <summary>Gets or sets <see cref="P:System.Windows.Media.Animation.IKeyFrame.KeyTime" /> values associated with a KeyFrame object. </summary>
	/// <returns>A <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance.</returns>
	KeyTime KeyTime { get; set; }

	/// <summary>Gets or sets the value associated with a <see cref="T:System.Windows.Media.Animation.KeyTime" /> instance. </summary>
	/// <returns>The current value for this property. </returns>
	object Value { get; set; }
}
