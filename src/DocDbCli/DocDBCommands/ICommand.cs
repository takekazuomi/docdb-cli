using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocDBCommands
{
    public interface ICommand
    {
        Task RunAsync();
        bool Parse(string[] args);

        void PrintHelp();
    }
    public interface ICommandName
    {
        string Name { get; }
    }
}
