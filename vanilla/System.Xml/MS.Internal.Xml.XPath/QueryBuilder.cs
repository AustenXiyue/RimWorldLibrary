using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;

namespace MS.Internal.Xml.XPath;

internal sealed class QueryBuilder
{
	private enum Flags
	{
		None = 0,
		SmartDesc = 1,
		PosFilter = 2,
		Filter = 4
	}

	private enum Props
	{
		None = 0,
		PosFilter = 1,
		HasPosition = 2,
		HasLast = 4,
		NonFlat = 8
	}

	private string query;

	private bool allowVar;

	private bool allowKey;

	private bool allowCurrent;

	private bool needContext;

	private BaseAxisQuery firstInput;

	private int parseDepth;

	private const int MaxParseDepth = 1024;

	private void Reset()
	{
		parseDepth = 0;
		needContext = false;
	}

	private Query ProcessAxis(Axis root, Flags flags, out Props props)
	{
		Query query = null;
		if (root.Prefix.Length > 0)
		{
			needContext = true;
		}
		firstInput = null;
		Query query2;
		if (root.Input != null)
		{
			Flags flags2 = Flags.None;
			if ((flags & Flags.PosFilter) == 0)
			{
				if (root.Input is Axis axis && root.TypeOfAxis == Axis.AxisType.Child && axis.TypeOfAxis == Axis.AxisType.DescendantOrSelf && axis.NodeType == XPathNodeType.All)
				{
					Query qyParent;
					if (axis.Input != null)
					{
						qyParent = ProcessNode(axis.Input, Flags.SmartDesc, out props);
					}
					else
					{
						qyParent = new ContextQuery();
						props = Props.None;
					}
					query = new DescendantQuery(qyParent, root.Name, root.Prefix, root.NodeType, matchSelf: false, axis.AbbrAxis);
					if ((props & Props.NonFlat) != 0)
					{
						query = new DocumentOrderQuery(query);
					}
					props |= Props.NonFlat;
					return query;
				}
				if (root.TypeOfAxis == Axis.AxisType.Descendant || root.TypeOfAxis == Axis.AxisType.DescendantOrSelf)
				{
					flags2 |= Flags.SmartDesc;
				}
			}
			query2 = ProcessNode(root.Input, flags2, out props);
		}
		else
		{
			query2 = new ContextQuery();
			props = Props.None;
		}
		switch (root.TypeOfAxis)
		{
		case Axis.AxisType.Ancestor:
			query = new XPathAncestorQuery(query2, root.Name, root.Prefix, root.NodeType, matchSelf: false);
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.AncestorOrSelf:
			query = new XPathAncestorQuery(query2, root.Name, root.Prefix, root.NodeType, matchSelf: true);
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.Child:
			query = (((props & Props.NonFlat) == 0) ? new ChildrenQuery(query2, root.Name, root.Prefix, root.NodeType) : new CacheChildrenQuery(query2, root.Name, root.Prefix, root.NodeType));
			break;
		case Axis.AxisType.Parent:
			query = new ParentQuery(query2, root.Name, root.Prefix, root.NodeType);
			break;
		case Axis.AxisType.Descendant:
			if ((flags & Flags.SmartDesc) != 0)
			{
				query = new DescendantOverDescendantQuery(query2, matchSelf: false, root.Name, root.Prefix, root.NodeType, abbrAxis: false);
			}
			else
			{
				query = new DescendantQuery(query2, root.Name, root.Prefix, root.NodeType, matchSelf: false, abbrAxis: false);
				if ((props & Props.NonFlat) != 0)
				{
					query = new DocumentOrderQuery(query);
				}
			}
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.DescendantOrSelf:
			if ((flags & Flags.SmartDesc) != 0)
			{
				query = new DescendantOverDescendantQuery(query2, matchSelf: true, root.Name, root.Prefix, root.NodeType, root.AbbrAxis);
			}
			else
			{
				query = new DescendantQuery(query2, root.Name, root.Prefix, root.NodeType, matchSelf: true, root.AbbrAxis);
				if ((props & Props.NonFlat) != 0)
				{
					query = new DocumentOrderQuery(query);
				}
			}
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.Preceding:
			query = new PrecedingQuery(query2, root.Name, root.Prefix, root.NodeType);
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.Following:
			query = new FollowingQuery(query2, root.Name, root.Prefix, root.NodeType);
			props |= Props.NonFlat;
			break;
		case Axis.AxisType.FollowingSibling:
			query = new FollSiblingQuery(query2, root.Name, root.Prefix, root.NodeType);
			if ((props & Props.NonFlat) != 0)
			{
				query = new DocumentOrderQuery(query);
			}
			break;
		case Axis.AxisType.PrecedingSibling:
			query = new PreSiblingQuery(query2, root.Name, root.Prefix, root.NodeType);
			break;
		case Axis.AxisType.Attribute:
			query = new AttributeQuery(query2, root.Name, root.Prefix, root.NodeType);
			break;
		case Axis.AxisType.Self:
			query = new XPathSelfQuery(query2, root.Name, root.Prefix, root.NodeType);
			break;
		case Axis.AxisType.Namespace:
			query = (((root.NodeType != XPathNodeType.All && root.NodeType != XPathNodeType.Element && root.NodeType != XPathNodeType.Attribute) || root.Prefix.Length != 0) ? ((Query)new EmptyQuery()) : ((Query)new NamespaceQuery(query2, root.Name, root.Prefix, root.NodeType)));
			break;
		default:
			throw XPathException.Create("The XPath query '{0}' is not supported.", this.query);
		}
		return query;
	}

