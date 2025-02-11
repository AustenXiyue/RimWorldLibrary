namespace System.Windows.Input;

/// <summary>Provides data for preprocess input events. </summary>
public sealed class PreProcessInputEventArgs : ProcessInputEventArgs
{
	private bool _canceled;

	/// <summary>Determines whether processing of the input event was canceled. </summary>
	/// <returns>true if the processing was canceled; otherwise, false.</returns>
	public bool Canceled => _canceled;

	internal PreProcessInputEventArgs()
	{
	}

	internal override void Reset(StagingAreaInputItem input, InputManager inputManager)
	{
		_canceled = false;
		base.Reset(input, inputManager);
	}

	/// <summary>Cancels the processing of the input event. </summary>
	public void Cancel()
	{
		_canceled = true;
	}
}
