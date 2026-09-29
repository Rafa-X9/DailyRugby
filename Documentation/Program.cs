using System.Text;
using System.Text.Json;

while (true)
{
    Console.Clear();
    Console.WriteLine("  1. Add parameter");
    Console.WriteLine("  2. See parameters");
    Console.WriteLine("  3. Add command section");
    Console.WriteLine("  4. See all sections");
    Console.WriteLine("  5. Add command to section");
    Console.WriteLine("  6. See all commands in a section");
    Console.WriteLine("  7. Generate documentation");

    int choice = Input.GetInt("> ");
    Console.WriteLine();

    if (choice == 1)
    {
        List<Parameter> parameters = JsonParser.GetParameters();

        parameters.Add(new()
        {
            Name = Input.GetString("Name: "),
            Type = Input.GetString("Type: "),
            Default = Input.GetString("Default (empty for none): "),
            Autocomplete = Input.GetString("Autocomplete (empty for none): "),
            Description = Input.GetString("Description: ")
        });

        JsonParser.SaveParameters(parameters);
    }
    else if (choice == 2)
    {
        List<Parameter> parameters = JsonParser.GetParameters();

        foreach (var parameter in parameters)
        {
            Console.WriteLine("---");
            Console.WriteLine($"Parameter: {parameter.Name}");
            Console.WriteLine($"Type: {parameter.Type}");
            Console.WriteLine($"Default: {parameter.Default}");
            Console.WriteLine($"Autocomplete: {parameter.Autocomplete}");
            Console.WriteLine($"Description: {parameter.Description}");
        }

        Console.ReadLine();
    }
    else if (choice == 3)
    {
        var sections = JsonParser.GetSections();

        var section = new Section()
        {
          Name = Input.GetString("Name: ")
        };

        if (sections.Any(temp => temp.Name.Equals(section.Name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Section already exists");
            continue;
        }

        sections.Add(section);
        JsonParser.SaveSections(sections);
    }
    else if (choice == 4)
    {
        var sections = JsonParser.GetSections();

        foreach (var section in sections)
        {
            Console.WriteLine("---");
            Console.WriteLine($"Section '{section.Name}'");
            foreach (var command in section.Commands)
            {
                Console.WriteLine($"   {command.SlashCommand}");
            }
        }

        Console.ReadLine();
    }
    else if (choice == 5)
    {
        Console.WriteLine();

        var sections = JsonParser.GetSections();

        for (int i = 0; i < sections.Count; i++)
        {
            Console.WriteLine($"{i}: {sections[i].Name}, {sections[i].Commands.Count} commands");
        }

        var chosenSection = sections[Input.GetInt("Choose a section: ")];

        var command = new Command()
        {
          SlashCommand = Input.GetString("Command (/name-of-command): "),
          IsAdminOnly = Input.GetString("Is admin only (y/n): ")[0] == 'y',
          Explanation = Input.GetString("Explanation: ")
        };

        var parameters = JsonParser.GetParameters();

        Console.WriteLine();
        for (int i = 0; i < parameters.Count; i++)
        {
            Console.WriteLine($"{i}: {parameters[i].Name} ({parameters[i].Description})");
        }

        int index = 0;
        while (index != -1)
        {
            index = Input.GetInt("Parameter to add (-1 to end): ");
            if (index != -1) command.Parameters.Add(parameters[index].Id);
        }

        chosenSection.Commands.Add(command);
        JsonParser.SaveSections(sections);
    }
    else if (choice == 6)
    {
        var sections = JsonParser.GetSections();

        for (int i = 0; i < sections.Count; i++)
        {
            Console.WriteLine($"{i}: {sections[i].Name}");
        }

        var chosenSection = sections[Input.GetInt("Choose a section: ")];

        Console.WriteLine("---");
        Console.WriteLine($"Section {chosenSection.Name}");

        var parameters = JsonParser.GetParameters();

        foreach (var command in chosenSection.Commands)
        {
            var commandParameters = parameters
                .Where(temp => command.Parameters.Contains(temp.Id))
                .ToList();

            Console.WriteLine($"  {command.SlashCommand}");
            Console.WriteLine($"     IsAdminOnly = {command.IsAdminOnly}");
            Console.WriteLine($"     Explanation = {command.Explanation}");
            Console.WriteLine($"     Parameters");
            foreach (var parameter in commandParameters)
            {
                Console.WriteLine($"        {parameter.Name}: {parameter.Type}");
            }
        }

        Console.ReadLine();
    }
    else if (choice == 7)
    {
        StringBuilder document = new();
        document.AppendLine("# Commands");
        document.AppendLine();
        document.AppendLine("This file contains a list of all slash commands this bot has.");
        document.AppendLine();
        document.AppendLine("All commands are registered here alongside their description and a " +
            "list of their parameters. All parameters that don't have a default value are required.");

        var sections = JsonParser.GetSections();

        document.AppendLine();
        document.AppendLine("## Table of contents");

        foreach (var section in sections)
        {
            var words = $"{section.Name.ToLower()} commands"
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string id = string.Join('-', words);
            document.AppendLine();
            document.AppendLine($"* [{section.Name} commands](#{id})");
        }

        List<Parameter> parameters = JsonParser.GetParameters();

        foreach (var section in sections)
        {
            document.AppendLine();
            document.AppendLine($"## {section.Name} commands");

            foreach (var command in section.Commands)
            {
                document.AppendLine();
                document.AppendLine($"### `{command.SlashCommand}` " + 
                    $"{(command.IsAdminOnly ? "(*admin-only*)" : string.Empty)}");
                
                document.AppendLine();
                document.AppendLine(command.Explanation);

                if (command.Parameters.Count == 0) continue;

                document.AppendLine();
                document.AppendLine("Parameters:");
                
                document.AppendLine();
                document.AppendLine("| Parameter | Type | Default | Autocomplete | Description |");
                document.AppendLine("| --------- | ---- | ------- | ------------ | ----------- |");

                foreach (var commandId in command.Parameters)
                {
                    var parameter = parameters.First(temp => temp.Id == commandId);

                    string @default = !string.IsNullOrWhiteSpace(parameter.Default)
                        ? $"`{parameter.Default}`"
                        : "—";
                    
                    string autocomplete = !string.IsNullOrWhiteSpace(parameter.Autocomplete)
                        ? $"{parameter.Autocomplete}"
                        : "—";

                    document.AppendLine($"| `{parameter.Name}` " + 
                        $"| `{parameter.Type}` " + 
                        $"| {@default} " +
                        $"| {autocomplete} " +
                        $"| {parameter.Description} |");
                }
            }

            document.AppendLine();
            document.AppendLine("[Back to table of contents](#table-of-contents)");
        }

        const string path = @"C:\Users\Lenovo\Desktop\Rafael\Projetos\projetosaspnet\DailyRugby\COMMANDS.md";
        File.Delete(path);
        File.WriteAllText(path, document.ToString());
    }
    else
    {
        Console.WriteLine("Invalid choice");
    }
}

class Parameter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Default { get; set; }
    public required string Autocomplete { get; set; }
    public required string Description { get; set; }
}

class Section
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public List<Command> Commands { get; set; } = [];
}

