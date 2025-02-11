using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Data;
using System.Windows.Media;
using MS.Internal;
using MS.Internal.KnownBoxes;
using MS.Internal.Telemetry.PresentationFramework;

namespace System.Windows.Controls.Primitives;

/// <summary>Arranges <see cref="T:System.Windows.Controls.ToolBar" /> items inside a <see cref="T:System.Windows.Controls.ToolBar" />.</summary>
public class ToolBarPanel : StackPanel
{
	private List<UIElement> _generatedItemsCollection;

	internal double MinLength { get; private set; }

	internal double MaxLength { get; private set; }

	private ToolBar ToolBar => base.TemplatedParent as ToolBar;

	private ToolBarOverflowPanel ToolBarOverflowPanel => ToolBar?.ToolBarOverflowPanel;

	internal List<UIElement> GeneratedItemsCollection => _generatedItemsCollection;

	static ToolBarPanel()
	{
		ControlsTraceLogger.AddControl(TelemetryControls.ToolBarPanel);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.Primitives.ToolBarPanel" /> class.</summary>
	public ToolBarPanel()
	{
	}

	private bool MeasureGeneratedItems(bool asNeededPass, Size constraint, bool horizontal, double maxExtent, ref Size panelDesiredSize, out double overflowExtent)
	{
		ToolBarOverflowPanel toolBarOverflowPanel = ToolBarOverflowPanel;
		bool flag = false;
		bool result = false;
		bool flag2 = false;
		overflowExtent = 0.0;
		UIElementCollection internalChildren = base.InternalChildren;
		int num = internalChildren.Count;
		int num2 = 0;
		for (int i = 0; i < _generatedItemsCollection.Count; i++)
		{
			UIElement uIElement = _generatedItemsCollection[i];
			OverflowMode overflowMode = ToolBar.GetOverflowMode(uIElement);
			bool flag3 = overflowMode == OverflowMode.AsNeeded;
			if (flag3 == asNeededPass)
			{
				DependencyObject parent = VisualTreeHelper.GetParent(uIElement);
				if (overflowMode != OverflowMode.Always && !flag)
				{
					ToolBar.SetIsOverflowItem(uIElement, BooleanBoxes.FalseBox);
					uIElement.Measure(constraint);
					Size desiredSize = uIElement.DesiredSize;
					if (flag3)
					{
						double value = ((!horizontal) ? (desiredSize.Height + panelDesiredSize.Height) : (desiredSize.Width + panelDesiredSize.Width));
						if (DoubleUtil.GreaterThan(value, maxExtent))
						{
							flag = true;
						}
					}
					if (!flag)
					{
						if (horizontal)
						{
							panelDesiredSize.Width += desiredSize.Width;
							panelDesiredSize.Height = Math.Max(panelDesiredSize.Height, desiredSize.Height);
						}
						else
						{
							panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, desiredSize.Width);
							panelDesiredSize.Height += desiredSize.Height;
						}
						if (parent != this)
						{
							if (parent == toolBarOverflowPanel)
							{
								toolBarOverflowPanel?.Children.Remove(uIElement);
							}
							if (num2 < num)
							{
								internalChildren.InsertInternal(num2, uIElement);
							}
							else
							{
								internalChildren.AddInternal(uIElement);
							}
							num++;
						}
						num2++;
					}
				}
				if (overflowMode == OverflowMode.Always || flag)
				{
					result = true;
					if (uIElement.MeasureDirty)
					{
						ToolBar.SetIsOverflowItem(uIElement, BooleanBoxes.FalseBox);
						uIElement.Measure(constraint);
					}
					Size desiredSize2 = uIElement.DesiredSize;
					if (horizontal)
					{
						overflowExtent += desiredSize2.Width;
						panelDesiredSize.Height = Math.Max(panelDesiredSize.Height, desiredSize2.Height);
					}
					else
					{
						overflowExtent += desiredSize2.Height;
						panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, desiredSize2.Width);
					}
					ToolBar.SetIsOverflowItem(uIElement, BooleanBoxes.TrueBox);
					if (parent == this)
					{
						internalChildren.RemoveNoVerify(uIElement);
						num--;
						flag2 = true;
					}
					else if (parent == null)
					{
						flag2 = true;
					}
				}
			}
			else if (num2 < num && internalChildren[num2] == uIElement)
			{
				num2++;
			}
		}
		if (flag2)
		{
			toolBarOverflowPanel?.InvalidateMeasure();
		}
		return result;
	}

	/// <summary>Remeasures a toolbar panel.</summary>
	/// <returns>The desired size of the panel.</returns>
	/// <param name="constraint">The measurement constraints; a <see cref="T:System.Windows.Controls.Primitives.ToolBarPanel" /> cannot return a size larger than the constraint.</param>
	protected override Size MeasureOverride(Size constraint)
	{
		Size panelDesiredSize = default(Size);
		if (base.IsItemsHost)
		{
			Size constraint2 = constraint;
			bool flag = base.Orientation == Orientation.Horizontal;
			double maxExtent;
			if (flag)
			{
				constraint2.Width = double.PositiveInfinity;
				maxExtent = constraint.Width;
			}
			else
			{
				constraint2.Height = double.PositiveInfinity;
				maxExtent = constraint.Height;
			}
			double overflowExtent;
			bool flag2 = MeasureGeneratedItems(asNeededPass: false, constraint2, flag, maxExtent, ref panelDesiredSize, out overflowExtent);
			MinLength = (flag ? panelDesiredSize.Width : panelDesiredSize.Height);
			bool flag3 = MeasureGeneratedItems(asNeededPass: true, constraint2, flag, maxExtent, ref panelDesiredSize, out overflowExtent);
			MaxLength = (flag ? panelDesiredSize.Width : panelDesiredSize.Height) + overflowExtent;
			ToolBar?.SetValue(ToolBar.HasOverflowItemsPropertyKey, flag2 || flag3);
			return panelDesiredSize;
		}
		return base.MeasureOverride(constraint);
	}

	/// <summary>Arranges and sizes <see cref="T:System.Windows.Controls.ToolBar" /> items inside a <see cref="T:System.Windows.Controls.Primitives.ToolBarPanel" />.</summary>
	/// <returns>The size of the panel.</returns>
	/// <param name="arrangeSize">The size that the <see cref="T:System.Windows.Controls.Primitives.ToolBarPanel" /> assumes to position its children.</param>
	protected override Size ArrangeOverride(Size arrangeSize)
	{
		UIElementCollection internalChildren = base.InternalChildren;
		bool flag = base.Orientation == Orientation.Horizontal;
		Rect finalRect = new Rect(arrangeSize);
		double num = 0.0;
		int i = 0;
		for (int count = internalChildren.Count; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (flag)
			{
				finalRect.X += num;
				num = (finalRect.Width = uIElement.DesiredSize.Width);
				finalRect.Height = Math.Max(arrangeSize.Height, uIElement.DesiredSize.Height);
			}
			else
			{
				finalRect.Y += num;
				num = (finalRect.Height = uIElement.DesiredSize.Height);
				finalRect.Width = Math.Max(arrangeSize.Width, uIElement.DesiredSize.Width);
			}
			uIElement.Arrange(finalRect);
		}
		return arrangeSize;
	}

	internal override void OnPreApplyTemplate()
	{
		base.OnPreApplyTemplate();
		if (base.TemplatedParent is ToolBar && !HasNonDefaultValue(StackPanel.OrientationProperty))
		{
			Binding binding = new Binding();
			binding.RelativeSource = RelativeSource.TemplatedParent;
			binding.Path = new PropertyPath(ToolBar.OrientationProperty);
			SetBinding(StackPanel.OrientationProperty, binding);
		}
	}

	internal override void GenerateChildren()
	{
		base.GenerateChildren();
		UIElementCollection internalChildren = base.InternalChildren;
		if (_generatedItemsCollection == null)
		{
			_generatedItemsCollection = new List<UIElement>(internalChildren.Count);
		}
		else
		{
			_generatedItemsCollection.Clear();
		}
		ToolBarOverflowPanel toolBarOverflowPanel = ToolBarOverflowPanel;
		if (toolBarOverflowPanel != null)
		{
			toolBarOverflowPanel.Children.Clear();
			toolBarOverflowPanel.InvalidateMeasure();
		}
		int count = internalChildren.Count;
		for (int i = 0; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			ToolBar.SetIsOverflowItem(uIElement, BooleanBoxes.FalseBox);
			_generatedItemsCollection.Add(uIElement);
		}
	}

	internal override bool OnItemsChangedInternal(object sender, ItemsChangedEventArgs args)
	{
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Add:
			AddChildren(args.Position, args.ItemCount);
			break;
		case NotifyCollectionChangedAction.Remove:
			RemoveChildren(args.Position, args.ItemUICount);
			break;
		case NotifyCollectionChangedAction.Replace:
			ReplaceChildren(args.Position, args.ItemCount, args.ItemUICount);
			break;
		case NotifyCollectionChangedAction.Move:
			MoveChildren(args.OldPosition, args.Position, args.ItemUICount);
			break;
		case NotifyCollectionChangedAction.Reset:
			base.OnItemsChangedInternal(sender, args);
			break;
		}
		return true;
	}

	private void AddChildren(GeneratorPosition pos, int itemCount)
	{
		IItemContainerGenerator generator = base.Generator;
		using (generator.StartAt(pos, GeneratorDirection.Forward))
		{
			for (int i = 0; i < itemCount; i++)
			{
				if (generator.GenerateNext() is UIElement uIElement)
				{
					_generatedItemsCollection.Insert(pos.Index + 1 + i, uIElement);
					generator.PrepareItemContainer(uIElement);
				}
				else if (base.Generator is ItemContainerGenerator itemContainerGenerator)
				{
					itemContainerGenerator.Verify();
				}
			}
		}
	}

	private void RemoveChild(UIElement child)
	{
		DependencyObject parent = VisualTreeHelper.GetParent(child);
		if (parent == this)
		{
			base.InternalChildren.RemoveInternal(child);
			return;
		}
		ToolBarOverflowPanel toolBarOverflowPanel = ToolBarOverflowPanel;
		if (parent == toolBarOverflowPanel)
		{
			toolBarOverflowPanel?.Children.Remove(child);
		}
	}

	private void RemoveChildren(GeneratorPosition pos, int containerCount)
	{
		for (int i = 0; i < containerCount; i++)
		{
			RemoveChild(_generatedItemsCollection[pos.Index + i]);
		}
		_generatedItemsCollection.RemoveRange(pos.Index, containerCount);
	}

	private void ReplaceChildren(GeneratorPosition pos, int itemCount, int containerCount)
	{
		IItemContainerGenerator generator = base.Generator;
		using (generator.StartAt(pos, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
		{
			for (int i = 0; i < itemCount; i++)
			{
				if (generator.GenerateNext(out var isNewlyRealized) is UIElement uIElement && !isNewlyRealized)
				{
					RemoveChild(_generatedItemsCollection[pos.Index + i]);
					_generatedItemsCollection[pos.Index + i] = uIElement;
					generator.PrepareItemContainer(uIElement);
				}
				else if (base.Generator is ItemContainerGenerator itemContainerGenerator)
				{
					itemContainerGenerator.Verify();
				}
			}
		}
	}

	private void MoveChildren(GeneratorPosition fromPos, GeneratorPosition toPos, int containerCount)
	{
		if (!(fromPos == toPos))
		{
			int num = base.Generator.IndexFromGeneratorPosition(toPos);
			UIElement[] array = new UIElement[containerCount];
			for (int i = 0; i < containerCount; i++)
			{
				UIElement uIElement = _generatedItemsCollection[fromPos.Index + i];
				RemoveChild(uIElement);
				array[i] = uIElement;
			}
			_generatedItemsCollection.RemoveRange(fromPos.Index, containerCount);
			for (int j = 0; j < containerCount; j++)
			{
				_generatedItemsCollection.Insert(num + j, array[j]);
			}
		}
	}
}
