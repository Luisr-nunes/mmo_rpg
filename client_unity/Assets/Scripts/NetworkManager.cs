using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Collections.Concurrent;

[Serializable]
public class GameState {
    public PlayerData[] players;
    public ResourceData[] resources;
}

[Serializable]
public class PlayerData {
    public string id;
    public string name;
    public float x;
    public float y;
    public int wood;
}

[Serializable]
public class ResourceData {
    public string id;
    public string type;
    public float x;
    public float y;
    public bool active;
}

[Serializable]
public class ServerMessage {
    public string type;
    public string id; // Usado no pacote init
    public GameState state;
    
    // Usado no pacote dice_roll
    public string player_id;
    public int roll;
    public int amount;
    public string resource_type;
}

[Serializable]
public class ClientMoveMessage {
    public string type = "move";
    public float x;
    public float y;
}

[Serializable]
public class ClientCollectMessage {
    public string type = "collect";
    public string resource_id;
}

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;
    
    private ClientWebSocket ws;
    private CancellationTokenSource cts;
    private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();
    
    public string myId = "";
    public GameState currentState;
    
    public int lastRoll = 0;
    public bool lastRollSuccess = false;
    public float lastRollTime = -10f;

    void Awake()
    {
        Instance = this;
    }

    async void Start()
    {
        ws = new ClientWebSocket();
        cts = new CancellationTokenSource();
        try
        {
            await ws.ConnectAsync(new Uri("ws://localhost:8765"), cts.Token);
            Debug.Log("Conectado ao servidor Python!");
            ReceiveLoop();
        }
        catch(Exception e)
        {
            Debug.LogError("Erro na conexão: " + e.Message);
        }
    }

    void Update()
    {
        while (mainThreadActions.TryDequeue(out Action action))
        {
            action.Invoke();
        }
    }

    async void ReceiveLoop()
    {
        var buffer = new byte[8192];
        while (ws.State == WebSocketState.Open)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                System.Net.WebSockets.WebSocketReceiveResult result;
                do
                {
                    result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == System.Net.WebSockets.WebSocketMessageType.Text)
                {
                    string json = Encoding.UTF8.GetString(ms.ToArray());
                    mainThreadActions.Enqueue(() => ProcessMessage(json));
                }
            }
        }
    }

    void ProcessMessage(string json)
    {
        ServerMessage msg = JsonUtility.FromJson<ServerMessage>(json);
        
        if (msg.type == "init" || msg.type == "game_state")
        {
            if (msg.state == null || msg.state.players == null)
            {
                Debug.LogError("JsonUtility FALHOU ao extrair a lista de jogadores do JSON: " + json);
            }
        }
        
        if (msg.type == "init")
        {
            myId = msg.id;
            currentState = msg.state;
            Debug.Log("Logado com o ID: " + myId);
        }
        else if (msg.type == "game_state")
        {
            currentState = msg.state;
            // Aqui vamos chamar os scripts de visualização (GameManager) para atualizar a tela
        }
        else if (msg.type == "dice_roll")
        {
            Debug.Log("Chegou um dice_roll! Roll: " + msg.roll + " Amount: " + msg.amount + " Player: " + msg.player_id);
            // Guarda o resultado para a UI exibir
            if (msg.player_id == myId)
            {
                lastRoll = msg.roll;
                lastRollSuccess = msg.amount > 0;
                lastRollTime = Time.time;
                Debug.Log("D20 salvo com sucesso! Time: " + lastRollTime);
            }
        }
    }
    
    public void SendMove(float x, float y)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            var msg = new ClientMoveMessage { x = x, y = y };
            string json = JsonUtility.ToJson(msg);
            var bytes = Encoding.UTF8.GetBytes(json);
            ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
        }
    }
    
    public void SendCollect(string resourceId)
    {
        if (ws != null && ws.State == WebSocketState.Open)
        {
            var msg = new ClientCollectMessage { resource_id = resourceId };
            string json = JsonUtility.ToJson(msg);
            var bytes = Encoding.UTF8.GetBytes(json);
            ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        ws?.Dispose();
    }
}
