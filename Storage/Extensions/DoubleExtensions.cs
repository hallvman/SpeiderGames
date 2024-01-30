namespace Storage.Extensions
{
    public static class DoubleExtensions
    {
        public static int ToInt(this double d)
            => Convert.ToInt32(d);
    }
}