using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace HarmonyLib;

public class CodeMatcher
{
	private delegate CodeMatcher MatchDelegate();

	private readonly ILGenerator generator;

	private readonly List<CodeInstruction> codes = new List<CodeInstruction>();

	private Dictionary<string, CodeInstruction> lastMatches = new Dictionary<string, CodeInstruction>();

	private string lastError;

	private MatchDelegate lastMatchCall;

	public int Pos { get; private set; } = -1;

	public int Length => codes.Count;

	public bool IsValid
	{
		get
		{
			if (Pos >= 0)
			{
				return Pos < Length;
			}
			return false;
		}
	}

	public bool IsInvalid
	{
		get
		{
			if (Pos >= 0)
			{
				return Pos >= Length;
			}
			return true;
		}
	}

	public int Remaining => Length - Math.Max(0, Pos);

	public ref OpCode Opcode => ref codes[Pos].opcode;

	public ref object Operand => ref codes[Pos].operand;

	public ref List<Label> Labels => ref codes[Pos].labels;

	public ref List<ExceptionBlock> Blocks => ref codes[Pos].blocks;

	public CodeInstruction Instruction => codes[Pos];

	private void FixStart()
	{
		Pos = Math.Max(0, Pos);
	}

	private void SetOutOfBounds(int direction)
	{
		Pos = ((direction > 0) ? Length : (-1));
	}

	public CodeMatcher()
	{
	}

	public CodeMatcher(IEnumerable<CodeInstruction> instructions, ILGenerator generator = null)
	{
		this.generator = generator;
		codes = instructions.Select((CodeInstruction c) => new CodeInstruction(c)).ToList();
	}

	public CodeMatcher Clone()
	{
		return new CodeMatcher(codes, generator)
		{
			Pos = Pos,
			lastMatches = lastMatches,
			lastError = lastError,
			lastMatchCall = lastMatchCall
		};
	}

	public CodeInstruction InstructionAt(int offset)
	{
		return codes[Pos + offset];
	}

	public List<CodeInstruction> Instructions()
	{
		return codes;
	}

	public IEnumerable<CodeInstruction> InstructionEnumeration()
	{
		return codes.AsEnumerable();
	}

	public List<CodeInstruction> Instructions(int count)
	{
		return (from c in codes.GetRange(Pos, count)
			select new CodeInstruction(c)).ToList();
	}

	public List<CodeInstruction> InstructionsInRange(int start, int end)
	{
		List<CodeInstruction> list = codes;
		if (start > end)
		{
			int num = start;
			start = end;
			end = num;
		}
		list = list.GetRange(start, end - start + 1);
		return list.Select((CodeInstruction c) => new CodeInstruction(c)).ToList();
	}

	public List<CodeInstruction> InstructionsWithOffsets(int startOffset, int endOffset)
	{
		return InstructionsInRange(Pos + startOffset, Pos + endOffset);
	}

	public List<Label> DistinctLabels(IEnumerable<CodeInstruction> instructions)
	{
		return instructions.SelectMany((CodeInstruction instruction) => instruction.labels).Distinct().ToList();
	}

	public bool ReportFailure(MethodBase method, Action<string> logger)
	{
		if (IsValid)
		{
			return false;
		}
		string value = lastError ?? "Unexpected code";
		logger($"{value} in {method}");
		return true;
	}

	public CodeMatcher ThrowIfInvalid(string explanation)
	{
		if (explanation == null)
		{
			throw new ArgumentNullException("explanation");
		}
		if (IsInvalid)
		{
			throw new InvalidOperationException(explanation + " - Current state is invalid");
		}
		return this;
	}

	public CodeMatcher ThrowIfNotMatch(string explanation, params CodeMatch[] matches)
	{
		ThrowIfInvalid(explanation);
		if (!MatchSequence(Pos, matches))
		{
			throw new InvalidOperationException(explanation + " - Match failed");
		}
		return this;
	}

