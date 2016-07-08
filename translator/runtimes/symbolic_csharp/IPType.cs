public interface IPType {
}

public interface IPType<T> : IPType, ISymbolicHashable<T> {
	T DeepCopy();
}