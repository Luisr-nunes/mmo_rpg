using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    private Dictionary<string, GameObject> playerObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> treeObjects = new Dictionary<string, GameObject>();

    private Sprite[] idleDown;
    private Sprite[] idleUp;
    private Sprite[] idleSide;
    
    private Sprite[] walkDown;
    private Sprite[] walkUp;
    private Sprite[] walkSide;
    
    public Sprite treeAliveSprite;
    public Sprite treeStumpSprite;

    public TextMeshProUGUI uiText; // Texto do inventário (Arraste no editor)
    public TextMeshProUGUI rollText; // Texto do D20 (Arraste no editor)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        idleDown = Resources.LoadAll<Sprite>("Idle_Down");
        idleUp = Resources.LoadAll<Sprite>("Idle_Up");
        idleSide = Resources.LoadAll<Sprite>("Idle_Side");
        
        walkDown = Resources.LoadAll<Sprite>("Walk_Down");
        walkUp = Resources.LoadAll<Sprite>("Walk_Up");
        walkSide = Resources.LoadAll<Sprite>("Walk_Side");
        
        if (idleDown.Length == 0) Debug.LogWarning("Fatias do Idle_Down não encontradas! Não esqueça de fatiar no Sprite Editor.");
        if (treeAliveSprite == null || treeStumpSprite == null) Debug.LogWarning("Árvore não configurada no Inspector do GameManager!");
        
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
        
        // Atualiza a UI do Inventário
        if (uiText != null)
        {
            int myWood = 0;
            if (state.players != null) {
                foreach(var p in state.players) {
                    if (p.id == NetworkManager.Instance.myId) myWood = p.wood;
                }
            }
            uiText.text = "Madeiras: " + myWood;
        }
        
        // Atualiza a UI do D20
        if (rollText != null)
        {
            if (Time.time - NetworkManager.Instance.lastRollTime < 3f) // Mostra o roll por 3 segundos
            {
                if (NetworkManager.Instance.lastRollSuccess)
                    rollText.text = "<color=green>D20: " + NetworkManager.Instance.lastRoll + " (Sucesso!)</color>";
                else
                    rollText.text = "<color=red>D20: " + NetworkManager.Instance.lastRoll + " (Falha)</color>";
            }
            else
            {
                rollText.text = ""; // Esconde depois de 3 segundos
            }
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
                if (idleDown != null && idleDown.Length > 0) sr.sprite = idleDown[0];
                
                // O local player pode ter uma cor ou tag diferente
                if (pData.id == NetworkManager.Instance.myId)
                {
                    var pc = pObj.AddComponent<PlayerController>(); // Anexa controle de movimento
                    pc.idleDown = idleDown;
                    pc.idleUp = idleUp;
                    pc.idleSide = idleSide;
                    pc.walkDown = walkDown;
                    pc.walkUp = walkUp;
                    pc.walkSide = walkSide;
                    
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
                
                // Aumenta o tamanho da árvore para ficar proporcional ao boneco
                tObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);
                
                // Adiciona um BoxCollider2D (como Trigger) para o Player saber que chegou perto
                var col = tObj.AddComponent<BoxCollider2D>();
                col.isTrigger = true;
                col.size = new Vector2(0.5f, 0.5f); // 50 pixels de largura/altura
                
                // Adiciona um pequeno script auxiliar para guardar a ID
                var tagScript = tObj.AddComponent<ResourceTag>();
                tagScript.resourceId = rData.id;

                treeObjects[rData.id] = tObj;
            }

            // Atualiza Sprite (Viva vs Morta)
            var spr = treeObjects[rData.id].GetComponent<SpriteRenderer>();
            // Atualiza a imagem baseado no estado (ativo ou cortado)
            if (treeAliveSprite != null && treeStumpSprite != null)
            {
                if (rData.active) spr.sprite = treeAliveSprite;
                else spr.sprite = treeStumpSprite; 
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
