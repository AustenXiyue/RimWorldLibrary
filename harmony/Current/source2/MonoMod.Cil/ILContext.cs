using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using MonoMod.Utils;

namespace MonoMod.Cil;

internal class ILContext : IDisposable
{
	public delegate void Manipulator(ILContext il);

	internal List<ILLabel> _Labels = new List<ILLabel>();

	private bool disposedValue;

	private readonly List<DataScope<DynamicReferenceCell>> managedObjectRefs = new List<DataScope<DynamicReferenceCell>>();

	public MethodDefinition Method { get; private set; }

	public ILProcessor IL { get; private set; }

	public Mono.Cecil.Cil.MethodBody Body => Method.Body;

	public ModuleDefinition Module => Method.Module;

	public Mono.Collections.Generic.Collection<Instruction> Instrs => Body.Instructions;

	public System.Collections.ObjectModel.ReadOnlyCollection<ILLabel> Labels => _Labels.AsReadOnly();

	public bool IsReadOnly => IL == null;

	public event Action? OnDispose;

	public ILContext(MethodDefinition method)
	{
		Helpers.ThrowIfArgumentNull(method, "method");
		Method = method;
		IL = method.Body.GetILProcessor();
	}

	public void Invoke(Manipulator manip)
	{
		Helpers.ThrowIfArgumentNull(manip, "manip");
		if (IsReadOnly)
		{
			throw new InvalidOperationException();
		}
		foreach (Instruction instr in Instrs)
		{
			if (instr.Operand is Instruction target)
			{
				instr.Operand = new ILLabel(this, target);
			}
			else if (instr.Operand is Instruction[] source)
			{
				instr.Operand = source.Select((Instruction t) => new ILLabel(this, t)).ToArray();
			}
		}
		manip(this);
		if (IsReadOnly)
		{
			return;
		}
		foreach (Instruction instr2 in Instrs)
		{
			if (instr2.Operand is ILLabel iLLabel)
			{
				instr2.Operand = iLLabel.Target;
			}
			else if (instr2.Operand is ILLabel[] source2)
			{
				instr2.Operand = source2.Select((ILLabel l) => l.Target).ToArray();
			}
		}
		Method.FixShortLongOps();
	}

	public void MakeReadOnly()
	{
		Method = null;
		IL = null;
		_Labels.Clear();
		_Labels.Capacity = 0;
	}

	[Obsolete("Use new ILCursor(il).Goto(index)")]
	public ILCursor At(int index)
	{
		return new ILCursor(this).Goto(index);
	}

	[Obsolete("Use new ILCursor(il).Goto(index)")]
	public ILCursor At(ILLabel label)
	{
		return new ILCursor(this).GotoLabel(label);
	}

	[Obsolete("Use new ILCursor(il).Goto(index)")]
	public ILCursor At(Instruction instr)
	{
		return new ILCursor(this).Goto(instr);
	}

	public FieldReference Import(FieldInfo field)
	{
		return Module.ImportReference(field);
	}

	public MethodReference Import(MethodBase method)
	{
		return Module.ImportReference(method);
	}

	public TypeReference Import(Type type)
	{
		return Module.ImportReference(type);
	}

	public ILLabel DefineLabel()
	{
		return new ILLabel(this);
	}

	public ILLabel DefineLabel(Instruction target)
	{
		return new ILLabel(this, target);
	}

	public int IndexOf(Instruction? instr)
	{
		if (instr == null)
		{
			return Instrs.Count;
		}
		int num = Instrs.IndexOf(instr);
		if (num != -1)
		{
			return num;
		}
		return Instrs.Count;
	}

	public IEnumerable<ILLabel> GetIncomingLabels(Instruction? instr)
	{
		return _Labels.Where((ILLabel l) => l.Target == instr);
	}

	public int AddReference<T>(in T? value)
	{
		int count = managedObjectRefs.Count;
		DynamicReferenceCell cellRef;
		DataScope<DynamicReferenceCell> item = DynamicReferenceManager.AllocReference(in value, out cellRef);
		managedObjectRefs.Add(item);
		return count;
	}

	public T? GetReference<T>(int id)
	{
		if (id < 0 || id >= managedObjectRefs.Count)
		{
			throw new ArgumentOutOfRangeException("id");
		}
		return DynamicReferenceManager.GetValue<T>(managedObjectRefs[id].Data);
	}

	public void SetReference<T>(int id, in T? value)
	{
		if (id < 0 || id >= managedObjectRefs.Count)
		{
			throw new ArgumentOutOfRangeException("id");
		}
		DynamicReferenceManager.SetValue(managedObjectRefs[id].Data, in value);
	}

	public DynamicReferenceCell GetReferenceCell(int id)
	{
		if (id < 0 || id >= managedObjectRefs.Count)
		{
			throw new ArgumentOutOfRangeException("id");
		}
		return managedObjectRefs[id].Data;
	}

	public override string ToString()
	{
		if (Method == null)
		{
			return "// ILContext: READONLY";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine($"// ILContext: {Method}");
		foreach (Instruction instr in Instrs)
		{
			ToString(stringBuilder, instr);
		}
		return stringBuilder.ToString();
	}

	internal static StringBuilder ToString(StringBuilder builder, Instruction? instr)
	{
		if (instr == null)
		{
			return builder;
		}
		object operand = instr.Operand;
		if (operand is ILLabel iLLabel)
		{
			instr.Operand = iLLabel.Target;
		}
		else if (operand is ILLabel[] source)
		{
			instr.Operand = source.Select((ILLabel l) => l.Target).ToArray();
		}
		builder.AppendLine(instr.ToString());
		instr.Operand = operand;
		return builder;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		this.OnDispose?.Invoke();
		this.OnDispose = null;
		foreach (DataScope<DynamicReferenceCell> managedObjectRef in managedObjectRefs)
		{
			managedObjectRef.Dispose();
		}
		managedObjectRefs.Clear();
		managedObjectRefs.Capacity = 0;
		MakeReadOnly();
		disposedValue = true;
	}

	~ILContext()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
