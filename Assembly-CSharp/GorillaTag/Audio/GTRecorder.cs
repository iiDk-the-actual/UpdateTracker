using System;
using System.Collections;
using Photon.Voice.Unity;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class GTRecorder : Recorder, ITickSystemPost
	{
		public bool PostTickRunning { get; set; }

		private void OnEnable()
		{
			TickSystem<object>.AddPostTickCallback(this);
		}

		private void OnDisable()
		{
			TickSystem<object>.RemovePostTickCallback(this);
		}

		protected override MicWrapper CreateMicWrapper(string micDev, int samplingRateInt, VoiceLogger logger)
		{
			this._micWrapper = new GTMicWrapper(micDev, samplingRateInt, this.AllowPitchAdjustment, this.PitchAdjustment, this.AllowVolumeAdjustment, this.VolumeAdjustment, logger);
			return this._micWrapper;
		}

		private IEnumerator DoTestEcho()
		{
			base.DebugEchoMode = true;
			yield return new WaitForSeconds(this.DebugEchoLength);
			base.DebugEchoMode = false;
			yield return null;
			this._testEchoCoroutine = null;
			yield break;
		}

		public void PostTick()
		{
			if (this._micWrapper != null)
			{
				this._micWrapper.UpdateWrapper(this.AllowPitchAdjustment, this.PitchAdjustment, this.AllowVolumeAdjustment, this.VolumeAdjustment);
			}
		}

		public bool AllowPitchAdjustment;

		public float PitchAdjustment = 1f;

		public bool AllowVolumeAdjustment;

		public float VolumeAdjustment = 1f;

		public float DebugEchoLength = 5f;

		private GTMicWrapper _micWrapper;

		private Coroutine _testEchoCoroutine;
	}
}
