using System;

[Serializable]
public class Ejercicio
{
    public int id;
    public int dificultad;
    public string enunciado;
    public int[] elementos;
    public float[] respuestaCorrecta;
    public int maxIntentos;
    public int tiempoMaximo;
}