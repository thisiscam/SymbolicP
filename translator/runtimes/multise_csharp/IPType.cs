public interface IPType
{
    ValueSummary<SymbolicInteger> PTypeGetHashCode();
}

public interface IPType<T> : IPType
{
    ValueSummary<T> DeepCopy();
    ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<T> other);
}