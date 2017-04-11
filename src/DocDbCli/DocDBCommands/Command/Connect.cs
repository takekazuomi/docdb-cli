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
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "connect")]
    [ExportMetadata("Verb", "nop")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Connect : CommandBase, ICommand
    {
        protected override void CheckRequiredOption(Context contextBefore, Context contextAfter)
        {
            var msgs = new List<string>();

            if (string.IsNullOrEmpty(contextBefore.EndPoint))
                msgs.Add("-e=EndPoint");

            if (string.IsNullOrEmpty(contextBefore.AuthorizationKey))
                msgs.Add("-k=AccessKey");

            if (msgs.Count > 0)
                throw new InvalidOperationException("Missing required option " + string.Join(", ", msgs));
        }

        public async Task RunAsync()
        {
            DocumentClient client;
            using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
            {
                await client.OpenAsync();
                Context.WriteToFile();
            }
        }
    }
}

