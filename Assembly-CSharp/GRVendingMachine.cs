using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GRVendingMachine : MonoBehaviour
{
	public void Setup(GhostReactor reactor)
	{
		this.reactor = reactor;
	}

	public Transform GetSpawnMarker()
	{
		return this.itemSpawnLocation;
	}

	public void NavButtonPressedLeft()
	{
		this.hIndex = Mathf.Max(0, this.hIndex - 1);
		this.RefreshCardReaderDisplay();
	}

	public void NavButtonPressedRight()
	{
		this.hIndex = Mathf.Min(this.hIndex + 1, this.horizontalSteps - 1);
		this.RefreshCardReaderDisplay();
	}

	public void NavButtonPressedUp()
	{
		this.vIndex = Mathf.Max(0, this.vIndex - 1);
		this.RefreshCardReaderDisplay();
	}

	public void NavButtonPressedDown()
	{
		this.vIndex = Mathf.Min(this.vIndex + 1, this.verticalSteps - 1);
		this.RefreshCardReaderDisplay();
	}

	public void RequestPurchase()
	{
		if (!this.currentlyVending)
		{
			int num = this.vIndex * this.horizontalSteps + this.hIndex;
			if (num >= 0 && num < this.vendingEntries.Count)
			{
				this.vendingIndex = num;
				if (this.vendingCoroutine != null)
				{
					base.StopCoroutine(this.vendingCoroutine);
				}
				this.vendingCoroutine = base.StartCoroutine(this.VendingCoroutine());
			}
		}
	}

	private void RefreshCardReaderDisplay()
	{
		int num = this.vIndex * this.horizontalSteps + this.hIndex;
		if (num >= 0 && num < this.vendingEntries.Count)
		{
			int entityTypeId = this.vendingEntries[num].GetEntityTypeId();
			int itemCost = this.reactor.GetItemCost(entityTypeId);
			this.cardDisplayText.text = this.vendingEntries[num].itemName + "\n" + itemCost.ToString();
		}
	}

	private void Update()
	{
		if (!this.currentlyVending)
		{
			this.MoveTransportToSlot(this.hIndex, this.vIndex, this.horizontalSteps, this.verticalSteps, this.horizontalSpeed, this.verticalSpeed, Time.deltaTime);
		}
	}

	private bool MoveTransportToSlot(int x, int y, int rows, int cols, float xSpeed, float ySpeed, float dt)
	{
		Vector3 vector = Vector3.Lerp(this.horizontalMin.position, this.horizontalMax.position, (float)x / (float)(rows - 1));
		Vector3 vector2 = Vector3.Lerp(this.verticalMin.position, this.verticalMax.position, (float)y / (float)(cols - 1));
		this.horizontalTransport.position = Vector3.MoveTowards(this.horizontalTransport.position, vector, xSpeed * dt);
		this.verticalTransport.position = Vector3.MoveTowards(this.verticalTransport.position, vector2, ySpeed * dt);
		float sqrMagnitude = (this.horizontalTransport.position - vector).sqrMagnitude;
		float sqrMagnitude2 = (this.verticalTransport.position - vector2).sqrMagnitude;
		return sqrMagnitude > 0.001f || sqrMagnitude2 > 0.001f;
	}

	private IEnumerator VendingCoroutine()
	{
		this.currentlyVending = true;
		while (this.MoveTransportToSlot(this.hIndex, this.vIndex, this.horizontalSteps, this.verticalSteps, this.horizontalSpeed, this.verticalSpeed, Time.deltaTime))
		{
			yield return null;
		}
		int entityTypeId = this.vendingEntries[this.vendingIndex].GetEntityTypeId();
		int itemCost = this.reactor.GetItemCost(entityTypeId);
		if (this.debugUnlimitedPurchasing || VRRig.LocalRig.GetComponent<GRPlayer>().ShiftCredits >= itemCost)
		{
			this.vendingEntries[this.vendingIndex].transportVisual.gameObject.SetActive(true);
			while (this.MoveTransportToSlot(this.horizontalSteps - 1, this.verticalSteps - 1, this.horizontalSteps, this.verticalSteps, this.horizontalSpeed, this.verticalSpeed, Time.deltaTime))
			{
				yield return null;
			}
			float depositPosSqDist = (this.horizontalTransport.position - this.depositLocation.position).sqrMagnitude;
			while (depositPosSqDist > 0.001f)
			{
				this.horizontalTransport.position = Vector3.MoveTowards(this.horizontalTransport.position, this.depositLocation.position, this.horizontalSpeed * Time.deltaTime);
				depositPosSqDist = (this.horizontalTransport.position - this.depositLocation.position).sqrMagnitude;
				yield return null;
			}
			this.vendingEntries[this.vendingIndex].transportVisual.gameObject.SetActive(false);
			while (this.MoveTransportToSlot(this.horizontalSteps - 1, this.verticalSteps - 1, this.horizontalSteps, this.verticalSteps, this.horizontalSpeed, this.verticalSpeed, Time.deltaTime))
			{
				yield return null;
			}
		}
		this.currentlyVending = false;
		yield break;
	}

	[SerializeField]
	private Transform horizontalTransport;

	[SerializeField]
	private Transform verticalTransport;

	[SerializeField]
	private Transform horizontalMin;

	[SerializeField]
	private Transform horizontalMax;

	[SerializeField]
	private Transform verticalMin;

	[SerializeField]
	private Transform verticalMax;

	[SerializeField]
	private Transform depositLocation;

	[SerializeField]
	private Transform itemSpawnLocation;

	[SerializeField]
	private TMP_Text cardDisplayText;

	[SerializeField]
	private int horizontalSteps = 4;

	[SerializeField]
	private int verticalSteps = 3;

	[SerializeField]
	private float horizontalSpeed = 0.25f;

	[SerializeField]
	private float verticalSpeed = 0.25f;

	[SerializeField]
	private bool debugUnlimitedPurchasing;

	[SerializeField]
	private List<GRVendingMachine.VendingEntry> vendingEntries = new List<GRVendingMachine.VendingEntry>();

	private int hIndex;

	private int vIndex;

	private bool currentlyVending;

	private int vendingIndex;

	private Coroutine vendingCoroutine;

	public int VendingMachineId;

	private GhostReactor reactor;

	[Serializable]
	public struct VendingEntry
	{
		public int GetEntityTypeId()
		{
			if (!this.entityTypeIdSet)
			{
				this.entityTypeId = this.entityPrefab.gameObject.name.GetStaticHash();
				this.entityTypeIdSet = true;
			}
			return this.entityTypeId;
		}

		public Transform transportVisual;

		public GameEntity entityPrefab;

		public string itemName;

		private int entityTypeId;

		private bool entityTypeIdSet;
	}
}
