using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Persistence; 
public interface IDataAccess
{
    //ez az interfesz az Open-closed elv miatt hasznos, ide tovabbi olyan fuggvenyeket adhatunk a kesobbiekben, mint a fajlba mentes
    Task<(GameBoard, List<(int,int,int,int)>, int)> LoadAsync(String path);  //csupan deklaraljuk, hogy ezt kell majd implementalni

}


