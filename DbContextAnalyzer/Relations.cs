using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeTools;
using DbContextAnalyzer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;

namespace DbContextAnalyzer;

public sealed class Relations
{
    private readonly DbContext _dbScContext;
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly Stack<string> _preventLoopList = new();
    public readonly Dictionary<string, EntityData> Entities = [];

    // ReSharper disable once ConvertToPrimaryConstructor
    public Relations(DbContext dbScContext, ExcludesRulesParametersDomain excludesRulesParameters)
    {
        _dbScContext = dbScContext;
        _excludesRulesParameters = excludesRulesParameters;
    }

    public void SaveJson(string fileFullName)
    {
        var sampleParamsJsonText = JsonConvert.SerializeObject(Entities, Formatting.Indented);
        //  new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

        //string fileName = $"{name}.json";
        //File.WriteAllText(Path.Combine(_getJsonCreatorParameters.ModelsFolderName, fileName), sampleParamsJsonText);
        File.WriteAllText(fileFullName, sampleParamsJsonText);
        //Console.WriteLine($"{fileName} created");
    }

    public void DbContextAnalysis()
    {
        foreach (var entityType in _dbScContext.Model.GetEntityTypes())
        {
            if (entityType.GetKeys().Count(w => w.IsPrimaryKey()) != 1)
                continue;
            var tableName = GetTableName(entityType);

            if (tableName is not null && _excludesRulesParameters?.ExcludeTables is not null &&
                _excludesRulesParameters.ExcludeTables.Contains(tableName))
                continue;

            //აქ ხდება გამონაკლისი ველების გათვალისწინება
            EntityAnalysis(entityType);
        }
    }

