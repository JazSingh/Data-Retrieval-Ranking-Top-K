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
            //Overlap            
            //Bandwith
            //IDF (gebruikt bandwidth)
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
            //niks gevonden = stop
            //Ck verz candidaten van grootte k
            //Lk verzamelingen echte freq itemsets van grootte k
            //C1 --> singletons
            //C2 --> Pairs
        }

        private void CalcIDF()
        {
            Console.WriteLine("\tCalculating IDFs...");

            Console.WriteLine("\tFinished Calculating IDFs!");
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
