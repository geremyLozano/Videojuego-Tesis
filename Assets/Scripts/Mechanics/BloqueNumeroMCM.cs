using TMPro;
using UnityEngine;

public class BloqueNumeroMCM : MonoBehaviour
{
    public int valor;
    public SpriteRenderer square;
    public bool bloqueado = false;
    private MecanicaMCM mecanicaMCM;
    
    private void Start()
    {
        mecanicaMCM = FindFirstObjectByType<MecanicaMCM>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            BloqueNumeroMCM bloqueNumero =
                GetComponent<BloqueNumeroMCM>();

            // Si el bloque ya fue acertado, no hacer nada
            if (bloqueNumero.bloqueado)
                return;

            string valor =
                bloqueNumero.GetComponentInChildren<TMPro.TMP_Text>().text;

            int valorInt = int.Parse(valor);

            int indiceFactor =
                VariablesGlobales.Instance.indiceFactorMCM;

            Debug.Log("El valor del bloque es: " + valorInt);
            Debug.Log("El indice del bloque es: " + indiceFactor);

            if (indiceFactor >=
                VariablesGlobales.Instance.factoresMCM.Length)
            {
                return;
            }

            int factorActual =
                VariablesGlobales.Instance.factoresMCM[indiceFactor];

            Debug.Log("El factorActual del bloque es: " + factorActual);

            if (valorInt == factorActual)
            {
                Debug.Log("¡Factor correcto!");

                square.color = Color.green;

                VariablesGlobales.Instance.cantCorrectos++;
                VariablesGlobales.Instance.indiceFactorMCM++;

                mecanicaMCM.AplicarFactor(factorActual);

                // Bloquear el bloque correcto
                bloqueNumero.bloqueado = true;

                // Desactivar su collider
                GetComponent<BoxCollider2D>().enabled = false;
            }
            else
            {
                Debug.Log("Factor incorrecto");

                square.color = Color.red;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }

}