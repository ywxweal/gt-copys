using Fusion;
using Photon.Pun;
using UnityEngine;

[NetworkBehaviourWeaved(0)]
public abstract class NetworkComponent<T> : NetworkBehaviour, IPunObservable, IPunInstantiateMagicCallback where T : struct, INetworkStruct
{
	protected virtual T data { get; set; }

	public bool IsLocallyOwned => NetworkSystem.Instance.IsObjectLocallyOwned(base.gameObject);

	public bool ShouldWriteObjectData => NetworkSystem.Instance.ShouldWriteObjectData(base.gameObject);

	public bool ShouldUpdateobject => NetworkSystem.Instance.ShouldUpdateObject(base.gameObject);

	public int OwnerID => NetworkSystem.Instance.GetOwningPlayerID(base.gameObject);

	protected abstract void DataChangeCallback(PhotonMessageInfoWrapped info = default(PhotonMessageInfoWrapped));

	protected abstract void NetUpdate(float deltaTime);

	protected virtual void ResimNetUpdate(float deltaTime)
	{
	}

	public virtual void Update()
	{
		NetUpdate(Time.deltaTime);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
	}

	public virtual void OnSpawned()
	{
		Debug.Log("Spawned callback triggered!", base.gameObject);
	}

	public override void Spawned()
	{
		if (NetworkSystem.Instance.IsOnline && !Object.IsSceneObject)
		{
			OnSpawned();
		}
	}

	public void OnPhotonInstantiate(PhotonMessageInfo info)
	{
		OnSpawned();
	}

	public override void Render()
	{
		OnRender();
	}

	protected virtual void OnRender()
	{
	}

	public override void CopyBackingFieldsToState(bool P_0)
	{
	}

	public override void CopyStateToBackingFields()
	{
	}
}
