using System.Threading.Tasks;

namespace SharedArea
{
    public interface IConsumer<T> where T : class
    {
        Task Consume(ConsumeContext<T> context);
    }
}