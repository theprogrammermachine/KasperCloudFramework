namespace SharedArea
{
    public interface AnswerContext<Q> where Q : class
    {
        Q Question { get; set; }
    }
}