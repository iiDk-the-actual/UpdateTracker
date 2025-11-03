using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderProjectileTarget : MonoBehaviour, IBuilderPieceFunctional
	{
		private void Awake()
		{
			this.hitNotifier.OnProjectileHit += this.OnProjectileHit;
			foreach (Collider collider in this.colliders)
			{
				collider.contactOffset = 0.0001f;
			}
		}

		private void OnDestroy()
		{
			this.hitNotifier.OnProjectileHit -= this.OnProjectileHit;
		}

		private void OnDisable()
		{
			this.hitCount = 0;
			if (this.scoreText != null)
			{
				this.scoreText.text = this.hitCount.ToString("D2");
			}
		}

		private void OnProjectileHit(SlingshotProjectile projectile, Collision collision)
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
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 11);
			}
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
			if (newState == 11)
			{
				return;
			}
			this.lastHitTime = (double)Time.time;
			this.hitCount = Mathf.Clamp((int)newState, 0, 10);
			this.PlayHitEffects();
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			if (!this.IsStateValid(newState))
			{
				return;
			}
			if (instigator == null)
			{
				return;
			}
			if (newState != 11)
			{
				return;
			}
			this.hitCount++;
			this.hitCount %= 11;
			this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, (byte)this.hitCount, instigator.GetPlayerRef(), timeStamp);
		}

		public bool IsStateValid(byte state)
		{
			return state <= 11;
		}

		private void PlayHitEffects()
		{
			if (this.hitSoundbank != null)
			{
				this.hitSoundbank.Play();
			}
			if (this.hitAnimation != null && this.hitAnimation.clip != null)
			{
				this.hitAnimation.Play();
			}
			if (this.scoreText != null)
			{
				this.scoreText.text = this.hitCount.ToString("D2");
			}
		}

		public void FunctionalPieceUpdate()
		{
		}

		public float GetInteractionDistace()
		{
			return 20f;
		}

		[SerializeField]
		private BuilderPiece myPiece;

		[SerializeField]
		private SlingshotProjectileHitNotifier hitNotifier;

		[SerializeField]
		protected float hitCooldown = 2f;

		[Tooltip("Optional Sounds to play on hit")]
		[SerializeField]
		protected SoundBankPlayer hitSoundbank;

		[Tooltip("Optional Sounds to play on hit")]
		[SerializeField]
		protected Animation hitAnimation;

		[SerializeField]
		protected List<Collider> colliders;

		[SerializeField]
		private TMP_Text scoreText;

		private double lastHitTime;

		private int hitCount;

		private const byte MAX_SCORE = 10;

		private const byte HIT = 11;
	}
}
