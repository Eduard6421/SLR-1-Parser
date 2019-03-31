using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualBasic.CompilerServices;
using System.Text.RegularExpressions;

namespace Tema2
{
    class Program
    {
        private static string InputFile = @"..\..\..\data.in";
        private static string WordsFile = @"..\..\..\words.in";
        private static string OutputFile = @"..\..\..\data.out";
        private static string TableFile = @"..\..\..\table.out";

        private static List<List<Production>> States;
        private static List<List<Production>> finalStates;
        private static List<Production> FirstProductionList;
        private static HashSet<Transition> TransitionList;
        private static Dictionary<char, HashSet<char>> FirstSet;
        private static Dictionary<char, HashSet<char>> FollowSet;
        private static Dictionary<(int, char), string> Actions = new Dictionary<(int, char), string>();
        private static Dictionary<(int, char), int> GoTo = new Dictionary<(int, char), int>();
        private static List<String> Words;
        private static StreamWriter file;


        private static int[] fathers;
        private static int[] actual;

        public static void AddExtraProduction()
        {
            Production extraProduction = new Production('X');
            extraProduction.ProductionList.Add('S');
            FirstProductionList.Insert(0, extraProduction);
        }
        public static void ReadData()
        {
            /* Citeste datele din fisier. 
                Datele trebuie sa fie in formatul
                S Ab
                A Dc
                D cd
                D cA
                Sa inceapa cu S si orice productie multipla exemplu : D ->  cd | Ca 
                trebuie puse ca
                D -> cd 
                D -> Ca
            */

            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(InputFile);

            while ((line = file.ReadLine()) != null)
            {
                Production newProduction = new Production();

                System.Console.WriteLine(line);
                char ProductionSymbol = line[0];
                char currentSymbol;

                if (!FirstSet.ContainsKey(ProductionSymbol))
                    FirstSet.Add(ProductionSymbol, new HashSet<char>());

                if (char.IsLower(ProductionSymbol))
                {
                    throw new Exception("Productie cu litera mica");
                }

                newProduction.ProductionSymbol = ProductionSymbol;

                for (int i = 2; i < line.Length; ++i)
                {
                    currentSymbol = line[i];
                    newProduction.ProductionList.Add(currentSymbol);
                    if (!FirstSet.ContainsKey(currentSymbol))
                        FirstSet.Add(currentSymbol, new HashSet<char>());
                }

                FirstProductionList.Add(newProduction);
                if (!FollowSet.ContainsKey(ProductionSymbol))
                    FollowSet.Add(ProductionSymbol, new HashSet<char>());
            }

        }

        public static void ReadWords()
        {

            string line;
            Words = new List<string>();

            System.IO.StreamReader file = new System.IO.StreamReader(WordsFile);

            while ((line = file.ReadLine()) != null)
            {
                Words.Add(line);
            }

        }

        public static void CreateParseTree()
        {
            bool complete = false;
            while (!complete)
            {
                complete = true;

                for (int i = 0; i < States.Count; ++i)
                {

                    List<Production> newState = new List<Production>();
                    HashSet<char> chars = new HashSet<char>();
                    for (int k = 0; k < States[i].Count; ++k)
                    {
                        while (!States[i][k].IsClosed() && States[i][k].GetCurrentSymbol() == '~')
                            States[i][k].DotPosition += 1;

                        if (!States[i][k].IsClosed())
                            chars.Add(States[i][k].GetCurrentSymbol());
                    }


                    foreach (char car in chars)
                    {
                        newState = new List<Production>();

                        for (int j = 0; j < States[i].Count; ++j)
                        {
                            if (!States[i][j].IsClosed() && States[i][j].GetCurrentSymbol() == car)
                            {

                                Production prod = new Production();


                                prod.ProductionList = States[i][j].ProductionList;
                                prod.DotPosition = States[i][j].DotPosition;
                                prod.ProductionSymbol = States[i][j].ProductionSymbol;

                                prod.DotPosition = prod.DotPosition + 1;

                                while (!prod.IsClosed() && prod.GetCurrentSymbol() == '~')
                                    prod.DotPosition += 1;

                                newState.Add(prod);

                            }
                        }
                        Transition newTranzition = new Transition();
                        newTranzition.Character = car;
                        newTranzition.FromState = i;
                        newTranzition.IsGoto = char.IsUpper(car);


                        List<Production> tmpState = new List<Production>(newState);

                        if (newState.Count > 0)
                        {
                            newState = new List<Production>(tmpState);

                            HashSet<Production> tmpHash = new HashSet<Production>();


                            do
                            {
                                newState = new List<Production>(tmpState);
                                for (int ind = 0; ind < tmpState.Count; ++ind)
                                {
                                    if (tmpState[ind].IsBehindProduction())
                                    {
                                        for (int ind1 = 0; ind1 < FirstProductionList.Count; ++ind1)
                                        {
                                            if (FirstProductionList[ind1].ProductionSymbol ==
                                                tmpState[ind].GetCurrentSymbol() && !tmpHash.Contains(FirstProductionList[ind1]))
                                            {
                                                tmpState.Add(FirstProductionList[ind1]);
                                                tmpHash.Add(FirstProductionList[ind1]);
                                            }
                                        }

                                    }
                                }

                            } while (tmpState.Count != newState.Count);

                            bool contains = false;
                            for (int i2 = 0; i2 < States.Count; ++i2)
                            {


                                if (States[i2].Except(newState).ToList().Count == 0)
                                {
                                    contains = true;
                                    newTranzition.ToState = i2;
                                    TransitionList.Add(newTranzition);

                                }

                            }

                            if (!contains)
                            {
                                complete = false;
                                newTranzition.ToState = States.Count;
                                TransitionList.Add(newTranzition);
                                States.Add(newState);

                            }

                        }

                    }

                }

            }
        }

