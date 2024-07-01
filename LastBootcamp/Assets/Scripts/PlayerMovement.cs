using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float turnSpeed = 360f;
    private Rigidbody rb;

    public GameObject newspaperPrefab; // Gazete prefab'ý
    public Transform throwPoint; // Fýrlatma noktasý
    public float throwForce = 10f; // Fýrlatma gücü

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleMovement();
        HandleThrow();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal") * turnSpeed * Time.deltaTime;
        float vertical = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        Vector3 movement = transform.forward * vertical;
        rb.MovePosition(rb.position + movement);

        Vector3 rotation = new Vector3(0, horizontal, 0);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotation));
    }

    void HandleThrow()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowNewspaper();
        }
    }

    void ThrowNewspaper()
    {
        GameObject newspaper = Instantiate(newspaperPrefab, throwPoint.position, throwPoint.rotation);
        Rigidbody rbNewspaper = newspaper.GetComponent<Rigidbody>();
        rbNewspaper.AddForce(throwPoint.forward * throwForce, ForceMode.Impulse);
    }
}
