/*
 * 
 * İSİMLENDİRMEDE DEĞİŞKENLER CAMEL CASE, METHODLAR VE SINIFLAR PASCAL CASE, CONST'LAR TAMAMI BÜYÜK HARFLERLE YAZILMIŞTIR.
 * KAYNAK KODUNDA YORUM SATIRLARI HARİCİNDE TÜRKÇE KARAKTER KULLANILMAMIŞTIR.
 * 
 */

using System;

namespace MazeGame
{
    internal partial class Controller
    {
        const ushort MAPSIZEX = 10;
        const ushort MAPSIZEY = 10;
        const ushort ESSENTIALROADCOUNT = 3;
        const ushort ROADSTRAIGHTNESS = 20;
        const int DRAWLEFT = 0, DRAWTOP = 0;

        internal Controller()
        {
            //Haritaya kullanıcı tarafından çok fazla yol yapılan girilmemesi için denetim.
            ushort essentialRoadCount = ESSENTIALROADCOUNT;
            if (ESSENTIALROADCOUNT > Math.Ceiling(MAPSIZEX / 3.0 )) essentialRoadCount = (ushort)Math.Ceiling(MAPSIZEX / 3.0);
            if (essentialRoadCount > 9) essentialRoadCount = 9;

            //
            ushort mapSizeX = MAPSIZEX;
            if (MAPSIZEX < 3) mapSizeX = 3;
            else if (MAPSIZEX > 40) mapSizeX = 40;

            //
            ushort mapSizeY = MAPSIZEY;
            if (MAPSIZEY < 3) mapSizeY = 3;
            else if (MAPSIZEY > 40) mapSizeY = 40;

            //Lokal Değişkenler
            bool bombVisibility = false;
            bool userInMap = false;
            byte gameOver = 0;
            byte[,] map = new byte[mapSizeX, mapSizeY];
            ushort[] userCoordinate = new ushort[2];
            short[,] roadEssentialPoints = new short[mapSizeY, essentialRoadCount];
            var rand = new Random();
            short randomNumber;
            short score = 0;

            //Yolların oluşturulması.
            for (int y = mapSizeY - 1; y >= 0; y--)
            {
                for (int essentialRoad = 0; essentialRoad < essentialRoadCount; essentialRoad++)
                {
                    //İlk esas noktaların bulunması
                    if (y == mapSizeY - 1)
                    {
                        do
                        {
                            randomNumber = (short)rand.Next(0, mapSizeX);
                        }
                        while (IsThereARoadNear(randomNumber, y, map, mapSizeX));
                        map[randomNumber, y]++;
                        roadEssentialPoints[y, essentialRoad] = randomNumber;
                    }
                    else
                    {
                        //Esas noktanın yöneliminin belirlenmesi
                        if (rand.Next(0, ROADSTRAIGHTNESS + 10) >= 10) randomNumber=0;
                        else
                        {
                            if (roadEssentialPoints[y + 1, essentialRoad] == 0) randomNumber = (short)rand.Next(minValue: 0, maxValue: 2);
                            else if (roadEssentialPoints[y + 1, essentialRoad] == mapSizeX - 1) randomNumber = (short)rand.Next(minValue: -1, maxValue: 1);
                            else randomNumber = (short)rand.Next(minValue: -1, maxValue: 2);
                        }

                        //Yeni esas noktanın belirlenmesi
                        roadEssentialPoints[y, essentialRoad] = (short)(roadEssentialPoints[y + 1, essentialRoad] + randomNumber);

                        //Önceki esas noktayı haritada yol olarak işaretle.
                        map[roadEssentialPoints[y + 1, essentialRoad], y]=1;

                        //Güncel esas noktayı haritada yol olarak işaretle.
                        map[roadEssentialPoints[y, essentialRoad], y] = 1;
                    }

                }
            }
            
            //Bombaların oluşturulması
            randomNumber = (short)rand.Next(0, essentialRoadCount);
            for (int essentialRoad = 0; essentialRoad < essentialRoadCount; essentialRoad++)
            {
                if (randomNumber != essentialRoad)
                {
                    int yPoint;
                    bool xControl;
                    do
                    {
                        yPoint = rand.Next(0, mapSizeY - 1);
                        xControl = true;
                        int xValue = roadEssentialPoints[yPoint, essentialRoad];
                        for (int essentialRoad2 = 0; essentialRoad2 < essentialRoadCount; essentialRoad2++)
                        {
                            if (essentialRoad2 != essentialRoad)
                            {
                                if (xValue == roadEssentialPoints[yPoint, essentialRoad2]) xControl = false;
                            }
                        }
                    } while (!xControl);
                    map[roadEssentialPoints[yPoint, essentialRoad], yPoint] = 2;
                }
            }

            ushort[] essentialPointsSortID = SortEssentialPoints(roadEssentialPoints, essentialRoadCount, mapSizeY);

            //Karşılama Ekranı
            HelpScreen(0);

            //Oyun Akışı
            do
            {
                Console.Clear();
                PrintMap(map, mapSizeX, mapSizeY, roadEssentialPoints, essentialRoadCount, essentialPointsSortID, bombVisibility, userCoordinate, userInMap, ref gameOver, ref score);
                //PrintEssantialPoints(roadEssentialPoints, essentialRoadCount, cursorLeft: (ushort)(mapSizeX * 2 + 2), cursorTop: 0, mapSizeX);

                while (!Console.KeyAvailable){ }//Tuşa basılı değilken çalışır.
                ConsoleKey key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape) break;
                else if (gameOver == 0)
                {
                    if (key == ConsoleKey.G) bombVisibility = !bombVisibility;
                    else if (userInMap && (key == ConsoleKey.W || key == ConsoleKey.A || key == ConsoleKey.S || key == ConsoleKey.D)) ActionInMaze(key, map, ref userCoordinate, ref score, ref bombVisibility, mapSizeX, mapSizeY, roadEssentialPoints, essentialRoadCount, essentialPointsSortID, ref userInMap, ref gameOver);
                    else if (!userInMap && (key == ConsoleKey.D1 || key == ConsoleKey.D2 || key == ConsoleKey.D3 || key == ConsoleKey.D4 || key == ConsoleKey.D5 || key == ConsoleKey.D6 || key == ConsoleKey.D7 || key == ConsoleKey.D8 || key == ConsoleKey.D9)) SelectRoad(key, ref userCoordinate, ref userInMap, mapSizeX, mapSizeY, map);
                }


            } while (true);
        }

