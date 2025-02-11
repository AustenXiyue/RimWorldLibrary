using MS.Internal.PresentationCore;

namespace MS.Internal.FontCache;

[FriendAccessAllowed]
internal static class HashFn
{
	private const int HASH_MULTIPLIER = 101;

	internal static int HashMultiply(int hash)
	{
		return hash * 101;
	}

	internal static int HashScramble(int hash)
	{
		return (int)((uint)(314159269 * hash) % 1000000007u);
	}

	internal unsafe static int HashMemory(void* pv, int numBytes, int hash)
	{
		byte* ptr = (byte*)pv;
		while (numBytes-- > 0)
		{
			hash = HashMultiply(hash) + *ptr;
			ptr++;
		}
		return hash;
	}

	internal static int HashString(string s, int hash)
	{
		foreach (char c in s)
		{
			hash = HashMultiply(hash) + c;
		}
		return hash;
	}
}
