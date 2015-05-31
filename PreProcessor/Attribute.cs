using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreProcessor
{
    public abstract class Attribute
    {
        string name;
        double QF;
        double IDF;
        double QFIDF;
        double importance;

        public Attribute(string name)
        {
            this.name = name;
        }

        public void SetQF(int RQF, int RMaxQF)
        {
            if (RMaxQF == 0) // er wordt nooit gezocht in de workload naar dit attribuut, QF moet laag zijn
                QF = 1f/7500f;
            else
                QF = ((double)RQF + 1)
                   / ((double)RMaxQF + 1);

            SetImportance();
        }

        private void SetImportance()
        {
            importance = Math.Log10(QF);
        }

        public void SetIDF(double idf)
        {
            IDF = idf;
        }
        public double GetImportance()
        {
            return importance;
        }

        public double GetQFIDF()
        {
            return QF * IDF;
        }
    }

    public class CatAttribute : Attribute
    {
        public CatAttribute(string name) 
            : base(name)
        {
            //mag dit leeg blijven?
        }
        }

    public class NumAttribute : Attribute
    {
        public NumAttribute(string name)
            : base(name)
        { 
            //mag leeg blijven? :p
        }
    }
}
