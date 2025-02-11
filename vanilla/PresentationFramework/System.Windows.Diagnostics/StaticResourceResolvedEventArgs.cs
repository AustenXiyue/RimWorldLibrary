namespace System.Windows.Diagnostics;

public class StaticResourceResolvedEventArgs : EventArgs
{
	public object TargetObject { get; private set; }

	public object TargetProperty { get; private set; }

	public ResourceDictionary ResourceDictionary { get; private set; }

	public object ResourceKey { get; private set; }

	internal StaticResourceResolvedEventArgs(object targetObject, object targetProperty, ResourceDictionary rd, object key)
	{
		TargetObject = targetObject;
		TargetProperty = targetProperty;
		ResourceDictionary = rd;
		ResourceKey = key;
	}
}
