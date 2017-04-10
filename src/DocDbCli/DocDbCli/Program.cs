using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace DocDbCli
{
    class Options
    {
        public string ConnectionString { get; set; }
        public int Verbose { get; set; }
        public  bool Help { get; set; }
        public string DatabaseName { get; set; }

        public string DataCollectionName { get; set; }
    }

    class Program
    {
        readonly Options _options = new Options();

        async Task RunAsync(string[] args)
        {
            bool help = false;
            var p = new OptionSet()
            {
                {"c|ConnectionString=", v => _options.ConnectionString = v},
                {"d|DatabaseName=", v => _options.DatabaseName = v},
                {"n|DataCollectionName=", v => _options.DataCollectionName = v},
                {"v|verbose", v => ++_options.Verbose},
                {"h|?|help", v => _options.Help = v != null},
            };
            var extra = p.Parse(args);
        }


        static void Main(string[] args)
        {
            (new Program()).RunAsync(args).Wait();
        }
    }
}
