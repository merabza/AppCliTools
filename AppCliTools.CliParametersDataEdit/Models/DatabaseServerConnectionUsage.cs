namespace AppCliTools.CliParametersDataEdit.Models;

//მონაცემთა ბაზასთან კავშირის გამოყენების ადგილი. Name კავშირის მენიუში ჩანს, MenuLink კი იმ მენიუს ბმულია
//(მთავარი მენიუდან მენიუს ჩანაწერების სახელები "/"-ით), რომელშიც პროგრამა ამ ჩანაწერის არჩევისას გადადის
public sealed record DatabaseServerConnectionUsage(string Name, string MenuLink);
