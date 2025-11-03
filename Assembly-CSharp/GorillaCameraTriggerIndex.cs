using System;
using UnityEngine;

public class GorillaCameraTriggerIndex : MonoBehaviour
{
	private void Start()
	{
		this.parentTrigger = base.GetComponentInParent<GorillaCameraSceneTrigger>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("SceneChanger"))
		{
			this.parentTrigger.mostRecentSceneTrigger = this;
			this.parentTrigger.ChangeScene(this);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.CompareTag("SceneChanger"))
		{
			this.parentTrigger.ChangeScene(this);
		}
	}

	public int sceneTriggerIndex;

	public GorillaCameraSceneTrigger parentTrigger;
}
