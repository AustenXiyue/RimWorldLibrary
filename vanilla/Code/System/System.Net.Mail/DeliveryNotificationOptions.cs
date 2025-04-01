namespace System.Net.Mail;

/// <summary>Describes the delivery notification options for e-mail.</summary>
[Flags]
public enum DeliveryNotificationOptions
{
	/// <summary>No notification information will be sent. The mail server will utilize its configured behavior to determine whether it should generate a delivery notification.</summary>
	None = 0,
	/// <summary>Notify if the delivery is successful.</summary>
	OnSuccess = 1,
	/// <summary>Notify if the delivery is unsuccessful.</summary>
	OnFailure = 2,
	/// <summary>Notify if the delivery is delayed.</summary>
	Delay = 4,
	/// <summary>A notification should not be generated under any circumstances.</summary>
	Never = 0x8000000
}
