using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public class KIDUI_AnimatedEllipsis : MonoBehaviour
{
	private void Awake()
	{
		if (this._ellipsisObjects != null)
		{
			return;
		}
		this.SetupEllipsis();
	}

	private void Start()
	{
	}

	private void OnDisable()
	{
		this.StopAnimation();
	}

	private void SetupEllipsis()
	{
		if (this._ellipsisRoot == null)
		{
			this._ellipsisRoot = base.gameObject;
		}
		this._ellipsisObjects = new ValueTuple<GameObject, float, float, float>[this._ellipsisStartingValues.Count];
		for (int i = 0; i < this._ellipsisStartingValues.Count; i++)
		{
			float num = this._ellipsisStartingValues[i];
			this._ellipsisObjects[i].Item1 = Object.Instantiate<GameObject>(this._ellipsisPrefab, this._ellipsisRoot.transform);
			this._ellipsisObjects[i].Item1.transform.localScale = new Vector3(num, num, num);
			this._ellipsisObjects[i].Item2 = (this._ellipsisObjects[i].Item3 = num);
		}
	}

	private IEnumerator EllipsisAnimation()
	{
		int currIndex = 0;
		while (this._runAnimation)
		{
			for (int i = 0; i < this._ellipsisObjects.Length; i++)
			{
				int num = i - currIndex;
				if (num < 0)
				{
					num = this._ellipsisStartingValues.Count + num;
				}
				float num2 = this._ellipsisStartingValues[num];
				this._ellipsisObjects[i].Item1.transform.localScale = Vector3.one * num2;
			}
			int num3 = currIndex;
			currIndex = num3 + 1;
			if (currIndex >= this._ellipsisObjects.Length)
			{
				currIndex = 0;
			}
			yield return new WaitForSeconds(this._pauseBetweenScale);
		}
		yield break;
	}

	private IEnumerator EllipsisAnimation2()
	{
		float time = 0f;
		while (this._runAnimation)
		{
			for (int i = 0; i < this._ellipsisObjects.Length; i++)
			{
				float num = this._scaleDuration / (float)(this._ellipsisObjects.Length + 1) * (float)i;
				float num2 = this.LerpLoop(this._startingScale, this._endScale, time, num, this._scaleDuration);
				this._ellipsisObjects[i].Item1.transform.localScale = new Vector3(num2, num2, num2);
			}
			time += Time.deltaTime * this._animationSpeedMultiplier;
			yield return null;
		}
		yield break;
	}

	public async Task StartAnimation()
	{
		if (this._ellipsisObjects == null)
		{
			this.SetupEllipsis();
		}
		if (this._animationCoroutine != null)
		{
			Debug.LogWarningFormat("[KID::UI::ELLIPSIS] Animation is already running.", Array.Empty<object>());
			await this.StopAnimation();
		}
		for (int i = 0; i < this._ellipsisCount; i++)
		{
			this._ellipsisObjects[i].Item1.transform.localScale = new Vector3(this._ellipsisObjects[i].Item2, this._ellipsisObjects[i].Item2, this._ellipsisObjects[i].Item2);
		}
		this._ellipsisRoot.SetActive(true);
		this._runAnimation = true;
		if (this._shouldLerp)
		{
			this._animationCoroutine = base.StartCoroutine(this.EllipsisAnimation2());
		}
		else
		{
			this._animationCoroutine = base.StartCoroutine(this.EllipsisAnimation());
		}
	}

	public async Task StopAnimation()
	{
		this._runAnimation = false;
		base.StopAllCoroutines();
		await Task.Delay(100);
		this._animationCoroutine = null;
		this._ellipsisRoot.SetActive(false);
	}

	public float LerpLoop(float start, float end, float time, float offsetTime, float duration)
	{
		float num = (offsetTime - time) % duration / duration;
		float num2 = this._ellipsisAnimationCurve.Evaluate(num);
		return Mathf.Lerp(start, end, num2);
	}

	[Header("Ellipsis Spawning")]
	[SerializeField]
	private bool _animateOnStart = true;

	[SerializeField]
	private int _ellipsisCount = 3;

	[SerializeField]
	private GameObject _ellipsisPrefab;

	[SerializeField]
	private GameObject _ellipsisRoot;

	[SerializeField]
	private List<float> _ellipsisStartingValues = new List<float>();

	[Header("Animation Settings")]
	[SerializeField]
	private bool _shouldLerp;

	[SerializeField]
	private AnimationCurve _ellipsisAnimationCurve;

	[SerializeField]
	private float _animationSpeedMultiplier = 0.25f;

	[SerializeField]
	private float _startingScale = 0.33f;

	[SerializeField]
	private float _intermediaryScale = 0.66f;

	[SerializeField]
	private float _endScale = 1f;

	[SerializeField]
	private float _scaleDuration = 0.25f;

	[SerializeField]
	private float _pauseBetweenScale = 0.25f;

	[SerializeField]
	private float _pauseBetweenCycles = 0.5f;

	private bool _runAnimation;

	private float _nextChange;

	[TupleElementNames(new string[] { "ellipsis", "startingScale", "currentScale", "lerpT" })]
	private ValueTuple<GameObject, float, float, float>[] _ellipsisObjects;

	private Coroutine _animationCoroutine;
}
