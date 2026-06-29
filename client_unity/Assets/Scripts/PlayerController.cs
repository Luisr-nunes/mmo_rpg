using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 1.0f; // Unidades Unity (100 pixels) por segundo
    
    // Lista de árvores na área para coletar
    private List<string> nearbyResources = new List<string>();

    public Sprite[] idleDown;
    public Sprite[] idleUp;
    public Sprite[] idleSide;
    
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkSide;

    private SpriteRenderer spriteRenderer;
    private float animationTimer = 0f;
    private int currentFrame = 0;
    
    // 0 = Down, 1 = Up, 2 = Side
    private int facingDirection = 0; 

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float dx = 0;
        float dy = 0;
        
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed) dx += 1;
            if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed) dx -= 1;
            if (Keyboard.current.upArrowKey.isPressed || Keyboard.current.wKey.isPressed) dy += 1;
            if (Keyboard.current.downArrowKey.isPressed || Keyboard.current.sKey.isPressed) dy -= 1;
            
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)
            {
                if (nearbyResources.Count > 0)
                {
                    NetworkManager.Instance.SendCollect(nearbyResources[0]);
                }
            }
        }
        
        bool isWalking = (dx != 0 || dy != 0);
        
        // Atualiza a direção que o personagem está olhando
        if (dx != 0) facingDirection = 2; // Side
        else if (dy > 0) facingDirection = 1; // Up
        else if (dy < 0) facingDirection = 0; // Down
        
        if (dx < 0 && spriteRenderer != null) spriteRenderer.flipX = true;
        else if (dx > 0 && spriteRenderer != null) spriteRenderer.flipX = false;
        
        // Seleciona a Array correta de sprites baseado no estado
        Sprite[] currentArray = null;
        if (isWalking)
        {
            if (facingDirection == 0) currentArray = walkDown;
            else if (facingDirection == 1) currentArray = walkUp;
            else currentArray = walkSide;
        }
        else
        {
            if (facingDirection == 0) currentArray = idleDown;
            else if (facingDirection == 1) currentArray = idleUp;
            else currentArray = idleSide;
        }
        
        // Animação via Código!
        if (currentArray != null && currentArray.Length > 0 && spriteRenderer != null)
        {
            animationTimer += Time.deltaTime;
            
            // Walk geralmente é mais rápido. Idle é mais devagar.
            float frameSpeed = isWalking ? 0.1f : 0.2f; 
            
            if (animationTimer >= frameSpeed) 
            {
                animationTimer = 0f;
                currentFrame++;
                if (currentFrame >= currentArray.Length) currentFrame = 0; 
                spriteRenderer.sprite = currentArray[currentFrame];
            }
        }
        
        if (isWalking)
        {
            // Normaliza para não andar mais rápido nas diagonais
            Vector2 dir = new Vector2(dx, dy).normalized;
            transform.Translate(dir * speed * Time.deltaTime);
            
            // Envia a posição real para o servidor
            // Multiplicamos por 100 para enviar na escala de pixels e invertemos o Y
            float sendX = transform.position.x * 100f;
            float sendY = -transform.position.y * 100f;
            
            NetworkManager.Instance.SendMove(sendX, sendY);
        }
        
        // Faz a câmera seguir o jogador local suavemente
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = Mathf.Lerp(camPos.x, transform.position.x, 0.1f);
        camPos.y = Mathf.Lerp(camPos.y, transform.position.y, 0.1f);
        Camera.main.transform.position = camPos;
    }
    
    // Quando entra na área de Trigger da árvore (configurado no GameManager)
    void OnTriggerEnter2D(Collider2D col)
    {
        var tag = col.GetComponent<ResourceTag>();
        if (tag != null)
        {
            if (!nearbyResources.Contains(tag.resourceId))
                nearbyResources.Add(tag.resourceId);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        var tag = col.GetComponent<ResourceTag>();
        if (tag != null)
        {
            nearbyResources.Remove(tag.resourceId);
        }
    }
}
