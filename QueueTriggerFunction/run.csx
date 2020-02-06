#r "Newtonsoft.Json"
#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

public static void Run(string myQueueItem, ILogger log)
{    

    log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");        
    
    var cotacao = JsonConvert.DeserializeObject<Acao>(myQueueItem);

    if (!String.IsNullOrWhiteSpace(cotacao.Codigo) &&
         cotacao.Valor.HasValue && cotacao.Valor > 0)
    {
        cotacao.Codigo = cotacao.Codigo.Trim().ToUpper();

        var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        var acaoTable = storageAccount.CreateCloudTableClient().GetTableReference(Environment.GetEnvironmentVariable("TableQuotation"));
        if (acaoTable.CreateIfNotExistsAsync().Result)
            log.LogInformation("Criando a tabela: " + Environment.GetEnvironmentVariable("TableQuotation"));

        AcaoEntity dadosAcao =
            new AcaoEntity(
                cotacao.Codigo,
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

        dadosAcao.Valor = cotacao.Valor.Value;

        var insertOperation = TableOperation.Insert(dadosAcao);
        var resultInsert = acaoTable.ExecuteAsync(insertOperation).Result;

        log.LogInformation($"QueueTriggerFunction: {myQueueItem}");
    }
    else
        log.LogError($"AcoesQueueTrigger - Erro validação: {myQueueItem}");

}

public class Acao
{
    public string Codigo { get; set; }        
    public double? Valor { get; set; }        
}

public class AcaoEntity : TableEntity
{
    public AcaoEntity(string codigo, string horario)
    {
        PartitionKey = codigo;
        RowKey = horario;
    }

    public AcaoEntity() { }
        
    public double Valor { get; set; }
}