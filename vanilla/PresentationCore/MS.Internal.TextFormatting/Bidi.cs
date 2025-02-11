using System;
using System.Collections.Generic;
using System.Windows.Media.TextFormatting;

namespace MS.Internal.TextFormatting;

internal static class Bidi
{
	private static class Helper
	{
		public static ulong LeftShift(ulong x, byte y)
		{
			return x << (int)y;
		}

		public static ulong LeftShift(ulong x, int y)
		{
			return x << y;
		}

		public static void SetBit(ref ulong x, byte y)
		{
			x |= LeftShift(1uL, y);
		}

		public static void ResetBit(ref ulong x, int y)
		{
			x &= ~LeftShift(1uL, y);
		}

		public static bool IsBitSet(ulong x, byte y)
		{
			return (x & LeftShift(1uL, y)) != 0;
		}

		public static bool IsBitSet(ulong x, int y)
		{
			return (x & LeftShift(1uL, y)) != 0;
		}

		public static bool IsOdd(byte x)
		{
			return (x & 1) != 0;
		}

		public static bool IsOdd(int x)
		{
			return (x & 1) != 0;
		}
	}

	internal class BidiStack
	{
		private const byte EmbeddingLevelInvalid = 62;

		private ulong stack;

		private byte currentStackLevel;

		public BidiStack()
		{
			currentStackLevel = 0;
		}

		public bool Init(ulong initialStack)
		{
			byte maximumLevel = GetMaximumLevel(initialStack);
			byte minimumLevel = GetMinimumLevel(initialStack);
			if (maximumLevel >= 62 || minimumLevel < 0)
			{
				return false;
			}
			stack = initialStack;
			currentStackLevel = maximumLevel;
			return true;
		}

		public bool Push(bool pushToGreaterEven)
		{
			if (!PushCore(ref stack, pushToGreaterEven, currentStackLevel, out var newMaximumLevel))
			{
				return false;
			}
			currentStackLevel = newMaximumLevel;
			return true;
		}

		public bool Pop()
		{
			if (!PopCore(ref stack, currentStackLevel, out var newMaximumLevel))
			{
				return false;
			}
			currentStackLevel = newMaximumLevel;
			return true;
		}

		public byte GetStackBottom()
		{
			return GetMinimumLevel(stack);
		}

		public byte GetCurrentLevel()
		{
			return currentStackLevel;
		}

		public ulong GetData()
		{
			return stack;
		}

		internal static bool Push(ref ulong stack, bool pushToGreaterEven, out byte topLevel)
		{
			byte maximumLevel = GetMaximumLevel(stack);
			return PushCore(ref stack, pushToGreaterEven, maximumLevel, out topLevel);
		}

		internal static bool Pop(ref ulong stack, out byte topLevel)
		{
			byte maximumLevel = GetMaximumLevel(stack);
			return PopCore(ref stack, maximumLevel, out topLevel);
		}

		internal static byte GetMaximumLevel(ulong inputStack)
		{
			byte result = 0;
			for (int num = 63; num >= 0; num--)
			{
				if (Helper.IsBitSet(inputStack, num))
				{
					result = (byte)num;
					break;
				}
			}
			return result;
		}

		private static bool PushCore(ref ulong stack, bool pushToGreaterEven, byte currentStackLevel, out byte newMaximumLevel)
		{
			newMaximumLevel = (pushToGreaterEven ? GreaterEven(currentStackLevel) : GreaterOdd(currentStackLevel));
			if (newMaximumLevel >= 62)
			{
				newMaximumLevel = currentStackLevel;
				return false;
			}
			Helper.SetBit(ref stack, newMaximumLevel);
			return true;
		}

		private static bool PopCore(ref ulong stack, byte currentStackLevel, out byte newMaximumLevel)
		{
			newMaximumLevel = currentStackLevel;
			if (currentStackLevel == 0 || (currentStackLevel == 1 && (stack & 1) == 0L))
			{
				return false;
			}
			newMaximumLevel = (Helper.IsBitSet(stack, currentStackLevel - 1) ? ((byte)(currentStackLevel - 1)) : ((byte)(currentStackLevel - 2)));
			Helper.ResetBit(ref stack, currentStackLevel);
			return true;
		}

		private static byte GetMinimumLevel(ulong inputStack)
		{
			byte result = byte.MaxValue;
			for (byte b = 0; b <= 63; b++)
			{
				if (Helper.IsBitSet(inputStack, b))
				{
					result = b;
					break;
				}
			}
			return result;
		}

		private static byte GreaterEven(byte level)
		{
			if (!Helper.IsOdd(level))
			{
				return (byte)(level + 2);
			}
			return (byte)(level + 1);
		}

		private static byte GreaterOdd(byte level)
		{
			if (!Helper.IsOdd(level))
			{
				return (byte)(level + 1);
			}
			return (byte)(level + 2);
		}
	}

	public enum Flags : uint
	{
		DirectionLeftToRight = 0u,
		DirectionRightToLeft = 1u,
		FirstStrongAsBaseDirection = 2u,
		PreviousStrongIsArabic = 4u,
		ContinueAnalysis = 8u,
		IncompleteText = 0x10u,
		MaximumHint = 0x20u,
		IgnoreDirectionalControls = 0x40u,
		OverrideEuropeanNumberResolution = 0x80u
	}

	private enum OverrideClass
	{
		OverrideClassNeutral,
		OverrideClassLeft,
		OverrideClassRight
	}

	private enum StateMachineState
	{
		S_L,
		S_AL,
		S_R,
		S_AN,
		S_EN,
		S_ET,
		S_ANfCS,
		S_ENfCS,
		S_N
	}

	private enum StateMachineAction
	{
		ST_ST,
		ST_ET,
		ST_NUMSEP,
		ST_N,
		SEP_ST,
		CS_NUM,
		SEP_ET,
		SEP_NUMSEP,
		SEP_N,
		ES_AN,
		ET_ET,
		ET_NUMSEP,
		ET_EN,
		ET_N,
		NUM_NUMSEP,
		NUM_NUM,
		EN_L,
		EN_AL,
		EN_ET,
		EN_N,
		BN_ST,
		NSM_ST,
		NSM_ET,
		N_ST,
		N_ET
	}