	private void ThrowIfNotMatch(string explanation, int direction, CodeMatch[] matches)
	{
		ThrowIfInvalid(explanation);
		int pos = Pos;
		try
		{
			if (Match(matches, direction, useEnd: false).IsInvalid)
			{
				throw new InvalidOperationException(explanation + " - Match failed");
			}
		}
		finally
		{
			Pos = pos;
		}
	}

	public CodeMatcher ThrowIfNotMatchForward(string explanation, params CodeMatch[] matches)
	{
		ThrowIfNotMatch(explanation, 1, matches);
		return this;
	}

	public CodeMatcher ThrowIfNotMatchBack(string explanation, params CodeMatch[] matches)
	{
		ThrowIfNotMatch(explanation, -1, matches);
		return this;
	}

	public CodeMatcher ThrowIfFalse(string explanation, Func<CodeMatcher, bool> stateCheckFunc)
	{
		if (stateCheckFunc == null)
		{
			throw new ArgumentNullException("stateCheckFunc");
		}
		ThrowIfInvalid(explanation);
		if (!stateCheckFunc(this))
		{
			throw new InvalidOperationException(explanation + " - Check function returned false");
		}
		return this;
	}

	public CodeMatcher SetInstruction(CodeInstruction instruction)
	{
		codes[Pos] = instruction;
		return this;
	}

	public CodeMatcher SetInstructionAndAdvance(CodeInstruction instruction)
	{
		SetInstruction(instruction);
		Pos++;
		return this;
	}

	public CodeMatcher Set(OpCode opcode, object operand)
	{
		Opcode = opcode;
		Operand = operand;
		return this;
	}

	public CodeMatcher SetAndAdvance(OpCode opcode, object operand)
	{
		Set(opcode, operand);
		Pos++;
		return this;
	}

	public CodeMatcher SetOpcodeAndAdvance(OpCode opcode)
	{
		Opcode = opcode;
		Pos++;
		return this;
	}

	public CodeMatcher SetOperandAndAdvance(object operand)
	{
		Operand = operand;
		Pos++;
		return this;
	}

	public CodeMatcher DeclareLocal(Type variableType, out LocalBuilder localVariable)
	{
		localVariable = generator.DeclareLocal(variableType);
		return this;
	}

	public CodeMatcher DefineLabel(out Label label)
	{
		label = generator.DefineLabel();
		return this;
	}

	public CodeMatcher CreateLabel(out Label label)
	{
		label = generator.DefineLabel();
		Labels.Add(label);
		return this;
	}

	public CodeMatcher CreateLabelAt(int position, out Label label)
	{
		label = generator.DefineLabel();
		AddLabelsAt(position, new Label[1] { label });
		return this;
	}

	public CodeMatcher CreateLabelWithOffsets(int offset, out Label label)
	{
		label = generator.DefineLabel();
		return AddLabelsAt(Pos + offset, new Label[1] { label });
	}

	public CodeMatcher AddLabels(IEnumerable<Label> labels)
	{
		Labels.AddRange(labels);
		return this;
	}

	public CodeMatcher AddLabelsAt(int position, IEnumerable<Label> labels)
	{
		codes[position].labels.AddRange(labels);
		return this;
	}

	public CodeMatcher SetJumpTo(OpCode opcode, int destination, out Label label)
	{
		CreateLabelAt(destination, out label);
		return Set(opcode, label);
	}

	public CodeMatcher Insert(params CodeInstruction[] instructions)
	{
		codes.InsertRange(Pos, instructions);
		return this;
	}

	public CodeMatcher Insert(IEnumerable<CodeInstruction> instructions)
	{
		codes.InsertRange(Pos, instructions);
		return this;
	}

	public CodeMatcher InsertBranch(OpCode opcode, int destination)
	{
		CreateLabelAt(destination, out var label);
		codes.Insert(Pos, new CodeInstruction(opcode, label));
		return this;
	}

	public CodeMatcher InsertAndAdvance(params CodeInstruction[] instructions)
	{
		foreach (CodeInstruction codeInstruction in instructions)
		{
			Insert(codeInstruction);
			Pos++;
		}
		return this;
	}

