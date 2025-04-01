using System.CodeDom;
using System.Collections.Generic;

namespace System.Runtime.Serialization;

internal class ContractCodeDomInfo
{
	internal bool IsProcessed;

	internal CodeTypeDeclaration TypeDeclaration;

	internal CodeTypeReference TypeReference;

	internal CodeNamespace CodeNamespace;

	internal bool ReferencedTypeExists;

	internal bool UsesWildcardNamespace;

	private string clrNamespace;

	private Dictionary<string, object> memberNames;

	internal string ClrNamespace
	{
		get
		{
			if (!ReferencedTypeExists)
			{
				return clrNamespace;
			}
			return null;
		}
		set
		{
			if (ReferencedTypeExists)
			{
				throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Cannot set namespace for already referenced type. Base type is '{0}'.", TypeReference.BaseType)));
			}
			clrNamespace = value;
		}
	}

	internal Dictionary<string, object> GetMemberNames()
	{
		if (ReferencedTypeExists)
		{
			throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString("Cannot set members for already referenced type. Base type is '{0}'.", TypeReference.BaseType)));
		}
		if (memberNames == null)
		{
			memberNames = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
		}
		return memberNames;
	}
}
