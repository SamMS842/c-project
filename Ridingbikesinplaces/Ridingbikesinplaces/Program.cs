//player = Green, NPC = Cyan, Hostile = Dark Red, Boss = Red, Console = Deep Purple, Other = Yellow

using System.Runtime.InteropServices;

namespace Ridingbikesinplaces
{
    //other
    public abstract class Position
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Inventory
    {
        private Item[] _inventory;

        public Inventory(int size)
        {
            _inventory = new Item[size];
        }

        public Inventory(Item[] items)
        {
            _inventory = (Item[])items.Clone(); // Copy to be safe
        }

        public void Reset(Item resetItem)
        {
            for (int i = 0; i < _inventory.Length; i++)
            {
                _inventory[i] = resetItem;
            }
        }
        
        public bool AddItem(Item item, int position)
        {
            if (position < 0 || position >= _inventory.Length) // || = or
            {
                return false;
            }

            if (_inventory[position] is Empty)
            {
                _inventory[position] = item;
                return true;
            }

            return false;
        }

        public bool RemoveItem(int position)
        {
            if (position < 0 || position >= _inventory.Length) // || = or
            {
                return false;
            }

            if (_inventory[position] is not Empty)
            {
                _inventory[position] = new Empty()
                {
                    Name = "",
                    Description = "You can place an item here!"
                };
                return true;
            }

            return false;
        }

        public bool MoveItem(int start, int end)
        {
            if (_inventory[start] is not Empty)
            {
                Item item1ToMove = _inventory[start];
                Item item2ToMove = _inventory[end];
                AddItem(item1ToMove, end);
                AddItem(item2ToMove, start);
                return true;
            }

            return false;
        }

        public bool Open(Entity target, int X, int Y) // only use x and y if you need to place elsewere
        {
            //open, equip, destroy, description, hand
            int linesWrote = 0;

            int length = terminal.writeArry(ConsoleColor.Green, _inventory, X, Y);
            terminal.qWrite(ConsoleColor.Green, "--type h or help for commands!", X + length + 2, Y);
            string userInput = Console.ReadLine();
            linesWrote++;
            try
            {
                if (userInput.Contains("help") || userInput == "h")
                {
                    terminal.qWrite(ConsoleColor.DarkMagenta,
                        "<Index>equip/-e, <Index>destroy/-r, <Index>description/-d, help/h, hand, back, move/-m. Eg <2>Destroy, <10>equip",
                        X,
                        Y + 1);
                    linesWrote++;
                }
                else if (userInput.Contains("back") || userInput == "b")
                {
                    terminal.qClear(linesWrote, X, Y);
                    return true;
                }
                else if (userInput.Contains("hand"))
                {
                    terminal.qWrite(ConsoleColor.Green, target.Hand.Name, X, Y + 1);
                    linesWrote++;
                }
                else if (userInput.Contains("move") || userInput.Contains("-m"))
                {
                    pMoveItem(X, Y + 1);
                }
                else // if its not back or help
                {
                    int start = userInput.IndexOf('<') + 1; // get the item of the invetory we want to accses
                    int end = userInput.IndexOf('>');
                    string indexStr = userInput.Substring(start, end - start);
                    if (start >= 1 && end <= _inventory.Length && start < end)
                    {
                        if (int.TryParse(indexStr,
                                out int number)) //if the user unputs a valid number it convers into an int
                        {
                            if (userInput.Contains("equip") || userInput.Contains("-e"))
                            {
                                if (_inventory[number - 1] is Weapon)
                                {
                                    target.Hand = (Weapon)_inventory[number - 1];
                                    _inventory[number - 1] = new Empty();
                                }
                                else
                                {
                                    terminal.qWrite(ConsoleColor.DarkMagenta, "This item is not a weapon!", X, Y + 1);
                                    linesWrote++;
                                }
                            }

                            else if (userInput.Contains("destroy") || userInput.Contains("-r"))
                            {
                                _inventory[number - 1] = new Empty();
                            }

                            else if (userInput.Contains("description") || userInput.Contains("-d"))
                            {
                                terminal.qWrite(ConsoleColor.DarkMagenta, _inventory[number - 1].Description, X, Y + 1);
                                linesWrote++;
                            }
                        }

                        else
                        {
                            terminal.qWrite(ConsoleColor.DarkMagenta, "Invalid input", X, Y + 1);
                            linesWrote++;
                        }
                    }
                }
            }
            catch
            {
                terminal.qWrite(ConsoleColor.DarkMagenta, "!!!Invalid input -ERROR-!!!", X, Y + 1);
                Console.ReadLine();
                terminal.clear(X, Y + 1);
            }

            Console.ReadLine();
            terminal.qClear(linesWrote, X, Y);
            Open(target, X, Y);
            return false;
        }

        public bool pMoveItem(int X, int Y)
        {
            int linesWrote = 0;
            int start;
            int end;
            terminal.qWrite(ConsoleColor.DarkMagenta, "What item would you like to move? ", X, Y);
            terminal.writeArry(ConsoleColor.Green, _inventory, X + "What item would you like to move?".Length, Y);
            string userInput = Console.ReadLine();
            terminal.clear(X, Y);
            terminal.qWrite(ConsoleColor.DarkMagenta, "Were would you like to move? ", X, Y);
            terminal.writeArry(ConsoleColor.Green, _inventory, X + "Were would you like to move? ".Length, Y);
            string userInput2 = Console.ReadLine();
            linesWrote++;
            if (int.TryParse(userInput, out start) && int.TryParse(userInput2, out end))
            {
                if (_inventory[start] is Empty)
                {
                    terminal.qWrite(ConsoleColor.DarkMagenta, "Cannot move empty item", X, Y + 1);
                    Console.ReadLine();
                    terminal.clear(X, Y + 1);
                }

                else
                {
                    MoveItem(start, end);
                    terminal.writeArry(ConsoleColor.Green, _inventory, X, Y + 1);
                    Console.ReadLine();
                    linesWrote++;
                    terminal.qClear(linesWrote, X, Y);
                    return true;
                }
            }
            else
            {
                terminal.qWrite(ConsoleColor.DarkMagenta, "!!!Invalid input!!!", X, Y + 1);
                Console.ReadLine();
                terminal.clear(X, Y + 1);
            }

            return false;
        }

        public bool pAddItem(Item item, int X, int Y) // p = player (player needs to chose were to put the item
        {
            int cPosX = X;
            int linesWrote = 0;

            //"were would you like to place <item>"
            terminal.qWrite(ConsoleColor.DarkMagenta, "(0=trash) Were would you like to place? " + item.Name, X, Y);
            terminal.writeArry(ConsoleColor.Green, _inventory, X + "(0=trash) Were would you like to place? ".Length, Y);
            string userInput = Console.ReadLine();
            linesWrote++;

            int number;
            if (int.TryParse(userInput, out number))
            {
                if (AddItem(item, number))
                {
                    //sucsfull item added to inventory
                    //terminal.clear(X, Y);
                    //terminal.writeArry(ConsoleColor.Green, inventory, X, Y);
                    terminal.qClear(linesWrote, X, Y);
                    return true;
                }
                else //was an item alredy there or invalid input
                {
                    if (number <= 9 && number >= 0)
                    {
                        if (number == 0)
                        {
                            return false;
                        }
                        else
                        {
                            terminal.qWrite(ConsoleColor.DarkMagenta, "[Trash(1)][Move(2)]", X,
                                Y + 1); //using writer so i dont have to corsinate were to write relly just saves my time
                            linesWrote++;
                            string userInput1 = Console.ReadLine();
                            int number1;
                            if (int.TryParse(userInput1, out number1))
                            {
                                terminal.clear(X, Y);
                                if (number1 == 1)
                                {
                                    RemoveItem(number);
                                    AddItem(item, number); // this will return true if it works so no reutn here :)
                                }
                                else if (number1 == 2)
                                {
                                    pMoveItem(X, Y);
                                }
                            }
                        }

                    }
                }
            }
            else
            {
                terminal.qWrite(ConsoleColor.DarkMagenta, "!Invalid Input!" + item.Name, X, Y + 1);
                Console.ReadLine();
                linesWrote++;
                pAddItem(item, X, Y);
            }

            terminal.qClear(linesWrote, X, Y);
            return false;
        }

        public Weapon[] ReturnWeapons()
        {
            return _inventory.OfType<Weapon>().ToArray();
        }
    }

