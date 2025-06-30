using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Bullet : MonoBehaviourPun
{
    private Rigidbody rigidbody;

    private string playerHealthChangeText = "PlayerHealthChange";
    private float BulletSpeed = 30f;

    private int shooterActorNumber;
    private int DestroyMask;

    [SerializeField] private float addGravity = 10f;

    private void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        StartCoroutine(AutoDestroy(3f));
        DestroyMask = (1 << (int)EnumLayer.Ground) | (1 << (int)EnumLayer.Structure);
        MoveBullet();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine) return;
        
        if (other.gameObject.layer == (int)EnumLayer.Player)
        {
            PhotonView pv = other.GetComponent<PhotonView>();

            // other이 null이 아니고 자기 자신이 쏜 총알이면 무시
            if (pv != null && pv.Owner.ActorNumber == shooterActorNumber)
                return;

            pv.RPC(playerHealthChangeText, RpcTarget.Others, -5f);
            Debug.Log("플레이어");
            PhotonNetwork.Destroy(gameObject);
        }

        if ((DestroyMask & (1 << other.gameObject.layer)) != 0)
        {
            Debug.Log("땅이나 구조물");
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void MoveBullet()
    {
        rigidbody.AddForce((transform.forward * BulletSpeed)
                           + (-transform.up * addGravity), ForceMode.VelocityChange);
    }

    public void SetShooter(int actorNumber)
    {
        shooterActorNumber = actorNumber;
    }

    IEnumerator AutoDestroy(float time)
    {
        yield return new WaitForSeconds(time);

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}