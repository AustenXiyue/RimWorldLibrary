using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Security.RightsManagement;
using System.Text;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class UserUseLicenseDictionaryLoader
{
	private Dictionary<ContentUser, UseLicense> _dict;

	private UTF8Encoding _utf8Encoding = new UTF8Encoding();

	internal Dictionary<ContentUser, UseLicense> LoadedDictionary => _dict;

	internal UserUseLicenseDictionaryLoader(RightsManagementEncryptionTransform rmet)
	{
		_dict = new Dictionary<ContentUser, UseLicense>(ContentUser._contentUserComparer);
		Invariant.Assert(rmet != null);
		Load(rmet);
	}

	private void Load(RightsManagementEncryptionTransform rmet)
	{
		rmet.EnumUseLicenseStreams(AddUseLicenseFromStreamToDictionary, null);
	}

	private void AddUseLicenseFromStreamToDictionary(RightsManagementEncryptionTransform rmet, StreamInfo si, object param, ref bool stop)
	{
		using Stream input = si.GetStream(FileMode.Open, FileAccess.Read);
		using BinaryReader utf8Reader = new BinaryReader(input, _utf8Encoding);
		ContentUser user;
		UseLicense value = rmet.LoadUseLicenseAndUserFromStream(utf8Reader, out user);
		_dict.Add(user, value);
	}
}
