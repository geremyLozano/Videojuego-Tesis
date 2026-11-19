using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MecanicaMCM : MonoBehaviour
{
    public GameObject bloquePrefab;
    public float espacioZonas = 1f;
    private List<GameObject> bloquesProcedimiento = new List<GameObject>();
    public float distanciaMinimaBloques = 1.0f;
    private int[] valoresProcedimiento;
    private int filaProcedimiento = 0;
    private BoxCollider2D spawnProcedimientoActual;

    public void CargarEjercicio(Ejercicio ejercicio, BoxCollider2D spawnCuadrilla,
                                BoxCollider2D spawnProcedimiento)
    {
        Debug.Log("Cargando MCMC:");
        Debug.Log(ejercicio.enunciado);

        GenerarCuadricula(ejercicio, spawnCuadrilla);
        GenerarProcedimiento(ejercicio, spawnProcedimiento);
        List<int> factores = ObtenerFactoresMCM(ejercicio.elementos);
        List<Celda> camino = new List<Celda>();
        foreach (int factor in factores)
        {
            Debug.Log("Factor MCM: " + factor);
        }
    }

    private List<int> ObtenerFactoresMCM(int[] numeros)
    {
        List<int> factores = new List<int>();

        int[] copia = new int[numeros.Length];
        numeros.CopyTo(copia, 0);

        int divisor = 2;

        while (true)
        {
            bool sePudoDividir = false;

            for (int i = 0; i < copia.Length; i++)
            {
                if (copia[i] % divisor == 0)
                {
                    copia[i] /= divisor;
                    sePudoDividir = true;
                }
            }

            if (sePudoDividir)
            {
                factores.Add(divisor);
            }
            else
            {
                divisor++;
            }

            bool terminado = true;

            for (int i = 0; i < copia.Length; i++)
            {
                if (copia[i] > 1)
                {
                    terminado = false;
                    break;
                }
            }

            if (terminado)
                break;
        }


        VariablesGlobales.Instance.factoresMCM = factores.ToArray();
        foreach (int factor in VariablesGlobales.Instance.factoresMCM)
        {
            Debug.Log("Factores guardados: " + factor);
        }


        return factores;
    }

    private void GenerarCuadricula(Ejercicio ejercicio, BoxCollider2D spawnCuadricula)
    {
        List<int> factores =
            ObtenerFactoresMCM(ejercicio.elementos);

        int tamaño =
            ObtenerTamañoCuadricula(factores.Count);

        List<Celda> camino =
            GenerarCamino(tamaño, factores.Count);

        if (camino == null)
        {
            Debug.LogError("No se pudo generar un camino.");
            return;
        }

        Bounds bounds = spawnCuadricula.bounds;

        float espacio = 1.2f;
        float margen = 0.6f;
        float inicioX = bounds.min.x + margen;
        float inicioY = bounds.max.y - margen;

        for (int y = 0; y < tamaño; y++)
        {
            for (int x = 0; x < tamaño; x++)
            {
                Vector3 posicion = new Vector3(
                    inicioX + x * espacio,
                    inicioY - y * espacio,
                    0f
                );

                int valor;

                int indiceCamino = -1;

                for (int i = 0; i < camino.Count; i++)
                {
                    if (camino[i].x == x &&
                        camino[i].y == y)
                    {
                        indiceCamino = i;
                        break;
                    }
                }

                if (indiceCamino != -1)
                {
                    valor = factores[indiceCamino];
                }
                else
                {
                    valor = ObtenerValorDistractor(factores);
                }

                // Buscar si esta celda pertenece al camino

                CrearBloque(valor, posicion);
            }
        }
    }

    public void CrearBloque(int valor, Vector3 posicion)
    {
        GameObject bloque = Instantiate(bloquePrefab, posicion, Quaternion.identity);

        BloqueNumeroMCM bloqueNumero = bloque.GetComponent<BloqueNumeroMCM>();
        bloqueNumero.valor = valor;

        bloque.GetComponentInChildren<TMPro.TMP_Text>().text =
            valor.ToString();
    }

    private int ObtenerTamañoCuadricula(int cantidadFactores)
    {
        if (cantidadFactores <= 2)
            return 2;
        if (cantidadFactores == 3 || cantidadFactores == 4)
            return 3;

        if (cantidadFactores == 5 || cantidadFactores == 6)
            return 4;

        return 5;
    }

    private void GenerarProcedimiento(Ejercicio ejercicio, BoxCollider2D spawnProcedimiento)
    {
        spawnProcedimientoActual = spawnProcedimiento;
        Bounds bounds = spawnProcedimiento.bounds;
        int cantidad = ejercicio.elementos.Length;
        valoresProcedimiento = new int[cantidad];
        for (int j = 0; j < cantidad; j++)
        {
            valoresProcedimiento[j] = ejercicio.elementos[j];
        }
        float margen = 0.6f;
        float inicioX = bounds.min.x + margen;
        float inicioY = bounds.max.y - margen;

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 posicion = new Vector3(inicioX + i * espacioZonas, inicioY, 0f);

            GameObject bloque = Instantiate(bloquePrefab, posicion, Quaternion.identity);

            BloqueNumeroMCM bloqueNumero = bloque.GetComponent<BloqueNumeroMCM>();
            bloqueNumero.enabled = false;

            bloque.GetComponentInChildren<TMPro.TMP_Text>().text = ejercicio.elementos[i].ToString();
            Collider2D collider = bloque.GetComponent<Collider2D>();

            if (collider != null)
                collider.enabled = false;

            bloquesProcedimiento.Add(bloque);
        }
    }

    private struct Celda
    {
        public int x;
        public int y;

        public Celda(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Celda))
                return false;

            Celda otra = (Celda)obj;

            return x == otra.x && y == otra.y;
        }

        public override int GetHashCode()
        {
            return x * 31 + y;
        }
    }

    private bool BuscarCamino(
    int tamaño,
    int cantidadPasos,
    List<Celda> camino)
    {
        // Ya tenemos la cantidad necesaria
        if (camino.Count == cantidadPasos)
        {
            Celda actual = camino[camino.Count - 1];

            return EsBorde(actual, tamaño);
        }

        Celda actualCelda =
            camino[camino.Count - 1];

        List<Celda> movimientos = new List<Celda>()
    {
        new Celda(actualCelda.x + 1, actualCelda.y),
        new Celda(actualCelda.x - 1, actualCelda.y),
        new Celda(actualCelda.x, actualCelda.y + 1),
        new Celda(actualCelda.x, actualCelda.y - 1)
    };

        // Mezclamos los movimientos para que
        // el camino no sea siempre igual
        for (int i = 0; i < movimientos.Count; i++)
        {
            int j = Random.Range(i, movimientos.Count);

            Celda temporal = movimientos[i];
            movimientos[i] = movimientos[j];
            movimientos[j] = temporal;
        }

        foreach (Celda siguiente in movimientos)
        {
            // Fuera de la cuadrícula
            if (siguiente.x < 0 ||
                siguiente.x >= tamaño ||
                siguiente.y < 0 ||
                siguiente.y >= tamaño)
            {
                continue;
            }

            // Ya utilizamos esta celda
            if (camino.Contains(siguiente))
            {
                continue;
            }

            camino.Add(siguiente);

            if (BuscarCamino(
                tamaño,
                cantidadPasos,
                camino))
            {
                return true;
            }

            // Este camino no funcionó,
            // retrocedemos
            camino.RemoveAt(camino.Count - 1);
        }

        return false;
    }

    private List<Celda> GenerarCamino(int tamaño, int cantidadPasos)
    {
        List<Celda> camino = new List<Celda>();

        // Elegimos una entrada aleatoria en un borde
        int borde = Random.Range(0, 4);

        Celda inicio = new Celda(0, Random.Range(0, tamaño));


        camino.Add(inicio);

        bool encontrado = BuscarCamino(
            tamaño,
            cantidadPasos,
            camino
        );

        if (encontrado)
            return camino;

        return null;
    }

    private bool EsBorde(Celda celda, int tamaño)
    {
        return celda.x == 0 ||
               celda.x == tamaño - 1 ||
               celda.y == 0 ||
               celda.y == tamaño - 1;
    }

    private int ObtenerValorDistractor(List<int> factores)
    {
        int[] candidatos =
        {
        2, 3, 4, 5, 6, 7, 8, 9, 10
    };

        List<int> disponibles = new List<int>();

        foreach (int candidato in candidatos)
        {
            if (!factores.Contains(candidato))
            {
                disponibles.Add(candidato);
            }
        }

        if (disponibles.Count == 0)
            return 1;

        return disponibles[
            Random.Range(0, disponibles.Count)
        ];
    }

    public void AplicarFactor(int factor)
    {
        AgregarFactorAFila(factor);
        for (int i = 0; i < valoresProcedimiento.Length; i++)
        {
            if (valoresProcedimiento[i] % factor == 0)
            {
                valoresProcedimiento[i] /= factor;
            }
        }
        filaProcedimiento++;
        CrearFilaProcedimiento(factor);
    }

    private void CrearFilaProcedimiento(int factor)
    {
        Bounds bounds = spawnProcedimientoActual.bounds;

        float margen = 0.6f;

        float inicioX = bounds.min.x + margen;

        float inicioY = bounds.max.y - margen - filaProcedimiento * espacioZonas;

        for (int i = 0; i < valoresProcedimiento.Length; i++)
        {
            Vector3 posicion = new Vector3(
                inicioX + i * espacioZonas,
                inicioY,
                0f
            );

            CrearBloqueProcedimiento(
                valoresProcedimiento[i],
                posicion
            );
        }

    }

    private GameObject CrearBloqueProcedimiento(int valor, Vector3 posicion)
    {
        GameObject bloque = Instantiate(
            bloquePrefab,
            posicion,
            Quaternion.identity
        );

        bloque.GetComponentInChildren<TMPro.TMP_Text>().text =
            valor.ToString();

        Collider2D collider =
            bloque.GetComponent<Collider2D>();

        if (collider != null)
            collider.enabled = false;

        bloquesProcedimiento.Add(bloque);

        return bloque;
    }

    private void CrearSeparador(Vector3 posicion)
    {
        GameObject separador = Instantiate(
            bloquePrefab,
            posicion,
            Quaternion.identity
        );

        separador.GetComponentInChildren<TMPro.TMP_Text>().text = "|";

        SpriteRenderer sprite =
            separador.GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
            sprite.enabled = false;

        Collider2D collider =
            separador.GetComponent<Collider2D>();

        if (collider != null)
            collider.enabled = false;

        separador.GetComponent<BloqueNumeroMCM>().enabled = false;

        bloquesProcedimiento.Add(separador);
    }

    private void AgregarFactorAFila(int factor)
    {
        Bounds bounds = spawnProcedimientoActual.bounds;

        float margen = 0.6f;

        float inicioX = bounds.min.x + margen;

        float inicioY =
            bounds.max.y -
            margen -
            filaProcedimiento * espacioZonas;

        Vector3 posicionSeparador = new Vector3(
            inicioX + valoresProcedimiento.Length * espacioZonas - 0.6f,
            inicioY,
            0f
        );

        CrearSeparador(posicionSeparador);

        Vector3 posicionFactor = new Vector3(
            inicioX + valoresProcedimiento.Length * espacioZonas,
            inicioY,
            0f
        );

        CrearBloqueProcedimiento(
            factor,
            posicionFactor
        );
    }

}