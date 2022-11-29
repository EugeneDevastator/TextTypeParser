namespace AnalyzerNext;

public class AnalyzerNext
{
    private LayoutData _data;
    private Sampler _sampler;

    public AnalyzerNext(LayoutData data, Sampler sampler)
    {
        _sampler = sampler;
        _data = data;
    }

    public void GenerateLayout()
    {
        var posSets = _sampler.PriorityPositions();
        
        foreach (var posSet in posSets)
        {
            // get next most used keys for places
            // fill others with blanks.
            // make variations for available places
            // var v = new Variations(keys, number of places);
            //
            //
        }
    }
    
    
}