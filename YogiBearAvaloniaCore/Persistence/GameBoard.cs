using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YogiBearAvaloniaCore.Persistence {
    public class GameBoard
    {
        private Fillers[,] map;

        private int size;
        public int Size => size;

        public GameBoard(int size)
        {
            this.size = size;
            map = new Fillers[size, size];
        }

        //ha raindexelunk egy peldanyra, akkor annak mapjanak a megfelelo mezojet adjuk vissza
        public Fillers this[int x, int y] => map[x, y]; //van erre egyaltalan szukseg??


        //ennek segitsegevel tudjuk beolvasaskor irni az egyes mezoket
        public void SetField(int x, int y, Fillers filler)
        {
            map[x, y] = filler;
        }

    }
}


