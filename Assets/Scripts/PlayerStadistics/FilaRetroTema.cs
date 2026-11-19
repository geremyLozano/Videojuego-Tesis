using UnityEngine;
using TMPro;

public class FilaRetroTema : MonoBehaviour
{
    public TMP_Text textoTema;
    public TMP_Text textoEjercicios;
    public TMP_Text textoErrores;
    public TMP_Text textoTiempo;

    public void Configurar(string tema,int ejercicios, int errores, float tiempoPromedio)
    {
        textoTema.text = tema;
        textoEjercicios.text = ejercicios.ToString();
        textoErrores.text = errores.ToString();
        textoTiempo.text = FormatearTiempo(tiempoPromedio);
    }

    private string FormatearTiempo(float segundos)
    {
        int minutos = Mathf.FloorToInt(segundos / 60f);
        int segundosRestantes = Mathf.FloorToInt(segundos % 60f);

        return minutos.ToString("00") + ":" +
               segundosRestantes.ToString("00");
    }
}