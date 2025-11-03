using System;
using System.Collections.Generic;
using UnityEngine;

public class GRArmorEnemy : MonoBehaviour
{
	private void Awake()
	{
		this.SetHp(0);
		this.entity = base.GetComponent<GameEntity>();
	}

	public void SetHp(int hp)
	{
		this.hp = hp;
		this.RefreshArmor();
	}

	private void RefreshArmor()
	{
		bool flag = this.hp > 0;
		GRArmorEnemy.Hide(this.renderers, !flag);
		if (flag && this.armorStateMaterials.Count > 0 && this.armorStateMaterials.Count == this.armorStateThresholds.Length)
		{
			Material material = this.armorStateMaterials[0];
			int num = 0;
			while (num < this.armorStateMaterials.Count && this.hp <= this.armorStateThresholds[num])
			{
				material = this.armorStateMaterials[num];
				if (this.hp == this.armorStateThresholds[num])
				{
					break;
				}
				num++;
			}
			if (material != this.renderers[0].material)
			{
				this.renderers[0].material = material;
				this.SetArmorColor(this.GetArmorColor());
			}
		}
	}

	public void SetArmorColor(Color newColor)
	{
		if (this.renderers != null && this.renderers.Count > 0)
		{
			this.renderers[0].material.SetColor("_BaseColor", newColor);
		}
	}

	public Color GetArmorColor()
	{
		Color color = Color.white;
		if (this.renderers.Count > 0)
		{
			color = this.renderers[0].material.GetColor("_BaseColor");
		}
		return color;
	}

	public static void Hide(List<Renderer> renderers, bool hide)
	{
		if (renderers == null)
		{
			return;
		}
		for (int i = 0; i < renderers.Count; i++)
		{
			if (renderers[i] != null)
			{
				renderers[i].enabled = !hide;
			}
		}
	}

	public void PlayHitFx(Vector3 position)
	{
		this.PlayFx(this.fxHit, position);
		this.PlaySound(this.hitSound, this.hitSoundVolume, position);
	}

	public void PlayBlockFx(Vector3 position)
	{
		this.PlayFx(this.fxBlock, position);
		this.PlaySound(this.blockSound, this.blockSoundVolume, position);
	}

	public void PlayDestroyFx(Vector3 position)
	{
		this.PlayFx(this.fxDestroy, position);
		this.PlaySound(this.destroySound, this.destroySoundVolume, position);
	}

	private void PlayFx(GameObject fx, Vector3 position)
	{
		if (fx == null)
		{
			return;
		}
		fx.SetActive(false);
		fx.SetActive(true);
	}

	private void PlaySound(AudioClip clip, float volume, Vector3 position)
	{
		this.audioSource.clip = clip;
		this.audioSource.volume = volume;
		this.audioSource.Play();
	}

	public void FragmentArmor()
	{
		if (this.entity.IsAuthority())
		{
			float num = 0f;
			for (int i = 0; i < this.numFragmentsWhenShattered; i++)
			{
				num += 360f / (float)this.numFragmentsWhenShattered;
				Quaternion quaternion = Quaternion.Euler(0f, num, this.fragmentLaunchPitch);
				Vector3 vector = quaternion * this.fragmentSpawnOffset;
				this.entity.manager.RequestCreateItem(this.armorFragmentPrefab.name.GetStaticHash(), base.transform.position + vector, quaternion, (long)this.entity.GetNetId());
			}
		}
	}

	[SerializeField]
	private List<Renderer> renderers;

	[SerializeField]
	private AudioSource audioSource;

	[SerializeField]
	private GameObject fxHit;

	[SerializeField]
	private AudioClip hitSound;

	[SerializeField]
	private float hitSoundVolume;

	[SerializeField]
	private GameObject fxBlock;

	[SerializeField]
	private AudioClip blockSound;

	[SerializeField]
	private float blockSoundVolume;

	[SerializeField]
	private GameObject fxDestroy;

	[SerializeField]
	private AudioClip destroySound;

	[SerializeField]
	private float destroySoundVolume;

	[SerializeField]
	private List<Material> armorStateMaterials;

	[SerializeField]
	private int[] armorStateThresholds;

	private GameEntity entity;

	public GameObject armorFragmentPrefab;

	public Vector3 fragmentSpawnOffset = new Vector3(0f, 0.5f, 0.5f);

	public int numFragmentsWhenShattered = 3;

	public float fragmentLaunchPitch = 30f;

	private int hp;
}
