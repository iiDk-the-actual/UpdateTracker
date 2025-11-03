using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderZoneRenderers : MonoBehaviour
{
	private void Start()
	{
		this.allRenderers.Clear();
		this.allRenderers.AddRange(this.renderers);
		foreach (GameObject gameObject in this.rootObjects)
		{
			this.allRenderers.AddRange(gameObject.GetComponentsInChildren<Renderer>(true));
		}
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
		this.inBuilderZone = true;
		this.OnZoneChanged();
	}

	private void OnDestroy()
	{
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.instance.IsZoneActive(GTZone.monkeBlocks);
		if (flag && !this.inBuilderZone)
		{
			this.inBuilderZone = flag;
			foreach (Renderer renderer in this.allRenderers)
			{
				renderer.enabled = true;
			}
			using (List<Canvas>.Enumerator enumerator2 = this.canvases.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					Canvas canvas = enumerator2.Current;
					canvas.enabled = true;
				}
				return;
			}
		}
		if (!flag && this.inBuilderZone)
		{
			this.inBuilderZone = flag;
			foreach (Renderer renderer2 in this.allRenderers)
			{
				renderer2.enabled = false;
			}
			foreach (Canvas canvas2 in this.canvases)
			{
				canvas2.enabled = false;
			}
		}
	}

	public List<Renderer> renderers;

	public List<Canvas> canvases;

	public List<GameObject> rootObjects;

	private bool inBuilderZone;

	private List<Renderer> allRenderers = new List<Renderer>(200);
}