    private void EntityAnalysis(IEntityType entityType)
    {
        //დავადგინოთ ცხრილის სახელი.
        var tableName = GetTableName(entityType);

        Console.WriteLine("EntityAnalysis tableName: {0}", tableName);
        Console.WriteLine("GetReferencingForeignKeys");
        var referencingForeignKeys = entityType.GetReferencingForeignKeys().ToList();
        foreach (var referencingForeignKey in referencingForeignKeys)
        {
            Console.WriteLine("EntityAnalysis referencingForeignKey ConstraintName: {0}",
                referencingForeignKey.GetConstraintName());
            Console.WriteLine("EntityAnalysis referencingForeignKey DefaultName: {0}",
                referencingForeignKey.GetDefaultName());
        }

        Console.WriteLine("GetForeignKeys");
        foreach (var foreignKey in entityType.GetForeignKeys())
        {
            Console.WriteLine("EntityAnalysis foreignKey ConstraintName: {0}", foreignKey.GetConstraintName());
            Console.WriteLine("EntityAnalysis foreignKey DefaultName: {0}", foreignKey.GetDefaultName());
        }

        if (tableName is null || Entities.ContainsKey(tableName))
            //თუ ეს ცხრილი უკვე ყოფილა გაანალიზებულების სიაში, მაშინ აქ აღარაფერი გვესაქმება
            return;

        if (_excludesRulesParameters.ExcludeTables.Contains(tableName))
            return;

        //entityType, 

        //დავადგინოთ ცხრილს აქვს თუ არა გასაღები
        var primaryKeys = entityType.GetKeys().Where(w => w.IsPrimaryKey()).ToList();
        //ასეთი რამე სავარაუდოდ არ მოხდება, მაგრამ მაინც ვამოწმებ. ანუ ერთ ცხრილში არ შეიძლება ერთზე მეტი მთავარი გასაღები იყოს.
        if (primaryKeys.Count > 1)
            throw new Exception($"Multiple primary keys in table {tableName}");
        //თუ აღმოჩნდა, რომ რომელიღაც ცხრილს მთავარი გასაღები არ აქვს, მაშინ ასეთ ბაზას ჯერჯერობით არ განვიხილავ
        if (primaryKeys.Count < 1)
            throw new Exception($"No primary key in table {tableName}");
        //გასაღები უნდა იყოს აუცილებლად 1 ცალი, ჯერჯერობით სხვა ვერსიებს არ განვიხილავთ
        var primaryKey = primaryKeys[0];
        //მთავარი გასაღები ერთი ველისაგან უნდა შედგებოდეს. სხვანაირ ბაზას ჯერ არ განვიხილავ
        if (primaryKey.Properties.Count > 1)
            throw new Exception($"Multiple fields primary key in table {tableName}");
        var entityData = new EntityData
        {
            TableName = tableName, PrimaryKeyFieldName = primaryKey.Properties[0].Name, EntityType = entityType
        };

        var ignoreFields = _excludesRulesParameters.ExcludeFields.Where(w => w.TableName == tableName)
            .Select(s => s.FieldName).ToList();

        var haveOneToOneReference = entityType.GetForeignKeys()
            .Any(s => s.Properties.Any(w => w.Name == entityData.PrimaryKeyFieldName));

        var needsToCreateTempData = entityData.OptimalIndexFieldsData.Count == 0 && referencingForeignKeys.Count > 0;
        var usePrimaryKey = haveOneToOneReference || primaryKey.Properties[0].ValueGenerated == ValueGenerated.Never;

        var fieldsBase = entityType.GetProperties().Where(w =>
            ((needsToCreateTempData || usePrimaryKey) && w.IsPrimaryKey()) ||
            (!w.IsPrimaryKey() && !ignoreFields.Contains(w.Name)));

        entityData.FieldsData = GetFieldsData(entityType.ClrType, fieldsBase, tableName);
        entityData.NeedsToCreateTempData = needsToCreateTempData;
        entityData.UsePrimaryKey = usePrimaryKey;
        entityData.Level = GetMaxLevel(entityData);


        Entities.Add(tableName, entityData);

        //თუ გამონაკლის წესებში მითითებულია ინდექსის ველები ამ ცხრილისათვის, მაშინ გამოვიყენოთ ეს ველები და დამატებით ოპტიმალური ინდექსების ძებნა საჭირო აღარ არის
        var exKeyFieldNames = _excludesRulesParameters.KeyFieldNames.SingleOrDefault(s => s.TableName == tableName);
        if (exKeyFieldNames is not null)
        {
            var exKeyFields = exKeyFieldNames.Keys;
            var optimalIndexFieldsData = exKeyFields
                .Select(keyField => entityData.FieldsData.SingleOrDefault(ss =>
                    ss.Name == _excludesRulesParameters.GetNewFieldName(tableName, keyField))).OfType<FieldData>()
                .ToList();
            entityData.OptimalIndexFieldsData = optimalIndexFieldsData;
        }

        //თუ მთავარი გასაღების დაგენერირება ავტომატურად არ ხდება, მაშინ უნდა მოხდეს მისი გამოყენება და ოპტიმალური ინდექსის ძებნა აღარ არის საჭირო
        if (primaryKey.Properties[0].ValueGenerated != ValueGenerated.Never)
        {
            //თუ მთავარი გასაღები თვითონ ივსება და ამ ცხრილზე სხვა ცხრილები არის დამოკიდებული.
            //მაშინ მოვძებნოთ ოპტიმალური ინდექსი
            var optimalIndex = GetOptimalUniIndex(entityType);
            if (optimalIndex is not null)
                entityData.OptimalIndexFieldsData = optimalIndex.Properties.Select(s =>
                    entityData.FieldsData.Single(ss =>
                        ss.Name == _excludesRulesParameters.GetNewFieldName(tableName, s.Name))).ToList();
        }

        var substituteFields = entityData.FieldsData.Where(w =>
            w.SubstituteField != null && w.SubstituteField.TableName == tableName).ToList();

        if (substituteFields.Count == 1)
            entityData.SelfRecursiveField = substituteFields[0];

        if (entityData.OptimalIndexFieldsData.Count == 0)
            return;

        Console.WriteLine("entityData.OptimalIndex.Properties: {0}",
            string.Join(", ", entityData.OptimalIndexFieldsData.Select(s => s.Name)));
        Console.WriteLine("entityData.FieldsData: {0}", string.Join(", ", entityData.FieldsData.Select(s => s.Name)));
    }

    public static string? GetTableName(IEntityType entityType)
    {
        return entityType.GetTableName();
    }

    private int GetMaxLevel(EntityData entityData)
    {
        return entityData.FieldsData.Count == 0 ? 0 : entityData.FieldsData.Max(GetLevel) + 1;
    }

