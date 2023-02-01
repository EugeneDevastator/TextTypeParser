using System.Collections;
using Combinatorics.Collections;

namespace AnalyzerUtils;

public class Combine<T> : IEnumerable<IReadOnlyList<T>>
{
    private IEnumerable<T> _elements;
    private bool _needDuplicateOrder = false;
    private int _places = -1;
    private bool _orderMatters = false;
    private bool _cloneEntries = false;

    public Combine(IEnumerable<T> elements)
    {
        _elements = elements;
    }

    public Combine<T> PlacesCount(int places)
    {
        _places = places;
        return this;
    }

    public Combine<T> DuplicateOrderIrrelevant()
    {
        _needDuplicateOrder = false;
        return this;
    }

    public Combine<T> CloneEntries()
    {
        _cloneEntries = true;
        return this;
    }
    
    public Combine<T> DuplicateOrderMatters()
    {
        _needDuplicateOrder = true;
        return this;
    }

    public Combine<T> OrderMatters()
    {
        _orderMatters = true;
        return this;
    }

    public Combine<T> OrderIrrelevant()
    {
        _orderMatters = false;
        return this;
    }

    public IEnumerable<IReadOnlyList<T>> Make()
    {
        if (_places < 0) // consider places == element count
        {
            if (!_orderMatters)
                return new List<IReadOnlyList<T>>() { _elements.ToList() };
            if (_needDuplicateOrder)
                return new Permutations<T>(_elements, GenerateOption.WithRepetition);
            return new Permutations<T>(_elements, GenerateOption.WithoutRepetition);
        }

        else // there are places provided.
        {
            if (_orderMatters)
            {
                if (_needDuplicateOrder)
                {
                    return new Variations<T>(_elements, _places, GenerateOption.WithRepetition);
                }

                return new Variations<T>(_elements.Distinct(), _places, GenerateOption.WithoutRepetition);
            }
            else // order doesnt matter
            {
                if (_needDuplicateOrder)
                {
                    return new Combinations<T>(_elements, _places, GenerateOption.WithRepetition);
                }
                return new Combinations<T>(_elements, _places, GenerateOption.WithoutRepetition);
            }
        }

        //TODO placing subsets into amount of places
        return null;
    }

    public IEnumerator<IReadOnlyList<T>> GetEnumerator()
    {
        return Make().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}