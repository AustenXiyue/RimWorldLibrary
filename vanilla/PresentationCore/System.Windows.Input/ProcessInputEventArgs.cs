using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Provides data for postprocess input events. </summary>
public class ProcessInputEventArgs : NotifyInputEventArgs
{
	private bool _allowAccessToStagingArea;

	internal ProcessInputEventArgs()
	{
	}

	internal override void Reset(StagingAreaInputItem input, InputManager inputManager)
	{
		_allowAccessToStagingArea = true;
		base.Reset(input, inputManager);
	}

	/// <summary>Puts the specified input event onto the top of the specified staging area stack. </summary>
	/// <returns>The staging area input item that wraps the specified input.</returns>
	/// <param name="input">The input event to put on the staging area stack.</param>
	/// <param name="promote">An existing staging area item to promote the state from.</param>
	public StagingAreaInputItem PushInput(InputEventArgs input, StagingAreaInputItem promote)
	{
		if (!_allowAccessToStagingArea)
		{
			throw new InvalidOperationException(SR.NotAllowedToAccessStagingArea);
		}
		return base.UnsecureInputManager.PushInput(input, promote);
	}

	/// <summary>Puts the specified input event onto the top of the staging area stack. </summary>
	/// <returns>The staging area input item.</returns>
	/// <param name="input">The input event to put on the staging area stack. </param>
	public StagingAreaInputItem PushInput(StagingAreaInputItem input)
	{
		if (!_allowAccessToStagingArea)
		{
			throw new InvalidOperationException(SR.NotAllowedToAccessStagingArea);
		}
		return base.UnsecureInputManager.PushInput(input);
	}

	/// <summary>Removes the input event off the top of the staging area stack. </summary>
	/// <returns>The input event that was on the top of the staging area stack. This will be null if the staging area is empty.</returns>
	public StagingAreaInputItem PopInput()
	{
		if (!_allowAccessToStagingArea)
		{
			throw new InvalidOperationException(SR.NotAllowedToAccessStagingArea);
		}
		return base.UnsecureInputManager.PopInput();
	}

	/// <summary>Gets, but does not pop, the input event on the top of the staging area stack.</summary>
	/// <returns>The input event that is on the top of the staging area stack. </returns>
	public StagingAreaInputItem PeekInput()
	{
		if (!_allowAccessToStagingArea)
		{
			throw new InvalidOperationException(SR.NotAllowedToAccessStagingArea);
		}
		return base.UnsecureInputManager.PeekInput();
	}
}
