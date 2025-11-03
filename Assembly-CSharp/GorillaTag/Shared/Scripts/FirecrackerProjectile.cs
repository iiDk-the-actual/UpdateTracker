using System;
using System.Collections;
using GorillaTag.Cosmetics;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Shared.Scripts
{
	public class FirecrackerProjectile : MonoBehaviour, ITickSystemTick, IProjectile
	{
		public bool TickRunning { get; set; }

		public void Tick()
		{
			if (Time.time - this.timeCreated > this.forceBackToPoolAfterSec || Time.time - this.timeExploded > this.explosionTime)
			{
				UnityEvent<FirecrackerProjectile> onDetonationComplete = this.OnDetonationComplete;
				if (onDetonationComplete == null)
				{
					return;
				}
				onDetonationComplete.Invoke(this);
			}
		}

		private void OnEnable()
		{
			TickSystem<object>.AddCallbackTarget(this);
			this.m_timer.Start();
			this.timeExploded = float.PositiveInfinity;
			this.timeCreated = float.PositiveInfinity;
			this.collisionEntered = false;
			if (this.disableWhenHit)
			{
				this.disableWhenHit.SetActive(true);
			}
			UnityEvent onEnableObject = this.OnEnableObject;
			if (onEnableObject == null)
			{
				return;
			}
			onEnableObject.Invoke();
		}

		private void OnDisable()
		{
			TickSystem<object>.RemoveCallbackTarget(this);
			this.m_timer.Stop();
			if (this.useTransferrableObjectState)
			{
				UnityEvent onResetProjectileState = this.OnResetProjectileState;
				if (onResetProjectileState == null)
				{
					return;
				}
				onResetProjectileState.Invoke();
			}
		}

		private void Awake()
		{
			this.rb = base.GetComponent<Rigidbody>();
			this.audioSource = base.GetComponent<AudioSource>();
			this.m_timer.callback = new Action(this.Detonate);
		}

		private void Detonate()
		{
			this.m_timer.Stop();
			this.timeExploded = Time.time;
			if (this.disableWhenHit)
			{
				this.disableWhenHit.SetActive(false);
			}
			this.collisionEntered = false;
		}

		internal void SetTransferrableState(TransferrableObject.SyncOptions syncType, int state)
		{
			if (!this.useTransferrableObjectState)
			{
				return;
			}
			if (syncType != TransferrableObject.SyncOptions.Bool)
			{
				if (syncType != TransferrableObject.SyncOptions.Int)
				{
					return;
				}
				UnityEvent<int> onItemStateIntChanged = this.OnItemStateIntChanged;
				if (onItemStateIntChanged == null)
				{
					return;
				}
				onItemStateIntChanged.Invoke(state);
				return;
			}
			else
			{
				bool flag = (state & 1) != 0;
				bool flag2 = (state & 2) != 0;
				bool flag3 = (state & 4) != 0;
				bool flag4 = (state & 8) != 0;
				if (flag)
				{
					UnityEvent onItemStateBoolATrue = this.OnItemStateBoolATrue;
					if (onItemStateBoolATrue != null)
					{
						onItemStateBoolATrue.Invoke();
					}
				}
				else
				{
					UnityEvent onItemStateBoolAFalse = this.OnItemStateBoolAFalse;
					if (onItemStateBoolAFalse != null)
					{
						onItemStateBoolAFalse.Invoke();
					}
				}
				if (flag2)
				{
					UnityEvent onItemStateBoolBTrue = this.OnItemStateBoolBTrue;
					if (onItemStateBoolBTrue != null)
					{
						onItemStateBoolBTrue.Invoke();
					}
				}
				else
				{
					UnityEvent onItemStateBoolBFalse = this.OnItemStateBoolBFalse;
					if (onItemStateBoolBFalse != null)
					{
						onItemStateBoolBFalse.Invoke();
					}
				}
				if (flag3)
				{
					UnityEvent onItemStateBoolCTrue = this.OnItemStateBoolCTrue;
					if (onItemStateBoolCTrue != null)
					{
						onItemStateBoolCTrue.Invoke();
					}
				}
				else
				{
					UnityEvent onItemStateBoolCFalse = this.OnItemStateBoolCFalse;
					if (onItemStateBoolCFalse != null)
					{
						onItemStateBoolCFalse.Invoke();
					}
				}
				if (flag4)
				{
					UnityEvent onItemStateBoolDTrue = this.OnItemStateBoolDTrue;
					if (onItemStateBoolDTrue == null)
					{
						return;
					}
					onItemStateBoolDTrue.Invoke();
					return;
				}
				else
				{
					UnityEvent onItemStateBoolDFalse = this.OnItemStateBoolDFalse;
					if (onItemStateBoolDFalse == null)
					{
						return;
					}
					onItemStateBoolDFalse.Invoke();
					return;
				}
			}
		}

		public void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progress)
		{
			base.transform.position = startPosition;
			base.transform.rotation = startRotation;
			base.transform.localScale = Vector3.one * ownerRig.scaleFactor;
			this.rb.linearVelocity = velocity;
		}

		private void OnCollisionEnter(Collision other)
		{
			if (this.collisionEntered)
			{
				return;
			}
			Vector3 point = other.contacts[0].point;
			Vector3 normal = other.contacts[0].normal;
			UnityEvent<FirecrackerProjectile, Vector3> onCollisionEntered = this.OnCollisionEntered;
			if (onCollisionEntered != null)
			{
				onCollisionEntered.Invoke(this, normal);
			}
			if (this.sizzleDuration > 0f)
			{
				base.StartCoroutine(this.Sizzle(point, normal));
			}
			else
			{
				UnityEvent<FirecrackerProjectile, Vector3> onDetonationStart = this.OnDetonationStart;
				if (onDetonationStart != null)
				{
					onDetonationStart.Invoke(this, point);
				}
				this.Detonate(point, normal);
			}
			this.collisionEntered = true;
		}

		private IEnumerator Sizzle(Vector3 contactPoint, Vector3 normal)
		{
			if (this.audioSource && this.sizzleAudioClip != null)
			{
				this.audioSource.GTPlayOneShot(this.sizzleAudioClip, 1f);
			}
			yield return new WaitForSeconds(this.sizzleDuration);
			UnityEvent<FirecrackerProjectile, Vector3> onDetonationStart = this.OnDetonationStart;
			if (onDetonationStart != null)
			{
				onDetonationStart.Invoke(this, contactPoint);
			}
			this.Detonate(contactPoint, normal);
			yield break;
		}

		private void Detonate(Vector3 contactPoint, Vector3 normal)
		{
			this.timeExploded = Time.time;
			GameObject gameObject = ObjectPools.instance.Instantiate(this.explosionEffect, contactPoint, true);
			gameObject.transform.up = normal;
			gameObject.transform.position = base.transform.position;
			SoundBankPlayer soundBankPlayer;
			if (gameObject.TryGetComponent<SoundBankPlayer>(out soundBankPlayer) && soundBankPlayer.soundBank)
			{
				soundBankPlayer.Play();
			}
			if (this.disableWhenHit)
			{
				this.disableWhenHit.SetActive(false);
			}
			this.collisionEntered = false;
		}

		[SerializeField]
		private GameObject explosionEffect;

		[SerializeField]
		private float forceBackToPoolAfterSec = 20f;

		[SerializeField]
		private float explosionTime = 5f;

		[SerializeField]
		private GameObject disableWhenHit;

		[SerializeField]
		private float sizzleDuration;

		[SerializeField]
		private AudioClip sizzleAudioClip;

		[Space]
		public UnityEvent OnEnableObject;

		public UnityEvent<FirecrackerProjectile, Vector3> OnCollisionEntered;

		public UnityEvent<FirecrackerProjectile, Vector3> OnDetonationStart;

		public UnityEvent<FirecrackerProjectile> OnDetonationComplete;

		private Rigidbody rb;

		private float timeCreated = float.PositiveInfinity;

		private float timeExploded = float.PositiveInfinity;

		private AudioSource audioSource;

		private TickSystemTimer m_timer = new TickSystemTimer(40f);

		private bool collisionEntered;

		[SerializeField]
		private bool useTransferrableObjectState;

		[SerializeField]
		protected UnityEvent OnResetProjectileState;

		[SerializeField]
		protected string boolADebugName;

		[SerializeField]
		protected UnityEvent OnItemStateBoolATrue;

		[SerializeField]
		protected UnityEvent OnItemStateBoolAFalse;

		[SerializeField]
		protected string boolBDebugName;

		[SerializeField]
		protected UnityEvent OnItemStateBoolBTrue;

		[SerializeField]
		protected UnityEvent OnItemStateBoolBFalse;

		[SerializeField]
		protected string boolCDebugName;

		[SerializeField]
		protected UnityEvent OnItemStateBoolCTrue;

		[SerializeField]
		protected UnityEvent OnItemStateBoolCFalse;

		[SerializeField]
		protected string boolDDebugName;

		[SerializeField]
		protected UnityEvent OnItemStateBoolDTrue;

		[SerializeField]
		protected UnityEvent OnItemStateBoolDFalse;

		[SerializeField]
		protected UnityEvent<int> OnItemStateIntChanged;
	}
}
