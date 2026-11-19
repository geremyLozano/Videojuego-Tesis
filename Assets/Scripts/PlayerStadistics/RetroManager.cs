using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RetroManager : MonoBehaviour
{
    public GameObject retro;
    public TMP_Text erroresTotalesTexto;
    public TMP_Text tiempoTotalTexto;
    public TMP_Text tiempoPromedioTexto;

    public RetroFinal retroFinal = new RetroFinal();

    public GameObject filaPrefab;
    public Transform contenedorTabla;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            retro.SetActive(true);
            MostrarRetroFinal();
        }
    }

    public void MostrarRetroFinal()
    {
        retroFinal.erroresTotales = 0;
        retroFinal.tiempoTotal = 0f;

        for (int i = 0; i < DatosPartida.retroEjercicio.Count; i++)
        {
            retroFinal.erroresTotales +=
                DatosPartida.retroEjercicio[i].cantIntentos;

            retroFinal.tiempoTotal +=
                DatosPartida.retroEjercicio[i].tiempo;
        }

        if (DatosPartida.retroEjercicio.Count > 0)
        {
            retroFinal.tiempoPromedioXEjercicio =
                retroFinal.tiempoTotal /
                DatosPartida.retroEjercicio.Count;
        }

        erroresTotalesTexto.text =
            "Errores totales: " + retroFinal.erroresTotales;

        tiempoTotalTexto.text =
            "Tiempo total: " + FormatearTiempo(retroFinal.tiempoTotal);

        tiempoPromedioTexto.text =
            "Tiempo promedio: " +
            FormatearTiempo(retroFinal.tiempoPromedioXEjercicio);
        MostrarTablaPorTema();
    }

    private string FormatearTiempo(float segundos)
    {
        int minutos = Mathf.FloorToInt(segundos / 60f);
        int segundosRestantes = Mathf.FloorToInt(segundos % 60f);

        return minutos.ToString("00") + ":" +
               segundosRestantes.ToString("00");
    }

    private void MostrarTablaPorTema()
    {
        foreach (Transform hijo in contenedorTabla)
        {
            Destroy(hijo.gameObject);
        }

        List<string> temasMostrados = new List<string>();

        for (int i = 0; i < DatosPartida.temas.Count; i++)
        {
            string nombreTema = DatosPartida.temas[i].nombre;

            // Si ya mostramos este tema, no creamos otra fila
            if (temasMostrados.Contains(nombreTema))
            {
                continue;
            }

            temasMostrados.Add(nombreTema);

            int cantidadEjercicios = 0;
            int errores = 0;
            float tiempoTotal = 0f;

            // Buscar todos los ejercicios de este tema
            for (int j = 0; j < DatosPartida.temas.Count; j++)
            {
                if (DatosPartida.temas[j].nombre == nombreTema)
                {
                    cantidadEjercicios++;

                    errores +=
                        DatosPartida.retroEjercicio[j].cantIntentos;

                    tiempoTotal +=
                        DatosPartida.retroEjercicio[j].tiempo;
                }
            }

            float tiempoPromedio = 0f;

            if (cantidadEjercicios > 0)
            {
                tiempoPromedio =
                    tiempoTotal / cantidadEjercicios;
            }

            GameObject nuevaFila =
                Instantiate(filaPrefab, contenedorTabla);

            FilaRetroTema fila =
                nuevaFila.GetComponent<FilaRetroTema>();

            fila.Configurar(nombreTema, cantidadEjercicios, errores,tiempoPromedio);
        }
    }
}