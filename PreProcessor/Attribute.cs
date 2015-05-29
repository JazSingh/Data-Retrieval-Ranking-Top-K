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
            QF = ((float)RQF + 1) 
               / ((float)RMaxQF + 1);

            SetImportance();
        }

        private void SetImportance()
        {
            importance = (float) Math.Log10(QF);
        }

        public float GetImportance()
        {
            return importance;
        }

        public float GetQFIDF()
        {
            return QF * IDF;
        }

        public abstract void SetIDF(/*nog bepalen*/);
    }

    public class CatAttribute : Attribute
    {
        public CatAttribute(string name) 
            : base(name)
        {
            //mag dit leeg blijven?
        }

        public override void SetIDF()
        {
            throw new NotImplementedException();
        }
    }

    public class NumAttribute : Attribute
    {
        public NumAttribute(string name)
            : base(name)
        { 
            //mag leeg blijven? :p
        }

        public override void SetIDF()
        {
            throw new NotImplementedException();
        }
    }
}
