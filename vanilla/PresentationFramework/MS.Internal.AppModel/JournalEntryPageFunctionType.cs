using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal class JournalEntryPageFunctionType : JournalEntryPageFunctionSaver, ISerializable
{
	private MS.Internal.SecurityCriticalDataForSet<string> _typeName;

	internal JournalEntryPageFunctionType(JournalEntryGroupState jeGroupState, PageFunctionBase pageFunction)
		: base(jeGroupState, pageFunction)
	{
		string assemblyQualifiedName = pageFunction.GetType().AssemblyQualifiedName;
		_typeName = new MS.Internal.SecurityCriticalDataForSet<string>(assemblyQualifiedName);
	}

	protected JournalEntryPageFunctionType(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_typeName = new MS.Internal.SecurityCriticalDataForSet<string>(info.GetString("_typeName"));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("_typeName", _typeName.Value);
	}

	internal override void SaveState(object contentObject)
	{
		base.SaveState(contentObject);
	}

	internal override PageFunctionBase ResumePageFunction()
	{
		Invariant.Assert(_typeName.Value != null, "JournalEntry does not contain the Type for the PageFunction to be created");
		Type type = Type.GetType(_typeName.Value);
		PageFunctionBase pageFunctionBase;
		try
		{
			pageFunctionBase = (PageFunctionBase)Activator.CreateInstance(type);
		}
		catch (Exception innerException)
		{
			throw new Exception(SR.Format(SR.FailedResumePageFunction, _typeName.Value), innerException);
		}
		InitializeComponent(pageFunctionBase);
		RestoreState(pageFunctionBase);
		return pageFunctionBase;
	}

	private void InitializeComponent(PageFunctionBase pageFunction)
	{
		if (pageFunction is IComponentConnector componentConnector)
		{
			componentConnector.InitializeComponent();
		}
	}
}
