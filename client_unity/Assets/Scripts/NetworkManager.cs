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
    
    // Usado no pacote roll_result
    public string player_id;
    public int roll;
    public bool success;
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
        var buffer = new byte[16384];
        while (ws.State == WebSocketState.Open)
        {
            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                mainThreadActions.Enqueue(() => ProcessMessage(json));
            }
        }
    }

    void ProcessMessage(string json)
    {
        Debug.Log("RAW JSON: " + json);
        ServerMessage msg = JsonUtility.FromJson<ServerMessage>(json);
        
        if (msg.state != null && msg.state.players != null)
        {
            Debug.Log("JsonUtility Parseou com Sucesso! Jogadores: " + msg.state.players.Length);
        }
        else
        {
            Debug.LogError("JsonUtility FALHOU ao extrair a lista de jogadores do JSON.");
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
        else if (msg.type == "roll_result")
        {
            // Guarda o resultado para a UI exibir
            if (msg.player_id == myId)
            {
                lastRoll = msg.roll;
                lastRollSuccess = msg.success;
                lastRollTime = Time.time;
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
