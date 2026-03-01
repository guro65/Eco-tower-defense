using System.Collections.Generic;
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
    private CircleCollider2D colliderRange;
    private float ultimoAlcance;

    private static Personagem personagemSelecionado;

    // Controle de inimigos e cooldown individual
    private Dictionary<Inimigo, float> inimigosCooldown = new Dictionary<Inimigo, float>();

    private void Awake()
    {
        // 🔹 Pega o collider da range (não o BoxCollider2D do personagem)
        colliderRange = GetComponent<CircleCollider2D>();
        if (colliderRange == null)
        {
            colliderRange = gameObject.AddComponent<CircleCollider2D>();
        }
        colliderRange.isTrigger = true;
        colliderRange.radius = alcance;

        EncontrarRangeDoFilho();

        // Range invisível por padrão, collider ativo
        if (rangeSprite != null)
        {
            rangeSprite.enabled = false;
            AtualizarRangeVisual();
        }

        ultimoAlcance = alcance;
    }

    private void Update()
    {
        AtualizarStatusDinamicos();
        AtualizarCooldowns();
        TentarAtacarTodos();
    }

    // ================= ATAQUE =================

    private void TentarAtacarTodos()
    {
        List<Inimigo> inimigosParaAtacar = new List<Inimigo>(inimigosCooldown.Keys);

        foreach (Inimigo inimigo in inimigosParaAtacar)
        {
            if (inimigo == null)
            {
                inimigosCooldown.Remove(inimigo);
                continue;
            }

            if (PodeAtacar(inimigo))
            {
                Atacar(inimigo);
                inimigosCooldown[inimigo] = tempoEntreAtaques; // reset cooldown
            }
        }
    }

    private void Atacar(Inimigo inimigo)
    {
        if (inimigo == null)
            return;

        inimigo.ReceberDano(dano);
        Debug.Log(nomePersonagem + " causou " + dano + " de dano em " + inimigo.name);
    }

    private bool PodeAtacar(Inimigo inimigo)
    {
        if (!inimigosCooldown.ContainsKey(inimigo))
        {
            inimigosCooldown[inimigo] = 0f;
            return true;
        }

        return inimigosCooldown[inimigo] <= 0f;
    }

    private void AtualizarCooldowns()
    {
        List<Inimigo> keys = new List<Inimigo>(inimigosCooldown.Keys);
        foreach (Inimigo inimigo in keys)
        {
            inimigosCooldown[inimigo] -= Time.deltaTime;
            if (inimigo == null)
                inimigosCooldown.Remove(inimigo);
        }
    }

    // ================= DETECÇÃO =================

    private void OnTriggerEnter2D(Collider2D other)
    {
        Inimigo inimigo = other.GetComponent<Inimigo>();
        if (inimigo != null && !inimigosCooldown.ContainsKey(inimigo))
        {
            inimigosCooldown.Add(inimigo, 0f); // pronto para atacar
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Inimigo inimigo = other.GetComponent<Inimigo>();
        if (inimigo != null && inimigosCooldown.ContainsKey(inimigo))
        {
            inimigosCooldown.Remove(inimigo);
        }
    }

    // ================= SELEÇÃO =================

    public void Selecionar()
    {
        if (personagemSelecionado != null && personagemSelecionado != this)
        {
            personagemSelecionado.Desselecionar();
        }

        personagemSelecionado = this;

        if (rangeSprite != null)
        {
            rangeSprite.enabled = true; // aparece para o jogador
            AtualizarRangeVisual();    // garante que a escala esteja correta
        }
    }

    public void Desselecionar()
    {
        if (rangeSprite != null)
            rangeSprite.enabled = false;

        if (personagemSelecionado == this)
            personagemSelecionado = null;
    }

    public static void DesselecionarAtual()
    {
        if (personagemSelecionado != null)
            personagemSelecionado.Desselecionar();
    }

    // ================= RANGE VISUAL =================

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

    // ================= DETECÇÃO DINÂMICA DE INIMIGOS =================

    private void DetectarInimigosDentroDoRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, alcance);
        foreach (Collider2D hit in hits)
        {
            Inimigo inimigo = hit.GetComponent<Inimigo>();
            if (inimigo != null && !inimigosCooldown.ContainsKey(inimigo))
            {
                inimigosCooldown.Add(inimigo, 0f);
            }
        }
    }

    // ================= ATUALIZAÇÃO DE STATUS =================

    private void AtualizarStatusDinamicos()
    {
        // Atualiza alcance caso tenha mudado
        if (alcance != ultimoAlcance)
        {
            // Atualiza collider
            if (colliderRange != null)
                colliderRange.radius = alcance;

            // Atualiza sprite
            AtualizarRangeVisual();

            // Detecta inimigos já dentro da nova range
            DetectarInimigosDentroDoRange();

            ultimoAlcance = alcance;
        }
    }

    // ================= GETTERS/SETTERS =================

    public void DefinirAlcance(float novoAlcance)
    {
        alcance = novoAlcance;
        AtualizarStatusDinamicos();
    }

    public void DefinirDano(float novoDano)
    {
        dano = novoDano;
    }

    public void DefinirTempoEntreAtaques(float novoTempo)
    {
        tempoEntreAtaques = novoTempo;
    }

    public string GetNome() => nomePersonagem;
    public float GetDano() => dano;
    public float GetTempoEntreAtaques() => tempoEntreAtaques;
    public float GetAlcance() => alcance;
}