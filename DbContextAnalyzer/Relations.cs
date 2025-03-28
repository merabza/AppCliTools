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
        //  new JsonSerializerSettings {ReferenceLoopHandling = ReferenceLoopHandling.Ignore});

        //string fileName = $"{name}.json";
        //File.WriteAllText(Path.Combine(_getJsonCreatorParameters.ModelsFolderName, fileName), sampleParamsJsonText);
        File.WriteAllText(fileFullName, sampleParamsJsonText);
        //Console.WriteLine($"{fileName} created");
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

    private void EntityAnalysis(IEntityType entityType)
    {
        //დავადგინოთ ცხრილის სახელი.
        var tableName = GetTableName(entityType);
        if (tableName is null || Entities.ContainsKey(tableName))
            //თუ ეს ცხრილი უკვე ყოფილა გაანალიზებულების სიაში, მაშინ აქ აღარაფერი გვესაქმება
            return;

        if (_excludesRulesParameters.ExcludeTables.Contains(tableName))
            return;

        //entityType, 
        var entityData = new EntityData(tableName);

        Entities.Add(tableName, entityData);

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
        entityData.PrimaryKeyFieldName = primaryKey.Properties[0].Name;

        //თუ მთავარი გასაღების დაგენერირება ავტომატურად არ ხდება, მაშინ უნდა მოხდეს მისი გამოყენება და ოპტიმალური ინდექსის ძებნა აღარ არის საჭირო
        if (primaryKey.Properties[0].ValueGenerated != ValueGenerated.Never &&
            entityType.GetReferencingForeignKeys().Any())
            //თუ მთავარი გასაღები თვითონ ივსება და ამ ცხრილზე სხვა ცხრილები არის დამოკიდებული.
            //მაშინ მოვძებნოთ ოპტიმალური ინდექსი
            entityData.OptimalIndex = GetOptimalUniIndex(entityType);

        var haveOneToOneReference = entityType.GetForeignKeys()
            .Any(s => s.Properties.Any(w => w.Name == entityData.PrimaryKeyFieldName));

        var needsToCreateTempData = entityData.OptimalIndex == null && entityType.GetReferencingForeignKeys().Any();
        var usePrimaryKey = haveOneToOneReference || primaryKey.Properties[0].ValueGenerated == ValueGenerated.Never;

        var ignoreFields = _excludesRulesParameters.ExcludeFields.Where(w => w.TableName == tableName)
            .Select(s => s.FieldName).ToList();

        var fieldsBase = entityType.GetProperties().Where(w =>
            ((needsToCreateTempData || usePrimaryKey) && w.IsPrimaryKey()) ||
            (!w.IsPrimaryKey() && !ignoreFields.Contains(w.Name)));
        entityData.NeedsToCreateTempData = needsToCreateTempData;
        entityData.UsePrimaryKey = usePrimaryKey;
        entityData.FieldsData = GetFieldsData(fieldsBase, tableName);
        entityData.Level = GetMaxLevel(entityData);

        var substituteFields = entityData.FieldsData.Where(w =>
            w.SubstituteField != null && w.SubstituteField.TableName == tableName).ToList();

        if (substituteFields.Count == 1)
            entityData.SelfRecursiveField = substituteFields[0];

        if (entityData.OptimalIndex == null)
            return;
        entityData.OptimalIndexFieldsData = entityData.OptimalIndex.Properties
            .Select(s => entityData.FieldsData.Single(ss => ss.Name == s.Name)).ToList();
        //if (entityData.OptimalIndex.Properties.Count != 1)
        //    return;
    }

    public static string? GetTableName(IEntityType entityType)
    {
        return entityType.GetTableName();
    }

    private int GetMaxLevel(EntityData entityData)
    {
        //List<string> principals = _excludesRulesParameters.TableRelationships
        //  .Where(w => w.Dependent == entityData.TableName).Select(s => s.Principal).ToList();
        //List<KeyValuePair<string, EntityData>> principalsFiltered = Entities.Where(w => principals.Contains(w.Key)).ToList();
        //int maxByPrincipalTables = principalsFiltered.Count == 0 ? 0 : principalsFiltered.Max(m => m.Value.Level) + 1;
        return entityData.FieldsData.Count == 0 ? 0 : entityData.FieldsData.Max(GetLevel) + 1;
        //return maxByPrincipalTables > maxByFields ? maxByPrincipalTables : maxByFields;
    }

    private int GetLevel(FieldData fieldData)
    {
        if (fieldData.SubstituteField == null)
            return 0;
        //if (fieldData.SubstituteField.Fields != null)
        //  return fieldData.SubstituteField.Fields.Select(GetLevel).DefaultIfEmpty().Max() + 1;
        var corEnt = Entities.SingleOrDefault(s => s.Key == fieldData.SubstituteField.TableName);
        return corEnt.Value.Level;
    }

    private List<FieldData> GetFieldsData(IEnumerable<IProperty> fieldsBase, string tableName, FieldData? parent = null)
    {
        var replaceDict = _excludesRulesParameters.GetReplaceFieldsDictByTableName(tableName);

        return fieldsBase.Select(Selector).ToList();

        FieldData Selector(IProperty s)
        {
            var preferredName = replaceDict.TryGetValue(s.Name, out var value) ? value : s.Name;

            //var isNullable = s.IsNullable;
            //var isNullableByParents = parent == null ? s.IsNullable : parent.IsNullableByParents || s.IsNullable;
            //var fieldData = new FieldData(preferredName, s.Name, GetRealTypeName(s.ClrType.Name, s.GetColumnType(), isNullableByParents), (parent == null ? string.Empty : parent.FullName) + preferredName, isNullable, isNullableByParents);

            var fieldData = FieldData.Create(s, preferredName, parent);

            var forKeys = s.GetContainingForeignKeys().ToList();
            switch (forKeys.Count)
            {
                case 0:
                    return fieldData;
                case > 1:
                    throw new Exception($"Multiple Foreign Keys in table {tableName} for field {preferredName}");
            }

            //fieldData.SubstituteForeignKey = forKeys[0];

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

            //fieldData.SubstituteField = new SubstituteFieldData(parent: fieldData, tableName: substTableName,
            //    primaryKeyFieldName: Entities[substTableName].PrimaryKeyFieldName,
            //    fields: Entities[substTableName].OptimalIndex != null
            //        ? GetFieldsData(Entities[substTableName].OptimalIndex.Properties.AsEnumerable(), substTableName,
            //            fieldData)
            //        : null);

            var optIndex = entity.OptimalIndex;

            fieldData.SubstituteField = new SubstituteFieldData(substTableName,
                optIndex is not null
                    ? GetFieldsData(optIndex.Properties.AsEnumerable(), substTableName, fieldData)
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