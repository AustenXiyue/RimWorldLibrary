using System.ComponentModel;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace System.Windows.Media;

internal static class TypeConverterHelper
{
	internal static UriHolder GetUriFromUriContext(ITypeDescriptorContext context, object inputString)
	{
		UriHolder result = default(UriHolder);
		if (inputString is string)
		{
			result.OriginalUri = new Uri((string)inputString, UriKind.RelativeOrAbsolute);
		}
		else
		{
			result.OriginalUri = (Uri)inputString;
		}
		if (!result.OriginalUri.IsAbsoluteUri && context != null)
		{
			IUriContext uriContext = (IUriContext)context.GetService(typeof(IUriContext));
			if (uriContext != null)
			{
				if (uriContext.BaseUri != null)
				{
					result.BaseUri = uriContext.BaseUri;
					if (!result.BaseUri.IsAbsoluteUri)
					{
						result.BaseUri = new Uri(BaseUriHelper.BaseUri, result.BaseUri);
					}
				}
				else
				{
					result.BaseUri = BaseUriHelper.BaseUri;
				}
			}
		}
		return result;
	}
}
