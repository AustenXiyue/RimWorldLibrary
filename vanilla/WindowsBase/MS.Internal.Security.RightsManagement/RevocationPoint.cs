using System;

namespace MS.Internal.Security.RightsManagement;

internal class RevocationPoint
{
	private string _id;

	private string _idType;

	private Uri _url;

	private SystemTime _frequency;

	private string _name;

	private string _publicKey;

	internal string Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}

	internal string IdType
	{
		get
		{
			return _idType;
		}
		set
		{
			_idType = value;
		}
	}

	internal Uri Url
	{
		get
		{
			return _url;
		}
		set
		{
			_url = value;
		}
	}

	internal SystemTime Frequency
	{
		get
		{
			return _frequency;
		}
		set
		{
			_frequency = value;
		}
	}

	internal string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}

	internal string PublicKey
	{
		get
		{
			return _publicKey;
		}
		set
		{
			_publicKey = value;
		}
	}
}
