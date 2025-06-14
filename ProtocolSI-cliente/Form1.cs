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
        private string username = "Cliente1";  // <---- PLACEHOLDER, depois tem de vir do login

        private byte[] chaveSimetrica;  // ### NÃO INICIALIZAR FIXA
        private byte[] ivSimetrica;     // ###

        public Form1()
        {
            InitializeComponent();
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
            // Recebe chave AES cifrada
            networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                byte[] chaveAesCifrada = protocolSI.GetData();

                // Descifra chave AES com chave privada RSA
                chaveSimetrica = rsaClient.Decrypt(chaveAesCifrada, false); // false = PKCS1, usar true para OAEP

                Console.WriteLine("Chave AES recebida e descifrada.");
            }

            // Recebe IV AES (em base64)
            int bytesRead = networkStream.Read(protocolSI.Buffer, 0, protocolSI.Buffer.Length);
            if (bytesRead > 0 && protocolSI.GetCmdType() == ProtocolSICmdType.DATA)
            {
                byte[] chaveAesCifrada = protocolSI.GetData();
                chaveSimetrica = rsaClient.Decrypt(chaveAesCifrada, false);
                Console.WriteLine("Chave AES recebida e descifrada.");
            }
            else
            {
                Console.WriteLine("Erro a receber chave AES cifrada.");
                // Tratar erro (ex: fechar conexão)
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // Pode ficar vazio ou adicionar validação
        }
    }
}
