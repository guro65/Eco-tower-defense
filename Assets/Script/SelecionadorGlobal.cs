using UnityEngine;
using UnityEngine.InputSystem;

public class SelecionadorGlobal : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 posicaoMouse = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            // Raycast que retorna TODOS os colliders no ponto
            RaycastHit2D[] hits = Physics2D.RaycastAll(posicaoMouse, Vector2.zero);

            Personagem personagemEncontrado = null;

            // Procura por BoxCollider2D com tag "Personagem"
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider is BoxCollider2D)
                {
                    // Verifica se tem a tag "Personagem"
                    if (hit.collider.gameObject.CompareTag("Personagem"))
                    {
                        Personagem personagem = hit.collider.GetComponent<Personagem>();

                        if (personagem != null)
                        {
                            personagemEncontrado = personagem;
                            break; // Para no primeiro encontrado
                        }
                    }
                }
            }

            if (personagemEncontrado != null)
            {
                personagemEncontrado.Selecionar();
            }
            else
            {
                Personagem.DesselecionarAtual();
            }
        }
    }
}