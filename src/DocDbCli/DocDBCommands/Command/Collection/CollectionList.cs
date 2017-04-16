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
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "collection")]
    [ExportMetadata("Verb", "list")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class CollectionList : CommandBase
    {
        protected override Task RunAsync(DocumentClient client)
        {
            if (GetDatabaseIfExists(client, Context.DatabaseName) != null)
            {
                var databaseLink = UriFactory.CreateDatabaseUri(Context.DatabaseName);
                var collection = client.CreateDocumentCollectionQuery(databaseLink, Context.FeedOptions);
                foreach (var documentCollection in collection)
                {
                    Console.WriteLine("Id:{0}, ResourceId:{1}, ConflictsLink:{2}", documentCollection.Id, documentCollection.ResourceId, documentCollection.ConflictsLink);
                }
            }
            else
            {
                Log.Warning("Not exist Database:{0}, Collection:{1}", Context.DatabaseName, Context.DataCollectionName);
            }

            // TODO
            return Task.FromResult<bool>(true);
        }
    }
}

