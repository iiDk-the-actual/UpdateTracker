using System;
using System.Collections.Generic;
using Photon.Voice.Unity;
using UnityEngine;

namespace GorillaTag.Audio
{
	public class LoudSpeakerNetwork : MonoBehaviour
	{
		public AudioSource[] SpeakerSources
		{
			get
			{
				return this._speakerSources;
			}
		}

		private void Awake()
		{
			if (this._speakerSources == null || this._speakerSources.Length == 0)
			{
				this._speakerSources = base.transform.GetComponentsInChildren<AudioSource>();
			}
			this._currentSpeakers = new List<Speaker>();
		}

		private void Start()
		{
			RigContainer rigContainer;
			if (this.GetParentRigContainer(out rigContainer) && rigContainer.Voice != null)
			{
				GTSpeaker gtspeaker = (GTSpeaker)rigContainer.Voice.SpeakerInUse;
				if (gtspeaker != null)
				{
					gtspeaker.AddExternalAudioSources(this._speakerSources);
				}
			}
		}

		private bool GetParentRigContainer(out RigContainer rigContainer)
		{
			if (this._rigContainer == null)
			{
				this._rigContainer = base.transform.GetComponentInParent<RigContainer>();
			}
			rigContainer = this._rigContainer;
			return rigContainer != null;
		}

		private void OnEnable()
		{
			RigContainer rigContainer;
			if (this.GetParentRigContainer(out rigContainer))
			{
				rigContainer.AddLoudSpeakerNetwork(this);
			}
		}

		private void OnDisable()
		{
			RigContainer rigContainer;
			if (this.GetParentRigContainer(out rigContainer))
			{
				rigContainer.RemoveLoudSpeakerNetwork(this);
			}
		}

		public void AddSpeaker(Speaker speaker)
		{
			if (this._currentSpeakers.Contains(speaker))
			{
				return;
			}
			this._currentSpeakers.Add(speaker);
		}

		public void RemoveSpeaker(Speaker speaker)
		{
			this._currentSpeakers.Remove(speaker);
		}

		public void StartBroadcastSpeakerOutput(VRRig player)
		{
			GorillaTagger.Instance.rigSerializer.BroadcastLoudSpeakerNetwork(true, player.OwningNetPlayer.ActorNumber);
		}

		public void BroadcastLoudSpeakerNetwork(int actorNumber, bool isLocal = false)
		{
			if (isLocal)
			{
				if (this._localRecorder == null)
				{
					this._localRecorder = (GTRecorder)NetworkSystem.Instance.LocalRecorder;
				}
				if (this._localRecorder != null)
				{
					this._localRecorder.DebugEchoMode = true;
					if (this.ReparentLocalSpeaker)
					{
						Transform transform = this._rigContainer.Voice.SpeakerInUse.transform;
						transform.transform.SetParent(base.transform, false);
						transform.localPosition = Vector3.zero;
					}
				}
				return;
			}
			using (List<Speaker>.Enumerator enumerator = this._currentSpeakers.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					GTSpeaker gtspeaker = (GTSpeaker)enumerator.Current;
					gtspeaker.ToggleAudioSource(true);
					gtspeaker.BroadcastExternal = true;
					RigContainer rigContainer;
					if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(actorNumber), out rigContainer))
					{
						Transform transform2 = rigContainer.Voice.SpeakerInUse.transform;
						transform2.SetParent(base.transform, false);
						transform2.localPosition = Vector3.zero;
					}
				}
			}
			this._currentSpeakerActor = actorNumber;
		}

		public void StopBroadcastSpeakerOutput(VRRig player)
		{
			GorillaTagger.Instance.rigSerializer.BroadcastLoudSpeakerNetwork(false, player.OwningNetPlayer.ActorNumber);
		}

		public void StopBroadcastLoudSpeakerNetwork(int actorNumber, bool isLocal = false)
		{
			if (isLocal)
			{
				if (this._localRecorder == null)
				{
					this._localRecorder = (GTRecorder)NetworkSystem.Instance.LocalRecorder;
				}
				if (this._localRecorder != null)
				{
					this._localRecorder.DebugEchoMode = false;
					RigContainer rigContainer;
					if (this.ReparentLocalSpeaker && this.GetParentRigContainer(out rigContainer))
					{
						Transform transform = rigContainer.Voice.SpeakerInUse.transform;
						transform.SetParent(rigContainer.SpeakerHead, false);
						transform.localPosition = Vector3.zero;
					}
				}
				return;
			}
			if (actorNumber == this._currentSpeakerActor)
			{
				using (List<Speaker>.Enumerator enumerator = this._currentSpeakers.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						GTSpeaker gtspeaker = (GTSpeaker)enumerator.Current;
						gtspeaker.ToggleAudioSource(false);
						gtspeaker.BroadcastExternal = false;
						RigContainer rigContainer2;
						if (VRRigCache.Instance.TryGetVrrig(NetworkSystem.Instance.GetPlayer(actorNumber), out rigContainer2))
						{
							Transform transform2 = rigContainer2.Voice.SpeakerInUse.transform;
							transform2.SetParent(rigContainer2.SpeakerHead, false);
							transform2.localPosition = Vector3.zero;
						}
					}
				}
				this._currentSpeakerActor = -1;
			}
		}

		[SerializeField]
		private AudioSource[] _speakerSources;

		[SerializeField]
		private List<Speaker> _currentSpeakers;

		[SerializeField]
		private int _currentSpeakerActor = -1;

		public bool ReparentLocalSpeaker = true;

		private RigContainer _rigContainer;

		private GTRecorder _localRecorder;
	}
}
