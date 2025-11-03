using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace GorillaTagScripts.Builder
{
	public class BuilderProjectileLauncher : MonoBehaviour, IBuilderPieceFunctional, IBuilderPieceComponent
	{
		private void LaunchProjectile(int timeStamp)
		{
			if (Time.time > this.lastFireTime + this.fireCooldown)
			{
				this.lastFireTime = Time.time;
				int num = PoolUtils.GameObjHashCode(this.projectilePrefab);
				try
				{
					GameObject gameObject = ObjectPools.instance.Instantiate(num, true);
					this.projectileScale = this.myPiece.GetScale();
					gameObject.transform.localScale = Vector3.one * this.projectileScale;
					BuilderProjectile component = gameObject.GetComponent<BuilderProjectile>();
					int num2 = HashCode.Combine<int, int>(this.myPiece.pieceId, timeStamp);
					if (this.allProjectiles.ContainsKey(num2))
					{
						this.allProjectiles.Remove(num2);
					}
					this.allProjectiles.Add(num2, component);
					SlingshotProjectile.AOEKnockbackConfig aoeknockbackConfig = new SlingshotProjectile.AOEKnockbackConfig
					{
						aeoOuterRadius = this.knockbackConfig.aeoOuterRadius * this.projectileScale,
						aeoInnerRadius = this.knockbackConfig.aeoInnerRadius * this.projectileScale,
						applyAOEKnockback = this.knockbackConfig.applyAOEKnockback,
						impactVelocityThreshold = this.knockbackConfig.impactVelocityThreshold * this.projectileScale,
						knockbackVelocity = this.knockbackConfig.knockbackVelocity * this.projectileScale,
						playerProximityEffect = this.knockbackConfig.playerProximityEffect
					};
					component.aoeKnockbackConfig = new SlingshotProjectile.AOEKnockbackConfig?(aoeknockbackConfig);
					component.gravityMultiplier = this.gravityMultiplier;
					component.Launch(this.launchPosition.position, this.launchVelocity * this.projectileScale * this.launchPosition.up, this, num2, this.projectileScale, timeStamp);
					if (this.launchSound != null && this.launchSound.clip != null)
					{
						this.launchSound.Play();
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
					throw;
				}
			}
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				return;
			}
			if ((BuilderProjectileLauncher.FunctionalState)newState == this.currentState)
			{
				return;
			}
			this.currentState = (BuilderProjectileLauncher.FunctionalState)newState;
			if (newState == 1)
			{
				this.LaunchProjectile(timeStamp);
				this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
			}
		}

		public void OnStateRequest(byte newState, NetPlayer instigator, int timeStamp)
		{
		}

		public bool IsStateValid(byte state)
		{
			return state <= 1;
		}

		public void FunctionalPieceUpdate()
		{
			for (int i = this.launchedProjectiles.Count - 1; i >= 0; i--)
			{
				this.launchedProjectiles[i].UpdateProjectile();
			}
			if (PhotonNetwork.IsMasterClient && this.lastFireTime + this.fireCooldown < Time.time)
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
		}

		public void OnPieceActivate()
		{
			this.myPiece.GetTable().RegisterFunctionalPiece(this);
		}

		public void OnPieceDeactivate()
		{
			this.myPiece.GetTable().UnregisterFunctionalPiece(this);
			for (int i = this.launchedProjectiles.Count - 1; i >= 0; i--)
			{
				this.launchedProjectiles[i].Deactivate();
			}
		}

		public void RegisterProjectile(BuilderProjectile projectile)
		{
			this.launchedProjectiles.Add(projectile);
		}

		public void UnRegisterProjectile(BuilderProjectile projectile)
		{
			this.launchedProjectiles.Remove(projectile);
			this.allProjectiles.Remove(projectile.projectileId);
		}

		private List<BuilderProjectile> launchedProjectiles = new List<BuilderProjectile>();

		[SerializeField]
		protected BuilderPiece myPiece;

		[SerializeField]
		protected float fireCooldown = 2f;

		[Tooltip("launch in Y direction")]
		[SerializeField]
		private Transform launchPosition;

		[SerializeField]
		private float launchVelocity;

		[SerializeField]
		private AudioSource launchSound;

		[SerializeField]
		protected GameObject projectilePrefab;

		protected float projectileScale = 0.06f;

		[SerializeField]
		protected float gravityMultiplier = 1f;

		public SlingshotProjectile.AOEKnockbackConfig knockbackConfig;

		private float lastFireTime;

		private BuilderProjectileLauncher.FunctionalState currentState;

		private Dictionary<int, BuilderProjectile> allProjectiles = new Dictionary<int, BuilderProjectile>();

		private enum FunctionalState
		{
			Idle,
			Fire
		}
	}
}
