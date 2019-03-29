using System;
using System.Collections.Generic;
using System.Text;

namespace Tema2
{
    class Production
    { 
        public char ProductionSymbol;
        public int DotPosition;
        public List<Char> ProductionList;


        public override string ToString()
        {
            string output;

            output = ProductionSymbol + " -> ";
            for (int i = 0; i < ProductionList.Count; ++i)
            {
                if (DotPosition == i)
                    output += '*';
                output += ProductionList[i];

            }

            if (DotPosition == ProductionList.Count)
            {
                output += '*';
            }

            return output;

        }


        public Production(char symbol)
        {
            ProductionSymbol = symbol;
            DotPosition = 0;
            ProductionList = new List<char>();
        }

        public Production()
        {
            ProductionList = new List<Char>();
            DotPosition = 0;
        }

        public bool IsTerminal()
        {
            return false;
        }

        public bool IsBehindProduction()
        {
            if (DotPosition == ProductionList.Count)
            {
                return false;
            }
            return char.IsUpper(ProductionList[DotPosition]);

        }

        public bool IsBehindTerminal()
        {
            if (DotPosition == ProductionList.Count)
            {
                return false;
            }
            return !char.IsUpper(ProductionList[DotPosition]);

        }

        public char GetFirst()
        {
            return ProductionList[0];
        }
        public char GetCurrentSymbol()
        {
            return ProductionList[DotPosition];
        }


        public bool IsClosed()
        {
            if (DotPosition == ProductionList.Count)
            {
                return true;
            }

            return false;
        }


    }
}
