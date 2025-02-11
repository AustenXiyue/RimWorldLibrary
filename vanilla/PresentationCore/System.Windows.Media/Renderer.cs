using System.Windows.Media.Composition;

namespace System.Windows.Media;

internal static class Renderer
{
	public static void Render(nint pRenderTarget, DUCE.Channel channel, Visual visual, int width, int height, double dpiX, double dpiY)
	{
		Render(pRenderTarget, channel, visual, width, height, dpiX, dpiY, Matrix.Identity, Rect.Empty);
	}

	internal static void Render(nint pRenderTarget, DUCE.Channel channel, Visual visual, int width, int height, double dpiX, double dpiY, Matrix worldTransform, Rect windowClip)
	{
		DUCE.Resource resource = default(DUCE.Resource);
		DUCE.Resource resource2 = default(DUCE.Resource);
		DUCE.ResourceHandle hCompositionTarget = DUCE.ResourceHandle.Null;
		DUCE.ResourceHandle resourceHandle = DUCE.ResourceHandle.Null;
		Matrix matrix = new Matrix(dpiX * (1.0 / 96.0), 0.0, 0.0, dpiY * (1.0 / 96.0), 0.0, 0.0);
		matrix = worldTransform * matrix;
		MatrixTransform matrixTransform = new MatrixTransform(matrix);
		DUCE.ResourceHandle hTransform = ((DUCE.IResource)matrixTransform).AddRefOnChannel(channel);
		try
		{
			resource.CreateOrAddRefOnChannel(resource, channel, DUCE.ResourceType.TYPE_GENERICRENDERTARGET);
			hCompositionTarget = resource.Handle;
			DUCE.CompositionTarget.PrintInitialize(hCompositionTarget, pRenderTarget, width, height, channel);
			resource2.CreateOrAddRefOnChannel(resource2, channel, DUCE.ResourceType.TYPE_VISUAL);
			resourceHandle = resource2.Handle;
			DUCE.CompositionNode.SetTransform(resourceHandle, hTransform, channel);
			DUCE.CompositionTarget.SetRoot(hCompositionTarget, resourceHandle, channel);
			channel.CloseBatch();
			channel.Commit();
			RenderContext renderContext = new RenderContext();
			renderContext.Initialize(channel, resourceHandle);
			visual.Precompute();
			visual.Render(renderContext, 0u);
			channel.CloseBatch();
			channel.Commit();
			channel.Present();
			MediaContext.CurrentMediaContext.NotifySyncChannelMessage(channel);
		}
		finally
		{
			if (!resourceHandle.IsNull)
			{
				DUCE.CompositionNode.RemoveAllChildren(resourceHandle, channel);
				((DUCE.IResource)visual).ReleaseOnChannel(channel);
				resource2.ReleaseOnChannel(channel);
			}
			if (!hTransform.IsNull)
			{
				((DUCE.IResource)matrixTransform).ReleaseOnChannel(channel);
			}
			if (!hCompositionTarget.IsNull)
			{
				DUCE.CompositionTarget.SetRoot(hCompositionTarget, DUCE.ResourceHandle.Null, channel);
				resource.ReleaseOnChannel(channel);
			}
			channel.CloseBatch();
			channel.Commit();
			channel.Present();
			MediaContext.CurrentMediaContext.NotifySyncChannelMessage(channel);
		}
	}
}
