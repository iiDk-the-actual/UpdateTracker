using System;
using GorillaGameModes;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTagScripts
{
	public sealed class GorillaAmbushManager : GorillaTagManager
	{
		public override GameModeType GameType()
		{
			if (!this.isGhostTag)
			{
				return GameModeType.Ambush;
			}
			return GameModeType.Ghost;
		}

		public static int HandEffectHash
		{
			get
			{
				return GorillaAmbushManager.handTapHash;
			}
		}

		public static float HandFXScaleModifier { get; private set; }

		public bool isGhostTag { get; private set; }

		public override void Awake()
		{
			base.Awake();
			if (this.handTapFX != null)
			{
				GorillaAmbushManager.handTapHash = PoolUtils.GameObjHashCode(this.handTapFX);
			}
			GorillaAmbushManager.HandFXScaleModifier = this.handTapScaleFactor;
		}

		private void Start()
		{
			this.hasScryingPlane = this.scryingPlaneRef.TryResolve<MeshRenderer>(out this.scryingPlane);
			this.hasScryingPlane3p = this.scryingPlane3pRef.TryResolve<MeshRenderer>(out this.scryingPlane3p);
		}

		public override string GameModeName()
		{
			if (!this.isGhostTag)
			{
				return "AMBUSH";
			}
			return "GHOST";
		}

		public override string GameModeNameRoomLabel()
		{
			string text = (this.isGhostTag ? "GAME_MODE_GHOST_ROOM_LABEL" : "GAME_MODE_AMBUSH_ROOM_LABEL");
			string text2 = (this.isGhostTag ? "(GHOST GAME)" : "(AMBUSH GAME)");
			string text3;
			if (!LocalisationManager.TryGetKeyForCurrentLocale(text, out text3, text2))
			{
				Debug.LogError("[LOCALIZATION::GORILLA_GAME_MANAGER] Failed to get key for Game Mode [" + text + "]");
			}
			return text3;
		}

		public override void UpdatePlayerAppearance(VRRig rig)
		{
			int num = this.MyMatIndex(rig.creator);
			rig.ChangeMaterialLocal(num);
			bool flag = base.IsInfected(rig.Creator);
			bool flag2 = base.IsInfected(NetworkSystem.Instance.LocalPlayer);
			rig.bodyRenderer.SetGameModeBodyType(flag ? GorillaBodyType.Skeleton : GorillaBodyType.Default);
			rig.SetInvisibleToLocalPlayer(flag && !flag2);
			if (this.isGhostTag && rig.isOfflineVRRig)
			{
				CosmeticsController.instance.SetHideCosmeticsFromRemotePlayers(flag);
				if (this.hasScryingPlane)
				{
					this.scryingPlane.enabled = flag2;
				}
				if (this.hasScryingPlane3p)
				{
					this.scryingPlane3p.enabled = flag2;
				}
			}
		}

		public override int MyMatIndex(NetPlayer forPlayer)
		{
			if (!base.IsInfected(forPlayer))
			{
				return 0;
			}
			return 13;
		}

		public override void StopPlaying()
		{
			base.StopPlaying();
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				GorillaSkin.ApplyToRig(vrrig, null, GorillaSkin.SkinType.gameMode);
				vrrig.bodyRenderer.SetGameModeBodyType(GorillaBodyType.Default);
				vrrig.SetInvisibleToLocalPlayer(false);
			}
			CosmeticsController.instance.SetHideCosmeticsFromRemotePlayers(false);
			if (this.hasScryingPlane)
			{
				this.scryingPlane.enabled = false;
			}
			if (this.hasScryingPlane3p)
			{
				this.scryingPlane3p.enabled = false;
			}
		}

		public GameObject handTapFX;

		public GorillaSkin ambushSkin;

		[SerializeField]
		private AudioClip[] firstPersonTaggedSounds;

		[SerializeField]
		private float firstPersonTaggedSoundVolume;

		private static int handTapHash = -1;

		public float handTapScaleFactor = 0.5f;

		public float crawlingSpeedForMaxVolume;

		[SerializeField]
		private XSceneRef scryingPlaneRef;

		[SerializeField]
		private XSceneRef scryingPlane3pRef;

		private const int STEALTH_MATERIAL_INDEX = 13;

		private MeshRenderer scryingPlane;

		private bool hasScryingPlane;

		private MeshRenderer scryingPlane3p;

		private bool hasScryingPlane3p;
	}
}
