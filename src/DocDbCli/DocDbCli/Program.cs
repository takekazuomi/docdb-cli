using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using DocDB;
using Serilog;

namespace DocDbCli
{
    class Program
    {
        private readonly CompositionContainer _container;

        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<ICommand, ICommandName>> _commands;
#pragma warning restore 649

        private Program()
        {
            Log.Logger = new LoggerConfiguration()
              .MinimumLevel.Debug()
              //.WriteTo.LiterateConsole()
              .WriteTo.RollingFile("logs\\{Date}.txt")
              .CreateLogger();

            var catalog = new AggregateCatalog();
            var directoryCatalog = new DirectoryCatalog(".", "DocDB*.dll");

            catalog.Catalogs.Add(directoryCatalog);

            _container = new CompositionContainer(catalog);

            try
            {
                _container.ComposeParts(this);
            }
            catch (CompositionException compositionException)
            {
                Log.Error(compositionException, "MEF Composition Error");
            }
        }

        async Task RunAsync(string[] args)
        {

            var queue = new Queue<string>(args);
            var name = -queue.Count == 0 ? "help" : queue.Dequeue();
            var cmd = _commands.FirstOrDefault(lazy => lazy.Metadata.Name == name);
            if (cmd != null)
            {
                if (cmd.Value.Parse(queue.ToArray()))
                    await cmd.Value.RunAsync();
                else
                    cmd.Value.PrintHelp();
            }
        }

        static void Main(string[] args)
        {
            try
            {
                (new Program()).RunAsync(args).Wait();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine("Error: {0}", e.Message);
                Log.Error(e, "main");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