        public static void PrintAllStates()
        {
            for (int i = 0; i < finalStates.Count; ++i)
            {
                System.Console.WriteLine("State number " + i.ToString());
                for (int j = 0; j < finalStates[i].Count; ++j)
                {
                    System.Console.WriteLine(finalStates[i][j]);
                }
            }
        }

        public static void PrintAllTranzitions()
        {
            File.CreateText(TableFile).Close();
            file = File.AppendText(TableFile);

            foreach (Transition t in TransitionList)
            {

                System.Console.WriteLine(t.FromState + " to " + t.ToState + " with " + t.Character + " operation type :" + (t.IsGoto ? "Move" : "Shift"));
                if (t.IsGoto)
                {
                    GoTo[(t.FromState, t.Character)] = t.ToState;
                    file.WriteLine(String.Format("GOTO({0},{1}) == {2}", t.FromState, t.Character, t.ToState));
                }
                else
                {
                    Actions[(t.FromState, t.Character)] = "S" + t.ToState;
                    file.WriteLine(String.Format("ACTION({0},{1}) == S{2}", t.FromState, t.Character, t.ToState));
                }
            }
        }
        public static void CombineStates()
        {
            HashSet<Tuple<int, int>> sameStates = new HashSet<Tuple<int, int>>();
            HashSet<Transition> newHash = new HashSet<Transition>();
            finalStates = new List<List<Production>>();

            for (int i = 0; i < States.Count; ++i)
            {
                fathers[i] = i;
            }

            for (int i = 0; i < States.Count; ++i)
            {
                for (int j = 0; j < States.Count; ++j)
                {
                    if (i != j)
                    {
                        if (States[i].Except(States[j]).ToList().Count == 0) { }
                        int min = i < j ? i : j;
                        int max = i > j ? i : j;

                        while (fathers[min] != min)
                        {
                            min = fathers[min];
                        }
                        fathers[max] = min;

                    }
                }
            }


            int numOfState = 0;
            for (int i = 0; i < States.Count; ++i)
            {
                if (i == fathers[i])
                {
                    actual[i] = numOfState;
                    List<Production> combinedProduction = new List<Production>();

                    for (int j = 0; j < States.Count; ++j)
                    {
                        if (fathers[j] == i)
                        {
                            foreach (Production t in States[j])
                            {
                                combinedProduction.Add(t);
                                actual[j] = numOfState;
                            }
                        }
                    }

                    if (combinedProduction.Count > 0)
                        finalStates.Add(combinedProduction);
                    numOfState++;
                }
            }

            foreach (Transition t in TransitionList)
            {
                t.ToState = actual[fathers[t.ToState]];
                t.FromState = actual[fathers[t.FromState]];
                if (!newHash.Contains(t))
                    newHash.Add(t);
            }



            TransitionList = newHash;

        }



        public static void AddFirstState()
        {
            bool complete = true;
            HashSet<Production> newState = new HashSet<Production>();
            HashSet<Production> tmpState = new HashSet<Production>();

            tmpState.Add(FirstProductionList[0]);

            do
            {
                newState = new HashSet<Production>(tmpState);

                foreach (var currentProd in newState)
                {
                    for (int j = 0; j < FirstProductionList.Count; ++j)
                    {
                        if (currentProd.GetFirst() == FirstProductionList[j].ProductionSymbol)
                        {
                            tmpState.Add(FirstProductionList[j]);
                            complete = false;
                        }
                    }

                }

            } while (tmpState.Count > newState.Count);

            States.Add(newState.ToList());
        }

