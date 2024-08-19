using System.IO;
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
            /*if (args.Length != 3)
            {
                Console.WriteLine("Programa precisa ser executado com 3 argumentos:");
                Console.WriteLine("Ação preço_de_venda preço_de_compra");
                return;
            }*/

            //String ativo = args[0];
            //float precoVenda = float.Parse(args[1]);
            //float precoCompra = float.Parse(args[2]);
            String ativo = "PETR4";
            float precoVenda = 22.67f;
            float precoCompra = 22.59f;


            XmlDocument xmlDocument = new();
            try
            {
                XmlTextReader reader = new("config.xml")
                {
                    WhitespaceHandling = WhitespaceHandling.None
                };
                reader.MoveToContent();
                xmlDocument.Load(reader);
            }
            catch (FileNotFoundException ex) 
            {
                Console.WriteLine(ex.Message);
            }

            Config config = LerConfig(xmlDocument);

            //while (true) 
            //{
            
                Task<String> result = GetJsonFromRequest(ativo, config.GetToken());
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

        static Config LerConfig(XmlDocument xmlDocument)
        {
            XmlNodeList? xmlTokenApiNodeList = xmlDocument.SelectNodes("root//tokenApi");
            XmlNodeList? xmlDestinationEmailNodeList = xmlDocument.SelectNodes("root//destinationEmail");
            XmlNodeList? xmlHostNodeList = xmlDocument.SelectNodes("root//host");
            XmlNodeList? xmlPortNodeList = xmlDocument.SelectNodes("root//port");
            XmlNodeList? xmlUsernameNodeList = xmlDocument.SelectNodes("root//credentials//username");
            XmlNodeList? xmlPasswordNodeList = xmlDocument.SelectNodes("root//credentials//password");
            String token, destinationEmail, host, username, password;
            int port;

            token = xmlTokenApiNodeList[0].InnerText;
            destinationEmail = xmlDestinationEmailNodeList[0].InnerText;
            host = xmlHostNodeList[0].InnerText;
            port = int.Parse(xmlPortNodeList[0].InnerText);
            username = xmlUsernameNodeList[0].InnerText;
            password = xmlPasswordNodeList[0].InnerText;

            return new Config(token, destinationEmail, host, port, username, password);
        }

        static async Task<String> GetJsonFromRequest(String ativo, String token)
        {
            using HttpClient client = new();
            Uri endPoint = new($"https://brapi.dev/api/quote/{ativo}?range=1d&interval=1d&token={token}");
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
                if (root.ContainsKey("results")) {
                    JsonNode? preco = root["results"][0]["regularMarketPrice"];
                    if (preco != null)
                    {
                        return float.Parse(preco.ToString());
                    }
                }
            }
            return -1f;
        }
    }
}
