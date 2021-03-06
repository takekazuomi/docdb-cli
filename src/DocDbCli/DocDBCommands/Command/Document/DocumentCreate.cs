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
using System.Threading.Tasks;
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

    public class DocumentCreate : CommandDocBase
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


        protected override async Task RunAsync(DocumentClient client)
        {
            var collectionUri = UriFactory.CreateDocumentCollectionUri(Context.DatabaseName, Context.DataCollectionName);
            // TODO Why cannot use json string direct?
            var newDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(JsonText);
            var result = await client.CreateDocumentAsync(collectionUri, newDictionary, new RequestOptions());
            if (Context.Verbose > 0)
            {
                Console.WriteLine("RequestCharge: {0}", result.RequestCharge);
                Console.WriteLine(result.Resource);
            }
            if (Context.Verbose > 1)
            {
                var msg = result.ResponseHeaders.ToJoinedString("\n\t", " : ");
                Console.WriteLine("ResponseHeaders:\n\t{0}", msg);
            }
        }
    }
}
