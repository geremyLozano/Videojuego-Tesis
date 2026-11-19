using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DocenteEjercicioItemUI : MonoBehaviour
{
    [Header("Identificación")]
    public TMP_Text tituloText;
    public TMP_Text idAsignadoText;

    [Header("Campos de Entrada")]
    public TMP_InputField inputEnunciado;
    public TMP_Dropdown dropdownDificultad;
    public TMP_InputField inputElementos;
    public TMP_InputField inputRespuestas;
    public TMP_InputField inputMaxIntentos;
    public TMP_InputField inputTiempoMaximo;

    [Header("Retroalimentación")]
    public TMP_Text errorText;

    [HideInInspector] public string curso;
    [HideInInspector] public string tema;
    [HideInInspector] public int autoId;
    [HideInInspector] public int indiceEnTema;
    [HideInInspector] public bool aprobadoDuplicado = false;

    public void Configurar(string curso, string tema, int indice, int total, int autoId)
    {
        this.curso = curso;
        this.tema = tema;
        this.indiceEnTema = indice;
        this.autoId = autoId;
        this.aprobadoDuplicado = false;

        if (tituloText != null)
        {
            tituloText.text = $"{curso} > {tema} — Ejercicio {indice} de {total}";
        }

        if (idAsignadoText != null)
        {
            idAsignadoText.text = $"ID: {autoId}";
        }

        if (dropdownDificultad != null)
        {
            dropdownDificultad.value = 0; // Nivel 1 por defecto
        }

        if (inputMaxIntentos != null && string.IsNullOrEmpty(inputMaxIntentos.text))
        {
            inputMaxIntentos.text = "3";
        }

        if (inputTiempoMaximo != null && string.IsNullOrEmpty(inputTiempoMaximo.text))
        {
            inputTiempoMaximo.text = "60";
        }

        LimpiarError();
    }

    public void ActualizarId(int nuevoId)
    {
        this.autoId = nuevoId;
        if (idAsignadoText != null)
        {
            idAsignadoText.text = $"ID: {nuevoId}";
        }
    }

    public void MostrarError(string mensaje)
    {
        if (errorText != null)
        {
            errorText.text = mensaje;
            errorText.gameObject.SetActive(!string.IsNullOrEmpty(mensaje));
        }
    }

    public void LimpiarError()
    {
        if (errorText != null)
        {
            errorText.text = string.Empty;
            errorText.gameObject.SetActive(false);
        }
    }

    public Ejercicio ObtenerEjercicioDraft()
    {
        Ejercicio ej = new Ejercicio();
        ej.id = autoId;
        ej.dificultad = dropdownDificultad != null ? dropdownDificultad.value + 1 : 1;
        ej.enunciado = inputEnunciado != null ? inputEnunciado.text.Trim() : string.Empty;

        if (DocenteValidation.TryParseElementos(inputElementos != null ? inputElementos.text : "", out int[] elems, out _))
        {
            ej.elementos = elems;
        }
        else
        {
            ej.elementos = new int[0];
        }

        if (DocenteValidation.TryParseRespuestas(inputRespuestas != null ? inputRespuestas.text : "", out float[] resps, out _))
        {
            ej.respuestaCorrecta = resps;
        }
        else
        {
            ej.respuestaCorrecta = new float[0];
        }

        int intentos = 3;
        if (inputMaxIntentos != null && int.TryParse(inputMaxIntentos.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valInt))
        {
            intentos = valInt;
        }
        ej.maxIntentos = intentos;

        int tiempo = 60;
        if (inputTiempoMaximo != null && int.TryParse(inputTiempoMaximo.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valTmp))
        {
            tiempo = valTmp;
        }
        ej.tiempoMaximo = tiempo;

        return ej;
    }

    public bool Validar(out Ejercicio ejercicioValido, out string mensajeError)
    {
        ejercicioValido = null;
        mensajeError = string.Empty;

        // 1. Enunciado
        string enunciado = inputEnunciado != null ? inputEnunciado.text.Trim() : string.Empty;
        if (string.IsNullOrWhiteSpace(enunciado))
        {
            mensajeError = "El enunciado no puede estar vacío.";
            MostrarError(mensajeError);
            return false;
        }

        // 2. Dificultad (1 a 3)
        int dificultad = dropdownDificultad != null ? dropdownDificultad.value + 1 : 1;
        if (dificultad < 1 || dificultad > 3)
        {
            mensajeError = "La dificultad debe ser 1 (Fácil), 2 (Medio) o 3 (Difícil).";
            MostrarError(mensajeError);
            return false;
        }

        // 3. Elementos numéricos
        string elementosTxt = inputElementos != null ? inputElementos.text : string.Empty;
        if (!DocenteValidation.TryParseElementos(elementosTxt, out int[] elementos, out string errElem))
        {
            mensajeError = $"Elementos inválidos: {errElem}";
            MostrarError(mensajeError);
            return false;
        }

        // 4. Respuestas correctas
        string respuestasTxt = inputRespuestas != null ? inputRespuestas.text : string.Empty;
        if (!DocenteValidation.TryParseRespuestas(respuestasTxt, out float[] respuestas, out string errResp))
        {
            mensajeError = $"Respuesta inválida: {errResp}";
            MostrarError(mensajeError);
            return false;
        }

        // 5. Max intentos
        string intentosTxt = inputMaxIntentos != null ? inputMaxIntentos.text.Trim() : "3";
        if (!int.TryParse(intentosTxt, NumberStyles.Integer, CultureInfo.InvariantCulture, out int maxIntentos) || maxIntentos <= 0)
        {
            mensajeError = "El máximo de intentos debe ser un número entero mayor a 0.";
            MostrarError(mensajeError);
            return false;
        }

        // 6. Tiempo máximo
        string tiempoTxt = inputTiempoMaximo != null ? inputTiempoMaximo.text.Trim() : "60";
        if (!int.TryParse(tiempoTxt, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tiempoMaximo) || tiempoMaximo <= 0)
        {
            mensajeError = "El tiempo máximo debe ser un número entero mayor a 0 segundos.";
            MostrarError(mensajeError);
            return false;
        }

        LimpiarError();

        ejercicioValido = new Ejercicio
        {
            id = autoId,
            dificultad = dificultad,
            enunciado = enunciado,
            elementos = elementos,
            respuestaCorrecta = respuestas,
            maxIntentos = maxIntentos,
            tiempoMaximo = tiempoMaximo
        };

        return true;
    }
}
