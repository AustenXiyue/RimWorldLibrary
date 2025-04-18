using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace System.Dynamic;

/// <summary>Represents a set of binding restrictions on the <see cref="T:System.Dynamic.DynamicMetaObject" /> under which the dynamic binding is valid.</summary>
[DebuggerTypeProxy(typeof(BindingRestrictionsProxy))]
[DebuggerDisplay("{DebugView}")]
public abstract class BindingRestrictions
{
	private sealed class TestBuilder
	{
		private struct AndNode
		{
			internal int Depth;

			internal Expression Node;
		}

		private readonly HashSet<BindingRestrictions> _unique = new HashSet<BindingRestrictions>();

		private readonly Stack<AndNode> _tests = new Stack<AndNode>();

		internal void Append(BindingRestrictions restrictions)
		{
			if (_unique.Add(restrictions))
			{
				Push(restrictions.GetExpression(), 0);
			}
		}

		internal Expression ToExpression()
		{
			Expression expression = _tests.Pop().Node;
			while (_tests.Count > 0)
			{
				expression = Expression.AndAlso(_tests.Pop().Node, expression);
			}
			return expression;
		}

		private void Push(Expression node, int depth)
		{
			while (_tests.Count > 0 && _tests.Peek().Depth == depth)
			{
				node = Expression.AndAlso(_tests.Pop().Node, node);
				depth++;
			}
			_tests.Push(new AndNode
			{
				Node = node,
				Depth = depth
			});
		}
	}

	private sealed class MergedRestriction : BindingRestrictions
	{
		internal readonly BindingRestrictions Left;

		internal readonly BindingRestrictions Right;

		internal MergedRestriction(BindingRestrictions left, BindingRestrictions right)
		{
			Left = left;
			Right = right;
		}

		internal override Expression GetExpression()
		{
			TestBuilder testBuilder = new TestBuilder();
			Stack<BindingRestrictions> stack = new Stack<BindingRestrictions>();
			BindingRestrictions bindingRestrictions = this;
			while (true)
			{
				if (bindingRestrictions is MergedRestriction mergedRestriction)
				{
					stack.Push(mergedRestriction.Right);
					bindingRestrictions = mergedRestriction.Left;
					continue;
				}
				testBuilder.Append(bindingRestrictions);
				if (stack.Count == 0)
				{
					break;
				}
				bindingRestrictions = stack.Pop();
			}
			return testBuilder.ToExpression();
		}
	}

	private sealed class CustomRestriction : BindingRestrictions
	{
		private readonly Expression _expression;

		internal CustomRestriction(Expression expression)
		{
			_expression = expression;
		}

		public override bool Equals(object obj)
		{
			return (obj as CustomRestriction)?._expression == _expression;
		}

		public override int GetHashCode()
		{
			return 0x24924924 ^ _expression.GetHashCode();
		}

		internal override Expression GetExpression()
		{
			return _expression;
		}
	}

	private sealed class TypeRestriction : BindingRestrictions
	{
		private readonly Expression _expression;

		private readonly Type _type;

		internal TypeRestriction(Expression parameter, Type type)
		{
			_expression = parameter;
			_type = type;
		}

		public override bool Equals(object obj)
		{
			TypeRestriction typeRestriction = obj as TypeRestriction;
			if (typeRestriction?._expression == _expression)
			{
				return TypeUtils.AreEquivalent(typeRestriction._type, _type);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return 0x49249249 ^ _expression.GetHashCode() ^ _type.GetHashCode();
		}

		internal override Expression GetExpression()
		{
			return Expression.TypeEqual(_expression, _type);
		}
	}

	private sealed class InstanceRestriction : BindingRestrictions
	{
		private readonly Expression _expression;

		private readonly object _instance;

		internal InstanceRestriction(Expression parameter, object instance)
		{
			_expression = parameter;
			_instance = instance;
		}

		public override bool Equals(object obj)
		{
			InstanceRestriction instanceRestriction = obj as InstanceRestriction;
			if (instanceRestriction?._expression == _expression)
			{
				return instanceRestriction._instance == _instance;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return -1840700270 ^ RuntimeHelpers.GetHashCode(_instance) ^ _expression.GetHashCode();
		}

		internal override Expression GetExpression()
		{
			if (_instance == null)
			{
				return Expression.Equal(Expression.Convert(_expression, typeof(object)), System.Linq.Expressions.Utils.Null);
			}
			ParameterExpression parameterExpression = Expression.Parameter(typeof(object), null);
			return Expression.Block(new TrueReadOnlyCollection<ParameterExpression>(parameterExpression), new TrueReadOnlyCollection<Expression>(Expression.Assign(parameterExpression, Expression.Constant(_instance, typeof(object))), Expression.AndAlso(Expression.NotEqual(parameterExpression, System.Linq.Expressions.Utils.Null), Expression.Equal(Expression.Convert(_expression, typeof(object)), parameterExpression))));
		}
	}

