using System;

namespace SpectrumCore.Utils
{
    public static class ExtensionUtils
    {
        #region double[] extension
        public static short[] ToShortsArray(this double[] origionArray)
        {
            short[] array = new short[origionArray.Length];
            for (int i = 0; i < origionArray.Length; i++)
            {
                array[i] = (short)origionArray[i];
            }
            return array;
        }
        #endregion

        #region short[] extension
        public static double[] ToDoublesArray(this short[] origionArray)
        {
            double[] array = new double[origionArray.Length];
            for (int i = 0; i < origionArray.Length; i++)
            {
                array[i] = origionArray[i];
            }
            return array;
        }
        #endregion

        #region float[] extension
        public static double[] ToDoublesArray(this float[] origionArray)
        {
            double[] array = new double[origionArray.Length];
            for (int i = 0; i < origionArray.Length; i++)
            {
                array[i] = origionArray[i];
            }
            return array;
        }
        #endregion
    }
}
