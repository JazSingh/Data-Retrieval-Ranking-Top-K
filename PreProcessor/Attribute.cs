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

        public void SetQF(int RQF, int RMaxQF)
        {
            QF = ((float)RQF + 1) 
               / ((float)RmaxQF + 1);
        }

        public abstract void SetIDF(/*nog bepalen*/);
    }

    public class CatAttribute : Attribute
    {

    }

    public class NumAttribute : Attribute
    {

    }
}
