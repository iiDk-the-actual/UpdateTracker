using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GRToolPurchaseStation : MonoBehaviour
{
	public int ActiveEntryIndex
	{
		get
		{
			return this.activeEntryIndex;
		}
	}

	public void Init(GhostReactorManager grManager, GhostReactor reactor)
	{
		this.grManager = grManager;
		this.reactor = reactor;
	}

	public void RequestPurchaseButton(int actorNumber)
	{
		if (actorNumber == NetworkSystem.Instance.LocalPlayer.ActorNumber)
		{
			this.grManager.ToolPurchaseStationRequest(this.PurchaseStationId, GhostReactorManager.ToolPurchaseStationAction.TryPurchase);
		}
	}

	public void ShiftRightButton()
	{
		this.grManager.ToolPurchaseStationRequest(this.PurchaseStationId, GhostReactorManager.ToolPurchaseStationAction.ShiftRight);
	}

	public void ShiftLeftButton()
	{
		this.grManager.ToolPurchaseStationRequest(this.PurchaseStationId, GhostReactorManager.ToolPurchaseStationAction.ShiftLeft);
	}

	public void ShiftRightAuthority()
	{
		this.activeEntryIndex = (this.activeEntryIndex + 1) % this.toolEntries.Count;
	}

	public void ShiftLeftAuthority()
	{
		this.activeEntryIndex = ((this.activeEntryIndex > 0) ? (this.activeEntryIndex - 1) : (this.toolEntries.Count - 1));
	}

	public void DebugPurchase()
	{
		int entityTypeId = this.toolEntries[this.activeEntryIndex].GetEntityTypeId();
		Vector3 localPosition = this.toolEntries[this.activeEntryIndex].displayToolParent.GetChild(0).localPosition;
		Quaternion localRotation = this.toolEntries[this.activeEntryIndex].displayToolParent.GetChild(0).localRotation;
		Quaternion quaternion = this.depositTransform.rotation * localRotation;
		Vector3 vector = this.depositTransform.position + this.depositTransform.rotation * localPosition;
		this.grManager.gameEntityManager.RequestCreateItem(entityTypeId, vector, quaternion, 0L);
		this.OnPurchaseSucceeded();
	}

	public bool TryPurchaseAuthority(GRPlayer player, out int itemCost)
	{
		int entityTypeId = this.toolEntries[this.activeEntryIndex].GetEntityTypeId();
		itemCost = this.reactor.GetItemCost(entityTypeId);
		if (this.debugIgnoreToolCost || player.ShiftCredits >= itemCost)
		{
			Vector3 localPosition = this.toolEntries[this.activeEntryIndex].displayToolParent.GetChild(0).localPosition;
			Quaternion localRotation = this.toolEntries[this.activeEntryIndex].displayToolParent.GetChild(0).localRotation;
			Quaternion quaternion = this.depositTransform.rotation * localRotation;
			Vector3 vector = this.depositTransform.position + this.depositTransform.rotation * localPosition;
			this.grManager.gameEntityManager.RequestCreateItem(entityTypeId, vector, quaternion, 0L);
			return true;
		}
		return false;
	}

	public void OnSelectionUpdate(int newSelectedIndex)
	{
		this.activeEntryIndex = Mathf.Clamp(newSelectedIndex % this.toolEntries.Count, 0, this.toolEntries.Count - 1);
		this.audioSource.PlayOneShot(this.nextItemAudio, this.nextItemVolume);
		this.displayItemNameText.text = this.toolEntries[this.activeEntryIndex].toolName;
		this.displayItemCostText.text = this.toolEntries[this.activeEntryIndex].toolCost.ToString();
	}

	public void OnPurchaseSucceeded()
	{
		this.animatingDeposit = true;
		this.animationStartTime = Time.time;
		this.audioSource.PlayOneShot(this.purchaseAudio, this.purchaseVolume);
		UnityEvent onSucceeded = this.idCardScanner.onSucceeded;
		if (onSucceeded != null)
		{
			onSucceeded.Invoke();
		}
		if (this.displayedEntryIndex < 0 || this.displayedEntryIndex >= this.toolEntries.Count)
		{
			this.displayedEntryIndex = this.activeEntryIndex;
		}
	}

	public void OnPurchaseFailed()
	{
		this.audioSource.PlayOneShot(this.purchaseFailedAudio, this.purchaseFailedVolume);
		UnityEvent onFailed = this.idCardScanner.onFailed;
		if (onFailed == null)
		{
			return;
		}
		onFailed.Invoke();
	}

	public Transform GetSpawnMarker()
	{
		return this.toolSpawnLocation;
	}

	public string GetCurrentToolName()
	{
		return this.toolEntries[this.activeEntryIndex].toolName;
	}

	private void Awake()
	{
		this.depositLidOpenRot = Quaternion.Euler(this.depositLidOpenEuler);
		this.toolEntryRot = Quaternion.Euler(this.toolEntryRotEuler);
		this.toolExitRot = Quaternion.Euler(this.toolExitRotEuler);
	}

	private void Update()
	{
		if (!this.animatingSwap && !this.animatingDeposit && this.activeEntryIndex != this.displayedEntryIndex)
		{
			this.animatingSwap = true;
			this.animationStartTime = Time.time;
			this.animPrevToolIndex = this.displayedEntryIndex;
			this.animNextToolIndex = this.activeEntryIndex;
			this.toolEntryRot = Quaternion.AngleAxis(this.toolEntryRotDegrees, Random.onUnitSphere);
		}
		if (this.animatingSwap)
		{
			float num = (Time.time - this.animationStartTime) / this.nextToolAnimationTime;
			Transform transform = null;
			if (this.animPrevToolIndex >= 0 && this.animPrevToolIndex < this.toolEntries.Count)
			{
				transform = this.toolEntries[this.animPrevToolIndex].displayToolParent;
				transform.localRotation = Quaternion.Slerp(Quaternion.identity, this.toolExitRot, this.toolExitRotTimingCurve.Evaluate(num));
				transform.localPosition = Vector3.Lerp(Vector3.zero, this.toolExitPosOffset, this.toolExitPosTimingCurve.Evaluate(num));
			}
			Transform displayToolParent = this.toolEntries[this.animNextToolIndex].displayToolParent;
			displayToolParent.localRotation = Quaternion.Slerp(this.toolEntryRot, Quaternion.identity, this.toolEntryRotTimingCurve.Evaluate(num));
			displayToolParent.localPosition = Vector3.Lerp(this.toolEntryPosOffset, Vector3.zero, this.toolEntryPosTimingCurve.Evaluate(num));
			displayToolParent.gameObject.SetActive(true);
			if (num >= 1f)
			{
				if (transform != null)
				{
					transform.gameObject.SetActive(false);
				}
				this.displayedEntryIndex = this.animNextToolIndex;
				this.animatingSwap = false;
				return;
			}
		}
		else if (this.animatingDeposit)
		{
			float num2 = (Time.time - this.animationStartTime) / this.toolDepositAnimationTime;
			Transform displayToolParent2 = this.toolEntries[this.displayedEntryIndex].displayToolParent;
			Vector3 localPosition = displayToolParent2.localPosition;
			localPosition.y = Mathf.Lerp(0f, this.depositTransform.localPosition.y, this.toolDepositMotionCurveY.Evaluate(this.toolDepositTimingCurve.Evaluate(num2)));
			localPosition.z = Mathf.Lerp(0f, this.depositTransform.localPosition.z, this.toolDepositMotionCurveZ.Evaluate(this.toolDepositTimingCurve.Evaluate(num2)));
			displayToolParent2.localPosition = localPosition;
			this.depositLidTransform.localRotation = Quaternion.Slerp(Quaternion.identity, this.depositLidOpenRot, this.depositLidTimingCurve.Evaluate(num2));
			if (num2 >= 1f)
			{
				this.depositLidTransform.localRotation = Quaternion.identity;
				displayToolParent2.gameObject.SetActive(false);
				this.displayedEntryIndex = -1;
				this.animatingDeposit = false;
			}
		}
	}

	[SerializeField]
	private List<GRToolPurchaseStation.ToolEntry> toolEntries = new List<GRToolPurchaseStation.ToolEntry>();

	[SerializeField]
	private Transform displayTransform;

	[SerializeField]
	private Transform depositTransform;

	[SerializeField]
	private Transform toolSpawnLocation;

	[SerializeField]
	private TMP_Text displayItemNameText;

	[SerializeField]
	private TMP_Text displayItemCostText;

	[SerializeField]
	private float nextToolAnimationTime = 0.5f;

	[SerializeField]
	private float toolDepositAnimationTime = 1f;

	[SerializeField]
	private Vector3 toolEntryPosOffset = new Vector3(0f, 0.25f, 0f);

	[SerializeField]
	private Vector3 toolEntryRotEuler = new Vector3(0f, 0f, 15f);

	[SerializeField]
	private float toolEntryRotDegrees = 15f;

	[SerializeField]
	private Vector3 toolExitPosOffset = new Vector3(0f, 0f, -0.25f);

	[SerializeField]
	private Vector3 toolExitRotEuler = new Vector3(180f, 0f, 0f);

	[SerializeField]
	private AnimationCurve toolEntryPosTimingCurve;

	[SerializeField]
	private AnimationCurve toolEntryRotTimingCurve;

	[SerializeField]
	private AnimationCurve toolExitPosTimingCurve;

	[SerializeField]
	private AnimationCurve toolExitRotTimingCurve;

	[SerializeField]
	private AnimationCurve toolDepositTimingCurve;

	[SerializeField]
	private AnimationCurve toolDepositMotionCurveY;

	[SerializeField]
	private AnimationCurve toolDepositMotionCurveZ;

	[SerializeField]
	private Transform depositLidTransform;

	[SerializeField]
	private Vector3 depositLidOpenEuler = new Vector3(65f, 0f, 0f);

	[SerializeField]
	private AnimationCurve depositLidTimingCurve;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private AudioClip nextItemAudio;

	[SerializeField]
	private float nextItemVolume = 0.5f;

	[SerializeField]
	private AudioClip purchaseAudio;

	[SerializeField]
	private float purchaseVolume = 0.5f;

	[SerializeField]
	private AudioClip purchaseFailedAudio;

	[SerializeField]
	private float purchaseFailedVolume = 0.5f;

	[SerializeField]
	private IDCardScanner idCardScanner;

	private int activeEntryIndex = 1;

	private int displayedEntryIndex = -1;

	private float animationStartTime;

	private bool animatingDeposit;

	private bool animatingSwap;

	private int animPrevToolIndex;

	private int animNextToolIndex;

	private Quaternion depositLidOpenRot = Quaternion.identity;

	private Quaternion toolEntryRot = Quaternion.identity;

	private Quaternion toolExitRot = Quaternion.identity;

	private Coroutine vendingCoroutine;

	private bool debugIgnoreToolCost;

	[HideInInspector]
	public int PurchaseStationId;

	private GhostReactorManager grManager;

	private GhostReactor reactor;

	[Serializable]
	public struct ToolEntry
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

		public Transform displayToolParent;

		public GameEntity entityPrefab;

		public string toolName;

		public int toolCost;

		private int entityTypeId;

		private bool entityTypeIdSet;
	}
}