	private bool CanBeNumber(Query q)
	{
		if (q.StaticType != XPathResultType.Any)
		{
			return q.StaticType == XPathResultType.Number;
		}
		return true;
	}

	private Query ProcessFilter(Filter root, Flags flags, out Props props)
	{
		bool num = (flags & Flags.Filter) == 0;
		Props props2;
		Query query = ProcessNode(root.Condition, Flags.None, out props2);
		if (CanBeNumber(query) || (props2 & (Props)6) != 0)
		{
			props2 |= Props.HasPosition;
			flags |= Flags.PosFilter;
		}
		flags &= (Flags)(-2);
		Query query2 = ProcessNode(root.Input, flags | Flags.Filter, out props);
		if (root.Input.Type != AstNode.AstType.Filter)
		{
			props &= (Props)(-2);
		}
		if ((props2 & Props.HasPosition) != 0)
		{
			props |= Props.PosFilter;
		}
		if (query2 is FilterQuery filterQuery && (props2 & Props.HasPosition) == 0 && filterQuery.Condition.StaticType != XPathResultType.Any)
		{
			Query query3 = filterQuery.Condition;
			if (query3.StaticType == XPathResultType.Number)
			{
				query3 = new LogicalExpr(Operator.Op.EQ, new NodeFunctions(Function.FunctionType.FuncPosition, null), query3);
			}
			query = new BooleanExpr(Operator.Op.AND, query3, query);
			query2 = filterQuery.qyInput;
		}
		if ((props & Props.PosFilter) != 0 && query2 is DocumentOrderQuery)
		{
			query2 = ((DocumentOrderQuery)query2).input;
		}
		if (firstInput == null)
		{
			firstInput = query2 as BaseAxisQuery;
		}
		bool flag = (query2.Properties & QueryProps.Merge) != 0;
		bool flag2 = (query2.Properties & QueryProps.Reverse) != 0;
		if ((props2 & Props.HasPosition) != 0)
		{
			if (flag2)
			{
				query2 = new ReversePositionQuery(query2);
			}
			else if ((props2 & Props.HasLast) != 0)
			{
				query2 = new ForwardPositionQuery(query2);
			}
		}
		if (num && firstInput != null)
		{
			if (flag && (props & Props.PosFilter) != 0)
			{
				query2 = new FilterQuery(query2, query, noPosition: false);
				Query qyInput = firstInput.qyInput;
				if (!(qyInput is ContextQuery))
				{
					firstInput.qyInput = new ContextQuery();
					firstInput = null;
					return new MergeFilterQuery(qyInput, query2);
				}
				firstInput = null;
				return query2;
			}
			firstInput = null;
		}
		return new FilterQuery(query2, query, (props2 & Props.HasPosition) == 0);
	}

