using System;
using System.IO.Packaging;
using MS.Internal.WindowsBase;

namespace MS.Internal.IO.Packaging;

internal static class PackUriHelper
{
	internal sealed class ValidatedPartUri : Uri, IComparable<ValidatedPartUri>, IEquatable<ValidatedPartUri>
	{
		private ValidatedPartUri _normalizedPartUri;

		private string _partUriString;

		private string _normalizedPartUriString;

		private bool _isNormalized;

		private bool _isRelationshipPartUri;

		private static readonly string _relationshipPartUpperCaseExtension = ".RELS";

		private static readonly string _relationshipPartUpperCaseSegmentName = "_RELS";

		private static readonly string _relsrelsUpperCaseExtension = _relationshipPartUpperCaseExtension + _relationshipPartUpperCaseExtension;

		private static readonly Uri _containerRelationshipNormalizedPartUri = new ValidatedPartUri("/_RELS/.RELS", isNormalized: true, computeIsRelationship: false, isRelationshipPartUri: true);

		private static readonly char[] _forwardSlashSeparator = new char[1] { '/' };

		internal string PartUriString => _partUriString;

		internal string NormalizedPartUriString
		{
			get
			{
				if (_normalizedPartUriString == null)
				{
					_normalizedPartUriString = GetNormalizedPartUriString();
				}
				return _normalizedPartUriString;
			}
		}

		internal ValidatedPartUri NormalizedPartUri
		{
			get
			{
				if (_normalizedPartUri == null)
				{
					_normalizedPartUri = GetNormalizedPartUri();
				}
				return _normalizedPartUri;
			}
		}

		internal bool IsNormalized => _isNormalized;

		internal bool IsRelationshipPartUri => _isRelationshipPartUri;

		internal ValidatedPartUri(string partUriString)
			: this(partUriString, isNormalized: false, computeIsRelationship: true, isRelationshipPartUri: false)
		{
		}

		internal ValidatedPartUri(string partUriString, bool isRelationshipUri)
			: this(partUriString, isNormalized: false, computeIsRelationship: false, isRelationshipUri)
		{
		}

		int IComparable<ValidatedPartUri>.CompareTo(ValidatedPartUri otherPartUri)
		{
			return Compare(otherPartUri);
		}

		bool IEquatable<ValidatedPartUri>.Equals(ValidatedPartUri otherPartUri)
		{
			return Compare(otherPartUri) == 0;
		}

		private ValidatedPartUri(string partUriString, bool isNormalized, bool computeIsRelationship, bool isRelationshipPartUri)
			: base(partUriString, UriKind.Relative)
		{
			_partUriString = partUriString;
			_isNormalized = isNormalized;
			if (computeIsRelationship)
			{
				_isRelationshipPartUri = IsRelationshipUri();
			}
			else
			{
				_isRelationshipPartUri = isRelationshipPartUri;
			}
		}

		private bool IsRelationshipUri()
		{
			bool flag = false;
			if (!NormalizedPartUriString.EndsWith(_relationshipPartUpperCaseExtension, StringComparison.Ordinal))
			{
				return false;
			}
			if (System.IO.Packaging.PackUriHelper.ComparePartUri(_containerRelationshipNormalizedPartUri, this) == 0)
			{
				return true;
			}
			string[] array = NormalizedPartUriString.Split(_forwardSlashSeparator);
			if (array.Length >= 3 && array[^1].Length > _relationshipPartExtensionName.Length)
			{
				flag = string.CompareOrdinal(array[^2], _relationshipPartUpperCaseSegmentName) == 0;
			}
			if (array.Length > 3 && flag && array[^1].EndsWith(_relsrelsUpperCaseExtension, StringComparison.Ordinal) && string.CompareOrdinal(array[^3], _relationshipPartUpperCaseSegmentName) == 0)
			{
				throw new ArgumentException(SR.NotAValidRelationshipPartUri);
			}
			return flag;
		}

		private string GetNormalizedPartUriString()
		{
			if (_isNormalized)
			{
				return _partUriString;
			}
			return _partUriString.ToUpperInvariant();
		}

		private ValidatedPartUri GetNormalizedPartUri()
		{
			if (IsNormalized)
			{
				return this;
			}
			return new ValidatedPartUri(_normalizedPartUriString, isNormalized: true, computeIsRelationship: false, IsRelationshipPartUri);
		}

		private int Compare(ValidatedPartUri otherPartUri)
		{
			if (otherPartUri == null)
			{
				return 1;
			}
			return string.CompareOrdinal(NormalizedPartUriString, otherPartUri.NormalizedPartUriString);
		}
	}

	private static readonly Uri _defaultUri;

	private static readonly Uri _packageRootUri;

	private static readonly string _relationshipPartExtensionName;

	internal static Uri PackageRootUri => _packageRootUri;

