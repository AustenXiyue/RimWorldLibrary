using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Packaging;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
using System.Text;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging.CompoundFile;

internal class RightsManagementEncryptionTransform : IDataTransform
{
	internal delegate void UseLicenseStreamCallback(RightsManagementEncryptionTransform rmet, StreamInfo si, object param, ref bool stop);

	private class LoadUseLicenseForUserParams
	{
		private ContentUser _user;

		private UseLicense _useLicense;

		internal ContentUser User => _user;

		internal UseLicense UseLicense
		{
			get
			{
				return _useLicense;
			}
			set
			{
				_useLicense = value;
			}
		}

		internal LoadUseLicenseForUserParams(ContentUser user)
		{
			_user = user;
			_useLicense = null;
		}
	}

	private CryptoProvider _cryptoProvider;

	private PublishLicense _publishLicense;

	private bool _fixedSettings;

	private static readonly UnicodeEncoding _unicodeEncoding = new UnicodeEncoding(bigEndian: false, byteOrderMark: false);

	private VersionedStreamOwner _publishLicenseStream;

	private byte[] _publishLicenseHeaderExtraBytes;

	private StorageInfo _useLicenseStorage;

	private const string LicenseStreamNamePrefix = "EUL-";

	private static readonly int LicenseStreamNamePrefixLength = "EUL-".Length;

	private const string FeatureName = "Microsoft.Metadata.DRMTransform";

	private const int PublishLicenseLengthMax = 1000000;

	private const int UseLicenseLengthMax = 1000000;

	private const int UserNameLengthMax = 1000;

	private static readonly int UseLicenseStreamLengthMin = 2 * ContainerUtilities.Int32Size + 1;

	private static readonly VersionPair CurrentFeatureVersion = new VersionPair(1, 0);

	private static readonly VersionPair MinimumReaderVersion = new VersionPair(1, 0);

	private static readonly VersionPair MinimumUpdaterVersion = new VersionPair(1, 0);

	private const int MaxPublishLicenseHeaderLen = 4096;

	private static readonly char[] Base32EncodingTable = new char[33]
	{
		'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J',
		'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T',
		'U', 'V', 'W', 'X', 'Y', 'Z', '2', '3', '4', '5',
		'6', '7', '='
	};

	private static readonly byte[] Padding = new byte[3];

	private const int SizeofByte = 1;

	public bool IsReady
	{
		get
		{
			if (_cryptoProvider != null)
			{
				if (!_cryptoProvider.CanDecrypt)
				{
					return _cryptoProvider.CanEncrypt;
				}
				return true;
			}
			return false;
		}
	}

	public bool FixedSettings => _fixedSettings;

	internal int TransformIdentifierType => 1;

	public object TransformIdentifier => ClassTransformIdentifier;

