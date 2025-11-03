using System;
using System.Collections;
using GorillaTagScripts;
using UnityEngine;

public class PartyGameModeWarning : MonoBehaviour
{
	public bool ShouldShowWarning
	{
		get
		{
			return FriendshipGroupDetection.Instance.IsInParty && FriendshipGroupDetection.Instance.AnyPartyMembersOutsideFriendCollider();
		}
	}

	private void Awake()
	{
		GameObject[] array = this.showParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
	}

	public void Show()
	{
		this.visibleUntilTimestamp = Time.time + this.visibleDuration;
		if (this.hideCoroutine == null)
		{
			this.hideCoroutine = base.StartCoroutine(this.HideCo());
		}
	}

	private IEnumerator HideCo()
	{
		GameObject[] array = this.showParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
		array = this.hideParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		float lastVisible;
		do
		{
			lastVisible = this.visibleUntilTimestamp;
			yield return new WaitForSeconds(this.visibleUntilTimestamp - Time.time);
		}
		while (lastVisible != this.visibleUntilTimestamp);
		array = this.showParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		array = this.hideParts;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(true);
		}
		this.hideCoroutine = null;
		yield break;
	}

	[SerializeField]
	private GameObject[] showParts;

	[SerializeField]
	private GameObject[] hideParts;

	[SerializeField]
	private float visibleDuration;

	private float visibleUntilTimestamp;

	private Coroutine hideCoroutine;
}
