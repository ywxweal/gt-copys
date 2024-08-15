using Photon.Pun;

public class TransformViewTeleportSerializer : MonoBehaviourPun, IPunObservable
{
	private bool willTeleport;

	private PhotonTransformView transformView;

	private void Start()
	{
		transformView = GetComponent<PhotonTransformView>();
	}

	public void SetWillTeleport()
	{
		willTeleport = true;
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(willTeleport);
			willTeleport = false;
		}
		else if ((bool)stream.ReceiveNext())
		{
		}
	}
}
