using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderShootingGallery : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			foreach (Collider collider in this.colliders)
			{
				collider.contactOffset = 0.0001f;
			}
			this.wheelHitNotifier.OnProjectileHit += this.OnWheelHit;
			this.cowboyHitNotifier.OnProjectileHit += this.OnCowboyHit;
		}

		private void OnDestroy()
		{
			this.wheelHitNotifier.OnProjectileHit -= this.OnWheelHit;
			this.cowboyHitNotifier.OnProjectileHit -= this.OnCowboyHit;
		}

		private void OnWheelHit(SlingshotProjectile projectile, Collision collision)
		{
			if (this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return;
			}
			if (projectile.projectileOwner == null || projectile.projectileOwner != NetworkSystem.Instance.LocalPlayer)
			{
				return;
			}
			if (this.lastHitTime + (double)this.hitCooldown < (double)Time.time)
			{
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 1);
			}
		}

		private void OnCowboyHit(SlingshotProjectile projectile, Collision collision)
		{
			if (this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return;
			}
			if (projectile.projectileOwner == null || projectile.projectileOwner != NetworkSystem.Instance.LocalPlayer)
			{
				return;
			}
			if (this.lastHitTime + (double)this.hitCooldown < (double)Time.time)
			{
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 2);
			}
		}

		private void CowboyHitEffects()
		{
			if (this.cowboyHitSound != null)
			{
				this.cowboyHitSound.Play();
			}
			if (this.cowboyHitAnimation != null && this.cowboyHitAnimation.clip != null)
			{
				this.cowboyHitAnimation.Play();
			}
		}

		private void WheelHitEffects()
		{
			if (this.wheelHitSound != null)
			{
				this.wheelHitSound.Play();
			}
			if (this.wheelHitAnimation != null && this.wheelHitAnimation.clip != null)
			{
				this.wheelHitAnimation.Play();
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			this.currentState = BuilderShootingGallery.FunctionalState.Idle;
			this.cowboyInitLocalPos = this.cowboyTransform.transform.localPosition;
			this.cowboyInitLocalRotation = this.cowboyTransform.transform.localRotation;
			this.wheelInitLocalRot = this.wheelTransform.transform.localRotation;
			this.distance = Vector3.Distance(this.cowboyStart.position, this.cowboyEnd.position);
			this.cowboyCycleDuration = this.distance / (this.cowboyVelocity * this.myPiece.GetScale());
			this.wheelCycleDuration = 1f / this.wheelVelocity;
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
			if (!this.activated && this.myPiece.state == BuilderPiece.State.AttachedAndPlaced)
			{
				this.myPiece.GetTable().RegisterFunctionalPieceFixedUpdate(this);
				this.activated = true;
			}
		}

		public void OnPieceActivate()
		{
			this.cowboyTransform.SetLocalPositionAndRotation(this.cowboyInitLocalPos, this.cowboyInitLocalRotation);
			this.wheelTransform.SetLocalPositionAndRotation(this.wheelTransform.localPosition, this.wheelInitLocalRot);
			if (!this.activated)
			{
				this.myPiece.GetTable().RegisterFunctionalPieceFixedUpdate(this);
				this.activated = true;
			}
		}

		public void OnPieceDeactivate()
		{
			if (this.currentState != BuilderShootingGallery.FunctionalState.Idle)
			{
				this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				this.myPiece.GetTable().UnregisterFunctionalPiece(this);
			}
			if (this.activated)
			{
				this.myPiece.GetTable().UnregisterFunctionalPieceFixedUpdate(this);
				this.activated = false;
			}
			this.cowboyTransform.SetLocalPositionAndRotation(this.cowboyInitLocalPos, this.cowboyInitLocalRotation);
			this.wheelTransform.SetLocalPositionAndRotation(this.wheelTransform.localPosition, this.wheelInitLocalRot);
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (instigator == null)
			{
				return;
			}
			if (!this.IsStateValid(newState))
			{
				return;
			}
			if (newState == 1 && this.currentState == BuilderShootingGallery.FunctionalState.Idle)
			{
				this.lastHitTime = (double)Time.time;
				this.WheelHitEffects();
				this.myPiece.GetTable().RegisterFunctionalPiece(this);
			}
			else if (newState == 2 && this.currentState == BuilderShootingGallery.FunctionalState.Idle)
			{
				this.lastHitTime = (double)Time.time;
				this.CowboyHitEffects();
				this.myPiece.GetTable().RegisterFunctionalPiece(this);
			}
			this.currentState = (BuilderShootingGallery.FunctionalState)newState;
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (!this.IsStateValid(newState) || instigator == null)
			{
				return;
			}
			if (this.lastHitTime + (double)this.hitCooldown < (double)Time.time)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, newState, instigator.GetPlayerRef(), timeStamp);
			}
		}

		public bool IsStateValid(byte state)
		{
			return state <= 2;
		}

		public void FunctionalPieceUpdate()
		{
			if (this.lastHitTime + (double)this.hitCooldown < (double)Time.time)
			{
				this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				this.myPiece.GetTable().UnregisterFunctionalPiece(this);
			}
		}

		public void FunctionalPieceFixedUpdate()
		{
			if (this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return;
			}
			this.currT = this.CowboyCycleCompletionPercent();
			this.currForward = this.IsEvenCycle();
			float num = (this.currForward ? this.currT : (1f - this.currT));
			float num2 = this.WheelCycleCompletionPercent();
			float num3 = this.cowboyCurve.Evaluate(num);
			this.cowboyTransform.localPosition = Vector3.Lerp(this.cowboyStart.localPosition, this.cowboyEnd.localPosition, num3);
			Quaternion quaternion = Quaternion.AngleAxis(num2 * 360f, Vector3.right);
			this.wheelTransform.localRotation = quaternion;
		}

		private long NetworkTimeMs()
		{
			if (PhotonNetwork.InRoom)
			{
				return (long)((ulong)(PhotonNetwork.ServerTimestamp + int.MinValue));
			}
			return (long)(Time.time * 1000f);
		}

		private long CowboyCycleLengthMs()
		{
			return (long)(this.cowboyCycleDuration * 1000f);
		}

		private long WheelCycleLengthMs()
		{
			return (long)(this.wheelCycleDuration * 1000f);
		}

		public double CowboyPlatformTime()
		{
			long num = this.NetworkTimeMs();
			long num2 = this.CowboyCycleLengthMs();
			return (double)(num - num / num2 * num2) / 1000.0;
		}

		public double WheelPlatformTime()
		{
			long num = this.NetworkTimeMs();
			long num2 = this.WheelCycleLengthMs();
			return (double)(num - num / num2 * num2) / 1000.0;
		}

		public int CowboyCycleCount()
		{
			return (int)(this.NetworkTimeMs() / this.CowboyCycleLengthMs());
		}

		public float CowboyCycleCompletionPercent()
		{
			return Mathf.Clamp((float)(this.CowboyPlatformTime() / (double)this.cowboyCycleDuration), 0f, 1f);
		}

		public float WheelCycleCompletionPercent()
		{
			return Mathf.Clamp((float)(this.WheelPlatformTime() / (double)this.wheelCycleDuration), 0f, 1f);
		}

		public bool IsEvenCycle()
		{
			return this.CowboyCycleCount() % 2 == 0;
		}

		public BuilderPiece myPiece;

		[SerializeField]
		private Transform wheelTransform;

		[SerializeField]
		private Transform cowboyTransform;

		[SerializeField]
		private SlingshotProjectileHitNotifier wheelHitNotifier;

		[SerializeField]
		private SlingshotProjectileHitNotifier cowboyHitNotifier;

		[SerializeField]
		protected List<Collider> colliders;

		[SerializeField]
		protected SoundBankPlayer wheelHitSound;

		[SerializeField]
		protected Animation wheelHitAnimation;

		[SerializeField]
		protected SoundBankPlayer cowboyHitSound;

		[SerializeField]
		private Animation cowboyHitAnimation;

		[SerializeField]
		private float hitCooldown = 1f;

		private double lastHitTime;

		private BuilderShootingGallery.FunctionalState currentState;

		private bool activated;

		[SerializeField]
		private float cowboyVelocity;

		[SerializeField]
		private Transform cowboyStart;

		[SerializeField]
		private Transform cowboyEnd;

		[SerializeField]
		private AnimationCurve cowboyCurve;

		[SerializeField]
		private float wheelVelocity;

		private Quaternion cowboyInitLocalRotation = Quaternion.identity;

		private Vector3 cowboyInitLocalPos = Vector3.zero;

		private Quaternion wheelInitLocalRot = Quaternion.identity;

		private float cowboyCycleDuration;

		private float wheelCycleDuration;

		private float distance;

		private float currT;

		private bool currForward;

		private float dtSinceServerUpdate;

		private int lastServerTimeStamp;

		private float rotateStartAmt;

		private float rotateAmt;

		private enum FunctionalState
		{
			Idle,
			HitWheel,
			HitCowboy
		}
	}
}
