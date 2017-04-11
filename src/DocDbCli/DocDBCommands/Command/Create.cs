/* 
 * Copyright 2015-2017 Takekazu Omi
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "document")]
    [ExportMetadata("Verb", "create")]
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
                using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
                {
                    var collection = GetCollectionIfExists(client, Context.DatabaseName, Context.DataCollectionName);
                    if (collection == null)
                    {
                        throw new ArgumentException(string.Format("Database {0}, Collection {1} doesn't exist", Context.DatabaseName, Context.DataCollectionName));
                    }

                    var collectionUri = UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, Context.DataCollectionName);
                    // TODO Why cannot use json string direct?
                    var newDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonText);
                    var response = await client.CreateDocumentAsync(collectionUri, newDictionary, new RequestOptions());
                    if (Context.Verbose > 0)
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
