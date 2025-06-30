using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    public string roomText = "TestRoom";
    public string playerPrefabText = "PlayerPrefab";
    public Vector3 spawnPos = new Vector3(0, 0, 0);

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("서버 접속 시도 중...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버 연결됨, 룸 입장 시도...");
        PhotonNetwork.JoinOrCreateRoom(roomText, new RoomOptions { MaxPlayers = 6 }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("룸 입장 완료: " + roomText);
        PhotonNetwork.Instantiate(playerPrefabText, spawnPos, Quaternion.identity);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("룸 입장 실패: " + message);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Photon 연결 끊김: " + cause);
    }
}