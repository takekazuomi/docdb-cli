using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "query")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Query : CommandBase, ICommand
    {
        public string QueryText { get; set; }

        protected override void BeforeParse(OptionSet optionset)
        {
            Log.Debug("Query.BeforeParse");
            optionset.Add("q|query=", text => QueryText = text);
            base.BeforeParse(optionset);
        }

        public async Task RunAsync()
        {
            try
            {
                DocumentClient client;
                using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
                {
                    var collection = GetCollectionIfExists(client, Context.DatabaseName, Context.DataCollectionName);
                    if (collection == null)
                    {
                        throw new ArgumentException(string.Format("Database {0}, Collection {1} doesn't exist", Context.DatabaseName, Context.DataCollectionName));
                    }

                    var collectionUri = UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, Context.DataCollectionName);
                    var query = client.CreateDocumentQuery(collectionUri, QueryText, Context.FeedOptions).AsDocumentQuery();
                    while (query.HasMoreResults)
                    {
                        var result = await query.ExecuteNextAsync();
                        if (Context.Verbose > 0)
                            Console.WriteLine("RequestCharge: {0}", result.RequestCharge);
                        Console.WriteLine(JsonConvert.SerializeObject(result.AsEnumerable()));
                    }
                }
            }
            catch (DocumentClientException e)
            {
                Console.Error.WriteLine("query error: {0}", e.Message);
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}