	internal CryptoProvider CryptoProvider
	{
		get
		{
			return _cryptoProvider;
		}
		set
		{
			if (_fixedSettings)
			{
				throw new InvalidOperationException(SR.CannotChangeCryptoProvider);
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (!value.CanEncrypt && !value.CanDecrypt)
			{
				throw new ArgumentException(SR.CryptoProviderIsNotReady, "value");
			}
			_cryptoProvider = value;
		}
	}

	internal static string ClassTransformIdentifier => "{C73DFACD-061F-43B0-8B64-0C620D2A8B50}";

	internal RightsManagementEncryptionTransform(TransformEnvironment transformEnvironment)
	{
		Stream primaryInstanceData = transformEnvironment.GetPrimaryInstanceData();
		_useLicenseStorage = transformEnvironment.GetInstanceDataStorage();
		_publishLicenseStream = new VersionedStreamOwner(primaryInstanceData, new FormatVersion("Microsoft.Metadata.DRMTransform", MinimumReaderVersion, MinimumUpdaterVersion, CurrentFeatureVersion));
	}

	internal PublishLicense LoadPublishLicense()
	{
		if (_publishLicenseStream.Length <= 0)
		{
			return null;
		}
		_publishLicenseStream.Seek(0L, SeekOrigin.Begin);
		BinaryReader binaryReader = new BinaryReader(_publishLicenseStream, Encoding.UTF8);
		int num = binaryReader.ReadInt32();
		if (num < ContainerUtilities.Int32Size)
		{
			throw new FileFormatException(SR.PublishLicenseStreamCorrupt);
		}
		if (num > 4096)
		{
			throw new FileFormatException(SR.Format(SR.PublishLicenseStreamHeaderTooLong, num, 4096));
		}
		int num2 = num - ContainerUtilities.Int32Size;
		if (num2 > 0)
		{
			_publishLicenseHeaderExtraBytes = new byte[num2];
			if (PackagingUtilities.ReliableRead(_publishLicenseStream, _publishLicenseHeaderExtraBytes, 0, num2) != num2)
			{
				throw new FileFormatException(SR.PublishLicenseStreamCorrupt);
			}
		}
		_publishLicense = new PublishLicense(ReadLengthPrefixedString(binaryReader, Encoding.UTF8, 1000000));
		return _publishLicense;
	}

	internal void SavePublishLicense(PublishLicense publishLicense)
	{
		if (publishLicense == null)
		{
			throw new ArgumentNullException("publishLicense");
		}
		if (_fixedSettings)
		{
			throw new InvalidOperationException(SR.CannotChangePublishLicense);
		}
		_publishLicenseStream.Seek(0L, SeekOrigin.Begin);
		BinaryWriter binaryWriter = new BinaryWriter(_publishLicenseStream, Encoding.UTF8);
		int num = ContainerUtilities.Int32Size;
		if (_publishLicenseHeaderExtraBytes != null)
		{
			num = checked(num + _publishLicenseHeaderExtraBytes.Length);
		}
		binaryWriter.Write(num);
		if (_publishLicenseHeaderExtraBytes != null)
		{
			_publishLicenseStream.Write(_publishLicenseHeaderExtraBytes, 0, _publishLicenseHeaderExtraBytes.Length);
		}
		WriteByteLengthPrefixedDwordPaddedString(publishLicense.ToString(), binaryWriter, Encoding.UTF8);
		binaryWriter.Flush();
		_publishLicense = publishLicense;
	}

	internal UseLicense LoadUseLicense(ContentUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		LoadUseLicenseForUserParams loadUseLicenseForUserParams = new LoadUseLicenseForUserParams(user);
		EnumUseLicenseStreams(LoadUseLicenseForUser, loadUseLicenseForUserParams);
		return loadUseLicenseForUserParams.UseLicense;
	}

	internal void SaveUseLicense(ContentUser user, UseLicense useLicense)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		if (useLicense == null)
		{
			throw new ArgumentNullException("useLicense");
		}
		if (user.AuthenticationType != 0 && user.AuthenticationType != AuthenticationType.Passport)
		{
			throw new ArgumentException(SR.OnlyPassportOrWindowsAuthenticatedUsersAreAllowed, "user");
		}
		EnumUseLicenseStreams(DeleteUseLicenseForUser, user);
		SaveUseLicenseForUser(user, useLicense);
	}

	internal void DeleteUseLicense(ContentUser user)
	{
		if (user == null)
		{
			throw new ArgumentNullException("user");
		}
		EnumUseLicenseStreams(DeleteUseLicenseForUser, user);
	}

	internal IDictionary<ContentUser, UseLicense> GetEmbeddedUseLicenses()
	{
		return new ReadOnlyDictionary<ContentUser, UseLicense>(new UserUseLicenseDictionaryLoader(this).LoadedDictionary);
	}

	Stream IDataTransform.GetTransformedStream(Stream encodedStream, IDictionary transformContext)
	{
		_fixedSettings = true;
		return new VersionedStream(new RightsManagementEncryptedStream(encodedStream, _cryptoProvider), _publishLicenseStream);
	}

	internal void EnumUseLicenseStreams(UseLicenseStreamCallback callback, object param)
	{
		if (callback == null)
		{
			throw new ArgumentNullException("callback");
		}
		bool stop = false;
		StreamInfo[] streams = _useLicenseStorage.GetStreams();
		foreach (StreamInfo streamInfo in streams)
		{
			if (string.CompareOrdinal("EUL-".ToUpperInvariant(), 0, streamInfo.Name.ToUpperInvariant(), 0, LicenseStreamNamePrefixLength) == 0)
			{
				callback(this, streamInfo, param, ref stop);
				if (stop)
				{
					break;
				}
			}
		}
	}