	private sealed class BindingRestrictionsProxy
	{
		private readonly BindingRestrictions _node;

		public bool IsEmpty => _node == Empty;

		public Expression Test => _node.ToExpression();

		public BindingRestrictions[] Restrictions
		{
			get
			{
				List<BindingRestrictions> list = new List<BindingRestrictions>();
				Stack<BindingRestrictions> stack = new Stack<BindingRestrictions>();
				BindingRestrictions bindingRestrictions = _node;
				while (true)
				{
					if (bindingRestrictions is MergedRestriction mergedRestriction)
					{
						stack.Push(mergedRestriction.Right);
						bindingRestrictions = mergedRestriction.Left;
						continue;
					}
					list.Add(bindingRestrictions);
					if (stack.Count == 0)
					{
						break;
					}
					bindingRestrictions = stack.Pop();
				}
				return list.ToArray();
			}
		}

		public BindingRestrictionsProxy(BindingRestrictions node)
		{
			ContractUtils.RequiresNotNull(node, "node");
			_node = node;
		}

		public override string ToString()
		{
			return _node.DebugView;
		}
	}

	/// <summary>Represents an empty set of binding restrictions. This field is read only.</summary>
	public static readonly BindingRestrictions Empty = new CustomRestriction(System.Linq.Expressions.Utils.Constant(value: true));

	private const int TypeRestrictionHash = 1227133513;

	private const int InstanceRestrictionHash = -1840700270;

	private const int CustomRestrictionHash = 613566756;

	private string DebugView => ToExpression().ToString();

	private BindingRestrictions()
	{
	}

	internal abstract Expression GetExpression();

	/// <summary>Merges the set of binding restrictions with the current binding restrictions.</summary>
	/// <returns>The new set of binding restrictions.</returns>
	/// <param name="restrictions">The set of restrictions with which to merge the current binding restrictions.</param>
	public BindingRestrictions Merge(BindingRestrictions restrictions)
	{
		ContractUtils.RequiresNotNull(restrictions, "restrictions");
		if (this == Empty)
		{
			return restrictions;
		}
		if (restrictions == Empty)
		{
			return this;
		}
		return new MergedRestriction(this, restrictions);
	}

	/// <summary>Creates the binding restriction that check the expression for runtime type identity.</summary>
	/// <returns>The new binding restrictions.</returns>
	/// <param name="expression">The expression to test.</param>
	/// <param name="type">The exact type to test.</param>
	public static BindingRestrictions GetTypeRestriction(Expression expression, Type type)
	{
		ContractUtils.RequiresNotNull(expression, "expression");
		ContractUtils.RequiresNotNull(type, "type");
		return new TypeRestriction(expression, type);
	}

	internal static BindingRestrictions GetTypeRestriction(DynamicMetaObject obj)
	{
		if (obj.Value == null && obj.HasValue)
		{
			return GetInstanceRestriction(obj.Expression, null);
		}
		return GetTypeRestriction(obj.Expression, obj.LimitType);
	}

	/// <summary>Creates the binding restriction that checks the expression for object instance identity.</summary>
	/// <returns>The new binding restrictions.</returns>
	/// <param name="expression">The expression to test.</param>
	/// <param name="instance">The exact object instance to test.</param>
	public static BindingRestrictions GetInstanceRestriction(Expression expression, object instance)
	{
		ContractUtils.RequiresNotNull(expression, "expression");
		return new InstanceRestriction(expression, instance);
	}

	/// <summary>Creates the binding restriction that checks the expression for arbitrary immutable properties.</summary>
	/// <returns>The new binding restrictions.</returns>
	/// <param name="expression">The expression representing the restrictions.</param>
	public static BindingRestrictions GetExpressionRestriction(Expression expression)
	{
		ContractUtils.RequiresNotNull(expression, "expression");
		ContractUtils.Requires(expression.Type == typeof(bool), "expression");
		return new CustomRestriction(expression);
	}

	/// <summary>Combines binding restrictions from the list of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances into one set of restrictions.</summary>
	/// <returns>The new set of binding restrictions.</returns>
	/// <param name="contributingObjects">The list of <see cref="T:System.Dynamic.DynamicMetaObject" /> instances from which to combine restrictions.</param>
	public static BindingRestrictions Combine(IList<DynamicMetaObject> contributingObjects)
	{
		BindingRestrictions bindingRestrictions = Empty;
		if (contributingObjects != null)
		{
			foreach (DynamicMetaObject contributingObject in contributingObjects)
			{
				if (contributingObject != null)
				{
					bindingRestrictions = bindingRestrictions.Merge(contributingObject.Restrictions);
				}
			}
		}
		return bindingRestrictions;
	}

	/// <summary>Creates the <see cref="T:System.Linq.Expressions.Expression" /> representing the binding restrictions.</summary>
	/// <returns>The expression tree representing the restrictions.</returns>
	public Expression ToExpression()
	{
		return GetExpression();
	}
}
