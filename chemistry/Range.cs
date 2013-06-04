using System;
using System.Collections.Generic;
using System.Text;

namespace cstari.chemistry
{
    public struct Range
    {
        private int min;
        public int max;

        public Range(int a, int b)
        {
            min = Math.Min(a, b);
            max = Math.Max(a, b);
        }

        public int Min
        {
            set
            {
                min = value;

                if (min > max)
                {
                    int i = min;
                    min = max;
                    max = min;
                }
            }
            get
            {
                return min;
            }
        }

        public int Max
        {
            set
            {
                max = value;

                if (min > max)
                {
                    int i = min;
                    min = max;
                    max = min;
                }
            }
            get
            {
                return max;
            }
        }

        public int Limit(int i)
        {
            if (i < Min)
                return Min;
            if (i > Max)
                return Max;
            return i;
        }

        public float RelativeLocation(int i)
        {
            return (float)(i - Min) / (Max - Min);
        }
    }

}
