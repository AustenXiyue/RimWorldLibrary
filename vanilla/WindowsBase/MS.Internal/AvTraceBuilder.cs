using System.Globalization;
using System.Text;

namespace MS.Internal;

internal class AvTraceBuilder
{
	private StringBuilder _sb;

	public AvTraceBuilder()
	{
		_sb = new StringBuilder();
	}

	public AvTraceBuilder(string message)
	{
		_sb = new StringBuilder(message);
	}

	public void Append(string message)
	{
		_sb.Append(message);
	}

	public void AppendFormat(string message, params object[] args)
	{
		object[] array = new object[args.Length];
		for (int i = 0; i < args.Length; i++)
		{
			string text = args[i] as string;
			array[i] = ((text != null) ? text : AvTrace.ToStringHelper(args[i]));
		}
		_sb.AppendFormat(CultureInfo.InvariantCulture, message, array);
	}

	public void AppendFormat(string message, object arg1)
	{
		_sb.AppendFormat(CultureInfo.InvariantCulture, message, new object[1] { AvTrace.ToStringHelper(arg1) });
	}

	public void AppendFormat(string message, object arg1, object arg2)
	{
		_sb.AppendFormat(CultureInfo.InvariantCulture, message, new object[2]
		{
			AvTrace.ToStringHelper(arg1),
			AvTrace.ToStringHelper(arg2)
		});
	}

	public void AppendFormat(string message, string arg1)
	{
		_sb.AppendFormat(CultureInfo.InvariantCulture, message, new object[1] { AvTrace.AntiFormat(arg1) });
	}

	public void AppendFormat(string message, string arg1, string arg2)
	{
		_sb.AppendFormat(CultureInfo.InvariantCulture, message, new object[2]
		{
			AvTrace.AntiFormat(arg1),
			AvTrace.AntiFormat(arg2)
		});
	}

	public override string ToString()
	{
		return _sb.ToString();
	}
}
