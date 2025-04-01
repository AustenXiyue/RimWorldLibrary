using System.Security.Authentication;

namespace System.Net;

internal static class SecurityProtocol
{
	public const SslProtocols AllowedSecurityProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

	public const SslProtocols DefaultSecurityProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;

	public const SslProtocols SystemDefaultSecurityProtocols = SslProtocols.None;

	public static void ThrowOnNotAllowed(SslProtocols protocols, bool allowNone = true)
	{
		if ((!allowNone && protocols == SslProtocols.None) || (protocols & ~(SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12)) != 0)
		{
			throw new NotSupportedException("The requested security protocol is not supported.");
		}
	}
}
