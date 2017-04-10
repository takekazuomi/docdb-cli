﻿using Microsoft.Azure.Documents;
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
                Console.Error.WriteLine("query error: {0}", e.Message);
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}
