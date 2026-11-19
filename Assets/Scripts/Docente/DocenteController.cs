using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocenteController : MonoBehaviour
{
    [Header("Referencias Principales")]
    public ContenidoManager contenidoManager;
    public UIControlMenu uiControlMenu;

    [Header("Vistas")]
    public GameObject panelSeleccion;
    public GameObject panelFormularios;

    [Header("Paso 1: Selección de Curso y Temas")]
    public TMP_Dropdown dropdownCurso;
    public Transform containerTemas;
    public GameObject prefabTopicItem;
    public Button btnContinuarAFormulario;
    public Button btnCerrarSeleccion;
    public TMP_Text textoErrorSeleccion;

    [Header("Paso 2: Formulario de Ejercicios")]
    public TMP_Text textoResumenSesion;
    public Button btnVolverASeleccion;
    public Transform containerEjercicios;
    public GameObject prefabEjercicioItem;
    public ScrollRect scrollEjercicios;
    public Button btnGuardar;
    public Button btnCancelarFormulario;
    public TMP_Text textoEstadoGuardado;
    public GameObject indicadorProgreso;

    [Header("Modal de Advertencia de Duplicado")]
    public GameObject panelModalDuplicado;
    public TMP_Text textoTituloDuplicado;
    public TMP_Text textoCursoTemaDuplicado;
    public TMP_Text textoExistente;
    public TMP_Text textoNuevo;
    public TMP_Text textoDiferencias;
    public Button btnCancelarEsteEjercicio;
    public Button btnAgregarDeTodasFormas;

    [Header("Modal de Mensajes")]
    public GameObject panelModalMensaje;
    public TMP_Text textoTituloMensaje;
    public TMP_Text textoCuerpoMensaje;
    public Button btnAceptarMensaje;

    // Estado interno
    private readonly string[] cursosPredefinidos = { "Geometría", "Álgebra", "Aritmética", "Trigonometría" };
    private Dictionary<string, List<string>> temasPorCurso = new Dictionary<string, List<string>>();
    private List<DocenteTopicItemUI> topicItemsInstanciados = new List<DocenteTopicItemUI>();
    private List<DocenteEjercicioItemUI> ejercicioItemsInstanciados = new List<DocenteEjercicioItemUI>();

    // Estado para resolución de duplicados en corrutina
    private bool esperandoDecisionDuplicado = false;
    private bool decisionDuplicadoAprobada = false;

    void Awake()
    {
        if (contenidoManager == null)
        {
            contenidoManager = FindAnyObjectByType<ContenidoManager>();
        }
        if (uiControlMenu == null)
        {
            uiControlMenu = FindAnyObjectByType<UIControlMenu>();
        }

        ConfigurarBotones();
    }

    public void Inicializar()
    {
        if (contenidoManager == null)
        {
            contenidoManager = FindAnyObjectByType<ContenidoManager>();
        }
        if (uiControlMenu == null)
        {
            uiControlMenu = FindAnyObjectByType<UIControlMenu>();
        }

        if (contenidoManager != null)
        {
            contenidoManager.CargarGuardados();
        }

        CargarEstructuraCursosYTemas();
        ConfigurarDropdownCursos();
        MostrarPasoSeleccion();

        if (panelModalDuplicado != null) panelModalDuplicado.SetActive(false);
        if (panelModalMensaje != null) panelModalMensaje.SetActive(false);
        if (indicadorProgreso != null) indicadorProgreso.SetActive(false);
        if (textoEstadoGuardado != null) textoEstadoGuardado.text = string.Empty;
    }

    private void DestruirHijos(Transform parent)
    {
        if (parent == null) return;
        List<GameObject> lista = new List<GameObject>();
        foreach (Transform child in parent)
        {
            lista.Add(child.gameObject);
        }
        foreach (GameObject go in lista)
        {
            if (Application.isPlaying) Destroy(go);
            else DestroyImmediate(go);
        }
    }

    private void ConfigurarBotones()
    {
        if (btnContinuarAFormulario != null)
        {
            btnContinuarAFormulario.onClick.RemoveAllListeners();
            btnContinuarAFormulario.onClick.AddListener(OnContinuarAFormularioClick);
        }

        if (btnCerrarSeleccion != null)
        {
            btnCerrarSeleccion.onClick.RemoveAllListeners();
            btnCerrarSeleccion.onClick.AddListener(CerrarInterfaz);
        }

        if (btnVolverASeleccion != null)
        {
            btnVolverASeleccion.onClick.RemoveAllListeners();
            btnVolverASeleccion.onClick.AddListener(MostrarPasoSeleccion);
        }

        if (btnCancelarFormulario != null)
        {
            btnCancelarFormulario.onClick.RemoveAllListeners();
            btnCancelarFormulario.onClick.AddListener(CerrarInterfaz);
        }

        if (btnGuardar != null)
        {
            btnGuardar.onClick.RemoveAllListeners();
            btnGuardar.onClick.AddListener(OnGuardarClick);
        }

        if (btnCancelarEsteEjercicio != null)
        {
            btnCancelarEsteEjercicio.onClick.RemoveAllListeners();
            btnCancelarEsteEjercicio.onClick.AddListener(OnCancelarEsteEjercicioClick);
        }

        if (btnAgregarDeTodasFormas != null)
        {
            btnAgregarDeTodasFormas.onClick.RemoveAllListeners();
            btnAgregarDeTodasFormas.onClick.AddListener(OnAgregarDeTodasFormasClick);
        }

        if (btnAceptarMensaje != null)
        {
            btnAceptarMensaje.onClick.RemoveAllListeners();
            btnAceptarMensaje.onClick.AddListener(() =>
            {
                if (panelModalMensaje != null) panelModalMensaje.SetActive(false);
            });
        }
    }

    private void CargarEstructuraCursosYTemas()
    {
        temasPorCurso.Clear();

        // Obtener temas desde contentPanels de UIControlMenu
        if (uiControlMenu != null && uiControlMenu.contentPanels != null)
        {
            for (int i = 0; i < uiControlMenu.contentPanels.Length && i < cursosPredefinidos.Length; i++)
            {
                string curso = cursosPredefinidos[i];
                Transform panel = uiControlMenu.contentPanels[i];
                List<string> temas = new List<string>();

                if (panel != null)
                {
                    foreach (Transform child in panel)
                    {
                        string nombreTema = child.name.Trim();
                        if (!string.IsNullOrEmpty(nombreTema) && !temas.Contains(nombreTema))
                        {
                            temas.Add(nombreTema);
                        }
                    }
                }

                temasPorCurso[curso] = temas;
            }
        }

        // Si hay temas existentes en contenidos_guardados que correspondan a estos cursos, agregarlos si faltan
        if (contenidoManager != null && contenidoManager.contenidos != null)
        {
            foreach (Curso c in contenidoManager.contenidos)
            {
                if (c == null || string.IsNullOrEmpty(c.curso)) continue;
                if (!temasPorCurso.ContainsKey(c.curso))
                {
                    // Solo aceptar si es uno de los cursos predefinidos
                    bool predefinido = false;
                    foreach (string cp in cursosPredefinidos)
                    {
                        if (string.Equals(cp, c.curso, System.StringComparison.OrdinalIgnoreCase))
                        {
                            predefinido = true;
                            break;
                        }
                    }
                    if (predefinido)
                    {
                        temasPorCurso[c.curso] = new List<string>();
                    }
                }

                if (temasPorCurso.ContainsKey(c.curso) && c.temas != null)
                {
                    foreach (Tema t in c.temas)
                    {
                        if (t != null && !string.IsNullOrEmpty(t.nombre) && !temasPorCurso[c.curso].Contains(t.nombre))
                        {
                            temasPorCurso[c.curso].Add(t.nombre);
                        }
                    }
                }
            }
        }
    }

    private void ConfigurarDropdownCursos()
    {
        if (dropdownCurso == null) return;

        dropdownCurso.ClearOptions();
        List<string> opciones = new List<string>(cursosPredefinidos);
        dropdownCurso.AddOptions(opciones);
        dropdownCurso.onValueChanged.RemoveAllListeners();
        dropdownCurso.onValueChanged.AddListener(OnCursoSeleccionadoChanged);

        // Actualizar lista de temas para el curso inicial
        ActualizarListaTemas(dropdownCurso.value);
    }

    private void OnCursoSeleccionadoChanged(int index)
    {
        ActualizarListaTemas(index);
    }

    private void ActualizarListaTemas(int cursoIndex)
    {
        if (containerTemas == null || prefabTopicItem == null) return;

        // Limpiar items anteriores
        DestruirHijos(containerTemas);
        topicItemsInstanciados.Clear();

        if (cursoIndex < 0 || cursoIndex >= cursosPredefinidos.Length) return;
        string curso = cursosPredefinidos[cursoIndex];

        if (temasPorCurso.TryGetValue(curso, out List<string> temas))
        {
            foreach (string tema in temas)
            {
                GameObject go = Instantiate(prefabTopicItem, containerTemas);
                go.SetActive(true);
                DocenteTopicItemUI item = go.GetComponent<DocenteTopicItemUI>();
                if (item != null)
                {
                    item.Configurar(tema);
                    topicItemsInstanciados.Add(item);
                }
            }
        }

        if (textoErrorSeleccion != null)
        {
            textoErrorSeleccion.text = string.Empty;
        }
    }

    public void MostrarPasoSeleccion()
    {
        if (panelSeleccion != null) panelSeleccion.SetActive(true);
        if (panelFormularios != null) panelFormularios.SetActive(false);
    }

    public void OnContinuarAFormularioClick()
    {
        List<(string tema, int cantidad)> seleccionados = new List<(string, int)>();

        foreach (var item in topicItemsInstanciados)
        {
            if (item != null && item.EstaSeleccionado())
            {
                int cant = item.ObtenerCantidad();
                if (cant > 0)
                {
                    seleccionados.Add((item.temaNombre, cant));
                }
            }
        }

        if (seleccionados.Count == 0)
        {
            if (textoErrorSeleccion != null)
            {
                textoErrorSeleccion.text = "Debes seleccionar al menos un tema y especificar una cantidad válida.";
            }
            return;
        }

        if (textoErrorSeleccion != null)
        {
            textoErrorSeleccion.text = string.Empty;
        }

        GenerarFormulariosEjercicios(seleccionados);
    }

    private void GenerarFormulariosEjercicios(List<(string tema, int cantidad)> seleccionados)
    {
        if (containerEjercicios == null || prefabEjercicioItem == null) return;

        // Limpiar items previos
        DestruirHijos(containerEjercicios);
        ejercicioItemsInstanciados.Clear();

        string curso = cursosPredefinidos[dropdownCurso.value];
        int totalEjercicios = 0;
        foreach (var s in seleccionados) totalEjercicios += s.cantidad;

        if (textoResumenSesion != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"Curso: {curso} | Total: {totalEjercicios} ejercicios (");
            for (int i = 0; i < seleccionados.Count; i++)
            {
                sb.Append($"{seleccionados[i].tema}: {seleccionados[i].cantidad}");
                if (i < seleccionados.Count - 1) sb.Append(", ");
            }
            sb.Append(")");
            textoResumenSesion.text = sb.ToString();
        }

        foreach (var sel in seleccionados)
        {
            string tema = sel.tema;
            int cantidad = sel.cantidad;
            List<int> idsEnTema = new List<int>();

            for (int i = 1; i <= cantidad; i++)
            {
                int autoId = DocenteValidation.CalcularSiguienteId(contenidoManager, curso, tema, idsEnTema);
                idsEnTema.Add(autoId);

                GameObject go = Instantiate(prefabEjercicioItem, containerEjercicios);
                go.SetActive(true);
                DocenteEjercicioItemUI item = go.GetComponent<DocenteEjercicioItemUI>();
                if (item != null)
                {
                    item.Configurar(curso, tema, i, cantidad, autoId);
                    ejercicioItemsInstanciados.Add(item);
                }
            }
        }

        if (panelSeleccion != null) panelSeleccion.SetActive(false);
        if (panelFormularios != null) panelFormularios.SetActive(true);
        if (scrollEjercicios != null) scrollEjercicios.verticalNormalizedPosition = 1f;
    }

    public void OnGuardarClick()
    {
        StartCoroutine(GuardarContenidoRutina());
    }

    private IEnumerator GuardarContenidoRutina()
    {
        if (ejercicioItemsInstanciados.Count == 0)
        {
            MostrarMensajeModal("Sin Ejercicios", "No hay formularios de ejercicios para guardar.");
            yield break;
        }

        SetControlesBloqueados(true);
        MostrarEstado("Validando contenido...", true);

        // 1. Validar todos los formularios individualmente
        bool hayErrores = false;
        List<Ejercicio> ejerciciosValidados = new List<Ejercicio>();

        for (int i = 0; i < ejercicioItemsInstanciados.Count; i++)
        {
            DocenteEjercicioItemUI item = ejercicioItemsInstanciados[i];
            if (item == null) continue;

            if (!item.Validar(out Ejercicio ej, out string err))
            {
                hayErrores = true;
            }
            else
            {
                ejerciciosValidados.Add(ej);
            }
        }

        if (hayErrores)
        {
            SetControlesBloqueados(false);
            MostrarEstado("Existen errores en los formularios. Revísalos antes de guardar.", false);
            MostrarMensajeModal("Errores de Validación", "Se encontraron campos vacíos o valores inválidos. Revisa las notas rojas en cada ejercicio.");
            yield break;
        }

        // 2. Detección de posibles duplicados
        MostrarEstado("Comprobando posibles ejercicios duplicados...", true);
        string curso = cursosPredefinidos[dropdownCurso.value];

        List<Ejercicio> ejerciciosAprobados = new List<Ejercicio>();

        for (int i = 0; i < ejercicioItemsInstanciados.Count; i++)
        {
            DocenteEjercicioItemUI item = ejercicioItemsInstanciados[i];
            Ejercicio nuevo = item.ObtenerEjercicioDraft();

            // Si ya fue aprobado en un paso previo de esta sesión, no volver a preguntar
            if (item.aprobadoDuplicado)
            {
                ejerciciosAprobados.Add(nuevo);
                continue;
            }

            DocenteValidation.ComparacionDuplicado comp =
                DocenteValidation.DetectarDuplicado(contenidoManager, curso, item.tema, nuevo, ejerciciosAprobados);

            if (comp.esDuplicado)
            {
                // Mostrar modal de advertencia y esperar decisión explícita
                AbrirModalDuplicado(comp);
                esperandoDecisionDuplicado = true;

                yield return new WaitUntil(() => !esperandoDecisionDuplicado);

                if (decisionDuplicadoAprobada)
                {
                    item.aprobadoDuplicado = true;
                    ejerciciosAprobados.Add(nuevo);
                }
                else
                {
                    // Docente canceló este ejercicio para corregirlo
                    SetControlesBloqueados(false);
                    MostrarEstado($"Registro detenido: corrige el ejercicio '{nuevo.enunciado}' en el formulario.", false);
                    yield break;
                }
            }
            else
            {
                ejerciciosAprobados.Add(nuevo);
            }
        }

        // 3. Agrupar ejercicios por tema y asegurar IDs no conflictivos
        MostrarEstado("Procesando ejercicios y asignando identificadores...", true);
        Dictionary<string, List<Ejercicio>> porTema = new Dictionary<string, List<Ejercicio>>();

        foreach (var item in ejercicioItemsInstanciados)
        {
            if (!porTema.ContainsKey(item.tema))
            {
                porTema[item.tema] = new List<Ejercicio>();
            }

            Ejercicio ej = item.ObtenerEjercicioDraft();
            porTema[item.tema].Add(ej);
        }

        // Reasignar IDs definitivos sin conflicto
        foreach (var kvp in porTema)
        {
            string tema = kvp.Key;
            List<Ejercicio> lista = kvp.Value;
            List<int> idsOcupados = new List<int>();

            foreach (var ej in lista)
            {
                int finalId = DocenteValidation.CalcularSiguienteId(contenidoManager, curso, tema, idsOcupados);
                ej.id = finalId;
                idsOcupados.Add(finalId);
            }
        }

        // 4. Generar ContenidoJSON y persistir por cada tema
        MostrarEstado("Guardando cambios en contenidos_guardados.json...", true);
        yield return null;

        int totalGuardados = 0;
        StringBuilder detalleGuardado = new StringBuilder();

        foreach (var kvp in porTema)
        {
            string tema = kvp.Key;
            List<Ejercicio> lista = kvp.Value;

            ContenidoJSON cjson = new ContenidoJSON();
            cjson.curso = curso;
            cjson.tema = tema;
            cjson.ejercicios = lista;

            string jsonTexto = JsonUtility.ToJson(cjson, true);

            Debug.Log($"[Docente] Importando JSON para {curso} / {tema}:\n{jsonTexto}");

            contenidoManager.CargarJSONDesdeTexto(jsonTexto);

            totalGuardados += lista.Count;
            detalleGuardado.AppendLine($"• {tema}: {lista.Count} ejercicios.");
        }

        yield return new WaitForSeconds(0.2f);

        SetControlesBloqueados(false);
        MostrarEstado("¡Contenidos guardados con éxito!", false);

        string mensajeFinal = $"Se guardaron correctamente {totalGuardados} ejercicios en el curso '{curso}':\n\n{detalleGuardado}\nLos ejercicios ya están disponibles para las partidas.";
        MostrarMensajeModal("¡Guardado Exitoso!", mensajeFinal);

        // Regresar al paso de selección
        MostrarPasoSeleccion();
    }

    private void AbrirModalDuplicado(DocenteValidation.ComparacionDuplicado comp)
    {
        if (panelModalDuplicado == null) return;

        if (textoTituloDuplicado != null)
        {
            textoTituloDuplicado.text = "⚠️ Posible Ejercicio Duplicado Detectado";
        }

        if (textoCursoTemaDuplicado != null)
        {
            textoCursoTemaDuplicado.text = $"Curso: {comp.curso} | Tema: {comp.tema}";
        }

        if (textoExistente != null)
        {
            string elems = comp.existente.elementos != null ? string.Join(", ", comp.existente.elementos) : "-";
            string resps = comp.existente.respuestaCorrecta != null ? string.Join(", ", comp.existente.respuestaCorrecta) : "-";
            textoExistente.text = $"<b>EJERCICIO GUARDADO (ID: {comp.existente.id})</b>\n\n" +
                                  $"<b>Enunciado:</b> {comp.existente.enunciado}\n" +
                                  $"<b>Dificultad:</b> Nivel {comp.existente.dificultad}\n" +
                                  $"<b>Elementos:</b> [{elems}]\n" +
                                  $"<b>Respuesta(s):</b> [{resps}]\n" +
                                  $"<b>Intentos:</b> {comp.existente.maxIntentos} | <b>Tiempo:</b> {comp.existente.tiempoMaximo}s";
        }

        if (textoNuevo != null)
        {
            string elems = comp.nuevo.elementos != null ? string.Join(", ", comp.nuevo.elementos) : "-";
            string resps = comp.nuevo.respuestaCorrecta != null ? string.Join(", ", comp.nuevo.respuestaCorrecta) : "-";
            textoNuevo.text = $"<b>EJERCICIO NUEVO A REGISTRAR</b>\n\n" +
                              $"<b>Enunciado:</b> {comp.nuevo.enunciado}\n" +
                              $"<b>Dificultad:</b> Nivel {comp.nuevo.dificultad}\n" +
                              $"<b>Elementos:</b> [{elems}]\n" +
                              $"<b>Respuesta(s):</b> [{resps}]\n" +
                              $"<b>Intentos:</b> {comp.nuevo.maxIntentos} | <b>Tiempo:</b> {comp.nuevo.tiempoMaximo}s";
        }

        if (textoDiferencias != null)
        {
            textoDiferencias.text = $"<b>Comparativa de diferencias:</b>\n{comp.detalleDiferencias}";
        }

        panelModalDuplicado.SetActive(true);
    }

    private void OnCancelarEsteEjercicioClick()
    {
        decisionDuplicadoAprobada = false;
        esperandoDecisionDuplicado = false;
        if (panelModalDuplicado != null) panelModalDuplicado.SetActive(false);
    }

    private void OnAgregarDeTodasFormasClick()
    {
        decisionDuplicadoAprobada = true;
        esperandoDecisionDuplicado = false;
        if (panelModalDuplicado != null) panelModalDuplicado.SetActive(false);
    }

    private void MostrarMensajeModal(string titulo, string mensaje)
    {
        if (panelModalMensaje != null)
        {
            if (textoTituloMensaje != null) textoTituloMensaje.text = titulo;
            if (textoCuerpoMensaje != null) textoCuerpoMensaje.text = mensaje;
            panelModalMensaje.SetActive(true);
        }
        else
        {
            Debug.Log($"[{titulo}] {mensaje}");
        }
    }

    private void MostrarEstado(string mensaje, bool mostrarProgreso)
    {
        if (textoEstadoGuardado != null)
        {
            textoEstadoGuardado.text = mensaje;
        }

        if (indicadorProgreso != null)
        {
            indicadorProgreso.SetActive(mostrarProgreso);
        }
    }

    private void SetControlesBloqueados(bool bloqueado)
    {
        if (btnGuardar != null) btnGuardar.interactable = !bloqueado;
        if (btnVolverASeleccion != null) btnVolverASeleccion.interactable = !bloqueado;
        if (btnCancelarFormulario != null) btnCancelarFormulario.interactable = !bloqueado;
    }

    public void CerrarInterfaz()
    {
        if (uiControlMenu != null)
        {
            uiControlMenu.CerrarPanelDocente();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
