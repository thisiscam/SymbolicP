using System.Collections.Generic;

public struct DefaultArray<T> {
	System.Collections.Generic.List<T> data;
	Func<T> defaultValueFunction;

	public DefaultArray<T>(Func<T> defaultValueFunction) 
	{
		this.defaultValueFunction = defaultValueFunction;	
	}

	public T this[int index] { 
        get {
        	if(index >= data.Count) {
        		for(int i=0; i < index; i++)
        		{
        			data.Add(defaultValueFunction.Invoke());
        		}
        	}
            return this.data [index];
        }
        set {
        	if(index >= data.Count) {
        		for(int i=0; i < index; i++)
        		{
        			data.Add(defaultValueFunction.Invoke());
        		}
        	}
            this.data [index] = value;
        }
    }
}