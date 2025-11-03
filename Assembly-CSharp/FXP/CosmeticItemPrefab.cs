using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GameObjectScheduling;
using GorillaExtensions;
using GorillaNetworking;
using GorillaNetworking.Store;
using TMPro;
using UnityEngine;

namespace FXP
{
	public class CosmeticItemPrefab : MonoBehaviour
	{
		private void Awake()
		{
			this.JonsAwakeCode();
		}

		private void JonsAwakeCode()
		{
			this.lastUpdated = -this.updateClock;
			this.isValid = this.goPedestal && this.goMannequin && this.goCosmeticItem && this.goCosmeticItemNameplate && this.goClock && this.goPreviewMode && this.goAttractMode && this.goPurchaseMode;
			this.goPreviewModeSFX = this.goPreviewMode.transform.GetComponentInChildren<AudioSource>();
			this.goAttractModeSFX = this.goAttractMode.transform.FindChildRecursive("SFXAttractMode").GetComponent<AudioSource>();
			this.goPurchaseModeSFX = this.goPurchaseMode.transform.FindChildRecursive("SFXPurchaseMode").GetComponent<AudioSource>();
			this.goAttractModeVFX = this.goAttractMode.transform.FindChildRecursive("VFXAttractMode").GetComponent<ParticleSystem>();
			this.goPurchaseModeVFX = this.goPurchaseMode.transform.FindChildRecursive("VFXPurchaseMode").GetComponent<ParticleSystem>();
			this.clockTextMesh = this.goClock.GetComponent<TextMeshPro>();
			this.clockTextMeshIsValid = this.clockTextMesh != null;
			if (this.clockTextMeshIsValid)
			{
				this.defaultCountdownTextTemplate = this.clockTextMesh.text;
			}
			this.isValid = this.goPreviewModeSFX && this.goAttractModeSFX && this.goPurchaseModeSFX;
		}

		private void OnDisable()
		{
			if (StoreUpdater.instance != null)
			{
				this.countdownTimerCoRoutine = null;
				this.StopCountdownCoroutine();
				StoreUpdater.instance.PedestalAsleep(this);
			}
		}

		private void OnEnable()
		{
			if (this.goPreviewModeSFX == null)
			{
				this.goPreviewModeSFX = this.goPreviewMode.transform.GetComponentInChildren<AudioSource>();
			}
			if (this.goAttractModeSFX == null)
			{
				this.goAttractModeSFX = this.goAttractMode.transform.transform.GetComponentInChildren<AudioSource>();
			}
			if (this.goPurchaseModeSFX == null)
			{
				this.goPurchaseModeSFX = this.goPurchaseMode.transform.transform.GetComponentInChildren<AudioSource>();
			}
			this.isValid = this.goPreviewModeSFX && this.goAttractModeSFX && this.goPurchaseModeSFX;
			if (StoreUpdater.instance != null)
			{
				StoreUpdater.instance.PedestalAwakened(this);
			}
		}