	internal UseLicense LoadUseLicenseAndUserFromStream(BinaryReader utf8Reader, out ContentUser user)
	{
		utf8Reader.BaseStream.Seek(0L, SeekOrigin.Begin);
		if (utf8Reader.ReadInt32() < UseLicenseStreamLengthMin)
		{
			throw new FileFormatException(SR.UseLicenseStreamCorrupt);
		}
		byte[] bytes = Convert.FromBase64String(ReadLengthPrefixedString(utf8Reader, Encoding.UTF8, 1000));
		ParseTypePrefixedUserName(new string(_unicodeEncoding.GetChars(bytes)), out var authenticationType, out var userName);
		user = new ContentUser(userName, authenticationType);
		return new UseLicense(ReadLengthPrefixedString(utf8Reader, Encoding.UTF8, 1000000));
	}

	private void LoadUseLicenseForUser(RightsManagementEncryptionTransform rmet, StreamInfo si, object param, ref bool stop)
	{
		if (!(param is LoadUseLicenseForUserParams { User: var user } loadUseLicenseForUserParams))
		{
			throw new ArgumentException(SR.CallbackParameterInvalid, "param");
		}
		using Stream input = si.GetStream(FileMode.Open, FileAccess.Read);
		using BinaryReader utf8Reader = new BinaryReader(input, Encoding.UTF8);
		if (rmet.LoadUserFromStream(utf8Reader).GenericEquals(user))
		{
			loadUseLicenseForUserParams.UseLicense = rmet.LoadUseLicenseFromStream(utf8Reader);
			stop = true;
		}
	}

	private void DeleteUseLicenseForUser(RightsManagementEncryptionTransform rmet, StreamInfo si, object param, ref bool stop)
	{
		if (!(param is ContentUser userObj))
		{
			throw new ArgumentException(SR.CallbackParameterInvalid, "param");
		}
		ContentUser contentUser = null;
		using (Stream input = si.GetStream(FileMode.Open, FileAccess.Read))
		{
			using BinaryReader utf8Reader = new BinaryReader(input, Encoding.UTF8);
			contentUser = rmet.LoadUserFromStream(utf8Reader);
		}
		if (contentUser.GenericEquals(userObj))
		{
			si.Delete();
		}
	}

	internal void SaveUseLicenseForUser(ContentUser user, UseLicense useLicense)
	{
		string streamName = MakeUseLicenseStreamName();
		using Stream output = new StreamInfo(_useLicenseStorage, streamName).Create();
		using BinaryWriter binaryWriter = new BinaryWriter(output, Encoding.UTF8);
		string s = MakeTypePrefixedUserName(user);
		string s2 = Convert.ToBase64String(_unicodeEncoding.GetBytes(s));
		byte[] bytes = Encoding.UTF8.GetBytes(s2);
		int num = bytes.Length;
		int value = checked(2 * ContainerUtilities.Int32Size + num + ContainerUtilities.CalculateDWordPadBytesLength(num));
		binaryWriter.Write(value);
		binaryWriter.Write(num);
		binaryWriter.Write(bytes, 0, num);
		WriteDwordPadding(num, binaryWriter);
		WriteByteLengthPrefixedDwordPaddedString(useLicense.ToString(), binaryWriter, Encoding.UTF8);
	}

	private static char[] Base32EncodeWithoutPadding(byte[] bytes)
	{
		int num = checked(bytes.Length * 8);
		int num2 = num / 5;
		if (num % 5 != 0)
		{
			num2++;
		}
		char[] array = new char[num2];
		for (int i = 0; i < num2; i++)
		{
			int num3 = i * 5;
			int num4 = 0;
			for (int j = num3; j - num3 < 5 && j < num; j++)
			{
				int num5 = j / 8;
				int num6 = j % 8;
				if ((bytes[num5] & (1 << num6)) != 0)
				{
					num4 += 1 << j - num3;
				}
			}
			array[i] = Base32EncodingTable[num4];
		}
		return array;
	}

	private static string MakeUseLicenseStreamName()
	{
		return "EUL-" + new string(Base32EncodeWithoutPadding(Guid.NewGuid().ToByteArray()));
	}

