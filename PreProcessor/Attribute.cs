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
        float QF;
        float IDF;
        float QFIDF;
        float importance;

        public Attribute(string name)
        {
            this.name = name;
        }

        public void SetQF(int RQF, int RMaxQF)
        {
            if (RMaxQF == 0) // er wordt nooit gezocht in de workload naar dit attribuut, QF moet laag zijn
                QF = 1f/10000f;
            else
                QF = ((float)RQF + 1)
                   / ((float)RMaxQF + 1);

            SetImportance();
        }

        private void SetImportance()
        {
            importance = (float) Math.Log10(QF);
        }

        public void SetIDF(float idf)
        {
            IDF = idf;
        }
        public float GetImportance()
        {
            return importance;
        }

        public float GetQFIDF()
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
