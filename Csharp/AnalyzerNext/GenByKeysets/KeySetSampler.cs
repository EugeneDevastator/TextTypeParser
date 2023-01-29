namespace AnalyzerUtils;

public class KeySetSampler
{
    private IDataContainer _data;

    public KeySetSampler(IDataContainer data)
    {
        _data = data;
    }

    public int GetFingerMainCount(int i, ref LogicalKeySet set) => _data.GetKeyCount(set.Main[i]);
    
    public float GetTotalScore(ref LogicalKeySet set, byte[] quadFingerPlaces)
    {
        float sum = 0;
        for (byte i = 0; i < set.Fingers; i++)
        {
            sum += SampleFinger(i, ref set, ref quadFingerPlaces);
        }
        return sum;
    }

    public float SampleFinger(byte i, ref LogicalKeySet set, ref byte[] quadFingerPlaces)
    {
        float sum = 0;
            //for quads it is easier to type nearby keys.
            var mul = quadFingerPlaces.Contains(i) ? 1 : 2;
            
            sum += _data.GetAdjMetric(set.Main[i], set.FirstComplement[i]);

            if (set.SecondComplement[i] != '_')
            {
                sum += _data.GetAdjMetric(set.Main[i], set.SecondComplement[i]);
                sum += mul * _data.GetAdjMetric(set.FirstComplement[i], set.SecondComplement[i]);
                if (set.Remainder[i] != '_')
                {
                    sum += _data.GetAdjMetric(set.Main[i], set.Remainder[i]);
                    sum += _data.GetAdjMetric(set.FirstComplement[i], set.Remainder[i]);
                    sum += _data.GetAdjMetric(set.SecondComplement[i], set.Remainder[i]);
                }
            }

            if (set.Remainder[i] != '_')
            {
                sum += _data.GetAdjMetric(set.Main[i], set.Remainder[i]);
                sum += _data.GetAdjMetric(set.FirstComplement[i], set.Remainder[i]);
                //sum += _data.GetAdjMetric(set.SecondComplement[i], set.Remainder[i]);
            }

            return sum;
    }
    
    public float GetTotalScore(ref LogicalKeySet set)
    {
        float sum = 0;
        for (int i = 0; i < set.Fingers; i++)
        {
            //for quads it is easier to type nearby keys.
            var mul = set.Remainder[i] == '_' ? 3 : 1;
            
            sum += _data.GetAdjMetric(set.Main[i], set.FirstComplement[i]);
            
            if (set.SecondComplement[i] == '_') continue;
            sum += _data.GetAdjMetric(set.Main[i], set.SecondComplement[i]);
            sum += mul * _data.GetAdjMetric(set.FirstComplement[i], set.SecondComplement[i]);
            
            if (set.Remainder[i] == '_') continue;
            sum += _data.GetAdjMetric(set.Main[i], set.Remainder[i]);
            sum += _data.GetAdjMetric(set.FirstComplement[i], set.Remainder[i]);
            sum += _data.GetAdjMetric(set.SecondComplement[i], set.Remainder[i]);
        }
        return sum;
    }
}