using System.Xml.XPath;

namespace System.Xml.Xsl.XsltOld;

internal class VariableAction : ContainerAction, IXsltContextVariable
{
	public static object BeingComputedMark = new object();

	private const int ValueCalculated = 2;

	protected XmlQualifiedName name;

	protected string nameStr;

	protected string baseUri;

	protected int selectKey = -1;

	protected int stylesheetid;

	protected VariableType varType;

	private int varKey;

	internal int Stylesheetid => stylesheetid;

	internal XmlQualifiedName Name => name;

	internal string NameStr => nameStr;

	internal VariableType VarType => varType;

	internal int VarKey => varKey;

	internal bool IsGlobal
	{
		get
		{
			if (varType != 0)
			{
				return varType == VariableType.GlobalParameter;
			}
			return true;
		}
	}

	XPathResultType IXsltContextVariable.VariableType => XPathResultType.Any;

	bool IXsltContextVariable.IsLocal
	{
		get
		{
			if (varType != VariableType.LocalVariable)
			{
				return varType == VariableType.LocalParameter;
			}
			return true;
		}
	}

	bool IXsltContextVariable.IsParam
	{
		get
		{
			if (varType != VariableType.LocalParameter)
			{
				return varType == VariableType.GlobalParameter;
			}
			return true;
		}
	}

	internal VariableAction(VariableType type)
	{
		varType = type;
	}

	internal override void Compile(Compiler compiler)
	{
		stylesheetid = compiler.Stylesheetid;
		baseUri = compiler.Input.BaseURI;
		CompileAttributes(compiler);
		CheckRequiredAttribute(compiler, name, "name");
		if (compiler.Recurse())
		{
			CompileTemplate(compiler);
			compiler.ToParent();
			if (selectKey != -1 && containedActions != null)
			{
				throw XsltException.Create("The variable or parameter '{0}' cannot have both a 'select' attribute and non-empty content.", nameStr);
			}
		}
		if (containedActions != null)
		{
			baseUri = baseUri + "#" + compiler.GetUnicRtfId();
		}
		else
		{
			baseUri = null;
		}
		varKey = compiler.InsertVariable(this);
	}

	internal override bool CompileAttribute(Compiler compiler)
	{
		string localName = compiler.Input.LocalName;
		string value = compiler.Input.Value;
		if (Ref.Equal(localName, compiler.Atoms.Name))
		{
			nameStr = value;
			name = compiler.CreateXPathQName(nameStr);
		}
		else
		{
			if (!Ref.Equal(localName, compiler.Atoms.Select))
			{
				return false;
			}
			selectKey = compiler.AddQuery(value);
		}
		return true;
	}

	internal override void Execute(Processor processor, ActionFrame frame)
	{
		object obj = null;
		switch (frame.State)
		{
		default:
			return;
		case 0:
		{
			if (IsGlobal)
			{
				if (frame.GetVariable(varKey) != null)
				{
					frame.Finished();
					return;
				}
				frame.SetVariable(varKey, BeingComputedMark);
			}
			if (varType == VariableType.GlobalParameter)
			{
				obj = processor.GetGlobalParameter(name);
			}
			else if (varType == VariableType.LocalParameter)
			{
				obj = processor.GetParameter(name);
			}
			if (obj != null)
			{
				break;
			}
			if (selectKey != -1)
			{
				obj = processor.RunQuery(frame, selectKey);
				break;
			}
			if (containedActions == null)
			{
				obj = string.Empty;
				break;
			}
			NavigatorOutput output = new NavigatorOutput(baseUri);
			processor.PushOutput(output);
			processor.PushActionFrame(frame);
			frame.State = 1;
			return;
		}
		case 1:
			obj = ((NavigatorOutput)processor.PopOutput()).Navigator;
			break;
		case 2:
			break;
		}
		frame.SetVariable(varKey, obj);
		frame.Finished();
	}

	object IXsltContextVariable.Evaluate(XsltContext xsltContext)
	{
		return ((XsltCompileContext)xsltContext).EvaluateVariable(this);
	}
}
