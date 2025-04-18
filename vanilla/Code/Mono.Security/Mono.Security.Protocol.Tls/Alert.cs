namespace Mono.Security.Protocol.Tls;

internal class Alert
{
	private AlertLevel level;

	private AlertDescription description;

	public AlertLevel Level => level;

	public AlertDescription Description => description;

	public string Message => GetAlertMessage(description);

	public bool IsWarning
	{
		get
		{
			if (level != AlertLevel.Warning)
			{
				return false;
			}
			return true;
		}
	}

	public bool IsCloseNotify
	{
		get
		{
			if (IsWarning && description == AlertDescription.CloseNotify)
			{
				return true;
			}
			return false;
		}
	}

	public Alert(AlertDescription description)
	{
		this.description = description;
		level = inferAlertLevel(description);
	}

	public Alert(AlertLevel level, AlertDescription description)
	{
		this.level = level;
		this.description = description;
	}

	private static AlertLevel inferAlertLevel(AlertDescription description)
	{
		switch (description)
		{
		case AlertDescription.CloseNotify:
		case AlertDescription.UserCancelled:
		case AlertDescription.NoRenegotiation:
			return AlertLevel.Warning;
		default:
			return AlertLevel.Fatal;
		}
	}

	public static string GetAlertMessage(AlertDescription description)
	{
		return "The authentication or decryption has failed.";
	}
}
