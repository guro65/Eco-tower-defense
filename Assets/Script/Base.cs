using UnityEngine;

public class Base : MonoBehaviour
{
    private Caminho caminho;
    private BoxCollider2D colliderBase;

    private void Start()
    {
        caminho = FindObjectOfType<Caminho>();
        colliderBase = GetComponent<BoxCollider2D>();

        if (colliderBase == null)
        {
            Debug.LogError("Base precisa de um BoxCollider2D com isTrigger ativado!");
        }

        if (caminho == null)
        {
            Debug.LogError("Caminho não encontrado na cena!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Verifica se o objeto que entrou tem a tag "Inimigo"
        if (collision.CompareTag("Inimigo"))
        {
            Inimigo inimigo = collision.GetComponent<Inimigo>();

            if (inimigo != null)
            {
                // Registra o inimigo como derrotado no sistema de ondas
                if (SistemaDeOndas.instancia != null)
                {
                    SistemaDeOndas.instancia.RegistrarInimigoDerrotado();
                }

                // A base perde vida
                if (caminho != null)
                {
                    caminho.PerdidaVidaBase(1);
                }

                Debug.Log("Inimigo chegou na base e foi deletado!");
            }

            // Destroi o inimigo
            Destroy(collision.gameObject);
        }
    }
}