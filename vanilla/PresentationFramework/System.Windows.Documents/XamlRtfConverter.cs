namespace System.Windows.Documents;

internal class XamlRtfConverter
{
	internal const int RtfCodePage = 1252;

	private bool _forceParagraph;

	private WpfPayload _wpfPayload;

	internal bool ForceParagraph
	{
		get
		{
			return _forceParagraph;
		}
		set
		{
			_forceParagraph = value;
		}
	}

	internal WpfPayload WpfPayload
	{
		get
		{
			return _wpfPayload;
		}
		set
		{
			_wpfPayload = value;
		}
	}

	internal XamlRtfConverter()
	{
	}

	internal string ConvertXamlToRtf(string xamlContent)
	{
		if (xamlContent == null)
		{
			throw new ArgumentNullException("xamlContent");
		}
		string result = string.Empty;
		if (xamlContent != string.Empty)
		{
			XamlToRtfWriter xamlToRtfWriter = new XamlToRtfWriter(xamlContent);
			if (WpfPayload != null)
			{
				xamlToRtfWriter.WpfPayload = WpfPayload;
			}
			xamlToRtfWriter.Process();
			result = xamlToRtfWriter.Output;
		}
		return result;
	}

	internal string ConvertRtfToXaml(string rtfContent)
	{
		if (rtfContent == null)
		{
			throw new ArgumentNullException("rtfContent");
		}
		string result = string.Empty;
		if (rtfContent != string.Empty)
		{
			RtfToXamlReader rtfToXamlReader = new RtfToXamlReader(rtfContent);
			rtfToXamlReader.ForceParagraph = ForceParagraph;
			if (WpfPayload != null)
			{
				rtfToXamlReader.WpfPayload = WpfPayload;
			}
			rtfToXamlReader.Process();
			result = rtfToXamlReader.Output;
		}
		return result;
	}
}
