using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreProcessor
{
    class Controller
    {
        DatabaseController dc;
        WorkloadParser wp;

        //MetaDB tables
        //Categorical
        QFIDFCatTable Type = new QFIDFCatTable("type");
        QFIDFCatTable Brand = new QFIDFCatTable("brand");
        QFIDFCatTable Model = new QFIDFCatTable("model");
        //Numerical
        QFIDFNumTable Mpg = new QFIDFNumTable("mpg");
        QFIDFNumTable Cylinders = new QFIDFNumTable("cylinders");
        QFIDFNumTable Displacement = new QFIDFNumTable("displacement");
        QFIDFNumTable Horsepower = new QFIDFNumTable("horsepower");
        QFIDFNumTable Weight = new QFIDFNumTable("weight");
        QFIDFNumTable Acceleration = new QFIDFNumTable("acceleration");
        QFIDFNumTable ModelYear = new QFIDFNumTable("model_year");
        QFIDFNumTable Origin = new QFIDFNumTable("origin");
        //Overlap table
        OverlapTable AttributeOverlap = new OverlapTable();
        //Bandwith table
        BandwithTable Bandwith = new BandwithTable();

        List<QFIDFTable> QFIDFTables;
        Dictionary<string, int> RMaxQFs = new Dictionary<string, int>();

        public Controller()
        {
            dc = new DatabaseController();
            wp = new WorkloadParser();
            InitializeTables();
            QFIDFTables = new List<QFIDFTable> 
            {
                Type, Brand, Model, Mpg, 
                Cylinders, Displacement, Horsepower, Weight, Acceleration, ModelYear, Origin
            };
            Start();
        }

        private void Start()
        {
            Console.WriteLine("Calculating...");
            CalcQF();
            CalcJaccard();
            //IDF
            //Bandwith
            Console.WriteLine("Finished Calculating!");
        }
        private void CalcQF()
        {
            Console.WriteLine("\tCalculating QFs...");
            var avf = wp.ContructAttrValFreqs();
            foreach (var table in QFIDFTables)
                RMaxQFs.Add(table.Name, wp.CalculateRMaxQF(avf, table.Name));
            foreach (var table in QFIDFTables)
                table.SetQF(avf, RMaxQFs);
            Console.WriteLine("\tFinished Calculating QFs!");
        }

        private void CalcJaccard()
        {
            Console.WriteLine("\tCalculating Jaccard...");
            int threshold = CalcThreshold();
            var singletons = wp.ContructAttrValFreqs();
            FilterSingletons(ref singletons, threshold);
            var pairs = CreatePairs(singletons);
            FilterPairs(ref pairs, threshold);
            FillOverlapTable(pairs, singletons);
            Console.WriteLine("\tFinished Calculating Jaccard!");
        }

        private void FillOverlapTable(Dictionary<Tuple<string, string, string>, int> pairs, Dictionary<Tuple<string, string>, int> singletons)
        {
            foreach (var kvp in pairs)
            {
                //k = |A intersect B|
                //|A union B| = |A| + |B| - |A intersect B|
                float k = ((float)kvp.Value)
                    / ((float)singletons[new Tuple<string, string>(kvp.Key.Item1, kvp.Key.Item2)] + singletons[new Tuple<string, string>(kvp.Key.Item1, kvp.Key.Item3)] - kvp.Value);
                AttributeOverlap.table.Add(kvp.Key, k);
            }
        }

        private void FilterPairs(ref Dictionary<Tuple<string, string, string>, int> pairs, int threshold)
        {
            Dictionary<Tuple<string, string, string>, int> pairsNew = new Dictionary<Tuple<string, string, string>, int>();
            foreach (var kvp in pairs)
                if (kvp.Value >= threshold)
                    pairsNew.Add(kvp.Key, kvp.Value);
            pairs = pairsNew;
        }

        private void FilterSingletons(ref Dictionary<Tuple<string, string>, int> singletons, int threshold)
        {
            Dictionary<Tuple<string, string>, int> singletonsNew = new Dictionary<Tuple<string, string>, int>();
            foreach (var kvp in singletons)
                if (kvp.Value >= threshold)
                    singletonsNew.Add(kvp.Key, kvp.Value);
            singletons = singletonsNew;
        }

        private Dictionary<Tuple<string, string, string>, int> CreatePairs(Dictionary<Tuple<string, string>, int> singletons)
        {
            var pairs = new Dictionary<Tuple<string, string, string>, int>();
            var singles = singletons.ToArray();
            for (int i = 0; i < singles.Length; i++)
            {
                for(int j = i+1; j < singles.Length; j++)
                {
                    if(singles[i].Key.Item1 == singles[j].Key.Item1) //zelfde kolom?
                    {
                        int intersection = wp.PairsTogether(singles[i].Key.Item1, singles[i].Key.Item2, singles[j].Key.Item2);
                        pairs.Add(new Tuple<string, string, string>(singles[i].Key.Item1, singles[i].Key.Item2, singles[j].Key.Item2), intersection);
                    }
                }
            }
            return pairs;
        }

        //1%
        private int CalcThreshold()
        {
            float TotalQueries = (float) wp.SumFreqs();
            return (int) Math.Ceiling(TotalQueries * 0.01);
        }

        public void InitializeTables()
        {
            Console.WriteLine("Initializing table classes....");
            Type.Initialize(dc.GetDistinctAttributeValues("type"));
            Brand.Initialize(dc.GetDistinctAttributeValues("brand"));
            Model.Initialize(dc.GetDistinctAttributeValues("model"));

            Mpg.Initialize(dc.GetDistinctAttributeValues("mpg"));
            Cylinders.Initialize(dc.GetDistinctAttributeValues("cylinders"));
            Displacement.Initialize(dc.GetDistinctAttributeValues("displacement"));
            Horsepower.Initialize(dc.GetDistinctAttributeValues("horsepower"));
            Weight.Initialize(dc.GetDistinctAttributeValues("weight"));
            Acceleration.Initialize(dc.GetDistinctAttributeValues("acceleration"));
            ModelYear.Initialize(dc.GetDistinctAttributeValues("model_year"));
            Origin.Initialize(dc.GetDistinctAttributeValues("origin"));
            Console.WriteLine("DONE Initializing table classes!");
        }
    }
}
