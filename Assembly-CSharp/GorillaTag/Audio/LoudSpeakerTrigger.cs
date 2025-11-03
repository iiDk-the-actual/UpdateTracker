using System;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class LoudSpeakerTrigger : MonoBehaviour
	{
		public void SetRecorder(GTRecorder recorder)
		{
			this._recorder = recorder;
		}

		public void OnPlayerEnter(VRRig player)
		{
			if (this._recorder != null && this._network != null)
			{
				this._recorder.AllowPitchAdjustment = true;
				this._recorder.PitchAdjustment = this.PitchAdjustment;
				this._network.StartBroadcastSpeakerOutput(player);
			}
		}

		public void OnPlayerExit(VRRig player)
		{
			if (this._recorder != null && this._network != null)
			{
				this._recorder.AllowPitchAdjustment = false;
				this._recorder.PitchAdjustment = 1f;
				this._network.StopBroadcastSpeakerOutput(player);
			}
		}

		public float PitchAdjustment = 1f;

		[SerializeField]
		private LoudSpeakerNetwork _network;

		[SerializeField]
		private GTRecorder _recorder;
	}
}
