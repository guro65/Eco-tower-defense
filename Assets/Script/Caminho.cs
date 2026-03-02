using UnityEngine;
using System.Collections.Generic;

public class Caminho : MonoBehaviour
{
    [Header("Waypoints")]
    private Transform[] pontos;
    public Transform[] Pontos => pontos;

    [Header("Spawn")]
    [SerializeField] private Transform posicaoSpawn;
    [SerializeField] private float tempoGeracaoInimigo = 2f;
    [SerializeField] private List<GameObject> prefabsInimigos = new List<GameObject>(); // Lista de prefabs

    [Header("Base")]
    [SerializeField] private Transform posicaoBase;
    [SerializeField] private int vidaBase = 5;

    private int vidaAtual;
    private float tempoProximoSpawn = 0f;

    private void Awake()
    {
        pontos = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            pontos[i] = transform.GetChild(i);

        vidaAtual = vidaBase;
    }

    private void Update()
    {
        if (SistemaDeOndas.instancia != null && SistemaDeOndas.instancia.PodeSpawnarInimigo())
        {
            tempoProximoSpawn -= Time.deltaTime;
            if (tempoProximoSpawn <= 0f)
            {
                SpawnarInimigoAleatorio();
                tempoProximoSpawn = tempoGeracaoInimigo;
            }
        }
    }

    private void SpawnarInimigoAleatorio()
    {
        if (posicaoSpawn == null || prefabsInimigos == null || prefabsInimigos.Count == 0)
        {
            Debug.LogError("Posição de Spawn ou Lista de Prefabs de Inimigo não configuradas!");
            return;
        }

        // Seleciona prefab aleatório da lista
        GameObject prefab = prefabsInimigos[Random.Range(0, prefabsInimigos.Count)];
        GameObject inimigoGO = Instantiate(prefab, posicaoSpawn.position, Quaternion.identity);

        Inimigo inimigo = inimigoGO.GetComponent<Inimigo>();
        if (inimigo != null)
        {
            inimigo.caminho = this;

            // Sorteia tipos aleatórios (1 ou 2 tipos)
            SortearTiposAleatorios(inimigo);

            // Configura aparência e vida aleatórias
            inimigo.ConfigurarAparenciaEVida();

            Debug.Log("Inimigo spawnado com tipos: " + string.Join(", ", inimigo.tipos));
        }
    }

    private void SortearTiposAleatorios(Inimigo inimigo)
    {
        var valoresTipo = System.Enum.GetValues(typeof(Inimigo.TipoInimigo));
        inimigo.tipos.Clear();

        // Sorteia 1 ou 2 tipos
        int quantidadeTipos = Random.Range(1, 3);
        List<Inimigo.TipoInimigo> tiposSorteados = new List<Inimigo.TipoInimigo>();

        while (tiposSorteados.Count < quantidadeTipos)
        {
            Inimigo.TipoInimigo novoTipo = (Inimigo.TipoInimigo)valoresTipo.GetValue(Random.Range(0, valoresTipo.Length));
            if (!tiposSorteados.Contains(novoTipo))
                tiposSorteados.Add(novoTipo);
        }

        inimigo.tipos.AddRange(tiposSorteados);
    }

    public void PerdidaVidaBase(int dano = 1)
    {
        vidaAtual -= dano;
        Debug.Log("Base perdeu " + dano + " de vida. Vida atual: " + vidaAtual);

        if (vidaAtual <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("GAME OVER! A base foi destruída!");
        Time.timeScale = 0f;
    }

    public Transform GetPosicaoBase() => posicaoBase;
    public int GetVidaBase() => vidaAtual;
    public int GetVidaMaximaBase() => vidaBase;
    public float GetTempoGeracaoInimigo() => tempoGeracaoInimigo;

    public void DefinirTempoGeracaoInimigo(float novoTempo)
    {
        tempoGeracaoInimigo = Mathf.Max(0.1f, novoTempo);
    }
}