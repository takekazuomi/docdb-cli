using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace DocDB.Command
{
    [Export(typeof(ICommand))]
    [ExportMetadata("Name", "help")]
    [PartCreationPolicy(CreationPolicy.NonShared)]

    public class Help : CommandBase, ICommand
    {
        public Task RunAsync()
        {
            Console.WriteLine("help");
            return DoneTask;
        }
    }
}
