using System;
using GorillaExtensions;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaLocomotion.Swimming;
using GorillaTag.GuidedRefs;
using Photon.Pun;
using UnityEngine;

namespace GorillaTag
{
	public class InfectionLavaController : MonoBehaviour, IGorillaSerializeableScene, IGorillaSerializeable, ITickSystemPost, IGuidedRefReceiverMono, IGuidedRefMonoBehaviour, IGuidedRefObject
	{
		public static InfectionLavaController Instance
		{
			get
			{
				return InfectionLavaController.instance;
			}
		}

		public bool LavaCurrentlyActivated
		{
			get
			{
				return this.reliableState.state > InfectionLavaController.RisingLavaState.Drained;
			}
		}

		public Plane LavaPlane
		{
			get
			{
				return new Plane(this.lavaSurfacePlaneTransform.up, this.lavaSurfacePlaneTransform.position);
			}
		}

		public Vector3 SurfaceCenter
		{
			get
			{
				return this.lavaSurfacePlaneTransform.position;
			}
		}

		private int PlayerCount
		{
			get
			{
				int num = 1;
				GorillaGameManager gorillaGameManager = GorillaGameManager.instance;
				if (gorillaGameManager != null && gorillaGameManager.currentNetPlayerArray != null)
				{
					num = gorillaGameManager.currentNetPlayerArray.Length;
				}
				return num;
			}
		}

		private bool InCompetitiveQueue
		{
			get
			{
				return NetworkSystem.Instance.InRoom && NetworkSystem.Instance.GameModeString.Contains("COMPETITIVE");
			}
		}

		private void Awake()
		{
			if (InfectionLavaController.instance.IsNotNull())
			{
				Object.Destroy(base.gameObject);
				return;
			}
			InfectionLavaController.instance = this;
			RoomSystem.LeftRoomEvent += new Action(this.OnLeftRoom);
			RoomSystem.PlayerLeftEvent += new Action<NetPlayer>(this.OnPlayerLeftRoom);
			((IGuidedRefObject)this).GuidedRefInitialize();
			if (this.lavaVolume != null)
			{
				this.lavaVolume.ColliderEnteredWater += this.OnColliderEnteredLava;
			}
			if (this.lavaActivationProjectileHitNotifier != null)
			{
				this.lavaActivationProjectileHitNotifier.OnProjectileHit += this.OnActivationLavaProjectileHit;
			}
		}

		protected void OnEnable()
		{
			if (!this.guidedRefsFullyResolved)
			{
				return;
			}
			this.VerifyReferences();
			TickSystem<object>.AddPostTickCallback(this);
		}

		void IGorillaSerializeableScene.OnSceneLinking(GorillaSerializerScene netObj)
		{
			this.networkObject = netObj;
		}

		protected void OnDisable()
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		private void VerifyReferences()
		{
			this.IfNullThenLogAndDisableSelf(this.lavaMeshTransform, "lavaMeshTransform", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaSurfacePlaneTransform, "lavaSurfacePlaneTransform", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaVolume, "lavaVolume", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaActivationRenderer, "lavaActivationRenderer", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaActivationStartPos, "lavaActivationStartPos", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaActivationEndPos, "lavaActivationEndPos", -1);
			this.IfNullThenLogAndDisableSelf(this.lavaActivationProjectileHitNotifier, "lavaActivationProjectileHitNotifier", -1);
			for (int i = 0; i < this.volcanoEffects.Length; i++)
			{
				this.IfNullThenLogAndDisableSelf(this.volcanoEffects[i], "volcanoEffects", i);
			}
		}

		private void IfNullThenLogAndDisableSelf(Object obj, string fieldName, int index = -1)
		{
			if (obj != null)
			{
				return;
			}
			fieldName = ((index != -1) ? string.Format("{0}[{1}]", fieldName, index) : fieldName);
			Debug.LogError("InfectionLavaController: Disabling self because reference `" + fieldName + "` is null.", this);
			base.enabled = false;
		}

