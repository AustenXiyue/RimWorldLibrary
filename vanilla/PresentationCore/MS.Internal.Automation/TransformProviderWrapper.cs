using System;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace MS.Internal.Automation;

internal class TransformProviderWrapper : MarshalByRefObject, ITransformProvider
{
	private AutomationPeer _peer;

	private ITransformProvider _iface;

	public bool CanMove => (bool)ElementUtil.Invoke(_peer, GetCanMove, null);

	public bool CanResize => (bool)ElementUtil.Invoke(_peer, GetCanResize, null);

	public bool CanRotate => (bool)ElementUtil.Invoke(_peer, GetCanRotate, null);

	private TransformProviderWrapper(AutomationPeer peer, ITransformProvider iface)
	{
		_peer = peer;
		_iface = iface;
	}

	public void Move(double x, double y)
	{
		ElementUtil.Invoke(_peer, Move, new double[2] { x, y });
	}

	public void Resize(double width, double height)
	{
		ElementUtil.Invoke(_peer, Resize, new double[2] { width, height });
	}

	public void Rotate(double degrees)
	{
		ElementUtil.Invoke(_peer, Rotate, degrees);
	}

	internal static object Wrap(AutomationPeer peer, object iface)
	{
		return new TransformProviderWrapper(peer, (ITransformProvider)iface);
	}

	private object Move(object arg)
	{
		double[] array = (double[])arg;
		_iface.Move(array[0], array[1]);
		return null;
	}

	private object Resize(object arg)
	{
		double[] array = (double[])arg;
		_iface.Resize(array[0], array[1]);
		return null;
	}

	private object Rotate(object arg)
	{
		_iface.Rotate((double)arg);
		return null;
	}

	private object GetCanMove(object unused)
	{
		return _iface.CanMove;
	}

	private object GetCanResize(object unused)
	{
		return _iface.CanResize;
	}

	private object GetCanRotate(object unused)
	{
		return _iface.CanRotate;
	}
}
