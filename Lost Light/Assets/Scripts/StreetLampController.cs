using UnityEngine;
using UnityEngine.InputSystem;

public class StreetLampController : MonoBehaviour
{
    [SerializeField] private Light lampLight; // Point Light referans�
    private bool isLampOn = false;
    private GameObject player; // Oyuncunun referans�

    public float activationDistance = 2.0f; // Lamban�n aktif olaca�� mesafe

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Oyuncunun tag'i "Player" olmal�
        lampLight = GetComponentInChildren<Light>();
    }

    public void ToggleLamp()
    {
        isLampOn = !isLampOn;
        lampLight.enabled = isLampOn;
    }

    public bool IsPlayerInRange()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer <= activationDistance;
        }
        return false;
    }
}
