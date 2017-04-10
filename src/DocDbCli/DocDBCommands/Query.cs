using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;

namespace DocDBCommands
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "query")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Query : CommandBase, ICommand
    {
        public string QueryText { get; set; }

        private static readonly FeedOptions DefaultFeedOptions = new FeedOptions
        {
            EnableCrossPartitionQuery = true
        };

        private static readonly ConnectionPolicy DefaultConnectionPolicy = new ConnectionPolicy
        {
            ConnectionMode = ConnectionMode.Gateway,
            ConnectionProtocol = Protocol.Https
        };

        private Database GetDatabaseIfExists(DocumentClient client, string databaseName)
        {
            return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Get the collection if it exists, null if it doesn't
        /// </summary>
        /// <returns>The requested collection</returns>
        private DocumentCollection GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            if (GetDatabaseIfExists(client, databaseName) == null)
            {
                return null;
            }

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName)).Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

        protected override void BeforeParse(OptionSet optionset)
        {
            Log.Debug("Query.BeforeParse");
            optionset.Add("q|query=", text => QueryText = text);
            base.BeforeParse(optionset);
        }


        public async Task RunAsync()
        {
            DocumentClient client;

            try
            {
                using (client = new DocumentClient(new Uri(_options.EndPoint), _options.AuthorizationKey, DefaultConnectionPolicy))
                {
                    var collection = GetCollectionIfExists(client, _options.DatabaseName, _options.DataCollectionName);
                    if (collection == null)
                    {
                        throw new ArgumentException(string.Format("Database {0}, Collection {1} doesn't exist", _options.DatabaseName, _options.DataCollectionName));
                    }

                    var collectionUri = UriFactory.CreateDocumentCollectionUri(_options.DatabaseName, _options.DataCollectionName);
                    var query = client.CreateDocumentQuery(collectionUri, QueryText, DefaultFeedOptions).AsDocumentQuery();
                    while (query.HasMoreResults)
                    {
                        var result = await query.ExecuteNextAsync();
                        if (_options.Verbose > 0)
                            Console.WriteLine("RequestCharge: {0}", result.RequestCharge);
                        Console.WriteLine(JsonConvert.SerializeObject(result.AsEnumerable()));
                    }
                }
            }
            catch (DocumentClientException e)
            {
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}
