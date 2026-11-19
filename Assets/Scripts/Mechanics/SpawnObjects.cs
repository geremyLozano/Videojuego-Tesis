using System.Collections.Generic;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{

    public GameObject bloquePrefab;
    public GameObject zonaPrefab;
    public GameObject zonaPrefab2;

    public float espacioZonas = 2f;
    public float distanciaMinimaBloques = 1.0f;

    private List<int> valores = new List<int>();
    private List<int> valoresExtra = new List<int>();
    private List<GameObject> bloquesExtra = new List<GameObject>();

    public void SpawnZonaColocacion(int cantidad, float inicioX, Bounds bounds, Ejercicio ejercicio, bool esMCD)
    {
        if (esMCD)
        {
            // Una sola zona para todas las respuestas
            ZonaColocacion zona = Instantiate(
                zonaPrefab2,
                bounds.center,
                Quaternion.identity
            ).GetComponent<ZonaColocacion>();

            zona.valoresEsperados = ejercicio.respuestaCorrecta;
        }
        else
        {
            // Zonas normales
            for (int i = 0; i <= cantidad; i++)
            {
                Vector3 posicionZona = new Vector3(
                    inicioX + i * espacioZonas,
                    bounds.center.y,
                    0f
                );

                ZonaColocacion zona = Instantiate(zonaPrefab,posicionZona,Quaternion.identity
                                        ).GetComponent<ZonaColocacion>();

                if (i < cantidad)
                {
                    zona.valoresEsperados = new float[]
                    {
                    ejercicio.elementos[i]
                    };
                }
                else
                {
                    zona.valoresEsperados = ejercicio.respuestaCorrecta;
                }
            }
        }
    }

    public void SpawnBloques(int cantidad, Bounds boundsBloques, int[] ejercicio, List<Vector3> posicionesUsadas)
    {
        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicion = ObtenerPosicionValida(
                boundsBloques,
                posicionesUsadas
            );

            CrearBloque(ejercicio[i], posicion,false);
        }
    }

    public void SpawnRespuesta(Bounds boundsRespuesta, Ejercicio ejercicio, List<Vector3> posicionesUsadas)
    {
        valores.Clear();
        valoresExtra.Clear();
        // Agregar respuestas correctas
        foreach (float respuesta in ejercicio.respuestaCorrecta)
        {
            valores.Add((int)respuesta);
        }

        // Generar 3 distractores
        while (valores.Count < ejercicio.respuestaCorrecta.Length + 3)
        {
            int distractor = Random.Range(1, 20);

            // No repetir
            if (valores.Contains(distractor))
                continue;

            // No usar números que ya están en SpawnBloques
            bool usadoEnBloques = false;

            foreach (int elemento in ejercicio.elementos)
            {
                if (elemento == distractor)
                {
                    usadoEnBloques = true;
                    break;
                }
            }

            if (!usadoEnBloques)
            {
                valores.Add(distractor);
                valoresExtra.Add(distractor);
            }

        }

        // Spawn de los bloques
        foreach (int valor in valores)
        {
            Vector3 posicion = ObtenerPosicionValida(
                boundsRespuesta,
                posicionesUsadas
            );
            bool esExtra = valoresExtra.Contains(valor);
            CrearBloque(valor, posicion,esExtra);
        }

    }

    public void CrearBloque(int valor, Vector3 posicion, bool esExtra)
    {
        GameObject bloque = Instantiate(bloquePrefab,posicion,Quaternion.identity);

        BloqueNumero bloqueNumero = bloque.GetComponent<BloqueNumero>();
        bloqueNumero.valor = valor;

        bloque.GetComponentInChildren<TMPro.TMP_Text>().text =
            valor.ToString();

        if (esExtra)
        {
            bloquesExtra.Add(bloque);
        }
    }

    private Vector3 ObtenerPosicionValida(Bounds bounds, List<Vector3> posicionesUsadas)
    {
        Vector3 posicion;
        bool posicionValida;

        do
        {
            float x = Random.Range(bounds.min.x, bounds.max.x);
            float y = Random.Range(bounds.min.y, bounds.max.y);

            posicion = new Vector3(x, y, 0f);

            posicionValida = true;

            foreach (Vector3 usada in posicionesUsadas)
            {
                if (Vector3.Distance(posicion, usada) < distanciaMinimaBloques)
                {
                    posicionValida = false;
                    break;
                }
            }

        } while (!posicionValida);

        posicionesUsadas.Add(posicion);
        return posicion;
    }

    public void EliminarBloquesExtra()
    {
        foreach(GameObject bloque in bloquesExtra)
        {
            if (bloque != null)
            {
                Destroy(bloque);
            }
        }
        bloquesExtra.Clear();
    }
}