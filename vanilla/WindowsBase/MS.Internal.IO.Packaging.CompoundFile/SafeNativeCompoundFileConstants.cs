namespace MS.Internal.IO.Packaging.CompoundFile;

internal static class SafeNativeCompoundFileConstants
{
	internal const int STGM_READ = 0;

	internal const int STGM_WRITE = 1;

	internal const int STGM_READWRITE = 2;

	internal const int STGM_READWRITE_Bits = 3;

	internal const int STGM_SHARE_DENY_NONE = 64;

	internal const int STGM_SHARE_DENY_READ = 48;

	internal const int STGM_SHARE_DENY_WRITE = 32;

	internal const int STGM_SHARE_EXCLUSIVE = 16;

	internal const int STGM_PRIORITY = 262144;

	internal const int STGM_CREATE = 4096;

	internal const int STGM_CONVERT = 131072;

	internal const int STGM_FAILIFTHERE = 0;

	internal const int STGM_DIRECT = 0;

	internal const int STGM_TRANSACTED = 65536;

	internal const int STGM_NOSCRATCH = 1048576;

	internal const int STGM_NOSNAPSHOT = 2097152;

	internal const int STGM_SIMPLE = 134217728;

	internal const int STGM_DIRECT_SWMR = 4194304;

	internal const int STGM_DELETEONRELEASE = 67108864;

	internal const int STREAM_SEEK_SET = 0;

	internal const int STREAM_SEEK_CUR = 1;

	internal const int STREAM_SEEK_END = 2;

	internal const int STATFLAG_NONAME = 1;

	internal const int STATFLAG_NOOPEN = 2;

	internal const int STGTY_STORAGE = 1;

	internal const int STGTY_STREAM = 2;

	internal const int STGTY_LOCKBYTES = 3;

	internal const int STGTY_PROPERTY = 4;

	internal const uint PROPSETFLAG_ANSI = 2u;

	internal const int S_OK = 0;

	internal const int S_FALSE = 1;

	internal const int STG_E_FILENOTFOUND = -2147287038;

	internal const int STG_E_ACCESSDENIED = -2147287035;

	internal const int STG_E_FILEALREADYEXISTS = -2147286960;

	internal const int STG_E_INVALIDNAME = -2147286788;

	internal const int STG_E_INVALIDFLAG = -2147286785;
}
