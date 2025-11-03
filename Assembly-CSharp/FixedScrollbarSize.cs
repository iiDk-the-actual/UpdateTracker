using System;
using UnityEngine;
using UnityEngine.UI;

public class FixedScrollbarSize : MonoBehaviour
{
	private void OnEnable()
	{
		this.EnforceScrollbarSize();
		CanvasUpdateRegistry.instance.Equals(null);
		Canvas.willRenderCanvases += this.EnforceScrollbarSize;
	}

	private void OnDisable()
	{
		Canvas.willRenderCanvases -= this.EnforceScrollbarSize;
	}

	private void EnforceScrollbarSize()
	{
		if (this.ScrollRect.horizontalScrollbar && this.ScrollRect.horizontalScrollbar.size != this.HorizontalBarSize)
		{
			this.ScrollRect.horizontalScrollbar.size = this.HorizontalBarSize;
		}
		if (this.ScrollRect.verticalScrollbar && this.ScrollRect.verticalScrollbar.size != this.VerticalBarSize)
		{
			this.ScrollRect.verticalScrollbar.size = this.VerticalBarSize;
		}
	}

	public ScrollRect ScrollRect;

	public float HorizontalBarSize = 0.2f;

	public float VerticalBarSize = 0.2f;
}
