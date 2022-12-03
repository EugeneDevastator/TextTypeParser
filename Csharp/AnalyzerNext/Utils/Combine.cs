using Combinatorics.Collections;

namespace AnalyzerNext;

public class Combine<T>
{
    private IEnumerable<T> _elements;
    private bool _allowDuplicates = false;
    private int _minPlaces = -1;
    private int _maxPlaces = -1;
    private bool _orderMatters = false;

    public Combine(IEnumerable<T> elements)
    {
        _elements = elements;
    }
    
    public Combine<T> WithPlaces(int places)
    {
        _maxPlaces = places;
        _minPlaces = places;
        return this;
    }

    public Combine<T> WithPlaces(int minPlaces, int maxPlaces)
    {
        _maxPlaces = maxPlaces;
        _minPlaces = minPlaces;
        return this;
    }

    public Combine<T> AllowDuplicates(bool allow)
    {
        _allowDuplicates = allow;
        return this;
    }

    public Combine<T> OrderMatters(bool orderMatters)
    {
        _orderMatters = orderMatters;
        return this;
    }

    public IEnumerable<IReadOnlyList<T>> Make()
    {
        if (_minPlaces < 0) // consider places == element count
        {
            if (!_orderMatters)
                return new List<IReadOnlyList<T>>() { _elements.ToList() };
            if (_allowDuplicates)
                return new Permutations<T>(_elements, GenerateOption.WithRepetition);
            return new Permutations<T>(_elements, GenerateOption.WithoutRepetition);
        }

        if (_minPlaces == _maxPlaces)
        {
            if (_orderMatters)
            {
                if (_allowDuplicates)
                {
                    return new Variations<T>(_elements, _minPlaces, GenerateOption.WithRepetition);
                }
                return new Variations<T>(_elements, _minPlaces,GenerateOption.WithoutRepetition);
            }
            
            // order doesnt matter
            if (_allowDuplicates)
            {
                return new Combinations<T>(_elements, _minPlaces, GenerateOption.WithRepetition);
            }
            return new Combinations<T>(_elements, _minPlaces,GenerateOption.WithoutRepetition);
        }
        //TODO placing subsets into amount of places
        return null;
    }
}