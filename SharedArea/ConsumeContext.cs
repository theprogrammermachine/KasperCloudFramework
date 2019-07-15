namespace SharedArea
{
    public class ConsumeContext<T> where T : class
    {
        public T Message { get; set; }
    }
}