using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class PlayerController : MonoBehaviourPun
{
    private Rigidbody playerRigidBody;
    private PlayerCameraAim playerCameraAim;
    private PlayerAction input;

    private GameObject currentHook; 
    private bool isGrappling = false; 


    private float activeMoveSpeed;
    private float activeVerticalSpeed;
    private float activeMaxSpeed;
    [SerializeField] private float baseMoveSpeed = 10f;
    [SerializeField] private float baseVerticalSpeed = 10f;
    [SerializeField] private float baseMaxSpeed = 10f;
    [SerializeField] private float pullSpeed = 20f;
    [SerializeField] private float maxGrappleDistance = 10f;//그래플 최대 거리
    [SerializeField] private float runCoefficient = 3f;


    [SerializeField] private float dashForce = 45f;

    private bool isPulling = false;
    
    private bool dashPressed = false;
    private Vector3 lastMoveDirection = Vector3.forward;

    [SerializeField] private CinemachineBrain mainCamera;
    [SerializeField] private CinemachineVirtualCamera virtualCamera;


    [SerializeField] private Transform hookTarget;
    [SerializeField] public Transform FirePos;
    [SerializeField] private Transform firePoint;
    [SerializeField] private string firePointText = "FirePoint";
    [SerializeField] private string FirePosText = "FirePos";
    [SerializeField] private string bulletText = "Bullet";
    [SerializeField] private string HookText = "Hook";
    Quaternion fireOffset = Quaternion.Euler(-20f, 0f, 0f);


    private void Awake()
    {
        if (IsPhotonViewIsMine())
        {
            input = new PlayerAction();
            playerRigidBody = GetComponent<Rigidbody>();
        }
    }

    private void OnEnable()
    {
        if (IsPhotonViewIsMine())
        {
            //이동,그래플 활성화
            input.Enable();

            input.PlayerActionMap.Run.started += OnRunStarted;
            input.PlayerActionMap.Run.canceled += OnRunCanceled;

            input.PlayerActionMap.Dash.started += OnDashStarted;

            input.PlayerActionMap.Attack.started += OnAttackStarted;

            input.PlayerActionMap.Grapple.started += OnGrappleStarted;
            input.PlayerActionMap.Grapple.canceled += OnGrappleCanceled;
        }
    }

    private void OnDisable()
    {
        if (IsPhotonViewIsMine())
        {
            //이동,그래플 비활성화
            input.Disable();

            input.PlayerActionMap.Run.started -= OnRunStarted;
            input.PlayerActionMap.Run.canceled -= OnRunCanceled;

            input.PlayerActionMap.Dash.started -= OnDashStarted;

            input.PlayerActionMap.Attack.started -= OnAttackStarted;

            input.PlayerActionMap.Grapple.started -= OnGrappleStarted;
            input.PlayerActionMap.Grapple.canceled -= OnGrappleCanceled;
        }
    }

    

    

    private void Start()
    {
        if (IsPhotonViewIsMine())
        {
            //카메라 찾아주기
            mainCamera = FindObjectOfType<CinemachineBrain>();
            virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>(true);
            virtualCamera.gameObject.SetActive(true);
            playerCameraAim = GetComponent<PlayerCameraAim>();
            //virtualCamera.Follow = transform;
            //virtualCamera.LookAt = transform;
            playerCameraAim.SetInput(input);
            playerCameraAim.player = transform;
            playerCameraAim.virtualCamera = virtualCamera;

            firePoint = transform.Find(firePointText);

            FirePos = transform.Find("LeftArm/FirePos");

            activeMoveSpeed = baseMoveSpeed;
            activeVerticalSpeed = baseVerticalSpeed;
            activeMaxSpeed = baseMaxSpeed;
        }
    }


    private void FixedUpdate()
    {
        if (!IsPhotonViewIsMine()) return;

        Move();

        if (dashPressed)
        {
            Dash();
            dashPressed = false;
        }

        if (isPulling && hookTarget != null)
        {
            Hook hookScript = currentHook.GetComponent<Hook>();

            if (hookScript != null && hookScript.IsStuck())
            {
                float distance = Vector3.Distance(transform.position, hookTarget.position);

                // 너무 멀면 끌기 종료 (원한다면 제거)
                if (distance > maxGrappleDistance)
                {
                    isPulling = false;
                    isGrappling = false;  
                    hookTarget = null;
                    PhotonNetwork.Destroy(currentHook);
                    currentHook = null;   
                    return;
                }

                // 일정 거리 이상일 때 끌기
                if (distance > 2f)
                {
                    Vector3 direction = (hookTarget.position - transform.position).normalized;
                    playerRigidBody.AddForce(direction * pullSpeed, ForceMode.Acceleration);
                }
                else
                {
                    isPulling = false;
                }
            }
        }

    }

    private void Move()
    {
        // 1. 입력 받기
        Vector2 moveInput = input.PlayerActionMap.Move.ReadValue<Vector2>();

        // 2. 카메라 기준 방향 설정
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // y축 방향 제거 (지면 기준 방향으로)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // 3. 카메라 기준 이동 방향 계산
        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        // 4. 수직 이동 처리 (Space / Ctrl)
        float y = 0f;
        if (Keyboard.current.spaceKey.isPressed) y += 1f;
        if (Keyboard.current.leftCtrlKey.isPressed) y -= 1f;

        // 5. 최종 이동 벡터
        Vector3 force = moveDirection.normalized * activeMoveSpeed + Vector3.up * (y * activeVerticalSpeed);

        // 6. 이동 적용
        playerRigidBody.AddForce(force, ForceMode.Force);

        // 7. 마지막 방향 저장
        if (force != Vector3.zero)
        {
            lastMoveDirection = force;
        }

        // 8. 최대 속도 제한
        if (playerRigidBody.velocity.magnitude > activeMaxSpeed)
        {
            playerRigidBody.velocity = playerRigidBody.velocity.normalized * activeMaxSpeed;
        }

        // 9. 이동 방향이 있을 때 회전
        if (force != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(force);
            transform.rotation = Quaternion.Slerp
                (transform.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            //transform.rotation = targetRotation;
        }
    }

    private void OnRunStarted(InputAction.CallbackContext context)
    {
        if (Mathf.Approximately(activeMoveSpeed, baseMoveSpeed) &&
            Mathf.Approximately(activeVerticalSpeed, baseVerticalSpeed))
        {
            activeMoveSpeed *= runCoefficient;
            activeVerticalSpeed *= runCoefficient;
            activeMaxSpeed *= runCoefficient;
        }
    }

    private void OnRunCanceled(InputAction.CallbackContext context)
    {
        activeMoveSpeed = baseMoveSpeed;
        activeVerticalSpeed = baseVerticalSpeed;
        activeMaxSpeed = baseMaxSpeed;
    }

    private void Dash()
    {
        playerRigidBody.AddForce(lastMoveDirection.normalized
                                 * dashForce, ForceMode.Impulse);
    }

    private void OnDashStarted(InputAction.CallbackContext context)
    {
        dashPressed = true;
    }

    private void OnAttackStarted(InputAction.CallbackContext context)
    {
        GameObject bullet = PhotonNetwork.Instantiate(bulletText, firePoint.position,
            mainCamera.transform.rotation * fireOffset, 0);

        bullet.GetComponent<Bullet>().SetShooter(photonView.Owner.ActorNumber);
    }
    private void OnGrappleStarted(InputAction.CallbackContext context)
    {
        if (isGrappling || currentHook != null) return;

        Vector3 origin = FirePos.position;
        Vector3 direction = mainCamera.transform.forward;

   
        // 2. Hook 발사 위치 계산 (Hit 지점으로 방향)
        Vector3 spawnPos = origin + direction.normalized * 1f;

        currentHook = PhotonNetwork.Instantiate(HookText, spawnPos, Quaternion.LookRotation(direction), 0);

        Collider hookCol = currentHook.GetComponent<Collider>();
        Collider playerCol = GetComponent<Collider>();
        if (hookCol != null && playerCol != null)
        {
            Physics.IgnoreCollision(hookCol, playerCol);
        }

        Rigidbody rb = currentHook.GetComponent<Rigidbody>();
        if (rb != null && currentHook.GetComponent<PhotonView>().IsMine)
        {
            rb.AddForce(direction * 50f, ForceMode.Impulse);
        }

        Hook hookScript = currentHook.GetComponent<Hook>();
        if (hookScript != null)
        {
            hookScript.SetTarget(FirePos);
        }

        hookTarget = currentHook.transform;
        isPulling = true;
        isGrappling = true;
    }


    private void OnGrappleCanceled(InputAction.CallbackContext context)
    {
        if (currentHook != null)
        {
            PhotonNetwork.Destroy(currentHook);
            currentHook = null;
            isGrappling = false;
            isPulling = false;
            hookTarget = null;
        }
    }
    private bool IsPhotonViewIsMine()
    {
        return photonView.IsMine;
    }
}