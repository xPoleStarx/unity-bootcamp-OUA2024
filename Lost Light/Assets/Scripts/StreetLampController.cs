using UnityEngine;
using UnityEngine.InputSystem;

public class StreetLampController : MonoBehaviour
{
    [SerializeField] private Light lampLight; // Point Light referansý
    private bool isLampOn = false;
    private GameObject player; // Oyuncunun referansý

    public float activationDistance = 2.0f; // Lambanýn aktif olacaðý mesafe

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player"); // Oyuncunun tag'i "Player" olmalý
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
