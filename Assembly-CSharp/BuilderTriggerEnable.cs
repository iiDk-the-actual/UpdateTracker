using System;
using System.Collections.Generic;
using UnityEngine;

public class BuilderTriggerEnable : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.attachedRigidbody == null)
		{
			return;
		}
		VRRig component = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (component == null || component.OwningNetPlayer == null)
		{
			return;
		}
		if (!component.OwningNetPlayer.IsLocal)
		{
			return;
		}
		if (this.activateOnEnter != null)
		{
			for (int i = 0; i < this.activateOnEnter.Count; i++)
			{
				if (this.activateOnEnter[i] != null)
				{
					this.activateOnEnter[i].SetActive(true);
				}
			}
		}
		if (this.deactivateOnEnter != null)
		{
			for (int j = 0; j < this.deactivateOnEnter.Count; j++)
			{
				if (this.deactivateOnEnter[j] != null)
				{
					this.deactivateOnEnter[j].SetActive(false);
				}
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.attachedRigidbody == null)
		{
			return;
		}
		VRRig component = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		if (component == null || component.OwningNetPlayer == null)
		{
			return;
		}
		if (!component.OwningNetPlayer.IsLocal)
		{
			return;
		}
		if (this.activateOnExit != null)
		{
			for (int i = 0; i < this.activateOnExit.Count; i++)
			{
				if (this.activateOnExit[i] != null)
				{
					this.activateOnExit[i].SetActive(true);
				}
			}
		}
		if (this.deactivateOnExit != null)
		{
			for (int j = 0; j < this.deactivateOnExit.Count; j++)
			{
				if (this.deactivateOnExit[j] != null)
				{
					this.deactivateOnExit[j].SetActive(false);
				}
			}
		}
	}

	public List<GameObject> activateOnEnter;

	public List<GameObject> deactivateOnEnter;

	public List<GameObject> activateOnExit;

	public List<GameObject> deactivateOnExit;
}
