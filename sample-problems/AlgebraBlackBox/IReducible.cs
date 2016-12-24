namespace AlgebraBlackBox
{
    public interface IReducible<T>
    {
        T AsReduced(bool ensureClone = false);

        bool IsReducible { get; }
    }

    public interface IReducibleGene : IReducible<IGene>, IGene
    {
        IGene Reduce();
    }

}