static class Extensions
{
    public static IEnumerable<(int x,int y)> Coords<T>(this T[,] array){
        for (int k = 0; k < array.GetLength(1); k++)
        {
            for (int i = 0; i < array.GetLength(0); i++)
            {
                yield return (i, k);
            }            
        }
    }
}