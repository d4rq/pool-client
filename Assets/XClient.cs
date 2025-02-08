using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MyProtocol;
using MyProtocol.Packets;
using MyProtocol.Serializator;
using TMPro;
using UnityEngine;

namespace Client
{
    public class XClient
    {
        public Action<byte[]> OnPacketRecieve { get; set; }

        private readonly ConcurrentQueue<byte[]> _packetSendingQueue = new ConcurrentQueue<byte[]>();

        private Socket _socket;
        private IPEndPoint _serverEndPoint;

        private bool handshakeCompleted = false;
        private bool gameJoined = false;

        public void Connect(string ip, int port)
        {
            Connect(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        private void Connect(IPEndPoint server)
        {
            _serverEndPoint = server;

            var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = ipHostInfo.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            if (ipAddress == null)
            {
                throw new Exception("No valid IPv4 address found.");
            }

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(_serverEndPoint);

            Console.WriteLine($"Connected to server at {_serverEndPoint}");

            Task.Run(RecievePackets);
            Task.Run(SendPackets);
        }

        public void QueuePacketSend(byte[] packet)
        {
            if (packet.Length > 256)
            {
                throw new Exception("Max packet size is 256 bytes.");
            }

            _packetSendingQueue.Enqueue(packet);
        }

        private async Task RecievePackets()
        {
            while (true)
            {
                var buff = new byte[256];
                int receivedBytes = await _socket.ReceiveAsync(buff, SocketFlags.None);

                if (receivedBytes == 0)
                {
                    Console.WriteLine("Server closed the connection.");
                    return;
                }

                buff = buff.TakeWhile((b, i) =>
                {
                    if (b != 0xFF) return true;
                    return buff[i + 1] != 0;
                }).Concat(new byte[] { 0xFF, 0 }).ToArray();

                //HandleIncomingPacket(buff);
                OnPacketRecieve(buff);
            }
        }

        private void HandleIncomingPacket(byte[] packet)
        {
            var parsedPacket = Packet.Parse(packet);
            if (parsedPacket != null)
            {
                var packetType = PacketTypeManager.GetTypeFromPacket(parsedPacket);

                if (packetType == PacketType.Handshake && !handshakeCompleted)
                {
                    ProcessHandshake(parsedPacket);
                }
                else if (packetType == PacketType.PlayerJoin && !gameJoined)
                {
                    ProcessPlayerJoin(parsedPacket);
                }
                else if (packetType == PacketType.StrikeBall)
                {
                    ProcessStrikeBall(parsedPacket);
                }
                else if (packetType == PacketType.GameResult)
                {
                    ProcessGameResult(parsedPacket);
                }
                else if (packetType == PacketType.BallPocketed)
                {
                    ProcessBallPocketed(parsedPacket);
                }
                else if (packetType == PacketType.ChangeTurn)
                {
                    ProcessChangeTurn(parsedPacket);
                }
                else if (packetType == PacketType.PlayerList)
                {
                    ProcessPlayerList(parsedPacket);
                }
                else if (packetType == PacketType.Error)
                {
                    ProcessError(parsedPacket);
                }
            }
        }

        // Вынесенная обработка хэндшейка
        private void ProcessHandshake(Packet packet)
        {
            var handshake = PacketConverter.Deserialize<PacketHandshake>(packet);
            handshake.MagicHandshakeNumber -= 15;

            Console.WriteLine($"Sending response to handshake with MagicHandshakeNumber: {handshake.MagicHandshakeNumber}");

            QueuePacketSend(PacketConverter.Serialize(PacketType.Handshake, handshake).ToPacket());

            handshakeCompleted = true; // Завершаем хэндшейк
        }

        private void ProcessPlayerJoin(Packet packet)
        {
            var playerJoin = PacketConverter.Deserialize<PacketPlayerJoin>(packet);
            if (playerJoin.ErrorMessage != "")
            {
                Console.WriteLine(playerJoin.ErrorMessage);
                return;
            }
            Console.WriteLine($"{playerJoin.PlayerName} has joined the game.");
        }

        private void ProcessStrikeBall(Packet packet)
        {
            var strikeBall = PacketConverter.Deserialize<PacketStrikeBall>(packet);
            Console.WriteLine($"Strike ball with power {strikeBall.Power} and direction {strikeBall.Direction.X}, {strikeBall.Direction.Y}");
        }

        private void ProcessGameResult(Packet packet)
        {
            var gameResult = PacketConverter.Deserialize<PacketGameResult>(packet);
            Console.WriteLine($"Game over! Winner: {gameResult.WinnerName}, Scores: P1-{gameResult.Player1Score}, P2-{gameResult.Player2Score}, P3-{gameResult.Player3Score}, P4-{gameResult.Player4Score}");
        }

        private void ProcessBallPocketed(Packet packet)
        {
            var ballInPocket = PacketConverter.Deserialize<PacketBallPocketed>(packet);
            Console.WriteLine($"{ballInPocket.PlayerNames} strike ball in pocket {ballInPocket.BallNumber}");
        }

        private void ProcessChangeTurn(Packet packet)
        {
            var ChangeСourse = PacketConverter.Deserialize<PacketChangeTurn>(packet);
            Console.WriteLine($"Now is turn {ChangeСourse.PlayerName}");
        }

        private void ProcessPlayerList(Packet packet)
        {
            for (int i = 0; i < PacketConverter.Deserialize<PacketPlayerList>(packet).Players.Count; i++)
            {
                GameObject.FindGameObjectsWithTag("Name")[i].GetComponent<TMP_Text>().text = PacketConverter.Deserialize<PacketPlayerList>(packet).Players[i];
            }
        }

        private void ProcessError(Packet packet)
        {
            var error = PacketConverter.Deserialize<PacketError>(packet);
            Console.WriteLine($"Error: {error.ErrorMessage}");
        }

        private async Task SendPackets()
        {
            while (true)
            {
                if (_packetSendingQueue.Count == 0)
                {
                    await Task.Delay(100);
                    continue;
                }

                byte[]? packet;
                while (!_packetSendingQueue.TryDequeue(out packet))
                {
                }
                if (_socket.Connected)
                {
                    await _socket.SendAsync(new ArraySegment<byte>(packet), SocketFlags.None);
                }
                else
                {
                    Console.WriteLine("Socket is disconnected. Aborting send.");
                    break;
                }

                await Task.Delay(100);
            }
        }

        // Метод для отправки пакета с присоединением игрока
        public void SendPlayerJoinPacket(string playerName)
        {
            var playerJoinPacket = new PacketPlayerJoin { PlayerName = playerName };
            QueuePacketSend(PacketConverter.Serialize(PacketType.PlayerJoin, playerJoinPacket).ToPacket());
        }

        // Метод для отправки пакета удара по шару
        public void SendStrikeBallPacket(float power, Vector2D direction)
        {
            Console.WriteLine("Send Strike");
            var strikeBallPacket = new PacketStrikeBall
            {
                Power = power,
                Direction = direction
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.StrikeBall, strikeBallPacket).ToPacket());
        }


        // Метод для отправки пакета результата игры
        public void SendGameResultPacket(string winnerName, int p1Score, int p2Score, int p3Score, int p4Score)
        {
            var gameResultPacket = new PacketGameResult
            {
                WinnerName = winnerName,
                Player1Score = p1Score,
                Player2Score = p2Score,
                Player3Score = p3Score,
                Player4Score = p4Score
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.GameResult, gameResultPacket).ToPacket());
        }

        public void SendBallPocket(string playerName, int ballNumber)
        {
            var ballPocketed = new PacketBallPocketed()
            {
                PlayerNames = playerName,
                BallNumber = ballNumber
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.BallPocketed, ballPocketed).ToPacket());
        }

        public void SendChangeTurn(string playerName)
        {
            var changeTurn = new PacketChangeTurn()
            {
                PlayerName = playerName
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.ChangeTurn, changeTurn).ToPacket());
        }

        public void SendPlayerList(List<string> players)
        {
            var playerList = new PacketPlayerList()
            {
                Players = players
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.PlayerList, playerList).ToPacket());
        }

        public void SendError(string errorMessage)
        {
            var error = new PacketError()
            {
                ErrorMessage = errorMessage
            };
            QueuePacketSend(PacketConverter.Serialize(PacketType.Error, error).ToPacket());
        }
    }
}