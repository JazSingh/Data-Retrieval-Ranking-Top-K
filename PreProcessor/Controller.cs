using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAnalyseP1
{
    class Controller
    {
        DatabaseController dc;
        WorkloadParser wp;
        public Controller()
        {
            dc = new DatabaseController();
            wp = new WorkloadParser();

        }

        private Dictionary<Tuple<string, string>, float> CalcQF(string col) //TODO
        {
            var avf = wp.ContructAttrValFreqs();
            int RMaxQF = wp.CalculateRMaxQF(avf);

            Dictionary<Tuple<string,string>, float> hqf = new Dictionary<Tuple<string, string>, float>(avf.Count);



            //foreach (var kvp in kak)
            //    Console.WriteLine("{0}\t{1} = {2}", kvp.Value, kvp.Key.Item1, kvp.Key.Item2);
            return hqf;
        }
    }
}