	private static string MakeTypePrefixedUserName(ContentUser user)
	{
		IFormatProvider formatProvider = null;
		IFormatProvider provider = formatProvider;
		Span<char> initialBuffer = stackalloc char[128];
		DefaultInterpolatedStringHandler handler = new DefaultInterpolatedStringHandler(1, 2, formatProvider, initialBuffer);
		handler.AppendFormatted(user.AuthenticationType);
		handler.AppendLiteral(":");
		handler.AppendFormatted(user.Name);
		return string.Create(provider, initialBuffer, ref handler);
	}

	private static void ParseTypePrefixedUserName(string typePrefixedUserName, out AuthenticationType authenticationType, out string userName)
	{
		authenticationType = AuthenticationType.Windows;
		int num = typePrefixedUserName.IndexOf(':');
		if (num < 1 || num >= typePrefixedUserName.Length - 1)
		{
			throw new FileFormatException(SR.InvalidTypePrefixedUserName);
		}
		userName = typePrefixedUserName.Substring(num + 1);
		string x = typePrefixedUserName.Substring(0, num);
		bool flag = false;
		if (((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)x, (object?)Enum.GetName(typeof(AuthenticationType), AuthenticationType.Windows)))
		{
			authenticationType = AuthenticationType.Windows;
			flag = true;
		}
		else if (((IEqualityComparer)ContainerUtilities.StringCaseInsensitiveComparer).Equals((object?)x, (object?)Enum.GetName(typeof(AuthenticationType), AuthenticationType.Passport)))
		{
			authenticationType = AuthenticationType.Passport;
			flag = true;
		}
		if (!flag)
		{
			throw new FileFormatException(SR.Format(SR.InvalidAuthenticationTypeString, typePrefixedUserName));
		}
	}

	private UseLicense LoadUseLicenseFromStream(BinaryReader utf8Reader)
	{
		utf8Reader.BaseStream.Seek(0L, SeekOrigin.Begin);
		if (utf8Reader.ReadInt32() < UseLicenseStreamLengthMin)
		{
			throw new FileFormatException(SR.UseLicenseStreamCorrupt);
		}
		ReadLengthPrefixedString(utf8Reader, Encoding.UTF8, 1000);
		return new UseLicense(ReadLengthPrefixedString(utf8Reader, Encoding.UTF8, 1000000));
	}

	private ContentUser LoadUserFromStream(BinaryReader utf8Reader)
	{
		utf8Reader.BaseStream.Seek(0L, SeekOrigin.Begin);
		if (utf8Reader.ReadInt32() < UseLicenseStreamLengthMin)
		{
			throw new FileFormatException(SR.UseLicenseStreamCorrupt);
		}
		byte[] bytes = Convert.FromBase64String(ReadLengthPrefixedString(utf8Reader, Encoding.UTF8, 1000));
		ParseTypePrefixedUserName(new string(_unicodeEncoding.GetChars(bytes)), out var authenticationType, out var userName);
		return new ContentUser(userName, authenticationType);
	}

	private static string ReadLengthPrefixedString(BinaryReader reader, Encoding encoding, int maxLength)
	{
		int num = reader.ReadInt32();
		if (num > maxLength)
		{
			throw new FileFormatException(SR.Format(SR.ExcessiveLengthPrefix, num, maxLength));
		}
		byte[] array = reader.ReadBytes(num);
		if (array.Length != num)
		{
			throw new FileFormatException(SR.InvalidStringFormat);
		}
		string @string = encoding.GetString(array);
		SkipDwordPadding(array.Length, reader);
		return @string;
	}

	private static void SkipDwordPadding(int length, BinaryReader reader)
	{
		int num = length % ContainerUtilities.Int32Size;
		if (num != 0 && reader.ReadBytes(ContainerUtilities.Int32Size - num).Length != ContainerUtilities.Int32Size - num)
		{
			throw new FileFormatException(SR.InvalidStringFormat);
		}
	}

	private static void WriteDwordPadding(int length, BinaryWriter writer)
	{
		int num = length % ContainerUtilities.Int32Size;
		if (num != 0)
		{
			writer.Write(Padding, 0, ContainerUtilities.Int32Size - num);
		}
	}

	private static void WriteByteLengthPrefixedDwordPaddedString(string s, BinaryWriter writer, Encoding encoding)
	{
		byte[] bytes = encoding.GetBytes(s);
		int num = bytes.Length;
		writer.Write(num);
		writer.Write(bytes);
		WriteDwordPadding(num, writer);
	}
}
