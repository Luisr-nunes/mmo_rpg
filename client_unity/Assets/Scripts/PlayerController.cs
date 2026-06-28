using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float speed = 1.0f; // Unidades Unity (100 pixels) por segundo
    
    // Lista de árvores na área para coletar
    private List<string> nearbyResources = new List<string>();

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
        
        if (dx != 0 || dy != 0)
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
