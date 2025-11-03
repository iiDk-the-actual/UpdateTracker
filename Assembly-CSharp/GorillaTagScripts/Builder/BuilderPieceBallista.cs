using System;
using System.Collections;
using System.Collections.Generic;
using CjLib;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts.Builder
{
	public class BuilderPieceBallista : MonoBehaviour, IBuilderPieceComponent, IBuilderPieceFunctional
	{
		private void Awake()
		{
			this.animator.SetFloat(this.pitchParamHash, this.pitch);
			this.appliedAnimatorPitch = this.pitch;
			this.launchDirection = this.launchEnd.position - this.launchStart.position;
			this.launchRampDistance = this.launchDirection.magnitude;
			this.launchDirection /= this.launchRampDistance;
			this.playerPullInRate = Mathf.Exp(this.playerMagnetismStrength);
			if (this.handTrigger != null)
			{
				this.handTrigger.TriggeredEvent.AddListener(new UnityAction(this.OnHandTriggerPressed));
			}
			this.hasLaunchParticles = this.launchParticles != null;
		}

		private void OnDestroy()
		{
			if (this.handTrigger != null)
			{
				this.handTrigger.TriggeredEvent.RemoveListener(new UnityAction(this.OnHandTriggerPressed));
			}
		}

		private void OnHandTriggerPressed()
		{
			if (this.autoLaunch)
			{
				return;
			}
			if (this.ballistaState == BuilderPieceBallista.BallistaState.PlayerInTrigger)
			{
				this.myPiece.GetTable().builderNetworking.RequestFunctionalPieceStateChange(this.myPiece.pieceId, 4);
			}
		}

		private void UpdateStateMaster()
		{
			if (!NetworkSystem.Instance.InRoom || !NetworkSystem.Instance.IsMasterClient)
			{
				return;
			}
			AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
			switch (this.ballistaState)
			{
			case BuilderPieceBallista.BallistaState.Idle:
				this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				return;
			case BuilderPieceBallista.BallistaState.Loading:
				if (currentAnimatorStateInfo.shortNameHash == this.loadStateHash && (double)Time.time > this.loadCompleteTime)
				{
					if (this.playerInTrigger && this.playerRigInTrigger != null && (this.launchBigMonkes || (double)this.playerRigInTrigger.scaleFactor < 0.99))
					{
						this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 3, this.playerRigInTrigger.Creator.GetPlayerRef(), NetworkSystem.Instance.ServerTimestamp);
						return;
					}
					this.playerInTrigger = false;
					this.playerRigInTrigger = null;
					this.ballistaState = BuilderPieceBallista.BallistaState.WaitingForTrigger;
					return;
				}
				break;
			case BuilderPieceBallista.BallistaState.WaitingForTrigger:
				if (!this.playerInTrigger || this.playerRigInTrigger == null || (!this.launchBigMonkes && this.playerRigInTrigger.scaleFactor >= 0.99f))
				{
					this.playerInTrigger = false;
					this.playerRigInTrigger = null;
					return;
				}
				if (this.playerInTrigger)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 3, this.playerRigInTrigger.Creator.GetPlayerRef(), NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				break;
			case BuilderPieceBallista.BallistaState.PlayerInTrigger:
				if (!this.playerInTrigger || this.playerRigInTrigger == null || (!this.launchBigMonkes && this.playerRigInTrigger.scaleFactor >= 0.99f))
				{
					this.playerInTrigger = false;
					this.playerRigInTrigger = null;
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 2, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				if (this.autoLaunch && (double)Time.time > this.enteredTriggerTime + (double)this.autoLaunchDelay)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 4, this.playerRigInTrigger.Creator.GetPlayerRef(), NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				break;
			case BuilderPieceBallista.BallistaState.PrepareForLaunch:
			case BuilderPieceBallista.BallistaState.PrepareForLaunchLocal:
			{
				if (!this.playerInTrigger || this.playerRigInTrigger == null || (!this.launchBigMonkes && this.playerRigInTrigger.scaleFactor >= 0.99f))
				{
					this.playerInTrigger = false;
					this.playerRigInTrigger = null;
					this.ResetFlags();
					this.myPiece.functionalPieceState = 0;
					this.ballistaState = BuilderPieceBallista.BallistaState.Idle;
					return;
				}
				Vector3 playerBodyCenterPosition = this.GetPlayerBodyCenterPosition(this.playerRigInTrigger.transform, this.playerRigInTrigger.scaleFactor);
				Vector3 vector = Vector3.Dot(playerBodyCenterPosition - this.launchStart.position, this.launchDirection) * this.launchDirection + this.launchStart.position;
				Vector3 vector2 = playerBodyCenterPosition - vector;
				if (Vector3.Lerp(Vector3.zero, vector2, Mathf.Exp(-this.playerPullInRate * Time.deltaTime)).sqrMagnitude < this.playerReadyToFireDist * this.myPiece.GetScale() * this.playerReadyToFireDist * this.myPiece.GetScale())
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 6, this.playerRigInTrigger.Creator.GetPlayerRef(), NetworkSystem.Instance.ServerTimestamp);
					return;
				}
				break;
			}
			case BuilderPieceBallista.BallistaState.Launching:
			case BuilderPieceBallista.BallistaState.LaunchingLocal:
				if (currentAnimatorStateInfo.shortNameHash == this.idleStateHash)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 1, PhotonNetwork.LocalPlayer, NetworkSystem.Instance.ServerTimestamp);
				}
				break;
			default:
				return;
			}
		}

		private void ResetFlags()
		{
			this.playerLaunched = false;
			this.loadCompleteTime = double.MaxValue;
		}

		private void UpdatePlayerPosition()
		{
			if (this.ballistaState != BuilderPieceBallista.BallistaState.PrepareForLaunchLocal && this.ballistaState != BuilderPieceBallista.BallistaState.LaunchingLocal)
			{
				return;
			}
			float deltaTime = Time.deltaTime;
			GTPlayer instance = GTPlayer.Instance;
			Vector3 playerBodyCenterPosition = this.GetPlayerBodyCenterPosition(instance.headCollider.transform, instance.scale);
			Vector3 vector = playerBodyCenterPosition - this.launchStart.position;
			BuilderPieceBallista.BallistaState ballistaState = this.ballistaState;
			if (ballistaState == BuilderPieceBallista.BallistaState.PrepareForLaunchLocal)
			{
				Vector3 vector2 = Vector3.Dot(vector, this.launchDirection) * this.launchDirection + this.launchStart.position;
				Vector3 vector3 = playerBodyCenterPosition - vector2;
				Vector3 vector4 = Vector3.Lerp(Vector3.zero, vector3, Mathf.Exp(-this.playerPullInRate * deltaTime));
				instance.transform.position = instance.transform.position + (vector4 - vector3);
				instance.SetPlayerVelocity(Vector3.zero);
				instance.SetMaximumSlipThisFrame();
				return;
			}
			if (ballistaState != BuilderPieceBallista.BallistaState.LaunchingLocal)
			{
				return;
			}
			if (!this.playerLaunched)
			{
				float num = Vector3.Dot(this.launchBone.position - this.launchStart.position, this.launchDirection) / this.launchRampDistance;
				float num2 = Vector3.Dot(vector, this.launchDirection) / this.launchRampDistance;
				float num3 = 0.25f * this.myPiece.GetScale() / this.launchRampDistance;
				float num4 = Mathf.Max(num + num3, num2);
				float num5 = num4 * this.launchRampDistance;
				Vector3 vector5 = this.launchDirection * num5 + this.launchStart.position;
				instance.transform.position + (vector5 - playerBodyCenterPosition);
				instance.transform.position = instance.transform.position + (vector5 - playerBodyCenterPosition);
				instance.SetPlayerVelocity(Vector3.zero);
				instance.SetMaximumSlipThisFrame();
				if (num4 >= 1f)
				{
					this.playerLaunched = true;
					this.launchedTime = (double)Time.time;
					instance.SetPlayerVelocity(this.launchSpeed * this.myPiece.GetScale() * this.launchDirection);
					instance.SetMaximumSlipThisFrame();
					return;
				}
			}
			else if ((double)Time.time < this.launchedTime + (double)this.slipOverrideDuration)
			{
				instance.SetMaximumSlipThisFrame();
			}
		}

		private Vector3 GetPlayerBodyCenterPosition(Transform headTransform, float playerScale)
		{
			return headTransform.position + Quaternion.Euler(0f, headTransform.rotation.eulerAngles.y, 0f) * new Vector3(0f, 0f, this.playerBodyOffsetFromHead.z * playerScale) + Vector3.down * (this.playerBodyOffsetFromHead.y * playerScale);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (this.playerRigInTrigger != null)
			{
				return;
			}
			if (other.GetComponent<CapsuleCollider>() == null)
			{
				return;
			}
			if (other.attachedRigidbody == null)
			{
				return;
			}
			VRRig vrrig = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
			if (vrrig == null)
			{
				if (!(GTPlayer.Instance.bodyCollider == other))
				{
					return;
				}
				vrrig = GorillaTagger.Instance.offlineVRRig;
			}
			if (!this.launchBigMonkes && (double)vrrig.scaleFactor > 0.99)
			{
				return;
			}
			this.playerRigInTrigger = vrrig;
			this.playerInTrigger = true;
		}

		private void OnTriggerExit(Collider other)
		{
			if (this.playerRigInTrigger == null || !this.playerInTrigger)
			{
				return;
			}
			if (other.GetComponent<CapsuleCollider>() == null)
			{
				return;
			}
			if (other.attachedRigidbody == null)
			{
				return;
			}
			VRRig vrrig = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
			if (vrrig == null)
			{
				if (!(GTPlayer.Instance.bodyCollider == other))
				{
					return;
				}
				vrrig = GorillaTagger.Instance.offlineVRRig;
			}
			if (this.playerRigInTrigger.Equals(vrrig))
			{
				this.playerInTrigger = false;
				this.playerRigInTrigger = null;
			}
		}

		public void OnPieceCreate(int pieceType, int pieceId)
		{
			if (!this.myPiece.GetTable().isTableMutable)
			{
				this.launchBigMonkes = true;
			}
			this.ballistaState = BuilderPieceBallista.BallistaState.Idle;
			this.playerInTrigger = false;
			this.playerRigInTrigger = null;
			this.playerLaunched = false;
		}

		public void OnPieceDestroy()
		{
			this.myPiece.functionalPieceState = 0;
			this.ballistaState = BuilderPieceBallista.BallistaState.Idle;
		}

		public void OnPiecePlacementDeserialized()
		{
			this.launchDirection = this.launchEnd.position - this.launchStart.position;
			this.launchRampDistance = this.launchDirection.magnitude;
			this.launchDirection /= this.launchRampDistance;
		}

		public void OnPieceActivate()
		{
			foreach (Collider collider in this.triggers)
			{
				collider.enabled = true;
			}
			this.animator.SetFloat(this.pitchParamHash, this.pitch);
			this.appliedAnimatorPitch = this.pitch;
			this.launchDirection = this.launchEnd.position - this.launchStart.position;
			this.launchRampDistance = this.launchDirection.magnitude;
			this.launchDirection /= this.launchRampDistance;
			this.myPiece.GetTable().RegisterFunctionalPiece(this);
		}

		public void OnPieceDeactivate()
		{
			foreach (Collider collider in this.triggers)
			{
				collider.enabled = false;
			}
			if (this.hasLaunchParticles)
			{
				this.launchParticles.Stop();
				this.launchParticles.Clear();
			}
			this.myPiece.functionalPieceState = 0;
			this.ballistaState = BuilderPieceBallista.BallistaState.Idle;
			this.playerInTrigger = false;
			this.playerRigInTrigger = null;
			this.ResetFlags();
			this.myPiece.GetTable().UnregisterFunctionalPiece(this);
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
			if ((BuilderPieceBallista.BallistaState)newState == this.ballistaState)
			{
				return;
			}
			if (newState == 4)
			{
				if (this.ballistaState == BuilderPieceBallista.BallistaState.PlayerInTrigger && this.playerInTrigger && this.playerRigInTrigger != null)
				{
					this.myPiece.GetTable().builderNetworking.FunctionalPieceStateChangeMaster(this.myPiece.pieceId, 4, this.playerRigInTrigger.Creator.GetPlayerRef(), timeStamp);
					return;
				}
			}
			else
			{
				Debug.LogWarning("BuilderPiece Ballista unexpected state request for " + newState.ToString());
			}
		}

		public void OnStateChanged(byte newState, NetPlayer instigator, int timeStamp)
		{
			if (!this.IsStateValid(newState))
			{
				return;
			}
			BuilderPieceBallista.BallistaState ballistaState = (BuilderPieceBallista.BallistaState)newState;
			if (ballistaState == this.ballistaState)
			{
				return;
			}
			switch (newState)
			{
			case 0:
				this.ResetFlags();
				goto IL_02C2;
			case 1:
				this.ResetFlags();
				foreach (Collider collider in this.disableWhileLaunching)
				{
					collider.enabled = true;
				}
				if (this.ballistaState == BuilderPieceBallista.BallistaState.Launching || this.ballistaState == BuilderPieceBallista.BallistaState.LaunchingLocal)
				{
					this.loadCompleteTime = (double)(Time.time + this.reloadDelay);
					if (this.loadSFX != null)
					{
						this.loadSFX.Play();
					}
				}
				else
				{
					this.loadCompleteTime = (double)(Time.time + this.loadTime);
				}
				this.animator.SetTrigger(this.loadTriggerHash);
				goto IL_02C2;
			case 2:
			case 5:
				goto IL_02C2;
			case 3:
				this.enteredTriggerTime = (double)Time.time;
				if (this.autoLaunch && this.cockSFX != null)
				{
					this.cockSFX.Play();
					goto IL_02C2;
				}
				goto IL_02C2;
			case 4:
			{
				this.playerLaunched = false;
				if (!this.autoLaunch && this.cockSFX != null)
				{
					this.cockSFX.Play();
				}
				if (!instigator.IsLocal)
				{
					goto IL_02C2;
				}
				GTPlayer instance = GTPlayer.Instance;
				if (Vector3.Distance(this.GetPlayerBodyCenterPosition(instance.headCollider.transform, instance.scale), this.launchStart.position) > this.prepareForLaunchDistance * this.myPiece.GetScale() || (!this.launchBigMonkes && (double)GorillaTagger.Instance.offlineVRRig.scaleFactor >= 0.99))
				{
					goto IL_02C2;
				}
				ballistaState = BuilderPieceBallista.BallistaState.PrepareForLaunchLocal;
				using (List<Collider>.Enumerator enumerator = this.disableWhileLaunching.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Collider collider2 = enumerator.Current;
						collider2.enabled = false;
					}
					goto IL_02C2;
				}
				break;
			}
			case 6:
				break;
			default:
				goto IL_02C2;
			}
			this.playerLaunched = false;
			this.animator.SetTrigger(this.fireTriggerHash);
			if (this.launchSFX != null)
			{
				this.launchSFX.Play();
			}
			if (this.hasLaunchParticles)
			{
				this.launchParticles.Play();
			}
			if (this.debugDrawTrajectoryOnLaunch)
			{
				base.StartCoroutine(this.DebugDrawTrajectory(8f));
			}
			if (instigator.IsLocal && this.ballistaState == BuilderPieceBallista.BallistaState.PrepareForLaunchLocal)
			{
				ballistaState = BuilderPieceBallista.BallistaState.LaunchingLocal;
				GorillaTagger.Instance.StartVibration(true, GorillaTagger.Instance.tapHapticStrength * 2f, GorillaTagger.Instance.tapHapticDuration * 4f);
				GorillaTagger.Instance.StartVibration(false, GorillaTagger.Instance.tapHapticStrength * 2f, GorillaTagger.Instance.tapHapticDuration * 4f);
			}
			IL_02C2:
			this.ballistaState = ballistaState;
		}

		public bool IsStateValid(byte state)
		{
			return state < 8;
		}

		public void FunctionalPieceUpdate()
		{
			if (this.myPiece == null || this.myPiece.state != BuilderPiece.State.AttachedAndPlaced)
			{
				return;
			}
			if (NetworkSystem.Instance.IsMasterClient)
			{
				this.UpdateStateMaster();
			}
			this.UpdatePlayerPosition();
		}

		private void UpdatePredictionLine()
		{
			float num = 0.033333335f;
			Vector3 vector = this.launchEnd.position;
			Vector3 vector2 = (this.launchEnd.position - this.launchStart.position).normalized * this.launchSpeed;
			for (int i = 0; i < 240; i++)
			{
				this.predictionLinePoints[i] = vector;
				vector += vector2 * num;
				vector2 += Vector3.down * 9.8f * num;
			}
		}

		private IEnumerator DebugDrawTrajectory(float duration)
		{
			this.UpdatePredictionLine();
			float startTime = Time.time;
			while (Time.time < startTime + duration)
			{
				DebugUtil.DrawLine(this.launchStart.position, this.launchEnd.position, Color.yellow, true);
				DebugUtil.DrawLines(this.predictionLinePoints, Color.yellow, true);
				yield return null;
			}
			yield break;
		}

		[SerializeField]
		private BuilderPiece myPiece;

		[SerializeField]
		private List<Collider> triggers;

		[SerializeField]
		private List<Collider> disableWhileLaunching;

		[Tooltip("Trigger to start the launch if not autoLaunch")]
		[SerializeField]
		private BuilderSmallHandTrigger handTrigger;

		[Tooltip("Should the player launch without a hand trigger press")]
		[SerializeField]
		private bool autoLaunch;

		[SerializeField]
		private float autoLaunchDelay = 0.75f;

		private double enteredTriggerTime;

		public Animator animator;

		public Transform launchStart;

		public Transform launchEnd;

		public Transform launchBone;

		[SerializeField]
		private SoundBankPlayer loadSFX;

		[SerializeField]
		private SoundBankPlayer launchSFX;

		[SerializeField]
		private SoundBankPlayer cockSFX;

		[SerializeField]
		private ParticleSystem launchParticles;

		private bool hasLaunchParticles;

		public float reloadDelay = 1f;

		public float loadTime = 1.933f;

		public float slipOverrideDuration = 0.1f;

		private double launchedTime;

		public float playerMagnetismStrength = 3f;

		[Tooltip("Speed will be scaled by piece scale")]
		public float launchSpeed = 20f;

		[Range(0f, 1f)]
		public float pitch;

		private bool debugDrawTrajectoryOnLaunch;

		private int loadTriggerHash = Animator.StringToHash("Load");

		private int fireTriggerHash = Animator.StringToHash("Fire");

		private int pitchParamHash = Animator.StringToHash("Pitch");

		private int idleStateHash = Animator.StringToHash("Idle");

		private int loadStateHash = Animator.StringToHash("Load");

		private int fireStateHash = Animator.StringToHash("Fire");

		private bool playerInTrigger;

		private VRRig playerRigInTrigger;

		private bool playerLaunched;

		private float playerReadyToFireDist = 1.6667f;

		private float prepareForLaunchDistance = 2.5f;

		private Vector3 launchDirection;

		private float launchRampDistance;

		private float playerPullInRate;

		private float appliedAnimatorPitch;

		private bool launchBigMonkes;

		private Vector3 playerBodyOffsetFromHead = new Vector3(0f, -0.4f, -0.15f);

		private double loadCompleteTime;

		private BuilderPieceBallista.BallistaState ballistaState;

		private const int predictionLineSamples = 240;

		private Vector3[] predictionLinePoints = new Vector3[240];

		private enum BallistaState
		{
			Idle,
			Loading,
			WaitingForTrigger,
			PlayerInTrigger,
			PrepareForLaunch,
			PrepareForLaunchLocal,
			Launching,
			LaunchingLocal,
			Count
		}
	}
}