    private int GetLevel(FieldData fieldData)
    {
        if (fieldData.SubstituteField == null)
            return 0;
        var corEnt = Entities.SingleOrDefault(s => s.Key == fieldData.SubstituteField.TableName);
        return corEnt.Value.Level;
    }

    private List<FieldData> GetFieldsData(Type? tableClrType, IEnumerable<IProperty> fieldsBase, string tableName,
        FieldData? parent = null)
    {
        var replaceDict = _excludesRulesParameters.GetReplaceFieldsDictByTableName(tableName);

        return fieldsBase.Select(Selector).ToList();

        FieldData Selector(IProperty s)
        {
            var preferredName = replaceDict.TryGetValue(s.Name, out var value) ? value : s.Name;

            var fieldData = FieldData.Create(tableClrType, s, preferredName, parent);

            var forKeys = s.GetContainingForeignKeys().ToList();
            switch (forKeys.Count)
            {
                case 0:
                    return fieldData;
                case > 1:
                    throw new Exception($"Multiple Foreign Keys in table {tableName} for field {preferredName}");
            }

            if (_preventLoopList.Contains(tableName))
                throw new Exception($"table {tableName} loops for indexes");

            _preventLoopList.Push(tableName);
            var substEntityType = forKeys[0].PrincipalEntityType;
            EntityAnalysis(substEntityType);
            _preventLoopList.Pop();

            var substTableName = GetTableName(substEntityType) ??
                                 throw new Exception($"substitute table for table {tableName} have no name");

            if (_excludesRulesParameters.ExcludeTables.Contains(substTableName))
                return fieldData;

            if (!Entities.TryGetValue(substTableName, out var entity))
                throw new Exception($"substitute table {substTableName} Not analyzed for table {tableName}");

            var optimalIndexFieldsData = entity.OptimalIndexFieldsData;
            var optimalIndexFieldNames = optimalIndexFieldsData.Select(data => data.Name).ToList();

            fieldData.SubstituteField = new SubstituteFieldData(substTableName,
                optimalIndexFieldsData.Count > 0
                    ? GetFieldsData(tableClrType,
                        entity.EntityType.GetProperties().Where(x => optimalIndexFieldNames.Contains(x.Name)),
                        substTableName, fieldData)
                    : []);
            var nav = forKeys[0].DependentToPrincipal ??
                      throw new Exception($"Foreign Keys nam in table {tableName} is empty");
            fieldData.NavigationFieldName = nav.Name;
            return fieldData;
        }
    }

