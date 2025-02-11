using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using MS.Internal.PresentationCore;

namespace MS.Internal.Automation;

internal class SynchronizedInputAdaptor : ISynchronizedInputProvider
{
	private readonly DependencyObject _owner;

	internal SynchronizedInputAdaptor(DependencyObject owner)
	{
		Invariant.Assert(owner != null);
		_owner = owner;
	}

	void ISynchronizedInputProvider.StartListening(SynchronizedInputType inputType)
	{
		if (inputType != SynchronizedInputType.KeyDown && inputType != SynchronizedInputType.KeyUp && inputType != SynchronizedInputType.MouseLeftButtonDown && inputType != SynchronizedInputType.MouseLeftButtonUp && inputType != SynchronizedInputType.MouseRightButtonDown && inputType != SynchronizedInputType.MouseRightButtonUp)
		{
			throw new ArgumentException(SR.Format(SR.Automation_InvalidSynchronizedInputType, inputType));
		}
		if (_owner is UIElement uIElement)
		{
			if (!uIElement.StartListeningSynchronizedInput(inputType))
			{
				throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
			}
		}
		else if (_owner is ContentElement contentElement)
		{
			if (!contentElement.StartListeningSynchronizedInput(inputType))
			{
				throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
			}
		}
		else if (!((UIElement3D)_owner).StartListeningSynchronizedInput(inputType))
		{
			throw new InvalidOperationException(SR.Automation_RecursivePublicCall);
		}
	}

	void ISynchronizedInputProvider.Cancel()
	{
		if (_owner is UIElement uIElement)
		{
			uIElement.CancelSynchronizedInput();
		}
		else if (_owner is ContentElement contentElement)
		{
			contentElement.CancelSynchronizedInput();
		}
		else
		{
			((UIElement3D)_owner).CancelSynchronizedInput();
		}
	}
}