		private void OnDestroy()
		{
			if (InfectionLavaController.instance == this)
			{
				InfectionLavaController.instance = null;
			}
			TickSystem<object>.RemovePostTickCallback(this);
			this.UpdateLava(0f);
			if (this.lavaVolume != null)
			{
				this.lavaVolume.ColliderEnteredWater -= this.OnColliderEnteredLava;
			}
			if (this.lavaActivationProjectileHitNotifier != null)
			{
				this.lavaActivationProjectileHitNotifier.OnProjectileHit -= this.OnActivationLavaProjectileHit;
			}
		}

		bool ITickSystemPost.PostTickRunning { get; set; }

		void ITickSystemPost.PostTick()
		{
			this.prevTime = this.currentTime;
			this.currentTime = (NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.SimTime : Time.timeAsDouble);
			if (this.networkObject.HasAuthority)
			{
				this.UpdateReliableState(this.currentTime, ref this.reliableState);
			}
			this.UpdateLocalState(this.currentTime, this.reliableState);
			this.localLagLavaProgressOffset = Mathf.MoveTowards(this.localLagLavaProgressOffset, 0f, this.lagResolutionLavaProgressPerSecond * Time.deltaTime);
			this.UpdateLava(this.lavaProgressSmooth + this.localLagLavaProgressOffset);
			this.UpdateVolcanoActivationLava((float)this.reliableState.activationProgress);
			this.CheckLocalPlayerAgainstLava(this.currentTime);
		}

