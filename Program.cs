using System.Net;
using System.Net.Mail;
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
            float precoVenda = Single.Parse(args[1], System.Globalization.CultureInfo.GetCultureInfo("en-US"));
            float precoCompra = float.Parse(args[2], System.Globalization.CultureInfo.GetCultureInfo("en-US"));

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
            catch (FileNotFoundException)
            {
                Console.WriteLine("Não foi possível encontrar o arquivo de configurações, certifique-se que este se encontra na mesma pasta do executável");
                Environment.Exit(1);
            }

            Config config = LerConfig(xmlDocument);

            while (true) 
            {
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
                        EnviarEmail(true, config, ativo, precoAtual, precoCompra);
                        Console.WriteLine($"Email enviado para comprar ativo com o valor R${precoAtual}");
                    }
                    else if (precoAtual > precoVenda)
                    {
                        EnviarEmail(false, config, ativo, precoAtual, precoVenda);
                        Console.WriteLine($"Email enviado para vender ativo com o valor R${precoAtual}");
                    }
                }

                Thread.Sleep(intervaloRequisicao * 1000);
            }
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

        static async Task<String> GetJsonFromRequest(String acao, String token)
        {
            using HttpClient client = new();
            Uri endPoint = new($"https://brapi.dev/api/quote/{acao}?range=1d&interval=1d&token={token}");
            var response = await client.GetAsync(endPoint);

            switch(response.StatusCode)
            {
                case HttpStatusCode.BadRequest:
                    Console.WriteLine($"Má requisição, verifique os dados do arquivo de configuração, código status: {response.StatusCode}");
                    Environment.Exit(1);
                    break;
                case HttpStatusCode.Unauthorized:
                    Console.WriteLine($"Não autorizado, o seu token é inválido, por favor, verifique o seu token em brapi.dev/dashboard, código status: {response.StatusCode}");
                    Environment.Exit(1);
                    break;
                case HttpStatusCode.PaymentRequired:
                    Console.WriteLine($"Você atingiu o limite de requisições para o seu plano. " +
                        $"Por favor, considere fazer um upgrade para um plano melhor em brapi.dev/dashboard, código status: {response.StatusCode}");
                    Environment.Exit(1);
                    break;
                case HttpStatusCode.NotFound:
                    Console.WriteLine($"Não encontramos a ação {acao}, código status: {response.StatusCode}");
                    Environment.Exit(1);
                    break;
                default:
                    break;
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
                        return preco.GetValue<float>();
                    }
                }
            }
            return -1f;
        }
        static void EnviarEmail(bool isCompra, Config config, String acao, float precoAtualAtivo, float precoCompraVenda)
        {
            var client = new SmtpClient(config.GetHost(), config.GetPort())
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(config.GetUsername(), config.GetPassword())
            };

            if (isCompra)
            {
                String assuntoCompra = $"Alerta para compra da ação {acao}!";
                String mensagemCompra = $"Aconselhamos a compra da ação {acao}, pois o preço do ativo é R${precoAtualAtivo} e está abaixo do valor estipulado: R${precoCompraVenda}";
                client.Send(
                    new MailMessage(config.GetUsername(), config.GetDestinationEmail(), assuntoCompra, mensagemCompra));
            }
            else
            {
                String assuntoVenda = $"Alerta para venda da ação {acao}!";
                String mensagemVenda = $"Aconselhamos a venda da ação {acao}, pois o preço do ativo é R${precoAtualAtivo} e está acima do valor solicitado: R${precoCompraVenda}";
                client.Send(
                    new MailMessage(config.GetUsername(), config.GetDestinationEmail(), assuntoVenda, mensagemVenda));
            }
        }
    }
}
