using UnityEngine;

public class DDAManager : MonoBehaviour
{
    public ContenidoManager contenidoManager;
    public enum Desempeno
    {
        Alto,
        Adecuado,
        Bajo
    }

    public enum Accion
    {
        Aumentar,
        Mantener,
        Reducir
    }

    public Desempeno EvaluarDesempeno(Ejercicio ejercicio, RetroEjercicio resultado, int racha, bool correcto)
    {
        //R1
        if (!correcto && resultado.tiempo > ejercicio.tiempoMaximo)
        {
            return Desempeno.Bajo;
        }

        //R2
        if (correcto && resultado.cantIntentos > ejercicio.maxIntentos)
        {
            return Desempeno.Bajo;
        }

        //R3
        if (correcto && resultado.cantIntentos <= ejercicio.maxIntentos && resultado.tiempo > ejercicio.tiempoMaximo)
        {
            return Desempeno.Adecuado;
        }

        if (correcto && resultado.cantIntentos <= ejercicio.maxIntentos &&
            resultado.tiempo <= ejercicio.tiempoMaximo && racha >= 2)
        {
            return Desempeno.Alto;
        }

        return Desempeno.Adecuado;
    }

    public Accion ObtenerAccion(Desempeno desempeno)
    {
        switch (desempeno)
        {
            case Desempeno.Alto:
                return Accion.Aumentar;

            case Desempeno.Bajo:
                return Accion.Reducir;

            default:
                return Accion.Mantener;
        }
    }

    public void EjecutarDDA(Accion accion)
    {
        int indice = VariablesGlobales.Instance.indiceActual;
        int dificultad = VariablesGlobales.Instance.difucultad;
        string curso = VariablesGlobales.Instance.curso;
        if (accion == Accion.Aumentar)
        {
            if (dificultad < 3)
            {
                dificultad++;
                for (int i = indice + 1; i < 10; i++)
                {
                    
                    Ejercicio ejercicio = contenidoManager.ObtenerEjercicioAleatorio(curso, DatosPartida.temas[i].nombre, dificultad);
                    DatosPartida.ejercicios[i] = ejercicio;
                    Debug.Log("Dificultad actualizada " + dificultad);
                }
                VariablesGlobales.Instance.difucultad = dificultad;

            }
        }
        if (accion == Accion.Reducir)
        {

            if (dificultad <= 3 && dificultad > 1)
            {
                dificultad--;
                for (int i = indice + 1; i < 10; i++)
                {
                    
                    Ejercicio ejercicio = contenidoManager.ObtenerEjercicioAleatorio(curso, DatosPartida.temas[i].nombre, dificultad);
                    DatosPartida.ejercicios[i] = ejercicio;
                    Debug.Log("Dificultad actualizada " + dificultad);
                }
            }
            VariablesGlobales.Instance.difucultad = dificultad;
        }
    }
}
