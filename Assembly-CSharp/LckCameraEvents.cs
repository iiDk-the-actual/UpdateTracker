using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

public class LckCameraEvents : MonoBehaviour
{
	private void OnEnable()
	{
		RenderPipelineManager.beginCameraRendering += this.RenderPipelineManagerOnbeginCameraRendering;
		RenderPipelineManager.endCameraRendering += this.RenderPipelineManagerOnendCameraRendering;
	}

	private void OnDisable()
	{
		RenderPipelineManager.beginCameraRendering -= this.RenderPipelineManagerOnbeginCameraRendering;
		RenderPipelineManager.endCameraRendering -= this.RenderPipelineManagerOnendCameraRendering;
	}

	private void RenderPipelineManagerOnbeginCameraRendering(ScriptableRenderContext scriptableRenderContext, Camera camera)
	{
		if (this._camera != camera)
		{
			return;
		}
		UnityEvent unityEvent = this.onPreRender;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	private void RenderPipelineManagerOnendCameraRendering(ScriptableRenderContext scriptableRenderContext, Camera camera)
	{
		if (this._camera != camera)
		{
			return;
		}
		UnityEvent unityEvent = this.onPostRender;
		if (unityEvent == null)
		{
			return;
		}
		unityEvent.Invoke();
	}

	[SerializeField]
	private Camera _camera;

	public UnityEvent onPreRender;

	public UnityEvent onPostRender;
}