	internal class State
	{
		private ulong m_levelStack;

		private ulong m_overrideLevels;

		protected DirectionClass NumberClass;

		protected DirectionClass StrongCharClass;

		private ushort m_overflow;

		public virtual DirectionClass LastStrongClass
		{
			get
			{
				return StrongCharClass;
			}
			set
			{
				StrongCharClass = value;
			}
		}

		public virtual DirectionClass LastNumberClass
		{
			get
			{
				return NumberClass;
			}
			set
			{
				NumberClass = value;
			}
		}

		public ulong LevelStack
		{
			get
			{
				return m_levelStack;
			}
			set
			{
				m_levelStack = value;
			}
		}

		public ulong OverrideLevels
		{
			get
			{
				return m_overrideLevels;
			}
			set
			{
				m_overrideLevels = value;
			}
		}

		public ushort Overflow
		{
			get
			{
				return m_overflow;
			}
			set
			{
				m_overflow = value;
			}
		}

		public State(bool isRightToLeft)
		{
			OverrideLevels = 0uL;
			Overflow = 0;
			NumberClass = DirectionClass.Left;
			StrongCharClass = DirectionClass.Left;
			LevelStack = ((!isRightToLeft) ? 1u : 2u);
		}
	}

	private static readonly StateMachineAction[,] Action;

	private static readonly StateMachineState[,] NextState;

	private static readonly byte[,] ImplictPush;

	private static readonly byte[,] CharProperty;

	private static readonly StateMachineState[] ClassToState;

	private static readonly byte[] FastPathClass;

	private static char CharHidden;

	private const byte ParagraphTerminatorLevel = byte.MaxValue;

	private const int PositionInvalid = -1;

	private const byte BaseLevelLeft = 0;

	private const byte BaseLevelRight = 1;

	private const uint EmptyStack = 0u;

	private const uint StackLtr = 1u;

	private const uint StackRtl = 2u;

	private const int MaxLevel = 63;

