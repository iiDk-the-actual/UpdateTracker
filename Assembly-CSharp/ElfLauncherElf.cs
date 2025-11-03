using System;
using System.Collections;
using UnityEngine;

public class ElfLauncherElf : MonoBehaviour
{
	private void OnEnable()
	{
		base.StartCoroutine(this.ReturnToPoolAfterDelayCo());
	}

	private IEnumerator ReturnToPoolAfterDelayCo()
	{
		yield return new WaitForSeconds(this.destroyAfterDuration);
		ObjectPools.instance.Destroy(base.gameObject);
		yield break;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (this.bounceAudioCoolingDownUntilTimestamp > Time.time)
		{
			return;
		}
		this.bounceAudio.Play();
		this.bounceAudioCoolingDownUntilTimestamp = Time.time + this.bounceAudioCooldownDuration;
	}

	private void FixedUpdate()
	{
		this.rb.AddForce(base.transform.lossyScale.x * Physics.gravity * this.rb.mass, ForceMode.Force);
	}

	[SerializeField]
	private Rigidbody rb;

	[SerializeField]
	private SoundBankPlayer bounceAudio;

	[SerializeField]
	private float bounceAudioCooldownDuration;

	[SerializeField]
	private float destroyAfterDuration;

	private float bounceAudioCoolingDownUntilTimestamp;
}
