using System;
using UnityEngine;

public class SIResourceCollectionDepositTrigger : MonoBehaviour
{
	private void Awake()
	{
		this.resourceDeposit = this.parentCollection.GetComponent<ISIResourceDeposit>();
	}

	private void OnTriggerEnter(Collider other)
	{
		SIResource componentInParent = other.GetComponentInParent<SIResource>();
		if (componentInParent == null)
		{
			return;
		}
		if (componentInParent.CanDeposit())
		{
			this.resourceDeposit.ResourceDeposited(componentInParent);
		}
	}

	public GameObject parentCollection;

	private ISIResourceDeposit resourceDeposit;
}
