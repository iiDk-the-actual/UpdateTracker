using System;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	public class GorillaPlayerTimerButton : MonoBehaviour
	{
		private void Awake()
		{
			this.materialProps = new MaterialPropertyBlock();
		}

		private void Start()
		{
			this.TryInit();
		}

		private void OnEnable()
		{
			this.TryInit();
		}

		private void TryInit()
		{
			if (this.isInitialized)
			{
				return;
			}
			if (PlayerTimerManager.instance == null)
			{
				return;
			}
			PlayerTimerManager.instance.OnTimerStopped.AddListener(new UnityAction<int, int>(this.OnTimerStopped));
			PlayerTimerManager.instance.OnLocalTimerStarted.AddListener(new UnityAction(this.OnLocalTimerStarted));
			if (this.isBothStartAndStop)
			{
				this.isStartButton = !PlayerTimerManager.instance.IsLocalTimerStarted();
			}
			this.isInitialized = true;
		}

		private void OnDisable()
		{
			if (PlayerTimerManager.instance != null)
			{
				PlayerTimerManager.instance.OnTimerStopped.RemoveListener(new UnityAction<int, int>(this.OnTimerStopped));
				PlayerTimerManager.instance.OnLocalTimerStarted.RemoveListener(new UnityAction(this.OnLocalTimerStarted));
			}
			this.isInitialized = false;
		}

		private void OnLocalTimerStarted()
		{
			if (this.isBothStartAndStop)
			{
				this.isStartButton = false;
			}
		}

		private void OnTimerStopped(int actorNum, int timeDelta)
		{
			if (this.isBothStartAndStop && actorNum == NetworkSystem.Instance.LocalPlayer.ActorNumber)
			{
				this.isStartButton = true;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!base.enabled)
			{
				return;
			}
			GorillaTriggerColliderHandIndicator componentInParent = other.GetComponentInParent<GorillaTriggerColliderHandIndicator>();
			if (componentInParent == null)
			{
				return;
			}
			if (Time.time < this.lastTriggeredTime + this.debounceTime)
			{
				return;
			}
			if (!NetworkSystem.Instance.InRoom)
			{
				return;
			}
			GorillaTagger.Instance.StartVibration(componentInParent.isLeftHand, GorillaTagger.Instance.tapHapticStrength, GorillaTagger.Instance.tapHapticDuration);
			this.mesh.GetPropertyBlock(this.materialProps);
			this.materialProps.SetColor(ShaderProps._BaseColor, this.pressColor);
			this.mesh.SetPropertyBlock(this.materialProps);
			PlayerTimerManager.instance.RequestTimerToggle(this.isStartButton);
			this.lastTriggeredTime = Time.time;
		}

		private void OnTriggerExit(Collider other)
		{
			if (!base.enabled)
			{
				return;
			}
			if (other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() == null)
			{
				return;
			}
			this.mesh.GetPropertyBlock(this.materialProps);
			this.materialProps.SetColor(ShaderProps._BaseColor, this.notPressedColor);
			this.mesh.SetPropertyBlock(this.materialProps);
		}

		private float lastTriggeredTime;

		[SerializeField]
		private bool isStartButton;

		[SerializeField]
		private bool isBothStartAndStop;

		[SerializeField]
		private float debounceTime = 0.5f;

		[SerializeField]
		private MeshRenderer mesh;

		[SerializeField]
		private Color pressColor;

		[SerializeField]
		private Color notPressedColor;

		private MaterialPropertyBlock materialProps;

		private bool isInitialized;
	}
}
