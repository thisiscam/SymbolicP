public interface IPType {
}

public interface IPType<T> : IPType {
	T DeepCopy();

	SymbolicBool PTypeEquals (T other);
}