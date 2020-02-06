#r "Microsoft.WindowsAzure.Storage"

using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;


public static void Run(TimerInfo myTimer, ILogger log)
{
    log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

    var storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
    
    var disponibilidadeTable = storageAccount.CreateCloudTableClient().GetTableReference(Environment.GetEnvironmentVariable("TableName"));

    if (disponibilidadeTable.CreateIfNotExistsAsync().Result)
        log.LogInformation("Criando a tabela: " + Environment.GetEnvironmentVariable("TableName"));

    DisponibilidadeEntity dadosDisponibilidade = new DisponibilidadeEntity(Environment.GetEnvironmentVariable("PartitionKey"),DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    dadosDisponibilidade.Mensagem = "TimerTriggerFunction em execucao";
    var insertOperation = TableOperation.Insert(dadosDisponibilidade);
    var resultInsert = disponibilidadeTable.ExecuteAsync(insertOperation).Result;

    log.LogInformation($"**** Teste de disponibilidade executado em : {DateTime.Now}");



}

public class DisponibilidadeEntity : TableEntity
{
    public DisponibilidadeEntity(string local, string horario)
    {
        PartitionKey = local;
        RowKey = horario;
    }

    public DisponibilidadeEntity() { }
        
    public string Mensagem { get; set; }
}
