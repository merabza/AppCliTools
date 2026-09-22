namespace AppCliTools.CliParameters.Tests;

//Console.SetOut პროცესის დონეზე მოქმედებს: კონსოლის ტექსტის ამღები ტესტები პარალელურად არ უნდა გაეშვას
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ConsoleCaptureCollection
{
    public const string Name = "ConsoleCapture";
}