    public class terminal
    {
        public static int writeArry(ConsoleColor color, Item[] arry, int X, int Y)
        {
            int length = 0;
            Console.SetCursorPosition(X, Y);
            foreach (Item item in arry)
            {
                Console.ForegroundColor = color;
                Console.Write('[');
                Console.Write(item.Name);
                Console.Write(']');
                length += item.Name.Length + 2;
            }

            return length;
        }

        public static void qWrite(ConsoleColor foreground, string text, int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            Console.ForegroundColor = foreground;
            Console.Write(text);
        } //only use for dialogs and cut sences

        public static void write(ConsoleColor color, string text, double totalWaitTime, int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            Console.ForegroundColor = color;
            var waitTime = totalWaitTime / text.Length;

            for (var i = 0; i < text.Length; i++)
            {
                Console.Write(text[i]);
                Thread.Sleep((int)Math.Round(waitTime));
            }

            Console.ResetColor();
        } // same thing here 

        public static void clear(int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            Console.Write(new string(' ', Math.Max(0, Console.WindowWidth - X - 1)));
        }

        public static void qClear(int linesToClear, int X, int Y)
        {
            for (int i = 0; i < linesToClear; i++)
            {
                Console.SetCursorPosition(X, Y + i);
                Console.Write(new string(' ',
                    Math.Max(0,
                        Console.WindowWidth - X -
                        1))); // Console Widown with returns the whole consle not from the cursor to end so you have to subtract x and 1 to stop line wrapping
            }
        }

