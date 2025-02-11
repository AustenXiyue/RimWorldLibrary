using MS.Internal;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

internal class InputProviderSite : IDisposable
{
	private bool _isDisposed;

	private SecurityCriticalDataClass<InputManager> _inputManager;

	private SecurityCriticalDataClass<IInputProvider> _inputProvider;

	public InputManager InputManager => CriticalInputManager;

	internal InputManager CriticalInputManager => _inputManager.Value;

	public bool IsDisposed => _isDisposed;

	internal InputProviderSite(InputManager inputManager, IInputProvider inputProvider)
	{
		_inputManager = new SecurityCriticalDataClass<InputManager>(inputManager);
		_inputProvider = new SecurityCriticalDataClass<IInputProvider>(inputProvider);
	}

	public void Dispose()
	{
		GC.SuppressFinalize(this);
		if (!_isDisposed)
		{
			_isDisposed = true;
			if (_inputManager != null && _inputProvider != null)
			{
				_inputManager.Value.UnregisterInputProvider(_inputProvider.Value);
			}
			_inputManager = null;
			_inputProvider = null;
		}
	}

	public bool ReportInput(InputReport inputReport)
	{
		if (IsDisposed)
		{
			throw new ObjectDisposedException(SR.InputProviderSiteDisposed);
		}
		bool result = false;
		InputReportEventArgs inputReportEventArgs = new InputReportEventArgs(null, inputReport);
		inputReportEventArgs.RoutedEvent = InputManager.PreviewInputReportEvent;
		if (_inputManager != null)
		{
			result = _inputManager.Value.ProcessInput(inputReportEventArgs);
		}
		return result;
	}
}
