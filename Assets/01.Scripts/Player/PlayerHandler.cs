using Photon.Pun;
using UnityEngine;

public class PlayerHandler : MonoBehaviourPun
{
    private PlayerStatus playerStatus = new PlayerStatus();
    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    [PunRPC]    
    public void PlayerHealthChange(float health)
    {
        playerStatus.Health += health;
        Debug.Log($"[{photonView.Owner.ActorNumber}] 피격됨! 현재 체력:{playerStatus.Health}");

        photonView.RPC("UpdateHealth", RpcTarget.Others, playerStatus.Health);
    }
    
    [PunRPC]
    public void UpdateHealth(float newHealth)
    {
        if (photonView.IsMine) return; // 나는 이미 체력 알고 있음
        playerStatus.Health = newHealth;
        Debug.Log($"[갱신됨] 다른 사람이 본 내 체력: {photonView.Owner.ActorNumber},{playerStatus.Health}");
    }
}