        public static void writeMap(Map map, int X, int Y)
        {
            Console.SetCursorPosition(X, Y);
            for (int y = 0; y < map.GetLength(0); y++) //writing the map to the console
            {
                for (int x = 0; x < map.GetLength(1); x++)
                {
                    terminal.qWrite(map.map[y, x].Color, map.map[y, x].Letter, x, y);
                }
            }
        }

        public static int writeArt(ConsoleColor color, IEnumerable<string> text, int X, int Y)
        {
            int linesWrote = 0;
            for (int i = 0; i < text.Count(); i++)
            {
                terminal.qWrite(color, text.ElementAt(i), X, Y + i);
                linesWrote = i;
            }

            return linesWrote;
        }

        public static string menu(string[] arry, int X, int Y)
        {
            int position = 0;
            while (true)
            {
                for (int i = 0; i < arry.Count(); i++) //draw menu then get userinput
                {
                    Console.Write('[');
                    if (i == position)
                    {
                        terminal.qWrite(ConsoleColor.Green, arry[i], X, Y);
                    }
                    else
                    {
                        terminal.qWrite(ConsoleColor.DarkMagenta, arry[i], X, Y);
                    }

                    Console.Write(']');
                    X = Console.CursorLeft;
                }

                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.LeftArrow && position > 0)
                {
                    position--;
                }

                if (key.Key == ConsoleKey.RightArrow && position < arry.Count() - 1)
                {
                    position++;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    if (arry[position] == "Back")
                    {
                        return "";
                    }

                    return arry[position];

                }
            }

            return null;
        }

        public static void iClear(int X, int Y)
        {
            Console.ReadLine();
            terminal.clear(X, Y);
        }

