namespace AppCliTools.CliParametersApiClientsEdit;

//Api კლიენტის გამოყენების ადგილი. Name კლიენტის მენიუში ჩანს, MenuLink კი იმ მენიუს ბმულია (მთავარი მენიუდან მენიუს
//ჩანაწერების სახელები "/"-ით), რომელშიც პროგრამა ამ ჩანაწერის არჩევისას გადადის
public sealed record ApiClientUsage(string Name, string MenuLink);
