using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using GorillaExtensions;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Events;

namespace GorillaTagScripts
{
	[NetworkBehaviourWeaved(6)]
	public class LurkerGhost : NetworkComponent
	{
		protected override void Awake()
		{
			base.Awake();
			this.possibleTargets = new List<NetPlayer>();
			this.targetPlayer = null;
			this.targetTransform = null;
			this.targetVRRig = null;
		}

		protected override void Start()
		{
			base.Start();
			this.waypointRegions = this.waypointsContainer.GetComponentsInChildren<ZoneBasedObject>();
			this.PickNextWaypoint();
			this.ChangeState(LurkerGhost.ghostState.patrol);
		}

		private void LateUpdate()
		{
			this.UpdateState();
			this.UpdateGhostVisibility();
		}

		private void PickNextWaypoint()
		{
			if (this.waypoints.Count == 0 || this.lastWaypointRegion == null || !this.lastWaypointRegion.IsLocalPlayerInZone())
			{
				ZoneBasedObject zoneBasedObject = ZoneBasedObject.SelectRandomEligible(this.waypointRegions, "");
				if (zoneBasedObject == null)
				{
					zoneBasedObject = this.lastWaypointRegion;
				}
				if (zoneBasedObject == null)
				{
					return;
				}
				this.lastWaypointRegion = zoneBasedObject;
				this.waypoints.Clear();
				foreach (object obj in zoneBasedObject.transform)
				{
					Transform transform = (Transform)obj;
					this.waypoints.Add(transform);
				}
			}
			int num = Random.Range(0, this.waypoints.Count);
			this.currentWaypoint = this.waypoints[num];
			this.targetRotation = Quaternion.LookRotation(this.currentWaypoint.position - base.transform.position);
			this.waypoints.RemoveAt(num);
		}

		private void Patrol()
		{
			Transform transform = this.currentWaypoint;
			if (transform != null)
			{
				base.transform.position = Vector3.MoveTowards(base.transform.position, transform.position, this.patrolSpeed * Time.deltaTime);
				base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, this.targetRotation, 360f * Time.deltaTime);
			}
		}

		private void PlaySound(AudioClip clip, bool loop)
		{
			if (this.audioSource && this.audioSource.isPlaying)
			{
				this.audioSource.GTStop();
			}
			if (this.audioSource && clip != null)
			{
				this.audioSource.clip = clip;
				this.audioSource.loop = loop;
				this.audioSource.GTPlay();
			}
		}

		private bool PickPlayer(float maxDistance)
		{
			if (base.IsMine)
			{
				this.possibleTargets.Clear();
				for (int i = 0; i < GorillaParent.instance.vrrigs.Count; i++)
				{
					if ((GorillaParent.instance.vrrigs[i].transform.position - base.transform.position).magnitude < maxDistance && GorillaParent.instance.vrrigs[i].creator != this.targetPlayer)
					{
						this.possibleTargets.Add(GorillaParent.instance.vrrigs[i].creator);
					}
				}
				this.targetPlayer = null;
				this.targetTransform = null;
				this.targetVRRig = null;
				if (this.possibleTargets.Count > 0)
				{
					int num = Random.Range(0, this.possibleTargets.Count);
					this.PickPlayer(this.possibleTargets[num]);
				}
			}
			else
			{
				this.targetPlayer = null;
				this.targetTransform = null;
				this.targetVRRig = null;
			}
			return this.targetPlayer != null && this.targetTransform != null;
		}

		private void PickPlayer(NetPlayer player)
		{
			int num = GorillaParent.instance.vrrigs.FindIndex((VRRig x) => x.creator != null && x.creator == player);
			if (num > -1 && num < GorillaParent.instance.vrrigs.Count)
			{
				this.targetPlayer = GorillaParent.instance.vrrigs[num].creator;
				this.targetTransform = GorillaParent.instance.vrrigs[num].head.rigTarget;
				this.targetVRRig = GorillaParent.instance.vrrigs[num];
			}
		}

