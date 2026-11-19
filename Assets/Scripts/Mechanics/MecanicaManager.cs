using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class MecanicaManager : MonoBehaviour
{
    public MecanicaSucesiones mecanicaSucesiones;
    public MecanicaMCD mecanicaMCD;
    public MecanicaMCM mecanicaMCM;

    private GameObject doorOpen;
    public Ejercicio ejercicio;
    public DDAManager ddaManager;
    private Animator animator;
    private bool esperaCargaMecanica = false;
    public SpawnObjects bloquesExtra;

    private Coroutine cronometro;

    private void Start()
    {

    }

    void Update()
    {
        AbrirPuerta();
    }

    public void CargarMecanica(int indice, BoxCollider2D spawnZonaColocacion,
                                BoxCollider2D spawnBloque, BoxCollider2D spawnRespuesta, 
                                BoxCollider2D spawnCuadrillaMCM, BoxCollider2D spawnProcedimientoMCM, GameObject door)
    {
        if (DatosPartida.ejercicios == null ||
            indice >= DatosPartida.ejercicios.Count)
        {
            Debug.LogWarning("Ejercicio no disponible.");
            return;
        }
        VariablesGlobales.Instance.indiceActual = indice;
        ejercicio = DatosPartida.ejercicios[indice];
        string tema = DatosPartida.temas[indice].nombre;
        doorOpen = door;

        Debug.Log("Cargando ejercicio: " + ejercicio.enunciado);
        Debug.Log("Tema: " + tema);

        switch (tema)
        {
            case "Sucesiones":
                CargarSucesiones(ejercicio, spawnZonaColocacion, spawnBloque, spawnRespuesta);
                break;

            case "Fracciones":
                CargarFracciones(ejercicio);
                break;

            case "MCD":
                CargarMCD(ejercicio, spawnZonaColocacion, spawnBloque, spawnRespuesta);
                break;
            
            case "MCM":
                CargarMCM(ejercicio,spawnCuadrillaMCM,spawnProcedimientoMCM);
                break;

            default:
                Debug.LogWarning("No existe mecánica para: " + tema);
                break;
        }
        esperaCargaMecanica = true;
    }

    public void IniciarTiempoEjercicio(int indice)
    {
        DatosPartida.retroEjercicio[indice].tiempo = 0f;
        cronometro = StartCoroutine(Cronometro(indice));
    }

    private IEnumerator Cronometro(int indice)
    {
        while (true)
        {
            DatosPartida.retroEjercicio[indice].tiempo += Time.deltaTime;
            yield return null;
        }
    }

    private void DetenerTiempoEjercicio()
    {
        if(cronometro != null)
        {
            StopCoroutine(cronometro);
            cronometro = null;
        }
    }

    private void CargarSucesiones(Ejercicio ejercicio, BoxCollider2D spawnZonaColocacion,
                                BoxCollider2D spawnBloque, BoxCollider2D spawnRespuesta)
    {
        mecanicaSucesiones.CargarEjercicio(ejercicio, spawnZonaColocacion, spawnBloque, spawnRespuesta);
    }

    private void CargarFracciones(Ejercicio ejercicio)
    {
        Debug.Log("→ Mecánica de Fracciones");
    }

    private void CargarMCD(Ejercicio ejercicio, BoxCollider2D spawnZonaColocacion,
                                BoxCollider2D spawnBloque, BoxCollider2D spawnRespuesta)
    {
        mecanicaMCD.CargarEjercicio(ejercicio, spawnZonaColocacion, spawnBloque, spawnRespuesta);
    }

    private void CargarMCM(Ejercicio ejercicio, BoxCollider2D spawnCuadrilla, BoxCollider2D spawnProcedimiento)
    {
        mecanicaMCM.CargarEjercicio(ejercicio,spawnCuadrilla,spawnProcedimiento);
        VariablesGlobales.Instance.indiceFactorMCM = 0;
    }

    public void AbrirPuerta()
    {
        if (esperaCargaMecanica)
        {
            int cantidadNecesaria;

            if (DatosPartida.temas[VariablesGlobales.Instance.indiceActual].nombre == "MCD")
            {
                cantidadNecesaria = ejercicio.respuestaCorrecta.Length;
            }else if (DatosPartida.temas[VariablesGlobales.Instance.indiceActual].nombre == "MCM")
            {
                cantidadNecesaria = VariablesGlobales.Instance.factoresMCM.Length;
            }
            else
            {
                cantidadNecesaria = ejercicio.elementos.Length + ejercicio.respuestaCorrecta.Length;
            }

            if (VariablesGlobales.Instance.cantCorrectos == cantidadNecesaria)
            {
                BoxCollider2D collider = doorOpen.GetComponent<BoxCollider2D>();
                bloquesExtra.EliminarBloquesExtra();
                animator = doorOpen.GetComponent<Animator>();
                animator.SetBool("isOpen",true);
                int indice = VariablesGlobales.Instance.indiceActual;
                DetenerTiempoEjercicio();
                VariablesGlobales.Instance.ActualizarRacha(DatosPartida.ejercicios[indice],DatosPartida.retroEjercicio[indice]);
                DDAManager.Desempeno desempeno = ddaManager.EvaluarDesempeno(DatosPartida.ejercicios[indice],DatosPartida.retroEjercicio[indice],VariablesGlobales.Instance.racha,true);
                DDAManager.Accion accion = ddaManager.ObtenerAccion(desempeno);
                ddaManager.EjecutarDDA(accion);

                collider.enabled = false;
                VariablesGlobales.Instance.cantCorrectos = 0;
                VariablesGlobales.Instance.indiceFactorMCM = 0;
                VariablesGlobales.Instance.factoresMCM= new int[0];
            }
        }

    }

    
}