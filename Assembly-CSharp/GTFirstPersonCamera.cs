using System;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-2147483648)]
public class GTFirstPersonCamera : MonoBehaviour
{
	public static Camera camera { get; private set; }

	public void Awake()
	{
		GTFirstPersonCamera.camera = base.GetComponent<Camera>();
		if (GTFirstPersonCamera.camera == null)
		{
			Debug.LogError("[GTFirstPersonCamera]  ERROR!!!  Could not find Camera on same GameObject!");
			return;
		}
		RenderPipelineManager.beginCameraRendering += this._OnPreRender;
	}

	private void _OnPreRender(ScriptableRenderContext context, Camera cam)
	{
		if (cam == GTFirstPersonCamera.camera)
		{
			Action onPreRenderEvent = GTFirstPersonCamera.OnPreRenderEvent;
			if (onPreRenderEvent == null)
			{
				return;
			}
			onPreRenderEvent();
		}
	}

	private const string preLog = "[GTFirstPersonCamera]  ";

	private const string preErr = "[GTFirstPersonCamera]  ERROR!!!  ";

	public static Action OnPreRenderEvent;
}
