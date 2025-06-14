using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using EI.SI;

namespace ProtocolSI_servidor
{
    internal class Program
    {
        private const int PORT = 10000;

        // Agora armazenamos chave pública RSA do cliente por TcpClient
        private static Dictionary<TcpClient, RSACryptoServiceProvider> clients = new Dictionary<TcpClient, RSACryptoServiceProvider>();

        // Guardar chave AES por cliente para decriptar mensagens
        private static Dictionary<TcpClient, Aes> aesKeys = new Dictionary<TcpClient, Aes>(); // ###

        private static RSACryptoServiceProvider rsaServer;
        private static string publicKeyServer;

        static void Main(string[] args)
        {
            // Gera as chaves RSA do servidor
            rsaServer = new RSACryptoServiceProvider(2048);
            publicKeyServer = rsaServer.ToXmlString(false); // Só chave pública

            TcpListener listener = new TcpListener(IPAddress.Any, PORT);
            listener.Start();
            Console.WriteLine("Servidor a correr na porta " + PORT);

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine("Cliente conectado.");
                Thread t = new Thread(() => HandleClient(client));
                t.Start();
            }
        }

        static void HandleClient(TcpClient client)
        {
            NetworkStream networkStream = client.GetStream();
            ProtocolSI protocolSI = new ProtocolSI();

            try
            {
                // 1. Recebe chave pública do cliente (string XML)
                int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                if (bytesRead > 0 && protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
                {
                    string clientPublicKeyXML = protocolSI.GetStringFromData();
                    Console.WriteLine("Chave pública recebida do cliente.");

                    // Converter a chave pública XML para RSACryptoServiceProvider
                    RSACryptoServiceProvider rsaCliente = new RSACryptoServiceProvider();
                    rsaCliente.FromXmlString(clientPublicKeyXML);

                    lock (clients)
                    {
                        clients.Add(client, rsaCliente);
                    }

                    // 2. Gerar chave AES para este cliente ###

                    Aes aes = Aes.Create();
                    aes.KeySize = 128; // 128 bits
                    aes.GenerateKey();
                    aes.GenerateIV();

                    // Guardar chave AES para o cliente
                    lock (aesKeys)
                    {
                        aesKeys.Add(client, aes);
                    }

                    // Cifrar chave AES com chave pública do cliente
                    byte[] chaveAesCifrada = rsaCliente.Encrypt(aes.Key, false); // false = PKCS1 (podes usar OAEP se quiseres)

                    // Enviar chave AES cifrada (binário) via ProtocolSI
                    byte[] packetChave = protocolSI.Make(ProtocolSICmdType.DATA, chaveAesCifrada);
                    networkStream.Write(packetChave, 0, packetChave.Length);

                    // Enviar IV em texto base64 (pode ser em claro)
                    string ivBase64 = Convert.ToBase64String(aes.IV);
                    byte[] packetIV = protocolSI.Make(ProtocolSICmdType.DATA, ivBase64);
                    networkStream.Write(packetIV, 0, packetIV.Length);

                    // 3. Envia chave pública do servidor (opcional, mantive para continuidade)
                    byte[] pubKeyPacket = protocolSI.Make(ProtocolSICmdType.DATA, publicKeyServer);
                    networkStream.Write(pubKeyPacket, 0, pubKeyPacket.Length);
                }

                // Resto da comunicação
                while (true)
                {
                    int bytesReadMsg = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    if (bytesReadMsg == 0) break;

                    ProtocolSICmdType cmd = protocolSI.GetCmdType();

                    if (cmd == ProtocolSICmdType.DATA)
                    {
                        string data = protocolSI.GetStringFromData();

                        // Esperamos mensagem formatada como: username|mensagemCifradaBase64|assinaturaBase64
                        string[] partes = data.Split('|');
                        if (partes.Length != 3)
                        {
                            RegistarLog("Mensagem com formato inválido recebida.");
                            continue;
                        }

                        string username = partes[0];
                        string mensagemCifradaBase64 = partes[1];
                        string assinaturaBase64 = partes[2];

                        byte[] mensagemCifrada = Convert.FromBase64String(mensagemCifradaBase64);
                        byte[] assinatura = Convert.FromBase64String(assinaturaBase64);

                        string mensagemOriginal = "";

                        try
                        {
                            Aes aes;
                            lock (aesKeys)
                            {
                                aes = aesKeys[client];
                            }
                            mensagemOriginal = DesencriptarMensagem(mensagemCifrada, aes.Key, aes.IV);
                        }
                        catch (Exception ex)
                        {
                            RegistarLog($"Erro ao desencriptar mensagem do {username}: {ex.Message}");
                            continue;
                        }

                        bool assinaturaValida = false;
                        lock (clients)
                        {
                            if (clients.TryGetValue(client, out RSACryptoServiceProvider rsaCliente))
                            {
                                assinaturaValida = VerificarAssinatura(rsaCliente, Encoding.UTF8.GetBytes(mensagemOriginal), assinatura);
                            }
                            else
                            {
                                RegistarLog($"Chave pública do cliente não encontrada para {username}.");
                            }
                        }

                        if (assinaturaValida)
                        {
                            RegistarLog($"Mensagem válida de {username}: {mensagemOriginal}");
                            Console.WriteLine($"{username}: {mensagemOriginal}");

                            // Enviar ACK
                            byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
                            networkStream.Write(ack, 0, ack.Length);

                            // Broadcast mensagem desencriptada
                            BroadcastMessage($"{username}: {mensagemOriginal}", client);
                        }
                        else
                        {
                            RegistarLog($"Assinatura inválida da mensagem de {username}");
                        }
                    }
                    else if (cmd == ProtocolSICmdType.EOT)
                    {
                        Console.WriteLine("Cliente desconectado.");
                        byte[] ack = protocolSI.Make(ProtocolSICmdType.ACK);
                        networkStream.Write(ack, 0, ack.Length);
                        break;
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Erro de ligação com o cliente.");
            }
            finally
            {
                lock (clients)
                {
                    clients.Remove(client);
                }
                lock (aesKeys)
                {
                    if (aesKeys.ContainsKey(client))
                        aesKeys.Remove(client);
                }
                networkStream.Close();
                client.Close();
            }
        }

        static void BroadcastMessage(string message, TcpClient sender)
        {
            lock (clients)
            {
                foreach (var pair in clients)
                {
                    TcpClient client = pair.Key;
                    if (client != sender)
                    {
                        try
                        {
                            NetworkStream ns = client.GetStream();
                            ProtocolSI protocolSI = new ProtocolSI();
                            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, message);
                            ns.Write(packet, 0, packet.Length);
                        }
                        catch
                        {
                            // Ignorar erros de envio
                        }
                    }
                }
            }
        }

        static void RegistarLog(string texto)
        {
            string path = "log.txt";
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {texto}";
            File.AppendAllText(path, logEntry + Environment.NewLine, Encoding.UTF8);
        }

        static bool VerificarAssinatura(RSACryptoServiceProvider rsa, byte[] dados, byte[] assinatura)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return rsa.VerifyData(dados, sha256, assinatura);
            }
        }

        static string DesencriptarMensagem(byte[] dadosCifrados, byte[] chave, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = chave;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream(dadosCifrados))
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (StreamReader sr = new StreamReader(cs))
                {
                    return sr.ReadToEnd();
                }
            }
        }
    }
}
