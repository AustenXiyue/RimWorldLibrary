using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Runtime.InteropServices;
using Unity;

namespace System.Reflection.Emit;

/// <summary>Generates Microsoft intermediate language (MSIL) instructions.</summary>
[StructLayout(LayoutKind.Sequential)]
[ComVisible(true)]
[ComDefaultInterface(typeof(_ILGenerator))]
[ClassInterface(ClassInterfaceType.None)]
public class ILGenerator : _ILGenerator
{
	private struct LabelFixup
	{
		public int offset;

		public int pos;

		public int label_idx;
	}

	private struct LabelData
	{
		public int addr;

		public int maxStack;

		public LabelData(int addr, int maxStack)
		{
			this.addr = addr;
			this.maxStack = maxStack;
		}
	}

	private byte[] code;

	private int code_len;

	private int max_stack;

	private int cur_stack;

	private LocalBuilder[] locals;

	private ILExceptionInfo[] ex_handlers;

	private int num_token_fixups;

	private ILTokenInfo[] token_fixups;

	private LabelData[] labels;

	private int num_labels;

	private LabelFixup[] fixups;

	private int num_fixups;

	internal Module module;

	private int cur_block;

	private Stack open_blocks;

	private TokenGenerator token_gen;

	private const int defaultFixupSize = 4;

	private const int defaultLabelsSize = 4;

	private const int defaultExceptionStackSize = 2;

	private ArrayList sequencePointLists;

	private SequencePointList currentSequence;

	internal bool HasDebugInfo => sequencePointLists != null;

	internal TokenGenerator TokenGenerator => token_gen;

	/// <summary>Gets the current offset, in bytes, in the Microsoft intermediate language (MSIL) stream that is being emitted by the <see cref="T:System.Reflection.Emit.ILGenerator" />.</summary>
	/// <returns>The offset in the MSIL stream at which the next instruction will be emitted. </returns>
	public virtual int ILOffset => code_len;

	internal ILGenerator(Module m, TokenGenerator token_gen, int size)
	{
		if (size < 0)
		{
			size = 128;
		}
		code = new byte[size];
		token_fixups = new ILTokenInfo[8];
		module = m;
		this.token_gen = token_gen;
	}

	private void add_token_fixup(MemberInfo mi)
	{
		if (num_token_fixups == token_fixups.Length)
		{
			ILTokenInfo[] array = new ILTokenInfo[num_token_fixups * 2];
			token_fixups.CopyTo(array, 0);
			token_fixups = array;
		}
		token_fixups[num_token_fixups].member = mi;
		token_fixups[num_token_fixups++].code_pos = code_len;
	}

	private void make_room(int nbytes)
	{
		if (code_len + nbytes >= code.Length)
		{
			byte[] destinationArray = new byte[(code_len + nbytes) * 2 + 128];
			Array.Copy(code, 0, destinationArray, 0, code.Length);
			code = destinationArray;
		}
	}

	private void emit_int(int val)
	{
		code[code_len++] = (byte)(val & 0xFF);
		code[code_len++] = (byte)((val >> 8) & 0xFF);
		code[code_len++] = (byte)((val >> 16) & 0xFF);
		code[code_len++] = (byte)((val >> 24) & 0xFF);
	}

	private void ll_emit(OpCode opcode)
	{
		if (opcode.Size == 2)
		{
			code[code_len++] = opcode.op1;
		}
		code[code_len++] = opcode.op2;
		switch (opcode.StackBehaviourPush)
		{
		case StackBehaviour.Push1:
		case StackBehaviour.Pushi:
		case StackBehaviour.Pushi8:
		case StackBehaviour.Pushr4:
		case StackBehaviour.Pushr8:
		case StackBehaviour.Pushref:
		case StackBehaviour.Varpush:
			cur_stack++;
			break;
		case StackBehaviour.Push1_push1:
			cur_stack += 2;
			break;
		}
		if (max_stack < cur_stack)
		{
			max_stack = cur_stack;
		}
		switch (opcode.StackBehaviourPop)
		{
		case StackBehaviour.Pop1:
		case StackBehaviour.Popi:
		case StackBehaviour.Popref:
			cur_stack--;
			break;
		case StackBehaviour.Pop1_pop1:
		case StackBehaviour.Popi_pop1:
		case StackBehaviour.Popi_popi:
		case StackBehaviour.Popi_popi8:
		case StackBehaviour.Popi_popr4:
		case StackBehaviour.Popi_popr8:
		case StackBehaviour.Popref_pop1:
		case StackBehaviour.Popref_popi:
			cur_stack -= 2;
			break;
		case StackBehaviour.Popi_popi_popi:
		case StackBehaviour.Popref_popi_popi:
		case StackBehaviour.Popref_popi_popi8:
		case StackBehaviour.Popref_popi_popr4:
		case StackBehaviour.Popref_popi_popr8:
		case StackBehaviour.Popref_popi_popref:
			cur_stack -= 3;
			break;
		case StackBehaviour.Push0:
		case StackBehaviour.Push1:
		case StackBehaviour.Push1_push1:
		case StackBehaviour.Pushi:
		case StackBehaviour.Pushi8:
		case StackBehaviour.Pushr4:
		case StackBehaviour.Pushr8:
		case StackBehaviour.Pushref:
		case StackBehaviour.Varpop:
			break;
		}
	}

