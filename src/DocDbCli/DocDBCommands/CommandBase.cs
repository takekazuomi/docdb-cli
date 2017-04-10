using Mono.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDBCommands
{
    public class Options
    {
        public string EndPoint { get; set; }
        public string AuthorizationKey { get; set; }
        public int Verbose { get; set; }
        public bool Help { get; set; }
        public string DatabaseName { get; set; }

        public string DataCollectionName { get; set; }
    }
    public class CommandBase
    {
        protected readonly Options _options = new Options();
        protected List<string> extra;
        protected static Task DoneTask { get; } = Task.FromResult(true);

        private OptionSet optionSet;

        public bool Parse(string[] args)
        {
            bool help = false;
            optionSet = new OptionSet()
            {
                {"e|EndPoint=", v => _options.EndPoint = v},
                {"k|AuthorizationKey=", v => _options.AuthorizationKey = v},
                {"d|DatabaseName=", v => _options.DatabaseName = v},
                {"c|DataCollectionName=", v => _options.DataCollectionName = v},
                {"v|verbose", v => ++_options.Verbose},
                {"h|?|help", v => _options.Help = v != null},
            };
            // call back here
            BeforeParse(optionSet);
            extra = optionSet.Parse(args);
            return !help;
        }
        protected virtual void BeforeParse(OptionSet optionset) {
            Log.Debug("CommandBase.BeforeParse");
        }

        public void PrintHelp()
        {
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
