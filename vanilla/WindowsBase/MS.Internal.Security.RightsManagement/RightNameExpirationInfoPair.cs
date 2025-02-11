using System;

namespace MS.Internal.Security.RightsManagement;

internal class RightNameExpirationInfoPair
{
	private string _rightName;

	private DateTime _validFrom;

	private DateTime _validUntil;

	internal string RightName => _rightName;

	internal DateTime ValidFrom => _validFrom;

	internal DateTime ValidUntil => _validUntil;

	internal RightNameExpirationInfoPair(string rightName, DateTime validFrom, DateTime validUntil)
	{
		_rightName = rightName;
		_validFrom = validFrom;
		_validUntil = validUntil;
	}
}
