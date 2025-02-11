using System;

namespace HarmonyLib;

public class ExceptionBlock
{
	public ExceptionBlockType blockType = blockType;

	public Type catchType = catchType ?? typeof(object);

	public ExceptionBlock(ExceptionBlockType blockType, Type catchType = null)
	{
	}
}
