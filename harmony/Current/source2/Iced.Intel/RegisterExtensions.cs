namespace Iced.Intel;

internal static class RegisterExtensions
{
	public static bool IsSegmentRegister(this Register register)
	{
		if (Register.ES <= register)
		{
			return register <= Register.GS;
		}
		return false;
	}

	public static bool IsGPR(this Register register)
	{
		if (Register.AL <= register)
		{
			return register <= Register.R15;
		}
		return false;
	}

	public static bool IsGPR8(this Register register)
	{
		if (Register.AL <= register)
		{
			return register <= Register.R15L;
		}
		return false;
	}

	public static bool IsGPR16(this Register register)
	{
		if (Register.AX <= register)
		{
			return register <= Register.R15W;
		}
		return false;
	}

	public static bool IsGPR32(this Register register)
	{
		if (Register.EAX <= register)
		{
			return register <= Register.R15D;
		}
		return false;
	}

	public static bool IsGPR64(this Register register)
	{
		if (Register.RAX <= register)
		{
			return register <= Register.R15;
		}
		return false;
	}

	public static bool IsXMM(this Register register)
	{
		if (Register.XMM0 <= register)
		{
			return register <= Register.XMM31;
		}
		return false;
	}

	public static bool IsYMM(this Register register)
	{
		if (Register.YMM0 <= register)
		{
			return register <= Register.YMM31;
		}
		return false;
	}

	public static bool IsZMM(this Register register)
	{
		if (Register.ZMM0 <= register)
		{
			return register <= Register.ZMM31;
		}
		return false;
	}

	public static bool IsIP(this Register register)
	{
		if (register != Register.EIP)
		{
			return register == Register.RIP;
		}
		return true;
	}

	public static bool IsK(this Register register)
	{
		if (Register.K0 <= register)
		{
			return register <= Register.K7;
		}
		return false;
	}

	public static bool IsCR(this Register register)
	{
		if (Register.CR0 <= register)
		{
			return register <= Register.CR15;
		}
		return false;
	}

	public static bool IsDR(this Register register)
	{
		if (Register.DR0 <= register)
		{
			return register <= Register.DR15;
		}
		return false;
	}

	public static bool IsTR(this Register register)
	{
		if (Register.TR0 <= register)
		{
			return register <= Register.TR7;
		}
		return false;
	}

	public static bool IsST(this Register register)
	{
		if (Register.ST0 <= register)
		{
			return register <= Register.ST7;
		}
		return false;
	}

	public static bool IsBND(this Register register)
	{
		if (Register.BND0 <= register)
		{
			return register <= Register.BND3;
		}
		return false;
	}

	public static bool IsMM(this Register register)
	{
		if (Register.MM0 <= register)
		{
			return register <= Register.MM7;
		}
		return false;
	}

	public static bool IsTMM(this Register register)
	{
		if (Register.TMM0 <= register)
		{
			return register <= Register.TMM7;
		}
		return false;
	}

	public static bool IsVectorRegister(this Register register)
	{
		if (Register.XMM0 <= register)
		{
			return register <= Register.ZMM31;
		}
		return false;
	}
}
