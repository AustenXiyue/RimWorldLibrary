using System.Runtime.CompilerServices;

namespace System.Windows.Markup;

[TypeForwardedFrom("PresentationFramework, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
internal interface IProvidePropertyFallback
{
	bool CanProvidePropertyFallback(string property);

	object ProvidePropertyFallback(string property, Exception cause);
}
