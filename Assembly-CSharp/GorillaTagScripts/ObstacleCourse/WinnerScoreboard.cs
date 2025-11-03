using System;
using TMPro;
using UnityEngine;

namespace GorillaTagScripts.ObstacleCourse
{
	public class WinnerScoreboard : MonoBehaviour
	{
		public void UpdateBoard(string winner, ObstacleCourse.RaceState _currentState)
		{
			if (this.output == null)
			{
				return;
			}
			switch (_currentState)
			{
			case ObstacleCourse.RaceState.Started:
				Debug.Log(this.raceStarted);
				this.output.text = this.raceStarted;
				return;
			case ObstacleCourse.RaceState.Waiting:
				Debug.Log(this.raceLoading);
				this.output.text = this.raceLoading;
				return;
			case ObstacleCourse.RaceState.Finished:
				Debug.Log(winner + " WON!!");
				this.output.text = winner + " WON!!";
				return;
			default:
				return;
			}
		}

		public string raceStarted = "RACE STARTED!";

		public string raceLoading = "RACE LOADING...";

		[SerializeField]
		private TextMeshPro output;
	}
}
