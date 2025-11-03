using System;
using System.Collections.Generic;
using UnityEngine;

public class ZoneConditionalVisibility : MonoBehaviour
{
	private void Awake()
	{
		if (this.renderersOnly)
		{
			this.renderers = new List<Renderer>(32);
			base.GetComponentsInChildren<Renderer>(false, this.renderers);
		}
	}

	private void Start()
	{
		this.OnZoneChanged();
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
	}

	private void OnDestroy()
	{
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.IsInZone(this.zone);
		if (this.invisibleWhileLoaded)
		{
			if (this.renderersOnly)
			{
				for (int i = 0; i < this.renderers.Count; i++)
				{
					if (this.renderers[i] != null)
					{
						this.renderers[i].enabled = !flag;
					}
				}
				return;
			}
			base.gameObject.SetActive(!flag);
			return;
		}
		else
		{
			if (this.renderersOnly)
			{
				for (int j = 0; j < this.renderers.Count; j++)
				{
					if (this.renderers[j] != null)
					{
						this.renderers[j].enabled = flag;
					}
				}
				return;
			}
			base.gameObject.SetActive(flag);
			return;
		}
	}

	[SerializeField]
	private GTZone zone;

	[SerializeField]
	private bool invisibleWhileLoaded;

	[SerializeField]
	private bool renderersOnly;

	private List<Renderer> renderers;
}
