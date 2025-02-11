using MS.Internal;

namespace System.Windows;

/// <summary>Provides data for the SourceChanged event, used for interoperation. This class cannot be inherited.</summary>
public sealed class SourceChangedEventArgs : RoutedEventArgs
{
	private SecurityCriticalData<PresentationSource> _oldSource;

	private SecurityCriticalData<PresentationSource> _newSource;

	private IInputElement _element;

	private IInputElement _oldParent;

	/// <summary>Gets the old source involved in this source change. </summary>
	/// <returns>The old <see cref="T:System.Windows.PresentationSource" />.</returns>
	public PresentationSource OldSource => _oldSource.Value;

	/// <summary>Gets the new source involved in this source change. </summary>
	/// <returns>The new <see cref="T:System.Windows.PresentationSource" />.</returns>
	public PresentationSource NewSource => _newSource.Value;

	/// <summary>Gets the element whose parent change causing the presentation source information to change. </summary>
	/// <returns>The element that is reporting the change.</returns>
	public IInputElement Element => _element;

	/// <summary>Gets the previous parent of the element whose parent change causing the presentation source information to change. </summary>
	/// <returns>The previous parent element source.</returns>
	public IInputElement OldParent => _oldParent;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.SourceChangedEventArgs" /> class, using supplied information for the old and new sources. </summary>
	/// <param name="oldSource">The old <see cref="T:System.Windows.PresentationSource" /> that this handler is being notified about.</param>
	/// <param name="newSource">The new <see cref="T:System.Windows.PresentationSource" /> that this handler is being notified about.</param>
	public SourceChangedEventArgs(PresentationSource oldSource, PresentationSource newSource)
		: this(oldSource, newSource, null, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.SourceChangedEventArgs" /> class, using supplied information for the old and new sources, the element that this change effects, and the previous reported parent of that element. </summary>
	/// <param name="oldSource">The old <see cref="T:System.Windows.PresentationSource" /> that this handler is being notified about.</param>
	/// <param name="newSource">The new <see cref="T:System.Windows.PresentationSource" /> that this handler is being notified about.</param>
	/// <param name="element">The element whose parent changed causing the source to change.</param>
	/// <param name="oldParent">The old parent of the element whose parent changed causing the source to change.</param>
	public SourceChangedEventArgs(PresentationSource oldSource, PresentationSource newSource, IInputElement element, IInputElement oldParent)
	{
		_oldSource = new SecurityCriticalData<PresentationSource>(oldSource);
		_newSource = new SecurityCriticalData<PresentationSource>(newSource);
		_element = element;
		_oldParent = oldParent;
	}

	protected override void InvokeEventHandler(Delegate genericHandler, object genericTarget)
	{
		((SourceChangedEventHandler)genericHandler)(genericTarget, this);
	}
}
