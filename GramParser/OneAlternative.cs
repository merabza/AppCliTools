using System.Collections.Generic;

namespace GramParser;

public class OneAlternative
{
    private readonly List<string> _atoms = new();

    public OneAlternative(string alternative, bool split = true)
    {
        if (split)
        {
            var strAtoms = alternative.Split(' ');
            _atoms.AddRange(strAtoms);
        }
        else
        {
            _atoms.Add(alternative);
        }
    }

    public int Count => _atoms.Count;
    public string this[int index] => _atoms[index];
}