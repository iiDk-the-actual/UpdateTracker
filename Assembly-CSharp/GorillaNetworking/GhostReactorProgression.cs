using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace GorillaNetworking
{
	public class GhostReactorProgression : MonoBehaviour
	{
		public void Awake()
		{
			GhostReactorProgression.instance = this;
		}

		public void Start()
		{
			if (ProgressionManager.Instance != null)
			{
				ProgressionManager.Instance.OnTrackRead += this.OnTrackRead;
				ProgressionManager.Instance.OnTrackSet += this.OnTrackSet;
				ProgressionManager.Instance.OnNodeUnlocked += delegate(string a, string b)
				{
					this.OnNodeUnlocked();
				};
				return;
			}
			Debug.Log("GRP: ProgressionManager is null!");
		}

		public async void GetStartingProgression(GRPlayer grPlayer)
		{
			await ProgressionUtil.WaitForMothershipSessionToken();
			this._grPlayer = grPlayer;
			ProgressionManager.Instance.GetProgression(this.progressionTrackId);
			if (this._grPlayer.gamePlayer.IsLocal())
			{
				this._grPlayer.mothershipId = MothershipClientContext.MothershipId;
				ProgressionManager.Instance.GetShiftCredit(this._grPlayer.mothershipId);
			}
		}

		public void SetProgression(int progressionAmountToAdd, GRPlayer grPlayer)
		{
			this._grPlayer = grPlayer;
			ProgressionManager.Instance.SetProgression(this.progressionTrackId, progressionAmountToAdd);
		}

		public void UnlockProgressionTreeNode(string treeId, string nodeId, GhostReactor reactor)
		{
			this._reactor = reactor;
			ProgressionManager.Instance.UnlockNode(treeId, nodeId);
		}

		private void OnTrackRead(string trackId, int progress)
		{
			if (this._grPlayer == null)
			{
				Debug.Log("GRP: OnTrackRead Failure: player is null");
				return;
			}
			if (trackId != this.progressionTrackId)
			{
				Debug.Log(string.Format("GRP: OnTrackRead Failure: track [{0}] progressionTrack [{1}] progress {2}", trackId, this.progressionTrackId, progress));
				return;
			}
			this._grPlayer.SetProgressionData(progress, progress, false);
		}

		private void OnTrackSet(string trackId, int progress)
		{
			if (this._grPlayer == null)
			{
				return;
			}
			if (trackId != this.progressionTrackId)
			{
				return;
			}
			this._grPlayer.SetProgressionData(progress, this._grPlayer.CurrentProgression.redeemedPoints, false);
		}

		private void OnNodeUnlocked()
		{
			if (this._reactor != null && this._reactor.toolProgression != null)
			{
				this._reactor.toolProgression.UpdateInventory();
				this._reactor.toolProgression.SetPendingTreeToProcess();
				this._reactor.UpdateLocalPlayerFromProgression();
			}
		}

		[return: TupleElementNames(new string[] { "tier", "grade", "totalPointsToNextLevel", "partialPointsToNextLevel" })]
		public static ValueTuple<int, int, int, int> GetGradePointDetails(int points)
		{
			GhostReactorProgression.LoadGRPSO();
			int num = 0;
			int num2 = 0;
			int i;
			for (i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				num2 = num;
				num += GhostReactorProgression.grPSO.progressionData[i].grades * GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
				if (points < num)
				{
					break;
				}
			}
			if (points > num)
			{
				return new ValueTuple<int, int, int, int>(i - 1, 0, 0, 0);
			}
			int pointsPerGrade = GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
			int num3 = (points - num2) / pointsPerGrade;
			int num4 = (points - num2) % pointsPerGrade;
			return new ValueTuple<int, int, int, int>(i, num3, pointsPerGrade, num4);
		}

		public static string GetTitleNameAndGrade(int points)
		{
			GhostReactorProgression.LoadGRPSO();
			int num = 0;
			for (int i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				num += GhostReactorProgression.grPSO.progressionData[i].grades * GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
				if (points < num)
				{
					return GhostReactorProgression.grPSO.progressionData[i].tierName + " " + (GhostReactorProgression.grPSO.progressionData[i].grades - Mathf.FloorToInt((float)((num - points) / GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade)) + 1).ToString();
				}
			}
			return "null";
		}

		public static string GetTitleName(int points)
		{
			GhostReactorProgression.LoadGRPSO();
			int num = 0;
			for (int i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				num += GhostReactorProgression.grPSO.progressionData[i].grades * GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
				if (points < num)
				{
					return GhostReactorProgression.grPSO.progressionData[i].tierName;
				}
			}
			return "null";
		}

		public static string GetTitleNameFromLevel(int level)
		{
			GhostReactorProgression.LoadGRPSO();
			for (int i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				if (GhostReactorProgression.grPSO.progressionData[i].tierId >= level)
				{
					return GhostReactorProgression.grPSO.progressionData[i].tierName;
				}
			}
			return "null";
		}

		public static int GetGrade(int points)
		{
			GhostReactorProgression.LoadGRPSO();
			int num = 0;
			for (int i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				num += GhostReactorProgression.grPSO.progressionData[i].grades * GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
				if (points < num)
				{
					return GhostReactorProgression.grPSO.progressionData[i].grades - Mathf.FloorToInt((float)((num - points) / GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade)) + 1;
				}
			}
			return -1;
		}

		public static int GetTitleLevel(int points)
		{
			GhostReactorProgression.LoadGRPSO();
			int num = 0;
			for (int i = 0; i < GhostReactorProgression.grPSO.progressionData.Count; i++)
			{
				num += GhostReactorProgression.grPSO.progressionData[i].grades * GhostReactorProgression.grPSO.progressionData[i].pointsPerGrade;
				if (points < num)
				{
					return GhostReactorProgression.grPSO.progressionData[i].tierId;
				}
			}
			return -1;
		}

		public static void LoadGRPSO()
		{
			if (GhostReactorProgression.grPSO == null)
			{
				GhostReactorProgression.grPSO = Resources.Load<GRProgressionScriptableObject>("ProgressionTiersData");
			}
		}

		public static GhostReactorProgression instance;

		private string progressionTrackId = "a0208736-e696-489b-81cd-c0c772489cc5";

		private GRPlayer _grPlayer;

		private GhostReactor _reactor;

		public static GRProgressionScriptableObject grPSO;

		public const string grPSODirectory = "ProgressionTiersData";
	}
}