		public void SwitchDisplayMode(CosmeticItemPrefab.EDisplayMode NewDisplayMode)
		{
			if (!this.isValid)
			{
				return;
			}
			if (NewDisplayMode.Equals(CosmeticItemPrefab.EDisplayMode.NULL))
			{
				return;
			}
			if (NewDisplayMode == this.currentDisplayMode)
			{
				return;
			}
			switch (NewDisplayMode)
			{
			case CosmeticItemPrefab.EDisplayMode.HIDDEN:
			{
				this.goPedestal.SetActive(false);
				this.goMannequin.SetActive(false);
				this.goCosmeticItem.SetActive(false);
				this.goCosmeticItemNameplate.SetActive(false);
				this.goClock.SetActive(false);
				this.goPreviewMode.SetActive(false);
				AudioSource audioSource = this.goPreviewModeSFX;
				if (audioSource != null)
				{
					audioSource.GTStop();
				}
				this.goAttractMode.SetActive(false);
				AudioSource audioSource2 = this.goAttractModeSFX;
				if (audioSource2 != null)
				{
					audioSource2.GTStop();
				}
				this.goPurchaseMode.SetActive(false);
				AudioSource audioSource3 = this.goPurchaseModeSFX;
				if (audioSource3 != null)
				{
					audioSource3.GTStop();
				}
				this.StopPreviewTimer();
				this.StopAttractTimer();
				break;
			}
			case CosmeticItemPrefab.EDisplayMode.PREVIEW:
				this.goPedestal.SetActive(true);
				this.goMannequin.SetActive(true);
				this.goCosmeticItem.SetActive(true);
				this.goCosmeticItemNameplate.SetActive(false);
				this.goClock.SetActive(true);
				this.goAttractMode.SetActive(false);
				this.goAttractModeSFX.GTStop();
				this.goPurchaseMode.SetActive(false);
				this.goPurchaseModeSFX.GTStop();
				this.goPreviewMode.SetActive(true);
				this.goPreviewModeSFX.GTPlay();
				this.StopPreviewTimer();
				this.StartPreviewTimer();
				break;
			case CosmeticItemPrefab.EDisplayMode.ATTRACT:
				this.goPedestal.SetActive(true);
				this.goMannequin.SetActive(true);
				this.goCosmeticItem.SetActive(true);
				this.goCosmeticItemNameplate.SetActive(true);
				this.goClock.SetActive(true);
				this.goPreviewMode.SetActive(false);
				this.goPreviewModeSFX.GTStop();
				this.goPurchaseMode.SetActive(false);
				this.goPurchaseModeSFX.GTStop();
				this.goAttractMode.SetActive(true);
				this.goAttractModeSFX.GTPlay();
				this.StopPreviewTimer();
				this.StartAttractTimer();
				break;
			case CosmeticItemPrefab.EDisplayMode.PURCHASE:
				this.goPedestal.SetActive(true);
				this.goMannequin.SetActive(true);
				this.goCosmeticItem.SetActive(true);
				this.goCosmeticItemNameplate.SetActive(true);
				this.goClock.SetActive(false);
				this.goPreviewMode.SetActive(false);
				this.goPreviewModeSFX.GTStop();
				this.goAttractMode.SetActive(false);
				this.goAttractModeSFX.GTStop();
				this.goPurchaseMode.SetActive(true);
				this.goPurchaseModeSFX.GTPlay();
				this.goCosmeticItemNameplate.GetComponent<TextMesh>().text = "Purchased!";
				this.StopPreviewTimer();
				break;
			case CosmeticItemPrefab.EDisplayMode.POSTPURCHASE:
				this.goPedestal.SetActive(true);
				this.goMannequin.SetActive(true);
				this.goCosmeticItem.SetActive(true);
				this.goCosmeticItemNameplate.SetActive(false);
				this.goClock.SetActive(false);
				this.goPreviewMode.SetActive(false);
				this.goPreviewModeSFX.GTStop();
				this.goAttractMode.SetActive(false);
				this.goAttractModeSFX.GTStop();
				this.goPurchaseMode.SetActive(false);
				this.goPurchaseModeSFX.GTStop();
				this.StopPreviewTimer();
				break;
			}
			this.currentDisplayMode = NewDisplayMode;
		}

		private void Update()
		{
			if (Time.time > this.lastUpdated + this.updateClock)
			{
				this.lastUpdated = Time.time;
				this.UpdateClock();
			}
		}

		private void UpdateClock()
		{
			if (this.currentUpdateEvent != null && this.clockTextMeshIsValid && this.clockTextMesh.isActiveAndEnabled)
			{
				TimeSpan timeSpan = this.currentUpdateEvent.EndTimeUTC.ToUniversalTime() - StoreUpdater.instance.DateTimeNowServerAdjusted;
				this.clockTextMesh.text = CountdownText.GetTimeDisplay(timeSpan, this.defaultCountdownTextTemplate);
			}
		}

		public void SetDefaultProperties()
		{
			if (!this.isValid)
			{
				return;
			}
			this.goPedestal.GetComponent<MeshFilter>().sharedMesh = this.defaultPedestalMesh;
			this.goPedestal.GetComponent<MeshRenderer>().sharedMaterial = this.defaultPedestalMaterial;
			this.goMannequin.GetComponent<MeshFilter>().sharedMesh = this.defaultMannequinMesh;
			this.goMannequin.GetComponent<MeshRenderer>().sharedMaterial = this.defaultMannequinMaterial;
			this.goCosmeticItem.GetComponent<MeshFilter>().sharedMesh = this.defaultCosmeticMesh;
			this.goCosmeticItem.GetComponent<MeshRenderer>().sharedMaterial = this.defaultCosmeticMaterial;
			this.goCosmeticItemNameplate.GetComponent<TextMesh>().text = this.defaultItemText;
			this.goPreviewModeSFX.clip = this.defaultSFXPreviewMode;
			this.goAttractModeSFX.clip = this.defaultSFXAttractMode;
			this.goPurchaseModeSFX.clip = this.defaultSFXPurchaseMode;
		}

