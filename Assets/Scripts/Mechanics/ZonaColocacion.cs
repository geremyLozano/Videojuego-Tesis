using UnityEngine;

public class ZonaColocacion : MonoBehaviour
{
    public BloqueNumero bloqueNumero;
    public float[] valoresEsperados;
    public SpriteRenderer spriteRenderer;
       
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("COLISIÓN CON: " + collision.name);
        if (collision.CompareTag("BloqueNumero"))
        {
            BloqueNumero bloque = collision.GetComponent<BloqueNumero>();
            if (bloque != null)
            {
                bloqueNumero = bloque;
                bloque.zonaActual = this;
                Debug.Log("Bloque con valor " + bloque.valor + " colocado en la zona de colocación.");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("BloqueNumero"))
        {
            BloqueNumero bloque = collision.GetComponent<BloqueNumero>();

            if (bloque != null)
            {
                bloqueNumero = bloque;
                bloque.zonaActual = this;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("SALIÓ DE ZONA: " + collision.name);
        if (collision.CompareTag("BloqueNumero"))
        {
            BloqueNumero bloque = collision.GetComponent<BloqueNumero>();
            if (bloque != null && bloque == bloqueNumero)
            {
                bloqueNumero = null;
                bloque.zonaActual = null;
                Debug.Log("Bloque con valor " + bloque.valor + " retirado de la zona de colocación.");
            }
        }
    }

    public bool ValidarBloque()
    {
        if(bloqueNumero == null)
        {
            Debug.Log("No hay bloque en la zona de colocación.");
            return false;
        }
        float valor = (float)bloqueNumero.valor;
        bool valorEncontrado = false;
        for(int i = 0; i < valoresEsperados.Length; i++)
        {
            if(valor == valoresEsperados[i])
                valorEncontrado = true;
        }
        if(valorEncontrado)
        {
            Debug.Log("Bloque con valor " + bloqueNumero.valor + " es correcto.");
            bloqueNumero.Bloquear();
            VariablesGlobales.Instance.cantCorrectos++;
            spriteRenderer.color = Color.green;
            return true;
        }
        else
        {
            spriteRenderer.color = Color.red;
            DatosPartida.retroEjercicio[VariablesGlobales.Instance.indiceActual].cantIntentos++;
            //Debug.Log("Bloque con valor " + bloqueNumero.valor + " es incorrecto. Se esperaba: " + valorEsperado);
            return false;
        }
    }

    public static ZonaColocacion ObtenerZonaDeBloque(BloqueNumero bloque)
    {
        ZonaColocacion[] zonas = FindObjectsByType<ZonaColocacion>(FindObjectsSortMode.None);

        foreach (ZonaColocacion zona in zonas)
        {
            if(zona.bloqueNumero == bloque)
            {
                return zona;
            }
        }
        return null;
    }

}