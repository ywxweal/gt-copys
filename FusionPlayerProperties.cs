using System;
using System.Runtime.InteropServices;
using Fusion;
using Fusion.CodeGen;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(3862)]
public class FusionPlayerProperties : NetworkBehaviour
{
	[StructLayout(LayoutKind.Explicit, Size = 896)]
	[NetworkStructWeaved(224)]
	public struct PlayerInfo : INetworkStruct
	{
		[FieldOffset(0)]
		[SerializeField]
		private FixedStorage_004017 _NickName;

		[FieldOffset(68)]
		[SerializeField]
		private FixedStorage_0040207 _properties;

		[Networked]
		public unsafe NetworkString<_16> NickName
		{
			readonly get
			{
				return *(NetworkString<_16>*)Native.ReferenceToPointer(ref _NickName);
			}
			set
			{
				*(NetworkString<_16>*)Native.ReferenceToPointer(ref _NickName) = value;
			}
		}

		[Networked]
		public unsafe NetworkDictionary<NetworkString<_32>, NetworkString<_32>> properties => new NetworkDictionary<NetworkString<_32>, NetworkString<_32>>((int*)Native.ReferenceToPointer(ref _properties), 3, ReaderWriter_0040Fusion_NetworkString_00601_003CFusion__32_003E.GetInstance(), ReaderWriter_0040Fusion_NetworkString_00601_003CFusion__32_003E.GetInstance());
	}

	public delegate void PlayerAttributeOnChanged();

	public PlayerAttributeOnChanged playerAttributeOnChanged;

	private static Changed<FusionPlayerProperties> _0024IL2CPP_CHANGED;

	private static ChangedDelegate<FusionPlayerProperties> _0024IL2CPP_CHANGED_DELEGATE;

	private static NetworkBehaviourCallbacks<FusionPlayerProperties> _0024IL2CPP_NETWORK_BEHAVIOUR_CALLBACKS;

	[DefaultForProperty("netPlayerAttributes", 0, 3862)]
	private SerializableDictionary<PlayerRef, PlayerInfo> _netPlayerAttributes;

	[Networked(OnChanged = "AttributesChanged")]
	[Capacity(10)]
	[NetworkedWeaved(0, 3862)]
	private unsafe NetworkDictionary<PlayerRef, PlayerInfo> netPlayerAttributes
	{
		get
		{
			if (Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing FusionPlayerProperties.netPlayerAttributes. Networked properties can only be accessed when Spawned() has been called.");
			}
			return new NetworkDictionary<PlayerRef, PlayerInfo>((int*)((byte*)Ptr + 0), 17, ReaderWriter_0040Fusion_PlayerRef.GetInstance(), ReaderWriter_0040FusionPlayerProperties__PlayerInfo.GetInstance());
		}
	}

	public PlayerInfo PlayerProperties => netPlayerAttributes[Runner.LocalPlayer];

	[Preserve]
	public static void AttributesChanged(Changed<FusionPlayerProperties> changed)
	{
		changed.Behaviour.OnAttributesChanged();
	}

	private void OnAttributesChanged()
	{
		playerAttributeOnChanged?.Invoke();
	}

	[Rpc(RpcSources.All, RpcTargets.All, InvokeLocal = true, TickAligned = true)]
	public unsafe void RPC_UpdatePlayerAttributes(PlayerInfo newInfo, RpcInfo info = default(RpcInfo))
	{
		if (InvokeRpc)
		{
			InvokeRpc = false;
		}
		else
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (Runner.Stage == SimulationStages.Resimulate)
			{
				return;
			}
			int localAuthorityMask = Object.GetLocalAuthorityMask();
			if ((localAuthorityMask & 7) == 0)
			{
				NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void FusionPlayerProperties::RPC_UpdatePlayerAttributes(FusionPlayerProperties/PlayerInfo,Fusion.RpcInfo)", Object, 7);
				return;
			}
			if (Runner.HasAnyActiveConnections())
			{
				int num = 8;
				num += 896;
				SimulationMessage* ptr = SimulationMessage.Allocate(Runner.Simulation, num);
				byte* data = SimulationMessage.GetData(ptr);
				int num2 = RpcHeader.Write(RpcHeader.Create(Object.Id, ObjectIndex, 1), data);
				*(PlayerInfo*)(data + num2) = newInfo;
				num2 += 896;
				ptr->Offset = num2 * 8;
				Runner.SendRpc(ptr);
			}
			if ((localAuthorityMask & 7) == 0)
			{
				return;
			}
			info = RpcInfo.FromLocal(Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
		}
		Debug.Log("Update Player attributes triggered");
		PlayerRef source = info.Source;
		if (netPlayerAttributes.ContainsKey(source))
		{
			Debug.Log("Current nickname is " + netPlayerAttributes[source].NickName.ToString());
			Debug.Log("Sent nickname is " + newInfo.NickName.ToString());
			if (netPlayerAttributes[source].Equals(newInfo))
			{
				Debug.Log("Info is already correct for this user. Shouldnt have received an RPC in this case.");
				return;
			}
		}
		netPlayerAttributes.Set(source, newInfo);
	}

	public override void Spawned()
	{
		Debug.Log("Player props SPAWNED!");
		if (Runner.Mode == SimulationModes.Client)
		{
			Debug.Log("SET Player Properties manager!");
		}
	}

	public string GetDisplayName(PlayerRef player)
	{
		return netPlayerAttributes[player].NickName.Value;
	}

	public string GetLocalDisplayName()
	{
		return netPlayerAttributes[Runner.LocalPlayer].NickName.Value;
	}

	public bool GetProperty(PlayerRef player, string propertyName, out string propertyValue)
	{
		if (netPlayerAttributes[player].properties.TryGet(propertyName, out var value))
		{
			propertyValue = value.Value;
			return true;
		}
		propertyValue = null;
		return false;
	}

	public bool PlayerHasEntry(PlayerRef player)
	{
		return netPlayerAttributes.ContainsKey(player);
	}

	public void RemovePlayerEntry(PlayerRef player)
	{
		if (Object.HasStateAuthority)
		{
			string value = netPlayerAttributes[player].NickName.Value;
			netPlayerAttributes.Remove(player);
			Debug.Log("Removed " + value + "player properties as they just left.");
		}
	}

	public override void CopyBackingFieldsToState(bool P_0)
	{
		NetworkBehaviourUtils.InitializeNetworkDictionary(netPlayerAttributes, _netPlayerAttributes, "netPlayerAttributes");
	}

	public override void CopyStateToBackingFields()
	{
		NetworkBehaviourUtils.CopyFromNetworkDictionary(netPlayerAttributes, ref _netPlayerAttributes);
	}

	[NetworkRpcWeavedInvoker(1, 7, 7)]
	[Preserve]
	protected unsafe static void RPC_UpdatePlayerAttributes_0040Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* data = SimulationMessage.GetData(message);
		int num = (RpcHeader.ReadSize(data) + 3) & -4;
		PlayerInfo playerInfo = *(PlayerInfo*)(data + num);
		num += 896;
		PlayerInfo newInfo = playerInfo;
		RpcInfo info = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((FusionPlayerProperties)behaviour).RPC_UpdatePlayerAttributes(newInfo, info);
	}
}
