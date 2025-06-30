using UnityEngine;
using Photon.Pun;

public class Hook : MonoBehaviourPun
{
    private Rigidbody rb;
    private bool isStuck = false;
    private LineRenderer lineRenderer;

    private Transform target; // ���� ������, �� �÷��̾� ��ġ

    public void SetTarget(Transform playerArm)
    {
        target = playerArm;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (lineRenderer != null && target != null)
        {
            lineRenderer.SetPosition(0, transform.position); //Hook������Ʈ ����
            lineRenderer.SetPosition(1, target.position);     //�÷��̾� ���� ������
        }
    }
    public bool IsStuck()
    {
        return isStuck;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isStuck && photonView.IsMine)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            transform.position = collision.contacts[0].point;
            isStuck = true;
        }
    }
}