    private static IIndex? GetOptimalUniIndex(IEntityType entityType)
    {
        //თავიდან დავუშვათ, რომ ოპტიმალური ინდექსი არ მოიძებნა

        //ამოვკრიბოთ ყველა უნიკალური ინდექსი
        //(არ ვიცი რამდენად საჭიროა იმის შემოწმება, რომ ველები IsNullable არ უნდა იყოს).
        //კიდევ იმასაც ვამოწმებ, რომ უნიკალური ინდექსის ველები ავტოგენერირებადი არ იყოს. (ესეც არ ვიცი რამდენად საჭიროა)
        var uniKeys = entityType.GetIndexes().Where(w =>
            w.IsUnique && !w.Properties.Any(p => p.IsNullable || p.ValueGenerated != ValueGenerated.Never)).ToList();

        //ჯერ გავარკვიოთ საერთოდ უნიკალური ინდექსები გვაქვს თუ არა
        if (uniKeys.Count == 0)
            //უნიკალური ინდექსები ამ ცხრილში არ ყოფილა.
            //ძებნის გაგრძელებას აზრი არ აქვს,
            return null;

        //უნიკალური ინდექსების სია გავფილტროთ ისე, რომ დარჩეს მხოლოდ ის ინდექსები, რომელთა ველები სხვა ცხრილებზე არ არის დამოკიდებული რელაციურად
        var independentUniKeys =
            uniKeys.FindAll(f => f.Properties.All(property => !property.GetContainingForeignKeys().Any()));

        switch (independentUniKeys.Count)
        {
            case 1:
                //მოიძებნა ზუსტად ერთი დამოუკიდებელი ინდექსი.
                return independentUniKeys[0];
            case > 1:
            {
                //დამოუკიდებელი ინდექსი ერთზე მეტია
                //გავფილტროთ ისე, რომ დაგვრჩეს ისეთი ინდექსები, რომელთაც მხოლოდ ერთი ველი აქვთ და ისიც სტრიქონი
                var oneFieldStringUniKeys = independentUniKeys.FindAll(w =>
                    w.IsUnique && w.Properties.Count == 1 && w.Properties[0].ClrType.Name == "String");
                switch (oneFieldStringUniKeys.Count)
                {
                    case 1:
                        return oneFieldStringUniKeys[0];
                    case > 1:
                    {
                        //თუ ასეთი ინდექსების რაოდენობა 1-ზე მეტია
                        //პირველად უპირატესობას ვაძლევთ იმ ინდექსს, რომლის ველის სახელიც მთავრდება "key" სტრიქონით
                        //თუ ასეთი არ მოიძებნა, მაშინ უპირატესობა ეძლევა იმ ინდექსს, რომლის ველის სახელიც მთავრდება "name" სტრიქონით
                        var res = oneFieldStringUniKeys.Find(f => f.Properties[0].Name.ToLower().EndsWith("key")) ??
                                  oneFieldStringUniKeys.Find(f => f.Properties[0].Name.ToLower().EndsWith("name"));
                        if (res != null)
                            return res;
                        break;
                    }
                }

                //აქ თუ მოვედით, ეს ნიშნავს, რომ ერთველიანი სტრიქონიანი ინდექსები არ გვაქვს.
                //თუმცა რაც გვაქვს დამოუკიდებელი ინდექსებია და მათგან უნდა ავარჩიოთ იმის მიხედვით, ვის აქვს ყველაზე ნაკლები ველი
                return independentUniKeys.OrderBy(o => o.Properties.Count).ToList()[0];
            }
            default:
                //აქ თუ მოვედით, დამოუკიდებელი ინდექსები არ გვაქვს.
                //თუმცა რაღაც ინდექსები გვაქვს, რომლიდანაც უნდა ავარჩიოთ ოპტიმალური

                //ოპტიმალურად უნდა ჩავთვალოთ პირველ რიგში ის ინდექსი, რომელსაც ყველაზე ნაკლები რაოდენობის დამოკიდებული ველები აქვს,
                //ხოლო შემდეგ იმის მიხედვით დანარჩენი ველების რაოდენობა, რომ ყველაზე ნაკლები ჰქონდეს.

                return uniKeys.OrderBy(o => o.Properties.Count(c => c.GetContainingForeignKeys().Any()))
                    .ThenBy(o => o.Properties.Count(c => !c.GetContainingForeignKeys().Any())).ToList()[0];

            //ოპტიმალური ინდექსის მოსაძებნად რეკურსია აღარ გამოვიყენე,
            //რადგან მაშინ საჭირო გახდებოდა სხვადასხვა ცხრილებიდან მოსული ინდექსების რანჟირება
            //რაც არაინტუიტიურია და თანაც არ მგონია კოდმა აქამდე მოაღწიოს, რადგან, მანამდე ბევრჯერ იყო დაანგარიშების შანსი

            //var optCandidates = uniKeys.Select(s=> new { s, count = s.Properties.Count(c => c.GetContainingForeignKeys().Any()) }).OrderBy(o=>o.count).ToList();
            ////დავადგინოთ რამდენია დამოკიდებული ველების მინიმალური რაოდენობა 
            //int minCount = optCandidates[0].count;
            ////ამოვკრიბოთ ყველა მინიმალური რაოდენობის დამოკიდებულ ველებიანი
            //List<IIndex> optCandidateUniKeys = optCandidates.Where(w => w.count == minCount).Select(s => s.s).ToList();
            ////თუ ასეთი მხოლოდ ერთი მოიძებნა, ის არის ოპტიმალური
            //if (optCandidateUniKeys.Count == 1)
            //{
            //  return optCandidateUniKeys[0];
            //}

            //foreach (var optCandidate in optCandidates.Where(w=>w.count == minCount))
            //{

            //}      
        }
    }
}