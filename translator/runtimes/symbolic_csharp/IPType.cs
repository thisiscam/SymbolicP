public interface IPType {
	SymbolicInteger PTypeGetHashCode ();
}

public interface IPType<T> : IPType {
	T DeepCopy();

	SymbolicBool PTypeEquals (T other);
}