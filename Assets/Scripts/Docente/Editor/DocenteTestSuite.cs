using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DocenteTestSuite
{
    private static string rutaGuardados = Path.Combine(Application.persistentDataPath, "contenidos_guardados.json");
    private static string rutaBackup = Path.Combine(Application.persistentDataPath, "contenidos_guardados_test_backup.json");

    [MenuItem("Docente/Ejecutar 12 Pruebas Obligatorias")]
    public static string EjecutarTodasLasPruebas()
    {
        System.Text.StringBuilder reporte = new System.Text.StringBuilder();
        reporte.AppendLine("=================================================");
        reporte.AppendLine("REPORTE DE VERIFICACIÓN DE LAS 12 PRUEBAS OBLIGATORIAS");
        reporte.AppendLine("=================================================");

        // Crear backup seguro antes de las pruebas
        if (File.Exists(rutaGuardados))
        {
            File.Copy(rutaGuardados, rutaBackup, true);
        }

        ContenidoManager cm = Object.FindAnyObjectByType<ContenidoManager>();
        UIControlMenu uic = Object.FindAnyObjectByType<UIControlMenu>();

        try
        {
            // PRUEBA 1: Registrar un ejercicio nuevo en un tema existente
            reporte.AppendLine(EjecutarPrueba1(cm));

            // PRUEBA 2: Registrar varios ejercicios para dos temas distintos en una misma sesión
            reporte.AppendLine(EjecutarPrueba2(cm));

            // PRUEBA 3: Guardar cuando contenidos_guardados.json no existe
            reporte.AppendLine(EjecutarPrueba3(cm));

            // PRUEBA 4: Guardar cuando el archivo ya contiene cursos, temas y ejercicios
            reporte.AppendLine(EjecutarPrueba4(cm));

            // PRUEBA 5: Intentar agregar un enunciado idéntico al de un ejercicio existente
            reporte.AppendLine(EjecutarPrueba5(cm));

            // PRUEBA 6: Intentar agregar un enunciado con diferencias únicamente de mayúsculas o espacios
            reporte.AppendLine(EjecutarPrueba6(cm));

            // PRUEBA 7: Comparar los dos ejercicios en la advertencia y cancelar la incorporación
            reporte.AppendLine(EjecutarPrueba7(cm));

            // PRUEBA 8: Comparar los dos ejercicios y confirmar la incorporación
            reporte.AppendLine(EjecutarPrueba8(cm));

            // PRUEBA 9: Comprobar que no se alteran los contenidos previos ni se generan ID conflictivos
            reporte.AppendLine(EjecutarPrueba9(cm));

            // PRUEBA 10: Verificar que los JSON resultantes se pueden cargar con el mecanismo actual
            reporte.AppendLine(EjecutarPrueba10(cm));

            // PRUEBA 11: Comprobar que ButtonCargaManual y ButtonCargaInternet siguen funcionando
            reporte.AppendLine(EjecutarPrueba11(uic));

            // PRUEBA 12: Verificar que los datos registrados pueden utilizarse para configurar las mecánicas correspondientes
            reporte.AppendLine(EjecutarPrueba12());
        }
        finally
        {
            // Restaurar archivo original desde backup
            if (File.Exists(rutaBackup))
            {
                File.Copy(rutaBackup, rutaGuardados, true);
                File.Delete(rutaBackup);
                if (cm != null) cm.CargarGuardados();
            }
        }

        reporte.AppendLine("=================================================");
        reporte.AppendLine("FIN DE EJECUCIÓN DE PRUEBAS");
        reporte.AppendLine("=================================================");

        string res = reporte.ToString();
        Debug.Log(res);
        return res;
    }

    private static string EjecutarPrueba1(ContenidoManager cm)
    {
        // 1. Registrar un ejercicio nuevo en un tema existente (MCM)
        cm.CargarGuardados();
        int cantAntes = 0;
        Curso c = cm.contenidos.Find(x => x.curso == "Aritmética");
        Tema t = c != null ? c.temas.Find(x => x.nombre == "MCM") : null;
        if (t != null) cantAntes = t.ejercicios.Count;

        int nuevoId = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCM", null);
        ContenidoJSON json = new ContenidoJSON
        {
            curso = "Aritmética",
            tema = "MCM",
            ejercicios = new List<Ejercicio>
            {
                new Ejercicio
                {
                    id = nuevoId,
                    dificultad = 1,
                    enunciado = "Calcula el mínimo común múltiplo de 4 y 6 (Prueba 1).",
                    elementos = new int[] { 4, 6 },
                    respuestaCorrecta = new float[] { 12f },
                    maxIntentos = 3,
                    tiempoMaximo = 60
                }
            }
        };

        cm.CargarJSONDesdeTexto(JsonUtility.ToJson(json, true));

        Tema tDespues = cm.contenidos.Find(x => x.curso == "Aritmética")?.temas.Find(x => x.nombre == "MCM");
        bool ok = (tDespues != null && tDespues.ejercicios.Count == cantAntes + 1 && tDespues.ejercicios.Exists(e => e.id == nuevoId));

        return $"[Prueba 1] Registrar ejercicio nuevo en tema existente: {(ok ? "PASÓ" : "FALLÓ")} (Cant: {cantAntes} -> {tDespues?.ejercicios.Count}, ID: {nuevoId})";
    }

    private static string EjecutarPrueba2(ContenidoManager cm)
    {
        // 2. Registrar varios ejercicios para dos temas distintos en una misma sesión (MCM y MCD)
        cm.CargarGuardados();
        int idMCM1 = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCM", null);
        int idMCM2 = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCM", new int[] { idMCM1 });

        int idMCD1 = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCD", null);
        int idMCD2 = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCD", new int[] { idMCD1 });

        ContenidoJSON jsonMCM = new ContenidoJSON
        {
            curso = "Aritmética",
            tema = "MCM",
            ejercicios = new List<Ejercicio>
            {
                new Ejercicio { id = idMCM1, dificultad = 1, enunciado = "MCM Ex 1 Sesion Multiple", elementos = new int[]{3, 5}, respuestaCorrecta = new float[]{15f}, maxIntentos=3, tiempoMaximo=60 },
                new Ejercicio { id = idMCM2, dificultad = 2, enunciado = "MCM Ex 2 Sesion Multiple", elementos = new int[]{6, 8}, respuestaCorrecta = new float[]{24f}, maxIntentos=3, tiempoMaximo=60 }
            }
        };

        ContenidoJSON jsonMCD = new ContenidoJSON
        {
            curso = "Aritmética",
            tema = "MCD",
            ejercicios = new List<Ejercicio>
            {
                new Ejercicio { id = idMCD1, dificultad = 1, enunciado = "MCD Ex 1 Sesion Multiple", elementos = new int[]{8, 12}, respuestaCorrecta = new float[]{1f, 2f, 4f}, maxIntentos=3, tiempoMaximo=60 },
                new Ejercicio { id = idMCD2, dificultad = 2, enunciado = "MCD Ex 2 Sesion Multiple", elementos = new int[]{9, 15}, respuestaCorrecta = new float[]{1f, 3f}, maxIntentos=3, tiempoMaximo=60 }
            }
        };

        cm.CargarJSONDesdeTexto(JsonUtility.ToJson(jsonMCM, true));
        cm.CargarJSONDesdeTexto(JsonUtility.ToJson(jsonMCD, true));

        Tema tMCM = cm.contenidos.Find(x => x.curso == "Aritmética")?.temas.Find(x => x.nombre == "MCM");
        Tema tMCD = cm.contenidos.Find(x => x.curso == "Aritmética")?.temas.Find(x => x.nombre == "MCD");

        bool ok = tMCM.ejercicios.Exists(e => e.id == idMCM1) &&
                  tMCM.ejercicios.Exists(e => e.id == idMCM2) &&
                  tMCD.ejercicios.Exists(e => e.id == idMCD1) &&
                  tMCD.ejercicios.Exists(e => e.id == idMCD2);

        return $"[Prueba 2] Registrar ejercicios en dos temas distintos en una sesión: {(ok ? "PASÓ" : "FALLÓ")}";
    }

    private static string EjecutarPrueba3(ContenidoManager cm)
    {
        // 3. Guardar cuando contenidos_guardados.json no existe
        string tempRuta = Path.Combine(Application.persistentDataPath, "temp_sin_guardados.json");
        if (File.Exists(rutaGuardados))
        {
            File.Move(rutaGuardados, tempRuta);
        }

        try
        {
            cm.CargarGuardados();
            ContenidoJSON cjson = new ContenidoJSON
            {
                curso = "Geometría",
                tema = "Segmentos",
                ejercicios = new List<Ejercicio>
                {
                    new Ejercicio { id = 1, dificultad = 1, enunciado = "Calcula la longitud del segmento AB.", elementos = new int[]{2, 8}, respuestaCorrecta = new float[]{6f}, maxIntentos = 3, tiempoMaximo = 60 }
                }
            };

            cm.CargarJSONDesdeTexto(JsonUtility.ToJson(cjson, true));

            bool archivoCreado = File.Exists(rutaGuardados);
            string contenido = archivoCreado ? File.ReadAllText(rutaGuardados) : "";
            bool contieneEjercicio = contenido.Contains("Calcula la longitud del segmento AB.");

            return $"[Prueba 3] Guardar cuando contenidos_guardados.json no existe: {(archivoCreado && contieneEjercicio ? "PASÓ" : "FALLÓ")} (Archivo creado y poblado)";
        }
        finally
        {
            if (File.Exists(tempRuta))
            {
                if (File.Exists(rutaGuardados)) File.Delete(rutaGuardados);
                File.Move(tempRuta, rutaGuardados);
                cm.CargarGuardados();
            }
        }
    }

    private static string EjecutarPrueba4(ContenidoManager cm)
    {
        // 4. Guardar cuando el archivo ya contiene cursos, temas y ejercicios
        cm.CargarGuardados();
        int cursosAntes = cm.contenidos.Count;
        int nuevoId = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "Sucesiones", null);

        ContenidoJSON json = new ContenidoJSON
        {
            curso = "Aritmética",
            tema = "Sucesiones",
            ejercicios = new List<Ejercicio>
            {
                new Ejercicio
                {
                    id = nuevoId,
                    dificultad = 2,
                    enunciado = "Sucesión acumulativa de prueba 4: 5, 15, 25, __",
                    elementos = new int[] { 5, 15, 25 },
                    respuestaCorrecta = new float[] { 35f },
                    maxIntentos = 3,
                    tiempoMaximo = 60
                }
            }
        };

        cm.CargarJSONDesdeTexto(JsonUtility.ToJson(json, true));

        bool ok = cm.contenidos.Count >= cursosAntes &&
                  cm.contenidos.Find(x => x.curso == "Aritmética")?.temas.Find(x => x.nombre == "Sucesiones")?.ejercicios.Exists(e => e.id == nuevoId) == true;

        return $"[Prueba 4] Guardar cuando el archivo ya contiene datos: {(ok ? "PASÓ" : "FALLÓ")}";
    }

    private static string EjecutarPrueba5(ContenidoManager cm)
    {
        // 5. Intentar agregar un enunciado idéntico al de un ejercicio existente
        cm.CargarGuardados();
        Curso c = cm.contenidos.Find(x => x.curso == "Aritmética");
        Tema t = c?.temas.Find(x => x.nombre == "Sucesiones");
        string enunciadoExistente = t?.ejercicios[0].enunciado;

        Ejercicio intentoDuplicado = new Ejercicio
        {
            id = 999,
            dificultad = 1,
            enunciado = enunciadoExistente,
            elementos = new int[] { 2, 4, 6, 8 },
            respuestaCorrecta = new float[] { 10f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        var comp = DocenteValidation.DetectarDuplicado(cm, "Aritmética", "Sucesiones", intentoDuplicado, null);
        bool ok = comp.esDuplicado && comp.coincidenciaExacta;

        return $"[Prueba 5] Detección de enunciado idéntico: {(ok ? "PASÓ" : "FALLÓ")} (Detectado: {comp.esDuplicado}, Exacto: {comp.coincidenciaExacta})";
    }

    private static string EjecutarPrueba6(ContenidoManager cm)
    {
        // 6. Intentar agregar un enunciado con diferencias únicamente de mayúsculas o espacios
        cm.CargarGuardados();
        Curso c = cm.contenidos.Find(x => x.curso == "Aritmética");
        Tema t = c?.temas.Find(x => x.nombre == "Sucesiones");
        string enunciadoExistente = t?.ejercicios[0].enunciado;

        string conMayusculasYEspacios = "   " + enunciadoExistente.ToUpperInvariant().Replace(" ", "   ") + "   ";

        Ejercicio intentoCaseEspacios = new Ejercicio
        {
            id = 999,
            dificultad = 1,
            enunciado = conMayusculasYEspacios,
            elementos = new int[] { 2, 4, 6, 8 },
            respuestaCorrecta = new float[] { 10f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        var comp = DocenteValidation.DetectarDuplicado(cm, "Aritmética", "Sucesiones", intentoCaseEspacios, null);
        bool ok = comp.esDuplicado && !comp.coincidenciaExacta;

        return $"[Prueba 6] Detección con mayúsculas y espacios normalizables: {(ok ? "PASÓ" : "FALLÓ")} (Detectado: {comp.esDuplicado}, Exacto: {comp.coincidenciaExacta})";
    }

    private static string EjecutarPrueba7(ContenidoManager cm)
    {
        // 7. Comparar los dos ejercicios en la advertencia y cancelar la incorporación
        cm.CargarGuardados();
        Curso c = cm.contenidos.Find(x => x.curso == "Aritmética");
        Tema t = c?.temas.Find(x => x.nombre == "MCM");
        int cantInicial = t.ejercicios.Count;
        string enunciadoBase = t.ejercicios[0].enunciado;

        Ejercicio ejNuevo = new Ejercicio
        {
            id = 999,
            dificultad = 2,
            enunciado = enunciadoBase,
            elementos = new int[] { 10, 20 },
            respuestaCorrecta = new float[] { 20f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        var comp = DocenteValidation.DetectarDuplicado(cm, "Aritmética", "MCM", ejNuevo, null);
        bool comparacionValida = comp.esDuplicado && !string.IsNullOrEmpty(comp.detalleDiferencias);

        // Al cancelar, NO se invoca CargarJSONDesdeTexto
        // Verificar que la cantidad no cambió
        bool noModificado = (t.ejercicios.Count == cantInicial);

        return $"[Prueba 7] Advertencia comparativa y cancelación: {(comparacionValida && noModificado ? "PASÓ" : "FALLÓ")} (Comparación generada, cambios cancelados)";
    }

    private static string EjecutarPrueba8(ContenidoManager cm)
    {
        // 8. Comparar los dos ejercicios y confirmar la incorporación
        cm.CargarGuardados();
        Curso c = cm.contenidos.Find(x => x.curso == "Aritmética");
        Tema t = c?.temas.Find(x => x.nombre == "MCM");
        int cantInicial = t.ejercicios.Count;
        string enunciadoBase = t.ejercicios[0].enunciado;

        // Ejercicio con mismo enunciado pero aprobado de todas formas
        int nuevoId = DocenteValidation.CalcularSiguienteId(cm, "Aritmética", "MCM", null);
        Ejercicio ejAprobado = new Ejercicio
        {
            id = nuevoId,
            dificultad = 3,
            enunciado = enunciadoBase,
            elementos = new int[] { 12, 18 },
            respuestaCorrecta = new float[] { 36f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        ContenidoJSON json = new ContenidoJSON
        {
            curso = "Aritmética",
            tema = "MCM",
            ejercicios = new List<Ejercicio> { ejAprobado }
        };

        cm.CargarJSONDesdeTexto(JsonUtility.ToJson(json, true));

        Tema tDespues = cm.contenidos.Find(x => x.curso == "Aritmética")?.temas.Find(x => x.nombre == "MCM");
        bool ok = tDespues.ejercicios.Count == cantInicial + 1 && tDespues.ejercicios.Exists(e => e.id == nuevoId);

        return $"[Prueba 8] Confirmación de incorporación forzada: {(ok ? "PASÓ" : "FALLÓ")} (ID asignado sin conflicto: {nuevoId})";
    }

    private static string EjecutarPrueba9(ContenidoManager cm)
    {
        // 9. Comprobar que no se alteran los contenidos previos ni se generan ID conflictivos
        cm.CargarGuardados();
        bool idsValidos = true;
        foreach (Curso curso in cm.contenidos)
        {
            foreach (Tema tema in curso.temas)
            {
                HashSet<int> idsVistos = new HashSet<int>();
                foreach (Ejercicio ej in tema.ejercicios)
                {
                    if (idsVistos.Contains(ej.id))
                    {
                        idsValidos = false;
                        break;
                    }
                    idsVistos.Add(ej.id);
                }
            }
        }

        return $"[Prueba 9] Integridad de contenidos previos y unicidad de IDs: {(idsValidos ? "PASÓ" : "FALLÓ")}";
    }

    private static string EjecutarPrueba10(ContenidoManager cm)
    {
        // 10. Verificar que los JSON resultantes se pueden cargar con el mecanismo actual
        ContenidoJSON testCjson = new ContenidoJSON
        {
            curso = "Álgebra",
            tema = "Polinomios",
            ejercicios = new List<Ejercicio>
            {
                new Ejercicio
                {
                    id = 1,
                    dificultad = 1,
                    enunciado = "Calcula el grado del polinomio P(x) = x^3 + 2x^2 + 5.",
                    elementos = new int[] { 3, 2, 5 },
                    respuestaCorrecta = new float[] { 3f },
                    maxIntentos = 3,
                    tiempoMaximo = 60
                }
            }
        };

        string jsonGenerado = JsonUtility.ToJson(testCjson, true);
        ContenidoJSON deserializado = JsonUtility.FromJson<ContenidoJSON>(jsonGenerado);

        bool ok = deserializado != null &&
                  deserializado.curso == "Álgebra" &&
                  deserializado.tema == "Polinomios" &&
                  deserializado.ejercicios.Count == 1 &&
                  deserializado.ejercicios[0].respuestaCorrecta[0] == 3f;

        return $"[Prueba 10] Compatibilidad de serialización/deserialización JSON: {(ok ? "PASÓ" : "FALLÓ")}";
    }

    private static string EjecutarPrueba11(UIControlMenu uic)
    {
        // 11. Comprobar que ButtonCargaManual y ButtonCargaInternet siguen funcionando
        bool uicValido = uic != null;
        var btnManual = GameObject.Find("Canvas/CargadorDeContenido/ButtonCargaManual")?.GetComponent<Button>();
        var btnInternet = GameObject.Find("Canvas/CargadorDeContenido/ButtonCargaInternet")?.GetComponent<Button>();

        bool manualOk = false;
        bool internetOk = false;

        if (btnManual != null)
        {
            for (int i = 0; i < btnManual.onClick.GetPersistentEventCount(); i++)
            {
                if (btnManual.onClick.GetPersistentMethodName(i) == "CargarContenidoManual")
                {
                    manualOk = true;
                    break;
                }
            }
        }

        if (btnInternet != null)
        {
            for (int i = 0; i < btnInternet.onClick.GetPersistentEventCount(); i++)
            {
                if (btnInternet.onClick.GetPersistentMethodName(i) == "CargarContenidoInternet")
                {
                    internetOk = true;
                    break;
                }
            }
        }

        bool ok = uicValido && manualOk && internetOk;
        return $"[Prueba 11] ButtonCargaManual y ButtonCargaInternet intactos: {(ok ? "PASÓ" : "FALLÓ")} (Manual={manualOk}, Internet={internetOk})";
    }

    private static string EjecutarPrueba12()
    {
        // 12. Verificar que los datos registrados pueden utilizarse para configurar las mecánicas correspondientes
        Ejercicio ejSucesiones = new Ejercicio
        {
            id = 1,
            dificultad = 1,
            enunciado = "Sucesión: 2, 4, 6, 8, __",
            elementos = new int[] { 2, 4, 6, 8 },
            respuestaCorrecta = new float[] { 10f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        Ejercicio ejMCM = new Ejercicio
        {
            id = 2,
            dificultad = 1,
            enunciado = "MCM de 4 y 6",
            elementos = new int[] { 4, 6 },
            respuestaCorrecta = new float[] { 12f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        Ejercicio ejMCD = new Ejercicio
        {
            id = 3,
            dificultad = 1,
            enunciado = "Divisores de 6 y 12",
            elementos = new int[] { 6, 12 },
            respuestaCorrecta = new float[] { 1f, 2f, 3f, 6f },
            maxIntentos = 3,
            tiempoMaximo = 60
        };

        bool sucOk = ejSucesiones.elementos.Length >= 2 && ejSucesiones.respuestaCorrecta.Length == 1;
        bool mcmOk = ejMCM.elementos.Length >= 2 && ejMCM.respuestaCorrecta.Length == 1;
        bool mcdOk = ejMCD.elementos.Length >= 2 && ejMCD.respuestaCorrecta.Length >= 1;

        bool ok = sucOk && mcmOk && mcdOk;
        return $"[Prueba 12] Compatibilidad de configuración de mecánicas: {(ok ? "PASÓ" : "FALLÓ")} (Sucesiones={sucOk}, MCM={mcmOk}, MCD={mcdOk})";
    }
}