	internal static bool IsPackUri(Uri uri)
	{
		if (uri != null)
		{
			return string.Compare(uri.Scheme, System.IO.Packaging.PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase) == 0;
		}
		return false;
	}

	internal static ValidatedPartUri ValidatePartUri(Uri partUri)
	{
		if (partUri is ValidatedPartUri)
		{
			return (ValidatedPartUri)partUri;
		}
		string partUriString;
		Exception exceptionIfPartUriInvalid = GetExceptionIfPartUriInvalid(partUri, out partUriString);
		if (exceptionIfPartUriInvalid != null)
		{
			throw exceptionIfPartUriInvalid;
		}
		return new ValidatedPartUri(partUriString);
	}

	internal static string GetStringForPartUri(Uri partUri)
	{
		if (!(partUri is ValidatedPartUri))
		{
			partUri = ValidatePartUri(partUri);
		}
		return ((ValidatedPartUri)partUri).PartUriString;
	}

	static PackUriHelper()
	{
		_defaultUri = new Uri("http://defaultcontainer/");
		_packageRootUri = new Uri("/", UriKind.Relative);
		_relationshipPartExtensionName = ".rels";
		if (!UriParser.IsKnownScheme(System.IO.Packaging.PackUriHelper.UriSchemePack))
		{
			UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), System.IO.Packaging.PackUriHelper.UriSchemePack, -1);
		}
	}

	private static Exception GetExceptionIfPartUriInvalid(Uri partUri, out string partUriString)
	{
		partUriString = string.Empty;
		if (partUri == null)
		{
			return new ArgumentNullException("partUri");
		}
		Exception ex = null;
		ex = GetExceptionIfAbsoluteUri(partUri);
		if (ex != null)
		{
			return ex;
		}
		string stringForPartUriFromAnyUri = GetStringForPartUriFromAnyUri(partUri);
		if (stringForPartUriFromAnyUri.Length == 0)
		{
			return new ArgumentException(SR.PartUriIsEmpty);
		}
		if (stringForPartUriFromAnyUri[0] != '/')
		{
			return new ArgumentException(SR.PartUriShouldStartWithForwardSlash);
		}
		ex = GetExceptionIfPartNameStartsWithTwoSlashes(stringForPartUriFromAnyUri);
		if (ex != null)
		{
			return ex;
		}
		ex = GetExceptionIfPartNameEndsWithSlash(stringForPartUriFromAnyUri);
		if (ex != null)
		{
			return ex;
		}
		ex = GetExceptionIfFragmentPresent(stringForPartUriFromAnyUri);
		if (ex != null)
		{
			return ex;
		}
		string components = new Uri(_defaultUri, stringForPartUriFromAnyUri).GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
		if (string.CompareOrdinal(partUri.OriginalString.ToUpperInvariant(), components.ToUpperInvariant()) != 0)
		{
			return new ArgumentException(SR.InvalidPartUri);
		}
		partUriString = stringForPartUriFromAnyUri;
		return null;
	}

	private static ArgumentException GetExceptionIfAbsoluteUri(Uri uri)
	{
		if (uri.IsAbsoluteUri)
		{
			return new ArgumentException(SR.URIShouldNotBeAbsolute);
		}
		return null;
	}

	private static ArgumentException GetExceptionIfFragmentPresent(string partName)
	{
		if (partName.Contains('#'))
		{
			return new ArgumentException(SR.PartUriCannotHaveAFragment);
		}
		return null;
	}

	private static ArgumentException GetExceptionIfPartNameEndsWithSlash(string partName)
	{
		if (partName.Length > 0 && partName[partName.Length - 1] == '/')
		{
			return new ArgumentException(SR.PartUriShouldNotEndWithForwardSlash);
		}
		return null;
	}

	private static ArgumentException GetExceptionIfPartNameStartsWithTwoSlashes(string partName)
	{
		if (partName.Length > 1 && partName[0] == '/' && partName[1] == '/')
		{
			return new ArgumentException(SR.PartUriShouldNotStartWithTwoForwardSlashes);
		}
		return null;
	}

	private static string GetStringForPartUriFromAnyUri(Uri partUri)
	{
		Uri uri = (partUri.IsAbsoluteUri ? new Uri(partUri.GetComponents(UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.SafeUnescaped), UriKind.Relative) : new Uri(partUri.GetComponents(UriComponents.SerializationInfoString, UriFormat.SafeUnescaped), UriKind.Relative));
		string components = uri.GetComponents(UriComponents.SerializationInfoString, UriFormat.UriEscaped);
		if (IsPartNameEmpty(components))
		{
			return string.Empty;
		}
		return components;
	}

	private static bool IsPartNameEmpty(string partName)
	{
		if (partName.Length == 0 || (partName.Length == 1 && partName[0] == '/'))
		{
			return true;
		}
		return false;
	}
}
