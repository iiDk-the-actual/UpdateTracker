using System;
using GorillaExtensions;
using GorillaTag.CosmeticSystem;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTag.Cosmetics
{
	public class DistanceCheckerCosmetic : MonoBehaviour, ISpawnable
	{
		public bool IsSpawned { get; set; }

		public ECosmeticSelectSide CosmeticSelectedSide { get; set; }

		public void OnSpawn(VRRig rig)
		{
			this.myRig = rig;
		}

		public void OnDespawn()
		{
		}

		private void OnEnable()
		{
			this.currentState = DistanceCheckerCosmetic.State.None;
			this.transferableObject = base.GetComponentInParent<TransferrableObject>();
			if (this.transferableObject != null)
			{
				this.ownerRig = this.transferableObject.ownerRig;
			}
			this.ResetClosestPlayer();
		}

		private void Update()
		{
			this.UpdateDistance();
		}

		private bool IsBelowThreshold(Vector3 distance)
		{
			return distance.IsShorterThan(this.distanceThreshold);
		}

		private bool IsAboveThreshold(Vector3 distance)
		{
			return distance.IsLongerThan(this.distanceThreshold);
		}

		private void UpdateClosestPlayer(bool others = false)
		{
			if (!PhotonNetwork.InRoom)
			{
				this.ResetClosestPlayer();
				return;
			}
			VRRig vrrig = this.currentClosestPlayer;
			this.closestDistance = Vector3.positiveInfinity;
			this.currentClosestPlayer = null;
			foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
			{
				if (!others || !(this.ownerRig != null) || !(vrrig2 == this.ownerRig))
				{
					Vector3 vector = vrrig2.transform.position - this.distanceFrom.position;
					if (this.IsBelowThreshold(vector) && vector.sqrMagnitude < this.closestDistance.sqrMagnitude)
					{
						this.closestDistance = vector;
						this.currentClosestPlayer = vrrig2;
					}
				}
			}
			if (this.currentClosestPlayer != null && this.currentClosestPlayer != vrrig)
			{
				UnityEvent<VRRig, float> unityEvent = this.onClosestPlayerBelowThresholdChanged;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(this.currentClosestPlayer, this.closestDistance.magnitude);
			}
		}

		private void ResetClosestPlayer()
		{
			this.closestDistance = Vector3.positiveInfinity;
			this.currentClosestPlayer = null;
		}

		private void UpdateDistance()
		{
			bool flag = true;
			switch (this.distanceTo)
			{
			case DistanceCheckerCosmetic.DistanceCondition.Owner:
			{
				Vector3 vector = this.myRig.transform.position - this.distanceFrom.position;
				if (this.IsBelowThreshold(vector))
				{
					this.UpdateState(DistanceCheckerCosmetic.State.BelowThreshold);
					return;
				}
				if (this.IsAboveThreshold(vector))
				{
					this.UpdateState(DistanceCheckerCosmetic.State.AboveThreshold);
				}
				break;
			}
			case DistanceCheckerCosmetic.DistanceCondition.Others:
				this.UpdateClosestPlayer(true);
				if (!PhotonNetwork.InRoom)
				{
					return;
				}
				foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
				{
					if (!(this.ownerRig != null) || !(vrrig == this.ownerRig))
					{
						Vector3 vector2 = vrrig.transform.position - this.distanceFrom.position;
						if (this.IsBelowThreshold(vector2))
						{
							this.UpdateState(DistanceCheckerCosmetic.State.BelowThreshold);
							flag = false;
						}
					}
				}
				if (flag)
				{
					this.UpdateState(DistanceCheckerCosmetic.State.AboveThreshold);
					return;
				}
				break;
			case DistanceCheckerCosmetic.DistanceCondition.Everyone:
				this.UpdateClosestPlayer(false);
				if (!PhotonNetwork.InRoom)
				{
					return;
				}
				foreach (VRRig vrrig2 in GorillaParent.instance.vrrigs)
				{
					Vector3 vector3 = vrrig2.transform.position - this.distanceFrom.position;
					if (this.IsBelowThreshold(vector3))
					{
						this.UpdateState(DistanceCheckerCosmetic.State.BelowThreshold);
						flag = false;
					}
				}
				if (flag)
				{
					this.UpdateState(DistanceCheckerCosmetic.State.AboveThreshold);
					return;
				}
				break;
			default:
				return;
			}
		}

		private void UpdateState(DistanceCheckerCosmetic.State newState)
		{
			if (this.currentState == newState)
			{
				return;
			}
			this.currentState = newState;
			if (this.currentState != DistanceCheckerCosmetic.State.AboveThreshold)
			{
				if (this.currentState == DistanceCheckerCosmetic.State.BelowThreshold)
				{
					UnityEvent unityEvent = this.onOneIsBelowThreshold;
					if (unityEvent == null)
					{
						return;
					}
					unityEvent.Invoke();
				}
				return;
			}
			UnityEvent unityEvent2 = this.onAllAreAboveThreshold;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke();
		}

		[SerializeField]
		private Transform distanceFrom;

		[SerializeField]
		private DistanceCheckerCosmetic.DistanceCondition distanceTo;

		[Tooltip("Receive events when above or below this distance")]
		public float distanceThreshold;

		public UnityEvent onOneIsBelowThreshold;

		public UnityEvent onAllAreAboveThreshold;

		public UnityEvent<VRRig, float> onClosestPlayerBelowThresholdChanged;

		private VRRig myRig;

		private DistanceCheckerCosmetic.State currentState;

		private Vector3 closestDistance;

		private VRRig currentClosestPlayer;

		private VRRig ownerRig;

		private TransferrableObject transferableObject;

		private enum State
		{
			AboveThreshold,
			BelowThreshold,
			None
		}

		private enum DistanceCondition
		{
			None,
			Owner,
			Others,
			Everyone
		}
	}
}
