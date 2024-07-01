using UnityEngine;

public class LampController : MonoBehaviour
{
    private Light lampLight;
    public float interactDistance = 5.0f;

    void Start()
    {
        lampLight = GetComponentInChildren<Light>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float distance = Vector3.Distance(transform.position, GameObject.FindWithTag("Player").transform.position);
            if (distance <= interactDistance)
            {
                lampLight.enabled = !lampLight.enabled;
            }
        }
    }
}