	private Query ProcessOperator(Operator root, out Props props)
	{
		Props props2;
		Query query = ProcessNode(root.Operand1, Flags.None, out props2);
		Props props3;
		Query query2 = ProcessNode(root.Operand2, Flags.None, out props3);
		props = props2 | props3;
		switch (root.OperatorType)
		{
		case Operator.Op.PLUS:
		case Operator.Op.MINUS:
		case Operator.Op.MUL:
		case Operator.Op.DIV:
		case Operator.Op.MOD:
			return new NumericExpr(root.OperatorType, query, query2);
		case Operator.Op.EQ:
		case Operator.Op.NE:
		case Operator.Op.LT:
		case Operator.Op.LE:
		case Operator.Op.GT:
		case Operator.Op.GE:
			return new LogicalExpr(root.OperatorType, query, query2);
		case Operator.Op.OR:
		case Operator.Op.AND:
			return new BooleanExpr(root.OperatorType, query, query2);
		case Operator.Op.UNION:
			props |= Props.NonFlat;
			return new UnionExpr(query, query2);
		default:
			return null;
		}
	}

	private Query ProcessVariable(Variable root)
	{
		needContext = true;
		if (!allowVar)
		{
			throw XPathException.Create("'{0}' is an invalid key pattern. It either contains a variable reference or 'key()' function.", query);
		}
		return new VariableQuery(root.Localname, root.Prefix);
	}

	private Query ProcessFunction(Function root, out Props props)
	{
		props = Props.None;
		switch (root.TypeOfFunction)
		{
		case Function.FunctionType.FuncLast:
		{
			NodeFunctions result4 = new NodeFunctions(root.TypeOfFunction, null);
			props |= Props.HasLast;
			return result4;
		}
		case Function.FunctionType.FuncPosition:
		{
			NodeFunctions result3 = new NodeFunctions(root.TypeOfFunction, null);
			props |= Props.HasPosition;
			return result3;
		}
		case Function.FunctionType.FuncCount:
			return new NodeFunctions(Function.FunctionType.FuncCount, ProcessNode((AstNode)root.ArgumentList[0], Flags.None, out props));
		case Function.FunctionType.FuncID:
		{
			IDQuery result2 = new IDQuery(ProcessNode((AstNode)root.ArgumentList[0], Flags.None, out props));
			props |= Props.NonFlat;
			return result2;
		}
		case Function.FunctionType.FuncLocalName:
		case Function.FunctionType.FuncNameSpaceUri:
		case Function.FunctionType.FuncName:
			if (root.ArgumentList != null && root.ArgumentList.Count > 0)
			{
				return new NodeFunctions(root.TypeOfFunction, ProcessNode((AstNode)root.ArgumentList[0], Flags.None, out props));
			}
			return new NodeFunctions(root.TypeOfFunction, null);
		case Function.FunctionType.FuncString:
		case Function.FunctionType.FuncConcat:
		case Function.FunctionType.FuncStartsWith:
		case Function.FunctionType.FuncContains:
		case Function.FunctionType.FuncSubstringBefore:
		case Function.FunctionType.FuncSubstringAfter:
		case Function.FunctionType.FuncSubstring:
		case Function.FunctionType.FuncStringLength:
		case Function.FunctionType.FuncNormalize:
		case Function.FunctionType.FuncTranslate:
			return new StringFunctions(root.TypeOfFunction, ProcessArguments(root.ArgumentList, out props));
		case Function.FunctionType.FuncNumber:
		case Function.FunctionType.FuncSum:
		case Function.FunctionType.FuncFloor:
		case Function.FunctionType.FuncCeiling:
		case Function.FunctionType.FuncRound:
			if (root.ArgumentList != null && root.ArgumentList.Count > 0)
			{
				return new NumberFunctions(root.TypeOfFunction, ProcessNode((AstNode)root.ArgumentList[0], Flags.None, out props));
			}
			return new NumberFunctions(Function.FunctionType.FuncNumber, null);
		case Function.FunctionType.FuncTrue:
		case Function.FunctionType.FuncFalse:
			return new BooleanFunctions(root.TypeOfFunction, null);
		case Function.FunctionType.FuncBoolean:
		case Function.FunctionType.FuncNot:
		case Function.FunctionType.FuncLang:
			return new BooleanFunctions(root.TypeOfFunction, ProcessNode((AstNode)root.ArgumentList[0], Flags.None, out props));
		case Function.FunctionType.FuncUserDefined:
		{
			needContext = true;
			if (!allowCurrent && root.Name == "current" && root.Prefix.Length == 0)
			{
				throw XPathException.Create("The 'current()' function cannot be used in a pattern.");
			}
			if (!allowKey && root.Name == "key" && root.Prefix.Length == 0)
			{
				throw XPathException.Create("'{0}' is an invalid key pattern. It either contains a variable reference or 'key()' function.", query);
			}
			FunctionQuery result = new FunctionQuery(root.Prefix, root.Name, ProcessArguments(root.ArgumentList, out props));
			props |= Props.NonFlat;
			return result;
		}
		default:
			throw XPathException.Create("The XPath query '{0}' is not supported.", query);
		}
	}

