using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json.Linq;

namespace DocDBCommands
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "create")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Create : CommandBase, ICommand
    {
        private string _jsonText;
        public string JsonText
        {
            set
            {
                if (value.Length > 0 && File.Exists(value))
                    _jsonText = File.ReadAllText(value);
                else
                    _jsonText = value;
            }
            get { return _jsonText; }
        }

        protected override void BeforeParse(OptionSet optionset)
        {
            Log.Debug("Create.BeforeParse");
            optionset.Add("j|json=", text => JsonText = text);
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
                    // TODO Why cannot use json string direct?
                    var newDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonText);
                    var response = await client.CreateDocumentAsync(collectionUri, newDictionary, new RequestOptions());
                    if (_options.Verbose > 0)
                    {
                        Console.WriteLine("RequestCharge: {0}", response.RequestCharge);
                        Console.WriteLine(response.Resource);
                    }
                }
            }
            catch (DocumentClientException e)
            {
                Console.Error.WriteLine("create error: {0}", e.Message);
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}
