using System.Reflection.Emit;

namespace HarmonyLib;

public static class Code
{
	public class Operand_ : CodeMatch
	{
		public Operand_ this[object operand = null, string name = null] => (Operand_)Set(operand, name);

		public Operand_()
			: base(null, null, null)
		{
		}
	}

	public class Nop_ : CodeMatch
	{
		public Nop_ this[object operand = null, string name = null] => (Nop_)Set(OpCodes.Nop, operand, name);

		public Nop_()
			: base(null, null, null)
		{
		}
	}

	public class Break_ : CodeMatch
	{
		public Break_ this[object operand = null, string name = null] => (Break_)Set(OpCodes.Break, operand, name);

		public Break_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_0_ : CodeMatch
	{
		public Ldarg_0_ this[object operand = null, string name = null] => (Ldarg_0_)Set(OpCodes.Ldarg_0, operand, name);

		public Ldarg_0_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_1_ : CodeMatch
	{
		public Ldarg_1_ this[object operand = null, string name = null] => (Ldarg_1_)Set(OpCodes.Ldarg_1, operand, name);

		public Ldarg_1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_2_ : CodeMatch
	{
		public Ldarg_2_ this[object operand = null, string name = null] => (Ldarg_2_)Set(OpCodes.Ldarg_2, operand, name);

		public Ldarg_2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_3_ : CodeMatch
	{
		public Ldarg_3_ this[object operand = null, string name = null] => (Ldarg_3_)Set(OpCodes.Ldarg_3, operand, name);

		public Ldarg_3_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_0_ : CodeMatch
	{
		public Ldloc_0_ this[object operand = null, string name = null] => (Ldloc_0_)Set(OpCodes.Ldloc_0, operand, name);

		public Ldloc_0_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_1_ : CodeMatch
	{
		public Ldloc_1_ this[object operand = null, string name = null] => (Ldloc_1_)Set(OpCodes.Ldloc_1, operand, name);

		public Ldloc_1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_2_ : CodeMatch
	{
		public Ldloc_2_ this[object operand = null, string name = null] => (Ldloc_2_)Set(OpCodes.Ldloc_2, operand, name);

		public Ldloc_2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_3_ : CodeMatch
	{
		public Ldloc_3_ this[object operand = null, string name = null] => (Ldloc_3_)Set(OpCodes.Ldloc_3, operand, name);

		public Ldloc_3_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_0_ : CodeMatch
	{
		public Stloc_0_ this[object operand = null, string name = null] => (Stloc_0_)Set(OpCodes.Stloc_0, operand, name);

		public Stloc_0_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_1_ : CodeMatch
	{
		public Stloc_1_ this[object operand = null, string name = null] => (Stloc_1_)Set(OpCodes.Stloc_1, operand, name);

		public Stloc_1_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_2_ : CodeMatch
	{
		public Stloc_2_ this[object operand = null, string name = null] => (Stloc_2_)Set(OpCodes.Stloc_2, operand, name);

		public Stloc_2_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_3_ : CodeMatch
	{
		public Stloc_3_ this[object operand = null, string name = null] => (Stloc_3_)Set(OpCodes.Stloc_3, operand, name);

		public Stloc_3_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_S_ : CodeMatch
	{
		public Ldarg_S_ this[object operand = null, string name = null] => (Ldarg_S_)Set(OpCodes.Ldarg_S, operand, name);

		public Ldarg_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarga_S_ : CodeMatch
	{
		public Ldarga_S_ this[object operand = null, string name = null] => (Ldarga_S_)Set(OpCodes.Ldarga_S, operand, name);

		public Ldarga_S_()
			: base(null, null, null)
		{
		}
	}

	public class Starg_S_ : CodeMatch
	{
		public Starg_S_ this[object operand = null, string name = null] => (Starg_S_)Set(OpCodes.Starg_S, operand, name);

		public Starg_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_S_ : CodeMatch
	{
		public Ldloc_S_ this[object operand = null, string name = null] => (Ldloc_S_)Set(OpCodes.Ldloc_S, operand, name);

		public Ldloc_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloca_S_ : CodeMatch
	{
		public Ldloca_S_ this[object operand = null, string name = null] => (Ldloca_S_)Set(OpCodes.Ldloca_S, operand, name);

		public Ldloca_S_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_S_ : CodeMatch
	{
		public Stloc_S_ this[object operand = null, string name = null] => (Stloc_S_)Set(OpCodes.Stloc_S, operand, name);

		public Stloc_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ldnull_ : CodeMatch
	{
		public Ldnull_ this[object operand = null, string name = null] => (Ldnull_)Set(OpCodes.Ldnull, operand, name);

		public Ldnull_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_M1_ : CodeMatch
	{
		public Ldc_I4_M1_ this[object operand = null, string name = null] => (Ldc_I4_M1_)Set(OpCodes.Ldc_I4_M1, operand, name);

		public Ldc_I4_M1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_0_ : CodeMatch
	{
		public Ldc_I4_0_ this[object operand = null, string name = null] => (Ldc_I4_0_)Set(OpCodes.Ldc_I4_0, operand, name);

		public Ldc_I4_0_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_1_ : CodeMatch
	{
		public Ldc_I4_1_ this[object operand = null, string name = null] => (Ldc_I4_1_)Set(OpCodes.Ldc_I4_1, operand, name);

		public Ldc_I4_1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_2_ : CodeMatch
	{
		public Ldc_I4_2_ this[object operand = null, string name = null] => (Ldc_I4_2_)Set(OpCodes.Ldc_I4_2, operand, name);

		public Ldc_I4_2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_3_ : CodeMatch
	{
		public Ldc_I4_3_ this[object operand = null, string name = null] => (Ldc_I4_3_)Set(OpCodes.Ldc_I4_3, operand, name);

		public Ldc_I4_3_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_4_ : CodeMatch
	{
		public Ldc_I4_4_ this[object operand = null, string name = null] => (Ldc_I4_4_)Set(OpCodes.Ldc_I4_4, operand, name);

		public Ldc_I4_4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_5_ : CodeMatch
	{
		public Ldc_I4_5_ this[object operand = null, string name = null] => (Ldc_I4_5_)Set(OpCodes.Ldc_I4_5, operand, name);

		public Ldc_I4_5_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_6_ : CodeMatch
	{
		public Ldc_I4_6_ this[object operand = null, string name = null] => (Ldc_I4_6_)Set(OpCodes.Ldc_I4_6, operand, name);

		public Ldc_I4_6_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_7_ : CodeMatch
	{
		public Ldc_I4_7_ this[object operand = null, string name = null] => (Ldc_I4_7_)Set(OpCodes.Ldc_I4_7, operand, name);

		public Ldc_I4_7_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_8_ : CodeMatch
	{
		public Ldc_I4_8_ this[object operand = null, string name = null] => (Ldc_I4_8_)Set(OpCodes.Ldc_I4_8, operand, name);

		public Ldc_I4_8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_S_ : CodeMatch
	{
		public Ldc_I4_S_ this[object operand = null, string name = null] => (Ldc_I4_S_)Set(OpCodes.Ldc_I4_S, operand, name);

		public Ldc_I4_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I4_ : CodeMatch
	{
		public Ldc_I4_ this[object operand = null, string name = null] => (Ldc_I4_)Set(OpCodes.Ldc_I4, operand, name);

		public Ldc_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_I8_ : CodeMatch
	{
		public Ldc_I8_ this[object operand = null, string name = null] => (Ldc_I8_)Set(OpCodes.Ldc_I8, operand, name);

		public Ldc_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_R4_ : CodeMatch
	{
		public Ldc_R4_ this[object operand = null, string name = null] => (Ldc_R4_)Set(OpCodes.Ldc_R4, operand, name);

		public Ldc_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldc_R8_ : CodeMatch
	{
		public Ldc_R8_ this[object operand = null, string name = null] => (Ldc_R8_)Set(OpCodes.Ldc_R8, operand, name);

		public Ldc_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Dup_ : CodeMatch
	{
		public Dup_ this[object operand = null, string name = null] => (Dup_)Set(OpCodes.Dup, operand, name);

		public Dup_()
			: base(null, null, null)
		{
		}
	}

	public class Pop_ : CodeMatch
	{
		public Pop_ this[object operand = null, string name = null] => (Pop_)Set(OpCodes.Pop, operand, name);

		public Pop_()
			: base(null, null, null)
		{
		}
	}

	public class Jmp_ : CodeMatch
	{
		public Jmp_ this[object operand = null, string name = null] => (Jmp_)Set(OpCodes.Jmp, operand, name);

		public Jmp_()
			: base(null, null, null)
		{
		}
	}

	public class Call_ : CodeMatch
	{
		public Call_ this[object operand = null, string name = null] => (Call_)Set(OpCodes.Call, operand, name);

		public Call_()
			: base(null, null, null)
		{
		}
	}

	public class Calli_ : CodeMatch
	{
		public Calli_ this[object operand = null, string name = null] => (Calli_)Set(OpCodes.Calli, operand, name);

		public Calli_()
			: base(null, null, null)
		{
		}
	}

	public class Ret_ : CodeMatch
	{
		public Ret_ this[object operand = null, string name = null] => (Ret_)Set(OpCodes.Ret, operand, name);

		public Ret_()
			: base(null, null, null)
		{
		}
	}

	public class Br_S_ : CodeMatch
	{
		public Br_S_ this[object operand = null, string name = null] => (Br_S_)Set(OpCodes.Br_S, operand, name);

		public Br_S_()
			: base(null, null, null)
		{
		}
	}

	public class Brfalse_S_ : CodeMatch
	{
		public Brfalse_S_ this[object operand = null, string name = null] => (Brfalse_S_)Set(OpCodes.Brfalse_S, operand, name);

		public Brfalse_S_()
			: base(null, null, null)
		{
		}
	}

	public class Brtrue_S_ : CodeMatch
	{
		public Brtrue_S_ this[object operand = null, string name = null] => (Brtrue_S_)Set(OpCodes.Brtrue_S, operand, name);

		public Brtrue_S_()
			: base(null, null, null)
		{
		}
	}

	public class Beq_S_ : CodeMatch
	{
		public Beq_S_ this[object operand = null, string name = null] => (Beq_S_)Set(OpCodes.Beq_S, operand, name);

		public Beq_S_()
			: base(null, null, null)
		{
		}
	}

	public class Bge_S_ : CodeMatch
	{
		public Bge_S_ this[object operand = null, string name = null] => (Bge_S_)Set(OpCodes.Bge_S, operand, name);

		public Bge_S_()
			: base(null, null, null)
		{
		}
	}

	public class Bgt_S_ : CodeMatch
	{
		public Bgt_S_ this[object operand = null, string name = null] => (Bgt_S_)Set(OpCodes.Bgt_S, operand, name);

		public Bgt_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ble_S_ : CodeMatch
	{
		public Ble_S_ this[object operand = null, string name = null] => (Ble_S_)Set(OpCodes.Ble_S, operand, name);

		public Ble_S_()
			: base(null, null, null)
		{
		}
	}

	public class Blt_S_ : CodeMatch
	{
		public Blt_S_ this[object operand = null, string name = null] => (Blt_S_)Set(OpCodes.Blt_S, operand, name);

		public Blt_S_()
			: base(null, null, null)
		{
		}
	}

	public class Bne_Un_S_ : CodeMatch
	{
		public Bne_Un_S_ this[object operand = null, string name = null] => (Bne_Un_S_)Set(OpCodes.Bne_Un_S, operand, name);

		public Bne_Un_S_()
			: base(null, null, null)
		{
		}
	}

	public class Bge_Un_S_ : CodeMatch
	{
		public Bge_Un_S_ this[object operand = null, string name = null] => (Bge_Un_S_)Set(OpCodes.Bge_Un_S, operand, name);

		public Bge_Un_S_()
			: base(null, null, null)
		{
		}
	}

	public class Bgt_Un_S_ : CodeMatch
	{
		public Bgt_Un_S_ this[object operand = null, string name = null] => (Bgt_Un_S_)Set(OpCodes.Bgt_Un_S, operand, name);

		public Bgt_Un_S_()
			: base(null, null, null)
		{
		}
	}

	public class Ble_Un_S_ : CodeMatch
	{
		public Ble_Un_S_ this[object operand = null, string name = null] => (Ble_Un_S_)Set(OpCodes.Ble_Un_S, operand, name);

		public Ble_Un_S_()
			: base(null, null, null)
		{
		}
	}

	public class Blt_Un_S_ : CodeMatch
	{
		public Blt_Un_S_ this[object operand = null, string name = null] => (Blt_Un_S_)Set(OpCodes.Blt_Un_S, operand, name);

		public Blt_Un_S_()
			: base(null, null, null)
		{
		}
	}

	public class Br_ : CodeMatch
	{
		public Br_ this[object operand = null, string name = null] => (Br_)Set(OpCodes.Br, operand, name);

		public Br_()
			: base(null, null, null)
		{
		}
	}

	public class Brfalse_ : CodeMatch
	{
		public Brfalse_ this[object operand = null, string name = null] => (Brfalse_)Set(OpCodes.Brfalse, operand, name);

		public Brfalse_()
			: base(null, null, null)
		{
		}
	}

	public class Brtrue_ : CodeMatch
	{
		public Brtrue_ this[object operand = null, string name = null] => (Brtrue_)Set(OpCodes.Brtrue, operand, name);

		public Brtrue_()
			: base(null, null, null)
		{
		}
	}

	public class Beq_ : CodeMatch
	{
		public Beq_ this[object operand = null, string name = null] => (Beq_)Set(OpCodes.Beq, operand, name);

		public Beq_()
			: base(null, null, null)
		{
		}
	}

	public class Bge_ : CodeMatch
	{
		public Bge_ this[object operand = null, string name = null] => (Bge_)Set(OpCodes.Bge, operand, name);

		public Bge_()
			: base(null, null, null)
		{
		}
	}

	public class Bgt_ : CodeMatch
	{
		public Bgt_ this[object operand = null, string name = null] => (Bgt_)Set(OpCodes.Bgt, operand, name);

		public Bgt_()
			: base(null, null, null)
		{
		}
	}

	public class Ble_ : CodeMatch
	{
		public Ble_ this[object operand = null, string name = null] => (Ble_)Set(OpCodes.Ble, operand, name);

		public Ble_()
			: base(null, null, null)
		{
		}
	}

	public class Blt_ : CodeMatch
	{
		public Blt_ this[object operand = null, string name = null] => (Blt_)Set(OpCodes.Blt, operand, name);

		public Blt_()
			: base(null, null, null)
		{
		}
	}

	public class Bne_Un_ : CodeMatch
	{
		public Bne_Un_ this[object operand = null, string name = null] => (Bne_Un_)Set(OpCodes.Bne_Un, operand, name);

		public Bne_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Bge_Un_ : CodeMatch
	{
		public Bge_Un_ this[object operand = null, string name = null] => (Bge_Un_)Set(OpCodes.Bge_Un, operand, name);

		public Bge_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Bgt_Un_ : CodeMatch
	{
		public Bgt_Un_ this[object operand = null, string name = null] => (Bgt_Un_)Set(OpCodes.Bgt_Un, operand, name);

		public Bgt_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Ble_Un_ : CodeMatch
	{
		public Ble_Un_ this[object operand = null, string name = null] => (Ble_Un_)Set(OpCodes.Ble_Un, operand, name);

		public Ble_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Blt_Un_ : CodeMatch
	{
		public Blt_Un_ this[object operand = null, string name = null] => (Blt_Un_)Set(OpCodes.Blt_Un, operand, name);

		public Blt_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Switch_ : CodeMatch
	{
		public Switch_ this[object operand = null, string name = null] => (Switch_)Set(OpCodes.Switch, operand, name);

		public Switch_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_I1_ : CodeMatch
	{
		public Ldind_I1_ this[object operand = null, string name = null] => (Ldind_I1_)Set(OpCodes.Ldind_I1, operand, name);

		public Ldind_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_U1_ : CodeMatch
	{
		public Ldind_U1_ this[object operand = null, string name = null] => (Ldind_U1_)Set(OpCodes.Ldind_U1, operand, name);

		public Ldind_U1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_I2_ : CodeMatch
	{
		public Ldind_I2_ this[object operand = null, string name = null] => (Ldind_I2_)Set(OpCodes.Ldind_I2, operand, name);

		public Ldind_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_U2_ : CodeMatch
	{
		public Ldind_U2_ this[object operand = null, string name = null] => (Ldind_U2_)Set(OpCodes.Ldind_U2, operand, name);

		public Ldind_U2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_I4_ : CodeMatch
	{
		public Ldind_I4_ this[object operand = null, string name = null] => (Ldind_I4_)Set(OpCodes.Ldind_I4, operand, name);

		public Ldind_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_U4_ : CodeMatch
	{
		public Ldind_U4_ this[object operand = null, string name = null] => (Ldind_U4_)Set(OpCodes.Ldind_U4, operand, name);

		public Ldind_U4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_I8_ : CodeMatch
	{
		public Ldind_I8_ this[object operand = null, string name = null] => (Ldind_I8_)Set(OpCodes.Ldind_I8, operand, name);

		public Ldind_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_I_ : CodeMatch
	{
		public Ldind_I_ this[object operand = null, string name = null] => (Ldind_I_)Set(OpCodes.Ldind_I, operand, name);

		public Ldind_I_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_R4_ : CodeMatch
	{
		public Ldind_R4_ this[object operand = null, string name = null] => (Ldind_R4_)Set(OpCodes.Ldind_R4, operand, name);

		public Ldind_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_R8_ : CodeMatch
	{
		public Ldind_R8_ this[object operand = null, string name = null] => (Ldind_R8_)Set(OpCodes.Ldind_R8, operand, name);

		public Ldind_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldind_Ref_ : CodeMatch
	{
		public Ldind_Ref_ this[object operand = null, string name = null] => (Ldind_Ref_)Set(OpCodes.Ldind_Ref, operand, name);

		public Ldind_Ref_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_Ref_ : CodeMatch
	{
		public Stind_Ref_ this[object operand = null, string name = null] => (Stind_Ref_)Set(OpCodes.Stind_Ref, operand, name);

		public Stind_Ref_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_I1_ : CodeMatch
	{
		public Stind_I1_ this[object operand = null, string name = null] => (Stind_I1_)Set(OpCodes.Stind_I1, operand, name);

		public Stind_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_I2_ : CodeMatch
	{
		public Stind_I2_ this[object operand = null, string name = null] => (Stind_I2_)Set(OpCodes.Stind_I2, operand, name);

		public Stind_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_I4_ : CodeMatch
	{
		public Stind_I4_ this[object operand = null, string name = null] => (Stind_I4_)Set(OpCodes.Stind_I4, operand, name);

		public Stind_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_I8_ : CodeMatch
	{
		public Stind_I8_ this[object operand = null, string name = null] => (Stind_I8_)Set(OpCodes.Stind_I8, operand, name);

		public Stind_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_R4_ : CodeMatch
	{
		public Stind_R4_ this[object operand = null, string name = null] => (Stind_R4_)Set(OpCodes.Stind_R4, operand, name);

		public Stind_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_R8_ : CodeMatch
	{
		public Stind_R8_ this[object operand = null, string name = null] => (Stind_R8_)Set(OpCodes.Stind_R8, operand, name);

		public Stind_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Add_ : CodeMatch
	{
		public Add_ this[object operand = null, string name = null] => (Add_)Set(OpCodes.Add, operand, name);

		public Add_()
			: base(null, null, null)
		{
		}
	}

	public class Sub_ : CodeMatch
	{
		public Sub_ this[object operand = null, string name = null] => (Sub_)Set(OpCodes.Sub, operand, name);

		public Sub_()
			: base(null, null, null)
		{
		}
	}

	public class Mul_ : CodeMatch
	{
		public Mul_ this[object operand = null, string name = null] => (Mul_)Set(OpCodes.Mul, operand, name);

		public Mul_()
			: base(null, null, null)
		{
		}
	}

	public class Div_ : CodeMatch
	{
		public Div_ this[object operand = null, string name = null] => (Div_)Set(OpCodes.Div, operand, name);

		public Div_()
			: base(null, null, null)
		{
		}
	}

	public class Div_Un_ : CodeMatch
	{
		public Div_Un_ this[object operand = null, string name = null] => (Div_Un_)Set(OpCodes.Div_Un, operand, name);

		public Div_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Rem_ : CodeMatch
	{
		public Rem_ this[object operand = null, string name = null] => (Rem_)Set(OpCodes.Rem, operand, name);

		public Rem_()
			: base(null, null, null)
		{
		}
	}

	public class Rem_Un_ : CodeMatch
	{
		public Rem_Un_ this[object operand = null, string name = null] => (Rem_Un_)Set(OpCodes.Rem_Un, operand, name);

		public Rem_Un_()
			: base(null, null, null)
		{
		}
	}

	public class And_ : CodeMatch
	{
		public And_ this[object operand = null, string name = null] => (And_)Set(OpCodes.And, operand, name);

		public And_()
			: base(null, null, null)
		{
		}
	}

	public class Or_ : CodeMatch
	{
		public Or_ this[object operand = null, string name = null] => (Or_)Set(OpCodes.Or, operand, name);

		public Or_()
			: base(null, null, null)
		{
		}
	}

	public class Xor_ : CodeMatch
	{
		public Xor_ this[object operand = null, string name = null] => (Xor_)Set(OpCodes.Xor, operand, name);

		public Xor_()
			: base(null, null, null)
		{
		}
	}

	public class Shl_ : CodeMatch
	{
		public Shl_ this[object operand = null, string name = null] => (Shl_)Set(OpCodes.Shl, operand, name);

		public Shl_()
			: base(null, null, null)
		{
		}
	}

	public class Shr_ : CodeMatch
	{
		public Shr_ this[object operand = null, string name = null] => (Shr_)Set(OpCodes.Shr, operand, name);

		public Shr_()
			: base(null, null, null)
		{
		}
	}

	public class Shr_Un_ : CodeMatch
	{
		public Shr_Un_ this[object operand = null, string name = null] => (Shr_Un_)Set(OpCodes.Shr_Un, operand, name);

		public Shr_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Neg_ : CodeMatch
	{
		public Neg_ this[object operand = null, string name = null] => (Neg_)Set(OpCodes.Neg, operand, name);

		public Neg_()
			: base(null, null, null)
		{
		}
	}

	public class Not_ : CodeMatch
	{
		public Not_ this[object operand = null, string name = null] => (Not_)Set(OpCodes.Not, operand, name);

		public Not_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_I1_ : CodeMatch
	{
		public Conv_I1_ this[object operand = null, string name = null] => (Conv_I1_)Set(OpCodes.Conv_I1, operand, name);

		public Conv_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_I2_ : CodeMatch
	{
		public Conv_I2_ this[object operand = null, string name = null] => (Conv_I2_)Set(OpCodes.Conv_I2, operand, name);

		public Conv_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_I4_ : CodeMatch
	{
		public Conv_I4_ this[object operand = null, string name = null] => (Conv_I4_)Set(OpCodes.Conv_I4, operand, name);

		public Conv_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_I8_ : CodeMatch
	{
		public Conv_I8_ this[object operand = null, string name = null] => (Conv_I8_)Set(OpCodes.Conv_I8, operand, name);

		public Conv_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_R4_ : CodeMatch
	{
		public Conv_R4_ this[object operand = null, string name = null] => (Conv_R4_)Set(OpCodes.Conv_R4, operand, name);

		public Conv_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_R8_ : CodeMatch
	{
		public Conv_R8_ this[object operand = null, string name = null] => (Conv_R8_)Set(OpCodes.Conv_R8, operand, name);

		public Conv_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_U4_ : CodeMatch
	{
		public Conv_U4_ this[object operand = null, string name = null] => (Conv_U4_)Set(OpCodes.Conv_U4, operand, name);

		public Conv_U4_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_U8_ : CodeMatch
	{
		public Conv_U8_ this[object operand = null, string name = null] => (Conv_U8_)Set(OpCodes.Conv_U8, operand, name);

		public Conv_U8_()
			: base(null, null, null)
		{
		}
	}

	public class Callvirt_ : CodeMatch
	{
		public Callvirt_ this[object operand = null, string name = null] => (Callvirt_)Set(OpCodes.Callvirt, operand, name);

		public Callvirt_()
			: base(null, null, null)
		{
		}
	}

	public class Cpobj_ : CodeMatch
	{
		public Cpobj_ this[object operand = null, string name = null] => (Cpobj_)Set(OpCodes.Cpobj, operand, name);

		public Cpobj_()
			: base(null, null, null)
		{
		}
	}

	public class Ldobj_ : CodeMatch
	{
		public Ldobj_ this[object operand = null, string name = null] => (Ldobj_)Set(OpCodes.Ldobj, operand, name);

		public Ldobj_()
			: base(null, null, null)
		{
		}
	}

	public class Ldstr_ : CodeMatch
	{
		public Ldstr_ this[object operand = null, string name = null] => (Ldstr_)Set(OpCodes.Ldstr, operand, name);

		public Ldstr_()
			: base(null, null, null)
		{
		}
	}

	public class Newobj_ : CodeMatch
	{
		public Newobj_ this[object operand = null, string name = null] => (Newobj_)Set(OpCodes.Newobj, operand, name);

		public Newobj_()
			: base(null, null, null)
		{
		}
	}

	public class Castclass_ : CodeMatch
	{
		public Castclass_ this[object operand = null, string name = null] => (Castclass_)Set(OpCodes.Castclass, operand, name);

		public Castclass_()
			: base(null, null, null)
		{
		}
	}

	public class Isinst_ : CodeMatch
	{
		public Isinst_ this[object operand = null, string name = null] => (Isinst_)Set(OpCodes.Isinst, operand, name);

		public Isinst_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_R_Un_ : CodeMatch
	{
		public Conv_R_Un_ this[object operand = null, string name = null] => (Conv_R_Un_)Set(OpCodes.Conv_R_Un, operand, name);

		public Conv_R_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Unbox_ : CodeMatch
	{
		public Unbox_ this[object operand = null, string name = null] => (Unbox_)Set(OpCodes.Unbox, operand, name);

		public Unbox_()
			: base(null, null, null)
		{
		}
	}

	public class Throw_ : CodeMatch
	{
		public Throw_ this[object operand = null, string name = null] => (Throw_)Set(OpCodes.Throw, operand, name);

		public Throw_()
			: base(null, null, null)
		{
		}
	}

	public class Ldfld_ : CodeMatch
	{
		public Ldfld_ this[object operand = null, string name = null] => (Ldfld_)Set(OpCodes.Ldfld, operand, name);

		public Ldfld_()
			: base(null, null, null)
		{
		}
	}

	public class Ldflda_ : CodeMatch
	{
		public Ldflda_ this[object operand = null, string name = null] => (Ldflda_)Set(OpCodes.Ldflda, operand, name);

		public Ldflda_()
			: base(null, null, null)
		{
		}
	}

	public class Stfld_ : CodeMatch
	{
		public Stfld_ this[object operand = null, string name = null] => (Stfld_)Set(OpCodes.Stfld, operand, name);

		public Stfld_()
			: base(null, null, null)
		{
		}
	}

	public class Ldsfld_ : CodeMatch
	{
		public Ldsfld_ this[object operand = null, string name = null] => (Ldsfld_)Set(OpCodes.Ldsfld, operand, name);

		public Ldsfld_()
			: base(null, null, null)
		{
		}
	}

	public class Ldsflda_ : CodeMatch
	{
		public Ldsflda_ this[object operand = null, string name = null] => (Ldsflda_)Set(OpCodes.Ldsflda, operand, name);

		public Ldsflda_()
			: base(null, null, null)
		{
		}
	}

	public class Stsfld_ : CodeMatch
	{
		public Stsfld_ this[object operand = null, string name = null] => (Stsfld_)Set(OpCodes.Stsfld, operand, name);

		public Stsfld_()
			: base(null, null, null)
		{
		}
	}

	public class Stobj_ : CodeMatch
	{
		public Stobj_ this[object operand = null, string name = null] => (Stobj_)Set(OpCodes.Stobj, operand, name);

		public Stobj_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I1_Un_ : CodeMatch
	{
		public Conv_Ovf_I1_Un_ this[object operand = null, string name = null] => (Conv_Ovf_I1_Un_)Set(OpCodes.Conv_Ovf_I1_Un, operand, name);

		public Conv_Ovf_I1_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I2_Un_ : CodeMatch
	{
		public Conv_Ovf_I2_Un_ this[object operand = null, string name = null] => (Conv_Ovf_I2_Un_)Set(OpCodes.Conv_Ovf_I2_Un, operand, name);

		public Conv_Ovf_I2_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I4_Un_ : CodeMatch
	{
		public Conv_Ovf_I4_Un_ this[object operand = null, string name = null] => (Conv_Ovf_I4_Un_)Set(OpCodes.Conv_Ovf_I4_Un, operand, name);

		public Conv_Ovf_I4_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I8_Un_ : CodeMatch
	{
		public Conv_Ovf_I8_Un_ this[object operand = null, string name = null] => (Conv_Ovf_I8_Un_)Set(OpCodes.Conv_Ovf_I8_Un, operand, name);

		public Conv_Ovf_I8_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U1_Un_ : CodeMatch
	{
		public Conv_Ovf_U1_Un_ this[object operand = null, string name = null] => (Conv_Ovf_U1_Un_)Set(OpCodes.Conv_Ovf_U1_Un, operand, name);

		public Conv_Ovf_U1_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U2_Un_ : CodeMatch
	{
		public Conv_Ovf_U2_Un_ this[object operand = null, string name = null] => (Conv_Ovf_U2_Un_)Set(OpCodes.Conv_Ovf_U2_Un, operand, name);

		public Conv_Ovf_U2_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U4_Un_ : CodeMatch
	{
		public Conv_Ovf_U4_Un_ this[object operand = null, string name = null] => (Conv_Ovf_U4_Un_)Set(OpCodes.Conv_Ovf_U4_Un, operand, name);

		public Conv_Ovf_U4_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U8_Un_ : CodeMatch
	{
		public Conv_Ovf_U8_Un_ this[object operand = null, string name = null] => (Conv_Ovf_U8_Un_)Set(OpCodes.Conv_Ovf_U8_Un, operand, name);

		public Conv_Ovf_U8_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I_Un_ : CodeMatch
	{
		public Conv_Ovf_I_Un_ this[object operand = null, string name = null] => (Conv_Ovf_I_Un_)Set(OpCodes.Conv_Ovf_I_Un, operand, name);

		public Conv_Ovf_I_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U_Un_ : CodeMatch
	{
		public Conv_Ovf_U_Un_ this[object operand = null, string name = null] => (Conv_Ovf_U_Un_)Set(OpCodes.Conv_Ovf_U_Un, operand, name);

		public Conv_Ovf_U_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Box_ : CodeMatch
	{
		public Box_ this[object operand = null, string name = null] => (Box_)Set(OpCodes.Box, operand, name);

		public Box_()
			: base(null, null, null)
		{
		}
	}

	public class Newarr_ : CodeMatch
	{
		public Newarr_ this[object operand = null, string name = null] => (Newarr_)Set(OpCodes.Newarr, operand, name);

		public Newarr_()
			: base(null, null, null)
		{
		}
	}

	public class Ldlen_ : CodeMatch
	{
		public Ldlen_ this[object operand = null, string name = null] => (Ldlen_)Set(OpCodes.Ldlen, operand, name);

		public Ldlen_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelema_ : CodeMatch
	{
		public Ldelema_ this[object operand = null, string name = null] => (Ldelema_)Set(OpCodes.Ldelema, operand, name);

		public Ldelema_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_I1_ : CodeMatch
	{
		public Ldelem_I1_ this[object operand = null, string name = null] => (Ldelem_I1_)Set(OpCodes.Ldelem_I1, operand, name);

		public Ldelem_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_U1_ : CodeMatch
	{
		public Ldelem_U1_ this[object operand = null, string name = null] => (Ldelem_U1_)Set(OpCodes.Ldelem_U1, operand, name);

		public Ldelem_U1_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_I2_ : CodeMatch
	{
		public Ldelem_I2_ this[object operand = null, string name = null] => (Ldelem_I2_)Set(OpCodes.Ldelem_I2, operand, name);

		public Ldelem_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_U2_ : CodeMatch
	{
		public Ldelem_U2_ this[object operand = null, string name = null] => (Ldelem_U2_)Set(OpCodes.Ldelem_U2, operand, name);

		public Ldelem_U2_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_I4_ : CodeMatch
	{
		public Ldelem_I4_ this[object operand = null, string name = null] => (Ldelem_I4_)Set(OpCodes.Ldelem_I4, operand, name);

		public Ldelem_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_U4_ : CodeMatch
	{
		public Ldelem_U4_ this[object operand = null, string name = null] => (Ldelem_U4_)Set(OpCodes.Ldelem_U4, operand, name);

		public Ldelem_U4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_I8_ : CodeMatch
	{
		public Ldelem_I8_ this[object operand = null, string name = null] => (Ldelem_I8_)Set(OpCodes.Ldelem_I8, operand, name);

		public Ldelem_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_I_ : CodeMatch
	{
		public Ldelem_I_ this[object operand = null, string name = null] => (Ldelem_I_)Set(OpCodes.Ldelem_I, operand, name);

		public Ldelem_I_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_R4_ : CodeMatch
	{
		public Ldelem_R4_ this[object operand = null, string name = null] => (Ldelem_R4_)Set(OpCodes.Ldelem_R4, operand, name);

		public Ldelem_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_R8_ : CodeMatch
	{
		public Ldelem_R8_ this[object operand = null, string name = null] => (Ldelem_R8_)Set(OpCodes.Ldelem_R8, operand, name);

		public Ldelem_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_Ref_ : CodeMatch
	{
		public Ldelem_Ref_ this[object operand = null, string name = null] => (Ldelem_Ref_)Set(OpCodes.Ldelem_Ref, operand, name);

		public Ldelem_Ref_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_I_ : CodeMatch
	{
		public Stelem_I_ this[object operand = null, string name = null] => (Stelem_I_)Set(OpCodes.Stelem_I, operand, name);

		public Stelem_I_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_I1_ : CodeMatch
	{
		public Stelem_I1_ this[object operand = null, string name = null] => (Stelem_I1_)Set(OpCodes.Stelem_I1, operand, name);

		public Stelem_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_I2_ : CodeMatch
	{
		public Stelem_I2_ this[object operand = null, string name = null] => (Stelem_I2_)Set(OpCodes.Stelem_I2, operand, name);

		public Stelem_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_I4_ : CodeMatch
	{
		public Stelem_I4_ this[object operand = null, string name = null] => (Stelem_I4_)Set(OpCodes.Stelem_I4, operand, name);

		public Stelem_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_I8_ : CodeMatch
	{
		public Stelem_I8_ this[object operand = null, string name = null] => (Stelem_I8_)Set(OpCodes.Stelem_I8, operand, name);

		public Stelem_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_R4_ : CodeMatch
	{
		public Stelem_R4_ this[object operand = null, string name = null] => (Stelem_R4_)Set(OpCodes.Stelem_R4, operand, name);

		public Stelem_R4_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_R8_ : CodeMatch
	{
		public Stelem_R8_ this[object operand = null, string name = null] => (Stelem_R8_)Set(OpCodes.Stelem_R8, operand, name);

		public Stelem_R8_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_Ref_ : CodeMatch
	{
		public Stelem_Ref_ this[object operand = null, string name = null] => (Stelem_Ref_)Set(OpCodes.Stelem_Ref, operand, name);

		public Stelem_Ref_()
			: base(null, null, null)
		{
		}
	}

	public class Ldelem_ : CodeMatch
	{
		public Ldelem_ this[object operand = null, string name = null] => (Ldelem_)Set(OpCodes.Ldelem, operand, name);

		public Ldelem_()
			: base(null, null, null)
		{
		}
	}

	public class Stelem_ : CodeMatch
	{
		public Stelem_ this[object operand = null, string name = null] => (Stelem_)Set(OpCodes.Stelem, operand, name);

		public Stelem_()
			: base(null, null, null)
		{
		}
	}

	public class Unbox_Any_ : CodeMatch
	{
		public Unbox_Any_ this[object operand = null, string name = null] => (Unbox_Any_)Set(OpCodes.Unbox_Any, operand, name);

		public Unbox_Any_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I1_ : CodeMatch
	{
		public Conv_Ovf_I1_ this[object operand = null, string name = null] => (Conv_Ovf_I1_)Set(OpCodes.Conv_Ovf_I1, operand, name);

		public Conv_Ovf_I1_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U1_ : CodeMatch
	{
		public Conv_Ovf_U1_ this[object operand = null, string name = null] => (Conv_Ovf_U1_)Set(OpCodes.Conv_Ovf_U1, operand, name);

		public Conv_Ovf_U1_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I2_ : CodeMatch
	{
		public Conv_Ovf_I2_ this[object operand = null, string name = null] => (Conv_Ovf_I2_)Set(OpCodes.Conv_Ovf_I2, operand, name);

		public Conv_Ovf_I2_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U2_ : CodeMatch
	{
		public Conv_Ovf_U2_ this[object operand = null, string name = null] => (Conv_Ovf_U2_)Set(OpCodes.Conv_Ovf_U2, operand, name);

		public Conv_Ovf_U2_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I4_ : CodeMatch
	{
		public Conv_Ovf_I4_ this[object operand = null, string name = null] => (Conv_Ovf_I4_)Set(OpCodes.Conv_Ovf_I4, operand, name);

		public Conv_Ovf_I4_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U4_ : CodeMatch
	{
		public Conv_Ovf_U4_ this[object operand = null, string name = null] => (Conv_Ovf_U4_)Set(OpCodes.Conv_Ovf_U4, operand, name);

		public Conv_Ovf_U4_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I8_ : CodeMatch
	{
		public Conv_Ovf_I8_ this[object operand = null, string name = null] => (Conv_Ovf_I8_)Set(OpCodes.Conv_Ovf_I8, operand, name);

		public Conv_Ovf_I8_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U8_ : CodeMatch
	{
		public Conv_Ovf_U8_ this[object operand = null, string name = null] => (Conv_Ovf_U8_)Set(OpCodes.Conv_Ovf_U8, operand, name);

		public Conv_Ovf_U8_()
			: base(null, null, null)
		{
		}
	}

	public class Refanyval_ : CodeMatch
	{
		public Refanyval_ this[object operand = null, string name = null] => (Refanyval_)Set(OpCodes.Refanyval, operand, name);

		public Refanyval_()
			: base(null, null, null)
		{
		}
	}

	public class Ckfinite_ : CodeMatch
	{
		public Ckfinite_ this[object operand = null, string name = null] => (Ckfinite_)Set(OpCodes.Ckfinite, operand, name);

		public Ckfinite_()
			: base(null, null, null)
		{
		}
	}

	public class Mkrefany_ : CodeMatch
	{
		public Mkrefany_ this[object operand = null, string name = null] => (Mkrefany_)Set(OpCodes.Mkrefany, operand, name);

		public Mkrefany_()
			: base(null, null, null)
		{
		}
	}

	public class Ldtoken_ : CodeMatch
	{
		public Ldtoken_ this[object operand = null, string name = null] => (Ldtoken_)Set(OpCodes.Ldtoken, operand, name);

		public Ldtoken_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_U2_ : CodeMatch
	{
		public Conv_U2_ this[object operand = null, string name = null] => (Conv_U2_)Set(OpCodes.Conv_U2, operand, name);

		public Conv_U2_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_U1_ : CodeMatch
	{
		public Conv_U1_ this[object operand = null, string name = null] => (Conv_U1_)Set(OpCodes.Conv_U1, operand, name);

		public Conv_U1_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_I_ : CodeMatch
	{
		public Conv_I_ this[object operand = null, string name = null] => (Conv_I_)Set(OpCodes.Conv_I, operand, name);

		public Conv_I_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_I_ : CodeMatch
	{
		public Conv_Ovf_I_ this[object operand = null, string name = null] => (Conv_Ovf_I_)Set(OpCodes.Conv_Ovf_I, operand, name);

		public Conv_Ovf_I_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_Ovf_U_ : CodeMatch
	{
		public Conv_Ovf_U_ this[object operand = null, string name = null] => (Conv_Ovf_U_)Set(OpCodes.Conv_Ovf_U, operand, name);

		public Conv_Ovf_U_()
			: base(null, null, null)
		{
		}
	}

	public class Add_Ovf_ : CodeMatch
	{
		public Add_Ovf_ this[object operand = null, string name = null] => (Add_Ovf_)Set(OpCodes.Add_Ovf, operand, name);

		public Add_Ovf_()
			: base(null, null, null)
		{
		}
	}

	public class Add_Ovf_Un_ : CodeMatch
	{
		public Add_Ovf_Un_ this[object operand = null, string name = null] => (Add_Ovf_Un_)Set(OpCodes.Add_Ovf_Un, operand, name);

		public Add_Ovf_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Mul_Ovf_ : CodeMatch
	{
		public Mul_Ovf_ this[object operand = null, string name = null] => (Mul_Ovf_)Set(OpCodes.Mul_Ovf, operand, name);

		public Mul_Ovf_()
			: base(null, null, null)
		{
		}
	}

	public class Mul_Ovf_Un_ : CodeMatch
	{
		public Mul_Ovf_Un_ this[object operand = null, string name = null] => (Mul_Ovf_Un_)Set(OpCodes.Mul_Ovf_Un, operand, name);

		public Mul_Ovf_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Sub_Ovf_ : CodeMatch
	{
		public Sub_Ovf_ this[object operand = null, string name = null] => (Sub_Ovf_)Set(OpCodes.Sub_Ovf, operand, name);

		public Sub_Ovf_()
			: base(null, null, null)
		{
		}
	}

	public class Sub_Ovf_Un_ : CodeMatch
	{
		public Sub_Ovf_Un_ this[object operand = null, string name = null] => (Sub_Ovf_Un_)Set(OpCodes.Sub_Ovf_Un, operand, name);

		public Sub_Ovf_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Endfinally_ : CodeMatch
	{
		public Endfinally_ this[object operand = null, string name = null] => (Endfinally_)Set(OpCodes.Endfinally, operand, name);

		public Endfinally_()
			: base(null, null, null)
		{
		}
	}

	public class Leave_ : CodeMatch
	{
		public Leave_ this[object operand = null, string name = null] => (Leave_)Set(OpCodes.Leave, operand, name);

		public Leave_()
			: base(null, null, null)
		{
		}
	}

	public class Leave_S_ : CodeMatch
	{
		public Leave_S_ this[object operand = null, string name = null] => (Leave_S_)Set(OpCodes.Leave_S, operand, name);

		public Leave_S_()
			: base(null, null, null)
		{
		}
	}

	public class Stind_I_ : CodeMatch
	{
		public Stind_I_ this[object operand = null, string name = null] => (Stind_I_)Set(OpCodes.Stind_I, operand, name);

		public Stind_I_()
			: base(null, null, null)
		{
		}
	}

	public class Conv_U_ : CodeMatch
	{
		public Conv_U_ this[object operand = null, string name = null] => (Conv_U_)Set(OpCodes.Conv_U, operand, name);

		public Conv_U_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix7_ : CodeMatch
	{
		public Prefix7_ this[object operand = null, string name = null] => (Prefix7_)Set(OpCodes.Prefix7, operand, name);

		public Prefix7_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix6_ : CodeMatch
	{
		public Prefix6_ this[object operand = null, string name = null] => (Prefix6_)Set(OpCodes.Prefix6, operand, name);

		public Prefix6_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix5_ : CodeMatch
	{
		public Prefix5_ this[object operand = null, string name = null] => (Prefix5_)Set(OpCodes.Prefix5, operand, name);

		public Prefix5_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix4_ : CodeMatch
	{
		public Prefix4_ this[object operand = null, string name = null] => (Prefix4_)Set(OpCodes.Prefix4, operand, name);

		public Prefix4_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix3_ : CodeMatch
	{
		public Prefix3_ this[object operand = null, string name = null] => (Prefix3_)Set(OpCodes.Prefix3, operand, name);

		public Prefix3_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix2_ : CodeMatch
	{
		public Prefix2_ this[object operand = null, string name = null] => (Prefix2_)Set(OpCodes.Prefix2, operand, name);

		public Prefix2_()
			: base(null, null, null)
		{
		}
	}

	public class Prefix1_ : CodeMatch
	{
		public Prefix1_ this[object operand = null, string name = null] => (Prefix1_)Set(OpCodes.Prefix1, operand, name);

		public Prefix1_()
			: base(null, null, null)
		{
		}
	}

	public class Prefixref_ : CodeMatch
	{
		public Prefixref_ this[object operand = null, string name = null] => (Prefixref_)Set(OpCodes.Prefixref, operand, name);

		public Prefixref_()
			: base(null, null, null)
		{
		}
	}

	public class Arglist_ : CodeMatch
	{
		public Arglist_ this[object operand = null, string name = null] => (Arglist_)Set(OpCodes.Arglist, operand, name);

		public Arglist_()
			: base(null, null, null)
		{
		}
	}

	public class Ceq_ : CodeMatch
	{
		public Ceq_ this[object operand = null, string name = null] => (Ceq_)Set(OpCodes.Ceq, operand, name);

		public Ceq_()
			: base(null, null, null)
		{
		}
	}

	public class Cgt_ : CodeMatch
	{
		public Cgt_ this[object operand = null, string name = null] => (Cgt_)Set(OpCodes.Cgt, operand, name);

		public Cgt_()
			: base(null, null, null)
		{
		}
	}

	public class Cgt_Un_ : CodeMatch
	{
		public Cgt_Un_ this[object operand = null, string name = null] => (Cgt_Un_)Set(OpCodes.Cgt_Un, operand, name);

		public Cgt_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Clt_ : CodeMatch
	{
		public Clt_ this[object operand = null, string name = null] => (Clt_)Set(OpCodes.Clt, operand, name);

		public Clt_()
			: base(null, null, null)
		{
		}
	}

	public class Clt_Un_ : CodeMatch
	{
		public Clt_Un_ this[object operand = null, string name = null] => (Clt_Un_)Set(OpCodes.Clt_Un, operand, name);

		public Clt_Un_()
			: base(null, null, null)
		{
		}
	}

	public class Ldftn_ : CodeMatch
	{
		public Ldftn_ this[object operand = null, string name = null] => (Ldftn_)Set(OpCodes.Ldftn, operand, name);

		public Ldftn_()
			: base(null, null, null)
		{
		}
	}

	public class Ldvirtftn_ : CodeMatch
	{
		public Ldvirtftn_ this[object operand = null, string name = null] => (Ldvirtftn_)Set(OpCodes.Ldvirtftn, operand, name);

		public Ldvirtftn_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarg_ : CodeMatch
	{
		public Ldarg_ this[object operand = null, string name = null] => (Ldarg_)Set(OpCodes.Ldarg, operand, name);

		public Ldarg_()
			: base(null, null, null)
		{
		}
	}

	public class Ldarga_ : CodeMatch
	{
		public Ldarga_ this[object operand = null, string name = null] => (Ldarga_)Set(OpCodes.Ldarga, operand, name);

		public Ldarga_()
			: base(null, null, null)
		{
		}
	}

	public class Starg_ : CodeMatch
	{
		public Starg_ this[object operand = null, string name = null] => (Starg_)Set(OpCodes.Starg, operand, name);

		public Starg_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloc_ : CodeMatch
	{
		public Ldloc_ this[object operand = null, string name = null] => (Ldloc_)Set(OpCodes.Ldloc, operand, name);

		public Ldloc_()
			: base(null, null, null)
		{
		}
	}

	public class Ldloca_ : CodeMatch
	{
		public Ldloca_ this[object operand = null, string name = null] => (Ldloca_)Set(OpCodes.Ldloca, operand, name);

		public Ldloca_()
			: base(null, null, null)
		{
		}
	}

	public class Stloc_ : CodeMatch
	{
		public Stloc_ this[object operand = null, string name = null] => (Stloc_)Set(OpCodes.Stloc, operand, name);

		public Stloc_()
			: base(null, null, null)
		{
		}
	}

	public class Localloc_ : CodeMatch
	{
		public Localloc_ this[object operand = null, string name = null] => (Localloc_)Set(OpCodes.Localloc, operand, name);

		public Localloc_()
			: base(null, null, null)
		{
		}
	}

	public class Endfilter_ : CodeMatch
	{
		public Endfilter_ this[object operand = null, string name = null] => (Endfilter_)Set(OpCodes.Endfilter, operand, name);

		public Endfilter_()
			: base(null, null, null)
		{
		}
	}

	public class Unaligned_ : CodeMatch
	{
		public Unaligned_ this[object operand = null, string name = null] => (Unaligned_)Set(OpCodes.Unaligned, operand, name);

		public Unaligned_()
			: base(null, null, null)
		{
		}
	}

	public class Volatile_ : CodeMatch
	{
		public Volatile_ this[object operand = null, string name = null] => (Volatile_)Set(OpCodes.Volatile, operand, name);

		public Volatile_()
			: base(null, null, null)
		{
		}
	}

	public class Tailcall_ : CodeMatch
	{
		public Tailcall_ this[object operand = null, string name = null] => (Tailcall_)Set(OpCodes.Tailcall, operand, name);

		public Tailcall_()
			: base(null, null, null)
		{
		}
	}

	public class Initobj_ : CodeMatch
	{
		public Initobj_ this[object operand = null, string name = null] => (Initobj_)Set(OpCodes.Initobj, operand, name);

		public Initobj_()
			: base(null, null, null)
		{
		}
	}

	public class Constrained_ : CodeMatch
	{
		public Constrained_ this[object operand = null, string name = null] => (Constrained_)Set(OpCodes.Constrained, operand, name);

		public Constrained_()
			: base(null, null, null)
		{
		}
	}

	public class Cpblk_ : CodeMatch
	{
		public Cpblk_ this[object operand = null, string name = null] => (Cpblk_)Set(OpCodes.Cpblk, operand, name);

		public Cpblk_()
			: base(null, null, null)
		{
		}
	}

	public class Initblk_ : CodeMatch
	{
		public Initblk_ this[object operand = null, string name = null] => (Initblk_)Set(OpCodes.Initblk, operand, name);

		public Initblk_()
			: base(null, null, null)
		{
		}
	}

	public class Rethrow_ : CodeMatch
	{
		public Rethrow_ this[object operand = null, string name = null] => (Rethrow_)Set(OpCodes.Rethrow, operand, name);

		public Rethrow_()
			: base(null, null, null)
		{
		}
	}

	public class Sizeof_ : CodeMatch
	{
		public Sizeof_ this[object operand = null, string name = null] => (Sizeof_)Set(OpCodes.Sizeof, operand, name);

		public Sizeof_()
			: base(null, null, null)
		{
		}
	}

	public class Refanytype_ : CodeMatch
	{
		public Refanytype_ this[object operand = null, string name = null] => (Refanytype_)Set(OpCodes.Refanytype, operand, name);

		public Refanytype_()
			: base(null, null, null)
		{
		}
	}

	public class Readonly_ : CodeMatch
	{
		public Readonly_ this[object operand = null, string name = null] => (Readonly_)Set(OpCodes.Readonly, operand, name);

		public Readonly_()
			: base(null, null, null)
		{
		}
	}

	public static Operand_ Operand => new Operand_();

	public static Nop_ Nop => new Nop_
	{
		opcode = OpCodes.Nop
	};

	public static Break_ Break => new Break_
	{
		opcode = OpCodes.Break
	};

	public static Ldarg_0_ Ldarg_0 => new Ldarg_0_
	{
		opcode = OpCodes.Ldarg_0
	};

	public static Ldarg_1_ Ldarg_1 => new Ldarg_1_
	{
		opcode = OpCodes.Ldarg_1
	};

	public static Ldarg_2_ Ldarg_2 => new Ldarg_2_
	{
		opcode = OpCodes.Ldarg_2
	};

	public static Ldarg_3_ Ldarg_3 => new Ldarg_3_
	{
		opcode = OpCodes.Ldarg_3
	};

	public static Ldloc_0_ Ldloc_0 => new Ldloc_0_
	{
		opcode = OpCodes.Ldloc_0
	};

	public static Ldloc_1_ Ldloc_1 => new Ldloc_1_
	{
		opcode = OpCodes.Ldloc_1
	};

	public static Ldloc_2_ Ldloc_2 => new Ldloc_2_
	{
		opcode = OpCodes.Ldloc_2
	};

	public static Ldloc_3_ Ldloc_3 => new Ldloc_3_
	{
		opcode = OpCodes.Ldloc_3
	};

	public static Stloc_0_ Stloc_0 => new Stloc_0_
	{
		opcode = OpCodes.Stloc_0
	};

	public static Stloc_1_ Stloc_1 => new Stloc_1_
	{
		opcode = OpCodes.Stloc_1
	};

	public static Stloc_2_ Stloc_2 => new Stloc_2_
	{
		opcode = OpCodes.Stloc_2
	};

	public static Stloc_3_ Stloc_3 => new Stloc_3_
	{
		opcode = OpCodes.Stloc_3
	};

	public static Ldarg_S_ Ldarg_S => new Ldarg_S_
	{
		opcode = OpCodes.Ldarg_S
	};

	public static Ldarga_S_ Ldarga_S => new Ldarga_S_
	{
		opcode = OpCodes.Ldarga_S
	};

	public static Starg_S_ Starg_S => new Starg_S_
	{
		opcode = OpCodes.Starg_S
	};

	public static Ldloc_S_ Ldloc_S => new Ldloc_S_
	{
		opcode = OpCodes.Ldloc_S
	};

	public static Ldloca_S_ Ldloca_S => new Ldloca_S_
	{
		opcode = OpCodes.Ldloca_S
	};

	public static Stloc_S_ Stloc_S => new Stloc_S_
	{
		opcode = OpCodes.Stloc_S
	};

	public static Ldnull_ Ldnull => new Ldnull_
	{
		opcode = OpCodes.Ldnull
	};

	public static Ldc_I4_M1_ Ldc_I4_M1 => new Ldc_I4_M1_
	{
		opcode = OpCodes.Ldc_I4_M1
	};

	public static Ldc_I4_0_ Ldc_I4_0 => new Ldc_I4_0_
	{
		opcode = OpCodes.Ldc_I4_0
	};

	public static Ldc_I4_1_ Ldc_I4_1 => new Ldc_I4_1_
	{
		opcode = OpCodes.Ldc_I4_1
	};

	public static Ldc_I4_2_ Ldc_I4_2 => new Ldc_I4_2_
	{
		opcode = OpCodes.Ldc_I4_2
	};

	public static Ldc_I4_3_ Ldc_I4_3 => new Ldc_I4_3_
	{
		opcode = OpCodes.Ldc_I4_3
	};

	public static Ldc_I4_4_ Ldc_I4_4 => new Ldc_I4_4_
	{
		opcode = OpCodes.Ldc_I4_4
	};

	public static Ldc_I4_5_ Ldc_I4_5 => new Ldc_I4_5_
	{
		opcode = OpCodes.Ldc_I4_5
	};

	public static Ldc_I4_6_ Ldc_I4_6 => new Ldc_I4_6_
	{
		opcode = OpCodes.Ldc_I4_6
	};

	public static Ldc_I4_7_ Ldc_I4_7 => new Ldc_I4_7_
	{
		opcode = OpCodes.Ldc_I4_7
	};

	public static Ldc_I4_8_ Ldc_I4_8 => new Ldc_I4_8_
	{
		opcode = OpCodes.Ldc_I4_8
	};

	public static Ldc_I4_S_ Ldc_I4_S => new Ldc_I4_S_
	{
		opcode = OpCodes.Ldc_I4_S
	};

	public static Ldc_I4_ Ldc_I4 => new Ldc_I4_
	{
		opcode = OpCodes.Ldc_I4
	};

	public static Ldc_I8_ Ldc_I8 => new Ldc_I8_
	{
		opcode = OpCodes.Ldc_I8
	};

	public static Ldc_R4_ Ldc_R4 => new Ldc_R4_
	{
		opcode = OpCodes.Ldc_R4
	};

	public static Ldc_R8_ Ldc_R8 => new Ldc_R8_
	{
		opcode = OpCodes.Ldc_R8
	};

	public static Dup_ Dup => new Dup_
	{
		opcode = OpCodes.Dup
	};

	public static Pop_ Pop => new Pop_
	{
		opcode = OpCodes.Pop
	};

	public static Jmp_ Jmp => new Jmp_
	{
		opcode = OpCodes.Jmp
	};

	public static Call_ Call => new Call_
	{
		opcode = OpCodes.Call
	};

	public static Calli_ Calli => new Calli_
	{
		opcode = OpCodes.Calli
	};

	public static Ret_ Ret => new Ret_
	{
		opcode = OpCodes.Ret
	};

	public static Br_S_ Br_S => new Br_S_
	{
		opcode = OpCodes.Br_S
	};

	public static Brfalse_S_ Brfalse_S => new Brfalse_S_
	{
		opcode = OpCodes.Brfalse_S
	};

	public static Brtrue_S_ Brtrue_S => new Brtrue_S_
	{
		opcode = OpCodes.Brtrue_S
	};

	public static Beq_S_ Beq_S => new Beq_S_
	{
		opcode = OpCodes.Beq_S
	};

	public static Bge_S_ Bge_S => new Bge_S_
	{
		opcode = OpCodes.Bge_S
	};

	public static Bgt_S_ Bgt_S => new Bgt_S_
	{
		opcode = OpCodes.Bgt_S
	};

	public static Ble_S_ Ble_S => new Ble_S_
	{
		opcode = OpCodes.Ble_S
	};

	public static Blt_S_ Blt_S => new Blt_S_
	{
		opcode = OpCodes.Blt_S
	};

	public static Bne_Un_S_ Bne_Un_S => new Bne_Un_S_
	{
		opcode = OpCodes.Bne_Un_S
	};

	public static Bge_Un_S_ Bge_Un_S => new Bge_Un_S_
	{
		opcode = OpCodes.Bge_Un_S
	};

	public static Bgt_Un_S_ Bgt_Un_S => new Bgt_Un_S_
	{
		opcode = OpCodes.Bgt_Un_S
	};

	public static Ble_Un_S_ Ble_Un_S => new Ble_Un_S_
	{
		opcode = OpCodes.Ble_Un_S
	};

	public static Blt_Un_S_ Blt_Un_S => new Blt_Un_S_
	{
		opcode = OpCodes.Blt_Un_S
	};

	public static Br_ Br => new Br_
	{
		opcode = OpCodes.Br
	};

	public static Brfalse_ Brfalse => new Brfalse_
	{
		opcode = OpCodes.Brfalse
	};

	public static Brtrue_ Brtrue => new Brtrue_
	{
		opcode = OpCodes.Brtrue
	};

	public static Beq_ Beq => new Beq_
	{
		opcode = OpCodes.Beq
	};

	public static Bge_ Bge => new Bge_
	{
		opcode = OpCodes.Bge
	};

	public static Bgt_ Bgt => new Bgt_
	{
		opcode = OpCodes.Bgt
	};

	public static Ble_ Ble => new Ble_
	{
		opcode = OpCodes.Ble
	};

	public static Blt_ Blt => new Blt_
	{
		opcode = OpCodes.Blt
	};

	public static Bne_Un_ Bne_Un => new Bne_Un_
	{
		opcode = OpCodes.Bne_Un
	};

	public static Bge_Un_ Bge_Un => new Bge_Un_
	{
		opcode = OpCodes.Bge_Un
	};

	public static Bgt_Un_ Bgt_Un => new Bgt_Un_
	{
		opcode = OpCodes.Bgt_Un
	};

	public static Ble_Un_ Ble_Un => new Ble_Un_
	{
		opcode = OpCodes.Ble_Un
	};

	public static Blt_Un_ Blt_Un => new Blt_Un_
	{
		opcode = OpCodes.Blt_Un
	};

	public static Switch_ Switch => new Switch_
	{
		opcode = OpCodes.Switch
	};

	public static Ldind_I1_ Ldind_I1 => new Ldind_I1_
	{
		opcode = OpCodes.Ldind_I1
	};

	public static Ldind_U1_ Ldind_U1 => new Ldind_U1_
	{
		opcode = OpCodes.Ldind_U1
	};

	public static Ldind_I2_ Ldind_I2 => new Ldind_I2_
	{
		opcode = OpCodes.Ldind_I2
	};

	public static Ldind_U2_ Ldind_U2 => new Ldind_U2_
	{
		opcode = OpCodes.Ldind_U2
	};

	public static Ldind_I4_ Ldind_I4 => new Ldind_I4_
	{
		opcode = OpCodes.Ldind_I4
	};

	public static Ldind_U4_ Ldind_U4 => new Ldind_U4_
	{
		opcode = OpCodes.Ldind_U4
	};

	public static Ldind_I8_ Ldind_I8 => new Ldind_I8_
	{
		opcode = OpCodes.Ldind_I8
	};

	public static Ldind_I_ Ldind_I => new Ldind_I_
	{
		opcode = OpCodes.Ldind_I
	};

	public static Ldind_R4_ Ldind_R4 => new Ldind_R4_
	{
		opcode = OpCodes.Ldind_R4
	};

	public static Ldind_R8_ Ldind_R8 => new Ldind_R8_
	{
		opcode = OpCodes.Ldind_R8
	};

	public static Ldind_Ref_ Ldind_Ref => new Ldind_Ref_
	{
		opcode = OpCodes.Ldind_Ref
	};

	public static Stind_Ref_ Stind_Ref => new Stind_Ref_
	{
		opcode = OpCodes.Stind_Ref
	};

	public static Stind_I1_ Stind_I1 => new Stind_I1_
	{
		opcode = OpCodes.Stind_I1
	};

	public static Stind_I2_ Stind_I2 => new Stind_I2_
	{
		opcode = OpCodes.Stind_I2
	};

	public static Stind_I4_ Stind_I4 => new Stind_I4_
	{
		opcode = OpCodes.Stind_I4
	};

	public static Stind_I8_ Stind_I8 => new Stind_I8_
	{
		opcode = OpCodes.Stind_I8
	};

	public static Stind_R4_ Stind_R4 => new Stind_R4_
	{
		opcode = OpCodes.Stind_R4
	};

	public static Stind_R8_ Stind_R8 => new Stind_R8_
	{
		opcode = OpCodes.Stind_R8
	};

	public static Add_ Add => new Add_
	{
		opcode = OpCodes.Add
	};

	public static Sub_ Sub => new Sub_
	{
		opcode = OpCodes.Sub
	};

	public static Mul_ Mul => new Mul_
	{
		opcode = OpCodes.Mul
	};

	public static Div_ Div => new Div_
	{
		opcode = OpCodes.Div
	};

	public static Div_Un_ Div_Un => new Div_Un_
	{
		opcode = OpCodes.Div_Un
	};

	public static Rem_ Rem => new Rem_
	{
		opcode = OpCodes.Rem
	};

	public static Rem_Un_ Rem_Un => new Rem_Un_
	{
		opcode = OpCodes.Rem_Un
	};

	public static And_ And => new And_
	{
		opcode = OpCodes.And
	};

	public static Or_ Or => new Or_
	{
		opcode = OpCodes.Or
	};

	public static Xor_ Xor => new Xor_
	{
		opcode = OpCodes.Xor
	};

	public static Shl_ Shl => new Shl_
	{
		opcode = OpCodes.Shl
	};

	public static Shr_ Shr => new Shr_
	{
		opcode = OpCodes.Shr
	};

	public static Shr_Un_ Shr_Un => new Shr_Un_
	{
		opcode = OpCodes.Shr_Un
	};

	public static Neg_ Neg => new Neg_
	{
		opcode = OpCodes.Neg
	};

	public static Not_ Not => new Not_
	{
		opcode = OpCodes.Not
	};

	public static Conv_I1_ Conv_I1 => new Conv_I1_
	{
		opcode = OpCodes.Conv_I1
	};

	public static Conv_I2_ Conv_I2 => new Conv_I2_
	{
		opcode = OpCodes.Conv_I2
	};

	public static Conv_I4_ Conv_I4 => new Conv_I4_
	{
		opcode = OpCodes.Conv_I4
	};

	public static Conv_I8_ Conv_I8 => new Conv_I8_
	{
		opcode = OpCodes.Conv_I8
	};

	public static Conv_R4_ Conv_R4 => new Conv_R4_
	{
		opcode = OpCodes.Conv_R4
	};

	public static Conv_R8_ Conv_R8 => new Conv_R8_
	{
		opcode = OpCodes.Conv_R8
	};

	public static Conv_U4_ Conv_U4 => new Conv_U4_
	{
		opcode = OpCodes.Conv_U4
	};

	public static Conv_U8_ Conv_U8 => new Conv_U8_
	{
		opcode = OpCodes.Conv_U8
	};

	public static Callvirt_ Callvirt => new Callvirt_
	{
		opcode = OpCodes.Callvirt
	};

	public static Cpobj_ Cpobj => new Cpobj_
	{
		opcode = OpCodes.Cpobj
	};

	public static Ldobj_ Ldobj => new Ldobj_
	{
		opcode = OpCodes.Ldobj
	};

	public static Ldstr_ Ldstr => new Ldstr_
	{
		opcode = OpCodes.Ldstr
	};

	public static Newobj_ Newobj => new Newobj_
	{
		opcode = OpCodes.Newobj
	};

	public static Castclass_ Castclass => new Castclass_
	{
		opcode = OpCodes.Castclass
	};

	public static Isinst_ Isinst => new Isinst_
	{
		opcode = OpCodes.Isinst
	};

	public static Conv_R_Un_ Conv_R_Un => new Conv_R_Un_
	{
		opcode = OpCodes.Conv_R_Un
	};

	public static Unbox_ Unbox => new Unbox_
	{
		opcode = OpCodes.Unbox
	};

	public static Throw_ Throw => new Throw_
	{
		opcode = OpCodes.Throw
	};

	public static Ldfld_ Ldfld => new Ldfld_
	{
		opcode = OpCodes.Ldfld
	};

	public static Ldflda_ Ldflda => new Ldflda_
	{
		opcode = OpCodes.Ldflda
	};

	public static Stfld_ Stfld => new Stfld_
	{
		opcode = OpCodes.Stfld
	};

	public static Ldsfld_ Ldsfld => new Ldsfld_
	{
		opcode = OpCodes.Ldsfld
	};

	public static Ldsflda_ Ldsflda => new Ldsflda_
	{
		opcode = OpCodes.Ldsflda
	};

	public static Stsfld_ Stsfld => new Stsfld_
	{
		opcode = OpCodes.Stsfld
	};

	public static Stobj_ Stobj => new Stobj_
	{
		opcode = OpCodes.Stobj
	};

	public static Conv_Ovf_I1_Un_ Conv_Ovf_I1_Un => new Conv_Ovf_I1_Un_
	{
		opcode = OpCodes.Conv_Ovf_I1_Un
	};

	public static Conv_Ovf_I2_Un_ Conv_Ovf_I2_Un => new Conv_Ovf_I2_Un_
	{
		opcode = OpCodes.Conv_Ovf_I2_Un
	};

	public static Conv_Ovf_I4_Un_ Conv_Ovf_I4_Un => new Conv_Ovf_I4_Un_
	{
		opcode = OpCodes.Conv_Ovf_I4_Un
	};

	public static Conv_Ovf_I8_Un_ Conv_Ovf_I8_Un => new Conv_Ovf_I8_Un_
	{
		opcode = OpCodes.Conv_Ovf_I8_Un
	};

	public static Conv_Ovf_U1_Un_ Conv_Ovf_U1_Un => new Conv_Ovf_U1_Un_
	{
		opcode = OpCodes.Conv_Ovf_U1_Un
	};

	public static Conv_Ovf_U2_Un_ Conv_Ovf_U2_Un => new Conv_Ovf_U2_Un_
	{
		opcode = OpCodes.Conv_Ovf_U2_Un
	};

	public static Conv_Ovf_U4_Un_ Conv_Ovf_U4_Un => new Conv_Ovf_U4_Un_
	{
		opcode = OpCodes.Conv_Ovf_U4_Un
	};

	public static Conv_Ovf_U8_Un_ Conv_Ovf_U8_Un => new Conv_Ovf_U8_Un_
	{
		opcode = OpCodes.Conv_Ovf_U8_Un
	};

	public static Conv_Ovf_I_Un_ Conv_Ovf_I_Un => new Conv_Ovf_I_Un_
	{
		opcode = OpCodes.Conv_Ovf_I_Un
	};

	public static Conv_Ovf_U_Un_ Conv_Ovf_U_Un => new Conv_Ovf_U_Un_
	{
		opcode = OpCodes.Conv_Ovf_U_Un
	};

	public static Box_ Box => new Box_
	{
		opcode = OpCodes.Box
	};

	public static Newarr_ Newarr => new Newarr_
	{
		opcode = OpCodes.Newarr
	};

	public static Ldlen_ Ldlen => new Ldlen_
	{
		opcode = OpCodes.Ldlen
	};

	public static Ldelema_ Ldelema => new Ldelema_
	{
		opcode = OpCodes.Ldelema
	};

	public static Ldelem_I1_ Ldelem_I1 => new Ldelem_I1_
	{
		opcode = OpCodes.Ldelem_I1
	};

	public static Ldelem_U1_ Ldelem_U1 => new Ldelem_U1_
	{
		opcode = OpCodes.Ldelem_U1
	};

	public static Ldelem_I2_ Ldelem_I2 => new Ldelem_I2_
	{
		opcode = OpCodes.Ldelem_I2
	};

	public static Ldelem_U2_ Ldelem_U2 => new Ldelem_U2_
	{
		opcode = OpCodes.Ldelem_U2
	};

	public static Ldelem_I4_ Ldelem_I4 => new Ldelem_I4_
	{
		opcode = OpCodes.Ldelem_I4
	};

	public static Ldelem_U4_ Ldelem_U4 => new Ldelem_U4_
	{
		opcode = OpCodes.Ldelem_U4
	};

	public static Ldelem_I8_ Ldelem_I8 => new Ldelem_I8_
	{
		opcode = OpCodes.Ldelem_I8
	};

	public static Ldelem_I_ Ldelem_I => new Ldelem_I_
	{
		opcode = OpCodes.Ldelem_I
	};

	public static Ldelem_R4_ Ldelem_R4 => new Ldelem_R4_
	{
		opcode = OpCodes.Ldelem_R4
	};

	public static Ldelem_R8_ Ldelem_R8 => new Ldelem_R8_
	{
		opcode = OpCodes.Ldelem_R8
	};

	public static Ldelem_Ref_ Ldelem_Ref => new Ldelem_Ref_
	{
		opcode = OpCodes.Ldelem_Ref
	};

	public static Stelem_I_ Stelem_I => new Stelem_I_
	{
		opcode = OpCodes.Stelem_I
	};

	public static Stelem_I1_ Stelem_I1 => new Stelem_I1_
	{
		opcode = OpCodes.Stelem_I1
	};

	public static Stelem_I2_ Stelem_I2 => new Stelem_I2_
	{
		opcode = OpCodes.Stelem_I2
	};

	public static Stelem_I4_ Stelem_I4 => new Stelem_I4_
	{
		opcode = OpCodes.Stelem_I4
	};

	public static Stelem_I8_ Stelem_I8 => new Stelem_I8_
	{
		opcode = OpCodes.Stelem_I8
	};

	public static Stelem_R4_ Stelem_R4 => new Stelem_R4_
	{
		opcode = OpCodes.Stelem_R4
	};

	public static Stelem_R8_ Stelem_R8 => new Stelem_R8_
	{
		opcode = OpCodes.Stelem_R8
	};

	public static Stelem_Ref_ Stelem_Ref => new Stelem_Ref_
	{
		opcode = OpCodes.Stelem_Ref
	};

	public static Ldelem_ Ldelem => new Ldelem_
	{
		opcode = OpCodes.Ldelem
	};

	public static Stelem_ Stelem => new Stelem_
	{
		opcode = OpCodes.Stelem
	};

	public static Unbox_Any_ Unbox_Any => new Unbox_Any_
	{
		opcode = OpCodes.Unbox_Any
	};

	public static Conv_Ovf_I1_ Conv_Ovf_I1 => new Conv_Ovf_I1_
	{
		opcode = OpCodes.Conv_Ovf_I1
	};

	public static Conv_Ovf_U1_ Conv_Ovf_U1 => new Conv_Ovf_U1_
	{
		opcode = OpCodes.Conv_Ovf_U1
	};

	public static Conv_Ovf_I2_ Conv_Ovf_I2 => new Conv_Ovf_I2_
	{
		opcode = OpCodes.Conv_Ovf_I2
	};

	public static Conv_Ovf_U2_ Conv_Ovf_U2 => new Conv_Ovf_U2_
	{
		opcode = OpCodes.Conv_Ovf_U2
	};

	public static Conv_Ovf_I4_ Conv_Ovf_I4 => new Conv_Ovf_I4_
	{
		opcode = OpCodes.Conv_Ovf_I4
	};

	public static Conv_Ovf_U4_ Conv_Ovf_U4 => new Conv_Ovf_U4_
	{
		opcode = OpCodes.Conv_Ovf_U4
	};

	public static Conv_Ovf_I8_ Conv_Ovf_I8 => new Conv_Ovf_I8_
	{
		opcode = OpCodes.Conv_Ovf_I8
	};

	public static Conv_Ovf_U8_ Conv_Ovf_U8 => new Conv_Ovf_U8_
	{
		opcode = OpCodes.Conv_Ovf_U8
	};

	public static Refanyval_ Refanyval => new Refanyval_
	{
		opcode = OpCodes.Refanyval
	};

	public static Ckfinite_ Ckfinite => new Ckfinite_
	{
		opcode = OpCodes.Ckfinite
	};

	public static Mkrefany_ Mkrefany => new Mkrefany_
	{
		opcode = OpCodes.Mkrefany
	};

	public static Ldtoken_ Ldtoken => new Ldtoken_
	{
		opcode = OpCodes.Ldtoken
	};

	public static Conv_U2_ Conv_U2 => new Conv_U2_
	{
		opcode = OpCodes.Conv_U2
	};

	public static Conv_U1_ Conv_U1 => new Conv_U1_
	{
		opcode = OpCodes.Conv_U1
	};

	public static Conv_I_ Conv_I => new Conv_I_
	{
		opcode = OpCodes.Conv_I
	};

	public static Conv_Ovf_I_ Conv_Ovf_I => new Conv_Ovf_I_
	{
		opcode = OpCodes.Conv_Ovf_I
	};

	public static Conv_Ovf_U_ Conv_Ovf_U => new Conv_Ovf_U_
	{
		opcode = OpCodes.Conv_Ovf_U
	};

	public static Add_Ovf_ Add_Ovf => new Add_Ovf_
	{
		opcode = OpCodes.Add_Ovf
	};

	public static Add_Ovf_Un_ Add_Ovf_Un => new Add_Ovf_Un_
	{
		opcode = OpCodes.Add_Ovf_Un
	};

	public static Mul_Ovf_ Mul_Ovf => new Mul_Ovf_
	{
		opcode = OpCodes.Mul_Ovf
	};

	public static Mul_Ovf_Un_ Mul_Ovf_Un => new Mul_Ovf_Un_
	{
		opcode = OpCodes.Mul_Ovf_Un
	};

	public static Sub_Ovf_ Sub_Ovf => new Sub_Ovf_
	{
		opcode = OpCodes.Sub_Ovf
	};

	public static Sub_Ovf_Un_ Sub_Ovf_Un => new Sub_Ovf_Un_
	{
		opcode = OpCodes.Sub_Ovf_Un
	};

	public static Endfinally_ Endfinally => new Endfinally_
	{
		opcode = OpCodes.Endfinally
	};

	public static Leave_ Leave => new Leave_
	{
		opcode = OpCodes.Leave
	};

	public static Leave_S_ Leave_S => new Leave_S_
	{
		opcode = OpCodes.Leave_S
	};

	public static Stind_I_ Stind_I => new Stind_I_
	{
		opcode = OpCodes.Stind_I
	};

	public static Conv_U_ Conv_U => new Conv_U_
	{
		opcode = OpCodes.Conv_U
	};

	public static Prefix7_ Prefix7 => new Prefix7_
	{
		opcode = OpCodes.Prefix7
	};

	public static Prefix6_ Prefix6 => new Prefix6_
	{
		opcode = OpCodes.Prefix6
	};

	public static Prefix5_ Prefix5 => new Prefix5_
	{
		opcode = OpCodes.Prefix5
	};

	public static Prefix4_ Prefix4 => new Prefix4_
	{
		opcode = OpCodes.Prefix4
	};

	public static Prefix3_ Prefix3 => new Prefix3_
	{
		opcode = OpCodes.Prefix3
	};

	public static Prefix2_ Prefix2 => new Prefix2_
	{
		opcode = OpCodes.Prefix2
	};

	public static Prefix1_ Prefix1 => new Prefix1_
	{
		opcode = OpCodes.Prefix1
	};

	public static Prefixref_ Prefixref => new Prefixref_
	{
		opcode = OpCodes.Prefixref
	};

	public static Arglist_ Arglist => new Arglist_
	{
		opcode = OpCodes.Arglist
	};

	public static Ceq_ Ceq => new Ceq_
	{
		opcode = OpCodes.Ceq
	};

	public static Cgt_ Cgt => new Cgt_
	{
		opcode = OpCodes.Cgt
	};

	public static Cgt_Un_ Cgt_Un => new Cgt_Un_
	{
		opcode = OpCodes.Cgt_Un
	};

	public static Clt_ Clt => new Clt_
	{
		opcode = OpCodes.Clt
	};

	public static Clt_Un_ Clt_Un => new Clt_Un_
	{
		opcode = OpCodes.Clt_Un
	};

	public static Ldftn_ Ldftn => new Ldftn_
	{
		opcode = OpCodes.Ldftn
	};

	public static Ldvirtftn_ Ldvirtftn => new Ldvirtftn_
	{
		opcode = OpCodes.Ldvirtftn
	};

	public static Ldarg_ Ldarg => new Ldarg_
	{
		opcode = OpCodes.Ldarg
	};

	public static Ldarga_ Ldarga => new Ldarga_
	{
		opcode = OpCodes.Ldarga
	};

	public static Starg_ Starg => new Starg_
	{
		opcode = OpCodes.Starg
	};

	public static Ldloc_ Ldloc => new Ldloc_
	{
		opcode = OpCodes.Ldloc
	};

	public static Ldloca_ Ldloca => new Ldloca_
	{
		opcode = OpCodes.Ldloca
	};

	public static Stloc_ Stloc => new Stloc_
	{
		opcode = OpCodes.Stloc
	};

	public static Localloc_ Localloc => new Localloc_
	{
		opcode = OpCodes.Localloc
	};

	public static Endfilter_ Endfilter => new Endfilter_
	{
		opcode = OpCodes.Endfilter
	};

	public static Unaligned_ Unaligned => new Unaligned_
	{
		opcode = OpCodes.Unaligned
	};

	public static Volatile_ Volatile => new Volatile_
	{
		opcode = OpCodes.Volatile
	};

	public static Tailcall_ Tailcall => new Tailcall_
	{
		opcode = OpCodes.Tailcall
	};

	public static Initobj_ Initobj => new Initobj_
	{
		opcode = OpCodes.Initobj
	};

	public static Constrained_ Constrained => new Constrained_
	{
		opcode = OpCodes.Constrained
	};

	public static Cpblk_ Cpblk => new Cpblk_
	{
		opcode = OpCodes.Cpblk
	};

	public static Initblk_ Initblk => new Initblk_
	{
		opcode = OpCodes.Initblk
	};

	public static Rethrow_ Rethrow => new Rethrow_
	{
		opcode = OpCodes.Rethrow
	};

	public static Sizeof_ Sizeof => new Sizeof_
	{
		opcode = OpCodes.Sizeof
	};

	public static Refanytype_ Refanytype => new Refanytype_
	{
		opcode = OpCodes.Refanytype
	};

	public static Readonly_ Readonly => new Readonly_
	{
		opcode = OpCodes.Readonly
	};
}
