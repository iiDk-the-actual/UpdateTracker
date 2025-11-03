using System;
using UnityEngine;

public class GTAnimToStaticSwapper : MonoBehaviour, IGorillaSliceableSimple
{
	protected void Awake()
	{
		if (this.m_animationComponent == null)
		{
			base.enabled = false;
			return;
		}
		if (this.m_animatedGameObjects == null)
		{
			this.m_animatedGameObjects = Array.Empty<GameObject>();
		}
		int num = 0;
		for (int i = 0; i < this.m_animatedGameObjects.Length; i++)
		{
			if (!(this.m_animatedGameObjects[i] == null))
			{
				this.m_animatedGameObjects[num] = this.m_animatedGameObjects[i];
				num++;
			}
		}
		if (num != this.m_animatedGameObjects.Length)
		{
			Array.Resize<GameObject>(ref this.m_animatedGameObjects, num);
		}
		this.SetGameObjectsActive(this.m_animatedGameObjects, false);
		this.SetGameObjectsActive(this.m_defaultStaticGameObjects, false);
		for (int j = 0; j < this.m_animsToGameObjectsSwapInfo.Length; j++)
		{
			this.SetGameObjectsActive(this.m_animsToGameObjectsSwapInfo[j].staticGameObjects, false);
		}
	}

	public void OnEnable()
	{
		((IGorillaSliceableSimple)this).SliceUpdate();
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.Update);
	}

	void IGorillaSliceableSimple.SliceUpdate()
	{
		int num;
		if (this.m_animationComponent.isPlaying)
		{
			num = -2;
		}
		else
		{
			AnimationClip clip = this.m_animationComponent.clip;
			num = -1;
			if (clip != null)
			{
				for (int i = 0; i < this.m_animsToGameObjectsSwapInfo.Length; i++)
				{
					if (this.m_animsToGameObjectsSwapInfo[i].animClip == clip)
					{
						num = i;
						break;
					}
				}
			}
		}
		if (this._swapIndexOrState == num)
		{
			return;
		}
		int swapIndexOrState = this._swapIndexOrState;
		if (swapIndexOrState != -2)
		{
			if (swapIndexOrState != -1)
			{
				if (this._swapIndexOrState >= 0 && this._swapIndexOrState < this.m_animsToGameObjectsSwapInfo.Length)
				{
					this.SetGameObjectsActive(this.m_animsToGameObjectsSwapInfo[this._swapIndexOrState].staticGameObjects, false);
				}
			}
			else
			{
				this.SetGameObjectsActive(this.m_defaultStaticGameObjects, false);
			}
		}
		else
		{
			this.SetGameObjectsActive(this.m_animatedGameObjects, false);
		}
		if (num != -2)
		{
			if (num != -1)
			{
				if (num >= 0 && num < this.m_animsToGameObjectsSwapInfo.Length)
				{
					this.SetGameObjectsActive(this.m_animsToGameObjectsSwapInfo[num].staticGameObjects, true);
				}
			}
			else
			{
				this.SetGameObjectsActive(this.m_defaultStaticGameObjects, true);
			}
		}
		else
		{
			this.SetGameObjectsActive(this.m_animatedGameObjects, true);
		}
		this._swapIndexOrState = num;
	}

	private void SetGameObjectsActive(GameObject[] gameObjects, bool isActive)
	{
		if (gameObjects == null)
		{
			return;
		}
		foreach (GameObject gameObject in gameObjects)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(isActive);
			}
		}
	}

	private const string preLog = "[GT/GTAnimToStaticSwapper]  ";

	private const string preErr = "[GT/GTAnimToStaticSwapper]  ERROR!!!  ";

	private const string preErrBeta = "[GT/GTAnimToStaticSwapper]  ERROR!!!  (beta only log)  ";

	[SerializeField]
	private Animation m_animationComponent;

	[Tooltip("these will be active when the animation is playing and deactivated when the animation completes.")]
	[SerializeField]
	private GameObject[] m_animatedGameObjects;

	[Tooltip("When playing stops, these GameObjects will be activated by default if a matching clip cannot be found when playing stops.")]
	[SerializeField]
	private GameObject[] m_defaultStaticGameObjects;

	[SerializeField]
	private GTAnimToStaticSwapper.AnimToStaticMeshSwapInfo[] m_animsToGameObjectsSwapInfo;

	private int lastStoppedClipId;

	private int _swapIndexOrState = int.MinValue;

	private const int _k_defaultStaticGameObjs = -1;

	private const int _k_state_animating = -2;

	[Serializable]
	public struct AnimToStaticMeshSwapInfo
	{
		public AnimationClip animClip;

		[Tooltip("These will be inactive while animation is playing and active when it completes.")]
		public GameObject[] staticGameObjects;
	}
}
