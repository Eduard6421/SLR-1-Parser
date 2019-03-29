using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.VisualBasic.CompilerServices;

namespace Tema2
{
    class Program
    {
        private static string InputFile = @"..\..\..\data.in";
        private static string OutputFile = @"..\..\..\data.out";

        private static List<List<Production>> States;
        private static List<List<Production>> finalStates;
        private static List<Production> FirstProductionList;
        private static HashSet<Transition> TransitionList;
        private static Dictionary<char, HashSet<char>> FirstSet;
        private static Dictionary<char, HashSet<char>> FollowSet;

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

        public static void CreateParseTree()
        {
            bool complete = false;

            while (!complete)
            {
                complete = true;

                for (int i = 0; i < States.Count; ++i)
                {
                    for (int j = 0; j < States[i].Count; ++j)
                    {
                        Production prod = new Production();

                        prod.ProductionList = States[i][j].ProductionList;
                        prod.DotPosition = States[i][j].DotPosition;
                        prod.ProductionSymbol = States[i][j].ProductionSymbol;

                        if (!prod.IsClosed())
                        {
                            Transition newTranzition = new Transition();

                            char character;
                            character = prod.GetCurrentSymbol();
                            prod.DotPosition = prod.DotPosition + 1;

                            for (int i1 = 0; i1 < States.Count; ++i1)
                            {
                                for (int j1 = 0; j1 < States[i1].Count; ++j1)
                                {
                                    if (prod.ProductionList == States[i1][j1].ProductionList &&
                                        prod.DotPosition == States[i1][j1].DotPosition)
                                    {
                                        newTranzition.Character = character;
                                        newTranzition.FromState = i;
                                        newTranzition.ToState = i1;
                                        newTranzition.IsGoto = char.IsUpper(character);

                                        TransitionList.Add(newTranzition);
                                        goto Label;

                                    }
                                }
                            }
                            List<Production> newState = new List<Production>();

                            newTranzition.Character = character;
                            newTranzition.FromState = i;
                            newTranzition.ToState = States.Count;
                            newTranzition.IsGoto = char.IsUpper(character);

                            newState.Add(prod);

                            for (int j3 = 0; j3 < newState.Count; ++j3)
                            {
                                prod = newState[j3];

                                if (prod.IsBehindProduction())
                                {
                                    for (int i3 = 0; i3 < FirstProductionList.Count; ++i3)
                                    {
                                        if (FirstProductionList[i3].ProductionSymbol == prod.GetCurrentSymbol() && !newState.Contains(FirstProductionList[i3]))
                                        {
                                            newState.Add(FirstProductionList[i3]);
                                        }
                                    }

                                }


                            }

                            complete = false;
                            States.Add(newState);
                            TransitionList.Add(newTranzition);
                        }
                    Label:
                        continue;
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
            foreach (Transition t in TransitionList)
            {
                System.Console.WriteLine(t.FromState + " to " + t.ToState + " with " + t.Character + " operation type :" + (t.IsGoto ? "Move" : "Shift"));
            }
        }
        public static void CombineStates()
        {
            HashSet<Tuple<int, int>> sameStates = new HashSet<Tuple<int, int>>();
            HashSet<Transition> newHash = new HashSet<Transition>();
            finalStates = new List<List<Production>>();


            foreach (Transition t in TransitionList)
            {
                foreach (Transition v in TransitionList)
                {
                    if (t != v && t.FromState == v.FromState && t.Character == v.Character)
                    {
                        int min = t.ToState < v.ToState ? t.ToState : v.ToState;
                        int max = t.ToState >= v.ToState ? t.ToState : v.ToState;
                        Tuple<int, int> newTuple = new Tuple<int, int>(min, max);
                        sameStates.Add(newTuple);
                    }
                }

            }

            foreach (Tuple<int, int> t in sameStates)
            {

                List<Production> combinedProduction = new List<Production>();

                combinedProduction = States[t.Item1];

                for (int i = 0; i < States[t.Item2].Count; ++i)
                {
                    if (!combinedProduction.Contains(States[t.Item2][i]))
                    {
                        combinedProduction.Add(States[t.Item2][i]);
                    }
                }

                finalStates.Add(combinedProduction);
            }

            for (int i = 0; i < States.Count; ++i)
            {
                bool addIt = true;

                foreach (Tuple<int, int> t in sameStates)
                {
                    if (i == t.Item2)
                    {
                        break;
                        addIt = false;
                    }
                }

                if (addIt)
                {
                    finalStates.Add(States[i]);
                }
            }

            foreach (Transition t in TransitionList)
            {
                foreach (Tuple<int, int> tup in sameStates)
                {
                    if (t.ToState == tup.Item2)
                        t.ToState = tup.Item1;
                    if (t.FromState == tup.Item2)
                        t.FromState = tup.Item1;
                }

            }


            foreach (Transition t in TransitionList)
            {
                newHash.Add(t);
            }

            TransitionList = newHash;

        }



        public static void AddFirstState()
        {
            bool complete = true;
            List<Production> newState = new List<Production>();

            Production prod = FirstProductionList[0];

            newState.Add(prod);


            for (int i = 0; i < newState.Count; ++i)
            {
                var currentProd = FirstProductionList[i];

                for (int j = 0; j < FirstProductionList.Count; ++j)
                {
                    if (currentProd.GetFirst() == FirstProductionList[j].ProductionSymbol)
                    {
                        newState.Add(FirstProductionList[j]);
                        complete = false;
                    }
                }

            }
            States.Add(newState);
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

                Dictionary<char, HashSet<char>> tmpFollow = new Dictionary<char, HashSet<char>>(FollowSet);

                for (int i = 0; i < FirstProductionList.Count; ++i)
                {
                    var production = FirstProductionList[i].ProductionList;

                    if (production.Count == 1 && char.IsUpper(production[0]))
                    {
                        foreach (var element in FollowSet[FirstProductionList[i].ProductionSymbol])
                        {
                            int num1 = FollowSet[production[0]].Count;
                            FollowSet[production[0]].Add(element);

                            if (num1 != FollowSet[production[0]].Count)
                            {
                                changed = true;
                            }
                        }
                    }

                    if (production.Count == 2 && char.IsUpper(production[1]))
                    {

                        foreach (var element in FollowSet[FirstProductionList[i].ProductionSymbol])
                        {

                            int num1 = FollowSet[production[1]].Count;
                            FollowSet[production[1]].Add(element);

                            if (num1 != FollowSet[production[1]].Count)
                            {
                                changed = true;
                            }

                        }
                    }


                    if (production.Count == 2 && char.IsUpper(production[0]) && FirstSet[production[1]].Contains('~'))
                    {
                        foreach (var element in FollowSet[FirstProductionList[i].ProductionSymbol])
                        {

                            int num1 = FollowSet[production[0]].Count;
                            FollowSet[production[0]].Add(element);

                            if (num1 != FollowSet[production[0]].Count)
                            {
                                changed = true;
                            }

                        }
                    }


                    if (production.Count == 3 && char.IsUpper(production[1]) && FirstSet[production[2]].Contains('~'))
                    {
                        foreach (var element in FollowSet[FirstProductionList[i].ProductionSymbol])
                        {

                            int num1 = FollowSet[production[1]].Count;
                            FollowSet[production[1]].Add(element);

                            if (num1 != FollowSet[production[1]].Count)
                            {
                                changed = true;
                            }

                        }
                    }




                    if (production.Count == 2 && char.IsUpper(production[0]) && production[1] != '~')
                    {

                        foreach (var element in FirstSet[production[1]])
                        {
                            if (element != '~')
                            {
                                int num1 = FollowSet[production[0]].Count;
                                FollowSet[production[0]].Add(element);

                                if (num1 != FollowSet[production[0]].Count)
                                    changed = true;
                            }
                        }
                    }

                    if (production.Count == 3 && char.IsUpper(production[1]) && production[2] != '~')
                    {

                        foreach (var element in FirstSet[production[2]])
                        {
                            if (element != '~')
                            {
                                int num1 = FollowSet[production[1]].Count;
                                FollowSet[production[1]].Add(element);

                                if (num1 != FollowSet[production[1]].Count)
                                    changed = true;
                            }
                        }
                    }


                }

            }
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
            CombineStates();

            PrintAllStates();
            PrintAllTranzitions();

            CreateParseTable();
            Console.ReadKey();
        }


        public static void CreateParseTable()
        {
            for (int i = 0; i < States.Count; ++i)
            {
                if (States[i][0].IsClosed())
                {
                    for (int j = 0; j < FirstProductionList.Count; ++j)
                    {
                        if (FollowSet.ContainsKey(FirstProductionList[j].ProductionSymbol) && FirstProductionList[j].ProductionList.SequenceEqual(States[i][0].ProductionList))
                        {
                            foreach (var element in FollowSet[FirstProductionList[j].ProductionSymbol])
                            {
                                System.Console.WriteLine(i + " to " + j + " with " + element + " operation type : Reduce");
                            }
                            goto nextlabel;
                        }
                    }

                }
            nextlabel:
                continue;
            }

            System.Console.WriteLine("1 to accept with $ ");

        }
    }

}
