using System.Collections.Generic;
using UnityEngine;

public class MecanicaSucesiones : MonoBehaviour
{
    public SpawnObjects spawner;

    public float espacioZonas = 2f;
    public float distanciaMinimaBloques = 1.0f;

    public void CargarEjercicio(Ejercicio ejercicio,BoxCollider2D spawnZonaColocacion, 
                                BoxCollider2D spawnBloque, BoxCollider2D spawnRespuesta)
    {
        Debug.Log("Cargando sucesion:");
        Debug.Log(ejercicio.enunciado);

        Bounds bounds = spawnZonaColocacion.bounds;
        int cantidad = ejercicio.elementos.Length;

        float anchoTotal = (cantidad - 1) * espacioZonas;
        float inicioX = bounds.center.x - anchoTotal / 2f;
        
        
        spawner.SpawnZonaColocacion(cantidad, inicioX, bounds,ejercicio,false);
        Bounds boundsBloques = spawnBloque.bounds;
        List<Vector3> posicionesUsadas = new List<Vector3>();
        spawner.SpawnBloques(cantidad, boundsBloques, ejercicio.elementos, posicionesUsadas);
        Bounds boundsRespuesta = spawnRespuesta.bounds;
        spawner.SpawnRespuesta(boundsRespuesta, ejercicio, posicionesUsadas);
    }


}