public class List<T> {
	const int INITIAL_CAPACITY = 4;

	internal SymbolicInteger _count = 0;
	internal SymbolicInteger _capacity = List<T>.INITIAL_CAPACITY;

	T[] data = new T[List<T>.INITIAL_CAPACITY];

	public void Add(T item) {
		if (this._count >= this._capacity) {
			SymbolicInteger new_capacity = this._capacity * 2;
			T[] new_data = new T[new_capacity];
			for (SymbolicInteger i = 0; i < this._count; i++) {
				new_data [i] = this.data [i];
			}
			new_data [this._count] = item;
			this.data = new_data;
			this._capacity = new_capacity;
			this._count++;
		} else {
			this.data [_count] = item;
			this._count++;
		}
	}

	public void Insert(SymbolicInteger idx, T item) {
		if (this._count >= this._capacity) {
			SymbolicInteger new_capacity = this._capacity * 2;
			T[] new_data = new T[new_capacity];
			SymbolicInteger i;
			for (i = 0; i < idx; i++) {
				new_data [i] = this.data [i];
			}
			new_data [i] = item;
			for (; i < this._capacity; i++) {
				new_data [i + 1] = this.data [i];
			}
			this._capacity = new_capacity;
			this._count++;
		} else {
			for (SymbolicInteger i = this._count; i > idx; i--) {
				this.data [i] = this.data [i - 1];
			}
			this.data [idx] = item;
			this._count++;
		}
	}
	public void RemoveAt(SymbolicInteger idx) {
		for (SymbolicInteger i = idx + 1; i < this._count; i++) {
			this.data [i - 1] = this.data [i];
		}
		this._count--;
	}
	public void RemoveRange(SymbolicInteger start, SymbolicInteger count) {
		for (SymbolicInteger i = start + count; i < this._count; i++) {
			this.data [i - count] = this.data [i];
		}
		this._count -= count;
	}

	public T this[SymbolicInteger index] { 
		get {
			return this.data [index];
		} 
		set {
			this.data [index] = value;
		}
	}

	public SymbolicInteger Count {
		get {
			return _count;
		}	
	}
}