class Command
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string SlashCommand { get; set; }
    public required string Explanation { get; set; }
    public required bool IsAdminOnly { get; set; }
    public List<Guid> Parameters { get; set; } = [];
}

static class Input
{
    public static string GetString(string message)
    {
        Console.Write(message);
        string? input = Console.ReadLine()
            ?? throw new FormatException("Input was null");
        return input;
    }

    public static int GetInt(string message)
    {
        Console.Write(message);
        return int.Parse(Console.ReadLine() ?? "error");
    }
}

static class JsonParser
{
    private const string ParametersPath = @"C:\Users\Lenovo\Desktop\Rafael\Projetos\projetosaspnet\DailyRugby\Documentation\parameters.json";
    private const string CommandsPath = @"C:\Users\Lenovo\Desktop\Rafael\Projetos\projetosaspnet\DailyRugby\Documentation\commands.json";

    public static List<Parameter> GetParameters()
    {
        if (!File.Exists(ParametersPath)) return [];

        string content = File.ReadAllText(ParametersPath);
        List<Parameter> parameters = JsonSerializer.Deserialize<List<Parameter>>(content)
            ?? throw new InvalidCastException("Couldn't parse parameters from JSON file");

        return parameters;
    }

    public static void SaveParameters(List<Parameter> parameters)
    {
        string json = JsonSerializer.Serialize(parameters);
        File.Delete(ParametersPath);
        File.WriteAllText(ParametersPath, json);
    }

    public static List<Section> GetSections()
    {
        if (!File.Exists(CommandsPath)) return [];

        string json = File.ReadAllText(CommandsPath);
        List<Section> sections = JsonSerializer.Deserialize<List<Section>>(json)
            ?? throw new InvalidCastException("Couldn't parse parameters from JSON file");

        return sections;
    }

    public static void SaveSections(List<Section> sections)
    {
        string json = JsonSerializer.Serialize(sections);
        File.Delete(CommandsPath);
        File.WriteAllText(CommandsPath, json);
    }
}