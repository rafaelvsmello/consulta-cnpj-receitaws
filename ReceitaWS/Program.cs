using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace ReceitaWS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Informe o número do CNPJ: ");
            string cnpj = Console.ReadLine().Replace(".", "").Replace("/", "").Replace("-", ""); // Função para remover os caracteres especiais

            var requisicao = HttpWebRequest.CreateHttp("https://www.receitaws.com.br/v1/cnpj/" + cnpj); // Criação do objeto que será utilizado para criar a requisição
            requisicao.Method = "GET"; // Passando o método GET para q requisão
            requisicao.UserAgent = "Requisicao"; // Header do HTTP para evitar o erro 403

            using (var resposta = requisicao.GetResponse()) // Obtendo a resposta da requisição
            {
                var streamDados = resposta.GetResponseStream(); 
                StreamReader reader = new StreamReader(streamDados);
                object objResponse = reader.ReadToEnd();

                var post = JsonConvert.DeserializeObject<Empresa>(objResponse.ToString()); // Utilizando a API do Json para desserializar o objeto response da requisição

                if (post.status.Equals("OK"))  // Tratando a resposta da requisição
                {
                    Console.WriteLine("NOME: {0}", post.nome);
                    Console.WriteLine("NOME FANTASIA: {0}", post.fantasia);
                    Console.WriteLine("ATIVIDADE PRINCIPAL: {0}", post.atividade_principal[0].text.ToUpper());
                    Console.WriteLine("LOGRADOURO: {0}, Nº {1}", post.logradouro, post.numero);
                    Console.WriteLine("BAIRRO: {0} CEP: {1}", post.bairro, post.cep);
                    Console.WriteLine("MUNICIPIO: {0} UF: {1}", post.municipio, post.uf);
                } 
                else 
                {
                    Console.WriteLine("Ocorreu um erro: {0}", post.message);
                }

                // Fechamento dos objetos
                streamDados.Close();
                reader.Close();
            }
        }
    }
}
