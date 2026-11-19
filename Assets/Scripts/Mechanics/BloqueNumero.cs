using TMPro;
using UnityEngine;

public class BloqueNumero : MonoBehaviour
{
    public int valor;
    public BoxCollider2D squerCollider;
    public Collider2D colliderFisico;
    public ZonaColocacion zonaActual;
    public bool bloqueado = false;
    public Vector2 direccionAgarrar;
    public TMP_Text pressF;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //Debug.Log("El jugador ha tocado el bloque con valor: " + valor);
            pressF.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            pressF.enabled = false;
        }
    }
    public void Agarrar()
    {
        squerCollider.enabled = false;
    }

    public void Soltar()
    {
        squerCollider.enabled = true;
    }

    public void Bloquear()
    {
        bloqueado = true;
        squerCollider.enabled = true;
    }
}