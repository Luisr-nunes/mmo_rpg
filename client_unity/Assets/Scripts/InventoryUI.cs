using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    private Canvas canvas;
    private TextMeshProUGUI woodText;
    public Sprite woodIconSprite; // O ícone da madeira (pode ser o toco por enquanto)

    void Start()
    {
        CreateUI();
    }

    void CreateUI()
    {
        // 1. Criar o Canvas
        GameObject canvasObj = new GameObject("InventoryCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Criar o Painel de Fundo (Inventory Bar)
        GameObject panelObj = new GameObject("InventoryPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.85f); // Cinza escuro quase opaco
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0); // Centro embaixo
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 15); // Um pouco acima do fundo
        panelRect.sizeDelta = new Vector2(300, 90); // Painel largo o suficiente para futuros slots

        // 3. Criar o Slot 1 (Madeira)
        GameObject slotObj = new GameObject("WoodSlot");
        slotObj.transform.SetParent(panelObj.transform, false);
        Image slotBg = slotObj.AddComponent<Image>();
        slotBg.color = new Color(0.25f, 0.25f, 0.25f, 1f); // Fundo do slot
        RectTransform slotRect = slotObj.GetComponent<RectTransform>();
        slotRect.anchorMin = new Vector2(0.5f, 0.5f);
        slotRect.anchorMax = new Vector2(0.5f, 0.5f);
        slotRect.pivot = new Vector2(0.5f, 0.5f);
        slotRect.anchoredPosition = new Vector2(-100, 0); // Posicionado à esquerda no painel
        slotRect.sizeDelta = new Vector2(70, 70); // Quadrado maior

        // 4. Criar o Ícone de Madeira (usando a Sprite dedicada de ícone)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(slotObj.transform, false);
        Image iconImage = iconObj.AddComponent<Image>();
        
        // Carrega o ícone 32x32 perfeito que acabamos de criar!
        Sprite iconSprite = Resources.Load<Sprite>("Icon_Wood");
        if (iconSprite != null)
        {
            iconImage.sprite = iconSprite;
            iconImage.preserveAspect = true;
        }
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(50, 50); // Ocupa quase o slot todo

        // 5. Criar o Texto da Quantidade
        GameObject textObj = new GameObject("CountText");
        textObj.transform.SetParent(slotObj.transform, false);
        woodText = textObj.AddComponent<TextMeshProUGUI>();
        woodText.text = "0";
        woodText.fontSize = 28;
        woodText.alignment = TextAlignmentOptions.BottomRight;
        woodText.color = Color.white;
        woodText.enableWordWrapping = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(0, 0);
        textRect.anchoredPosition = new Vector2(-5, 5); // Desloca levemente para dar margem à borda
        
        // Ocultar o texto velho feio do GameManager (já que a UI não precisa mais)
        var gm = FindObjectOfType<GameManager>();
        if (gm != null && gm.uiText != null)
        {
            gm.uiText.gameObject.SetActive(false);
        }
    }

    public void UpdateWood(int amount)
    {
        if (woodText != null)
        {
            woodText.text = amount.ToString();
        }
    }
}
