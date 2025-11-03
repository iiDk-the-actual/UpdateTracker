using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	[RequireComponent(typeof(TransferrableObject))]
	public class VenusFlyTrapHoldable : MonoBehaviour, ITickSystemTick
	{
		public bool TickRunning { get; set; }

		private void Awake()
		{
			this.transferrableObject = base.GetComponent<TransferrableObject>();
		}

		private void OnEnable()
		{
			TickSystem<object>.AddCallbackTarget(this);
			this.triggerEventNotifier.TriggerEnterEvent += this.TriggerEntered;
			this.state = VenusFlyTrapHoldable.VenusState.Open;
			this.localRotA = this.lipA.transform.localRotation;
			this.localRotB = this.lipB.transform.localRotation;
			if (this._events == null)
			{
				this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
				NetPlayer netPlayer = ((this.transferrableObject.myOnlineRig != null) ? this.transferrableObject.myOnlineRig.creator : ((this.transferrableObject.myRig != null) ? (this.transferrableObject.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : null));
				if (netPlayer != null)
				{
					this._events.Init(netPlayer);
				}
			}
			if (this._events != null)
			{
				this._events.Activate += this.OnTriggerEvent;
			}
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveCallbackTarget(this);
			this.triggerEventNotifier.TriggerEnterEvent -= this.TriggerEntered;
			if (this._events != null)
			{
				this._events.Activate -= this.OnTriggerEvent;
				this._events.Dispose();
				this._events = null;
			}
		}

		public void Tick()
		{
			if (this.transferrableObject.InHand() && this.audioSource && !this.audioSource.isPlaying && this.flyLoopingAudio != null)
			{
				this.audioSource.clip = this.flyLoopingAudio;
				this.audioSource.GTPlay();
			}
			if (!this.transferrableObject.InHand() && this.audioSource && this.audioSource.isPlaying)
			{
				this.audioSource.GTStop();
			}
			if (this.state == VenusFlyTrapHoldable.VenusState.Open)
			{
				return;
			}
			if (this.state == VenusFlyTrapHoldable.VenusState.Closed && Time.time - this.closedStartedTime >= this.closedDuration)
			{
				this.UpdateState(VenusFlyTrapHoldable.VenusState.Opening);
				if (this.audioSource && this.openingAudio != null)
				{
					this.audioSource.GTPlayOneShot(this.openingAudio, 1f);
				}
			}
			if (this.state == VenusFlyTrapHoldable.VenusState.Closing)
			{
				this.SmoothRotation(true);
				return;
			}
			if (this.state == VenusFlyTrapHoldable.VenusState.Opening)
			{
				this.SmoothRotation(false);
			}
		}

		private void SmoothRotation(bool isClosing)
		{
			if (isClosing)
			{
				Quaternion quaternion = Quaternion.Euler(this.targetRotationB);
				this.lipB.transform.localRotation = Quaternion.Lerp(this.lipB.transform.localRotation, quaternion, Time.deltaTime * this.speed);
				Quaternion quaternion2 = Quaternion.Euler(this.targetRotationA);
				this.lipA.transform.localRotation = Quaternion.Lerp(this.lipA.transform.localRotation, quaternion2, Time.deltaTime * this.speed);
				if (Quaternion.Angle(this.lipB.transform.localRotation, quaternion) < 1f && Quaternion.Angle(this.lipA.transform.localRotation, quaternion2) < 1f)
				{
					this.lipB.transform.localRotation = quaternion;
					this.lipA.transform.localRotation = quaternion2;
					this.UpdateState(VenusFlyTrapHoldable.VenusState.Closed);
					return;
				}
			}
			else
			{
				this.lipB.transform.localRotation = Quaternion.Lerp(this.lipB.transform.localRotation, this.localRotB, Time.deltaTime * this.speed / 2f);
				this.lipA.transform.localRotation = Quaternion.Lerp(this.lipA.transform.localRotation, this.localRotA, Time.deltaTime * this.speed / 2f);
				if (Quaternion.Angle(this.lipB.transform.localRotation, this.localRotB) < 1f && Quaternion.Angle(this.lipA.transform.localRotation, this.localRotA) < 1f)
				{
					this.lipB.transform.localRotation = this.localRotB;
					this.lipA.transform.localRotation = this.localRotA;
					this.UpdateState(VenusFlyTrapHoldable.VenusState.Open);
				}
			}
		}

		private void UpdateState(VenusFlyTrapHoldable.VenusState newState)
		{
			this.state = newState;
			if (this.state == VenusFlyTrapHoldable.VenusState.Closed)
			{
				this.closedStartedTime = Time.time;
			}
		}

		private void TriggerEntered(TriggerEventNotifier notifier, Collider other)
		{
			if (this.state != VenusFlyTrapHoldable.VenusState.Open)
			{
				return;
			}
			if (!other.gameObject.IsOnLayer(this.layers))
			{
				return;
			}
			if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
			{
				this._events.Activate.RaiseOthers(Array.Empty<object>());
			}
			this.OnTriggerLocal();
			GorillaTriggerColliderHandIndicator componentInChildren = other.GetComponentInChildren<GorillaTriggerColliderHandIndicator>();
			if (componentInChildren == null)
			{
				return;
			}
			GorillaTagger.Instance.StartVibration(componentInChildren.isLeftHand, this.hapticStrength, this.hapticDuration);
		}

		private void OnTriggerEvent(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "OnTriggerEvent");
			if (!this.callLimiter.CheckCallTime(Time.time))
			{
				return;
			}
			this.OnTriggerLocal();
		}

		private void OnTriggerLocal()
		{
			this.UpdateState(VenusFlyTrapHoldable.VenusState.Closing);
			if (this.audioSource && this.closingAudio != null)
			{
				this.audioSource.GTPlayOneShot(this.closingAudio, 1f);
			}
		}

		[SerializeField]
		private GameObject lipA;

		[SerializeField]
		private GameObject lipB;

		[SerializeField]
		private Vector3 targetRotationA;

		[SerializeField]
		private Vector3 targetRotationB;

		[SerializeField]
		private float closedDuration = 3f;

		[SerializeField]
		private float speed = 2f;

		[SerializeField]
		private UnityLayer layers;

		[SerializeField]
		private TriggerEventNotifier triggerEventNotifier;

		[SerializeField]
		private float hapticStrength = 0.5f;

		[SerializeField]
		private float hapticDuration = 0.1f;

		[SerializeField]
		private GameObject bug;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private AudioClip closingAudio;

		[SerializeField]
		private AudioClip openingAudio;

		[SerializeField]
		private AudioClip flyLoopingAudio;

		private CallLimiter callLimiter = new CallLimiter(10, 2f, 0.5f);

		private float closedStartedTime;

		private VenusFlyTrapHoldable.VenusState state;

		private Quaternion localRotA;

		private Quaternion localRotB;

		private RubberDuckEvents _events;

		private TransferrableObject transferrableObject;

		private enum VenusState
		{
			Closed,
			Open,
			Closing,
			Opening
		}
	}
}
