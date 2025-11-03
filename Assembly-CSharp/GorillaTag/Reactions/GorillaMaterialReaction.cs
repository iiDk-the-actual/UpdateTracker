using System;
using System.Collections.Generic;
using System.Text;
using GorillaExtensions;
using GorillaNetworking;
using UnityEngine;

namespace GorillaTag.Reactions
{
	public class GorillaMaterialReaction : MonoBehaviour, ITickSystemPost
	{
		public void PopulateRuntimeLookupArrays()
		{
			this._momentEnumCount = ((GorillaMaterialReaction.EMomentInState[])Enum.GetValues(typeof(GorillaMaterialReaction.EMomentInState))).Length;
			this._matCount = this._ownerVRRig.materialsToChangeTo.Length;
			this._mat_x_moment_x_activeBool_to_gObjs = new GameObject[this._momentEnumCount * this._matCount * 2][];
			for (int i = 0; i < this._matCount; i++)
			{
				for (int j = 0; j < this._momentEnumCount; j++)
				{
					GorillaMaterialReaction.EMomentInState emomentInState = (GorillaMaterialReaction.EMomentInState)j;
					List<GameObject> list = new List<GameObject>();
					List<GameObject> list2 = new List<GameObject>();
					foreach (GorillaMaterialReaction.ReactionEntry reactionEntry in this._statusEffectReactions)
					{
						int[] statusMaterialIndexes = reactionEntry.statusMaterialIndexes;
						for (int l = 0; l < statusMaterialIndexes.Length; l++)
						{
							if (statusMaterialIndexes[l] == i)
							{
								foreach (GorillaMaterialReaction.GameObjectStates gameObjectStates2 in reactionEntry.gameObjectStates)
								{
									switch (emomentInState)
									{
									case GorillaMaterialReaction.EMomentInState.OnEnter:
										if (gameObjectStates2.onEnter.change)
										{
											if (gameObjectStates2.onEnter.activeState)
											{
												list.Add(base.gameObject);
											}
											else
											{
												list2.Add(base.gameObject);
											}
										}
										break;
									case GorillaMaterialReaction.EMomentInState.OnStay:
										if (gameObjectStates2.onStay.change)
										{
											if (gameObjectStates2.onEnter.activeState)
											{
												list.Add(base.gameObject);
											}
											else
											{
												list2.Add(base.gameObject);
											}
										}
										break;
									case GorillaMaterialReaction.EMomentInState.OnExit:
										if (gameObjectStates2.onExit.change)
										{
											if (gameObjectStates2.onEnter.activeState)
											{
												list.Add(base.gameObject);
											}
											else
											{
												list2.Add(base.gameObject);
											}
										}
										break;
									default:
										Debug.LogError(string.Format("Unhandled enum value for {0}: {1}", "EMomentInState", emomentInState));
										break;
									}
								}
							}
						}
					}
					int num = i * this._momentEnumCount * 2 + j * 2;
					this._mat_x_moment_x_activeBool_to_gObjs[num] = list2.ToArray();
					this._mat_x_moment_x_activeBool_to_gObjs[num + 1] = list.ToArray();
				}
			}
		}

		protected void Awake()
		{
			this.RemoveAndReportNulls();
			this.PopulateRuntimeLookupArrays();
		}

		protected void OnEnable()
		{
			if (this._ownerVRRig == null)
			{
				this._ownerVRRig = base.GetComponentInParent<VRRig>(true);
			}
			if (this._ownerVRRig == null)
			{
				Debug.LogError("GorillaMaterialReaction: Disabling because could not find VRRig! Hierarchy path: " + base.transform.GetPath(), this);
				base.enabled = false;
				return;
			}
			this._reactionsRemaining = 0;
			for (int i = 0; i < this._statusEffectReactions.Length; i++)
			{
				this._reactionsRemaining += this._statusEffectReactions[i].gameObjectStates.Length;
			}
			this._currentMatIndexStartTime = 0.0;
			TickSystem<object>.AddCallbackTarget(this);
		}

		protected void OnDisable()
		{
			TickSystem<object>.RemoveCallbackTarget(this);
		}

		bool ITickSystemPost.PostTickRunning { get; set; }