		private void ClearCosmeticMesh()
		{
			Object.Destroy(this.goCosmeticItemGameObject);
		}

		private void ClearCosmeticAtlas()
		{
			if (this.goCosmeticItemMeshAtlas.IsNotNull())
			{
				Object.Destroy(this.goCosmeticItemMeshAtlas);
			}
		}

		public void SetCosmeticItemFromCosmeticController(CosmeticsController.CosmeticItem item)
		{
			if (!this.isValid)
			{
				return;
			}
			this.ClearCosmeticAtlas();
			this.ClearCosmeticMesh();
			this.oldItemID = this.itemID;
			this.itemID = item.itemName;
			this.itemName = item.displayName;
			if (item.overrideDisplayName != string.Empty)
			{
				this.itemName = item.overrideDisplayName;
			}
			this.HeadModel.SetCosmeticActive(this.itemID, false);
			this.SetCosmeticStand();
		}

		public void SetCosmeticStand()
		{
			this.cosmeticStand.thisCosmeticName = this.itemID;
			this.cosmeticStand.InitializeCosmetic();
			if (this.oldItemID.Length > 0)
			{
				if (this.oldItemID != this.itemID)
				{
					this.cosmeticStand.isOn = false;
				}
				this.cosmeticStand.UpdateColor();
			}
		}

		public void SetStoreUpdateEvent(StoreUpdateEvent storeUpdateEvent, bool playFX)
		{
			if (!this.isValid)
			{
				return;
			}
			if (playFX)
			{
				this.goAttractMode.SetActive(true);
				this.goAttractModeVFX.Play();
			}
			this.currentUpdateEvent = storeUpdateEvent;
			this.SetCosmeticItemFromCosmeticController(CosmeticsController.instance.GetItemFromDict(storeUpdateEvent.ItemName));
			if (base.isActiveAndEnabled)
			{
				this.countdownTimerCoRoutine = base.StartCoroutine(this.PlayCountdownTimer());
			}
			this.UpdateClock();
		}

		private IEnumerator PlayCountdownTimer()
		{
			yield return new WaitForSeconds(Mathf.Clamp((float)((this.currentUpdateEvent.EndTimeUTC.ToUniversalTime() - StoreUpdater.instance.DateTimeNowServerAdjusted).TotalSeconds - 10.0), 0f, float.MaxValue));
			this.PlaySFX();
			yield break;
		}

		public void StopCountdownCoroutine()
		{
			this.CountdownSFX.GTStop();
			this.goAttractModeVFX.Stop();
			if (this.countdownTimerCoRoutine != null)
			{
				base.StopCoroutine(this.countdownTimerCoRoutine);
				this.countdownTimerCoRoutine = null;
			}
		}

		private void PlaySFX()
		{
			if (this.currentUpdateEvent != null)
			{
				TimeSpan timeSpan = this.currentUpdateEvent.EndTimeUTC.ToUniversalTime() - StoreUpdater.instance.DateTimeNowServerAdjusted;
				if (timeSpan.TotalSeconds >= 10.0)
				{
					this.CountdownSFX.time = 0f;
					this.CountdownSFX.GTPlay();
					return;
				}
				this.CountdownSFX.time = 10f - (float)timeSpan.TotalSeconds;
				this.CountdownSFX.GTPlay();
			}
		}

		public void SetCosmeticItemProperties(string WhichGUID, string Name, List<Transform> SocketsList, int Socket, string PedestalMesh = null, string MannequinMesh = null)
		{
			if (!this.isValid)
			{
				return;
			}
			Guid guid;
			if (!Guid.TryParse(WhichGUID, out guid))
			{
				return;
			}
			this.itemName = Name;
			this.itemSocket = Socket;
			if (this.pedestalMesh != null)
			{
				this.goPedestal.GetComponent<MeshFilter>().sharedMesh = this.pedestalMesh;
			}
		}

