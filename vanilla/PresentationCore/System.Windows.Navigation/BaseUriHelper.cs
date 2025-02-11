using System.Globalization;
using System.IO.Packaging;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.IO.Packaging;
using MS.Internal.PresentationCore;

namespace System.Windows.Navigation;

/// <summary>Provides a method to resolve relative uniform resource identifiers (URIs) with respect to the base URI of a container, such as a <see cref="T:System.Windows.Controls.Frame" />.</summary>
public static class BaseUriHelper
{
	private const string SOOBASE = "SiteOfOrigin://";

	private static readonly Uri _siteOfOriginBaseUri;

	private const string APPBASE = "application://";

	private static readonly Uri _packAppBaseUri;

	private static MS.Internal.SecurityCriticalDataForSet<Uri> _baseUri;

	private const string _packageApplicationBaseUriEscaped = "application:///";

	private const string _packageSiteOfOriginBaseUriEscaped = "siteoforigin:///";

	/// <summary>Identifies the BaseUri attached property.</summary>
	public static readonly DependencyProperty BaseUriProperty;

	private const string COMPONENT = ";component";

	private const string VERSION = "v";

	private const char COMPONENT_DELIMITER = ';';

	private static Assembly _resourceAssembly;

	internal static Uri SiteOfOriginBaseUri
	{
		[FriendAccessAllowed]
		get
		{
			return _siteOfOriginBaseUri;
		}
	}

	internal static Uri PackAppBaseUri
	{
		[FriendAccessAllowed]
		get
		{
			return _packAppBaseUri;
		}
	}

	/// <summary>Gets or sets the base uniform resource identifier (URI).</summary>
	/// <returns>The base uniform resource identifier (URI).</returns>
	internal static Uri BaseUri
	{
		[FriendAccessAllowed]
		get
		{
			return _baseUri.Value;
		}
		[FriendAccessAllowed]
		set
		{
			_baseUri.Value = value;
		}
	}

	internal static Assembly ResourceAssembly
	{
		get
		{
			if (_resourceAssembly == null)
			{
				_resourceAssembly = Assembly.GetEntryAssembly();
			}
			return _resourceAssembly;
		}
		[FriendAccessAllowed]
		set
		{
			_resourceAssembly = value;
		}
	}

	static BaseUriHelper()
	{
		_siteOfOriginBaseUri = System.IO.Packaging.PackUriHelper.Create(new Uri("SiteOfOrigin://"));
		_packAppBaseUri = System.IO.Packaging.PackUriHelper.Create(new Uri("application://"));
		BaseUriProperty = DependencyProperty.RegisterAttached("BaseUri", typeof(Uri), typeof(BaseUriHelper), new PropertyMetadata((object)null));
		_baseUri = new MS.Internal.SecurityCriticalDataForSet<Uri>(_packAppBaseUri);
		PreloadedPackages.AddPackage(System.IO.Packaging.PackUriHelper.GetPackageUri(SiteOfOriginBaseUri), new SiteOfOriginContainer(), threadSafe: true);
	}

	/// <summary>Gets the value of the BaseUri for a specified <see cref="T:System.Windows.UIElement" />.</summary>
	/// <returns>The base URI of a given element.</returns>
	/// <param name="element">The element from which the property value is read. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="element" /> is null.</exception>
	public static Uri GetBaseUri(DependencyObject element)
	{
		Uri uri = GetBaseUriCore(element);
		if (uri == null)
		{
			uri = BaseUri;
		}
		else if (!uri.IsAbsoluteUri)
		{
			uri = new Uri(BaseUri, uri);
		}
		return uri;
	}

