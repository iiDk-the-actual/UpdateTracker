using System;
using TMPro;
using UnityEngine;

public class GRRecyclerScanner : MonoBehaviour
{
	private void Awake()
	{
		this.toolText.text = "";
		this.ratesText.text = "";
	}

	public void ScanItem(GRTool.GRToolType toolType)
	{
		int num = 0;
		switch (toolType)
		{
		case GRTool.GRToolType.Club:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.Club);
			break;
		case GRTool.GRToolType.Collector:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.Collector);
			break;
		case GRTool.GRToolType.Flash:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.Flash);
			break;
		case GRTool.GRToolType.Lantern:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.Lantern);
			break;
		case GRTool.GRToolType.Revive:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.Revive);
			break;
		case GRTool.GRToolType.ShieldGun:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.ShieldGun);
			break;
		case GRTool.GRToolType.DirectionalShield:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.DirectionalShield);
			break;
		case GRTool.GRToolType.DockWrist:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.DockWrist);
			break;
		case GRTool.GRToolType.HockeyStick:
			num = this.recycler.GetRecycleValue(GRTool.GRToolType.HockeyStick);
			break;
		}
		this.toolText.text = GRUtils.GetToolName(toolType);
		this.ratesText.text = num.ToString("D2") ?? "";
		this.audioSource.volume = this.recyclerBarcodeAudioVolume;
		this.audioSource.PlayOneShot(this.recyclerBarcodeAudio);
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
		GRTool componentInParent = other.gameObject.GetComponentInParent<GRTool>();
		if (componentInParent == null)
		{
			return;
		}
		GRTool.GRToolType grtoolType = GRTool.GRToolType.None;
		if (other.gameObject.GetComponentInParent<GRToolClub>() != null)
		{
			grtoolType = GRTool.GRToolType.Club;
		}
		else if (other.gameObject.GetComponentInParent<GRToolCollector>() != null)
		{
			grtoolType = GRTool.GRToolType.Collector;
		}
		else if (other.gameObject.GetComponentInParent<GRToolFlash>() != null)
		{
			grtoolType = GRTool.GRToolType.Flash;
		}
		else if (other.gameObject.GetComponentInParent<GRToolLantern>() != null)
		{
			grtoolType = GRTool.GRToolType.Lantern;
		}
		else if (other.gameObject.GetComponentInParent<GRToolRevive>() != null)
		{
			grtoolType = GRTool.GRToolType.Revive;
		}
		else if (other.gameObject.GetComponentInParent<GRToolShieldGun>() != null)
		{
			grtoolType = GRTool.GRToolType.ShieldGun;
		}
		else if (other.gameObject.GetComponentInParent<GRToolDirectionalShield>() != null)
		{
			grtoolType = GRTool.GRToolType.DirectionalShield;
		}
		else if (componentInParent.toolType == GRTool.GRToolType.HockeyStick || componentInParent.toolType == GRTool.GRToolType.DockWrist)
		{
			grtoolType = componentInParent.toolType;
		}
		this.recycler.reactor.grManager.RequestRecycleScanItem(grtoolType);
	}

	public GRRecycler recycler;

	[SerializeField]
	private TextMeshPro toolText;

	[SerializeField]
	private TextMeshPro ratesText;

	public AudioSource audioSource;

	public AudioClip recyclerBarcodeAudio;

	public float recyclerBarcodeAudioVolume = 0.5f;
}
