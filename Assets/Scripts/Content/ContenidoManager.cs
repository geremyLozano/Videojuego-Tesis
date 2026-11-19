using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ContenidoManager : MonoBehaviour
{
    public Curso contenidoActual;
    public List<Curso> contenidos = new List<Curso>();
    private string rutaGuardados;

    public void CargarGuardados()
    {
        rutaGuardados = Path.Combine(
            Application.persistentDataPath,
            "contenidos_guardados.json"
        );

        if (File.Exists(rutaGuardados))
        {
            string jsonGuardado = File.ReadAllText(rutaGuardados);

            ContenidosGuardados datosGuardados =
                JsonUtility.FromJson<ContenidosGuardados>(jsonGuardado);

            if (datosGuardados != null && datosGuardados.contenidos != null)
            {
                contenidos = datosGuardados.contenidos;
            }
            else if (contenidos == null)
            {
                contenidos = new List<Curso>();
            }

            Debug.Log("Contenidos cargados desde el archivo guardado.");
        }
        else
        {
            if (contenidos == null)
            {
                contenidos = new List<Curso>();
            }
            Debug.Log("No se encontró el archivo de contenidos guardados.");
        }
    }

    void Start()
    {
        CargarGuardados();
    }

    public void CargarJSONDesdeTexto(string json)
    {
        if (contenidos == null || (contenidos.Count == 0 && File.Exists(Path.Combine(Application.persistentDataPath, "contenidos_guardados.json"))))
        {
            CargarGuardados();
        }

        ContenidoJSON contenidoImportado =
            JsonUtility.FromJson<ContenidoJSON>(json);

        if (contenidoImportado == null)
        {
            Debug.LogError("Error: el contenido importado es nulo.");
            return;
        }

        if (contenidoImportado.ejercicios == null)
        {
            contenidoImportado.ejercicios = new List<Ejercicio>();
        }

        // AQUÍ VA TODO EL RESTO
        // de tu CargarJSON actual
        Debug.Log("Curso importado: " + contenidoImportado.curso);
        Debug.Log("Tema importado: " + contenidoImportado.tema);

        // Buscar si el curso ya existe
        Curso cursoExistente = contenidos.Find(
            c => c.curso == contenidoImportado.curso
        );

        // Si el curso no existe
        if (cursoExistente == null)
        {
            Curso nuevoCurso = new Curso();

            nuevoCurso.curso = contenidoImportado.curso;
            nuevoCurso.temas = new List<Tema>();

            Tema nuevoTema = new Tema();

            nuevoTema.nombre = contenidoImportado.tema;
            nuevoTema.ejercicios = contenidoImportado.ejercicios;

            nuevoCurso.temas.Add(nuevoTema);

            contenidos.Add(nuevoCurso);

            contenidoActual = nuevoCurso;

            GuardarContenido();

            Debug.Log("Nuevo curso agregado.");
            Debug.Log("Nuevo tema agregado: " + nuevoTema.nombre);
        }
        else
        {
            contenidoActual = cursoExistente;

            // Buscar si el tema ya existe
            Tema temaExistente = cursoExistente.temas.Find(
                t => t.nombre == contenidoImportado.tema
            );

            // Si el tema no existe
            if (temaExistente == null)
            {
                Tema nuevoTema = new Tema();

                nuevoTema.nombre = contenidoImportado.tema;
                nuevoTema.ejercicios = contenidoImportado.ejercicios;

                cursoExistente.temas.Add(nuevoTema);

                GuardarContenido();

                Debug.Log("Nuevo tema agregado: " + nuevoTema.nombre);
            }
            else
            {
                // El tema ya existe, buscamos ejercicios nuevos
                int nuevos = 0;

                foreach (Ejercicio ejercicioNuevo in contenidoImportado.ejercicios)
                {
                    bool existe = temaExistente.ejercicios.Exists(
                        e => e.id == ejercicioNuevo.id
                    );

                    if (!existe)
                    {
                        temaExistente.ejercicios.Add(ejercicioNuevo);
                        nuevos++;
                    }
                }

                if (nuevos > 0)
                {
                    GuardarContenido();

                    Debug.Log(
                        "Se agregaron " + nuevos +
                        " ejercicios nuevos al tema " +
                        temaExistente.nombre
                    );
                }
                else
                {
                    Debug.Log("No se encontraron ejercicios nuevos.");
                }
            }
        }

        // DEBUG
        Tema temaActual = contenidoActual.temas.Find(
            t => t.nombre == contenidoImportado.tema
        );
        GuardarContenido();
    }

    public void CargarJSON(string ruta)
    {
        // Leer archivo JSON
        string json = File.ReadAllText(ruta);
        CargarJSONDesdeTexto(json);

    }

    public Ejercicio ObtenerEjercicio(string tema, int id)
    {
        if (contenidoActual == null)
            return null;

        Tema temaBuscado = contenidoActual.temas.Find(
            t => t.nombre == tema
        );

        if (temaBuscado == null)
            return null;

        foreach (Ejercicio ejercicio in temaBuscado.ejercicios)
        {
            if (ejercicio.id == id)
            {
                return ejercicio;
            }
        }

        return null;
    }

    public Ejercicio ObtenerEjercicioAleatorio(string curso, string tema, int dificultad)
    {
        List<Ejercicio> disponibles = new List<Ejercicio>();

        Curso cursoBuscado = contenidos.Find(
            c => c.curso == curso
        );

        if (cursoBuscado == null)
            return null;

        Tema temaBuscado = cursoBuscado.temas.Find(
            t => t.nombre == tema
        );

        if (temaBuscado == null)
            return null;

        foreach (Ejercicio ejercicio in temaBuscado.ejercicios)
        {
            if (ejercicio.dificultad != dificultad)
                continue;

            // Verificar si ya existe en la lista actual
            bool repetido = false;

            if (DatosPartida.ejercicios != null &&
                DatosPartida.temas != null)
            {
                for (int i = 0; i < DatosPartida.ejercicios.Count; i++)
                {
                    if (DatosPartida.ejercicios[i].id == ejercicio.id &&
                        DatosPartida.temas[i].nombre == tema)
                    {
                        repetido = true;
                        break;
                    }
                }
            }

            if (!repetido)
            {
                disponibles.Add(ejercicio);
            }
        }

        if (disponibles.Count == 0)
            return null;

        int indice = Random.Range(0, disponibles.Count);

        return disponibles[indice];
    }

    public void GuardarContenido()
    {
        ContenidosGuardados datos = new ContenidosGuardados();

        datos.contenidos = contenidos;

        string json = JsonUtility.ToJson(datos, true);

        rutaGuardados = Path.Combine(
            Application.persistentDataPath,
            "contenidos_guardados.json"
        );

        File.WriteAllText(rutaGuardados, json);

        //Debug.Log("Contenido guardado en: " + rutaGuardados);
    }

    public void CrearListaInicial(
    string curso,
    List<string> seleccionados,
    int cantidadEjercicios,
    int dificultad)
    {
        DatosPartida.ejercicios = new List<Ejercicio>();
        DatosPartida.temas = new List<Tema>();

        int cantidadTemas = seleccionados.Count;

        if (cantidadTemas == 0)
        {
            Debug.LogWarning("No se seleccionaron temas.");
            return;
        }

        int ejerciciosPorTema = cantidadEjercicios / cantidadTemas;
        int sobrantes = cantidadEjercicios % cantidadTemas;

        for (int i = 0; i < cantidadTemas; i++)
        {
            string tema = seleccionados[i];

            int cantidadTema = ejerciciosPorTema;

            if (i < sobrantes)
            {
                cantidadTema++;
            }

            for (int j = 0; j < cantidadTema; j++)
            {
                Ejercicio ejercicio =
                    ObtenerEjercicioAleatorio(curso, tema, dificultad);

                if (ejercicio != null)
                {
                    DatosPartida.ejercicios.Add(ejercicio);
                    DatosPartida.temas.Add(new Tema { nombre = tema });
                }
            }
        }

        MezclarLista(
            DatosPartida.ejercicios,
            DatosPartida.temas
        );

        DatosPartida.retroEjercicio = new List<RetroEjercicio>();

        for (int i = 0; i < cantidadEjercicios; i++)
        {
            DatosPartida.retroEjercicio.Add(new RetroEjercicio());
        }
    }

    private void MezclarLista(List<Ejercicio> ejercicios, List<Tema> temas)
    {
        for (int i = ejercicios.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);

            Ejercicio ejercicioTemporal = ejercicios[i];
            ejercicios[i] = ejercicios[j];
            ejercicios[j] = ejercicioTemporal;

            Tema temaTemporal = temas[i];
            temas[i] = temas[j];
            temas[j] = temaTemporal;
        }
    }
}
