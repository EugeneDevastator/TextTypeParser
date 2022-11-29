using System.Text;

public class CSVExporter
{
    public const char DELIM = ';';

    public void WriteData<T>(string fullPath, T[,] data)
    {
        var xdim = data.GetLength(0);
        var ydim = data.GetLength(1);
        StringBuilder content = new();
        for (int k = 0; k < ydim; k++)
        {
            for (int i = 0; i < xdim; i++)
            {
                content.Append(data[i, k].ToString());
                if (i < xdim - 1)
                    content.Append(DELIM);
            }

            if (k < ydim - 1) ;
            content.Append("\n");
        }

        File.WriteAllText(fullPath, content.ToString());
    }

    public void WriteVector<T>(string fullPath, T[] data)
    {
        var xdim = data.Length;
        StringBuilder content = new();
        for (int i = 0; i < xdim; i++)
        {
            content.Append(data[i].ToString());
            if (i < xdim - 1)
                content.Append(DELIM);
        }

        File.WriteAllText(fullPath, content.ToString());
    }

    public T[] ReadVector<T>(string fullPath)
    {
        var line = File.ReadAllText(fullPath)
            .Split(DELIM)
            .Select(s => (T)Convert.ChangeType(s, typeof(T)))
            .ToArray();

        var output = new T[line.Length];
        for (int i = 0; i < line.Length; i++)
        {
            output[i] = line[i];
        }

        return output;
    }

    public T[,] ReadData<T>(string fullPath)
    {
        var lines = File.ReadAllLines(fullPath);
        var xdim = lines[0].Split(DELIM).Length;
        var ydim = lines.Length;
        var output = new T[xdim, ydim];

        for (int k = 0; k < ydim; k++)
        {
            var line = lines[k]
                .Split(DELIM)
                .Select(s => (T)Convert.ChangeType(s, typeof(T)))
                .ToArray();

            for (int i = 0; i < xdim; i++)
            {
                output[i, k] = line[i];
            }
        }

        return output;
    }
}