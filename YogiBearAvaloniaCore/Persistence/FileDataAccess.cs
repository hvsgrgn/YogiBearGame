using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Persistence; 
//planned format of files to read:
//n m k - number of rows and columns (equally n), number of baskets, number of rangers
//k1(x y dx dy) - k number of rangers, with four pieces of data (x coord, y coord, x direction, y direction) each
//k2(x y dx dy)
//k3(x y dx dy)
//0 0 0 1 0 0 0 2  - layout of the map, n rows and n columns each row
//3 0 0 3 0 0 2 1 - numbers stand for: 0:none, 1: tree, 2: bush, 3: basket
//single space is the dividor between columns, single enter between rows
// we dont store yogi bear's coordinates, bc its always 0,0 at the start
//n may be 8, 10 or 15: small, medium and large map
public class FileDataAccess : IDataAccess
{
    public async Task<(GameBoard, List<(int,int,int,int)>, int)> LoadAsync(String path)
    {
        try
        {
            using (StreamReader reader = new StreamReader(path))    // safe way to open, closes even on abort    
            {
                String line = await reader.ReadLineAsync() ?? String.Empty;
                String[] numbers = line.Split(' '); //felbontjuk a sort space-enkent
                int tableSize = int.Parse(numbers[0]);  //beolvassuk az alapadatokat
                int basketCount = int.Parse(numbers[1]);
                int rangerCount = int.Parse(numbers[2]);

                List<(int,int,int,int)> rangers = new List<(int,int,int,int)>();  //ures listat keszitunk elo a vadoroknek
                for (int i = 0; i < rangerCount; i++)  //a kovetkezo rangerCount mennyisegu sorral feltoltjuk a listat
                {
                    line = await reader.ReadLineAsync() ?? String.Empty;
                    numbers = line.Split(' ');

                    int x = int.Parse(numbers[0]);
                    int y = int.Parse(numbers[1]);
                    int directionX = int.Parse(numbers[2]);
                    int directionY = int.Parse(numbers[3]);
                    rangers.Add((x, y, directionX, directionY));
                }

                GameBoard board = new GameBoard(tableSize);

                //itt maga map meg ures fieldekbol all, majd most feltoltjuk

                for (int i = 0; i < tableSize; i++)
                {
                    line = await reader.ReadLineAsync() ?? String.Empty;
                    String[] row = line.Split(' '); // mind az n sort n db mezore bontjuk

                    for (int j = 0; j < tableSize; j++)
                    {
                        int input = int.Parse(row[j]);
                        switch (input)  //szamnak megfelelo ertekkel hivjuk a setfield metodust a megfelelo, eddig ures cellara
                        {
                            case 0:
                                board.SetField(i, j, Fillers.None); break;
                            case 1:
                                board.SetField(i, j, Fillers.Tree); break;
                            case 2:
                                board.SetField(i, j, Fillers.Bush); break;
                            case 3:
                                board.SetField(i, j, Fillers.Basket); break;
                            default:
                                throw new GameFileLoadInFailed("Unexpected board tile"); // ez most nem tudom kell-e, egesz idaig nem foglalkoztam a hibakkal. Ha igen, akkor ide kell csinalnom custom exceptiont!
                        }
                    }
                }
                return (board, rangers, basketCount); // itt megkap minden adatot a hivo form, amivel peldanyositja a gamemodelt
            }
        }
        catch (Exception ex) when (ex is not GameFileLoadInFailed)
        {
            throw new GameFileLoadInFailed("Failed to load game file");
        }
    }
}



