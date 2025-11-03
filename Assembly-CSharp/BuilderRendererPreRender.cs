using System;

public class BuilderRendererPreRender : MonoBehaviourPostTick
{
	private void Awake()
	{
	}

	public override void PostTick()
	{
		if (this.builderRenderer != null)
		{
			this.builderRenderer.PreRenderIndirect();
		}
	}

	public BuilderRenderer builderRenderer;
}
