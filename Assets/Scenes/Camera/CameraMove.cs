using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour
{
    public float velocidad = 10f;
    public void MoverCamara(Vector3 desplazamiento)
    {
        Vector3 destino = transform.position + desplazamiento;

        StartCoroutine(Mover(destino));
    }

    private IEnumerator Mover(Vector3 destino)
    {
        while (Vector3.Distance(transform.position, destino) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destino,
                velocidad * Time.deltaTime
            );

            yield return null;
        }

        transform.position = destino;
    }
}
