using System.Collections.Generic;
using UnityEngine;

public class BolaAOE
{
    private Vector3 posicao;
    private Vector3 posicaoOrigem;
    private Inimigo alvo;
    private float raio;
    private float velocidade;
    private float alcanceMaximo;
    private float dano;
    private float tempoVida = 0f;
    private float tempoMaximoVida = 10f;
    private Personagem personagemDono;

    public BolaAOE(Vector3 posicaoInicial, Inimigo alvoTarget, float raioAOE, float velPerseguicao, float alcanceMax, float danoBola, Personagem dono)
    {
        posicao = posicaoInicial;
        posicaoOrigem = posicaoInicial;
        alvo = alvoTarget;
        raio = raioAOE;
        velocidade = velPerseguicao;
        alcanceMaximo = alcanceMax;
        dano = danoBola;
        personagemDono = dono;
    }

    public void Atualizar(float deltaTime)
    {
        tempoVida += deltaTime;

        if (alvo == null || tempoVida > tempoMaximoVida)
            return;

        // Verifica se o alvo saiu do alcance máximo
        float distanciaDoOrigem = Vector3.Distance(posicaoOrigem, alvo.transform.position);
        if (distanciaDoOrigem > alcanceMaximo)
        {
            return; // A bola não segue mais (sai do alcance)
        }

        // Move a bola em direção ao inimigo
        Vector3 direcao = (alvo.transform.position - posicao).normalized;
        posicao += direcao * velocidade * deltaTime;

        // Detecta inimigos dentro do raio da bola
        Collider2D[] hits = Physics2D.OverlapCircleAll(posicao, raio);
        foreach (Collider2D hit in hits)
        {
            Inimigo inimigo = hit.GetComponent<Inimigo>();
            if (inimigo != null && Vector3.Distance(posicaoOrigem, inimigo.transform.position) <= alcanceMaximo)
            {
                inimigo.ReceberDano(dano);
            }
        }
    }

    public bool DeveSerRemovida()
    {
        return alvo == null || tempoVida > tempoMaximoVida;
    }

    public Vector3 GetPosicao() => posicao;
}