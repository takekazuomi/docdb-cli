using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Mono.Options;
using Newtonsoft.Json;
using Serilog;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "connect")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Connect : CommandBase, ICommand
    {
        public string QueryText { get; set; }

        public async Task RunAsync()
        {
            try
            {
                DocumentClient client;
                using (client = new DocumentClient(new Uri(Context.EndPoint), Context.AuthorizationKey, Context.ConnectionPolicy))
                {
                    await client.OpenAsync();
                    Context.WriteToFile();
                }
            }
            catch (DocumentClientException e)
            {
                Console.Error.WriteLine("connect error: {0}", e.Message);
                Log.Error(e, "DocumentClientException");
            }
        }
    }
}