		private void StartPreviewTimer()
		{
			if (!this.isValid)
			{
				return;
			}
			if (this.coroutinePreviewTimer != null)
			{
				base.StopCoroutine(this.coroutinePreviewTimer);
				this.coroutinePreviewTimer = null;
			}
			this.coroutinePreviewTimer = this.DoPreviewTimer(DateTime.UtcNow + TimeSpan.FromSeconds((double)((this.hoursInPreviewMode ?? this.defaultHoursInPreviewMode) * 60 * 60)));
			base.StartCoroutine(this.coroutinePreviewTimer);
		}

		private void StopPreviewTimer()
		{
			if (!this.isValid)
			{
				return;
			}
			if (this.coroutinePreviewTimer != null)
			{
				base.StopCoroutine(this.coroutinePreviewTimer);
				this.coroutinePreviewTimer = null;
			}
			this.clockTextMesh.text = "Clock";
		}

		private IEnumerator DoPreviewTimer(DateTime ReleaseTime)
		{
			if (this.isValid)
			{
				bool timerDone = false;
				TimeSpan remainingTime = ReleaseTime - DateTime.UtcNow;
				while (!timerDone)
				{
					string text;
					int delayTime;
					if (remainingTime.TotalSeconds <= 59.0)
					{
						text = remainingTime.Seconds.ToString() + "s";
						delayTime = 1;
					}
					else
					{
						delayTime = 60;
						text = string.Empty;
						if (remainingTime.Days > 0)
						{
							text = text + remainingTime.Days.ToString() + "d ";
						}
						if (remainingTime.Hours > 0)
						{
							text = text + remainingTime.Hours.ToString() + "h ";
						}
						if (remainingTime.Minutes > 0)
						{
							text = text + remainingTime.Minutes.ToString() + "m ";
						}
						text = text.TrimEnd();
					}
					this.clockTextMesh.text = text;
					yield return new WaitForSecondsRealtime((float)delayTime);
					remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds((double)delayTime));
					if (remainingTime.TotalSeconds <= 0.0)
					{
						timerDone = true;
					}
				}
				this.SwitchDisplayMode(CosmeticItemPrefab.EDisplayMode.ATTRACT);
				yield return null;
				remainingTime = default(TimeSpan);
			}
			yield break;
		}

		public void StartAttractTimer()
		{
			if (!this.isValid)
			{
				return;
			}
			if (this.coroutineAttractTimer != null)
			{
				base.StopCoroutine(this.coroutineAttractTimer);
				this.coroutineAttractTimer = null;
			}
			this.coroutineAttractTimer = this.DoAttractTimer(DateTime.UtcNow + TimeSpan.FromSeconds((double)((this.hoursInAttractMode ?? this.defaultHoursInAttractMode) * 60 * 60)));
			base.StartCoroutine(this.coroutineAttractTimer);
		}

		private void StopAttractTimer()
		{
			if (!this.isValid)
			{
				return;
			}
			if (this.coroutineAttractTimer != null)
			{
				base.StopCoroutine(this.coroutineAttractTimer);
				this.coroutineAttractTimer = null;
			}
			this.goClock.GetComponent<TextMesh>().text = "Clock";
		}

		private IEnumerator DoAttractTimer(DateTime ReleaseTime)
		{
			if (this.isValid)
			{
				bool timerDone = false;
				TimeSpan remainingTime = ReleaseTime - DateTime.UtcNow;
				while (!timerDone)
				{
					string text;
					int delayTime;
					if (remainingTime.TotalSeconds <= 59.0)
					{
						text = remainingTime.Seconds.ToString() + "s";
						delayTime = 1;
					}
					else
					{
						delayTime = 60;
						text = string.Empty;
						if (remainingTime.Days > 0)
						{
							text = text + remainingTime.Days.ToString() + "d ";
						}
						if (remainingTime.Hours > 0)
						{
							text = text + remainingTime.Hours.ToString() + "h ";
						}
						if (remainingTime.Minutes > 0)
						{
							text = text + remainingTime.Minutes.ToString() + "m ";
						}
						text = text.TrimEnd();
					}
					this.goClock.GetComponent<TextMesh>().text = text;
					yield return new WaitForSecondsRealtime((float)delayTime);
					remainingTime = remainingTime.Subtract(TimeSpan.FromSeconds((double)delayTime));
					if (remainingTime.TotalSeconds <= 0.0)
					{
						timerDone = true;
					}
				}
				this.SwitchDisplayMode(CosmeticItemPrefab.EDisplayMode.HIDDEN);
				yield return null;
				remainingTime = default(TimeSpan);
			}
			yield break;
		}

