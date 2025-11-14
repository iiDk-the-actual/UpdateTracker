using System;
using GorillaTagScripts.GhostReactor;
using TMPro;
using UnityEngine;

public class GRRecyclerScanner : MonoBehaviour
{
	private void Awake()
	{
		this.titleText.text = "";
		this.descriptionText.text = "";
		this.annotationText.text = "";
		this.recycleValueText.text = "";
	}

	public void ScanItem(GameEntityId id)
	{
		if (this.recycler != null && this.recycler.reactor != null && this.recycler.reactor.grManager != null && this.recycler.reactor.grManager.gameEntityManager != null)
		{
			GameEntity gameEntity = this.recycler.reactor.grManager.gameEntityManager.GetGameEntity(id);
			if (gameEntity == null)
			{
				return;
			}
			GRScannable component = gameEntity.GetComponent<GRScannable>();
			if (component == null)
			{
				return;
			}
			this.titleText.text = component.GetTitleText(this.recycler.reactor);
			this.descriptionText.text = component.GetBodyText(this.recycler.reactor);
			this.annotationText.text = component.GetAnnotationText(this.recycler.reactor);
			this.recycleValueText.text = string.Format("Recycle value: {0}", this.recycler.GetRecycleValue(gameEntity.gameObject.GetToolType()));
			this.audioSource.volume = this.recyclerBarcodeAudioVolume;
			this.audioSource.PlayOneShot(this.recyclerBarcodeAudio);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (this.recycler.reactor == null)
		{
			return;
		}
		if (!this.recycler.reactor.grManager.IsAuthority())
		{
			return;
		}
		GRScannable componentInParent = other.gameObject.GetComponentInParent<GRScannable>();
		if (componentInParent == null)
		{
			return;
		}
		this.recycler.reactor.grManager.RequestRecycleScanItem(componentInParent.gameEntity.id);
	}

	public GRRecycler recycler;

	[SerializeField]
	private TextMeshPro titleText;

	[SerializeField]
	private TextMeshPro descriptionText;

	[SerializeField]
	private TextMeshPro annotationText;

	[SerializeField]
	private TextMeshPro recycleValueText;

	public AudioSource audioSource;

	public AudioClip recyclerBarcodeAudio;

	public float recyclerBarcodeAudioVolume = 0.5f;
}
