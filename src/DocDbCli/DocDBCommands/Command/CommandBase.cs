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
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Mono.Options;

namespace DocDB.Command
{

    public abstract class CommandBase
    {
        protected Context _context;
        protected Context Context {
            get { return _context; }
        }

        protected List<string> Extra { get; set; }

        protected static Task DoneTask { get; } = Task.FromResult(true);

        private OptionSet _optionSet;

        public bool Parse(string[] args)
        {
            bool help = false;

            string profile=null;
            var context = new Context();
            _optionSet = new OptionSet()
            {
                {"e|EndPoint=", v => context.EndPoint = v},
                {"k|AuthorizationKey=", v => context.AuthorizationKey = v},
                {"d|DatabaseName=", v => context.DatabaseName = v},
                {"c|DataCollectionName=", v => context.DataCollectionName = v},
                {"v|verbose", v => ++context.Verbose},
                {"F|profile", v => profile=v},
                {"h|?|help", v => help = v != null},
            };

            // call back here
            BeforeParse(_optionSet);
            Extra = _optionSet.Parse(args);
            _context = Context.ReadFromFile(profile);
            _context.Apply(context);

            CheckRequiredOption(context, _context);

            return !help;
        }
        protected virtual void BeforeParse(OptionSet optionset)
        {
        }
        protected virtual void CheckRequiredOption(Context contextBefore, Context contextAfter)
        {
        }

 
        public void PrintHelp()
        {
            _optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}
