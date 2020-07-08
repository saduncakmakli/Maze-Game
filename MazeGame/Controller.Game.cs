using System;

namespace MazeGame
{
    internal partial class Controller
    {
        const ushort MAPSIZEX = 40;
        const ushort MAPSIZEY = 40;
        const ushort ESSENTIALROADCOUNT = 12;
        const ushort ROADSTRAIGHTNESS = 20;

        internal Controller()
        {
            Console.ReadKey();

            //Haritaya kullanıcı tarafından çok fazla yol yapılan girilmemesi için denetim.
            ushort essentialRoadCount = ESSENTIALROADCOUNT;
            if (ESSENTIALROADCOUNT > Math.Ceiling(MAPSIZEX / 3.0 )) essentialRoadCount = (ushort)Math.Ceiling(MAPSIZEX / 3.0);

            //
            ushort mapSizeX = MAPSIZEX;
            if (MAPSIZEX < 3) mapSizeX = 3;

            //
            ushort mapSizeY = MAPSIZEY;
            if (MAPSIZEY < 3) mapSizeY = 3;

            //Lokal Değişkenler
            byte[,] map = new byte[mapSizeX, mapSizeY];
            short[,] roadEssentialPoints = new short[mapSizeY, essentialRoadCount];
            var rand = new Random();
            short randomNumber;

            //Console.WriteLine(ContainNumberInArray(0, SearchRoadsInOneLine(9, map)));
            //Console.WriteLine(IsThereARoadNear(0, 9, map));

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

            PrintMatrix(map, mapSizeX, mapSizeY);
            PrintEssantialPoints(roadEssentialPoints, essentialRoadCount, cursorLeft: (ushort)(mapSizeX * 2 + 2), cursorTop: 0, mapSizeX);
        }

        public void PrintMatrix (byte[,] matrix, int sizeX, int sizeY)
        {
            for (int y = 0; y < sizeY; y++)
            {
                for (int x = 0; x < sizeX; x++)
                {
                    if(matrix[x, y] == 0) ConsoleTypewrite("[]", ConsoleColor.DarkRed);
                    else if(matrix[x, y] == 1) ConsoleTypewrite("  ", ConsoleColor.DarkYellow);
                    else ConsoleTypewrite(matrix[x, y] + "/", ConsoleColor.White);
                }
                Console.WriteLine();
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
    }
}
