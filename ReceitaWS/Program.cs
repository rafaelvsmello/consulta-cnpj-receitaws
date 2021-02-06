using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using LiteDB;

namespace ReceitaWS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Informe o número do CNPJ: ");            
            string cnpj = Console.ReadLine().Replace(".", "").Replace("/", "").Replace("-", ""); // Função para remover os caracteres especiais

            var result = Consultar(cnpj);

            if (!result)
            {
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
                        Console.WriteLine("\nNOME: {0}", post.nome); // Adicionado \n (quebra de linha) para facilitar a visualização
                        Console.WriteLine("NOME FANTASIA: {0}", post.fantasia);
                        Console.WriteLine("ATIVIDADE PRINCIPAL: {0}", post.atividade_principal[0].text.ToUpper());
                        Console.WriteLine("LOGRADOURO: {0}, Nº {1}", post.logradouro, post.numero);
                        Console.WriteLine("BAIRRO: {0} CEP: {1}", post.bairro, post.cep);
                        Console.WriteLine("MUNICIPIO: {0} UF: {1}", post.municipio, post.uf);

                        Console.Write("\nIncluir empresa no banco de dados (Y)/(N): "); // Adicionado \n (quebra de linha) para facilitar a visualização
                        string resp = Console.ReadLine().ToUpper();

                        if (resp.Equals("Y"))
                        {
                            using (var db = new LiteDatabase(@"C:\Data.db"))
                            {
                                var col = db.GetCollection<Empresa>("empresa");

                                var empresa = new Empresa
                                {
                                    cnpj = post.cnpj.Replace(".", "").Replace("/", "").Replace("-", ""),
                                    nome = post.nome,
                                    fantasia = post.fantasia,
                                    atividade_principal = post.atividade_principal,
                                    logradouro = post.logradouro,
                                    numero = post.numero,
                                    bairro = post.bairro,
                                    cep = post.cep,
                                    municipio = post.municipio,
                                    uf = post.uf
                                };

                                col.Insert(empresa);
                                Console.WriteLine("CADASTRO REALIZADO COM SUCESSO!");
                            }
                        }
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

        static bool Consultar(string cnpj)
        {
            using(var db = new LiteDatabase(@"C:\Data.db"))
            {
                var col = db.GetCollection<Empresa>("empresa");
                var empresa = col.FindOne(x => x.cnpj == cnpj);

                if (empresa != null)
                {
                    Console.WriteLine("\nNOME: {0}", empresa.nome); // Adicionado \n (quebra de linha) para facilitar a visualização
                    Console.WriteLine("NOME FANTASIA: {0}", empresa.fantasia);
                    Console.WriteLine("ATIVIDADE PRINCIPAL: {0}", empresa.atividade_principal[0].text.ToUpper());
                    Console.WriteLine("LOGRADOURO: {0}, Nº {1}", empresa.logradouro, empresa.numero);
                    Console.WriteLine("BAIRRO: {0} CEP: {1}", empresa.bairro, empresa.cep);
                    Console.WriteLine("MUNICIPIO: {0} UF: {1}", empresa.municipio, empresa.uf);
                    return true;
                } 
                else
                {
                    return false;
                }
            }
        }

    }
}
