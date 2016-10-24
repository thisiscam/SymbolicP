using System;

public class List<T> {
	internal int _count = 0;

	protected DefaultArray<T> data = new DefaultArray<T>(() => default(T));

	public void Add(T item) {
		this.data [_count] = item;
		this._count++;
	}

	public void Insert(SymbolicInteger idx, T item) {
		for (int i = this._count; i > idx; i--) {
			this.data [i] = this.data [i - 1];
		}
		this.data [idx] = item;
		this._count++;
	}

	public void RemoveAt(SymbolicInteger idx) {
		for (SymbolicInteger i = idx + 1; i < this._count; i++) {
			this.data [i - 1] = this.data [i];
		}
		this._count--;
	}

	public void RemoveRange(int start, int count) {
		for (int i = start + count; i < this._count; i++) {
			this.data [i - count] = this.data [i];
		}
		this._count = this._count - count;
	}

	public T this[int index] { 
		get {
			if(index >= this._count) {
				throw new IndexOutOfRangeException();
			}
			return this.data [index];
		} 
		set {
			if(index >= this._count) {
				throw new IndexOutOfRangeException();
			}
			this.data [index] = value;
		}
	}

	public T this[SymbolicInteger index] { 
		get {
			if(index >= this._count) {
				throw new IndexOutOfRangeException();
			}
			return this.data [index];
		} 
		set {
			if(index >= this._count) {
				throw new IndexOutOfRangeException();
			}
			this.data [index] = value;
		}
	}

	public int Count {
		get {
			return _count;
		}	
	}
}