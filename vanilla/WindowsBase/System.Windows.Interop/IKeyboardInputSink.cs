using System.Windows.Input;

namespace System.Windows.Interop;

/// <summary>Provides a keyboard sink for components that manages tabbing, accelerators, and mnemonics across interop boundaries and between HWNDs. This interface implements keyboard message management in WPF-Win32 interoperation scenarios.</summary>
public interface IKeyboardInputSink
{
	/// <summary>Gets or sets a reference to the component's container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface. </summary>
	/// <returns>A reference to the container's <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> interface.</returns>
	IKeyboardInputSite KeyboardInputSite { get; set; }

	/// <summary>Registers the <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> interface of a contained component. </summary>
	/// <returns>The <see cref="T:System.Windows.Interop.IKeyboardInputSite" /> site of the contained component.</returns>
	/// <param name="sink">The <see cref="T:System.Windows.Interop.IKeyboardInputSink" /> sink of the contained component.</param>
	IKeyboardInputSite RegisterKeyboardInputSink(IKeyboardInputSink sink);

	/// <summary>Processes keyboard input at the keydown message level.</summary>
	/// <returns>true if the message was handled by the method implementation; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool TranslateAccelerator(ref MSG msg, ModifierKeys modifiers);

	/// <summary>Sets focus on either the first tab stop or the last tab stop of the sink. </summary>
	/// <returns>true if the focus has been set as requested; false, if there are no tab stops.</returns>
	/// <param name="request">Specifies whether focus should be set to the first or the last tab stop.</param>
	bool TabInto(TraversalRequest request);

	/// <summary>Called when one of the mnemonics (access keys) for this sink is invoked. </summary>
	/// <returns>true if the message was handled; otherwise, false.</returns>
	/// <param name="msg">The message for the mnemonic and associated data. Do not modify this message structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool OnMnemonic(ref MSG msg, ModifierKeys modifiers);

	/// <summary>Processes WM_CHAR, WM_SYSCHAR, WM_DEADCHAR, and WM_SYSDEADCHAR input messages before <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> is called. </summary>
	/// <returns>true if the message was processed and <see cref="M:System.Windows.Interop.IKeyboardInputSink.OnMnemonic(System.Windows.Interop.MSG@,System.Windows.Input.ModifierKeys)" /> should not be called; otherwise, false.</returns>
	/// <param name="msg">The message and associated data. Do not modify this structure. It is passed by reference for performance reasons only.</param>
	/// <param name="modifiers">Modifier keys.</param>
	bool TranslateChar(ref MSG msg, ModifierKeys modifiers);

	/// <summary>Gets a value that indicates whether the sink or one of its contained components has focus. </summary>
	/// <returns>true if the sink or one of its contained components has focus; otherwise, false.</returns>
	bool HasFocusWithin();
}
