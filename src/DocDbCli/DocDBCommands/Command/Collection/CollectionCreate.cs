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
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "collection")]
    [ExportMetadata("Verb", "create")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class CollectionCreate : CommandDocBase
    {
        public string PartitionKey { get; set; }
        public string OfferType { get; set; }
        public int? OfferThroughput { get; set; }

        protected override void BeforeParse(OptionSet optionset)
        {
            optionset.Add("p|partitionKey=", v => PartitionKey = v);
            optionset.Add("t|throughput=", v => OfferThroughput = Int32.Parse(v));
            optionset.Add("o|offerType=", v => OfferType = v);
            base.BeforeParse(optionset);
        }

        protected override void CheckRequiredOption(Context contextBefore, Context contextAfter)
        {
            if (string.IsNullOrEmpty(contextBefore.DataCollectionName))
            {
                var msg = "Missing required option -c=CollectionName";
                throw new InvalidOperationException(msg);
            }
        }

        protected override async Task RunAsync(DocumentClient client)
        {
            var database = await client.CreateDatabaseIfNotExistsAsync(new Database {Id = Context.DatabaseName});

            var collection = new DocumentCollection {Id = Context.DataCollectionName};
            collection.PartitionKey.Paths.Add(PartitionKey);

            var databaseUri = UriFactory.CreateDatabaseUri(Context.DatabaseName);

            Console.WriteLine("{0}, {1}", databaseUri, database.Resource.SelfLink);
            var result = await client.CreateDocumentCollectionAsync(
                databaseUri,
                collection,
                new RequestOptions {OfferThroughput = OfferThroughput});

            if (Context.Verbose > 1)
            {
                var msg = result.ResponseHeaders.ToJoinedString("\n\t", " : ");
                Console.WriteLine("ResponseHeaders:\n\t{0}", msg);
            }
        }
    }
}
