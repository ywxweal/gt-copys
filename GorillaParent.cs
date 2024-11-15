using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaParent : MonoBehaviour
{
	public GameObject tagUI;

	public GameObject playerParent;

	public GameObject vrrigParent;

	[OnEnterPlay_SetNull]
	public static volatile GorillaParent instance;

	[OnEnterPlay_Set(false)]
	public static bool hasInstance;

	public List<VRRig> vrrigs;

	public Dictionary<NetPlayer, VRRig> vrrigDict = new Dictionary<NetPlayer, VRRig>(); // if 5 errors replace NetPlayer with Player

	private int i;

	private PhotonView[] childPhotonViews;

	private bool joinedRoom;

	private static bool replicatedClientReady;

	private static Action onReplicatedClientReady;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
			hasInstance = true;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected void OnDestroy()
	{
		if (instance == this)
		{
			hasInstance = false;
			instance = null;
		}
	}

	public void LateUpdate()
	{
		for (i = vrrigs.Count - 1; i > -1; i--)
		{
			if (vrrigs[i] == null)
			{
				vrrigs.RemoveAt(i);
			}
		}
		if (RoomSystem.JoinedRoom && GorillaTagger.Instance.offlineVRRig.photonView == null)
		{
			PhotonNetwork.Instantiate("GorillaPrefabs/Gorilla Player Networked", Vector3.zero, Quaternion.identity, 0);
		}
	}

	public static void ReplicatedClientReady()
	{
		replicatedClientReady = true;
		onReplicatedClientReady?.Invoke();
	}

	public static void OnReplicatedClientReady(Action action)
	{
		if (replicatedClientReady)
		{
			action();
		}
		else
		{
			onReplicatedClientReady = (Action)Delegate.Combine(onReplicatedClientReady, action);
		}
	}
}
