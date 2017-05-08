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
 * 
 * 
 * https://github.com/Azure/azure-documentdb-dotnet/tree/master/samples/partition-stats
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Serilog;
using Microsoft.Azure.Documents;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "collection")]
    [ExportMetadata("Verb", "partitionstats")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class CollectionPartitionStats : CommandDocBase
    {
        protected override async Task RunAsync(DocumentClient client)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, Context.DataCollectionName);

            var collection = await client.ReadDocumentCollectionAsync(collectionLink, new RequestOptions { PopulateQuotaInfo = true });

            if (Context.Verbose > 0)
                Console.WriteLine(string.Join("\n", collection.DumpValue()));

            var partitionKeyRanges = await GetPartitionKeyRanges(client, collectionLink);

            PrintSummaryStats(collection, partitionKeyRanges);

            if (partitionKeyRanges.Count > 1)
            {
//                await PrintPerPartitionStats(collection, partitionKeyRanges);
            }
        }

        private async Task<List<PartitionKeyRange>> GetPartitionKeyRanges(DocumentClient client, Uri collectionLink)
        {
            string pkRangesResponseContinuation = null;
            var partitionKeyRanges = new List<PartitionKeyRange>();

            do
            {
                var response = await client.ReadPartitionKeyRangeFeedAsync(collectionLink,
                    new FeedOptions { RequestContinuation = pkRangesResponseContinuation });

                partitionKeyRanges.AddRange(response);
                pkRangesResponseContinuation = response.ResponseContinuation;
            }
            while (pkRangesResponseContinuation != null);

            return partitionKeyRanges;
        }

        private static void PrintSummaryStats(ResourceResponse<DocumentCollection> collection, List<PartitionKeyRange> partitionKeyRanges)
        {
            
            Console.WriteLine("Summary: {0}", collection.CurrentResourceQuotaUsage);
            Console.WriteLine("\tpartitions: {0}", partitionKeyRanges.Count);

            string[] keyValuePairs = collection.CurrentResourceQuotaUsage.Split(';');

            foreach (string kvp in keyValuePairs)
            {
                string metricName = kvp.Split('=')[0];
                string metricValue = kvp.Split('=')[1];

                switch (metricName)
                {
                    case "collectionSize":
                        break;
                    case "documentsSize":
                        Console.WriteLine("\t{0}: {1} GB", metricName, Math.Round(int.Parse(metricValue) / (1024 * 1024.0), 3));
                        break;
                    case "documentsCount":
                        Console.WriteLine("\t{0}: {1:n0}", metricName, int.Parse(metricValue));
                        break;
                    case "storedProcedures":
                    case "triggers":
                    case "functions":
                        break;
                    default:
                        Console.WriteLine("\t{0}: {1}", metricName, metricValue);
                        break;
                }
            }
            Console.WriteLine();
        }
    }
}


