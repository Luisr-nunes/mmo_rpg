using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> treeObjects = new Dictionary<string, GameObject>();

    private Sprite[] playerSprites;
    private Sprite[] treeSprites;

    public Text uiText; // Pode ser arrastado no editor depois

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Carrega as fatias cortadas do Sprite Editor automaticamente da pasta Resources
        playerSprites = Resources.LoadAll<Sprite>("player");
        treeSprites = Resources.LoadAll<Sprite>("objects");
        
        if (playerSprites.Length == 0) Debug.LogWarning("Fatias do Player não encontradas! Não esqueça de fatiar no Sprite Editor.");
        if (treeSprites.Length == 0) Debug.LogWarning("Fatias das Árvores não encontradas! Não esqueça de fatiar no Sprite Editor.");
        
        GenerateGrassBackground();
    }

    void Update()
    {
        var state = NetworkManager.Instance.currentState;
        if (state == null) return;
        
        // Debug silencioso para checar se está lendo corretamente
        if (playerObjects.Count == 0 && state.players != null)
        {
            Debug.Log("Recebi " + state.players.Length + " players do servidor!");
        }

        UpdatePlayers(state.players);
        UpdateTrees(state.resources);
        
        // Atualiza a UI se o componente estiver conectado
        if (uiText != null)
        {
            uiText.text = "Conectado. ID: " + NetworkManager.Instance.myId;
        }
    }

    void UpdatePlayers(PlayerData[] players)
    {
        if (players == null) return;
        
        HashSet<string> currentIds = new HashSet<string>();

        foreach (var pData in players)
        {
            currentIds.Add(pData.id);
            if (!playerObjects.ContainsKey(pData.id))
            {
                // Cria um novo GameObject para o jogador
                GameObject pObj = new GameObject("Player_" + pData.id);
                var sr = pObj.AddComponent<SpriteRenderer>();
                
                // Pega a primeira fatia do player (Idle) se existir
                if (playerSprites.Length > 0) sr.sprite = playerSprites[0];
                
                // O local player pode ter uma cor ou tag diferente
                if (pData.id == NetworkManager.Instance.myId)
                {
                    pObj.AddComponent<PlayerController>(); // Anexa controle de movimento
                    
                    // Adiciona física para detectar quando esbarra na árvore
                    var rb = pObj.AddComponent<Rigidbody2D>();
                    rb.bodyType = RigidbodyType2D.Kinematic; // Para não cair com gravidade
                    var pCol = pObj.AddComponent<BoxCollider2D>();
                    pCol.size = new Vector2(0.5f, 0.5f);
                    
                    // Define a posição inicial do jogador local baseado no servidor
                    pObj.transform.position = new Vector3(pData.x / 100f, -pData.y / 100f, 0);
                }
                else
                {
                    sr.color = new Color(0.8f, 0.8f, 1f); // Outros players ficam levemente azulados
                }

                playerObjects[pData.id] = pObj;
            }

            // Atualiza Posição (Convertendo de Pixels do Python para Unity Units)
            // No Unity o Y cresce para cima, no Python cresce para baixo, então invertemos o Y.
            if (pData.id != NetworkManager.Instance.myId) // O PlayerController local cuida de si mesmo
            {
                playerObjects[pData.id].transform.position = new Vector3(pData.x / 100f, -pData.y / 100f, 0);
            }
        }
    }

    void UpdateTrees(ResourceData[] resources)
    {
        if (resources == null) return;
        
        foreach (var rData in resources)
        {
            if (rData.type != "wood") continue;
            
            if (!treeObjects.ContainsKey(rData.id))
            {
                GameObject tObj = new GameObject("Tree_" + rData.id);
                var sr = tObj.AddComponent<SpriteRenderer>();
                
                tObj.transform.position = new Vector3(rData.x / 100f, -rData.y / 100f, 0);
                
                // Adiciona um BoxCollider2D (como Trigger) para o Player saber que chegou perto
                var col = tObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.5f, 0.5f); // 50 pixels de raio
                
                // Adiciona um pequeno script auxiliar para guardar a ID
                var tagScript = tObj.AddComponent<ResourceTag>();
                tagScript.resourceId = rData.id;

                treeObjects[rData.id] = tObj;
            }

            // Atualiza Sprite (Viva vs Morta)
            var spr = treeObjects[rData.id].GetComponent<SpriteRenderer>();
            if (treeSprites.Length > 0)
            {
                // A árvore verde é a fatia [0], a árvore morta de inverno pode ser a fatia [3]
                // Isso dependerá de como for fatiado no Sprite Editor!
                if (rData.active) spr.sprite = treeSprites[0];
                else spr.sprite = treeSprites[3]; 
            }
        }
    }

    void GenerateGrassBackground()
    {
        Camera.main.backgroundColor = new Color(0.24f, 0.44f, 0.17f); // Fundo verde base
        Camera.main.orthographicSize = 1.5f; // Dá um "Zoom" violento para a gente ver os pixels de perto!
    }
}

// Pequeno script para guardar a ID da árvore na Unity
public class ResourceTag : MonoBehaviour
{
    public string resourceId;
}
