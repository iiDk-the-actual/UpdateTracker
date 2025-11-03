using System;
using UnityEngine;

namespace GorillaTagScripts.ObstacleCourse
{
	public class ObstacleCourse : MonoBehaviour
	{
		public int winnerActorNumber { get; private set; }

		private void Awake()
		{
			this.numPlayersOnCourse = 0;
			for (int i = 0; i < this.zoneTriggers.Length; i++)
			{
				ObstacleCourseZoneTrigger obstacleCourseZoneTrigger = this.zoneTriggers[i];
				if (!(obstacleCourseZoneTrigger == null))
				{
					obstacleCourseZoneTrigger.OnPlayerTriggerEnter += this.OnPlayerEnterZone;
					obstacleCourseZoneTrigger.OnPlayerTriggerExit += this.OnPlayerExitZone;
				}
			}
			this.TappableBell.OnTapped += this.OnEndLineTrigger;
		}

		private void OnDestroy()
		{
			for (int i = 0; i < this.zoneTriggers.Length; i++)
			{
				ObstacleCourseZoneTrigger obstacleCourseZoneTrigger = this.zoneTriggers[i];
				if (!(obstacleCourseZoneTrigger == null))
				{
					obstacleCourseZoneTrigger.OnPlayerTriggerEnter -= this.OnPlayerEnterZone;
					obstacleCourseZoneTrigger.OnPlayerTriggerExit -= this.OnPlayerExitZone;
				}
			}
			this.TappableBell.OnTapped -= this.OnEndLineTrigger;
		}

		private void Start()
		{
			this.RestartTimer(false);
		}

		public void InvokeUpdate()
		{
			if (NetworkSystem.Instance.InRoom && ObstacleCourseManager.Instance.IsMine && this.currentState == ObstacleCourse.RaceState.Finished && Time.time - this.startTime >= this.cooldownTime)
			{
				this.RestartTimer(true);
			}
		}

		public void OnPlayerEnterZone(Collider other)
		{
			if (ObstacleCourseManager.Instance.IsMine)
			{
				this.numPlayersOnCourse++;
			}
		}

		public void OnPlayerExitZone(Collider other)
		{
			if (ObstacleCourseManager.Instance.IsMine)
			{
				this.numPlayersOnCourse--;
			}
		}

		private void RestartTimer(bool playFx = true)
		{
			this.UpdateState(ObstacleCourse.RaceState.Started, playFx);
		}

		private void EndRace()
		{
			this.UpdateState(ObstacleCourse.RaceState.Finished, true);
			this.startTime = Time.time;
		}

		public void PlayWinningEffects()
		{
			if (this.confettiParticle)
			{
				this.confettiParticle.Play();
			}
			if (this.bannerRenderer)
			{
				UberShaderProperty baseColor = UberShader.BaseColor;
				Material material = this.bannerRenderer.material;
				RigContainer rigContainer = this.winnerRig;
				baseColor.SetValue<Color?>(material, (rigContainer != null) ? new Color?(rigContainer.Rig.playerColor) : null);
			}
			this.audioSource.GTPlay();
		}

		public void OnEndLineTrigger(VRRig rig)
		{
			if (ObstacleCourseManager.Instance.IsMine && this.currentState == ObstacleCourse.RaceState.Started)
			{
				this.winnerActorNumber = rig.creator.ActorNumber;
				this.winnerRig = rig.rigContainer;
				this.EndRace();
			}
		}

		public void Deserialize(int _winnerActorNumber, ObstacleCourse.RaceState _currentState)
		{
			if (!ObstacleCourseManager.Instance.IsMine)
			{
				this.winnerActorNumber = _winnerActorNumber;
				VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(this.winnerActorNumber), out this.winnerRig);
				this.UpdateState(_currentState, true);
			}
		}

		private void UpdateState(ObstacleCourse.RaceState state, bool playFX = true)
		{
			this.currentState = state;
			WinnerScoreboard winnerScoreboard = this.scoreboard;
			RigContainer rigContainer = this.winnerRig;
			winnerScoreboard.UpdateBoard((rigContainer != null) ? rigContainer.Rig.playerNameVisible : null, this.currentState);
			if (this.currentState == ObstacleCourse.RaceState.Finished)
			{
				this.PlayWinningEffects();
			}
			else if (this.currentState == ObstacleCourse.RaceState.Started && this.bannerRenderer)
			{
				UberShader.BaseColor.SetValue<Color>(this.bannerRenderer.material, Color.white);
			}
			this.UpdateStartingGate();
		}

		private void UpdateStartingGate()
		{
			if (this.currentState == ObstacleCourse.RaceState.Finished)
			{
				this.leftGate.transform.RotateAround(this.leftGate.transform.position, Vector3.up, 90f);
				this.rightGate.transform.RotateAround(this.rightGate.transform.position, Vector3.up, -90f);
				return;
			}
			if (this.currentState == ObstacleCourse.RaceState.Started)
			{
				this.leftGate.transform.RotateAround(this.leftGate.transform.position, Vector3.up, -90f);
				this.rightGate.transform.RotateAround(this.rightGate.transform.position, Vector3.up, 90f);
			}
		}

		public WinnerScoreboard scoreboard;

		private RigContainer winnerRig;

		public ObstacleCourseZoneTrigger[] zoneTriggers;

		[HideInInspector]
		public ObstacleCourse.RaceState currentState;

		[SerializeField]
		private ParticleSystem confettiParticle;

		[SerializeField]
		private Renderer bannerRenderer;

		[SerializeField]
		private TappableBell TappableBell;

		[SerializeField]
		private AudioSource audioSource;

		[SerializeField]
		private float cooldownTime = 20f;

		public GameObject leftGate;

		public GameObject rightGate;

		private int numPlayersOnCourse;

		private float startTime;

		public enum RaceState
		{
			Started,
			Waiting,
			Finished
		}
	}
}
