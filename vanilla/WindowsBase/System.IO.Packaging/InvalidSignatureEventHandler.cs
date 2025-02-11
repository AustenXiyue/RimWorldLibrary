namespace System.IO.Packaging;

/// <summary>Represents the method that handles the <see cref="E:System.IO.Packaging.PackageDigitalSignatureManager.InvalidSignatureEvent" /> that is raised when <see cref="M:System.IO.Packaging.PackageDigitalSignatureManager.VerifySignatures(System.Boolean)" /> detects an invalid signature.</summary>
/// <param name="sender">The invalid <see cref="T:System.IO.Packaging.PackageDigitalSignature" /> source of the event.</param>
/// <param name="e">The event data.</param>
public delegate void InvalidSignatureEventHandler(object sender, SignatureVerificationEventArgs e);
