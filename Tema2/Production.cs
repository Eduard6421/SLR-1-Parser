using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tema2
{
    class Production
    { 
        public char ProductionSymbol;
        public int DotPosition;
        public List<Char> ProductionList;

        public int ProductionLength()
        {
            int count = 0;

            for (int i = 0; i < ProductionList.Count; ++i)
            {
                count += (ProductionList[i] != '~') ? 1 : 0;
            }

            return count;
        }

        public string PrintableToString()
        {
            string output;

            output = ProductionSymbol + " -> ";
            for (int i = 0; i < ProductionList.Count; ++i)
            {
                output += ProductionList[i];

            }

            return output;
        }

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
        public override int GetHashCode()
        {


            return 13 * (this.ProductionSymbol - '0') +
                   7 * DotPosition+
                   5 * ProductionList.GetHashCode();
        }



        public override bool Equals(object obj)
        {
            Production trans = obj as Production;

            if (this.DotPosition == trans.DotPosition && this.ProductionSymbol == trans.ProductionSymbol && this.ProductionList.Count == trans.ProductionList.Count)
            {
                for (int i = 0; i < this.ProductionList.Count; ++i)
                {
                    if (this.ProductionList[i] != trans.ProductionList[i])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
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
