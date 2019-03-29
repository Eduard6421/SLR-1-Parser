using System;
using System.Collections.Generic;
using System.Text;

namespace Tema2
{
    class Transition 
    {
        public int FromState;
        public int ToState;
        public bool IsGoto;
        public char Character;

        public override bool Equals(object obj)
        {
            Transition trans = obj as Transition;
            return this.FromState == trans.FromState && this.ToState == trans.ToState &&
                   this.Character == trans.Character;
        }

        public override int GetHashCode()
        {
            return 13 * FromState.GetHashCode() +
                   7 * ToState.GetHashCode() +
                   5 * Character.GetHashCode();
        }
    }
}