	private static int target_len(OpCode opcode)
	{
		if (opcode.OperandType == OperandType.InlineBrTarget)
		{
			return 4;
		}
		return 1;
	}

	private void InternalEndClause()
	{
		switch (ex_handlers[cur_block].LastClauseType())
		{
		case -1:
		case 0:
		case 1:
			Emit(OpCodes.Leave, ex_handlers[cur_block].end);
			break;
		case 2:
		case 4:
			Emit(OpCodes.Endfinally);
			break;
		case 3:
			break;
		}
	}

	/// <summary>Begins a catch block.</summary>
	/// <param name="exceptionType">The <see cref="T:System.Type" /> object that represents the exception. </param>
	/// <exception cref="T:System.ArgumentException">The catch block is within a filtered exception. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="exceptionType" /> is null, and the exception filter block has not returned a value that indicates that finally blocks should be run until this catch block is located. </exception>
	/// <exception cref="T:System.NotSupportedException">The Microsoft intermediate language (MSIL) being generated is not currently in an exception block. </exception>
	public virtual void BeginCatchBlock(Type exceptionType)
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (open_blocks.Count <= 0)
		{
			throw new NotSupportedException("Not in an exception block");
		}
		if (exceptionType != null && exceptionType.IsUserType)
		{
			throw new NotSupportedException("User defined subclasses of System.Type are not yet supported.");
		}
		if (ex_handlers[cur_block].LastClauseType() == -1)
		{
			if (exceptionType != null)
			{
				throw new ArgumentException("Do not supply an exception type for filter clause");
			}
			Emit(OpCodes.Endfilter);
			ex_handlers[cur_block].PatchFilterClause(code_len);
		}
		else
		{
			InternalEndClause();
			ex_handlers[cur_block].AddCatch(exceptionType, code_len);
		}
		cur_stack = 1;
		if (max_stack < cur_stack)
		{
			max_stack = cur_stack;
		}
	}

	/// <summary>Begins an exception block for a filtered exception.</summary>
	/// <exception cref="T:System.NotSupportedException">The Microsoft intermediate language (MSIL) being generated is not currently in an exception block. -or-This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	public virtual void BeginExceptFilterBlock()
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (open_blocks.Count <= 0)
		{
			throw new NotSupportedException("Not in an exception block");
		}
		InternalEndClause();
		ex_handlers[cur_block].AddFilter(code_len);
	}

	/// <summary>Begins an exception block for a non-filtered exception.</summary>
	/// <returns>The label for the end of the block. This will leave you in the correct place to execute finally blocks or to finish the try.</returns>
	public virtual Label BeginExceptionBlock()
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (ex_handlers != null)
		{
			cur_block = ex_handlers.Length;
			ILExceptionInfo[] destinationArray = new ILExceptionInfo[cur_block + 1];
			Array.Copy(ex_handlers, destinationArray, cur_block);
			ex_handlers = destinationArray;
		}
		else
		{
			ex_handlers = new ILExceptionInfo[1];
			cur_block = 0;
		}
		open_blocks.Push(cur_block);
		ex_handlers[cur_block].start = code_len;
		return ex_handlers[cur_block].end = DefineLabel();
	}

	/// <summary>Begins an exception fault block in the Microsoft intermediate language (MSIL) stream.</summary>
	/// <exception cref="T:System.NotSupportedException">The MSIL being generated is not currently in an exception block. -or-This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	public virtual void BeginFaultBlock()
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (open_blocks.Count <= 0)
		{
			throw new NotSupportedException("Not in an exception block");
		}
		if (ex_handlers[cur_block].LastClauseType() == -1)
		{
			Emit(OpCodes.Leave, ex_handlers[cur_block].end);
			ex_handlers[cur_block].PatchFilterClause(code_len);
		}
		InternalEndClause();
		ex_handlers[cur_block].AddFault(code_len);
	}

	/// <summary>Begins a finally block in the Microsoft intermediate language (MSIL) instruction stream.</summary>
	/// <exception cref="T:System.NotSupportedException">The MSIL being generated is not currently in an exception block. </exception>
	public virtual void BeginFinallyBlock()
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (open_blocks.Count <= 0)
		{
			throw new NotSupportedException("Not in an exception block");
		}
		InternalEndClause();
		if (ex_handlers[cur_block].LastClauseType() == -1)
		{
			Emit(OpCodes.Leave, ex_handlers[cur_block].end);
			ex_handlers[cur_block].PatchFilterClause(code_len);
		}
		ex_handlers[cur_block].AddFinally(code_len);
	}

	/// <summary>Begins a lexical scope.</summary>
	/// <exception cref="T:System.NotSupportedException">This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	public virtual void BeginScope()
	{
	}

	/// <summary>Declares a local variable of the specified type.</summary>
	/// <returns>The declared local variable.</returns>
	/// <param name="localType">A <see cref="T:System.Type" /> object that represents the type of the local variable. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localType" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created by the <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method. </exception>
	public virtual LocalBuilder DeclareLocal(Type localType)
	{
		return DeclareLocal(localType, pinned: false);
	}

	/// <summary>Declares a local variable of the specified type, optionally pinning the object referred to by the variable.</summary>
	/// <returns>A <see cref="T:System.Reflection.Emit.LocalBuilder" /> object that represents the local variable.</returns>
	/// <param name="localType">A <see cref="T:System.Type" /> object that represents the type of the local variable.</param>
	/// <param name="pinned">true to pin the object in memory; otherwise, false.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localType" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The containing type has been created by the <see cref="M:System.Reflection.Emit.TypeBuilder.CreateType" /> method.-or-The method body of the enclosing method has been created by the <see cref="M:System.Reflection.Emit.MethodBuilder.CreateMethodBody(System.Byte[],System.Int32)" /> method. </exception>
	/// <exception cref="T:System.NotSupportedException">The method with which this <see cref="T:System.Reflection.Emit.ILGenerator" /> is associated is not represented by a <see cref="T:System.Reflection.Emit.MethodBuilder" />.</exception>
	public virtual LocalBuilder DeclareLocal(Type localType, bool pinned)
	{
		if (localType == null)
		{
			throw new ArgumentNullException("localType");
		}
		if (localType.IsUserType)
		{
			throw new NotSupportedException("User defined subclasses of System.Type are not yet supported.");
		}
		LocalBuilder localBuilder = new LocalBuilder(localType, this);
		localBuilder.is_pinned = pinned;
		if (locals != null)
		{
			LocalBuilder[] array = new LocalBuilder[locals.Length + 1];
			Array.Copy(locals, array, locals.Length);
			array[locals.Length] = localBuilder;
			locals = array;
		}
		else
		{
			locals = new LocalBuilder[1];
			locals[0] = localBuilder;
		}
		localBuilder.position = (ushort)(locals.Length - 1);
		return localBuilder;
	}

	/// <summary>Declares a new label.</summary>
	/// <returns>Returns a new label that can be used as a token for branching.</returns>
	public virtual Label DefineLabel()
	{
		if (labels == null)
		{
			labels = new LabelData[4];
		}
		else if (num_labels >= labels.Length)
		{
			LabelData[] destinationArray = new LabelData[labels.Length * 2];
			Array.Copy(labels, destinationArray, labels.Length);
			labels = destinationArray;
		}
		labels[num_labels] = new LabelData(-1, 0);
		return new Label(num_labels++);
	}

	/// <summary>Puts the specified instruction onto the stream of instructions.</summary>
	/// <param name="opcode">The Microsoft Intermediate Language (MSIL) instruction to be put onto the stream. </param>
	public virtual void Emit(OpCode opcode)
	{
		make_room(2);
		ll_emit(opcode);
	}

	/// <summary>Puts the specified instruction and character argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="arg">The character argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, byte arg)
	{
		make_room(3);
		ll_emit(opcode);
		code[code_len++] = arg;
	}

	/// <summary>Puts the specified instruction and metadata token for the specified constructor onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="con">A ConstructorInfo representing a constructor. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null. This exception is new in the .NET Framework 4.</exception>
	[ComVisible(true)]
	public virtual void Emit(OpCode opcode, ConstructorInfo con)
	{
		int token = token_gen.GetToken(con, create_open_instance: true);
		make_room(6);
		ll_emit(opcode);
		if (con.DeclaringType.Module == module || con is ConstructorOnTypeBuilderInst || con is ConstructorBuilder)
		{
			add_token_fixup(con);
		}
		emit_int(token);
		if (opcode.StackBehaviourPop == StackBehaviour.Varpop)
		{
			cur_stack -= con.GetParametersCount();
		}
	}

	/// <summary>Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. Defined in the OpCodes enumeration. </param>
	/// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, double arg)
	{
		byte[] bytes = BitConverter.GetBytes(arg);
		make_room(10);
		ll_emit(opcode);
		if (BitConverter.IsLittleEndian)
		{
			Array.Copy(bytes, 0, code, code_len, 8);
			code_len += 8;
			return;
		}
		code[code_len++] = bytes[7];
		code[code_len++] = bytes[6];
		code[code_len++] = bytes[5];
		code[code_len++] = bytes[4];
		code[code_len++] = bytes[3];
		code[code_len++] = bytes[2];
		code[code_len++] = bytes[1];
		code[code_len++] = bytes[0];
	}

	/// <summary>Puts the specified instruction and metadata token for the specified field onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="field">A FieldInfo representing a field. </param>
	public virtual void Emit(OpCode opcode, FieldInfo field)
	{
		int token = token_gen.GetToken(field, create_open_instance: true);
		make_room(6);
		ll_emit(opcode);
		if (field.DeclaringType.Module == module || field is FieldOnTypeBuilderInst || field is FieldBuilder)
		{
			add_token_fixup(field);
		}
		emit_int(token);
	}

	/// <summary>Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="arg">The Int argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, short arg)
	{
		make_room(4);
		ll_emit(opcode);
		code[code_len++] = (byte)(arg & 0xFF);
		code[code_len++] = (byte)((arg >> 8) & 0xFF);
	}

	/// <summary>Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, int arg)
	{
		make_room(6);
		ll_emit(opcode);
		emit_int(arg);
	}

	/// <summary>Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="arg">The numerical argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, long arg)
	{
		make_room(10);
		ll_emit(opcode);
		code[code_len++] = (byte)(arg & 0xFF);
		code[code_len++] = (byte)((arg >> 8) & 0xFF);
		code[code_len++] = (byte)((arg >> 16) & 0xFF);
		code[code_len++] = (byte)((arg >> 24) & 0xFF);
		code[code_len++] = (byte)((arg >> 32) & 0xFF);
		code[code_len++] = (byte)((arg >> 40) & 0xFF);
		code[code_len++] = (byte)((arg >> 48) & 0xFF);
		code[code_len++] = (byte)((arg >> 56) & 0xFF);
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream and leaves space to include a label when fixes are done.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="label">The label to which to branch from this location. </param>
	public virtual void Emit(OpCode opcode, Label label)
	{
		int num = target_len(opcode);
		make_room(6);
		ll_emit(opcode);
		if (cur_stack > labels[label.label].maxStack)
		{
			labels[label.label].maxStack = cur_stack;
		}
		if (fixups == null)
		{
			fixups = new LabelFixup[4];
		}
		else if (num_fixups >= fixups.Length)
		{
			LabelFixup[] destinationArray = new LabelFixup[fixups.Length * 2];
			Array.Copy(fixups, destinationArray, fixups.Length);
			fixups = destinationArray;
		}
		fixups[num_fixups].offset = num;
		fixups[num_fixups].pos = code_len;
		fixups[num_fixups].label_idx = label.label;
		num_fixups++;
		code_len += num;
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream and leaves space to include a label when fixes are done.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="labels">The array of label objects to which to branch from this location. All of the labels will be used. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="con" /> is null. This exception is new in the .NET Framework 4.</exception>
	public virtual void Emit(OpCode opcode, Label[] labels)
	{
		if (labels == null)
		{
			throw new ArgumentNullException("labels");
		}
		int num = labels.Length;
		make_room(6 + num * 4);
		ll_emit(opcode);
		for (int i = 0; i < num; i++)
		{
			if (cur_stack > this.labels[labels[i].label].maxStack)
			{
				this.labels[labels[i].label].maxStack = cur_stack;
			}
		}
		emit_int(num);
		if (fixups == null)
		{
			fixups = new LabelFixup[4 + num];
		}
		else if (num_fixups + num >= fixups.Length)
		{
			LabelFixup[] destinationArray = new LabelFixup[num + fixups.Length * 2];
			Array.Copy(fixups, destinationArray, fixups.Length);
			fixups = destinationArray;
		}
		int num2 = 0;
		int num3 = num * 4;
		while (num2 < num)
		{
			fixups[num_fixups].offset = num3;
			fixups[num_fixups].pos = code_len;
			fixups[num_fixups].label_idx = labels[num2].label;
			num_fixups++;
			code_len += 4;
			num2++;
			num3 -= 4;
		}
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the index of the given local variable.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="local">A local variable. </param>
	/// <exception cref="T:System.ArgumentException">The parent method of the <paramref name="local" /> parameter does not match the method associated with this <see cref="T:System.Reflection.Emit.ILGenerator" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="local" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="opcode" /> is a single-byte instruction, and <paramref name="local" /> represents a local variable with an index greater than Byte.MaxValue. </exception>
	public virtual void Emit(OpCode opcode, LocalBuilder local)
	{
		if (local == null)
		{
			throw new ArgumentNullException("local");
		}
		if (local.ilgen != this)
		{
			throw new ArgumentException("Trying to emit a local from a different ILGenerator.");
		}
		uint position = local.position;
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		make_room(6);
		if (opcode.StackBehaviourPop == StackBehaviour.Pop1)
		{
			cur_stack--;
			flag2 = true;
		}
		else if (opcode.StackBehaviourPush == StackBehaviour.Push1 || opcode.StackBehaviourPush == StackBehaviour.Pushi)
		{
			cur_stack++;
			flag3 = true;
			if (cur_stack > max_stack)
			{
				max_stack = cur_stack;
			}
			flag = opcode.StackBehaviourPush == StackBehaviour.Pushi;
		}
		if (flag)
		{
			if (position < 256)
			{
				code[code_len++] = 18;
				code[code_len++] = (byte)position;
				return;
			}
			code[code_len++] = 254;
			code[code_len++] = 13;
			code[code_len++] = (byte)(position & 0xFF);
			code[code_len++] = (byte)((position >> 8) & 0xFF);
		}
		else if (flag2)
		{
			if (position < 4)
			{
				code[code_len++] = (byte)(10 + position);
				return;
			}
			if (position < 256)
			{
				code[code_len++] = 19;
				code[code_len++] = (byte)position;
				return;
			}
			code[code_len++] = 254;
			code[code_len++] = 14;
			code[code_len++] = (byte)(position & 0xFF);
			code[code_len++] = (byte)((position >> 8) & 0xFF);
		}
		else if (flag3)
		{
			if (position < 4)
			{
				code[code_len++] = (byte)(6 + position);
				return;
			}
			if (position < 256)
			{
				code[code_len++] = 17;
				code[code_len++] = (byte)position;
				return;
			}
			code[code_len++] = 254;
			code[code_len++] = 12;
			code[code_len++] = (byte)(position & 0xFF);
			code[code_len++] = (byte)((position >> 8) & 0xFF);
		}
		else
		{
			ll_emit(opcode);
		}
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given method.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="meth">A MethodInfo representing a method. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="meth" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">
	///   <paramref name="meth" /> is a generic method for which the <see cref="P:System.Reflection.MethodInfo.IsGenericMethodDefinition" /> property is false.</exception>
	public virtual void Emit(OpCode opcode, MethodInfo meth)
	{
		if (meth == null)
		{
			throw new ArgumentNullException("meth");
		}
		if (meth is DynamicMethod && (opcode == OpCodes.Ldftn || opcode == OpCodes.Ldvirtftn || opcode == OpCodes.Ldtoken))
		{
			throw new ArgumentException("Ldtoken, Ldftn and Ldvirtftn OpCodes cannot target DynamicMethods.");
		}
		int token = token_gen.GetToken(meth, create_open_instance: true);
		make_room(6);
		ll_emit(opcode);
		Type declaringType = meth.DeclaringType;
		if (declaringType != null && (declaringType.Module == module || meth is MethodOnTypeBuilderInst || meth is MethodBuilder))
		{
			add_token_fixup(meth);
		}
		emit_int(token);
		if (meth.ReturnType != typeof(void))
		{
			cur_stack++;
		}
		if (opcode.StackBehaviourPop == StackBehaviour.Varpop)
		{
			cur_stack -= meth.GetParametersCount();
		}
	}

	private void Emit(OpCode opcode, MethodInfo method, int token)
	{
		make_room(6);
		ll_emit(opcode);
		Type declaringType = method.DeclaringType;
		if (declaringType != null && (declaringType.Module == module || method is MethodBuilder))
		{
			add_token_fixup(method);
		}
		emit_int(token);
		if (method.ReturnType != typeof(void))
		{
			cur_stack++;
		}
		if (opcode.StackBehaviourPop == StackBehaviour.Varpop)
		{
			cur_stack -= method.GetParametersCount();
		}
	}

	/// <summary>Puts the specified instruction and character argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="arg">The character argument pushed onto the stream immediately after the instruction. </param>
	[CLSCompliant(false)]
	public void Emit(OpCode opcode, sbyte arg)
	{
		make_room(3);
		ll_emit(opcode);
		code[code_len++] = (byte)arg;
	}

	/// <summary>Puts the specified instruction and a signature token onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="signature">A helper for constructing a signature token. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="signature" /> is null. </exception>
	public virtual void Emit(OpCode opcode, SignatureHelper signature)
	{
		int token = token_gen.GetToken(signature);
		make_room(6);
		ll_emit(opcode);
		emit_int(token);
	}

	/// <summary>Puts the specified instruction and numerical argument onto the Microsoft intermediate language (MSIL) stream of instructions.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="arg">The Single argument pushed onto the stream immediately after the instruction. </param>
	public virtual void Emit(OpCode opcode, float arg)
	{
		byte[] bytes = BitConverter.GetBytes(arg);
		make_room(6);
		ll_emit(opcode);
		if (BitConverter.IsLittleEndian)
		{
			Array.Copy(bytes, 0, code, code_len, 4);
			code_len += 4;
			return;
		}
		code[code_len++] = bytes[3];
		code[code_len++] = bytes[2];
		code[code_len++] = bytes[1];
		code[code_len++] = bytes[0];
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given string.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. </param>
	/// <param name="str">The String to be emitted. </param>
	public virtual void Emit(OpCode opcode, string str)
	{
		int token = token_gen.GetToken(str);
		make_room(6);
		ll_emit(opcode);
		emit_int(token);
	}

	/// <summary>Puts the specified instruction onto the Microsoft intermediate language (MSIL) stream followed by the metadata token for the given type.</summary>
	/// <param name="opcode">The MSIL instruction to be put onto the stream. </param>
	/// <param name="cls">A Type. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="cls" /> is null. </exception>
	public virtual void Emit(OpCode opcode, Type cls)
	{
		if (cls != null && cls.IsByRef)
		{
			throw new ArgumentException("Cannot get TypeToken for a ByRef type.");
		}
		make_room(6);
		ll_emit(opcode);
		int token = token_gen.GetToken(cls, opcode != OpCodes.Ldtoken);
		if (cls is TypeBuilderInstantiation || cls is SymbolType || cls is TypeBuilder || cls is GenericTypeParameterBuilder || cls is EnumBuilder)
		{
			add_token_fixup(cls);
		}
		emit_int(token);
	}

	/// <summary>Puts a call or callvirt instruction onto the Microsoft intermediate language (MSIL) stream to call a varargs method.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. Must be <see cref="F:System.Reflection.Emit.OpCodes.Call" />, <see cref="F:System.Reflection.Emit.OpCodes.Callvirt" />, or <see cref="F:System.Reflection.Emit.OpCodes.Newobj" />.</param>
	/// <param name="methodInfo">The varargs method to be called. </param>
	/// <param name="optionalParameterTypes">The types of the optional arguments if the method is a varargs method; otherwise, null. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="opcode" /> does not specify a method call.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="methodInfo" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The calling convention for the method is not varargs, but optional parameter types are supplied. This exception is thrown in the .NET Framework versions 1.0 and 1.1, In subsequent versions, no exception is thrown.</exception>
	[MonoLimitation("vararg methods are not supported")]
	public virtual void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
	{
		if (methodInfo == null)
		{
			throw new ArgumentNullException("methodInfo");
		}
		short value = opcode.Value;
		if (value != OpCodes.Call.Value && value != OpCodes.Callvirt.Value)
		{
			throw new NotSupportedException("Only Call and CallVirt are allowed");
		}
		if ((methodInfo.CallingConvention & CallingConventions.VarArgs) == 0)
		{
			optionalParameterTypes = null;
		}
		if (optionalParameterTypes != null)
		{
			if ((methodInfo.CallingConvention & CallingConventions.VarArgs) == 0)
			{
				throw new InvalidOperationException("Method is not VarArgs method and optional types were passed");
			}
			int token = token_gen.GetToken(methodInfo, optionalParameterTypes);
			Emit(opcode, methodInfo, token);
		}
		else
		{
			Emit(opcode, methodInfo);
		}
	}

	/// <summary>Puts a <see cref="F:System.Reflection.Emit.OpCodes.Calli" /> instruction onto the Microsoft intermediate language (MSIL) stream, specifying an unmanaged calling convention for the indirect call.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. Must be <see cref="F:System.Reflection.Emit.OpCodes.Calli" />.</param>
	/// <param name="unmanagedCallConv">The unmanaged calling convention to be used. </param>
	/// <param name="returnType">The <see cref="T:System.Type" /> of the result. </param>
	/// <param name="parameterTypes">The types of the required arguments to the instruction. </param>
	public virtual void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
	{
		SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(module as ModuleBuilder, (CallingConventions)0, unmanagedCallConv, returnType, parameterTypes);
		Emit(opcode, methodSigHelper);
	}

	/// <summary>Puts a <see cref="F:System.Reflection.Emit.OpCodes.Calli" /> instruction onto the Microsoft intermediate language (MSIL) stream, specifying a managed calling convention for the indirect call.</summary>
	/// <param name="opcode">The MSIL instruction to be emitted onto the stream. Must be <see cref="F:System.Reflection.Emit.OpCodes.Calli" />. </param>
	/// <param name="callingConvention">The managed calling convention to be used. </param>
	/// <param name="returnType">The <see cref="T:System.Type" /> of the result. </param>
	/// <param name="parameterTypes">The types of the required arguments to the instruction. </param>
	/// <param name="optionalParameterTypes">The types of the optional arguments for varargs calls. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <paramref name="optionalParameterTypes" /> is not null, but <paramref name="callingConvention" /> does not include the <see cref="F:System.Reflection.CallingConventions.VarArgs" /> flag.</exception>
	public virtual void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
	{
		if (optionalParameterTypes != null)
		{
			throw new NotImplementedException();
		}
		SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(module as ModuleBuilder, callingConvention, (CallingConvention)0, returnType, parameterTypes);
		Emit(opcode, methodSigHelper);
	}

	/// <summary>Emits the Microsoft intermediate language (MSIL) necessary to call <see cref="Overload:System.Console.WriteLine" /> with the given field.</summary>
	/// <param name="fld">The field whose value is to be written to the console. </param>
	/// <exception cref="T:System.ArgumentException">There is no overload of the <see cref="Overload:System.Console.WriteLine" /> method that accepts the type of the specified field. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="fld" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">The type of the field is <see cref="T:System.Reflection.Emit.TypeBuilder" /> or <see cref="T:System.Reflection.Emit.EnumBuilder" />, which are not supported. </exception>
	public virtual void EmitWriteLine(FieldInfo fld)
	{
		if (fld == null)
		{
			throw new ArgumentNullException("fld");
		}
		if (fld.IsStatic)
		{
			Emit(OpCodes.Ldsfld, fld);
		}
		else
		{
			Emit(OpCodes.Ldarg_0);
			Emit(OpCodes.Ldfld, fld);
		}
		Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[1] { fld.FieldType }));
	}

	/// <summary>Emits the Microsoft intermediate language (MSIL) necessary to call <see cref="Overload:System.Console.WriteLine" /> with the given local variable.</summary>
	/// <param name="localBuilder">The local variable whose value is to be written to the console. </param>
	/// <exception cref="T:System.ArgumentException">The type of <paramref name="localBuilder" /> is <see cref="T:System.Reflection.Emit.TypeBuilder" /> or <see cref="T:System.Reflection.Emit.EnumBuilder" />, which are not supported. -or-There is no overload of <see cref="Overload:System.Console.WriteLine" /> that accepts the type of <paramref name="localBuilder" />. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="localBuilder" /> is null. </exception>
	public virtual void EmitWriteLine(LocalBuilder localBuilder)
	{
		if (localBuilder == null)
		{
			throw new ArgumentNullException("localBuilder");
		}
		if (localBuilder.LocalType is TypeBuilder)
		{
			throw new ArgumentException("Output streams do not support TypeBuilders.");
		}
		Emit(OpCodes.Ldloc, localBuilder);
		Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[1] { localBuilder.LocalType }));
	}

	/// <summary>Emits the Microsoft intermediate language (MSIL) to call <see cref="Overload:System.Console.WriteLine" /> with a string.</summary>
	/// <param name="value">The string to be printed. </param>
	public virtual void EmitWriteLine(string value)
	{
		Emit(OpCodes.Ldstr, value);
		Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", new Type[1] { typeof(string) }));
	}

	/// <summary>Ends an exception block.</summary>
	/// <exception cref="T:System.InvalidOperationException">The end exception block occurs in an unexpected place in the code stream. </exception>
	/// <exception cref="T:System.NotSupportedException">The Microsoft intermediate language (MSIL) being generated is not currently in an exception block. </exception>
	public virtual void EndExceptionBlock()
	{
		if (open_blocks == null)
		{
			open_blocks = new Stack(2);
		}
		if (open_blocks.Count <= 0)
		{
			throw new NotSupportedException("Not in an exception block");
		}
		if (ex_handlers[cur_block].LastClauseType() == -1)
		{
			throw new InvalidOperationException("Incorrect code generation for exception block.");
		}
		InternalEndClause();
		MarkLabel(ex_handlers[cur_block].end);
		ex_handlers[cur_block].End(code_len);
		ex_handlers[cur_block].Debug(cur_block);
		open_blocks.Pop();
		if (open_blocks.Count > 0)
		{
			cur_block = (int)open_blocks.Peek();
		}
	}

	/// <summary>Ends a lexical scope.</summary>
	/// <exception cref="T:System.NotSupportedException">This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	public virtual void EndScope()
	{
	}

	/// <summary>Marks the Microsoft intermediate language (MSIL) stream's current position with the given label.</summary>
	/// <param name="loc">The label for which to set an index. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="loc" /> represents an invalid index into the label array.-or- An index for <paramref name="loc" /> has already been defined. </exception>
	public virtual void MarkLabel(Label loc)
	{
		if (loc.label < 0 || loc.label >= num_labels)
		{
			throw new ArgumentException("The label is not valid");
		}
		if (labels[loc.label].addr >= 0)
		{
			throw new ArgumentException("The label was already defined");
		}
		labels[loc.label].addr = code_len;
		if (labels[loc.label].maxStack > cur_stack)
		{
			cur_stack = labels[loc.label].maxStack;
		}
	}

	/// <summary>Marks a sequence point in the Microsoft intermediate language (MSIL) stream.</summary>
	/// <param name="document">The document for which the sequence point is being defined. </param>
	/// <param name="startLine">The line where the sequence point begins. </param>
	/// <param name="startColumn">The column in the line where the sequence point begins. </param>
	/// <param name="endLine">The line where the sequence point ends. </param>
	/// <param name="endColumn">The column in the line where the sequence point ends. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="startLine" /> or <paramref name="endLine" /> is &lt;= 0. </exception>
	/// <exception cref="T:System.NotSupportedException">This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	public virtual void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
	{
		if (currentSequence == null || currentSequence.Document != document)
		{
			if (sequencePointLists == null)
			{
				sequencePointLists = new ArrayList();
			}
			currentSequence = new SequencePointList(document);
			sequencePointLists.Add(currentSequence);
		}
		currentSequence.AddSequencePoint(code_len, startLine, startColumn, endLine, endColumn);
	}

	internal void GenerateDebugInfo(ISymbolWriter symbolWriter)
	{
		if (sequencePointLists == null)
		{
			return;
		}
		SequencePointList sequencePointList = (SequencePointList)sequencePointLists[0];
		SequencePointList sequencePointList2 = (SequencePointList)sequencePointLists[sequencePointLists.Count - 1];
		symbolWriter.SetMethodSourceRange(sequencePointList.Document, sequencePointList.StartLine, sequencePointList.StartColumn, sequencePointList2.Document, sequencePointList2.EndLine, sequencePointList2.EndColumn);
		foreach (SequencePointList sequencePointList3 in sequencePointLists)
		{
			symbolWriter.DefineSequencePoints(sequencePointList3.Document, sequencePointList3.GetOffsets(), sequencePointList3.GetLines(), sequencePointList3.GetColumns(), sequencePointList3.GetEndLines(), sequencePointList3.GetEndColumns());
		}
		if (locals != null)
		{
			LocalBuilder[] array = locals;
			foreach (LocalBuilder localBuilder in array)
			{
				if (localBuilder.Name != null && localBuilder.Name.Length > 0)
				{
					SignatureHelper localVarSigHelper = SignatureHelper.GetLocalVarSigHelper(module as ModuleBuilder);
					localVarSigHelper.AddArgument(localBuilder.LocalType);
					byte[] signature = localVarSigHelper.GetSignature();
					symbolWriter.DefineLocalVariable(localBuilder.Name, FieldAttributes.Public, signature, SymAddressKind.ILOffset, localBuilder.position, 0, 0, localBuilder.StartOffset, localBuilder.EndOffset);
				}
			}
		}
		sequencePointLists = null;
	}

	/// <summary>Emits an instruction to throw an exception.</summary>
	/// <param name="excType">The class of the type of exception to throw. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="excType" /> is not the <see cref="T:System.Exception" /> class or a derived class of <see cref="T:System.Exception" />.-or- The type does not have a default constructor. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="excType" /> is null. </exception>
	public virtual void ThrowException(Type excType)
	{
		if (excType == null)
		{
			throw new ArgumentNullException("excType");
		}
		if (!(excType == typeof(Exception)) && !excType.IsSubclassOf(typeof(Exception)))
		{
			throw new ArgumentException("Type should be an exception type", "excType");
		}
		ConstructorInfo constructor = excType.GetConstructor(Type.EmptyTypes);
		if (constructor == null)
		{
			throw new ArgumentException("Type should have a default constructor", "excType");
		}
		Emit(OpCodes.Newobj, constructor);
		Emit(OpCodes.Throw);
	}

	/// <summary>Specifies the namespace to be used in evaluating locals and watches for the current active lexical scope.</summary>
	/// <param name="usingNamespace">The namespace to be used in evaluating locals and watches for the current active lexical scope </param>
	/// <exception cref="T:System.ArgumentException">Length of <paramref name="usingNamespace" /> is zero. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="usingNamespace" /> is null. </exception>
	/// <exception cref="T:System.NotSupportedException">This <see cref="T:System.Reflection.Emit.ILGenerator" /> belongs to a <see cref="T:System.Reflection.Emit.DynamicMethod" />.</exception>
	[MonoTODO("Not implemented")]
	public virtual void UsingNamespace(string usingNamespace)
	{
		throw new NotImplementedException();
	}

	internal void label_fixup(MethodBase mb)
	{
		for (int i = 0; i < num_fixups; i++)
		{
			if (labels[fixups[i].label_idx].addr < 0)
			{
				throw new ArgumentException($"Label #{fixups[i].label_idx + 1} is not marked in method `{mb.Name}'");
			}
			int num = labels[fixups[i].label_idx].addr - (fixups[i].pos + fixups[i].offset);
			if (fixups[i].offset == 1)
			{
				code[fixups[i].pos] = (byte)(sbyte)num;
				continue;
			}
			int num2 = code_len;
			code_len = fixups[i].pos;
			emit_int(num);
			code_len = num2;
		}
	}

	internal void FixupTokens(Dictionary<int, int> token_map, Dictionary<int, MemberInfo> member_map)
	{
		for (int i = 0; i < num_token_fixups; i++)
		{
			int code_pos = token_fixups[i].code_pos;
			int key = code[code_pos] | (code[code_pos + 1] << 8) | (code[code_pos + 2] << 16) | (code[code_pos + 3] << 24);
			if (token_map.TryGetValue(key, out var value))
			{
				token_fixups[i].member = member_map[key];
				int num = code_len;
				code_len = code_pos;
				emit_int(value);
				code_len = num;
			}
		}
	}

	internal void SetExceptionHandlers(ILExceptionInfo[] exHandlers)
	{
		ex_handlers = exHandlers;
	}

	internal void SetTokenFixups(ILTokenInfo[] tokenFixups)
	{
		token_fixups = tokenFixups;
	}

	internal void SetCode(byte[] code, int max_stack)
	{
		this.code = (byte[])code.Clone();
		code_len = code.Length;
		this.max_stack = max_stack;
		cur_stack = 0;
	}

	internal unsafe void SetCode(byte* code, int code_size, int max_stack)
	{
		this.code = new byte[code_size];
		for (int i = 0; i < code_size; i++)
		{
			this.code[i] = code[i];
		}
		code_len = code_size;
		this.max_stack = max_stack;
		cur_stack = 0;
	}

	internal void Init(byte[] il, int maxStack, byte[] localSignature, IEnumerable<ExceptionHandler> exceptionHandlers, IEnumerable<int> tokenFixups)
	{
		SetCode(il, maxStack);
		if (exceptionHandlers != null)
		{
			Dictionary<Tuple<int, int>, List<ExceptionHandler>> dictionary = new Dictionary<Tuple<int, int>, List<ExceptionHandler>>();
			foreach (ExceptionHandler exceptionHandler in exceptionHandlers)
			{
				Tuple<int, int> key = new Tuple<int, int>(exceptionHandler.TryOffset, exceptionHandler.TryLength);
				if (!dictionary.TryGetValue(key, out var value))
				{
					value = new List<ExceptionHandler>();
					dictionary.Add(key, value);
				}
				value.Add(exceptionHandler);
			}
			List<ILExceptionInfo> list = new List<ILExceptionInfo>();
			foreach (KeyValuePair<Tuple<int, int>, List<ExceptionHandler>> item3 in dictionary)
			{
				ILExceptionInfo iLExceptionInfo = default(ILExceptionInfo);
				iLExceptionInfo.start = item3.Key.Item1;
				iLExceptionInfo.len = item3.Key.Item2;
				iLExceptionInfo.handlers = new ILExceptionBlock[item3.Value.Count];
				ILExceptionInfo item = iLExceptionInfo;
				list.Add(item);
				int num = 0;
				foreach (ExceptionHandler item4 in item3.Value)
				{
					item.handlers[num++] = new ILExceptionBlock
					{
						start = item4.HandlerOffset,
						len = item4.HandlerLength,
						filter_offset = item4.FilterOffset,
						type = (int)item4.Kind,
						extype = module.ResolveType(item4.ExceptionTypeToken)
					};
				}
			}
			SetExceptionHandlers(list.ToArray());
		}
		if (tokenFixups == null)
		{
			return;
		}
		List<ILTokenInfo> list2 = new List<ILTokenInfo>();
		foreach (int tokenFixup in tokenFixups)
		{
			int metadataToken = (int)BitConverter.ToUInt32(il, tokenFixup);
			ILTokenInfo iLTokenInfo = default(ILTokenInfo);
			iLTokenInfo.code_pos = tokenFixup;
			iLTokenInfo.member = ((ModuleBuilder)module).ResolveOrGetRegisteredToken(metadataToken, null, null);
			ILTokenInfo item2 = iLTokenInfo;
			list2.Add(item2);
		}
		SetTokenFixups(list2.ToArray());
	}

	[Obsolete("Use ILOffset", true)]
	internal static int Mono_GetCurrentOffset(ILGenerator ig)
	{
		return ig.code_len;
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array that receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ILGenerator.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ILGenerator.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ILGenerator.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">The method is called late-bound using the COM IDispatch interface.</exception>
	void _ILGenerator.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	internal ILGenerator()
	{
		ThrowStub.ThrowNotSupportedException();
	}
}
