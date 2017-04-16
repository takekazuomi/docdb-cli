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
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "document")]
    [ExportMetadata("Verb", "query")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class DocumentQuery : CommandDocBase
    {
        public string QueryText { get; set; }

        protected override void BeforeParse(OptionSet optionset)
        {
            Log.Debug("Query.BeforeParse");
            optionset.Add("q|query=", text => QueryText = text);
            base.BeforeParse(optionset);
        }
        protected override void CheckRequiredOption(Context contextBefore, Context contextAfter)
        {
            var msgs = new List<string>();

            if (string.IsNullOrEmpty(QueryText))
                msgs.Add("-q=query");

            if (msgs.Count > 0)
                throw new InvalidOperationException("Missing required option " + string.Join(", ", msgs));
        }
        protected override async Task RunAsync(DocumentClient client)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, Context.DataCollectionName);
            var query = client.CreateDocumentQuery(collectionUri, QueryText, Context.FeedOptions).AsDocumentQuery();
            while (query.HasMoreResults)
            {
                var result = await query.ExecuteNextAsync();
                if (Context.Verbose > 0)
                    Console.WriteLine("RequestCharge: {0}", result.RequestCharge);
                if (Context.Verbose > 1)
                {
                    var msg = result.ResponseHeaders.ToJoinedString("\n\t", " : ");
                    Console.WriteLine("ResponseHeaders:\n\t{0}", msg);
                }
                Console.WriteLine(JsonConvert.SerializeObject(result.AsEnumerable()));
            }
        }
    }
}


