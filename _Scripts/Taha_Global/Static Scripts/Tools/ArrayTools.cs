public static class ArraysTools
{
    /// <summary>
    /// this method return's true if tow arrays are the same
    /// </summary>
    public static bool _ArrayEqual(int[] first, int[] second)
    {
        if (first.Length > 0 && second.Length > 0)
        {
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public static int _ArrayZeroLength(int[] array)
    {
        int i = 0;
        foreach (int item in array)
        {
            if (!item.Equals(0))
                i++;
        }
        return i - array.Length;
    }
    public static int _ArraySum(int[] array)
    {
        int _sum = 0;
        if (array.Length > 0)
        {
            for (int i = 0; i < array.Length; i++)
            {
                _sum += array[i];
            }
        }
        return 0;
    }
    public static int[] _MakeZeroArray(int[] first)
    {
        for (int i = 0; i < first.Length; i++)
        {
            first[i] = 0;
        }
        return first;
    }
}