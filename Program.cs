using System.Net;
using System.Text.Json.Nodes;
using System.Xml;

namespace desafio_bt
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int intervaloRequisicao = 4 * 60;
            if (args.Length != 3)
            {
                Console.WriteLine("Programa precisa ser executado com 3 argumentos:");
                Console.WriteLine("Ação preço_de_venda preço_de_compra");
                return;
            }

            String ativo = args[0];
            float precoVenda = float.Parse(args[1]);
            float precoCompra = float.Parse(args[2]);
            
            XmlDocument xmlDocument = new();
            string path = @"config.xml";
            if (File.Exists(path))
            {
                xmlDocument.LoadXml(path);
                XmlNodeList? xmlNodeList = xmlDocument.SelectNodes("root");
                String token, destinationEmail, host, username, password;
                int port;

                if (xmlNodeList != null)
                {
                    token = xmlNodeList[0].InnerText;
                    destinationEmail = xmlNodeList[1].InnerText;
                    host = xmlNodeList[2].InnerText;
                    port = int.Parse(xmlNodeList[3].InnerText);
                    username = xmlNodeList[4].ChildNodes[0].InnerText;
                    password = xmlNodeList[4].ChildNodes[1].InnerText;

                    //while (true) 
                    //{
                        Task<String> result = HandleRequest(ativo, token);
                        if (result != null) 
                        {
                            float precoAtual = GetPrecoAtualAtivo(result.Result);
                            if (precoAtual < 0f)
                            {
                                Console.WriteLine("Erro ao verificar o preço da ação");
                            }
                            if (precoAtual < precoCompra)
                            {
                                Console.WriteLine("Mandar email para comprar");
                            }
                            else if (precoAtual > precoVenda)
                            {
                                Console.WriteLine("Mandar email para vender");
                            }
                        }

                        //Thread.Sleep(intervaloRequisicao * 1000);
                    //}
                }

            }
            else
            {
                Console.WriteLine("Arquivo de configuração não encontrado!");
            }
        }

        static async Task<String> HandleRequest(String ativo, String token)
        {
            using HttpClient client = new();
            Uri endPoint = new($"https://brapi.dev/api/quote/{ativo}?range=1d&interval=1m&token={token}");
            var response = await client.GetAsync(endPoint);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine($"Erro ao solicitar requisição, código status: {response.StatusCode}");
            }
 
            return response.Content.ReadAsStringAsync().Result;
        }

        static float GetPrecoAtualAtivo(String requestResult)
        {
            JsonObject? root = JsonNode.Parse(requestResult)?.AsObject();
            if (root != null) 
            {
                JsonNode? preco = root["regularMarketPrice"];
                if (preco != null)
                {
                    return float.Parse(preco.ToString());
                }
            }
            return -1f;
        }
    }
}
