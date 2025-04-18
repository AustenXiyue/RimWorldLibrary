using System;
using Mono.Cecil;

namespace MonoMod.Utils;

[Serializable]
internal class RelinkTargetNotFoundException : RelinkFailedException
{
	public new const string DefaultMessage = "MonoMod relinker failed finding";

	public RelinkTargetNotFoundException(IMetadataTokenProvider mtp, IMetadataTokenProvider? context = null)
		: base(RelinkFailedException.Format("MonoMod relinker failed finding", mtp, context), mtp, context)
	{
	}

	public RelinkTargetNotFoundException(string message, IMetadataTokenProvider mtp, IMetadataTokenProvider? context = null)
		: base(message ?? "MonoMod relinker failed finding", mtp, context)
	{
	}

	public RelinkTargetNotFoundException(string message, Exception innerException, IMetadataTokenProvider mtp, IMetadataTokenProvider? context = null)
		: base(message ?? "MonoMod relinker failed finding", innerException, mtp, context)
	{
	}
}
