using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI;

[AddComponentMenu("Event/Graphic Raycaster")]
[RequireComponent(typeof(Canvas))]
public class GraphicRaycaster : BaseRaycaster
{
	public enum BlockingObjects
	{
		None,
		TwoD,
		ThreeD,
		All
	}

	protected const int kNoEventMaskSet = -1;

	[FormerlySerializedAs("ignoreReversedGraphics")]
	[SerializeField]
	private bool m_IgnoreReversedGraphics = true;

	[FormerlySerializedAs("blockingObjects")]
	[SerializeField]
	private BlockingObjects m_BlockingObjects;

	[SerializeField]
	protected LayerMask m_BlockingMask = -1;

	private Canvas m_Canvas;

	[NonSerialized]
	private List<Graphic> m_RaycastResults = new List<Graphic>();

	[NonSerialized]
	private static readonly List<Graphic> s_SortedGraphics = new List<Graphic>();

	public override int sortOrderPriority
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return canvas.sortingOrder;
			}
			return base.sortOrderPriority;
		}
	}

	public override int renderOrderPriority
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				return canvas.rootCanvas.renderOrder;
			}
			return base.renderOrderPriority;
		}
	}

	public bool ignoreReversedGraphics
	{
		get
		{
			return m_IgnoreReversedGraphics;
		}
		set
		{
			m_IgnoreReversedGraphics = value;
		}
	}

	public BlockingObjects blockingObjects
	{
		get
		{
			return m_BlockingObjects;
		}
		set
		{
			m_BlockingObjects = value;
		}
	}

	private Canvas canvas
	{
		get
		{
			if (m_Canvas != null)
			{
				return m_Canvas;
			}
			m_Canvas = GetComponent<Canvas>();
			return m_Canvas;
		}
	}

	public override Camera eventCamera
	{
		get
		{
			if (canvas.renderMode == RenderMode.ScreenSpaceOverlay || (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null))
			{
				return null;
			}
			if (!(canvas.worldCamera != null))
			{
				return Camera.main;
			}
			return canvas.worldCamera;
		}
	}

	protected GraphicRaycaster()
	{
	}

	public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
	{
		if (canvas == null)
		{
			return;
		}
		IList<Graphic> graphicsForCanvas = GraphicRegistry.GetGraphicsForCanvas(canvas);
		if (graphicsForCanvas == null || graphicsForCanvas.Count == 0)
		{
			return;
		}
		Camera camera = eventCamera;
		int num = ((canvas.renderMode != 0 && !(camera == null)) ? camera.targetDisplay : canvas.targetDisplay);
		Vector3 vector = Display.RelativeMouseAt(eventData.position);
		if (vector != Vector3.zero)
		{
			if ((int)vector.z != num)
			{
				return;
			}
		}
		else
		{
			vector = eventData.position;
		}
		Vector2 vector2;
		if (camera == null)
		{
			float num2 = Screen.width;
			float num3 = Screen.height;
			if (num > 0 && num < Display.displays.Length)
			{
				num2 = Display.displays[num].systemWidth;
				num3 = Display.displays[num].systemHeight;
			}
			vector2 = new Vector2(vector.x / num2, vector.y / num3);
		}
		else
		{
			vector2 = camera.ScreenToViewportPoint(vector);
		}
		if (vector2.x < 0f || vector2.x > 1f || vector2.y < 0f || vector2.y > 1f)
		{
			return;
		}
		float num4 = float.MaxValue;
		Ray r = default(Ray);
		if (camera != null)
		{
			r = camera.ScreenPointToRay(vector);
		}
		if (canvas.renderMode != 0 && blockingObjects != 0)
		{
			float f = 100f;
			if (camera != null)
			{
				float z = r.direction.z;
				f = (Mathf.Approximately(0f, z) ? float.PositiveInfinity : Mathf.Abs((camera.farClipPlane - camera.nearClipPlane) / z));
			}
			if ((blockingObjects == BlockingObjects.ThreeD || blockingObjects == BlockingObjects.All) && ReflectionMethodsCache.Singleton.raycast3D != null)
			{
				RaycastHit[] array = ReflectionMethodsCache.Singleton.raycast3DAll(r, f, m_BlockingMask);
				if (array.Length != 0)
				{
					num4 = array[0].distance;
				}
			}
			if ((blockingObjects == BlockingObjects.TwoD || blockingObjects == BlockingObjects.All) && ReflectionMethodsCache.Singleton.raycast2D != null)
			{
				RaycastHit2D[] array2 = ReflectionMethodsCache.Singleton.getRayIntersectionAll(r, f, m_BlockingMask);
				if (array2.Length != 0)
				{
					num4 = array2[0].distance;
				}
			}
		}
		m_RaycastResults.Clear();
		Raycast(canvas, camera, vector, graphicsForCanvas, m_RaycastResults);
		int count = m_RaycastResults.Count;
		for (int i = 0; i < count; i++)
		{
			GameObject gameObject = m_RaycastResults[i].gameObject;
			bool flag = true;
			if (ignoreReversedGraphics)
			{
				if (camera == null)
				{
					Vector3 rhs = gameObject.transform.rotation * Vector3.forward;
					flag = Vector3.Dot(Vector3.forward, rhs) > 0f;
				}
				else
				{
					Vector3 vector3 = camera.transform.rotation * Vector3.forward * camera.nearClipPlane;
					flag = Vector3.Dot(gameObject.transform.position - camera.transform.position - vector3, gameObject.transform.forward) >= 0f;
				}
			}
			if (!flag)
			{
				continue;
			}
			float num5 = 0f;
			Transform transform = gameObject.transform;
			Vector3 forward = transform.forward;
			if (camera == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
			{
				num5 = 0f;
			}
			else
			{
				num5 = Vector3.Dot(forward, transform.position - r.origin) / Vector3.Dot(forward, r.direction);
				if (num5 < 0f)
				{
					continue;
				}
			}
			if (!(num5 >= num4))
			{
				RaycastResult raycastResult = default(RaycastResult);
				raycastResult.gameObject = gameObject;
				raycastResult.module = this;
				raycastResult.distance = num5;
				raycastResult.screenPosition = vector;
				raycastResult.displayIndex = num;
				raycastResult.index = resultAppendList.Count;
				raycastResult.depth = m_RaycastResults[i].depth;
				raycastResult.sortingLayer = canvas.sortingLayerID;
				raycastResult.sortingOrder = canvas.sortingOrder;
				raycastResult.worldPosition = r.origin + r.direction * num5;
				raycastResult.worldNormal = -forward;
				RaycastResult item = raycastResult;
				resultAppendList.Add(item);
			}
		}
	}

	private static void Raycast(Canvas canvas, Camera eventCamera, Vector2 pointerPosition, IList<Graphic> foundGraphics, List<Graphic> results)
	{
		int count = foundGraphics.Count;
		for (int i = 0; i < count; i++)
		{
			Graphic graphic = foundGraphics[i];
			if (graphic.raycastTarget && !graphic.canvasRenderer.cull && graphic.depth != -1 && RectTransformUtility.RectangleContainsScreenPoint(graphic.rectTransform, pointerPosition, eventCamera) && (!(eventCamera != null) || !(eventCamera.WorldToScreenPoint(graphic.rectTransform.position).z > eventCamera.farClipPlane)) && graphic.Raycast(pointerPosition, eventCamera))
			{
				s_SortedGraphics.Add(graphic);
			}
		}
		s_SortedGraphics.Sort((Graphic g1, Graphic g2) => g2.depth.CompareTo(g1.depth));
		count = s_SortedGraphics.Count;
		for (int j = 0; j < count; j++)
		{
			results.Add(s_SortedGraphics[j]);
		}
		s_SortedGraphics.Clear();
	}
}