	internal static bool IsPackApplicationUri(Uri uri)
	{
		if (uri.IsAbsoluteUri && SecurityHelper.AreStringTypesEqual(uri.Scheme, System.IO.Packaging.PackUriHelper.UriSchemePack))
		{
			return SecurityHelper.AreStringTypesEqual(System.IO.Packaging.PackUriHelper.GetPackageUri(uri).GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped), "application:///");
		}
		return false;
	}

	[FriendAccessAllowed]
	internal static void GetAssemblyAndPartNameFromPackAppUri(Uri uri, out Assembly assembly, out string partName)
	{
		GetAssemblyNameAndPart(new Uri(uri.AbsolutePath, UriKind.Relative), out partName, out var assemblyName, out var assemblyVersion, out var assemblyKey);
		if (string.IsNullOrEmpty(assemblyName))
		{
			assembly = ResourceAssembly;
		}
		else
		{
			assembly = GetLoadedAssembly(assemblyName, assemblyVersion, assemblyKey);
		}
	}

	[FriendAccessAllowed]
	internal static Assembly GetLoadedAssembly(string assemblyName, string assemblyVersion, string assemblyKey)
	{
		AssemblyName assemblyName2 = new AssemblyName(assemblyName);
		assemblyName2.CultureInfo = new CultureInfo(string.Empty);
		if (!string.IsNullOrEmpty(assemblyVersion))
		{
			assemblyName2.Version = new Version(assemblyVersion);
		}
		byte[] array = ParseAssemblyKey(assemblyKey);
		if (array != null)
		{
			assemblyName2.SetPublicKeyToken(array);
		}
		Assembly assembly = SafeSecurityHelper.GetLoadedAssembly(assemblyName2);
		if (assembly == null)
		{
			assembly = Assembly.Load(assemblyName2);
		}
		return assembly;
	}

	[FriendAccessAllowed]
	internal static void GetAssemblyNameAndPart(Uri uri, out string partName, out string assemblyName, out string assemblyVersion, out string assemblyKey)
	{
		Invariant.Assert(uri != null && !uri.IsAbsoluteUri, "This method accepts relative uri only.");
		string text = uri.ToString();
		int num = 0;
		if (text[0] == '/')
		{
			num = 1;
		}
		partName = text.Substring(num);
		assemblyName = string.Empty;
		assemblyVersion = string.Empty;
		assemblyKey = string.Empty;
		int num2 = text.IndexOf('/', num);
		string text2 = string.Empty;
		bool flag = false;
		if (num2 > 0)
		{
			text2 = text.Substring(num, num2 - num);
			if (text2.EndsWith(";component", StringComparison.OrdinalIgnoreCase))
			{
				partName = text.Substring(num2 + 1);
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		string[] array = text2.Split(';');
		int num3 = array.Length;
		if (num3 > 4 || num3 < 2)
		{
			throw new UriFormatException(SR.WrongFirstSegment);
		}
		assemblyName = Uri.UnescapeDataString(array[0]);
		for (int i = 1; i < num3 - 1; i++)
		{
			if (array[i].StartsWith("v", StringComparison.OrdinalIgnoreCase))
			{
				if (!string.IsNullOrEmpty(assemblyVersion))
				{
					throw new UriFormatException(SR.WrongFirstSegment);
				}
				assemblyVersion = array[i].Substring(1);
			}
			else
			{
				if (!string.IsNullOrEmpty(assemblyKey))
				{
					throw new UriFormatException(SR.WrongFirstSegment);
				}
				assemblyKey = array[i];
			}
		}
	}

	[FriendAccessAllowed]
	internal static bool IsComponentEntryAssembly(string component)
	{
		if (component.EndsWith(";component", StringComparison.OrdinalIgnoreCase))
		{
			string[] array = component.Split(';');
			int num = array.Length;
			if (num >= 2 && num <= 4)
			{
				string b = Uri.UnescapeDataString(array[0]);
				Assembly resourceAssembly = ResourceAssembly;
				if (resourceAssembly != null)
				{
					return string.Equals(SafeSecurityHelper.GetAssemblyPartialName(resourceAssembly), b, StringComparison.OrdinalIgnoreCase);
				}
				return false;
			}
		}
		return false;
	}

	[FriendAccessAllowed]
	internal static Uri GetResolvedUri(Uri baseUri, Uri orgUri)
	{
		return new Uri(baseUri, orgUri);
	}

	[FriendAccessAllowed]
	internal static Uri MakeRelativeToSiteOfOriginIfPossible(Uri sUri)
	{
		if (Uri.Compare(sUri, SiteOfOriginBaseUri, UriComponents.Scheme, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase) == 0 && string.Equals(System.IO.Packaging.PackUriHelper.GetPackageUri(sUri).GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped), "siteoforigin:///", StringComparison.OrdinalIgnoreCase))
		{
			return new Uri(sUri.GetComponents(UriComponents.SchemeAndServer, UriFormat.UriEscaped)).MakeRelativeUri(sUri);
		}
		return sUri;
	}

	[FriendAccessAllowed]
	internal static Uri ConvertPackUriToAbsoluteExternallyVisibleUri(Uri packUri)
	{
		Invariant.Assert(packUri.IsAbsoluteUri && SecurityHelper.AreStringTypesEqual(packUri.Scheme, PackAppBaseUri.Scheme));
		Uri uri = MakeRelativeToSiteOfOriginIfPossible(packUri);
		if (!uri.IsAbsoluteUri)
		{
			return new Uri(SiteOfOriginContainer.SiteOfOrigin, uri);
		}
		throw new InvalidOperationException(SR.Format(SR.CannotNavigateToApplicationResourcesInWebBrowser, packUri));
	}

	[FriendAccessAllowed]
	internal static Uri FixFileUri(Uri uri)
	{
		if (uri != null && uri.IsAbsoluteUri && SecurityHelper.AreStringTypesEqual(uri.Scheme, Uri.UriSchemeFile) && string.Compare(uri.OriginalString, 0, Uri.UriSchemeFile, 0, Uri.UriSchemeFile.Length, StringComparison.OrdinalIgnoreCase) != 0)
		{
			return new Uri(uri.AbsoluteUri);
		}
		return uri;
	}

	internal static Uri AppendAssemblyVersion(Uri uri, Assembly assemblyInfo)
	{
		Uri uri2 = null;
		Uri uri3 = null;
		AssemblyName assemblyName = new AssemblyName(assemblyInfo.FullName);
		string value = assemblyName.Version?.ToString();
		if (uri != null && !string.IsNullOrEmpty(value))
		{
			if (uri.IsAbsoluteUri)
			{
				if (IsPackApplicationUri(uri))
				{
					uri2 = new Uri(uri.AbsolutePath, UriKind.Relative);
					uri3 = new Uri(uri.GetLeftPart(UriPartial.Authority), UriKind.Absolute);
				}
			}
			else
			{
				uri2 = uri;
			}
			if (uri2 != null)
			{
				GetAssemblyNameAndPart(uri2, out var partName, out var assemblyName2, out var assemblyVersion, out var assemblyKey);
				bool flag = !string.IsNullOrEmpty(assemblyKey);
				if (!string.IsNullOrEmpty(assemblyName2) && string.IsNullOrEmpty(assemblyVersion) && assemblyName2.Equals(assemblyName.Name, StringComparison.Ordinal) && (!flag || AssemblyMatchesKeyString(assemblyName, assemblyKey)))
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append('/');
					stringBuilder.Append(assemblyName2);
					stringBuilder.Append(';');
					stringBuilder.Append("v");
					stringBuilder.Append(value);
					if (flag)
					{
						stringBuilder.Append(';');
						stringBuilder.Append(assemblyKey);
					}
					stringBuilder.Append(";component");
					stringBuilder.Append('/');
					stringBuilder.Append(partName);
					string text = stringBuilder.ToString();
					if (uri3 != null)
					{
						return new Uri(uri3, text);
					}
					return new Uri(text, UriKind.Relative);
				}
			}
		}
		return null;
	}

	internal static Uri GetBaseUriCore(DependencyObject element)
	{
		Uri uri = null;
		if (element == null)
		{
			throw new ArgumentNullException("element");
		}
		DependencyObject dependencyObject = element;
		while (dependencyObject != null)
		{
			uri = dependencyObject.GetValue(BaseUriProperty) as Uri;
			if (uri != null)
			{
				break;
			}
			if (dependencyObject is IUriContext uriContext)
			{
				uri = uriContext.BaseUri;
				if (uri != null)
				{
					break;
				}
			}
			if (dependencyObject is UIElement uIElement)
			{
				dependencyObject = uIElement.GetUIParent(continuePastVisualTree: true);
				continue;
			}
			if (dependencyObject is ContentElement contentElement)
			{
				dependencyObject = contentElement.Parent;
				continue;
			}
			if (!(dependencyObject is Visual reference))
			{
				break;
			}
			dependencyObject = VisualTreeHelper.GetParent(reference);
		}
		return uri;
	}

	private static bool AssemblyMatchesKeyString(AssemblyName asmName, string assemblyKey)
	{
		byte[] curKeyToken = ParseAssemblyKey(assemblyKey);
		return SafeSecurityHelper.IsSameKeyToken(asmName.GetPublicKeyToken(), curKeyToken);
	}

	private static byte[] ParseAssemblyKey(string assemblyKey)
	{
		if (!string.IsNullOrEmpty(assemblyKey))
		{
			int num = assemblyKey.Length / 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = byte.Parse(assemblyKey.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
			}
			return array;
		}
		return null;
	}
}
