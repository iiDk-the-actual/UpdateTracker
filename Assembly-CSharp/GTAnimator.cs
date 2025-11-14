using System;
using System.Collections.Generic;
using UnityEngine;

public class GTAnimator : MonoBehaviour, IDelayedExecListener
{
	public Animation animationComponent
	{
		get
		{
			return this.m_animationComponent;
		}
	}

	public bool hasAnimationComponent { get; private set; }

	protected void Awake()
	{
		this.Init();
	}

	public void Init()
	{
		if (this._wasInitCalled)
		{
			return;
		}
		this._wasInitCalled = true;
		this.hasAnimationComponent = this.m_animationComponent != null;
		bool hasAnimationComponent = this.hasAnimationComponent;
		this.m_animationMap.Init();
		foreach (GTAnimator.AnimClipAndGObjs animClipAndGObjs in this.m_animationMap.Values)
		{
			this._allStaticGobjs.UnionWith(animClipAndGObjs.endStaticGameObjects);
		}
	}

	public void OnEnable()
	{
	}

	public bool IsPlaying
	{
		get
		{
			return this.m_animationComponent.isPlaying;
		}
	}

	public void SetState(long enumValueAsLong)
	{
		if (!this._wasInitCalled)
		{
			this.Init();
		}
		if (this._currentStateAsLong != enumValueAsLong)
		{
			this.TryPlay(enumValueAsLong);
		}
	}

	public bool TryPlay(long enumValueAsLong)
	{
		GTAnimator.AnimClipAndGObjs animClipAndGObjs;
		if (!this.hasAnimationComponent || !this.m_animationMap.TryGet(enumValueAsLong, out animClipAndGObjs))
		{
			return false;
		}
		foreach (GameObject gameObject in this._allStaticGobjs)
		{
			gameObject.SetActive(false);
		}
		GameObject[] animatedGameObjects = this.m_animatedGameObjects;
		for (int i = 0; i < animatedGameObjects.Length; i++)
		{
			animatedGameObjects[i].SetActive(true);
		}
		this._currentStateAsLong = enumValueAsLong;
		this.m_animationComponent.clip = animClipAndGObjs.animClip;
		this.m_animationComponent.Play();
		if (animClipAndGObjs.soundBankToPlayOnStart)
		{
			animClipAndGObjs.soundBankToPlayOnStart.Play();
		}
		if (!animClipAndGObjs.animClip.isLooping)
		{
			this._frameCountWhenLastPlayed = Time.frameCount;
			GTDelayedExec.Add(this, animClipAndGObjs.animClip.length, this._frameCountWhenLastPlayed);
		}
		return true;
	}

	void IDelayedExecListener.OnDelayedAction(int contextId)
	{
		if (!base.enabled || this._frameCountWhenLastPlayed != contextId)
		{
			return;
		}
		this.m_animationComponent.Stop();
		for (int i = 0; i < this.m_animatedGameObjects.Length; i++)
		{
			if (this.m_animatedGameObjects[i] != null)
			{
				this.m_animatedGameObjects[i].SetActive(false);
			}
		}
		GTAnimator.AnimClipAndGObjs animClipAndGObjs;
		GameObject[] array;
		if (this.m_animationMap.TryGet(this._currentStateAsLong, out animClipAndGObjs) && animClipAndGObjs.endStaticGameObjects != null && animClipAndGObjs.endStaticGameObjects.Length != 0)
		{
			array = animClipAndGObjs.endStaticGameObjects;
		}
		else
		{
			array = this.m_defaultStaticGameObjects;
		}
		if (array != null)
		{
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] != null)
				{
					array[j].SetActive(true);
				}
			}
		}
		if (this._queuedStateAsLong != -9223372036854775808L)
		{
			long queuedStateAsLong = this._queuedStateAsLong;
			this._queuedStateAsLong = long.MinValue;
			this.TryPlay(queuedStateAsLong);
		}
	}

	public void Stop()
	{
		if (this.m_animationComponent != null)
		{
			this.m_animationComponent.Stop();
		}
	}

	public void QueueState(long enumValueAsLong)
	{
		if (!this._wasInitCalled)
		{
			this.Init();
		}
		if (this._queuedStateAsLong == enumValueAsLong || this._currentStateAsLong == enumValueAsLong)
		{
			return;
		}
		if (!this.IsPlaying || this._IsCurrentClipLoopable())
		{
			this.TryPlay(enumValueAsLong);
			return;
		}
		this._queuedStateAsLong = enumValueAsLong;
	}

	private bool _IsCurrentClipLoopable()
	{
		if (this.m_animationComponent == null)
		{
			return false;
		}
		AnimationClip clip = this.m_animationComponent.clip;
		if (clip == null)
		{
			return false;
		}
		WrapMode wrapMode = clip.wrapMode;
		return wrapMode == WrapMode.Loop || wrapMode == WrapMode.PingPong;
	}

	private const string preLog = "[GTAnimator]  ";

	private const string preErr = "[GTAnimator]  ERROR!!!  ";

	private const string preErrBeta = "[GTAnimator]  ERROR!!!  (beta only log)  ";

	[Tooltip("Assign a unity Animation component (not to be confused with less performant Animator Component).")]
	[SerializeField]
	private Animation m_animationComponent;

	[Tooltip("These will be activated when animation starts playing and deactivated when any anim finishes playing.")]
	[SerializeField]
	private GameObject[] m_animatedGameObjects;

	[Tooltip("If an enum map value is not defined then these will be activated.")]
	[SerializeField]
	private GameObject[] m_defaultStaticGameObjects;

	[Header("Enum To Animation Mapping")]
	[Tooltip("Map an enum's values to specific AnimationClips.")]
	[SerializeField]
	internal GTEnumValueMap<GTAnimator.AnimClipAndGObjs> m_animationMap;

	private readonly HashSet<GameObject> _allStaticGobjs = new HashSet<GameObject>();

	private const long _k_invalidState = -9223372036854775808L;

	private long _currentStateAsLong = long.MinValue;

	private int _frameCountWhenLastPlayed;

	private bool _wasInitCalled;

	private long _queuedStateAsLong = long.MinValue;

	[Serializable]
	public struct AnimClipAndGObjs
	{
		public AnimationClip animClip;

		public SoundBankPlayer soundBankToPlayOnStart;

		[Tooltip("These GameObjects will be activated when the animation clip finishes playing.")]
		public GameObject[] endStaticGameObjects;
	}
}
