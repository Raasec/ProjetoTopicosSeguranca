using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EI.SI;

namespace ProtocolSI_cliente
{
    public partial class Form1 : Form
    {
        private const int PORT = 10000;
        NetworkStream networkStream;
        ProtocolSI protocolSI;
        TcpClient client;

        private RSACryptoServiceProvider rsaClient;
        private string publicKeyClient;
        private string serverPublicKey;

        private byte[] chaveSimetrica;  // ### NÃO INICIALIZAR FIXA
        private byte[] ivSimetrica;     // ###

        private string username;

        public Form1(string username)
        {
            InitializeComponent();
            this.username = username;

            IPEndPoint endPoint = new IPEndPoint(IPAddress.Loopback, PORT);

            // Gera par de chaves do cliente
            rsaClient = new RSACryptoServiceProvider(2048);
            publicKeyClient = rsaClient.ToXmlString(false); // Só a pública

            client = new TcpClient();
            client.Connect(endPoint);

            networkStream = client.GetStream();
            protocolSI = new ProtocolSI();

            SendPublicKey();

            // Receber chave AES cifrada e IV
            ReceiveAesKeyAndIV();

            ReceiveServerPublicKey();

            Task.Run(() => ReceiveMessages());
        }


        private void SendPublicKey()
        {
            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, publicKeyClient);
            networkStream.Write(packet, 0, packet.Length);
        }

        private void ReceiveAesKeyAndIV()
        {
            // 1. Receber a chave AES cifrada
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                byte[] chaveAesCifrada = protocolSI.GetData();
                chaveSimetrica = rsaClient.Decrypt(chaveAesCifrada, false);
                Console.WriteLine("Chave AES recebida e descifrada.");
            }

            // 2. Receber o IV (em claro, base64)
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                string ivBase64 = protocolSI.GetStringFromData();
                ivSimetrica = Convert.FromBase64String(ivBase64);
                Console.WriteLine("IV recebido.");
            }
            else
            {
                Console.WriteLine("Erro ao receber IV.");
            }
        }


        private void ReceiveServerPublicKey()
        {
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                serverPublicKey = protocolSI.GetStringFromData();
                Console.WriteLine("Chave pública do servidor recebida.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string mensagemOriginal = textBox1.Text;
            textBox1.Clear();

            // Converter mensagem para bytes
            byte[] dadosMensagem = Encoding.UTF8.GetBytes(mensagemOriginal);

            // Assinar mensagem com chave privada RSA do cliente
            byte[] assinatura = AssinarMensagem(dadosMensagem);

            // Cifrar mensagem com chave AES e IV recebidos do servidor
            byte[] mensagemCifrada = CifrarMensagem(dadosMensagem, chaveSimetrica, ivSimetrica);

            // Converter para Base64 para enviar texto
            string mensagemCifradaBase64 = Convert.ToBase64String(mensagemCifrada);
            string assinaturaBase64 = Convert.ToBase64String(assinatura);

            // Criar string no formato: username|mensagemCifradaBase64|assinaturaBase64
            string pacoteEnvio = $"{username}|{mensagemCifradaBase64}|{assinaturaBase64}";

            // Enviar com protocolo
            byte[] packet = protocolSI.Make(ProtocolSICmdType.DATA, pacoteEnvio);
            networkStream.Write(packet, 0, packet.Length);

            // Esperar ACK do servidor
            while (protocolSI.GetCmdType() != ProtocolSICmdType.ACK)
            {
                int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                if (bytesRead == 0) break;
            }
        }

        private byte[] AssinarMensagem(byte[] dados)
        {
            using (var sha256 = SHA256.Create())
            {
                return rsaClient.SignData(dados, sha256);
            }
        }

        private byte[] CifrarMensagem(byte[] dados, byte[] chave, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = chave;
                aes.IV = iv;

                using (MemoryStream ms = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(dados, 0, dados.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CloseClient();
            this.Close();
        }

        private void CloseClient()
        {
            try
            {
                if (networkStream != null)
                {
                    byte[] eot = protocolSI.Make(ProtocolSICmdType.EOT);
                    networkStream.Write(eot, 0, eot.Length);
                    networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                }
            }
            catch (IOException)
            {
                // Ignorar erros
            }
            finally
            {
                networkStream?.Close();
                client?.Close();
            }
        }

        private void ReceiveMessages()
        {
            try
            {
                while (true)
                {
                    int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
                    if (bytesRead > 0)
                    {
                        ProtocolSICmdType cmd = protocolSI.GetCmdType();
                        if (cmd == ProtocolSICmdType.DATA)
                        {
                            string mensagemRecebida = protocolSI.GetStringFromData();
                            Invoke(new MethodInvoker(delegate
                            {
                                textBox2.AppendText(mensagemRecebida + Environment.NewLine);
                            }));
                        }
                        else if (cmd == ProtocolSICmdType.EOT)
                        {
                            break;
                        }
                    }
                }
            }
            catch (IOException)
            {
                Invoke(new MethodInvoker(delegate
                {
                    textBox2.AppendText("Conexão terminada inesperadamente." + Environment.NewLine);
                }));
                CloseClient();
            }
        }
        private static void EscreverLog(string ip, string username, string mensagem)
        {
            string logPath = "log.txt";
            string linhaLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] IP: {ip} | Utilizador: {username} | Mensagem: {mensagem}";

            try
            {
                File.AppendAllText(logPath, linhaLog + Environment.NewLine);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Erro ao escrever no log: " + ex.Message);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Pode ficar vazio ou adicionar validação
        }
    }
}
