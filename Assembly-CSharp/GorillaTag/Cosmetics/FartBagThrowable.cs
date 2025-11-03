using System;
using GorillaExtensions;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	public class FartBagThrowable : MonoBehaviour, IProjectile
	{
		public TransferrableObject ParentTransferable { get; set; }

		public event Action<IProjectile> OnDeflated;

		private void OnEnable()
		{
			this.placedOnFloor = false;
			this.deflated = false;
			this.handContactPoint = Vector3.negativeInfinity;
			this.handNormalVector = Vector3.zero;
			this.timeCreated = float.PositiveInfinity;
			this.placedOnFloorTime = float.PositiveInfinity;
			if (this.updateBlendShapeCosmetic)
			{
				this.updateBlendShapeCosmetic.ResetBlend();
			}
		}

		private void Update()
		{
			if (Time.time - this.timeCreated > this.forceDestroyAfterSec)
			{
				this.DeflateLocal();
			}
		}

		public void Launch(Vector3 startPosition, Quaternion startRotation, Vector3 velocity, float chargeFrac, VRRig ownerRig, int progress)
		{
			base.transform.position = startPosition;
			base.transform.rotation = startRotation;
			base.transform.localScale = Vector3.one * ownerRig.scaleFactor;
			this.rigidbody.linearVelocity = velocity;
			this.timeCreated = Time.time;
			this.InitialPhotonEvent();
		}

		private void InitialPhotonEvent()
		{
			this._events = base.gameObject.GetOrAddComponent<RubberDuckEvents>();
			if (this.ParentTransferable)
			{
				NetPlayer netPlayer = ((this.ParentTransferable.myOnlineRig != null) ? this.ParentTransferable.myOnlineRig.creator : ((this.ParentTransferable.myRig != null) ? (this.ParentTransferable.myRig.creator ?? NetworkSystem.Instance.LocalPlayer) : null));
				if (this._events != null && netPlayer != null)
				{
					this._events.Init(netPlayer);
				}
			}
			if (this._events != null)
			{
				this._events.Activate += this.DeflateEvent;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if ((this.handLayerMask.value & (1 << other.gameObject.layer)) != 0)
			{
				if (!this.placedOnFloor)
				{
					return;
				}
				this.handContactPoint = other.ClosestPoint(base.transform.position);
				this.handNormalVector = (this.handContactPoint - base.transform.position).normalized;
				if (Time.time - this.placedOnFloorTime > 0.3f)
				{
					this.Deflate();
				}
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			if ((this.floorLayerMask.value & (1 << other.gameObject.layer)) != 0)
			{
				this.placedOnFloor = true;
				this.placedOnFloorTime = Time.time;
				Vector3 normal = other.contacts[0].normal;
				base.transform.position = other.contacts[0].point + normal * this.placementOffset;
				Quaternion quaternion = Quaternion.LookRotation(Vector3.ProjectOnPlane(base.transform.forward, normal).normalized, normal);
				base.transform.rotation = quaternion;
			}
		}

		private void Deflate()
		{
			if (PhotonNetwork.InRoom && this._events != null && this._events.Activate != null)
			{
				this._events.Activate.RaiseOthers(new object[] { this.handContactPoint, this.handNormalVector });
			}
			this.DeflateLocal();
		}

		private void DeflateEvent(int sender, int target, object[] args, PhotonMessageInfoWrapped info)
		{
			if (sender != target)
			{
				return;
			}
			if (args.Length != 2)
			{
				return;
			}
			GorillaNot.IncrementRPCCall(info, "DeflateEvent");
			if (this.callLimiter.CheckCallTime(Time.time))
			{
				object obj = args[0];
				if (obj is Vector3)
				{
					Vector3 vector = (Vector3)obj;
					obj = args[1];
					if (obj is Vector3)
					{
						Vector3 vector2 = (Vector3)obj;
						float num = 10000f;
						if (!(in vector2).IsValid(in num))
						{
							return;
						}
						num = 10000f;
						if (!(in vector).IsValid(in num) || !this.ParentTransferable.targetRig.IsPositionInRange(vector, 4f))
						{
							return;
						}
						this.handNormalVector = vector2;
						this.handContactPoint = vector;
						this.DeflateLocal();
						return;
					}
				}
			}
		}

		private void DeflateLocal()
		{
			if (this.deflated)
			{
				return;
			}
			GameObject gameObject = ObjectPools.instance.Instantiate(this.deflationEffect, this.handContactPoint, true);
			gameObject.transform.up = this.handNormalVector;
			gameObject.transform.position = base.transform.position;
			SoundBankPlayer componentInChildren = gameObject.GetComponentInChildren<SoundBankPlayer>();
			if (componentInChildren.soundBank)
			{
				componentInChildren.Play();
			}
			this.placedOnFloor = false;
			this.timeCreated = float.PositiveInfinity;
			if (this.updateBlendShapeCosmetic)
			{
				this.updateBlendShapeCosmetic.FullyBlend();
			}
			this.deflated = true;
			base.Invoke("DisableObject", this.destroyWhenDeflateDelay);
		}

		private void DisableObject()
		{
			Action<IProjectile> onDeflated = this.OnDeflated;
			if (onDeflated != null)
			{
				onDeflated(this);
			}
			this.deflated = false;
		}

		private void OnDestroy()
		{
			if (this._events != null)
			{
				this._events.Activate -= this.DeflateEvent;
				this._events.Dispose();
				this._events = null;
			}
		}

		[SerializeField]
		private GameObject deflationEffect;

		[SerializeField]
		private float destroyWhenDeflateDelay = 3f;

		[SerializeField]
		private float forceDestroyAfterSec = 10f;

		[SerializeField]
		private float placementOffset = 0.2f;

		[SerializeField]
		private UpdateBlendShapeCosmetic updateBlendShapeCosmetic;

		[SerializeField]
		private LayerMask floorLayerMask;

		[SerializeField]
		private LayerMask handLayerMask;

		[SerializeField]
		private Rigidbody rigidbody;

		private bool placedOnFloor;

		private float placedOnFloorTime;

		private float timeCreated;

		private bool deflated;

		private Vector3 handContactPoint;

		private Vector3 handNormalVector;

		private CallLimiter callLimiter = new CallLimiter(10, 2f, 0.5f);

		private RubberDuckEvents _events;
	}
}