		public string PedestalID = "";

		public HeadModel HeadModel;

		[SerializeField]
		private Guid? itemGUID;

		[SerializeField]
		private string itemName = string.Empty;

		[SerializeField]
		private List<Transform> sockets = new List<Transform>();

		[SerializeField]
		private int itemSocket = int.MinValue;

		[SerializeField]
		private int? hoursInPreviewMode;

		[SerializeField]
		private int? hoursInAttractMode;

		[SerializeField]
		private Mesh pedestalMesh;

		[SerializeField]
		private Mesh mannequinMesh;

		[SerializeField]
		private Mesh cosmeticMesh;

		[SerializeField]
		private AudioClip sfxPreviewMode;

		[SerializeField]
		private AudioClip sfxAttractMode;

		[SerializeField]
		private AudioClip sfxPurchaseMode;

		[SerializeField]
		private ParticleSystem vfxPreviewMode;

		[SerializeField]
		private ParticleSystem vfxAttractMode;

		[SerializeField]
		private ParticleSystem vfxPurchaseMode;

		[SerializeField]
		private GameObject goPedestal;

		[SerializeField]
		private GameObject goMannequin;

		[SerializeField]
		private GameObject goCosmeticItem;

		[SerializeField]
		private GameObject goCosmeticItemGameObject;

		[SerializeField]
		private GameObject goCosmeticItemNameplate;

		[SerializeField]
		private GameObject goClock;

		[SerializeField]
		private GameObject goPreviewMode;

		[SerializeField]
		private GameObject goAttractMode;

		[SerializeField]
		private GameObject goPurchaseMode;

		[SerializeField]
		private Mesh defaultPedestalMesh;

		[SerializeField]
		private Material defaultPedestalMaterial;

		[SerializeField]
		private Mesh defaultMannequinMesh;

		[SerializeField]
		private Material defaultMannequinMaterial;

		[SerializeField]
		private Mesh defaultCosmeticMesh;

		[SerializeField]
		private Material defaultCosmeticMaterial;

		[SerializeField]
		private string defaultItemText;

		[SerializeField]
		private int defaultHoursInPreviewMode;

		[SerializeField]
		private int defaultHoursInAttractMode;

		[SerializeField]
		private AudioClip defaultSFXPreviewMode;

		[SerializeField]
		private AudioClip defaultSFXAttractMode;

		[SerializeField]
		private AudioClip defaultSFXPurchaseMode;

		private GameObject goCosmeticItemMeshAtlas;

		public AudioSource CountdownSFX;

		private CosmeticItemPrefab.EDisplayMode currentDisplayMode;

		private bool isValid;

		[Nullable(2)]
		private AudioSource goPreviewModeSFX;

		[Nullable(2)]
		private AudioSource goAttractModeSFX;

		[Nullable(2)]
		private AudioSource goPurchaseModeSFX;

		[Nullable(2)]
		private ParticleSystem goAttractModeVFX;

		[Nullable(2)]
		private ParticleSystem goPurchaseModeVFX;

		private IEnumerator coroutinePreviewTimer;

		private IEnumerator coroutineAttractTimer;

		private DateTime startTime;

		private TextMeshPro clockTextMesh;

		private bool clockTextMeshIsValid;

		private StoreUpdateEvent currentUpdateEvent;

		private string defaultCountdownTextTemplate = "";

		public CosmeticStand cosmeticStand;

		public string itemID = "";

		public string oldItemID = "";

		private Coroutine countdownTimerCoRoutine;

		private float updateClock = 60f;

		private float lastUpdated;

		[SerializeField]
		public enum EDisplayMode
		{
			NULL,
			HIDDEN,
			PREVIEW,
			ATTRACT,
			PURCHASE,
			POSTPURCHASE
		}
	}
}
