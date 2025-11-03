using System;
using UnityEngine;

public class ZoneConditionalComponentEnabling : MonoBehaviour
{
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
		bool flag2 = (this.invisibleWhileLoaded ? (!flag) : flag);
		if (this.components != null)
		{
			for (int i = 0; i < this.components.Length; i++)
			{
				if (this.components[i] != null)
				{
					this.components[i].enabled = flag2;
				}
			}
		}
		if (this.m_renderers != null)
		{
			for (int j = 0; j < this.m_renderers.Length; j++)
			{
				if (this.m_renderers[j] != null)
				{
					this.m_renderers[j].enabled = flag2;
				}
			}
		}
		if (this.m_colliders != null)
		{
			for (int k = 0; k < this.m_colliders.Length; k++)
			{
				if (this.m_colliders[k] != null)
				{
					this.m_colliders[k].enabled = flag2;
				}
			}
		}
	}

	[SerializeField]
	private GTZone zone;

	[SerializeField]
	private bool invisibleWhileLoaded;

	[SerializeField]
	private Behaviour[] components;

	[SerializeField]
	private Renderer[] m_renderers;

	[SerializeField]
	private Collider[] m_colliders;
}
