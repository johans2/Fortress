using System;
using System.Collections.Generic;

public struct Tuple<T1, T2>
{
	public T1 Item1 { get; private set; }
	public T2 Item2 { get; private set; }
		
    public Tuple (T1 item1, T2 item2)
	{
		Item1 = item1;
		Item2 = item2;
	}

    public override string ToString()
    {
        return string.Format("{0} : {1}", Item1.ToString(), Item2.ToString()); 
    }
    
}


