using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Personagem : MonoBehaviour
{
    [Header("Informações")]
    [SerializeField] private string nomePersonagem;

    [Header("Status")]
    [SerializeField] private float dano = 10f;
    [SerializeField] private float tempoEntreAtaques = 1f;
    [SerializeField] private float alcance = 5f;

    [Header("Configuração")]
    [SerializeField] private string tagRange = "RangeVisual";

    private SpriteRenderer rangeSprite;
    private float tempoProximoAtaque;
    private float ultimoAlcance;

    private static Personagem personagemSelecionado;

    private void Awake()
    {
        EncontrarRangeDoFilho();

        if (rangeSprite != null)
        {
            rangeSprite.gameObject.SetActive(false);
            AtualizarRangeVisual();
            ultimoAlcance = alcance;
        }
    }

    private void Update()
    {
        tempoProximoAtaque -= Time.deltaTime;

        if (alcance != ultimoAlcance)
        {
            AtualizarRangeVisual();
            ultimoAlcance = alcance;
        }
    }

    public void Selecionar()
    {
        if (personagemSelecionado != null && personagemSelecionado != this)
        {
            personagemSelecionado.Desselecionar();
        }

        personagemSelecionado = this;

        if (rangeSprite != null)
            rangeSprite.gameObject.SetActive(true);
    }

    public void Desselecionar()
    {
        if (rangeSprite != null)
            rangeSprite.gameObject.SetActive(false);

        if (personagemSelecionado == this)
            personagemSelecionado = null;
    }

    public static void DesselecionarAtual()
    {
        if (personagemSelecionado != null)
            personagemSelecionado.Desselecionar();
    }

    private void EncontrarRangeDoFilho()
    {
        foreach (Transform filho in transform)
        {
            if (filho.CompareTag(tagRange))
            {
                rangeSprite = filho.GetComponent<SpriteRenderer>();
                break;
            }
        }
    }

    private void AtualizarRangeVisual()
    {
        if (rangeSprite == null || rangeSprite.sprite == null)
            return;

        float tamanhoOriginal = rangeSprite.sprite.bounds.size.x;
        float diametro = alcance * 2f;
        float escalaFinal = diametro / tamanhoOriginal;

        rangeSprite.transform.localScale = new Vector3(escalaFinal, escalaFinal, 1f);
    }

    public bool PodeAtacar()
    {
        return tempoProximoAtaque <= 0f;
    }

    public void Atacar()
    {
        if (PodeAtacar())
        {
            Debug.Log(nomePersonagem + " causou " + dano + " de dano.");
            tempoProximoAtaque = tempoEntreAtaques;
        }
    }

    public void DefinirAlcance(float novoAlcance)
    {
        alcance = novoAlcance;
        AtualizarRangeVisual();
        ultimoAlcance = alcance;
    }

    public string GetNome() => nomePersonagem;
    public float GetDano() => dano;
    public float GetTempoEntreAtaques() => tempoEntreAtaques;
    public float GetAlcance() => alcance;
}