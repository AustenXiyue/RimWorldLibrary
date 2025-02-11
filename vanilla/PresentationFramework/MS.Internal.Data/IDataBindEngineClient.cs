using System.Windows;

namespace MS.Internal.Data;

internal interface IDataBindEngineClient
{
	DependencyObject TargetElement { get; }

	void TransferValue();

	void UpdateValue();

	bool AttachToContext(bool lastChance);

	void VerifySourceReference(bool lastChance);

	void OnTargetUpdated();
}
