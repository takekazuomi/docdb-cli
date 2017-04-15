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
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using DocDB;
using Serilog;
using Serilog.Events;

namespace DocDbCli
{
    class Program
    {
        private readonly CompositionContainer _container;

        [ImportMany]
#pragma warning disable 649
        private IEnumerable<Lazy<ICommand, ICommandMetadata>> _commands;
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
                if (Log.IsEnabled(LogEventLevel.Debug))
                {
                    var msg = _commands.ToList().Select(lazy => string.Format("Name:{0}, Verb:{1}", lazy.Metadata.Name, lazy.Metadata.Verb)).ToArray();
                    Log.Debug("dump commands {0}", string.Join(", ", msg));
                }
            }
            catch (CompositionException compositionException)
            {
                Log.Error(compositionException, "MEF Composition Error");
            }
        }

        async Task RunAsync(string[] args)
        {
            // TODO fix here
            var queue = new Queue<string>(args);
            var name = -queue.Count == 0 ? "help" : (queue.Peek().StartsWith("-") ? "help" : queue.Dequeue());
            var verb = -queue.Count == 0 ? "nop" : (queue.Peek().StartsWith("-") ? "nop" : queue.Dequeue());

            var cmd = _commands.FirstOrDefault(lazy => lazy.Metadata.Name == name && lazy.Metadata.Verb == verb);
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
            catch (AggregateException ae)
            {
                foreach (var e in ae.Flatten().InnerExceptions)
                    Console.Error.WriteLine("Error: {0}", e.Message);
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
