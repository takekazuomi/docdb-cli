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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;

namespace DocDB.Command
{

    public abstract class CommandDocBase : CommandBase, ICommand
    {
        protected abstract Task RunAsync(DocumentClient client);

        protected async Task<Database> GetDatabaseIfExists(DocumentClient client, string databaseName)
        {
            var feed = await client.ReadDatabaseFeedAsync(Context.FeedOptions);
            if (Context.Verbose > 1) {
                Console.WriteLine("FeedResponse:{0}", feed.Dump());
            }
            return feed.Where(f => f.Id == databaseName).FirstOrDefault();
            //return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Get the collection if it exists, null if it doesn't
        /// </summary>
        /// <returns>The requested collection</returns>
        protected async Task<DocumentCollection> GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            if ((await GetDatabaseIfExists(client, databaseName)) == null)
            {
                return null;
            }

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName)).Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

        public Task RunAsync()
        {
            DocumentClient client;
            using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
            {
                if (Context.Wait)
                {
                    Console.Error.Write("press enter to send request. ");
                    Console.ReadLine();
                }
                RunAsync(client).Wait();
            }
            return Task.FromResult<bool>(true);
        }
    }
}
