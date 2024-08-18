using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

internal class RoomSystem : MonoBehaviour, IInRoomCallbacks, IMatchmakingCallbacks
{
	private static RoomSystem callbackInstance;

	private static List<Player> playersInRoom = new List<Player>(10);

	private static string roomGameMode = "";

	public static List<Player> PlayersInRoom => playersInRoom;

	public static string RoomGameMode => roomGameMode;

	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		PhotonNetwork.AddCallbackTarget(this);
		callbackInstance = this;
	}

	void IMatchmakingCallbacks.OnJoinedRoom()
	{
		foreach (Player value2 in PhotonNetwork.CurrentRoom.Players.Values)
		{
			playersInRoom.Add(value2);
		}
		PlayerCosmeticsSystem.UpdatePlayerCosmetics(playersInRoom);
		if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out var value) && value is string text)
		{
			roomGameMode = text;
		}
	}

	void IInRoomCallbacks.OnPlayerEnteredRoom(Player newPlayer)
	{
		playersInRoom.Add(newPlayer);
		PlayerCosmeticsSystem.UpdatePlayerCosmetics(newPlayer);
	}

	void IMatchmakingCallbacks.OnLeftRoom()
	{
		playersInRoom.Clear();
		roomGameMode = "";
		PlayerCosmeticsSystem.StaticReset();
	}

	void IInRoomCallbacks.OnPlayerLeftRoom(Player otherPlayer)
	{
		playersInRoom.Remove(otherPlayer);
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
	}

	void IInRoomCallbacks.OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
	{
	}

	void IInRoomCallbacks.OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
	{
	}

	void IMatchmakingCallbacks.OnFriendListUpdate(List<FriendInfo> friendList)
	{
	}

	void IMatchmakingCallbacks.OnCreatedRoom()
	{
	}

	void IMatchmakingCallbacks.OnCreateRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRoomFailed(short returnCode, string message)
	{
	}

	void IMatchmakingCallbacks.OnJoinRandomFailed(short returnCode, string message)
	{
	}
}
