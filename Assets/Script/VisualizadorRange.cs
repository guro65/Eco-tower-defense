using UnityEngine;

public class VisualizadorRange : MonoBehaviour
{
    private Personagem personagem;
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;
    private CircleCollider2D colliderRange;

    [SerializeField] private Color corFullAOE = new Color(1f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private Color corCone = new Color(1f, 0.7f, 0.3f, 0.3f);
    [SerializeField] private Color corBolaAOE = new Color(0.5f, 1f, 0.5f, 0.3f);
    [SerializeField] private Color corUnico = new Color(0.5f, 0.5f, 1f, 0.3f);

    private float anguloAtualCone = 0f;

    private void Start()
    {
        personagem = GetComponentInParent<Personagem>();

        // Pega o CircleCollider2D deste GameObject
        colliderRange = GetComponent<CircleCollider2D>();

        // Pega ou cria o LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            ConfigurarLineRenderer();
        }

        // Pega o SpriteRenderer existente
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void ConfigurarLineRenderer()
    {
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.sortingOrder = 1;

        Material material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.material = material;

        lineRenderer.enabled = false;
    }

    private void Update()
    {
        if (personagem == null)
            return;

        if (spriteRenderer != null && spriteRenderer.enabled)
        {
            DesenharBaseadoNoTipo();
        }
        else
        {
            lineRenderer.enabled = false;
        }
    }

    private void DesenharBaseadoNoTipo()
    {
        switch (personagem.GetTipoRange())
        {
            case Personagem.TipoRange.FullAOE:
                DesenharFullAOE();
                break;
            case Personagem.TipoRange.Cone:
                DesenharCone();
                break;
            case Personagem.TipoRange.BolaAOE:
                DesenharBolaAOE();
                break;
            case Personagem.TipoRange.Unico:
                DesenharUnico();
                break;
        }
    }

    // ========== OBTER RAIO MÁXIMO ==========
    /// <summary>
    /// Obtém o raio do CircleCollider2D para respeitar o limite da range
    /// </summary>
    private float ObtenerRaioMaximo()
    {
        if (colliderRange != null)
        {
            return colliderRange.radius;
        }
        return personagem.GetAlcance();
    }

    // ========== FULL AOE ==========
    private void DesenharFullAOE()
    {
        lineRenderer.enabled = true;

        float raioMaximo = ObtenerRaioMaximo();
        int segmentos = 60;

        Vector3[] pontos = new Vector3[segmentos + 1];

        for (int i = 0; i <= segmentos; i++)
        {
            float angulo = (i / (float)segmentos) * 360f * Mathf.Deg2Rad;
            pontos[i] = new Vector3(Mathf.Cos(angulo) * raioMaximo, Mathf.Sin(angulo) * raioMaximo, 0);
        }

        lineRenderer.positionCount = pontos.Length;
        lineRenderer.SetPositions(pontos);
        lineRenderer.startColor = corFullAOE;
        lineRenderer.endColor = corFullAOE;
    }

    // ========== CONE ==========
    private void DesenharCone()
    {
        lineRenderer.enabled = true;

        float raioMaximo = ObtenerRaioMaximo();
        float anguloTotal = personagem.GetAnguloCone();
        float anguloMetade = anguloTotal / 2f;

        CalcularAnguloParaInimigo();

        int segmentos = 30;
        Vector3[] pontos = new Vector3[segmentos + 2];

        pontos[0] = Vector3.zero;

        for (int i = 0; i <= segmentos; i++)
        {
            float angulo = (anguloAtualCone - anguloMetade + (i / (float)segmentos) * anguloTotal) * Mathf.Deg2Rad;
            pontos[i + 1] = new Vector3(Mathf.Cos(angulo) * raioMaximo, Mathf.Sin(angulo) * raioMaximo, 0);
        }

        lineRenderer.positionCount = pontos.Length;
        lineRenderer.SetPositions(pontos);
        lineRenderer.startColor = corCone;
        lineRenderer.endColor = corCone;
    }

    // ========== BOLA AOE ==========
    private void DesenharBolaAOE()
    {
        lineRenderer.enabled = true;

        float raioBolaAOE = personagem.GetRaioBolaAOE();
        float raioMaximo = ObtenerRaioMaximo();

        // Garante que a bola não ultrapasse o raio máximo do collider
        float raioAjustado = Mathf.Min(raioBolaAOE, raioMaximo);

        int segmentos = 40;

        Vector3[] pontos = new Vector3[segmentos + 1];

        for (int i = 0; i <= segmentos; i++)
        {
            float angulo = (i / (float)segmentos) * 360f * Mathf.Deg2Rad;
            pontos[i] = new Vector3(Mathf.Cos(angulo) * raioAjustado, Mathf.Sin(angulo) * raioAjustado, 0);
        }

        lineRenderer.positionCount = pontos.Length;
        lineRenderer.SetPositions(pontos);
        lineRenderer.startColor = corBolaAOE;
        lineRenderer.endColor = corBolaAOE;
    }

    // ========== ÚNICO ==========
    private void DesenharUnico()
    {
        lineRenderer.enabled = true;

        Vector3[] pontos = new Vector3[2];
        pontos[0] = new Vector3(-0.2f, 0, 0);
        pontos[1] = new Vector3(0.2f, 0, 0);

        lineRenderer.positionCount = pontos.Length;
        lineRenderer.SetPositions(pontos);
        lineRenderer.startColor = corUnico;
        lineRenderer.endColor = corUnico;
    }

    // ========== CÁLCULO DE ÂNGULO PARA INIMIGO ==========
    private void CalcularAnguloParaInimigo()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(personagem.transform.position, ObtenerRaioMaximo());

        Inimigo inimigoMaisProximo = null;
        float menorDist = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Inimigo inimigo = hit.GetComponent<Inimigo>();
            if (inimigo != null)
            {
                float dist = Vector3.Distance(personagem.transform.position, inimigo.transform.position);
                if (dist < menorDist)
                {
                    menorDist = dist;
                    inimigoMaisProximo = inimigo;
                }
            }
        }

        if (inimigoMaisProximo != null)
        {
            Vector2 direcaoParaInimigo = (inimigoMaisProximo.transform.position - personagem.transform.position).normalized;
            anguloAtualCone = Mathf.Atan2(direcaoParaInimigo.y, direcaoParaInimigo.x) * Mathf.Rad2Deg;
        }
        else
        {
            anguloAtualCone = 0f;
        }
    }
}