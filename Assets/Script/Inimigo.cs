using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Inimigo : MonoBehaviour
{
    [Header("Informações")]
    public string nomeInimigo;

    public enum TipoInimigo
    {
        Normal,
        Acelerado,
        Regenerador,
        Resistente,
        Blindado
    }

    [Header("Tipos Ativos")]
    public List<TipoInimigo> tipos = new List<TipoInimigo>();

    [Header("Vida")]
    public float vidaInicial = 100f;
    public float vidaAtual = 100f;

    [Header("Movimento")]
    public float velocidadeBase = 3f;

    [Header("Acelerado")]
    public float multiplicadorVelocidade = 2f;

    [Header("Regenerador")]
    public float regeneracaoPorSegundo = 5f;

    [Header("Resistente")]
    public float resistenciaFixa = 30f;

    [Header("Blindado")]
    public int quantidadeEscudos = 3;
    private int escudosAtuais;

    [Header("Aparência")]
    [SerializeField] private List<Sprite> spritesPossiveis = new List<Sprite>();
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Caminho")]
    public Caminho caminho;
    private int indiceAtual = 0;
    private float velocidadeFinal;

    [Header("UI")]
    [SerializeField] private Image fillVida;
    [SerializeField] private Image fillEscudo;
    [SerializeField] private GameObject barraEscudo;
    [SerializeField] private TextMeshProUGUI textoVida;
    [SerializeField] private TextMeshProUGUI textoEscudo;
    [SerializeField] private Transform canvasUI;
    private Camera cam;

    private bool estaVivo = true;

    private void Awake()
    {
        if (tipos.Count == 0)
            tipos.Add(TipoInimigo.Normal);

        gameObject.tag = "Inimigo";

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        cam = Camera.main;

        escudosAtuais = quantidadeEscudos;

        velocidadeFinal = velocidadeBase;
        if (tipos.Contains(TipoInimigo.Acelerado))
            velocidadeFinal *= multiplicadorVelocidade;

        barraEscudo.SetActive(tipos.Contains(TipoInimigo.Blindado));

        vidaAtual = vidaInicial;
        AtualizarUI();

        if (caminho != null && caminho.Pontos.Length > 0)
            transform.position = caminho.Pontos[0].position;
    }

    private void Update()
    {
        if (estaVivo)
        {
            Mover();
            AplicarRegeneracao();
            RotacionarUI();
        }
    }

    // ================= CONFIGURAÇÃO ALEATÓRIA (CHAMADA AO SPAWNAR) =================

    public void ConfigurarAparenciaEVida()
    {
        // Sorteia sprite aleatório
        if (spritesPossiveis != null && spritesPossiveis.Count > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = spritesPossiveis[Random.Range(0, spritesPossiveis.Count)];
        }

        // Aplica multiplicador de vida baseado na onda
        vidaInicial = vidaInicial * SistemaDeOndas.multiplicadorVidaInimigo;
        vidaAtual = vidaInicial;

        AtualizarUI();
    }

    // ================= VIDA =================

    public void ReceberDano(float dano)
    {
        if (vidaAtual <= 0 || !estaVivo)
            return;

        // 🛡️ Escudo
        if (tipos.Contains(TipoInimigo.Blindado) && escudosAtuais > 0)
        {
            escudosAtuais--;
            AtualizarUI();
            return;
        }

        // 🟤 Resistente
        if (tipos.Contains(TipoInimigo.Resistente))
        {
            dano -= resistenciaFixa;
            if (dano < 0)
                dano = 0;
        }

        vidaAtual = Mathf.Clamp(vidaAtual - dano, 0, vidaInicial);
        AtualizarUI();

        if (vidaAtual <= 0)
        {
            Morrer();
        }
    }

    private void AplicarRegeneracao()
    {
        if (tipos.Contains(TipoInimigo.Regenerador) && vidaAtual > 0 && estaVivo)
        {
            vidaAtual = Mathf.Clamp(vidaAtual + regeneracaoPorSegundo * Time.deltaTime, 0, vidaInicial);
            AtualizarUI();
        }
    }

    private void Morrer()
    {
        estaVivo = false;

        if (SistemaDeOndas.instancia != null)
        {
            SistemaDeOndas.instancia.RegistrarInimigoDerrotado();
        }

        Debug.Log(nomeInimigo + " foi derrotado!");
        Destroy(gameObject);
    }

    // ================= MOVIMENTO =================

    private void Mover()
    {
        if (caminho == null || indiceAtual >= caminho.Pontos.Length)
            return;

        Transform alvo = caminho.Pontos[indiceAtual];
        Vector3 direcao = (alvo.position - transform.position).normalized;
        transform.position += direcao * velocidadeFinal * Time.deltaTime;

        if (Vector3.Distance(transform.position, alvo.position) < 0.1f)
        {
            indiceAtual++;
            if (indiceAtual >= caminho.Pontos.Length)
            {
                ChegouNaBase();
            }
        }
    }

    private void ChegouNaBase()
    {
        estaVivo = false;

        if (SistemaDeOndas.instancia != null)
        {
            SistemaDeOndas.instancia.RegistrarInimigoDerrotado();
        }

        Caminho caminhoScript = FindObjectOfType<Caminho>();
        if (caminhoScript != null)
        {
            caminhoScript.PerdidaVidaBase(1);
        }

        Debug.Log(nomeInimigo + " chegou na base!");
        Destroy(gameObject);
    }

    // ================= UI =================

    private void AtualizarUI()
    {
        if (fillVida != null)
        {
            fillVida.fillAmount = vidaAtual / vidaInicial;
            if (textoVida != null)
                textoVida.text = Mathf.RoundToInt(vidaAtual) + " / " + Mathf.RoundToInt(vidaInicial);
        }

        if (tipos.Contains(TipoInimigo.Blindado) && escudosAtuais > 0)
        {
            barraEscudo.SetActive(true);
            if (fillEscudo != null)
                fillEscudo.fillAmount = (float)escudosAtuais / quantidadeEscudos;
            if (textoEscudo != null)
                textoEscudo.text = escudosAtuais + " / " + quantidadeEscudos;
        }
        else
        {
            barraEscudo.SetActive(false);
        }
    }

    private void RotacionarUI()
    {
        if (canvasUI != null && cam != null)
            canvasUI.forward = cam.transform.forward;
    }

    public float GetVida() => vidaAtual;
    public float GetVidaMaxima() => vidaInicial;
    public bool EstaVivo() => estaVivo;
    public string GetNome() => nomeInimigo;
}