        public static void ComputeFirst()
        {
            bool finished = false;

            Dictionary<char, HashSet<char>> tempSet = new Dictionary<char, HashSet<char>>(FirstSet);

            foreach (KeyValuePair<char, HashSet<char>> entry in tempSet)
            {
                if (!char.IsUpper(entry.Key))
                {
                    FirstSet[entry.Key] = new HashSet<char>();
                    FirstSet[entry.Key].Add(entry.Key);
                }

            }

            List<Production> tempProductionList = new List<Production>(FirstProductionList);
            List<Production> deleteProductions = new List<Production>();
            while (!finished)
            {
                deleteProductions = new List<Production>();
                finished = true;

                for (int i = 0; i < tempProductionList.Count; ++i)
                {
                    if (FirstSet[tempProductionList[i].ProductionList[0]].Count > 0)
                    {
                        finished = false;
                        foreach (char symbol in FirstSet[tempProductionList[i].ProductionList[0]])
                        {
                            FirstSet[tempProductionList[i].ProductionSymbol].Add(symbol);
                        }
                        deleteProductions.Add(tempProductionList[i]);
                    }
                }
                tempProductionList = tempProductionList.Except(deleteProductions).ToList();
            }
        }

        public static void PrintFirst()
        {
            foreach (var entry in FirstSet)
            {
                if (char.IsUpper(entry.Key))
                {

                    Console.WriteLine(String.Format("First of {0}:", entry.Key));

                    foreach (var secondEntry in FirstSet[entry.Key])
                    {
                        Console.Write(secondEntry);
                    }

                    Console.WriteLine();
                }

            }
        }

        public static void PrintFollow()
        {
            foreach (var entry in FollowSet)
            {

                Console.WriteLine(String.Format("Follow of {0}", entry.Key));

                foreach (var secondEntry in FollowSet[entry.Key])
                {
                    Console.Write(secondEntry);
                }
                Console.WriteLine();
            }
        }



        public static void ComputeFollow()
        {
            bool changed = true;

            if (!FollowSet.ContainsKey('S'))
            {
                throw new Exception("The first production doesn't start with S");
            }

            FollowSet['S'].Add('$');


            while (changed)
            {
                changed = false;

                for (int i = 0; i < FirstProductionList.Count; ++i)
                {
                    for (int j = 0; j < FirstProductionList[i].ProductionList.Count - 1; ++j)
                    {
                        var letter = FirstProductionList[i].ProductionList[j];

                        if (char.IsUpper(letter))
                        {
                            int count = FollowSet[letter].Count;

                            foreach (char character in FirstSet[FirstProductionList[i].ProductionList[j + 1]])
                            {


                                if (character != '~')
                                    FollowSet[letter].Add(character);
                            }

                            if (FollowSet[letter].Count != count)
                            {
                                changed = true;
                            }
                        }
                    }


                    for (int j = 0; j < FirstProductionList[i].ProductionList.Count - 1; ++j)
                    {
                        var letter = FirstProductionList[i].ProductionList[j];

                        if (char.IsUpper(letter) && FirstSet[FirstProductionList[i].ProductionList[j + 1]].Contains('~'))
                        {
                            int count = FollowSet[letter].Count;

                            foreach (char character in FollowSet[FirstProductionList[i].ProductionSymbol])
                            {

                                FollowSet[letter].Add(character);
                            }

                            if (FollowSet[letter].Count != count)
                            {
                                changed = true;
                            }
                        }
                    }

                    var letter1 = FirstProductionList[i].ProductionList[FirstProductionList[i].ProductionList.Count - 1];


                    if (char.IsUpper(letter1))
                    {
                        int count = FollowSet[letter1].Count;

                        foreach (char character in FollowSet[FirstProductionList[i].ProductionSymbol])
                        {
                            FollowSet[letter1].Add(character);
                        }

                        if (FollowSet[letter1].Count != count)
                        {
                            changed = true;
                        }
                    }
                }
            }
        }
        public static void CreateParseTable()
        {


            for (int i = 0; i < States.Count; ++i)
            {
                for (int l = 0; l < States[i].Count; ++l)
                {
                    if (States[i][l].IsClosed())
                    {
                        for (int j = 0; j < FirstProductionList.Count; ++j)
                        {
                            if (FollowSet.ContainsKey(FirstProductionList[j].ProductionSymbol) && FirstProductionList[j].ProductionList.SequenceEqual(States[i][l].ProductionList) && FirstProductionList[j].ProductionSymbol == States[i][l].ProductionSymbol)
                            {
                                foreach (var element in FollowSet[FirstProductionList[j].ProductionSymbol])
                                {
                                    System.Console.WriteLine(i + " to " + j + " with " + element + " operation type : Reduce");
                                    file.WriteLine(String.Format("ACTION({0},{1}) == R{2}", i, element, j));
                                    Actions[(i, element)] = "R" + j;
                                    
                                }
                            }
                        }

                    }

                }

            }

            System.Console.WriteLine("1 to accept with $ ");
            file.WriteLine("ACTION(1,$) == A");
            Actions[(1, '$')] = "A";

            for (int i = 0; i < States.Count(); ++i)
            {
                if (States[i][0].ProductionSymbol == 'S' && States[i][0].IsClosed())
                {
                    System.Console.WriteLine(i + " to accept with $ ");
                    file.WriteLine("ACTION(" + i + ",$) == A");
                    Actions[(i, '$')] = "A";

                }
            }

            file.Close();

        }


