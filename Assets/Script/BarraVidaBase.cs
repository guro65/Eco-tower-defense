using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BarraVidaBase : MonoBehaviour
{
    private Caminho caminho;
    private Image barraVida;
    private TextMeshProUGUI textoVida;

    [SerializeField] private Color corVerde = Color.green;
    [SerializeField] private Color corLaranja = new Color(1f, 0.65f, 0f);
    [SerializeField] private Color corVermelho = Color.red;

    private void Start()
    {
        caminho = FindObjectOfType<Caminho>();
        barraVida = GetComponent<Image>();

        // Procura o texto em qualquer lugar da cena
        textoVida = FindObjectOfType<TextMeshProUGUI>();

        // Se não encontrar, tenta procurar como filho
        if (textoVida == null && transform.parent != null)
        {
            textoVida = transform.parent.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (barraVida == null)
        {
            Debug.LogError("BarraVidaBase precisa estar em um Image!");
        }

        if (caminho == null)
        {
            Debug.LogError("Caminho não encontrado na cena!");
        }
    }

    private void Update()
    {
        if (caminho != null)
        {
            AtualizarBarraVida();
        }
    }

    private void AtualizarBarraVida()
    {
        int vidaAtual = caminho.GetVidaBase();
        int vidaMaxima = caminho.GetVidaMaximaBase();

        barraVida.fillAmount = (float)vidaAtual / vidaMaxima;

        if (vidaAtual >= vidaMaxima * 0.75f)
        {
            barraVida.color = corVerde;
        }
        else if (vidaAtual >= vidaMaxima * 0.25f)
        {
            barraVida.color = corLaranja;
        }
        else
        {
            barraVida.color = corVermelho;
        }

        if (textoVida != null)
        {
            textoVida.text = vidaAtual + " / " + vidaMaxima;
        }
    }
}