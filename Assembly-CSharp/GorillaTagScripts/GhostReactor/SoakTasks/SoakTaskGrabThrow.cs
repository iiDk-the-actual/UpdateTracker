using System;
using System.Collections.Generic;
using GorillaExtensions;
using UnityEngine;

namespace GorillaTagScripts.GhostReactor.SoakTasks
{
	public sealed class SoakTaskGrabThrow : IGhostReactorSoakTask
	{
		public bool Complete { get; private set; }

		public SoakTaskGrabThrow(GRPlayer grPlayer)
		{
			this._grPlayer = grPlayer;
		}

		public bool Update()
		{
			GameEntityManager managerForZone = GameEntityManager.GetManagerForZone(this._grPlayer.gamePlayer.rig.zoneEntity.currentZone);
			if (managerForZone == null)
			{
				return false;
			}
			if (this._dropEntityTime == null || this._heldEntityId == null)
			{
				List<GameEntity> list = managerForZone.GetGameEntities().ShuffleIntoCollection<List<GameEntity>, GameEntity>();
				GameEntity gameEntity = null;
				foreach (GameEntity gameEntity2 in list)
				{
					if (!(gameEntity2 == null) && !gameEntity2.IsHeld() && gameEntity2.pickupable && !(gameEntity2.gameObject.GetComponent<GameAgent>() != null))
					{
						gameEntity = gameEntity2;
						break;
					}
				}
				if (gameEntity != null)
				{
					Debug.Log(string.Format("Soak grabbing entity {0}", gameEntity.id.index));
					managerForZone.RequestGrabEntity(gameEntity.id, true, Vector3.zero, Quaternion.identity);
					this._heldEntityId = new GameEntityId?(gameEntity.id);
					this._dropEntityTime = new float?(Time.time + 0.1f);
				}
			}
			else if (this._heldEntityId != null)
			{
				float? dropEntityTime = this._dropEntityTime;
				if (dropEntityTime != null)
				{
					float valueOrDefault = dropEntityTime.GetValueOrDefault();
					if (Time.time >= valueOrDefault)
					{
						Debug.Log(string.Format("Soak dropping entity {0}", this._heldEntityId.Value.index));
						managerForZone.RequestThrowEntity(this._heldEntityId.Value, true, Vector3.zero, Vector3.zero, Vector3.zero);
						this._heldEntityId = null;
						this._dropEntityTime = null;
						this.Complete = true;
					}
				}
			}
			return true;
		}

		public void Reset()
		{
			this._heldEntityId = null;
			this._dropEntityTime = null;
			this.Complete = false;
		}

		public const float TIME_TO_HOLD_ENTITY = 0.1f;

		private readonly GRPlayer _grPlayer;

		private GameEntityId? _heldEntityId;

		private float? _dropEntityTime;
	}
}
