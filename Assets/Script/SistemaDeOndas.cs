using UnityEngine;
using TMPro;

public class SistemaDeOndas : MonoBehaviour
{
    public static SistemaDeOndas instancia;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI textoOnda;
    [SerializeField] private TextMeshProUGUI textoInimigosRestantes;

    [Header("Configuração de Ondas")]
    [SerializeField] private int[] inimigosParaOndas = { 10, 15, 20, 25, 30 };

    [Header("Progressão de Vida")]
    [SerializeField] private float aumentoVidaPorOnda = 0.25f; // 25% de aumento por onda

    private int ondaAtual = 1;
    private int inimigosNecessarios = 0;
    private int inimigosDerrotados = 0;
    private bool podSpawnar = true;

    public static float multiplicadorVidaInimigo { get; private set; } = 1f;

    private void Awake()
    {
        if (instancia == null)
        {
            instancia = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        IniciarOnda();
        AtualizarUI();
    }

    private void Update()
    {
        VerificiarProgresso();
    }

    private void IniciarOnda()
    {
        // Define quantidade de inimigos para essa onda
        if (ondaAtual - 1 < inimigosParaOndas.Length)
        {
            inimigosNecessarios = inimigosParaOndas[ondaAtual - 1];
        }
        else
        {
            inimigosNecessarios = inimigosParaOndas[inimigosParaOndas.Length - 1] + (ondaAtual - inimigosParaOndas.Length) * 5;
        }

        // Calcula o multiplicador de vida para essa onda
        multiplicadorVidaInimigo = 1f + (ondaAtual - 1) * aumentoVidaPorOnda;

        inimigosDerrotados = 0;
        podSpawnar = true;

        Debug.Log("=== ONDA " + ondaAtual + " INICIADA ===");
        Debug.Log("Inimigos necessários: " + inimigosNecessarios);
        Debug.Log("Multiplicador de vida: " + multiplicadorVidaInimigo.ToString("F2") + "x");
    }

    private void VerificiarProgresso()
    {
        int inimigosNaCena = GameObject.FindGameObjectsWithTag("Inimigo").Length;

        if (!podSpawnar && inimigosNaCena == 0 && inimigosDerrotados >= inimigosNecessarios)
        {
            PassarParaProximaOnda();
        }
    }

    public void RegistrarInimigoDerrotado()
    {
        inimigosDerrotados++;
        AtualizarUI();
    }

    private void PassarParaProximaOnda()
    {
        ondaAtual++;
        Debug.Log("Passando para a ONDA " + ondaAtual);
        IniciarOnda();
        AtualizarUI();
    }

    public bool PodeSpawnarInimigo()
    {
        int inimigosCena = GameObject.FindGameObjectsWithTag("Inimigo").Length;

        if (inimigosDerrotados + inimigosCena < inimigosNecessarios)
        {
            return true;
        }

        podSpawnar = false;
        return false;
    }

    private void AtualizarUI()
    {
        int inimigosNaCena = GameObject.FindGameObjectsWithTag("Inimigo").Length;
        int faltam = Mathf.Max(0, inimigosNecessarios - inimigosDerrotados - inimigosNaCena);

        if (textoOnda != null)
        {
            textoOnda.text = "ONDA " + ondaAtual;
        }

        if (textoInimigosRestantes != null)
        {
            textoInimigosRestantes.text = "Inimigos: " + faltam;
        }
    }

    public int GetOndaAtual() => ondaAtual;
    public int GetInimigosNecessarios() => inimigosNecessarios;
    public int GetInimigosDerrotados() => inimigosDerrotados;
}