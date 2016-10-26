using System;
using System.Collections.Generic;

public class DefaultArray<T> {
	private System.Collections.Generic.List<T> data = new System.Collections.Generic.List<T>();

	private Func<T> defaultValueFunction;

	public DefaultArray(Func<T> defaultValueFunction) 
	{
		this.defaultValueFunction = defaultValueFunction;
	}

	public void EnsureLength(int length) 
	{
		for(int i=data.Count; i < length; i++)
		{
			data.Add(defaultValueFunction());
		}
	}

	public T this[int index] { 
		get {
			EnsureLength(index + 1);
			return this.data [index];
		} 
		set {
			EnsureLength(index + 1);
			this.data [index] = value;
		}
	}
}