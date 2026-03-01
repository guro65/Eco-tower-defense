using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Personagem : MonoBehaviour
{
    // ================= ENUM DE TIPO DE RANGE =================
    public enum TipoRange
    {
        FullAOE,    // Toda a área - todos tomam dano
        Cone,       // Cone de ataque
        BolaAOE,    // Bolinha que persegue inimigos
        Unico       // Só ataca o mais próximo
    }

    [Header("Informações")]
    [SerializeField] private string nomePersonagem;

    [Header("Status")]
    [SerializeField] private float dano = 10f;
    [SerializeField] private float tempoEntreAtaques = 1f;
    [SerializeField] private float alcance = 5f;

    [Header("Tipo de Range")]
    [SerializeField] private TipoRange tipoRange = TipoRange.FullAOE;

    [Header("Configuração Cone")]
    [SerializeField] private float angulooCone = 90f; // Em graus (0-360)

    [Header("Configuração Bola AOE")]
    [SerializeField] private float raioBolaAOE = 2f; // Raio da bola menor
    [SerializeField] private float velocidadeBola = 5f; // Velocidade de perseguição

    [Header("Configuração")]
    [SerializeField] private string tagRange = "RangeVisual";

    private SpriteRenderer rangeSprite;
    private CircleCollider2D colliderRange;
    private float ultimoAlcance;

    private static Personagem personagemSelecionado;

    // Controle de inimigos e cooldown individual
    private Dictionary<Inimigo, float> inimigosCooldown = new Dictionary<Inimigo, float>();

    // Para tipo Unico
    private Inimigo alvoAtual = null;

    // Para tipo BolaAOE
    private List<BolaAOE> bolasAtivas = new List<BolaAOE>();

    private void Awake()
    {
        colliderRange = GetComponent<CircleCollider2D>();
        if (colliderRange == null)
        {
            colliderRange = gameObject.AddComponent<CircleCollider2D>();
        }
        colliderRange.isTrigger = true;
        colliderRange.radius = alcance;

        EncontrarRangeDoFilho();

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
        AtualizarBolasAOE(); // Para tipo BolaAOE
    }

    // ================= ATAQUE =================

    private void TentarAtacarTodos()
    {
        switch (tipoRange)
        {
            case TipoRange.FullAOE:
                AtacarFullAOE();
                break;
            case TipoRange.Cone:
                AtacarCone();
                break;
            case TipoRange.BolaAOE:
                AtacarBolaAOE();
                break;
            case TipoRange.Unico:
                AtacarUnico();
                break;
        }
    }

    // ========== FULL AOE ==========
    private void AtacarFullAOE()
    {
        List<Inimigo> inimigosParaAtacar = new List<Inimigo>(inimigosCooldown.Keys);

        foreach (Inimigo inimigo in inimigosParaAtacar)
        {
            if (inimigo == null)
            {
                inimigosCooldown.Remove(inimigo);
                continue;
            }

            // Verifica se está dentro do alcance (FullAOE ataca todos dentro da range)
            if (EstaNoAlcance(inimigo) && PodeAtacar(inimigo))
            {
                Atacar(inimigo);
                inimigosCooldown[inimigo] = tempoEntreAtaques;
            }
        }
    }

    // ========== CONE ==========
    private void AtacarCone()
    {
        List<Inimigo> inimigosParaAtacar = new List<Inimigo>(inimigosCooldown.Keys);

        foreach (Inimigo inimigo in inimigosParaAtacar)
        {
            if (inimigo == null)
            {
                inimigosCooldown.Remove(inimigo);
                continue;
            }

            // Verifica se está dentro do cone E no alcance
            if (EstaNoAlcance(inimigo) && EstaNoConeDirecao(inimigo) && PodeAtacar(inimigo))
            {
                Atacar(inimigo);
                inimigosCooldown[inimigo] = tempoEntreAtaques;
            }
        }
    }

    // ========== BOLA AOE ==========
    private void AtacarBolaAOE()
    {
        List<Inimigo> inimigosParaAtacar = new List<Inimigo>(inimigosCooldown.Keys);

        foreach (Inimigo inimigo in inimigosParaAtacar)
        {
            if (inimigo == null)
            {
                inimigosCooldown.Remove(inimigo);
                continue;
            }

            // Cria uma bola que persegue o inimigo (a bola faz a verificação de dano)
            if (EstaNoAlcance(inimigo) && PodeAtacar(inimigo))
            {
                CriarBolaAOE(inimigo);
                inimigosCooldown[inimigo] = tempoEntreAtaques;
            }
        }
    }

    private void CriarBolaAOE(Inimigo alvo)
    {
        BolaAOE bola = new BolaAOE(transform.position, alvo, raioBolaAOE, velocidadeBola, alcance, dano, this);
        bolasAtivas.Add(bola);
    }

    private void AtualizarBolasAOE()
    {
        for (int i = bolasAtivas.Count - 1; i >= 0; i--)
        {
            BolaAOE bola = bolasAtivas[i];
            bola.Atualizar(Time.deltaTime);

            // Se a bola terminou sua vida útil ou o alvo morreu
            if (bola.DeveSerRemovida())
            {
                bolasAtivas.RemoveAt(i);
            }
        }
    }

    // ========== ATAQUE ÚNICO ==========
    private void AtacarUnico()
    {
        // Se o alvo atual é nulo ou morreu, busca o mais próximo
        if (alvoAtual == null || !inimigosCooldown.ContainsKey(alvoAtual))
        {
            EncontrarAlvoMaisProximo();
        }

        // Verifica se ainda está no alcance
        if (alvoAtual != null && EstaNoAlcance(alvoAtual) && inimigosCooldown.ContainsKey(alvoAtual) && PodeAtacar(alvoAtual))
        {
            Atacar(alvoAtual);
            inimigosCooldown[alvoAtual] = tempoEntreAtaques;
        }
    }

    private void EncontrarAlvoMaisProximo()
    {
        alvoAtual = null;
        float menorDist = float.MaxValue;

        foreach (Inimigo inimigo in inimigosCooldown.Keys)
        {
            if (inimigo == null) continue;

            float dist = Vector3.Distance(transform.position, inimigo.transform.position);
            if (dist < menorDist && EstaNoAlcance(inimigo))
            {
                menorDist = dist;
                alvoAtual = inimigo;
            }
        }
    }

    // ================= VERIFICAÇÕES DE ÁREA =================

    /// <summary>
    /// Verifica se o inimigo está dentro do alcance máximo
    /// </summary>
    private bool EstaNoAlcance(Inimigo inimigo)
    {
        float distancia = Vector3.Distance(transform.position, inimigo.transform.position);
        return distancia <= alcance;
    }

    /// <summary>
    /// Verifica se o inimigo está dentro do cone de ataque
    /// </summary>
    private bool EstaNoConeDirecao(Inimigo inimigo)
    {
        Vector2 direcaoParaInimigo = (inimigo.transform.position - transform.position).normalized;

        // Encontra o ângulo em relação ao inimigo mais próximo
        Inimigo inimigoMaisProximo = EncontrarInimigoMaisProximo();
        if (inimigoMaisProximo == null)
        {
            // Se não tem inimigo, usa a direção padrão (direita)
            direcaoParaInimigo = transform.right;
        }
        else
        {
            // Usa a direção do inimigo mais próximo
            direcaoParaInimigo = (inimigoMaisProximo.transform.position - transform.position).normalized;
        }

        Vector2 direcaoVerificacao = (inimigo.transform.position - transform.position).normalized;
        float angulo = Vector2.Angle(direcaoParaInimigo, direcaoVerificacao);

        return angulo <= angulooCone / 2f;
    }

    /// <summary>
    /// Encontra o inimigo mais próximo no alcance
    /// </summary>
    private Inimigo EncontrarInimigoMaisProximo()
    {
        Inimigo inimigoMaisProximo = null;
        float menorDist = float.MaxValue;

        foreach (Inimigo inimigo in inimigosCooldown.Keys)
        {
            if (inimigo == null) continue;

            float dist = Vector3.Distance(transform.position, inimigo.transform.position);
            if (dist < menorDist)
            {
                menorDist = dist;
                inimigoMaisProximo = inimigo;
            }
        }

        return inimigoMaisProximo;
    }

    // ========== UTILITÁRIOS ==========

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
            inimigosCooldown.Add(inimigo, 0f);
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
            rangeSprite.enabled = true;
            AtualizarRangeVisual();
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
        if (alcance != ultimoAlcance)
        {
            if (colliderRange != null)
                colliderRange.radius = alcance;

            AtualizarRangeVisual();
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

    public void DefinirTipoRange(TipoRange tipo)
    {
        tipoRange = tipo;
    }

    public void DefinirAnguloCone(float angulo)
    {
        angulooCone = Mathf.Clamp(angulo, 0f, 360f);
    }

    public void DefinirRaioBolaAOE(float raio)
    {
        raioBolaAOE = Mathf.Max(0.1f, raio);
    }

    public void DefinirVelocidadeBola(float velocidade)
    {
        velocidadeBola = Mathf.Max(0.1f, velocidade);
    }

    public string GetNome() => nomePersonagem;
    public float GetDano() => dano;
    public float GetTempoEntreAtaques() => tempoEntreAtaques;
    public float GetAlcance() => alcance;
    public TipoRange GetTipoRange() => tipoRange;
    public float GetAnguloCone() => angulooCone;
    public float GetRaioBolaAOE() => raioBolaAOE;
}