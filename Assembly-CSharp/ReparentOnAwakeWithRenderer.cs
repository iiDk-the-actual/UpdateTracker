using System;
using UnityEngine;
using UnityEngine.Rendering;

public class ReparentOnAwakeWithRenderer : MonoBehaviour, IBuildValidation
{
	public bool BuildValidationCheck()
	{
		if (base.GetComponent<MeshRenderer>() != null && this.myRenderer == null)
		{
			Debug.Log(base.name + " needs a reference to its renderer since it has one - ");
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		base.transform.SetParent(this.newParent, true);
		if (this.sortLast)
		{
			base.transform.SetAsLastSibling();
		}
		else
		{
			base.transform.SetAsFirstSibling();
		}
		if (this.myRenderer != null)
		{
			this.myRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
			this.myRenderer.lightProbeUsage = LightProbeUsage.CustomProvided;
			this.myRenderer.probeAnchor = this.newParent;
		}
	}

	[ContextMenu("Set Renderer")]
	public void SetMyRenderer()
	{
		this.myRenderer = base.GetComponent<MeshRenderer>();
	}

	public Transform newParent;

	public MeshRenderer myRenderer;

	[Tooltip("We're mostly using this for UI elements like text and images, so this will help you separate these in whatever target parent object.Keep images and texts together, otherwise you'll get extra draw calls. Put images above text or they'll overlap weird tho lol")]
	public bool sortLast;
}
