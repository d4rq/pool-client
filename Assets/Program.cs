using Client;
using MyProtocol;
using MyProtocol.Packets;
using MyProtocol.Serializator;
using System;
using System.Net.Sockets;
using System.Threading;
using TMPro;
using UnityEngine;

class Program : MonoBehaviour
{
    public static XClient client;
    public static string currentPlayer;

    private void Start()
    {
        Main();
    }

    private static int _handshakeMagic;

    private static void Main()
    {
        Console.Title = "XClient";
        Console.ForegroundColor = ConsoleColor.White;
            
        client = new XClient();
        var name = MenuInput.name;

        client.OnPacketRecieve += OnPacketRecieve;

        client.Connect("127.0.0.1", 4910);

        var rand = new System.Random();
        _handshakeMagic = rand.Next();

        Thread.Sleep(1000);
            
        Console.WriteLine("Sending handshake packet..");

        client.QueuePacketSend(
            PacketConverter.Serialize(
                    PacketType.Handshake,
                    new PacketHandshake
                    {
                        MagicHandshakeNumber = _handshakeMagic
                    })
                .ToPacket());

        client.QueuePacketSend(
            PacketConverter.Serialize(
                    PacketType.PlayerJoin,
                    new PacketPlayerJoin
                    {
                        PlayerName = name ?? "Денис",
                    })
                .ToPacket());

        client.QueuePacketSend(
            PacketConverter.Serialize(
                    PacketType.ChangeTurn,
                    new PacketChangeTurn
                    {
                        PlayerName = name ?? "Денис",
                    })
                .ToPacket());
    }

    private static void OnPacketRecieve(byte[] packet)
    {
        var parsed = Packet.Parse(packet);

        if (parsed != null)
        {
            ProcessIncomingPacket(parsed);
        }
    }

    private static void ProcessIncomingPacket(Packet packet)
    {
        var type = PacketTypeManager.GetTypeFromPacket(packet);

        switch (type)
        {
            case PacketType.Handshake:
                ProcessHandshake(packet);
                break;
            case PacketType.PlayerJoin:
                ProcessPlayerJoin(packet);
                break;
            case PacketType.StrikeBall:
                ProcessStrikeBall(packet);
                break;
            case PacketType.GameResult:
                ProcessGameResult(packet);
                break;
            case PacketType.PlayerList:
                ProcessPlayerList(packet);
                break;
            case PacketType.ChangeTurn:
                ProcessChangeTurn(packet);
                break;
            default:
                Console.WriteLine($"Unknown packet type: {type}");
                break;
        }
    }

    private static void ProcessPlayerList(Packet packet)
    {
        for (int i = 0; i < PacketConverter.Deserialize<PacketPlayerList>(packet).Players.Count; i++)
        {
            GameObject.FindGameObjectsWithTag("Name")[i].GetComponent<TMP_Text>().text = PacketConverter.Deserialize<PacketPlayerList>(packet).Players[i];
        }
    }

    private static void ProcessHandshake(Packet packet)
    {
        var handshake = PacketConverter.Deserialize<PacketHandshake>(packet);

        if (_handshakeMagic - handshake.MagicHandshakeNumber == 15)
        {
            Console.WriteLine("Handshake successful!");
        }
    }

    private static void ProcessPlayerJoin(Packet packet)
    {
        var playerJoin = PacketConverter.Deserialize<PacketPlayerJoin>(packet);
        Console.WriteLine($"{playerJoin.PlayerName} has joined the game.");
    }

    private static void ProcessStrikeBall(Packet packet)
    {
        var strikeBall = PacketConverter.Deserialize<PacketStrikeBall>(packet);

        var number = strikeBall.BallNumber;

        var direction = strikeBall.Direction;
        var vector = new Vector2(direction.X, direction.Y);
        var force = strikeBall.Power;

        var ball = GameObject.Find($"Ball ({number})");
        var b = ball.GetComponent<Rigidbody2D>();

        b.AddForce(vector * force, ForceMode2D.Impulse);
    }

    private static void ProcessGameResult(Packet packet)
    {
        var gameResult = PacketConverter.Deserialize<PacketGameResult>(packet);
        Console.WriteLine($"Game over! Winner: {gameResult.WinnerName}, Scores: P1-{gameResult.Player1Score}, P2-{gameResult.Player2Score}, P3-{gameResult.Player3Score}, P4-{gameResult.Player4Score}");
    }

    private static void ProcessChangeTurn(Packet packet)
    {
        var player = PacketConverter.Deserialize<PacketChangeTurn>(packet);

        currentPlayer = player.PlayerName;
        PocketHandler.needsChange = true;
    }
}
