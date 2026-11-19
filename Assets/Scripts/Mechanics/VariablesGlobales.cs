using System.Collections.Generic;
using UnityEngine;

public class VariablesGlobales : MonoBehaviour
{
    public static VariablesGlobales Instance;
    public int[] factoresMCM = new int[0];
    public int indiceFactorMCM = 0;
    public string curso;
    public int difucultad = 1;
    public int cantCorrectos = 0;
    public int indiceActual;
    public int racha = 0;

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ActualizarRacha(Ejercicio ejercicio, RetroEjercicio resultado)
    {
        if(resultado.cantIntentos == 0 && resultado.tiempo <= ejercicio.tiempoMaximo)
        {
            racha++;
        }
        else
        {
            racha=0;
        }
    }
}