using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class DocenteUIBuilder
{
    private const string FONT_PATH = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string SPRITE_BUTTON_HOVER = "Assets/Image/Tiny Swords/UI/Buttons/Button_Hover_3Slides.png";
    private const string SPRITE_BUTTON_BLUE = "Assets/Image/Tiny Swords/UI/Buttons/Button_Blue_3Slides.png";
    private const string SPRITE_BUTTON_RED = "Assets/Image/Tiny Swords/UI/Buttons/Button_Red_3Slides.png";
    private const string SPRITE_FONDO_PANEL = "Assets/Image/Tiny Swords/Terrain/Background/FondoMenuPanel.png";
    private const string SPRITE_BANNER_HORIZ = "Assets/Image/Tiny Swords/UI/Banners/Banner_Horizontal.png";
    private const string SPRITE_RETURN = "Assets/Image/Tiny Swords/Terrain/Background/ReturnButton.png";

    // Procedural UI sprites
    private const string SPR_FIELD_LIGHT = "Assets/Image/DocenteUI/field_bg_light.png";
    private const string SPR_FIELD_DARK = "Assets/Image/DocenteUI/field_bg_dark.png";
    private const string SPR_CARD_PARCHMENT = "Assets/Image/DocenteUI/card_bg_parchment.png";
    private const string SPR_ROW_ITEM = "Assets/Image/DocenteUI/row_bg_item.png";
    private const string SPR_CHECKBOX_BG = "Assets/Image/DocenteUI/checkbox_bg.png";
    private const string SPR_CHECKBOX_CHECK = "Assets/Image/DocenteUI/checkbox_check.png";
    private const string SPR_SCROLLBAR_HANDLE = "Assets/Image/DocenteUI/scrollbar_handle.png";
    private const string SPR_WINDOW_SLATE = "Assets/Image/DocenteUI/window_bg_slate.png";

    [MenuItem("Docente/Construir Interfaz Docente Completa")]
    public static void ConstruirInterfaz()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_PATH);
        Sprite sHover = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUTTON_HOVER);
        Sprite sBlue = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUTTON_BLUE);
        Sprite sRed = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BUTTON_RED);
        Sprite sBanner = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_BANNER_HORIZ);
        Sprite sReturn = AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_RETURN);

        // Sub-sprites de FondoMenuPanel
        Sprite sFondoHeader = null;
        Object[] fondoSubs = AssetDatabase.LoadAllAssetsAtPath(SPRITE_FONDO_PANEL);
        foreach (var sub in fondoSubs)
        {
            if (sub is Sprite s && s.name == "FondoMenuPanel_0")
            {
                sFondoHeader = s;
                break;
            }
        }

        Sprite sFieldLight = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_FIELD_LIGHT);
        Sprite sFieldDark = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_FIELD_DARK);
        Sprite sCardParchment = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_CARD_PARCHMENT);
        Sprite sRowItem = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_ROW_ITEM);
        Sprite sCheckBg = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_CHECKBOX_BG);
        Sprite sCheckMark = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_CHECKBOX_CHECK);
        Sprite sScrollHandle = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_SCROLLBAR_HANDLE);
        Sprite sWindowSlate = AssetDatabase.LoadAssetAtPath<Sprite>(SPR_WINDOW_SLATE);

        // Crear carpeta de prefabs si no existe
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Docente"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Docente");
        }

        // 1. Construir Prefab TopicItem
        GameObject prefabTopic = ConstruirPrefabTopicItem(font, sRowItem, sCheckBg, sCheckMark, sFieldDark);

        // 2. Construir Prefab EjercicioItem
        GameObject prefabEjercicio = ConstruirPrefabEjercicioItem(font, sCardParchment, sFieldLight);

        // 3. Construir PanelDocente en Canvas
        GameObject canvasGo = GameObject.Find("Canvas");
        if (canvasGo == null)
        {
            Debug.LogError("No se encontró el objeto Canvas en la escena.");
            return;
        }

        Transform existente = canvasGo.transform.Find("PanelDocente");
        if (existente != null)
        {
            Undo.DestroyObjectImmediate(existente.gameObject);
        }

        GameObject panelDocente = new GameObject("PanelDocente", typeof(RectTransform), typeof(Image), typeof(DocenteController));
        Undo.RegisterCreatedObjectUndo(panelDocente, "Crear PanelDocente");
        panelDocente.transform.SetParent(canvasGo.transform, false);

        RectTransform rtDocente = panelDocente.GetComponent<RectTransform>();
        rtDocente.anchorMin = Vector2.zero;
        rtDocente.anchorMax = Vector2.one;
        rtDocente.offsetMin = Vector2.zero;
        rtDocente.offsetMax = Vector2.zero;

        Image imgBackdrop = panelDocente.GetComponent<Image>();
        imgBackdrop.color = new Color(0.04f, 0.06f, 0.1f, 0.88f);
        imgBackdrop.raycastTarget = true;

        DocenteController controller = panelDocente.GetComponent<DocenteController>();
        controller.prefabTopicItem = prefabTopic;
        controller.prefabEjercicioItem = prefabEjercicio;
        controller.contenidoManager = Object.FindAnyObjectByType<ContenidoManager>();
        controller.uiControlMenu = Object.FindAnyObjectByType<UIControlMenu>();

        // Cuadro Principal (WindowBox)
        GameObject windowBox = CrearUIObject("WindowBox", panelDocente.transform);
        RectTransform rtWindow = windowBox.GetComponent<RectTransform>();
        rtWindow.anchorMin = new Vector2(0.5f, 0.5f);
        rtWindow.anchorMax = new Vector2(0.5f, 0.5f);
        rtWindow.pivot = new Vector2(0.5f, 0.5f);
        rtWindow.sizeDelta = new Vector2(750f, 535f);
        Image imgWindow = windowBox.AddComponent<Image>();
        imgWindow.sprite = sWindowSlate;
        imgWindow.type = Image.Type.Sliced;
        imgWindow.color = Color.white;

        // Encabezado (Header Bar)
        GameObject header = CrearUIObject("Header", windowBox.transform);
        RectTransform rtHeader = header.GetComponent<RectTransform>();
        rtHeader.anchorMin = new Vector2(0f, 1f);
        rtHeader.anchorMax = new Vector2(1f, 1f);
        rtHeader.pivot = new Vector2(0.5f, 1f);
        rtHeader.sizeDelta = new Vector2(0f, 56f);
        rtHeader.anchoredPosition = Vector2.zero;

        // Banner gráfico de madera en el centro del header
        GameObject bannerBg = CrearUIObject("BannerWood", header.transform);
        RectTransform rtBanner = bannerBg.GetComponent<RectTransform>();
        rtBanner.anchorMin = new Vector2(0.5f, 0.5f);
        rtBanner.anchorMax = new Vector2(0.5f, 0.5f);
        rtBanner.pivot = new Vector2(0.5f, 0.5f);
        rtBanner.sizeDelta = new Vector2(500f, 48f);
        Image imgBanner = bannerBg.AddComponent<Image>();
        imgBanner.sprite = sFondoHeader != null ? sFondoHeader : sBanner;
        imgBanner.type = Image.Type.Simple;
        imgBanner.preserveAspect = true;

        GameObject titleGo = CrearUIObject("Title", header.transform);
        RectTransform rtTitle = titleGo.GetComponent<RectTransform>();
        rtTitle.anchorMin = Vector2.zero;
        rtTitle.anchorMax = Vector2.one;
        rtTitle.offsetMin = new Vector2(20f, 0f);
        rtTitle.offsetMax = new Vector2(-60f, 0f);
        TextMeshProUGUI txtTitle = titleGo.AddComponent<TextMeshProUGUI>();
        txtTitle.font = font;
        txtTitle.text = "REGISTRO DOCENTE DE CONTENIDOS";
        txtTitle.fontSize = 18;
        txtTitle.fontStyle = FontStyles.Bold;
        txtTitle.alignment = TextAlignmentOptions.Center;
        txtTitle.color = Color.white;

        // Botón Cerrar en el Header
        GameObject btnCloseHeader = CrearBotonConTexto("BtnCerrarHeader", header.transform, sRed != null ? sRed : sReturn, "X", 16, font, new Vector2(36f, 36f));
        RectTransform rtClose = btnCloseHeader.GetComponent<RectTransform>();
        rtClose.anchorMin = new Vector2(1f, 0.5f);
        rtClose.anchorMax = new Vector2(1f, 0.5f);
        rtClose.pivot = new Vector2(1f, 0.5f);
        rtClose.anchoredPosition = new Vector2(-12f, 0f);
        controller.btnCerrarSeleccion = btnCloseHeader.GetComponent<Button>();

        // =========================================================================
        // PASO 1: PANEL SELECCIÓN
        // =========================================================================
        GameObject panelSel = CrearUIObject("PanelSeleccion", windowBox.transform);
        controller.panelSeleccion = panelSel;
        RectTransform rtSel = panelSel.GetComponent<RectTransform>();
        rtSel.anchorMin = Vector2.zero;
        rtSel.anchorMax = Vector2.one;
        rtSel.offsetMin = new Vector2(16f, 12f);
        rtSel.offsetMax = new Vector2(-16f, -60f);

        // Fila selección curso
        GameObject rowCurso = CrearUIObject("RowCurso", panelSel.transform);
        RectTransform rtRowCurso = rowCurso.GetComponent<RectTransform>();
        rtRowCurso.anchorMin = new Vector2(0f, 1f);
        rtRowCurso.anchorMax = new Vector2(1f, 1f);
        rtRowCurso.pivot = new Vector2(0.5f, 1f);
        rtRowCurso.sizeDelta = new Vector2(0f, 42f);
        rtRowCurso.anchoredPosition = new Vector2(0f, 0f);

        GameObject lblCursoGo = CrearUIObject("LabelCurso", rowCurso.transform);
        RectTransform rtLblCurso = lblCursoGo.GetComponent<RectTransform>();
        rtLblCurso.anchorMin = new Vector2(0f, 0.5f);
        rtLblCurso.anchorMax = new Vector2(0f, 0.5f);
        rtLblCurso.pivot = new Vector2(0f, 0.5f);
        rtLblCurso.sizeDelta = new Vector2(65f, 32f);
        rtLblCurso.anchoredPosition = new Vector2(4f, 0f);
        TextMeshProUGUI txtLblCurso = lblCursoGo.AddComponent<TextMeshProUGUI>();
        txtLblCurso.font = font;
        txtLblCurso.text = "Curso:";
        txtLblCurso.fontSize = 15;
        txtLblCurso.fontStyle = FontStyles.Bold;
        txtLblCurso.alignment = TextAlignmentOptions.MidlineLeft;
        txtLblCurso.color = Color.white;

        // Dropdown Curso estilizado
        GameObject dropdownCursoGo = CrearTMPDropdown("DropdownCurso", rowCurso.transform, font, sFieldLight, sScrollHandle);
        RectTransform rtDropCurso = dropdownCursoGo.GetComponent<RectTransform>();
        rtDropCurso.anchorMin = new Vector2(0f, 0.5f);
        rtDropCurso.anchorMax = new Vector2(0f, 0.5f);
        rtDropCurso.pivot = new Vector2(0f, 0.5f);
        rtDropCurso.sizeDelta = new Vector2(250f, 34f);
        rtDropCurso.anchoredPosition = new Vector2(75f, 0f);
        controller.dropdownCurso = dropdownCursoGo.GetComponent<TMP_Dropdown>();

        // Subtítulo / Instrucciones
        GameObject lblSub = CrearUIObject("LabelInstrucciones", rowCurso.transform);
        RectTransform rtLblSub = lblSub.GetComponent<RectTransform>();
        rtLblSub.anchorMin = new Vector2(0f, 0.5f);
        rtLblSub.anchorMax = new Vector2(1f, 0.5f);
        rtLblSub.pivot = new Vector2(0f, 0.5f);
        rtLblSub.offsetMin = new Vector2(340f, -16f);
        rtLblSub.offsetMax = new Vector2(0f, 16f);
        TextMeshProUGUI txtLblSub = lblSub.AddComponent<TextMeshProUGUI>();
        txtLblSub.font = font;
        txtLblSub.text = "Marca los temas y define cuántos ejercicios registrar:";
        txtLblSub.fontSize = 13;
        txtLblSub.fontStyle = FontStyles.Italic;
        txtLblSub.alignment = TextAlignmentOptions.MidlineLeft;
        txtLblSub.color = new Color(0.72f, 0.78f, 0.88f, 1f);

        // ScrollRect Temas (sin scrollbar horizontal blanco)
        GameObject scrollTemasGo = CrearScrollViewLimpio("ScrollTemas", panelSel.transform, sScrollHandle, out Transform contentTemas);
        RectTransform rtScrollTemas = scrollTemasGo.GetComponent<RectTransform>();
        rtScrollTemas.anchorMin = new Vector2(0f, 0f);
        rtScrollTemas.anchorMax = new Vector2(1f, 1f);
        rtScrollTemas.offsetMin = new Vector2(0f, 56f);
        rtScrollTemas.offsetMax = new Vector2(0f, -48f);
        controller.containerTemas = contentTemas;

        // Barra inferior paso 1
        GameObject bottomBarSel = CrearUIObject("BottomBar", panelSel.transform);
        RectTransform rtBottomSel = bottomBarSel.GetComponent<RectTransform>();
        rtBottomSel.anchorMin = new Vector2(0f, 0f);
        rtBottomSel.anchorMax = new Vector2(1f, 0f);
        rtBottomSel.pivot = new Vector2(0.5f, 0f);
        rtBottomSel.sizeDelta = new Vector2(0f, 48f);
        rtBottomSel.anchoredPosition = Vector2.zero;

        GameObject txtErrSelGo = CrearUIObject("TextoError", bottomBarSel.transform);
        RectTransform rtErrSel = txtErrSelGo.GetComponent<RectTransform>();
        rtErrSel.anchorMin = new Vector2(0f, 0f);
        rtErrSel.anchorMax = new Vector2(0.65f, 1f);
        rtErrSel.offsetMin = new Vector2(4f, 0f);
        rtErrSel.offsetMax = Vector2.zero;
        TextMeshProUGUI txtErrSel = txtErrSelGo.AddComponent<TextMeshProUGUI>();
        txtErrSel.font = font;
        txtErrSel.text = "";
        txtErrSel.fontSize = 13;
        txtErrSel.fontStyle = FontStyles.Bold;
        txtErrSel.color = new Color(1f, 0.35f, 0.35f, 1f);
        txtErrSel.alignment = TextAlignmentOptions.MidlineLeft;
        controller.textoErrorSeleccion = txtErrSel;

        GameObject btnContinuar = CrearBotonConTexto("BtnContinuar", bottomBarSel.transform, sBlue != null ? sBlue : sHover, "Continuar a Formularios  →", 15, font, new Vector2(230f, 44f));
        RectTransform rtBtnCont = btnContinuar.GetComponent<RectTransform>();
        rtBtnCont.anchorMin = new Vector2(1f, 0.5f);
        rtBtnCont.anchorMax = new Vector2(1f, 0.5f);
        rtBtnCont.pivot = new Vector2(1f, 0.5f);
        rtBtnCont.anchoredPosition = Vector2.zero;
        controller.btnContinuarAFormulario = btnContinuar.GetComponent<Button>();

        // =========================================================================
        // PASO 2: PANEL FORMULARIOS
        // =========================================================================
        GameObject panelForm = CrearUIObject("PanelFormularios", windowBox.transform);
        controller.panelFormularios = panelForm;
        RectTransform rtForm = panelForm.GetComponent<RectTransform>();
        rtForm.anchorMin = Vector2.zero;
        rtForm.anchorMax = Vector2.one;
        rtForm.offsetMin = new Vector2(16f, 12f);
        rtForm.offsetMax = new Vector2(-16f, -60f);

        // Barra superior paso 2
        GameObject topBarForm = CrearUIObject("TopBarForm", panelForm.transform);
        RectTransform rtTopForm = topBarForm.GetComponent<RectTransform>();
        rtTopForm.anchorMin = new Vector2(0f, 1f);
        rtTopForm.anchorMax = new Vector2(1f, 1f);
        rtTopForm.pivot = new Vector2(0.5f, 1f);
        rtTopForm.sizeDelta = new Vector2(0f, 38f);
        rtTopForm.anchoredPosition = Vector2.zero;

        GameObject btnVolver = CrearBotonConTexto("BtnVolver", topBarForm.transform, sHover, "← Volver a Temas", 13, font, new Vector2(145f, 34f));
        RectTransform rtBtnVolver = btnVolver.GetComponent<RectTransform>();
        rtBtnVolver.anchorMin = new Vector2(0f, 0.5f);
        rtBtnVolver.anchorMax = new Vector2(0f, 0.5f);
        rtBtnVolver.pivot = new Vector2(0f, 0.5f);
        rtBtnVolver.anchoredPosition = Vector2.zero;
        controller.btnVolverASeleccion = btnVolver.GetComponent<Button>();

        GameObject txtResumenGo = CrearUIObject("TextoResumen", topBarForm.transform);
        RectTransform rtResumen = txtResumenGo.GetComponent<RectTransform>();
        rtResumen.anchorMin = new Vector2(0f, 0f);
        rtResumen.anchorMax = new Vector2(1f, 1f);
        rtResumen.offsetMin = new Vector2(155f, 0f);
        rtResumen.offsetMax = Vector2.zero;
        TextMeshProUGUI txtResumen = txtResumenGo.AddComponent<TextMeshProUGUI>();
        txtResumen.font = font;
        txtResumen.text = "Curso: Aritmética | Total: 5 ejercicios";
        txtResumen.fontSize = 14;
        txtResumen.fontStyle = FontStyles.Bold;
        txtResumen.alignment = TextAlignmentOptions.MidlineLeft;
        txtResumen.color = new Color(0.92f, 0.95f, 1f, 1f);
        controller.textoResumenSesion = txtResumen;

        // ScrollRect Ejercicios
        GameObject scrollEjGo = CrearScrollViewLimpio("ScrollEjercicios", panelForm.transform, sScrollHandle, out Transform contentEjercicios);
        RectTransform rtScrollEj = scrollEjGo.GetComponent<RectTransform>();
        rtScrollEj.anchorMin = new Vector2(0f, 0f);
        rtScrollEj.anchorMax = new Vector2(1f, 1f);
        rtScrollEj.offsetMin = new Vector2(0f, 56f);
        rtScrollEj.offsetMax = new Vector2(0f, -44f);
        controller.scrollEjercicios = scrollEjGo.GetComponent<ScrollRect>();
        controller.containerEjercicios = contentEjercicios;

        // Barra inferior paso 2
        GameObject bottomBarForm = CrearUIObject("BottomBarForm", panelForm.transform);
        RectTransform rtBottomForm = bottomBarForm.GetComponent<RectTransform>();
        rtBottomForm.anchorMin = new Vector2(0f, 0f);
        rtBottomForm.anchorMax = new Vector2(1f, 0f);
        rtBottomForm.pivot = new Vector2(0.5f, 0f);
        rtBottomForm.sizeDelta = new Vector2(0f, 48f);
        rtBottomForm.anchoredPosition = Vector2.zero;

        GameObject txtEstadoGo = CrearUIObject("TextoEstado", bottomBarForm.transform);
        RectTransform rtEstado = txtEstadoGo.GetComponent<RectTransform>();
        rtEstado.anchorMin = new Vector2(0f, 0f);
        rtEstado.anchorMax = new Vector2(0.5f, 1f);
        rtEstado.offsetMin = new Vector2(4f, 0f);
        rtEstado.offsetMax = Vector2.zero;
        TextMeshProUGUI txtEstado = txtEstadoGo.AddComponent<TextMeshProUGUI>();
        txtEstado.font = font;
        txtEstado.text = "";
        txtEstado.fontSize = 13;
        txtEstado.fontStyle = FontStyles.Bold;
        txtEstado.color = new Color(0.35f, 0.75f, 1f, 1f);
        txtEstado.alignment = TextAlignmentOptions.MidlineLeft;
        controller.textoEstadoGuardado = txtEstado;

        // Indicador de progreso
        GameObject progGo = CrearUIObject("IndicadorProgreso", bottomBarForm.transform);
        RectTransform rtProg = progGo.GetComponent<RectTransform>();
        rtProg.anchorMin = new Vector2(0.48f, 0.5f);
        rtProg.anchorMax = new Vector2(0.48f, 0.5f);
        rtProg.sizeDelta = new Vector2(100f, 24f);
        TextMeshProUGUI txtProg = progGo.AddComponent<TextMeshProUGUI>();
        txtProg.font = font;
        txtProg.text = "⏳ Guardando...";
        txtProg.fontSize = 13;
        txtProg.fontStyle = FontStyles.Bold;
        txtProg.color = new Color(0.35f, 0.75f, 1f, 1f);
        progGo.SetActive(false);
        controller.indicadorProgreso = progGo;

        GameObject btnCancelar = CrearBotonConTexto("BtnCancelar", bottomBarForm.transform, sRed != null ? sRed : sHover, "Cancelar", 14, font, new Vector2(120f, 42f));
        RectTransform rtBtnCanc = btnCancelar.GetComponent<RectTransform>();
        rtBtnCanc.anchorMin = new Vector2(1f, 0.5f);
        rtBtnCanc.anchorMax = new Vector2(1f, 0.5f);
        rtBtnCanc.pivot = new Vector2(1f, 0.5f);
        rtBtnCanc.anchoredPosition = new Vector2(-205f, 0f);
        controller.btnCancelarFormulario = btnCancelar.GetComponent<Button>();

        GameObject btnGuardar = CrearBotonConTexto("BtnGuardar", bottomBarForm.transform, sBlue != null ? sBlue : sHover, "Guardar Contenidos", 14, font, new Vector2(195f, 44f));
        RectTransform rtBtnGuardar = btnGuardar.GetComponent<RectTransform>();
        rtBtnGuardar.anchorMin = new Vector2(1f, 0.5f);
        rtBtnGuardar.anchorMax = new Vector2(1f, 0.5f);
        rtBtnGuardar.pivot = new Vector2(1f, 0.5f);
        rtBtnGuardar.anchoredPosition = Vector2.zero;
        controller.btnGuardar = btnGuardar.GetComponent<Button>();

        // =========================================================================
        // MODAL 1: ADVERTENCIA DE DUPLICADOS
        // =========================================================================
        GameObject modalDup = CrearUIObject("ModalDuplicado", panelDocente.transform);
        controller.panelModalDuplicado = modalDup;
        RectTransform rtModalDup = modalDup.GetComponent<RectTransform>();
        rtModalDup.anchorMin = Vector2.zero;
        rtModalDup.anchorMax = Vector2.one;
        rtModalDup.offsetMin = Vector2.zero;
        rtModalDup.offsetMax = Vector2.zero;
        Image imgDupBg = modalDup.AddComponent<Image>();
        imgDupBg.color = new Color(0.04f, 0.06f, 0.1f, 0.9f);
        imgDupBg.raycastTarget = true;

        GameObject dupBox = CrearUIObject("DupBox", modalDup.transform);
        RectTransform rtDupBox = dupBox.GetComponent<RectTransform>();
        rtDupBox.anchorMin = new Vector2(0.5f, 0.5f);
        rtDupBox.anchorMax = new Vector2(0.5f, 0.5f);
        rtDupBox.sizeDelta = new Vector2(710f, 480f);
        Image imgDupBox = dupBox.AddComponent<Image>();
        imgDupBox.sprite = sWindowSlate;
        imgDupBox.type = Image.Type.Sliced;
        imgDupBox.color = Color.white;

        GameObject txtTituloDupGo = CrearUIObject("TituloDuplicado", dupBox.transform);
        RectTransform rtTitDup = txtTituloDupGo.GetComponent<RectTransform>();
        rtTitDup.anchorMin = new Vector2(0f, 1f);
        rtTitDup.anchorMax = new Vector2(1f, 1f);
        rtTitDup.pivot = new Vector2(0.5f, 1f);
        rtTitDup.sizeDelta = new Vector2(-40f, 32f);
        rtTitDup.anchoredPosition = new Vector2(0f, -12f);
        TextMeshProUGUI txtTitDup = txtTituloDupGo.AddComponent<TextMeshProUGUI>();
        txtTitDup.font = font;
        txtTitDup.text = "⚠️ POSIBLE EJERCICIO DUPLICADO DETECTADO";
        txtTitDup.fontSize = 16;
        txtTitDup.fontStyle = FontStyles.Bold;
        txtTitDup.color = new Color(1f, 0.4f, 0.35f, 1f);
        txtTitDup.alignment = TextAlignmentOptions.Center;
        controller.textoTituloDuplicado = txtTitDup;

        GameObject txtCursoTemaDupGo = CrearUIObject("CursoTemaDuplicado", dupBox.transform);
        RectTransform rtCTDup = txtCursoTemaDupGo.GetComponent<RectTransform>();
        rtCTDup.anchorMin = new Vector2(0f, 1f);
        rtCTDup.anchorMax = new Vector2(1f, 1f);
        rtCTDup.pivot = new Vector2(0.5f, 1f);
        rtCTDup.sizeDelta = new Vector2(-40f, 22f);
        rtCTDup.anchoredPosition = new Vector2(0f, -42f);
        TextMeshProUGUI txtCTDup = txtCursoTemaDupGo.AddComponent<TextMeshProUGUI>();
        txtCTDup.font = font;
        txtCTDup.text = "Curso: Aritmética | Tema: MCM";
        txtCTDup.fontSize = 13;
        txtCTDup.fontStyle = FontStyles.Bold;
        txtCTDup.color = new Color(0.8f, 0.86f, 0.95f, 1f);
        txtCTDup.alignment = TextAlignmentOptions.Center;
        controller.textoCursoTemaDuplicado = txtCTDup;

        // Fila de Comparación (Izquierda: Existente, Derecha: Nuevo)
        GameObject rowComp = CrearUIObject("RowComparacion", dupBox.transform);
        RectTransform rtRowComp = rowComp.GetComponent<RectTransform>();
        rtRowComp.anchorMin = new Vector2(0.5f, 0.5f);
        rtRowComp.anchorMax = new Vector2(0.5f, 0.5f);
        rtRowComp.sizeDelta = new Vector2(670f, 215f);
        rtRowComp.anchoredPosition = new Vector2(0f, 38f);

        // Cuadro Existente
        GameObject colExistente = CrearUIObject("ColExistente", rowComp.transform);
        RectTransform rtColExist = colExistente.GetComponent<RectTransform>();
        rtColExist.anchorMin = new Vector2(0f, 0f);
        rtColExist.anchorMax = new Vector2(0.485f, 1f);
        rtColExist.offsetMin = Vector2.zero;
        rtColExist.offsetMax = Vector2.zero;
        Image imgExist = colExistente.AddComponent<Image>();
        imgExist.sprite = sCardParchment;
        imgExist.type = Image.Type.Sliced;
        imgExist.color = Color.white;

        GameObject txtExistGo = CrearUIObject("TextoExistente", colExistente.transform);
        RectTransform rtTxtExist = txtExistGo.GetComponent<RectTransform>();
        rtTxtExist.anchorMin = Vector2.zero;
        rtTxtExist.anchorMax = Vector2.one;
        rtTxtExist.offsetMin = new Vector2(10f, 8f);
        rtTxtExist.offsetMax = new Vector2(-10f, -8f);
        TextMeshProUGUI txtExist = txtExistGo.AddComponent<TextMeshProUGUI>();
        txtExist.font = font;
        txtExist.fontSize = 12;
        txtExist.color = new Color(0.12f, 0.16f, 0.22f, 1f);
        txtExist.text = "Ejercicio Guardado...";
        controller.textoExistente = txtExist;

        // Cuadro Nuevo
        GameObject colNuevo = CrearUIObject("ColNuevo", rowComp.transform);
        RectTransform rtColNuevo = colNuevo.GetComponent<RectTransform>();
        rtColNuevo.anchorMin = new Vector2(0.515f, 0f);
        rtColNuevo.anchorMax = new Vector2(1f, 1f);
        rtColNuevo.offsetMin = Vector2.zero;
        rtColNuevo.offsetMax = Vector2.zero;
        Image imgNuevo = colNuevo.AddComponent<Image>();
        imgNuevo.sprite = sCardParchment;
        imgNuevo.type = Image.Type.Sliced;
        imgNuevo.color = new Color(0.94f, 0.98f, 1f, 1f);

        GameObject txtNuevoGo = CrearUIObject("TextoNuevo", colNuevo.transform);
        RectTransform rtTxtNuevo = txtNuevoGo.GetComponent<RectTransform>();
        rtTxtNuevo.anchorMin = Vector2.zero;
        rtTxtNuevo.anchorMax = Vector2.one;
        rtTxtNuevo.offsetMin = new Vector2(10f, 8f);
        rtTxtNuevo.offsetMax = new Vector2(-10f, -8f);
        TextMeshProUGUI txtNuevo = txtNuevoGo.AddComponent<TextMeshProUGUI>();
        txtNuevo.font = font;
        txtNuevo.fontSize = 12;
        txtNuevo.color = new Color(0.08f, 0.22f, 0.45f, 1f);
        txtNuevo.text = "Ejercicio Nuevo...";
        controller.textoNuevo = txtNuevo;

        // Cuadro de Diferencias
        GameObject boxDiff = CrearUIObject("BoxDiferencias", dupBox.transform);
        RectTransform rtBoxDiff = boxDiff.GetComponent<RectTransform>();
        rtBoxDiff.anchorMin = new Vector2(0.5f, 0.5f);
        rtBoxDiff.anchorMax = new Vector2(0.5f, 0.5f);
        rtBoxDiff.sizeDelta = new Vector2(670f, 90f);
        rtBoxDiff.anchoredPosition = new Vector2(0f, -125f);
        Image imgDiff = boxDiff.AddComponent<Image>();
        imgDiff.sprite = sFieldDark;
        imgDiff.type = Image.Type.Sliced;
        imgDiff.color = Color.white;

        GameObject txtDiffGo = CrearUIObject("TextoDiferencias", boxDiff.transform);
        RectTransform rtTxtDiff = txtDiffGo.GetComponent<RectTransform>();
        rtTxtDiff.anchorMin = Vector2.zero;
        rtTxtDiff.anchorMax = Vector2.one;
        rtTxtDiff.offsetMin = new Vector2(12f, 6f);
        rtTxtDiff.offsetMax = new Vector2(-12f, -6f);
        TextMeshProUGUI txtDiff = txtDiffGo.AddComponent<TextMeshProUGUI>();
        txtDiff.font = font;
        txtDiff.fontSize = 12;
        txtDiff.color = new Color(1f, 0.85f, 0.45f, 1f);
        txtDiff.text = "Diferencias...";
        controller.textoDiferencias = txtDiff;

        // Botones de acción modal duplicados
        GameObject btnCancDup = CrearBotonConTexto("BtnCancelarEsteEjercicio", dupBox.transform, sRed != null ? sRed : sHover, "Cancelar este ejercicio", 13, font, new Vector2(220f, 40f));
        RectTransform rtBtnCancDup = btnCancDup.GetComponent<RectTransform>();
        rtBtnCancDup.anchorMin = new Vector2(0.5f, 0f);
        rtBtnCancDup.anchorMax = new Vector2(0.5f, 0f);
        rtBtnCancDup.pivot = new Vector2(0.5f, 0f);
        rtBtnCancDup.anchoredPosition = new Vector2(-125f, 14f);
        controller.btnCancelarEsteEjercicio = btnCancDup.GetComponent<Button>();

        GameObject btnAgregDup = CrearBotonConTexto("BtnAgregarDeTodasFormas", dupBox.transform, sBlue != null ? sBlue : sHover, "Agregar de todas formas", 13, font, new Vector2(220f, 40f));
        RectTransform rtBtnAgregDup = btnAgregDup.GetComponent<RectTransform>();
        rtBtnAgregDup.anchorMin = new Vector2(0.5f, 0f);
        rtBtnAgregDup.anchorMax = new Vector2(0.5f, 0f);
        rtBtnAgregDup.pivot = new Vector2(0.5f, 0f);
        rtBtnAgregDup.anchoredPosition = new Vector2(125f, 14f);
        controller.btnAgregarDeTodasFormas = btnAgregDup.GetComponent<Button>();

        modalDup.SetActive(false);

        // =========================================================================
        // MODAL 2: MENSAJE (ÉXITO / ERROR)
        // =========================================================================
        GameObject modalMsg = CrearUIObject("ModalMensaje", panelDocente.transform);
        controller.panelModalMensaje = modalMsg;
        RectTransform rtModalMsg = modalMsg.GetComponent<RectTransform>();
        rtModalMsg.anchorMin = Vector2.zero;
        rtModalMsg.anchorMax = Vector2.one;
        rtModalMsg.offsetMin = Vector2.zero;
        rtModalMsg.offsetMax = Vector2.zero;
        Image imgMsgBg = modalMsg.AddComponent<Image>();
        imgMsgBg.color = new Color(0.04f, 0.06f, 0.1f, 0.85f);
        imgMsgBg.raycastTarget = true;

        GameObject msgBox = CrearUIObject("MsgBox", modalMsg.transform);
        RectTransform rtMsgBox = msgBox.GetComponent<RectTransform>();
        rtMsgBox.anchorMin = new Vector2(0.5f, 0.5f);
        rtMsgBox.anchorMax = new Vector2(0.5f, 0.5f);
        rtMsgBox.sizeDelta = new Vector2(500f, 240f);
        Image imgMsgBox = msgBox.AddComponent<Image>();
        imgMsgBox.sprite = sWindowSlate;
        imgMsgBox.type = Image.Type.Sliced;
        imgMsgBox.color = Color.white;

        GameObject txtTitMsgGo = CrearUIObject("TituloMensaje", msgBox.transform);
        RectTransform rtTitMsg = txtTitMsgGo.GetComponent<RectTransform>();
        rtTitMsg.anchorMin = new Vector2(0f, 1f);
        rtTitMsg.anchorMax = new Vector2(1f, 1f);
        rtTitMsg.pivot = new Vector2(0.5f, 1f);
        rtTitMsg.sizeDelta = new Vector2(-40f, 36f);
        rtTitMsg.anchoredPosition = new Vector2(0f, -14f);
        TextMeshProUGUI txtTitMsg = txtTitMsgGo.AddComponent<TextMeshProUGUI>();
        txtTitMsg.font = font;
        txtTitMsg.text = "¡Información!";
        txtTitMsg.fontSize = 17;
        txtTitMsg.fontStyle = FontStyles.Bold;
        txtTitMsg.color = new Color(0.35f, 0.75f, 1f, 1f);
        txtTitMsg.alignment = TextAlignmentOptions.Center;
        controller.textoTituloMensaje = txtTitMsg;

        GameObject txtBodyMsgGo = CrearUIObject("CuerpoMensaje", msgBox.transform);
        RectTransform rtBodyMsg = txtBodyMsgGo.GetComponent<RectTransform>();
        rtBodyMsg.anchorMin = new Vector2(0f, 0f);
        rtBodyMsg.anchorMax = new Vector2(1f, 1f);
        rtBodyMsg.offsetMin = new Vector2(25f, 60f);
        rtBodyMsg.offsetMax = new Vector2(-25f, -55f);
        TextMeshProUGUI txtBodyMsg = txtBodyMsgGo.AddComponent<TextMeshProUGUI>();
        txtBodyMsg.font = font;
        txtBodyMsg.text = "Mensaje...";
        txtBodyMsg.fontSize = 13.5f;
        txtBodyMsg.color = new Color(0.92f, 0.95f, 1f, 1f);
        txtBodyMsg.alignment = TextAlignmentOptions.TopLeft;
        controller.textoCuerpoMensaje = txtBodyMsg;

        GameObject btnOkMsg = CrearBotonConTexto("BtnAceptarMensaje", msgBox.transform, sBlue != null ? sBlue : sHover, "Aceptar", 14, font, new Vector2(140f, 38f));
        RectTransform rtBtnOkMsg = btnOkMsg.GetComponent<RectTransform>();
        rtBtnOkMsg.anchorMin = new Vector2(0.5f, 0f);
        rtBtnOkMsg.anchorMax = new Vector2(0.5f, 0f);
        rtBtnOkMsg.pivot = new Vector2(0.5f, 0f);
        rtBtnOkMsg.anchoredPosition = new Vector2(0f, 14f);
        controller.btnAceptarMensaje = btnOkMsg.GetComponent<Button>();

        modalMsg.SetActive(false);

        // =========================================================================
        // VINCULACIÓN CON UICONTROLMENU Y BOTÓN CARGA DIRECTA
        // =========================================================================
        UIControlMenu uiControl = Object.FindAnyObjectByType<UIControlMenu>();
        if (uiControl != null)
        {
            uiControl.panelDocente = panelDocente;
            EditorUtility.SetDirty(uiControl);
        }

        // Vincular ButtonCargaDirecta
        GameObject btnCargaDirectaGo = GameObject.Find("Canvas/CargadorDeContenido/ButtonCargaDirecta");
        if (btnCargaDirectaGo != null && uiControl != null)
        {
            Button btn = btnCargaDirectaGo.GetComponent<Button>();
            if (btn != null)
            {
                while (btn.onClick.GetPersistentEventCount() > 0)
                {
                    UnityEditor.Events.UnityEventTools.RemovePersistentListener(btn.onClick, 0);
                }
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, uiControl.CargarContenidoDirecto);
                EditorUtility.SetDirty(btn);
            }
        }

        // Iniciar panelDocente inactivo por defecto
        panelDocente.SetActive(false);

        EditorUtility.SetDirty(panelDocente);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

        Debug.Log("¡Interfaz Docente construida y optimizada visualmente con éxito!");
    }

    private static GameObject ConstruirPrefabTopicItem(TMP_FontAsset font, Sprite sRowBg, Sprite sCheckBg, Sprite sCheckMark, Sprite sFieldDark)
    {
        string path = "Assets/Prefabs/Docente/TopicItemPrefab.prefab";
        GameObject root = new GameObject("TopicItemPrefab", typeof(RectTransform), typeof(Image), typeof(DocenteTopicItemUI));
        RectTransform rtRoot = root.GetComponent<RectTransform>();
        rtRoot.sizeDelta = new Vector2(685f, 42f);
        Image imgRoot = root.GetComponent<Image>();
        imgRoot.sprite = sRowBg;
        imgRoot.type = Image.Type.Sliced;
        imgRoot.color = Color.white;

        DocenteTopicItemUI itemUI = root.GetComponent<DocenteTopicItemUI>();

        // Toggle
        GameObject toggleGo = CrearToggleLimpio("ToggleSeleccion", root.transform, sCheckBg, sCheckMark);
        RectTransform rtToggle = toggleGo.GetComponent<RectTransform>();
        rtToggle.anchorMin = new Vector2(0f, 0.5f);
        rtToggle.anchorMax = new Vector2(0f, 0.5f);
        rtToggle.pivot = new Vector2(0f, 0.5f);
        rtToggle.anchoredPosition = new Vector2(14f, 0f);
        itemUI.toggleSeleccion = toggleGo.GetComponent<Toggle>();

        // Nombre Tema
        GameObject txtNombreGo = CrearUIObject("NombreTema", root.transform);
        RectTransform rtNombre = txtNombreGo.GetComponent<RectTransform>();
        rtNombre.anchorMin = new Vector2(0f, 0.5f);
        rtNombre.anchorMax = new Vector2(0f, 0.5f);
        rtNombre.pivot = new Vector2(0f, 0.5f);
        rtNombre.sizeDelta = new Vector2(400f, 30f);
        rtNombre.anchoredPosition = new Vector2(48f, 0f);
        TextMeshProUGUI txtNombre = txtNombreGo.AddComponent<TextMeshProUGUI>();
        txtNombre.font = font;
        txtNombre.fontSize = 14.5f;
        txtNombre.fontStyle = FontStyles.Bold;
        txtNombre.color = Color.white;
        txtNombre.alignment = TextAlignmentOptions.MidlineLeft;
        txtNombre.text = "Nombre Tema";
        itemUI.nombreTemaText = txtNombre;

        // Label Cantidad
        GameObject lblCantGo = CrearUIObject("LabelCantidad", root.transform);
        RectTransform rtLblCant = lblCantGo.GetComponent<RectTransform>();
        rtLblCant.anchorMin = new Vector2(1f, 0.5f);
        rtLblCant.anchorMax = new Vector2(1f, 0.5f);
        rtLblCant.pivot = new Vector2(1f, 0.5f);
        rtLblCant.sizeDelta = new Vector2(75f, 30f);
        rtLblCant.anchoredPosition = new Vector2(-125f, 0f);
        TextMeshProUGUI txtLblCant = lblCantGo.AddComponent<TextMeshProUGUI>();
        txtLblCant.font = font;
        txtLblCant.fontSize = 13;
        txtLblCant.color = new Color(0.75f, 0.82f, 0.92f, 1f);
        txtLblCant.alignment = TextAlignmentOptions.MidlineRight;
        txtLblCant.text = "Ejercicios:";

        // Botón Menos
        GameObject btnMenosGo = CrearBotonSimple("BtnMenos", root.transform, sFieldDark, "-", 15, font, new Vector2(28f, 28f));
        RectTransform rtBtnMenos = btnMenosGo.GetComponent<RectTransform>();
        rtBtnMenos.anchorMin = new Vector2(1f, 0.5f);
        rtBtnMenos.anchorMax = new Vector2(1f, 0.5f);
        rtBtnMenos.pivot = new Vector2(1f, 0.5f);
        rtBtnMenos.anchoredPosition = new Vector2(-92f, 0f);
        itemUI.btnMenos = btnMenosGo.GetComponent<Button>();

        // Input Cantidad
        GameObject inputCantGo = CrearTMPInputField("InputCantidad", root.transform, font, sFieldDark, "1", 14, false, TextAlignmentOptions.Center);
        RectTransform rtInpCant = inputCantGo.GetComponent<RectTransform>();
        rtInpCant.anchorMin = new Vector2(1f, 0.5f);
        rtInpCant.anchorMax = new Vector2(1f, 0.5f);
        rtInpCant.pivot = new Vector2(1f, 0.5f);
        rtInpCant.sizeDelta = new Vector2(46f, 28f);
        rtInpCant.anchoredPosition = new Vector2(-42f, 0f);
        TMP_InputField inpCant = inputCantGo.GetComponent<TMP_InputField>();
        inpCant.contentType = TMP_InputField.ContentType.IntegerNumber;
        inpCant.textComponent.color = new Color(0.35f, 0.75f, 1f, 1f);
        inpCant.textComponent.fontStyle = FontStyles.Bold;
        itemUI.inputCantidad = inpCant;

        // Botón Más
        GameObject btnMasGo = CrearBotonSimple("BtnMas", root.transform, sFieldDark, "+", 15, font, new Vector2(28f, 28f));
        RectTransform rtBtnMas = btnMasGo.GetComponent<RectTransform>();
        rtBtnMas.anchorMin = new Vector2(1f, 0.5f);
        rtBtnMas.anchorMax = new Vector2(1f, 0.5f);
        rtBtnMas.pivot = new Vector2(1f, 0.5f);
        rtBtnMas.anchoredPosition = new Vector2(-10f, 0f);
        itemUI.btnMas = btnMasGo.GetComponent<Button>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject ConstruirPrefabEjercicioItem(TMP_FontAsset font, Sprite sCardBg, Sprite sFieldLight)
    {
        string path = "Assets/Prefabs/Docente/EjercicioItemPrefab.prefab";
        GameObject root = new GameObject("EjercicioItemPrefab", typeof(RectTransform), typeof(Image), typeof(DocenteEjercicioItemUI));
        RectTransform rtRoot = root.GetComponent<RectTransform>();
        rtRoot.sizeDelta = new Vector2(685f, 265f);
        Image imgRoot = root.GetComponent<Image>();
        imgRoot.sprite = sCardBg;
        imgRoot.type = Image.Type.Sliced;
        imgRoot.color = Color.white;

        DocenteEjercicioItemUI itemUI = root.GetComponent<DocenteEjercicioItemUI>();

        // Fila 1: Header (Título + ID)
        GameObject rowHeader = CrearUIObject("HeaderRow", root.transform);
        RectTransform rtHead = rowHeader.GetComponent<RectTransform>();
        rtHead.anchorMin = new Vector2(0f, 1f);
        rtHead.anchorMax = new Vector2(1f, 1f);
        rtHead.pivot = new Vector2(0.5f, 1f);
        rtHead.sizeDelta = new Vector2(-30f, 24f);
        rtHead.anchoredPosition = new Vector2(0f, -10f);

        GameObject txtTitGo = CrearUIObject("Titulo", rowHeader.transform);
        RectTransform rtTit = txtTitGo.GetComponent<RectTransform>();
        rtTit.anchorMin = new Vector2(0f, 0f);
        rtTit.anchorMax = new Vector2(0.72f, 1f);
        rtTit.offsetMin = Vector2.zero;
        rtTit.offsetMax = Vector2.zero;
        TextMeshProUGUI txtTit = txtTitGo.AddComponent<TextMeshProUGUI>();
        txtTit.font = font;
        txtTit.fontSize = 14f;
        txtTit.fontStyle = FontStyles.Bold;
        txtTit.color = new Color(0.12f, 0.18f, 0.28f, 1f);
        txtTit.alignment = TextAlignmentOptions.MidlineLeft;
        txtTit.text = "Curso > Tema — Ejercicio 1 de 1";
        itemUI.tituloText = txtTit;

        GameObject txtIdGo = CrearUIObject("IDAsignado", rowHeader.transform);
        RectTransform rtId = txtIdGo.GetComponent<RectTransform>();
        rtId.anchorMin = new Vector2(0.72f, 0f);
        rtId.anchorMax = new Vector2(1f, 1f);
        rtId.offsetMin = Vector2.zero;
        rtId.offsetMax = Vector2.zero;
        TextMeshProUGUI txtId = txtIdGo.AddComponent<TextMeshProUGUI>();
        txtId.font = font;
        txtId.fontSize = 12f;
        txtId.fontStyle = FontStyles.Italic;
        txtId.color = new Color(0.38f, 0.44f, 0.52f, 1f);
        txtId.alignment = TextAlignmentOptions.MidlineRight;
        txtId.text = "ID: 1";
        itemUI.idAsignadoText = txtId;

        // Fila 2: Enunciado (Label + InputField multilínea amplio)
        GameObject lblEnunGo = CrearUIObject("LabelEnunciado", root.transform);
        RectTransform rtLblEnun = lblEnunGo.GetComponent<RectTransform>();
        rtLblEnun.anchorMin = new Vector2(0f, 1f);
        rtLblEnun.anchorMax = new Vector2(1f, 1f);
        rtLblEnun.pivot = new Vector2(0.5f, 1f);
        rtLblEnun.sizeDelta = new Vector2(-30f, 16f);
        rtLblEnun.anchoredPosition = new Vector2(0f, -36f);
        TextMeshProUGUI txtLblEnun = lblEnunGo.AddComponent<TextMeshProUGUI>();
        txtLblEnun.font = font;
        txtLblEnun.fontSize = 12f;
        txtLblEnun.fontStyle = FontStyles.Bold;
        txtLblEnun.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblEnun.alignment = TextAlignmentOptions.MidlineLeft;
        txtLblEnun.text = "Enunciado del ejercicio:";

        GameObject inpEnunGo = CrearTMPInputField("InputEnunciado", root.transform, font, sFieldLight, "Escribe aquí el enunciado completo del ejercicio...", 13f, true);
        RectTransform rtInpEnun = inpEnunGo.GetComponent<RectTransform>();
        rtInpEnun.anchorMin = new Vector2(0f, 1f);
        rtInpEnun.anchorMax = new Vector2(1f, 1f);
        rtInpEnun.pivot = new Vector2(0.5f, 1f);
        rtInpEnun.sizeDelta = new Vector2(-30f, 54f);
        rtInpEnun.anchoredPosition = new Vector2(0f, -54f);
        TMP_InputField inpEnun = inpEnunGo.GetComponent<TMP_InputField>();
        inpEnun.lineType = TMP_InputField.LineType.MultiLineSubmit;
        itemUI.inputEnunciado = inpEnun;

        // Fila 3: Parámetros (Dificultad, Max Intentos, Tiempo Máximo en 3 columnas iguales)
        GameObject rowParams = CrearUIObject("RowParametros", root.transform);
        RectTransform rtParams = rowParams.GetComponent<RectTransform>();
        rtParams.anchorMin = new Vector2(0f, 1f);
        rtParams.anchorMax = new Vector2(1f, 1f);
        rtParams.pivot = new Vector2(0.5f, 1f);
        rtParams.sizeDelta = new Vector2(-30f, 52f);
        rtParams.anchoredPosition = new Vector2(0f, -114f);

        float colW = 210f;

        // Columna 1: Dificultad
        GameObject lblDifGo = CrearUIObject("LabelDificultad", rowParams.transform);
        RectTransform rtLblDif = lblDifGo.GetComponent<RectTransform>();
        rtLblDif.anchorMin = new Vector2(0f, 1f);
        rtLblDif.anchorMax = new Vector2(0f, 1f);
        rtLblDif.pivot = new Vector2(0f, 1f);
        rtLblDif.sizeDelta = new Vector2(colW, 16f);
        rtLblDif.anchoredPosition = Vector2.zero;
        TextMeshProUGUI txtLblDif = lblDifGo.AddComponent<TextMeshProUGUI>();
        txtLblDif.font = font;
        txtLblDif.fontSize = 12f;
        txtLblDif.fontStyle = FontStyles.Bold;
        txtLblDif.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblDif.text = "Dificultad:";

        GameObject dropDifGo = CrearTMPDropdown("DropdownDificultad", rowParams.transform, font, sFieldLight, null);
        RectTransform rtDropDif = dropDifGo.GetComponent<RectTransform>();
        rtDropDif.anchorMin = new Vector2(0f, 0f);
        rtDropDif.anchorMax = new Vector2(0f, 0f);
        rtDropDif.pivot = new Vector2(0f, 0f);
        rtDropDif.sizeDelta = new Vector2(colW, 32f);
        rtDropDif.anchoredPosition = Vector2.zero;
        TMP_Dropdown dropDif = dropDifGo.GetComponent<TMP_Dropdown>();
        dropDif.ClearOptions();
        dropDif.AddOptions(new List<string> { "1 - Nivel 1 (Fácil)", "2 - Nivel 2 (Medio)", "3 - Nivel 3 (Difícil)" });
        itemUI.dropdownDificultad = dropDif;

        // Columna 2: Max Intentos
        GameObject lblIntGo = CrearUIObject("LabelIntentos", rowParams.transform);
        RectTransform rtLblInt = lblIntGo.GetComponent<RectTransform>();
        rtLblInt.anchorMin = new Vector2(0f, 1f);
        rtLblInt.anchorMax = new Vector2(0f, 1f);
        rtLblInt.pivot = new Vector2(0f, 1f);
        rtLblInt.sizeDelta = new Vector2(colW, 16f);
        rtLblInt.anchoredPosition = new Vector2(222f, 0f);
        TextMeshProUGUI txtLblInt = lblIntGo.AddComponent<TextMeshProUGUI>();
        txtLblInt.font = font;
        txtLblInt.fontSize = 12f;
        txtLblInt.fontStyle = FontStyles.Bold;
        txtLblInt.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblInt.text = "Máx. Intentos:";

        GameObject inpIntGo = CrearTMPInputField("InputIntentos", rowParams.transform, font, sFieldLight, "3", 13f);
        RectTransform rtInpInt = inpIntGo.GetComponent<RectTransform>();
        rtInpInt.anchorMin = new Vector2(0f, 0f);
        rtInpInt.anchorMax = new Vector2(0f, 0f);
        rtInpInt.pivot = new Vector2(0f, 0f);
        rtInpInt.sizeDelta = new Vector2(colW, 32f);
        rtInpInt.anchoredPosition = new Vector2(222f, 0f);
        TMP_InputField inpInt = inpIntGo.GetComponent<TMP_InputField>();
        inpInt.contentType = TMP_InputField.ContentType.IntegerNumber;
        inpInt.text = "3";
        itemUI.inputMaxIntentos = inpInt;

        // Columna 3: Tiempo Máximo
        GameObject lblTmpGo = CrearUIObject("LabelTiempo", rowParams.transform);
        RectTransform rtLblTmp = lblTmpGo.GetComponent<RectTransform>();
        rtLblTmp.anchorMin = new Vector2(0f, 1f);
        rtLblTmp.anchorMax = new Vector2(0f, 1f);
        rtLblTmp.pivot = new Vector2(0f, 1f);
        rtLblTmp.sizeDelta = new Vector2(colW, 16f);
        rtLblTmp.anchoredPosition = new Vector2(444f, 0f);
        TextMeshProUGUI txtLblTmp = lblTmpGo.AddComponent<TextMeshProUGUI>();
        txtLblTmp.font = font;
        txtLblTmp.fontSize = 12f;
        txtLblTmp.fontStyle = FontStyles.Bold;
        txtLblTmp.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblTmp.text = "Tiempo Máximo (seg):";

        GameObject inpTmpGo = CrearTMPInputField("InputTiempo", rowParams.transform, font, sFieldLight, "60", 13f);
        RectTransform rtInpTmp = inpTmpGo.GetComponent<RectTransform>();
        rtInpTmp.anchorMin = new Vector2(0f, 0f);
        rtInpTmp.anchorMax = new Vector2(0f, 0f);
        rtInpTmp.pivot = new Vector2(0f, 0f);
        rtInpTmp.sizeDelta = new Vector2(colW, 32f);
        rtInpTmp.anchoredPosition = new Vector2(444f, 0f);
        TMP_InputField inpTmp = inpTmpGo.GetComponent<TMP_InputField>();
        inpTmp.contentType = TMP_InputField.ContentType.IntegerNumber;
        inpTmp.text = "60";
        itemUI.inputTiempoMaximo = inpTmp;

        // Fila 4: Elementos y Respuestas (Dos columnas equilibradas)
        GameObject rowDatos = CrearUIObject("RowDatos", root.transform);
        RectTransform rtDatos = rowDatos.GetComponent<RectTransform>();
        rtDatos.anchorMin = new Vector2(0f, 1f);
        rtDatos.anchorMax = new Vector2(1f, 1f);
        rtDatos.pivot = new Vector2(0.5f, 1f);
        rtDatos.sizeDelta = new Vector2(-30f, 52f);
        rtDatos.anchoredPosition = new Vector2(0f, -172f);

        float halfW = 320f;

        // Elementos
        GameObject lblElemGo = CrearUIObject("LabelElementos", rowDatos.transform);
        RectTransform rtLblElem = lblElemGo.GetComponent<RectTransform>();
        rtLblElem.anchorMin = new Vector2(0f, 1f);
        rtLblElem.anchorMax = new Vector2(0f, 1f);
        rtLblElem.pivot = new Vector2(0f, 1f);
        rtLblElem.sizeDelta = new Vector2(halfW, 16f);
        rtLblElem.anchoredPosition = Vector2.zero;
        TextMeshProUGUI txtLblElem = lblElemGo.AddComponent<TextMeshProUGUI>();
        txtLblElem.font = font;
        txtLblElem.fontSize = 12f;
        txtLblElem.fontStyle = FontStyles.Bold;
        txtLblElem.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblElem.text = "Elementos numéricos (ej: 2, 4, 6 o 6, 12):";

        GameObject inpElemGo = CrearTMPInputField("InputElementos", rowDatos.transform, font, sFieldLight, "Ej: 2, 4, 6, 8", 13f);
        RectTransform rtInpElem = inpElemGo.GetComponent<RectTransform>();
        rtInpElem.anchorMin = new Vector2(0f, 0f);
        rtInpElem.anchorMax = new Vector2(0f, 0f);
        rtInpElem.pivot = new Vector2(0f, 0f);
        rtInpElem.sizeDelta = new Vector2(halfW, 32f);
        rtInpElem.anchoredPosition = Vector2.zero;
        itemUI.inputElementos = inpElemGo.GetComponent<TMP_InputField>();

        // Respuestas
        GameObject lblRespGo = CrearUIObject("LabelRespuestas", rowDatos.transform);
        RectTransform rtLblResp = lblRespGo.GetComponent<RectTransform>();
        rtLblResp.anchorMin = new Vector2(0f, 1f);
        rtLblResp.anchorMax = new Vector2(0f, 1f);
        rtLblResp.pivot = new Vector2(0f, 1f);
        rtLblResp.sizeDelta = new Vector2(halfW, 16f);
        rtLblResp.anchoredPosition = new Vector2(335f, 0f);
        TextMeshProUGUI txtLblResp = lblRespGo.AddComponent<TextMeshProUGUI>();
        txtLblResp.font = font;
        txtLblResp.fontSize = 12f;
        txtLblResp.fontStyle = FontStyles.Bold;
        txtLblResp.color = new Color(0.2f, 0.26f, 0.35f, 1f);
        txtLblResp.text = "Respuesta(s) correcta(s) (ej: 10 o 1, 2, 3, 6):";

        GameObject inpRespGo = CrearTMPInputField("InputRespuestas", rowDatos.transform, font, sFieldLight, "Ej: 10 o 1, 2, 3, 6", 13f);
        RectTransform rtInpResp = inpRespGo.GetComponent<RectTransform>();
        rtInpResp.anchorMin = new Vector2(0f, 0f);
        rtInpResp.anchorMax = new Vector2(0f, 0f);
        rtInpResp.pivot = new Vector2(0f, 0f);
        rtInpResp.sizeDelta = new Vector2(halfW, 32f);
        rtInpResp.anchoredPosition = new Vector2(335f, 0f);
        itemUI.inputRespuestas = inpRespGo.GetComponent<TMP_InputField>();

        // Fila 5: Texto de Error
        GameObject txtErrGo = CrearUIObject("TextoError", root.transform);
        RectTransform rtTxtErr = txtErrGo.GetComponent<RectTransform>();
        rtTxtErr.anchorMin = new Vector2(0f, 0f);
        rtTxtErr.anchorMax = new Vector2(1f, 0f);
        rtTxtErr.pivot = new Vector2(0.5f, 0f);
        rtTxtErr.sizeDelta = new Vector2(-30f, 22f);
        rtTxtErr.anchoredPosition = new Vector2(0f, 8f);
        TextMeshProUGUI txtErr = txtErrGo.AddComponent<TextMeshProUGUI>();
        txtErr.font = font;
        txtErr.fontSize = 12f;
        txtErr.fontStyle = FontStyles.Bold;
        txtErr.color = new Color(0.85f, 0.15f, 0.15f, 1f);
        txtErr.alignment = TextAlignmentOptions.MidlineLeft;
        txtErr.text = "";
        txtErrGo.SetActive(false);
        itemUI.errorText = txtErr;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CrearUIObject(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CrearBotonConTexto(string name, Transform parent, Sprite sprite, string texto, float fontSize, TMP_FontAsset font, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = new Vector2(4f, 4f);
        rtTxt.offsetMax = new Vector2(-4f, 0f);

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = texto;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }

    private static GameObject CrearBotonSimple(string name, Transform parent, Sprite sprite, string texto, float fontSize, TMP_FontAsset font, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = size;

        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Sliced;
        img.color = Color.white;

        Button btn = go.GetComponent<Button>();
        btn.targetGraphic = img;

        GameObject txtGo = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtGo.transform.SetParent(go.transform, false);
        RectTransform rtTxt = txtGo.GetComponent<RectTransform>();
        rtTxt.anchorMin = Vector2.zero;
        rtTxt.anchorMax = Vector2.one;
        rtTxt.offsetMin = Vector2.zero;
        rtTxt.offsetMax = Vector2.zero;

        TextMeshProUGUI tmp = txtGo.GetComponent<TextMeshProUGUI>();
        tmp.font = font;
        tmp.text = texto;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        return go;
    }

    private static GameObject CrearTMPInputField(string name, Transform parent, TMP_FontAsset font, Sprite sprite, string placeholderText, float fontSize, bool multiline = false, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft)
    {
        var resources = new TMP_DefaultControls.Resources();
        GameObject inpGo = TMP_DefaultControls.CreateInputField(resources);
        inpGo.name = name;
        inpGo.transform.SetParent(parent, false);

        Image img = inpGo.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_InputField inputField = inpGo.GetComponent<TMP_InputField>();

        // Margen y texto
        Transform textArea = inpGo.transform.Find("Text Area");
        if (textArea != null)
        {
            RectTransform rtTA = textArea.GetComponent<RectTransform>();
            rtTA.offsetMin = new Vector2(8f, multiline ? 6f : 2f);
            rtTA.offsetMax = new Vector2(-8f, multiline ? -6f : -2f);
        }

        TMP_Text textComp = inputField.textComponent;
        if (textComp != null)
        {
            textComp.font = font;
            textComp.fontSize = fontSize;
            textComp.color = new Color(0.06f, 0.09f, 0.15f, 1f);
            textComp.alignment = alignment;
        }

        if (inputField.placeholder is TextMeshProUGUI placeholderComp)
        {
            placeholderComp.font = font;
            placeholderComp.fontSize = fontSize;
            placeholderComp.text = placeholderText;
            placeholderComp.color = new Color(0.42f, 0.48f, 0.58f, 1f); // Contraste visible y distinguible
            placeholderComp.fontStyle = FontStyles.Italic;
            placeholderComp.alignment = alignment;
        }

        return inpGo;
    }

    private static GameObject CrearTMPDropdown(string name, Transform parent, TMP_FontAsset font, Sprite sprite, Sprite sScrollHandle)
    {
        var resources = new TMP_DefaultControls.Resources();
        GameObject dropGo = TMP_DefaultControls.CreateDropdown(resources);
        dropGo.name = name;
        dropGo.transform.SetParent(parent, false);

        Image img = dropGo.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
        }

        TMP_Dropdown dropdown = dropGo.GetComponent<TMP_Dropdown>();
        if (dropdown.captionText != null)
        {
            dropdown.captionText.font = font;
            dropdown.captionText.fontSize = 13.5f;
            dropdown.captionText.fontStyle = FontStyles.Bold;
            dropdown.captionText.color = new Color(0.08f, 0.12f, 0.2f, 1f);
            RectTransform rtCap = dropdown.captionText.GetComponent<RectTransform>();
            rtCap.offsetMin = new Vector2(8f, 0f);
            rtCap.offsetMax = new Vector2(-28f, 0f);
        }

        // Flecha estilizada con icono textual nítido
        Transform arrow = dropGo.transform.Find("Arrow");
        if (arrow != null)
        {
            Image arrowImg = arrow.GetComponent<Image>();
            if (arrowImg != null)
            {
                Object.DestroyImmediate(arrowImg);
            }
            TextMeshProUGUI arrowTxt = arrow.gameObject.AddComponent<TextMeshProUGUI>();
            arrowTxt.font = font;
            arrowTxt.text = "▼";
            arrowTxt.fontSize = 10f;
            arrowTxt.color = new Color(0.2f, 0.28f, 0.4f, 1f);
            arrowTxt.alignment = TextAlignmentOptions.Center;
            RectTransform rtArr = arrow.GetComponent<RectTransform>();
            rtArr.sizeDelta = new Vector2(20f, 20f);
            rtArr.anchoredPosition = new Vector2(-10f, 0f);
        }

        // Template Scrollbar
        Transform template = dropGo.transform.Find("Template");
        if (template != null)
        {
            Image imgTpl = template.GetComponent<Image>();
            if (imgTpl != null)
            {
                imgTpl.sprite = sprite;
                imgTpl.type = Image.Type.Sliced;
                imgTpl.color = Color.white;
            }

            Transform scrollbar = template.Find("Scrollbar");
            if (scrollbar != null)
            {
                Image sbImg = scrollbar.GetComponent<Image>();
                if (sbImg != null) sbImg.color = new Color(0f, 0f, 0f, 0.1f);
                Transform handle = scrollbar.Find("Sliding Area/Handle");
                if (handle != null)
                {
                    Image hImg = handle.GetComponent<Image>();
                    if (hImg != null && sScrollHandle != null)
                    {
                        hImg.sprite = sScrollHandle;
                        hImg.type = Image.Type.Sliced;
                    }
                }
            }
        }

        return dropGo;
    }

    private static GameObject CrearToggleLimpio(string name, Transform parent, Sprite sCheckBg, Sprite sCheckMark)
    {
        GameObject toggleGo = DefaultControls.CreateToggle(new DefaultControls.Resources());
        toggleGo.name = name;
        toggleGo.transform.SetParent(parent, false);
        RectTransform rt = toggleGo.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(24f, 24f);

        Transform lbl = toggleGo.transform.Find("Label");
        if (lbl != null) Object.DestroyImmediate(lbl.gameObject);

        Transform bg = toggleGo.transform.Find("Background");
        if (bg != null)
        {
            RectTransform rtBg = bg.GetComponent<RectTransform>();
            rtBg.anchorMin = Vector2.zero;
            rtBg.anchorMax = Vector2.one;
            rtBg.offsetMin = Vector2.zero;
            rtBg.offsetMax = Vector2.zero;

            Image imgBg = bg.GetComponent<Image>();
            imgBg.sprite = sCheckBg;
            imgBg.type = Image.Type.Sliced;
            imgBg.color = Color.white;

            Transform cm = bg.Find("Checkmark");
            if (cm != null)
            {
                Image imgCm = cm.GetComponent<Image>();
                imgCm.sprite = sCheckMark;
                imgCm.type = Image.Type.Simple;
                imgCm.color = Color.white;
                RectTransform rtCm = cm.GetComponent<RectTransform>();
                rtCm.anchorMin = Vector2.zero;
                rtCm.anchorMax = Vector2.one;
                rtCm.offsetMin = new Vector2(2f, 2f);
                rtCm.offsetMax = new Vector2(-2f, -2f);
            }
        }

        return toggleGo;
    }

    private static GameObject CrearScrollViewLimpio(string name, Transform parent, Sprite sScrollHandle, out Transform contentTransform)
    {
        var resources = new DefaultControls.Resources();
        GameObject scrollGo = DefaultControls.CreateScrollView(resources);
        scrollGo.name = name;
        scrollGo.transform.SetParent(parent, false);

        ScrollRect sr = scrollGo.GetComponent<ScrollRect>();
        sr.horizontal = false;
        sr.vertical = true;
        sr.movementType = ScrollRect.MovementType.Clamped;

        // Eliminar completamente la barra horizontal blanca
        if (sr.horizontalScrollbar != null)
        {
            GameObject sbHoriz = sr.horizontalScrollbar.gameObject;
            sr.horizontalScrollbar = null;
            Object.DestroyImmediate(sbHoriz);
        }

        // Fondo transparente del scroll general
        Image img = scrollGo.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(0f, 0f, 0f, 0.12f);
        }

        // Viewport
        Transform viewport = scrollGo.transform.Find("Viewport");
        if (viewport != null)
        {
            RectTransform rtVp = viewport.GetComponent<RectTransform>();
            rtVp.offsetMin = Vector2.zero;
            rtVp.offsetMax = new Vector2(-12f, 0f); // Dejar espacio para scrollbar
        }

        // Configurar Scrollbar Vertical discreto
        if (sr.verticalScrollbar != null)
        {
            RectTransform rtSb = sr.verticalScrollbar.GetComponent<RectTransform>();
            rtSb.offsetMin = new Vector2(-10f, 0f);
            rtSb.offsetMax = Vector2.zero;

            Image imgSb = sr.verticalScrollbar.GetComponent<Image>();
            if (imgSb != null)
            {
                imgSb.color = new Color(0f, 0f, 0f, 0.25f);
            }

            Transform handle = sr.verticalScrollbar.transform.Find("Sliding Area/Handle");
            if (handle != null)
            {
                Image imgHandle = handle.GetComponent<Image>();
                if (imgHandle != null && sScrollHandle != null)
                {
                    imgHandle.sprite = sScrollHandle;
                    imgHandle.type = Image.Type.Sliced;
                    imgHandle.color = Color.white;
                }
            }
        }

        Transform content = viewport != null ? viewport.Find("Content") : null;
        contentTransform = content;

        if (content != null)
        {
            RectTransform rtContent = content.GetComponent<RectTransform>();
            rtContent.anchorMin = new Vector2(0f, 1f);
            rtContent.anchorMax = new Vector2(1f, 1f);
            rtContent.pivot = new Vector2(0.5f, 1f);
            rtContent.offsetMin = Vector2.zero;
            rtContent.offsetMax = Vector2.zero;

            VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(6, 6, 6, 6);
            vlg.spacing = 8;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        return scrollGo;
    }
}