		private void SeekPlayer()
		{
			if (this.targetTransform == null)
			{
				this.ChangeState(LurkerGhost.ghostState.patrol);
				return;
			}
			this.targetPosition = this.targetTransform.position + this.targetTransform.forward.x0z() * this.seekAheadDistance;
			this.targetRotation = Quaternion.LookRotation(this.targetTransform.position - base.transform.position);
			base.transform.position = Vector3.MoveTowards(base.transform.position, this.targetPosition, this.seekSpeed * Time.deltaTime);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, this.targetRotation, 720f * Time.deltaTime);
		}

		private void ChargeAtPlayer()
		{
			base.transform.position = Vector3.MoveTowards(base.transform.position, this.targetPosition, this.chargeSpeed * Time.deltaTime);
			base.transform.rotation = Quaternion.RotateTowards(base.transform.rotation, this.targetRotation, 720f * Time.deltaTime);
		}

		private void UpdateGhostVisibility()
		{
			switch (this.currentState)
			{
			case LurkerGhost.ghostState.patrol:
				this.meshRenderer.sharedMaterial = this.scryableMaterial;
				this.bonesMeshRenderer.sharedMaterial = this.scryableMaterialBones;
				return;
			case LurkerGhost.ghostState.seek:
			case LurkerGhost.ghostState.charge:
				if (this.targetPlayer == NetworkSystem.Instance.LocalPlayer || this.passingPlayer == NetworkSystem.Instance.LocalPlayer)
				{
					this.meshRenderer.sharedMaterial = this.visibleMaterial;
					this.bonesMeshRenderer.sharedMaterial = this.visibleMaterialBones;
					return;
				}
				this.meshRenderer.sharedMaterial = this.scryableMaterial;
				this.bonesMeshRenderer.sharedMaterial = this.scryableMaterialBones;
				return;
			case LurkerGhost.ghostState.possess:
				if (this.targetPlayer == NetworkSystem.Instance.LocalPlayer || this.passingPlayer == NetworkSystem.Instance.LocalPlayer)
				{
					this.meshRenderer.sharedMaterial = this.visibleMaterial;
					this.bonesMeshRenderer.sharedMaterial = this.visibleMaterialBones;
					return;
				}
				this.meshRenderer.sharedMaterial = this.scryableMaterial;
				this.bonesMeshRenderer.sharedMaterial = this.scryableMaterialBones;
				return;
			default:
				return;
			}
		}

		private void HauntObjects()
		{
			Collider[] array = new Collider[20];
			int num = Physics.OverlapSphereNonAlloc(base.transform.position, this.sphereColliderRadius, array);
			for (int i = 0; i < num; i++)
			{
				if (array[i].CompareTag("HauntedObject"))
				{
					UnityAction<GameObject> triggerHauntedObjects = this.TriggerHauntedObjects;
					if (triggerHauntedObjects != null)
					{
						triggerHauntedObjects(array[i].gameObject);
					}
				}
			}
		}

		private void ChangeState(LurkerGhost.ghostState newState)
		{
			this.currentState = newState;
			VRRig vrrig = null;
			switch (this.currentState)
			{
			case LurkerGhost.ghostState.patrol:
				this.PlaySound(this.patrolAudio, true);
				this.passingPlayer = null;
				this.cooldownTimeRemaining = Random.Range(this.cooldownDuration, this.maxCooldownDuration);
				this.currentRepeatHuntTimes = 0;
				break;
			case LurkerGhost.ghostState.charge:
				this.PlaySound(this.huntAudio, false);
				this.targetPosition = this.targetTransform.position;
				this.targetRotation = Quaternion.LookRotation(this.targetTransform.position - base.transform.position);
				break;
			case LurkerGhost.ghostState.possess:
				if (this.targetPlayer == NetworkSystem.Instance.LocalPlayer)
				{
					this.PlaySound(this.possessedAudio, true);
					GorillaTagger.Instance.StartVibration(true, this.hapticStrength, this.hapticDuration);
					GorillaTagger.Instance.StartVibration(false, this.hapticStrength, this.hapticDuration);
				}
				vrrig = GorillaGameManager.StaticFindRigForPlayer(this.targetPlayer);
				break;
			}
			Shader.SetGlobalFloat(this._BlackAndWhite, (float)((newState == LurkerGhost.ghostState.possess && this.targetPlayer == NetworkSystem.Instance.LocalPlayer) ? 1 : 0));
			if (vrrig != this.lastHauntedVRRig && this.lastHauntedVRRig != null)
			{
				this.lastHauntedVRRig.IsHaunted = false;
			}
			if (vrrig != null)
			{
				vrrig.IsHaunted = true;
			}
			this.lastHauntedVRRig = vrrig;
			this.UpdateGhostVisibility();
		}

		private void OnDestroy()
		{
			NetworkBehaviourUtils.InternalOnDestroy(this);
			Shader.SetGlobalFloat(this._BlackAndWhite, 0f);
		}

		private void UpdateState()
		{
			switch (this.currentState)
			{
			case LurkerGhost.ghostState.patrol:
				this.Patrol();
				if (base.IsMine)
				{
					if (this.currentWaypoint == null || Vector3.Distance(base.transform.position, this.currentWaypoint.position) < 0.2f)
					{
						this.PickNextWaypoint();
					}
					this.cooldownTimeRemaining -= Time.deltaTime;
					if (this.cooldownTimeRemaining <= 0f)
					{
						this.cooldownTimeRemaining = 0f;
						if (this.PickPlayer(this.maxHuntDistance))
						{
							this.ChangeState(LurkerGhost.ghostState.seek);
							return;
						}
					}
				}
				break;
			case LurkerGhost.ghostState.seek:
				this.SeekPlayer();
				if (base.IsMine && (this.targetPosition - base.transform.position).sqrMagnitude < this.seekCloseEnoughDistance * this.seekCloseEnoughDistance)
				{
					this.ChangeState(LurkerGhost.ghostState.charge);
					return;
				}
				break;
			case LurkerGhost.ghostState.charge:
				this.ChargeAtPlayer();
				if (base.IsMine && (this.targetPosition - base.transform.position).sqrMagnitude < 0.25f)
				{
					if ((this.targetTransform.position - this.targetPosition).magnitude < this.minCatchDistance)
					{
						this.ChangeState(LurkerGhost.ghostState.possess);
						return;
					}
					this.huntedPassedTime = 0f;
					this.ChangeState(LurkerGhost.ghostState.patrol);
					return;
				}
				break;
			case LurkerGhost.ghostState.possess:
				if (this.targetTransform != null)
				{
					float num = this.SpookyMagicNumbers.x + MathF.Abs(MathF.Sin(Time.time * this.SpookyMagicNumbers.y));
					float num2 = this.HauntedMagicNumbers.x * MathF.Sin(Time.time * this.HauntedMagicNumbers.y) + this.HauntedMagicNumbers.z * MathF.Sin(Time.time * this.HauntedMagicNumbers.w);
					float num3 = 0.5f + 0.5f * MathF.Sin(Time.time * this.SpookyMagicNumbers.z);
					Vector3 vector = this.targetTransform.position + new Vector3(num * (float)Math.Sin((double)num2), num3, num * (float)Math.Cos((double)num2));
					base.transform.position = Vector3.MoveTowards(base.transform.position, vector, this.chargeSpeed);
					base.transform.rotation = Quaternion.LookRotation(base.transform.position - this.targetTransform.position);
				}
				if (base.IsMine)
				{
					this.huntedPassedTime += Time.deltaTime;
					if (this.huntedPassedTime >= this.PossessionDuration)
					{
						this.huntedPassedTime = 0f;
						if (this.hauntNeighbors && this.currentRepeatHuntTimes < this.maxRepeatHuntTimes && this.PickPlayer(this.maxRepeatHuntDistance))
						{
							this.currentRepeatHuntTimes++;
							this.ChangeState(LurkerGhost.ghostState.seek);
							return;
						}
						this.ChangeState(LurkerGhost.ghostState.patrol);
					}
				}
				break;
			default:
				return;
			}
		}

		[Networked]
		[NetworkedWeaved(0, 6)]
		private unsafe LurkerGhost.LurkerGhostData Data
		{
			get
			{
				if (this.Ptr == null)
				{
					throw new InvalidOperationException("Error when accessing LurkerGhost.Data. Networked properties can only be accessed when Spawned() has been called.");
				}
				return *(LurkerGhost.LurkerGhostData*)(this.Ptr + 0);
			}
			set
			{
				if (this.Ptr == null)
				{
					throw new InvalidOperationException("Error when accessing LurkerGhost.Data. Networked properties can only be accessed when Spawned() has been called.");
				}
				*(LurkerGhost.LurkerGhostData*)(this.Ptr + 0) = value;
			}
		}

		public override void WriteDataFusion()
		{
			this.Data = new LurkerGhost.LurkerGhostData(this.currentState, this.currentIndex, this.targetPlayer.ActorNumber, this.targetPosition);
		}

		public override void ReadDataFusion()
		{
			this.ReadDataShared(this.Data.CurrentState, this.Data.CurrentIndex, this.Data.TargetActor, this.Data.TargetPos);
		}

		protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.Sender != PhotonNetwork.MasterClient)
			{
				return;
			}
			stream.SendNext(this.currentState);
			stream.SendNext(this.currentIndex);
			if (this.targetPlayer != null)
			{
				stream.SendNext(this.targetPlayer.ActorNumber);
			}
			else
			{
				stream.SendNext(-1);
			}
			stream.SendNext(this.targetPosition);
		}

		protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
		{
			if (info.Sender != PhotonNetwork.MasterClient)
			{
				return;
			}
			LurkerGhost.ghostState ghostState = (LurkerGhost.ghostState)stream.ReceiveNext();
			int num = (int)stream.ReceiveNext();
			int num2 = (int)stream.ReceiveNext();
			Vector3 vector = (Vector3)stream.ReceiveNext();
			this.ReadDataShared(ghostState, num, num2, vector);
		}

		private void ReadDataShared(LurkerGhost.ghostState state, int index, int targetActorNumber, Vector3 targetPos)
		{
			LurkerGhost.ghostState ghostState = this.currentState;
			this.currentState = state;
			this.currentIndex = index;
			NetPlayer netPlayer = this.targetPlayer;
			this.targetPlayer = NetworkSystem.Instance.GetPlayer(targetActorNumber);
			this.targetPosition = targetPos;
			float num = 10000f;
			if (!(in this.targetPosition).IsValid(in num))
			{
				RigContainer rigContainer;
				if (VRRigCache.Instance.TryGetVrrig(this.targetPlayer, out rigContainer))
				{
					this.targetPosition = (this.targetPlayer.IsLocal ? rigContainer.Rig.transform.position : rigContainer.Rig.syncPos);
				}
				else
				{
					this.targetPosition = base.transform.position;
				}
			}
			if (this.targetPlayer != netPlayer)
			{
				this.PickPlayer(this.targetPlayer);
			}
			if (ghostState != this.currentState || this.targetPlayer != netPlayer)
			{
				this.ChangeState(this.currentState);
			}
		}

		public override void OnOwnerChange(Player newOwner, Player previousOwner)
		{
			base.OnOwnerChange(newOwner, previousOwner);
			if (newOwner == PhotonNetwork.LocalPlayer)
			{
				this.ChangeState(this.currentState);
			}
		}

		[WeaverGenerated]
		public override void CopyBackingFieldsToState(bool A_1)
		{
			base.CopyBackingFieldsToState(A_1);
			this.Data = this._Data;
		}

		[WeaverGenerated]
		public override void CopyStateToBackingFields()
		{
			base.CopyStateToBackingFields();
			this._Data = this.Data;
		}

		public float patrolSpeed = 3f;

		public float seekSpeed = 6f;

		public float chargeSpeed = 6f;

		[Tooltip("Cooldown until the next time the ghost needs to hunt a new player")]
		public float cooldownDuration = 10f;

		[Tooltip("Max Cooldown (randomized)")]
		public float maxCooldownDuration = 10f;

		[Tooltip("How long the possession effects should last")]
		public float PossessionDuration = 15f;

		[Tooltip("Hunted objects within this radius will get triggered ")]
		public float sphereColliderRadius = 2f;

		[Tooltip("Maximum distance to the possible player to get hunted")]
		public float maxHuntDistance = 20f;

		[Tooltip("Minimum distance from the player to start the possession effects")]
		public float minCatchDistance = 2f;

		[Tooltip("Maximum distance to the possible player to get repeat hunted")]
		public float maxRepeatHuntDistance = 5f;

		[Tooltip("Maximum times the lurker can haunt a nearby player before going back on cooldown")]
		public int maxRepeatHuntTimes = 3;

		[Tooltip("Time in seconds before a haunted player can pass the lurker to another player by tagging")]
		public float tagCoolDown = 2f;

		[Tooltip("UP & DOWN, IN & OUT")]
		public Vector3 SpookyMagicNumbers = new Vector3(1f, 1f, 1f);

		[Tooltip("SPIN, SPIN, SPIN, SPIN")]
		public Vector4 HauntedMagicNumbers = new Vector4(1f, 2f, 3f, 1f);

		[Tooltip("Haptic vibration when haunted by the ghost")]
		public float hapticStrength = 1f;

		public float hapticDuration = 1.5f;

		public GameObject waypointsContainer;

		private ZoneBasedObject[] waypointRegions;

		private ZoneBasedObject lastWaypointRegion;

		private List<Transform> waypoints = new List<Transform>();

		private Transform currentWaypoint;

		public Material visibleMaterial;

		public Material scryableMaterial;

		public Material visibleMaterialBones;

		public Material scryableMaterialBones;

		public MeshRenderer meshRenderer;

		public MeshRenderer bonesMeshRenderer;

		[SerializeField]
		private AudioSource audioSource;

		public AudioClip patrolAudio;

		public AudioClip huntAudio;

		public AudioClip possessedAudio;

		public ThrowableSetDressing scryingGlass;

		public float scryingAngerAngle;

		public float scryingAngerDelay;

		public float seekAheadDistance;

		public float seekCloseEnoughDistance;

		private float scryingAngerAfterTimestamp;

		private int currentRepeatHuntTimes;

		public UnityAction<GameObject> TriggerHauntedObjects;

		private int currentIndex;

		private LurkerGhost.ghostState currentState;

		private float cooldownTimeRemaining;

		private List<NetPlayer> possibleTargets;

		private NetPlayer targetPlayer;

		private Transform targetTransform;

		private float huntedPassedTime;

		private Vector3 targetPosition;

		private Quaternion targetRotation;

		private VRRig targetVRRig;

		private ShaderHashId _BlackAndWhite = "_BlackAndWhite";

		private VRRig lastHauntedVRRig;

		private float nextTagTime;

		private NetPlayer passingPlayer;

		[SerializeField]
		private bool hauntNeighbors = true;

		[WeaverGenerated]
		[DefaultForProperty("Data", 0, 6)]
		[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
		private LurkerGhost.LurkerGhostData _Data;

		private enum ghostState
		{
			patrol,
			seek,
			charge,
			possess
		}

		[NetworkStructWeaved(6)]
		[StructLayout(LayoutKind.Explicit, Size = 24)]
		private struct LurkerGhostData : INetworkStruct
		{
			public LurkerGhost.ghostState CurrentState { readonly get; set; }

			public int CurrentIndex { readonly get; set; }

			public int TargetActor { readonly get; set; }

			[Networked]
			[NetworkedWeaved(3, 3)]
			public unsafe Vector3 TargetPos
			{
				readonly get
				{
					return *(Vector3*)Native.ReferenceToPointer<FixedStorage@3>(ref this._TargetPos);
				}
				set
				{
					*(Vector3*)Native.ReferenceToPointer<FixedStorage@3>(ref this._TargetPos) = value;
				}
			}

			public LurkerGhostData(LurkerGhost.ghostState state, int index, int actor, Vector3 pos)
			{
				this.CurrentState = state;
				this.CurrentIndex = index;
				this.TargetActor = actor;
				this.TargetPos = pos;
			}

			[FixedBufferProperty(typeof(Vector3), typeof(UnityValueSurrogate@ElementReaderWriterVector3), 0, order = -2147483647)]
			[WeaverGenerated]
			[SerializeField]
			[FieldOffset(12)]
			private FixedStorage@3 _TargetPos;
		}
	}
}
