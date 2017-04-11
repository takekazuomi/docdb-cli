﻿/* 
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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "collection")]
    [ExportMetadata("Verb", "create")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class CollectionCreate : CommandBase, ICommand
    {
 
        public async Task RunAsync()
        {
            try
            {
                DocumentClient client;
                using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
                {
                    if (GetDatabaseIfExists(client, Context.DatabaseName) != null)
                    {
                        var collection = client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(Context.DatabaseName), Context.FeedOptions);
                        foreach (var documentCollection in collection)
                        {
                            Console.WriteLine("Id:{0}, ResourceId:{1}", documentCollection.Id, documentCollection.ResourceId, documentCollection.ConflictsLink);
                        }
                    }
                    else
                    {
                        Log.Warning("Not exist Database:{0}, Collection:{1}", Context.DatabaseName, Context.DataCollectionName);
                    }
                }
            }
            catch (DocumentClientException e)
            {
                Console.Error.WriteLine("collection list error: {0}", e.Message);
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}