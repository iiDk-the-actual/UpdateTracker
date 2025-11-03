using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GRUIBuyItem : MonoBehaviour
{
	public void Setup(int standId)
	{
		this.standId = standId;
		this.buyItemButton.onPressButton.AddListener(new UnityAction(this.OnBuyItem));
		this.entityTypeId = this.entityPrefab.gameObject.name.GetStaticHash();
	}

	public void OnBuyItem()
	{
	}

	public Transform GetSpawnMarker()
	{
		return this.spawnMarker;
	}

	[SerializeField]
	private GorillaPressableButton buyItemButton;

	[SerializeField]
	private Text itemInfoLabel;

	[SerializeField]
	private Transform spawnMarker;

	[SerializeField]
	private GameEntity entityPrefab;

	private int entityTypeId;

	private int standId;
}
