using UnityEngine;
using Photon.Pun;

public class RopeAction : MonoBehaviour
{
    
    public static RopeAction Instance;

 
    public Transform firePoint;


    public string hookPrefabName = "Hook";


    public float shootForce = 50f;

    private void Awake()
    {
       
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartGrapple()
    {
        if (firePoint == null)
        {
            Debug.LogWarning("FirePoint가 할당되지 않았습니다!");
            return;
        }

      
        GameObject hook = PhotonNetwork.Instantiate(hookPrefabName, firePoint.position, firePoint.rotation, 0);

       
        Rigidbody rb = hook.GetComponent<Rigidbody>();
        if (rb != null && hook.GetComponent<PhotonView>().IsMine)
        {
            rb.AddForce(firePoint.forward * shootForce, ForceMode.Impulse);
        }
    }
}
