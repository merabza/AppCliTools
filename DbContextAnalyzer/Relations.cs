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
    private readonly DbContext _dbContext;
    private readonly ExcludesRulesParametersDomain _excludesRulesParameters;
    private readonly Stack<string> _preventLoopList = new();
    public readonly Dictionary<string, EntityData> Entities = [];

    // ReSharper disable once ConvertToPrimaryConstructor
    public Relations(DbContext dbContext, ExcludesRulesParametersDomain excludesRulesParameters)
    {
        _dbContext = dbContext;
        _excludesRulesParameters = excludesRulesParameters;
    }

    public void SaveJson(string fileFullName)
    {
        var sampleParamsJsonText = JsonConvert.SerializeObject(Entities, Formatting.Indented);
        File.WriteAllText(fileFullName, sampleParamsJsonText);
    }

    public void DbContextAnalysis()
    {
        foreach (var entityType in _dbContext.Model.GetEntityTypes())
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

    private EntityData? EntityAnalysis(IEntityType entityType)
    {
        //დავადგინოთ ცხრილის სახელი.
        var tableName = GetTableName(entityType);
        if (tableName is null || Entities.ContainsKey(tableName))
            //თუ ეს ცხრილი უკვე ყოფილა გაანალიზებულების სიაში, მაშინ აქ აღარაფერი გვესაქმება
            return tableName is null ? null : Entities[tableName];

        if (_excludesRulesParameters.ExcludeTables.Contains(tableName))
            return null;
        //დავადგინოთ ცხრილს აქვს თუ არა გასაღები
        var primaryKeys = entityType.GetKeys().Where(w => w.IsPrimaryKey()).ToList();
        switch (primaryKeys.Count)
        {
            //ასეთი რამე სავარაუდოდ არ მოხდება, მაგრამ მაინც ვამოწმებ. ანუ ერთ ცხრილში არ შეიძლება ერთზე მეტი მთავარი გასაღები იყოს.
            case > 1:
                throw new Exception($"Multiple primary keys in table {tableName}");
            //თუ აღმოჩნდა, რომ რომელიღაც ცხრილს მთავარი გასაღები არ აქვს, მაშინ ასეთ ბაზას ჯერჯერობით არ განვიხილავ
            case < 1:
                throw new Exception($"No primary key in table {tableName}");
        }

        //გასაღები უნდა იყოს აუცილებლად 1 ცალი, ჯერჯერობით სხვა ვერსიებს არ განვიხილავთ
        var primaryKey = primaryKeys[0];
        //მთავარი გასაღები ერთი ველისაგან უნდა შედგებოდეს. სხვანაირ ბაზას ჯერ არ განვიხილავ
        if (primaryKey.Properties.Count > 1)
            throw new Exception($"Multiple fields primary key in table {tableName}");

        //entityType, 
        var entityData = new EntityData
        {
            TableName = tableName, PrimaryKeyFieldName = primaryKey.Properties[0].Name, EntityType = entityType
        };

        Entities.Add(tableName, entityData);

        var needsToCreateTempData = false;

        var hasOneToOneReference = entityType.GetForeignKeys()
            .Any(s => s.Properties.Any(w => w.Name == entityData.PrimaryKeyFieldName));

        entityData.HasOneToOneReference = hasOneToOneReference;

        var hasReferencingForeignKeys = entityType.GetReferencingForeignKeys().Any();

        //თუ მთავარი გასაღების დაგენერირება ავტომატურად არ ხდება, მაშინ უნდა მოხდეს მისი გამოყენება და ოპტიმალური ინდექსის ძებნა აღარ არის საჭირო
        if (primaryKey.Properties[0].ValueGenerated == ValueGenerated.OnAdd && hasReferencingForeignKeys)
            //თუ მთავარი გასაღები თვითონ ივსება და ამ ცხრილზე სხვა ცხრილები არის დამოკიდებული.
            //მაშინ მოვძებნოთ ოპტიმალური ინდექსი
        {
            entityData.HasAutoNumber = true;
            //თუ გამონაკლის წესებში მითითებულია ინდექსის ველები ამ ცხრილისათვის, მაშინ გამოვიყენოთ ეს ველები და დამატებით ოპტიმალური ინდექსების ძებნა საჭირო აღარ არის
            var exKeyFieldNames = _excludesRulesParameters.KeyFieldNames.SingleOrDefault(s =>
                string.Equals(s.TableName, tableName, StringComparison.CurrentCultureIgnoreCase));
            if (exKeyFieldNames is not null)
            {
                var exKeyFields = exKeyFieldNames.Keys;
                entityData.OptimalIndexProperties = exKeyFields.Select(keyField =>
                    entityType.GetProperties().SingleOrDefault(ss => string.Equals(ss.Name,
                        _excludesRulesParameters.GetNewFieldName(tableName, keyField),
                        StringComparison.CurrentCultureIgnoreCase))).OfType<IProperty>().ToList();
            }
            else
            {
                entityData.OptimalIndexProperties = GetOptimalUniIndex(entityType)?.Properties.ToList() ?? [];
            }

            needsToCreateTempData = entityData.OptimalIndexProperties.Count == 0 && hasReferencingForeignKeys;
        }

        if (!needsToCreateTempData && hasOneToOneReference)
            needsToCreateTempData = true;

        var usePrimaryKey = hasOneToOneReference || primaryKey.Properties[0].ValueGenerated == ValueGenerated.Never;

        var ignoreFields = _excludesRulesParameters.ExcludeFields.Where(w => w.TableName == tableName)
            .Select(s => s.FieldName).ToList();

        var fieldsBase = entityType.GetProperties().Where(w =>
            ((needsToCreateTempData || usePrimaryKey) && w.IsPrimaryKey()) ||
            (!w.IsPrimaryKey() && !ignoreFields.Contains(w.Name)));
        entityData.NeedsToCreateTempData = needsToCreateTempData;
        entityData.UsePrimaryKey = usePrimaryKey;
        entityData.FieldsData = GetFieldsData(entityType.ClrType, fieldsBase, tableName);
        entityData.Level = GetMaxLevel(entityData);

        Console.WriteLine("EntityAnalysis tableName={0}", tableName);

        entityData.SelfRecursiveFields.AddRange(entityData.FieldsData.Where(w =>
            w.SubstituteField != null && w.SubstituteField.TableName == tableName));

        return entityData;
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
            var analysedSubstEntityType = EntityAnalysis(substEntityType);
            _preventLoopList.Pop();

            if (!(analysedSubstEntityType is not { HasAutoNumber: true } ||
                  !analysedSubstEntityType.HasOneToOneReference))
                return fieldData;

            var substTableName = GetTableName(substEntityType) ??
                                 throw new Exception($"substitute table for table {tableName} have no name");

            if (_excludesRulesParameters.ExcludeTables.Contains(substTableName))
                return fieldData;

            if (!Entities.TryGetValue(substTableName, out var entity))
                throw new Exception($"substitute table {substTableName} Not analyzed for table {tableName}");

            //var optIndex = entity.OptimalIndex;

            fieldData.SubstituteField = new SubstituteFieldData(substTableName,
                entity.OptimalIndexProperties.Count > 0
                    ? GetFieldsData(tableClrType, entity.OptimalIndexProperties, substTableName, fieldData)
                    : []);
            var navName = forKeys[0].DependentToPrincipal?.Name ?? forKeys[0].PrincipalEntityType.ClrType.Name;
            if (navName == null) throw new Exception($"Foreign Keys navName in table {tableName} is empty");

            fieldData.NavigationFieldName = navName;

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
                    w is { IsUnique: true, Properties.Count: 1 } && w.Properties[0].ClrType.Name == "String");
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
        }
    }
}