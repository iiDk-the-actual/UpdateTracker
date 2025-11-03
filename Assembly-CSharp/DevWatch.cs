using System;
using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DevWatch : MonoBehaviour
{
	private void Awake()
	{
		this.SearchButton.SearchEvent.AddListener(new UnityAction(this.SearchItems));
		this.TakeOwnershipButton.onClick.AddListener(new UnityAction(this.TakeOwneshipOfItem));
		this.DestroyObjectButton.onClick.AddListener(new UnityAction(this.TryDestroyItem));
	}

	public void SearchItems()
	{
		this.FoundNetworkObjects.Clear();
		RaycastHit[] array = Physics.SphereCastAll(new Ray(this.RayCastStartPos.position, this.RayCastDirection.position - this.RayCastStartPos.position), 0.3f, 100f);
		if (array.Length != 0)
		{
			foreach (RaycastHit raycastHit in array)
			{
				NetworkObject networkObject;
				if (raycastHit.collider.gameObject.TryGetComponent<NetworkObject>(out networkObject))
				{
					this.FoundNetworkObjects.Add(networkObject);
				}
			}
		}
	}

	public void Cleanup()
	{
		this.FoundNetworkObjects.Clear();
		if (this.Items.Count > 0)
		{
			for (int i = this.Items.Count - 1; i >= 0; i--)
			{
				Object.Destroy(this.Items[i]);
			}
		}
		this.Items.Clear();
		this.Panel1.SetActive(true);
		this.Panel2.SetActive(false);
	}

	public void ItemSelected(DevWatchSelectableItem item)
	{
		this.Panel1.SetActive(false);
		this.Panel2.SetActive(true);
		this.SelectedItem = item;
		this.SelectedItemName.text = item.ItemName.text;
	}

	public void TryDestroyItem()
	{
	}

	public void TakeOwneshipOfItem()
	{
	}

	public DevWatchButton SearchButton;

	public GameObject Panel1;

	public GameObject Panel2;

	public DevWatchSelectableItem SelectableItemPrefab;

	public List<DevWatchSelectableItem> Items;

	public Transform RayCastStartPos;

	public Transform RayCastDirection;

	public Transform ItemsFoundContainer;

	public Button TakeOwnershipButton;

	public Button DestroyObjectButton;

	public List<NetworkObject> FoundNetworkObjects = new List<NetworkObject>();

	public TextMeshProUGUI SelectedItemName;

	public DevWatchSelectableItem SelectedItem;
}