        public static void Main(string[] args)
        {


            States = new List<List<Production>>();
            FirstProductionList = new List<Production>();

            TransitionList = new HashSet<Transition>();

            FirstSet = new Dictionary<char, HashSet<char>>();
            FollowSet = new Dictionary<char, HashSet<char>>();

            ReadData();
            ComputeFirst();
            ComputeFollow();
            PrintFirst();
            PrintFollow();

            AddExtraProduction();
            AddFirstState();

            CreateParseTree();

            fathers = new int[States.Count];
            actual = new int[States.Count];
            finalStates = new List<List<Production>>(States);
            //CombineStates();

            PrintAllStates();
            PrintAllTranzitions();

            CreateParseTable();

            var result = IsSLR1();
            if (result == "")
            {
                System.Console.WriteLine("Gramatica este SLR(1)");
                ReadWords();
                foreach (string word in Words)
                {
                    System.Console.WriteLine(CheckWord(word));
                }
            }
            else
            {
                System.Console.WriteLine("Gramatica nu este SLR(1): " + result);
            }

            Console.ReadKey();
        }


        public static string CheckWord(string word)
        {
            return GetDerivation(word);
        }


        public static string IsSLR1()
        {

            for (int i = 0; i < finalStates.Count; ++i)
            {
                for (int j = 0; j < finalStates[i].Count; ++j)
                {
                    for (int k = 0; k < finalStates[i].Count; ++k)
                    {

                        if (j == k) continue;

                        if (finalStates[i][j].IsClosed() && finalStates[i][k].IsClosed())
                        {
                            var follow1 = FollowSet[finalStates[i][j].ProductionSymbol];
                            var follow2 = FollowSet[finalStates[i][k].ProductionSymbol];

                            follow1.IntersectWith(follow2);

                            //Reduce - Reduce conflict
                            if (follow1.Count > 0)
                            {
                                return "Reduce - Reduce conflict";
                            }
                        }

                        if (finalStates[i][j].IsClosed() && finalStates[i][k].IsBehindTerminal())
                        {
                            var follow1 = FollowSet[finalStates[i][j].ProductionSymbol];

                            //Shift - Reduce conflict
                            if (follow1.Contains(finalStates[i][k].GetCurrentSymbol()))
                            {
                                return "Shift - Reduce conflict";
                            }
                        }
                    }
                }
            }

            return "";
        }

        public static string GetDerivation(string word)
        {

            word = word + "$";
            int i = 0;
            var states = new Stack<int>();
            states.Push(0);

            var solution = new Stack<string>();

            while (true)
            {
                var state = states.Peek();
                var symbol = word[i];

                string expression;
                if (Actions.ContainsKey((state, symbol)))
                {
                    expression = Actions[(state, symbol)];
                }
                else
                {
                    return word + " nu este acceptat";
                }

                //Accept
                if (expression.First() == 'A')
                {

                    string output = word + " este acceptat: \n";
                    output += "1. " + FirstProductionList[1].PrintableToString() + "\n";
                    while (solution.Count() > 0)
                    {
                        output += solution.Pop();
                        output += "\n";
                    }

                    return output;
                }

                //Shift
                if (expression.First() == 'S')
                {
                    int nextState = Int32.Parse(expression.Substring(1));
                    states.Push(nextState);
                    i++;
                }
                else
                {
                    //Reduce
                    if (expression.First() == 'R')
                    {

                        var productionNr = Int32.Parse(expression.Substring(1));
                        var production = FirstProductionList[productionNr];

                        for (int t = 1; t <= production.ProductionLength(); ++t)
                            states.Pop();

                        solution.Push(productionNr + ". " + production.PrintableToString());

                        states.Push(GoTo[(states.Peek(), production.ProductionSymbol)]);
                    }
                    else
                    {
                        //error
                        return word + " nu este acceptat";
                    }
                }
            }

            return word + " nu este acceptat";
        }
    }

}


