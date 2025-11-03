using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class GRDropZone : MonoBehaviour
{
	private void Awake()
	{
		this.repelDirectionWorld = base.transform.TransformDirection(this.repelDirectionLocal.normalized);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!PhotonNetwork.IsMasterClient)
		{
			return;
		}
		GameEntity component = other.attachedRigidbody.GetComponent<GameEntity>();
		if (component != null && component.manager.ghostReactorManager != null)
		{
			GhostReactorManager.Get(component).EntityEnteredDropZone(component);
		}
	}

	public Vector3 GetRepelDirectionWorld()
	{
		return this.repelDirectionWorld;
	}

	public void PlayEffect()
	{
		if (this.vfxRoot != null && !this.playingEffect)
		{
			this.vfxRoot.SetActive(true);
			this.playingEffect = true;
			if (this.sfxPrefab != null)
			{
				ObjectPools.instance.Instantiate(this.sfxPrefab, base.transform.position, base.transform.rotation, true);
			}
			base.StartCoroutine(this.DelayedStopEffect());
		}
	}

	private IEnumerator DelayedStopEffect()
	{
		yield return new WaitForSeconds(this.effectDuration);
		this.vfxRoot.SetActive(false);
		this.playingEffect = false;
		yield break;
	}

	[SerializeField]
	private GameObject vfxRoot;

	[SerializeField]
	private GameObject sfxPrefab;

	public float effectDuration = 1f;

	private bool playingEffect;

	[SerializeField]
	private Vector3 repelDirectionLocal = Vector3.up;

	private Vector3 repelDirectionWorld = Vector3.up;
}
