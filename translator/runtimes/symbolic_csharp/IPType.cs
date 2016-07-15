public interface IPType {
//	SymbolicInteger GetHashCode ();
}

public interface IPType<T> : IPType {
	T DeepCopy();
}