using NotRunescape;
using OsrsTracker;

var bossLogs = new List<BossLog>();
var player = new Player();
var hitHistory = new List<int>();

Console.Write("Name?");
string playerName = Console.ReadLine()?.Trim();
if (string.IsNullOrWhiteSpace(playerName))
{
    playerName = "Unknown";
}
Console.WriteLine("=== OSRS Boss & Combat Tracker ===");
Console.WriteLine($"Welcome to Gielinor, {playerName}!");

while (true)
{
    Console.WriteLine($"\n[HP: {player.CurrentHp}/{player.MaxHp} | Gold: {player.Gold} GP]");
    Console.Write("[1] Log Boss Kill  [2] View Drop Log  [3] View Inventory  [4] Drop Item [5] Rest at Lumbridge [99] Fight Hill Giant  [0] Exit\nChoice: ");
    var input = Console.ReadLine()?.Trim();

    if (input == "0") break;

    if (input == "1")
    {
        Console.Write("Boss Name (e.g., Zulrah, Vorkath): ");
        string boss = Console.ReadLine() ?? "Unknown";

        Console.Write("Valuable Drop (e.g., Tanzanite Fang, None): ");
        string drop = Console.ReadLine() ?? "None";

        Console.Write("Did you get a unique drop? (y/n): ");
        bool isUnique = Console.ReadLine()?.Trim().ToLower() == "y";

        bossLogs.Add(new BossLog { BossName = boss, DropName = drop, IsUnique = isUnique });
        Console.WriteLine("Kill logged!");
    }
    else if (input == "2")
    {
        Console.WriteLine("\n--- Drop Log ---");
        Console.WriteLine($"Total Drops Logged: {bossLogs.Count}");
        if (bossLogs.Count == 0) Console.WriteLine("No drops logged yet!");
        for (int i = 0; i < bossLogs.Count; i++)
        {
            var log = bossLogs[i];
            string status = log.IsUnique ? "UNIQUE DROP!" : "Normal Drop";
            Console.WriteLine($"#{i + 1}: {log.BossName} - Drop: {log.DropName} [{status}] ({log.Timestamp:HH:mm})");
        }
    }
    else if (input == "3")
    {
        player.PrintInventory();
    }
    else if (input == "4")
    {
        HandleDropItem(player);
    }
    else if (input == "5")
    {
        player.CurrentHp = player.MaxHp;
        Console.WriteLine($"\nYou rest at Lumbridge and feel fully restored! HP: {player.CurrentHp}/{player.MaxHp}");
    }
    else if (input == "99")
    {
        StartGiantFight(player, bossLogs, hitHistory);
    }
}

static void HandleDropItem(Player player)
{
    player.PrintInventory();
    if (player.Inventory.Count == 0) return;

    Console.Write("\nEnter the exact name of the item to drop: ");
    string itemToDrop = Console.ReadLine()?.Trim() ?? "";

    string matchedKey = player.Inventory.Keys
        .FirstOrDefault(k => string.Equals(k, itemToDrop, StringComparison.OrdinalIgnoreCase));

    if (matchedKey == null)
    {
        Console.WriteLine("You don't have that item.");
        return;
    }
    
    Console.Write("How many to drop?: ");
    if (int.TryParse(Console.ReadLine(), out int amount) && amount > 0)
    {
        if (player.DropItem(matchedKey, amount))
        {
            Console.WriteLine($"Dropped {amount}x {matchedKey}.");
        }
        else
        {
            Console.WriteLine("You don't have enough of that item to drop.");
        }
    }
    else
    {
        Console.WriteLine("Invalid amount.");
    }
}