		private void JumpToState(InfectionLavaController.RisingLavaState state)
		{
			this.reliableState.state = state;
			switch (state)
			{
			case InfectionLavaController.RisingLavaState.Drained:
			{
				for (int i = 0; i < this.volcanoEffects.Length; i++)
				{
					VolcanoEffects volcanoEffects = this.volcanoEffects[i];
					if (volcanoEffects != null)
					{
						volcanoEffects.SetDrainedState();
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Erupting:
			{
				for (int j = 0; j < this.volcanoEffects.Length; j++)
				{
					VolcanoEffects volcanoEffects2 = this.volcanoEffects[j];
					if (volcanoEffects2 != null)
					{
						volcanoEffects2.SetEruptingState();
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Rising:
			{
				for (int k = 0; k < this.volcanoEffects.Length; k++)
				{
					VolcanoEffects volcanoEffects3 = this.volcanoEffects[k];
					if (volcanoEffects3 != null)
					{
						volcanoEffects3.SetRisingState();
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Full:
			{
				for (int l = 0; l < this.volcanoEffects.Length; l++)
				{
					VolcanoEffects volcanoEffects4 = this.volcanoEffects[l];
					if (volcanoEffects4 != null)
					{
						volcanoEffects4.SetFullState();
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Draining:
			{
				for (int m = 0; m < this.volcanoEffects.Length; m++)
				{
					VolcanoEffects volcanoEffects5 = this.volcanoEffects[m];
					if (volcanoEffects5 != null)
					{
						volcanoEffects5.SetDrainingState();
					}
				}
				return;
			}
			default:
				return;
			}
		}

		private void UpdateReliableState(double currentTime, ref InfectionLavaController.LavaSyncData syncData)
		{
			if (currentTime < syncData.stateStartTime)
			{
				syncData.stateStartTime = currentTime;
			}
			switch (syncData.state)
			{
			default:
				if (syncData.activationProgress > 1.0)
				{
					float playerCount = (float)this.PlayerCount;
					float num = (this.InCompetitiveQueue ? this.activationVotePercentageCompetitiveQueue : this.activationVotePercentageDefaultQueue);
					int num2 = Mathf.RoundToInt(playerCount * num);
					if (this.lavaActivationVoteCount >= num2)
					{
						for (int i = 0; i < this.lavaActivationVoteCount; i++)
						{
							this.lavaActivationVotePlayerIds[i] = 0;
						}
						this.lavaActivationVoteCount = 0;
						syncData.state = InfectionLavaController.RisingLavaState.Erupting;
						syncData.stateStartTime = currentTime;
						syncData.activationProgress = 1.0;
						for (int j = 0; j < this.volcanoEffects.Length; j++)
						{
							VolcanoEffects volcanoEffects = this.volcanoEffects[j];
							if (volcanoEffects != null)
							{
								volcanoEffects.SetEruptingState();
							}
						}
						return;
					}
				}
				else
				{
					float num3 = Mathf.Clamp((float)(currentTime - this.prevTime), 0f, 0.1f);
					double activationProgress = syncData.activationProgress;
					syncData.activationProgress = (double)Mathf.MoveTowards((float)syncData.activationProgress, 0f, this.lavaActivationDrainRateVsPlayerCount.Evaluate((float)this.PlayerCount) * num3);
					if (activationProgress > 0.0 && syncData.activationProgress <= 5E-324)
					{
						VolcanoEffects[] array = this.volcanoEffects;
						for (int k = 0; k < array.Length; k++)
						{
							array[k].OnVolcanoBellyEmpty();
						}
						return;
					}
				}
				break;
			case InfectionLavaController.RisingLavaState.Erupting:
				if (currentTime > syncData.stateStartTime + (double)this.eruptTime)
				{
					syncData.state = InfectionLavaController.RisingLavaState.Rising;
					syncData.stateStartTime = currentTime;
					for (int l = 0; l < this.volcanoEffects.Length; l++)
					{
						VolcanoEffects volcanoEffects2 = this.volcanoEffects[l];
						if (volcanoEffects2 != null)
						{
							volcanoEffects2.SetRisingState();
						}
					}
					return;
				}
				break;
			case InfectionLavaController.RisingLavaState.Rising:
				if (currentTime > syncData.stateStartTime + (double)this.riseTime)
				{
					syncData.state = InfectionLavaController.RisingLavaState.Full;
					syncData.stateStartTime = currentTime;
					for (int m = 0; m < this.volcanoEffects.Length; m++)
					{
						VolcanoEffects volcanoEffects3 = this.volcanoEffects[m];
						if (volcanoEffects3 != null)
						{
							volcanoEffects3.SetFullState();
						}
					}
					return;
				}
				break;
			case InfectionLavaController.RisingLavaState.Full:
				if (currentTime > syncData.stateStartTime + (double)this.fullTime)
				{
					syncData.state = InfectionLavaController.RisingLavaState.Draining;
					syncData.stateStartTime = currentTime;
					for (int n = 0; n < this.volcanoEffects.Length; n++)
					{
						VolcanoEffects volcanoEffects4 = this.volcanoEffects[n];
						if (volcanoEffects4 != null)
						{
							volcanoEffects4.SetDrainingState();
						}
					}
					return;
				}
				break;
			case InfectionLavaController.RisingLavaState.Draining:
				syncData.activationProgress = (double)Mathf.MoveTowards((float)syncData.activationProgress, 0f, this.lavaActivationDrainRateVsPlayerCount.Evaluate((float)this.PlayerCount) * Time.deltaTime);
				if (currentTime > syncData.stateStartTime + (double)this.drainTime)
				{
					syncData.state = InfectionLavaController.RisingLavaState.Drained;
					syncData.stateStartTime = currentTime;
					for (int num4 = 0; num4 < this.volcanoEffects.Length; num4++)
					{
						VolcanoEffects volcanoEffects5 = this.volcanoEffects[num4];
						if (volcanoEffects5 != null)
						{
							volcanoEffects5.SetDrainedState();
						}
					}
				}
				break;
			}
		}

		private void UpdateLocalState(double currentTime, InfectionLavaController.LavaSyncData syncData)
		{
			switch (syncData.state)
			{
			default:
			{
				this.lavaProgressLinear = 0f;
				this.lavaProgressSmooth = 0f;
				float num = (float)(currentTime - syncData.stateStartTime);
				foreach (VolcanoEffects volcanoEffects in this.volcanoEffects)
				{
					if (volcanoEffects != null)
					{
						volcanoEffects.UpdateDrainedState(num);
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Erupting:
			{
				this.lavaProgressLinear = 0f;
				this.lavaProgressSmooth = 0f;
				float num2 = (float)(currentTime - syncData.stateStartTime);
				float num3 = Mathf.Clamp01(num2 / this.eruptTime);
				foreach (VolcanoEffects volcanoEffects2 in this.volcanoEffects)
				{
					if (volcanoEffects2 != null)
					{
						volcanoEffects2.UpdateEruptingState(num2, this.eruptTime - num2, num3);
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Rising:
			{
				float num4 = (float)(currentTime - syncData.stateStartTime) / this.riseTime;
				this.lavaProgressLinear = Mathf.Clamp01(num4);
				this.lavaProgressSmooth = this.lavaProgressAnimationCurve.Evaluate(this.lavaProgressLinear);
				float num5 = (float)(currentTime - syncData.stateStartTime);
				foreach (VolcanoEffects volcanoEffects3 in this.volcanoEffects)
				{
					if (volcanoEffects3 != null)
					{
						volcanoEffects3.UpdateRisingState(num5, this.riseTime - num5, this.lavaProgressLinear);
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Full:
			{
				this.lavaProgressLinear = 1f;
				this.lavaProgressSmooth = 1f;
				float num6 = (float)(currentTime - syncData.stateStartTime);
				float num7 = Mathf.Clamp01(this.fullTime / num6);
				foreach (VolcanoEffects volcanoEffects4 in this.volcanoEffects)
				{
					if (volcanoEffects4 != null)
					{
						volcanoEffects4.UpdateFullState(num6, this.fullTime - num6, num7);
					}
				}
				return;
			}
			case InfectionLavaController.RisingLavaState.Draining:
			{
				float num8 = (float)(currentTime - syncData.stateStartTime);
				float num9 = Mathf.Clamp01(num8 / this.drainTime);
				this.lavaProgressLinear = 1f - num9;
				this.lavaProgressSmooth = this.lavaProgressAnimationCurve.Evaluate(this.lavaProgressLinear);
				foreach (VolcanoEffects volcanoEffects5 in this.volcanoEffects)
				{
					if (volcanoEffects5 != null)
					{
						volcanoEffects5.UpdateDrainingState(num8, this.riseTime - num8, num9);
					}
				}
				return;
			}
			}
		}

		private void UpdateLava(float fillProgress)
		{
			this.lavaScale = Mathf.Lerp(this.lavaMeshMinScale, this.lavaMeshMaxScale, fillProgress);
			if (this.lavaMeshTransform != null)
			{
				this.lavaMeshTransform.localScale = new Vector3(this.lavaMeshTransform.localScale.x, this.lavaMeshTransform.localScale.y, this.lavaScale);
			}
		}

		private void UpdateVolcanoActivationLava(float activationProgress)
		{
			this.activationProgessSmooth = Mathf.MoveTowards(this.activationProgessSmooth, activationProgress, this.lavaActivationVisualMovementProgressPerSecond * Time.deltaTime);
			this.lavaActivationRenderer.material.SetColor(ShaderProps._BaseColor, this.lavaActivationGradient.Evaluate(activationProgress));
			this.lavaActivationRenderer.transform.position = Vector3.Lerp(this.lavaActivationStartPos.position, this.lavaActivationEndPos.position, this.activationProgessSmooth);
		}

		private void CheckLocalPlayerAgainstLava(double currentTime)
		{
			if (GTPlayer.Instance.InWater && GTPlayer.Instance.CurrentWaterVolume == this.lavaVolume)
			{
				this.LocalPlayerInLava(currentTime, false);
			}
		}

		private void OnColliderEnteredLava(WaterVolume volume, Collider collider)
		{
			if (collider == GTPlayer.Instance.bodyCollider)
			{
				this.LocalPlayerInLava(NetworkSystem.Instance.InRoom ? NetworkSystem.Instance.SimTime : Time.timeAsDouble, true);
			}
		}

		private void LocalPlayerInLava(double currentTime, bool enteredLavaThisFrame)
		{
			GorillaGameManager gorillaGameManager = GorillaGameManager.instance;
			if (gorillaGameManager != null && gorillaGameManager.CanAffectPlayer(NetworkSystem.Instance.LocalPlayer, enteredLavaThisFrame) && (currentTime - this.lastTagSelfRPCTime > 0.5 || enteredLavaThisFrame))
			{
				this.lastTagSelfRPCTime = currentTime;
				GameMode.ReportHit();
			}
		}

		public void OnActivationLavaProjectileHit(SlingshotProjectile projectile, Collision collision)
		{
			if (projectile.gameObject.CompareTag("LavaRockProjectile"))
			{
				this.AddLavaRock(projectile.projectileOwner.ActorNumber);
			}
		}

		private void AddLavaRock(int playerId)
		{
			if (this.networkObject.HasAuthority && this.reliableState.state == InfectionLavaController.RisingLavaState.Drained)
			{
				float num = this.lavaActivationRockProgressVsPlayerCount.Evaluate((float)this.PlayerCount);
				this.reliableState.activationProgress = this.reliableState.activationProgress + (double)num;
				this.AddVoteForVolcanoActivation(playerId);
				VolcanoEffects[] array = this.volcanoEffects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].OnStoneAccepted(this.reliableState.activationProgress);
				}
			}
		}

		private void AddVoteForVolcanoActivation(int playerId)
		{
			if (this.networkObject.HasAuthority && this.lavaActivationVoteCount < 10)
			{
				bool flag = false;
				for (int i = 0; i < this.lavaActivationVoteCount; i++)
				{
					if (this.lavaActivationVotePlayerIds[i] == playerId)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					this.lavaActivationVotePlayerIds[this.lavaActivationVoteCount] = playerId;
					this.lavaActivationVoteCount++;
				}
			}
		}

		private void RemoveVoteForVolcanoActivation(int playerId)
		{
			if (this.networkObject.HasAuthority)
			{
				for (int i = 0; i < this.lavaActivationVoteCount; i++)
				{
					if (this.lavaActivationVotePlayerIds[i] == playerId)
					{
						this.lavaActivationVotePlayerIds[i] = this.lavaActivationVotePlayerIds[this.lavaActivationVoteCount - 1];
						this.lavaActivationVoteCount--;
						return;
					}
				}
			}
		}

		void IGorillaSerializeable.OnSerializeWrite(PhotonStream stream, PhotonMessageInfo info)
		{
			stream.SendNext((int)this.reliableState.state);
			stream.SendNext(this.reliableState.stateStartTime);
			stream.SendNext(this.reliableState.activationProgress);
			stream.SendNext(this.lavaActivationVoteCount);
			stream.SendNext(this.lavaActivationVotePlayerIds[0]);
			stream.SendNext(this.lavaActivationVotePlayerIds[1]);
			stream.SendNext(this.lavaActivationVotePlayerIds[2]);
			stream.SendNext(this.lavaActivationVotePlayerIds[3]);
			stream.SendNext(this.lavaActivationVotePlayerIds[4]);
			stream.SendNext(this.lavaActivationVotePlayerIds[5]);
			stream.SendNext(this.lavaActivationVotePlayerIds[6]);
			stream.SendNext(this.lavaActivationVotePlayerIds[7]);
			stream.SendNext(this.lavaActivationVotePlayerIds[8]);
			stream.SendNext(this.lavaActivationVotePlayerIds[9]);
		}

		void IGorillaSerializeable.OnSerializeRead(PhotonStream stream, PhotonMessageInfo info)
		{
			InfectionLavaController.RisingLavaState risingLavaState = (InfectionLavaController.RisingLavaState)((int)stream.ReceiveNext());
			this.reliableState.stateStartTime = ((double)stream.ReceiveNext()).GetFinite();
			this.reliableState.activationProgress = ((double)stream.ReceiveNext()).ClampSafe(0.0, 2.0);
			this.lavaActivationVoteCount = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[0] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[1] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[2] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[3] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[4] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[5] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[6] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[7] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[8] = (int)stream.ReceiveNext();
			this.lavaActivationVotePlayerIds[9] = (int)stream.ReceiveNext();
			float num = this.lavaProgressSmooth;
			if (risingLavaState != this.reliableState.state)
			{
				this.JumpToState(risingLavaState);
			}
			this.UpdateLocalState((double)((float)NetworkSystem.Instance.SimTime), this.reliableState);
			this.localLagLavaProgressOffset = num - this.lavaProgressSmooth;
		}

		public void OnPlayerLeftRoom(NetPlayer otherNetPlayer)
		{
			this.RemoveVoteForVolcanoActivation(otherNetPlayer.ActorNumber);
		}

		private void OnLeftRoom()
		{
			for (int i = 0; i < this.lavaActivationVotePlayerIds.Length; i++)
			{
				if (this.lavaActivationVotePlayerIds[i] != NetworkSystem.Instance.LocalPlayerID)
				{
					this.RemoveVoteForVolcanoActivation(this.lavaActivationVotePlayerIds[i]);
				}
			}
		}

		void IGorillaSerializeableScene.OnNetworkObjectDisable()
		{
		}

		void IGorillaSerializeableScene.OnNetworkObjectEnable()
		{
		}

		int IGuidedRefReceiverMono.GuidedRefsWaitingToResolveCount { get; set; }

		void IGuidedRefReceiverMono.OnAllGuidedRefsResolved()
		{
			this.guidedRefsFullyResolved = true;
			this.VerifyReferences();
			TickSystem<object>.AddPostTickCallback(this);
		}

		public void OnGuidedRefTargetDestroyed(int fieldId)
		{
			this.guidedRefsFullyResolved = false;
			TickSystem<object>.RemovePostTickCallback(this);
		}

		void IGuidedRefObject.GuidedRefInitialize()
		{
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaMeshTransform_gRef", ref this.lavaMeshTransform_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaSurfacePlaneTransform_gRef", ref this.lavaSurfacePlaneTransform_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaVolume_gRef", ref this.lavaVolume_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaActivationRenderer_gRef", ref this.lavaActivationRenderer_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaActivationStartPos_gRef", ref this.lavaActivationStartPos_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaActivationEndPos_gRef", ref this.lavaActivationEndPos_gRef);
			GuidedRefHub.RegisterReceiverField<InfectionLavaController>(this, "lavaActivationProjectileHitNotifier_gRef", ref this.lavaActivationProjectileHitNotifier_gRef);
			GuidedRefHub.RegisterReceiverArray<InfectionLavaController, VolcanoEffects>(this, "volcanoEffects_gRefs", ref this.volcanoEffects, ref this.volcanoEffects_gRefs);
			GuidedRefHub.ReceiverFullyRegistered<InfectionLavaController>(this);
		}

		bool IGuidedRefReceiverMono.GuidedRefTryResolveReference(GuidedRefTryResolveInfo target)
		{
			return GuidedRefHub.TryResolveField<InfectionLavaController, Transform>(this, ref this.lavaMeshTransform, this.lavaMeshTransform_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, Transform>(this, ref this.lavaSurfacePlaneTransform, this.lavaSurfacePlaneTransform_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, WaterVolume>(this, ref this.lavaVolume, this.lavaVolume_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, MeshRenderer>(this, ref this.lavaActivationRenderer, this.lavaActivationRenderer_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, Transform>(this, ref this.lavaActivationStartPos, this.lavaActivationStartPos_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, Transform>(this, ref this.lavaActivationEndPos, this.lavaActivationEndPos_gRef, target) || GuidedRefHub.TryResolveField<InfectionLavaController, SlingshotProjectileHitNotifier>(this, ref this.lavaActivationProjectileHitNotifier, this.lavaActivationProjectileHitNotifier_gRef, target) || GuidedRefHub.TryResolveArrayItem<InfectionLavaController, VolcanoEffects>(this, this.volcanoEffects, this.volcanoEffects_gRefs, target);
		}

		Transform IGuidedRefMonoBehaviour.get_transform()
		{
			return base.transform;
		}

		int IGuidedRefObject.GetInstanceID()
		{
			return base.GetInstanceID();
		}

		[OnEnterPlay_SetNull]
		private static InfectionLavaController instance;

		[SerializeField]
		private float lavaMeshMinScale = 3.17f;

		[Tooltip("If you throw rocks into the volcano quickly enough, then it will raise to this height.")]
		[SerializeField]
		private float lavaMeshMaxScale = 8.941086f;

		[SerializeField]
		private float eruptTime = 3f;

		[SerializeField]
		private float riseTime = 10f;

		[SerializeField]
		private float fullTime = 240f;

		[SerializeField]
		private float drainTime = 10f;

		[SerializeField]
		private float lagResolutionLavaProgressPerSecond = 0.2f;

		[SerializeField]
		private AnimationCurve lavaProgressAnimationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[Header("Volcano Activation")]
		[SerializeField]
		[Range(0f, 1f)]
		private float activationVotePercentageDefaultQueue = 0.42f;

		[SerializeField]
		[Range(0f, 1f)]
		private float activationVotePercentageCompetitiveQueue = 0.6f;

		[SerializeField]
		private Gradient lavaActivationGradient;

		[SerializeField]
		private AnimationCurve lavaActivationRockProgressVsPlayerCount = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private AnimationCurve lavaActivationDrainRateVsPlayerCount = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		[SerializeField]
		private float lavaActivationVisualMovementProgressPerSecond = 1f;

		[SerializeField]
		private bool debugLavaActivationVotes;

		[Header("Scene References")]
		[SerializeField]
		private Transform lavaMeshTransform;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaMeshTransform_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private Transform lavaSurfacePlaneTransform;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaSurfacePlaneTransform_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private WaterVolume lavaVolume;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaVolume_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private MeshRenderer lavaActivationRenderer;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaActivationRenderer_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private Transform lavaActivationStartPos;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaActivationStartPos_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private Transform lavaActivationEndPos;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaActivationEndPos_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private SlingshotProjectileHitNotifier lavaActivationProjectileHitNotifier;

		[SerializeField]
		private GuidedRefReceiverFieldInfo lavaActivationProjectileHitNotifier_gRef = new GuidedRefReceiverFieldInfo(true);

		[SerializeField]
		private VolcanoEffects[] volcanoEffects;

		[SerializeField]
		private GuidedRefReceiverArrayInfo volcanoEffects_gRefs = new GuidedRefReceiverArrayInfo(true);

		[DebugReadout]
		private InfectionLavaController.LavaSyncData reliableState;

		private int[] lavaActivationVotePlayerIds = new int[10];

		private int lavaActivationVoteCount;

		private float localLagLavaProgressOffset;

		[DebugReadout]
		private float lavaProgressLinear;

		[DebugReadout]
		private float lavaProgressSmooth;

		private double lastTagSelfRPCTime;

		private const string lavaRockProjectileTag = "LavaRockProjectile";

		private double currentTime;

		private double prevTime;

		private float activationProgessSmooth;

		private float lavaScale;

		private GorillaSerializerScene networkObject;

		private bool guidedRefsFullyResolved;

		public enum RisingLavaState
		{
			Drained,
			Erupting,
			Rising,
			Full,
			Draining
		}

		private struct LavaSyncData
		{
			public InfectionLavaController.RisingLavaState state;

			public double stateStartTime;

			public double activationProgress;
		}
	}
}
