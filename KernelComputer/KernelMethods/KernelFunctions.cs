using System;
using System.Collections.Generic;

namespace MachineLearning.KernelMethods
{
    public static class KernelFunctions
    {

        public static double Gamma = 1;
        private static double countBits(ulong[] bits)
        {
            double result = 0;
            for (int index = 0; index < bits.Length; index++)
            {

                for (; bits[index] > 0; result++)
                {
                    bits[index] &= bits[index] - 1; // clear the least significant bit set
                }
            }
            return result;
        }
        private static double squaredDistance(double[] vectorA, double[] vectorB)
        { 
           double result = 0;
           if(vectorA.Length != vectorB.Length)
               throw new ArgumentException("The vectors must have the same number of axes");
           for (int index = 0; index < vectorA.Length; index++)
               result += Math.Pow(vectorB[index] - vectorA[index], 2);
           return result;
        
        }
        public static double Tanimoto(ulong[] featuresX, ulong[] featuresY)
        {
            if (featuresX.Length != featuresY.Length)
                throw new ArgumentException("The arguments need to have the same length");
            ulong[] complementX, complementY, union, intersection, unionComplement, intersectionComplement;
            complementX = new ulong[featuresX.Length];
            complementY = new ulong[featuresY.Length];
            union = new ulong[featuresY.Length];
            intersection = new ulong[featuresY.Length];
            unionComplement = new ulong[featuresY.Length];
            intersectionComplement = new ulong[featuresY.Length];
            for (int index = 0; index < featuresX.Length; index++)
            {
                complementX[index] = ~featuresX[index];
                complementY[index] = ~featuresY[index];
                union[index] = featuresX[index] | featuresY[index];
                intersection[index] = featuresX[index] & featuresY[index];
                unionComplement[index] = complementX[index] | complementY[index];
                intersectionComplement[index] = complementX[index] & complementY[index];

            }
            double firstTerm = KernelFunctions.countBits(intersection) / KernelFunctions.countBits(union);
            double secondTerm = KernelFunctions.countBits(intersectionComplement) / KernelFunctions.countBits(unionComplement);
            return firstTerm + secondTerm;
        }
        public static double RBFKernel(List<double[]> featuresX, List<double[]> featuresY)
        {
            double result = 0;
            if (featuresX.Count != featuresY.Count)
                throw new ArgumentException("We have less players for one field configuration");
            if (featuresX[0].Length != featuresY[0].Length)
                throw new ArgumentException("The arguments need to have exactly the same length");
            for (int index = 0; index < featuresX.Count; index++)
            {
                for (int index2 = 0; index2 < featuresY.Count; index2++)
                {
                    result += Math.Pow(Math.E, -1/(2 * Math.Pow(KernelFunctions.Gamma,2)) 
                        * KernelFunctions.squaredDistance(featuresX[index], featuresY[index]));
                }
            }
            return result;
        }
    }
}
