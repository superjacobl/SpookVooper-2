using System.Threading.Tasks;
using SV2.Database.Models.Groups;
using SV2.Database.Models.Economy;
using SV2.Database.Models.Factories;
using SV2.Database.Models.Permissions;
using SV2.Database.Models.Users;
using SV2.Database.Models.Items;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SV2.Managers;

public class BaseRecipe
{
    public KeyValuePair<string, int> Output { get; set; }
    public string Name { get; set; }

    public List<Modifier> Modifiers { get; set; }
}

public class ModifierLevelDefinition
{
    public BuildInModifierTypes ModifierType { get; set; }
    public decimal ModifierValue { get; set; }
    public int Level { get; set; }
    public Dictionary<string, int> Inputs { get; set; }
    public double HourlyProduction { get; set; }
    public string RecipeName { get; set; }
}

public class ConsumerGood
{
    public string Name { get; set; }

    // in x per 1,000 citizens per year
    public double PopGrowthRateModifier { get; set; }

    // the score per 10k citizens that have this good filled
    public double EconomicScoreModifier { get; set; }
    
    // 10k citizens will consume this many units per hour
    public double PopConsumptionRate { get; set; }
}

public class ConsumerGoodGroup
{
    public List<ConsumerGood> ConsumerGoods { get; set; }
}

public class Material_Group
{
    public string Name { get; set; }
    public List<string> Materials { get; set; }
}

public class TopLevelResources
{
    public List<Material_Group> Material_Groups { get; set; }
    public List<BaseRecipe> Recipes { get; set; }
    
    [JsonPropertyName("Consumer Goods")]
    public ConsumerGoodGroup ConsumerGoodGroup { get; set; }
}

public static class ResourceManager 
{
    static public List<string> Resources = new();
    static public List<BaseRecipe> Recipes = new();
    static public List<Material_Group> Material_Groups = new();
    static public List<ConsumerGood> ConsumerGoods = new();
    static public List<ModifierLevelDefinition> ModifierLevelDefinitions = new();

    public static async Task Load()
    {
        TopLevelResources toplevelresource = await JsonSerializer.DeserializeAsync<TopLevelResources>(File.OpenRead("./Managers/resources.json"));

        Material_Groups = toplevelresource.Material_Groups;

        Resources = toplevelresource.Material_Groups.SelectMany(x => x.Materials).ToList();

        Recipes = toplevelresource.Recipes;

        // need to create item definitions

        foreach(string Resource in Resources)
        {
            TradeItemDefinition? def = DBCache.GetAll<TradeItemDefinition>().FirstOrDefault(x => x.OwnerId == "g-vooperia" && x.Name == Resource);

            if (def is null) {
                // now we need to create a definition for this resource
                def = new TradeItemDefinition("g-vooperia", Resource);

                await DBCache.Put<TradeItemDefinition>(def.Id, def);
                await VooperDB.Instance.TradeItemDefinitions.AddAsync(def);
            }
        }
        await VooperDB.Instance.SaveChangesAsync();
    }
}