static void StartGiantFight(Player player, List<BossLog> bossLogs, List<int > hitHistory)
{
    if (player.CurrentHp <= 0)
    {
        Console.WriteLine("\nYou are too weak to fight! Respawning at Lumbridge...");
        player.CurrentHp = player.MaxHp;
        return;
    }

    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("=== HILL GIANT CAVE ===");
    Console.WriteLine("A wild Hill Giant (Level 28) blocks your path!\n");
    Console.ResetColor();

    int giantHp = 35;
    var rng = new Random();
    bool hasFled = false;

    while (player.CurrentHp > 0 && giantHp > 0)
    {
        Console.WriteLine($"Your HP: {player.CurrentHp}/{player.MaxHp} | Hill Giant HP: {giantHp}");
        Console.Write("Action: [1] Slash with Rune Scimitar  [2] Eat Lobster [3] Special Attack  [4] Run Away\nChoice: [6] hitHistory:");
        var choice = Console.ReadLine()?.Trim();

        if (choice == "1")
        {
            int playerHit = rng.Next(0, 15);
            hitHistory.Add(playerHit);
            giantHp -= playerHit;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nYou slash the Hill Giant for a {playerHit}!");
            Console.ResetColor();
        }
        else if (choice == "2")
        {
            if (player.Inventory.ContainsKey("Lobster") && player.Inventory["Lobster"] > 0)
            {
                player.Inventory["Lobster"]--;
                player.CurrentHp = Math.Min(player.MaxHp, player.CurrentHp + 12);
                Console.WriteLine($"\nYou ate a Lobster! Restored HP to {player.CurrentHp}.");
            }
            else
            {
                Console.WriteLine("\nYou don't have any Lobsters in your inventory!");
            }
        }
        else if (choice == "3")
        {
            if (player.Gold >= 50)
            {
                player.Gold -= 50;
                int hit1 = rng.Next(0, 10);
                int hit2 = rng.Next(0, 10);
                int totalDamage = hit1 + hit2;
                giantHp -= totalDamage;

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\nYou unleash a Special Attack! ({hit1} + {hit2} = {totalDamage} damage) [-50 GP]");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine("\nYou don't have enough gold (50 GP) to cast a Special Attack!");
            }
        }
        else if (choice == "4")
        {
            bool escaped = rng.Next(0, 2) == 0;
            if (escaped)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nYou successfully flee from the Hill Giant!");
                Console.ResetColor();
                hasFled = true;
                break;
            }
            else
            {
                Console.WriteLine("\nYou try to flee, but the Hill Giant blocks your path!");
            }
        }
        else if (choice == "6")
        {
            Console.WriteLine("\n--- Top 3 Hits ---");

            if (hitHistory.Count == 0)
            {
                Console.WriteLine("No hits recorded yet!");
            }
            else
            {
                var topHits = hitHistory
                    .OrderByDescending(hit => hit)
                    .Take(3);

                foreach (var hit in topHits)
                {
                    Console.WriteLine($"Hit: {hit}");
                }
            }
        }

        if (giantHp > 0)
        {
            int giantHit = rng.Next(0, 6);
            player.CurrentHp -= giantHit;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"The Hill Giant swings his club for {giantHit} damage!\n");
            Console.ResetColor();
        }
    }

    if (hasFled)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\nYou escape the Hill Giant Cave with your life, leaving any loot behind.");
        Console.ResetColor();
        return;
    }

    if (player.CurrentHp > 0)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\nVICTORY! The Hill Giant collapses!");
        Console.ResetColor();

        // Selective Loot Prompt
        var droppedItems = new List<(string Name, bool IsUnique)>
        {
            ("Big Bones", false),
            ("Limpwurt Root", false),
            ("Giant Key", true)
        };

        Console.WriteLine("\n--- Ground Loot ---");
        foreach (var drop in droppedItems)
        {
            Console.Write($"Pick up {drop.Name}? (y/n): ");
            var choice = Console.ReadLine()?.Trim().ToLower();

            if (choice == "y")
            {
                player.AddItem(drop.Name, 1);
                bossLogs.Add(new BossLog
                {
                    BossName = "Hill Giant",
                    DropName = drop.Name,
                    IsUnique = drop.IsUnique
                });
                Console.WriteLine($"Picked up 1x {drop.Name} and logged it!");
            }
            else
            {
                Console.WriteLine($"Left {drop.Name} on the ground.");
            }
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\nOh dear, you are dead! Teleporting back to Lumbridge...");
        player.CurrentHp = player.MaxHp;
        Console.ResetColor();
    }
}