	static Bidi()
	{
		CharHidden = '\uffff';
		Action = new StateMachineAction[9, 11]
		{
			{
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.EN_L,
				StateMachineAction.ST_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.CS_NUM,
				StateMachineAction.NSM_ST,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.EN_AL,
				StateMachineAction.ST_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.CS_NUM,
				StateMachineAction.NSM_ST,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.SEP_ST,
				StateMachineAction.CS_NUM,
				StateMachineAction.NSM_ST,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.NUM_NUM,
				StateMachineAction.ST_ST,
				StateMachineAction.ES_AN,
				StateMachineAction.CS_NUM,
				StateMachineAction.CS_NUM,
				StateMachineAction.NSM_ST,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.ST_ST,
				StateMachineAction.NUM_NUM,
				StateMachineAction.ST_ST,
				StateMachineAction.CS_NUM,
				StateMachineAction.CS_NUM,
				StateMachineAction.ET_EN,
				StateMachineAction.NSM_ST,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_ET,
				StateMachineAction.ST_ET,
				StateMachineAction.ST_ET,
				StateMachineAction.EN_ET,
				StateMachineAction.ST_ET,
				StateMachineAction.SEP_ET,
				StateMachineAction.SEP_ET,
				StateMachineAction.ET_ET,
				StateMachineAction.NSM_ET,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ET
			},
			{
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.NUM_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.ET_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.NUM_NUMSEP,
				StateMachineAction.ST_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.ET_NUMSEP,
				StateMachineAction.SEP_NUMSEP,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ST
			},
			{
				StateMachineAction.ST_N,
				StateMachineAction.ST_N,
				StateMachineAction.ST_N,
				StateMachineAction.EN_N,
				StateMachineAction.ST_N,
				StateMachineAction.SEP_N,
				StateMachineAction.SEP_N,
				StateMachineAction.ET_N,
				StateMachineAction.NSM_ET,
				StateMachineAction.BN_ST,
				StateMachineAction.N_ET
			}
		};
		NextState = new StateMachineState[9, 11]
		{
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_L,
				StateMachineState.S_L,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_AN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_AL,
				StateMachineState.S_AL,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_R,
				StateMachineState.S_R,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_ANfCS,
				StateMachineState.S_ET,
				StateMachineState.S_AN,
				StateMachineState.S_AN,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_ENfCS,
				StateMachineState.S_ENfCS,
				StateMachineState.S_EN,
				StateMachineState.S_EN,
				StateMachineState.S_EN,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_ET,
				StateMachineState.S_ET,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_N,
				StateMachineState.S_ANfCS,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_N,
				StateMachineState.S_ENfCS,
				StateMachineState.S_N
			},
			{
				StateMachineState.S_L,
				StateMachineState.S_R,
				StateMachineState.S_AN,
				StateMachineState.S_EN,
				StateMachineState.S_AL,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_ET,
				StateMachineState.S_N,
				StateMachineState.S_N,
				StateMachineState.S_N
			}
		};
		ImplictPush = new byte[2, 4]
		{
			{ 0, 1, 2, 2 },
			{ 1, 0, 1, 1 }
		};
		CharProperty = new byte[6, 20]
		{
			{
				1, 1, 0, 0, 1, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				1, 1, 1, 1, 1, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				1, 1, 1, 0, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				1, 1, 1, 1, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				0, 0, 1, 1, 0, 0, 0, 0, 0, 0,
				0, 0, 0, 0, 0, 0, 0, 0, 0, 0
			},
			{
				1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
				1, 0, 0, 0, 0, 0, 0, 0, 0, 0
			}
		};
		ClassToState = new StateMachineState[21]
		{
			StateMachineState.S_L,
			StateMachineState.S_R,
			StateMachineState.S_AN,
			StateMachineState.S_EN,
			StateMachineState.S_AL,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L,
			StateMachineState.S_L
		};
		FastPathClass = new byte[21]
		{
			2, 3, 0, 0, 3, 1, 1, 0, 0, 0,
			1, 0, 0, 0, 0, 0, 0, 0, 1, 1,
			1
		};
	}

	internal static bool GetLastStongAndNumberClass(CharacterBufferRange charString, ref DirectionClass strongClass, ref DirectionClass numberClass)
	{
		int num = charString.Length - 1;
		while (num >= 0)
		{
			int unicodeScalar = charString[num];
			int num2 = 1;
			if ((charString[num] & 0xFC00) == 56320 && num > 0 && (charString[num - 1] & 0xFC00) == 55296)
			{
				unicodeScalar = (((charString[num - 1] & 0x3FF) << 10) | (charString[num] & 0x3FF)) + 65536;
				num2 = 2;
			}
			DirectionClass biDi = Classification.CharAttributeOf(Classification.GetUnicodeClass(unicodeScalar)).BiDi;
			if (biDi == DirectionClass.ParagraphSeparator)
			{
				return false;
			}
			if (CharProperty[1, (uint)biDi] == 1)
			{
				if (numberClass == DirectionClass.ClassInvalid)
				{
					numberClass = biDi;
				}
				if (biDi != DirectionClass.EuropeanNumber)
				{
					strongClass = biDi;
					break;
				}
			}
			num -= num2;
		}
		return true;
	}

	private static bool GetFirstStrongCharacter(CharacterBuffer charBuffer, int ichText, int cchText, ref DirectionClass strongClass)
	{
		DirectionClass directionClass = DirectionClass.ClassInvalid;
		int wordCount;
		for (int i = 0; i < cchText; i += wordCount)
		{
			int num = charBuffer[ichText + i];
			wordCount = 1;
			if ((num & 0xFC00) == 55296)
			{
				num = DoubleWideChar.GetChar(charBuffer, ichText, cchText, i, out wordCount);
			}
			directionClass = Classification.CharAttributeOf(Classification.GetUnicodeClass(num)).BiDi;
			if (CharProperty[0, (uint)directionClass] == 1 || directionClass == DirectionClass.ParagraphSeparator)
			{
				break;
			}
		}
		if (CharProperty[0, (uint)directionClass] == 1)
		{
			strongClass = directionClass;
			return true;
		}
		return false;
	}

	private static void ResolveNeutrals(IList<DirectionClass> characterClass, int classIndex, int count, DirectionClass startClass, DirectionClass endClass, byte runLevel)
	{
		if (characterClass != null && count != 0)
		{
			DirectionClass directionClass = ((startClass == DirectionClass.EuropeanNumber || startClass == DirectionClass.ArabicNumber || startClass == DirectionClass.ArabicLetter) ? DirectionClass.Right : startClass);
			DirectionClass directionClass2 = ((endClass == DirectionClass.EuropeanNumber || endClass == DirectionClass.ArabicNumber || endClass == DirectionClass.ArabicLetter) ? DirectionClass.Right : endClass);
			DirectionClass value = ((directionClass != directionClass2) ? (Helper.IsOdd(runLevel) ? DirectionClass.Right : DirectionClass.Left) : directionClass);
			for (int i = 0; i < count; i++)
			{
				characterClass[i + classIndex] = value;
			}
		}
	}

	private static void ChangeType(IList<DirectionClass> characterClass, int classIndex, int count, DirectionClass newClass)
	{
		if (characterClass != null && count != 0)
		{
			for (int i = 0; i < count; i++)
			{
				characterClass[i + classIndex] = newClass;
			}
		}
	}

	private static int ResolveNeutralAndWeak(IList<DirectionClass> characterClass, int classIndex, int runLength, DirectionClass sor, DirectionClass eor, byte runLevel, State stateIn, State stateOut, bool previousStrongIsArabic, Flags flags)
	{
		int num = -1;
		int num2 = -1;
		DirectionClass directionClass = DirectionClass.ClassInvalid;
		DirectionClass directionClass2 = DirectionClass.ClassInvalid;
		DirectionClass lastNumberClass = DirectionClass.ClassInvalid;
		DirectionClass directionClass3 = DirectionClass.ClassInvalid;
		DirectionClass directionClass4 = DirectionClass.ClassInvalid;
		bool flag = false;
		bool flag2 = false;
		int num3 = 0;
		if (runLength == 0)
		{
			return 0;
		}
		if (stateIn != null)
		{
			directionClass2 = stateIn.LastStrongClass;
			if (stateIn.LastNumberClass != DirectionClass.ClassInvalid)
			{
				lastNumberClass = (directionClass3 = (directionClass = stateIn.LastNumberClass));
			}
			else
			{
				directionClass3 = (directionClass = directionClass2);
			}
		}
		else if (previousStrongIsArabic)
		{
			directionClass3 = DirectionClass.ArabicLetter;
			directionClass = (directionClass2 = sor);
			flag = true;
		}
		else
		{
			directionClass3 = (directionClass = (directionClass2 = sor));
		}
		StateMachineState stateMachineState = ClassToState[(uint)directionClass3];
		int num4 = 0;
		for (num4 = 0; num4 < runLength; num4++)
		{
			directionClass4 = characterClass[num4 + classIndex];
			if (CharProperty[5, (uint)directionClass4] == 0)
			{
				return num3;
			}
			StateMachineAction stateMachineAction = Action[(int)stateMachineState, (uint)directionClass4];
			if (CharProperty[4, (uint)directionClass4] == 1)
			{
				lastNumberClass = directionClass4;
			}
			if (CharProperty[0, (uint)directionClass4] == 1)
			{
				flag = false;
			}
			switch (stateMachineAction)
			{
			case StateMachineAction.ST_ST:
				if (directionClass4 == DirectionClass.ArabicLetter)
				{
					characterClass[num4 + classIndex] = DirectionClass.Right;
				}
				if (num2 != -1)
				{
					num = num2;
					ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					num = (num2 = -1);
				}
				if (directionClass4 != DirectionClass.ArabicNumber || (directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Right))
				{
					directionClass2 = directionClass4;
				}
				flag2 = ((directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Left) ? true : false);
				directionClass = directionClass4;
				break;
			case StateMachineAction.ST_ET:
				if (num == -1)
				{
					num = num2;
				}
				if (directionClass4 == DirectionClass.ArabicLetter)
				{
					characterClass[num4 + classIndex] = DirectionClass.Right;
				}
				ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
				num = (num2 = -1);
				if (directionClass4 != DirectionClass.ArabicNumber || (directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Right))
				{
					directionClass2 = directionClass4;
				}
				flag2 = ((directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Left) ? true : false);
				directionClass = directionClass4;
				break;
			case StateMachineAction.ST_NUMSEP:
			{
				bool flag3 = false;
				if (directionClass4 == DirectionClass.ArabicLetter)
				{
					characterClass[num4 + classIndex] = DirectionClass.Right;
				}
				if ((directionClass2 == DirectionClass.ArabicLetter || flag) && ((directionClass4 == DirectionClass.EuropeanNumber && (flags & Flags.OverrideEuropeanNumberResolution) == 0) || directionClass4 == DirectionClass.ArabicNumber))
				{
					characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
					bool flag4 = true;
					int num5 = 0;
					for (int i = num2; i < num4; i++)
					{
						if (characterClass[i + classIndex] != DirectionClass.CommonSeparator && characterClass[i + classIndex] != DirectionClass.BoundaryNeutral)
						{
							flag4 = false;
							break;
						}
						if (characterClass[i + classIndex] == DirectionClass.CommonSeparator)
						{
							num5++;
						}
					}
					if (flag4 && num5 == 1)
					{
						ChangeType(characterClass, classIndex + num2, num4 - num2, characterClass[num4 + classIndex]);
						flag3 = true;
					}
				}
				else if (directionClass2 == DirectionClass.Left && directionClass4 == DirectionClass.EuropeanNumber)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
				}
				if (!flag3)
				{
					num = num2;
					ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
				}
				num = (num2 = -1);
				if ((directionClass4 != DirectionClass.ArabicNumber || (directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Right)) && ((directionClass2 != 0 && directionClass2 != DirectionClass.ArabicLetter) || directionClass4 != DirectionClass.EuropeanNumber))
				{
					directionClass2 = directionClass4;
				}
				flag2 = ((directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Left) ? true : false);
				directionClass = directionClass4;
				if (characterClass[num4 + classIndex] == DirectionClass.ArabicNumber)
				{
					directionClass4 = DirectionClass.ArabicNumber;
				}
				break;
			}
			case StateMachineAction.ST_N:
				if (directionClass4 == DirectionClass.ArabicLetter)
				{
					characterClass[num4 + classIndex] = DirectionClass.Right;
				}
				ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
				num = (num2 = -1);
				if (directionClass4 != DirectionClass.ArabicNumber || (directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Right))
				{
					directionClass2 = directionClass4;
				}
				flag2 = ((directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Left) ? true : false);
				directionClass = directionClass4;
				break;
			case StateMachineAction.EN_N:
				if ((flags & Flags.OverrideEuropeanNumberResolution) == 0 && (directionClass2 == DirectionClass.ArabicLetter || flag))
				{
					characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
					directionClass4 = DirectionClass.ArabicNumber;
				}
				else if (directionClass2 == DirectionClass.Left)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
				}
				ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
				num = (num2 = -1);
				flag2 = false;
				directionClass = directionClass4;
				break;
			case StateMachineAction.SEP_ST:
				if (num2 != -1)
				{
					num = num2;
					num2 = -1;
				}
				else
				{
					num = num4;
				}
				directionClass = directionClass4;
				break;
			case StateMachineAction.CS_NUM:
				if (num2 == -1)
				{
					num2 = num4;
				}
				directionClass = directionClass4;
				break;
			case StateMachineAction.SEP_ET:
				if (num == -1)
				{
					num = num2;
				}
				num2 = -1;
				directionClass = DirectionClass.GenericNeutral;
				break;
			case StateMachineAction.SEP_NUMSEP:
				num = num2;
				num2 = -1;
				directionClass = DirectionClass.GenericNeutral;
				break;
			case StateMachineAction.SEP_N:
				num2 = -1;
				break;
			case StateMachineAction.ES_AN:
				if (num2 != -1)
				{
					num = num2;
					num2 = -1;
				}
				else
				{
					num = num4;
				}
				directionClass = DirectionClass.GenericNeutral;
				break;
			case StateMachineAction.ET_NUMSEP:
				num = num2;
				num2 = num4;
				directionClass = directionClass4;
				break;
			case StateMachineAction.ET_EN:
				if (num2 == -1)
				{
					num2 = num4;
				}
				if (!(directionClass2 == DirectionClass.ArabicLetter || flag))
				{
					if (directionClass2 == DirectionClass.Left)
					{
						characterClass[num4 + classIndex] = DirectionClass.Left;
					}
					else
					{
						characterClass[num4 + classIndex] = DirectionClass.EuropeanNumber;
					}
					ChangeType(characterClass, classIndex + num2, num4 - num2, characterClass[num4 + classIndex]);
					num2 = -1;
				}
				directionClass = DirectionClass.EuropeanNumber;
				if (num4 < runLength - 1 && (characterClass[num4 + 1 + classIndex] == DirectionClass.EuropeanSeparator || characterClass[num4 + 1 + classIndex] == DirectionClass.CommonSeparator))
				{
					characterClass[num4 + 1 + classIndex] = DirectionClass.GenericNeutral;
				}
				break;
			case StateMachineAction.ET_N:
				if (num2 == -1)
				{
					num2 = num4;
				}
				directionClass = directionClass4;
				break;
			case StateMachineAction.NUM_NUMSEP:
				if (directionClass2 == DirectionClass.ArabicLetter || flag || flag2)
				{
					if ((flags & Flags.OverrideEuropeanNumberResolution) == 0)
					{
						characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
					}
				}
				else if (directionClass2 == DirectionClass.Left)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
				}
				else
				{
					directionClass2 = directionClass4;
				}
				ChangeType(characterClass, classIndex + num2, num4 - num2, characterClass[num4 + classIndex]);
				num2 = -1;
				directionClass = directionClass4;
				break;
			case StateMachineAction.EN_L:
				if (directionClass2 == DirectionClass.Left)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
				}
				if (num2 != -1)
				{
					num = num2;
					ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					num = (num2 = -1);
				}
				directionClass = directionClass4;
				break;
			case StateMachineAction.NUM_NUM:
				if ((flags & Flags.OverrideEuropeanNumberResolution) == 0 && (directionClass2 == DirectionClass.ArabicLetter || flag))
				{
					characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
					directionClass4 = DirectionClass.ArabicNumber;
				}
				else if (directionClass2 == DirectionClass.Left)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
				}
				if (num2 != -1)
				{
					num = num2;
					ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					num = (num2 = -1);
				}
				flag2 = ((directionClass4 == DirectionClass.ArabicNumber && directionClass2 == DirectionClass.Left) ? true : false);
				directionClass = directionClass4;
				break;
			case StateMachineAction.EN_AL:
				if ((flags & Flags.OverrideEuropeanNumberResolution) == 0)
				{
					characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
				}
				else
				{
					stateMachineState = StateMachineState.S_L;
				}
				if (num2 != -1)
				{
					num = num2;
					ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					num = (num2 = -1);
				}
				directionClass = characterClass[num4 + classIndex];
				break;
			case StateMachineAction.EN_ET:
				if (directionClass2 == DirectionClass.ArabicLetter || flag)
				{
					if ((flags & Flags.OverrideEuropeanNumberResolution) == 0)
					{
						characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
						directionClass4 = DirectionClass.ArabicNumber;
					}
					if (num == -1)
					{
						ResolveNeutrals(characterClass, classIndex + num2, num4 - num2, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					}
					else
					{
						ResolveNeutrals(characterClass, classIndex + num, num4 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					}
				}
				else if (directionClass2 == DirectionClass.Left)
				{
					characterClass[num4 + classIndex] = DirectionClass.Left;
					ChangeType(characterClass, classIndex + num2, num4 - num2, characterClass[num4 + classIndex]);
					if (num != -1)
					{
						ResolveNeutrals(characterClass, classIndex + num, num2 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, characterClass[num4 + classIndex], runLevel);
					}
					flag2 = false;
				}
				else
				{
					ChangeType(characterClass, classIndex + num2, num4 - num2, DirectionClass.EuropeanNumber);
					if (num != -1)
					{
						ResolveNeutrals(characterClass, classIndex + num, num2 - num, flag2 ? DirectionClass.ArabicNumber : directionClass2, directionClass4, runLevel);
					}
				}
				num = (num2 = -1);
				directionClass = directionClass4;
				break;
			case StateMachineAction.BN_ST:
				if (num2 == -1)
				{
					num2 = num4;
				}
				break;
			case StateMachineAction.NSM_ST:
				if (directionClass2 == DirectionClass.ArabicLetter)
				{
					switch (directionClass)
					{
					case DirectionClass.EuropeanNumber:
						if ((flags & Flags.OverrideEuropeanNumberResolution) == 0)
						{
							characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
						}
						else
						{
							characterClass[num4 + classIndex] = DirectionClass.EuropeanNumber;
						}
						break;
					default:
						characterClass[num4 + classIndex] = DirectionClass.Right;
						break;
					case DirectionClass.ArabicNumber:
						characterClass[num4 + classIndex] = DirectionClass.ArabicNumber;
						break;
					}
				}
				else
				{
					characterClass[num4 + classIndex] = ((flag2 || directionClass == DirectionClass.ArabicNumber) ? DirectionClass.ArabicNumber : ((directionClass == DirectionClass.EuropeanNumber && directionClass2 != 0) ? DirectionClass.EuropeanNumber : directionClass2));
				}
				if (num2 != -1)
				{
					ChangeType(characterClass, classIndex + num2, num4 - num2, characterClass[num4 + classIndex]);
					num2 = -1;
				}
				break;
			case StateMachineAction.NSM_ET:
				characterClass[num4 + classIndex] = directionClass;
				break;
			case StateMachineAction.N_ST:
				if (num2 != -1)
				{
					num = num2;
					num2 = -1;
				}
				else
				{
					num = num4;
				}
				directionClass = directionClass4;
				break;
			case StateMachineAction.N_ET:
				if (num == -1 && num2 != -1)
				{
					num = num2;
				}
				num2 = -1;
				directionClass = directionClass4;
				break;
			}
			stateMachineState = NextState[(int)stateMachineState, (uint)directionClass4];
			num3 = ((Math.Max(num, num2) == -1) ? (num4 + 1) : ((Math.Min(num, num2) == -1) ? Math.Max(num, num2) : Math.Min(num, num2)));
		}
		if (stateOut != null)
		{
			stateOut.LastStrongClass = directionClass2;
			stateOut.LastNumberClass = lastNumberClass;
			return num3;
		}
		if (num3 != num4)
		{
			ResolveNeutrals(characterClass, classIndex + num3, num4 - num3, flag2 ? DirectionClass.ArabicNumber : directionClass2, eor, runLevel);
		}
		return num4;
	}

	private static void ResolveImplictLevels(IList<DirectionClass> characterClass, CharacterBuffer charBuffer, int ichText, int runLength, IList<byte> levels, int index, byte paragraphEmbeddingLevel)
	{
		if (runLength == 0)
		{
			return;
		}
		int num = 0;
		for (num = runLength - 1; num >= 0; num--)
		{
			Invariant.Assert(CharProperty[3, (uint)characterClass[num + index]] == 1, "Cannot have unresolved classes during implict levels resolution");
			int num2 = charBuffer[ichText + index + num];
			int num3 = 1;
			if ((num2 & 0xFC00) == 56320 && num > 0 && (charBuffer[ichText + index + num - 1] & 0xFC00) == 55296)
			{
				num2 = (((charBuffer[ichText + index + num - 1] & 0x3FF) << 10) | (charBuffer[ichText + index + num] & 0x3FF)) + 65536;
				num3 = 2;
			}
			DirectionClass biDi = Classification.CharAttributeOf(Classification.GetUnicodeClass(num2)).BiDi;
			if (biDi == DirectionClass.ParagraphSeparator || biDi == DirectionClass.SegmentSeparator)
			{
				levels[num + index] = paragraphEmbeddingLevel;
			}
			else
			{
				levels[num + index] = (byte)(ImplictPush[Helper.IsOdd(levels[num + index]) ? 1u : 0u, (uint)characterClass[index + num]] + levels[num + index]);
			}
			if (num3 > 1)
			{
				levels[num + index - 1] = levels[num + index];
				num--;
			}
		}
	}

	public static bool Analyze(char[] chars, int cchText, int cchTextMaxHint, Flags flags, State state, out byte[] levels, out int cchResolved)
	{
		DirectionClass[] array = new DirectionClass[cchText];
		levels = new byte[cchText];
		return BidiAnalyzeInternal(new CharArrayCharacterBuffer(chars), 0, cchText, cchTextMaxHint, flags, state, levels, new PartialArray<DirectionClass>(array), out cchResolved);
	}

	internal static bool BidiAnalyzeInternal(CharacterBuffer charBuffer, int ichText, int cchText, int cchTextMaxHint, Flags flags, State state, IList<byte> levels, IList<DirectionClass> characterClass, out int cchResolved)
	{
		State state2 = null;
		State state3 = null;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		Invariant.Assert(levels != null && levels.Count >= cchText);
		Invariant.Assert(characterClass != null && characterClass.Count >= cchText);
		cchResolved = 0;
		if (charBuffer == null || cchText <= 0 || charBuffer.Count < cchText || (((flags & Flags.ContinueAnalysis) != 0 || (flags & Flags.IncompleteText) != 0) && state == null))
		{
			return false;
		}
		int wordCount;
		if ((flags & Flags.MaximumHint) != 0 && cchTextMaxHint > 0 && cchTextMaxHint < cchText)
		{
			if (cchTextMaxHint > 1 && (charBuffer[ichText + cchTextMaxHint - 2] & 0xFC00) == 55296)
			{
				cchTextMaxHint--;
			}
			int num4 = cchTextMaxHint - 1;
			int num5 = charBuffer[ichText + num4];
			wordCount = 1;
			if ((num5 & 0xFC00) == 55296)
			{
				num5 = DoubleWideChar.GetChar(charBuffer, ichText, cchText, num4, out wordCount);
			}
			DirectionClass biDi = Classification.CharAttributeOf(Classification.GetUnicodeClass(num5)).BiDi;
			num4 += wordCount;
			if (CharProperty[1, (uint)biDi] == 1)
			{
				for (; num4 < cchText && num4 - cchTextMaxHint < 20; num4 += wordCount)
				{
					num5 = charBuffer[ichText + num4];
					wordCount = 1;
					if ((num5 & 0xFC00) == 55296)
					{
						num5 = DoubleWideChar.GetChar(charBuffer, ichText, cchText, num4, out wordCount);
					}
					if (biDi != Classification.CharAttributeOf(Classification.GetUnicodeClass(num5)).BiDi)
					{
						break;
					}
				}
			}
			else
			{
				for (; num4 < cchText; num4 += wordCount)
				{
					num5 = charBuffer[ichText + num4];
					wordCount = 1;
					if ((num5 & 0xFC00) == 55296)
					{
						num5 = DoubleWideChar.GetChar(charBuffer, ichText, cchText, num4, out wordCount);
					}
					if (CharProperty[1, (uint)Classification.CharAttributeOf(Classification.GetUnicodeClass(num5)).BiDi] == 1)
					{
						break;
					}
				}
				num4++;
			}
			cchText = Math.Min(cchText, num4);
		}
		BidiStack bidiStack = new BidiStack();
		if ((flags & Flags.IncompleteText) != 0)
		{
			int unicodeScalar = charBuffer[ichText + cchText - 1];
			if (cchText > 1 && (charBuffer[ichText + cchText - 2] & 0xFC00) == 55296 && (charBuffer[ichText + cchText - 1] & 0xFC00) == 56320)
			{
				unicodeScalar = 65536 + (((charBuffer[ichText + cchText - 2] & 0x3FF) << 10) | (charBuffer[ichText + cchText - 1] & 0x3FF));
			}
			if (DirectionClass.ParagraphSeparator != Classification.CharAttributeOf(Classification.GetUnicodeClass(unicodeScalar)).BiDi)
			{
				state3 = state;
			}
		}
		if ((flags & Flags.ContinueAnalysis) != 0)
		{
			int unicodeScalar = charBuffer[ichText];
			if (cchText > 1 && (charBuffer[ichText] & 0xFC00) == 55296 && (charBuffer[ichText + 1] & 0xFC00) == 56320)
			{
				unicodeScalar = 65536 + (((charBuffer[ichText] & 0x3FF) << 10) | (charBuffer[ichText + 1] & 0x3FF));
			}
			DirectionClass biDi = Classification.CharAttributeOf(Classification.GetUnicodeClass(unicodeScalar)).BiDi;
			state2 = state;
			switch (biDi)
			{
			case DirectionClass.Left:
			case DirectionClass.Right:
			case DirectionClass.ArabicNumber:
			case DirectionClass.ArabicLetter:
				state2.LastNumberClass = biDi;
				state2.LastStrongClass = biDi;
				break;
			case DirectionClass.EuropeanNumber:
				state2.LastNumberClass = biDi;
				break;
			}
		}
		byte b;
		ushort num6;
		ulong x;
		OverrideClass overrideClass;
		if (state2 != null)
		{
			if (!bidiStack.Init(state2.LevelStack))
			{
				cchResolved = 0;
				return false;
			}
			b = bidiStack.GetCurrentLevel();
			num6 = state2.Overflow;
			x = state2.OverrideLevels;
			overrideClass = (Helper.IsBitSet(x, b) ? ((!Helper.IsOdd(b)) ? OverrideClass.OverrideClassLeft : OverrideClass.OverrideClassRight) : OverrideClass.OverrideClassNeutral);
		}
		else
		{
			b = 0;
			if ((flags & Flags.FirstStrongAsBaseDirection) != 0)
			{
				DirectionClass strongClass = DirectionClass.ClassInvalid;
				if (GetFirstStrongCharacter(charBuffer, ichText, cchText, ref strongClass) && strongClass != 0)
				{
					b = 1;
				}
			}
			else if ((flags & Flags.DirectionRightToLeft) != 0)
			{
				b = 1;
			}
			bidiStack.Init((ulong)b + 1uL);
			num6 = 0;
			x = 0uL;
			overrideClass = OverrideClass.OverrideClassNeutral;
		}
		byte stackBottom = bidiStack.GetStackBottom();
		int i = -1;
		byte b2;
		byte b3;
		byte b5;
		byte b4;
		DirectionClass directionClass;
		if (Helper.IsOdd(b))
		{
			b2 = b;
			b3 = (byte)(b + 1);
			b5 = (b4 = 3);
			directionClass = state2?.LastStrongClass ?? DirectionClass.Right;
		}
		else
		{
			b3 = b;
			b2 = (byte)(b + 1);
			b5 = (b4 = 2);
			directionClass = state2?.LastStrongClass ?? DirectionClass.Left;
		}
		if (state2 != null && (FastPathClass[(uint)directionClass] & 2) == 2)
		{
			b5 = FastPathClass[(uint)directionClass];
		}
		DirectionClass directionClass2 = DirectionClass.GenericNeutral;
		int j = 0;
		wordCount = 1;
		for (int k = j; k < cchText; k += wordCount)
		{
			int num7 = charBuffer[ichText + k];
			if ((num7 & 0xFC00) == 55296)
			{
				num7 = DoubleWideChar.GetChar(charBuffer, ichText, cchText, j, out wordCount);
			}
			if (num7 != CharHidden)
			{
				directionClass2 = Classification.CharAttributeOf(Classification.GetUnicodeClass(num7)).BiDi;
				if (directionClass2 == DirectionClass.EuropeanNumber && (flags & Flags.OverrideEuropeanNumberResolution) != 0)
				{
					directionClass2 = characterClass[k];
				}
				break;
			}
		}
		for (; j < cchText; j += wordCount)
		{
			wordCount = 1;
			int unicodeScalar = DoubleWideChar.GetChar(charBuffer, ichText, cchText, j, out wordCount);
			DirectionClass biDi = Classification.CharAttributeOf(Classification.GetUnicodeClass(unicodeScalar)).BiDi;
			if (unicodeScalar == CharHidden)
			{
				biDi = directionClass2;
			}
			if (FastPathClass[(uint)biDi] == 0)
			{
				break;
			}
			characterClass[j] = biDi;
			directionClass2 = biDi;
			if (FastPathClass[(uint)biDi] == 1)
			{
				if (biDi != DirectionClass.EuropeanSeparator && biDi != DirectionClass.CommonSeparator)
				{
					characterClass[j] = DirectionClass.GenericNeutral;
				}
				if (i == -1)
				{
					i = j;
				}
				continue;
			}
			if (i != -1)
			{
				byte value = ((b5 == FastPathClass[(uint)biDi]) ? ((b5 == 2) ? b3 : b2) : b);
				for (; i < j; i++)
				{
					levels[i] = value;
				}
				i = -1;
			}
			b5 = FastPathClass[(uint)biDi];
			levels[j] = ((b5 == 2) ? b3 : b2);
			if (wordCount == 2)
			{
				levels[j + 1] = levels[j];
			}
			directionClass = biDi;
		}
		if (j < cchText)
		{
			for (int l = 0; l < j; l++)
			{
				levels[l] = b;
			}
			byte value2 = b;
			int[] array = new int[cchText];
			for (; j < cchText; j += wordCount)
			{
				int num8 = charBuffer[ichText + j];
				wordCount = 1;
				if ((num8 & 0xFC00) == 55296)
				{
					num8 = DoubleWideChar.GetChar(charBuffer, ichText, cchText, j, out wordCount);
				}
				DirectionClass directionClass3 = Classification.CharAttributeOf(Classification.GetUnicodeClass(num8)).BiDi;
				levels[j] = bidiStack.GetCurrentLevel();
				if (num8 == CharHidden)
				{
					directionClass3 = directionClass2;
				}
				switch (directionClass3)
				{
				case DirectionClass.ParagraphSeparator:
					levels[j] = byte.MaxValue;
					array[num3] = j;
					if (j != cchText - 1)
					{
						num3++;
					}
					bidiStack.Init((ulong)b + 1uL);
					x = 0uL;
					overrideClass = OverrideClass.OverrideClassNeutral;
					num6 = 0;
					num2 = 0;
					goto case DirectionClass.SegmentSeparator;
				case DirectionClass.SegmentSeparator:
				case DirectionClass.WhiteSpace:
				case DirectionClass.OtherNeutral:
					characterClass[j] = DirectionClass.GenericNeutral;
					if (j > 0 && characterClass[j - 1] == DirectionClass.BoundaryNeutral && levels[j - 1] < levels[j] && levels[j] != byte.MaxValue)
					{
						levels[j - 1] = levels[j];
					}
					num2 = 0;
					break;
				case DirectionClass.LeftToRightEmbedding:
				case DirectionClass.RightToLeftEmbedding:
					characterClass[j] = DirectionClass.BoundaryNeutral;
					if ((flags & Flags.IgnoreDirectionalControls) != 0)
					{
						break;
					}
					if (!bidiStack.Push(directionClass3 == DirectionClass.LeftToRightEmbedding))
					{
						num6++;
					}
					else
					{
						array[num3] = j;
						if (j != cchText - 1)
						{
							num3++;
						}
						num2++;
					}
					overrideClass = OverrideClass.OverrideClassNeutral;
					levels[j] = value2;
					break;
				case DirectionClass.LeftToRightOverride:
				case DirectionClass.RightToLeftOverride:
					characterClass[j] = DirectionClass.BoundaryNeutral;
					if ((flags & Flags.IgnoreDirectionalControls) != 0)
					{
						break;
					}
					if (!bidiStack.Push(directionClass3 == DirectionClass.LeftToRightOverride))
					{
						num6++;
					}
					else
					{
						Helper.ResetBit(ref x, bidiStack.GetCurrentLevel());
						overrideClass = ((directionClass3 == DirectionClass.LeftToRightOverride) ? OverrideClass.OverrideClassLeft : OverrideClass.OverrideClassRight);
						array[num3] = j;
						if (j != cchText - 1)
						{
							num3++;
						}
						num2++;
					}
					levels[j] = value2;
					break;
				case DirectionClass.PopDirectionalFormat:
					characterClass[j] = DirectionClass.BoundaryNeutral;
					if ((flags & Flags.IgnoreDirectionalControls) != 0)
					{
						break;
					}
					if (num6 != 0)
					{
						num6--;
					}
					else if (bidiStack.Pop())
					{
						int currentLevel = bidiStack.GetCurrentLevel();
						overrideClass = (Helper.IsBitSet(x, currentLevel) ? ((!Helper.IsOdd(currentLevel)) ? OverrideClass.OverrideClassLeft : OverrideClass.OverrideClassRight) : OverrideClass.OverrideClassNeutral);
						if (num2 > 0)
						{
							num3--;
							num2--;
						}
						else
						{
							array[num3] = j;
							if (j != cchText - 1)
							{
								num3++;
							}
						}
					}
					levels[j] = value2;
					break;
				default:
					num2 = 0;
					if (directionClass3 == DirectionClass.EuropeanNumber && (flags & Flags.OverrideEuropeanNumberResolution) != 0)
					{
						Invariant.Assert(characterClass[j] == DirectionClass.ArabicNumber || characterClass[j] == DirectionClass.EuropeanNumber);
					}
					else
					{
						characterClass[j] = directionClass3;
					}
					if (overrideClass != 0)
					{
						characterClass[j] = ((overrideClass != OverrideClass.OverrideClassLeft) ? DirectionClass.Right : DirectionClass.Left);
					}
					if (j > 0 && characterClass[j - 1] == DirectionClass.BoundaryNeutral && levels[j - 1] < levels[j])
					{
						levels[j - 1] = levels[j];
					}
					break;
				}
				value2 = levels[j];
				if (wordCount > 1)
				{
					levels[j + 1] = levels[j];
					characterClass[j + 1] = characterClass[j];
				}
				directionClass2 = characterClass[j];
			}
			num3++;
			if (state3 != null)
			{
				state3.LevelStack = bidiStack.GetData();
				state3.OverrideLevels = x;
				state3.Overflow = num6;
			}
			byte val = b;
			bool flag = false;
			for (j = 0; j < num3; j++)
			{
				bool flag2 = levels[array[j]] == byte.MaxValue;
				if (flag2)
				{
					levels[array[j]] = b;
				}
				int num9 = ((j != 0) ? (array[j - 1] + 1) : 0);
				int num10 = ((j != num3 - 1 && flag2) ? 1 : 0);
				int num11 = ((j == num3 - 1) ? (cchText - num9 - num10) : (array[j] - num9 + 1 - num10));
				bool flag3 = num3 - 1 == j && (flags & Flags.IncompleteText) != 0 && state3 != null;
				bool flag4 = j == 0 && state2 != null;
				DirectionClass sor = ((!(j == 0 || flag)) ? (Helper.IsOdd(Math.Max(val, levels[num9])) ? DirectionClass.Right : DirectionClass.Left) : (Helper.IsOdd(Math.Max(b, levels[num9])) ? DirectionClass.Right : DirectionClass.Left));
				val = levels[num9];
				DirectionClass eor;
				if (num3 - 1 == j || flag2)
				{
					eor = (Helper.IsOdd(Math.Max(levels[num9], b)) ? DirectionClass.Right : DirectionClass.Left);
				}
				else
				{
					int m;
					for (m = j + 1; m < num3 - 1 && array[m] - array[m - 1] == 1 && characterClass[array[m]] == DirectionClass.BoundaryNeutral; m++)
					{
					}
					eor = (Helper.IsOdd(Math.Max(levels[num9], levels[array[m - 1] + 1])) ? DirectionClass.Right : DirectionClass.Left);
				}
				int num12 = ResolveNeutralAndWeak(characterClass, num9, num11, sor, eor, levels[num9], flag4 ? state2 : null, flag3 ? state3 : null, j == 0 && state2 == null && (flags & Flags.PreviousStrongIsArabic) != 0, flags);
				if (flag3)
				{
					num = num11 - num12;
				}
				ResolveImplictLevels(characterClass, charBuffer, ichText, num11 - num, levels, num9, stackBottom);
				flag = flag2;
			}
			cchResolved = cchText - num;
			if ((flags & Flags.IncompleteText) != 0 && state3 == null)
			{
				state.OverrideLevels = 0uL;
				state.Overflow = 0;
				if ((stackBottom & 1) != 0)
				{
					state.LastStrongClass = DirectionClass.Right;
					state.LastNumberClass = DirectionClass.Right;
					state.LevelStack = 2uL;
				}
				else
				{
					state.LastStrongClass = DirectionClass.Left;
					state.LastNumberClass = DirectionClass.Left;
					state.LevelStack = 1uL;
				}
			}
			return true;
		}
		cchResolved = cchText;
		if (state != null)
		{
			state.LastStrongClass = directionClass;
		}
		if (i != -1)
		{
			if ((flags & Flags.IncompleteText) == 0)
			{
				byte value = ((b5 == b4) ? ((b5 == 2) ? b3 : b2) : b);
				for (; i < cchText; i++)
				{
					levels[i] = value;
				}
			}
			else
			{
				cchResolved = i;
			}
		}
		return true;
	}
}
