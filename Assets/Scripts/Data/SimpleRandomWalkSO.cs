using UnityEngine;

[CreateAssetMenu(fileName = "SimpleRandomWalkParameters_", menuName = "PCG/SimpleRandomWalkData")] // Permite crear un menú en Unity para crear instancias de este ScriptableObject.

public class SimpleRandomWalkSO : ScriptableObject // Permite crear un objeto de datos que se puede usar en Unity para almacenar información sobre un paseo aleatorio simple.
{
    public int iterations = 10;
    public int walkLength = 10;
    public bool startRandomlyEachIteration = true;
}
