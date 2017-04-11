﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;
using Serilog;

namespace DocDB.Command
{

    public class CommandBase
    {
        protected Context _context; 
        public Context Context {
            get { return _context; }
        }
        protected List<string> extra;
        protected static Task DoneTask { get; } = Task.FromResult(true);

        private OptionSet optionSet;

        public bool Parse(string[] args)
        {
            bool help = false;

            string profile=null;
            var context = new Context();
            optionSet = new OptionSet()
            {
                {"e|EndPoint=", v => context.EndPoint = v},
                {"k|AuthorizationKey=", v => context.AuthorizationKey = v},
                {"d|DatabaseName=", v => context.DatabaseName = v},
                {"c|DataCollectionName=", v => context.DataCollectionName = v},
                {"v|verbose", v => ++context.Verbose},
                {"p|profile", v => profile=v},
                {"h|?|help", v => context.Help = v != null},
            };
            // call back here
            BeforeParse(optionSet);
            extra = optionSet.Parse(args);
            _context = Context.ReadFromFile(profile);
            _context.Apply(context);

            return !help;
        }
        protected virtual void BeforeParse(OptionSet optionset) {
            Log.Debug("CommandBase.BeforeParse");
        }

        public void PrintHelp()
        {
            optionSet.WriteOptionDescriptions(Console.Out);
        }

 
        protected Database GetDatabaseIfExists(DocumentClient client, string databaseName)
        {
            return client.CreateDatabaseQuery().Where(d => d.Id == databaseName).AsEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Get the collection if it exists, null if it doesn't
        /// </summary>
        /// <returns>The requested collection</returns>
        protected DocumentCollection GetCollectionIfExists(DocumentClient client, string databaseName, string collectionName)
        {
            if (GetDatabaseIfExists(client, databaseName) == null)
            {
                return null;
            }

            return client.CreateDocumentCollectionQuery(UriFactory.CreateDatabaseUri(databaseName)).Where(c => c.Id == collectionName).AsEnumerable().FirstOrDefault();
        }

    }

}