		void ITickSystemPost.PostTick()
		{
			if (!GorillaComputer.hasInstance || this._ownerVRRig == null)
			{
				return;
			}
			GorillaComputer instance = GorillaComputer.instance;
			int num = ((GorillaGameManager.instance == null) ? 0 : GorillaGameManager.instance.MyMatIndex(this._ownerVRRig.creator));
			if (this._previousMatIndex == num && this._reactionsRemaining <= 0)
			{
				return;
			}
			double num2 = (double)instance.startupMillis / 1000.0 + Time.realtimeSinceStartupAsDouble;
			bool flag = false;
			if (this._currentMomentInState == GorillaMaterialReaction.EMomentInState.OnExit && this._previousMatIndex != num)
			{
				this._currentMomentInState = GorillaMaterialReaction.EMomentInState.OnEnter;
				flag = true;
				this._currentMatIndexStartTime = num2;
				this._currentMomentDuration = -1.0;
				GorillaGameManager instance2 = GorillaGameManager.instance;
				if (instance2 != null)
				{
					GorillaTagManager gorillaTagManager = instance2 as GorillaTagManager;
					if (gorillaTagManager != null)
					{
						this._currentMomentDuration = (double)gorillaTagManager.tagCoolDown;
					}
				}
			}
			else if (this._currentMomentInState == GorillaMaterialReaction.EMomentInState.OnEnter && this._previousMatIndex == num && (this._currentMomentDuration < 0.0 || this._currentMomentDuration < num2 - this._currentMatIndexStartTime))
			{
				this._currentMomentInState = GorillaMaterialReaction.EMomentInState.OnStay;
				flag = true;
				this._currentMomentDuration = -1.0;
			}
			else if (this._currentMomentInState == GorillaMaterialReaction.EMomentInState.OnStay && this._previousMatIndex != num)
			{
				this._currentMomentInState = GorillaMaterialReaction.EMomentInState.OnExit;
				flag = true;
				this._currentMomentDuration = -1.0;
			}
			this._previousMatIndex = num;
			if (!flag)
			{
				return;
			}
			for (int i = 0; i < 2; i++)
			{
				GameObject[] array = this._mat_x_moment_x_activeBool_to_gObjs[(int)(num * this._momentEnumCount * 2 + this._currentMomentInState * GorillaMaterialReaction.EMomentInState.OnExit + i)];
				for (int j = 0; j < array.Length; j++)
				{
					array[j].SetActive(i == 1);
				}
			}
		}

		private void RemoveAndReportNulls()
		{
			StringBuilder stringBuilder = new StringBuilder(1024);
			if (this._statusEffectReactions == null)
			{
				Debug.Log(string.Format("{0}: The array `{1}` is null. ", "GorillaMaterialReaction", this._statusEffectReactions) + "(this should never happen)");
				this._statusEffectReactions = Array.Empty<GorillaMaterialReaction.ReactionEntry>();
			}
			for (int i = 0; i < this._statusEffectReactions.Length; i++)
			{
				GorillaMaterialReaction.GameObjectStates[] gameObjectStates = this._statusEffectReactions[i].gameObjectStates;
				if (gameObjectStates == null)
				{
					this._statusEffectReactions[i].gameObjectStates = Array.Empty<GorillaMaterialReaction.GameObjectStates>();
				}
				else
				{
					int num = 0;
					int[] array = new int[gameObjectStates.Length];
					for (int j = 0; j < gameObjectStates.Length; j++)
					{
						if (gameObjectStates[j].gameObject == null)
						{
							array[num] = j;
							num++;
						}
						else
						{
							array[num] = -1;
						}
					}
					if (num == 0)
					{
						return;
					}
					stringBuilder.Clear();
					stringBuilder.Append("GorillaMaterialReaction");
					stringBuilder.Append(": Removed null references in array `");
					stringBuilder.Append("_statusEffectReactions");
					stringBuilder.Append("[").Append(i).Append("].")
						.Append("gameObjectStates");
					stringBuilder.Append("' at indexes: ");
					stringBuilder.AppendJoin<int>(", ", array);
					stringBuilder.Append(".");
					Debug.LogError(stringBuilder.ToString(), this);
					GorillaMaterialReaction.GameObjectStates[] array2 = new GorillaMaterialReaction.GameObjectStates[gameObjectStates.Length - num];
					int num2 = 0;
					for (int k = 0; k < gameObjectStates.Length; k++)
					{
						if (!(gameObjectStates[k].gameObject == null))
						{
							array2[num2++] = gameObjectStates[k];
						}
					}
					this._statusEffectReactions[i].gameObjectStates = array2;
				}
			}
		}

		[SerializeField]
		private GorillaMaterialReaction.ReactionEntry[] _statusEffectReactions;

		private int _previousMatIndex;

		private GorillaMaterialReaction.EMomentInState _currentMomentInState;

		private double _currentMatIndexStartTime;

		private double _currentMomentDuration;

		private int _reactionsRemaining;

		private int _momentEnumCount;

		private int _matCount;

		private GameObject[][] _mat_x_moment_x_activeBool_to_gObjs;

		private VRRig _ownerVRRig;

		[Serializable]
		public struct ReactionEntry
		{
			[Tooltip("If any of these statuses are true then this reaction will be executed.")]
			public int[] statusMaterialIndexes;

			public GorillaMaterialReaction.GameObjectStates[] gameObjectStates;
		}

		[Serializable]
		public struct GameObjectStates
		{
			public GameObject gameObject;

			[GorillaMaterialReaction.MomentInStateAttribute]
			public GorillaMaterialReaction.MomentInStateActiveOption onEnter;

			[GorillaMaterialReaction.MomentInStateAttribute]
			public GorillaMaterialReaction.MomentInStateActiveOption onStay;

			[GorillaMaterialReaction.MomentInStateAttribute]
			public GorillaMaterialReaction.MomentInStateActiveOption onExit;
		}

		[Serializable]
		public struct MomentInStateActiveOption
		{
			public bool change;

			public bool activeState;
		}

		public enum EMomentInState
		{
			OnEnter,
			OnStay,
			OnExit
		}

		public class MomentInStateAttribute : Attribute
		{
		}
	}
}
