using System;
using System.Runtime.InteropServices;
using MS.Internal.WindowsBase;

namespace MS.Internal.Security;

[MS.Internal.WindowsBase.FriendAccessAllowed]
internal sealed class AttachmentService : IDisposable
{
	[ComImport]
	[Guid("4125DD96-E03A-4103-8F70-E0597D803B9C")]
	private class AttachmentServices
	{
	}

	[ComImport]
	[Guid("73DB1241-1E85-4581-8E4F-A81E1D0F8C57")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface ISecuritySuppressedIAttachmentExecute
	{
		int SetClientTitle(string pszTitle);

		int SetClientGuid(ref Guid guid);

		int SetLocalPath(string pszLocalPath);

		int SetFileName(string pszFileName);

		int SetSource(string pszSource);

		int SetReferrer(string pszReferrer);

		int CheckPolicy();

		int Prompt(nint hwnd, ATTACHMENT_PROMPT prompt, out ATTACHMENT_ACTION paction);

		int Save();

		int Execute(nint hwnd, string pszVerb, out nint phProcess);

		int SaveWithUI(nint hwnd);

		int ClearClientState();
	}

	private enum ATTACHMENT_PROMPT
	{
		ATTACHMENT_PROMPT_NONE,
		ATTACHMENT_PROMPT_SAVE,
		ATTACHMENT_PROMPT_EXEC,
		ATTACHMENT_PROMPT_EXEC_OR_SAVE
	}

	private enum ATTACHMENT_ACTION
	{
		ATTACHMENT_ACTION_CANCEL,
		ATTACHMENT_ACTION_SAVE,
		ATTACHMENT_ACTION_EXEC
	}

	private ISecuritySuppressedIAttachmentExecute _native;

	private readonly Guid _clientId = new Guid("{D5734190-005C-4d76-B0DD-2FA89BE0B622}");

	private AttachmentService()
	{
		_native = (ISecuritySuppressedIAttachmentExecute)new AttachmentServices();
		_native.SetClientGuid(ref _clientId);
	}

	internal static void SaveWithUI(nint parent, Uri source, Uri target)
	{
		using AttachmentService attachmentService = new AttachmentService();
		ISecuritySuppressedIAttachmentExecute native = attachmentService._native;
		native.SetSource(source.OriginalString);
		native.SetLocalPath(target.LocalPath);
		native.SaveWithUI(parent);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	private void Dispose(bool disposing)
	{
		if (disposing && _native != null)
		{
			Marshal.ReleaseComObject(_native);
			_native = null;
		}
	}

	~AttachmentService()
	{
		Dispose(disposing: true);
	}
}
