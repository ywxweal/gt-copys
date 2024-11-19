using System;
using Fusion;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
internal abstract class GorillaWrappedSerializer<T> : NetworkBehaviour, IPunObservable, IPunInstantiateMagicCallback, IOnPhotonViewPreNetDestroy, IPhotonViewCallback where T : struct, INetworkStruct
{
	protected bool successfullInstantiate;

	private IWrappedSerializable serializeTarget;

	private Type targetType;

	protected GameObject targetObject;

	[SerializeField]
	protected PhotonView photonView;

	[SerializeField]
	protected NetworkObject networkObject;

	protected virtual T data { get; set; }

	public bool IsLocallyOwned => photonView.IsMine;

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		if (!(photonView == null))
		{
			PhotonMessageInfoWrapped wrappedInfo = new PhotonMessageInfoWrapped(info);
			ProcessSpawn(wrappedInfo);
		}
	}

	public override void Spawned()
	{
		PhotonMessageInfoWrapped wrappedInfo = new PhotonMessageInfoWrapped(Object.StateAuthority.PlayerId, Runner.Tick.Raw);
		ProcessSpawn(wrappedInfo);
	}

	private void ProcessSpawn(PhotonMessageInfoWrapped wrappedInfo)
	{
		successfullInstantiate = OnSpawnSetupCheck(wrappedInfo, out targetObject, out targetType);
		if (successfullInstantiate)
		{
			if (targetObject?.GetComponent(targetType) is IWrappedSerializable wrappedSerializable)
			{
				serializeTarget = wrappedSerializable;
			}
			if (targetType == null || targetObject == null || serializeTarget == null)
			{
				successfullInstantiate = false;
			}
		}
		if (successfullInstantiate)
		{
			OnSuccesfullySpawned(wrappedInfo);
		}
		else
		{
			FailedToSpawn();
		}
	}

	protected virtual bool OnSpawnSetupCheck(PhotonMessageInfoWrapped wrappedInfo, out GameObject outTargetObject, out Type outTargetType)
	{
		outTargetType = typeof(IWrappedSerializable);
		outTargetObject = base.gameObject;
		return true;
	}

	protected abstract void OnSuccesfullySpawned(PhotonMessageInfoWrapped info);

	private void FailedToSpawn()
	{
		Debug.LogError("Failed to network instantiate");
		if (photonView.IsMine)
		{
			PhotonNetwork.Destroy(photonView);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
		photonView.ObservedComponents.Remove(this);
	}

	protected abstract void OnFailedSpawn();

	public override void FixedUpdateNetwork()
	{
		if (Object.HasStateAuthority)
		{
			data = (T)serializeTarget.OnSerializeWrite();
		}
		else
		{
			serializeTarget.OnSerializeRead(data);
		}
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (successfullInstantiate && info.Sender == info.photonView.Owner && serializeTarget != null)
		{
			if (stream.IsWriting)
			{
				serializeTarget.OnSerializeWrite(stream, info);
			}
			else
			{
				serializeTarget.OnSerializeRead(stream, info);
			}
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		OnBeforeDespawn();
	}

	void IOnPhotonViewPreNetDestroy.OnPreNetDestroy(PhotonView rootView)
	{
		OnBeforeDespawn();
	}

	protected abstract void OnBeforeDespawn();

	public override void CopyBackingFieldsToState(bool P_0)
	{
	}

	public override void CopyStateToBackingFields()
	{
	}
}
