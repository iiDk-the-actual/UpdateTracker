using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class GorillaEnemyAI : MonoBehaviourPun, IPunObservable, IInRoomCallbacks
{
	private void Start()
	{
		this.agent = base.GetComponent<NavMeshAgent>();
		this.r = base.GetComponent<Rigidbody>();
		this.r.useGravity = true;
		if (!base.photonView.IsMine)
		{
			this.agent.enabled = false;
			this.r.isKinematic = true;
		}
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(base.transform.position);
			stream.SendNext(base.transform.eulerAngles);
			return;
		}
		this.targetPosition = (Vector3)stream.ReceiveNext();
		this.targetRotation = (Vector3)stream.ReceiveNext();
	}

	private void Update()
	{
		if (PhotonNetwork.IsMasterClient)
		{
			this.FindClosestPlayer();
			if (this.playerTransform != null)
			{
				this.agent.destination = this.playerTransform.position;
			}
			base.transform.LookAt(new Vector3(this.playerTransform.transform.position.x, base.transform.position.y, this.playerTransform.position.z));
			this.r.linearVelocity *= 0.99f;
			return;
		}
		base.transform.position = Vector3.Lerp(base.transform.position, this.targetPosition, this.lerpValue);
		base.transform.eulerAngles = Vector3.Lerp(base.transform.eulerAngles, this.targetRotation, this.lerpValue);
	}

	private void FindClosestPlayer()
	{
		VRRig[] array = Object.FindObjectsByType<VRRig>(FindObjectsSortMode.None);
		VRRig vrrig = null;
		float num = 100000f;
		foreach (VRRig vrrig2 in array)
		{
			Vector3 vector = vrrig2.transform.position - base.transform.position;
			if (vector.magnitude < num)
			{
				vrrig = vrrig2;
				num = vector.magnitude;
			}
		}
		this.playerTransform = vrrig.transform;
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.layer == 19)
		{
			PhotonNetwork.Destroy(base.photonView);
		}
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			this.agent.enabled = true;
			this.r.isKinematic = false;
		}
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	public Transform playerTransform;

	private NavMeshAgent agent;

	private Rigidbody r;

	private Vector3 targetPosition;

	private Vector3 targetRotation;

	public float lerpValue;
}
