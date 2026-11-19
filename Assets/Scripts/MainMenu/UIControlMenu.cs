using UnityEngine;
using UnityEngine.UI;
using UnityFileDialog;
using UnityEngine.Networking;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class UIControlMenu : MonoBehaviour
{
    public GameObject[] panels;
    public ContenidoManager contenidoManager;
    public GameObject MenuNuevoJuego;
    public Transform[] contentPanels;
    public TMP_Text cursoSeleccionado;
    public GameObject panelCargadorContenido;
    public GameObject panelDocente;
    public string urlContenido;
    private int idCurso;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnButtonPressed(int id)
    {
        idCurso = id;
        DeseleccionarToggle(id);
        Debug.Log("Button Pressed" + id);
        if (id == 0) //Geometria
        {
            ShowPanel(0);
            cursoSeleccionado.text = "Geometría";
        }
        else if (id == 1) //Álgebra
        {
            ShowPanel(1);
            cursoSeleccionado.text = "Álgebra";
        }
        else if (id == 2)//Aritmetica
        {
            ShowPanel(2);
            cursoSeleccionado.text = "Aritmética";
        }
        else if (id == 3)//Razonamiento Matemático
        {
            ShowPanel(3);
            cursoSeleccionado.text = "Trigonometría";
        }
    }

    public void OnPressedContinue()
    {
        Debug.Log("Continue Pressed");
        ObtenerSeleccionados(idCurso);
    }

    public void ShowPanel(int id)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == id)
            {
                panels[i].SetActive(true);
            }
            else
            {
                panels[i].SetActive(false);
            }
        }
    }

    public void OnPressedBack()
    {
        MenuNuevoJuego.SetActive(false);
    }

    public void OnPressedNewGame()
    {
        MenuNuevoJuego.SetActive(true);
    }

    public void ObtenerSeleccionados(int id)
    {
        List<string> seleccionados = new List<string>();
        foreach (Transform child in contentPanels[id])
        {
            Toggle toggle = child.GetComponent<Toggle>();
            if (toggle != null && toggle.isOn)
            {
                seleccionados.Add(child.name);
                Debug.Log("Seleccionado: " + child.name);
            }
        }
        string curso = "";

        if (id == 0)
            curso = "Geometría";
        else if (id == 1)
            curso = "Álgebra";
        else if (id == 2)
            curso = "Aritmética";
        else if (id == 3)
            curso = "Trigonometría";

        VariablesGlobales.Instance.curso = curso;

        contenidoManager.CrearListaInicial(curso, seleccionados, 10, 1);

    }

    public void DeseleccionarToggle(int id)
    {
        foreach (Transform content in contentPanels)
        {
            if (content == contentPanels[id])
            {
                continue;
            }
            foreach (Transform child in content)
            {
                Toggle toggle = child.GetComponent<Toggle>();
                if (toggle != null)
                {
                    toggle.isOn = false;
                }
            }
        }

    }

    public void CargarContenido()
    {
        panelCargadorContenido.SetActive(true);
    }
    public void CerrarCargarContenido()
    {
        panelCargadorContenido.SetActive(false);
    }

    public void CargarContenidoManual()
    {
        string ruta = FileBrowser.PickFile(
            directory: Application.persistentDataPath,
            filterName: "Archivos JSON",
            filterExtensions: new[] { "json" },
            title: "Seleccionar contenido"
        );

        if (!string.IsNullOrEmpty(ruta))
        {
            Debug.Log("Archivo seleccionado: " + ruta);

            contenidoManager.CargarJSON(ruta);
        }
        else
        {
            Debug.Log("No se seleccionó ningún archivo.");
        }
        panelCargadorContenido.SetActive(false);
    }

    public void CargarContenidoInternet()
    {
        Debug.Log("Cargando de Internet");
        StartCoroutine(DescargarJSON(urlContenido));
        Debug.Log("Descargado");
        panelCargadorContenido.SetActive(false);
    }

    public void CargarContenidoDirecto()
    {
        Debug.Log("Abriendo Interfaz Docente de Carga de Contenidos");
        if (panelDocente != null)
        {
            panelDocente.SetActive(true);
            var controller = panelDocente.GetComponent<DocenteController>();
            if (controller != null)
            {
                controller.Inicializar();
            }
        }
        if (panelCargadorContenido != null)
        {
            panelCargadorContenido.SetActive(false);
        }
    }

    public void CerrarPanelDocente()
    {
        if (panelDocente != null)
        {
            panelDocente.SetActive(false);
        }
        if (panelCargadorContenido != null)
        {
            panelCargadorContenido.SetActive(true);
        }
    }

    private IEnumerator DescargarJSON(string url)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(
                    "Error al descargar JSON: " + request.error
                );

                yield break;
            }

            string json = request.downloadHandler.text;

            Debug.Log("JSON descargado correctamente.");

            // Procesamos el JSON descargado
            Debug.Log("CONTENIDO RECIBIDO:");
            Debug.Log(json);
            contenidoManager.CargarJSONDesdeTexto(json);
        }
    }
}
