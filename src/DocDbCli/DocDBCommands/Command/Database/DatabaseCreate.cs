using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "database")]
    [ExportMetadata("Verb", "create")]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DatabaseCreate : CommandDocBase
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
        protected override async Task RunAsync(DocumentClient client)
        {
            await client.OpenAsync();
            Context.WriteToFile();
        }
    }
}
