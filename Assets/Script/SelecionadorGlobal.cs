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
            RaycastHit2D hit = Physics2D.Raycast(posicaoMouse, Vector2.zero);

            if (hit.collider != null)
            {
                Personagem personagem = hit.collider.GetComponent<Personagem>();

                if (personagem != null)
                {
                    personagem.Selecionar();
                }
                else
                {
                    Personagem.DesselecionarAtual();
                }
            }
            else
            {
                Personagem.DesselecionarAtual();
            }
        }
    }
}