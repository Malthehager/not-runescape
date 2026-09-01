namespace OsrsTracker;

public class Player
{
    public const int StarterGold = 100;
    public int CurrentHp { get; set; } = 35;
    public int MaxHp { get; set; } = 35;
    public int Gold { get; set; } = StarterGold;
    public Dictionary<string, int> Inventory { get; set; } = new()
    {
        { "Lobster", 3 },
        { "Rune Scimitar", 1 }
    };

    public void AddItem(string item, int amount)
    {
        if (Inventory.ContainsKey(item))
            Inventory[item] += amount;
        else
            Inventory[item] = amount;
    }

    public bool DropItem(string item, int amount)
    {
        if (!Inventory.ContainsKey(item) || Inventory[item] < amount)
        {
            return false;
        }

        Inventory[item] -= amount;
        if (Inventory[item] <= 0)
        {
            Inventory.Remove(item);
        }
        return true;
    }

    public void PrintInventory()
    {
        Console.WriteLine("\n--- Inventory ---");
        if (Inventory.Count == 0)
        {
            Console.WriteLine("Your inventory is empty.");
            return;
        }

        foreach (var item in Inventory)
        {
            Console.WriteLine($"- {item.Key}: {item.Value}");
        }
    }
}