using UnityEngine;

public class CambioHabitacion : MonoBehaviour
{
    public Vector3 desplazamientoCamara;
    public GameObject camara;
    public bool movement = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (movement)
            {
                desplazamientoCamara = new Vector3(18f, 0f, 0f);
                movement = false;
            }
            else
            {
                desplazamientoCamara = new Vector3(-18f, 0f, 0f);
                movement = true;
            }
            camara.transform.position += desplazamientoCamara;
        }
    }
}