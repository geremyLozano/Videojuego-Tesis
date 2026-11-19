using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public static class DocenteValidation
{
    public class ComparacionDuplicado
    {
        public bool esDuplicado;
        public bool coincidenciaExacta;
        public Ejercicio existente;
        public Ejercicio nuevo;
        public string tema;
        public string curso;
        public string detalleDiferencias;
    }

    public static string NormalizarEnunciado(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;
        string trimmed = texto.Trim();
        string singleSpaced = Regex.Replace(trimmed, @"\s+", " ");
        return singleSpaced.ToLowerInvariant();
    }

    public static bool TryParseElementos(string input, out int[] resultado, out string error)
    {
        resultado = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Debe ingresar al menos un elemento numérico.";
            return false;
        }

        string clean = input.Replace(';', ',');
        string[] tokens = clean.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 0)
        {
            error = "Debe ingresar al menos un elemento numérico.";
            return false;
        }

        List<int> lista = new List<int>();
        foreach (string token in tokens)
        {
            string item = token.Trim();
            if (int.TryParse(item, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            {
                lista.Add(val);
            }
            else
            {
                error = $"'{item}' no es un número entero válido.";
                return false;
            }
        }

        resultado = lista.ToArray();
        return true;
    }

    public static bool TryParseRespuestas(string input, out float[] resultado, out string error)
    {
        resultado = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            error = "Debe ingresar al menos una respuesta correcta.";
            return false;
        }

        string clean = input.Replace(';', ',');
        string[] tokens = clean.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

        if (tokens.Length == 1 && tokens[0].Trim().Contains(" "))
        {
            tokens = tokens[0].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        }

        if (tokens.Length == 0)
        {
            error = "Debe ingresar al menos una respuesta correcta.";
            return false;
        }

        List<float> lista = new List<float>();
        foreach (string token in tokens)
        {
            string item = token.Trim();
            if (float.TryParse(item, NumberStyles.Float, CultureInfo.InvariantCulture, out float val) ||
                float.TryParse(item, NumberStyles.Float, CultureInfo.CurrentCulture, out val))
            {
                lista.Add(val);
            }
            else
            {
                error = $"'{item}' no es un número válido.";
                return false;
            }
        }

        resultado = lista.ToArray();
        return true;
    }

    public static int CalcularSiguienteId(ContenidoManager manager, string curso, string tema, IEnumerable<int> idsEnSesion)
    {
        int maxId = 0;

        if (manager != null && manager.contenidos != null)
        {
            Curso c = manager.contenidos.Find(x => x.curso == curso);
            if (c != null && c.temas != null)
            {
                Tema t = c.temas.Find(x => x.nombre == tema);
                if (t != null && t.ejercicios != null)
                {
                    foreach (Ejercicio ej in t.ejercicios)
                    {
                        if (ej != null && ej.id > maxId)
                        {
                            maxId = ej.id;
                        }
                    }
                }
            }
        }

        if (idsEnSesion != null)
        {
            foreach (int id in idsEnSesion)
            {
                if (id > maxId)
                {
                    maxId = id;
                }
            }
        }

        return maxId + 1;
    }

    public static ComparacionDuplicado DetectarDuplicado(
        ContenidoManager manager,
        string curso,
        string tema,
        Ejercicio nuevo,
        List<Ejercicio> ejerciciosSesionPrevios)
    {
        if (nuevo == null || string.IsNullOrWhiteSpace(nuevo.enunciado))
        {
            return new ComparacionDuplicado { esDuplicado = false };
        }

        string normNuevo = NormalizarEnunciado(nuevo.enunciado);

        // 1. Buscar en contenidos guardados
        if (manager != null && manager.contenidos != null)
        {
            Curso c = manager.contenidos.Find(x => x.curso == curso);
            if (c != null && c.temas != null)
            {
                Tema t = c.temas.Find(x => x.nombre == tema);
                if (t != null && t.ejercicios != null)
                {
                    foreach (Ejercicio ej in t.ejercicios)
                    {
                        if (ej == null) continue;
                        if (NormalizarEnunciado(ej.enunciado) == normNuevo)
                        {
                            return ConstruirComparacion(curso, tema, ej, nuevo);
                        }
                    }
                }
            }
        }

        // 2. Buscar en ejercicios previos de la misma sesión
        if (ejerciciosSesionPrevios != null)
        {
            foreach (Ejercicio prev in ejerciciosSesionPrevios)
            {
                if (prev == null) continue;
                if (NormalizarEnunciado(prev.enunciado) == normNuevo)
                {
                    return ConstruirComparacion(curso, tema, prev, nuevo);
                }
            }
        }

        return new ComparacionDuplicado { esDuplicado = false };
    }

    private static ComparacionDuplicado ConstruirComparacion(string curso, string tema, Ejercicio existente, Ejercicio nuevo)
    {
        ComparacionDuplicado comp = new ComparacionDuplicado();
        comp.esDuplicado = true;
        comp.curso = curso;
        comp.tema = tema;
        comp.existente = existente;
        comp.nuevo = nuevo;
        comp.coincidenciaExacta = (existente.enunciado == nuevo.enunciado);

        StringBuilder sb = new StringBuilder();

        if (comp.coincidenciaExacta)
        {
            sb.AppendLine("• Enunciado: Coincidencia textual exacta.");
        }
        else
        {
            sb.AppendLine("• Enunciado: Coincide tras normalizar mayúsculas/minúsculas y espacios.");
        }

        // Comparar elementos
        string elExist = existente.elementos != null ? string.Join(", ", existente.elementos) : "ninguno";
        string elNuevo = nuevo.elementos != null ? string.Join(", ", nuevo.elementos) : "ninguno";
        if (elExist == elNuevo)
        {
            sb.AppendLine($"• Elementos: Idénticos [{elNuevo}].");
        }
        else
        {
            sb.AppendLine($"• Elementos diferentes: Guardado=[{elExist}] vs Nuevo=[{elNuevo}].");
        }

        // Comparar respuestas
        string respExist = existente.respuestaCorrecta != null ? string.Join(", ", existente.respuestaCorrecta) : "ninguna";
        string respNuevo = nuevo.respuestaCorrecta != null ? string.Join(", ", nuevo.respuestaCorrecta) : "ninguna";
        if (respExist == respNuevo)
        {
            sb.AppendLine($"• Respuestas: Idénticas [{respNuevo}].");
        }
        else
        {
            sb.AppendLine($"• Respuestas diferentes: Guardado=[{respExist}] vs Nuevo=[{respNuevo}].");
        }

        // Comparar dificultad
        if (existente.dificultad == nuevo.dificultad)
        {
            sb.AppendLine($"• Dificultad: Idéntica (Nivel {nuevo.dificultad}).");
        }
        else
        {
            sb.AppendLine($"• Dificultad diferente: Guardado=Nivel {existente.dificultad} vs Nuevo=Nivel {nuevo.dificultad}.");
        }

        // Comparar intentos y tiempo
        if (existente.maxIntentos != nuevo.maxIntentos || existente.tiempoMaximo != nuevo.tiempoMaximo)
        {
            sb.AppendLine($"• Parámetros: Guardado=({existente.maxIntentos} intentos, {existente.tiempoMaximo}s) vs Nuevo=({nuevo.maxIntentos} intentos, {nuevo.tiempoMaximo}s).");
        }

        comp.detalleDiferencias = sb.ToString();
        return comp;
    }
}
