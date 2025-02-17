using System.Xml.XPath;

namespace System.Xml.Xsl.XsltOld;

internal class ForEachAction : ContainerAction
{
	private const int ProcessedSort = 2;

	private const int ProcessNextNode = 3;

	private const int PositionAdvanced = 4;

	private const int ContentsProcessed = 5;

	private int selectKey = -1;

	private ContainerAction sortContainer;

	internal override void Compile(Compiler compiler)
	{
		CompileAttributes(compiler);
		CheckRequiredAttribute(compiler, selectKey != -1, "select");
		compiler.CanHaveApplyImports = false;
		if (compiler.Recurse())
		{
			CompileSortElements(compiler);
			CompileTemplate(compiler);
			compiler.ToParent();
		}
	}

	internal override bool CompileAttribute(Compiler compiler)
	{
		string localName = compiler.Input.LocalName;
		string value = compiler.Input.Value;
		if (Ref.Equal(localName, compiler.Atoms.Select))
		{
			selectKey = compiler.AddQuery(value);
			return true;
		}
		return false;
	}

	internal override void Execute(Processor processor, ActionFrame frame)
	{
		switch (frame.State)
		{
		default:
			return;
		case 0:
			if (sortContainer != null)
			{
				processor.InitSortArray();
				processor.PushActionFrame(sortContainer, frame.NodeSet);
				frame.State = 2;
				return;
			}
			goto case 2;
		case 2:
			frame.InitNewNodeSet(processor.StartQuery(frame.NodeSet, selectKey));
			if (sortContainer != null)
			{
				frame.SortNewNodeSet(processor, processor.SortArray);
			}
			frame.State = 3;
			goto case 3;
		case 3:
			if (frame.NewNextNode(processor))
			{
				frame.State = 4;
				break;
			}
			frame.Finished();
			return;
		case 4:
			break;
		case 5:
			frame.State = 3;
			goto case 3;
		case 1:
			return;
		}
		processor.PushActionFrame(frame, frame.NewNodeSet);
		frame.State = 5;
	}

	protected void CompileSortElements(Compiler compiler)
	{
		NavigatorInput input = compiler.Input;
		do
		{
			switch (input.NodeType)
			{
			case XPathNodeType.Element:
				if (Ref.Equal(input.NamespaceURI, input.Atoms.UriXsl) && Ref.Equal(input.LocalName, input.Atoms.Sort))
				{
					if (sortContainer == null)
					{
						sortContainer = new ContainerAction();
					}
					sortContainer.AddAction(compiler.CreateSortAction());
					break;
				}
				return;
			case XPathNodeType.Text:
				return;
			case XPathNodeType.SignificantWhitespace:
				AddEvent(compiler.CreateTextEvent());
				break;
			}
		}
		while (input.Advance());
	}
}
