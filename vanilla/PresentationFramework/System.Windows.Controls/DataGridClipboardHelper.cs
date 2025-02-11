using System.Globalization;
using System.IO;
using System.Text;
using MS.Internal.Text;

namespace System.Windows.Controls;

internal static class DataGridClipboardHelper
{
	private const string DATAGRIDVIEW_htmlPrefix = "Version:1.0\r\nStartHTML:00000097\r\nEndHTML:{0}\r\nStartFragment:00000133\r\nEndFragment:{1}\r\n";

	private const string DATAGRIDVIEW_htmlStartFragment = "<HTML>\r\n<BODY>\r\n<!--StartFragment-->";

	private const string DATAGRIDVIEW_htmlEndFragment = "\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>";

	internal static void FormatCell(object cellValue, bool firstCell, bool lastCell, StringBuilder sb, string format)
	{
		bool flag = string.Equals(format, DataFormats.CommaSeparatedValue, StringComparison.OrdinalIgnoreCase);
		if (flag || string.Equals(format, DataFormats.Text, StringComparison.OrdinalIgnoreCase) || string.Equals(format, DataFormats.UnicodeText, StringComparison.OrdinalIgnoreCase))
		{
			if (cellValue != null)
			{
				bool escapeApplied = false;
				int length = sb.Length;
				FormatPlainText(cellValue.ToString(), flag, new StringWriter(sb, CultureInfo.CurrentCulture), ref escapeApplied);
				if (escapeApplied)
				{
					sb.Insert(length, '"');
				}
			}
			if (lastCell)
			{
				sb.Append('\r');
				sb.Append('\n');
			}
			else
			{
				sb.Append(flag ? ',' : '\t');
			}
		}
		else if (string.Equals(format, DataFormats.Html, StringComparison.OrdinalIgnoreCase))
		{
			if (firstCell)
			{
				sb.Append("<TR>");
			}
			sb.Append("<TD>");
			if (cellValue != null)
			{
				FormatPlainTextAsHtml(cellValue.ToString(), new StringWriter(sb, CultureInfo.CurrentCulture));
			}
			else
			{
				sb.Append("&nbsp;");
			}
			sb.Append("</TD>");
			if (lastCell)
			{
				sb.Append("</TR>");
			}
		}
	}

	internal static void GetClipboardContentForHtml(StringBuilder content)
	{
		content.Insert(0, "<TABLE>");
		content.Append("</TABLE>");
		byte[] bytes = Encoding.Unicode.GetBytes(content.ToString());
		byte[] array = InternalEncoding.Convert(Encoding.Unicode, Encoding.UTF8, bytes);
		int num = 135 + array.Length;
		int num2 = num + 36;
		string value = string.Format(CultureInfo.InvariantCulture, "Version:1.0\r\nStartHTML:00000097\r\nEndHTML:{0}\r\nStartFragment:00000133\r\nEndFragment:{1}\r\n", num2.ToString("00000000", CultureInfo.InvariantCulture), num.ToString("00000000", CultureInfo.InvariantCulture)) + "<HTML>\r\n<BODY>\r\n<!--StartFragment-->";
		content.Insert(0, value);
		content.Append("\r\n<!--EndFragment-->\r\n</BODY>\r\n</HTML>");
	}

	private static void FormatPlainText(string s, bool csv, TextWriter output, ref bool escapeApplied)
	{
		if (s == null)
		{
			return;
		}
		int length = s.Length;
		for (int i = 0; i < length; i++)
		{
			char c = s[i];
			switch (c)
			{
			case '\t':
				if (!csv)
				{
					output.Write(' ');
				}
				else
				{
					output.Write('\t');
				}
				break;
			case '"':
				if (csv)
				{
					output.Write("\"\"");
					escapeApplied = true;
				}
				else
				{
					output.Write('"');
				}
				break;
			case ',':
				if (csv)
				{
					escapeApplied = true;
				}
				output.Write(',');
				break;
			default:
				output.Write(c);
				break;
			}
		}
		if (escapeApplied)
		{
			output.Write('"');
		}
	}

	private static void FormatPlainTextAsHtml(string s, TextWriter output)
	{
		if (s == null)
		{
			return;
		}
		int length = s.Length;
		char c = '\0';
		for (int i = 0; i < length; i++)
		{
			char c2 = s[i];
			switch (c2)
			{
			case '<':
				output.Write("&lt;");
				break;
			case '>':
				output.Write("&gt;");
				break;
			case '"':
				output.Write("&quot;");
				break;
			case '&':
				output.Write("&amp;");
				break;
			case ' ':
				if (c == ' ')
				{
					output.Write("&nbsp;");
				}
				else
				{
					output.Write(c2);
				}
				break;
			case '\n':
				output.Write("<br>");
				break;
			default:
				if (c2 >= '\u00a0' && c2 < 'Ā')
				{
					output.Write("&#");
					int num = c2;
					output.Write(num.ToString(NumberFormatInfo.InvariantInfo));
					output.Write(';');
				}
				else
				{
					output.Write(c2);
				}
				break;
			case '\r':
				break;
			}
			c = c2;
		}
	}
}
