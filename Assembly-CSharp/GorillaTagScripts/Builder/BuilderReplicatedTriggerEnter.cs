using System;
using System.Collections.Generic;
using GorillaLocomotion;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class BuilderReplicatedTriggerEnter : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			this.colliders.Clear();
			foreach (BuilderSmallHandTrigger builderSmallHandTrigger in this.handTriggers)
			{
				builderSmallHandTrigger.TriggeredEvent.AddListener(new UnityAction(this.OnHandTriggerEntered));
				Collider component = builderSmallHandTrigger.GetComponent<Collider>();
				if (component != null)
				{
					this.colliders.Add(component);
				}
			}
			foreach (BuilderSmallMonkeTrigger builderSmallMonkeTrigger in this.bodyTriggers)
			{
				builderSmallMonkeTrigger.onPlayerEnteredTrigger += this.OnBodyTriggerEntered;
				Collider component2 = builderSmallMonkeTrigger.GetComponent<Collider>();
				if (component2 != null)
				{
					this.colliders.Add(component2);
				}
			}
		}

		private void OnDestroy()
		{
			BuilderSmallHandTrigger[] array = this.handTriggers;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].TriggeredEvent.RemoveListener(new UnityAction(this.OnHandTriggerEntered));
			}
			BuilderSmallMonkeTrigger[] array2 = this.bodyTriggers;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].onPlayerEnteredTrigger -= this.OnBodyTriggerEntered;
			}
		}

		private void PlayTriggerEffects(NetPlayer target)
		{
			UnityEvent onTriggered = this.OnTriggered;
			if (onTriggered != null)
			{
				onTriggered.Invoke();
			}
			if (this.animationOnTrigger != null && this.animationOnTrigger.clip != null)
			{
				this.animationOnTrigger.Rewind();
				this.animationOnTrigger.Play();
			}
			if (this.activateSoundBank != null)
			{
				this.activateSoundBank.Play();
			}
			if (target.IsLocal)
			{
				VRRig rig = VRRigCache.Instance.localRig.Rig;
				if (rig != null)
				{
					float num = 1.5f * rig.scaleFactor;
					if ((rig.transform.position - base.transform.position).sqrMagnitude > num * num)
					{
						return;
					}
					GTPlayer.Instance.SetMaximumSlipThisFrame();
					GTPlayer.Instance.ApplyKnockback(this.knockbackDirection.forward, this.knockbackVelocity * rig.scaleFactor, false);
					GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength / 2f, Time.fixedDeltaTime);
					GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength / 2f, Time.fixedDeltaTime);
				}
			}
		}

		private void OnHandTriggerEntered()
		{
			if (this.CanTrigger())
			{
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 1);
			}
		}

		private void OnBodyTriggerEntered(int playerNumber)
		{
			if (!NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			NetPlayer player = NetworkSystem.Instance.GetPlayer(playerNumber);
			if (player == null)
			{
				return;
			}
			if (this.CanTrigger())
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, player.GetPlayerRef(), NetworkSystem.Instance.ServerTimestamp);
			}
		}

		private bool CanTrigger()
		{
			return this.isPieceActive && this.currentState == BuilderReplicatedTriggerEnter.FunctionalState.Idle && Time.time > this.lastTriggerTime + this.triggerCooldown;
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			this.currentState = BuilderReplicatedTriggerEnter.FunctionalState.Idle;
		}

		public void OnPieceDestroy()
		{
		}

		public void OnPiecePlacementDeserialized()
		{
		}

		public void OnPieceActivate()
		{
			this.isPieceActive = true;
			foreach (Collider collider in this.colliders)
			{
				collider.enabled = true;
			}
		}

		public void OnPieceDeactivate()
		{
			this.isPieceActive = false;
			if (this.currentState == BuilderReplicatedTriggerEnter.FunctionalState.TriggerEntered)
			{
				this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				this.myPiece.GetTable().UnregisterFunctionalPiece(this);
			}
			foreach (Collider collider in this.colliders)
			{
				collider.enabled = false;
			}
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				return;
			}
			if (newState == 1 && this.currentState != BuilderReplicatedTriggerEnter.FunctionalState.TriggerEntered)
			{
				this.lastTriggerTime = Time.time;
				this.myPiece.GetTable().RegisterFunctionalPiece(this);
				this.PlayTriggerEffects(instigator);
			}
			this.currentState = (BuilderReplicatedTriggerEnter.FunctionalState)newState;
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
			if (newState == 1 && this.CanTrigger())
			{
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, newState, instigator.GetPlayerRef(), timeStamp);
			}
		}

		public bool IsStateValid(byte state)
		{
			return state <= 1;
		}

		public void FunctionalPieceUpdate()
		{
			if (this.lastTriggerTime + this.triggerCooldown < Time.time)
			{
				this.myPiece.SetFunctionalPieceState(0, NetworkSystem.Instance.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				this.myPiece.GetTable().UnregisterFunctionalPiece(this);
			}
		}

		[SerializeField]
		protected BuilderPiece myPiece;

		[Tooltip("How long in seconds to wait between trigger events")]
		[SerializeField]
		protected float triggerCooldown = 0.5f;

		[SerializeField]
		private BuilderSmallHandTrigger[] handTriggers;

		[SerializeField]
		private BuilderSmallMonkeTrigger[] bodyTriggers;

		[Tooltip("Optional Animation to play when triggered")]
		[SerializeField]
		private Animation animationOnTrigger;

		[Tooltip("Optional Sound to play when triggered")]
		[SerializeField]
		private SoundBankPlayer activateSoundBank;

		[Tooltip("Knockback the triggering player?")]
		[SerializeField]
		private bool knockbackOnTriggerEnter;

		[SerializeField]
		private float knockbackVelocity;

		[Tooltip("uses Forward of the transform provided")]
		[SerializeField]
		private Transform knockbackDirection;

		private List<Collider> colliders = new List<Collider>(5);

		private bool isPieceActive;

		private float lastTriggerTime;

		private BuilderReplicatedTriggerEnter.FunctionalState currentState;

		public UnityEvent OnTriggered;

		private enum FunctionalState
		{
			Idle,
			TriggerEntered
		}
	}
}