        public static int[] getInput(int position, int maxBound)
        {
            ConsoleKey key = Console.ReadKey(true).Key;
            switch (key) 
            {
                case ConsoleKey.LeftArrow:
                    if (position > 0)
                    {
                        position--;
                    }
                    break;
                case ConsoleKey.RightArrow:
                    if (position < maxBound)
                    {
                        position++;
                    }
                    break;
                case ConsoleKey.Enter:
                    
                    return [position, -1];
                    break; 
            }
            return [position, 0];
        }
    }


    //Events
    public abstract class Event
    {
        public Player Player { get; init; }
        public ItemManager ItemManager { get; init; }

        public abstract bool Start(Map map);
    }

    public class Dialog : Event
    {
        public string[] speech { get; set; }
        public Item[] items { get; set; }
        public bool spoken { get; set; }

        public override bool Start(Map map)
        {
            int linesWrote = 0;
            if (spoken == false)
            {
                for (int Y = 0; Y < speech.Length; Y++)
                {
                    if (speech[Y].StartsWith("/give ")) //cheking if player has been given an item
                    {
                        int start = speech[Y].IndexOf('<');
                        int end = speech[Y].IndexOf('>');
                        if (start != -1 && end != -1 && end > start) //making sure < > are there to stop errors 
                        {
                            string itemName = speech[Y].Substring(start + 1, end - start - 1);
                            Item item = ItemManager.GetItem(itemName);
                            if (item != null)
                            {
                                terminal.write(ConsoleColor.DarkMagenta, "You have been given " + items[0].Name, 350,
                                    map.X + 1, Y);
                                linesWrote++;
                                Player.Inventory.pAddItem(item, map.X + 1, Y + 1);
                            }
                        }
                    }
                    else if (speech[Y].StartsWith("/end"))
                    {
                        terminal.qClear(linesWrote, map.X + 1, 0);
                        Player.canMove = true;
                        spoken = true;
                        return true;
                    }
                    else
                    {
                        terminal.write(ConsoleColor.Cyan, speech[Y], 1000, map.X + 1, Y);
                        linesWrote++;
                    }

                    Console.ReadLine();
                }
            }
            else
            {
                terminal.write(ConsoleColor.Cyan, "I have nothing else to say!", 750, map.X + 1, 0);
                Console.ReadLine();
                terminal.clear(map.X + 1, 0);
                Player.canMove = true;
                return true;
            }

            return false;
        }
    }

    class Battle : Event
    {
        public Enemy[] Enemies { get; set; }
        private bool PlayerTurn = true;
        private bool battleEned = false;
        
        private bool CheckAllEHealth()
        {
            int count = 0;
            foreach (Enemy enemy in Enemies)
            {
                if (enemy.health > 0)
                {
                    count++;
                }
            }

            if (count == Enemies.Length)
            {
                return true;
            }

            return false;
        }

        private Weapon SelectWeapon(int X, int Y)
        {
            Weapon[] weapons = Player.Inventory.ReturnWeapons();
            int startX = X;
            int[] positionA = [0];
            int position = 0;
            while (weapons.Length != 0)
            {

                for (int i = 0; i < weapons.Length; i++)
                {
                    if (position == i)
                    {
                        terminal.qWrite(ConsoleColor.Green, weapons[i].Name, X, Y);
                    }
                    else
                    {
                        terminal.qWrite(ConsoleColor.DarkGray, weapons[i].Name, X, Y);
                    }
                    X += weapons[i].Name.Length + 2;
                }
                positionA =  terminal.getInput(position, weapons.Length);
                position = positionA[0];
                X = startX;
                
                if (positionA[1] == -1)
                {
                    return weapons[position];
                }
                
            }

            return null;
        }
        
        public override bool Start(Map map)
        {
            int linesWrote = 0;
            terminal.write(ConsoleColor.DarkMagenta, "Battle starting!", 500, map.X + 1, 0);
            terminal.iClear(map.X + 1, 0);
            while (battleEned == false) //makes sure no one is dead
            {
                int firstX = map.X + 1;
                int firstY = 0;
                Random random = new Random();
                int X = 0;
                int linesWorte = 0;
                Console.SetCursorPosition(map.X, 0);
                X = map.X+1;
                int[] position = [0];
                PlayerTurn = true;
                while (PlayerTurn)
                {
                    int tally = 0;
                    for (int i = 0; i < Enemies.Length; i++)
                    {
                        if (Enemies[i].health <= 0)
                        {
                            tally++;
                        }
                        if (tally == Enemies.Length)
                        {
                            terminal.qClear(Enemies[0].art.Height + 3, firstX, firstY);
                            Player.canMove = true;
                            return true;
                        }
                    }

                    for (int i = 0; i < Enemies.Length; i++)
                    {
                        if (Enemies[i].health > 0)
                        {
                            if (position[0] == i) //prints the art as green
                            {
                                terminal.writeArt(ConsoleColor.DarkGreen, Enemies[i].art.Lines, X, 0);
                            }
                            else
                            {
                                terminal.writeArt(ConsoleColor.DarkRed, Enemies[i].art.Lines, X, 0);
                            }

                            terminal.clear(X + 1, Enemies[i].art.Height);
                            terminal.qWrite(ConsoleColor.DarkRed, " H = " + $"{Enemies[i].health}", X + 1,
                                Enemies[i].art.Height);
                            X += Enemies[i].art.Width + 1;
                        }
                    }
                    X = map.X+1;
                    position = terminal.getInput(position[0], Enemies.Length);
                    
                    if (position[1] == -1)//user inutted a return
                    {
                        int targetedEnemy = position[0]; 
                        Weapon selectedWeapon = SelectWeapon(X, Enemies[targetedEnemy].art.Height+2);
                        if (selectedWeapon != null)
                        {
                            Enemies[targetedEnemy].health -= selectedWeapon.Damage;
                            if (Enemies[targetedEnemy].health <= 0)
                            {
                                terminal.write(ConsoleColor.DarkGreen, "You killed "+ Enemies[targetedEnemy].name, X, 500 ,Enemies[targetedEnemy].art.Height+2);
                                terminal.iClear(X, Enemies[targetedEnemy].art.Height+2);
                            }
                            
                        }


                        PlayerTurn = false;
                    }
                    
                }

                while (!PlayerTurn)
                {
                    
                    bool hasAttacked;
                    for (int i = 0; i < Enemies.Length; i++)
                    {
                        if (Enemies[i].health > 0)
                        {
                            int roll = random.Next(1, 100);
                            if (roll >= 75)
                            {
                                hasAttacked = false;
                            }
                            else
                            {
                                hasAttacked = true;
                            }


                            if (hasAttacked)
                            {
                                terminal.qWrite(ConsoleColor.DarkRed, Enemies[i].name + "Has attcked you and did " + Enemies[i].Hand.Damage, X, Enemies[i].art.Height + 2);
                                Player.Health -= Enemies[i].Hand.Damage;
                                terminal.clear(0, map.Y+1);
                                terminal.qWrite(ConsoleColor.Green, "Health = " + Player.Health, 0, map.Y+1);
                            }
                            else
                            {
                                terminal.qWrite(ConsoleColor.DarkRed,
                                    Enemies[i].name + "Tried attacking you and missed", X, Enemies[i].art.Height + 2);
                            }

                            terminal.iClear(X, Enemies[i].art.Height + 2);
                        }
                    }

                    PlayerTurn = true;
                }
                
                terminal.qClear(Enemies[0].art.Height + 3, firstX, firstY);
            }

            return false;
        }
    }

    //maps and objects for map
    public class Map : MapObject
    {
        public MapObject[,] map { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public int GetLength(int dimension)
        {
            return map.GetLength(dimension);
        }
    };

    public abstract class MapObject
    {
        public string Letter { get; set; }
        public ConsoleColor Color { get; set; }
        public bool Solid { get; set; }
        public Event Event { get; set; }

    }

    public class mEnemy : MapObject
    {
        private Enemy[] Enemies { get; set; }
    }

    public class Air : MapObject
    {
        public Air()
        {
            Letter = " ";
            Color = ConsoleColor.Black;
            Solid = false;
        }
    }

    public class Wall : MapObject
    {
        public Wall()
        {
            Letter = "#";
            Color = ConsoleColor.DarkRed;
            Solid = true;
        }
    }


    //Enitys players Enimys

    public abstract class Entity : Position
    {
        public string Letter { get; init; }
        public string name { get; set; }
        public bool canMove { get; set; }
        public Inventory Inventory { get; set; }
        public Weapon Hand { get; set; }
    }

    public class Enemy : Entity
    {
        public required int health { get; set; }
        public Dialog dialog { get; set; }
        public Art art { get; set; }
        public double hitChance { get; set; }
    }

    public class Player : Entity
    {
        public string Name { get; set; }
        public int Health { get; set; }


        public MapObject CheckEntity(MapObject[,] map)
        {
            if (map[Y - 1, X].Event != null) //up
            {
                return map[Y - 1, X];
            }
            else if (map[Y + 1, X].Event != null) //down
            {
                return map[Y + 1, X];
            }
            else if (map[Y, X - 1].Event != null) //left
            {
                return map[Y, X - 1];
            }
            else if (map[Y, X + 1].Event != null)
            {
                return map[Y, X + 1];
            }

            return null;
        }
    }

    public class NPC : MapObject
    {
    }

    //Items
    public class ItemManager
    {
        private Dictionary<string, Item> items = new Dictionary<string, Item>();

        public ItemManager()
        {
            items.Add("damagedSteelSword", new Weapon()
            {
                Name = "Damaged Steel Sword",
                Damage = 5
            });
            items.Add("flimsyWoodenSword", new Weapon()
            {
                Name = "Flimsy Wooden Sword Wooden Sword",
                Damage = 2
            });
            items.Add("weakHealthPotion", new HealthPotion()
            {
                Name = "Weak Health Potion",
                Description = "Heals 10 Health",
                HealthHealed = 10
            });
            items.Add("nothing", new Empty()
            {
                Name = "Empty",
                Description = "You can place an item here!"
            });
        }

        public Item GetItem(string name)
        {
            if (items.TryGetValue(name, out Item item))
            {
                return item;
            }

            return null;
        }

        public Weapon GetWeapon(string name)
        {
            Item item = GetItem(name);
            return item as Weapon;
        }
        
        public bool ItemExists(string itemKey)
        {
            return items.ContainsKey(itemKey);
        }
    }

    public abstract class Item
    {
        public string Name { get; set; }
        public string Description { get; init; }
    }

    public abstract class Consumible : Item
    {
    }

    public class HealthPotion : Consumible
    {
        public int HealthHealed { get; set; }
    }

    public class Weapon : Item
    {
        public int Damage { get; set; }
        //public Enchantment Enchantment { get; set; }
    }

    public class Empty : Item
    {
        public Empty()
        {
            Name = "Empty";
            Description = "You can place an item here!";
        }
    }

    //game systems
    static class Artlibary
    {
        public static Art goblin = new Art(1, 11);
        public static Art goblin_beard = new Art(13, 11);
        public static Art alien = new Art(25, 33);
    }

    public class Art
    {
        public IEnumerable<string> Lines { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
    
        public Art(int skip, int take)
        {
            // Assuming gameArt.txt is in your project root
            string projectRoot = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..");//dk what this doinf exactly but dont touch it 
            string artFilePath = Path.Combine(projectRoot, "gameArt.txt");
            
            Lines = File
                .ReadLines("gameArt.txt").Skip(skip)
                .Take(take).ToArray();
            Width = Lines.Any() ? Lines.Max(s => s.Length) : 0;// i got no clue what this is doing cl
            Height = Lines.Count();
        }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            //items
            ItemManager itemManager = new ItemManager();
            Item nothing = itemManager.GetItem("nothing");
            Item damagedSteelSword = itemManager.GetItem("damagedSteelSword");
            Weapon flimsyWoodenSword = (Weapon)itemManager.GetItem("flimsyWoodenSword");
            Item weakHealthPotion = itemManager.GetItem("weakHealthPotion");
            //player stuff
            Player player = new Player
            {
                X = 2,
                Y = 2,
                Letter = "@",
                Inventory = new Inventory(10),
                Name = "Warrior",
                Health = 100,
                canMove = true
            };
            Enemy testEnemy1 = new Enemy
            {
                name = "Davy Jones",
                canMove = false,
                art = Artlibary.goblin,
                health = 15,
                Hand = flimsyWoodenSword,
                Inventory = new Inventory([weakHealthPotion]),
                hitChance = 75/100
            };
            Enemy testEnemy2 = new Enemy
            {
                name = "Petrified Peter",
                canMove = false,
                art = Artlibary.goblin_beard,
                health = 12,
                Hand = flimsyWoodenSword,
                Inventory = new Inventory([weakHealthPotion]),
                hitChance = 75/100
            };

            player.Inventory.Reset(nothing);

            //map stuff
            MapObject wall = new Wall();
            MapObject Air_ = new Air();
            MapObject npc1 = new NPC()
            {
                Letter = "?",
                Solid = true,
                Color = ConsoleColor.Cyan,
                Event = new Dialog()
                {
                    items =
                    [
                        damagedSteelSword
                    ],
                    speech =
                    [
                        "You should't travle these deserts unarmmed . . . . ",
                        "A Level one like your self could get seriolsy hurt. Here take this . . . Please",
                        "/give <damagedSteelSword>",
                        "I know its not much but it should suffice in defending your self",
                        "/end"
                    ],
                    spoken = false,
                    ItemManager = itemManager,
                    Player = player
                }
            };
            MapObject Bat1 = new mEnemy()
            {
                Color = ConsoleColor.Red,
                Event = new Battle()
                {
                    ItemManager = itemManager,
                    Player = player,
                    Enemies = [testEnemy1, testEnemy2]
                },
                Letter = "G",
                Solid = true
            };
            Map testLevel = new Map()
            {
                map = new MapObject[,]
                {
                    { wall, wall, wall, wall, wall, wall, wall, wall, wall, wall, wall, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, npc1, Air_, Air_, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, wall, wall, wall, Air_, Air_, Air_, Air_, wall, wall, wall, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, Air_, Bat1, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, Air_, wall },
                    { wall, wall, wall, wall, wall, wall, wall, wall, wall, wall, wall, wall },
                },
                X = 11,
                Y = 10
            };

            //program
            Console.Clear();
            Console.CursorVisible = false;
            Map currentMap = testLevel;

            while (true) //game loop
            {
                terminal.writeMap(currentMap, 0, 0);

                Console.SetCursorPosition(player.X, player.Y); // first wite the player to the map
                Console.Write(player.Letter); // @ = player

                ConsoleKey key = Console.ReadKey(true).Key;
                if (player.canMove == true)
                {
                    switch (key)
                    {
                        case ConsoleKey.I: //inventory
                            player.canMove = false;
                            player.Inventory.Open(player, currentMap.X + 1, 0);
                            player.canMove = true;
                            break;
                        case ConsoleKey.T: //talk
                            //check if npc is arount them
                            if (player.CheckEntity(currentMap.map) != null)
                            {
                                Console.SetCursorPosition(13, 13);
                                Console.WriteLine("Is NPC");
                                player.canMove = false; //stops player input
                                player.CheckEntity(currentMap.map).Event.Start(currentMap);
                            }

                            break;
                        case ConsoleKey.UpArrow:
                            if (currentMap.map[player.Y - 1, player.X].Solid == false)
                            {
                                if (player.Y > 0) player.Y--;
                            }

                            break;
                        case ConsoleKey.DownArrow:
                            if (currentMap.map[player.Y + 1, player.X].Solid == false)
                            {
                                if (player.Y < Console.WindowHeight - 1) player.Y++;
                            }

                            break;
                        case ConsoleKey.LeftArrow:
                            if (currentMap.map[player.Y, player.X - 1].Solid == false)
                            {
                                if (player.X > 0) player.X--;
                            }

                            break;
                        case ConsoleKey.RightArrow:
                            if (currentMap.map[player.Y, player.X + 1].Solid == false)
                            {
                                if (player.X < Console.WindowWidth - 1) player.X++;
                            }

                            break;
                    }

                    if (player.CheckEntity(currentMap.map) is { Event: Battle } entity)//check if we need to start a battle
                    {
                        if (entity.Event.Start(currentMap) == true)
                        {
                            Bat1.Letter = " ";
                            Bat1.Solid = false;
                            Bat1.Event = null;
                        }
                    }
                }

                terminal.qWrite(ConsoleColor.Green, "Health = " + player.Health, 0, currentMap.Y+1);






            }

        }

    }
}