using System;

namespace ConsoleCoffeeMaker
{
    public class ConsoleCommand
    {
        public ConsoleCommand(string commandText, string commandDescription)
        {
            Command = commandText;
            Description = commandDescription;
        }

        public string Command { get; private set; }
        public string Description { get; private set; }

        public bool IsCommandFor(string command)
        {
            return string.Equals(Command, command, StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return $"{Command} - {Description}";
        }
    }
}