        public void PrintMap (byte[,] map, int sizeX, int sizeY, short[,] essentialPoints, ushort essentialRoadCount, ushort[] essentialSortID, bool bombVisibility, ushort[] userCoordinate, bool userInMap, ref byte gameOver, ref short score)
        {
            //Haritanın çizim konumu
            Console.SetCursorPosition(DRAWLEFT, DRAWTOP);

            //Haritanın çizimi
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if (map[x, y] == 0) ConsoleTypewrite("0 ", ConsoleColor.Blue);
                    else if (map[x, y] == 1)
                    {
                        if (userInMap && userCoordinate[0] == x && userCoordinate[1]== y) ConsoleTypewrite("K ", ConsoleColor.DarkYellow);
                        else ConsoleTypewrite("1 ", ConsoleColor.White);
                    }
                    else if (map[x, y] == 2)
                    {
                        if (bombVisibility) ConsoleTypewrite("2 ", ConsoleColor.DarkRed);
                        else ConsoleTypewrite("1 ", ConsoleColor.White);
                    }
                    else ConsoleTypewrite(map[x, y] + " ", ConsoleColor.White);
                }
                Console.SetCursorPosition(DRAWLEFT, Console.CursorTop+1);
            }

            //Başlangıç konumlarının çizimi
            Console.SetCursorPosition(DRAWLEFT, Console.CursorTop + 1);
            for (int essentialNumber = 0; essentialNumber < essentialRoadCount; essentialNumber++)
            {
                ConsoleTypewrite(essentialSortID[essentialNumber], cursorLeft: essentialPoints[sizeY - 1, essentialNumber] * 2 + DRAWLEFT, cursorTop: Console.CursorTop);
            }

            ConsoleTypewrite(post: "Skorunuz " + score + ".", cursorLeft: DRAWLEFT, cursorTop: Console.CursorTop + 2);

            if (gameOver == 1)
            {
                ConsoleTypewrite(post: "Bombaya denk geldiniz. Oyun Bitti.", cursorLeft: DRAWLEFT, cursorTop: Console.CursorTop + 3);
            }

            if (gameOver == 2)
            {
                ConsoleTypewrite(post: "Tebrikler kazandiniz. Oyun Bitti.", cursorLeft: DRAWLEFT, cursorTop: Console.CursorTop + 3);
            }
        }

        public int[] SearchRoadsInOneLine(int line, byte[,] map, int mapSizeX)
        {
            int count = 0;

            for (int x = 0; x < mapSizeX; x++)
            {
                if (map[x, line] == 1) count++;
            }
            int[] roads = new int[count];

            count = 0;
            for (int x = 0; x < mapSizeX; x++)
            {
                if (map[x, line] == 1) { roads[count] = x; count++; }
            }

            return roads;
        }

        public bool ContainNumberInArray(int searchingNumber, int[] array)
        {
            bool IsContain = false;
            foreach (int number in array)
            {
                if (number == searchingNumber) IsContain = true;
            }
            return IsContain;
        }

        public bool IsThereARoadNear(int searchingColoum, int serachingLine, byte[,] map, ushort mapSizeX)
        {
            bool IsThere = false;
            
            foreach (int number in SearchRoadsInOneLine(serachingLine, map, mapSizeX))
            {
                if (number == searchingColoum || number == searchingColoum-1 || number == searchingColoum+1) IsThere = true;
            }
            return IsThere;
        }

        public void PrintEssantialPoints (short[,] roadEssentialPoints, ushort essentialRoadCount, ushort cursorLeft, ushort cursorTop, ushort mapSizeX)
        {
            int t = 1;
            int howManyCharacters = 0;
            short maxCharactersPoints = (short)Math.Log10(mapSizeX);
            Console.SetCursorPosition(cursorLeft, cursorTop);
            foreach (short essentialpoint in roadEssentialPoints)
            {
                ConsoleTypewrite(essentialpoint + " ", ConsoleColor.Cyan);
                howManyCharacters += (essentialpoint != 0 ? (int)Math.Log10(essentialpoint) + 1 : 1) + 1;
                if (t % essentialRoadCount == 0)
                {
                    Console.SetCursorPosition(Console.CursorLeft - howManyCharacters, Console.CursorTop + 1);
                    howManyCharacters = 0;
                }
                t++;
            }
        }

        public ushort[]  SortEssentialPoints (short[,] roadEssentialPoints, ushort essentialRoadCount, int sizeY)
        {
            ushort[] sortID = new ushort[essentialRoadCount];
            for (int essentialNumber = 0; essentialNumber < essentialRoadCount; essentialNumber++)
            {
                ushort essentialSortID = 1;
                for (int essentialNumber2 = 0; essentialNumber2 < essentialRoadCount; essentialNumber2++)
                {
                    if (roadEssentialPoints[sizeY - 1, essentialNumber] > roadEssentialPoints[sizeY - 1, essentialNumber2]) essentialSortID++;
                }
                sortID[essentialNumber] = essentialSortID;
            }
            return sortID;
        }

        public void HelpScreen(ushort SLOWNESS)
        {
            ConsoleTypewriteLine("\nLABIRENT OYUNU", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkGreen);
            ConsoleTypewriteLine("\n", slowness: SLOWNESS);
            ConsoleTypewriteLine("\nHoşgeldiniz...", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewriteLine("\nBaşlangıç için bir yol seçilmelidir.", slowness: SLOWNESS);
            ConsoleTypewriteLine("\nLabirentteki yollar üzerine gizli bombalar yerleştirilmiştir.", slowness: SLOWNESS);
            ConsoleTypewriteLine("\nOyunun amacı bombalara yakalanmadan labirenti tamamlamaktır.", slowness: SLOWNESS);
            ConsoleTypewriteLine("\nTekrar başlangıç noktalarına dönerseniz eğer yol değiştirebilirsiniz.", slowness: SLOWNESS);
            ConsoleTypewriteLine("\n", slowness: SLOWNESS);
            ConsoleTypewrite("\nYol seçmek için klayvedeki sayı tuşlarını kullanabilirsiniz. '", slowness: SLOWNESS);
            ConsoleTypewrite("1", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewrite("','", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("2", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewrite("','", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("3", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewriteLine("'", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("\nLabirent içinde hareket etmek için '", slowness: SLOWNESS);
            ConsoleTypewrite("W", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewrite("','", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("A", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewrite("','", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("S", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewrite("','", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("D", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewriteLine("' tuşlarını kullanabilirsiniz.", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("\nBombaları görmek için '", slowness: SLOWNESS);
            ConsoleTypewrite("G", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewriteLine("' tuşuna basın.", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("\nBaşlamak için '", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            ConsoleTypewrite("ENTER", slowness: SLOWNESS, consoleColor: ConsoleColor.DarkRed);
            ConsoleTypewriteLine("'a basınız.", slowness: SLOWNESS, consoleColor: ConsoleColor.White);
            do
            {
                while (!Console.KeyAvailable) { }//Tuşa basılı değilken çalışır.
            } while (Console.ReadKey(true).Key != ConsoleKey.Enter);
        }

        public void ActionInMaze(ConsoleKey key, byte[,] map, ref ushort[] userCoordinate, ref short score, ref bool bombVisibility, int sizeX, int sizeY, short[,] essentialPoints, ushort essentialRoadCount, ushort[] essentialSortID, ref bool userInMap, ref byte gameOver)
        {
            byte destination;
            switch (key)
            {
                case ConsoleKey.W:
                    if (userCoordinate[1] > 0)
                    {
                        destination = map[userCoordinate[0], userCoordinate[1] - 1];
                        if (destination == 1)
                            if (userCoordinate[1] > 1) WalkInMaze(direction.Forward, ref score, ref userCoordinate);
                            else
                            {
                                userCoordinate[1]--;
                                bombVisibility = true;
                                gameOver = 2;
                            }
                        else if (destination == 0) score--;
                        else if (destination == 2)
                        {
                            bombVisibility = true;
                            gameOver = 1;
                        }
                    }
                    break;
                case ConsoleKey.A:
                    if (userCoordinate[0] > 0)
                    {
                        destination = map[userCoordinate[0] - 1, userCoordinate[1]];
                        if (destination == 1) WalkInMaze(direction.Left, ref score, ref userCoordinate);
                        else if (destination == 0) score--;
                        else if (destination == 2)
                        {
                            bombVisibility = true;
                            gameOver = 1;
                        }
                    }
                    break;
                case ConsoleKey.S:
                    if (userCoordinate[1] < sizeY-1)
                    {
                        destination = map[userCoordinate[0], userCoordinate[1] + 1];
                        if (destination == 1) WalkInMaze(direction.Bacward, ref score, ref userCoordinate);
                        else if (destination == 0) score--;
                        else if (destination == 2)
                        {
                            bombVisibility = true;
                            gameOver = 1;
                        }
                    }
                    else
                    {
                        userInMap = false;
                        //Yol değştir.
                    }
                    break;
                case ConsoleKey.D:
                    if (userCoordinate[0] < sizeX-1)
                    {
                        destination = map[userCoordinate[0] + 1, userCoordinate[1]];
                        if (destination == 1) WalkInMaze(direction.Right, ref score, ref userCoordinate);
                        else if (destination == 0) score--;
                        else if (destination == 2)
                        {
                            bombVisibility = true;
                            gameOver = 1;
                        }
                    }
                    break;

                default:
                    break;
            }
        }

        internal enum direction {Forward,Bacward,Left,Right}

        public void WalkInMaze(direction direction, ref short score, ref ushort[] userCoordinate)
        {
            switch(direction)
            {
                case direction.Forward:
                    userCoordinate[1]--;
                    break;

                case direction.Bacward:
                    userCoordinate[1]++;
                    break;

                case direction.Left:
                    userCoordinate[0]--;
                    break;

                case direction.Right:
                    userCoordinate[0]++;
                    break;
            }
            score++;
        }

        public void SelectRoad(ConsoleKey key, ref ushort[]userCoordinate, ref bool userInMap, ushort sizeX, ushort sizeY, byte[,] map)
        {
            userCoordinate[1] = (ushort)(sizeY - 1);
            userInMap = true;
            int road = 0;
            int selectedRoad = 0;
            switch(key)
            {
                case ConsoleKey.D1:
                    selectedRoad = 1;
                    break;

                case ConsoleKey.D2:
                    selectedRoad = 2;
                    break;

                case ConsoleKey.D3:
                    selectedRoad = 3;
                    break;

                case ConsoleKey.D4:
                    selectedRoad = 4;
                    break;

                case ConsoleKey.D5:
                    selectedRoad = 5;
                    break;

                case ConsoleKey.D6:
                    selectedRoad = 6;
                    break;

                case ConsoleKey.D7:
                    selectedRoad = 7;
                    break;

                case ConsoleKey.D8:
                    selectedRoad = 8;
                    break;

                case ConsoleKey.D9:
                    selectedRoad = 9;
                    break;
            }

            for (ushort x = 0; x < sizeX; x++)
            {
                if (map[x, sizeY - 1] == 1)
                {
                    road++;
                    if (road == selectedRoad) userCoordinate[0] = x;
                }
            }
        }
    }
}
