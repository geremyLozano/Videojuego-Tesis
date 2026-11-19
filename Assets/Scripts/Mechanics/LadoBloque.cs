using UnityEngine;

public class LadoBloque : MonoBehaviour
{
    public BloqueNumero bloque;
    public Vector2 direccion;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            bloque.direccionAgarrar = direccion;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // No hacemos nada aquí.
        }
    }
}