using System;

namespace MS.Internal.AppModel;

[Serializable]
internal struct ReturnEventSaverInfo
{
	internal string _delegateTypeName;

	internal string _targetTypeName;

	internal string _delegateMethodName;

	internal bool _delegateInSamePF;

	internal ReturnEventSaverInfo(string delegateTypeName, string targetTypeName, string delegateMethodName, bool fSamePf)
	{
		_delegateTypeName = delegateTypeName;
		_targetTypeName = targetTypeName;
		_delegateMethodName = delegateMethodName;
		_delegateInSamePF = fSamePf;
	}
}
