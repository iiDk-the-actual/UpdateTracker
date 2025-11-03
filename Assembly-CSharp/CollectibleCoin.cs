using System;
using BoingKit;
using UnityEngine;

public class CollectibleCoin : MonoBehaviour
{
	public void Update()
	{
		BoingBehavior component = base.GetComponent<BoingBehavior>();
		if (this.m_taken)
		{
			if (Time.time - this.m_respawnTimerStartTime < this.RespawnTime)
			{
				return;
			}
			base.transform.position = this.m_respawnPosition + 0.4f * Vector3.down;
			if (component != null)
			{
				component.Reboot();
			}
			base.transform.position = this.m_respawnPosition;
			this.m_taken = false;
		}
		GameObject gameObject = GameObject.Find("Character");
		GameObject gameObject2 = GameObject.Find("Coin Icon");
		GameObject gameObject3 = GameObject.Find("Coin Counter");
		if ((gameObject.transform.position - base.transform.position).sqrMagnitude > 0.4f)
		{
			return;
		}
		this.m_respawnPosition = base.transform.position;
		if (component != null)
		{
			Vector3Spring positionSpring = component.PositionSpring;
			positionSpring.Reset(base.transform.position, new Vector3(100f, 0f, 0f));
			component.PositionSpring = positionSpring;
		}
		base.transform.position = gameObject2.transform.position + new Vector3(-2f, 0.5f, 0f);
		TextMesh component2 = gameObject3.GetComponent<TextMesh>();
		component2.text = (Convert.ToInt32(component2.text) + 1).ToString();
		this.m_respawnTimerStartTime = Time.time;
		this.m_taken = true;
	}

	public float RespawnTime;

	private bool m_taken;

	private Vector3 m_respawnPosition;

	private float m_respawnTimerStartTime;
}
