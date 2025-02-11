namespace WinRT;

internal class ActivationFactory<T> : BaseActivationFactory
{
	private static WeakLazy<ActivationFactory<T>> _factory = new WeakLazy<ActivationFactory<T>>();

	public ActivationFactory()
		: base(typeof(T).Namespace, typeof(T).FullName)
	{
	}

	public static ObjectReference<I> As<I>()
	{
		return _factory.Value._As<I>();
	}

	public static ObjectReference<I> ActivateInstance<I>()
	{
		return _factory.Value._ActivateInstance<I>();
	}
}
