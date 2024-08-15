using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonUserCache : IInRoomCallbacks, IMatchmakingCallbacks
{
	private static PhotonUserCache gUserCache;

	private readonly Dictionary<int, GameObject> _playerObjects = new Dictionary<int, GameObject>();

	private TimeSince _timeSinceLastRefresh;

	private bool _needsRefresh;

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize()
	{
		gUserCache = new PhotonUserCache();
		PhotonNetwork.AddCallbackTarget(gUserCache);
	}

	public static void Invalidate()
	{
		if (gUserCache != null)
		{
			PhotonNetwork.RemoveCallbackTarget(gUserCache);
			gUserCache._playerObjects.Clear();
			gUserCache = null;
		}
		Initialize();
	}

	public static bool TryGetPlayerObject(int actorNumber, out GameObject playerObject)
	{
		gUserCache.SafeRefresh();
		return gUserCache._playerObjects.TryGetValue(actorNumber, out playerObject);
	}

	private void SafeRefresh()
	{
		if (_needsRefresh)
		{
			Refresh();
		}
	}

	private void Refresh(bool force = false)
	{
		bool flag = (float)_timeSinceLastRefresh < 3f;
		if (flag)
		{
			_needsRefresh = true;
			if (!force)
			{
				return;
			}
		}
		PhotonView[] array = Object.FindObjectsOfType<PhotonView>();
		foreach (PhotonView photonView in array)
		{
			Player owner = photonView.Owner;
			if (owner != null)
			{
				_playerObjects[owner.ActorNumber] = photonView.gameObject;
			}
		}
		if (!flag && _needsRefresh)
		{
			_needsRefresh = false;
		}
		_timeSinceLastRefresh = 0f;
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
		Refresh();
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		Refresh(force: true);
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		_playerObjects.Clear();
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player player)
	{
		if (_playerObjects.ContainsKey(player.ActorNumber))
		{
			_playerObjects.Remove(player.ActorNumber);
		}
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable changedProperties)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player player, Hashtable changedProperties)
	{
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
	}
}