	public CodeMatcher InsertAndAdvance(IEnumerable<CodeInstruction> instructions)
	{
		foreach (CodeInstruction instruction in instructions)
		{
			InsertAndAdvance(instruction);
		}
		return this;
	}

	public CodeMatcher InsertBranchAndAdvance(OpCode opcode, int destination)
	{
		InsertBranch(opcode, destination);
		Pos++;
		return this;
	}

	public CodeMatcher RemoveInstruction()
	{
		codes.RemoveAt(Pos);
		return this;
	}

	public CodeMatcher RemoveInstructions(int count)
	{
		codes.RemoveRange(Pos, count);
		return this;
	}

	public CodeMatcher RemoveInstructionsInRange(int start, int end)
	{
		if (start > end)
		{
			int num = start;
			start = end;
			end = num;
		}
		codes.RemoveRange(start, end - start + 1);
		return this;
	}

	public CodeMatcher RemoveInstructionsWithOffsets(int startOffset, int endOffset)
	{
		return RemoveInstructionsInRange(Pos + startOffset, Pos + endOffset);
	}

	public CodeMatcher Advance(int offset)
	{
		Pos += offset;
		if (!IsValid)
		{
			SetOutOfBounds(offset);
		}
		return this;
	}

	public CodeMatcher Start()
	{
		Pos = 0;
		return this;
	}

	public CodeMatcher End()
	{
		Pos = Length - 1;
		return this;
	}

	public CodeMatcher SearchForward(Func<CodeInstruction, bool> predicate)
	{
		return Search(predicate, 1);
	}

	public CodeMatcher SearchBackwards(Func<CodeInstruction, bool> predicate)
	{
		return Search(predicate, -1);
	}

	private CodeMatcher Search(Func<CodeInstruction, bool> predicate, int direction)
	{
		FixStart();
		while (IsValid && !predicate(Instruction))
		{
			Pos += direction;
		}
		lastError = (IsInvalid ? $"Cannot find {predicate}" : null);
		return this;
	}

	public CodeMatcher MatchStartForward(params CodeMatch[] matches)
	{
		return Match(matches, 1, useEnd: false);
	}

	public CodeMatcher MatchEndForward(params CodeMatch[] matches)
	{
		return Match(matches, 1, useEnd: true);
	}

	public CodeMatcher MatchStartBackwards(params CodeMatch[] matches)
	{
		return Match(matches, -1, useEnd: false);
	}

	public CodeMatcher MatchEndBackwards(params CodeMatch[] matches)
	{
		return Match(matches, -1, useEnd: true);
	}

	private CodeMatcher Match(CodeMatch[] matches, int direction, bool useEnd)
	{
		lastMatchCall = delegate
		{
			FixStart();
			while (IsValid)
			{
				if (MatchSequence(Pos, matches))
				{
					if (useEnd)
					{
						Pos += matches.Length - 1;
					}
					break;
				}
				Pos += direction;
			}
			lastError = (IsInvalid ? ("Cannot find " + matches.Join()) : null);
			return this;
		};
		return lastMatchCall();
	}

	public CodeMatcher Repeat(Action<CodeMatcher> matchAction, Action<string> notFoundAction = null)
	{
		int num = 0;
		if (lastMatchCall == null)
		{
			throw new InvalidOperationException("No previous Match operation - cannot repeat");
		}
		while (IsValid)
		{
			matchAction(this);
			lastMatchCall();
			num++;
		}
		lastMatchCall = null;
		if (num == 0)
		{
			notFoundAction?.Invoke(lastError);
		}
		return this;
	}

	public CodeInstruction NamedMatch(string name)
	{
		return lastMatches[name];
	}

	private bool MatchSequence(int start, CodeMatch[] matches)
	{
		if (start < 0)
		{
			return false;
		}
		lastMatches = new Dictionary<string, CodeInstruction>();
		foreach (CodeMatch codeMatch in matches)
		{
			if (start >= Length || !codeMatch.Matches(codes, codes[start]))
			{
				return false;
			}
			if (codeMatch.name != null)
			{
				lastMatches.Add(codeMatch.name, codes[start]);
			}
			start++;
		}
		return true;
	}
}
