using System;
using System.Collections;
using GorillaExtensions;
using GorillaGameModes;
using GT_CustomMapSupportRuntime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.VirtualStumpCustomMaps
{
	public class VirtualStumpReturnWatch : MonoBehaviour
	{
		private void Start()
		{
			if (this.returnButton != null)
			{
				this.returnButton.onStartPressingButton.AddListener(new UnityAction(this.OnStartedPressingButton));
				this.returnButton.onStopPressingButton.AddListener(new UnityAction(this.OnStoppedPressingButton));
				this.returnButton.onPressButton.AddListener(new UnityAction(this.OnButtonPressed));
			}
		}

		private void OnDestroy()
		{
			if (this.returnButton != null)
			{
				this.returnButton.onStartPressingButton.RemoveListener(new UnityAction(this.OnStartedPressingButton));
				this.returnButton.onStopPressingButton.RemoveListener(new UnityAction(this.OnStoppedPressingButton));
				this.returnButton.onPressButton.RemoveListener(new UnityAction(this.OnButtonPressed));
			}
		}

		public static void SetWatchProperties(VirtualStumpReturnWatchProps props)
		{
			VirtualStumpReturnWatch.currentCustomMapProps = props;
			VirtualStumpReturnWatch.currentCustomMapProps.holdDuration = Mathf.Clamp(VirtualStumpReturnWatch.currentCustomMapProps.holdDuration, 0.5f, 5f);
			VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Infection = Mathf.Clamp(VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Infection, 0.5f, 5f);
			VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Custom = Mathf.Clamp(VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Custom, 0.5f, 5f);
		}

		private float GetCurrentHoldDuration()
		{
			if (GorillaGameManager.instance.IsNull())
			{
				return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration;
			}
			switch (GorillaGameManager.instance.GameType())
			{
			case GameModeType.Infection:
				if (VirtualStumpReturnWatch.currentCustomMapProps.infectionOverride)
				{
					return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Infection;
				}
				return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration;
			case GameModeType.Custom:
				if (VirtualStumpReturnWatch.currentCustomMapProps.customModeOverride)
				{
					return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration_Custom;
				}
				return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration;
			}
			return VirtualStumpReturnWatch.currentCustomMapProps.holdDuration;
		}

		private void OnStartedPressingButton()
		{
			this.startPressingButtonTime = Time.time;
			this.currentlyBeingPressed = true;
			this.returnButton.pressDuration = this.GetCurrentHoldDuration();
			this.ShowCountdownText();
			this.updateCountdownCoroutine = base.StartCoroutine(this.UpdateCountdownText());
		}

		private void OnStoppedPressingButton()
		{
			this.currentlyBeingPressed = false;
			this.HideCountdownText();
			if (this.updateCountdownCoroutine != null)
			{
				base.StopCoroutine(this.updateCountdownCoroutine);
				this.updateCountdownCoroutine = null;
			}
		}

		private void OnButtonPressed()
		{
			this.currentlyBeingPressed = false;
			if (ZoneManagement.IsInZone(GTZone.customMaps) && !CustomMapManager.IsLocalPlayerInVirtualStump())
			{
				bool flag = VirtualStumpReturnWatch.currentCustomMapProps.shouldTagPlayer;
				bool flag2 = VirtualStumpReturnWatch.currentCustomMapProps.shouldKickPlayer;
				if (GorillaGameManager.instance.IsNotNull())
				{
					switch (GorillaGameManager.instance.GameType())
					{
					case GameModeType.Infection:
						if (VirtualStumpReturnWatch.currentCustomMapProps.infectionOverride)
						{
							flag = VirtualStumpReturnWatch.currentCustomMapProps.shouldTagPlayer_Infection;
							flag2 = VirtualStumpReturnWatch.currentCustomMapProps.shouldKickPlayer_Infection;
						}
						break;
					case GameModeType.Custom:
						if (VirtualStumpReturnWatch.currentCustomMapProps.customModeOverride)
						{
							flag = VirtualStumpReturnWatch.currentCustomMapProps.shouldTagPlayer_CustomMode;
							flag2 = VirtualStumpReturnWatch.currentCustomMapProps.shouldKickPlayer_CustomMode;
						}
						break;
					}
				}
				if (flag2 && NetworkSystem.Instance.InRoom && !NetworkSystem.Instance.SessionIsPrivate)
				{
					NetworkSystem.Instance.ReturnToSinglePlayer();
				}
				else if (flag)
				{
					GameMode.ReportHit();
				}
				CustomMapManager.ReturnToVirtualStump();
			}
		}

		private void ShowCountdownText()
		{
			if (this.countdownText.IsNull())
			{
				return;
			}
			int num = 1 + Mathf.FloorToInt(this.GetCurrentHoldDuration());
			this.countdownText.text = num.ToString();
			this.countdownText.gameObject.SetActive(true);
			if (this.buttonText.IsNotNull())
			{
				this.buttonText.gameObject.SetActive(false);
			}
		}

		private void HideCountdownText()
		{
			if (this.countdownText.IsNull())
			{
				return;
			}
			this.countdownText.text = "";
			this.countdownText.gameObject.SetActive(false);
			if (this.buttonText.IsNotNull())
			{
				this.buttonText.gameObject.SetActive(true);
			}
		}

		private IEnumerator UpdateCountdownText()
		{
			while (this.currentlyBeingPressed)
			{
				if (this.countdownText.IsNull())
				{
					yield break;
				}
				float num = this.GetCurrentHoldDuration() - (Time.time - this.startPressingButtonTime);
				int num2 = 1 + Mathf.FloorToInt(num);
				this.countdownText.text = num2.ToString();
				yield return null;
			}
			yield break;
		}

		[SerializeField]
		private HeldButton returnButton;

		[SerializeField]
		private TMP_Text buttonText;

		[SerializeField]
		private TMP_Text countdownText;

		private static VirtualStumpReturnWatchProps currentCustomMapProps;

		private float startPressingButtonTime = -1f;

		private bool currentlyBeingPressed;

		private Coroutine updateCountdownCoroutine;
	}
}
