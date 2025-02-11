namespace System.Windows.Interop;

/// <summary>Represents the method that handles the <see cref="E:System.Windows.Interop.ComponentDispatcher.ThreadFilterMessage" /> and <see cref="E:System.Windows.Interop.ComponentDispatcher.ThreadPreprocessMessage" /> events. </summary>
/// <param name="msg">A structure with the message data.</param>
/// <param name="handled">true if the message was handled; otherwise, false.</param>
public delegate void ThreadMessageEventHandler(ref MSG msg, ref bool handled);
