using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Random = UnityEngine.Random;

public class PlayerColorSync : MonoBehaviourPunCallbacks
{
    [SerializeField] private Material playerMaterial;
    private string playerColorText = "PlayerColor";
    private TrailRenderer playerTrailRenderer;

    private void Awake()
    {
        playerMaterial = GetComponent<Renderer>().material;
        playerTrailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            playerMaterial.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

            Vector3 colorVec = new Vector3(playerMaterial.color.r, playerMaterial.color.g, playerMaterial.color.b);
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
            {
                { playerColorText, colorVec }
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            
            //플레이어 라인 렌더러 색 변경
            playerTrailRenderer.material.color = playerMaterial.color*3f;
        }
        else
        {
            if (photonView.Owner.CustomProperties.ContainsKey(playerColorText))
            {
                Vector3 colorVec = (Vector3)photonView.Owner.CustomProperties[playerColorText];
                playerMaterial.color = new Color(colorVec.x, colorVec.y, colorVec.z);

                //플레이어 라인 렌더러 색 변경
                playerTrailRenderer.material.color = playerMaterial.color*3f;
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (photonView.Owner == targetPlayer && changedProps.ContainsKey(playerColorText))
        {
            Vector3 vec = (Vector3)changedProps[playerColorText];
            Color color = new Color(vec.x, vec.y, vec.z);
            playerMaterial.color = color;
        }
    }
}