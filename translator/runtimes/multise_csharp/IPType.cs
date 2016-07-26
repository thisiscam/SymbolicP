public interface IPType
{
    ValueSummary<SymbolicInteger> PTypeGetHashCode(ValueSummary<IPType> self);
}

public interface IPType<T> : IPType
{
    ValueSummary<T> DeepCopy(ValueSummary<IPType<T>> self);
    ValueSummary<SymbolicBool> PTypeEquals(ValueSummary<IPType<T>> self, ValueSummary<T> other);
}