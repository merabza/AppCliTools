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
            //ერთი ერთეულის გაანალიზება
            EntityAnalysis(entityType);
    }

    //ამ მეთოდის დანიშნულებაა შეამოწმოს ერთეული გამოდგება თუ არა გასაანალიზებლად
    //ამისათვის მოწმდება ერთეულს აქვს თუ არა მთავარი გასაღები
    //და ხომ არ არის შესაბამისი ცხრილის სახელი გამონაკლისების სიაში
    //როგორც გვერდითი ეფექტი, ეს მეთოდი ადგენს ერთეულის შესაბამისი ცხრილის სახელს
    private string? ValidateEntityForAnalise(IEntityType entityType)
    {
        //თუ მთავარი გასაღები არ აქვს, ან მთავარი გასაღების ველების რაოდენობა 1-ის ტოლი არ არის, ასეთ ცხრილებს არ ვაკოპირებთ
        if (entityType.GetKeys().Count(w => w.IsPrimaryKey()) != 1)
            return null;

        //დავადგინოთ ცხრილის სახელი ერთეულის ტიპიდან გამომდინარე
        var tableName = GetTableName(entityType);

        //თუ ცხრილი არის გამონაკლისების სიაში, მას აღარ განვიხილავთ
        if (tableName is not null && _excludesRulesParameters.ExcludeTables.Contains(tableName))
            return null;

        return tableName;
    }

    //ამ მეთოდის საშუალებით ხდება ერთი ერთეულის გაანალიზება
    private EntityData? EntityAnalysis(IEntityType entityType)
    {
        //ერთეულის ვალიდაცია და შესაბამისი ცხრილის სახელის დადგენა
        var tableName = ValidateEntityForAnalise(entityType);

        //თუ ცხრილის სახელი არ დაგვიბრუნა ვალიდაციამ, ეს ნიშნავს, რომ ან ამ ერთეულს არ ვაანალიზებთ, ან ცხრილის სახელის დადგენა ვერ მოხერხდა.
        //თუ ცხრილის სახელი ვერ დადგინდა შესაბამისად ამ ერთეულს არ ვაანალიზებთ
        //ასევე თუ ერთეული უკვე გაანალიზებულების ლექსიკონშია, თავიდან აღარ ვაანალიზებთ
        if (tableName is null || Entities.ContainsKey(tableName))
            return tableName is null ? null : Entities[tableName];

        //დავადგინოთ ცხრილის მთავარი გასაღები
        var primaryKeys = entityType.GetKeys().Where(w => w.IsPrimaryKey()).ToList();

        //გასაღები უნდა იყოს აუცილებლად 1 ცალი, ჯერჯერობით სხვა ვერსიებს არ განვიხილავთ
        var primaryKey = primaryKeys[0];

        //შევქმნათ ობიექტი ერთეულთან დაკავშირებით რა ინფორმაციასაც შევაგროვებთ, იმის შესანახად
        //ჯერ გადავინახოთ ის ინფორმაცია, რაც უკვე ვიცით
        //ეს არის ცხრილის სახელი მთავარი გასაღების ველის სახელი და თვითონ ერთეულის ობიექტი
        var entityData = new EntityData
        {
            TableName = tableName, PrimaryKeyFieldName = primaryKey.Properties[0].Name, EntityType = entityType
        };

        //ჩავამატოთ უკვე გაანალიზებული ერთეულების სიაში
        Entities.Add(tableName, entityData);

        //დავადგინოთ ცხრილის მთავარი გასაღები ასევე არის თუ არა ავტონამბერი.
        entityData.HasAutoNumber = primaryKey.Properties[0].ValueGenerated == ValueGenerated.OnAdd;

        ////დავადგინოთ რომელიმე გარე გასაღები არის თუ არა ამავდროულად ამ ცხრილის მთავარი გასაღების ველი
        ////ამით დგინდება, ეს ცხრილი დაკავშირებულია სხვა ცხრილთან ერთი ერთთან კავშირით
        //var hasOneToOneReference = !isAutoNumber && entityType.GetForeignKeys()
        //    .Any(s => s.Properties.Any(w => w.Name == entityData.PrimaryKeyFieldName));

        //დავადგინოთ რომელ ცხრილთან არის ეს ცხრილი დაკავშირებული ერთი ერთთან კავშირით
        var referencingOneToOneForeignKey = entityType.GetForeignKeys()
            .SingleOrDefault(s => s.Properties.Any(w => w.Name == entityData.PrimaryKeyFieldName));

        var oneToOneParentType = referencingOneToOneForeignKey?.PrincipalEntityType;
        if (oneToOneParentType != null)
        {
            var analysedOneToOneParentEntityType = AnaliseEntityTypeWithPreventLoop(tableName, oneToOneParentType);
            if (analysedOneToOneParentEntityType != null)
            {
                entityData.HasOneToOneReference = true;
                entityData.HasAutoNumberByOneToOnePrincipal = analysedOneToOneParentEntityType.HasAutoNumber ||
                                                              analysedOneToOneParentEntityType
                                                                  .HasAutoNumberByOneToOnePrincipal;
            }
        }

        //დავადგინოთ სჭირდება თუ არა ამ ერთეულის შესაბამისი მონაცემების სიდერს დროებითი ინფორმაციის შენახვა.
        //დროებითი ინფორმაციის შენახვა საჭიროა შემდეგ შემთხვევებში:
        //  თუ ამ ცხრილის მთავარი გასაღები არის ავტონამბერი და მიბმულია რელაციური კავშირით სხვა რომელიმე ცხრილის გარე გასაღებთან
        //    მაშინ უნდა დავადგინოთ რომელი ინდექსი უნდა გამოვიყენოთ როგორც ალტერნატიოული ინდექსი.
        //      თუ გამონაკლის წესებში მითითებულია, გამოსაყენებელი, მაშინ ეს ინდექსი უნდა გამოვიყემოთ ალტერნატიულ ინდექსად
        //      თუ გამონაკლის წესებში არ არის მითითებული, მაშინ დავადგინოთ ოპტიმალური ჩამნაცვლებელი ინდექსი.
        //      თუ ჩამნაცვლებელი ინდექსის დაანგარიშება ვერ მოხერხდა, მაშინ სიდერს სჭირდება დროებითი ინფორმაციის შენახვა

        var needsToCreateTempData = false;

        var hasReferencingForeignKeys = entityType.GetReferencingForeignKeys().Any();

        //თუ მთავარი გასაღების დაგენერირება ავტომატურად არ ხდება, მაშინ უნდა მოხდეს მისი გამოყენება და ოპტიმალური ინდექსის ძებნა აღარ არის საჭირო
        if ((entityData.HasAutoNumber || entityData.HasAutoNumberByOneToOnePrincipal) && hasReferencingForeignKeys)
            //თუ მთავარი გასაღები თვითონ ივსება და ამ ცხრილზე სხვა ცხრილები არის დამოკიდებული.
            //მაშინ მოვძებნოთ ოპტიმალური ინდექსი
        {
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

        var usePrimaryKey = primaryKey.Properties[0].ValueGenerated == ValueGenerated.Never;

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

            var substEntityType = forKeys[0].PrincipalEntityType;

            var analysedSubstEntityType = AnaliseEntityTypeWithPreventLoop(tableName, substEntityType);

            if (analysedSubstEntityType is not null && analysedSubstEntityType.HasAutoNumberByOneToOnePrincipal)
                return fieldData;

            //if (analysedSubstEntityType != null &&
            //    analysedSubstEntityType.HasAutoNumber &&
            //    analysedSubstEntityType.HasOneToOneReference == false)
            //    return fieldData;

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
            if (navName == null)
                throw new Exception($"Foreign Keys navName in table {tableName} is empty");

            fieldData.NavigationFieldName = navName;

            return fieldData;
        }
    }

    private EntityData? AnaliseEntityTypeWithPreventLoop(string preventLoopTableName, IEntityType entityType)
    {
        if (_preventLoopList.Contains(preventLoopTableName))
            throw new Exception($"table {preventLoopTableName} loops for indexes");

        _preventLoopList.Push(preventLoopTableName);
        var analysedEntityType = EntityAnalysis(entityType);
        _preventLoopList.Pop();

        return analysedEntityType;
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