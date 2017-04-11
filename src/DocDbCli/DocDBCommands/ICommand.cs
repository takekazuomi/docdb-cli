using System.Threading.Tasks;

namespace DocDB
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
