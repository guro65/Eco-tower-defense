using UnityEngine;

public class Caminho : MonoBehaviour
{
    private Transform[] pontos;

    public Transform[] Pontos => pontos;

    private void Awake()
    {
        pontos = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            pontos[i] = transform.GetChild(i);
        }
    }
}