	private List<Query> ProcessArguments(ArrayList args, out Props props)
	{
		int num = args?.Count ?? 0;
		List<Query> list = new List<Query>(num);
		props = Props.None;
		for (int i = 0; i < num; i++)
		{
			list.Add(ProcessNode((AstNode)args[i], Flags.None, out var props2));
			props |= props2;
		}
		return list;
	}

	private Query ProcessNode(AstNode root, Flags flags, out Props props)
	{
		if (++parseDepth > 1024)
		{
			throw XPathException.Create("The xpath query is too complex.");
		}
		Query result = null;
		props = Props.None;
		switch (root.Type)
		{
		case AstNode.AstType.Axis:
			result = ProcessAxis((Axis)root, flags, out props);
			break;
		case AstNode.AstType.Operator:
			result = ProcessOperator((Operator)root, out props);
			break;
		case AstNode.AstType.Filter:
			result = ProcessFilter((Filter)root, flags, out props);
			break;
		case AstNode.AstType.ConstantOperand:
			result = new OperandQuery(((Operand)root).OperandValue);
			break;
		case AstNode.AstType.Variable:
			result = ProcessVariable((Variable)root);
			break;
		case AstNode.AstType.Function:
			result = ProcessFunction((Function)root, out props);
			break;
		case AstNode.AstType.Group:
			result = new GroupQuery(ProcessNode(((Group)root).GroupNode, Flags.None, out props));
			break;
		case AstNode.AstType.Root:
			result = new AbsoluteQuery();
			break;
		}
		parseDepth--;
		return result;
	}

	private Query Build(AstNode root, string query)
	{
		Reset();
		this.query = query;
		Props props;
		return ProcessNode(root, Flags.None, out props);
	}

	internal Query Build(string query, bool allowVar, bool allowKey)
	{
		this.allowVar = allowVar;
		this.allowKey = allowKey;
		allowCurrent = true;
		return Build(XPathParser.ParseXPathExpresion(query), query);
	}

	internal Query Build(string query, out bool needContext)
	{
		Query result = Build(query, allowVar: true, allowKey: true);
		needContext = this.needContext;
		return result;
	}

	internal Query BuildPatternQuery(string query, bool allowVar, bool allowKey)
	{
		this.allowVar = allowVar;
		this.allowKey = allowKey;
		allowCurrent = false;
		return Build(XPathParser.ParseXPathPattern(query), query);
	}

	internal Query BuildPatternQuery(string query, out bool needContext)
	{
		Query result = BuildPatternQuery(query, allowVar: true, allowKey: true);
		needContext = this.needContext;
		return result;
	}
}
