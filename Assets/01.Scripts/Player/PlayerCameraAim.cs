using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerCameraAim : MonoBehaviourPun
{
    private PlayerAction input;

    public Transform player;
    public CinemachineVirtualCamera virtualCamera;
    public Vector3 viewOffset = new Vector3(0, 1, -6);
    public float rotationSpeed = 30f;

    [SerializeField] private float yaw = 0f;
    [SerializeField] private float pitch = 15f;

    public float minPitch = -80f;
    public float maxPitch = 80f;
    
    public Vector3 offset = new Vector3(0, 2, -6);
    public float smoothSpeed = 10f;  
    public LayerMask obstacleLayers;
    
    private Vector3 currentVelocity;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void LateUpdate()
    {
        //if (target == null) return;
        if (!photonView.IsMine) return;

        MouseAim();
    }

    private void MouseAim()
    {
        // Input System 기반 마우스 입력 받기
        Vector2 mouseDelta = input.PlayerActionMap.MouseAim.ReadValue<Vector2>();

        yaw += mouseDelta.x * rotationSpeed * Time.deltaTime;
        pitch -= mouseDelta.y * rotationSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = player.position + rotation * viewOffset;

        virtualCamera.transform.position = desiredPosition;
        virtualCamera.transform.LookAt(player);
    }

    //아직 덜 완성 나중에 다시 보자 벽 부딪치면 시점이동
    private void HandleCameraCollision()
    {
        if (player == null) return;

        // 회전 적용된 방향으로 offset 계산
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = player.position + rotation * offset;

        Vector3 direction = targetPosition - player.position;
        float distance = direction.magnitude;

        if (Physics.Raycast(player.position, direction.normalized, out RaycastHit hit, distance, obstacleLayers))
        {
            // 충돌 시 살짝 앞쪽으로 당기기
            targetPosition = hit.point - direction.normalized * 0.2f;
        }

        // 카메라 위치 부드럽게 이동
        virtualCamera.transform.position = Vector3.SmoothDamp(
            virtualCamera.transform.position,
            targetPosition,
            ref currentVelocity,
            0.05f
        );

        virtualCamera.transform.rotation = rotation;
        virtualCamera.transform.LookAt(player.position + Vector3.up * 1.5f);
    }

    public void SetInput(PlayerAction a)
    {
        input = a;
    }
}